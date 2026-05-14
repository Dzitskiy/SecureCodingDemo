# Dependency Security

## Что не так

Supply chain риск появляется, когда сборка зависит от непроверенных feeds, floating versions и уязвимых transitive packages.

## Типовой уязвимый кейс

Разработчик подключает пакет с `latest` или с частного неаудированного source, не проверяя provenance и advisories.

## Payload

```json
{"packageName":"demo.tool","version":"latest","source":"https://evil.example/nuget"}
```

## Последствия

- Typosquatting.
- Попадание malicious package в build pipeline.
- Уязвимые transitive dependencies в production.

## Как исправлять

- Фиксировать точные версии.
- Ограничивать package sources allowlist-списком.
- Запускать SCA и advisory scans в CI.

## Production checklist

- SBOM generation.
- Signed package/provenance checks.
- Политика обновлений и triage уязвимостей по severity/SLA.
