namespace Cashflow.Entries.Infrastructure.Outbox;

internal sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    public Guid Id { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string? TraceParent { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }

    public static OutboxMessage Create(Guid id, string type, string payload, string? traceParent, DateTimeOffset createdAt) => new()
    {
        Id = id,
        Type = type,
        Payload = payload,
        TraceParent = traceParent,
        CreatedAt = createdAt
    };

    public void MarkPublished(DateTimeOffset publishedAt) => PublishedAt = publishedAt;
}
