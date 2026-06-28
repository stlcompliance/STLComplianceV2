import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
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
    ReferenceSearchPicker: ({
      label,
      value,
      onChange,
      allowQuickCreate,
      testId,
    }: {
      label?: string
      value: string
      onChange: (value: string) => void
      allowQuickCreate?: boolean
      testId?: string
    }) => (
      <div data-testid={testId}>
        {label ? <span>{label}</span> : null}
        <p>{allowQuickCreate ? 'Quick create enabled' : 'Quick create disabled'}</p>
        <p>Selected asset: {value || 'none'}</p>
        <button type="button" onClick={() => onChange('asset-quick-created-1')}>
          Quick create asset
        </button>
      </div>
    ),
  }
})

vi.mock('../../api/client', async () => {
  const actual = await vi.importActual<typeof import('../../api/client')>('../../api/client')

  return {
    ...actual,
    createDefectDraft: vi.fn(),
    getAsset: vi.fn(),
    getAssetReadiness: vi.fn(),
    getCatalogs: vi.fn(),
    getDefect: vi.fn(),
    getDefectCreateFieldset: vi.fn(),
    getDefectEvidence: vi.fn(),
    getInspectionRun: vi.fn(),
    getMe: vi.fn(),
    getPeople: vi.fn(),
    getPmProgram: vi.fn(),
    getSites: vi.fn(),
    getWorkOrder: vi.fn(),
    previewDefectDraft: vi.fn(),
    searchAssets: vi.fn(),
    submitDefectDraft: vi.fn(),
    updateDefectDraft: vi.fn(),
    uploadDefectEvidence: vi.fn(),
  }
})

import {
  createDefectDraft,
  getAsset,
  getAssetReadiness,
  getCatalogs,
  getDefect,
  getDefectCreateFieldset,
  getDefectEvidence,
  getInspectionRun,
  getMe,
  getPeople,
  getPmProgram,
  getSites,
  getWorkOrder,
  previewDefectDraft,
  searchAssets,
  submitDefectDraft,
  updateDefectDraft,
  uploadDefectEvidence,
} from '../../api/client'
import { DefectCreatePage } from './DefectCreatePage'

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
  isPlatformAdmin: false,
}

