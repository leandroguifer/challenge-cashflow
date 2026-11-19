using Cashflow.Core.Messaging;

namespace Cashflow.Balance.Application.Reports;

internal sealed record GetCompanyDailyBalancesQuery(DateOnly StartDate, DateOnly EndDate)
    : IQueryRequest<IReadOnlyList<CompanyDailyBalanceResponse>>;
