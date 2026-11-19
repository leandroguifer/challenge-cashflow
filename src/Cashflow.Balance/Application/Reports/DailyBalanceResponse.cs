namespace Cashflow.Balance.Application.Reports;

internal sealed record DailyBalanceResponse
{
    public string StoreId { get; init; } = string.Empty;
    public DateOnly BusinessDate { get; init; }
    public decimal TotalCredits { get; init; }
    public decimal TotalDebits { get; init; }
    public decimal Balance { get; init; }
    public int EntryCount { get; init; }
}