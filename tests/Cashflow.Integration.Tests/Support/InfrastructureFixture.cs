using DotNet.Testcontainers.Configurations;
using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Cashflow.Integration.Tests.Support;

public sealed class InfrastructureFixture : IAsyncLifetime
{
    private const string SkipVariable = "CASHFLOW_TESTS_SKIP_DOCKER";
    private const string SuperUser = "postgres";
    private const string SuperPassword = "postgres";

    private PostgreSqlContainer? postgres;
    private RabbitMqContainer? rabbitMq;
    private string? definitionsFile;

    public string? UnavailableReason { get; private set; }

    internal string PostgresHost => postgres!.Hostname;

    internal int PostgresPort => postgres!.GetMappedPublicPort(5432);

    internal string RabbitHost => rabbitMq!.Hostname;

    internal int RabbitPort => rabbitMq!.GetMappedPublicPort(5672);

    public async ValueTask InitializeAsync()
    {
        if (Environment.GetEnvironmentVariable(SkipVariable) is "1" or "true")
        {
            UnavailableReason = $"Integration tests skipped: {SkipVariable} is set.";
            return;
        }

        try
        {
            postgres = new PostgreSqlBuilder("postgres:17")
                .WithUsername(SuperUser)
                .WithPassword(SuperPassword)
                .Build();

            definitionsFile = Path.Combine(Path.GetTempPath(), $"cashflow-rabbit-{Guid.NewGuid():N}.json");
            await File.WriteAllBytesAsync(definitionsFile, RabbitDefinitions.RenderDefinitions());

            rabbitMq = new RabbitMqBuilder("rabbitmq:4-management")
                .WithBindMount(Path.Combine(Repository.Root, "docker", "rabbitmq", "rabbitmq.conf"), "/etc/rabbitmq/conf.d/20-definitions.conf", AccessMode.ReadOnly)
                .WithBindMount(definitionsFile, "/tmp/definitions.json", AccessMode.ReadOnly)
                .Build();

            await Task.WhenAll(postgres.StartAsync(), rabbitMq.StartAsync());
            await CreateRolesAsync();
        }
        catch (Exception exception)
        {
            UnavailableReason = $"Docker is not available for the integration tests: {exception.GetType().Name}: {exception.Message}";
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (rabbitMq is not null)
        {
            await rabbitMq.DisposeAsync();
        }

        if (postgres is not null)
        {
            await postgres.DisposeAsync();
        }

        if (definitionsFile is not null)
        {
            File.Delete(definitionsFile);
        }
    }

    internal void RequireAvailable() => Assert.SkipWhen(UnavailableReason is not null, UnavailableReason ?? string.Empty);

    internal string Connection(string database, string user, string password) =>
        new NpgsqlConnectionStringBuilder
        {
            Host = PostgresHost,
            Port = PostgresPort,
            Database = database,
            Username = user,
            Password = password,
            MaxPoolSize = 8
        }.ConnectionString;

    internal string SuperConnection(string database) => Connection(database, SuperUser, SuperPassword);

    internal string RoleConnection(string database, string role) => Connection(database, role, TestCredentials.DatabasePassword);

    private async Task CreateRolesAsync()
    {
        await using var connection = new NpgsqlConnection(SuperConnection("postgres"));
        await connection.OpenAsync();

        foreach (var role in new[]
        {
            TestCredentials.EntriesApp,
            TestCredentials.EntriesPublisher,
            TestCredentials.BalanceApp,
            TestCredentials.BalanceConsumer
        })
        {
            await using var command = new NpgsqlCommand($"CREATE ROLE {role} LOGIN PASSWORD '{TestCredentials.DatabasePassword}'", connection);
            await command.ExecuteNonQueryAsync();
        }
    }
}
