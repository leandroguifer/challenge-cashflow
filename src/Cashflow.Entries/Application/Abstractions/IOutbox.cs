using Cashflow.Entries.Contracts;

namespace Cashflow.Entries.Application.Abstractions;

internal interface IOutbox
{
    void Enqueue(EntryPostedEvent message);
}
