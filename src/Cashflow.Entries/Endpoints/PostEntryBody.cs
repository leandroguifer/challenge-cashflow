using Cashflow.Entries.Domain;

namespace Cashflow.Entries.Endpoints;

internal sealed record PostEntryBody(EntryType Type, decimal Amount, string? Description);