namespace SecureCodingDemo.Modules;

public static class ConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapConfigurationEndpoints(this IEndpointRouteBuilder app)
    {
        var configuration = app.MapGroup("/api/configuration").WithTags("22. Security Misconfiguration");

        configuration.MapGet("/unsafe-headers", (IWebHostEnvironment environment, IConfiguration appConfiguration) =>
            Results.Ok(new
            {
                environment = environment.EnvironmentName,
                contentRoot = environment.ContentRootPath,
                jwtIssuer = appConfiguration["Jwt:Issuer"],
                sampleStackTrace = "System.InvalidOperationException: demo failure at SecureCodingDemo.Modules.ConfigurationEndpoints",
                risk = "The response exposes diagnostic details and does not set security headers."
            }))
            .WithSummary("Exposes diagnostic details and omits security headers");

        configuration.MapGet("/safe-headers", (HttpContext context) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["Referrer-Policy"] = "no-referrer";
            context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'";

            return Results.Ok(new
            {
                status = "ok",
                details = "Diagnostic details are available only in protected server-side logs."
            });
        })
        .WithSummary("Returns a neutral response with baseline security headers");

        return app;
    }
}
