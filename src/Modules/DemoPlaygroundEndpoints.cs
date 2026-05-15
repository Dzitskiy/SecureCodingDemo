using SecureCodingDemo.Infrastructure;

namespace SecureCodingDemo.Modules;

public static class DemoPlaygroundEndpoints
{
    public static IEndpointRouteBuilder MapDemoPlaygroundEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/demos", (DemoScenarioCatalog catalog) =>
            Results.Ok(catalog.GetScenarios()))
            .WithTags("00. Catalog")
            .WithSummary("Return all demo scenarios with ready-made test requests");

        app.MapGet("/api/demos/{slug}", (string slug, DemoScenarioCatalog catalog) =>
        {
            var scenario = catalog.GetScenario(slug);
            return scenario is null ? Results.NotFound() : Results.Ok(scenario);
        })
        .WithTags("00. Catalog")
        .WithSummary("Return one demo scenario with ready-made unsafe and safe requests");

        app.MapGet("/demo", () => Results.LocalRedirect("/demo.html"))
            .ExcludeFromDescription();

        return app;
    }
}
