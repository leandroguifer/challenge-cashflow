using Microsoft.Extensions.Configuration;

namespace Cashflow.Entries.Application;

internal sealed class BusinessClock(TimeProvider timeProvider, IConfiguration configuration)
{
    private readonly TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(configuration["Business:TimeZone"] ?? "America/Sao_Paulo");

    public DateTimeOffset Now => timeProvider.GetUtcNow();

    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(Now, timeZone).DateTime);
}