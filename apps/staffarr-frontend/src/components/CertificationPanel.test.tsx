import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { CertificationPanel } from './CertificationPanel'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const mod = await importOriginal<typeof import('@stl/shared-ui')>()
  return {
    ...mod,
    StaticSearchPicker: ({
      value,
      onChange,
      options,
      testId,
      placeholder,
    }: {
      value: string
      onChange: (value: string) => void
      options: Array<{ value: string; label: string }>
      testId?: string
      placeholder?: string
    }) => (
      <label>
        {placeholder}
        <input
          data-testid={testId}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        />
        <ul>
          {options.map((option) => (
            <li key={option.value}>{option.label}</li>
          ))}
        </ul>
      </label>
    ),
  }
})

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
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isSubmitting={false}
        actionErrorMessage={null}
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
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isSubmitting={false}
        actionErrorMessage={null}
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
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isSubmitting={false}
        actionErrorMessage={null}
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

  it('submits grants using the searchable certification definition picker', async () => {
    const onGrantCertification = vi.fn().mockResolvedValue(undefined)

    render(
      <CertificationPanel
        personId="person-1"
        personDisplayName="Alex"
        definitions={definitions}
        certifications={[]}
        canManage
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isSubmitting={false}
        actionErrorMessage={null}
        onGrantCertification={onGrantCertification}
        onUpdateCertification={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    fireEvent.change(screen.getByTestId('certification-grant-definition'), {
      target: { value: 'def-1' },
    })
    fireEvent.click(screen.getByRole('button', { name: /Grant manual certification/i }))

    expect(onGrantCertification).toHaveBeenCalledWith({
      certificationDefinitionId: 'def-1',
      grantedAt: null,
      expiresAt: null,
      notes: null,
    })
  })

  it('renders action errors in shared callout', () => {
    render(
      <CertificationPanel
        personId="person-1"
        personDisplayName="Alex"
        definitions={definitions}
        certifications={certifications}
        canManage
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        isSubmitting={false}
        actionErrorMessage="Forbidden by policy"
        onGrantCertification={vi.fn().mockResolvedValue(undefined)}
        onUpdateCertification={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByRole('alert')).toBeTruthy()
    expect(screen.getByText('Certification update failed')).toBeTruthy()
    expect(screen.getByText('Forbidden: Forbidden by policy')).toBeTruthy()
  })

  it('renders retryable read error callout', () => {
    const onRetryRead = vi.fn()
    render(
      <CertificationPanel
        personId="person-1"
        personDisplayName="Alex"
        definitions={[]}
        certifications={[]}
        canManage
        isLoading={false}
        isError
        readErrorMessage="certification reads failed"
        onRetryRead={onRetryRead}
        isSubmitting={false}
        actionErrorMessage={null}
        onGrantCertification={vi.fn().mockResolvedValue(undefined)}
        onUpdateCertification={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    expect(screen.getByText('Certifications unavailable')).toBeTruthy()
    expect(screen.getByText('certification reads failed')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry certifications' }))
    expect(onRetryRead).toHaveBeenCalledTimes(1)
  })
})
