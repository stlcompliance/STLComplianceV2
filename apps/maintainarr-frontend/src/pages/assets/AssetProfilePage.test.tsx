import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import type { AssetExternalIntelligenceOverviewResponse } from '../../api/types'
import { AssetProfilePage } from './AssetProfilePage'

const session = {
  accessToken: 'token-123',
  accessTokenExpiresAt: '2026-06-01T00:00:00Z',
  userId: 'user-1',
  personId: 'person_1001',
  tenantId: 'tenant-1',
  tenantSlug: 'demo',
  tenantDisplayName: 'Demo Tenant',
  displayName: 'Demo Admin',
  email: 'demo@example.test',
}

const asset = {
  assetId: 'asset-1',
  assetTypeId: 'type-1',
  typeKey: 'pickup',
  typeName: 'Pickup',
  classKey: 'vehicle',
  className: 'Vehicle',
  assetTag: 'TRK-100',
  name: 'Truck 100',
  description: '',
  lifecycleStatus: 'in_service',
  siteRef: 'North Yard',
  createdAt: '2026-06-01T00:00:00Z',
  updatedAt: '2026-06-01T01:00:00Z',
}

const editFieldset = {
  key: 'assets',
  label: 'Assets',
  entityType: 'asset',
  purpose: 'edit',
  fields: [
    {
      key: 'unitNumber',
      label: 'Unit / Asset Number',
      description: '',
      type: 'string',
      control: 'text',
      required: true,
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
      validation: { minLength: 2, maxLength: 64 },
      defaultValue: null,
      visibility: null,
      sectionKey: 'identity',
      options: null,
    },
    {
      key: 'assetClass',
      label: 'Asset Class',
      description: '',
      type: 'string',
      control: 'select',
      required: true,
      catalogKey: 'assetClass',
      referenceKey: null,
      source: 'maintainarr_catalog',
      sourceOfTruth: 'MaintainArr',
      storedValue: 'catalog_key',
      displayValue: 'catalog_label',
      allowCustom: false,
      customRequiresApproval: false,
      drivesLogic: true,
      drivesInspectionBranching: true,
      drivesPMApplicability: true,
      drivesCompliance: false,
      drivesReporting: true,
      drivesReadiness: true,
      dependsOn: null,
      validation: null,
      defaultValue: null,
      visibility: null,
      sectionKey: 'identity',
      options: [{ key: 'vehicle', label: 'Vehicle', description: '', sortOrder: 1, parentOptionKey: null, isActive: true, dependency: null, metadata: null }],
    },
    {
      key: 'assetType',
      label: 'Asset Type',
      description: '',
      type: 'string',
      control: 'select',
      required: true,
      catalogKey: 'assetType',
      referenceKey: null,
      source: 'maintainarr_catalog',
      sourceOfTruth: 'MaintainArr',
      storedValue: 'catalog_key',
      displayValue: 'catalog_label',
      allowCustom: false,
      customRequiresApproval: false,
      drivesLogic: true,
      drivesInspectionBranching: true,
      drivesPMApplicability: true,
      drivesCompliance: false,
      drivesReporting: true,
      drivesReadiness: true,
      dependsOn: null,
      validation: null,
      defaultValue: null,
      visibility: null,
      sectionKey: 'identity',
      options: [{ key: 'pickup', label: 'Pickup', description: '', sortOrder: 1, parentOptionKey: null, isActive: true, dependency: null, metadata: null }],
    },
    {
      key: 'VIN',
      label: 'VIN',
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
      validation: { maxLength: 17, pattern: '^[A-HJ-NPR-Z0-9]{11,17}$' },
      defaultValue: null,
      visibility: { assetClass: ['vehicle'] },
      sectionKey: 'classification',
      options: null,
    },
  ],
}

