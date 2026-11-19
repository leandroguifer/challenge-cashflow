using Cashflow.Entries.Domain;

namespace Cashflow.Entries.Application.Abstractions;

internal interface ICashRegisterRepository
{
    Task<CashRegister?> FindAsync(string storeId, DateOnly businessDate, CancellationToken cancellationToken);

    void AddCashRegister(CashRegister cashRegister);

    void AddEntry(Entry entry);
}