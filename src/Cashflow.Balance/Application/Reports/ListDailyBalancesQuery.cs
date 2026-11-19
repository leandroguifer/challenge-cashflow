using Cashflow.Core.Messaging;

namespace Cashflow.Balance.Application.Reports;

internal sealed record ListDailyBalancesQuery(string? StoreId, DateOnly StartDate, DateOnly EndDate)
    : IQueryRequest<IReadOnlyList<DailyBalanceResponse>>;