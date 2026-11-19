using Cashflow.Core.Messaging;
using Cashflow.Entries.Domain;

namespace Cashflow.Entries.Application.Postings;

internal sealed record PostEntryCommand(
    string StoreId,
    EntryType Type,
    decimal Amount,
    string? Description) : ICommandRequest<EntryResponse>;