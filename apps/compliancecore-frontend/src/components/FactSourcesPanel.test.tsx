import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import type {
  CreateFactSourceRequest,
  FactDefinitionResponse,
  FactSourceResponse,
  UpdateFactSourceRequest,
} from '../api/types'
import { listReportDefinitions } from '../api/reportarrClient'
import { FactSourcesPanel } from './FactSourcesPanel'

vi.mock('../api/reportarrClient', () => ({
  listReportDefinitions: vi.fn(),
}))

type LiveReportDefinition = Awaited<ReturnType<typeof listReportDefinitions>>[number]

const liveReportDefinitions = [
  {
    reportDefinitionId: 'rpt-001',
    reportNumber: 'RPT-001',
    reportKey: 'accident-register',
    title: 'Accident register',
    description: 'Live report definition.',
    reportType: 'compliance',
    status: 'draft',
    datasetRefs: [],
    readModelRefs: [],
    parameterRefs: [],
    defaultFilters: [],
    layoutDefinition: 'layout:grid:1x1',
    sectionRefs: [],
    exportFormats: ['pdf'],
    accessPolicyRef: '',
    ownerPersonId: 'person-live',
    createdAt: '2026-05-27T00:00:00Z',
    createdByPersonId: 'person-live',
    updatedAt: '2026-05-27T00:00:00Z',
    updatedByPersonId: 'person-live',
    tenantId: 'tenant-live',
  },
  {
    reportDefinitionId: 'rpt-002',
    reportNumber: 'RPT-002',
    reportKey: 'followup-summary',
    title: 'Follow-up summary',
    description: 'Second live report definition.',
    reportType: 'operational',
    status: 'draft',
    datasetRefs: [],
    readModelRefs: [],
    parameterRefs: [],
    defaultFilters: [],
    layoutDefinition: 'layout:grid:1x1',
    sectionRefs: [],
    exportFormats: ['pdf'],
    accessPolicyRef: '',
    ownerPersonId: 'person-live',
    createdAt: '2026-05-27T00:00:00Z',
    createdByPersonId: 'person-live',
    updatedAt: '2026-05-27T00:00:00Z',
    updatedByPersonId: 'person-live',
    tenantId: 'tenant-live',
  },
] as unknown as LiveReportDefinition[]

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
  accessToken?: string
  onCreateFactSource?: CreateFactSourceHandler
  onUpdateFactSource?: UpdateFactSourceHandler
} = {}) {
  const {
    factDefinitions = [],
    factSources = [],
    canManage = true,
    accessToken = '',
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
        accessToken={accessToken}
        onCreateFactSource={onCreateFactSource}
        onUpdateFactSource={onUpdateFactSource}
        isSavingFactSource={false}
      />
    </QueryClientProvider>,
  )

  return { onCreateFactSource, onUpdateFactSource }
}

describe('FactSourcesPanel', () => {
  it('loads report references from live report definitions', async () => {
    vi.mocked(listReportDefinitions).mockResolvedValueOnce(liveReportDefinitions)

    renderPanel({ factDefinitions: [accidentRegisterFactDefinition], accessToken: 'test-access-token' })

    await waitFor(() => {
      expect(screen.getByLabelText('Report reference')).toHaveValue('reportarr:report:rpt-001')
    })
    expect(screen.getByRole('option', { name: 'Accident register (accident-register) - ReportArr' })).toBeInTheDocument()
    expect(screen.getByRole('option', { name: 'Follow-up summary (followup-summary) - ReportArr' })).toBeInTheDocument()
  })

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
    fireEvent.click(screen.getByRole('button', { name: 'Create fact mapping' }))

    await waitFor(() => {
      expect(onCreateFactSource).toHaveBeenCalledWith({
        factDefinitionId: 'fd-1',
        sourceKey: 'manual_driver_license_valid',
        sourceType: 'static_config',
        label: 'Manual Valid driver license',
        description: 'Manual source mapping for driver_license_valid.',
        productKey: 'staffarr',
        productReference: null,
        configJson: '{\n  "booleanValue": true\n}',
        priority: 0,
      })
    })
  })

  it('defaults accident register facts to a generated report mapping', async () => {
    vi.mocked(listReportDefinitions).mockResolvedValueOnce(liveReportDefinitions)

    const { onCreateFactSource } = renderPanel({ factDefinitions: [accidentRegisterFactDefinition], accessToken: 'test-access-token' })

    expect(screen.getByDisplayValue('Report generated')).toHaveValue('report_generated')
    await waitFor(() => {
      expect(screen.getByLabelText('Report reference')).toHaveValue('reportarr:report:rpt-001')
    })
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
    expect(screen.getByText('Choose the calculation reference that describes how prerequisite facts roll up.')).toBeInTheDocument()
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

    expect(screen.getByText('Scoped to StaffArr and limited to record/document types.')).toBeInTheDocument()
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
