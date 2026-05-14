# CSP

## Что не так

CSP не лечит XSS сама по себе, но уменьшает impact. Если policy содержит `unsafe-inline`, `unsafe-eval` и широкие внешние источники, защитный слой практически исчезает.

## Типовой уязвимый кейс

Frontend требует inline scripts, поэтому команда разрешает всё подряд, не ограничивая script sources.

## Payload

```html
<script>alert('inline')</script>
```

## Последствия

- Любая XSS становится заметно опаснее.
- Растёт доверенная зона вокруг third-party scripts.
- Сложнее понять, какие внешние источники действительно нужны.

## Как исправлять

- Строить policy от `default-src 'self'`.
- Использовать nonce/hash вместо `unsafe-inline`.
- Вводить CSP через Report-Only и инвентаризацию зависимостей.

## Production checklist

- `object-src 'none'`, `frame-ancestors 'none'`.
- Review всех CDN и third-party scripts.
- Monitoring по CSP violation reports.
