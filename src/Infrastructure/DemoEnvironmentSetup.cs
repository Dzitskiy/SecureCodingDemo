namespace SecureCodingDemo.Infrastructure;

public static class DemoEnvironmentSetup
{
    public static void EnsureDemoAssets(string contentRootPath)
    {
        var dataRoot = Path.Combine(contentRootPath, "App_Data");
        var publicRoot = Path.Combine(dataRoot, "public");
        var privateRoot = Path.Combine(dataRoot, "private");
        var uploadRoot = Path.Combine(dataRoot, "uploads");

        Directory.CreateDirectory(publicRoot);
        Directory.CreateDirectory(privateRoot);
        Directory.CreateDirectory(uploadRoot);

        var publicReadme = Path.Combine(publicRoot, "readme.txt");
        if (!File.Exists(publicReadme))
        {
            File.WriteAllText(publicReadme, "This file is intentionally public for the path traversal demo.");
        }

        var secretFile = Path.Combine(privateRoot, "secrets.txt");
        if (!File.Exists(secretFile))
        {
            File.WriteAllText(secretFile, "demo-secret=do-not-commit-real-secrets");
        }
    }
}
