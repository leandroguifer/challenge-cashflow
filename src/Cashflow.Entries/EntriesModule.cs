using Cashflow.Core.Modules;
using Cashflow.Core.Observability;
using Cashflow.Core.Security;
using Cashflow.Entries.Infrastructure.Persistence;

namespace Cashflow.Entries;

internal sealed class EntriesModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddObservability("cashflow-entries");
        services.AddKeycloakAuthentication(configuration);
        services.AddEntriesPersistence(configuration);
        services.AddHostedService<EntriesMigrationService>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", () => Results.Ok(new { service = "entries", status = "healthy" }))
            .AllowAnonymous();
    }
}