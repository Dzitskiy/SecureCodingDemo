using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.RateLimiting;
using SecureCodingDemo.Services;

namespace SecureCodingDemo.Modules;

public static class DataSecurityEndpoints
{
    public static IEndpointRouteBuilder MapDataSecurityEndpoints(this IEndpointRouteBuilder app)
    {
        MapAuthenticationEndpoints(app);
        MapSqlEndpoints(app);
        MapMassAssignmentEndpoints(app);
        MapOrderEndpoints(app);
        MapDeserializationEndpoints(app);
        MapDependencyEndpoints(app);
        MapSecretsEndpoints(app);
        MapReflectionEndpoints(app);
        return app;
    }

    private static void MapAuthenticationEndpoints(IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth").WithTags("11. JWT / IDOR");

        auth.MapPost("/token", (TokenRequest request, DemoRepository repository, TokenService tokens) =>
        {
            var user = repository.SafeLogin(request.UserName, request.Password);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new
            {
                accessToken = tokens.CreateToken(user.Id, user.UserName, user.IsAdmin),
                user = ToPublicUser(user)
            });
        })
        .WithSummary("Issues a demo JWT for safe IDOR endpoints");
    }

    private static void MapSqlEndpoints(IEndpointRouteBuilder app)
    {
        var sql = app.MapGroup("/api/sql").WithTags("04. SQL Injection");

        sql.MapPost("/unsafe-login", (TokenRequest request, DemoRepository repository) =>
        {
            var result = repository.UnsafeLogin(request.UserName, request.Password);
            return result.User is null
                ? Results.Unauthorized()
                : Results.Ok(new
                {
                    result.Sql,
                    result.InjectionDetected,
                    user = ToPublicUser(result.User)
                });
        })
        .WithSummary("Builds a SQL query by string interpolation");

        sql.MapPost("/safe-login", (TokenRequest request, DemoRepository repository) =>
        {
            var user = repository.SafeLogin(request.UserName, request.Password);
            return user is null
                ? Results.Unauthorized()
                : Results.Ok(new
                {
                    sql = "select * from users where username = @username and password_hash = @passwordHash",
                    parameters = new[] { "@username", "@passwordHash" },
                    user = ToPublicUser(user)
                });
        })
        .WithSummary("Models parameterized SQL and separates data from syntax");
    }

    private static void MapMassAssignmentEndpoints(IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/api/users").WithTags("07. Mass Assignment");

        users.MapPost("/unsafe-profile", (UnsafeProfileUpdate update, DemoRepository repository) =>
        {
            var user = repository.UpdateUnsafeProfile(update);
            return Results.Ok(new
            {
                user = ToPublicUser(user),
                risk = "The request body can change IsAdmin because the entity shape is bound directly."
            });
        })
        .WithSummary("Binds directly to fields that should not be client-controlled");

        users.MapPost("/safe-profile", (SafeProfileUpdate update, DemoRepository repository) =>
        {
            var user = repository.UpdateSafeProfile(update);
            return Results.Ok(new
            {
                user = ToPublicUser(user),
                ignoredFields = new[] { "isAdmin", "roles", "tenantId" }
            });
        })
        .WithSummary("Uses an input DTO with only writable fields");
    }

    private static void MapOrderEndpoints(IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("/api/orders").WithTags("11. JWT / IDOR");

        orders.MapGet("/unsafe/{orderId:int}", (int orderId, DemoRepository repository) =>
        {
            var order = repository.GetOrderUnsafe(orderId);
            return order is null ? Results.NotFound() : Results.Ok(order);
        })
        .WithSummary("Returns an object by id without ownership checks");

        orders.MapGet("/safe/{orderId:int}", (int orderId, ClaimsPrincipal user, DemoRepository repository) =>
        {
            var userId = GetCurrentUserId(user);
            if (userId is null)
            {
                return Results.Unauthorized();
            }

            var order = repository.GetOrderForUser(orderId, userId.Value);
            return order is null ? Results.NotFound() : Results.Ok(order);
        })
        .RequireAuthorization()
        .WithSummary("Checks object ownership using the authenticated user id");
    }

    private static void MapDeserializationEndpoints(IEndpointRouteBuilder app)
    {
        var deserialization = app.MapGroup("/api/deserialization").WithTags("16. Insecure Deserialization");

        deserialization.MapPost("/unsafe-action", (ActionEnvelope envelope) =>
        {
            var type = Type.GetType(envelope.Type);
            if (type is null || Activator.CreateInstance(type) is not IDemoAction action)
            {
                return Results.BadRequest(new { error = "Type cannot be loaded as a demo action." });
            }

            return Results.Ok(new
            {
                executed = type.FullName,
                result = action.Execute(envelope.Payload),
                risk = "The client controls the CLR type name."
            });
        })
        .WithSummary("Uses a client-supplied CLR type name");

        deserialization.MapPost("/safe-action", (SafeActionEnvelope envelope) =>
        {
            IDemoAction action = envelope.Kind.ToLowerInvariant() switch
            {
                "echo" => new EchoAction(),
                _ => throw new BadHttpRequestException("Unsupported action kind.")
            };

            return Results.Ok(new { executed = envelope.Kind, result = action.Execute(envelope.Payload) });
        })
        .WithSummary("Uses an allowlisted logical action kind");
    }

    private static void MapDependencyEndpoints(IEndpointRouteBuilder app)
    {
        var dependencies = app.MapGroup("/api/dependencies").WithTags("13. Dependency Security");

        dependencies.MapPost("/unsafe-install-plan", (PackageRequest request) =>
            Results.Ok(new
            {
                command = $"dotnet add package {request.PackageName} --version {request.Version} --source {request.Source}",
                risk = "The package, version and source are accepted without trust checks."
            }))
            .WithSummary("Builds a package install plan from arbitrary input");

        dependencies.MapPost("/safe-install-plan", (PackageRequest request) =>
        {
            var packageAllowlist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Serilog.AspNetCore",
                "Swashbuckle.AspNetCore",
                "Npgsql"
            };

            if (!packageAllowlist.Contains(request.PackageName))
            {
                return Results.BadRequest(new { error = "Package is not allowlisted for this demo." });
            }

            if (!Regex.IsMatch(request.Version, @"^\d+\.\d+\.\d+$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)))
            {
                return Results.BadRequest(new { error = "Use a pinned semantic version, not latest or floating ranges." });
            }

            if (!Uri.TryCreate(request.Source, UriKind.Absolute, out var source)
                || source.Scheme != Uri.UriSchemeHttps
                || !source.Host.Equals("api.nuget.org", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest(new { error = "Use the trusted NuGet source." });
            }

            return Results.Ok(new { package = request.PackageName, request.Version, source = source.ToString() });
        })
        .WithSummary("Requires allowlisted packages, pinned versions and trusted source");
    }

    private static void MapSecretsEndpoints(IEndpointRouteBuilder app)
    {
        var secrets = app.MapGroup("/api/secrets").WithTags("17. Secrets Management");

        secrets.MapGet("/unsafe", (IConfiguration configuration) =>
            Results.Ok(new
            {
                jwtSigningKey = configuration["Jwt:SigningKey"],
                postgres = configuration.GetConnectionString("Postgres"),
                risk = "Secrets are returned to the caller."
            }))
            .WithSummary("Leaks configured secrets");

        secrets.MapGet("/safe", (IConfiguration configuration) =>
            Results.Ok(new
            {
                jwtSigningKey = Mask(configuration["Jwt:SigningKey"]),
                postgres = Mask(configuration.GetConnectionString("Postgres")),
                production = "Read secrets from environment variables or a secret store and never return them from API responses."
            }))
            .WithSummary("Returns only redacted secret metadata");
    }

    private static void MapReflectionEndpoints(IEndpointRouteBuilder app)
    {
        var reflection = app.MapGroup("/api/reflection").WithTags("20. Unsafe Reflection");

        reflection.MapPost("/unsafe-invoke", (ReflectionRequest request, ReflectionDemoService service) =>
            Results.Ok(new
            {
                method = request.MethodName,
                result = service.UnsafeInvoke(request.MethodName)
            }))
            .WithSummary("Invokes a method name supplied by the client");

        reflection.MapPost("/safe-invoke", (SafeReflectionRequest request, ReflectionDemoService service) =>
        {
            var result = service.SafeInvoke(request.Operation);
            return result is null
                ? Results.BadRequest(new { error = "Allowed operations: status, version." })
                : Results.Ok(new { request.Operation, result });
        })
        .WithSummary("Maps public operation names to allowlisted handlers");
    }

    private static object ToPublicUser(DemoUser user)
    {
        return new
        {
            user.Id,
            user.UserName,
            user.DisplayName,
            user.Department,
            user.IsAdmin
        };
    }

    private static int? GetCurrentUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    private static string? Mask(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return value.Length <= 4 ? "****" : $"{value[..2]}****{value[^2..]}";
    }
}

public sealed record TokenRequest(string UserName, string Password);

public sealed record ActionEnvelope(string Type, string Payload);

public sealed record SafeActionEnvelope(string Kind, string Payload);

public interface IDemoAction
{
    string Execute(string payload);
}

public sealed class EchoAction : IDemoAction
{
    public string Execute(string payload) => payload;
}

public sealed class GrantAdminAction : IDemoAction
{
    public string Execute(string payload) => $"dangerous grant-admin action accepted payload: {payload}";
}

public sealed record PackageRequest(string PackageName, string Version, string Source);

public sealed record ReflectionRequest(string MethodName);

public sealed record SafeReflectionRequest(string Operation);
