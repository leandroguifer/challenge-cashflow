using Cashflow.Integration.Tests.Support;
using Npgsql;

namespace Cashflow.Integration.Tests;

[Trait("Category", "Integration")]
public sealed class EntriesRowLevelSecurityTests(DatabasesFixture fixture) : IClassFixture<DatabasesFixture>
{
    private static readonly DateOnly Day = new(2026, 11, 19);

    private string App => fixture.Databases.EntriesAs(TestCredentials.EntriesApp);

    private static string NewStore() => $"LOJA{Guid.NewGuid().ToString("N")[..8]}".ToUpperInvariant();

    private async Task<Guid> InsertCashRegisterAsync(string store)
    {
        var id = Guid.NewGuid();

        await Sql.ExecuteAsync(
            App,
            "insert into cash_registers (id, store_id, business_date, status, opened_by, opened_at) values (@id, @store, @day, 1, 'tester', @now)",
            [store],
            ("id", id),
            ("store", store),
            ("day", Day),
            ("now", DateTimeOffset.UtcNow));

        return id;
    }

    private Task<int> InsertEntryAsync(Guid cashRegisterId, string store, IReadOnlyList<string> sessionStores) =>
        Sql.ExecuteAsync(
            App,
            "insert into entries (id, cash_register_id, store_id, business_date, type, amount, description, created_by, created_at) values (@id, @register, @store, @day, 1, 10, 'Lancamento de teste', 'tester', @now)",
            sessionStores,
            ("id", Guid.NewGuid()),
            ("register", cashRegisterId),
            ("store", store),
            ("day", Day),
            ("now", DateTimeOffset.UtcNow));

    private static async Task AssertDeniedAsync(Func<Task> action)
    {
        var exception = await Assert.ThrowsAsync<PostgresException>(action);

        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, exception.SqlState);
    }

    [Fact(DisplayName = "RF10: a loja A não enxerga o caixa da loja B, e sem loja informada nada é devolvido")]
    [Trait("Requirement", "RF10")]
    public async Task AppRoleSeesOnlyTheStoresInTheSession()
    {
        var storeA = NewStore();
        var storeB = NewStore();
        await InsertCashRegisterAsync(storeA);
        await InsertCashRegisterAsync(storeB);

        const string Count = "select count(*) from cash_registers where store_id in (@a, @b)";
        (string, object?)[] parameters = [("a", storeA), ("b", storeB)];

        Assert.Equal(1, await Sql.ScalarAsync<int>(App, Count, [storeA], parameters));
        Assert.Equal(1, await Sql.ScalarAsync<int>(App, Count, [storeB], parameters));
        Assert.Equal(2, await Sql.ScalarAsync<int>(App, Count, [storeA, storeB], parameters));
        Assert.Equal(0, await Sql.ScalarAsync<int>(App, Count, null, parameters));
    }

    [Fact(DisplayName = "RF10: o usuário da loja B não consegue gravar lançamento da loja A")]
    [Trait("Requirement", "RF10")]
    public async Task CannotInsertEntryOfAnotherStore()
    {
        var storeA = NewStore();
        var storeB = NewStore();
        var registerA = await InsertCashRegisterAsync(storeA);

        await AssertDeniedAsync(() => InsertEntryAsync(registerA, storeA, [storeB]));
    }

    [Fact(DisplayName = "RF02: lançamentos são imutáveis, o banco nega UPDATE e DELETE ao papel da aplicação")]
    [Trait("Requirement", "RF02")]
    public async Task EntriesAreImmutableForTheAppRole()
    {
        var store = NewStore();
        var register = await InsertCashRegisterAsync(store);
        await InsertEntryAsync(register, store, [store]);

        await AssertDeniedAsync(() => Sql.ExecuteAsync(App, "update entries set amount = 999 where store_id = @store", [store], ("store", store)));
        await AssertDeniedAsync(() => Sql.ExecuteAsync(App, "delete from entries where store_id = @store", [store], ("store", store)));
    }
}
