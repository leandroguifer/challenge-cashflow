using Cashflow.Entries.Application.Abstractions;
using Cashflow.Entries.Domain;

namespace Cashflow.Entries.Tests.Support;

internal sealed class FakeCashRegisterRepository : ICashRegisterRepository
{
    public List<CashRegister> CashRegisters { get; } = [];
    public List<Entry> Entries { get; } = [];

    public Task<CashRegister?> FindAsync(string storeId, DateOnly businessDate, CancellationToken cancellationToken) =>
        Task.FromResult(CashRegisters.FirstOrDefault(x => x.StoreId == storeId && x.BusinessDate == businessDate));

    public void AddCashRegister(CashRegister cashRegister) => CashRegisters.Add(cashRegister);

    public void AddEntry(Entry entry) => Entries.Add(entry);
}
