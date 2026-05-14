# Regex DoS

## Что не так

Некоторые regex-паттерны имеют catastrophic backtracking и на специальной строке съедают CPU несоразмерно входным данным.

## Типовой уязвимый кейс

Приложение использует сложную регулярку без timeout и без ограничения длины input.

## Payload

```text
aaaaaaaaaaaaaaaaaaaaaaaa!
```

## Последствия

- CPU spikes.
- Request starvation.
- Эффект дешёвой атаки даже при низком объёме трафика.

## Как исправлять

- Избегать nested quantifiers и двусмысленных шаблонов.
- Ограничивать длину input.
- Использовать timeout и при необходимости заменять regex на parser.

## Production checklist

- Fuzz worst-case inputs.
- Review regex complexity в code review.
- Отдельные perf tests для validation-heavy endpoints.
