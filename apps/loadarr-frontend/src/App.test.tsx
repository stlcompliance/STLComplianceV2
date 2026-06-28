import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import type { ReactNode } from 'react'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { saveSession } from './auth/sessionStorage'
import * as client from './api/client'
import { App } from './App'

vi.mock('@stl/shared-ui', async () => {
  return {
    ApiErrorCallout: ({ title, message }: { title: string; message: string }) => (
      <div>
        <strong>{title}</strong>
        <p>{message}</p>
      </div>
    ),
    ControlledSelect: ({ value, onChange, options, className, disabled }: any) => (
      <select
        className={className}
        value={value}
        disabled={disabled}
        onChange={(event) => onChange(event.currentTarget.value)}
      >
        <option value="">Select</option>
        {(options ?? []).map((option: { value: string; label: string; inactive?: boolean }) => (
          <option key={option.value} value={option.value} disabled={option.inactive}>
            {option.label}
          </option>
        ))}
      </select>
    ),
    FormField: ({
      label,
      children,
      className,
    }: {
      label: string
      children: ReactNode
      className?: string
    }) => (
      <label className={className}>
        <span>{label}</span>
        {children}
      </label>
    ),
    ProductWorkspaceFrame: ({ children, productName }: { children: ReactNode; productName: string }) => (
      <div data-testid="workspace-frame">
        <h2>{productName}</h2>
        {children}
      </div>
    ),
    ReferenceProviderClient: class ReferenceProviderClient {
      constructor(_options: unknown) {}
    },
    ReferenceSearchPicker: ({ value, onChange, placeholder, disabled }: any) => (
      <input
        value={value}
        disabled={disabled}
        placeholder={placeholder}
        onChange={(event) => onChange(event.currentTarget.value)}
      />
    ),
    StaticSearchPicker: ({ value, onChange, options, className, placeholder, disabled }: any) => (
      <select
        value={value}
        disabled={disabled}
        className={className}
        onChange={(event) => onChange(event.currentTarget.value)}
      >
        <option value="">{placeholder ?? 'Select'}</option>
        {(options ?? []).map((option: { value: string; label: string; inactive?: boolean }) => (
          <option key={option.value} value={option.value} disabled={option.inactive}>
            {option.label}
          </option>
        ))}
      </select>
    ),
    buildProductLaunchUrlMap: () => ({}),
    formatProductLaunchError: (error: unknown) => String(error),
    getLaunchCatalog: vi.fn().mockResolvedValue({ products: [{ productKey: 'loadarr' }] }),
    resolveProductWorkspaceBootstrapError: () => null,
    resolveSuiteHomeUrl: () => '/',
    useProductWorkspaceLaunch: () => ({
      mutate: vi.fn(),
      isPending: false,
      isError: false,
      error: null,
    }),
  }
})

const jsonResponse = <T,>(data: T, status = 200) =>
  ({
    ok: status >= 200 && status < 300,
    status,
    json: async () => data,
  }) as Response

const baseSession = {
  accessToken: 'token-1',
  accessTokenExpiresAt: '2030-01-01T00:00:00Z',
  userId: 'user-1',
  personId: 'person-1',
  tenantId: 'tenant-1',
  tenantSlug: 'tenant-1',
  tenantDisplayName: 'Tenant 1',
  displayName: 'Casey Operator',
  email: 'casey@loadarr.test',
  isPlatformAdmin: false,
}

const receivingLocation = {
  id: 'loc-dock-01',
  staffarrSiteNameSnapshot: 'STL North Yard',
  staffarrSiteOrgUnitId: 'staff-site-stl-north',
  name: 'Receiving Dock 1',
  locationType: 'dock',
  path: 'STL North Yard / Main Warehouse / Dock 1',
  active: true,
  complianceRestrictions: ['ambient'],
  capacityPercent: 78,
  notes: 'Open for receipts and outbound staging',
}

const quarantineLocation = {
  id: 'loc-quarantine-01',
  staffarrSiteNameSnapshot: 'STL North Yard',
  staffarrSiteOrgUnitId: 'staff-site-stl-north',
  name: 'Quarantine Bay',
  locationType: 'quarantine_area',
  path: 'STL North Yard / Quality / Quarantine Bay',
  active: true,
  complianceRestrictions: ['quality_hold', 'blocked'],
  capacityPercent: 41,
  notes: 'Blocked from allocation until investigation closes',
}

const supplyItemReference = {
  supplyarrItemId: 'SUP-VALVE-KIT-A',
  itemNumberSnapshot: 'SUP-VALVE-KIT-A',
  itemNameSnapshot: 'Valve repair kit A',
  unitOfMeasureSnapshot: 'each',
  itemTypeSnapshot: 'kit',
  isLotControlled: false,
  isSerialControlled: false,
  isHazardous: false,
  requiresSds: false,
  requiresTraceabilityCapture: true,
  updatedAtUtc: '2026-06-01T00:00:00Z',
}

const truckStockRecord = {
  id: 'truck-stock-17-kit',
  truckStockNumber: 'TS-17-KIT',
  staffarrSiteOrgUnitId: 'staff-site-stl-north',
  staffarrSiteNameSnapshot: 'STL North Yard',
  truckLocationId: 'loc-truck-17',
  truckLocationNameSnapshot: 'Truck 17',
  supplyarrItemId: 'SUP-VALVE-KIT-A',
  itemNameSnapshot: 'Valve repair kit A',
  unitOfMeasure: 'each',
  assignedPersonId: 'person-1',
  assignedPersonNameSnapshot: 'Casey Operator',
  quantityOnHand: 4,
  minimumQuantity: 2,
  maximumQuantity: 8,
  status: 'ready',
  lastCountedAtUtc: '2026-06-26T12:00:00Z',
  lastMovementAtUtc: '2026-06-26T12:00:00Z',
  notes: 'Truck stock ready for route issue',
  traceTags: ['truck-stock'],
}

const kitRecord = {
  id: 'kit-ppe-hazmat-04',
  kitNumber: 'KIT-PPE-04',
  staffarrSiteOrgUnitId: 'staff-site-stl-north',
  staffarrSiteNameSnapshot: 'STL North Yard',
  locationId: 'loc-dock-01',
  locationNameSnapshot: 'Receiving Dock 1',
  primaryItemId: 'SUP-VALVE-KIT-A',
  kitNameSnapshot: 'Hazmat response kit',
  unitOfMeasure: 'kit',
  assignedPersonId: 'person-1',
  assignedPersonNameSnapshot: 'Casey Operator',
  quantityOnHand: 1,
  minimumQuantity: 1,
  maximumQuantity: 4,
  status: 'built',
  lastActionAtUtc: '2026-06-26T12:00:00Z',
  lastMovementAtUtc: '2026-06-26T12:00:00Z',
  notes: 'Ready for assignment',
  traceTags: ['kit'],
}

const createWorkspaceSummary = (options: { unexplainedInventory?: any[]; inventory?: any[] } = {}) => {
  const unexplainedInventory = options.unexplainedInventory ?? []

  return {
    generatedAt: '2026-06-26T12:00:00Z',
    metrics: {
      activeLocations: 2,
      quantityOnHand: 52,
      quantityCommitted: 4,
      quantityBlocked: 0,
      openTasks: 1,
      openHolds: 0,
      unexplainedInventory: unexplainedInventory.length,
    },
    locations: [receivingLocation, quarantineLocation],
    inventory: options.inventory ?? [],
    tasks: [],
    holds: [],
    routeHandoffs: [],
    evidence: [],
    unexplainedInventory,
  }
}

let workspaceSummary = createWorkspaceSummary()
let supplyArrItemReferences = [supplyItemReference]
let countsResponse: any[] = []
let adjustmentsResponse: any[] = []
let truckStockResponse: any[] = []
let kitResponse: any[] = []

function getFieldControl(container: HTMLElement, labelText: string, scopeSelector: string = 'body') {
  const scope = scopeSelector === 'body' ? container.ownerDocument.body : container.querySelector(scopeSelector)
  if (!scope) {
    throw new Error(`Unable to find scope: ${scopeSelector}`)
  }

  const labels = Array.from(scope.querySelectorAll('label'))
  const label = labels.find((candidate) => candidate.textContent?.includes(labelText))
  if (!label) {
    throw new Error(`Unable to find label: ${labelText}`)
  }

  const control = label.querySelector('input, select, textarea')
  if (!control) {
    throw new Error(`Unable to find control for label: ${labelText}`)
  }

  return control as HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement
}

vi.mock('./api/client', () => {
  return {
    getSessionBootstrap: vi.fn(),
    getLoadArrPermissionCatalog: vi.fn(),
    loadArrFetch: vi.fn(),
  }
})

