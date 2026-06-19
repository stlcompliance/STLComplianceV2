import { existsSync, readdirSync, readFileSync, statSync } from 'node:fs'
import path from 'node:path'

const ignoredDirectories = new Set([
  '.git',
  '.tmp-playwright',
  'bin',
  'build',
  'coverage',
  'dist',
  'node_modules',
  'obj',
  'out',
])

const ignoredExtensions = new Set([
  '.bmp',
  '.gif',
  '.ico',
  '.jpeg',
  '.jpg',
  '.lock',
  '.pdf',
  '.png',
  '.svg',
  '.tsbuildinfo',
  '.webp',
  '.zip',
])

const scannedExtensions = new Set([
  '.css',
  '.scss',
  '.html',
  '.js',
  '.jsx',
  '.mjs',
  '.cjs',
  '.ts',
  '.tsx',
  '.json',
  '.md',
])

const rawColorPattern = /(?:#[0-9a-fA-F]{3,8}(?![0-9a-fA-F])|rgba?\([^)]*\)|hsla?\([^)]*\))/g
const lightOnlyClassPattern =
  /\b(?:bg-white|text-black|text-gray-(?:800|900|950)|text-slate-(?:900|950)|bg-gray-(?:50|100|200)|bg-slate-(?:50|100|200)|border-gray-(?:100|200|300)|border-slate-(?:100|200|300)|hover:bg-gray-(?:50|100|200)|hover:bg-slate-(?:50|100|200)|hover:bg-white)\b/g
const fixturePresentationPattern =
  /\b(?:statusClass|badgeClass|badgeColor|toneColor|colorClass|backgroundColor|borderColor)\b\s*[:=]\s*['"`](?:#|rgb|hsl|bg-|text-|border-)|\b(?:statusClass|badgeClass|badgeColor|toneColor|colorClass)\b/g

function findRepoRoot(startDir) {
  let current = startDir
  while (true) {
    if (existsSync(path.join(current, 'docs', 'constitutions', 'ownership.md'))) {
      return current
    }

    const parent = path.dirname(current)
    if (parent === current) {
      throw new Error('Could not locate STLComplianceV2 repository root.')
    }
    current = parent
  }
}

const repoRoot = findRepoRoot(process.cwd())

const allowedRawColorFiles = new Set([
  normalizePath(path.relative(repoRoot, path.join(repoRoot, 'packages', 'shared-ui', 'src', 'theme.css'))),
])

const scanRoots = [
  path.join(repoRoot, 'apps'),
  path.join(repoRoot, 'packages', 'shared-ui', 'src'),
  path.join(repoRoot, 'tests'),
]

function normalizePath(filePath) {
  return filePath.split(path.sep).join('/')
}

function shouldScanFile(filePath) {
  const ext = path.extname(filePath)
  if (ignoredExtensions.has(ext)) {
    return false
  }
  if (!scannedExtensions.has(ext)) {
    return false
  }

  const name = path.basename(filePath)
  return name !== 'package-lock.json'
}

function walkFiles(rootDir, files = []) {
  if (!existsSync(rootDir)) {
    return files
  }

  for (const entry of readdirSync(rootDir)) {
    if (ignoredDirectories.has(entry)) {
      continue
    }

    const fullPath = path.join(rootDir, entry)
    const stats = statSync(fullPath)
    if (stats.isDirectory()) {
      walkFiles(fullPath, files)
      continue
    }

    if (stats.isFile() && shouldScanFile(fullPath)) {
      files.push(fullPath)
    }
  }

  return files
}

function collectMatches(rule, pattern, line, relativePath, lineNumber) {
  if (line.includes('theme-audit-allow brand-color')) {
    return []
  }

  const findings = []
  for (const match of line.matchAll(pattern)) {
    findings.push({
      key: [rule, relativePath, match[0], line.trim()].join('\t'),
      rule,
      relativePath,
      lineNumber,
      match: match[0],
    })
  }
  return findings
}

function collectFindings() {
  const findings = []
  const files = scanRoots.flatMap((rootDir) => walkFiles(rootDir))

  for (const filePath of files) {
    const relativePath = normalizePath(path.relative(repoRoot, filePath))
    const lines = readFileSync(filePath, 'utf8').split(/\r?\n/)

    for (const [index, line] of lines.entries()) {
      if (!allowedRawColorFiles.has(relativePath)) {
        findings.push(...collectMatches('raw-color', rawColorPattern, line, relativePath, index + 1))
      }
      if (!allowedRawColorFiles.has(relativePath)) {
        findings.push(
          ...collectMatches('forbidden-light-tailwind-class', lightOnlyClassPattern, line, relativePath, index + 1),
        )
      }
      findings.push(
        ...collectMatches('fixture-presentation-color', fixturePresentationPattern, line, relativePath, index + 1),
      )
    }
  }

  return findings
}

function summarizeFindings(findings) {
  const summary = new Map()
  for (const finding of findings) {
    summary.set(finding.rule, (summary.get(finding.rule) ?? 0) + 1)
  }
  return [...summary.entries()].sort(([left], [right]) => left.localeCompare(right))
}

const findings = collectFindings()

console.log('Theme audit summary:')
const summary = summarizeFindings(findings)
if (summary.length === 0) {
  console.log('- no violations')
} else {
  for (const [rule, count] of summary) {
    console.log(`- ${rule}: ${count}`)
  }
}

if (findings.length > 0) {
  console.error('\nTheme audit violations detected:')
  for (const finding of findings.slice(0, 50)) {
    console.error(
      `- ${finding.rule} in ${finding.relativePath}:${finding.lineNumber}: ${finding.match}`,
    )
  }
  if (findings.length > 50) {
    console.error(`...and ${findings.length - 50} more.`)
  }
  console.error('\nUse shared semantic tokens in packages/shared-ui/src/theme.css. Brand/logo colors require an explicit theme-audit-allow brand-color annotation.')
  process.exit(1)
}

console.log('No theme audit violations.')
