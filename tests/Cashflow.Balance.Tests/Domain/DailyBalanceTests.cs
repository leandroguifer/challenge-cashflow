using Cashflow.Balance.Domain;

namespace Cashflow.Balance.Tests.Domain;

[Trait("Category", "Unit")]
public sealed class DailyBalanceTests
{
    private static readonly DateOnly Day = new(2026, 11, 19);
    private static readonly DateTimeOffset Now = new(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);

    [Fact(DisplayName = "RF05 e RF06: o saldo do dia é créditos menos débitos")]
    [Trait("Requirement", "RF05")]
    [Trait("Requirement", "RF06")]
    public void BalanceIsCreditsMinusDebits()
    {
        var balance = DailyBalance.Create("LOJA01", Day, Now).Value!;

        balance.Apply(EntryType.Credit, 250m, Now);
        balance.Apply(EntryType.Credit, 50.50m, Now);
        balance.Apply(EntryType.Debit, 40m, Now);

        Assert.Equal(300.50m, balance.TotalCredits);
        Assert.Equal(40m, balance.TotalDebits);
        Assert.Equal(260.50m, balance.TotalBalance);
        Assert.Equal(3, balance.EntriesCount);
    }
}
