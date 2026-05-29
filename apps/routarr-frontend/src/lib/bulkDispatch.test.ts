import { describe, expect, it, vi } from 'vitest'

import {
  confirmBulkDispatchPreview,
  formatBulkDispatchBlockedMessage,
  formatBulkDispatchItemSummary,
  resolveBulkDispatchIgnoreFlags,
} from './bulkDispatch'
import type { BulkDispatchItemPreview, DispatchAssignmentPreviewResponse } from '../api/types'

function assignmentPreview(
  overrides: Partial<DispatchAssignmentPreviewResponse> = {},
): DispatchAssignmentPreviewResponse {
  return {
    tripId: 'trip-1',
    assignmentKind: 'driver',
    canAssign: true,
    hasBlockingConflicts: false,
    blockingDriverAvailability: [],
    blockingEquipmentAvailability: [],
    overlappingTrips: [],
    driverEligibility: null,
    assetDispatchability: null,
    workflowGates: null,
    ...overrides,
  }
}

function bulkItem(
  overrides: Partial<BulkDispatchItemPreview> = {},
): BulkDispatchItemPreview {
  return {
    tripId: 'trip-1',
    tripNumber: 'TR-1',
    title: 'North run',
    currentDispatchStatus: 'planned',
    canApply: true,
    hasBlockingConflicts: false,
    driverPreview: null,
    vehiclePreview: null,
    statusPreview: null,
    ...overrides,
  }
}

describe('formatBulkDispatchItemSummary', () => {
  it('includes workflow gate block messaging', () => {
    const summary = formatBulkDispatchItemSummary(
      bulkItem({
        canApply: false,
        hasBlockingConflicts: true,
        driverPreview: assignmentPreview({
          hasBlockingConflicts: true,
          workflowGates: {
            outcome: 'block',
            reasonCode: 'license_invalid',
            message: 'Driver license invalid',
            isBlocking: true,
            gates: [],
          },
        }),
      }),
    )

    expect(summary).toContain('Driver: Compliance workflow: Driver license invalid')
  })
})

describe('resolveBulkDispatchIgnoreFlags', () => {
  it('sets ignoreWorkflowGateBlocks when any preview has a blocking workflow gate', () => {
    const flags = resolveBulkDispatchIgnoreFlags([
      bulkItem({
        driverPreview: assignmentPreview({
          hasBlockingConflicts: true,
          workflowGates: {
            outcome: 'block',
            reasonCode: 'license_invalid',
            message: 'Driver license invalid',
            isBlocking: true,
            gates: [],
          },
        }),
      }),
    ])

    expect(flags).toEqual({
      ignoreAvailabilityConflicts: false,
      ignoreEligibilityBlocks: false,
      ignoreDispatchabilityBlocks: false,
      ignoreWorkflowGateBlocks: true,
    })
  })
})

describe('formatBulkDispatchBlockedMessage', () => {
  it('summarizes workflow gate blocks across items', () => {
    const message = formatBulkDispatchBlockedMessage({
      summary: { total: 1, canApplyCount: 0, blockedCount: 1 },
      items: [
        bulkItem({
          canApply: false,
          driverPreview: assignmentPreview({
            hasBlockingConflicts: true,
            workflowGates: {
              outcome: 'block',
              reasonCode: 'license_invalid',
              message: 'Driver license invalid',
              isBlocking: true,
              gates: [],
            },
          }),
        }),
      ],
    })

    expect(message).toContain('workflow gate block')
  })
})

describe('confirmBulkDispatchPreview', () => {
  it('returns null when user declines workflow gate override', () => {
    const confirm = vi.fn().mockReturnValue(false)
    const result = confirmBulkDispatchPreview(
      {
        summary: { total: 1, canApplyCount: 0, blockedCount: 1 },
        items: [
          bulkItem({
            canApply: false,
            driverPreview: assignmentPreview({
              hasBlockingConflicts: true,
              workflowGates: {
                outcome: 'block',
                reasonCode: 'license_invalid',
                message: 'Driver license invalid',
                isBlocking: true,
                gates: [],
              },
            }),
          }),
        ],
      },
      confirm,
    )

    expect(result).toBeNull()
    expect(confirm).toHaveBeenCalledOnce()
    expect(confirm.mock.calls[0]?.[0]).toContain('workflow gate block')
  })

  it('returns ignoreWorkflowGateBlocks when user accepts workflow gate override', () => {
    const confirm = vi.fn().mockReturnValue(true)
    const result = confirmBulkDispatchPreview(
      {
        summary: { total: 1, canApplyCount: 0, blockedCount: 1 },
        items: [
          bulkItem({
            canApply: false,
            driverPreview: assignmentPreview({
              hasBlockingConflicts: true,
              workflowGates: {
                outcome: 'block',
                reasonCode: 'license_invalid',
                message: 'Driver license invalid',
                isBlocking: true,
                gates: [],
              },
            }),
          }),
        ],
      },
      confirm,
    )

    expect(result).toEqual({
      ignoreAvailabilityConflicts: false,
      ignoreEligibilityBlocks: false,
      ignoreDispatchabilityBlocks: false,
      ignoreWorkflowGateBlocks: true,
    })
  })

  it('prompts on workflow gate warning when all items can apply', () => {
    const confirm = vi.fn().mockReturnValue(true)
    const result = confirmBulkDispatchPreview(
      {
        summary: { total: 1, canApplyCount: 1, blockedCount: 0 },
        items: [
          bulkItem({
            driverPreview: assignmentPreview({
              workflowGates: {
                outcome: 'warn',
                reasonCode: 'training_due',
                message: 'Training due soon',
                isBlocking: false,
                gates: [],
              },
            }),
          }),
        ],
      },
      confirm,
    )

    expect(result).not.toBeNull()
    expect(confirm).toHaveBeenCalledWith(
      'TR-1: Compliance workflow gate warning — Training due soon. Continue?',
    )
  })
})
