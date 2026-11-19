using Microsoft.AspNetCore.Builder;

namespace Cashflow.Core.Security;

public static class ApiSecurityExtensions
{
    public static void UseApiSecurity(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseWebCors();
        app.UseStatusCodePages();
        app.UseAuthentication();
        app.UseAuthorization();
    }
}
