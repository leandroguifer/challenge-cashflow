using Cashflow.Core.Messaging;
using Cashflow.Core.Results;
using Cashflow.Core.Security;
using Cashflow.Entries.Application.Abstractions;
using Cashflow.Entries.Application.Postings;

namespace Cashflow.Entries.Application.CashRegisters;

internal sealed class GetCashRegisterDayHandler(
    ICashRegisterRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    BusinessClock clock) : IRequestHandler<GetCashRegisterDayQuery, CashRegisterDayResponse>
{
    public async Task<Result<CashRegisterDayResponse>> HandleAsync(GetCashRegisterDayQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.Stores.Contains(request.StoreId))
        {
            return Result.Failure<CashRegisterDayResponse>(ApplicationErrors.StoreForbidden);
        }

        await unitOfWork.BeginAsync(cancellationToken);

        var cashRegister = await repository.FindAsync(request.StoreId, clock.Today, cancellationToken);
        if (cashRegister is null)
        {
            return Result.Success(new CashRegisterDayResponse(null, []));
        }

        var entries = await repository.ListEntriesAsync(cashRegister.Id, cancellationToken);

        return Result.Success(new CashRegisterDayResponse(
            CashRegisterResponse.From(cashRegister),
            entries.Select(EntryResponse.From).ToList()));
    }
}
