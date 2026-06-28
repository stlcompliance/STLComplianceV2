import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter, Route, Routes } from 'react-router-dom'

vi.mock('@stl/shared-ui', () => {
  return {
    PageHeader: ({ title, subtitle }: { title: string; subtitle?: string }) => (
      <header>
        <h1>{title}</h1>
        {subtitle ? <p>{subtitle}</p> : null}
      </header>
    ),
    QuestionnaireFlow: ({
      title,
      subtitle,
      submitLabel,
    }: {
      title: string
      subtitle?: string
      submitLabel?: string
    }) => (
      <section data-testid="questionnaire-flow">
        <h2>{title}</h2>
        {subtitle ? <p>{subtitle}</p> : null}
        {submitLabel ? <button type="button">{submitLabel}</button> : null}
      </section>
    ),
  }
})

import { AssetCreatePage } from './AssetCreatePage'

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

const createFieldset = {
  key: 'assets',
  label: 'Assets',
  entityType: 'asset',
  purpose: 'create',
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
      options: [
        { key: 'vehicle', label: 'Vehicle', description: '', sortOrder: 1, parentOptionKey: null, isActive: true, dependency: null, metadata: null },
        { key: 'trailer', label: 'Trailer', description: '', sortOrder: 2, parentOptionKey: null, isActive: true, dependency: null, metadata: null },
      ],
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
      dependsOn: { assetClass: 'assetClass' },
      validation: null,
      defaultValue: null,
      visibility: null,
      sectionKey: 'identity',
      options: [
        { key: 'pickup', label: 'Pickup', description: '', sortOrder: 1, parentOptionKey: null, isActive: true, dependency: { assetClass: 'vehicle' }, metadata: null },
        { key: 'reefer_trailer', label: 'Reefer Trailer', description: '', sortOrder: 2, parentOptionKey: null, isActive: true, dependency: { assetClass: 'trailer' }, metadata: null },
      ],
    },
    {
      key: 'assetStatus',
      label: 'Asset Status',
      description: '',
      type: 'string',
      control: 'select',
      required: true,
      catalogKey: 'assetStatus',
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
      defaultValue: 'active',
      visibility: null,
      sectionKey: 'identity',
      options: [{ key: 'active', label: 'Active', description: '', sortOrder: 1, parentOptionKey: null, isActive: true, dependency: null, metadata: null }],
    },
    {
      key: 'lifecycleStatus',
      label: 'Lifecycle Status',
      description: '',
      type: 'string',
      control: 'select',
      required: true,
      catalogKey: 'lifecycleStatus',
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
      defaultValue: 'in_service',
      visibility: null,
      sectionKey: 'identity',
      options: [{ key: 'in_service', label: 'In Service', description: '', sortOrder: 1, parentOptionKey: null, isActive: true, dependency: null, metadata: null }],
    },
    {
      key: 'criticality',
      label: 'Criticality',
      description: '',
      type: 'string',
      control: 'select',
      required: true,
      catalogKey: 'criticality',
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
      defaultValue: 'medium',
      visibility: null,
      sectionKey: 'identity',
      options: [{ key: 'medium', label: 'Medium', description: '', sortOrder: 1, parentOptionKey: null, isActive: true, dependency: null, metadata: null }],
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

function renderCreatePage() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={['/assets/new']}>
        <Routes>
          <Route path="/assets/new" element={<AssetCreatePage />} />
          <Route path="/assets/:assetId" element={<div>Created detail route</div>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('AssetCreatePage', () => {
  afterEach(() => {
    cleanup()
    sessionStorage.clear()
    vi.restoreAllMocks()
  })

  it('hydrates the create fieldset, reveals optional sections after basics, filters dependent options, and routes after create', async () => {
    sessionStorage.setItem('stl.maintainarr.session', JSON.stringify(session))
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockImplementation(async (input, init) => {
      const url = String(input)
      if (url === '/api/me') {
        return new Response(JSON.stringify({ ...session, tenantRoleKey: 'maintainarr_admin', isPlatformAdmin: false, productKey: 'maintainarr', launchableProductKeys: ['maintainarr'] }), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        })
      }
      if (url === '/api/v1/fieldsets/assets/create') {
        return new Response(JSON.stringify(createFieldset), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        })
      }
      if (url === '/api/v1/assets' && init?.method === 'POST') {
        return new Response(JSON.stringify({
          assetId: 'asset-1',
          assetTypeId: 'type-1',
          typeKey: 'pickup',
          typeName: 'Pickup',
          classKey: 'vehicle',
          className: 'Vehicle',
          assetTag: 'TRK-100',
          name: 'TRK-100',
          description: '',
          lifecycleStatus: 'in_service',
          siteRef: null,
          createdAt: '2026-06-01T00:00:00Z',
          updatedAt: '2026-06-01T00:00:00Z',
        }), {
          status: 201,
          headers: { 'Content-Type': 'application/json' },
        })
      }
      return new Response('{}', { status: 404, headers: { 'Content-Type': 'application/json' } })
    })

    renderCreatePage()

    expect(await screen.findByLabelText('Unit / Asset Number *')).toBeInTheDocument()
    expect(screen.queryByLabelText('VIN')).not.toBeInTheDocument()
    expect(screen.getByRole('button', { name: /next/i })).toBeDisabled()

    fireEvent.change(screen.getByLabelText('Unit / Asset Number *'), { target: { value: 'TRK-100' } })
    fireEvent.change(screen.getByLabelText('Asset Class *'), { target: { value: 'vehicle' } })
    expect(screen.getByRole('option', { name: 'Pickup' })).toBeInTheDocument()
    expect(screen.queryByRole('option', { name: 'Reefer Trailer' })).not.toBeInTheDocument()
    fireEvent.change(screen.getByLabelText('Asset Type *'), { target: { value: 'pickup' } })

    await waitFor(() => expect(screen.getByRole('button', { name: /next/i })).toBeEnabled())
    fireEvent.click(screen.getByRole('button', { name: /next/i }))

    expect(await screen.findByLabelText('VIN')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: /next/i }))

    expect(await screen.findByRole('button', { name: /create asset/i })).toBeEnabled()
    fireEvent.click(screen.getByRole('button', { name: /create asset/i }))

    expect(await screen.findByText('Created detail route')).toBeInTheDocument()
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/fieldsets/assets/create', expect.any(Object))
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/assets', expect.objectContaining({ method: 'POST' }))
  })

  it('shows a local validation hint instead of calling VIN decode for unsupported characters', async () => {
    sessionStorage.setItem('stl.maintainarr.session', JSON.stringify(session))
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockImplementation(async (input) => {
      const url = String(input)
      if (url === '/api/me') {
        return new Response(JSON.stringify({ ...session, tenantRoleKey: 'maintainarr_admin', isPlatformAdmin: false, productKey: 'maintainarr', launchableProductKeys: ['maintainarr'] }), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        })
      }
      if (url === '/api/v1/fieldsets/assets/create') {
        return new Response(JSON.stringify(createFieldset), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        })
      }
      return new Response('{}', { status: 404, headers: { 'Content-Type': 'application/json' } })
    })

    renderCreatePage()

    fireEvent.change(await screen.findByLabelText('Unit / Asset Number *'), { target: { value: 'TRK-100' } })
    fireEvent.change(screen.getByLabelText('Asset Class *'), { target: { value: 'vehicle' } })
    fireEvent.change(screen.getByLabelText('Asset Type *'), { target: { value: 'pickup' } })
    await waitFor(() => expect(screen.getByRole('button', { name: /next/i })).toBeEnabled())
    fireEvent.click(screen.getByRole('button', { name: /next/i }))

    fireEvent.change(await screen.findByLabelText('VIN'), { target: { value: '1FT-ABC' } })

    expect(await screen.findByText('VIN preview supports letters and numbers only. Remove separators or unsupported characters to run the decode.')).toBeInTheDocument()
    expect(
      fetchMock.mock.calls.some(([input]) => String(input).startsWith('/api/v1/external-intelligence/vin/decode')),
    ).toBe(false)
  })

  it('normalizes supported VIN preview input before calling decode', async () => {
    sessionStorage.setItem('stl.maintainarr.session', JSON.stringify(session))
    let decodeBody: { vin: string; modelYear: number | null } | null = null
    vi.spyOn(globalThis, 'fetch').mockImplementation(async (input, init) => {
      const url = String(input)
      if (url === '/api/me') {
        return new Response(JSON.stringify({ ...session, tenantRoleKey: 'maintainarr_admin', isPlatformAdmin: false, productKey: 'maintainarr', launchableProductKeys: ['maintainarr'] }), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        })
      }
      if (url === '/api/v1/fieldsets/assets/create') {
        return new Response(JSON.stringify(createFieldset), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        })
      }
      if (url === '/api/v1/external-intelligence/vin/decode' && init?.method === 'POST') {
        decodeBody = JSON.parse(String(init.body))
        return new Response(JSON.stringify({
          providerKey: 'nhtsa',
          vin: '1FTFW1E58',
          normalizedVin: '1FTFW1E58',
          modelYear: null,
          isPartial: true,
          searchCriteria: 'VIN:1FTFW1E58',
          message: 'Decode completed with partial reference coverage.',
          errorCode: null,
          errorText: null,
          additionalErrorText: null,
          decodedFields: { Make: 'Ford', Model: 'F-150' },
          suggestions: [],
          identifiers: [],
          snapshotId: null,
        }), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        })
      }
      return new Response('{}', { status: 404, headers: { 'Content-Type': 'application/json' } })
    })

    renderCreatePage()

    fireEvent.change(await screen.findByLabelText('Unit / Asset Number *'), { target: { value: 'TRK-100' } })
    fireEvent.change(screen.getByLabelText('Asset Class *'), { target: { value: 'vehicle' } })
    fireEvent.change(screen.getByLabelText('Asset Type *'), { target: { value: 'pickup' } })
    await waitFor(() => expect(screen.getByRole('button', { name: /next/i })).toBeEnabled())
    fireEvent.click(screen.getByRole('button', { name: /next/i }))

    fireEvent.change(await screen.findByLabelText('VIN'), { target: { value: '1ft fw1e58' } })

    await screen.findByText('Search criteria: VIN:1FTFW1E58')
    expect(decodeBody).toEqual({ vin: '1FTFW1E58', modelYear: null })
  })
})


