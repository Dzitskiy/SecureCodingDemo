using System.Security.Cryptography;
using Microsoft.AspNetCore.StaticFiles;
using SecureCodingDemo.Services;

namespace SecureCodingDemo.Modules;

public static class NetworkAndFileEndpoints
{
    public static IEndpointRouteBuilder MapNetworkAndFileEndpoints(this IEndpointRouteBuilder app)
    {
        MapSsrfEndpoints(app);
        MapUploadEndpoints(app);
        MapFileReadEndpoints(app);
        return app;
    }

    private static void MapSsrfEndpoints(IEndpointRouteBuilder app)
    {
        var ssrf = app.MapGroup("/api/ssrf").WithTags("08. SSRF");

        ssrf.MapPost("/unsafe-fetch", async (UrlRequest request, IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
        {
            var client = httpClientFactory.CreateClient("ssrf-demo");
            var response = await client.GetAsync(request.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            return Results.Ok(new
            {
                request.Url,
                status = (int)response.StatusCode,
                risk = "The server fetched an arbitrary caller-controlled URL."
            });
        })
        .WithSummary("Fetches an arbitrary URL");

        ssrf.MapPost("/safe-fetch", async (UrlRequest request, SafeUrlInspector inspector, IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
        {
            var inspection = await inspector.InspectAsync(request.Url, cancellationToken);
            if (!inspection.IsAllowed || inspection.Uri is null)
            {
                return Results.BadRequest(new { error = inspection.Reason });
            }

            var client = httpClientFactory.CreateClient("ssrf-demo");
            using var response = await client.GetAsync(inspection.Uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            return Results.Ok(new
            {
                url = inspection.Uri.ToString(),
                status = (int)response.StatusCode,
                policy = "HTTPS allowlist plus DNS private-address checks."
            });
        })
        .WithSummary("Fetches only allowlisted HTTPS URLs that resolve to public IP addresses");
    }

    private static void MapUploadEndpoints(IEndpointRouteBuilder app)
    {
        var upload = app.MapGroup("/api/upload").WithTags("09. Dangerous File Upload");

        upload.MapPost("/unsafe", async (HttpRequest request, IWebHostEnvironment environment, CancellationToken cancellationToken) =>
        {
            var form = await request.ReadFormAsync(cancellationToken);
            var file = form.Files.GetFile("file");
            if (file is null)
            {
                return Results.BadRequest(new { error = "Upload a multipart file named 'file'." });
            }

            var uploadRoot = Path.Combine(environment.ContentRootPath, "App_Data", "uploads");
            Directory.CreateDirectory(uploadRoot);
            var targetPath = Path.Combine(uploadRoot, file.FileName);
            await using var stream = File.Create(targetPath);
            await file.CopyToAsync(stream, cancellationToken);

            return Results.Ok(new
            {
                savedAs = targetPath,
                risk = "The original filename and extension were trusted."
            });
        })
        .Accepts<IFormFile>("multipart/form-data")
        .DisableAntiforgery()
        .WithSummary("Stores an uploaded file with the caller-provided filename");

        upload.MapPost("/safe", async (HttpRequest request, IWebHostEnvironment environment, CancellationToken cancellationToken) =>
        {
            var form = await request.ReadFormAsync(cancellationToken);
            var file = form.Files.GetFile("file");
            if (file is null)
            {
                return Results.BadRequest(new { error = "Upload a multipart file named 'file'." });
            }

            if (file.Length is <= 0 or > 1_048_576)
            {
                return Results.BadRequest(new { error = "File size must be between 1 byte and 1 MB." });
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".txt", ".png", ".jpg", ".pdf" };
            if (!allowedExtensions.Contains(extension))
            {
                return Results.BadRequest(new { error = "Allowed extensions: .txt, .png, .jpg, .pdf." });
            }

            var uploadRoot = Path.Combine(environment.ContentRootPath, "App_Data", "uploads");
            Directory.CreateDirectory(uploadRoot);
            var safeName = $"{RandomNumberGenerator.GetHexString(16)}{extension}";
            var targetPath = Path.Combine(uploadRoot, safeName);
            await using var stream = File.Create(targetPath);
            await file.CopyToAsync(stream, cancellationToken);

            return Results.Ok(new
            {
                savedAs = safeName,
                originalName = Path.GetFileName(file.FileName),
                size = file.Length
            });
        })
        .Accepts<IFormFile>("multipart/form-data")
        .DisableAntiforgery()
        .WithSummary("Validates extension and size, then stores under a generated name");
    }

    private static void MapFileReadEndpoints(IEndpointRouteBuilder app)
    {
        var files = app.MapGroup("/api/files").WithTags("10. Path Traversal");

        files.MapGet("/unsafe-read", async (string path, IWebHostEnvironment environment, CancellationToken cancellationToken) =>
        {
            var publicRoot = Path.Combine(environment.ContentRootPath, "App_Data", "public");
            var targetPath = Path.Combine(publicRoot, path);
            var content = await File.ReadAllTextAsync(targetPath, cancellationToken);
            return Results.Ok(new { targetPath, content });
        })
        .WithSummary("Combines a user-supplied path with the public directory");

        files.MapGet("/safe-read", async (string fileName, IWebHostEnvironment environment, CancellationToken cancellationToken) =>
        {
            var allowedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "readme.txt" };
            var normalizedName = Path.GetFileName(fileName);
            if (!allowedFiles.Contains(normalizedName))
            {
                return Results.BadRequest(new { error = "Allowed files: readme.txt." });
            }

            var publicRoot = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "App_Data", "public"));
            var targetPath = Path.GetFullPath(Path.Combine(publicRoot, normalizedName));
            if (!targetPath.StartsWith(publicRoot, StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest(new { error = "Resolved path escaped the public directory." });
            }

            var content = await File.ReadAllTextAsync(targetPath, cancellationToken);
            return Results.Ok(new { fileName = normalizedName, content });
        })
        .WithSummary("Uses filename allowlist and full-path containment checks");
    }
}

public sealed record UrlRequest(string Url);
