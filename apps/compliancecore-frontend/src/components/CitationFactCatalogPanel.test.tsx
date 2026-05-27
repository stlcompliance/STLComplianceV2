import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { CitationFactCatalogPanel } from './CitationFactCatalogPanel'

describe('CitationFactCatalogPanel', () => {
  it('renders empty catalog states', () => {
    render(
      <CitationFactCatalogPanel
        citations={[]}
        factDefinitions={[]}
        factRequirements={[]}
        canManage={false}
        onSeedCatalog={() => undefined}
        isSeeding={false}
      />,
    )

    expect(screen.getByText(/No regulatory citations registered yet/)).toBeInTheDocument()
    expect(screen.getByText(/No fact definitions in the catalog yet/)).toBeInTheDocument()
    expect(screen.getByText(/No fact requirements linked/)).toBeInTheDocument()
  })

  it('renders catalog rows and seed action', () => {
    render(
      <CitationFactCatalogPanel
        citations={[
          {
            citationId: 'c-1',
            regulatoryProgramId: 'p-1',
            regulatoryProgramKey: 'fmcsa_safety',
            regulatoryProgramLabel: 'FMCSA Safety Compliance',
            rulePackId: 'rp-1',
            rulePackKey: 'driver_qualification',
            rulePackLabel: 'Driver Qualification Rules',
            citationKey: 'cfr_391_11',
            label: 'General qualifications of drivers',
            sourceReference: '49 CFR 391.11',
            description: 'General driver qualification requirements.',
            versionNumber: 1,
            supersedesCitationId: null,
            isActive: true,
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ]}
        factDefinitions={[
          {
            factDefinitionId: 'fd-1',
            factKey: 'driver_license_valid',
            label: 'Valid driver license',
            description: 'Driver holds a valid license.',
            valueType: 'boolean',
            isActive: true,
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ]}
        factRequirements={[
          {
            factRequirementId: 'fr-1',
            factDefinitionId: 'fd-1',
            factKey: 'driver_license_valid',
            factLabel: 'Valid driver license',
            rulePackId: 'rp-1',
            rulePackKey: 'driver_qualification',
            citationId: null,
            citationKey: null,
            requirementKey: 'dq_license_check',
            label: 'License validity check',
            description: 'Driver license must be valid for qualification.',
            isRequired: true,
            isActive: true,
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ]}
        canManage={true}
        onSeedCatalog={() => undefined}
        isSeeding={false}
      />,
    )

    expect(screen.getByText('General qualifications of drivers')).toBeInTheDocument()
    expect(screen.getByText('49 CFR 391.11')).toBeInTheDocument()
    expect(screen.getByText('Valid driver license')).toBeInTheDocument()
    expect(screen.getByText('License validity check')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Seed sample catalog' })).toBeInTheDocument()
  })
})
