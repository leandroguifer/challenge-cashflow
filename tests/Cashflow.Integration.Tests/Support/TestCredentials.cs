namespace Cashflow.Integration.Tests.Support;

internal static class TestCredentials
{
    public const string DatabasePassword = "cashflow-test";

    public const string EntriesApp = "entries_app";
    public const string EntriesPublisher = "entries_publisher";
    public const string BalanceApp = "balance_app";
    public const string BalanceConsumer = "balance_consumer";

    public static readonly RabbitAccount RabbitAdmin = new("admin-test", "admin-test-password");
    public static readonly RabbitAccount RabbitEntries = new("entries-test", "entries-test-password");
    public static readonly RabbitAccount RabbitBalance = new("balance-test", "balance-test-password");

    public const string Exchange = "entries.events";
    public const string RoutingKey = "entries.entry-posted";
    public const string Queue = "balance.entry-posted";
    public const string DeadLetterQueue = "balance.entry-posted.dlq";
}
