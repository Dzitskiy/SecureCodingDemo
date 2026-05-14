# JWT / IDOR

## Что не так

JWT подтверждает identity, но не выдаёт автоматически право на любой объект. Если API возвращает запись только по `id`, появляется IDOR.

## Типовой уязвимый кейс

Пользователь с токеном для `userId=1` запрашивает заказ другого пользователя по известному ID.

## Payload

```text
GET /api/orders/unsafe/103
```

## Последствия

- Horizontal privilege escalation.
- Утечка персональных и бизнес-данных.
- Использование устаревших ролей из старых access token.

## Как исправлять

- Проверять ownership на каждый объект.
- Критичные claims перепроверять по актуальному server-side source of truth.
- Делать access tokens короткоживущими.

## Production checklist

- Policy-based authorization на уровне business operation.
- Revocation strategy после role change.
- Audit для object access failures и enumeration patterns.
