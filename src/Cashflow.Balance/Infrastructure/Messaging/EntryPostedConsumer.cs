using System.Text.Json;
using Cashflow.Balance.Application.Consumption;
using Cashflow.Balance.Contracts;
using Cashflow.Balance.Domain;
using Cashflow.Core.Messaging;
using Cashflow.Core.Observability;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Cashflow.Balance.Infrastructure.Messaging;

internal sealed partial class EntryPostedConsumer(
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<EntryPostedConsumer> logger) : BackgroundService
{
    private const string Queue = "balance.entry-posted";
    private const ushort PrefetchCount = 20;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                LogBrokerUnavailable(exception);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        var factory = new ConnectionFactory
        {
            HostName = settings.Host,
            Port = settings.Port,
            UserName = settings.User,
            Password = settings.Password
        };

        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.BasicQosAsync(0, PrefetchCount, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, args) => HandleAsync(channel, args, stoppingToken);

        await channel.BasicConsumeAsync(Queue, autoAck: false, consumer, stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleAsync(IChannel channel, BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        using var activity = MessagingTelemetry.StartConsumer("entries.entry-posted process", args.BasicProperties.Headers);

        try
        {
            var message = JsonSerializer.Deserialize<EntryPostedEvent>(args.Body.Span, JsonOptions);

            if (message is null || !Enum.TryParse<EntryType>(message.Type, ignoreCase: true, out var type))
            {
                await channel.BasicNackAsync(args.DeliveryTag, false, false, cancellationToken);
                return;
            }

            await using var scope = scopeFactory.CreateAsyncScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IDispatcher>();

            var result = await dispatcher.SendAsync(
                new ApplyEntryPostedCommand(message.EventId, message.StoreId, message.BusinessDate, type, message.Amount),
                cancellationToken);

            if (result.IsSuccess)
            {
                await channel.BasicAckAsync(args.DeliveryTag, false, cancellationToken);
                return;
            }

            LogMessageRejected(message.EventId, result.Error!.Code);
            await channel.BasicNackAsync(args.DeliveryTag, false, false, cancellationToken);
        }
        catch (JsonException)
        {
            await channel.BasicNackAsync(args.DeliveryTag, false, false, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogProcessingFailed(exception);
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            await channel.BasicNackAsync(args.DeliveryTag, false, true, cancellationToken);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "RabbitMQ unavailable, retrying in 5 seconds")]
    private partial void LogBrokerUnavailable(Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Message {EventId} rejected: {ErrorCode}")]
    private partial void LogMessageRejected(Guid eventId, string errorCode);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to process message, it will be redelivered")]
    private partial void LogProcessingFailed(Exception exception);
}