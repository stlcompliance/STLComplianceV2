import { existsSync, mkdirSync, readdirSync, readFileSync, writeFileSync } from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

import react from '@vitejs/plugin-react'
import { defineConfig, type Plugin } from 'vitest/config'

const appRoot = path.dirname(fileURLToPath(import.meta.url))
const repoRoot = path.resolve(appRoot, '../..')
const docsRoot = path.join(repoRoot, 'docs', 'user')
const platformAdminPattern =
  /\bplatform[-_\s]+admin(?:s|istrator(?:s)?|istration|istrative)?\b|\bplatform_admin\b/i
const restrictedComplianceCoreLinePattern =
  /Compliance Core|compliancecore\.|compliance-core\/|compliance-core-user-guide|rule-import-to-evaluation/i
const virtualKbDocsModuleId = 'virtual:kb-docs'
const resolvedVirtualKbDocsModuleId = `\0${virtualKbDocsModuleId}`

type RawKbDoc = {
  relativePath: string
  markdown: string
}

function toSlug(relativePath: string): string {
  const withoutExtension = relativePath.replace(/\.md$/i, '').replaceAll(path.sep, '/')
  if (withoutExtension === 'index') {
    return 'overview'
  }

  return withoutExtension.replace(/\/index$/i, '').replace(/\//g, '--')
}

function collectMarkdownFiles(directory: string): string[] {
  if (!existsSync(directory)) {
    return []
  }

  return readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const fullPath = path.join(directory, entry.name)
    if (entry.isDirectory()) {
      return collectMarkdownFiles(fullPath)
    }

    return entry.isFile() && entry.name.endsWith('.md') ? [fullPath] : []
  })
}

function isKbMarkdownFile(filePath: string): boolean {
  const normalizedFile = path.normalize(filePath)
  const normalizedDocsRoot = path.normalize(docsRoot)
  return normalizedFile.startsWith(`${normalizedDocsRoot}${path.sep}`) && normalizedFile.endsWith('.md')
}

function collectKbPaths(): string[] {
  const articlePaths = collectKbDocs().map(({ relativePath }) => `/articles/${toSlug(relativePath)}`)

  const sectionPaths = [
    '/sections/getting-started',
    '/sections/roles',
    '/sections/products',
    '/sections/how-to',
    '/sections/workflows',
    '/sections/compliance',
    '/sections/troubleshooting',
    '/sections/reference',
    '/sections/faq',
  ]

  return ['/', ...sectionPaths, ...articlePaths].filter((route) => !platformAdminPattern.test(route))
}

function sanitizeMarkdown(raw: string): string {
  return raw
    .split(/\r?\n/)
    .filter((line) => !platformAdminPattern.test(line))
    .filter((line) => !restrictedComplianceCoreLinePattern.test(line))
    .join('\n')
    .replace(/\n{3,}/g, '\n\n')
    .trim()
}

function isExcludedKbDocPath(relativePath: string): boolean {
  return (
    relativePath === 'index.md' ||
    relativePath.endsWith('/index.md') ||
    relativePath === 'roles/platform-admin-guide.md' ||
    relativePath === 'roles/compliance-admin-guide.md' ||
    relativePath === 'products/compliance-core-user-guide.md' ||
    relativePath === 'workflows/rule-import-to-evaluation.md' ||
    relativePath.startsWith('how-to/compliance-core/')
  )
}

function collectKbDocs(): RawKbDoc[] {
  return collectMarkdownFiles(docsRoot)
    .map((filePath) => ({
      relativePath: path.relative(docsRoot, filePath).replaceAll(path.sep, '/'),
      markdown: sanitizeMarkdown(readFileSync(filePath, 'utf8')),
    }))
    .filter(({ relativePath, markdown }) => !isExcludedKbDocPath(relativePath) && markdown.length > 0)
    .filter(({ relativePath, markdown }) => !platformAdminPattern.test(`${relativePath}\n${markdown}`))
}

function virtualKbDocsPlugin(): Plugin {
  return {
    name: 'virtual-kb-docs',
    resolveId(id) {
      return id === virtualKbDocsModuleId ? resolvedVirtualKbDocsModuleId : null
    },
    load(id) {
      if (id !== resolvedVirtualKbDocsModuleId) {
        return null
      }

      collectMarkdownFiles(docsRoot).forEach((filePath) => this.addWatchFile(filePath))
      return `export const rawKbArticles = ${JSON.stringify(collectKbDocs())};`
    },
    handleHotUpdate({ file, server }) {
      if (!isKbMarkdownFile(file)) {
        return
      }

      const virtualDocsModule = server.moduleGraph.getModuleById(resolvedVirtualKbDocsModuleId)
      if (virtualDocsModule) {
        server.moduleGraph.invalidateModule(virtualDocsModule)
      }

      server.ws.send({ type: 'full-reload' })
      return []
    },
  }
}

function kbArtifactsPlugin(): Plugin {
  return {
    name: 'kb-site-artifacts',
    closeBundle() {
      const outDir = path.resolve(appRoot, 'dist')
      mkdirSync(outDir, { recursive: true })

      const base = (process.env.VITE_KB_BASE_URL ?? 'https://kb.stlcompliance.com').replace(/\/+$/, '')
      const lastmod = new Date().toISOString().slice(0, 10)
      const entries = collectKbPaths()
        .map((route) => {
          const loc = route === '/' ? base : `${base}${route}`
          const priority = route === '/' ? '1.0' : route.startsWith('/articles/') ? '0.7' : '0.8'
          return `  <url>\n    <loc>${loc}</loc>\n    <lastmod>${lastmod}</lastmod>\n    <changefreq>weekly</changefreq>\n    <priority>${priority}</priority>\n  </url>`
        })
        .join('\n')

      writeFileSync(
        path.join(outDir, 'sitemap.xml'),
        `<?xml version="1.0" encoding="UTF-8"?>\n<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">\n${entries}\n</urlset>\n`,
        'utf8',
      )

      writeFileSync(
        path.join(outDir, 'robots.txt'),
        `User-agent: *\nAllow: /\n\nSitemap: ${base}/sitemap.xml\n`,
        'utf8',
      )
    },
  }
}

export default defineConfig({
  plugins: [react(), virtualKbDocsPlugin(), kbArtifactsPlugin()],
  server: {
    port: 5178,
    fs: {
      allow: [repoRoot],
    },
  },
  preview: {
    port: 5178,
    host: true,
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    include: ['src/**/*.{test,spec}.{ts,tsx}'],
  },
})
