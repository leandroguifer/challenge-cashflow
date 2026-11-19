using Cashflow.Entries.Application;
using Microsoft.Extensions.Configuration;

namespace Cashflow.Entries.Tests.Support;

internal static class Clock
{
    public static readonly DateTimeOffset Noon = new(2026, 11, 19, 15, 0, 0, TimeSpan.Zero);

    public static BusinessClock At(DateTimeOffset utcNow) =>
        new(new FixedTimeProvider(utcNow), new ConfigurationBuilder().Build());
}
