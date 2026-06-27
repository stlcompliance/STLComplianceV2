import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', async () => {
  const actual = await vi.importActual<typeof import('@stl/shared-ui')>('@stl/shared-ui')

  const Picker = ({
    label,
    value,
    options,
    onChange,
    placeholder,
    emptyLabel,
    testId,
    disabled,
    id,
  }: {
    label?: string
    value: string
    options: Array<{ value: string; label: string; inactive?: boolean }>
    onChange: (value: string) => void
    placeholder?: string
    emptyLabel?: string
    testId?: string
    disabled?: boolean
    id?: string
  }) => {
    const fieldId = id ?? testId
    return (
      <label htmlFor={fieldId} className="block">
        {label ? <span>{label}</span> : null}
        <select
          id={fieldId}
          aria-label={label ?? fieldId}
          data-testid={testId}
          value={value}
          disabled={disabled}
          onChange={(event) => onChange(event.target.value)}
        >
          <option value="">{emptyLabel ?? placeholder ?? 'Select…'}</option>
          {options.map((option) => (
            <option key={option.value} value={option.value} disabled={option.inactive}>
              {option.label}
            </option>
          ))}
        </select>
      </label>
    )
  }

  return {
    ...actual,
    PageHeader: ({ title, subtitle }: { title: string; subtitle?: string }) => (
      <header>
        <h1>{title}</h1>
        {subtitle ? <p>{subtitle}</p> : null}
      </header>
    ),
    ControlledSelect: Picker,
    StaticSearchPicker: Picker,
  }
})

vi.mock('../../api/client', async () => {
  const actual = await vi.importActual<typeof import('../../api/client')>('../../api/client')

  return {
    ...actual,
    checkDuplicateWorkOrderDraft: vi.fn(),
    createWorkOrderDraft: vi.fn(),
    getAssetReadiness: vi.fn(),
    getAssets: vi.fn(),
    getDefect: vi.fn(),
    getDefects: vi.fn(),
    getMe: vi.fn(),
    getPmSchedules: vi.fn(),
    getWorkOrderCreateFieldset: vi.fn(),
    openWorkOrderDraft: vi.fn(),
    previewWorkOrderDraft: vi.fn(),
    scheduleWorkOrderDraft: vi.fn(),
    startWorkOrderDraft: vi.fn(),
    updateWorkOrderDraft: vi.fn(),
    validateWorkOrderDraft: vi.fn(),
  }
})

import {
  checkDuplicateWorkOrderDraft,
  createWorkOrderDraft,
  getAssetReadiness,
  getAssets,
  getDefect,
  getDefects,
  getMe,
  getPmSchedules,
  getWorkOrderCreateFieldset,
  openWorkOrderDraft,
  previewWorkOrderDraft,
  scheduleWorkOrderDraft,
  startWorkOrderDraft,
  validateWorkOrderDraft,
} from '../../api/client'
import { WorkOrderCreatePage } from './WorkOrderCreatePage'

const session = {
  accessToken: 'token-123',
  accessTokenExpiresAt: '2026-06-01T00:00:00Z',
  userId: 'user-1',
  personId: 'person-100',
  tenantId: 'tenant-1',
  tenantSlug: 'demo',
  tenantDisplayName: 'Sparta Fleet',
  displayName: 'Dana Maintenance Admin',
  email: 'dana@example.test',
}

function option(key: string, label: string) {
  return {
    key,
    label,
    description: '',
    sortOrder: 1,
    parentOptionKey: null,
    isActive: true,
    dependency: null,
    metadata: null,
  }
}

