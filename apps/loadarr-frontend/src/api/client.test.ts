import { afterEach, describe, expect, it, vi } from 'vitest'
import {
  LoadArrApiError,
  getLoadArrExpectedReceipts,
  getLoadArrPermissionCatalog,
  getLoadArrRouteSurfaceRecord,
  getLoadArrTenantSettings,
  getSessionBootstrap,
  loadArrFetch,
  replaceLoadArrTenantSettings,
  redeemHandoff,
} from './client'

describe('loadarr api client', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('surfaces session bootstrap auth failures with status metadata', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(JSON.stringify({ code: 'auth.session_expired' }), {
        status: 401,
        headers: { 'Content-Type': 'application/json' },
      }),
    )

    await expect(getSessionBootstrap('token-123')).rejects.toMatchObject({
      name: 'LoadArrApiError',
      status: 401,
      body: '{"code":"auth.session_expired"}',
    })
  })

  it('preserves handoff API response bodies for launch failure messaging', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(JSON.stringify({ code: 'launch.handoff_expired' }), {
        status: 401,
        headers: { 'Content-Type': 'application/json' },
      }),
    )

    const error = await redeemHandoff('handoff-123').catch((caught) => caught)

    expect(error).toBeInstanceOf(LoadArrApiError)
    expect(error).toMatchObject({
      status: 401,
      body: '{"code":"launch.handoff_expired"}',
    })
  })

  it('normalizes legacy launch-key aliases in session bootstrap responses', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          userId: 'user-1',
          personId: 'person-1',
          tenantId: 'tenant-1',
          sessionId: 'session-1',
          tenantRoleKey: 'tenant_member',
          isPlatformAdmin: false,
          productKey: 'loadarr',
          hasLoadArrAccess: true,
          launchableProductKeys: ['loadarr', 'ordarr'],
        }),
        {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        },
      ),
    )

    await expect(getSessionBootstrap('token-123')).resolves.toMatchObject({
      hasLoadArrAccess: true,
      launchableProductKeys: ['loadarr', 'ordarr'],
    })
  })

  it('sends v1 requests with bearer auth headers instead of credentialed same-origin mode', async () => {
    const fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(null, { status: 200 }),
    )

    await loadArrFetch('/api/v1/workspace/summary', 'token-123', {
      headers: { Accept: 'application/json' },
    })

    expect(fetchSpy).toHaveBeenCalledWith(
      '/api/v1/workspace/summary',
      expect.objectContaining({
        headers: expect.any(Headers),
      }),
    )

    const [, init] = fetchSpy.mock.calls[0]!
    const headers = new Headers(init?.headers)

    expect(init?.credentials).toBeUndefined()
    expect(headers.get('Accept')).toBe('application/json')
    expect(headers.get('Authorization')).toBe('Bearer token-123')
  })

  it('loads the permission catalog with authorized requests', async () => {
    const fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          permissions: [
            {
              productKey: 'loadarr',
              permissionKey: 'loadarr.dashboard.read',
              label: 'Read Dashboard',
              description: 'View the LoadArr operational dashboard.',
              scope: 'product',
              sensitivity: 'standard',
              status: 'active',
            },
          ],
        }),
        {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        },
      ),
    )

    const response = await getLoadArrPermissionCatalog('token-456')

    expect(fetchSpy).toHaveBeenCalledWith(
      '/api/v1/admin/permissions',
      expect.objectContaining({
        headers: expect.any(Headers),
      }),
    )

    const [, init] = fetchSpy.mock.calls[0]!
    const headers = new Headers(init?.headers)
    expect(headers.get('Authorization')).toBe('Bearer token-456')
    expect(response.permissions).toHaveLength(1)
    expect(response.permissions[0]).toMatchObject({
      productKey: 'loadarr',
      permissionKey: 'loadarr.dashboard.read',
    })
  })

  it('loads LoadArr tenant settings with bearer auth', async () => {
    const fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          version: 1,
          rowVersion: 'rv-001',
          createdAt: '2026-06-18T00:00:00Z',
          createdByPersonId: 'person-1',
          updatedAt: '2026-06-18T00:00:00Z',
          updatedByPersonId: 'person-1',
          updatedByDisplayNameSnapshot: 'Morgan Ellis',
          settings: {
            receiving: {
              allowOverReceipt: true,
              overReceiptTolerancePercent: 5,
            },
          },
          validation: {
            errors: [],
            warnings: [],
            dependencyHints: [],
          },
        }),
        {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        },
      ),
    )

    const response = await getLoadArrTenantSettings('token-settings')

    expect(fetchSpy).toHaveBeenCalledWith(
      '/api/v1/loadarr/tenant-settings',
      expect.objectContaining({
        headers: expect.any(Headers),
      }),
    )

    const [, init] = fetchSpy.mock.calls[0]!
    const headers = new Headers(init?.headers)
    expect(headers.get('Authorization')).toBe('Bearer token-settings')
    expect(response).toMatchObject({
      version: 1,
      rowVersion: 'rv-001',
      settings: {
        receiving: {
          allowOverReceipt: true,
        },
      },
    })
  })

  it('replaces LoadArr tenant settings with row version, reason, and acknowledged warnings', async () => {
    const fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          version: 2,
          rowVersion: 'rv-002',
          createdAt: '2026-06-18T00:00:00Z',
          createdByPersonId: 'person-1',
          updatedAt: '2026-06-18T00:05:00Z',
          updatedByPersonId: 'person-1',
          updatedByDisplayNameSnapshot: 'Morgan Ellis',
          settings: {
            inventoryControl: {
              allowNegativeInventory: true,
            },
          },
          validation: {
            errors: [],
            warnings: [],
            dependencyHints: [],
          },
        }),
        {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        },
      ),
    )

    await replaceLoadArrTenantSettings(
      'token-settings',
      'rv-001',
      { inventoryControl: { allowNegativeInventory: true } },
      'Temporary recovery policy',
      ['loadarr.ui.inventory.negative_inventory'],
    )

    expect(fetchSpy).toHaveBeenCalledWith(
      '/api/v1/loadarr/tenant-settings',
      expect.objectContaining({
        method: 'PUT',
        headers: expect.any(Headers),
      }),
    )

    const [, init] = fetchSpy.mock.calls[0]!
    const headers = new Headers(init?.headers)
    expect(headers.get('Authorization')).toBe('Bearer token-settings')
    expect(JSON.parse(String(init?.body))).toEqual({
      rowVersion: 'rv-001',
      settings: {
        inventoryControl: {
          allowNegativeInventory: true,
        },
      },
      reason: 'Temporary recovery policy',
      warningsAcknowledged: ['loadarr.ui.inventory.negative_inventory'],
    })
  })

  it('loads route surface collections with query filters and bearer auth', async () => {
    const fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          items: [
            {
              id: 'task-receive-24018',
              expectedReceiptNumber: 'EXP-PO-10492',
              status: 'ready_to_receive',
              sourceProductKey: 'supplyarr',
              sourceObjectType: 'purchase_order',
              sourceObjectId: 'PO-10492',
              supplierNameSnapshot: 'Midwest Fleet Supply',
              staffarrSiteOrgUnitId: 'staff-site-stl-north',
              staffarrSiteNameSnapshot: 'STL North Yard',
              warehouseLocationId: 'loc-dock-01',
              locationNameSnapshot: 'Receiving Dock 1',
              supplyarrItemId: 'SUP-VALVE-KIT-A',
              itemNameSnapshot: 'Valve repair kit A',
              expectedQuantity: 38,
              receivedQuantity: 0,
              unitOfMeasure: 'each',
              expectedAtUtc: '2026-06-03T15:00:00Z',
              lastUpdatedAtUtc: '2026-06-02T20:10:00Z',
              receivingSessionId: 'recv-24018',
              signals: ['purchase_receipt'],
            },
          ],
          total: 1,
        }),
        {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        },
      ),
    )

    const response = await getLoadArrExpectedReceipts('token-789', {
      status: 'ready_to_receive',
      locationId: 'loc-dock-01',
      includeEmpty: false,
      ignored: undefined,
    })

    expect(fetchSpy).toHaveBeenCalledWith(
      '/api/v1/loadarr/expected-receipts?status=ready_to_receive&locationId=loc-dock-01&includeEmpty=false',
      expect.objectContaining({
        headers: expect.any(Headers),
      }),
    )

    const [, init] = fetchSpy.mock.calls[0]!
    const headers = new Headers(init?.headers)
    expect(headers.get('Authorization')).toBe('Bearer token-789')
    expect(response.total).toBe(1)
    expect(response.items[0]).toMatchObject({
      id: 'task-receive-24018',
      expectedReceiptNumber: 'EXP-PO-10492',
    })
  })

  it('loads route surface detail records from canonical collection paths', async () => {
    const fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(JSON.stringify({ id: 'handoff-rt-7781', status: 'ready' }), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      }),
    )

    const response = await getLoadArrRouteSurfaceRecord<{ id: string; status: string }>(
      'token-000',
      'shipping',
      'handoff-rt-7781',
    )

    expect(fetchSpy).toHaveBeenCalledWith(
      '/api/v1/loadarr/shipping/handoff-rt-7781',
      expect.objectContaining({
        headers: expect.any(Headers),
      }),
    )
    expect(response).toEqual({ id: 'handoff-rt-7781', status: 'ready' })
  })
})
