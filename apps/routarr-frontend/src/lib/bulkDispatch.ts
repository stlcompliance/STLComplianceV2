import type {
  BulkDispatchItemPreview,
  BulkDispatchPreviewResponse,
  DispatchAssignmentPreviewResponse,
} from '../api/types'

import { formatAssignmentConflictMessage, resolveAssignmentIgnoreFlags } from './dispatchAssignment'
import { buildDispatchAssignmentGateLines } from './dispatchGateMessaging'

export type BulkDispatchIgnoreFlags = {
  ignoreAvailabilityConflicts: boolean
  ignoreEligibilityBlocks: boolean
  ignoreDispatchabilityBlocks: boolean
  ignoreWorkflowGateBlocks: boolean
}

function appendAssignmentSummary(
  parts: string[],
  preview: DispatchAssignmentPreviewResponse | null | undefined,
  role: 'driver' | 'vehicle',
) {
  if (!preview) {
    return
  }

  const lines = buildDispatchAssignmentGateLines(preview).filter((line) => line.severity !== 'info')
  if (lines.length === 0) {
    return
  }

  const prefix = role === 'driver' ? 'Driver' : 'Vehicle'
  if (preview.hasBlockingConflicts) {
    parts.push(`${prefix}: ${formatAssignmentConflictMessage(preview)}`)
    return
  }

  for (const line of lines) {
    parts.push(`${prefix} ${line.label.toLowerCase()}: ${line.detail}`)
  }
}

export function formatBulkDispatchItemSummary(item: BulkDispatchItemPreview) {
  const parts: string[] = []

  if (item.driverPreview?.hasBlockingConflicts) {
    parts.push('driver conflict')
  }
  if (item.vehiclePreview?.hasBlockingConflicts) {
    parts.push('vehicle conflict')
  }
  if (item.statusPreview && !item.statusPreview.canTransition) {
    parts.push('status blocked')
  }

  appendAssignmentSummary(parts, item.driverPreview, 'driver')
  appendAssignmentSummary(parts, item.vehiclePreview, 'vehicle')

  const uniqueParts = [...new Set(parts.filter((part) => part.length > 0))]
  return uniqueParts.length > 0 ? uniqueParts.join('; ') : 'ready'
}

export function resolveBulkDispatchIgnoreFlags(
  items: BulkDispatchItemPreview[],
): BulkDispatchIgnoreFlags {
  let ignoreAvailabilityConflicts = false
  let ignoreEligibilityBlocks = false
  let ignoreDispatchabilityBlocks = false
  let ignoreWorkflowGateBlocks = false

  for (const item of items) {
    for (const preview of [item.driverPreview, item.vehiclePreview]) {
      if (!preview) {
        continue
      }

      const flags = resolveAssignmentIgnoreFlags(preview)
      ignoreAvailabilityConflicts ||= flags.ignoreConflicts
      ignoreEligibilityBlocks ||= flags.ignoreEligibilityBlocks
      ignoreDispatchabilityBlocks ||= flags.ignoreDispatchabilityBlocks
      ignoreWorkflowGateBlocks ||= flags.ignoreWorkflowGateBlocks
    }
  }

  return {
    ignoreAvailabilityConflicts,
    ignoreEligibilityBlocks,
    ignoreDispatchabilityBlocks,
    ignoreWorkflowGateBlocks,
  }
}

function countBlockedItemsByKind(items: BulkDispatchItemPreview[]) {
  let availabilityBlocks = 0
  let eligibilityBlocks = 0
  let dispatchabilityBlocks = 0
  let workflowGateBlocks = 0
  let statusBlocks = 0

  for (const item of items.filter((entry) => !entry.canApply)) {
    if (item.statusPreview && !item.statusPreview.canTransition) {
      statusBlocks += 1
    }

    for (const preview of [item.driverPreview, item.vehiclePreview]) {
      if (!preview?.hasBlockingConflicts) {
        continue
      }

      if (
        preview.blockingDriverAvailability.length > 0
        || preview.blockingEquipmentAvailability.length > 0
        || preview.overlappingTrips.length > 0
      ) {
        availabilityBlocks += 1
      }
      if (preview.driverEligibility?.isBlocking) {
        eligibilityBlocks += 1
      }
      if (preview.assetDispatchability?.isBlocking) {
        dispatchabilityBlocks += 1
      }
      if (preview.workflowGates?.isBlocking) {
        workflowGateBlocks += 1
      }
    }
  }

  return {
    availabilityBlocks,
    eligibilityBlocks,
    dispatchabilityBlocks,
    workflowGateBlocks,
    statusBlocks,
  }
}

