namespace Cashflow.Integration.Tests.Support;

internal sealed record CompanyBalanceDto(
    DateOnly BusinessDate,
    decimal TotalCredits,
    decimal TotalDebits,
    decimal Balance,
    int EntryCount);
