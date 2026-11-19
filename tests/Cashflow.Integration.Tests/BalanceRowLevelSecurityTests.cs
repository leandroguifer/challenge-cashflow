using Cashflow.Integration.Tests.Support;

namespace Cashflow.Integration.Tests;

[Trait("Category", "Integration")]
public sealed class BalanceRowLevelSecurityTests(DatabasesFixture fixture) : IClassFixture<DatabasesFixture>
{
    private static readonly DateOnly Day = new(2026, 11, 19);

    private string App => fixture.Databases.BalanceAs(TestCredentials.BalanceApp);

    private string Consumer => fixture.Databases.BalanceAs(TestCredentials.BalanceConsumer);

    private static string NewStore() => $"LOJA{Guid.NewGuid().ToString("N")[..8]}".ToUpperInvariant();

    private Task<int> InsertBalanceAsync(string store) =>
        Sql.ExecuteAsync(
            Consumer,
            "insert into daily_balances (store_id, business_date, total_credits, total_debits, entries_count, updated_at) values (@store, @day, 100, 30, 2, @now)",
            null,
            ("store", store),
            ("day", Day),
            ("now", DateTimeOffset.UtcNow));

    [Fact(DisplayName = "RF10: a consulta do saldo da loja A não enxerga o saldo da loja B")]
    [Trait("Requirement", "RF10")]
    public async Task ReaderSeesOnlyTheStoresInTheSession()
    {
        var storeA = NewStore();
        var storeB = NewStore();
        await InsertBalanceAsync(storeA);
        await InsertBalanceAsync(storeB);

        const string Count = "select count(*) from daily_balances where store_id in (@a, @b)";
        (string, object?)[] parameters = [("a", storeA), ("b", storeB)];

        Assert.Equal(1, await Sql.ScalarAsync<int>(App, Count, [storeA], parameters));
        Assert.Equal(1, await Sql.ScalarAsync<int>(App, Count, [storeB], parameters));
        Assert.Equal(2, await Sql.ScalarAsync<int>(App, Count, [storeA, storeB], parameters));
    }

    [Fact(DisplayName = "RF10: sem a loja informada a consulta de saldo não devolve nenhuma linha")]
    [Trait("Requirement", "RF10")]
    public async Task WithoutStoreContextNothingIsVisible()
    {
        var store = NewStore();
        await InsertBalanceAsync(store);

        const string Count = "select count(*) from daily_balances where store_id = @store";

        Assert.Equal(0, await Sql.ScalarAsync<int>(App, Count, null, ("store", store)));
        Assert.Equal(0, await Sql.ScalarAsync<int>(App, Count, [], ("store", store)));
    }
}
