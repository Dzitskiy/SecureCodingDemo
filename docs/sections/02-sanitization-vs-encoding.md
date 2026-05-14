# Sanitization vs Encoding

## Что не так

Encoding и sanitization решают разные задачи. Encoding нужен почти всегда при выводе untrusted data. Sanitization нужен только тогда, когда приложение сознательно разрешает ограниченный HTML.

## Типовой уязвимый кейс

Разработчик удаляет только `<script>` через regex и считает HTML безопасным, хотя payload может использовать SVG, `javascript:` URL или inline handlers.

## Payload

```html
<svg><a xlink:href="javascript:alert(1)">click</a></svg>
```

## Последствия

- Stored XSS при сохранении якобы очищенного HTML.
- Блоки интерфейса рендерят активный контент вместо текста.
- Самодельный sanitizer быстро отстаёт от реальных payload-техник.

## Как исправлять

- Plain text всегда кодировать.
- Rich text очищать только зрелой allowlist-библиотекой.
- Не хранить в голове, что sanitization заменяет contextual output encoding.

## Production checklist

- Единая policy для rich text.
- Отдельные тесты для SVG, `data:` и inline handlers.
- Документированный список разрешённых тегов и атрибутов.
