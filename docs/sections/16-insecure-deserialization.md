# Insecure Deserialization

## Что не так

Если клиент выбирает concrete type для десериализации, он влияет на то, какие объекты создаются внутри приложения и какой код потом сработает в pipeline.

## Типовой уязвимый кейс

API принимает `type` и `payload`, после чего пытается создать произвольный .NET type по имени.

## Payload

```json
{"type":"SecureCodingDemo.Modules.GrantAdminAction","payload":{"UserId":"1"}}
```

## Последствия

- Обход бизнес-правил через неожиданные типы-команды.
- Gadget abuse и сайд-эффекты конструкторов/сеттеров.
- Повышение привилегий и broken workflow validation.

## Как исправлять

- Не принимать type name от клиента.
- Десериализовать только в известные DTO.
- Если полиморфизм действительно нужен, вводить короткий allowlist discriminator values.

## Production checklist

- Schema validation на входе.
- Review serializer options в shared libraries.
- Отдельные тесты на unsafe polymorphism.
