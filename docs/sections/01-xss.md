# XSS

## Что не так

XSS возникает, когда приложение вставляет пользовательский ввод в HTML без контекстного encoding. Браузер воспринимает строку как разметку и выполняет активное содержимое в origin приложения.

## Типовой уязвимый кейс

Приложение рендерит комментарий как `<div>{input}</div>`, не кодируя `<script>`, event handlers и другие HTML-конструкции.

## Payload

```text
<img src=x onerror=alert(document.domain)>
```

## Последствия

- Кража cookie и bearer token из браузерного контекста.
- Выполнение действий от имени пользователя.
- Подмена интерфейса и phishing внутри доверенного origin.

## Как исправлять

- Для обычного текста всегда применять HTML encoding.
- Для rich text использовать allowlist-based sanitization.
- Добавлять CSP как дополнительный барьер, а не как замену исправлению.

## Production checklist

- `HttpOnly`, `SameSite`, `Secure` для cookie.
- Review всех мест, где формируется HTML.
- Автотесты на dangerous payloads и stored XSS сценарии.
