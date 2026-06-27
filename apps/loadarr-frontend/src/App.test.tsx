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
  isLotControlled: true,
  isSerialControlled: false,
  isHazardous: false,
  requiresSds: false,
  updatedAtUtc: '2026-06-01T00:00:00Z',
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

const createMutationResponse = {
  record: {
    id: 'unx-created-1',
    recordNumber: 'UNX-2026-0001',
    status: 'needs_review',
    discoverySource: 'cycle_count_variance',
    staffarrSiteOrgUnitId: 'staff-site-stl-north',
    staffarrSiteNameSnapshot: 'STL North Yard',
    warehouseLocationId: 'loc-dock-01',
    locationNameSnapshot: 'Receiving Dock 1',
    supplyarrItemId: 'SUP-VALVE-KIT-A',
    itemNameSnapshot: 'Valve repair kit A',
    expectedQuantity: 3,
    quantity: 5,
    varianceQuantity: 2,
    unitOfMeasure: 'each',
    lotCode: 'L2405-77',
    serialCode: null,
    discoveredByPersonId: 'person-1',
    reasonCode: 'unknown_origin_review',
    evidenceSummary: 'Found during cycle count',
    complianceEvaluationId: null,
    resolutionState: 'not_trusted_available',
    discoveredAtUtc: '2026-06-26T12:05:00Z',
    resolvedAtUtc: null,
  },
  originEvent: null,
  movement: null,
  reviewTask: {
    id: 'task-unx-1',
    taskType: 'unexplained_inventory_review',
    title: 'Resolve unexplained Valve repair kit A',
    priority: 'high',
    status: 'ready',
    locationNameSnapshot: 'Receiving Dock 1',
    assignedRole: 'Inventory Supervisor',
    supplyarrItemId: 'SUP-VALVE-KIT-A',
    quantity: 5,
    dueAtUtc: '2026-06-26T15:05:00Z',
    requiredSignals: ['approval_required', 'origin_unknown', 'stock_not_available'],
  },
}

const resolveMutationResponse = {
  record: {
    id: 'unx-1',
    recordNumber: 'UNX-2026-0099',
    status: 'resolved_valid_stock',
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
    resolutionState: 'trusted_available',
    discoveredAtUtc: '2026-06-26T12:05:00Z',
    resolvedAtUtc: '2026-06-26T12:15:00Z',
  },
  originEvent: {
    id: 'origin-unx-1',
    originType: 'unexplained_inventory_resolution',
    supplyarrItemId: 'SUP-VALVE-KIT-A',
    quantity: 11,
    unitOfMeasure: 'each',
    locationNameSnapshot: 'Receiving Dock 1',
  },
  movement: {
    id: 'move-unx-1',
    movementType: 'adjust',
    reasonCode: 'supervisor_approved_valid_stock',
  },
  reviewTask: null,
}

let workspaceSummary = createWorkspaceSummary()
let supplyArrItemReferences = [supplyItemReference]
let countsResponse: any[] = []
let adjustmentsResponse: any[] = []

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

    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'loadarr-ops',
      isPlatformAdmin: false,
      productKey: 'loadarr',
      hasLoadArrAccess: true,
      launchableProductKeys: ['loadarr'],
    } as any)
    vi.mocked(client.getLoadArrPermissionCatalog).mockResolvedValue({ permissions: [] } as any)
    vi.mocked(client.loadArrFetch).mockImplementation(async (url: string, _accessToken?: string, init?: RequestInit) => {
      const method = (init?.method ?? 'GET').toUpperCase()
      const body = typeof init?.body === 'string' ? JSON.parse(init.body) : null

      if (url === '/api/v1/workspace/summary' && method === 'GET') {
        return jsonResponse(workspaceSummary)
      }

      if (url === '/api/v1/workspace/supplyarr-item-references' && method === 'GET') {
        return jsonResponse({ items: supplyArrItemReferences })
      }

      if (url === '/api/v1/counts' && method === 'GET') {
        return jsonResponse({ items: countsResponse })
      }

      if (url === '/api/v1/receiving' && method === 'GET') {
        return jsonResponse({ items: [] })
      }

      if (url === '/api/v1/transfers' && method === 'GET') {
        return jsonResponse({ items: [] })
      }

      if (url === '/api/v1/adjustments' && method === 'GET') {
        return jsonResponse({ items: adjustmentsResponse })
      }

      if (url === '/api/v1/truck-stock' && method === 'GET') {
        return jsonResponse({ items: [] })
      }

      if (url === '/api/v1/kits' && method === 'GET') {
        return jsonResponse({ items: [] })
      }

      if (url === '/api/v1/unexplained-inventory' && method === 'POST') {
        expect(body).toMatchObject({
          discoverySource: 'dock_found_stock',
          warehouseLocationId: 'loc-dock-01',
          supplyarrItemId: 'SUP-VALVE-KIT-A',
          expectedQuantity: 3,
          quantity: 5,
          discoveredByPersonId: 'person-1',
          reasonCode: 'unknown_origin_review',
          evidenceSummary: 'Found by the receiving dock',
          complianceEvaluationId: null,
        })

        return jsonResponse(createMutationResponse, 201)
      }

      if (url === '/api/v1/unexplained-inventory/unx-1/resolve' && method === 'POST') {
        expect(body).toMatchObject({
          approvedByPersonId: 'person-1',
          reasonCode: 'supervisor_approved_valid_stock',
          complianceEvaluationId: null,
          evidenceSummary: 'Cleared after supervisor review',
        })

        return jsonResponse(resolveMutationResponse)
      }

      return jsonResponse({ items: [] })
    })
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
    sessionStorage.clear()
  })

  it('records unexplained inventory from the unexplained workflow', async () => {
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

    expect(await screen.findByText(/UNX-2026-0001/)).toBeTruthy()
    expect(await screen.findByText(/not_trusted_available/)).toBeTruthy()
    expect(await screen.findByText(/Resolve unexplained Valve repair kit A/)).toBeTruthy()
  })

  it('defaults unexplained resolution to the first record and resolves it', async () => {
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

    expect(await screen.findByText(/UNX-2026-0099.*trusted_available/)).toBeTruthy()
    expect(await screen.findByText(/supervisor_approved_valid_stock/)).toBeTruthy()
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
