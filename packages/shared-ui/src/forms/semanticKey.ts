const SEMANTIC_SEGMENT_MIN_LENGTH = 2
const SEMANTIC_SEGMENT_MAX_LENGTH = 64

const DEFAULT_STOP_WORDS = new Set([
  'a',
  'an',
  'and',
  'for',
  'in',
  'of',
  'on',
  'or',
  'the',
  'to',
  'with',
])

const KNOWN_ALIASES: Array<{ phrase: string; alias: string }> = [
  { phrase: 'driver qualification file', alias: 'dqf' },
  { phrase: 'safety data sheet', alias: 'sds' },
  { phrase: 'commercial driver license', alias: 'cdl' },
  { phrase: 'department of transportation', alias: 'dot' },
  { phrase: 'federal motor carrier safety administration', alias: 'fmcsa' },
  { phrase: 'occupational safety and health administration', alias: 'osha' },
  { phrase: 'mine safety and health administration', alias: 'msha' },
  { phrase: 'environmental protection agency', alias: 'epa' },
  { phrase: 'preventive maintenance', alias: 'pm' },
  { phrase: 'personal protective equipment', alias: 'ppe' },
  { phrase: 'lockout tagout', alias: 'loto' },
  { phrase: 'powered industrial truck', alias: 'pit' },
]

type BuildSemanticKeyInput = {
  domain: string
  kind: string
  title: string
  aliases?: readonly string[]
  existingKeys?: readonly string[]
}

function normalizePhrase(value: string): string {
  return value
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, ' ')
    .replace(/\s+/g, ' ')
}

function sanitizeSegment(value: string): string {
  return value
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]/g, '')
    .slice(0, SEMANTIC_SEGMENT_MAX_LENGTH)
}

function makeUniqueSemanticKey(candidate: string, existingKeys: readonly string[]): string {
  const normalizedExisting = new Set(existingKeys.map((key) => key.trim().toLowerCase()).filter(Boolean))
  const normalizedCandidate = candidate.trim().toLowerCase()

  if (!normalizedCandidate || !normalizedExisting.has(normalizedCandidate)) {
    return normalizedCandidate
  }

  let suffix = 2
  while (normalizedExisting.has(`${normalizedCandidate}.${suffix}`)) {
    suffix += 1
  }

  return `${normalizedCandidate}.${suffix}`
}

export function compactSemanticSlug(title: string, stopWords: ReadonlySet<string> = DEFAULT_STOP_WORDS): string {
  const normalized = normalizePhrase(title)
  const tokens = normalized.split(' ').filter(Boolean)
  const filtered = tokens.filter((token) => !stopWords.has(token))
  const source = filtered.length > 0 ? filtered : tokens
  const compact = source.join('')
  return compact.slice(0, SEMANTIC_SEGMENT_MAX_LENGTH)
}

export function chooseSemanticAlias(title: string, aliases: readonly string[] = []): string | null {
  for (const alias of aliases) {
    const normalizedAlias = sanitizeSegment(alias)
    if (normalizedAlias.length >= SEMANTIC_SEGMENT_MIN_LENGTH) {
      return normalizedAlias
    }
  }

  const normalizedTitle = normalizePhrase(title)
  for (const known of KNOWN_ALIASES) {
    if (normalizedTitle.includes(known.phrase)) {
      return known.alias
    }
  }

  return null
}

export function buildSemanticKey({
  domain,
  kind,
  title,
  aliases = [],
  existingKeys = [],
}: BuildSemanticKeyInput): string {
  const normalizedDomain = sanitizeSegment(domain)
  const normalizedKind = sanitizeSegment(kind)
  if (!normalizedDomain || !normalizedKind) {
    return ''
  }

  const alias = chooseSemanticAlias(title, aliases)
  const slug = alias ?? compactSemanticSlug(title)
  if (slug.length < SEMANTIC_SEGMENT_MIN_LENGTH) {
    return ''
  }

  const candidate = `${normalizedDomain}.${normalizedKind}.${slug}`
  return makeUniqueSemanticKey(candidate, existingKeys)
}
