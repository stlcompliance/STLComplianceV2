import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import type {
  CreateFactSourceRequest,
  FactDefinitionResponse,
  FactSourceResponse,
  UpdateFactSourceRequest,
} from '../api/types'
import { FactSourcesPanel } from './FactSourcesPanel'

afterEach(() => {
  cleanup()
})

const factDefinition: FactDefinitionResponse = {
  factDefinitionId: 'fd-1',
  factKey: 'driver_license_valid',
  label: 'Valid driver license',
  description: 'Driver holds a valid license.',
  valueType: 'boolean',
  isActive: true,
  createdAt: '2026-05-27T00:00:00Z',
  updatedAt: '2026-05-27T00:00:00Z',
}

const factSource: FactSourceResponse = {
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
}

type CreateFactSourceHandler = (payload: CreateFactSourceRequest) => unknown
type UpdateFactSourceHandler = (factSourceId: string, payload: UpdateFactSourceRequest) => unknown

function renderPanel(options: {
  factDefinitions?: FactDefinitionResponse[]
  factSources?: FactSourceResponse[]
  canManage?: boolean
  onCreateFactSource?: CreateFactSourceHandler
  onUpdateFactSource?: UpdateFactSourceHandler
} = {}) {
  const {
    factDefinitions = [],
    factSources = [],
    canManage = true,
    onCreateFactSource = vi.fn<CreateFactSourceHandler>(),
    onUpdateFactSource = vi.fn<UpdateFactSourceHandler>(),
  } = options

  render(
    <FactSourcesPanel
      factDefinitions={factDefinitions}
      factSources={factSources}
      canManage={canManage}
      onCreateFactSource={onCreateFactSource}
      onUpdateFactSource={onUpdateFactSource}
      isSavingFactSource={false}
    />,
  )

  return { onCreateFactSource, onUpdateFactSource }
}

describe('FactSourcesPanel', () => {
  it('renders empty source states', () => {
    renderPanel({ canManage: false })

    expect(screen.getByText(/No fact definitions yet/)).toBeInTheDocument()
    expect(screen.getByText(/No fact sources registered yet/)).toBeInTheDocument()
    expect(screen.getByText(/Fact mappings are read-only/)).toBeInTheDocument()
  })

  it('creates a manual fact mapping', async () => {
    const { onCreateFactSource } = renderPanel({ factDefinitions: [factDefinition] })

    fireEvent.click(screen.getByRole('button', { name: 'Create fact mapping' }))

    await waitFor(() => {
      expect(onCreateFactSource).toHaveBeenCalledWith({
        factDefinitionId: 'fd-1',
        sourceKey: 'manual_driver_license_valid',
        sourceType: 'static_config',
        label: 'Manual Valid driver license',
        description: 'Manual source mapping for driver_license_valid.',
        productKey: null,
        productReference: null,
        configJson: '{\n  "booleanValue": true\n}',
        priority: 0,
      })
    })
  })

  it('renders source rows and edits a fact mapping', async () => {
    const { onUpdateFactSource } = renderPanel({
      factDefinitions: [factDefinition],
      factSources: [factSource],
    })

    expect(screen.getByText('Default license valid')).toBeInTheDocument()
    expect(screen.getByText('default_license_flag')).toBeInTheDocument()
    expect(screen.getAllByText('Static config').length).toBeGreaterThan(0)

    fireEvent.click(screen.getByRole('button', { name: 'Edit fact mapping Default license valid' }))
    fireEvent.change(screen.getByLabelText('Mapping label'), {
      target: { value: 'Manual license override' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Save fact mapping' }))

    await waitFor(() => {
      expect(onUpdateFactSource).toHaveBeenCalledWith('fs-1', {
        label: 'Manual license override',
        description: 'Static default for resolve.',
        productKey: null,
        productReference: null,
        configJson: '{"booleanValue":true}',
        priority: 0,
        isActive: true,
      })
    })
  })
})
