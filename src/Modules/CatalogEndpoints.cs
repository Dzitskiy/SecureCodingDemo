using SecureCodingDemo.Infrastructure;

namespace SecureCodingDemo.Modules;

public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/sections", (DemoScenarioCatalog catalog) =>
            Results.Ok(catalog.GetScenarios()))
            .WithTags("00. Catalog")
            .WithSummary("Catalog of secure coding demo sections");

        return app;
    }
}
