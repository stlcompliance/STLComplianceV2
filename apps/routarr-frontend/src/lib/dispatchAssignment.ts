import type { DispatchAssignmentPreviewResponse } from '../api/types'

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
  if (preview.validationMessages && preview.validationMessages.length > 0) {
    return preview.validationMessages.join(' ')
  }

  const parts: string[] = []
  if (preview.blockingDriverAvailability.length > 0) {
    parts.push(`${preview.blockingDriverAvailability.length} driver availability block(s)`)
  }
  if (preview.blockingEquipmentAvailability.length > 0) {
    parts.push(`${preview.blockingEquipmentAvailability.length} equipment availability block(s)`)
  }
  if (preview.overlappingTrips.length > 0) {
    parts.push(`${preview.overlappingTrips.length} overlapping trip(s)`)
  }
  if (preview.driverEligibility?.isBlocking) {
    parts.push(`eligibility: ${preview.driverEligibility.message}`)
  } else if (preview.driverEligibility?.outcome === 'warn') {
    parts.push(`eligibility warning: ${preview.driverEligibility.message}`)
  }
  if (preview.assetDispatchability?.isBlocking) {
    parts.push(`dispatchability: ${preview.assetDispatchability.message}`)
  } else if (preview.assetDispatchability?.outcome === 'warn') {
    parts.push(`dispatchability warning: ${preview.assetDispatchability.message}`)
  }
  if (preview.workflowGates?.isBlocking) {
    parts.push(`workflow gate: ${preview.workflowGates.message}`)
  } else if (preview.workflowGates?.outcome === 'warn') {
    parts.push(`workflow gate warning: ${preview.workflowGates.message}`)
  }
  return parts.join(', ')
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