const readyReadiness = {
  assetId: 'asset-quick-created-1',
  assetTag: 'TRK-900',
  assetName: 'Truck 900',
  lifecycleStatus: 'active',
  readinessStatus: 'ready',
  readinessBasis: 'maintenance_clear',
  calculatedAt: '2026-06-01T00:00:00Z',
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

const assetSearchResults = [
  {
    assetId: 'asset-quick-created-1',
    assetTypeId: 'type-truck',
    typeKey: 'truck',
    typeName: 'Truck',
    classKey: 'vehicle',
    className: 'Vehicle',
    assetTag: 'TRK-900',
    name: 'Truck 900',
    description: 'Quick-created asset',
    lifecycleStatus: 'active',
    siteRef: 'site-1',
    staffarrSiteOrgUnitId: 'site-1',
    staffarrSiteNameSnapshot: 'North Yard',
    openDefectCount: 0,
    openWorkOrderCount: 0,
    readinessStatus: 'ready',
    createdAt: '2026-06-01T00:00:00Z',
    updatedAt: '2026-06-01T00:00:00Z',
  },
]

const defectFieldset = {
  key: 'defects-create',
  label: 'Defect create',
  entityType: 'defect',
  purpose: 'create',
  fields: [],
}

const defectDraft = {
  defectId: 'defect-1',
  assetId: 'asset-quick-created-1',
  assetTag: 'TRK-900',
  assetName: 'Truck 900',
  inspectionRunId: null,
  checklistItemId: null,
  checklistItemKey: null,
  checklistItemPrompt: null,
  title: '',
  description: '',
  severity: 'medium',
  status: 'draft',
  source: 'manual',
  reportedByUserId: 'user-1',
  createdAt: '2026-06-01T00:00:00Z',
  updatedAt: '2026-06-01T00:00:00Z',
  resolvedAt: null,
  evidenceCount: 0,
  priority: 'normal',
  reportSource: 'manual',
  reportedByPersonId: 'person-100',
  discoveredByPersonId: 'person-100',
  reportedAt: '2026-06-01T00:00:00Z',
  discoveredAt: '2026-06-01T00:00:00Z',
  isSafetyCritical: false,
  isComplianceImpacting: false,
  isOperabilityImpacting: false,
  sourceType: 'manual',
  sourceReferenceId: null,
  incidentReferenceId: null,
  readinessNotes: null,
  correctiveAction: null,
}

const defectPreview = {
  defect: defectDraft,
  findings: [],
  duplicateMatches: [],
  assetReadiness: readyReadiness,
  canSubmit: false,
  canCreateWorkOrder: false,
  canMarkAssetNotReady: false,
}

const people = [
  {
    key: 'person-100',
    id: 'person-100',
    label: 'Dana Maintenance Admin',
    source: 'staffarr',
    sourceOfTruth: 'StaffArr',
    storedValue: 'person-100',
    displayValue: 'Dana Maintenance Admin',
    isActive: true,
  },
]

const sites = [
  {
    key: 'site-1',
    id: 'site-1',
    label: 'North Yard',
    source: 'staffarr',
    sourceOfTruth: 'StaffArr',
    storedValue: 'site-1',
    displayValue: 'North Yard',
    isActive: true,
  },
]

vi.mocked(createDefectDraft).mockResolvedValue(defectDraft as never)
vi.mocked(getAsset).mockResolvedValue({
  assetId: 'asset-quick-created-1',
  assetTypeId: 'type-truck',
  typeKey: 'truck',
  typeName: 'Truck',
  classKey: 'vehicle',
  className: 'Vehicle',
  assetTag: 'TRK-900',
  name: 'Truck 900',
  description: 'Quick-created asset',
  lifecycleStatus: 'active',
  siteRef: 'site-1',
  createdAt: '2026-06-01T00:00:00Z',
  updatedAt: '2026-06-01T00:00:00Z',
} as never)
vi.mocked(getAssetReadiness).mockResolvedValue(readyReadiness as never)
vi.mocked(getCatalogs).mockResolvedValue([] as never)
vi.mocked(getDefectCreateFieldset).mockResolvedValue(defectFieldset as never)
vi.mocked(getDefectEvidence).mockResolvedValue([] as never)
vi.mocked(getMe).mockResolvedValue({
  userId: 'user-1',
  personId: 'person-100',
  email: 'dana@example.test',
  displayName: 'Dana Maintenance Admin',
  tenantId: 'tenant-1',
  tenantRoleKey: 'tenant_admin',
  isPlatformAdmin: false,
  productKey: 'maintainarr',
  launchableProductKeys: ['maintainarr'],
} as never)
vi.mocked(getPeople).mockResolvedValue(people as never)
vi.mocked(getSites).mockResolvedValue(sites as never)
vi.mocked(previewDefectDraft).mockResolvedValue(defectPreview as never)
vi.mocked(searchAssets).mockResolvedValue(assetSearchResults as never)
vi.mocked(getDefect).mockResolvedValue({} as never)
vi.mocked(getInspectionRun).mockResolvedValue({} as never)
vi.mocked(getPmProgram).mockResolvedValue({} as never)
vi.mocked(getWorkOrder).mockResolvedValue({} as never)
vi.mocked(submitDefectDraft).mockResolvedValue({} as never)
vi.mocked(updateDefectDraft).mockResolvedValue({} as never)
vi.mocked(uploadDefectEvidence).mockResolvedValue({} as never)

function renderPage(path = '/defects/create') {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[path]}>
        <Routes>
          <Route path="/defects/create" element={<DefectCreatePage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('DefectCreatePage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
    sessionStorage.clear()
  })

  it('quick creates an asset reference from defect intake and auto-saves the draft', async () => {
    sessionStorage.setItem('stl.maintainarr.session', JSON.stringify(session))

    renderPage()

    const assetPicker = await screen.findByTestId('defect-asset-reference')
    expect(within(assetPicker).getByText('Quick create enabled')).toBeInTheDocument()

    fireEvent.click(within(assetPicker).getByRole('button', { name: 'Quick create asset' }))

    await waitFor(() => {
      expect(createDefectDraft).toHaveBeenCalledWith(
        'token-123',
        expect.objectContaining({
          assetId: 'asset-quick-created-1',
        }),
      )
    })

    expect(screen.getByRole('button', { name: 'Update Draft' })).toBeInTheDocument()
    expect(previewDefectDraft).toHaveBeenCalledWith('token-123', 'defect-1')
  })
})

