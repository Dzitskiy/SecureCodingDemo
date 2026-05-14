using System.Text.Json;

namespace SecureCodingDemo.Infrastructure;

public sealed class DemoScenarioCatalog
{
    private readonly DocumentationCatalog _documentationCatalog;

    public DemoScenarioCatalog(DocumentationCatalog documentationCatalog)
    {
        _documentationCatalog = documentationCatalog;
    }

    public IReadOnlyList<DemoScenarioSummary> GetScenarios()
    {
        var sections = _documentationCatalog
            .GetSections()
            .ToDictionary(section => section.Slug, StringComparer.OrdinalIgnoreCase);

        return BuildDefinitions()
            .Where(definition => sections.ContainsKey(definition.Slug))
            .Select(definition =>
            {
                var section = sections[definition.Slug];
                return new DemoScenarioSummary(
                    section.Slug,
                    section.Title,
                    definition.Category,
                    definition.Summary,
                    $"/api/docs/{section.Slug}",
                    definition.Unsafe.Request.Path,
                    definition.Safe.Request.Path);
            })
            .ToArray();
    }

    public DemoScenario? GetScenario(string slug)
    {
        var sections = _documentationCatalog
            .GetSections()
            .ToDictionary(section => section.Slug, StringComparer.OrdinalIgnoreCase);

        if (!sections.TryGetValue(slug, out var section))
        {
            return null;
        }

        var definition = BuildDefinitions()
            .FirstOrDefault(item => item.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

        return definition is null
            ? null
            : new DemoScenario(
                section.Slug,
                section.Title,
                definition.Category,
                definition.Summary,
                $"/api/docs/{section.Slug}",
                definition.Notes,
                definition.Unsafe,
                definition.Safe);
    }

    private static IReadOnlyList<DemoScenarioDefinition> BuildDefinitions()
    {
        var largeJson = JsonSerializer.Serialize(new
        {
            payload = new string('a', 17_000)
        });

        return
        [
            new(
                "xss",
                "Input, Output and Browser Security",
                "Сравни рендеринг пользовательского HTML без экранирования и с HTML-encoding.",
                new DemoVariant(
                    "Unsafe",
                    "Пользовательский ввод вставляется в HTML как есть, поэтому браузер может выполнить script или event handler.",
                    new DemoRequestTemplate("POST", "/api/xss/unsafe", "json", """{"value":"<img src=x onerror=\"alert('xss-demo')\">"}""", null, "application/json", null, null, null, "html"),
                    ["Открой preview и увидишь выполнение inline-скрипта или обработчика события."]),
                new DemoVariant(
                    "Safe",
                    "Ввод HTML-encoded, поэтому браузер показывает строку как текст, а не как исполняемый код.",
                    new DemoRequestTemplate("POST", "/api/xss/safe", "json", """{"value":"<img src=x onerror=\"alert('xss-demo')\">"}""", null, "application/json", null, null, null, "html"),
                    ["В preview HTML отобразится как текст без выполнения JavaScript."])),
            new(
                "sanitization-vs-encoding",
                "Input, Output and Browser Security",
                "Показывает разницу между хранением произвольного rich text и sanitization по allowlist.",
                new DemoVariant(
                    "Unsafe",
                    "Разметка и опасные атрибуты сохраняются как есть.",
                    new DemoRequestTemplate("POST", "/api/html/unsafe-rich-text", "json", """{"value":"<p>Hello</p><img src=x onerror=\"alert('rich-text-xss')\"><a href=\"javascript:alert('link')\">click</a>"}""", null, "application/json", null, null, null, "json"),
                    ["Ответ вернет HTML с опасными тегами и атрибутами без очистки."]),
                new DemoVariant(
                    "Safe",
                    "Оставляет только allowlist тегов и HTTPS-ссылки.",
                    new DemoRequestTemplate("POST", "/api/html/safe-rich-text", "json", """{"value":"<p>Hello</p><img src=x onerror=\"alert('rich-text-xss')\"><a href=\"javascript:alert('link')\">click</a>"}""", null, "application/json", null, null, null, "json"),
                    ["Сравни sanitizedHtml с исходным payload и увидишь, что опасные части удалены."])),
            new(
                "whitelist-validation",
                "Input, Output and Browser Security",
                "Показывает, почему безопаснее принимать конечный набор допустимых значений, а не произвольную строку.",
                new DemoVariant(
                    "Unsafe",
                    "Сервер принимает значение как есть и не ограничивает его формат.",
                    new DemoRequestTemplate("POST", "/api/validation/unsafe", "json", """{"value":"../../etc/passwd"}""", null, "application/json", null, null, null, "json"),
                    ["Такое значение может позже стать частью SQL, пути к файлу или команды."]),
                new DemoVariant(
                    "Safe",
                    "Сервер принимает только allowlist кодов стран.",
                    new DemoRequestTemplate("POST", "/api/validation/safe-country-code", "json", """{"value":"../../etc/passwd"}""", null, "application/json", null, null, null, "json"),
                    ["Поменяй payload на RU, US, DE, FR или CN, чтобы увидеть допустимый сценарий."])),
            new(
                "sql-injection",
                "Injection and Dangerous Execution",
                "Показывает разницу между конкатенацией SQL и параметризованным запросом.",
                new DemoVariant(
                    "Unsafe",
                    "Логин строится через string interpolation и поддается SQL injection.",
                    new DemoRequestTemplate("POST", "/api/sql/unsafe-login", "json", """{"userName":"alice' OR '1'='1","password":"irrelevant"}""", null, "application/json", null, null, null, "json"),
                    ["В ответе видно сформированную SQL-строку и признак injectionDetected."]),
                new DemoVariant(
                    "Safe",
                    "Параметры отделены от SQL-синтаксиса.",
                    new DemoRequestTemplate("POST", "/api/sql/safe-login", "json", """{"userName":"alice' OR '1'='1","password":"irrelevant"}""", null, "application/json", null, null, null, "json"),
                    ["Для успешного логина попробуй alice / correct-horse-battery-staple."])),
            new(
                "dos-resource-exhaustion",
                "Availability and Resource Protection",
                "Сравни endpoint, который без ограничений читает body, и endpoint с лимитом размера и concurrency limiter.",
                new DemoVariant(
                    "Unsafe",
                    "Считывает и эхо-возвращает весь body.",
                    new DemoRequestTemplate("POST", "/api/dos/unsafe-echo", "text", new string('A', 20_000), null, "text/plain", null, null, null, "json"),
                    ["Наглядно видно, что сервер безоговорочно принимает и возвращает крупный payload."]),
                new DemoVariant(
                    "Safe",
                    "Ограничивает размер body до 16 KB и требует корректный JSON.",
                    new DemoRequestTemplate("POST", "/api/dos/safe-echo", "json", largeJson, null, "application/json", null, null, null, "json"),
                    ["Ожидаемая реакция на sample payload: BadRequest из-за превышения лимита."])),
            new(
                "rate-limiting",
                "Availability and Resource Protection",
                "Показывает, как одно и то же login API ведет себя без throttling и с fixed-window limiter.",
                new DemoVariant(
                    "Unsafe",
                    "Без rate limiting brute force попытки почти бесплатны.",
                    new DemoRequestTemplate("POST", "/api/login/unsafe", "json", """{"userName":"alice","password":"wrong-password"}""", null, "application/json", null, null, null, "json"),
                    ["Нажимай Run несколько раз подряд: endpoint всегда отвечает одинаково."]),
                new DemoVariant(
                    "Safe",
                    "После 5 попыток в минуту endpoint начнет отвечать 429.",
                    new DemoRequestTemplate("POST", "/api/login/safe", "json", """{"userName":"alice","password":"wrong-password"}""", null, "application/json", null, null, null, "json"),
                    ["Запусти safe-вариант 6 раз подряд, чтобы увидеть rate limit."])),
            new(
                "mass-assignment",
                "Authorization and Access Boundaries",
                "Показывает, как прямое биндинг-сопоставление позволяет клиенту менять поля, которые должны быть server-controlled.",
                new DemoVariant(
                    "Unsafe",
                    "Клиент может передать isAdmin в том же DTO.",
                    new DemoRequestTemplate("POST", "/api/users/unsafe-profile", "json", """{"displayName":"Mallory","department":"Operations","isAdmin":true}""", null, "application/json", null, null, null, "json"),
                    ["В ответе пользователь станет администратором, хотя это не должно контролироваться клиентом."]),
                new DemoVariant(
                    "Safe",
                    "API принимает только writable-поля.",
                    new DemoRequestTemplate("POST", "/api/users/safe-profile", "json", """{"displayName":"Mallory","department":"Operations","isAdmin":true}""", null, "application/json", null, null, null, "json"),
                    ["Сравни результат с unsafe: флаг isAdmin будет проигнорирован."])),
            new(
                "ssrf",
                "Network and File Security",
                "Показывает разницу между произвольным outbound request и allowlist-политикой с DNS-проверкой.",
                new DemoVariant(
                    "Unsafe",
                    "Сервер делает запрос по URL, который полностью контролирует клиент.",
                    new DemoRequestTemplate("POST", "/api/ssrf/unsafe-fetch", "json", """{"url":"https://example.com/"}""", null, "application/json", null, null, null, "json"),
                    ["Поменяй sample URL на внутренний или неожиданный адрес, чтобы смоделировать SSRF."]),
                new DemoVariant(
                    "Safe",
                    "Разрешены только HTTPS и явный allowlist хостов.",
                    new DemoRequestTemplate("POST", "/api/ssrf/safe-fetch", "json", """{"url":"http://127.0.0.1/"}""", null, "application/json", null, null, null, "json"),
                    ["Ожидаемая реакция: safe endpoint отклонит localhost и non-HTTPS."])),
            new(
                "dangerous-file-upload",
                "Network and File Security",
                "Показывает, как небезопасная загрузка доверяет имени и расширению файла.",
                new DemoVariant(
                    "Unsafe",
                    "Файл сохраняется под исходным именем без проверки расширения.",
                    new DemoRequestTemplate("POST", "/api/upload/unsafe", "multipart", null, null, null, "shell.aspx", "<script>alert('uploaded')</script>", null, "json"),
                    ["Unsafe endpoint примет даже подозрительное расширение и сохранит имя как есть."]),
                new DemoVariant(
                    "Safe",
                    "Разрешены только ограниченные расширения и сгенерированное имя файла.",
                    new DemoRequestTemplate("POST", "/api/upload/safe", "multipart", null, null, null, "notes.txt", "Quarterly report draft", null, "json"),
                    ["Safe endpoint заменит имя файла на случайное и проверит размер/расширение."])),
            new(
                "path-traversal",
                "Network and File Security",
                "Показывает, как выход за пределы разрешенного каталога происходит через небезопасное склеивание path segments.",
                new DemoVariant(
                    "Unsafe",
                    "Клиент управляет относительным путем и может добраться до private-файлов.",
                    new DemoRequestTemplate("GET", "/api/files/unsafe-read", "query", null, "path=../private/secrets.txt", null, null, null, null, "json"),
                    ["Unsafe endpoint прочитает demo-secret из private-каталога."]),
                new DemoVariant(
                    "Safe",
                    "Используется allowlist имен и проверка containment после нормализации пути.",
                    new DemoRequestTemplate("GET", "/api/files/safe-read", "query", null, "fileName=readme.txt", null, null, null, null, "json"),
                    ["Попробуй заменить fileName на ../private/secrets.txt и увидишь отказ."])),
            new(
                "jwt-idor",
                "Authorization and Access Boundaries",
                "Показывает, что знания идентификатора недостаточно, если сервер проверяет ownership по JWT.",
                new DemoVariant(
                    "Unsafe",
                    "Возвращает заказ только по orderId, не проверяя владельца.",
                    new DemoRequestTemplate("GET", "/api/orders/unsafe/103", "none", null, null, null, null, null, null, "json"),
                    ["Order 103 принадлежит Bob, но unsafe endpoint все равно вернет его любому клиенту."]),
                new DemoVariant(
                    "Safe",
                    "Требует JWT и проверяет, что order принадлежит авторизованному пользователю.",
                    new DemoRequestTemplate("GET", "/api/orders/safe/103", "none", null, null, null, null, null, new DemoAuthTemplate("/api/auth/token", "alice", "correct-horse-battery-staple"), "json"),
                    ["Токен для Alice будет получен автоматически; order 103 ей не принадлежит, поэтому safe endpoint вернет not found."])),
            new(
                "dangerous-logging",
                "Configuration, Secrets and Observability",
                "Сравни полное логирование payload и логирование с redaction чувствительных полей.",
                new DemoVariant(
                    "Unsafe",
                    "Логи получают пароль и токен в открытом виде.",
                    new DemoRequestTemplate("POST", "/api/logging/unsafe", "json", """{"userName":"alice","password":"secret-password","accessToken":"demo-token"}""", null, "application/json", null, null, null, "json"),
                    ["Содержимое ответа и логов показывает, что чувствительные поля утекли."]),
                new DemoVariant(
                    "Safe",
                    "Чувствительные поля редактируются до записи в лог.",
                    new DemoRequestTemplate("POST", "/api/logging/safe", "json", """{"userName":"alice","password":"secret-password","accessToken":"demo-token"}""", null, "application/json", null, null, null, "json"),
                    ["В ответе и логах password/accessToken будут замаскированы."])),
            new(
                "dependency-security",
                "Dependency and Supply Chain Security",
                "Показывает, почему пакет, версия и источник должны проходить policy-check, а не браться напрямую из запроса.",
                new DemoVariant(
                    "Unsafe",
                    "План установки строится из произвольного package/source/version.",
                    new DemoRequestTemplate("POST", "/api/dependencies/unsafe-install-plan", "json", """{"packageName":"TotallyUnknown.Package","version":"latest","source":"http://evil.example/feed"}""", null, "application/json", null, null, null, "json"),
                    ["Unsafe endpoint просто возвращает команду установки без trust-checks."]),
                new DemoVariant(
                    "Safe",
                    "Разрешены только allowlisted packages, pinned versions и trusted source.",
                    new DemoRequestTemplate("POST", "/api/dependencies/safe-install-plan", "json", """{"packageName":"Serilog.AspNetCore","version":"9.0.0","source":"https://api.nuget.org/v3/index.json"}""", null, "application/json", null, null, null, "json"),
                    ["Попробуй поменять package или version на floating, чтобы увидеть policy rejection."])),
            new(
                "cancellation-token",
                "Defensive Runtime Practices",
                "Сравни долгую операцию, которая игнорирует отмену, и операцию, принимающую CancellationToken.",
                new DemoVariant(
                    "Unsafe",
                    "Даже если клиент отменит ожидание, сервер продолжит Task.Delay до конца.",
                    new DemoRequestTemplate("GET", "/api/cancellation/unsafe", "query", null, "seconds=5", null, null, null, null, "json"),
                    ["Для ручной проверки можно остановить клиентский запрос раньше срока и посмотреть, что серверный unsafe-work не уважает cancel."]),
                new DemoVariant(
                    "Safe",
                    "Операция передает CancellationToken дальше в медленную работу.",
                    new DemoRequestTemplate("GET", "/api/cancellation/safe", "query", null, "seconds=5", null, null, null, null, "json"),
                    ["Safe endpoint полезнее проверять через клиент, который умеет прерывать запрос."])),
            new(
                "csp",
                "Input, Output and Browser Security",
                "Показывает страницу без CSP и страницу с restrictive Content-Security-Policy.",
                new DemoVariant(
                    "Unsafe",
                    "Страница отдается без защитных заголовков браузера.",
                    new DemoRequestTemplate("GET", "/api/csp/unsafe-page", "none", null, null, null, null, null, null, "html"),
                    ["Открой actual route или preview, чтобы увидеть inline script на странице."]),
                new DemoVariant(
                    "Safe",
                    "Ответ выставляет строгий CSP и не содержит inline script.",
                    new DemoRequestTemplate("GET", "/api/csp/safe-page", "none", null, null, null, null, null, null, "html"),
                    ["Для проверки заголовка используй response headers или открой actual route в отдельной вкладке."])),
            new(
                "insecure-deserialization",
                "Injection and Dangerous Execution",
                "Показывает опасность client-controlled CLR type names и безопасную альтернативу с logical action kind.",
                new DemoVariant(
                    "Unsafe",
                    "Клиент сам выбирает тип, который сервер попытается создать и выполнить.",
                    new DemoRequestTemplate("POST", "/api/deserialization/unsafe-action", "json", """{"type":"SecureCodingDemo.Modules.GrantAdminAction, SecureCodingDemo","payload":"user=bob"}""", null, "application/json", null, null, null, "json"),
                    ["Unsafe endpoint выполнит demo-action, если тип удалось загрузить по имени."]),
                new DemoVariant(
                    "Safe",
                    "Разрешены только логические виды действий из allowlist.",
                    new DemoRequestTemplate("POST", "/api/deserialization/safe-action", "json", """{"kind":"echo","payload":"hello"}""", null, "application/json", null, null, null, "json"),
                    ["Попробуй заменить kind на grant-admin, чтобы увидеть отказ."])),
            new(
                "secrets-management",
                "Configuration, Secrets and Observability",
                "Показывает прямую утечку конфигурационных секретов и redacted-safe вариант.",
                new DemoVariant(
                    "Unsafe",
                    "Секреты возвращаются клиенту в теле ответа.",
                    new DemoRequestTemplate("GET", "/api/secrets/unsafe", "none", null, null, null, null, null, null, "json"),
                    ["Unsafe endpoint специально демонстрирует, почему API не должен раскрывать signing keys и connection strings."]),
                new DemoVariant(
                    "Safe",
                    "API возвращает только маскированные значения и рекомендации по хранению секретов.",
                    new DemoRequestTemplate("GET", "/api/secrets/safe", "none", null, null, null, null, null, null, "json"),
                    ["Сравни поле jwtSigningKey между unsafe и safe."])),
            new(
                "cache-stampede",
                "Availability and Resource Protection",
                "Сравни обновление cache без single-flight lock и с ним.",
                new DemoVariant(
                    "Unsafe",
                    "Несколько параллельных запросов могут одновременно пересчитывать один и тот же report.",
                    new DemoRequestTemplate("GET", "/api/cache/unsafe-report", "none", null, null, null, null, null, null, "json"),
                    ["Открой несколько вкладок или быстро нажимай Run, чтобы увидеть рост refreshNumber."]),
                new DemoVariant(
                    "Safe",
                    "Пересчет защищен semaphore lock и делает только один refresh.",
                    new DemoRequestTemplate("GET", "/api/cache/safe-report", "none", null, null, null, null, null, null, "json"),
                    ["Сравни поведение с unsafe при быстрых повторных запросах."])),
            new(
                "regex-dos",
                "Availability and Resource Protection",
                "Показывает проблему catastrophic backtracking без timeout и ограничений шаблона.",
                new DemoVariant(
                    "Unsafe",
                    "Регулярное выражение допускает ReDoS-паттерн без timeout.",
                    new DemoRequestTemplate("POST", "/api/regex/unsafe-validate", "json", """{"value":"aaaaaaaaaaaaaaaaaaaaaaaaaaaa!"}""", null, "application/json", null, null, null, "json"),
                    ["Чем длиннее строка с суффиксом !, тем заметнее риск backtracking."]),
                new DemoVariant(
                    "Safe",
                    "Шаблон ограничен по длине и использует timeout.",
                    new DemoRequestTemplate("POST", "/api/regex/safe-validate", "json", """{"value":"aaaaaaaaaaaaaaaaaaaaaaaaaaaa!"}""", null, "application/json", null, null, null, "json"),
                    ["Safe endpoint либо быстро отвечает, либо завершает проверку по timeout."])),
            new(
                "unsafe-reflection",
                "Injection and Dangerous Execution",
                "Показывает difference между client-controlled method name и allowlisted operation mapping.",
                new DemoVariant(
                    "Unsafe",
                    "Клиент может вызвать внутренний метод по имени.",
                    new DemoRequestTemplate("POST", "/api/reflection/unsafe-invoke", "json", """{"methodName":"DropUserSessions"}""", null, "application/json", null, null, null, "json"),
                    ["Unsafe endpoint выполнит внутреннюю операцию, если имя метода совпало."]),
                new DemoVariant(
                    "Safe",
                    "Разрешены только публичные операции status и version.",
                    new DemoRequestTemplate("POST", "/api/reflection/safe-invoke", "json", """{"operation":"status"}""", null, "application/json", null, null, null, "json"),
                    ["Попробуй указать dropusersessions и safe endpoint его отклонит."])),
            new(
                "broken-access-control",
                "Authorization and Access Boundaries",
                "Практический пример broken access control через доступ к чужому объекту по известному id.",
                new DemoVariant(
                    "Unsafe",
                    "Доступ к объекту не зависит от контекста пользователя.",
                    new DemoRequestTemplate("GET", "/api/orders/unsafe/103", "none", null, null, null, null, null, null, "json"),
                    ["Unsafe variant возвращает payroll export Bob без каких-либо проверок."]),
                new DemoVariant(
                    "Safe",
                    "Ресурс выдается только владельцу из JWT claims.",
                    new DemoRequestTemplate("GET", "/api/orders/safe/103", "none", null, null, null, null, null, new DemoAuthTemplate("/api/auth/token", "alice", "correct-horse-battery-staple"), "json"),
                    ["Сценарий специально настроен на чужой orderId, чтобы safe endpoint отказал Alice."])),
            new(
                "security-misconfiguration",
                "Configuration, Secrets and Observability",
                "Показывает response с лишними diagnostic details и ответ с базовыми security headers.",
                new DemoVariant(
                    "Unsafe",
                    "Ответ раскрывает environment, content root и diagnostic details.",
                    new DemoRequestTemplate("GET", "/api/configuration/unsafe-headers", "none", null, null, null, null, null, null, "json"),
                    ["Смотри и на body, и на отсутствие защитных заголовков."]),
                new DemoVariant(
                    "Safe",
                    "Ответ нейтрален и выставляет базовые security headers.",
                    new DemoRequestTemplate("GET", "/api/configuration/safe-headers", "none", null, null, null, null, null, null, "json"),
                    ["Сравни response headers между unsafe и safe вариантами."]))
        ];
    }
}

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

internal sealed record DemoScenarioDefinition(
    string Slug,
    string Category,
    string Summary,
    DemoVariant Unsafe,
    DemoVariant Safe,
    IReadOnlyList<string>? Notes = null)
{
    public IReadOnlyList<string> Notes { get; } = Notes ?? [];
}
