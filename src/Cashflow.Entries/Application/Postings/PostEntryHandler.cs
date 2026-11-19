using Cashflow.Core.Messaging;
using Cashflow.Core.Results;
using Cashflow.Core.Security;
using Cashflow.Entries.Application.Abstractions;
using Cashflow.Entries.Contracts;

namespace Cashflow.Entries.Application.Postings;

internal sealed class PostEntryHandler(
    ICashRegisterRepository repository,
    IUnitOfWork unitOfWork,
    IOutbox outbox,
    ICurrentUser currentUser,
    BusinessClock clock) : IRequestHandler<PostEntryCommand, EntryResponse>
{
    public async Task<Result<EntryResponse>> HandleAsync(PostEntryCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.Stores.Contains(request.StoreId))
        {
            return Result.Failure<EntryResponse>(ApplicationErrors.StoreForbidden);
        }

        await unitOfWork.BeginAsync(cancellationToken);

        var cashRegister = await repository.FindAsync(request.StoreId, clock.Today, cancellationToken);
        if (cashRegister is null)
        {
            return Result.Failure<EntryResponse>(ApplicationErrors.CashRegisterNotFound);
        }

        var posted = cashRegister.PostEntry(request.Type, request.Amount, request.Description, currentUser.UserName, clock.Now);
        if (posted.IsFailure)
        {
            return Result.Failure<EntryResponse>(posted.Error!);
        }

        var entry = posted.Value!;
        repository.AddEntry(entry);
        outbox.Enqueue(new EntryPostedEvent(
            Guid.NewGuid(),
            entry.Id,
            entry.StoreId,
            entry.BusinessDate,
            entry.Type.ToString(),
            entry.Amount,
            entry.CreatedAt));

        var committed = await unitOfWork.CommitAsync(cancellationToken);
        if (committed.IsFailure)
        {
            return Result.Failure<EntryResponse>(committed.Error!);
        }

        return Result.Success(EntryResponse.From(entry));
    }
}