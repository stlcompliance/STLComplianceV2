import { describe, expect, it, vi } from 'vitest'

import {
  confirmDispatchAssignmentPreview,
  formatAssignmentConflictMessage,
} from './dispatchAssignment'
import type { DispatchAssignmentPreviewResponse } from '../api/types'

function emptyPreview(
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

function eligibilitySummary(
  overrides: Partial<DispatchAssignmentPreviewResponse['driverEligibility']> = {},
) {
  return {
    outcome: 'block' as const,
    reasonCode: 'cdl_expired',
    message: 'CDL expired',
    isBlocking: true,
    trainArr: null,
    staffArr: null,
    ...overrides,
  }
}

describe('confirmDispatchAssignmentPreview', () => {
  it('returns null when user declines blocking conflicts', () => {
    const confirm = vi.fn().mockReturnValue(false)
    const result = confirmDispatchAssignmentPreview(
      emptyPreview({
        hasBlockingConflicts: true,
        blockingDriverAvailability: [
          {
            availabilityId: 'a1',
            availabilityStatus: 'unavailable',
            startsAt: new Date().toISOString(),
            endsAt: new Date().toISOString(),
            reason: 'PTO',
          },
        ],
      }),
      confirm,
    )
    expect(result).toBeNull()
    expect(confirm).toHaveBeenCalledOnce()
  })

  it('returns ignore flags when user accepts blocking conflicts', () => {
    const confirm = vi.fn().mockReturnValue(true)
    const result = confirmDispatchAssignmentPreview(
      emptyPreview({
        hasBlockingConflicts: true,
        driverEligibility: eligibilitySummary(),
      }),
      confirm,
    )
    expect(result).toEqual({
      ignoreConflicts: false,
      ignoreEligibilityBlocks: true,
      ignoreDispatchabilityBlocks: false,
      ignoreWorkflowGateBlocks: false,
    })
  })

  it('prompts on eligibility warning when no blocking conflicts', () => {
    const confirm = vi.fn().mockReturnValue(true)
    const result = confirmDispatchAssignmentPreview(
      emptyPreview({
        driverEligibility: eligibilitySummary({
          outcome: 'warn',
          isBlocking: false,
          message: 'Training due soon',
        }),
      }),
      confirm,
    )
    expect(result).not.toBeNull()
    expect(confirm).toHaveBeenCalledWith(
      'Driver eligibility warning: Training due soon. Continue assignment?',
    )
  })
})

describe('formatAssignmentConflictMessage', () => {
  it('summarizes availability blocks', () => {
    const message = formatAssignmentConflictMessage(
      emptyPreview({
        blockingDriverAvailability: [
          {
            availabilityId: 'a1',
            availabilityStatus: 'unavailable',
            startsAt: new Date().toISOString(),
            endsAt: new Date().toISOString(),
            reason: 'PTO',
          },
        ],
      }),
    )
    expect(message).toContain('Driver availability')
    expect(message).toContain('PTO')
  })
})
