using Cashflow.Balance.Application.Abstractions;
using Cashflow.Balance.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Balance.Infrastructure.Persistence;

internal sealed class DailyBalanceRepository(BalanceDbContext context) : IDailyBalanceRepository
{
    public Task<bool> WasProcessedAsync(Guid eventId, CancellationToken cancellationToken) =>
        context.ProcessedEvents.AnyAsync(x => x.EventId == eventId, cancellationToken);

    public Task<DailyBalance?> FindForUpdateAsync(string storeId, DateOnly businessDate, CancellationToken cancellationToken) =>
        context.DailyBalances
            .FromSql($"select * from daily_balances where store_id = {storeId} and business_date = {businessDate} for update")
            .SingleOrDefaultAsync(cancellationToken);

    public void Add(DailyBalance dailyBalance) => context.DailyBalances.Add(dailyBalance);

    public void MarkProcessed(Guid eventId, DateTimeOffset processedAt) =>
        context.ProcessedEvents.Add(new ProcessedEvent { EventId = eventId, ProcessedAt = processedAt });
}