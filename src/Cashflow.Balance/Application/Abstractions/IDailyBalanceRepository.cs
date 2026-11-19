using Cashflow.Balance.Domain;

namespace Cashflow.Balance.Application.Abstractions;

internal interface IDailyBalanceRepository
{
    Task<bool> WasProcessedAsync(Guid eventId, CancellationToken cancellationToken);

    Task<DailyBalance?> FindForUpdateAsync(string storeId, DateOnly businessDate, CancellationToken cancellationToken);

    void Add(DailyBalance dailyBalance);

    void MarkProcessed(Guid eventId, DateTimeOffset processedAt);
}