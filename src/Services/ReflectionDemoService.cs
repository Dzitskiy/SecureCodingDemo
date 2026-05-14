namespace SecureCodingDemo.Services;

public sealed class ReflectionDemoService
{
    private readonly Dictionary<string, Func<string>> _allowedOperations;

    public ReflectionDemoService()
    {
        _allowedOperations = new(StringComparer.OrdinalIgnoreCase)
        {
            ["status"] = GetStatus,
            ["version"] = GetVersion
        };
    }

    public object? UnsafeInvoke(string methodName)
    {
        var method = GetType().GetMethod(methodName);
        return method?.Invoke(this, null);
    }

    public string? SafeInvoke(string operation)
    {
        return _allowedOperations.TryGetValue(operation, out var handler) ? handler() : null;
    }

    public string GetStatus() => "demo service is running";

    public string GetVersion() => "1.0-demo";

    public string DropUserSessions() => "dangerous operation would have invalidated every session";
}