const externalIntelligenceOverview = {
  assetId: 'asset-1',
  vin: '1FTFW1E58MFA00001',
  providers: [
    {
      providerKey: 'nhtsa',
      displayName: 'NHTSA',
      description: 'Federal safety data',
      sourceOfTruth: 'NHTSA',
      status: 'healthy',
      supportsVinDecode: true,
      supportsRecallLookup: true,
      supportsComplaintLookup: true,
      supportsReferenceLookups: true,
      supportsEquipmentReferences: false,
      lastCheckedAt: '2026-06-01T00:00:00Z',
      lastSuccessfulAt: '2026-06-01T00:00:00Z',
      lastError: null,
    },
  ],
  summary: {
    identifierCount: 1,
    snapshotCount: 1,
    suggestionCount: 1,
    activeRecallCount: 1,
    complaintCount: 1,
    lastRefreshedAt: '2026-06-02T00:00:00Z',
  },
  identifiers: [
    {
      identifierId: 'identifier-1',
      assetId: 'asset-1',
      sourceSystem: 'nhtsa',
      identifierType: 'vin',
      identifierValue: '1FTFW1E58MFA00001',
      normalizedValue: '1FTFW1E58MFA00001',
      isPrimary: true,
      isVerified: true,
      metadata: { make: 'Ford' },
      observedAt: '2026-06-01T00:00:00Z',
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
    },
  ],
  snapshots: [
    {
      snapshotId: 'snapshot-1',
      assetId: 'asset-1',
      providerKey: 'nhtsa',
      snapshotType: 'vin_decode',
      sourceObjectRef: '1FTFW1E58MFA00001',
      summary: 'VIN decode snapshot',
      details: { Make: 'Ford', Model: 'F-150' },
      capturedAt: '2026-06-01T00:00:00Z',
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
    },
  ],
  suggestions: [
    {
      suggestionId: 'suggestion-1',
      assetId: 'asset-1',
      snapshotId: 'snapshot-1',
      providerKey: 'nhtsa',
      fieldKey: 'make',
      fieldLabel: 'Make',
      currentValue: 'Unknown',
      proposedValue: 'Ford',
      reason: 'VIN decode identified the manufacturer.',
      confidence: 0.94,
      status: 'pending',
      reviewedByPersonId: null,
      reviewedAt: null,
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
    },
  ],
  recalls: [
    {
      recallId: 'recall-1',
      assetId: 'asset-1',
      providerKey: 'nhtsa',
      campaignNumber: '24V-123',
      actionNumber: null,
      manufacturer: 'Ford',
      component: 'Air bags',
      summary: 'Air bag recall',
      consequence: 'Deployment may fail.',
      remedy: 'Replace inflator module.',
      notes: '',
      modelYear: '2021',
      make: 'Ford',
      model: 'F-150',
      reportReceivedDate: '2026-05-01T00:00:00Z',
      status: 'active',
      qualityHoldId: null,
      capturedAt: '2026-06-01T00:00:00Z',
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
    },
  ],
  complaints: [
    {
      odiNumber: 'ODI-1',
      manufacturer: 'Ford',
      crash: false,
      fire: true,
      numberOfInjuries: null,
      numberOfDeaths: null,
      dateOfIncident: null,
      dateComplaintFiled: '2026-05-15T00:00:00Z',
      vin: '1FTFW1E58MFA00001',
      components: ['air bags'],
      summary: 'Complaint summary',
    },
  ],
} satisfies AssetExternalIntelligenceOverviewResponse

