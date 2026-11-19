using System.Diagnostics;

namespace Cashflow.Core.Observability;

public static class MessagingTelemetry
{
    public const string SourceName = "Cashflow.Messaging";

    private static readonly ActivitySource Source = new(SourceName);

    public static string? CurrentTraceParent()
    {
        var current = Activity.Current;
        return current is { IdFormat: ActivityIdFormat.W3C } ? current.Id : null;
    }

    public static Activity? StartProducer(string name, string? traceParent)
    {
        ActivityContext.TryParse(traceParent, null, out var parent);
        return Source.StartActivity(name, ActivityKind.Producer, parent);
    }

    public static Activity? StartConsumer(string name, IDictionary<string, object?>? headers)
    {
        ActivityContext.TryParse(
            TraceContextHeaders.Read(headers, TraceContextHeaders.TraceParent),
            TraceContextHeaders.Read(headers, TraceContextHeaders.TraceState),
            out var parent);

        return Source.StartActivity(name, ActivityKind.Consumer, parent);
    }
}
