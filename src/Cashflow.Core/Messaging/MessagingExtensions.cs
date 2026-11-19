using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Cashflow.Core.Messaging;

public static class MessagingExtensions
{
    public static void AddMessaging(this IServiceCollection services, Assembly assembly)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        foreach (var type in assembly.GetTypes().Where( t => t is {IsClass: true, IsAbstract: false}))
        {
            foreach (var @interface in type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            {
                services.AddScoped(@interface, type);
            }
        }
    }
}