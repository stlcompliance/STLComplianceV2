import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', async () => {
  const actual = await vi.importActual<typeof import('@stl/shared-ui')>('@stl/shared-ui')

  return {
    ...actual,
    PageHeader: ({ title, subtitle }: { title: string; subtitle?: string }) => (
      <header>
        <h1>{title}</h1>
        {subtitle ? <p>{subtitle}</p> : null}
      </header>
    ),
    DetailBadge: ({ label }: { label: string }) => <span>{label}</span>,
    ControlledSelect: ({
      label,
      value,
      options,
      onChange,
      placeholder,
      emptyLabel,
      testId,
      disabled,
    }: {
      label: string
      value: string
      options: Array<{ value: string; label: string; inactive?: boolean }>
      onChange: (value: string) => void
      placeholder?: string
      emptyLabel?: string
      testId?: string
      disabled?: boolean
    }) => (
      <label>
        <span>{label}</span>
        <select
          aria-label={label}
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
    ),
    StaticSearchPicker: ({
      label,
      value,
      options,
      onChange,
      placeholder,
      testId,
      disabled,
    }: {
      label: string
      value: string
      options: Array<{ value: string; label: string; inactive?: boolean }>
      onChange: (value: string) => void
      placeholder?: string
      testId?: string
      disabled?: boolean
    }) => (
      <label>
        <span>{label}</span>
        <select
          aria-label={label}
          data-testid={testId}
          value={value}
          disabled={disabled}
          onChange={(event) => onChange(event.target.value)}
        >
          <option value="">{placeholder ?? 'Select…'}</option>
          {options.map((option) => (
            <option key={option.value} value={option.value} disabled={option.inactive}>
              {option.label}
            </option>
          ))}
        </select>
      </label>
    ),
    ReferenceProviderClient: class ReferenceProviderClient {
      constructor(_options: unknown) {}
    },
    ReferencePicker: ({
      label,
      value,
      onChange,
      placeholder,
      testId,
      disabled,
    }: {
      label?: string
      value: { displayLabelSnapshot?: string } | null
      onChange: (value: {
        referenceId: string
        displayLabelSnapshot: string
        ownerProductKey: string
        referenceType: string
        statusSnapshot?: string | null
      } | null) => void
      placeholder?: string
      testId?: string
      disabled?: boolean
    }) => (
      <div>
        <label>
          {label ? <span>{label}</span> : null}
          <input
            aria-label={label ?? placeholder ?? 'Reference picker'}
            data-testid={testId}
            disabled={disabled}
            value={value?.displayLabelSnapshot ?? ''}
            readOnly
          />
        </label>
        <button
          type="button"
          disabled={disabled}
          onClick={() =>
            onChange({
              referenceId: 'site-quick-created-1',
              displayLabelSnapshot: 'North Yard',
              ownerProductKey: 'staffarr',
              referenceType: 'site',
              statusSnapshot: 'planned',
            })
          }
        >
          Quick create site
        </button>
        <button type="button" disabled={disabled} onClick={() => onChange(null)}>
          Clear site
        </button>
      </div>
    ),
    GeneratedKeyField: ({
      label,
      generatedKey,
      manualOverride,
      onManualOverrideChange,
    }: {
      label: string
      generatedKey: string
      manualOverride: string
      onManualOverrideChange?: (value: string) => void
    }) => (
      <label>
        <span>{label}</span>
        <input
          aria-label={label}
          value={manualOverride || generatedKey}
          onChange={(event) => onManualOverrideChange?.(event.target.value)}
        />
      </label>
    ),
  }
})

vi.mock('../../api/client', async () => {
  const actual = await vi.importActual<typeof import('../../api/client')>('../../api/client')

  return {
    ...actual,
    getMe: vi.fn(),
    getPmPrograms: vi.fn(),
    getCatalogs: vi.fn(),
    getAssetClasses: vi.fn(),
    getAssetTypes: vi.fn(),
    getAssets: vi.fn(),
    getSites: vi.fn(),
    getDepartments: vi.fn(),
    getTeams: vi.fn(),
    getPeople: vi.fn(),
    getInspectionTemplates: vi.fn(),
    getInspectionTemplate: vi.fn(),
    getComplianceCoreCatalogOptions: vi.fn(),
    getParts: vi.fn(),
    previewPmProgramScope: vi.fn(),
    previewPmProgramDue: vi.fn(),
    createPmProgram: vi.fn(),
    activatePmProgram: vi.fn(),
  }
})

import {
  activatePmProgram,
  createPmProgram,
  getAssetClasses,
  getAssetTypes,
  getAssets,
  getCatalogs,
  getComplianceCoreCatalogOptions,
  getInspectionTemplate,
  getInspectionTemplates,
  getMe,
  getDepartments,
  getParts,
  getPeople,
  getPmPrograms,
  getSites,
  getTeams,
  previewPmProgramDue,
  previewPmProgramScope,
} from '../../api/client'
import { PmProgramCreatePage } from './PmProgramCreatePage'

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

