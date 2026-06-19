import { existsSync, readdirSync, readFileSync, statSync, writeFileSync } from 'node:fs'
import path from 'node:path'

const args = new Set(process.argv.slice(2))
const updateBaseline = args.has('--update-baseline')
const baselineFileName = 'theme-audit-baseline.json'

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

const rawColorPattern = /(?:#[0-9a-fA-F]{3,8}\b|\brgba?\([^)]*\)|\bhsla?\([^)]*\))/g
const lightOnlyClassPattern =
  /\b(?:bg-white|text-black|text-gray-(?:800|900|950)|bg-gray-(?:50|100|200)|border-gray-(?:100|200|300)|hover:bg-gray-(?:50|100|200)|hover:bg-white|bg-slate-100|bg-slate-200|text-slate-950)\b/g
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
const baselinePath = path.join(repoRoot, 'tools', baselineFileName)

const allowedRawColorFiles = new Set([
  normalizePath(path.relative(repoRoot, path.join(repoRoot, 'packages', 'shared-ui', 'src', 'theme.css'))),
  normalizePath(path.relative(repoRoot, path.join(repoRoot, 'scripts', 'audit-theme.mjs'))),
  normalizePath(path.relative(repoRoot, baselinePath)),
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
      findings.push(
        ...collectMatches('forbidden-light-tailwind-class', lightOnlyClassPattern, line, relativePath, index + 1),
      )
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

function compactFindings(findings) {
  const counts = new Map()
  for (const finding of findings) {
    counts.set(finding.key, (counts.get(finding.key) ?? 0) + 1)
  }
  return [...counts.entries()]
    .sort(([left], [right]) => left.localeCompare(right))
    .map(([key, count]) => ({ key, count }))
}

function readBaseline() {
  if (!existsSync(baselinePath)) {
    return { version: 1, findings: [] }
  }
  return JSON.parse(readFileSync(baselinePath, 'utf8'))
}

function countByKey(compacted) {
  return new Map(compacted.map((entry) => [entry.key, entry.count]))
}

const findings = collectFindings()
const compacted = compactFindings(findings)

if (updateBaseline) {
  writeFileSync(
    baselinePath,
    `${JSON.stringify(
      {
        version: 1,
        generatedAt: new Date().toISOString(),
        description:
          'Baseline for existing STL theme debt. npm run audit:theme fails when new raw colors, light-only utility classes, or presentation color fixture fields are added.',
        findings: compacted,
      },
      null,
      2,
    )}\n`,
  )
  console.log(`Theme audit baseline updated: ${normalizePath(path.relative(repoRoot, baselinePath))}`)
}

const baseline = readBaseline()
const baselineCounts = countByKey(baseline.findings ?? [])
const currentCounts = countByKey(compacted)
const newFindings = compacted.filter((entry) => entry.count > (baselineCounts.get(entry.key) ?? 0))

console.log('Theme audit summary:')
for (const [rule, count] of summarizeFindings(findings)) {
  console.log(`- ${rule}: ${count}`)
}

if (newFindings.length > 0) {
  console.error('\nNew theme audit violations detected:')
  for (const entry of newFindings.slice(0, 25)) {
    const [rule, relativePath, match, line] = entry.key.split('\t')
    console.error(`- ${rule} in ${relativePath}: ${match} :: ${line}`)
  }
  if (newFindings.length > 25) {
    console.error(`...and ${newFindings.length - 25} more.`)
  }
  console.error('\nUse shared semantic tokens in packages/shared-ui/src/theme.css, or update the baseline only after intentional cleanup.')
  process.exit(1)
}

console.log('No new theme audit violations.')
