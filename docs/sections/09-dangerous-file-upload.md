# Dangerous File Upload

## Что не так

Upload уязвим, когда приложение доверяет client MIME type, исходному имени файла и хранит содержимое так, будто оно уже безопасно.

## Типовой уязвимый кейс

Сервер принимает `invoice.pdf`, хотя внутри лежит script, polyglot file или исполняемый контент.

## Payload

```text
shell.aspx renamed to invoice.pdf
```

## Последствия

- Remote code execution при небезопасной выдаче файла.
- Malware hosting.
- Stored XSS и overwrite существующих файлов.

## Как исправлять

- Проверять extension allowlist и magic bytes.
- Генерировать серверное имя файла.
- Хранить upload вне web root и отдавать через download handler.

## Production checklist

- Antivirus/content disarm pipeline.
- Ограничения по размеру и типу.
- Метаданные файла и scan result сохраняются отдельно.
