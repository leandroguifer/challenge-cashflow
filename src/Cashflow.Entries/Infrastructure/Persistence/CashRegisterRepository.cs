using Cashflow.Entries.Application.Abstractions;
using Cashflow.Entries.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Entries.Infrastructure.Persistence;

internal sealed class CashRegisterRepository(EntriesDbContext context) : ICashRegisterRepository
{
    public Task<CashRegister?> FindAsync(string storeId, DateOnly businessDate, CancellationToken cancellationToken) =>
        context.CashRegisters.FirstOrDefaultAsync(x => x.StoreId == storeId && x.BusinessDate == businessDate, cancellationToken);

    public void AddCashRegister(CashRegister cashRegister) => context.CashRegisters.Add(cashRegister);

    public void AddEntry(Entry entry) => context.Entries.Add(entry);
}