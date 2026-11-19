namespace Cashflow.Integration.Tests.Support;

internal static class Eventually
{
    public static async Task<T> ValueAsync<T>(Func<Task<T>> read, Func<T, bool> done, TimeSpan timeout, TimeSpan? interval = null)
    {
        var pause = interval ?? TimeSpan.FromMilliseconds(100);
        var deadline = DateTimeOffset.UtcNow + timeout;
        T? last = default;

        while (DateTimeOffset.UtcNow < deadline)
        {
            try
            {
                last = await read();

                if (done(last))
                {
                    return last;
                }
            }
            catch (HttpRequestException)
            {
            }

            await Task.Delay(pause);
        }

        throw new TimeoutException($"Condition not met within {timeout.TotalSeconds:0.#}s. Last value: {last}");
    }
}
