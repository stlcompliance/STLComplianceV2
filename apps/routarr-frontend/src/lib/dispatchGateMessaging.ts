import type { DispatchAssignmentPreviewResponse } from '../api/types'

export type DispatchGateLineSeverity = 'block' | 'warn' | 'info'

export type DispatchGateLineCategory =
  | 'availability'
  | 'overlap'
  | 'eligibility'
  | 'dispatchability'
  | 'workflow'
  | 'other'

export interface DispatchGateLine {
  severity: DispatchGateLineSeverity
  category: DispatchGateLineCategory
  label: string
  detail: string
  reasonCode?: string | null
}

function workflowGateLines(
  summary: NonNullable<DispatchAssignmentPreviewResponse['workflowGates']>,
): DispatchGateLine[] {
  if (summary.gates.length > 0) {
    return summary.gates.map((gate) => ({
      severity: gate.isBlocking ? 'block' : gate.outcome === 'warn' ? 'warn' : 'info',
      category: 'workflow',
      label: gate.gateKey,
      detail: gate.message,
      reasonCode: gate.reasonCode,
    }))
  }

  if (summary.outcome === 'allow' && !summary.isBlocking) {
    return []
  }

  return [
    {
      severity: summary.isBlocking ? 'block' : summary.outcome === 'warn' ? 'warn' : 'info',
      category: 'workflow',
      label: 'Compliance workflow',
      detail: summary.message,
      reasonCode: summary.reasonCode,
    },
  ]
}

/**
 * Structured gate/conflict lines for inline UI (bulk preview, unassigned queue, assignment board).
 */
export function buildDispatchAssignmentGateLines(
  preview: DispatchAssignmentPreviewResponse,
): DispatchGateLine[] {
  const lines: DispatchGateLine[] = []

  for (const conflict of preview.blockingDriverAvailability) {
    lines.push({
      severity: 'block',
      category: 'availability',
      label: 'Driver availability',
      detail: `${conflict.availabilityStatus}: ${conflict.reason}`,
    })
  }

  for (const conflict of preview.blockingEquipmentAvailability) {
    lines.push({
      severity: 'block',
      category: 'availability',
      label: 'Equipment availability',
      detail: `${conflict.availabilityStatus}: ${conflict.reason}`,
    })
  }

  for (const overlap of preview.overlappingTrips) {
    lines.push({
      severity: 'block',
      category: 'overlap',
      label: 'Overlapping trip',
      detail: `${overlap.tripNumber} (${overlap.dispatchStatus.replace('_', ' ')})`,
    })
  }

  const eligibility = preview.driverEligibility
  if (eligibility) {
    if (eligibility.isBlocking) {
      lines.push({
        severity: 'block',
        category: 'eligibility',
        label: 'Driver eligibility',
        detail: eligibility.message,
        reasonCode: eligibility.reasonCode,
      })
    } else if (eligibility.outcome === 'warn') {
      lines.push({
        severity: 'warn',
        category: 'eligibility',
        label: 'Driver eligibility',
        detail: eligibility.message,
        reasonCode: eligibility.reasonCode,
      })
    }
  }

  const dispatchability = preview.assetDispatchability
  if (dispatchability) {
    if (dispatchability.isBlocking) {
      lines.push({
        severity: 'block',
        category: 'dispatchability',
        label: 'Asset dispatchability',
        detail: dispatchability.message,
        reasonCode: dispatchability.reasonCode,
      })
    } else if (dispatchability.outcome === 'warn') {
      lines.push({
        severity: 'warn',
        category: 'dispatchability',
        label: 'Asset dispatchability',
        detail: dispatchability.message,
        reasonCode: dispatchability.reasonCode,
      })
    }
  }

  if (preview.workflowGates) {
    lines.push(...workflowGateLines(preview.workflowGates))
  }

  if (preview.conflictSummary?.hasMissingExternalData) {
    lines.push({
      severity: 'warn',
      category: 'other',
      label: 'External data',
      detail: 'Missing or unavailable data detected in pre-dispatch checks.',
      reasonCode: 'external_data_unavailable',
    })
  }

  if (preview.conflictSummary?.hasStaleExternalData) {
    lines.push({
      severity: 'warn',
      category: 'other',
      label: 'External data',
      detail: 'Potentially stale data detected in pre-dispatch checks.',
      reasonCode: 'external_data_stale',
    })
  }

  if (lines.length === 0 && preview.validationMessages?.length) {
    for (const message of preview.validationMessages) {
      lines.push({
        severity: preview.hasBlockingConflicts ? 'block' : 'warn',
        category: 'other',
        label: 'Assignment check',
        detail: message,
        reasonCode: preview.primaryBlockCode,
      })
    }
  }

  if (lines.length === 0 && !preview.hasBlockingConflicts) {
    lines.push({
      severity: 'info',
      category: 'other',
      label: 'Ready',
      detail: 'No blocking conflicts for this assignment.',
    })
  }

  return lines
}

export function formatDispatchAssignmentGateLines(
  lines: DispatchGateLine[],
  options?: { includeReasonCodes?: boolean },
): string {
  const includeReasonCodes = options?.includeReasonCodes ?? true
  return lines
    .filter((line) => line.severity !== 'info' || lines.length === 1)
    .map((line) => {
      const code =
        includeReasonCodes && line.reasonCode ? ` (${line.reasonCode})` : ''
      return `${line.label}: ${line.detail}${code}`
    })
    .join('\n')
}

export function formatDispatchAssignmentGateConfirmMessage(
  preview: DispatchAssignmentPreviewResponse,
  actionLabel = 'Assign anyway?',
): string {
  const lines = buildDispatchAssignmentGateLines(preview).filter((line) => line.severity !== 'info')
  const body =
    lines.length > 0
      ? formatDispatchAssignmentGateLines(lines)
      : formatAssignmentConflictFallback(preview)

  return `Assignment blocked:\n${body}\n\n${actionLabel}`
}

function formatAssignmentConflictFallback(preview: DispatchAssignmentPreviewResponse): string {
  const parts: string[] = []
  if (preview.primaryBlockCode) {
    parts.push(preview.primaryBlockCode)
  }
  if (preview.validationMessages?.length) {
    parts.push(...preview.validationMessages)
  }
  return parts.join('\n') || 'Conflicts detected.'
}
