using System.Diagnostics;
using System.Net;
using Cashflow.Integration.Tests.Support;

namespace Cashflow.Integration.Tests;

[Trait("Category", "Integration")]
[Collection(SequentialServices.Name)]
public sealed class EntryToBalanceFlowTests(InfrastructureFixture infrastructure) : ServicesTestBase(infrastructure)
{
    private const string Store = "LOJA01";
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(5);

    [Fact(DisplayName = "RNF03: o saldo é atualizado em menos de 5 segundos depois de cada lançamento")]
    [Trait("Requirement", "RNF03")]
    public async Task BalanceIsUpdatedWithinFiveSeconds()
    {
        var entries = await StartEntriesAsync();
        var balance = await StartBalanceAsync();
        var owner = TestUser.Owner();
        var today = BusinessDay.Today();

        using var opened = await entries.OpenCashRegisterAsync(Store);
        Assert.Equal(HttpStatusCode.OK, opened.StatusCode);

        var elapsed = new List<TimeSpan>();
        decimal expected = 0;

        for (var index = 1; index <= 5; index++)
        {
            var amount = 10m * index;
            expected += amount;

            var stopwatch = Stopwatch.StartNew();
            using var posted = await entries.PostEntryAsync(Store, "Credit", amount, $"Lançamento de medição {index}");
            Assert.Equal(HttpStatusCode.OK, posted.StatusCode);

            await Eventually.ValueAsync(
                () => balance.DailyBalancesAsync(owner, Store, today),
                rows => rows.Count == 1 && rows[0].TotalCredits == expected,
                TimeSpan.FromSeconds(30),
                TimeSpan.FromMilliseconds(50));

            elapsed.Add(stopwatch.Elapsed);
        }

        TestContext.Current.TestOutputHelper?.WriteLine("Latência do lançamento até o saldo (ms): " + string.Join(", ", elapsed.Select(time => time.TotalMilliseconds.ToString("0", System.Globalization.CultureInfo.InvariantCulture))));

        Assert.All(elapsed, time => Assert.True(time < Limit, $"Atualização levou {time.TotalSeconds:0.00}s"));
    }

    [Fact(DisplayName = "RF05 e RF07: crédito e débito resultam no saldo do dia da loja e da empresa")]
    [Trait("Requirement", "RF05")]
    [Trait("Requirement", "RF07")]
    public async Task CreditsAndDebitsProduceTheDailyBalance()
    {
        var entries = await StartEntriesAsync();
        var balance = await StartBalanceAsync();
        var owner = TestUser.Owner();
        var today = BusinessDay.Today();

        await entries.OpenCashRegisterAsync(Store);
        await entries.OpenCashRegisterAsync("LOJA02");
        await entries.PostEntryAsync(Store, "Credit", 100m, "Venda no balcão");
        await entries.PostEntryAsync(Store, "Debit", 30m, "Compra de material");
        await entries.PostEntryAsync("LOJA02", "Credit", 250m, "Serviço de revisão");

        var company = await Eventually.ValueAsync(
            () => balance.CompanyBalancesAsync(owner, today),
            rows => rows.Count == 1 && rows[0].EntryCount == 3,
            TimeSpan.FromSeconds(30));

        var stores = await balance.DailyBalancesAsync(owner, null, today);

        Assert.Equal(350m, company[0].TotalCredits);
        Assert.Equal(30m, company[0].TotalDebits);
        Assert.Equal(320m, company[0].Balance);
        var first = Assert.Single(stores, row => row.StoreId == Store);
        Assert.Equal(70m, first.Balance);
        Assert.Equal(2, first.EntryCount);
        Assert.Equal(250m, Assert.Single(stores, row => row.StoreId == "LOJA02").Balance);
    }

    [Fact(DisplayName = "RF08: os lançamentos do dia podem ser consultados por loja")]
    [Trait("Requirement", "RF08")]
    public async Task EntriesOfTheDayCanBeListed()
    {
        var entries = await StartEntriesAsync();

        await entries.OpenCashRegisterAsync(Store);
        await entries.PostEntryAsync(Store, "Credit", 100m, "Venda no balcão");
        await entries.PostEntryAsync(Store, "Debit", 30m, "Compra de material");

        using var response = await entries.ListEntriesAsync(Store, TestUser.Assistant(Store));
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Venda no balcão", body, StringComparison.Ordinal);
        Assert.Contains("Compra de material", body, StringComparison.Ordinal);
    }
}
