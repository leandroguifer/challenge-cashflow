using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Cashflow.Core.Security;

public static class SecurityExtensions
{
    public static void AddKeycloakAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var keycloakOptions = configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>() ?? new KeycloakOptions();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = keycloakOptions.Authority;
            options.Audience = keycloakOptions.Audience;
            options.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = options.Audience,
                ValidateIssuer = true,
                ValidIssuer = options.Authority,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                RoleClaimType = "roles",
                NameClaimType = "preferred_username"
            };
        });

        services.RemoveDataProtectionStartup();

        services.AddProblemDetails();

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build())
            .AddPolicy(Policies.Cashier, policy => policy.RequireRole(Roles.Cashier))
            .AddPolicy(Policies.EntriesReader, policy => policy.RequireRole(Roles.Cashier, Roles.Assistant, Roles.Owner))
            .AddPolicy(Policies.BalanceReader, policy => policy.RequireRole(Roles.Assistant, Roles.Owner));
    }
}