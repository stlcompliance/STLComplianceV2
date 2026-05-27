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

  it('renders TrainArr publication source label', () => {
    render(
      <CertificationPanel
        personId="person-1"
        personDisplayName="Alex"
        definitions={definitions}
        certifications={[
          {
            ...certifications[0],
            sourceType: 'trainarr_publication',
            externalPublicationId: 'pub-trainarr-1',
            notes: 'Granted via TrainArr assignment completion.',
          },
        ]}
        canManage={false}
        isSubmitting={false}
        errorMessage={null}
        onGrantCertification={vi.fn().mockResolvedValue(undefined)}
        onUpdateCertification={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByText(/TrainArr qualification/i)).toBeTruthy()
    expect(screen.getByText(/pub-trainarr-1/i)).toBeTruthy()
  })

  it('renders TrainArr lifecycle status for revoked publication grants', () => {
    render(
      <CertificationPanel
        personId="person-1"
        personDisplayName="Alex"
        definitions={definitions}
        certifications={[
          {
            ...certifications[0],
            sourceType: 'trainarr_publication',
            status: 'revoked',
            effectiveStatus: 'revoked',
            externalPublicationId: 'pub-trainarr-revoked',
            notes: 'TrainArr revoke: Qualification revoked after policy violation.',
          },
        ]}
        canManage={false}
        isSubmitting={false}
        errorMessage={null}
        onGrantCertification={vi.fn().mockResolvedValue(undefined)}
        onUpdateCertification={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByText(/TrainArr lifecycle: Revoked/i)).toBeTruthy()
    expect(screen.getByText(/pub-trainarr-revoked/i)).toBeTruthy()
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
