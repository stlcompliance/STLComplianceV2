import type { ComplianceExceptionExemptionResponse } from '../api/types'

export type DisplayEntry = {
  label: string
  value: string
}

function formatDate(value: string | null): string {
  if (!value) {
    return 'Not set'
  }

  const parsed = new Date(value)
  return Number.isNaN(parsed.getTime())
    ? value
    : parsed.toLocaleString(undefined, {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: 'numeric',
        minute: '2-digit',
      })
}

function summarizeJsonKeys(json: string): string {
  try {
    const parsed = JSON.parse(json) as unknown
    if (!parsed || typeof parsed !== 'object' || Array.isArray(parsed)) {
      return 'Valid JSON, not an object'
    }

    const keys = Object.keys(parsed as Record<string, unknown>)
    return keys.length > 0 ? `Top-level keys: ${keys.join(', ')}` : 'Valid JSON object with no keys'
  } catch {
    return 'Invalid JSON'
  }
}

export function buildExceptionExemptionSummary(exemption: ComplianceExceptionExemptionResponse): DisplayEntry[] {
  return [
    { label: 'Key', value: exemption.key },
    { label: 'Label', value: exemption.label },
    { label: 'Status', value: exemption.active ? 'Active' : 'Inactive' },
    { label: 'Type', value: exemption.type },
    { label: 'Effect', value: exemption.effectType },
    { label: 'Scope', value: [exemption.governingBody, exemption.programKey, exemption.packKey].filter(Boolean).join(' · ') || 'Tenant-wide' },
    {
      label: 'Applies to',
      value: [exemption.appliesToSubjectKind, exemption.appliesToSourceProduct, exemption.appliesToSourceEntity]
        .filter(Boolean)
        .join(' · ') || 'Not specified',
    },
    { label: 'Condition logic', value: summarizeJsonKeys(exemption.conditionLogicJson) },
    { label: 'Issued by', value: exemption.issuingAuthority || 'Not set' },
    { label: 'Authorization number', value: exemption.authorizationNumber || 'Not set' },
    { label: 'Effective', value: formatDate(exemption.effectiveAt) },
    { label: 'Expires', value: formatDate(exemption.expiresAt) },
  ]
}

export function buildExceptionExemptionTechnicalDetails(exemption: ComplianceExceptionExemptionResponse): DisplayEntry[] {
  return [
    { label: 'Exception exemption ID', value: exemption.exceptionExemptionId },
    { label: 'Tenant ID', value: exemption.tenantId },
    { label: 'Governing body', value: exemption.governingBody },
    { label: 'Program key', value: exemption.programKey },
    { label: 'Pack key', value: exemption.packKey },
    { label: 'Citation key', value: exemption.citationKey },
    { label: 'Applicability key', value: exemption.applicabilityKey },
    { label: 'Condition logic JSON', value: exemption.conditionLogicJson },
    {
      label: 'Required evidence option group ID',
      value: exemption.requiredEvidenceOptionGroupId ?? 'Not set',
    },
    { label: 'Created at', value: formatDate(exemption.createdAt) },
    { label: 'Updated at', value: formatDate(exemption.updatedAt) },
  ]
}
