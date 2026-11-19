using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cashflow.Core.Security;

public static class CorsExtensions
{
    private const string PolicyName = "web";
    private const string OriginsKey = "CORS_ORIGINS";

    public static void AddWebCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = (configuration[OriginsKey] ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        services.AddCors(options => options.AddPolicy(PolicyName, policy => policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()));
    }

    public static void UseWebCors(this IApplicationBuilder app) => app.UseCors(PolicyName);
}