describe('LoadArr app', () => {
  beforeEach(() => {
    saveSession(baseSession)
    workspaceSummary = createWorkspaceSummary()
    supplyArrItemReferences = [supplyItemReference]
    countsResponse = []
    adjustmentsResponse = []
    truckStockResponse = []
    kitResponse = []

    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'loadarr-ops',
      isPlatformAdmin: false,
      productKey: 'loadarr',
      launchableProductKeys: ['loadarr'],
    } as any)
    vi.mocked(client.getLoadArrPermissionCatalog).mockResolvedValue({ permissions: [] } as any)
    vi.mocked(client.loadArrFetch).mockImplementation(async (url: string, _accessToken?: string, init?: RequestInit) => {
      const method = (init?.method ?? 'GET').toUpperCase()

      if (url === '/api/v1/workspace/summary' && method === 'GET') {
        return jsonResponse(workspaceSummary)
      }

      if (url === '/api/v1/workspace/locations?active=true' && method === 'GET') {
        return jsonResponse({ items: [receivingLocation, quarantineLocation] })
      }

      if (url === '/api/v1/workspace/supplyarr-item-references' && method === 'GET') {
        return jsonResponse({ items: supplyArrItemReferences })
      }

      if (url === '/api/v1/counts' && method === 'GET') {
        return jsonResponse({ items: countsResponse })
      }

      if (url === '/api/v1/counts' && method === 'POST') {
        return jsonResponse({ error: 'dependency_unavailable' }, 503)
      }

      if (url.startsWith('/api/v1/counts/') && method === 'POST') {
        return jsonResponse({ error: 'dependency_unavailable' }, 503)
      }

      if (url === '/api/v1/receiving' && method === 'GET') {
        return jsonResponse({ items: [] })
      }

      if (url === '/api/v1/transfers' && method === 'GET') {
        return jsonResponse({ items: [] })
      }

      if (url === '/api/v1/integrations/items' && method === 'GET') {
        return jsonResponse(
          {
            errorCode: 'dependency_unavailable',
            message:
              'LoadArr integration items are unavailable because LoadArr does not yet have authoritative tenant-scoped integration synchronization for this tenant.',
          },
          503,
        )
      }

      if (url === '/api/v1/holds' && method === 'POST') {
        return jsonResponse({ error: 'dependency_unavailable' }, 503)
      }

      if (url.startsWith('/api/v1/holds/') && method === 'POST') {
        return jsonResponse({ error: 'dependency_unavailable' }, 503)
      }

      if (url === '/api/v1/adjustments' && method === 'GET') {
        return jsonResponse({ items: adjustmentsResponse })
      }

      if (url === '/api/v1/adjustments' && method === 'POST') {
        return jsonResponse({ error: 'dependency_unavailable' }, 503)
      }

      if (url.startsWith('/api/v1/adjustments/') && method === 'POST') {
        return jsonResponse({ error: 'dependency_unavailable' }, 503)
      }

      if (url === '/api/v1/truck-stock' && method === 'GET') {
        return jsonResponse({ items: truckStockResponse })
      }

      if (url.startsWith('/api/v1/truck-stock/') && method === 'POST') {
        return jsonResponse({ error: 'dependency_unavailable' }, 503)
      }

      if (url === '/api/v1/kits' && method === 'GET') {
        return jsonResponse({ items: kitResponse })
      }

      if (url.startsWith('/api/v1/kits/') && method === 'POST') {
        return jsonResponse({ error: 'dependency_unavailable' }, 503)
      }

      if (url === '/api/v1/unexplained-inventory' && method === 'POST') {
        return jsonResponse({ error: 'dependency_unavailable' }, 503)
      }

      if (url.startsWith('/api/v1/unexplained-inventory/') && method === 'POST') {
        return jsonResponse({ error: 'dependency_unavailable' }, 503)
      }

      return jsonResponse({ items: [] })
    })
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
    sessionStorage.clear()
  })

  it('hides summary-dependent inventory views when the workspace summary is unavailable', async () => {
    const defaultImplementation = vi.mocked(client.loadArrFetch).getMockImplementation()
    vi.mocked(client.loadArrFetch).mockImplementation(async (...args: Parameters<typeof client.loadArrFetch>) => {
      const [url] = args
      if (url === '/api/v1/workspace/summary') {
        return jsonResponse({ error: 'unavailable' }, 503)
      }

      return defaultImplementation!(...args)
    })

    render(
      <MemoryRouter initialEntries={['/work/inventory']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByText('LoadArr workspace unavailable')).toBeTruthy()
    expect(
      await screen.findByText(
        'LoadArr workspace is unavailable right now. LoadArr did not return authoritative data, so this view stays hidden until the API responds.',
      ),
    ).toBeTruthy()
    expect(screen.queryByText('Local snapshot')).toBeNull()
    expect(screen.queryByText('Active locations')).toBeNull()
  })

  it('blocks the dashboard route until an authoritative dashboard read model is available', async () => {
    render(
      <MemoryRouter initialEntries={['/work/dashboard']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Dashboard unavailable' })).toBeTruthy()
    expect(
      await screen.findByText(
        'Dashboard is unavailable because LoadArr does not yet have an authoritative route-surface read model for this tenant.',
      ),
    ).toBeTruthy()
    expect(await screen.findByText('No dashboard summary is being shown')).toBeTruthy()
    expect(screen.queryByRole('heading', { name: 'Balance rollup' })).toBeNull()
    expect(screen.queryByText('Active locations')).toBeNull()
  })

  it('blocks the permission catalog route for users without LoadArr admin read access', async () => {
    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'tenant_member',
      isPlatformAdmin: false,
      productKey: 'loadarr',
      launchableProductKeys: ['loadarr'],
    } as any)

    render(
      <MemoryRouter initialEntries={['/admin/permissions']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Permission catalog unavailable' })).toBeTruthy()
    expect(
      await screen.findByText(
        'This page requires LoadArr admin, manager, or warehouse leadership access.',
      ),
    ).toBeTruthy()
    expect(vi.mocked(client.getLoadArrPermissionCatalog)).not.toHaveBeenCalled()
  })

  it('blocks the stock ledger route until authoritative warehouse history is available', async () => {
    render(
      <MemoryRouter initialEntries={['/records/stock-ledger']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Stock ledger unavailable' })).toBeTruthy()
    expect(
      await screen.findByText(
        'Stock ledger is unavailable because LoadArr does not yet have an authoritative warehouse read model for this tenant.',
      ),
    ).toBeTruthy()
    expect(await screen.findByText('No warehouse history is being shown')).toBeTruthy()
    expect(screen.queryByRole('heading', { name: 'Ledger summary' })).toBeNull()
  })

  it('blocks the warehouse history route until authoritative warehouse history is available', async () => {
    render(
      <MemoryRouter initialEntries={['/records/receiving-history']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Warehouse history unavailable' })).toBeTruthy()
    expect(
      await screen.findByText(
        'Warehouse history is unavailable because LoadArr does not yet have an authoritative warehouse read model for this tenant.',
      ),
    ).toBeTruthy()
    expect(await screen.findByText('No warehouse history is being shown')).toBeTruthy()
    expect(screen.queryByRole('heading', { name: 'Records and operations' })).toBeNull()
  })

  it('blocks the count history route until authoritative warehouse history is available', async () => {
    render(
      <MemoryRouter initialEntries={['/records/count-history']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Count history unavailable' })).toBeTruthy()
    expect(
      await screen.findByText(
        'Count history is unavailable because LoadArr does not yet have an authoritative warehouse read model for this tenant.',
      ),
    ).toBeTruthy()
    expect(await screen.findByText('No count history is being shown')).toBeTruthy()
    expect(screen.queryByRole('heading', { name: 'Cycle counts' })).toBeNull()
    expect(screen.queryByRole('button', { name: 'Record count' })).toBeNull()
  })

  it('blocks the adjustment history route until authoritative warehouse history is available', async () => {
    render(
      <MemoryRouter initialEntries={['/records/adjustment-history']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Adjustment history unavailable' })).toBeTruthy()
    expect(
      await screen.findByText(
        'Adjustment history is unavailable because LoadArr does not yet have an authoritative warehouse read model for this tenant.',
      ),
    ).toBeTruthy()
    expect(await screen.findByText('No adjustment history is being shown')).toBeTruthy()
    expect(screen.queryByRole('heading', { name: 'Adjustment history' })).toBeNull()
    expect(screen.queryByRole('button', { name: 'Create adjustment' })).toBeNull()
  })

  it('blocks the expected receipts route until authoritative route-surface records are available', async () => {
    render(
      <MemoryRouter initialEntries={['/work/expected-receipts']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Expected receipts unavailable' })).toBeTruthy()
    expect(
      await screen.findByText(
        'Expected receipts are unavailable because LoadArr does not yet have an authoritative route-surface read model for this tenant.',
      ),
    ).toBeTruthy()
    expect(await screen.findByText('No expected-receipt watchlist is being shown')).toBeTruthy()
    expect(screen.queryByRole('heading', { name: 'Expected receipt watchlist' })).toBeNull()
  })

  it.each([
    {
      path: '/work/expected-receipts/task-receive-24018',
      heading: 'Expected receipts unavailable',
      legacyDetailLabel: 'Expected receipt detail',
    },
    {
      path: '/work/backorders/truck-stock-17-rotor',
      heading: 'Backorders unavailable',
      legacyDetailLabel: 'Backorder detail',
    },
    {
      path: '/supply/vendor-returns/bal-brake-rotor',
      heading: 'Vendor returns unavailable',
      legacyDetailLabel: 'Vendor return detail',
    },
    {
      path: '/work/exceptions/quarantine',
      heading: 'Exceptions unavailable',
      legacyDetailLabel: 'Exception detail',
    },
    {
      path: '/work/shipping/handoff-rt-7781',
      heading: 'Shipping unavailable',
      legacyDetailLabel: 'Handoff detail',
    },
  ])(
    'blocks the $path detail route instead of showing reconstructed route-surface detail content',
    async ({ path, heading, legacyDetailLabel }) => {
      render(
        <MemoryRouter initialEntries={[path]}>
          <QueryClientProvider client={new QueryClient()}>
            <App />
          </QueryClientProvider>
        </MemoryRouter>,
      )

      expect(await screen.findByRole('heading', { name: heading })).toBeTruthy()
      expect(screen.queryByLabelText(legacyDetailLabel)).toBeNull()
    },
  )

  it('blocks the purchase order receipts route without reusing the expected receipts surface', async () => {
    render(
      <MemoryRouter initialEntries={['/supply/purchase-order-receipts']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Purchase order receipts unavailable' })).toBeTruthy()
    expect(
      await screen.findByText(
        'Purchase order receipts are unavailable because LoadArr does not yet have an authoritative route-surface read model for this tenant.',
      ),
    ).toBeTruthy()
    expect(await screen.findByText('No purchase order receipt coordination is being shown')).toBeTruthy()
    expect(screen.queryByRole('heading', { name: 'Expected receipt watchlist' })).toBeNull()
  })

  it('blocks the dock schedule route until authoritative route-surface records are available', async () => {
    render(
      <MemoryRouter initialEntries={['/work/dock-schedule']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Dock schedule unavailable' })).toBeTruthy()
    expect(
      await screen.findByText(
        'Dock schedule is unavailable because LoadArr does not yet have an authoritative route-surface read model for this tenant.',
      ),
    ).toBeTruthy()
    expect(await screen.findByText('No dock schedule is being shown')).toBeTruthy()
  })

  it('blocks the staging route until authoritative staging assignments are available', async () => {
    render(
      <MemoryRouter initialEntries={['/work/staging']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Staging unavailable' })).toBeTruthy()
    expect(
      await screen.findByText(
        'Staging is unavailable because LoadArr does not yet have an authoritative route-surface read model for this tenant.',
      ),
    ).toBeTruthy()
    expect(await screen.findByText('No staging assignments are being shown')).toBeTruthy()
    expect(screen.queryByRole('heading', { name: 'Truck stock' })).toBeNull()
    expect(screen.queryByRole('button', { name: 'Issue from truck' })).toBeNull()
  })

  it('blocks the reorder signals route without reusing the generic supply coordination summary', async () => {
    render(
      <MemoryRouter initialEntries={['/supply/reorder-signals']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Reorder signals unavailable' })).toBeTruthy()
    expect(
      await screen.findByText(
        'Reorder signals are unavailable because LoadArr does not yet have an authoritative route-surface read model for this tenant.',
      ),
    ).toBeTruthy()
    expect(await screen.findByText('No reorder signals are being shown')).toBeTruthy()
    expect(screen.queryByRole('heading', { name: 'Supply coordination' })).toBeNull()
  })

  it('blocks the shipping route until authoritative route-surface records are available', async () => {
    render(
      <MemoryRouter initialEntries={['/work/shipping']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Shipping unavailable' })).toBeTruthy()
    expect(
      await screen.findByText(
        'Shipping is unavailable because LoadArr does not yet have an authoritative route-surface read model for this tenant.',
      ),
    ).toBeTruthy()
    expect(await screen.findByText('No shipping handoffs are being shown')).toBeTruthy()
    expect(screen.queryByRole('heading', { name: 'Route and product handoffs' })).toBeNull()
  })

  it('routes the legacy /work/issues path to backorders instead of warehouse exceptions', async () => {
    render(
      <MemoryRouter initialEntries={['/work/issues']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Backorders unavailable' })).toBeTruthy()
    expect(
      await screen.findByText(
        'Backorders are unavailable because LoadArr does not yet have an authoritative route-surface read model for this tenant.',
      ),
    ).toBeTruthy()
    expect(screen.queryByRole('heading', { name: 'Exceptions unavailable' })).toBeNull()
  })

  it.each([
    {
      path: '/setup/location-rules',
      heading: 'Location rules unavailable',
      message:
        'Location rules are unavailable because LoadArr does not yet have an authoritative route-surface read model for this tenant.',
      emptyTitle: 'No location rules are being shown',
    },
    {
      path: '/setup/item-references',
      heading: 'Item references unavailable',
      message:
        'Item references are unavailable because LoadArr does not yet have an authoritative route-surface read model for this tenant.',
      emptyTitle: 'No item references are being shown',
    },
    {
      path: '/setup/inventory-policies',
      heading: 'Inventory policies unavailable',
      message:
        'Inventory policies are unavailable because LoadArr does not yet have an authoritative route-surface read model for this tenant.',
      emptyTitle: 'No inventory policies are being shown',
    },
    {
      path: '/setup/devices-labels',
      heading: 'Devices and labels unavailable',
      message:
        'Devices and labels are unavailable because LoadArr does not yet have an authoritative route-surface read model for this tenant.',
      emptyTitle: 'No device or label profiles are being shown',
    },
  ])(
    'blocks the $path route until authoritative setup route-surface records are available',
    async ({ path, heading, message, emptyTitle }) => {
      render(
        <MemoryRouter initialEntries={[path]}>
          <QueryClientProvider client={new QueryClient()}>
            <App />
          </QueryClientProvider>
        </MemoryRouter>,
      )

      expect(await screen.findByRole('heading', { name: heading })).toBeTruthy()
      expect(await screen.findByText(message)).toBeTruthy()
      expect(await screen.findByText(emptyTitle)).toBeTruthy()
      expect(screen.queryByRole('heading', { name: 'Tenant settings' })).toBeNull()
      expect(screen.queryByRole('heading', { name: 'Balance rollup' })).toBeNull()
    },
  )

  it('blocks the integrations route with the real integration dependency state instead of shipping handoffs', async () => {
    render(
      <MemoryRouter initialEntries={['/admin/integrations']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Integrations unavailable' })).toBeTruthy()
    expect(
      await screen.findByText(
        'LoadArr integrations are unavailable because LoadArr does not yet have authoritative tenant-scoped integration synchronization for this tenant.',
      ),
    ).toBeTruthy()
    expect(await screen.findByText('No integration status is being shown')).toBeTruthy()
    expect(screen.queryByRole('heading', { name: 'Route and product handoffs' })).toBeNull()
    expect(vi.mocked(client.loadArrFetch)).toHaveBeenCalledWith(
      '/api/v1/integrations/items',
      'token-1',
      expect.objectContaining({
        headers: { Accept: 'application/json' },
      }),
    )
  })

  it('shows permission denial on the integrations route when the API rejects integration access', async () => {
    const defaultImplementation = vi.mocked(client.loadArrFetch).getMockImplementation()
    vi.mocked(client.loadArrFetch).mockImplementation(async (...args: Parameters<typeof client.loadArrFetch>) => {
      const [url, _accessToken, init] = args
      const method = (init?.method ?? 'GET').toUpperCase()

      if (url === '/api/v1/integrations/items' && method === 'GET') {
        return jsonResponse({ errorCode: 'forbidden' }, 403)
      }

      return defaultImplementation!(...args)
    })

    render(
      <MemoryRouter initialEntries={['/admin/integrations']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Integrations unavailable' })).toBeTruthy()
    expect(
      await screen.findByText(
        'LoadArr integrations are unavailable because this user does not have permission to read the current LoadArr data.',
      ),
    ).toBeTruthy()
    expect(screen.queryByRole('heading', { name: 'Route and product handoffs' })).toBeNull()
  })

  it('keeps receiving visible but blocks creation when authoritative location references are unavailable', async () => {
    const defaultImplementation = vi.mocked(client.loadArrFetch).getMockImplementation()
    vi.mocked(client.loadArrFetch).mockImplementation(async (...args: Parameters<typeof client.loadArrFetch>) => {
      const [url] = args
      if (url === '/api/v1/workspace/locations?active=true') {
        return jsonResponse({ error: 'unavailable' }, 503)
      }

      return defaultImplementation!(...args)
    })

    render(
      <MemoryRouter initialEntries={['/work/receiving']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Manual receiving' })).toBeTruthy()
    expect(await screen.findByText('Receiving creation unavailable')).toBeTruthy()
    expect(
      await screen.findByText(
        'Receiving creation is unavailable until authoritative StaffArr locations are available in LoadArr.',
      ),
    ).toBeTruthy()
    expect((screen.getByRole('button', { name: 'Save receiving draft' }) as HTMLButtonElement).disabled).toBe(true)
    expect((screen.getByRole('button', { name: 'Complete receiving' }) as HTMLButtonElement).disabled).toBe(true)
  })

  it('saves and completes a receiving draft from authoritative references even when the workspace summary is unavailable', async () => {
    const defaultImplementation = vi.mocked(client.loadArrFetch).getMockImplementation()
    vi.mocked(client.loadArrFetch).mockImplementation(async (...args: Parameters<typeof client.loadArrFetch>) => {
      const [url, _accessToken, init] = args
      const method = (init?.method ?? 'GET').toUpperCase()

      if (url === '/api/v1/workspace/summary') {
        return jsonResponse({ error: 'unavailable' }, 503)
      }

      if (url === '/api/v1/receiving' && method === 'POST') {
        return jsonResponse({
          id: 'recv-240627-01',
          receivingNumber: 'RCV-240627-0001',
          receivingType: 'manual',
          status: 'open',
          staffarrSiteOrgUnitId: 'staff-site-stl-north',
          staffarrSiteNameSnapshot: 'STL North Yard',
          sourceProductKey: 'loadarr',
          sourceObjectType: 'manual_receipt',
          sourceObjectId: 'manual:recv-240627-01',
          supplierNameSnapshot: 'Midwest Supplier Co.',
          startedByPersonId: 'person-1',
          completedByPersonId: null,
          startedAtUtc: '2026-06-27T15:00:00Z',
          completedAtUtc: null,
          lines: [
            {
              id: 'line-240627-01',
              supplyarrItemId: 'SUP-VALVE-KIT-A',
              itemNameSnapshot: 'Valve repair kit A',
              expectedQuantity: 3,
              receivedQuantity: 5,
              unitOfMeasure: 'each',
              warehouseLocationId: 'loc-dock-01',
              locationNameSnapshot: 'Receiving Dock 1',
              lotCode: 'L2405-77',
              serialCode: null,
              condition: 'new',
              status: 'ready_to_complete',
              discrepancyReasonCode: null,
              evidenceSummary: 'Receipt photos attached for review.',
            },
          ],
        }, 201)
      }

      if (url === '/api/v1/receiving/recv-240627-01/complete' && method === 'POST') {
        return jsonResponse({
          session: {
            id: 'recv-240627-01',
            receivingNumber: 'RCV-240627-0001',
            receivingType: 'manual',
            status: 'completed',
            staffarrSiteOrgUnitId: 'staff-site-stl-north',
            staffarrSiteNameSnapshot: 'STL North Yard',
            sourceProductKey: 'loadarr',
            sourceObjectType: 'manual_receipt',
            sourceObjectId: 'manual:recv-240627-01',
            supplierNameSnapshot: 'Midwest Supplier Co.',
            startedByPersonId: 'person-1',
            completedByPersonId: 'person-1',
            startedAtUtc: '2026-06-27T15:00:00Z',
            completedAtUtc: '2026-06-27T15:10:00Z',
            lines: [
              {
                id: 'line-240627-01',
                supplyarrItemId: 'SUP-VALVE-KIT-A',
                itemNameSnapshot: 'Valve repair kit A',
                expectedQuantity: 3,
                receivedQuantity: 5,
                unitOfMeasure: 'each',
                warehouseLocationId: 'loc-dock-01',
                locationNameSnapshot: 'Receiving Dock 1',
                lotCode: 'L2405-77',
                serialCode: null,
                condition: 'new',
                status: 'received',
                discrepancyReasonCode: null,
                evidenceSummary: 'Receipt photos attached for review.',
              },
            ],
          },
          originEvent: {
            id: 'origin-240627-01',
            originType: 'manual',
            supplyarrItemId: 'SUP-VALVE-KIT-A',
            quantity: 5,
            unitOfMeasure: 'each',
            locationNameSnapshot: 'Receiving Dock 1',
          },
          movement: {
            id: 'move-240627-01',
            movementType: 'receive',
            reasonCode: 'receiving_complete',
          },
          balance: {
            id: 'bal-240627-01',
            supplyarrItemId: 'SUP-VALVE-KIT-A',
            itemNameSnapshot: 'Valve repair kit A',
            unitOfMeasureSnapshot: 'each',
            state: 'available',
            locationId: 'loc-dock-01',
            locationNameSnapshot: 'Receiving Dock 1',
            quantityOnHand: 5,
            quantityReserved: 0,
            quantityAllocated: 0,
            quantityBlocked: 0,
            originEventType: 'manual',
            originReference: 'manual_receipt:manual:recv-240627-01',
            traceTags: ['receiving:RCV-240627-0001'],
            notes: 'Received through RCV-240627-0001.',
          },
          putawayTask: {
            id: 'task-240627-01',
            taskType: 'putaway',
            title: 'Put away Valve repair kit A',
            priority: 'normal',
            status: 'ready',
            locationNameSnapshot: 'Receiving Dock 1',
            assignedRole: 'Warehouse Associate',
            supplyarrItemId: 'SUP-VALVE-KIT-A',
            quantity: 5,
            dueAtUtc: '2026-06-27T19:00:00Z',
            requiredSignals: ['origin_event_created', 'movement_recorded', 'location_scan_required'],
          },
        }, 200)
      }

      return defaultImplementation!(...args)
    })

    const { container } = render(
      <MemoryRouter initialEntries={['/work/receiving']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Manual receiving' })).toBeTruthy()

    fireEvent.change(getFieldControl(container, 'Receiving location', '.workflow-panel'), {
      target: { value: 'loc-dock-01' },
    })
    fireEvent.change(getFieldControl(container, 'Supplier snapshot', '.workflow-panel'), {
      target: { value: 'Midwest Supplier Co.' },
    })
    fireEvent.change(getFieldControl(container, 'SupplyArr item', '.workflow-panel'), {
      target: { value: 'SUP-VALVE-KIT-A' },
    })
    fireEvent.change(getFieldControl(container, 'Expected quantity', '.workflow-panel'), {
      target: { value: '3' },
    })
    fireEvent.change(getFieldControl(container, 'Received quantity', '.workflow-panel'), {
      target: { value: '5' },
    })
    fireEvent.change(getFieldControl(container, 'Lot code', '.workflow-panel'), {
      target: { value: 'L2405-77' },
    })
    fireEvent.change(getFieldControl(container, 'Evidence summary', '.workflow-panel'), {
      target: { value: 'Receipt photos attached for review.' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Save receiving draft' }))

    await waitFor(() => {
      const postCall = vi
        .mocked(client.loadArrFetch)
        .mock.calls.find(
          ([url, _accessToken, init]) => url === '/api/v1/receiving' && (init?.method ?? 'GET').toUpperCase() === 'POST',
        )
      expect(postCall).toBeTruthy()
      const payload = JSON.parse(postCall![2]!.body as string)
      expect(payload.clientRequestId).toMatch(/^recv-/)
      expect(payload.startedByPersonId).toBe('person-1')
      expect(payload.warehouseLocationId).toBe('loc-dock-01')
      expect(payload.supplyarrItemId).toBe('SUP-VALVE-KIT-A')
    })

    expect(
      await screen.findByText(
        'Receiving draft saved. Completion will use the saved draft shown in the audit panel.',
      ),
    ).toBeTruthy()
    expect(await screen.findByText('RCV-240627-0001 · open')).toBeTruthy()
    expect((screen.getByRole('button', { name: 'Complete receiving' }) as HTMLButtonElement).disabled).toBe(false)

    fireEvent.click(screen.getByRole('button', { name: 'Complete receiving' }))

    await waitFor(() => {
      const postCall = vi
        .mocked(client.loadArrFetch)
        .mock.calls.find(
          ([url, _accessToken, init]) =>
            url === '/api/v1/receiving/recv-240627-01/complete' &&
            (init?.method ?? 'GET').toUpperCase() === 'POST',
        )
      expect(postCall).toBeTruthy()
      const payload = JSON.parse(postCall![2]!.body as string)
      expect(payload.sourceObjectId).toBe('manual:recv-240627-01')
      expect(payload.completedByPersonId).toBe('person-1')
      expect(payload.supplyarrItemId).toBe('SUP-VALVE-KIT-A')
    })

    expect(await screen.findByText('Put away Valve repair kit A · ready')).toBeTruthy()
    expect(await screen.findByText('This receiving draft is already completed.')).toBeTruthy()
    expect((screen.getByRole('button', { name: 'Complete receiving' }) as HTMLButtonElement).disabled).toBe(true)
  })

  it('blocks receiving completion when the current form no longer matches the saved draft', async () => {
    const defaultImplementation = vi.mocked(client.loadArrFetch).getMockImplementation()
    vi.mocked(client.loadArrFetch).mockImplementation(async (...args: Parameters<typeof client.loadArrFetch>) => {
      const [url, _accessToken, init] = args
      const method = (init?.method ?? 'GET').toUpperCase()

      if (url === '/api/v1/workspace/summary') {
        return jsonResponse({ error: 'unavailable' }, 503)
      }

      if (url === '/api/v1/receiving' && method === 'POST') {
        return jsonResponse({
          id: 'recv-240627-02',
          receivingNumber: 'RCV-240627-0002',
          receivingType: 'manual',
          status: 'open',
          staffarrSiteOrgUnitId: 'staff-site-stl-north',
          staffarrSiteNameSnapshot: 'STL North Yard',
          sourceProductKey: 'loadarr',
          sourceObjectType: 'manual_receipt',
          sourceObjectId: 'manual:recv-240627-02',
          supplierNameSnapshot: 'Midwest Supplier Co.',
          startedByPersonId: 'person-1',
          completedByPersonId: null,
          startedAtUtc: '2026-06-27T15:00:00Z',
          completedAtUtc: null,
          lines: [
            {
              id: 'line-240627-02',
              supplyarrItemId: 'SUP-VALVE-KIT-A',
              itemNameSnapshot: 'Valve repair kit A',
              expectedQuantity: 3,
              receivedQuantity: 5,
              unitOfMeasure: 'each',
              warehouseLocationId: 'loc-dock-01',
              locationNameSnapshot: 'Receiving Dock 1',
              lotCode: 'L2405-77',
              serialCode: null,
              condition: 'new',
              status: 'ready_to_complete',
              discrepancyReasonCode: null,
              evidenceSummary: 'Receipt photos attached for review.',
            },
          ],
        }, 201)
      }

      return defaultImplementation!(...args)
    })

    const { container } = render(
      <MemoryRouter initialEntries={['/work/receiving']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Manual receiving' })).toBeTruthy()

    fireEvent.change(getFieldControl(container, 'Receiving location', '.workflow-panel'), {
      target: { value: 'loc-dock-01' },
    })
    fireEvent.change(getFieldControl(container, 'Supplier snapshot', '.workflow-panel'), {
      target: { value: 'Midwest Supplier Co.' },
    })
    fireEvent.change(getFieldControl(container, 'SupplyArr item', '.workflow-panel'), {
      target: { value: 'SUP-VALVE-KIT-A' },
    })
    fireEvent.change(getFieldControl(container, 'Expected quantity', '.workflow-panel'), {
      target: { value: '3' },
    })
    fireEvent.change(getFieldControl(container, 'Received quantity', '.workflow-panel'), {
      target: { value: '5' },
    })
    fireEvent.change(getFieldControl(container, 'Lot code', '.workflow-panel'), {
      target: { value: 'L2405-77' },
    })
    fireEvent.change(getFieldControl(container, 'Evidence summary', '.workflow-panel'), {
      target: { value: 'Receipt photos attached for review.' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Save receiving draft' }))

    expect(
      await screen.findByText('Receiving draft saved. Completion will use the saved draft shown in the audit panel.'),
    ).toBeTruthy()

    fireEvent.change(getFieldControl(container, 'Received quantity', '.workflow-panel'), {
      target: { value: '6' },
    })

    expect(
      await screen.findByText(
        'Save the current receiving draft changes before completion so LoadArr completes the authoritative server version.',
      ),
    ).toBeTruthy()
    expect((screen.getByRole('button', { name: 'Complete receiving' }) as HTMLButtonElement).disabled).toBe(true)
  })

  it('preserves receiving form input and shows the backend message when draft save fails', async () => {
    const defaultImplementation = vi.mocked(client.loadArrFetch).getMockImplementation()
    vi.mocked(client.loadArrFetch).mockImplementation(async (...args: Parameters<typeof client.loadArrFetch>) => {
      const [url, _accessToken, init] = args
      const method = (init?.method ?? 'GET').toUpperCase()
      if (url === '/api/v1/receiving' && method === 'POST') {
        return jsonResponse(
          { message: 'SupplyArr synchronization is unavailable while saving this receiving draft.' },
          503,
        )
      }

      return defaultImplementation!(...args)
    })

    const { container } = render(
      <MemoryRouter initialEntries={['/work/receiving']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Manual receiving' })).toBeTruthy()

    fireEvent.change(getFieldControl(container, 'Receiving location', '.workflow-panel'), {
      target: { value: 'loc-dock-01' },
    })
    fireEvent.change(getFieldControl(container, 'Supplier snapshot', '.workflow-panel'), {
      target: { value: 'Midwest Supplier Co.' },
    })
    fireEvent.change(getFieldControl(container, 'SupplyArr item', '.workflow-panel'), {
      target: { value: 'SUP-VALVE-KIT-A' },
    })
    fireEvent.change(getFieldControl(container, 'Expected quantity', '.workflow-panel'), {
      target: { value: '3' },
    })
    fireEvent.change(getFieldControl(container, 'Received quantity', '.workflow-panel'), {
      target: { value: '5' },
    })
    fireEvent.change(getFieldControl(container, 'Lot code', '.workflow-panel'), {
      target: { value: 'L2405-77' },
    })
    fireEvent.change(getFieldControl(container, 'Evidence summary', '.workflow-panel'), {
      target: { value: 'Receipt photos attached for review.' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Save receiving draft' }))

    expect(await screen.findByText('LoadArr write failed')).toBeTruthy()
    expect(await screen.findByText('SupplyArr synchronization is unavailable while saving this receiving draft.')).toBeTruthy()

    expect((getFieldControl(container, 'Receiving location', '.workflow-panel') as HTMLSelectElement).value).toBe(
      'loc-dock-01',
    )
    expect((getFieldControl(container, 'Supplier snapshot', '.workflow-panel') as HTMLInputElement).value).toBe(
      'Midwest Supplier Co.',
    )
    expect((getFieldControl(container, 'SupplyArr item', '.workflow-panel') as HTMLSelectElement).value).toBe(
      'SUP-VALVE-KIT-A',
    )
    expect((getFieldControl(container, 'Received quantity', '.workflow-panel') as HTMLInputElement).value).toBe('5')
    expect((getFieldControl(container, 'Lot code', '.workflow-panel') as HTMLInputElement).value).toBe(
      'L2405-77',
    )
    expect(
      await screen.findByText(
        'SupplyArr requires serial/lot traceability capture for this item. Capture the applicable lot or serial evidence before completion.',
      ),
    ).toBeTruthy()
    expect((getFieldControl(container, 'Evidence summary', '.workflow-panel') as HTMLTextAreaElement).value).toBe(
      'Receipt photos attached for review.',
    )
  })

  it('saves a transfer draft without depending on the warehouse balance read model', async () => {
    const defaultImplementation = vi.mocked(client.loadArrFetch).getMockImplementation()
    vi.mocked(client.loadArrFetch).mockImplementation(async (...args: Parameters<typeof client.loadArrFetch>) => {
      const [url, _accessToken, init] = args
      const method = (init?.method ?? 'GET').toUpperCase()

      if (url === '/api/v1/workspace/summary') {
        return jsonResponse({ error: 'unavailable' }, 503)
      }

      if (url === '/api/v1/transfers' && method === 'POST') {
        return jsonResponse({
          id: 'xfer-240627-01',
          transferNumber: 'TRF-240627-0001',
          status: 'draft',
          transferType: 'bin_to_bin',
          staffarrSiteOrgUnitId: 'staff-site-stl-north',
          staffarrSiteNameSnapshot: 'STL North Yard',
          fromLocationId: 'loc-dock-01',
          fromLocationNameSnapshot: 'Receiving Dock 1',
          toLocationId: 'loc-quarantine-01',
          toLocationNameSnapshot: 'Quarantine Bay',
          requestedByPersonId: 'person-1',
          completedByPersonId: null,
          reasonCode: 'putaway',
          createdAtUtc: '2026-06-27T15:05:00Z',
          completedAtUtc: null,
          lines: [
            {
              id: 'xfer-line-240627-01',
              supplyarrItemId: 'SUP-VALVE-KIT-A',
              itemNameSnapshot: 'Valve repair kit A',
              quantity: 2,
              unitOfMeasure: 'each',
              lotCode: null,
              serialCode: null,
              status: 'draft',
            },
          ],
        }, 201)
      }

      return defaultImplementation!(...args)
    })

    const { container } = render(
      <MemoryRouter initialEntries={['/work/transfers']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Controlled transfer' })).toBeTruthy()
    expect((getFieldControl(container, 'Available at source', '.workflow-panel') as HTMLInputElement).value).toBe(
      'Authoritative source balance unavailable',
    )

    fireEvent.change(getFieldControl(container, 'From StaffArr location', '.workflow-panel'), {
      target: { value: 'loc-dock-01' },
    })
    fireEvent.change(getFieldControl(container, 'To StaffArr location', '.workflow-panel'), {
      target: { value: 'loc-quarantine-01' },
    })
    fireEvent.change(getFieldControl(container, 'SupplyArr item', '.workflow-panel'), {
      target: { value: 'SUP-VALVE-KIT-A' },
    })
    fireEvent.change(getFieldControl(container, 'Transfer quantity', '.workflow-panel'), {
      target: { value: '2' },
    })
    fireEvent.change(getFieldControl(container, 'Evidence summary', '.workflow-panel'), {
      target: { value: 'Move to quarantine after dock review.' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Save transfer draft' }))

    await waitFor(() => {
      const postCall = vi
        .mocked(client.loadArrFetch)
        .mock.calls.find(
          ([url, _accessToken, init]) => url === '/api/v1/transfers' && (init?.method ?? 'GET').toUpperCase() === 'POST',
        )
      expect(postCall).toBeTruthy()
      const payload = JSON.parse(postCall![2]!.body as string)
      expect(payload.clientRequestId).toMatch(/^xfer-/)
      expect(payload.requestedByPersonId).toBe('person-1')
      expect(payload.fromLocationId).toBe('loc-dock-01')
      expect(payload.toLocationId).toBe('loc-quarantine-01')
    })

    expect(
      await screen.findByText(
        'Transfer draft saved. Transfer completion is unavailable until LoadArr has authoritative warehouse movement and balance truth for this tenant.',
      ),
    ).toBeTruthy()
    expect(await screen.findByText('TRF-240627-0001 · draft')).toBeTruthy()
    expect((screen.getByRole('button', { name: 'Complete transfer' }) as HTMLButtonElement).disabled).toBe(true)
  })

  it('preserves transfer form input and shows the backend message when draft save fails', async () => {
    const defaultImplementation = vi.mocked(client.loadArrFetch).getMockImplementation()
    vi.mocked(client.loadArrFetch).mockImplementation(async (...args: Parameters<typeof client.loadArrFetch>) => {
      const [url, _accessToken, init] = args
      const method = (init?.method ?? 'GET').toUpperCase()
      if (url === '/api/v1/transfers' && method === 'POST') {
        return jsonResponse(
          { message: 'StaffArr location validation is unavailable while saving this transfer draft.' },
          503,
        )
      }

      return defaultImplementation!(...args)
    })

    const { container } = render(
      <MemoryRouter initialEntries={['/work/transfers']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Controlled transfer' })).toBeTruthy()

    fireEvent.change(getFieldControl(container, 'Transfer type', '.workflow-panel'), {
      target: { value: 'bin_to_bin' },
    })
    fireEvent.change(getFieldControl(container, 'Reason code', '.workflow-panel'), {
      target: { value: 'putaway' },
    })
    fireEvent.change(getFieldControl(container, 'From StaffArr location', '.workflow-panel'), {
      target: { value: 'loc-dock-01' },
    })
    fireEvent.change(getFieldControl(container, 'To StaffArr location', '.workflow-panel'), {
      target: { value: 'loc-quarantine-01' },
    })
    fireEvent.change(getFieldControl(container, 'SupplyArr item', '.workflow-panel'), {
      target: { value: 'SUP-VALVE-KIT-A' },
    })
    fireEvent.change(getFieldControl(container, 'Transfer quantity', '.workflow-panel'), {
      target: { value: '2' },
    })
    fireEvent.change(getFieldControl(container, 'Evidence summary', '.workflow-panel'), {
      target: { value: 'Move to quarantine after dock review.' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Save transfer draft' }))

    expect(await screen.findByText('LoadArr write failed')).toBeTruthy()
    expect(await screen.findByText('StaffArr location validation is unavailable while saving this transfer draft.')).toBeTruthy()

    expect((getFieldControl(container, 'Transfer type', '.workflow-panel') as HTMLSelectElement).value).toBe(
      'bin_to_bin',
    )
    expect((getFieldControl(container, 'Reason code', '.workflow-panel') as HTMLSelectElement).value).toBe(
      'putaway',
    )
    expect((getFieldControl(container, 'From StaffArr location', '.workflow-panel') as HTMLSelectElement).value).toBe(
      'loc-dock-01',
    )
    expect((getFieldControl(container, 'To StaffArr location', '.workflow-panel') as HTMLSelectElement).value).toBe(
      'loc-quarantine-01',
    )
    expect((getFieldControl(container, 'SupplyArr item', '.workflow-panel') as HTMLSelectElement).value).toBe(
      'SUP-VALVE-KIT-A',
    )
    expect((getFieldControl(container, 'Transfer quantity', '.workflow-panel') as HTMLInputElement).value).toBe('2')
    expect((getFieldControl(container, 'Evidence summary', '.workflow-panel') as HTMLTextAreaElement).value).toBe(
      'Move to quarantine after dock review.',
    )
  })

  it('preserves unexplained inventory form input when the write fails', async () => {
    workspaceSummary = createWorkspaceSummary()

    const { container } = render(
      <MemoryRouter initialEntries={['/work/unexplained']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Unexplained inventory' })).toBeTruthy()

    fireEvent.change(getFieldControl(container, 'Discovery source', '.workflow-panel'), {
      target: { value: 'dock_found_stock' },
    })
    fireEvent.change(getFieldControl(container, 'StaffArr location', '.workflow-panel'), {
      target: { value: 'loc-dock-01' },
    })
    fireEvent.change(getFieldControl(container, 'SupplyArr item', '.workflow-panel'), {
      target: { value: 'SUP-VALVE-KIT-A' },
    })
    fireEvent.change(getFieldControl(container, 'Expected quantity', '.workflow-panel'), { target: { value: '3' } })
    fireEvent.change(getFieldControl(container, 'Found quantity', '.workflow-panel'), { target: { value: '5' } })
    fireEvent.change(getFieldControl(container, 'Reason code', '.workflow-panel'), {
      target: { value: 'unknown_origin_review' },
    })
    fireEvent.change(getFieldControl(container, 'Discovered by', '.workflow-panel'), { target: { value: 'person-1' } })
    fireEvent.change(getFieldControl(container, 'Evidence summary', '.workflow-panel'), {
      target: { value: 'Found by the receiving dock' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Record unexplained inventory' }))

    await waitFor(() => {
      expect(vi.mocked(client.loadArrFetch)).toHaveBeenCalledWith(
        '/api/v1/unexplained-inventory',
        'token-1',
        expect.objectContaining({ method: 'POST' }),
      )
    })

    expect(await screen.findByText('LoadArr write failed')).toBeTruthy()
    expect(
      await screen.findByText(
        'Unexplained inventory creation failed. The API write was not confirmed, so no local record was created.',
      ),
    ).toBeTruthy()

    expect((getFieldControl(container, 'Discovery source', '.workflow-panel') as HTMLSelectElement).value).toBe(
      'dock_found_stock',
    )
    expect((getFieldControl(container, 'StaffArr location', '.workflow-panel') as HTMLSelectElement).value).toBe(
      'loc-dock-01',
    )
    expect((getFieldControl(container, 'SupplyArr item', '.workflow-panel') as HTMLSelectElement).value).toBe(
      'SUP-VALVE-KIT-A',
    )
    expect((getFieldControl(container, 'Expected quantity', '.workflow-panel') as HTMLInputElement).value).toBe('3')
    expect((getFieldControl(container, 'Found quantity', '.workflow-panel') as HTMLInputElement).value).toBe('5')
    expect((getFieldControl(container, 'Reason code', '.workflow-panel') as HTMLSelectElement).value).toBe(
      'unknown_origin_review',
    )
    expect((getFieldControl(container, 'Discovered by', '.workflow-panel') as HTMLInputElement).value).toBe(
      'person-1',
    )
    expect((getFieldControl(container, 'Evidence summary', '.workflow-panel') as HTMLTextAreaElement).value).toBe(
      'Found by the receiving dock',
    )
  })

  it('preserves unexplained resolution input when the write fails', async () => {
    workspaceSummary = createWorkspaceSummary({
      unexplainedInventory: [
        {
          id: 'unx-1',
          recordNumber: 'UNX-2026-0099',
          status: 'needs_review',
          discoverySource: 'cycle_count_variance',
          staffarrSiteOrgUnitId: 'staff-site-stl-north',
          staffarrSiteNameSnapshot: 'STL North Yard',
          warehouseLocationId: 'loc-dock-01',
          locationNameSnapshot: 'Receiving Dock 1',
          supplyarrItemId: 'SUP-VALVE-KIT-A',
          itemNameSnapshot: 'Valve repair kit A',
          expectedQuantity: 8,
          quantity: 11,
          varianceQuantity: 3,
          unitOfMeasure: 'each',
          lotCode: 'L2405-77',
          serialCode: null,
          discoveredByPersonId: 'person-1',
          reasonCode: 'cycle_count_variance',
          evidenceSummary: 'Found during cycle count',
          complianceEvaluationId: null,
          resolutionState: 'not_trusted_available',
          discoveredAtUtc: '2026-06-26T12:05:00Z',
          resolvedAtUtc: null,
        },
      ],
      inventory: [
        {
          id: 'bal-1',
          supplyarrItemId: 'SUP-VALVE-KIT-A',
          itemNameSnapshot: 'Valve repair kit A',
          unitOfMeasureSnapshot: 'each',
          state: 'available',
          locationId: 'loc-dock-01',
          locationNameSnapshot: 'Receiving Dock 1',
          quantityOnHand: 11,
          quantityReserved: 0,
          quantityAllocated: 0,
          quantityBlocked: 0,
          originEventType: 'purchase_receipt',
          originReference: 'PO-1001',
          traceTags: ['receipt:2026-06-26'],
          notes: 'Received from SupplyArr',
        },
      ],
    })

    const { container } = render(
      <MemoryRouter initialEntries={['/work/unexplained']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Unexplained inventory' })).toBeTruthy()
    await waitFor(() => {
      expect((getFieldControl(container, 'Queue record', '.side-panel') as HTMLSelectElement).value).toBe('unx-1')
      expect((getFieldControl(container, 'Reviewer', '.side-panel') as HTMLInputElement).value).toBe('person-1')
    })
    expect(screen.getByRole('heading', { name: 'Custody timeline' })).toBeTruthy()
    expect(screen.getByText('Recent receipt')).toBeTruthy()
    expect(screen.getByText('Custody history suggests a likely receipt or transfer source.')).toBeTruthy()

    fireEvent.change(getFieldControl(container, 'Reason code', '.side-panel'), {
      target: { value: 'supervisor_approved_valid_stock' },
    })
    fireEvent.change(getFieldControl(container, 'Evidence summary', '.side-panel'), {
      target: { value: 'Cleared after supervisor review' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Resolve' }))

    await waitFor(() => {
      expect(vi.mocked(client.loadArrFetch)).toHaveBeenCalledWith(
        '/api/v1/unexplained-inventory/unx-1/resolve',
        'token-1',
        expect.objectContaining({ method: 'POST' }),
      )
    })

    expect(await screen.findByText('LoadArr write failed')).toBeTruthy()
    expect(
      await screen.findByText(
        'Unexplained inventory resolve failed. The API write was not confirmed, so no local record was created.',
      ),
    ).toBeTruthy()

    expect((getFieldControl(container, 'Queue record', '.side-panel') as HTMLSelectElement).value).toBe('unx-1')
    expect((getFieldControl(container, 'Current state', '.side-panel') as HTMLInputElement).value).toBe(
      'not_trusted_available',
    )
    expect((getFieldControl(container, 'Reason code', '.side-panel') as HTMLSelectElement).value).toBe(
      'supervisor_approved_valid_stock',
    )
    expect((getFieldControl(container, 'Reviewer', '.side-panel') as HTMLInputElement).value).toBe('person-1')
    expect((getFieldControl(container, 'Evidence summary', '.side-panel') as HTMLTextAreaElement).value).toBe(
      'Cleared after supervisor review',
    )
  })

  it('preserves cycle count form input when the write fails', async () => {
    const { container } = render(
      <MemoryRouter initialEntries={['/work/cycle-counts']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Cycle counts' })).toBeTruthy()

    fireEvent.change(getFieldControl(container, 'StaffArr location', '.workflow-panel'), {
      target: { value: 'loc-dock-01' },
    })
    fireEvent.change(getFieldControl(container, 'SupplyArr item', '.workflow-panel'), {
      target: { value: 'SUP-VALVE-KIT-A' },
    })
    fireEvent.change(getFieldControl(container, 'Expected quantity', '.workflow-panel'), {
      target: { value: '3' },
    })
    fireEvent.change(getFieldControl(container, 'Counted quantity', '.workflow-panel'), {
      target: { value: '5' },
    })
    fireEvent.change(getFieldControl(container, 'Counted by', '.workflow-panel'), {
      target: { value: 'person-1' },
    })
    fireEvent.change(getFieldControl(container, 'Reason code', '.workflow-panel'), {
      target: { value: 'cycle_count_variance' },
    })
    fireEvent.change(getFieldControl(container, 'Evidence summary', '.workflow-panel'), {
      target: { value: 'Count variance found during nightly review.' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Record count' }))

    await waitFor(() => {
      expect(vi.mocked(client.loadArrFetch)).toHaveBeenCalledWith(
        '/api/v1/counts',
        'token-1',
        expect.objectContaining({ method: 'POST' }),
      )
    })

    expect(await screen.findByText('LoadArr write failed')).toBeTruthy()
    expect(
      await screen.findByText(
        'Count recording failed. The API write was not confirmed, so no local record was created.',
      ),
    ).toBeTruthy()

    expect((getFieldControl(container, 'StaffArr location', '.workflow-panel') as HTMLSelectElement).value).toBe(
      'loc-dock-01',
    )
    expect((getFieldControl(container, 'SupplyArr item', '.workflow-panel') as HTMLSelectElement).value).toBe(
      'SUP-VALVE-KIT-A',
    )
    expect((getFieldControl(container, 'Expected quantity', '.workflow-panel') as HTMLInputElement).value).toBe('3')
    expect((getFieldControl(container, 'Counted quantity', '.workflow-panel') as HTMLInputElement).value).toBe('5')
    expect((getFieldControl(container, 'Counted by', '.workflow-panel') as HTMLInputElement).value).toBe('person-1')
    expect((getFieldControl(container, 'Reason code', '.workflow-panel') as HTMLSelectElement).value).toBe(
      'cycle_count_variance',
    )
    expect((getFieldControl(container, 'Evidence summary', '.workflow-panel') as HTMLTextAreaElement).value).toBe(
      'Count variance found during nightly review.',
    )
  })

  it('preserves adjustment form input when the write fails', async () => {
    const { container } = render(
      <MemoryRouter initialEntries={['/work/cycle-counts']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Cycle counts' })).toBeTruthy()

    fireEvent.change(getFieldControl(container, 'Adjustment type', '.side-panel'), {
      target: { value: 'gain' },
    })
    fireEvent.change(getFieldControl(container, 'StaffArr location', '.side-panel'), {
      target: { value: 'loc-dock-01' },
    })
    fireEvent.change(getFieldControl(container, 'SupplyArr item', '.side-panel'), {
      target: { value: 'SUP-VALVE-KIT-A' },
    })
    fireEvent.change(getFieldControl(container, 'Quantity delta', '.side-panel'), {
      target: { value: '2' },
    })
    fireEvent.change(getFieldControl(container, 'Created by', '.side-panel'), {
      target: { value: 'person-1' },
    })
    fireEvent.change(getFieldControl(container, 'Reason code', '.side-panel'), {
      target: { value: 'cycle_count_variance' },
    })
    fireEvent.change(getFieldControl(container, 'Evidence summary', '.side-panel'), {
      target: { value: 'Manual adjustment requested after recount.' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Create adjustment' }))

    await waitFor(() => {
      expect(vi.mocked(client.loadArrFetch)).toHaveBeenCalledWith(
        '/api/v1/adjustments',
        'token-1',
        expect.objectContaining({ method: 'POST' }),
      )
    })

    expect(await screen.findByText('LoadArr write failed')).toBeTruthy()
    expect(
      await screen.findByText(
        'Adjustment creation failed. The API write was not confirmed, so no local record was created.',
      ),
    ).toBeTruthy()

    expect((getFieldControl(container, 'Adjustment type', '.side-panel') as HTMLSelectElement).value).toBe('gain')
    expect((getFieldControl(container, 'StaffArr location', '.side-panel') as HTMLSelectElement).value).toBe(
      'loc-dock-01',
    )
    expect((getFieldControl(container, 'SupplyArr item', '.side-panel') as HTMLSelectElement).value).toBe(
      'SUP-VALVE-KIT-A',
    )
    expect((getFieldControl(container, 'Quantity delta', '.side-panel') as HTMLInputElement).value).toBe('2')
    expect((getFieldControl(container, 'Created by', '.side-panel') as HTMLInputElement).value).toBe('person-1')
    expect((getFieldControl(container, 'Reason code', '.side-panel') as HTMLSelectElement).value).toBe(
      'cycle_count_variance',
    )
    expect((getFieldControl(container, 'Evidence summary', '.side-panel') as HTMLTextAreaElement).value).toBe(
      'Manual adjustment requested after recount.',
    )
  })

  it('blocks truck stock actions when authoritative truck stock is unavailable', async () => {
    const defaultImplementation = vi.mocked(client.loadArrFetch).getMockImplementation()
    vi.mocked(client.loadArrFetch).mockImplementation(async (...args: Parameters<typeof client.loadArrFetch>) => {
      const [url] = args
      if (url === '/api/v1/truck-stock') {
        return jsonResponse({ error: 'dependency_unavailable' }, 503)
      }

      return defaultImplementation!(...args)
    })

    render(
      <MemoryRouter initialEntries={['/work/truck-stock']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect((await screen.findAllByText('Truck stock actions unavailable')).length).toBeGreaterThan(0)
    expect(
      (
        await screen.findAllByText(
          'Truck stock actions are unavailable until LoadArr can read authoritative truck stock records for this tenant.',
        )
      ).length,
    ).toBeGreaterThan(0)
    expect((screen.getByRole('button', { name: 'Issue from truck' }) as HTMLButtonElement).disabled).toBe(true)
  })

  it('preserves truck stock form input when the write fails', async () => {
    truckStockResponse = [truckStockRecord]

    const { container } = render(
      <MemoryRouter initialEntries={['/work/truck-stock']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Truck stock' })).toBeTruthy()

    await waitFor(() => {
      expect((getFieldControl(container, 'Truck stock record', '.workflow-panel') as HTMLSelectElement).value).toBe(
        'truck-stock-17-kit',
      )
    })

    fireEvent.change(getFieldControl(container, 'Quantity', '.workflow-panel'), {
      target: { value: '2' },
    })
    fireEvent.change(getFieldControl(container, 'Reason code', '.workflow-panel'), {
      target: { value: 'route_replenishment' },
    })
    fireEvent.change(getFieldControl(container, 'Evidence summary', '.workflow-panel'), {
      target: { value: 'Issued for route replenishment.' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Issue from truck' }))

    await waitFor(() => {
      expect(vi.mocked(client.loadArrFetch)).toHaveBeenCalledWith(
        '/api/v1/truck-stock/truck-stock-17-kit/issue',
        'token-1',
        expect.objectContaining({ method: 'POST' }),
      )
    })

    expect(await screen.findByText('LoadArr write failed')).toBeTruthy()
    expect(
      await screen.findByText(
        'Truck stock issue failed. The API write was not confirmed, so no local record was created.',
      ),
    ).toBeTruthy()

    expect((getFieldControl(container, 'Truck stock record', '.workflow-panel') as HTMLSelectElement).value).toBe(
      'truck-stock-17-kit',
    )
    expect((getFieldControl(container, 'Quantity', '.workflow-panel') as HTMLInputElement).value).toBe('2')
    expect((getFieldControl(container, 'Reason code', '.workflow-panel') as HTMLSelectElement).value).toBe(
      'route_replenishment',
    )
    expect((getFieldControl(container, 'Evidence summary', '.workflow-panel') as HTMLTextAreaElement).value).toBe(
      'Issued for route replenishment.',
    )
  })

  it('blocks kit actions when authoritative kit records are unavailable', async () => {
    const defaultImplementation = vi.mocked(client.loadArrFetch).getMockImplementation()
    vi.mocked(client.loadArrFetch).mockImplementation(async (...args: Parameters<typeof client.loadArrFetch>) => {
      const [url] = args
      if (url === '/api/v1/kits') {
        return jsonResponse({ error: 'dependency_unavailable' }, 503)
      }

      return defaultImplementation!(...args)
    })

    render(
      <MemoryRouter initialEntries={['/work/kits']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect((await screen.findAllByText('Kit actions unavailable')).length).toBeGreaterThan(0)
    expect(
      (
        await screen.findAllByText(
          'Kit actions are unavailable until LoadArr can read authoritative kit records for this tenant.',
        )
      ).length,
    ).toBeGreaterThan(0)
    expect((screen.getByRole('button', { name: 'Build' }) as HTMLButtonElement).disabled).toBe(true)
  })

  it('preserves kit form input when the write fails', async () => {
    kitResponse = [kitRecord]

    const { container } = render(
      <MemoryRouter initialEntries={['/work/kits']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Kit operations' })).toBeTruthy()

    await waitFor(() => {
      expect((getFieldControl(container, 'Kit record', '.workflow-panel') as HTMLSelectElement).value).toBe(
        'kit-ppe-hazmat-04',
      )
    })

    fireEvent.change(getFieldControl(container, 'Quantity', '.workflow-panel'), {
      target: { value: '1' },
    })
    fireEvent.change(getFieldControl(container, 'Reason code', '.workflow-panel'), {
      target: { value: 'kit_build' },
    })
    fireEvent.change(getFieldControl(container, 'Evidence summary', '.workflow-panel'), {
      target: { value: 'Built one kit after replenishment.' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Build' }))

    await waitFor(() => {
      expect(vi.mocked(client.loadArrFetch)).toHaveBeenCalledWith(
        '/api/v1/kits/kit-ppe-hazmat-04/build',
        'token-1',
        expect.objectContaining({ method: 'POST' }),
      )
    })

    expect(await screen.findByText('LoadArr write failed')).toBeTruthy()
    expect(
      await screen.findByText(
        'Kit build failed. The API write was not confirmed, so no local record was created.',
      ),
    ).toBeTruthy()

    expect((getFieldControl(container, 'Kit record', '.workflow-panel') as HTMLSelectElement).value).toBe(
      'kit-ppe-hazmat-04',
    )
    expect((getFieldControl(container, 'Quantity', '.workflow-panel') as HTMLInputElement).value).toBe('1')
    expect((getFieldControl(container, 'Reason code', '.workflow-panel') as HTMLSelectElement).value).toBe(
      'kit_build',
    )
    expect((getFieldControl(container, 'Evidence summary', '.workflow-panel') as HTMLTextAreaElement).value).toBe(
      'Built one kit after replenishment.',
    )
  })

  it('surfaces investigation history for cycle count variance', async () => {
    workspaceSummary = createWorkspaceSummary({
      inventory: [
        {
          id: 'bal-1',
          supplyarrItemId: 'SUP-VALVE-KIT-A',
          itemNameSnapshot: 'Valve repair kit A',
          unitOfMeasureSnapshot: 'each',
          state: 'available',
          locationId: 'loc-dock-01',
          locationNameSnapshot: 'Receiving Dock 1',
          quantityOnHand: 5,
          quantityReserved: 0,
          quantityAllocated: 0,
          quantityBlocked: 0,
          originEventType: 'purchase_receipt',
          originReference: 'PO-1001',
          traceTags: ['receipt:2026-06-26'],
          notes: 'Received from SupplyArr',
        },
      ],
    })
    countsResponse = [
      {
        id: 'cnt-1',
        countNumber: 'CNT-2026-0001',
        status: 'variance_pending_approval',
        countType: 'cycle_count',
        staffarrSiteOrgUnitId: 'staff-site-stl-north',
        staffarrSiteNameSnapshot: 'STL North Yard',
        warehouseLocationId: 'loc-dock-01',
        locationNameSnapshot: 'Receiving Dock 1',
        supplyarrItemId: 'SUP-VALVE-KIT-A',
        itemNameSnapshot: 'Valve repair kit A',
        expectedQuantity: 3,
        countedQuantity: 5,
        varianceQuantity: 2,
        unitOfMeasure: 'each',
        countedByPersonId: 'person-1',
        approvedByPersonId: null,
        reasonCode: 'cycle_count_variance',
        inventoryAdjustmentId: null,
        evidenceSummary: 'Found during cycle count',
        createdAtUtc: '2026-06-26T12:05:00Z',
        completedAtUtc: '2026-06-26T12:10:00Z',
        approvedAtUtc: null,
        updatedAtUtc: '2026-06-26T12:10:00Z',
      },
    ]
    adjustmentsResponse = [
      {
        id: 'adj-1',
        adjustmentNumber: 'ADJ-2026-0001',
        status: 'posted',
        adjustmentType: 'gain',
        staffarrSiteOrgUnitId: 'staff-site-stl-north',
        staffarrSiteNameSnapshot: 'STL North Yard',
        warehouseLocationId: 'loc-dock-01',
        locationNameSnapshot: 'Receiving Dock 1',
        supplyarrItemId: 'SUP-VALVE-KIT-A',
        itemNameSnapshot: 'Valve repair kit A',
        quantityDelta: 2,
        unitOfMeasure: 'each',
        reasonCode: 'cycle_count_gain',
        createdByPersonId: 'person-1',
        approvedByPersonId: 'person-2',
        inventoryOriginEventId: 'cnt-1',
        evidenceSummary: 'Approved after recount',
        createdAtUtc: '2026-06-26T12:12:00Z',
        approvedAtUtc: '2026-06-26T12:14:00Z',
        updatedAtUtc: '2026-06-26T12:14:00Z',
      },
    ]

    render(
      <MemoryRouter initialEntries={['/work/cycle-counts']}>
        <QueryClientProvider client={new QueryClient()}>
          <App />
        </QueryClientProvider>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Cycle counts' })).toBeTruthy()
    expect(screen.getByRole('heading', { name: 'Investigation' })).toBeTruthy()
    expect(screen.getByText('Positive variance points to receipt or transfer timing.')).toBeTruthy()
    expect(screen.getByText('Recent receipt')).toBeTruthy()
    expect(screen.getByText('Prior adjustment')).toBeTruthy()
    expect(screen.getByText('Recheck receipts, transfers, and recount before approving the variance.')).toBeTruthy()
  })
})
