using SecureCodingDemo.Infrastructure;

namespace SecureCodingDemo.Modules;

public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/sections", (DocumentationCatalog catalog) =>
            catalog.GetSections().Select(section => new
            {
                section.Slug,
                section.Title,
                section.UnsafeEndpoint,
                section.SafeEndpoint
            }))
            .WithTags("00. Catalog")
            .WithSummary("Catalog of secure coding demo sections");

        return app;
    }
}
