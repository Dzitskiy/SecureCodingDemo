# Dangerous Logging

## Что не так

Логи часто живут дольше и доступны шире, чем primary data. Поэтому секреты в логах создают отдельный канал утечки.

## Типовой уязвимый кейс

Приложение пишет в structured log пароль, bearer token или session id целиком.

## Payload

```json
{"username":"alice","password":"P@ssw0rd!","token":"eyJ..."}
```

## Последствия

- Утечка секретов через SIEM, file shares и backups.
- Невозможность быстро удалить секрет из исторических логов.
- Secondary breach через поддержку и админов логов.

## Как исправлять

- Логировать только безопасные признаки: presence, length, hash, correlation ID.
- Маскировать чувствительные поля на уровне sink/processors.
- Ограничивать доступ и retention.

## Production checklist

- Logging policy и data classification.
- Периодическая ревизия log templates.
- Тесты на redaction для auth и payment flows.
