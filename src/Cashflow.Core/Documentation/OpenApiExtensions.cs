using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace Cashflow.Core.Documentation;

public static class OpenApiExtensions
{
    public static void AddApiDocumentation(this IServiceCollection services) =>
        services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecurityTransformer>());

    public static void MapApiDocumentation(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapOpenApi().AllowAnonymous();
        endpoints.MapScalarApiReference(options => options.AddPreferredSecuritySchemes(BearerSecurityTransformer.SchemeName)).AllowAnonymous();
    }
}
