using Cashflow.Balance.Application.Reports;

namespace Cashflow.Balance.Application.Abstractions;

internal interface IDailyBalanceReader
{
    Task<IReadOnlyList<DailyBalanceResponse>> ListAsync(string? storeId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken);
}