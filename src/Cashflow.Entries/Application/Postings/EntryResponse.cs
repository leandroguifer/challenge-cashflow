using Cashflow.Entries.Domain;

namespace Cashflow.Entries.Application.Postings;

internal sealed record EntryResponse(
    Guid Id,
    Guid CashRegisterId,
    string StoreId,
    DateOnly BusinessDate,
    EntryType Type,
    decimal Amount,
    string? Description,
    DateTimeOffset CreatedAt)
{
    public static EntryResponse From(Entry entry) => new(
        entry.Id,
        entry.CashRegisterId,
        entry.StoreId,
        entry.BusinessDate,
        entry.Type,
        entry.Amount,
        entry.Description,
        entry.CreatedAt);
}