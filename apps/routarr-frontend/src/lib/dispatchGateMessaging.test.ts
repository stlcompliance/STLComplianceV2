import { describe, expect, it } from 'vitest'

import {
  buildDispatchAssignmentGateLines,
  formatDispatchAssignmentGateConfirmMessage,
} from './dispatchGateMessaging'
import type { DispatchAssignmentPreviewResponse } from '../api/types'

function emptyPreview(
  overrides: Partial<DispatchAssignmentPreviewResponse> = {},
): DispatchAssignmentPreviewResponse {
  return {
    tripId: 'trip-1',
    assignmentKind: 'driver',
    canAssign: false,
    hasBlockingConflicts: true,
    blockingDriverAvailability: [],
    blockingEquipmentAvailability: [],
    overlappingTrips: [],
    driverEligibility: null,
    assetDispatchability: null,
    workflowGates: null,
    ...overrides,
  }
}

describe('buildDispatchAssignmentGateLines', () => {
  it('expands workflow gate results with per-gate keys', () => {
    const lines = buildDispatchAssignmentGateLines(
      emptyPreview({
        workflowGates: {
          outcome: 'block',
          reasonCode: 'license_invalid',
          message: 'Driver license invalid',
          isBlocking: true,
          gates: [
            {
              gateKey: 'driver_qualification',
              outcome: 'block',
              reasonCode: 'license_invalid',
              message: 'Driver license invalid',
              isBlocking: true,
            },
          ],
        },
      }),
    )

    expect(lines).toHaveLength(1)
    expect(lines[0]?.label).toBe('driver_qualification')
    expect(lines[0]?.detail).toContain('Driver license invalid')
  })

  it('includes eligibility and dispatchability warnings', () => {
    const lines = buildDispatchAssignmentGateLines(
      emptyPreview({
        hasBlockingConflicts: false,
        driverEligibility: {
          outcome: 'warn',
          reasonCode: 'training_due',
          message: 'Training due soon',
          isBlocking: false,
          trainArr: null,
          staffArr: null,
        },
        assetDispatchability: {
          outcome: 'warn',
          reasonCode: 'pm_due',
          message: 'PM overdue',
          isBlocking: false,
          maintainArr: null,
        },
      }),
    )

    expect(lines.map((line) => line.category)).toEqual(['eligibility', 'dispatchability'])
  })

  it('includes missing/stale external data warnings from conflict summary', () => {
    const lines = buildDispatchAssignmentGateLines(
      emptyPreview({
        hasBlockingConflicts: false,
        conflictSummary: {
          driverAvailabilityBlocks: 0,
          equipmentAvailabilityBlocks: 0,
          overlappingTrips: 0,
          eligibilityBlocking: false,
          eligibilityWarning: false,
          dispatchabilityBlocking: false,
          dispatchabilityWarning: false,
          workflowGateBlocking: false,
          workflowGateWarning: true,
          hasMissingExternalData: true,
          hasStaleExternalData: true,
        },
      }),
    )

    expect(lines.some((line) => line.reasonCode === 'external_data_unavailable')).toBe(true)
    expect(lines.some((line) => line.reasonCode === 'external_data_stale')).toBe(true)
  })
})

describe('formatDispatchAssignmentGateConfirmMessage', () => {
  it('lists structured gate lines in confirm text', () => {
    const message = formatDispatchAssignmentGateConfirmMessage(
      emptyPreview({
        workflowGates: {
          outcome: 'block',
          reasonCode: 'license_invalid',
          message: 'Driver license invalid',
          isBlocking: true,
          gates: [],
        },
      }),
    )

    expect(message).toContain('Assignment blocked')
    expect(message).toContain('Driver license invalid')
    expect(message).toContain('Assign anyway?')
  })
})
