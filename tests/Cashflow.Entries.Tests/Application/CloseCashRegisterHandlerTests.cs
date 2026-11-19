using Cashflow.Entries.Application.CashRegisters;
using Cashflow.Entries.Domain;
using Cashflow.Entries.Tests.Support;

namespace Cashflow.Entries.Tests.Application;

[Trait("Category", "Unit")]
public sealed class CloseCashRegisterHandlerTests
{
    [Fact(DisplayName = "RF04: fechar o caixa encerra o dia da loja")]
    [Trait("Requirement", "RF04")]
    public async Task CloseOpenCashRegisterClosesDay()
    {
        var repository = new FakeCashRegisterRepository();
        repository.AddCashRegister(CashRegister.Open("LOJA01", new DateOnly(2026, 11, 19), "caixa", Clock.Noon).Value!);
        var handler = new CloseCashRegisterHandler(
            repository,
            new FakeUnitOfWork(),
            new FakeCurrentUser("caixa.loja01", "LOJA01"),
            Clock.At(Clock.Noon));

        var result = await handler.HandleAsync(new CloseCashRegisterCommand("LOJA01"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CashRegisterStatus.Closed, result.Value!.Status);
    }
}
