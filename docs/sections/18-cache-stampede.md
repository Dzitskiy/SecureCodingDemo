# Cache Stampede

## Что не так

Когда горячий ключ истекает одновременно для множества запросов, все они пытаются одновременно восстановить одно и то же значение из БД или другого медленного источника.

## Типовой уязвимый кейс

Обычный cache-aside без координации: после expiry каждый запрос повторяет expensive rebuild.

## Payload

```text
50 parallel requests right after cache expiry
```

## Последствия

- DB overload.
- Всплески latency.
- Cascading failures на соседних сервисах и зависимостях.

## Как исправлять

- Single-flight locking на rebuild cache entry.
- Shared cache вроде Redis для reuse между инстансами.
- Stale-while-revalidate и jitter на TTL.

## Production checklist

- Warmup для hot keys.
- Metrics по cache miss burst.
- Тестирование на synchronized expiry scenario.
