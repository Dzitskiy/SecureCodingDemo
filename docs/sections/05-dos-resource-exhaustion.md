# DoS / Resource Exhaustion

## Что не так

Переполнение памяти и thread starvation чаще всего появляются из-за полного buffering тела запроса, долгих синхронных операций и игнорирования cancellation.

## Типовой уязвимый кейс

API читает huge JSON целиком в строку, затем делает блокирующую обработку на worker thread.

## Payload

```json
{"items":["AAAA.... repeated millions of times"]}
```

## Последствия

- Рост памяти и нагрузка на GC.
- Thread pool starvation и скачок latency.
- Перезапуски процесса или деградация соседних endpoints.

## Как исправлять

- Лимитировать размер request body.
- Использовать streaming deserialize и chunked processing.
- Прекращать работу по `CancellationToken`.

## Production checklist

- Request quotas на ingress.
- Timeouts и backpressure для downstream calls.
- Separate capacity tests на oversized input.
