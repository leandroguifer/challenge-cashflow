namespace Cashflow.Integration.Tests.Support;

internal static class ServiceSettings
{
    public static IReadOnlyDictionary<string, string> Entries(InfrastructureFixture infrastructure, TestDatabases databases) =>
        new Dictionary<string, string>
        {
            ["ConnectionStrings__Default"] = databases.EntriesAs(TestCredentials.EntriesApp),
            ["ConnectionStrings__Migrations"] = databases.EntriesMigrations,
            ["ConnectionStrings__Publisher"] = databases.EntriesAs(TestCredentials.EntriesPublisher),
            ["RabbitMq__Host"] = infrastructure.RabbitHost,
            ["RabbitMq__Port"] = infrastructure.RabbitPort.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["RabbitMq__User"] = TestCredentials.RabbitEntries.User,
            ["RabbitMq__Password"] = TestCredentials.RabbitEntries.Password,
            ["Keycloak__Authority"] = "http://localhost/realms/test",
            ["Keycloak__Audience"] = "cashflow-entries",
            ["CORS_ORIGINS"] = "http://localhost",
            ["OTEL_SDK_DISABLED"] = "true"
        };

    public static IReadOnlyDictionary<string, string> Balance(InfrastructureFixture infrastructure, TestDatabases databases) =>
        new Dictionary<string, string>
        {
            ["ConnectionStrings__Default"] = databases.BalanceAs(TestCredentials.BalanceApp),
            ["ConnectionStrings__Migrations"] = databases.BalanceMigrations,
            ["ConnectionStrings__Consumer"] = databases.BalanceAs(TestCredentials.BalanceConsumer),
            ["RabbitMq__Host"] = infrastructure.RabbitHost,
            ["RabbitMq__Port"] = infrastructure.RabbitPort.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["RabbitMq__User"] = TestCredentials.RabbitBalance.User,
            ["RabbitMq__Password"] = TestCredentials.RabbitBalance.Password,
            ["Keycloak__Authority"] = "http://localhost/realms/test",
            ["Keycloak__Audience"] = "cashflow-balance",
            ["CORS_ORIGINS"] = "http://localhost",
            ["OTEL_SDK_DISABLED"] = "true"
        };
}
