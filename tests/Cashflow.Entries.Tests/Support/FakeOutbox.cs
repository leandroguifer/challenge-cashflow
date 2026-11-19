using Cashflow.Entries.Application.Abstractions;
using Cashflow.Entries.Contracts;

namespace Cashflow.Entries.Tests.Support;

internal sealed class FakeOutbox : IOutbox
{
    public List<EntryPostedEvent> Messages { get; } = [];

    public void Enqueue(EntryPostedEvent message) => Messages.Add(message);
}
