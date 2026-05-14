using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.RateLimiting;
using SecureCodingDemo.Services;

namespace SecureCodingDemo.Modules;

public static class AvailabilityEndpoints
{
    public static IEndpointRouteBuilder MapAvailabilityEndpoints(this IEndpointRouteBuilder app)
    {
        MapDosEndpoints(app);
        MapRateLimitEndpoints(app);
        MapCancellationEndpoints(app);
        MapCacheEndpoints(app);
        return app;
    }

    private static void MapDosEndpoints(IEndpointRouteBuilder app)
    {
        var dos = app.MapGroup("/api/dos").WithTags("05. DoS / Resource Exhaustion");

        dos.MapPost("/unsafe-echo", async (HttpRequest request) =>
        {
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            return Results.Ok(new
            {
                receivedBytes = body.Length,
                echo = body,
                risk = "The endpoint reads and echoes the whole request body."
            });
        })
        .WithSummary("Reads and echoes an unbounded request body");

        dos.MapPost("/safe-echo", async (HttpRequest request, CancellationToken cancellationToken) =>
        {
            if (request.ContentLength is > 16_384)
            {
                return Results.BadRequest(new { error = "Request body is limited to 16 KB in this demo." });
            }

            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync(cancellationToken);
            if (body.Length > 16_384)
            {
                return Results.BadRequest(new { error = "Request body is limited to 16 KB in this demo." });
            }

            var parsed = JsonSerializer.Deserialize<JsonElement>(body);
            return Results.Ok(new { receivedBytes = body.Length, parsed.ValueKind });
        })
        .RequireRateLimiting("expensive")
        .WithSummary("Limits body size, honors cancellation and uses a concurrency limiter");
    }

    private static void MapRateLimitEndpoints(IEndpointRouteBuilder app)
    {
        var login = app.MapGroup("/api/login").WithTags("06. Rate Limiting");

        login.MapPost("/unsafe", (TokenRequest request) =>
            Results.Ok(new
            {
                accepted = request.UserName,
                risk = "No rate limit is applied, so brute force attempts are cheap."
            }))
            .WithSummary("Accepts login attempts without throttling");

        login.MapPost("/safe", [EnableRateLimiting("login")] (TokenRequest request) =>
            Results.Ok(new
            {
                accepted = request.UserName,
                limiter = "5 requests per minute"
            }))
            .WithSummary("Applies a fixed-window login rate limit");
    }

    private static void MapCancellationEndpoints(IEndpointRouteBuilder app)
    {
        var cancellation = app.MapGroup("/api/cancellation").WithTags("14. CancellationToken");

        cancellation.MapGet("/unsafe", async (int seconds) =>
        {
            var delay = TimeSpan.FromSeconds(Math.Clamp(seconds, 1, 30));
            var started = Stopwatch.GetTimestamp();
            await Task.Delay(delay);
            return Results.Ok(new
            {
                waited = delay.TotalSeconds,
                elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds,
                risk = "The operation ignores request cancellation."
            });
        })
        .WithSummary("Starts slow work without passing CancellationToken");

        cancellation.MapGet("/safe", async (int seconds, CancellationToken cancellationToken) =>
        {
            var delay = TimeSpan.FromSeconds(Math.Clamp(seconds, 1, 30));
            var started = Stopwatch.GetTimestamp();
            await Task.Delay(delay, cancellationToken);
            return Results.Ok(new
            {
                waited = delay.TotalSeconds,
                elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds
            });
        })
        .WithSummary("Passes CancellationToken into slow work");
    }

    private static void MapCacheEndpoints(IEndpointRouteBuilder app)
    {
        var cache = app.MapGroup("/api/cache").WithTags("18. Cache Stampede");

        cache.MapGet("/unsafe-report", async (CacheStampedeService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetUnsafeReportAsync(cancellationToken)))
            .WithSummary("Refreshes cache without a single-flight lock");

        cache.MapGet("/safe-report", async (CacheStampedeService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetSafeReportAsync(cancellationToken)))
            .WithSummary("Refreshes cache under a single-flight lock");
    }
}
