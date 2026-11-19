using Cashflow.Balance;
using Cashflow.Entries;

namespace Cashflow.Integration.Tests.Support;

public abstract class ServicesTestBase(InfrastructureFixture infrastructure) : DatabaseTestBase(infrastructure)
{
    private readonly List<IAsyncDisposable> hosts = [];

    internal RabbitTestClient Rabbit { get; } = new(infrastructure);

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        await Rabbit.PurgeAsync(TestCredentials.Queue);
        await Rabbit.PurgeAsync(TestCredentials.DeadLetterQueue);
    }

    public override async ValueTask DisposeAsync()
    {
        for (var index = hosts.Count - 1; index >= 0; index--)
        {
            await hosts[index].DisposeAsync();
        }

        await base.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    internal async Task<ApiHost<EntriesModule>> StartEntriesAsync()
    {
        var host = await ApiHost<EntriesModule>.StartAsync(ServiceSettings.Entries(Infrastructure, Databases));
        hosts.Add(host);
        return host;
    }

    internal async Task<ApiHost<BalanceModule>> StartBalanceAsync()
    {
        var host = await ApiHost<BalanceModule>.StartAsync(ServiceSettings.Balance(Infrastructure, Databases));
        hosts.Add(host);
        return host;
    }
}
