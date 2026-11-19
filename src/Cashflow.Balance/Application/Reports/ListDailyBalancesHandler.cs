using Cashflow.Balance.Application.Abstractions;
using Cashflow.Core.Messaging;
using Cashflow.Core.Results;
using Cashflow.Core.Security;

namespace Cashflow.Balance.Application.Reports;

internal sealed class ListDailyBalancesHandler(IDailyBalanceReader reader, ICurrentUser currentUser)
    : IRequestHandler<ListDailyBalancesQuery, IReadOnlyList<DailyBalanceResponse>>
{
    public async Task<Result<IReadOnlyList<DailyBalanceResponse>>> HandleAsync(ListDailyBalancesQuery request, CancellationToken cancellationToken)
    {
        if (request.StartDate > request.EndDate)
        {
            return Result.Failure<IReadOnlyList<DailyBalanceResponse>>(ApplicationErrors.InvalidPeriod);
        }

        if (request.StoreId is not null && !currentUser.Stores.Contains(request.StoreId))
        {
            return Result.Failure<IReadOnlyList<DailyBalanceResponse>>(ApplicationErrors.StoreForbidden);
        }

        var items = await reader.ListAsync(request.StoreId, request.StartDate, request.EndDate, cancellationToken);

        return Result.Success(items);
    }
}