function field(overrides: Record<string, unknown>) {
  return {
    key: '',
    label: '',
    description: '',
    type: 'string',
    control: 'text',
    required: false,
    catalogKey: null,
    referenceKey: null,
    source: 'maintainarr_record',
    sourceOfTruth: 'MaintainArr',
    storedValue: 'catalog_key',
    displayValue: 'catalog_label',
    allowCustom: false,
    customRequiresApproval: false,
    drivesLogic: false,
    drivesInspectionBranching: false,
    drivesPMApplicability: false,
    drivesCompliance: false,
    drivesReporting: false,
    drivesReadiness: false,
    dependsOn: null,
    validation: null,
    defaultValue: null,
    visibility: null,
    sectionKey: 'scope',
    options: null,
    ...overrides,
  }
}

const workOrderFieldset = {
  key: 'work-orders',
  label: 'Work Orders',
  entityType: 'work_order',
  purpose: 'create',
  fields: [
    field({
      key: 'workOrderType',
      label: 'Work Order Type',
      control: 'select',
      required: true,
      sectionKey: 'basics',
      catalogKey: 'workOrderType',
      defaultValue: 'corrective',
      options: [
        option('corrective', 'Corrective'),
        option('defect_repair', 'Defect Repair'),
        option('preventive', 'Preventive'),
        option('emergency', 'Emergency'),
      ],
    }),
    field({
      key: 'priority',
      label: 'Priority',
      control: 'select',
      required: true,
      sectionKey: 'basics',
      catalogKey: 'priority',
      defaultValue: 'medium',
      options: [option('low', 'Low'), option('medium', 'Medium'), option('high', 'High'), option('urgent', 'Urgent')],
    }),
    field({
      key: 'scopeSummary',
      label: 'Scope Summary',
      control: 'text',
      required: false,
      sectionKey: 'scope',
    }),
    field({
      key: 'failureMode',
      label: 'Failure Mode',
      control: 'select',
      sectionKey: 'classification',
      catalogKey: 'failureMode',
      options: [option('hydraulic', 'Hydraulic'), option('electrical', 'Electrical')],
    }),
    field({
      key: 'severity',
      label: 'Severity',
      control: 'select',
      sectionKey: 'classification',
      catalogKey: 'severity',
      options: [option('low', 'Low'), option('medium', 'Medium'), option('high', 'High')],
    }),
    field({
      key: 'repairDisposition',
      label: 'Repair Disposition',
      control: 'select',
      sectionKey: 'classification',
      catalogKey: 'repairDisposition',
      options: [option('repair', 'Repair'), option('inspect', 'Inspect')],
    }),
    field({
      key: 'rootCause',
      label: 'Root Cause',
      control: 'select',
      sectionKey: 'classification',
      catalogKey: 'rootCause',
      options: [option('wear', 'Wear'), option('impact', 'Impact')],
    }),
    field({
      key: 'assignedTechnicianPersonId',
      label: 'Assigned Technician',
      control: 'asyncCombobox',
      sectionKey: 'assignment',
      referenceKey: 'people',
      options: [option('person-tech-1', 'Taylor Tech')],
    }),
    field({
      key: 'siteId',
      label: 'Site',
      control: 'asyncCombobox',
      sectionKey: 'assignment',
      referenceKey: 'sites',
      options: [option('site-1', 'Dallas Yard')],
    }),
    field({
      key: 'departmentId',
      label: 'Department',
      control: 'asyncCombobox',
      sectionKey: 'assignment',
      referenceKey: 'departments',
      options: [option('dept-1', 'Fleet Maintenance')],
    }),
    field({
      key: 'teamId',
      label: 'Team',
      control: 'asyncCombobox',
      sectionKey: 'assignment',
      referenceKey: 'teams',
      options: [option('team-1', 'Night Shift')],
    }),
    field({
      key: 'governingBodyKey',
      label: 'Governing Body',
      control: 'select',
      sectionKey: 'readiness',
      referenceKey: 'compliancecore_reference',
      options: [option('dot', 'DOT')],
    }),
    field({
      key: 'documentType',
      label: 'Document Type',
      control: 'multiSelect',
      sectionKey: 'documents',
      catalogKey: 'documentType',
      options: [option('photo', 'Photo'), option('report', 'Report')],
    }),
    field({
      key: 'notes',
      label: 'Notes',
      control: 'text',
      sectionKey: 'documents',
    }),
  ],
} as const

