# Unsafe Reflection

## Что не так

Reflection становится опасным, когда untrusted input управляет именем метода, типа или члена assembly. Тогда клиент получает доступ к внутренним действиям, которых не было в публичном контракте API.

## Типовой уязвимый кейс

API принимает `methodName` и через reflection вызывает любой public method service-класса.

## Payload

```json
{"methodName":"DropUserSessions"}
```

## Последствия

- Вызов unintended code paths.
- Обход compile-time API contract.
- Рост attack surface без явного изменения публичного API.

## Как исправлять

- Заменить reflection dispatch на allowlist handlers или command map.
- Разделять internal methods и public API actions.
- Ограничивать reflection конфигурацией и авторизацией, если без неё нельзя.

## Production checklist

- Review мест с `GetMethod`, `Invoke`, `Activator`.
- Запрет reflection по клиентским строкам в coding standards.
- Тесты на попытки вызвать non-approved operations.
