# CancellationToken

## Что не так

Если backend игнорирует отмену запроса, он продолжает занимать CPU, сокеты и DB connections даже после disconnect клиента или gateway timeout.

## Типовой уязвимый кейс

Долгий endpoint делает `Task.Delay`, DB calls и внешние HTTP requests без передачи `CancellationToken`.

## Payload

```text
/api/cancellation/unsafe?seconds=120
```

## Последствия

- Накопление бесполезной работы.
- Thread pool и connection pool exhaustion.
- Рост latency для нормальных запросов.

## Как исправлять

- Передавать `CancellationToken` во все async API.
- Создавать linked token с серверным timeout budget.
- Проверять cancellation внутри собственных циклов.

## Production checklist

- Time budget на каждый тип операции.
- Metrics по canceled vs completed requests.
- Review всех background-style loops внутри request pipeline.
