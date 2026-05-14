# SecureCodingDemo

Учебное приложение для демонстрации типовых ошибок Secure Coding в ASP.NET Core. 
Каждый пример уязвимости содержит описание проблемы, сценарий атаки, пример уязвимого поведения, безопасный подход и production checklist.

>Для быстрого старта открой `/demo`: там можно выбрать любой сценарий, выполнить готовый `unsafe` и `safe` запрос и сразу сравнить результат. Для raw API exploration используй `/swagger`.

## Запуск локально

```powershell
dotnet restore SecureCodingDemo.slnx
dotnet run --project src/SecureCodingDemo.csproj
```

## Запуск через Docker Compose

```powershell
docker compose up --build
```

Приложение будет доступно на `http://localhost:8080`, PostgreSQL на `localhost:5432`, Redis на `localhost:6379`.

## Базовые принципы Secure Coding

- Не доверять никакому внешнему вводу: данные от клиента должны валидироваться, ограничиваться и кодироваться в нужном контексте.
- Разделять данные и инструкции: SQL, HTML, file paths, reflection targets и package sources не должны строиться из произвольной строки.
- Минимизировать привилегии: DB users, service accounts, JWT claims, публичные DTO и filesystem access должны иметь только необходимый минимум прав.
- Уважать ресурсные лимиты: body size, timeouts, cancellation, rate limiting и concurrency limits обязательны для public API.
- Защищать supply chain: фиксировать версии, проверять advisories, использовать доверенные package sources и хранить секреты вне репозитория.

## Secure Coding в ASP.NET Core

<details open>
<summary><strong>Ввод, HTML-вывод и безопасность браузера / Input, Output and Browser Security</strong></summary>

Темы про данные, которые приходят от пользователя и затем попадают в HTML, UI или бизнес-логику. Главная идея блока: валидировать ввод, кодировать вывод в нужном контексте и использовать браузерные защитные механизмы как дополнительный слой.

- [Whitelist Validation / Валидация по allowlist](docs/sections/03-whitelist-validation.md)
- [XSS / Межсайтовый скриптинг](docs/sections/01-xss.md)
- [Sanitization vs Encoding / Очистка HTML и контекстное кодирование](docs/sections/02-sanitization-vs-encoding.md)
- [CSP / Content Security Policy](docs/sections/15-csp.md)

</details>

<details>
<summary><strong>Инъекции и опасное выполнение кода / Injection and Dangerous Execution</strong></summary>

Темы, где пользовательский ввод начинает управлять инструкциями приложения: SQL-запросом, типом объекта, reflection-вызовом или другим исполняемым поведением.

- [SQL Injection / SQL-инъекция](docs/sections/04-sql-injection.md)
- [Insecure Deserialization / Небезопасная десериализация](docs/sections/16-insecure-deserialization.md)
- [Unsafe Reflection / Небезопасное отражение](docs/sections/20-unsafe-reflection.md)

</details>

<details>
<summary><strong>Авторизация и границы доступа / Authorization and Access Boundaries</strong></summary>

Темы про проверку прав на конкретное действие и объект. Аутентификация сама по себе не гарантирует доступ: API должен проверять ownership, роли, tenant boundaries и набор полей, которые клиенту разрешено менять.

- [Broken Access Control / Нарушение контроля доступа](docs/sections/21-broken-access-control.md)
- [JWT / IDOR / Прямой доступ к чужим объектам](docs/sections/11-jwt-idor.md)
- [Mass Assignment / Массовое присваивание](docs/sections/07-mass-assignment.md)

</details>

<details>
<summary><strong>Сеть и файловая система / Network and File Security</strong></summary>

Темы про опасные границы между API и внешней средой: исходящие HTTP-запросы, загрузка файлов, имена файлов и пути на диске.

- [SSRF / Подделка серверных запросов](docs/sections/08-ssrf.md)
- [Dangerous File Upload / Опасная загрузка файлов](docs/sections/09-dangerous-file-upload.md)
- [Path Traversal / Выход за пределы разрешенного каталога](docs/sections/10-path-traversal.md)

</details>

<details>
<summary><strong>Устойчивость и защита ресурсов / Availability and Resource Protection</strong></summary>

Темы про сценарии, где приложение можно перегрузить дорогими запросами, конкурентными обращениями, регулярными выражениями или массовыми повторными вычислениями.

- [DoS / Resource Exhaustion / Исчерпание ресурсов](docs/sections/05-dos-resource-exhaustion.md)
- [Rate Limiting / Ограничение частоты запросов](docs/sections/06-rate-limiting.md)
- [Regex DoS / Отказ в обслуживании через регулярные выражения](docs/sections/19-regex-dos.md)
- [Cache Stampede / Лавина пересчета кеша](docs/sections/18-cache-stampede.md)

</details>

<details>
<summary><strong>Защитное программирование в ASP.NET Core / Defensive Runtime Practices</strong></summary>

Практики реализации, которые не всегда являются отдельной уязвимостью, но критичны для корректного поведения Web API под нагрузкой, при отмене запросов и при работе с внешними ресурсами.

- [CancellationToken / Корректная отмена операций](docs/sections/14-cancellation-token.md)

</details>

<details>
<summary><strong>Конфигурация, секреты и наблюдаемость / Configuration, Secrets and Observability</strong></summary>

Темы про настройки приложения, хранение чувствительных данных и безопасные логи. Ошибки в этом блоке часто не ломают бизнес-логику напрямую, но резко упрощают эксплуатацию других уязвимостей.

- [Security Misconfiguration / Небезопасная конфигурация](docs/sections/22-security-misconfiguration.md)
- [Secrets Management / Управление секретами](docs/sections/17-secrets-management.md)
- [Dangerous Logging / Опасное логирование](docs/sections/12-dangerous-logging.md)

</details>

<details>
<summary><strong>Безопасность зависимостей и цепочки поставки / Dependency and Supply Chain Security</strong></summary>

Темы про NuGet-пакеты, версии, источники зависимостей и контроль уязвимостей в стороннем коде.

- [Dependency Security / Безопасность зависимостей](docs/sections/13-dependency-security.md)

</details>

## Security Note

This repository contains intentionally vulnerable examples for training. Do not copy `unsafe` implementations into production code.
