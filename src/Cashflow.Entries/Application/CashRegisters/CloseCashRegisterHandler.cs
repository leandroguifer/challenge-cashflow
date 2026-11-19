using Cashflow.Core.Messaging;
using Cashflow.Core.Results;
using Cashflow.Core.Security;
using Cashflow.Entries.Application.Abstractions;

namespace Cashflow.Entries.Application.CashRegisters;

internal sealed class CloseCashRegisterHandler(
    ICashRegisterRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    BusinessClock clock) : IRequestHandler<CloseCashRegisterCommand, CashRegisterResponse>
{
    public async Task<Result<CashRegisterResponse>> HandleAsync(CloseCashRegisterCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.Stores.Contains(request.StoreId))
        {
            return Result.Failure<CashRegisterResponse>(ApplicationErrors.StoreForbidden);
        }

        await unitOfWork.BeginAsync(cancellationToken);

        var cashRegister = await repository.FindAsync(request.StoreId, clock.Today, cancellationToken);
        if (cashRegister is null)
        {
            return Result.Failure<CashRegisterResponse>(ApplicationErrors.CashRegisterNotFound);
        }

        var closed = cashRegister.Close(currentUser.UserName, clock.Now);
        if (closed.IsFailure)
        {
            return Result.Failure<CashRegisterResponse>(closed.Error!);
        }

        var committed = await unitOfWork.CommitAsync(cancellationToken);
        if (committed.IsFailure)
        {
            return Result.Failure<CashRegisterResponse>(committed.Error!);
        }

        return Result.Success(CashRegisterResponse.From(cashRegister));
    }
}