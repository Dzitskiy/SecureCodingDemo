namespace SecureCodingDemo.Infrastructure;

public sealed class DocumentationCatalog
{
    private readonly IWebHostEnvironment _environment;

    public DocumentationCatalog(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public IReadOnlyList<SectionInfo> GetSections()
    {
        var docsRoot = DocsPathResolver.ResolveDocsRoot(_environment);
        if (!Directory.Exists(docsRoot))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(docsRoot, "*.md")
            .Where(DocsPathResolver.IsSectionFile)
            .OrderBy(Path.GetFileName)
            .Select(CreateSectionInfo)
            .ToArray();
    }

    private static SectionInfo CreateSectionInfo(string path)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        var slug = fileName[(fileName.IndexOf('-') + 1)..];
        var firstLine = File.ReadLines(path).FirstOrDefault() ?? fileName;
        var title = firstLine.TrimStart('#', ' ');
        return new SectionInfo(slug, title, path);
    }
}

public sealed record SectionInfo(
    string Slug,
    string Title,
    string Path);
