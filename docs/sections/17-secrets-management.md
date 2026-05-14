# Secrets Management

## Что не так

Секреты в коде и git-истории живут дольше, чем ожидает команда. Даже после исправления значение остаётся в истории, forks, backups и CI artifacts.

## Типовой уязвимый кейс

`appsettings.json`, README или исходники содержат реальные или правдоподобные credentials.

## Payload

```text
grep -R "password=" .
```

## Последствия

- Долгоживущая утечка credential.
- Сложная и дорогая ротация.
- Невозможность точно оценить зону компрометации.

## Как исправлять

- Хранить prod secrets во внешнем vault/provider.
- Для dev использовать user secrets или локальные environment variables.
- Предпочитать managed identity и short-lived credentials.

## Production checklist

- Secret scanning в CI и pre-commit.
- Регулярная ротация и inventory секретов.
- Раздельные scopes для dev/test/prod.
