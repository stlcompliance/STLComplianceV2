import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { ReadinessPanel } from './ReadinessPanel'
import type { PersonReadinessResponse } from '../api/types'

const sampleReadiness: PersonReadinessResponse = {
  personId: '11111111-1111-1111-1111-111111111111',
  readinessStatus: 'not_ready',
  readinessBasis: 'certifications',
  calculatedAt: '2026-05-27T12:00:00.000Z',
  sourceTimestamp: '2026-05-27T11:45:00.000Z',
  snapshotAgeMinutes: 15,
  snapshotFreshnessStatus: 'fresh',
  confidenceLevel: 'high',
  reasonCodes: ['certification_missing'],
  activeOverride: null,
  requirements: [
    {
      certificationDefinitionId: '22222222-2222-2222-2222-222222222222',
      certificationKey: 'readiness.safety_orientation',
      certificationName: 'Safety Orientation',
      requirementStatus: 'missing',
      recordEffectiveStatus: null,
      expiresAt: null,
    },
  ],
  blockers: [
    {
      blockerSource: 'certification',
      certificationKey: 'readiness.safety_orientation',
      certificationName: 'Safety Orientation',
      qualificationKey: null,
      qualificationName: null,
      blockerType: 'missing',
      message: 'Safety Orientation is required but has not been granted.',
    },
  ],
}

const trainingBlockerReadiness: PersonReadinessResponse = {
  ...sampleReadiness,
  readinessBasis: 'training_blockers',
  confidenceLevel: 'low',
  reasonCodes: ['training_blockers', 'training_overdue', 'certification_missing'],
  blockers: [
    {
      blockerSource: 'training',
      certificationKey: null,
      certificationName: null,
      qualificationKey: 'qual.hazmat_remediation',
      qualificationName: 'Hazmat Remediation',
      blockerType: 'overdue',
      message: 'Required hazmat remediation training is overdue.',
    },
    ...sampleReadiness.blockers,
  ],
}

const overrideReadiness: PersonReadinessResponse = {
  ...sampleReadiness,
  readinessStatus: 'ready',
  readinessBasis: 'manual_override',
  confidenceLevel: 'medium',
  reasonCodes: ['ready', 'manual_override', 'certification_missing'],
  activeOverride: {
    overrideId: '33333333-3333-3333-3333-333333333333',
    reason: 'Supervisor approved temporary assignment pending training completion.',
    grantedAt: '2026-05-27T10:00:00.000Z',
    expiresAt: null,
    grantedByUserId: '44444444-4444-4444-4444-444444444444',
  },
}

