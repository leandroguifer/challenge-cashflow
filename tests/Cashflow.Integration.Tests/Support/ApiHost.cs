using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Cashflow.Integration.Tests.Support;

internal sealed class ApiHost<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private ApiHost()
    {
    }

    public static async Task<ApiHost<TEntryPoint>> StartAsync(IReadOnlyDictionary<string, string> settings)
    {
        await Gate.WaitAsync();

        try
        {
            using var environment = new EnvironmentScope(settings);
            var host = new ApiHost<TEntryPoint>();
            await Task.Run(() => _ = host.Server);
            return host;
        }
        finally
        {
            Gate.Release();
        }
    }

    public async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, TestUser? user = null, object? body = null)
    {
        using var request = new HttpRequestMessage(method, url);
        user?.Apply(request);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        using var client = CreateClient();
        return await client.SendAsync(request);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = TestAuthHandler.SchemeName;
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultForbidScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }
}
