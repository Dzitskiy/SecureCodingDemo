using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using SecureCodingDemo.Infrastructure;

namespace SecureCodingDemo.Modules;

public static class DemoPlaygroundEndpoints
{
    public static IEndpointRouteBuilder MapDemoPlaygroundEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/demos", (DemoScenarioCatalog catalog) =>
            Results.Ok(catalog.GetScenarios()))
            .WithTags("00. Catalog")
            .WithSummary("Return all demo scenarios with ready-made test requests");

        app.MapGet("/api/demos/{slug}", (string slug, DemoScenarioCatalog catalog) =>
        {
            var scenario = catalog.GetScenario(slug);
            return scenario is null ? Results.NotFound() : Results.Ok(scenario);
        })
        .WithTags("00. Catalog")
        .WithSummary("Return one demo scenario with ready-made unsafe and safe requests");

        app.MapGet("/demo", () => Results.Text(BuildHtmlPage(), "text/html"))
            .ExcludeFromDescription();

        return app;
    }

    private static string BuildHtmlPage()
    {
        const string html = """
<!DOCTYPE html>
<html lang="ru">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>Secure Coding Demo Playground</title>
  <style>
    :root {
      --bg: #f3efe6;
      --panel: rgba(255,255,255,0.82);
      --panel-strong: rgba(255,255,255,0.94);
      --text: #1f2430;
      --muted: #556070;
      --line: rgba(31,36,48,0.12);
      --unsafe: #b6422e;
      --safe: #1f7a52;
      --accent: #004e64;
      --shadow: 0 18px 50px rgba(31,36,48,0.12);
      --radius: 22px;
      --code: #11212d;
    }

    * { box-sizing: border-box; }
    body {
      margin: 0;
      font-family: "Segoe UI", "Trebuchet MS", sans-serif;
      color: var(--text);
      background:
        radial-gradient(circle at top left, rgba(0,78,100,0.18), transparent 30%),
        radial-gradient(circle at top right, rgba(182,66,46,0.16), transparent 28%),
        linear-gradient(180deg, #f7f4ec 0%, #eee7d7 100%);
    }

    a { color: var(--accent); }

    .layout {
      display: grid;
      grid-template-columns: 320px 1fr;
      gap: 24px;
      min-height: 100vh;
      padding: 24px;
    }

    .sidebar,
    .main {
      min-width: 0;
    }

    .panel {
      background: var(--panel);
      border: 1px solid var(--line);
      box-shadow: var(--shadow);
      border-radius: var(--radius);
      backdrop-filter: blur(16px);
    }

    .hero {
      padding: 24px;
      margin-bottom: 20px;
      background:
        linear-gradient(135deg, rgba(0,78,100,0.92), rgba(9,114,117,0.82)),
        linear-gradient(180deg, rgba(255,255,255,0.08), rgba(255,255,255,0));
      color: white;
    }

    .hero h1,
    .detail h2,
    .variant h3 {
      margin: 0;
      font-family: Georgia, "Times New Roman", serif;
      letter-spacing: 0.01em;
    }

    .hero p {
      margin: 12px 0 0;
      color: rgba(255,255,255,0.9);
      line-height: 1.5;
    }

    .sidebar-list {
      padding: 12px;
      max-height: calc(100vh - 220px);
      overflow: auto;
    }

    .scenario-button {
      width: 100%;
      text-align: left;
      padding: 14px 16px;
      border-radius: 18px;
      border: 1px solid transparent;
      background: transparent;
      cursor: pointer;
      transition: transform 180ms ease, background 180ms ease, border-color 180ms ease;
      margin-bottom: 10px;
    }

    .scenario-button:hover,
    .scenario-button.active {
      background: var(--panel-strong);
      border-color: rgba(0,78,100,0.2);
      transform: translateY(-1px);
    }

    .scenario-title {
      font-weight: 700;
      display: block;
      margin-bottom: 4px;
    }

    .scenario-meta,
    .muted {
      color: var(--muted);
      font-size: 0.92rem;
      line-height: 1.4;
    }

    .detail {
      padding: 24px;
      margin-bottom: 20px;
    }

    .detail-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 20px;
      margin-top: 18px;
    }

    .badge {
      display: inline-flex;
      align-items: center;
      gap: 8px;
      padding: 6px 12px;
      border-radius: 999px;
      background: rgba(0,78,100,0.08);
      color: var(--accent);
      font-size: 0.9rem;
      font-weight: 700;
      margin-bottom: 12px;
    }

    .variants {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 20px;
    }

    .variant {
      padding: 22px;
    }

    .variant.unsafe { border-top: 5px solid rgba(182,66,46,0.9); }
    .variant.safe { border-top: 5px solid rgba(31,122,82,0.9); }

    .variant-label {
      display: inline-block;
      padding: 5px 10px;
      border-radius: 999px;
      font-size: 0.82rem;
      font-weight: 700;
      margin-bottom: 12px;
      color: white;
    }

    .variant.unsafe .variant-label { background: var(--unsafe); }
    .variant.safe .variant-label { background: var(--safe); }

    .field {
      margin-bottom: 14px;
    }

    .field label {
      display: block;
      font-size: 0.82rem;
      text-transform: uppercase;
      letter-spacing: 0.06em;
      color: var(--muted);
      margin-bottom: 6px;
    }

    input[type="text"],
    textarea,
    select {
      width: 100%;
      border: 1px solid rgba(17,33,45,0.12);
      border-radius: 14px;
      padding: 12px 14px;
      font: inherit;
      color: var(--code);
      background: rgba(255,255,255,0.88);
    }

    textarea {
      min-height: 140px;
      resize: vertical;
      font-family: Consolas, "Courier New", monospace;
      font-size: 0.92rem;
      line-height: 1.45;
    }

    .button-row {
      display: flex;
      flex-wrap: wrap;
      gap: 10px;
      margin: 16px 0;
    }

    button.action,
    a.action-link {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      padding: 12px 16px;
      border: none;
      border-radius: 14px;
      text-decoration: none;
      font: inherit;
      font-weight: 700;
      cursor: pointer;
      transition: transform 180ms ease, opacity 180ms ease;
    }

    button.action:hover,
    a.action-link:hover { transform: translateY(-1px); }

    .unsafe .primary { background: rgba(182,66,46,0.95); color: white; }
    .safe .primary { background: rgba(31,122,82,0.95); color: white; }
    .secondary { background: rgba(0,78,100,0.1); color: var(--accent); }

    .output {
      background: #13212c;
      color: #eaf5ff;
      border-radius: 18px;
      padding: 16px;
      overflow: hidden;
    }

    .output pre {
      white-space: pre-wrap;
      word-break: break-word;
      font-family: Consolas, "Courier New", monospace;
      font-size: 0.86rem;
      margin: 0;
      max-height: 320px;
      overflow: auto;
    }

    .output iframe {
      width: 100%;
      min-height: 220px;
      border: 1px solid rgba(255,255,255,0.12);
      border-radius: 12px;
      background: white;
      margin-top: 14px;
    }

    .checkpoints,
    .notes {
      margin: 12px 0 0;
      padding-left: 18px;
      line-height: 1.5;
    }

    .empty-state {
      padding: 32px;
      text-align: center;
      color: var(--muted);
    }

    @media (max-width: 1100px) {
      .layout,
      .variants,
      .detail-grid {
        grid-template-columns: 1fr;
      }

      .sidebar-list {
        max-height: none;
      }
    }
  </style>
</head>
<body>
  <div class="layout">
    <aside class="sidebar">
      <section class="hero panel">
        <h1>Secure Coding Demo</h1>
        <p>Выбери кейс из документации, сравни `unsafe` и `safe` вариант, запусти sample request и сразу посмотри разницу в ответе, headers и поведении.</p>
      </section>
      <section class="panel sidebar-list" id="scenario-list">
        <div class="empty-state">Загружаю список сценариев…</div>
      </section>
    </aside>

    <main class="main">
      <section class="panel detail" id="detail-panel">
        <div class="empty-state">Открываю первый сценарий…</div>
      </section>
      <section class="variants" id="variants-panel"></section>
    </main>
  </div>

  <script>
    const state = { scenarios: [], selected: null };

    async function fetchJson(url, options) {
      const response = await fetch(url, options);
      if (!response.ok) {
        const body = await response.text();
        throw new Error(`${response.status} ${response.statusText}\n${body}`);
      }
      return await response.json();
    }

    function escapeHtml(value) {
      return value
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;');
    }

    function formatCurl(spec) {
      const url = buildUrl(spec.path, spec.query);
      const parts = [`curl -i -X ${spec.method} "${url}"`];
      if (spec.auth) {
        parts.push('-H "Authorization: Bearer <demo-token>"');
      }
      if (spec.kind === 'json') {
        parts.push('-H "Content-Type: application/json"');
        parts.push(`--data-raw '${spec.body ?? ""}'`);
      } else if (spec.kind === 'text') {
        parts.push(`-H "Content-Type: ${spec.contentType || "text/plain"}"`);
        parts.push(`--data-raw '${spec.body ?? ""}'`);
      } else if (spec.kind === 'multipart') {
        parts.push(`-F "file=@${spec.fileName || "sample.txt"}"`);
      }
      return parts.join(' ');
    }

    function buildUrl(path, query) {
      if (!query) {
        return path;
      }
      return `${path}?${query}`;
    }

    async function getToken(auth) {
      const result = await fetchJson(auth.tokenPath, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          userName: auth.userName,
          password: auth.password
        })
      });
      return result.accessToken;
    }

    function renderList() {
      const container = document.getElementById('scenario-list');
      container.innerHTML = state.scenarios.map((scenario) => `
        <button class="scenario-button ${state.selected && state.selected.slug === scenario.slug ? 'active' : ''}" data-slug="${scenario.slug}">
          <span class="scenario-title">${escapeHtml(scenario.title)}</span>
          <div class="scenario-meta">${escapeHtml(scenario.category)}</div>
          <div class="scenario-meta">${escapeHtml(scenario.summary)}</div>
        </button>
      `).join('');

      for (const button of container.querySelectorAll('[data-slug]')) {
        button.addEventListener('click', () => loadScenario(button.dataset.slug));
      }
    }

    function renderDetail() {
      const detail = document.getElementById('detail-panel');
      if (!state.selected) {
        detail.innerHTML = '<div class="empty-state">Сценарий не найден.</div>';
        return;
      }

      detail.innerHTML = `
        <div class="badge">${escapeHtml(state.selected.category)}</div>
        <h2>${escapeHtml(state.selected.title)}</h2>
        <p>${escapeHtml(state.selected.summary)}</p>
        <div class="detail-grid">
          <div>
            <strong>Документация</strong>
            <p class="muted"><a href="${state.selected.documentationUrl}" target="_blank" rel="noreferrer">Открыть Markdown раздел</a></p>
          </div>
          <div>
            <strong>Идея проверки</strong>
            <ul class="notes">
              ${(state.selected.notes || []).map((note) => `<li>${escapeHtml(note)}</li>`).join('') || '<li>Сравни статус, body и headers между unsafe и safe вариантом.</li>'}
            </ul>
          </div>
        </div>
      `;
    }

    function variantTemplate(kind, variant) {
      const request = variant.request;
      const hasBody = request.kind === 'json' || request.kind === 'text';
      const hasQuery = request.kind === 'query';
      const isMultipart = request.kind === 'multipart';
      const canOpen = request.method === 'GET' && !request.auth;
      const curl = formatCurl(request);

      return `
        <article class="panel variant ${kind}">
          <span class="variant-label">${escapeHtml(variant.label)}</span>
          <h3>${escapeHtml(variant.description)}</h3>
          <div class="field">
            <label>Endpoint</label>
            <input type="text" id="${kind}-path" value="${escapeHtml(request.path)}">
          </div>
          <div class="field">
            <label>Method</label>
            <input type="text" id="${kind}-method" value="${escapeHtml(request.method)}">
          </div>
          ${hasQuery ? `
          <div class="field">
            <label>Query</label>
            <input type="text" id="${kind}-query" value="${escapeHtml(request.query || '')}">
          </div>` : ''}
          ${hasBody ? `
          <div class="field">
            <label>Body</label>
            <textarea id="${kind}-body">${escapeHtml(request.body || '')}</textarea>
          </div>` : ''}
          ${isMultipart ? `
          <div class="field">
            <label>Имя файла</label>
            <input type="text" id="${kind}-file-name" value="${escapeHtml(request.fileName || 'sample.txt')}">
          </div>
          <div class="field">
            <label>Содержимое файла</label>
            <textarea id="${kind}-file-content">${escapeHtml(request.fileContent || '')}</textarea>
          </div>
          <div class="field">
            <label>Или выбрать реальный файл</label>
            <input type="file" id="${kind}-file-upload">
          </div>` : ''}
          <div class="field">
            <label>Sample curl</label>
            <textarea readonly>${escapeHtml(curl)}</textarea>
          </div>
          <div class="button-row">
            <button class="action primary" data-run="${kind}">Run ${escapeHtml(variant.label)}</button>
            ${canOpen ? `<a class="action-link secondary" href="${buildUrl(request.path, request.query)}" target="_blank" rel="noreferrer">Open actual route</a>` : ''}
          </div>
          ${request.auth ? `<p class="muted">JWT будет получен автоматически для ${escapeHtml(request.auth.userName)}.</p>` : ''}
          <ul class="checkpoints">
            ${variant.checkpoints.map((item) => `<li>${escapeHtml(item)}</li>`).join('')}
          </ul>
          <div class="output" id="${kind}-output">
            <pre>Пока запрос не запускался.</pre>
          </div>
        </article>
      `;
    }

    function renderVariants() {
      const container = document.getElementById('variants-panel');
      if (!state.selected) {
        container.innerHTML = '';
        return;
      }

      container.innerHTML = [
        variantTemplate('unsafe', state.selected.unsafe),
        variantTemplate('safe', state.selected.safe)
      ].join('');

      for (const button of container.querySelectorAll('[data-run]')) {
        button.addEventListener('click', () => runVariant(button.dataset.run));
      }
    }

    function readVariant(kind) {
      const variant = state.selected[kind];
      const request = structuredClone(variant.request);
      request.path = document.getElementById(`${kind}-path`).value.trim();
      request.method = document.getElementById(`${kind}-method`).value.trim().toUpperCase();
      if (request.kind === 'query') {
        request.query = document.getElementById(`${kind}-query`).value.trim();
      }
      if (request.kind === 'json' || request.kind === 'text') {
        request.body = document.getElementById(`${kind}-body`).value;
      }
      if (request.kind === 'multipart') {
        request.fileName = document.getElementById(`${kind}-file-name`).value.trim();
        request.fileContent = document.getElementById(`${kind}-file-content`).value;
      }
      return request;
    }

    async function runVariant(kind) {
      const output = document.getElementById(`${kind}-output`);
      output.innerHTML = '<pre>Выполняю запрос…</pre>';

      try {
        const request = readVariant(kind);
        const headers = {};
        if (request.auth) {
          const token = await getToken(request.auth);
          headers['Authorization'] = `Bearer ${token}`;
        }

        const options = { method: request.method, headers };
        if (request.kind === 'json') {
          headers['Content-Type'] = request.contentType || 'application/json';
          options.body = request.body || '{}';
        } else if (request.kind === 'text') {
          headers['Content-Type'] = request.contentType || 'text/plain';
          options.body = request.body || '';
        } else if (request.kind === 'multipart') {
          const form = new FormData();
          const fileInput = document.getElementById(`${kind}-file-upload`);
          if (fileInput.files && fileInput.files.length > 0) {
            form.append('file', fileInput.files[0], fileInput.files[0].name);
          } else {
            const blob = new Blob([request.fileContent || 'demo file'], { type: 'text/plain' });
            form.append('file', blob, request.fileName || 'sample.txt');
          }
          options.body = form;
        }

        const url = buildUrl(request.path, request.query);
        const response = await fetch(url, options);
        const contentType = response.headers.get('content-type') || '';
        const text = await response.text();
        const headerLines = Array.from(response.headers.entries())
          .map(([name, value]) => `${name}: ${value}`)
          .join('\n');

        let pretty = text;
        if (contentType.includes('application/json')) {
          try {
            pretty = JSON.stringify(JSON.parse(text), null, 2);
          } catch {
          }
        }

        const htmlPreview = contentType.includes('text/html')
          ? `<iframe sandbox="allow-scripts allow-same-origin" srcdoc="${escapeHtml(text)}"></iframe>`
          : '';

        output.innerHTML = `
          <pre>Status: ${response.status} ${response.statusText}

Headers:
${escapeHtml(headerLines || '(no interesting headers)')}

Body:
${escapeHtml(pretty || '(empty body)')}</pre>
          ${htmlPreview}
        `;
      } catch (error) {
        output.innerHTML = `<pre>${escapeHtml(error.message || String(error))}</pre>`;
      }
    }

    async function loadScenario(slug) {
      state.selected = await fetchJson(`/api/demos/${slug}`);
      renderList();
      renderDetail();
      renderVariants();
      history.replaceState({}, '', `#${slug}`);
    }

    async function bootstrap() {
      try {
        state.scenarios = await fetchJson('/api/demos');
        const initialSlug = window.location.hash.replace('#', '') || state.scenarios[0]?.slug;
        renderList();
        if (initialSlug) {
          await loadScenario(initialSlug);
        }
      } catch (error) {
        document.getElementById('detail-panel').innerHTML = `<div class="empty-state">${escapeHtml(error.message || String(error))}</div>`;
      }
    }

    bootstrap();
  </script>
</body>
</html>
""";

        return html;
    }
}
