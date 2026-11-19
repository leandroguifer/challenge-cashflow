namespace Cashflow.Integration.Tests.Support;

public sealed class DatabasesFixture(InfrastructureFixture infrastructure) : IAsyncLifetime
{
    private TestDatabases? databases;

    internal TestDatabases Databases
    {
        get
        {
            infrastructure.RequireAvailable();
            return databases!;
        }
    }

    public async ValueTask InitializeAsync()
    {
        if (infrastructure.UnavailableReason is null)
        {
            databases = await TestDatabases.CreateAsync(infrastructure);
        }
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
