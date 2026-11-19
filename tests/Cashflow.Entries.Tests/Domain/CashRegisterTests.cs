using Cashflow.Entries.Domain;

namespace Cashflow.Entries.Tests.Domain;

[Trait("Category", "Unit")]
public sealed class CashRegisterTests
{
    private static readonly DateOnly Day = new(2026, 11, 19);
    private static readonly DateTimeOffset Now = new(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);

    [Fact(DisplayName = "RF01: abrir o caixa cria um caixa aberto da loja no dia")]
    [Trait("Requirement", "RF01")]
    public void OpenCreatesOpenCashRegister()
    {
        var result = CashRegister.Open("LOJA01", Day, "caixa.loja01", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("LOJA01", result.Value!.StoreId);
        Assert.Equal(Day, result.Value.BusinessDate);
        Assert.Equal(CashRegisterStatus.Open, result.Value.Status);
    }

    [Fact(DisplayName = "RF02 e RF03: com o caixa aberto, entrada e saída viram lançamentos do caixa")]
    [Trait("Requirement", "RF02")]
    [Trait("Requirement", "RF03")]
    public void PostEntryOpenCashRegisterCreatesEntry()
    {
        var cashRegister = CashRegister.Open("LOJA01", Day, "caixa", Now).Value!;

        var credit = cashRegister.PostEntry(EntryType.Credit, 150.50m, "Troca de óleo", "caixa", Now);
        var debit = cashRegister.PostEntry(EntryType.Debit, 40m, "Compra de filtros", "caixa", Now);

        Assert.True(credit.IsSuccess);
        Assert.True(debit.IsSuccess);
        Assert.Equal(cashRegister.Id, credit.Value!.CashRegisterId);
        Assert.Equal(EntryType.Credit, credit.Value.Type);
        Assert.Equal(EntryType.Debit, debit.Value!.Type);
    }

    [Fact(DisplayName = "RF04: caixa fechado não aceita mais lançamentos")]
    [Trait("Requirement", "RF04")]
    public void PostEntryClosedCashRegisterFails()
    {
        var cashRegister = CashRegister.Open("LOJA01", Day, "caixa", Now).Value!;
        cashRegister.Close("caixa", Now.AddHours(1));

        var result = cashRegister.PostEntry(EntryType.Debit, 10m, "Compra de filtros", "caixa", Now);

        Assert.True(result.IsFailure);
        Assert.Equal(CashRegisterErrors.NotOpen, result.Error);
    }
}
