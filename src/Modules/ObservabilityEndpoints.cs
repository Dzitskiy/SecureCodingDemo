namespace SecureCodingDemo.Modules;

public static class ObservabilityEndpoints
{
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "token",
        "accessToken",
        "refreshToken",
        "authorization",
        "secret"
    };

    public static IEndpointRouteBuilder MapObservabilityEndpoints(this IEndpointRouteBuilder app)
    {
        var logging = app.MapGroup("/api/logging").WithTags("12. Dangerous Logging");

        logging.MapPost("/unsafe", (Dictionary<string, string> payload, ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("DangerousLoggingDemo");
            logger.LogWarning("Unsafe login payload: {@Payload}", payload);
            return Results.Ok(new
            {
                logged = payload,
                risk = "Sensitive fields were written to application logs."
            });
        })
        .WithSummary("Logs the full request payload including secrets");

        logging.MapPost("/safe", (Dictionary<string, string> payload, ILoggerFactory loggerFactory) =>
        {
            var redacted = payload.ToDictionary(
                item => item.Key,
                item => SensitiveKeys.Contains(item.Key) ? "***REDACTED***" : item.Value,
                StringComparer.OrdinalIgnoreCase);

            var logger = loggerFactory.CreateLogger("SafeLoggingDemo");
            logger.LogInformation("Safe login payload: {@Payload}", redacted);
            return Results.Ok(new { logged = redacted });
        })
        .WithSummary("Redacts sensitive fields before logging");

        return app;
    }
}
