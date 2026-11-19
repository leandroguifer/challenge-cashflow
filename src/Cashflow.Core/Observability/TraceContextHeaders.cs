using System.Diagnostics;
using System.Text;

namespace Cashflow.Core.Observability;

public static class TraceContextHeaders
{
    public const string TraceParent = "traceparent";
    public const string TraceState = "tracestate";

    public static void Inject(Activity? activity, IDictionary<string, object?> headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        if (activity?.Id is null)
        {
            return;
        }

        headers[TraceParent] = activity.Id;

        if (!string.IsNullOrEmpty(activity.TraceStateString))
        {
            headers[TraceState] = activity.TraceStateString;
        }
    }

    public static string? Read(IDictionary<string, object?>? headers, string key)
    {
        if (headers is null || !headers.TryGetValue(key, out var value))
        {
            return null;
        }

        return value switch
        {
            string text => text,
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            _ => null
        };
    }
}
