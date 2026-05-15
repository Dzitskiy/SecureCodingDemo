namespace SecureCodingDemo.Infrastructure;

public sealed class DemoTopicCatalog
{
    private static readonly IReadOnlyList<DemoTopicDefinition> Topics =
    [
        new("xss", "Input, Output and Browser Security", "Сравни рендеринг пользовательского HTML без экранирования и с HTML-encoding."),
        new("sanitization-vs-encoding", "Input, Output and Browser Security", "Показывает разницу между хранением произвольного rich text и очисткой по allowlist."),
        new("whitelist-validation", "Input, Output and Browser Security", "Показывает, почему безопаснее принимать конечный набор допустимых значений, а не произвольную строку."),
        new("sql-injection", "Injection and Dangerous Execution", "Показывает разницу между конкатенацией SQL и параметризованным запросом."),
        new("dos-resource-exhaustion", "Availability and Resource Protection", "Сравни endpoint, который без ограничений читает body, и endpoint с лимитом размера и concurrency limiter."),
        new("rate-limiting", "Availability and Resource Protection", "Показывает, как одно и то же login API ведет себя без throttling и с fixed-window limiter."),
        new("mass-assignment", "Authorization and Access Boundaries", "Показывает, как прямое сопоставление входной модели позволяет клиенту менять поля, которые должны контролироваться сервером."),
        new("ssrf", "Network and File Security", "Показывает разницу между произвольным исходящим запросом и политикой allowlist с DNS-проверкой."),
        new("dangerous-file-upload", "Network and File Security", "Показывает, как небезопасная загрузка доверяет имени и расширению файла."),
        new("path-traversal", "Network and File Security", "Показывает, как выход за пределы разрешенного каталога происходит через небезопасное склеивание частей пути."),
        new("jwt-idor", "Authorization and Access Boundaries", "Показывает, что знания идентификатора недостаточно, если сервер проверяет владение ресурсом по JWT."),
        new("dangerous-logging", "Configuration, Secrets and Observability", "Сравни полное логирование payload и логирование с маскировкой чувствительных полей."),
        new("dependency-security", "Dependency and Supply Chain Security", "Показывает, почему пакет, версия и источник должны проходить policy-check, а не браться напрямую из запроса."),
        new("cancellation-token", "Defensive Runtime Practices", "Сравни долгую операцию, которая игнорирует отмену, и операцию, принимающую CancellationToken."),
        new("csp", "Input, Output and Browser Security", "Показывает страницу без CSP и страницу со строгой Content-Security-Policy."),
        new("insecure-deserialization", "Injection and Dangerous Execution", "Показывает опасность client-controlled CLR type names и безопасную альтернативу с логическим видом действия."),
        new("secrets-management", "Configuration, Secrets and Observability", "Показывает прямую утечку конфигурационных секретов и безопасный вариант с маскировкой."),
        new("cache-stampede", "Availability and Resource Protection", "Сравни обновление cache без single-flight lock и с ним."),
        new("regex-dos", "Availability and Resource Protection", "Показывает проблему catastrophic backtracking без timeout и ограничений шаблона."),
        new("unsafe-reflection", "Injection and Dangerous Execution", "Показывает разницу между вызовом метода по имени из запроса и allowlist-сопоставлением операций."),
        new("broken-access-control", "Authorization and Access Boundaries", "Практический пример broken access control через доступ к чужому объекту по известному id."),
        new("security-misconfiguration", "Configuration, Secrets and Observability", "Показывает ответ с лишними диагностическими деталями и вариант с базовыми защитными заголовками.")
    ];

    public IReadOnlyList<DemoTopicDefinition> GetTopics() => Topics;

    public DemoTopicDefinition? GetTopic(string slug)
    {
        return Topics.FirstOrDefault(topic => topic.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed record DemoTopicDefinition(
    string Slug,
    string Category,
    string Summary);
