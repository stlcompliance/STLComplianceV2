import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { FactSourcesPanel } from './FactSourcesPanel'

describe('FactSourcesPanel', () => {
  it('renders empty source states', () => {
    render(
      <FactSourcesPanel
        factDefinitions={[]}
        factSources={[]}
      />,
    )

    expect(screen.getByText(/No fact definitions yet/)).toBeInTheDocument()
    expect(screen.getByText(/No fact sources registered yet/)).toBeInTheDocument()
  })

  it('renders source rows and seed action', () => {
    render(
      <FactSourcesPanel
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
        factSources={[
          {
            factSourceId: 'fs-1',
            factDefinitionId: 'fd-1',
            factKey: 'driver_license_valid',
            factLabel: 'Valid driver license',
            sourceKey: 'default_license_flag',
            sourceType: 'static_config',
            label: 'Default license valid',
            description: 'Static default for resolve.',
            productKey: null,
            productReference: null,
            configJson: '{"booleanValue":true}',
            priority: 0,
            isActive: true,
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ]}
      />,
    )

    expect(screen.getByText('Default license valid')).toBeInTheDocument()
    expect(screen.getByText('default_license_flag')).toBeInTheDocument()
    expect(screen.getByText('static_config')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Seed sample sources' })).not.toBeInTheDocument()
  })
})
