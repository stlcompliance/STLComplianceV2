import { createHash } from 'node:crypto'
import { readFileSync, writeFileSync } from 'node:fs'
import { pathToFileURL } from 'node:url'

export function buildSecurityHeaders(html, trustedUrls = []) {
  const origins = [...new Set(trustedUrls.filter((value) => typeof value === 'string' && /^https?:\/\//.test(value))
    .map((value) => new URL(value).origin))]
  const normalized = html.replace(/\r\n?/g, '\n')
  const hashes = (tag) => [...normalized.matchAll(new RegExp(`<${tag}\\b([^>]*)>([\\s\\S]*?)<\\/${tag}>`, 'gi'))]
    .filter((match) => !/\bsrc\s*=/i.test(match[1]) && match[2].trim())
    .map((match) => `'sha256-${createHash('sha256').update(match[2]).digest('base64')}'`)
    .join(' ')
  const policy = [
    "default-src 'self'",
    "base-uri 'self'",
    "object-src 'none'",
    "frame-ancestors 'none'",
    "form-action 'self'",
    "img-src 'self' data: blob:",
    `script-src 'self' ${hashes('script')}`.trim(),
    `style-src 'self' ${hashes('style')}`.trim(),
    "style-src-attr 'unsafe-inline'",
    `connect-src 'self' ${origins.join(' ')}`.trim(),
    "font-src 'self' data:",
    "media-src 'self' blob: data:",
    "frame-src 'self' blob:",
    "worker-src 'self' blob:",
  ].join('; ')
  return `add_header Content-Security-Policy "${policy}" always;\n`
}

if (process.argv[1] && import.meta.url === pathToFileURL(process.argv[1]).href) {
  const publicSettings = JSON.parse(process.env.FRONTEND_ENV_JSON ?? '{}')
  writeFileSync(process.argv[3], buildSecurityHeaders(readFileSync(process.argv[2], 'utf8'), Object.values(publicSettings)))
}
