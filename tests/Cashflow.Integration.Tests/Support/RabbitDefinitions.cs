namespace Cashflow.Integration.Tests.Support;

internal static class RabbitDefinitions
{
    public static byte[] RenderDefinitions()
    {
        var content = Repository.ReadText("docker", "rabbitmq", "definitions.json")
            .Replace("@RABBITMQ_ADMIN_USER@", TestCredentials.RabbitAdmin.User, StringComparison.Ordinal)
            .Replace("@RABBITMQ_ADMIN_PASSWORD@", TestCredentials.RabbitAdmin.Password, StringComparison.Ordinal)
            .Replace("@ENTRIES_RABBITMQ_USER@", TestCredentials.RabbitEntries.User, StringComparison.Ordinal)
            .Replace("@ENTRIES_RABBITMQ_PASSWORD@", TestCredentials.RabbitEntries.Password, StringComparison.Ordinal)
            .Replace("@BALANCE_RABBITMQ_USER@", TestCredentials.RabbitBalance.User, StringComparison.Ordinal)
            .Replace("@BALANCE_RABBITMQ_PASSWORD@", TestCredentials.RabbitBalance.Password, StringComparison.Ordinal);

        return System.Text.Encoding.UTF8.GetBytes(content);
    }
}
