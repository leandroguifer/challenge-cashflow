using RabbitMQ.Client;

namespace Cashflow.Integration.Tests.Support;

internal sealed class RabbitTestClient(InfrastructureFixture infrastructure)
{
    public async Task PublishAsync(RabbitAccount account, string routingKey, byte[] body)
    {
        await using var connection = await ConnectAsync(account);
        await using var channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true));

        var properties = new BasicProperties { Persistent = true, ContentType = "application/json" };

        await channel.BasicPublishAsync(TestCredentials.Exchange, routingKey, mandatory: true, properties, body);
    }

    public async Task<uint> CountAsync(string queue)
    {
        await using var connection = await ConnectAsync(TestCredentials.RabbitAdmin);
        await using var channel = await connection.CreateChannelAsync();

        var declared = await channel.QueueDeclarePassiveAsync(queue);
        return declared.MessageCount;
    }

    public async Task PurgeAsync(string queue)
    {
        await using var connection = await ConnectAsync(TestCredentials.RabbitAdmin);
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueuePurgeAsync(queue);
    }

    private async Task<IConnection> ConnectAsync(RabbitAccount account)
    {
        var factory = new ConnectionFactory
        {
            HostName = infrastructure.RabbitHost,
            Port = infrastructure.RabbitPort,
            UserName = account.User,
            Password = account.Password
        };

        return await factory.CreateConnectionAsync();
    }
}
