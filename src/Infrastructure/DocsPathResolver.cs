using System.Text.RegularExpressions;

namespace SecureCodingDemo.Infrastructure;

public static partial class DocsPathResolver
{
    public static string ResolveDocsRoot(IWebHostEnvironment environment)
    {
        var candidates = new[]
        {
            Path.Combine(environment.ContentRootPath, "docs"),
            Path.Combine(environment.ContentRootPath, "Docs"),
            Path.Combine(environment.ContentRootPath, "..", "docs"),
            Path.Combine(environment.ContentRootPath, "..", "..", "docs"),
            Path.Combine(AppContext.BaseDirectory, "docs"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "docs")
        };

        return candidates
            .Select(Path.GetFullPath)
            .First(Directory.Exists);
    }

    public static bool IsSectionFile(string pathOrFileName)
    {
        return SectionFileNameRegex().IsMatch(Path.GetFileNameWithoutExtension(pathOrFileName));
    }

    [GeneratedRegex(@"^\d+-")]
    private static partial Regex SectionFileNameRegex();
}
