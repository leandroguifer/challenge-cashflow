namespace Cashflow.Integration.Tests.Support;

internal sealed class EnvironmentScope : IDisposable
{
    private readonly Dictionary<string, string?> previous = [];

    public EnvironmentScope(IReadOnlyDictionary<string, string> values)
    {
        foreach (var (key, value) in values)
        {
            previous[key] = Environment.GetEnvironmentVariable(key);
            Environment.SetEnvironmentVariable(key, value);
        }
    }

    public void Dispose()
    {
        foreach (var (key, value) in previous)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
