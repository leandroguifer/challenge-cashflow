using System.Diagnostics;
using System.Net;
using Cashflow.Integration.Tests.Support;

namespace Cashflow.Integration.Tests;

[Trait("Category", "Integration")]
[Collection(SequentialServices.Name)]
public sealed class ConsumerResilienceTests(InfrastructureFixture infrastructure) : ServicesTestBase(infrastructure)
{
    private const string Store = "LOJA02";
    private const int EntryCount = 5;

    [Fact(DisplayName = "RNF01 e RNF03: com o Consolidado parado os lançamentos são aceitos e o saldo se acerta quando ele volta")]
    [Trait("Requirement", "RNF01")]
    [Trait("Requirement", "RNF03")]
    public async Task EntriesKeepWorkingWhileBalanceIsDown()
    {
        var entries = await StartEntriesAsync();
        var today = BusinessDay.Today();

        using var opened = await entries.OpenCashRegisterAsync(Store);
        Assert.Equal(HttpStatusCode.OK, opened.StatusCode);

        decimal total = 0;
        var statuses = new List<HttpStatusCode>();

        for (var index = 1; index <= EntryCount; index++)
        {
            var amount = 10m * index;
            total += amount;

            using var response = await entries.PostEntryAsync(Store, "Credit", amount, $"Lançamento sem consolidado {index}");
            statuses.Add(response.StatusCode);
        }

        Assert.All(statuses, status => Assert.Equal(HttpStatusCode.OK, status));

        var waiting = await Eventually.ValueAsync(
            () => Rabbit.CountAsync(TestCredentials.Queue),
            count => count == EntryCount,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMilliseconds(250));

        var pending = await Eventually.ValueAsync(
            () => Sql.ScalarAsync<int>(Databases.EntriesMigrations, "select count(*) from outbox_messages where published_at is null"),
            count => count == 0,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMilliseconds(250));

        Assert.Equal((uint)EntryCount, waiting);
        Assert.Equal(0, pending);
        Assert.Equal(0, await Sql.ScalarAsync<int>(Databases.BalanceMigrations, "select count(*) from daily_balances"));

        var stopwatch = Stopwatch.StartNew();
        var balance = await StartBalanceAsync();

        var rows = await Eventually.ValueAsync(
            () => balance.DailyBalancesAsync(TestUser.Owner(), Store, today),
            result => result.Count == 1 && result[0].EntryCount == EntryCount,
            TimeSpan.FromMinutes(5),
            TimeSpan.FromMilliseconds(100));

        TestContext.Current.TestOutputHelper?.WriteLine($"Saldo acertado {stopwatch.Elapsed.TotalSeconds:0.00}s depois de o Consolidado voltar.");

        Assert.Equal(total, rows[0].TotalCredits);
        Assert.Equal(0m, rows[0].TotalDebits);
        Assert.Equal(0u, await Rabbit.CountAsync(TestCredentials.Queue));
        Assert.Equal(0u, await Rabbit.CountAsync(TestCredentials.DeadLetterQueue));
    }

    [Fact(DisplayName = "RNF05: nenhum lançamento aceito se perde, o evento pendente no outbox é publicado")]
    [Trait("Requirement", "RNF05")]
    public async Task PendingOutboxMessagesArePublished()
    {
        var entries = await StartEntriesAsync();

        await entries.OpenCashRegisterAsync(Store);
        await entries.PostEntryAsync(Store, "Debit", 25m, "Compra de material");

        var pending = await Eventually.ValueAsync(
            () => Sql.ScalarAsync<int>(Databases.EntriesMigrations, "select count(*) from outbox_messages where published_at is null"),
            count => count == 0,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMilliseconds(250));

        Assert.Equal(0, pending);
        Assert.Equal(1, await Sql.ScalarAsync<int>(Databases.EntriesMigrations, "select count(*) from outbox_messages where published_at is not null"));
    }
}
