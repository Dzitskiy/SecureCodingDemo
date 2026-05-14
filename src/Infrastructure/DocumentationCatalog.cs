using System.Text.RegularExpressions;

namespace SecureCodingDemo.Infrastructure;

public sealed partial class DocumentationCatalog
{
    private readonly IWebHostEnvironment _environment;

    private static readonly IReadOnlyDictionary<string, EndpointPair> EndpointPairs =
        new Dictionary<string, EndpointPair>(StringComparer.OrdinalIgnoreCase)
        {
            ["xss"] = new("/api/xss/unsafe", "/api/xss/safe"),
            ["sanitization-vs-encoding"] = new("/api/html/unsafe-rich-text", "/api/html/safe-rich-text"),
            ["whitelist-validation"] = new("/api/validation/unsafe", "/api/validation/safe-country-code"),
            ["sql-injection"] = new("/api/sql/unsafe-login", "/api/sql/safe-login"),
            ["dos-resource-exhaustion"] = new("/api/dos/unsafe-echo", "/api/dos/safe-echo"),
            ["rate-limiting"] = new("/api/login/unsafe", "/api/login/safe"),
            ["mass-assignment"] = new("/api/users/unsafe-profile", "/api/users/safe-profile"),
            ["ssrf"] = new("/api/ssrf/unsafe-fetch", "/api/ssrf/safe-fetch"),
            ["dangerous-file-upload"] = new("/api/upload/unsafe", "/api/upload/safe"),
            ["path-traversal"] = new("/api/files/unsafe-read?path=../private/secrets.txt", "/api/files/safe-read?fileName=readme.txt"),
            ["jwt-idor"] = new("/api/orders/unsafe/103", "/api/orders/safe/103"),
            ["dangerous-logging"] = new("/api/logging/unsafe", "/api/logging/safe"),
            ["dependency-security"] = new("/api/dependencies/unsafe-install-plan", "/api/dependencies/safe-install-plan"),
            ["cancellation-token"] = new("/api/cancellation/unsafe?seconds=30", "/api/cancellation/safe?seconds=30"),
            ["csp"] = new("/api/csp/unsafe-page", "/api/csp/safe-page"),
            ["insecure-deserialization"] = new("/api/deserialization/unsafe-action", "/api/deserialization/safe-action"),
            ["secrets-management"] = new("/api/secrets/unsafe", "/api/secrets/safe"),
            ["cache-stampede"] = new("/api/cache/unsafe-report", "/api/cache/safe-report"),
            ["regex-dos"] = new("/api/regex/unsafe-validate", "/api/regex/safe-validate"),
            ["unsafe-reflection"] = new("/api/reflection/unsafe-invoke", "/api/reflection/safe-invoke"),
            ["broken-access-control"] = new("/api/orders/unsafe/103", "/api/orders/safe/103"),
            ["security-misconfiguration"] = new("/api/configuration/unsafe-headers", "/api/configuration/safe-headers")
        };

    public DocumentationCatalog(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public IReadOnlyList<SectionInfo> GetSections()
    {
        var docsRoot = ResolveDocsSectionsRoot();
        if (!Directory.Exists(docsRoot))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(docsRoot, "*.md")
            .OrderBy(Path.GetFileName)
            .Select(CreateSectionInfo)
            .ToArray();
    }

    public DocumentationSection? GetSection(string slug)
    {
        var section = GetSections().FirstOrDefault(item => item.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
        if (section is null)
        {
            return null;
        }

        var markdown = File.ReadAllText(section.Path);
        return new DocumentationSection(section.Slug, section.Title, section.UnsafeEndpoint, section.SafeEndpoint, markdown);
    }

    private static SectionInfo CreateSectionInfo(string path)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        var slug = SectionFileNameRegex().Replace(fileName, string.Empty);
        var firstLine = File.ReadLines(path).FirstOrDefault() ?? fileName;
        var title = firstLine.TrimStart('#', ' ');
        EndpointPairs.TryGetValue(slug, out var pair);

        return new SectionInfo(
            slug,
            title,
            pair?.Unsafe,
            pair?.Safe,
            path);
    }

    private string ResolveDocsSectionsRoot()
    {
        var candidates = new[]
        {
            Path.Combine(_environment.ContentRootPath, "docs", "sections"),
            Path.Combine(_environment.ContentRootPath, "Docs", "sections"),
            Path.Combine(_environment.ContentRootPath, "..", "docs", "sections"),
            Path.Combine(_environment.ContentRootPath, "..", "..", "docs", "sections"),
            Path.Combine(AppContext.BaseDirectory, "docs", "sections"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "docs", "sections")
        };

        return candidates
            .Select(Path.GetFullPath)
            .FirstOrDefault(Directory.Exists)
            ?? Path.GetFullPath(candidates[0]);
    }

    [GeneratedRegex(@"^\d+-")]
    private static partial Regex SectionFileNameRegex();
}

public sealed record EndpointPair(string Unsafe, string Safe);

public sealed record SectionInfo(
    string Slug,
    string Title,
    string? UnsafeEndpoint,
    string? SafeEndpoint,
    string Path);

public sealed record DocumentationSection(
    string Slug,
    string Title,
    string? UnsafeEndpoint,
    string? SafeEndpoint,
    string Markdown);
