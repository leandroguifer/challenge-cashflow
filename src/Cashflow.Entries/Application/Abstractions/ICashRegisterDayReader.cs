using Cashflow.Entries.Application.CashRegisters;

namespace Cashflow.Entries.Application.Abstractions;

internal interface ICashRegisterDayReader
{
    Task<CashRegisterDayResponse> GetAsync(string storeId, DateOnly businessDate, CancellationToken cancellationToken);
}
