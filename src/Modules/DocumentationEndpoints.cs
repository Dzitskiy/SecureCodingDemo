using SecureCodingDemo.Infrastructure;

namespace SecureCodingDemo.Modules;

public static class DocumentationEndpoints
{
    public static IEndpointRouteBuilder MapDocumentationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/docs/{slug}", (string slug, DocumentationCatalog catalog) =>
        {
            var section = catalog.GetSection(slug);
            return section is null ? Results.NotFound() : Results.Ok(section);
        })
        .WithTags("00. Catalog")
        .WithSummary("Return Markdown documentation for a section");

        return app;
    }
}
