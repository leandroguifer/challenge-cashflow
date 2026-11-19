using Cashflow.Balance.Application.Abstractions;
using Cashflow.Balance.Domain;
using Cashflow.Core.Messaging;
using Cashflow.Core.Results;

namespace Cashflow.Balance.Application.Consumption;

internal sealed class ApplyEntryPostedHandler(
    IDailyBalanceRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IRequestHandler<ApplyEntryPostedCommand, ApplyEntryPostedResponse>
{
    public async Task<Result<ApplyEntryPostedResponse>> HandleAsync(ApplyEntryPostedCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginAsync(cancellationToken);

        if (await repository.WasProcessedAsync(request.EventId, cancellationToken))
        {
            return Result.Success(new ApplyEntryPostedResponse(false));
        }

        var now = timeProvider.GetUtcNow();

        var dailyBalance = await repository.FindForUpdateAsync(request.StoreId, request.BusinessDate, cancellationToken);
        if (dailyBalance is null)
        {
            var created = DailyBalance.Create(request.StoreId, request.BusinessDate, now);
            if (created.IsFailure)
            {
                return Result.Failure<ApplyEntryPostedResponse>(created.Error!);
            }

            dailyBalance = created.Value!;
            repository.Add(dailyBalance);
        }

        var applied = dailyBalance.Apply(request.Type, request.Amount, now);
        if (applied.IsFailure)
        {
            return Result.Failure<ApplyEntryPostedResponse>(applied.Error!);
        }

        repository.MarkProcessed(request.EventId, now);

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new ApplyEntryPostedResponse(true));
    }
}