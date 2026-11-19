namespace Cashflow.Entries.Infrastructure.Outbox;

internal static class OutboxTopology
{
    public const string Exchange = "entries.events";
    public const string EntryPostedRoutingKey = "entries.entry-posted";
}
