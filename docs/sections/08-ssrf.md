# SSRF

## Что не так

SSRF позволяет использовать сервер как доверенный сетевой прокси. Через него атакующий получает доступ к внутренним сервисам и cloud metadata endpoints.

## Типовой уязвимый кейс

API принимает произвольный URL и без дополнительной проверки скачивает его от имени сервера.

## Payload

```text
http://169.254.169.254/latest/meta-data/
```

## Последствия

- Доступ к внутренним admin panels и сервисам.
- Утечка cloud credentials и instance metadata.
- Pivoting внутрь private network.

## Как исправлять

- Разрешать только allowlist host names и HTTPS.
- После DNS resolution блокировать loopback и private IP ranges.
- Ограничивать redirects, response size и timeout.

## Production checklist

- Egress firewall или outbound proxy.
- Метаданные облака закрыты IMDSv2 / policy controls.
- Threat monitoring по suspicious outbound destinations.
