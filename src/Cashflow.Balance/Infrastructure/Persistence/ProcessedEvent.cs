namespace Cashflow.Balance.Infrastructure.Persistence;

internal sealed class ProcessedEvent
{
    public Guid EventId { get; set; }
    public DateTimeOffset ProcessedAt { get; set; }
}