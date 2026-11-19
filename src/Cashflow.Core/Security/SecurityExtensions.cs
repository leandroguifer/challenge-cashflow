using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Cashflow.Core.Security;

public static class SecurityExtensions
{
    public static void AddKeycloakAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>() ?? new KeycloakOptions();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer("Bearer", options =>
        {
            options.Authority = options.Authority;
            options.Audience = options.Audience;
            options.RequireHttpsMetadata = options.RequireHttpsMetadata;
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
        })
        ;

    }
}