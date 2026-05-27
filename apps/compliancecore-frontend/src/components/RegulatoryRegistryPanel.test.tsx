import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { RegulatoryRegistryPanel } from './RegulatoryRegistryPanel'

describe('RegulatoryRegistryPanel', () => {
  it('renders empty registry states', () => {
    render(
      <RegulatoryRegistryPanel
        governingBodies={[]}
        jurisdictions={[]}
        programs={[]}
        rulePacks={[]}
        canManage={false}
        onSeedRegistry={() => undefined}
        isSeeding={false}
        onAdvanceRulePack={() => undefined}
        isAdvancingRulePack={false}
      />,
    )

    expect(screen.getByText(/No governing bodies registered yet/)).toBeInTheDocument()
    expect(screen.getByText(/No rule packs defined yet/)).toBeInTheDocument()
  })

  it('renders registry rows and rule pack status actions', () => {
    render(
      <RegulatoryRegistryPanel
        governingBodies={[
          {
            governingBodyId: 'gb-1',
            bodyKey: 'dot',
            label: 'U.S. DOT',
            description: 'Federal transportation authority.',
            isActive: true,
            createdAt: '2026-05-27T00:00:00Z',
          },
        ]}
        jurisdictions={[
          {
            jurisdictionId: 'j-1',
            governingBodyId: 'gb-1',
            governingBodyKey: 'dot',
            governingBodyLabel: 'U.S. DOT',
            jurisdictionKey: 'us_federal',
            label: 'United States Federal',
            description: 'Federal jurisdiction.',
            isActive: true,
            createdAt: '2026-05-27T00:00:00Z',
          },
        ]}
        programs={[
          {
            regulatoryProgramId: 'p-1',
            jurisdictionId: 'j-1',
            jurisdictionKey: 'us_federal',
            jurisdictionLabel: 'United States Federal',
            programKey: 'fmcsa_safety',
            label: 'FMCSA Safety Compliance',
            description: 'Safety program.',
            isActive: true,
            createdAt: '2026-05-27T00:00:00Z',
          },
        ]}
        rulePacks={[
          {
            rulePackId: 'rp-1',
            regulatoryProgramId: 'p-1',
            regulatoryProgramKey: 'fmcsa_safety',
            regulatoryProgramLabel: 'FMCSA Safety Compliance',
            packKey: 'driver_qualification',
            label: 'Driver Qualification Rules',
            description: 'Baseline driver qualification rules.',
            versionNumber: 1,
            status: 'draft',
            isActive: true,
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ]}
        canManage={true}
        onSeedRegistry={() => undefined}
        isSeeding={false}
        onAdvanceRulePack={() => undefined}
        isAdvancingRulePack={false}
      />,
    )

    expect(screen.getAllByText('U.S. DOT').length).toBeGreaterThan(0)
    expect(screen.getByText('FMCSA Safety Compliance')).toBeInTheDocument()
    expect(screen.getByText('Driver Qualification Rules')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Seed sample registry' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Submit for review' })).toBeInTheDocument()
  })
})