function mockProfileFetches() {
  return vi.spyOn(globalThis, 'fetch').mockImplementation(async (input) => {
    const url = String(input)
    if (url === '/api/me') {
      return new Response(JSON.stringify({ ...session, tenantRoleKey: 'maintainarr_admin', isPlatformAdmin: false, productKey: 'maintainarr', hasMaintainArrEntitlement: true, entitlements: ['maintainarr'] }), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      })
    }
    if (url === '/api/v1/assets/asset-1') {
      return new Response(JSON.stringify(asset), { status: 200, headers: { 'Content-Type': 'application/json' } })
    }
    if (url === '/api/v1/fieldsets/assets/asset-1/edit') {
      return new Response(JSON.stringify(editFieldset), { status: 200, headers: { 'Content-Type': 'application/json' } })
    }
    if (url === '/api/v1/assets/asset-1/field-context') {
      return new Response(JSON.stringify({ assetId: 'asset-1', fields: [] }), { status: 200, headers: { 'Content-Type': 'application/json' } })
    }
    if (url === '/api/v1/assets/asset-1/external-intelligence') {
      return new Response(JSON.stringify(externalIntelligenceOverview), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      })
    }
    if (url === '/api/v1/readiness?assetId=asset-1') {
      return new Response(JSON.stringify({
        assetId: 'asset-1',
        assetTag: 'TRK-100',
        assetName: 'Truck 100',
        lifecycleStatus: 'in_service',
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
      }), { status: 200, headers: { 'Content-Type': 'application/json' } })
    }
    if (url === '/api/defects?assetId=asset-1&status=open') {
      return new Response(JSON.stringify([]), { status: 200, headers: { 'Content-Type': 'application/json' } })
    }
    if (url === '/api/work-orders?assetId=asset-1&status=open') {
      return new Response(JSON.stringify([]), { status: 200, headers: { 'Content-Type': 'application/json' } })
    }
    if (url === '/api/preventive-maintenance/schedules') {
      return new Response(JSON.stringify([]), { status: 200, headers: { 'Content-Type': 'application/json' } })
    }
    if (url === '/api/assets/asset-1/meters') {
      return new Response(JSON.stringify([]), { status: 200, headers: { 'Content-Type': 'application/json' } })
    }
    if (url === '/api/v1/assets/asset-1/telematics-ingestion?limit=8') {
      return new Response(JSON.stringify({
        assetId: 'asset-1',
        assetTag: 'TRK-100',
        assetName: 'Truck 100',
        totalCount: 2,
        limit: 8,
        processedCount: 1,
        ignoredCount: 1,
        defectCount: 1,
        items: [
          {
            inboundEventId: 'event-2',
            sourceEventId: 'source-2',
            sourceProduct: 'routarr',
            eventKind: 'incident.created',
            outcome: 'ignored',
            summary: 'Driver inspection ping · Vehicle TRK-100 · Incident idle_check',
            vehicleRefKey: 'TRK-100',
            tripNumber: 'TRIP-200',
            incidentType: 'idle_check',
            incidentSeverity: 'low',
            dvirResult: null,
            createdDefectId: null,
            correlationId: 'corr-2',
            occurredAt: '2026-06-01T02:00:00Z',
            createdAt: '2026-06-01T02:00:05Z',
          },
          {
            inboundEventId: 'event-1',
            sourceEventId: 'source-1',
            sourceProduct: 'routarr',
            eventKind: 'driver_reported_defect.created',
            outcome: 'processed',
            summary: 'Driver-reported defect · Trip TRIP-100 · Vehicle TRK-100 · DVIR fail',
            vehicleRefKey: 'TRK-100',
            tripNumber: 'TRIP-100',
            incidentType: null,
            incidentSeverity: null,
            dvirResult: 'fail',
            createdDefectId: 'defect-1',
            correlationId: 'corr-1',
            occurredAt: '2026-06-01T01:30:00Z',
            createdAt: '2026-06-01T01:30:05Z',
          },
        ],
      }), { status: 200, headers: { 'Content-Type': 'application/json' } })
    }
    return new Response('{}', { status: 404, headers: { 'Content-Type': 'application/json' } })
  })
}

function renderProfile(initialPath: string, edit = false) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialPath]}>
        <Routes>
          <Route path="/assets/:assetId" element={<AssetProfilePage />} />
          <Route path="/assets/:assetId/edit" element={<AssetProfilePage editModeDefault={edit} />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('AssetProfilePage', () => {
  afterEach(() => {
    sessionStorage.clear()
    vi.restoreAllMocks()
  })

  it('renders read-only values by default with useful empty states', async () => {
    sessionStorage.setItem('stl.maintainarr.session', JSON.stringify(session))
    mockProfileFetches()

    renderProfile('/assets/asset-1')

    expect((await screen.findAllByText('Truck 100')).length).toBeGreaterThan(0)
    expect(screen.getAllByText('TRK-100').length).toBeGreaterThan(0)
    expect(screen.getByText('VIN not recorded')).toBeInTheDocument()
    expect(screen.getByTestId('asset-shop-floor-card')).toBeInTheDocument()
    expect(screen.getByTestId('asset-shop-floor-scan-code')).toHaveTextContent('maintainarr://asset/asset-1')
    expect(screen.getByTestId('asset-telematics-ingestion-card')).toBeInTheDocument()
    expect(screen.getByText('Telematics / diagnostics ingestion')).toBeInTheDocument()
    expect(screen.getByText('1 processed')).toBeInTheDocument()
    expect(screen.getByText('Driver-reported defect · Trip TRIP-100 · Vehicle TRK-100 · DVIR fail')).toBeInTheDocument()
    expect(screen.getByText('External intelligence')).toBeInTheDocument()
    expect(screen.getByText('VIN: 1FTFW1E58MFA00001')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /edit asset/i })).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /save asset/i })).not.toBeInTheDocument()

    await waitFor(() => {
      const text = document.body.textContent ?? ''
      expect(text).not.toContain('undefined')
      expect(text).not.toContain('null')
      expect(text).not.toContain('Tenant 1')
      expect(text).not.toContain('Site 1')
    })
  })

  it('renders editable fieldset controls only in edit mode', async () => {
    sessionStorage.setItem('stl.maintainarr.session', JSON.stringify(session))
    mockProfileFetches()

    renderProfile('/assets/asset-1/edit', true)

    expect(await screen.findByDisplayValue('TRK-100')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /save asset/i })).toBeDisabled()
    expect(screen.getByLabelText('VIN')).toBeInTheDocument()
  })
})
