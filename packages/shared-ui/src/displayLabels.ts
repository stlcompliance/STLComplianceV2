const OFFICIAL_LABELS = new Map<string, string>([
  ['stl compliance', 'STL Compliance'],
  ['nexarr', 'NexArr'],
  ['staffarr', 'StaffArr'],
  ['trainarr', 'TrainArr'],
  ['maintainarr', 'MaintainArr'],
  ['routarr', 'RoutArr'],
  ['supplyarr', 'SupplyArr'],
  ['loadarr', 'LoadArr'],
  ['ordarr', 'OrdArr'],
  ['customarr', 'CustomArr'],
  ['recordarr', 'RecordArr'],
  ['reportarr', 'ReportArr'],
  ['assurarr', 'AssurArr'],
  ['field companion', 'Field Companion'],
  ['fieldcompanion', 'Field Companion'],
  ['compliance core', 'Compliance Core'],
  ['compliancecore', 'Compliance Core'],
  ['referencedatacore', 'ReferenceDataCore'],
  ['reference data core', 'ReferenceDataCore'],
])

const ACRONYMS = new Set([
  'API',
  'CAPA',
  'CDL',
  'CSV',
  'DOT',
  'DQF',
  'ELD',
  'EPA',
  'FMCSA',
  'HOS',
  'HTML',
  'ID',
  'JSON',
  'KPI',
  'MFA',
  'MSHA',
  'OCR',
  'OSHA',
  'PDF',
  'PM',
  'POD',
  'PPE',
  'SCAR',
  'SDS',
  'SSO',
  'UI',
  'UUID',
  'VIN',
  'XLSX',
  'ZIP',
])

const GUID_PATTERN =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i

const TECHNICAL_KEY_PATTERN =
  /^[a-z]+(?:[._:-][a-z0-9]+){2,}$/i

function normalizeKnownLabel(value: string): string | null {
  const normalized = value.trim().replace(/[_-]+/g, ' ').replace(/\s+/g, ' ').toLowerCase()
  return OFFICIAL_LABELS.get(normalized) ?? null
}

function splitSystemWords(value: string): string[] {
  return value
    .trim()
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/([A-Z]+)([A-Z][a-z])/g, '$1 $2')
    .replace(/[._:/-]+/g, ' ')
    .replace(/\s+/g, ' ')
    .split(' ')
    .filter(Boolean)
}

function formatWord(word: string, index: number): string {
  const known = normalizeKnownLabel(word)
  if (known) {
    return known
  }

  const upper = word.toUpperCase()
  if (ACRONYMS.has(upper)) {
    return upper
  }

  const lower = word.toLowerCase()
  return index === 0 ? `${lower.charAt(0).toUpperCase()}${lower.slice(1)}` : lower
}

export function formatDisplayLabel(value: string | number | null | undefined, fallback = 'Not available'): string {
  if (value === null || value === undefined) {
    return fallback
  }

  const text = String(value).trim()
  if (!text) {
    return fallback
  }

  const known = normalizeKnownLabel(text)
  if (known) {
    return known
  }

  return splitSystemWords(text).map(formatWord).join(' ') || fallback
}

export function isLikelyInternalIdentifier(value: string | null | undefined): boolean {
  const text = value?.trim()
  if (!text) {
    return false
  }

  if (GUID_PATTERN.test(text)) {
    return true
  }

  if (/^[a-z]+-[0-9a-f]{6,}$/i.test(text)) {
    return true
  }

  return TECHNICAL_KEY_PATTERN.test(text) && !normalizeKnownLabel(text)
}

export function unavailableReferenceLabel(value: string | null | undefined): string {
  if (!value?.trim()) {
    return 'Unavailable record'
  }

  return isLikelyInternalIdentifier(value) ? 'Unavailable record' : formatDisplayLabel(value)
}