describe('ReadinessPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders readiness status, blockers, and requirements', () => {
    render(
      <ReadinessPanel
        personId={sampleReadiness.personId}
        personDisplayName="Alex Worker"
        readiness={sampleReadiness}
        isLoading={false}
        canOverride={false}
        isSubmittingOverride={false}
        overrideErrorMessage={null}
        onGrantOverride={async () => {}}
        onClearOverride={async () => {}}
      />,
    )

    expect(screen.getByText('Workforce readiness')).toBeTruthy()
    expect(screen.getByText('Not ready')).toBeTruthy()
    expect(screen.getByText('Missing requirements')).toBeTruthy()
    expect(screen.getAllByText('Safety Orientation').length).toBeGreaterThan(0)
    expect(screen.getByText(/Safety Orientation is required but has not been granted/i)).toBeTruthy()
    expect(screen.getAllByText('Missing').length).toBeGreaterThan(0)
    expect(screen.getByText(/certification_missing/i)).toBeTruthy()
    expect(screen.getByText(/15 min old/i)).toBeTruthy()
  })

  it('renders training blockers with a training label', () => {
    render(
      <ReadinessPanel
        personId={trainingBlockerReadiness.personId}
        personDisplayName="Alex Worker"
        readiness={trainingBlockerReadiness}
        isLoading={false}
        canOverride={false}
        isSubmittingOverride={false}
        overrideErrorMessage={null}
        onGrantOverride={async () => {}}
        onClearOverride={async () => {}}
      />,
    )

    expect(screen.getByText('Hazmat Remediation')).toBeTruthy()
    expect(screen.getByText('Training')).toBeTruthy()
    expect(screen.getByText(/Required hazmat remediation training is overdue/i)).toBeTruthy()
    expect(screen.getByText(/Low confidence/i)).toBeTruthy()
  })

  it('renders active override banner and clear action when authorized', () => {
    render(
      <ReadinessPanel
        personId={overrideReadiness.personId}
        personDisplayName="Alex Worker"
        readiness={overrideReadiness}
        isLoading={false}
        canOverride
        isSubmittingOverride={false}
        overrideErrorMessage={null}
        onGrantOverride={async () => {}}
        onClearOverride={async () => {}}
      />,
    )

    expect(screen.getByText(/Manual readiness override active/i)).toBeTruthy()
    expect(screen.getByText(/Supervisor approved temporary assignment/i)).toBeTruthy()
    expect(screen.getByRole('button', { name: /Clear override/i })).toBeTruthy()
    expect(screen.getByText(/Medium confidence/i)).toBeTruthy()
  })

  it('submits grant override when form is completed', async () => {
    const onGrantOverride = vi.fn().mockResolvedValue(undefined)

    render(
      <ReadinessPanel
        personId={sampleReadiness.personId}
        personDisplayName="Alex Worker"
        readiness={sampleReadiness}
        isLoading={false}
        canOverride
        isSubmittingOverride={false}
        overrideErrorMessage={null}
        onGrantOverride={onGrantOverride}
        onClearOverride={async () => {}}
      />,
    )

    fireEvent.change(screen.getByLabelText(/Reason/i), {
      target: { value: 'Emergency coverage approved by operations manager for 48 hours.' },
    })
    fireEvent.click(screen.getByRole('button', { name: /Grant readiness override/i }))

    expect(onGrantOverride).toHaveBeenCalledWith({
      reason: 'Emergency coverage approved by operations manager for 48 hours.',
      expiresAt: null,
    })
  })

  it('renders override errors in shared callout', () => {
    render(
      <ReadinessPanel
        personId={sampleReadiness.personId}
        personDisplayName="Alex Worker"
        readiness={sampleReadiness}
        isLoading={false}
        canOverride
        isSubmittingOverride={false}
        overrideErrorMessage="Override request denied"
        onGrantOverride={async () => {}}
        onClearOverride={async () => {}}
      />,
    )

    expect(screen.getByRole('alert')).toBeTruthy()
    expect(screen.getByText('Readiness override failed')).toBeTruthy()
    expect(screen.getByText('Override request denied')).toBeTruthy()
  })

  it('renders unavailable readiness state in shared callout', () => {
    render(
      <ReadinessPanel
        personId={sampleReadiness.personId}
        personDisplayName="Alex Worker"
        readiness={null}
        isLoading={false}
        canOverride={false}
        isSubmittingOverride={false}
        overrideErrorMessage={null}
        onGrantOverride={async () => {}}
        onClearOverride={async () => {}}
      />,
    )

    expect(screen.getByRole('alert')).toBeTruthy()
    expect(screen.getByText('Readiness summary unavailable')).toBeTruthy()
    expect(screen.getByText('Could not load readiness status for this person.')).toBeTruthy()
  })

  it('renders retryable read error callout when readiness query fails', () => {
    const onRetryRead = vi.fn()
    render(
      <ReadinessPanel
        personId={sampleReadiness.personId}
        personDisplayName="Alex Worker"
        readiness={null}
        isLoading={false}
        isError
        readErrorMessage="readiness backend unavailable"
        onRetryRead={onRetryRead}
        canOverride={false}
        isSubmittingOverride={false}
        overrideErrorMessage={null}
        onGrantOverride={async () => {}}
        onClearOverride={async () => {}}
      />,
    )

    expect(screen.getByText('Readiness summary unavailable')).toBeTruthy()
    expect(screen.getByText('readiness backend unavailable')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry readiness' }))
    expect(onRetryRead).toHaveBeenCalledTimes(1)
  })
})
