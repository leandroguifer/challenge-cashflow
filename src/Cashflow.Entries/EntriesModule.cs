using System.Text.Json.Serialization;
using Cashflow.Core.Messaging;
using Cashflow.Core.Modules;
using Cashflow.Core.Observability;
using Cashflow.Core.Security;
using Cashflow.Entries.Application;
using Cashflow.Entries.Endpoints;
using Cashflow.Entries.Infrastructure.Outbox;
using Cashflow.Entries.Infrastructure.Persistence;

namespace Cashflow.Entries;

internal sealed class EntriesModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddObservability("cashflow-entries");
        services.AddKeycloakAuthentication(configuration);
        services.AddWebCors(configuration);
        services.AddEntriesPersistence(configuration);
        services.AddHostedService<EntriesMigrationService>();
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));
        services.AddHostedService<OutboxPublisher>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<BusinessClock>();
        services.AddMessaging(typeof(EntriesModule).Assembly);
        services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", () => Results.Ok(new { service = "entries", status = "healthy" }))
            .AllowAnonymous();

        endpoints.MapEntriesEndpoints();
    }
}