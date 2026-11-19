namespace Cashflow.Integration.Tests.Support;

internal sealed record DailyBalanceDto(
    string StoreId,
    DateOnly BusinessDate,
    decimal TotalCredits,
    decimal TotalDebits,
    decimal Balance,
    int EntryCount);
