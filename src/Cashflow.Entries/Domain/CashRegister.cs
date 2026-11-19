using Cashflow.Core.Results;

namespace Cashflow.Entries.Domain;

internal sealed class CashRegister
{
    private CashRegister()
    {
    }

    public Guid Id { get; private set; }
    public string StoreId { get; private set; } = string.Empty;
    public DateOnly BusinessDate { get; private set; }
    public CashRegisterStatus Status { get; private set; }
    public string OpenedBy { get; private set; } = string.Empty;
    public DateTimeOffset OpenedAt { get; private set; }
    public string? ClosedBy { get; private set; } = string.Empty;
    public DateTimeOffset? ClosedAt { get; private set; }

    public static Result<CashRegister> Open(string storeId, DateOnly businessDate, string openedBy)
    {
        if (string.IsNullOrWhiteSpace(storeId))
            return Result.Failure<CashRegister>(CashRegisterErrors.StoreRequired);

        var cashRegister = new CashRegister
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            BusinessDate = businessDate,
            Status = CashRegisterStatus.Open,
            OpenedBy = openedBy,
            OpenedAt = DateTimeOffset.UtcNow
        };

        return Result.Success(cashRegister);
    }

    public Result<Entry> PostEntry(EntryType type, decimal amount, string? description, string createdBy, DateTimeOffset now)
    {
        if (Status != CashRegisterStatus.Open)
            return Result.Failure<Entry>(CashRegisterErrors.NotOpen);

        return Entry.Create(Id, StoreId, BusinessDate, type, amount, description, createdBy, now);
    }

    public Result Close(string closedBy, DateTimeOffset now)
    {
        if (Status != CashRegisterStatus.Open)
            return Result.Failure(CashRegisterErrors.NotOpen);

        Status = CashRegisterStatus.Closed;
        ClosedBy = closedBy;
        ClosedAt = now;

        return Result.Success();
    }
}