using Cashflow.Core.Modules;
using Cashflow.Core.Observability;
using Cashflow.Core.Security;
using Cashflow.Balance.Infrastructure.Persistence;

namespace Cashflow.Balance;

internal sealed class BalanceModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddObservability("balance");
        services.AddKeycloakAuthentication(configuration);
        services.AddBalancePersistence(configuration);
        services.AddHostedService<BalanceMigrationService>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", () => Results.Ok(new { service = "balance", status = "healthy" }))
            .AllowAnonymous();
    }
}