const asset = {
  assetId: 'asset-1',
  assetTag: 'FL-100',
  name: 'Forklift 100',
  lifecycleStatus: 'active',
}

const defectSummary = {
  defectId: 'defect-1',
  assetId: asset.assetId,
  assetTag: asset.assetTag,
  assetName: asset.name,
  title: 'Hydraulic leak',
  severity: 'high',
  status: 'open',
  source: 'manual',
  reportedByUserId: 'user-2',
  createdAt: '2026-06-07T10:00:00Z',
  updatedAt: '2026-06-07T10:00:00Z',
  resolvedAt: null,
  evidenceCount: 0,
}

const defectDetail = {
  ...defectSummary,
  inspectionRunId: null,
  checklistItemId: null,
  checklistItemKey: null,
  title: 'Hydraulic leak repair',
  description: 'Hydraulic line leak detected near the mast.',
  downtimeFollowUp: null,
}

const pmSchedule = {
  pmScheduleId: 'pm-1',
  assetId: asset.assetId,
  scheduleKey: 'PM-100',
  name: 'Quarterly inspection',
  description: 'Quarterly preventive maintenance',
  dueStatus: 'due_today',
}

const readiness = {
  assetId: asset.assetId,
  assetTag: asset.assetTag,
  assetName: asset.name,
  lifecycleStatus: asset.lifecycleStatus,
  readinessStatus: 'ready',
  readinessBasis: 'maintenance_clear',
  calculatedAt: '2026-06-07T10:00:00Z',
  blockers: [],
  signals: {
    openCriticalDefectCount: 0,
    openHighDefectCount: 0,
    activeWorkOrderCount: 0,
    pmDueCount: 0,
    pmOverdueCount: 0,
    failedInspectionCount: 0,
  },
}

const blockedReadiness = {
  ...readiness,
  readinessStatus: 'not_ready',
  readinessBasis: 'maintenance_blockers',
  blockers: [
    {
      blockerType: 'active_work_order',
      message: 'An active blocker keeps this asset out of service.',
      sourceEntityType: 'work_order',
      sourceEntityId: 'wo-blocked-1',
      relatedEntityId: null,
    },
  ],
  signals: {
    ...readiness.signals,
    activeWorkOrderCount: 1,
  },
}

const validationFinding = {
  category: 'validation',
  severity: 'warning',
  code: 'work_order.title_review',
  message: 'Title is ready for review.',
  fieldKey: 'title',
  sectionKey: 'basics',
  source: 'maintainarr',
}

const duplicateMatch = {
  workOrderId: 'wo-dupe-1',
  workOrderNumber: 'WO-20260607-0002',
  title: 'Hydraulic leak repair',
  status: 'open',
  assetTag: asset.assetTag,
  assetName: asset.name,
  matchReason: 'Same asset and same defect source',
  similarityScore: 94,
}

const previewFinding = {
  category: 'compliance',
  severity: 'warning',
  code: 'work_order.preview_note',
  message: 'Preview completed successfully.',
  fieldKey: null,
  sectionKey: 'review',
  source: 'maintainarr',
}

function workOrderResponse(status: string) {
  return {
    workOrderId: 'wo-draft-1',
    workOrderNumber: 'WO-20260607-0001',
    assetId: asset.assetId,
    assetTag: asset.assetTag,
    assetName: asset.name,
    defectId: defectDetail.defectId,
    defectTitle: defectDetail.title,
    pmScheduleId: null,
    pmScheduleName: null,
    title: defectDetail.title,
    description: defectDetail.description,
    priority: 'high',
    status,
    source: 'defect',
    createdByUserId: session.userId,
    createdAt: '2026-06-07T10:00:00Z',
    updatedAt: '2026-06-07T10:05:00Z',
    startedAt: status === 'in_progress' ? '2026-06-07T10:05:00Z' : null,
    completedAt: null,
    cancelledAt: null,
    assignedTechnicianPersonId: null,
    draftPlanJson: JSON.stringify({ workOrderType: 'defect_repair' }),
    plannedStartAt: null,
    plannedDueAt: null,
    blockers: [],
    closeout: null,
    downtimeFollowUp: null,
  }
}

