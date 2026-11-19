using Cashflow.Entries.Application;
using Cashflow.Entries.Application.Postings;
using Cashflow.Entries.Domain;
using Cashflow.Entries.Tests.Support;

namespace Cashflow.Entries.Tests.Application;

[Trait("Category", "Unit")]
public sealed class PostEntryHandlerTests
{
    private static readonly DateOnly Today = new(2026, 11, 19);

    private readonly FakeCashRegisterRepository repository = new();
    private readonly FakeUnitOfWork unitOfWork = new();
    private readonly FakeOutbox outbox = new();

    private PostEntryHandler CreateHandler(params string[] stores) =>
        new(repository, unitOfWork, outbox, new FakeCurrentUser("caixa.loja01", stores), Clock.At(Clock.Noon));

    private void GivenCashRegister(string storeId, bool closed = false)
    {
        var cashRegister = CashRegister.Open(storeId, Today, "caixa", Clock.Noon).Value!;
        if (closed)
        {
            cashRegister.Close("caixa", Clock.Noon);
        }

        repository.AddCashRegister(cashRegister);
    }

    [Fact(DisplayName = "RF02 e RNF05: lançar grava o lançamento e o evento do outbox na mesma transação")]
    [Trait("Requirement", "RF02")]
    [Trait("Requirement", "RNF05")]
    public async Task PostCreditPersistsEntryAndEvent()
    {
        GivenCashRegister("LOJA01");

        var result = await CreateHandler("LOJA01").HandleAsync(
            new PostEntryCommand("LOJA01", EntryType.Credit, 150m, "Troca de óleo"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var entry = Assert.Single(repository.Entries);
        var message = Assert.Single(outbox.Messages);
        Assert.Equal(entry.Id, message.EntryId);
        Assert.Equal("LOJA01", message.StoreId);
        Assert.Equal("Credit", message.Type);
        Assert.Equal(150m, message.Amount);
        Assert.Equal(1, unitOfWork.CommitCount);
    }

    [Fact(DisplayName = "RF10: o caixa não lança em outra loja e nenhum evento é gerado")]
    [Trait("Requirement", "RF10")]
    public async Task PostOtherStoreIsForbidden()
    {
        GivenCashRegister("LOJA02");

        var result = await CreateHandler("LOJA01").HandleAsync(
            new PostEntryCommand("LOJA02", EntryType.Credit, 10m, "Venda no balcão"),
            CancellationToken.None);

        Assert.Equal(ApplicationErrors.StoreForbidden, result.Error);
        Assert.Empty(repository.Entries);
        Assert.Empty(outbox.Messages);
    }

    [Fact(DisplayName = "RF04: com o caixa fechado o lançamento falha e nenhum evento é gerado")]
    [Trait("Requirement", "RF04")]
    public async Task PostClosedCashRegisterFails()
    {
        GivenCashRegister("LOJA01", closed: true);

        var result = await CreateHandler("LOJA01").HandleAsync(
            new PostEntryCommand("LOJA01", EntryType.Credit, 10m, "Venda no balcão"),
            CancellationToken.None);

        Assert.Equal(CashRegisterErrors.NotOpen, result.Error);
        Assert.Empty(repository.Entries);
        Assert.Empty(outbox.Messages);
    }
}
