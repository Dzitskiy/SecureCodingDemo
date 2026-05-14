# Whitelist Validation

## Что не так

Слабая валидация проверяет только наличие допустимого фрагмента, а не весь формат значения. Поэтому `../../secret.txt` легко проходит проверки типа `Contains(".txt")`.

## Типовой уязвимый кейс

API принимает имя файла, но на деле допускает path separators, URL-encoded traversal и служебные суффиксы.

## Payload

```text
..\..\private\secrets.txt
```

## Последствия

- Path traversal и чтение файлов вне допустимого каталога.
- Передача вредоносных значений в filesystem, shell или downstream services.

## Как исправлять

- Описывать допустимый формат через anchored regex.
- Нормализовать путь после валидации.
- Проверять, что итоговый target остаётся внутри trusted root.

## Production checklist

- Не смешивать имя файла и путь в одном поле.
- Логировать отклонённые значения как threat telemetry.
- Покрывать тестами encoded traversal payloads.
