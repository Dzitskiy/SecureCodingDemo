# Path Traversal

## Что не так

`Path.Combine` не делает путь безопасным. Если пользователь контролирует относительный путь, он может выйти из допустимого каталога через `../` и его варианты.

## Типовой уязвимый кейс

API читает файл из `public`, но принимает `../private/secrets.txt`.

## Payload

```text
../private/secrets.txt
```

## Последствия

- Arbitrary file read.
- Утечка secrets, config и private keys.
- Дальнейшая компрометация по найденным credentials.

## Как исправлять

- Получать canonical base path.
- Нормализовать итоговый target.
- Проверять, что final path начинается с trusted root.

## Production checklist

- Предпочитать document IDs вместо raw path input.
- Покрывать тестами encoded и mixed-separator traversal.
- Убрать прямой filesystem access из публичных API там, где возможно.