function previewResponse() {
  return {
    workOrder: workOrderResponse('draft'),
    findings: [previewFinding],
    duplicateMatches: [duplicateMatch],
    assetReadiness: readiness,
    canOpen: true,
    canSchedule: true,
    canStart: true,
  }
}

function LocationProbe() {
  const location = useLocation()
  return (
    <div data-testid="location-probe">
      {location.pathname}
      {location.search}
    </div>
  )
}

function renderPage(initialPath: string) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialPath]}>
        <Routes>
          <Route
            path="/work-orders/create"
            element={(
              <>
                <LocationProbe />
                <WorkOrderCreatePage />
              </>
            )}
          />
          <Route path="/work-orders/details" element={<LocationProbe />} />
          <Route path="/launch" element={<LocationProbe />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

function mockCommonResponses(source: 'defect' | 'schedule') {
  vi.mocked(getMe).mockResolvedValue({
    userId: session.userId,
    personId: session.personId,
    email: session.email,
    displayName: session.displayName,
    tenantId: session.tenantId,
    tenantRoleKey: 'tenant_admin',
    isPlatformAdmin: false,
    productKey: 'maintainarr',
    hasMaintainArrAccess: true,
    launchableProductKeys: ['maintainarr'],
  } as never)

  vi.mocked(getWorkOrderCreateFieldset).mockResolvedValue(workOrderFieldset as never)
  vi.mocked(getAssets).mockResolvedValue([asset] as never)
  vi.mocked(getPmSchedules).mockResolvedValue([pmSchedule] as never)
  vi.mocked(getAssetReadiness).mockResolvedValue(readiness as never)
  vi.mocked(getDefects).mockResolvedValue((source === 'defect' ? [defectSummary] : []) as never)
  vi.mocked(getDefect).mockResolvedValue(defectDetail as never)
  vi.mocked(createWorkOrderDraft).mockResolvedValue(workOrderResponse('draft') as never)
  vi.mocked(validateWorkOrderDraft).mockResolvedValue({
    isValid: true,
    findings: [validationFinding],
  } as never)
  vi.mocked(checkDuplicateWorkOrderDraft).mockResolvedValue([duplicateMatch] as never)
  vi.mocked(previewWorkOrderDraft).mockResolvedValue(previewResponse() as never)
  vi.mocked(openWorkOrderDraft).mockResolvedValue(workOrderResponse('open') as never)
  vi.mocked(scheduleWorkOrderDraft).mockResolvedValue(workOrderResponse('scheduled') as never)
  vi.mocked(startWorkOrderDraft).mockResolvedValue(workOrderResponse('in_progress') as never)
}

async function advanceToReview(stepCount = 9) {
  for (let index = 0; index < stepCount; index += 1) {
    fireEvent.click(screen.getByRole('button', { name: 'Next' }))
  }

  await screen.findByRole('button', { name: 'Preview' })
}

describe('WorkOrderCreatePage', () => {
  afterEach(() => {
    cleanup()
    sessionStorage.clear()
    vi.clearAllMocks()
  })

  it('hydrates query-param sources, gates progression, saves a draft, previews it, and opens the work order', async () => {
    sessionStorage.setItem('stl.maintainarr.session', JSON.stringify(session))
    mockCommonResponses('defect')

    renderPage('/work-orders/create?assetId=asset-1&defectId=defect-1')

    await waitFor(() => {
      expect(screen.getByTestId('work-order-asset-picker')).toHaveValue('asset-1')
      expect(screen.getByLabelText('Title *')).toHaveValue('Hydraulic leak repair')
      expect(screen.getByTestId('work-order-field-workOrderType')).toHaveValue('defect_repair')
      expect(screen.getByTestId('location-probe')).toHaveTextContent('/work-orders/create')
    }, { timeout: 5000 })

    fireEvent.change(screen.getByTestId('work-order-field-priority'), { target: { value: 'urgent' } })
    fireEvent.change(screen.getByLabelText('Title *'), { target: { value: '' } })
    expect(screen.getByRole('button', { name: 'Next' })).toBeDisabled()

    fireEvent.change(screen.getByLabelText('Title *'), { target: { value: 'Hydraulic leak repair' } })
    expect(screen.getByRole('button', { name: 'Next' })).toBeEnabled()

    fireEvent.click(screen.getByRole('button', { name: 'Next' }))
    await waitFor(() => {
      expect(screen.getByTestId('work-order-defect-picker')).toHaveValue('defect-1')
    })

    await advanceToReview(8)

    fireEvent.click(screen.getAllByRole('button', { name: 'Save draft' })[0])
    await waitFor(() => {
      expect(screen.getByTestId('location-probe')).toHaveTextContent('workOrderId=wo-draft-1')
      expect(screen.getByText('Draft wo-draft-1')).toBeInTheDocument()
    })

    await waitFor(() => {
      expect(screen.getByText('Server findings')).toBeInTheDocument()
      expect(screen.getByText('Potential duplicates')).toBeInTheDocument()
      expect(screen.getByText('Preview has not been run yet.')).toBeInTheDocument()
    })

    fireEvent.click(screen.getByRole('button', { name: 'Preview' }))

    await waitFor(() => {
      const emergencyPanel = screen.getByTestId('work-order-emergency-response-panel')
      expect(emergencyPanel).toBeInTheDocument()
      expect(within(emergencyPanel).getByText(/Start the emergency work order if the asset is unsafe or disabled/)).toBeInTheDocument()
      expect(screen.getByText('Emergency response')).toBeInTheDocument()
      expect(screen.getByText('Immediate containment')).toBeInTheDocument()
      expect(screen.getByText('Rapid dispatch')).toBeInTheDocument()
      expect(screen.getByText('Request triage')).toBeInTheDocument()
      expect(screen.getByText('Defect repair request')).toBeInTheDocument()
      expect(screen.getByText('Defect source')).toBeInTheDocument()
      expect(screen.getByText('Ready for routing')).toBeInTheDocument()
      expect(screen.getByText('Review the duplicate matches, then decide whether to merge or continue.')).toBeInTheDocument()
      expect(screen.getByText('Preview findings')).toBeInTheDocument()
      expect(screen.getByText(/Emergency path detected\./)).toBeInTheDocument()
      expect(screen.getByRole('button', { name: 'Start emergency work order' })).toBeInTheDocument()
      expect(screen.getByText(/work_order\.preview_note/)).toBeInTheDocument()
      expect(screen.getByText('Same asset and same defect source · Similarity 94%')).toBeInTheDocument()
    })

    fireEvent.click(screen.getByRole('button', { name: 'Open work order' }))

    await waitFor(() => {
      expect(screen.getByTestId('location-probe')).toHaveTextContent('/work-orders/details?workOrderId=wo-draft-1')
    })
    expect(createWorkOrderDraft).toHaveBeenCalledTimes(1)
    expect(openWorkOrderDraft).toHaveBeenCalledTimes(1)
  })

  it('promotes manual emergency breakdowns to an emergency work order type', async () => {
    sessionStorage.setItem('stl.maintainarr.session', JSON.stringify(session))
    mockCommonResponses('schedule')
    vi.mocked(getPmSchedules).mockResolvedValue([] as never)
    vi.mocked(getAssetReadiness).mockResolvedValue(blockedReadiness as never)
    let capturedDraftPlanJson: string | null = null
    vi.mocked(createWorkOrderDraft).mockImplementation(async (_accessToken, payload) => {
      capturedDraftPlanJson = payload.draftPlanJson
      return workOrderResponse('draft') as never
    })

    renderPage('/work-orders/create?assetId=asset-1')

    await waitFor(() => {
      expect(screen.getByTestId('work-order-field-workOrderType')).toHaveValue('emergency')
    }, { timeout: 5000 })

    fireEvent.change(screen.getByLabelText('Title *'), { target: { value: 'Roadside rescue' } })

    await advanceToReview()

    expect(screen.getByTestId('work-order-emergency-response-panel')).toBeInTheDocument()

    fireEvent.click(screen.getAllByRole('button', { name: 'Save draft' })[0])

    await waitFor(() => {
      expect(screen.getByTestId('location-probe')).toHaveTextContent('workOrderId=wo-draft-1')
    })

    expect(capturedDraftPlanJson).not.toBeNull()
    expect(JSON.parse(capturedDraftPlanJson!)).toMatchObject({ workOrderType: 'emergency' })
  }, 15000)

  it('can schedule a draft created from a PM schedule source', async () => {
    sessionStorage.setItem('stl.maintainarr.session', JSON.stringify(session))
    mockCommonResponses('schedule')

    renderPage('/work-orders/create?assetId=asset-1&pmScheduleId=pm-1')

    await waitFor(() => expect(screen.getByRole('button', { name: 'Next' })).toBeEnabled(), { timeout: 5000 })
    fireEvent.change(screen.getByLabelText('Title *'), { target: { value: 'Quarterly inspection work order' } })
    fireEvent.click(screen.getByRole('button', { name: 'Next' }))
    await waitFor(() => {
      expect(screen.getByTestId('work-order-pm-schedule-picker')).toHaveValue('pm-1')
    })

    fireEvent.click(screen.getAllByRole('button', { name: 'Save draft' })[0])
    await waitFor(() => expect(screen.getByTestId('location-probe')).toHaveTextContent('workOrderId=wo-draft-1'))

    await advanceToReview(8)
    fireEvent.click(screen.getByRole('button', { name: 'Preview' }))

    await waitFor(() => expect(screen.getByRole('button', { name: 'Schedule work order' })).toBeEnabled())
    fireEvent.click(screen.getByRole('button', { name: 'Schedule work order' }))

    await waitFor(() => {
      expect(screen.getByTestId('location-probe')).toHaveTextContent('/work-orders/details?workOrderId=wo-draft-1')
    })
    expect(scheduleWorkOrderDraft).toHaveBeenCalledTimes(1)
  })

  it('can start a draft after previewing it', async () => {
    sessionStorage.setItem('stl.maintainarr.session', JSON.stringify(session))
    mockCommonResponses('defect')

    renderPage('/work-orders/create?assetId=asset-1&defectId=defect-1')

    await waitFor(() => expect(screen.getByLabelText('Title *')).toHaveValue('Hydraulic leak repair'))
    fireEvent.click(screen.getAllByRole('button', { name: 'Save draft' })[0])
    await waitFor(() => expect(screen.getByTestId('location-probe')).toHaveTextContent('workOrderId=wo-draft-1'))

    await advanceToReview()
    fireEvent.click(screen.getByRole('button', { name: 'Preview' }))

    await waitFor(() => expect(screen.getByRole('button', { name: 'Start work order' })).toBeEnabled())
    fireEvent.click(screen.getByRole('button', { name: 'Start work order' }))

    await waitFor(() => {
      expect(screen.getByTestId('location-probe')).toHaveTextContent('/work-orders/details?workOrderId=wo-draft-1')
    })
    expect(startWorkOrderDraft).toHaveBeenCalledTimes(1)
  })
})

