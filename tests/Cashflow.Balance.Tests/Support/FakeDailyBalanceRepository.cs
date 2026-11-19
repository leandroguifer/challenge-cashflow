using Cashflow.Balance.Application.Abstractions;
using Cashflow.Balance.Domain;

namespace Cashflow.Balance.Tests.Support;

internal sealed class FakeDailyBalanceRepository : IDailyBalanceRepository
{
    public List<DailyBalance> Balances { get; } = [];
    public HashSet<Guid> ProcessedEvents { get; } = [];

    public Task<bool> WasProcessedAsync(Guid eventId, CancellationToken cancellationToken) =>
        Task.FromResult(ProcessedEvents.Contains(eventId));

    public Task<DailyBalance?> FindForUpdateAsync(string storeId, DateOnly businessDate, CancellationToken cancellationToken) =>
        Task.FromResult(Balances.FirstOrDefault(x => x.StoreId == storeId && x.BusinessDate == businessDate));

    public void Add(DailyBalance dailyBalance) => Balances.Add(dailyBalance);

    public void MarkProcessed(Guid eventId, DateTimeOffset processedAt) => ProcessedEvents.Add(eventId);
}
