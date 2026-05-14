# Mass Assignment

## Что не так

Если внешний JSON напрямую мапится на domain/entity модель, клиент получает доступ к серверным полям вроде `Role`, `IsAdmin` или `Balance`.

## Типовой уязвимый кейс

Public DTO содержит чувствительные поля, которые UI обычно не показывает, но атакующий отправляет их вручную.

## Payload

```json
{"displayName":"Mallory","department":"IT","isAdmin":true,"role":"Admin"}
```

## Последствия

- Privilege escalation.
- Нарушение business invariants.
- Незаметные серверные state changes.

## Как исправлять

- Делать отдельные input DTO для каждой операции.
- Серверно присваивать privileged fields после авторизации.
- Проверять ownership и допустимость изменения до сохранения.

## Production checklist

- Запрет auto-bind на internal entities.
- Code review на публичные write-модели.
- Контрактные тесты на попытки подмены запрещённых полей.
