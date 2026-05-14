using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Ganss.Xss;

namespace SecureCodingDemo.Modules;

public static class InputSecurityEndpoints
{
    public static IEndpointRouteBuilder MapInputSecurityEndpoints(this IEndpointRouteBuilder app)
    {
        var xss = app.MapGroup("/api/xss").WithTags("01. XSS");

        xss.MapPost("/unsafe", (HtmlInput input) =>
            Results.Text($"<html><body><h1>Comment</h1><div>{input.Value}</div></body></html>", "text/html"))
            .WithSummary("Renders user input as raw HTML");

        xss.MapPost("/safe", (HtmlInput input) =>
        {
            var encoded = HtmlEncoder.Default.Encode(input.Value);
            return Results.Text($"<html><body><h1>Comment</h1><div>{encoded}</div></body></html>", "text/html");
        })
        .WithSummary("HTML-encodes user input before rendering");

        var html = app.MapGroup("/api/html").WithTags("02. Sanitization vs Encoding");

        html.MapPost("/unsafe-rich-text", (HtmlInput input) =>
            Results.Ok(new
            {
                renderedHtml = input.Value,
                risk = "Active HTML, event handlers and javascript: URLs remain executable."
            }))
            .WithSummary("Stores arbitrary rich text without sanitization");

        html.MapPost("/safe-rich-text", (HtmlInput input) =>
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedSchemes.Clear();
            sanitizer.AllowedSchemes.Add("https");
            sanitizer.AllowedTags.Clear();
            foreach (var tag in new[] { "p", "strong", "em", "ul", "ol", "li", "a", "code", "pre", "br" })
            {
                sanitizer.AllowedTags.Add(tag);
            }

            return Results.Ok(new
            {
                sanitizedHtml = sanitizer.Sanitize(input.Value),
                policy = "Allowlist tags and HTTPS links only."
            });
        })
        .WithSummary("Sanitizes rich text with an explicit allowlist");

        var validation = app.MapGroup("/api/validation").WithTags("03. Whitelist Validation");

        validation.MapPost("/unsafe", (ValidationInput input) =>
            Results.Ok(new
            {
                accepted = input.Value,
                risk = "The value is accepted as-is and may later become a path, SQL fragment or command argument."
            }))
            .WithSummary("Accepts unbounded input");

        validation.MapPost("/safe-country-code", (ValidationInput input) =>
        {
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "RU", "US", "DE", "FR", "CN" };
            return allowed.Contains(input.Value)
                ? Results.Ok(new { accepted = input.Value.ToUpperInvariant() })
                : Results.BadRequest(new { error = "Use one of: RU, US, DE, FR, CN." });
        })
        .WithSummary("Validates against a finite allowlist");

        var regex = app.MapGroup("/api/regex").WithTags("19. Regex DoS");

        regex.MapPost("/unsafe-validate", (RegexInput input) =>
        {
            var matched = Regex.IsMatch(input.Value, "^(a+)+$");
            return Results.Ok(new { matched, pattern = "^(a+)+$", warning = "No timeout is used." });
        })
        .WithSummary("Uses a catastrophic regex without timeout");

        regex.MapPost("/safe-validate", (RegexInput input) =>
        {
            try
            {
                var matched = Regex.IsMatch(input.Value, "^[a]{1,64}$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
                return Results.Ok(new { matched, pattern = "^[a]{1,64}$", timeout = "100ms" });
            }
            catch (RegexMatchTimeoutException)
            {
                return Results.BadRequest(new { error = "Regex evaluation timed out." });
            }
        })
        .WithSummary("Uses a bounded regex and a match timeout");

        var csp = app.MapGroup("/api/csp").WithTags("15. CSP");

        csp.MapGet("/unsafe-page", () =>
            Results.Text("<html><body><script>alert('inline')</script><h1>No CSP</h1></body></html>", "text/html"))
            .WithSummary("Returns a page without Content-Security-Policy");

        csp.MapGet("/safe-page", (HttpContext context) =>
        {
            context.Response.Headers.ContentSecurityPolicy = "default-src 'none'; style-src 'self'; img-src 'self'; base-uri 'none'; frame-ancestors 'none'";
            var html = $"<html><body><h1>{WebUtility.HtmlEncode("CSP blocks inline scripts")}</h1></body></html>";
            return Results.Text(html, "text/html");
        })
        .WithSummary("Returns a page with a restrictive CSP header");

        return app;
    }
}

public sealed record HtmlInput(string Value);

public sealed record ValidationInput(string Value);

public sealed record RegexInput(string Value);
