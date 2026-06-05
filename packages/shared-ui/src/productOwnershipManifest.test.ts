import { readdirSync, readFileSync } from 'node:fs'
import { dirname, join, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'
import { describe, expect, it } from 'vitest'
import { SUITE_PRODUCT_CATALOG } from './productCatalog'
import {
  IMPLEMENTED_PRODUCT_KEYS,
  IMPLEMENTED_PRODUCT_OWNERSHIP,
  normalizeProductKey,
} from './productOwnershipManifest'

const srcDir = dirname(fileURLToPath(import.meta.url))
const repoRoot = resolve(srcDir, '../../..')
const constitutionPath = join(repoRoot, 'docs', 'constitutions', 'ownership.md')
const appsRoot = join(repoRoot, 'apps')

function extractConstitutionProductKeys(markdown: string): string[] {
  return Array.from(markdown.matchAll(/^={20,}\r?\n([^\r\n]+)\r?\n={20,}$/gm))
    .map(([, heading]) => heading.trim())
    .filter(
      (heading) =>
        heading.endsWith('Arr') ||
        heading === 'Compliance Core' ||
        heading === 'Field Companion',
    )
    .map((heading) => normalizeProductKey(heading.replace(/\s+/g, '-')))
}

function collectNamedFiles(root: string, fileNames: ReadonlySet<string>): string[] {
  const files: string[] = []

  for (const entry of readdirSync(root, { withFileTypes: true })) {
    if (entry.name === 'node_modules' || entry.name === 'bin' || entry.name === 'obj') {
      continue
    }

    const fullPath = join(root, entry.name)
    if (entry.isDirectory()) {
      files.push(...collectNamedFiles(fullPath, fileNames))
      continue
    }

    if (fileNames.has(entry.name)) {
      files.push(fullPath)
    }
  }

  return files
}

function assertNoForbiddenPattern(
  filePath: string,
  content: string,
  patterns: readonly RegExp[],
): void {
  for (const pattern of patterns) {
    if (pattern.test(content)) {
      throw new Error(`${filePath} matched forbidden pattern ${pattern}`)
    }
  }
}

describe('productOwnershipManifest', () => {
  it('covers every implemented product with a constitution ownership section', () => {
    const constitution = readFileSync(constitutionPath, 'utf8')
    const constitutionProductKeys = new Set(extractConstitutionProductKeys(constitution))

    expect(constitutionProductKeys.has('customarr')).toBe(true)
    expect(constitutionProductKeys.has('ordarr')).toBe(true)

    for (const productKey of IMPLEMENTED_PRODUCT_KEYS) {
      expect(constitutionProductKeys.has(productKey)).toBe(true)
    }
  })

  it('keeps shared catalog metadata derived from the ownership manifest', () => {
    expect(SUITE_PRODUCT_CATALOG).toHaveLength(IMPLEMENTED_PRODUCT_OWNERSHIP.length)

    for (const entry of IMPLEMENTED_PRODUCT_OWNERSHIP) {
      const catalogEntry = SUITE_PRODUCT_CATALOG.find(
        (candidate) => candidate.productKey === entry.productKey,
      )

      expect(catalogEntry).toBeDefined()
      expect(catalogEntry?.displayName).toBe(entry.displayName)
      expect(catalogEntry?.description).toBe(entry.catalogDescription)
      expect(catalogEntry?.sortOrder).toBe(entry.sortOrder)
    }
  })

  it('blocks public Reports navigation outside ReportArr', () => {
    const frontendRoots = readdirSync(appsRoot, { withFileTypes: true })
      .filter(
        (entry) =>
          entry.isDirectory() &&
          entry.name.endsWith('-frontend') &&
          entry.name !== 'reportarr-frontend',
      )
      .map((entry) => join(appsRoot, entry.name))

    const frontendFiles = frontendRoots.flatMap((root) =>
      collectNamedFiles(root, new Set(['App.tsx', 'productNav.ts', 'workspaceSection.ts'])),
    )

    const forbiddenPatterns = [
      /label:\s*['"]Reports['"]/,
      /to:\s*['"]\/reports['"]/,
      /path:\s*['"]\/reports['"]/,
      /key:\s*['"]reports['"]/,
      /section\s*===\s*['"]reports['"]/,
      /startsWith\(\s*['"]\/reports['"]\s*\)/,
      /Route\s+path=['"]\/reports['"]/,
    ] as const

    for (const filePath of frontendFiles) {
      const content = readFileSync(filePath, 'utf8')
      assertNoForbiddenPattern(filePath, content, forbiddenPatterns)
    }
  })

  it('blocks public report endpoint mapping outside ReportArr', () => {
    const apiRoots = readdirSync(appsRoot, { withFileTypes: true })
      .filter(
        (entry) =>
          entry.isDirectory() && entry.name.endsWith('-api') && entry.name !== 'reportarr-api',
      )
      .map((entry) => join(appsRoot, entry.name))

    const apiFiles = apiRoots.flatMap((root) =>
      collectNamedFiles(root, new Set(['Program.cs', 'V1FeatureAliasEndpoints.cs'])),
    )

    const forbiddenPatterns = [
      /Map[A-Za-z0-9]*Report[A-Za-z0-9]*\s*\(/,
      /\/api\/reports/,
      /\/api\/v1\/reports/,
    ] as const

    for (const filePath of apiFiles) {
      const content = readFileSync(filePath, 'utf8')
      assertNoForbiddenPattern(filePath, content, forbiddenPatterns)
    }
  })
})
