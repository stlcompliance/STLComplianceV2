import assert from 'node:assert/strict'
import { createHash } from 'node:crypto'
import { test } from 'node:test'
import { buildSecurityHeaders } from './frontend-security.mjs'

test('authorizes the exact inline theme script without authorizing arbitrary scripts', () => {
  const script = '\nwindow.theme = "dark";\n'
  const hash = createHash('sha256').update(script).digest('base64')
  const header = buildSecurityHeaders(`<script>${script}</script><script src="/app.js"></script>`)
  assert.ok(header.includes(`script-src 'self' 'sha256-${hash}'`))
  assert.ok(!/script-src[^;]*unsafe/.test(header))
  assert.ok(header.includes("frame-ancestors 'none'"))
})

test('hashes normalized browser newlines and preserves script whitespace', () => {
  assert.equal(buildSecurityHeaders('<script>\r\nrun();\r\n</script>'),
    buildSecurityHeaders('<script>\nrun();\n</script>'))
  assert.notEqual(buildSecurityHeaders('<script>run();</script>'),
    buildSecurityHeaders('<script> run(); </script>'))
})

test('restricts style elements while supporting React style attributes', () => {
  const header = buildSecurityHeaders('<style>body { color: white; }</style>')
  assert.match(header, /style-src 'self' 'sha256-[^']+'/)
  assert.ok(header.includes("style-src-attr 'unsafe-inline'"))
})

test('allows explicit suite session origins without wildcard connection trust', () => {
  const header = buildSecurityHeaders('', ['https://suite.example.com/login', 'https://suite.example.com', '', 'data:text/plain,hello'])
  assert.ok(header.includes("connect-src 'self' https://suite.example.com;"))
  assert.ok(!header.includes('data:text/plain'))
})
