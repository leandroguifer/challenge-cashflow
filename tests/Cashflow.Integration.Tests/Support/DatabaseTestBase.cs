namespace Cashflow.Integration.Tests.Support;

public abstract class DatabaseTestBase(InfrastructureFixture infrastructure) : IAsyncLifetime
{
    protected InfrastructureFixture Infrastructure => infrastructure;

    internal TestDatabases Databases { get; private set; } = null!;

    public virtual async ValueTask InitializeAsync() =>
        Databases = await TestDatabases.CreateAsync(infrastructure);

    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
