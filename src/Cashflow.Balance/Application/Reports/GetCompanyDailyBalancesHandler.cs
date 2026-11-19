using Cashflow.Balance.Application.Abstractions;
using Cashflow.Core.Messaging;
using Cashflow.Core.Results;

namespace Cashflow.Balance.Application.Reports;

internal sealed class GetCompanyDailyBalancesHandler(IDailyBalanceReader reader)
    : IRequestHandler<GetCompanyDailyBalancesQuery, IReadOnlyList<CompanyDailyBalanceResponse>>
{
    public async Task<Result<IReadOnlyList<CompanyDailyBalanceResponse>>> HandleAsync(GetCompanyDailyBalancesQuery request, CancellationToken cancellationToken)
    {
        if (request.StartDate > request.EndDate)
        {
            return Result.Failure<IReadOnlyList<CompanyDailyBalanceResponse>>(ApplicationErrors.InvalidPeriod);
        }

        var items = await reader.ListCompanyAsync(request.StartDate, request.EndDate, cancellationToken);

        return Result.Success(items);
    }
}
