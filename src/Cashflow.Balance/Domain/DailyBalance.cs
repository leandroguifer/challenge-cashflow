using Cashflow.Core.Results;

namespace Cashflow.Balance.Domain;

internal sealed class DailyBalance
{
    private DailyBalance()
    {
    }
    public string StoreId { get; private set; } = string.Empty;
    public DateOnly BusinessDate { get; private set; }
    public decimal TotalCredits { get; private set; }
    public decimal TotalDebits { get; private set; }
    public int EntriesCount { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public decimal TotalBalance => TotalCredits - TotalDebits;

    public static Result<DailyBalance> Create(string storeId, DateOnly businessDate,  DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(storeId))
            return Result.Failure<DailyBalance>(DailyBalanceErrors.StoreRequired);

        var dailyBalance = new DailyBalance
        {
            StoreId = storeId,
            BusinessDate = businessDate,
            UpdatedAt = now
        };

        return Result.Success(dailyBalance);
    }

    public Result Apply(EntryType type, decimal amount, DateTimeOffset now)
    {
        if (amount <= 0)
            return Result.Failure(DailyBalanceErrors.InvalidAmount);

        if (type == EntryType.Credit)
            TotalCredits += amount;
        else
            TotalDebits += amount;

        EntriesCount++;
        UpdatedAt = now;

        return Result.Success();
    }
}