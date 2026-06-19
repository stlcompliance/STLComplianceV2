import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
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

const accidentRegisterFactDefinition: FactDefinitionResponse = {
  factDefinitionId: 'fd-2',
  factKey: 't49_accident_register_current',
  label: 'Accident register current',
  description: 'Recordable accidents are entered in the accident register with required details.',
  valueType: 'boolean',
  isActive: true,
  createdAt: '2026-05-27T00:00:00Z',
  updatedAt: '2026-05-27T00:00:00Z',
}

const calculatedFactDefinition: FactDefinitionResponse = {
  factDefinitionId: 'fd-3',
  factKey: 't49_driver_dq_file_complete',
  label: 'Driver DQ file complete',
  description: 'Driver qualification file is complete when all supporting document facts are valid.',
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

  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

  render(
    <QueryClientProvider client={queryClient}>
      <FactSourcesPanel
        factDefinitions={factDefinitions}
        factSources={factSources}
        canManage={canManage}
        onCreateFactSource={onCreateFactSource}
        onUpdateFactSource={onUpdateFactSource}
        isSavingFactSource={false}
      />
    </QueryClientProvider>,
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

    fireEvent.change(screen.getByLabelText('Source product'), {
      target: { value: 'staffarr' },
    })
    fireEvent.change(screen.getByLabelText('Product reference'), {
      target: { value: 'staffarr:record_type:person_application' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Create fact mapping' }))

    await waitFor(() => {
      expect(onCreateFactSource).toHaveBeenCalledWith({
        factDefinitionId: 'fd-1',
        sourceKey: 'manual_driver_license_valid',
        sourceType: 'static_config',
        label: 'Manual Valid driver license',
        description: 'Manual source mapping for driver_license_valid.',
        productKey: 'staffarr',
        productReference: 'staffarr:record_type:person_application',
        configJson: '{\n  "booleanValue": true\n}',
        priority: 0,
      })
    })
  })

  it('defaults accident register facts to a generated report mapping', async () => {
    const { onCreateFactSource } = renderPanel({ factDefinitions: [accidentRegisterFactDefinition] })

    expect(screen.getByDisplayValue('Report generated')).toHaveValue('report_generated')
    expect(screen.getByLabelText('Report reference')).toHaveValue('reportarr:report:rpt-001')
    expect(screen.getByText('Included event classes')).toBeInTheDocument()
    expect(screen.getByLabelText('Accident')).toBeChecked()

    fireEvent.change(screen.getByLabelText('Report reference'), {
      target: { value: 'reportarr:report:rpt-002' },
    })

    fireEvent.click(screen.getByRole('button', { name: 'Create fact mapping' }))

    await waitFor(() => {
      expect(onCreateFactSource).toHaveBeenCalledWith({
        factDefinitionId: 'fd-2',
        sourceKey: 'report_t49_accident_register_current',
        sourceType: 'report_generated',
        label: 'Generated Accident register current',
        description: 'Generated report mapping for t49_accident_register_current.',
        productKey: 'reportarr',
        productReference: 'reportarr:report:rpt-002',
        configJson: '{\n  "includedEventClasses": [\n    "accident"\n  ]\n}',
        priority: 0,
      })
    })
  })

  it('defaults DQ complete facts to a calculated mapping', () => {
    renderPanel({ factDefinitions: [calculatedFactDefinition] })

    expect(screen.getByDisplayValue('Calculated')).toHaveValue('calculated')
    expect(screen.getByLabelText('Calculation mode')).toHaveValue('all_true')
    expect(screen.getByText('Calculated prerequisites JSON')).toBeInTheDocument()
    expect(screen.getByLabelText('Product reference')).toHaveValue('compliancecore:calculation:fact_coverage')
    expect(screen.getByRole('option', { name: 'Fact coverage calculation - Compliance Core' })).toBeInTheDocument()
  })

  it('updates the calculated mode dropdown into config JSON', () => {
    renderPanel({ factDefinitions: [calculatedFactDefinition] })

    fireEvent.change(screen.getByLabelText('Calculation mode'), {
      target: { value: 'any_false' },
    })

    const configLabel = screen.getByText('Calculated prerequisites JSON')
    const configJson = configLabel.parentElement?.querySelector('textarea') as HTMLTextAreaElement | null
    expect(configJson).not.toBeNull()
    expect(configJson?.value).toContain('"calculationMode": "any_false"')
  })

  it('scopes product references to the selected source product', () => {
    renderPanel({ factDefinitions: [factDefinition] })

    fireEvent.change(screen.getByLabelText('Source product'), {
      target: { value: 'staffarr' },
    })

    const productReferenceSelect = screen.getByLabelText('Product reference')
    expect(productReferenceSelect).toBeEnabled()
    expect(screen.getByRole('option', { name: 'Person application record type - StaffArr' })).toBeInTheDocument()
    expect(screen.queryByRole('option', { name: 'Application document type - RecordArr' })).toBeNull()
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
