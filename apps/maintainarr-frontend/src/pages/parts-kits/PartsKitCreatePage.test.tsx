import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
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
    multiple,
  }: {
    label?: string
    value: string | string[]
    options?: Array<{ value: string; label: string; inactive?: boolean }>
    onChange: (value: string | string[]) => void
    placeholder?: string
    emptyLabel?: string
    testId?: string
    disabled?: boolean
    id?: string
    multiple?: boolean
  }) => {
    const fieldId = id ?? testId
    return (
      <label htmlFor={fieldId} className="block">
        {label ? <span>{label}</span> : null}
        <select
          id={fieldId}
          aria-label={label ?? fieldId}
          data-testid={testId}
          multiple={multiple}
          value={value ?? (multiple ? [] : '')}
          disabled={disabled}
          onChange={(event) => {
            if (multiple) {
              onChange(Array.from(event.target.selectedOptions).map((option) => option.value))
              return
            }
            onChange(event.target.value)
          }}
        >
          {!multiple ? <option value="">{emptyLabel ?? placeholder ?? 'Select…'}</option> : null}
          {(options ?? []).map((option) => (
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
    AsyncSearchPicker: Picker,
    AsyncMultiPicker: Picker,
    DetailBadge: ({ label }: { label: string }) => <span>{label}</span>,
    GeneratedKeyField: ({
      label,
      generatedKey,
      manualOverride,
      onManualOverrideChange,
    }: {
      label: string
      generatedKey: string
      manualOverride?: string
      onManualOverrideChange: (value: string) => void
    }) => (
      <label>
        <span>{label}</span>
        <input
          aria-label={label}
          value={manualOverride ?? generatedKey}
          onChange={(event) => onManualOverrideChange(event.target.value)}
        />
      </label>
    ),
    ReferenceSearchPicker: ({
      label,
      value,
      onChange,
      allowQuickCreate = true,
      referenceType,
      testId,
    }: {
      label?: string
      value: string
      onChange: (value: string) => void
      allowQuickCreate?: boolean
      referenceType: string
      testId?: string
    }) => (
      <div data-testid={testId}>
        {label ? <span>{label}</span> : null}
        <p>{allowQuickCreate ? 'Quick create enabled' : 'Quick create disabled'}</p>
        <p>Selected {referenceType}: {value || 'none'}</p>
        <button
          type="button"
          aria-label={`Quick create ${referenceType}`}
          onClick={() => onChange('part-quick-created-1')}
        >
          Quick create {referenceType}
        </button>
      </div>
    ),
  }
})

vi.mock('../../api/client', async () => {
  const actual = await vi.importActual<typeof import('../../api/client')>('../../api/client')

  return {
    ...actual,
    activateMaintenancePartsKit: vi.fn(),
    createMaintenancePartsKit: vi.fn(),
    getCatalogs: vi.fn(),
    getAsset: vi.fn(),
    getMaintenancePartsKit: vi.fn(),
    getMaintenancePartsKits: vi.fn(),
    getMe: vi.fn(),
    getPeople: vi.fn(),
    getSites: vi.fn(),
    getTeams: vi.fn(),
    previewMaintenancePartsKit: vi.fn(),
    searchAssets: vi.fn(),
    submitMaintenancePartsKitForApproval: vi.fn(),
    updateMaintenancePartsKit: vi.fn(),
    validateMaintenancePartsKit: vi.fn(),
  }
})

import {
  activateMaintenancePartsKit,
  createMaintenancePartsKit,
  getCatalogs,
  getAsset,
  getMaintenancePartsKit,
  getMaintenancePartsKits,
  getMe,
  getPeople,
  getSites,
  getTeams,
  previewMaintenancePartsKit,
  searchAssets,
  submitMaintenancePartsKitForApproval,
  updateMaintenancePartsKit,
  validateMaintenancePartsKit,
} from '../../api/client'
import { PartsKitCreatePage } from './PartsKitCreatePage'

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

const partsKitResponse = {
  partsKitId: 'kit-1',
  kitNumber: 'KIT-001',
  title: 'Brake service kit',
  description: '',
  assetTypeApplicability: [],
  workOrderTypeApplicability: [],
  pmPlanRef: null,
  kitCategoryKey: null,
  kitTypeKey: null,
  priorityKey: null,
  owningSiteRef: null,
  owningTeamRef: null,
  ownerPersonId: null,
  ownerRoleKey: null,
  tags: [],
  definition: {
    applicabilityWorkOrderTypes: [],
    applicabilityPmProgramRefs: [],
    applicabilityInspectionTemplateRefs: [],
    applicabilityDefectTypes: [],
    applicabilityTaskTemplateRefs: [],
    applicabilityRepairCategories: [],
    workSourceCompatibilities: [],
    assetScope: {
      assetClassKeys: [],
      assetTypeKeys: [],
      assetCategoryKeys: [],
      assetStatusKeys: [],
      siteRefs: [],
      departmentRefs: [],
      makeKeys: [],
      modelKeys: [],
      yearFrom: null,
      yearTo: null,
      fuelTypeKeys: [],
      bodyTypeKeys: [],
      configurationKeys: [],
      variantFlags: [],
      requiredAttributes: [],
      excludedAttributes: [],
      includedAssetIds: [],
      excludedAssetIds: [],
    },
    items: [
      {
        itemRef: 'item-1',
        supplyarrPartId: 'part-quick-created-1',
        itemDescriptionSnapshot: 'Front brake pads',
        partNumberSnapshot: 'BP-100',
        manufacturerPartNumberSnapshot: null,
        vendorPartNumberSnapshot: null,
        quantity: 2,
        unitOfMeasure: 'each',
        required: true,
        criticality: 'medium',
        substituteAllowed: false,
        preferredSubstituteRefs: [],
        consumable: false,
        serialized: false,
        coreReturnExpected: false,
        hazardous: false,
        warrantySensitive: false,
        requiredByTask: null,
        notes: null,
        tags: [],
        isPlaceholder: false,
      },
    ],
    quantityRules: [],
    availability: {
      enabled: false,
      preferredFulfillmentSource: null,
      showSiteAvailability: true,
      showNearbyAvailability: true,
      showOnOrder: true,
      showEstimatedLeadTime: true,
      requestReservation: false,
      notes: null,
    },
    workOrderBehavior: {
      canBeManuallyAdded: true,
      autoSuggestOnMatchingWorkOrder: false,
      autoAddToMatchingWorkOrder: false,
      autoAddToPmGeneratedWorkOrder: false,
      autoAddAfterFailedInspectionQuestion: false,
      autoAddAfterMatchingDefectType: false,
      requireSupervisorApprovalBeforeAdding: false,
      requirePartsReviewBeforeWorkCanStart: false,
      requireAvailabilityCheckBeforeScheduling: false,
      allowTechnicianAdjustQuantities: true,
      requireAdjustmentReason: false,
      allowTechnicianRemoveOptionalItems: true,
      allowTechnicianRemoveRequiredItems: false,
      requireReasonToRemoveRequiredItem: false,
      snapshotKitItemsOntoWorkOrder: true,
      keepLiveReferenceAfterWorkOrderCreation: false,
    },
    compliance: {
      complianceRelated: false,
      governingBodyKeys: [],
      citationRefs: [],
      safetyCritical: false,
      readinessSensitive: false,
      missingRequiredPartsBlockWorkStart: false,
      missingRequiredPartsBlockWorkCompletion: false,
      requireSupervisorApprovalForSubstitution: false,
      requireDocumentationForSubstitution: false,
      requireFinalInspectionAfterUse: false,
      linkedInspectionTemplateId: null,
    },
    approval: {
      requiresApprovalBeforeActivation: false,
      approverRoleKey: null,
      approverPersonId: null,
      retireReplacedKitAfterActivation: false,
      notesForApprover: null,
    },
    changeReason: null,
    versionLabel: null,
  },
  effectiveAt: '2026-06-01T00:00:00Z',
  expiresAt: null,
  version: 1,
  status: 'draft',
}

const meResponse = {
  userId: 'user-1',
  personId: 'person-100',
  email: 'dana@example.test',
  displayName: 'Dana Maintenance Admin',
  tenantId: 'tenant-1',
  tenantRoleKey: 'tenant_admin',
  isPlatformAdmin: false,
  productKey: 'maintainarr',
  launchableProductKeys: ['maintainarr'],
}

function renderPage(path = '/parts-kits/create') {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[path]}>
        <Routes>
          <Route path="/parts-kits/create" element={<PartsKitCreatePage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('PartsKitCreatePage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
    sessionStorage.clear()
  })

  it('quick creates a part reference and saves it into the kit definition', async () => {
    sessionStorage.setItem('stl.maintainarr.session', JSON.stringify(session))

    vi.mocked(getMe).mockResolvedValue(meResponse as never)
    vi.mocked(getCatalogs).mockResolvedValue([] as never)
    vi.mocked(getSites).mockResolvedValue([
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
    ] as never)
    vi.mocked(getTeams).mockResolvedValue([] as never)
    vi.mocked(getPeople).mockResolvedValue([] as never)
    vi.mocked(createMaintenancePartsKit).mockResolvedValue(partsKitResponse as never)
    vi.mocked(updateMaintenancePartsKit).mockResolvedValue(partsKitResponse as never)
    vi.mocked(getMaintenancePartsKits).mockResolvedValue({ items: [] } as never)
    vi.mocked(getMaintenancePartsKit).mockResolvedValue(partsKitResponse as never)
    vi.mocked(previewMaintenancePartsKit).mockResolvedValue({} as never)
    vi.mocked(validateMaintenancePartsKit).mockResolvedValue({} as never)
    vi.mocked(getAsset).mockResolvedValue({} as never)
    vi.mocked(searchAssets).mockResolvedValue([] as never)
    vi.mocked(activateMaintenancePartsKit).mockResolvedValue(partsKitResponse as never)
    vi.mocked(submitMaintenancePartsKitForApproval).mockResolvedValue({ status: 'pending_approval' } as never)

    renderPage()

    await waitFor(() => {
      expect(screen.getByText('Basics')).toBeInTheDocument()
    })

    fireEvent.click(screen.getAllByRole('button', { name: 'Continue' }).at(-1)!)
    fireEvent.click(screen.getAllByRole('button', { name: 'Continue' }).at(-1)!)
    fireEvent.click(screen.getAllByRole('button', { name: 'Continue' }).at(-1)!)

    const itemsSection = await screen.findByText('Kit item 1')
    expect(itemsSection).toBeInTheDocument()

    expect(screen.getByText('Quick create enabled')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Quick create part' }))

    fireEvent.click(screen.getAllByRole('button', { name: 'Save and continue' }).at(-1)!)

    await waitFor(() => {
      expect(createMaintenancePartsKit).toHaveBeenCalledWith(
        'token-123',
        expect.objectContaining({
          definition: expect.objectContaining({
            items: [
              expect.objectContaining({
                supplyarrPartId: 'part-quick-created-1',
              }),
            ],
          }),
        }),
      )
    })
  })
})

