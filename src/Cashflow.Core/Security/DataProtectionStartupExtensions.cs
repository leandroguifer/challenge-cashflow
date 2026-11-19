using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cashflow.Core.Security;

public static class DataProtectionStartupExtensions
{
    private const string HostedServiceTypeName = "Microsoft.AspNetCore.DataProtection.Internal.DataProtectionHostedService";

    public static void RemoveDataProtectionStartup(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var descriptors = services
            .Where(descriptor => descriptor.ServiceType == typeof(IHostedService)
                && descriptor.ImplementationType?.FullName == HostedServiceTypeName)
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