export function formatBulkDispatchBlockedMessage(preview: BulkDispatchPreviewResponse) {
  const counts = countBlockedItemsByKind(preview.items)
  const parts: string[] = []

  if (counts.workflowGateBlocks > 0) {
    parts.push(`${counts.workflowGateBlocks} workflow gate block(s)`)
  }
  if (counts.eligibilityBlocks > 0) {
    parts.push(`${counts.eligibilityBlocks} eligibility block(s)`)
  }
  if (counts.dispatchabilityBlocks > 0) {
    parts.push(`${counts.dispatchabilityBlocks} dispatchability block(s)`)
  }
  if (counts.availabilityBlocks > 0) {
    parts.push(`${counts.availabilityBlocks} availability conflict(s)`)
  }
  if (counts.statusBlocks > 0) {
    parts.push(`${counts.statusBlocks} status block(s)`)
  }

  return parts.length > 0 ? parts.join(', ') : 'see preview details'
}

function collectBulkDispatchWarnings(items: BulkDispatchItemPreview[]) {
  const messages: string[] = []

  for (const item of items) {
    for (const preview of [item.driverPreview, item.vehiclePreview]) {
      if (!preview || preview.hasBlockingConflicts) {
        continue
      }

      if (preview.driverEligibility?.outcome === 'warn' && !preview.driverEligibility.isBlocking) {
        messages.push(
          `${item.tripNumber}: driver eligibility warning — ${preview.driverEligibility.message}. Continue?`,
        )
      } else if (
        preview.assetDispatchability?.outcome === 'warn'
        && !preview.assetDispatchability.isBlocking
      ) {
        messages.push(
          `${item.tripNumber}: asset dispatchability warning — ${preview.assetDispatchability.message}. Continue?`,
        )
      } else if (preview.workflowGates?.outcome === 'warn' && !preview.workflowGates.isBlocking) {
        messages.push(
          `${item.tripNumber}: Compliance workflow gate warning — ${preview.workflowGates.message}. Continue?`,
        )
      }
    }
  }

  return messages
}

/**
 * Interprets a bulk dispatch preview and asks the user to confirm blocking or warning outcomes.
 * Returns ignore flags when the caller should proceed, or null when the user cancelled.
 */
export function confirmBulkDispatchPreview(
  preview: BulkDispatchPreviewResponse,
  confirm: (message: string) => boolean,
): BulkDispatchIgnoreFlags | null {
  const ignoreFlags = resolveBulkDispatchIgnoreFlags(preview.items)

  if (preview.summary.blockedCount > 0) {
    const confirmed = confirm(
      `${preview.summary.blockedCount} trip(s) have blocking conflicts (${formatBulkDispatchBlockedMessage(preview)}). Apply anyway?`,
    )
    if (!confirmed) {
      return null
    }

    return ignoreFlags
  }

  for (const message of collectBulkDispatchWarnings(preview.items)) {
    if (!confirm(message)) {
      return null
    }
  }

  return {
    ignoreAvailabilityConflicts: false,
    ignoreEligibilityBlocks: false,
    ignoreDispatchabilityBlocks: false,
    ignoreWorkflowGateBlocks: false,
  }
}

export function buildBulkDispatchPreviewResponse(
  items: BulkDispatchItemPreview[],
): BulkDispatchPreviewResponse {
  const blockedCount = items.filter((item) => !item.canApply).length
  return {
    summary: {
      total: items.length,
      canApplyCount: items.length - blockedCount,
      blockedCount,
    },
    items,
  }
}
