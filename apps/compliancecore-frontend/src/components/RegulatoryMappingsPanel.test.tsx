import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { RegulatoryMappingsPanel } from './RegulatoryMappingsPanel'

describe('RegulatoryMappingsPanel', () => {
  it('renders empty mappings state', () => {
    render(
      <RegulatoryMappingsPanel
        mappings={[]}
      />,
    )

    expect(screen.getByText(/No regulatory mappings registered yet/)).toBeInTheDocument()
  })

  it('renders mapping rows and seed action', () => {
    render(
      <RegulatoryMappingsPanel
        mappings={[
          {
            regulatoryMappingId: 'rm-1',
            mappingKey: 'dq_vehicle_inspection',
            label: 'Vehicle inspection under driver qualification',
            description: 'Maps vehicle inspection compliance key to FMCSA driver qualification.',
            targetKind: 'compliance_key',
            regulatoryProgramId: 'p-1',
            regulatoryProgramKey: 'fmcsa_safety',
            regulatoryProgramLabel: 'FMCSA Safety Compliance',
            rulePackId: 'rp-1',
            rulePackKey: 'driver_qualification',
            rulePackLabel: 'Driver Qualification Rules',
            citationId: null,
            citationKey: null,
            factDefinitionId: null,
            factKey: null,
            complianceKeyId: 'ck-1',
            complianceKey: 'vehicle_inspection',
            materialKeyId: null,
            materialKey: null,
            isActive: true,
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ]}
      />,
    )

    expect(screen.getByText('Vehicle inspection under driver qualification')).toBeInTheDocument()
    expect(screen.getByText('vehicle_inspection')).toBeInTheDocument()
    expect(screen.getByText(/FMCSA Safety Compliance/)).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Seed sample mapping' })).not.toBeInTheDocument()
  })
})
