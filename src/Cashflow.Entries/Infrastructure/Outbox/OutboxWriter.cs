using System.Text.Json;
using Cashflow.Core.Observability;
using Cashflow.Entries.Application.Abstractions;
using Cashflow.Entries.Contracts;
using Cashflow.Entries.Infrastructure.Persistence;

namespace Cashflow.Entries.Infrastructure.Outbox;

internal sealed class OutboxWriter(EntriesDbContext context) : IOutbox
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Enqueue(EntryPostedEvent message) =>
        context.OutboxMessages.Add(OutboxMessage.Create(
            message.EventId,
            OutboxTopology.EntryPostedRoutingKey,
            JsonSerializer.Serialize(message, JsonOptions),
            MessagingTelemetry.CurrentTraceParent(),
            message.OccurredAt));
}
