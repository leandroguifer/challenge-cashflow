using Cashflow.Core.Messaging;
using Cashflow.Core.Results;
using Cashflow.Core.Security;
using Cashflow.Entries.Application.Abstractions;

namespace Cashflow.Entries.Application.CashRegisters;

internal sealed class GetCashRegisterDayHandler(
    ICashRegisterDayReader reader,
    ICurrentUser currentUser,
    BusinessClock clock) : IRequestHandler<GetCashRegisterDayQuery, CashRegisterDayResponse>
{
    public async Task<Result<CashRegisterDayResponse>> HandleAsync(GetCashRegisterDayQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.Stores.Contains(request.StoreId))
        {
            return Result.Failure<CashRegisterDayResponse>(ApplicationErrors.StoreForbidden);
        }

        var day = await reader.GetAsync(request.StoreId, clock.Today, cancellationToken);

        return Result.Success(day);
    }
}
