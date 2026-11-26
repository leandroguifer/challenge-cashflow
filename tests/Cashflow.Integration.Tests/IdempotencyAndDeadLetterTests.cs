using System.Text;
using Cashflow.Integration.Tests.Support;

namespace Cashflow.Integration.Tests;

[Trait("Category", "Integration")]
[Collection(SequentialServices.Name)]
public sealed class IdempotencyAndDeadLetterTests(InfrastructureFixture infrastructure) : ServicesTestBase(infrastructure)
{
    private const string Store = "LOJA01";

    private static byte[] Utf8(string text) => Encoding.UTF8.GetBytes(text);

    private static string Event(Guid eventId, string store, DateOnly day, string type, decimal amount) =>
        $$"""
        {"eventId":"{{eventId}}","entryId":"{{Guid.NewGuid()}}","storeId":"{{store}}","businessDate":"{{day:yyyy-MM-dd}}","type":"{{type}}","amount":{{amount.ToString(System.Globalization.CultureInfo.InvariantCulture)}},"occurredAt":"{{DateTimeOffset.UtcNow:O}}"}
        """;

    [Fact(DisplayName = "RNF05: o mesmo evento entregue várias vezes não altera o saldo duas vezes")]
    [Trait("Requirement", "RNF05")]
    public async Task RepeatedEventDoesNotChangeTheBalance()
    {
        var entries = await StartEntriesAsync();
        var balance = await StartBalanceAsync();
        var owner = TestUser.Owner();
        var today = BusinessDay.Today();

        await entries.OpenCashRegisterAsync(Store);
        await entries.PostEntryAsync(Store, "Credit", 100m, "Venda no balcão");

        await Eventually.ValueAsync(
            () => balance.DailyBalancesAsync(owner, Store, today),
            rows => rows.Count == 1,
            TimeSpan.FromSeconds(30));

        var payload = await Sql.ScalarAsync<string>(Databases.EntriesMigrations, "select payload::text from outbox_messages limit 1");

        for (var delivery = 0; delivery < 3; delivery++)
        {
            await Rabbit.PublishAsync(TestCredentials.RabbitEntries, TestCredentials.RoutingKey, Utf8(payload!));
        }

        await Rabbit.PublishAsync(
            TestCredentials.RabbitEntries,
            TestCredentials.RoutingKey,
            Utf8(Event(Guid.NewGuid(), "LOJA02", today, "Credit", 5m)));

        await Eventually.ValueAsync(
            () => balance.DailyBalancesAsync(owner, "LOJA02", today),
            rows => rows.Count == 1,
            TimeSpan.FromSeconds(30));

        var rows = await balance.DailyBalancesAsync(owner, Store, today);
        var row = Assert.Single(rows);
        Assert.Equal(100m, row.TotalCredits);
        Assert.Equal(1, row.EntryCount);
    }

    [Fact(DisplayName = "RNF05: mensagens inválidas vão para a fila de erros e não alteram o saldo")]
    [Trait("Requirement", "RNF05")]
    public async Task InvalidMessagesGoToTheDeadLetterQueue()
    {
        await StartBalanceAsync();
        var today = BusinessDay.Today();

        await Rabbit.PublishAsync(TestCredentials.RabbitEntries, TestCredentials.RoutingKey, Utf8("{isso não é json"));
        await Rabbit.PublishAsync(TestCredentials.RabbitEntries, TestCredentials.RoutingKey, Utf8(Event(Guid.NewGuid(), Store, today, "Sideways", 10m)));
        await Rabbit.PublishAsync(TestCredentials.RabbitEntries, TestCredentials.RoutingKey, Utf8(Event(Guid.NewGuid(), Store, today, "Credit", 0m)));

        var deadLetters = await Eventually.ValueAsync(
            () => Rabbit.CountAsync(TestCredentials.DeadLetterQueue),
            count => count == 3,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMilliseconds(250));

        Assert.Equal(3u, deadLetters);
        Assert.Equal(0, await Sql.ScalarAsync<int>(Databases.BalanceMigrations, "select count(*) from daily_balances"));
    }

    [Fact(DisplayName = "RNF05: mensagem que falha de forma repetida vai para a fila de erros depois do limite de tentativas")]
    [Trait("Requirement", "RNF05")]
    public async Task RepeatedFailureGoesToTheDeadLetterQueue()
    {
        await StartBalanceAsync();

        await Rabbit.PublishAsync(
            TestCredentials.RabbitEntries,
            TestCredentials.RoutingKey,
            Utf8(Event(Guid.NewGuid(), Store, BusinessDay.Today(), "Credit", 100_000_000_000_000_000m)));

        var deadLetters = await Eventually.ValueAsync(
            () => Rabbit.CountAsync(TestCredentials.DeadLetterQueue),
            count => count == 1,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMilliseconds(250));

        Assert.Equal(1u, deadLetters);
        Assert.Equal(0, await Sql.ScalarAsync<int>(Databases.BalanceMigrations, "select count(*) from daily_balances"));
    }

    [Fact(DisplayName = "RNF05: falha temporária no banco não conta para o limite e o evento é processado quando o banco volta")]
    [Trait("Requirement", "RNF05")]
    public async Task TemporaryFailureDoesNotCountTowardsTheLimit()
    {
        await StartBalanceAsync();

        await Sql.ExecuteAsync(Databases.BalanceMigrations, $"alter database {Databases.Balance} connection limit 0");
        await Sql.ExecuteAsync(
            Databases.BalanceMigrations,
            $"select pg_terminate_backend(pid) from pg_stat_activity where datname = '{Databases.Balance}' and pid <> pg_backend_pid()");

        await Rabbit.PublishAsync(
            TestCredentials.RabbitEntries,
            TestCredentials.RoutingKey,
            Utf8(Event(Guid.NewGuid(), Store, BusinessDay.Today(), "Credit", 10m)));

        await Task.Delay(TimeSpan.FromSeconds(12), TestContext.Current.CancellationToken);

        Assert.Equal(0u, await Rabbit.CountAsync(TestCredentials.DeadLetterQueue));

        await Sql.ExecuteAsync(Databases.BalanceMigrations, $"alter database {Databases.Balance} connection limit -1");

        var processed = await Eventually.ValueAsync(
            () => Sql.ScalarAsync<int>(Databases.BalanceMigrations, "select count(*) from daily_balances"),
            count => count == 1,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMilliseconds(250));

        Assert.Equal(1, processed);
        Assert.Equal(0u, await Rabbit.CountAsync(TestCredentials.DeadLetterQueue));
    }
}
