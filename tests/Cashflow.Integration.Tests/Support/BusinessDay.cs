namespace Cashflow.Integration.Tests.Support;

internal static class BusinessDay
{
    public static DateOnly Today()
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).DateTime);
    }
}
