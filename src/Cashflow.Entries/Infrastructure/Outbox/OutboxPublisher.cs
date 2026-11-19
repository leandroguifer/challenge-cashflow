using System.Diagnostics;
using System.Text;
using Cashflow.Core.Messaging;
using Cashflow.Core.Observability;
using Cashflow.Entries.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Cashflow.Entries.Infrastructure.Outbox;

internal sealed partial class OutboxPublisher(
    IConfiguration configuration,
    IOptions<RabbitMqOptions> rabbitMqOptions,
    IOptions<OutboxOptions> outboxOptions,
    TimeProvider timeProvider,
    ILogger<OutboxPublisher> logger) : BackgroundService
{
    private readonly DbContextOptions<EntriesDbContext> dbOptions = CreateDbOptions(configuration);

    private IConnection? connection;
    private IChannel? channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = outboxOptions.Value;

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var broker = await EnsureChannelAsync(stoppingToken);
                    var published = await PublishBatchAsync(broker, settings.BatchSize, stoppingToken);

                    if (published < settings.BatchSize)
                    {
                        await Task.Delay(settings.PollingInterval, stoppingToken);
                    }
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    LogPublishFailed(exception);
                    await ResetChannelAsync();
                    await Task.Delay(settings.RetryDelay, stoppingToken);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        finally
        {
            await ResetChannelAsync();
        }
    }

    private static DbContextOptions<EntriesDbContext> CreateDbOptions(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Publisher");
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        return new DbContextOptionsBuilder<EntriesDbContext>().UseNpgsql(connectionString).Options;
    }

    private async Task<IChannel> EnsureChannelAsync(CancellationToken cancellationToken)
    {
        if (channel is { IsOpen: true })
        {
            return channel;
        }

        await ResetChannelAsync();

        var settings = rabbitMqOptions.Value;
        var factory = new ConnectionFactory
        {
            HostName = settings.Host,
            Port = settings.Port,
            UserName = settings.User,
            Password = settings.Password
        };

        connection = await factory.CreateConnectionAsync(cancellationToken);
        channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
            cancellationToken);

        return channel;
    }

    private async Task ResetChannelAsync()
    {
        if (channel is not null)
        {
            await channel.DisposeAsync();
            channel = null;
        }

        if (connection is not null)
        {
            await connection.DisposeAsync();
            connection = null;
        }
    }

    private async Task<int> PublishBatchAsync(IChannel broker, int batchSize, CancellationToken cancellationToken)
    {
        await using var context = new EntriesDbContext(dbOptions);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var pending = await context.OutboxMessages
            .FromSql($"select * from outbox_messages where published_at is null order by created_at limit {batchSize} for update skip locked")
            .ToListAsync(cancellationToken);

        var published = 0;

        try
        {
            foreach (var message in pending)
            {
                await PublishAsync(broker, message, cancellationToken);
                message.MarkPublished(timeProvider.GetUtcNow());
                published++;
            }
        }
        finally
        {
            if (published > 0)
            {
                await context.SaveChangesAsync(CancellationToken.None);
                await transaction.CommitAsync(CancellationToken.None);
            }
        }

        return published;
    }

    private static async Task PublishAsync(IChannel broker, OutboxMessage message, CancellationToken cancellationToken)
    {
        using var activity = MessagingTelemetry.StartProducer($"{message.Type} publish", message.TraceParent);

        var headers = new Dictionary<string, object?>();
        TraceContextHeaders.Inject(activity, headers);

        var properties = new BasicProperties
        {
            Persistent = true,
            MessageId = message.Id.ToString(),
            ContentType = "application/json",
            Type = message.Type,
            Headers = headers
        };

        try
        {
            await broker.BasicPublishAsync(
                OutboxTopology.Exchange,
                message.Type,
                mandatory: true,
                properties,
                Encoding.UTF8.GetBytes(message.Payload),
                cancellationToken);
        }
        catch (Exception) when (activity is not null)
        {
            activity.SetStatus(ActivityStatusCode.Error);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Outbox publication failed, pending messages kept for retry")]
    private partial void LogPublishFailed(Exception exception);
}
