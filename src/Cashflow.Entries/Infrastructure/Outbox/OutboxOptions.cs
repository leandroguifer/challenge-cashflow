namespace Cashflow.Entries.Infrastructure.Outbox;

internal sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);
    public int BatchSize { get; set; } = 50;
}
