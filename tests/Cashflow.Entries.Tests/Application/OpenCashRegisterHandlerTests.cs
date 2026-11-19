using Cashflow.Entries.Application;
using Cashflow.Entries.Application.CashRegisters;
using Cashflow.Entries.Tests.Support;

namespace Cashflow.Entries.Tests.Application;

[Trait("Category", "Unit")]
public sealed class OpenCashRegisterHandlerTests
{
    [Fact(DisplayName = "RF01: cada loja tem um caixa aberto por dia, a segunda abertura dá conflito")]
    [Trait("Requirement", "RF01")]
    public async Task OpenTwiceConflicts()
    {
        var repository = new FakeCashRegisterRepository();
        var handler = new OpenCashRegisterHandler(
            repository,
            new FakeUnitOfWork(),
            new FakeCurrentUser("caixa.loja01", "LOJA01"),
            Clock.At(Clock.Noon));
        await handler.HandleAsync(new OpenCashRegisterCommand("LOJA01"), CancellationToken.None);

        var result = await handler.HandleAsync(new OpenCashRegisterCommand("LOJA01"), CancellationToken.None);

        Assert.Equal(ApplicationErrors.CashRegisterAlreadyOpened, result.Error);
        Assert.Single(repository.CashRegisters);
    }
}
