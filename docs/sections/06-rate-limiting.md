# Rate Limiting

## Что не так

Без ограничений login и expensive endpoints становятся удобной целью для brute force и flood-атак. Даже неуспешные запросы тратят CPU, DB connections и log volume.

## Типовой уязвимый кейс

Логин принимает бесконечное число попыток с одного IP или для одного пользователя без замедления и lockout.

## Payload

```json
{"username":"admin","password":"guess"}
```

## Последствия

- Credential stuffing.
- Account takeover pressure.
- Деградация API под burst-нагрузкой.

## Как исправлять

- Применять fixed-window, token-bucket или sliding-window limiter.
- Ограничивать не только по IP, но и по identity/client key.
- Комбинировать с MFA, bot detection и telemetry.

## Production checklist

- Отдельные лимиты для login, search и expensive reports.
- Alerting на рост 429 и аномальные patterns.
- Репутационные блокировки и progressive delays.
