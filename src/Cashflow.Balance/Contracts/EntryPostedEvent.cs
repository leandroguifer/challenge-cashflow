namespace Cashflow.Balance.Contracts;

internal sealed record EntryPostedEvent(
    Guid EventId,
    Guid EntryId,
    string StoreId,
    DateOnly BusinessDate,
    string Type,
    decimal Amount,
    DateTimeOffset OccurredAt);