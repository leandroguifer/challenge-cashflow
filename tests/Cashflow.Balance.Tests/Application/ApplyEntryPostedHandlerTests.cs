using Cashflow.Balance.Application.Consumption;
using Cashflow.Balance.Domain;
using Cashflow.Balance.Tests.Support;

namespace Cashflow.Balance.Tests.Application;

[Trait("Category", "Unit")]
public sealed class ApplyEntryPostedHandlerTests
{
    private static readonly DateOnly Day = new(2026, 11, 19);
    private static readonly DateTimeOffset Now = new(2026, 11, 19, 15, 0, 0, TimeSpan.Zero);

    private readonly FakeDailyBalanceRepository repository = new();
    private readonly FakeUnitOfWork unitOfWork = new();

    private ApplyEntryPostedHandler CreateHandler() => new(repository, unitOfWork, new FixedTimeProvider(Now));

    private static ApplyEntryPostedCommand Command(
        Guid? eventId = null,
        string storeId = "LOJA01",
        DateOnly? day = null,
        EntryType type = EntryType.Credit,
        decimal amount = 100m) =>
        new(eventId ?? Guid.NewGuid(), storeId, day ?? Day, type, amount);

    [Fact(DisplayName = "RF05: cada evento de lançamento do mesmo dia acumula no saldo da loja")]
    [Trait("Requirement", "RF05")]
    public async Task NextEventsAccumulate()
    {
        var handler = CreateHandler();

        await handler.HandleAsync(Command(type: EntryType.Credit, amount: 250m), CancellationToken.None);
        await handler.HandleAsync(Command(type: EntryType.Debit, amount: 40m), CancellationToken.None);

        var balance = Assert.Single(repository.Balances);
        Assert.Equal(210m, balance.TotalBalance);
        Assert.Equal(2, balance.EntriesCount);
    }

    [Fact(DisplayName = "RF05: lojas e dias diferentes têm saldos separados")]
    [Trait("Requirement", "RF05")]
    public async Task DifferentStoresAndDaysAreSeparate()
    {
        var handler = CreateHandler();

        await handler.HandleAsync(Command(storeId: "LOJA01", amount: 10m), CancellationToken.None);
        await handler.HandleAsync(Command(storeId: "LOJA02", amount: 20m), CancellationToken.None);
        await handler.HandleAsync(Command(storeId: "LOJA01", day: Day.AddDays(1), amount: 30m), CancellationToken.None);

        Assert.Equal(3, repository.Balances.Count);
        Assert.Equal(10m, repository.Balances.Single(x => x.StoreId == "LOJA01" && x.BusinessDate == Day).TotalCredits);
        Assert.Equal(20m, repository.Balances.Single(x => x.StoreId == "LOJA02").TotalCredits);
        Assert.Equal(30m, repository.Balances.Single(x => x.BusinessDate == Day.AddDays(1)).TotalCredits);
    }

    [Fact(DisplayName = "RNF05: o mesmo evento entregue de novo não altera o saldo")]
    [Trait("Requirement", "RNF05")]
    public async Task RepeatedEventDoesNotChangeBalance()
    {
        var handler = CreateHandler();
        var command = Command(amount: 100m);
        await handler.HandleAsync(command, CancellationToken.None);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.Applied);
        var balance = Assert.Single(repository.Balances);
        Assert.Equal(100m, balance.TotalCredits);
        Assert.Equal(1, balance.EntriesCount);
    }

    [Fact(DisplayName = "RNF05: só o identificador do evento evita a repetição, lançamentos iguais com outro evento contam")]
    [Trait("Requirement", "RNF05")]
    public async Task SameDataWithDifferentEventIdCountsAgain()
    {
        var handler = CreateHandler();

        await handler.HandleAsync(Command(amount: 100m), CancellationToken.None);
        await handler.HandleAsync(Command(amount: 100m), CancellationToken.None);

        Assert.Equal(200m, Assert.Single(repository.Balances).TotalCredits);
    }
}
