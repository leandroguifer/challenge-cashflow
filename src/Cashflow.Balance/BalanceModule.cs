using Cashflow.Core.Modules;
using Cashflow.Core.Observability;
using Cashflow.Core.Security;
using Cashflow.Balance.Infrastructure.Persistence;
using Cashflow.Core.Messaging;
using Cashflow.Balance.Infrastructure.Messaging;
using Cashflow.Balance.Endpoints;

namespace Cashflow.Balance;

internal sealed class BalanceModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddObservability("balance");
        services.AddKeycloakAuthentication(configuration);
        services.AddWebCors(configuration);
        services.AddBalancePersistence(configuration);
        services.AddHostedService<BalanceMigrationService>();
        services.AddSingleton(TimeProvider.System);
        services.AddMessaging(typeof(BalanceModule).Assembly);
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddHostedService<EntryPostedConsumer>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", () => Results.Ok(new { service = "balance", status = "healthy" }))
            .AllowAnonymous();

        endpoints.MapBalanceEndpoints();
    }
}