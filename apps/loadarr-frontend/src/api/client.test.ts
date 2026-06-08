import { afterEach, describe, expect, it, vi } from 'vitest'
import {
  LoadArrApiError,
  getLoadArrExpectedReceipts,
  getLoadArrPermissionCatalog,
  getLoadArrRouteSurfaceRecord,
  getSessionBootstrap,
  loadArrFetch,
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
