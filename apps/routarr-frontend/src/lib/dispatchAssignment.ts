import type { DispatchAssignmentPreviewResponse } from '../api/types'

import {
  buildDispatchAssignmentGateLines,
  formatDispatchAssignmentGateConfirmMessage,
  formatDispatchAssignmentGateLines,
} from './dispatchGateMessaging'

export type DragAssignmentPayload =
  | { kind: 'driver'; personId: string }
  | { kind: 'vehicle'; vehicleRefKey: string }

export const BOARD_ASSIGNABLE_STATUSES = new Set([
  'planned',
  'assigned',
  'dispatched',
  'in_progress',
])

export const DRAG_MIME = 'application/routarr-assignment'

export function formatAssignmentConflictMessage(preview: DispatchAssignmentPreviewResponse) {
  const lines = buildDispatchAssignmentGateLines(preview).filter((line) => line.severity !== 'info')
  if (lines.length > 0) {
    return formatDispatchAssignmentGateLines(lines, { includeReasonCodes: false })
  }

  if (preview.validationMessages && preview.validationMessages.length > 0) {
    return preview.validationMessages.join('; ')
  }

  return 'Conflicts detected'
}

export function resolveAssignmentIgnoreFlags(preview: DispatchAssignmentPreviewResponse) {
  const hasAvailabilityConflict =
    preview.blockingDriverAvailability.length > 0
    || preview.blockingEquipmentAvailability.length > 0
    || preview.overlappingTrips.length > 0
  const hasEligibilityBlock = preview.driverEligibility?.isBlocking === true
  const hasDispatchabilityBlock = preview.assetDispatchability?.isBlocking === true
  const hasWorkflowGateBlock = preview.workflowGates?.isBlocking === true

  return {
    ignoreConflicts: hasAvailabilityConflict,
    ignoreEligibilityBlocks: hasEligibilityBlock,
    ignoreDispatchabilityBlocks: hasDispatchabilityBlock,
    ignoreWorkflowGateBlocks: hasWorkflowGateBlock,
    hasEligibilityWarn:
      preview.driverEligibility?.outcome === 'warn' && !preview.driverEligibility.isBlocking,
    hasDispatchabilityWarn:
      preview.assetDispatchability?.outcome === 'warn' && !preview.assetDispatchability.isBlocking,
    hasWorkflowGateWarn:
      preview.workflowGates?.outcome === 'warn' && !preview.workflowGates.isBlocking,
  }
}

export function parseDragPayload(raw: string): DragAssignmentPayload | null {
  try {
    return JSON.parse(raw) as DragAssignmentPayload
  } catch {
    return null
  }
}

export type AssignmentIgnoreFlags = {
  ignoreConflicts: boolean
  ignoreEligibilityBlocks: boolean
  ignoreDispatchabilityBlocks: boolean
  ignoreWorkflowGateBlocks: boolean
}

/**
 * Interprets a dispatch assignment preview and asks the user to confirm blocking or warning outcomes.
 * Returns ignore flags when the caller should proceed, or null when the user cancelled.
 */
export function confirmDispatchAssignmentPreview(
  preview: DispatchAssignmentPreviewResponse,
  confirm: (message: string) => boolean,
): AssignmentIgnoreFlags | null {
  const flags = resolveAssignmentIgnoreFlags(preview)

  if (preview.hasBlockingConflicts) {
    const confirmed = confirm(formatDispatchAssignmentGateConfirmMessage(preview))
    if (!confirmed) {
      return null
    }
    return {
      ignoreConflicts: flags.ignoreConflicts,
      ignoreEligibilityBlocks: flags.ignoreEligibilityBlocks,
      ignoreDispatchabilityBlocks: flags.ignoreDispatchabilityBlocks,
      ignoreWorkflowGateBlocks: flags.ignoreWorkflowGateBlocks,
    }
  }

  if (flags.hasEligibilityWarn) {
    const confirmed = confirm(
      `Driver eligibility warning: ${preview.driverEligibility!.message}. Continue assignment?`,
    )
    if (!confirmed) {
      return null
    }
  } else if (flags.hasDispatchabilityWarn) {
    const confirmed = confirm(
      `Asset dispatchability warning: ${preview.assetDispatchability!.message}. Continue assignment?`,
    )
    if (!confirmed) {
      return null
    }
  } else if (flags.hasWorkflowGateWarn) {
    const confirmed = confirm(
      `Compliance workflow gate warning: ${preview.workflowGates!.message}. Continue assignment?`,
    )
    if (!confirmed) {
      return null
    }
  }

  return {
    ignoreConflicts: false,
    ignoreEligibilityBlocks: false,
    ignoreDispatchabilityBlocks: false,
    ignoreWorkflowGateBlocks: false,
  }
}
