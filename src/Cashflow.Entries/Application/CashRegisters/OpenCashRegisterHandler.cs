using Cashflow.Core.Messaging;
using Cashflow.Core.Results;
using Cashflow.Core.Security;
using Cashflow.Entries.Application.Abstractions;
using Cashflow.Entries.Domain;

namespace Cashflow.Entries.Application.CashRegisters;

internal sealed class OpenCashRegisterHandler(
    ICashRegisterRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    BusinessClock clock) : IRequestHandler<OpenCashRegisterCommand, CashRegisterResponse>
{
    public async Task<Result<CashRegisterResponse>> HandleAsync(OpenCashRegisterCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.Stores.Contains(request.StoreId))
        {
            return Result.Failure<CashRegisterResponse>(ApplicationErrors.StoreForbidden);
        }

        await unitOfWork.BeginAsync(cancellationToken);

        var existing = await repository.FindAsync(request.StoreId, clock.Today, cancellationToken);
        if (existing is not null)
        {
            return Result.Failure<CashRegisterResponse>(ApplicationErrors.CashRegisterAlreadyOpened);
        }

        var opened = CashRegister.Open(request.StoreId, clock.Today, currentUser.UserName, clock.Now);
        if (opened.IsFailure)
        {
            return Result.Failure<CashRegisterResponse>(opened.Error!);
        }

        repository.AddCashRegister(opened.Value!);

        var committed = await unitOfWork.CommitAsync(cancellationToken);
        if (committed.IsFailure)
        {
            return Result.Failure<CashRegisterResponse>(committed.Error!);
        }

        return Result.Success(CashRegisterResponse.From(opened.Value!));
    }
}