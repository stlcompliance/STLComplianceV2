import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { ReadinessPanel } from './ReadinessPanel'
import type { PersonReadinessResponse } from '../api/types'

const sampleReadiness: PersonReadinessResponse = {
  personId: '11111111-1111-1111-1111-111111111111',
  readinessStatus: 'not_ready',
  calculatedAt: '2026-05-27T12:00:00.000Z',
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
      certificationKey: 'readiness.safety_orientation',
      certificationName: 'Safety Orientation',
      blockerType: 'missing',
      message: 'Safety Orientation is required but has not been granted.',
    },
  ],
}

describe('ReadinessPanel', () => {
  it('renders readiness status, blockers, and requirements', () => {
    render(
      <ReadinessPanel
        personId={sampleReadiness.personId}
        personDisplayName="Alex Worker"
        readiness={sampleReadiness}
        isLoading={false}
      />,
    )

    expect(screen.getByText('Workforce readiness')).toBeTruthy()
    expect(screen.getByText('Not ready')).toBeTruthy()
    expect(screen.getByText(/Safety Orientation is required but has not been granted/i)).toBeTruthy()
    expect(screen.getByText('Missing')).toBeTruthy()
  })
})
