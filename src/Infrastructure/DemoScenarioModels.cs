namespace SecureCodingDemo.Infrastructure;

public sealed record DemoScenarioSummary(
    string Slug,
    string Title,
    string Category,
    string Summary,
    string DocumentationUrl,
    string UnsafeEndpoint,
    string SafeEndpoint);

public sealed record DemoScenario(
    string Slug,
    string Title,
    string Category,
    string Summary,
    string DocumentationUrl,
    IReadOnlyList<string> Notes,
    DemoVariant Unsafe,
    DemoVariant Safe);

public sealed record DemoVariant(
    string Label,
    string Description,
    DemoRequestTemplate Request,
    IReadOnlyList<string> Checkpoints);

public sealed record DemoRequestTemplate(
    string Method,
    string Path,
    string Kind,
    string? Body,
    string? Query,
    string? ContentType,
    string? FileName,
    string? FileContent,
    DemoAuthTemplate? Auth,
    string DisplayMode);

public sealed record DemoAuthTemplate(
    string TokenPath,
    string UserName,
    string Password);