function catalog(key: string, options: Array<{ key: string; label: string }>) {
  return {
    key,
    label: key,
    description: '',
    owner: 'maintainarr',
    scope: 'tenant',
    isSystem: true,
    isTenantExtendable: true,
    isActive: true,
    options: options.map((option, index) => ({
      key: option.key,
      label: option.label,
      description: '',
      sortOrder: index + 1,
      parentOptionKey: null,
      isActive: true,
      dependency: null,
      metadata: null,
    })),
  }
}

function referenceOption(key: string, label: string) {
  return {
    key,
    id: key,
    label,
    source: 'staffarr',
    sourceOfTruth: 'StaffArr',
    storedValue: key,
    displayValue: label,
    isActive: true,
  }
}

function renderPage() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={['/app/maintainarr/pm-programs/create']}>
        <PmProgramCreatePage />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('PmProgramCreatePage', () => {
  afterEach(() => {
    sessionStorage.clear()
    vi.clearAllMocks()
  })

  it('starts with basics expanded and advances to scope when the required basics are complete', async () => {
    sessionStorage.setItem('stl.maintainarr.session', JSON.stringify(session))

    vi.mocked(getMe).mockResolvedValue({
      userId: session.userId,
      personId: session.personId,
      email: session.email,
      displayName: session.displayName,
      tenantId: session.tenantId,
      tenantRoleKey: 'tenant_admin',
      isPlatformAdmin: false,
      productKey: 'maintainarr',
      launchableProductKeys: ['maintainarr'],
    })
    vi.mocked(getPmPrograms).mockResolvedValue([])
    vi.mocked(getCatalogs).mockResolvedValue([
      catalog('PMProgram', [
        { key: 'fleet_safety', label: 'Fleet Safety' },
        { key: 'compliance', label: 'Compliance' },
      ]),
      catalog('PMType', [
        { key: 'inspection', label: 'Inspection' },
        { key: 'service', label: 'Service' },
      ]),
      catalog('priority', [
        { key: 'high', label: 'High' },
        { key: 'medium', label: 'Medium' },
      ]),
      catalog('workOrderPriority', [
        { key: 'high', label: 'High' },
        { key: 'normal', label: 'Normal' },
      ]),
      catalog('workOrderType', [{ key: 'preventive', label: 'Preventive' }]),
      catalog('assetStatus', [{ key: 'active', label: 'Active' }]),
      catalog('readinessStatus', [{ key: 'ready', label: 'Ready' }]),
      catalog('make', [{ key: 'freightliner', label: 'Freightliner' }]),
      catalog('model', [{ key: 'm2', label: 'M2' }]),
      catalog('fuelType', [{ key: 'diesel', label: 'Diesel' }]),
      catalog('meterUnit', [{ key: 'miles', label: 'Miles' }]),
      catalog('meterReadingSource', [{ key: 'manual', label: 'Manual' }]),
    ])
    vi.mocked(getAssetClasses).mockResolvedValue([
      {
        assetClassId: 'class-1',
        classKey: 'vehicle',
        name: 'Vehicle',
        description: '',
        status: 'active',
        createdAt: '2026-06-01T00:00:00Z',
      },
    ])
    vi.mocked(getAssetTypes).mockResolvedValue([
      {
        assetTypeId: 'type-1',
        assetClassId: 'class-1',
        classKey: 'vehicle',
        className: 'Vehicle',
        typeKey: 'truck',
        name: 'Truck',
        description: '',
        status: 'active',
        createdAt: '2026-06-01T00:00:00Z',
      },
    ])
    vi.mocked(getAssets).mockResolvedValue([
      {
        assetId: 'asset-1',
        assetTypeId: 'type-1',
        typeKey: 'truck',
        typeName: 'Truck',
        classKey: 'vehicle',
        className: 'Vehicle',
        assetTag: 'TRK-101',
        name: 'Truck 101',
        description: '',
        lifecycleStatus: 'active',
        siteRef: 'site-1',
        createdAt: '2026-06-01T00:00:00Z',
        updatedAt: '2026-06-01T00:00:00Z',
      },
    ])
    vi.mocked(getSites).mockResolvedValue([referenceOption('site-1', 'Sparta Fleet Yard')])
    vi.mocked(getDepartments).mockResolvedValue([referenceOption('dept-1', 'Maintenance')])
    vi.mocked(getTeams).mockResolvedValue([referenceOption('team-1', 'Fleet PM Team')])
    vi.mocked(getPeople).mockResolvedValue([referenceOption('person-100', 'Dana Maintenance Admin')])
    vi.mocked(getInspectionTemplates).mockResolvedValue([])
    vi.mocked(getComplianceCoreCatalogOptions).mockResolvedValue([])
    vi.mocked(getParts).mockResolvedValue([])
    vi.mocked(getInspectionTemplate).mockResolvedValue({
      inspectionTemplateId: 'template-1',
      templateKey: 'template-1',
      name: 'Pre-trip inspection',
      description: '',
      version: 1,
      status: 'active',
      categories: [],
      checklistItems: [],
      linkedAssetTypes: [],
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
    })
    vi.mocked(previewPmProgramScope).mockResolvedValue({
      matchedAssetCount: 0,
      excludedAssetCount: 0,
      sampleAssets: [],
      warnings: [],
      canActivate: false,
    })
    vi.mocked(previewPmProgramDue).mockResolvedValue({
      dueLogic: 'Any trigger due',
      items: [],
      warnings: [],
      requiresExplicitConfirmation: false,
    })
    vi.mocked(createPmProgram).mockResolvedValue({
      pmProgramId: 'pm-1',
      programKey: 'quarterly-fleet-pm',
      name: 'Quarterly Fleet PM',
      description: '',
      scopeType: 'asset_scope',
      assetTypeId: null,
      assetTypeKey: null,
      assetTypeName: null,
      assetId: null,
      assetTag: null,
      assetName: null,
      status: 'draft',
      autoGenerateWorkOrder: false,
      defaultWorkOrderTemplateRef: null,
      autoGenerateInspection: false,
      inspectionTemplateId: null,
      inspectionTemplateKey: null,
      inspectionTemplateName: null,
      schedules: [],
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
      categoryKey: 'fleet_safety',
      workTypeKey: 'inspection',
      priorityKey: 'high',
      owningSiteRef: 'site-1',
      owningTeamRef: 'team-1',
      owningDepartmentRef: 'dept-1',
      ownerPersonId: 'person-100',
      ownerRoleKey: 'maintenance_lead',
      tags: ['fleet'],
      activatedAt: null,
      pausedAt: null,
      retiredAt: null,
      matchedAssetCount: 0,
      scopeSummary: 'Applies to matching vehicles.',
      dueSummary: 'Due summary',
      workPackageSummary: 'Work package summary',
      inspectionSummary: 'Inspection summary',
      complianceSummary: 'Compliance summary',
      automationSummary: 'Automation summary',
    })
    vi.mocked(activatePmProgram).mockResolvedValue({
      pmProgramId: 'pm-1',
      programKey: 'quarterly-fleet-pm',
      name: 'Quarterly Fleet PM',
      description: '',
      scopeType: 'asset_scope',
      assetTypeId: null,
      assetTypeKey: null,
      assetTypeName: null,
      assetId: null,
      assetTag: null,
      assetName: null,
      status: 'active',
      autoGenerateWorkOrder: false,
      defaultWorkOrderTemplateRef: null,
      autoGenerateInspection: false,
      inspectionTemplateId: null,
      inspectionTemplateKey: null,
      inspectionTemplateName: null,
      schedules: [],
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
      categoryKey: 'fleet_safety',
      workTypeKey: 'inspection',
      priorityKey: 'high',
      owningSiteRef: 'site-1',
      owningTeamRef: 'team-1',
      owningDepartmentRef: 'dept-1',
      ownerPersonId: 'person-100',
      ownerRoleKey: 'maintenance_lead',
      tags: ['fleet'],
      activatedAt: '2026-06-01T00:00:00Z',
      pausedAt: null,
      retiredAt: null,
      matchedAssetCount: 0,
      scopeSummary: 'Applies to matching vehicles.',
      dueSummary: 'Due summary',
      workPackageSummary: 'Work package summary',
      inspectionSummary: 'Inspection summary',
      complianceSummary: 'Compliance summary',
      automationSummary: 'Automation summary',
    })

    renderPage()

    await screen.findByText('Program Basics')

    const basicsSection = screen.getByTestId('pm-program-section-basics')
    const scopeSection = screen.getByTestId('pm-program-section-scope')
    const ownerSummary = screen.getByText('Owner summary').parentElement as HTMLElement
    const scopeToggle = () => within(scopeSection).getAllByRole('button')[0]

    expect(within(basicsSection).getByTestId('pm-program-name')).toBeInTheDocument()
    expect(within(scopeSection).queryByTestId('pm-program-scope-asset-classes')).not.toBeInTheDocument()
    expect(scopeToggle()).toBeDisabled()
    expect(ownerSummary).toHaveTextContent('Site: Not set')

    fireEvent.click(screen.getByRole('button', { name: 'Quick create site' }))
    await waitFor(() => expect(ownerSummary).toHaveTextContent('Site: North Yard'))

    await waitFor(() => expect(screen.getByRole('option', { name: 'Fleet Safety' })).toBeInTheDocument())

    fireEvent.change(screen.getByLabelText('Program name'), {
      target: { value: 'Quarterly Fleet PM' },
    })
    fireEvent.change(screen.getByLabelText('PM category'), {
      target: { value: 'fleet_safety' },
    })
    fireEvent.change(screen.getByLabelText('Work type'), {
      target: { value: 'inspection' },
    })
    fireEvent.change(screen.getByLabelText('Priority'), {
      target: { value: 'high' },
    })

    await waitFor(() => expect(within(scopeSection).getByTestId('pm-program-scope-asset-classes')).toBeInTheDocument())
    expect(scopeToggle()).not.toBeDisabled()
    expect(within(basicsSection).getByText(/Quarterly Fleet PM · /)).toBeInTheDocument()
  })
})


