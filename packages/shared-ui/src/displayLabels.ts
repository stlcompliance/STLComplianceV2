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

const ROLE_LABELS = new Map<string, string>([
  ['platform_admin', 'Platform Admin'],
  ['platform_owner', 'Platform Owner'],
  ['platform_support', 'Platform Support'],
  ['tenant_admin', 'Tenant Admin'],
  ['tenant_user', 'Tenant User'],
  ['service_client', 'Service Client'],
  ['product_service', 'Product Service'],
  ['read_only_auditor', 'Read only auditor'],
  ['role_editor', 'Role Editor'],
])

const STATUS_LABELS = new Map<string, string>([
  ['active', 'Active'],
  ['inactive', 'Inactive'],
  ['archived', 'Archived'],
  ['draft', 'Draft'],
  ['pending_review', 'Pending review'],
  ['in_progress', 'In progress'],
  ['needs_attention', 'Needs attention'],
  ['failed_validation', 'Needs correction'],
  ['completed', 'Complete'],
  ['cancelled', 'Canceled'],
  ['canceled', 'Canceled'],
  ['blocked', 'Blocked'],
  ['active_credentials', 'Active credentials'],
  ['expired_credentials', 'Expired credentials'],
  ['revoked_credentials', 'Revoked credentials'],
  ['healthy', 'Healthy'],
  ['degraded', 'Degraded'],
  ['unhealthy', 'Unhealthy'],
  ['trial', 'Trial'],
  ['suspended', 'Suspended'],
  ['watch', 'Watch'],
  ['review', 'Review'],
  ['info', 'Info'],
  ['neutral', 'Neutral'],
  ['good', 'Good'],
  ['warn', 'Warning'],
  ['bad', 'Blocked'],
  ['error', 'Error'],
])

const PERMISSION_ACTION_LABELS = new Map<string, string>([
  ['read', 'View'],
  ['view', 'View'],
  ['list', 'View'],
  ['search', 'Search'],
  ['create', 'Create'],
  ['add', 'Add'],
  ['update', 'Edit'],
  ['edit', 'Edit'],
  ['patch', 'Update'],
  ['delete', 'Delete'],
  ['remove', 'Remove'],
  ['manage', 'Manage'],
  ['assign', 'Assign'],
  ['approve', 'Approve'],
  ['export', 'Export'],
  ['import', 'Import'],
  ['invite', 'Invite'],
  ['revoke', 'Revoke'],
  ['enable', 'Enable'],
  ['disable', 'Disable'],
  ['run', 'Run'],
  ['send', 'Send'],
  ['sync', 'Sync'],
  ['download', 'Download'],
  ['upload', 'Upload'],
  ['publish', 'Publish'],
  ['launch', 'Launch'],
  ['schedule', 'Schedule'],
  ['close', 'Close'],
  ['open', 'Open'],
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

export function formatProductDisplayName(value: string | null | undefined, fallback = 'Product'): string {
  if (value === null || value === undefined) {
    return fallback
  }

  const text = String(value).trim()
  if (!text) {
    return fallback
  }

  return formatDisplayLabel(text, fallback)
}

export function formatRoleDisplayName(value: string | null | undefined, fallback = 'Role'): string {
  if (value === null || value === undefined) {
    return fallback
  }

  const text = String(value).trim().toLowerCase()
  if (!text) {
    return fallback
  }

  return ROLE_LABELS.get(text) ?? formatDisplayLabel(text, fallback)
}

function extractPermissionScope(segments: string[]): string[] {
  if (!segments.length) {
    return []
  }

  const [first, ...rest] = segments
  if (
    [
      'staffarr',
      'trainarr',
      'maintainarr',
      'routarr',
      'supplyarr',
      'customarr',
      'ordarr',
      'loadarr',
      'recordarr',
      'reportarr',
      'assurarr',
      'compliancecore',
      'nexarr',
      'platform',
    ].includes(first.toLowerCase())
  ) {
    return rest.slice(0, -1)
  }

  return segments.slice(0, -1)
}

export function formatPermissionDisplayName(value: string | null | undefined, fallback = 'Permission'): string {
  if (value === null || value === undefined) {
    return fallback
  }

  const text = String(value).trim()
  if (!text) {
    return fallback
  }

  const segments = text.split(/[.:/_-]+/).filter(Boolean)
  if (!segments.length) {
    return fallback
  }

  const action = segments.at(-1)?.toLowerCase() ?? ''
  const actionLabel = PERMISSION_ACTION_LABELS.get(action)
  if (!actionLabel) {
    return formatDisplayLabel(text, fallback)
  }

  const scopeSegments = extractPermissionScope(segments)
  const scopeText = scopeSegments.join(' ')
  if (!scopeText) {
    return actionLabel
  }

  const scopeLabel = formatDisplayLabel(scopeText, fallback)
  return scopeLabel === fallback ? actionLabel : `${actionLabel} ${scopeLabel.toLowerCase()}`
}

export function formatStatusLabel(value: string | number | null | undefined, fallback = 'Not available'): string {
  if (value === null || value === undefined) {
    return fallback
  }

  const text = String(value).trim()
  if (!text) {
    return fallback
  }

  const normalized = text.toLowerCase().replace(/[-\s]+/g, '_')
  return STATUS_LABELS.get(normalized) ?? formatDisplayLabel(text, fallback)
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
