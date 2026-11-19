using Cashflow.Core.Results;

namespace Cashflow.Entries.Domain;

internal sealed class Entry
{
    public const int MinDescriptionLength = 5;
    public const int MaxDescriptionLength = 200;

    private Entry()
    {
    }
    public Guid Id { get; private set; }
    public Guid CashRegisterId { get; private set; }
    public string StoreId { get; private set; } = string.Empty;
    public DateOnly BusinessDate { get; private set; }
    public EntryType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string? Description { get; private set; } = string.Empty;
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    public static Result<Entry> Create(Guid cashRegisterId, string storeId, DateOnly businessDate, EntryType type, decimal amount, string? description, string createdBy, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(storeId))
            return Result.Failure<Entry>(EntryErrors.StoreRequired);

        if (amount <= 0)
            return Result.Failure<Entry>(EntryErrors.InvalidAmount);

        if (string.IsNullOrWhiteSpace(description) || description.Length < MinDescriptionLength)
            return Result.Failure<Entry>(EntryErrors.DescriptionTooShort);

        if (description.Length > MaxDescriptionLength)
            return Result.Failure<Entry>(EntryErrors.DescriptionTooLong);

        var entry = new Entry
        {
            Id = Guid.NewGuid(),
            CashRegisterId = cashRegisterId,
            StoreId = storeId,
            BusinessDate = businessDate,
            Type = type,
            Amount = amount,
            Description = description,
            CreatedBy = createdBy,
            CreatedAt = createdAt
        };

        return Result.Success(entry);
    }
}