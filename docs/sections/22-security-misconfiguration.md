# Security Misconfiguration

## Что не так

Security Misconfiguration появляется, когда приложение запускается с небезопасными настройками: подробные ошибки наружу, отсутствующие security headers, открытый debug режим, широкие CORS/CSP policy или лишние management endpoints.

## Типовой уязвимый кейс

API возвращает клиенту environment, stack trace и внутренние настройки, а HTTP-ответ не содержит базовых защитных headers.

## Payload

```text
GET /api/configuration/unsafe-headers
```

## Последствия

- Утечка путей, версий, environment и внутренней структуры приложения.
- Упрощение XSS, clickjacking и MIME sniffing атак.
- Повышенный риск эксплуатации из-за включенных debug или default-настроек.

## Как исправлять

- Не возвращать stack trace и внутренние настройки внешним клиентам.
- Добавлять базовые security headers: `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, CSP.
- Разделять development и production configuration.

## Production checklist

- `ASPNETCORE_ENVIRONMENT=Production` в production.
- Swagger, debug endpoints и подробные ошибки закрыты или защищены.
- CORS, CSP, cookies и headers проходят security review перед релизом.
