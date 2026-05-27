import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { CertificationPanel } from './CertificationPanel'

const definitions = [
  {
    certificationDefinitionId: 'def-1',
    certificationKey: 'readiness.safety_orientation',
    name: 'Safety Orientation',
    description: 'Baseline safety orientation.',
    category: 'readiness',
    defaultValidityDays: 365,
    status: 'active' as const,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
]

const certifications = [
  {
    personCertificationId: 'cert-1',
    personId: 'person-1',
    certificationDefinitionId: 'def-1',
    certificationKey: 'readiness.safety_orientation',
    certificationName: 'Safety Orientation',
    category: 'readiness',
    sourceType: 'manual',
    status: 'active' as const,
    effectiveStatus: 'active' as const,
    grantedAt: new Date().toISOString(),
    expiresAt: new Date(Date.now() + 86400000).toISOString(),
    notes: 'Manual grant for onboarding.',
    grantedByUserId: 'actor-1',
    externalPublicationId: null,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
]

describe('CertificationPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders certification records and readiness catalog entries', () => {
    render(
      <CertificationPanel
        personId="person-1"
        personDisplayName="Alex"
        definitions={definitions}
        certifications={certifications}
        canManage
        isSubmitting={false}
        errorMessage={null}
        onGrantCertification={vi.fn().mockResolvedValue(undefined)}
        onUpdateCertification={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByText('Certifications for Alex')).toBeTruthy()
    expect(screen.getAllByText('Safety Orientation').length).toBeGreaterThan(0)
    expect(screen.getByText(/Manual grant for onboarding/i)).toBeTruthy()
    expect(screen.getByText('Readiness catalog')).toBeTruthy()
    expect(screen.getAllByText(/readiness.safety_orientation/i).length).toBeGreaterThan(0)
  })
})
