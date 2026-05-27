import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { VocabularyPanel } from './VocabularyPanel'

describe('VocabularyPanel', () => {
  it('renders vocabulary types count and empty terms state', () => {
    render(
      <VocabularyPanel
        types={[
          {
            typeKey: 'material_hazard',
            label: 'Material Hazard',
            description: 'Hazard classes',
            sortOrder: 10,
            isActive: true,
          },
        ]}
        terms={[]}
        complianceKeys={[]}
        materialKeys={[]}
        selectedTypeKey=""
        onSelectType={() => undefined}
        canManage={false}
        onCreateTerm={() => undefined}
        isCreatingTerm={false}
      />,
    )

    expect(screen.getByText(/All types \(1\)/)).toBeInTheDocument()
    expect(screen.getByText(/No vocabulary terms yet/)).toBeInTheDocument()
  })

  it('renders vocabulary term rows and key lists', () => {
    render(
      <VocabularyPanel
        types={[]}
        terms={[
          {
            termId: 'term-1',
            termKey: 'flammable',
            label: 'Flammable',
            vocabularyTypeKey: 'material_hazard',
            description: 'Can ignite under defined conditions.',
            isActive: true,
            aliases: ['Fire hazard'],
            createdAt: '2026-05-27T00:00:00Z',
          },
        ]}
        complianceKeys={[
          {
            complianceKeyId: 'ck-1',
            key: 'driver_qualification',
            label: 'Driver Qualification',
            category: 'compliance_domain',
            description: 'Driver qualification domain.',
            isActive: true,
            createdAt: '2026-05-27T00:00:00Z',
          },
        ]}
        materialKeys={[
          {
            materialKeyId: 'mk-1',
            key: 'gas',
            label: 'Gas',
            category: 'physical_state',
            description: 'Material exists as gas.',
            isActive: true,
            createdAt: '2026-05-27T00:00:00Z',
          },
        ]}
        selectedTypeKey="material_hazard"
        onSelectType={() => undefined}
        canManage={true}
        onCreateTerm={() => undefined}
        isCreatingTerm={false}
      />,
    )

    expect(screen.getByText('Flammable')).toBeInTheDocument()
    expect(screen.getByText('Driver Qualification')).toBeInTheDocument()
    expect(screen.getByText('Gas')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Add sample term' })).toBeInTheDocument()
  })
})
