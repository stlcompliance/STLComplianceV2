import { afterEach, describe, expect, it, vi } from 'vitest'
import {
  getInventoryLocations,
  getParts,
  getPurchaseOrders,
  getBackorders,
  getVendorReturns,
  getPricingSnapshots,
  getLeadTimeSnapshots,
  getReceivingReceipts,
  getPurchaseRequests,
  getVendors,
  SupplyArrApiError,
} from './client'

describe('supplyarr api client', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('loads vendors on success', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [
          {
            partyId: '11111111-1111-1111-1111-111111111111',
            partyKey: 'acme-parts',
            partyType: 'vendor',
            displayName: 'Acme Parts Co.',
            legalName: 'Acme Parts Company LLC',
            taxIdentifier: null,
            approvalStatus: 'pending',
            status: 'active',
            notes: '',
            contacts: [],
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ],
      }),
    )

    const vendors = await getVendors('token')
    expect(vendors).toHaveLength(1)
    expect(vendors[0].partyKey).toBe('acme-parts')
  })

  it('loads parts on success', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [
          {
            partId: '33333333-3333-3333-3333-333333333333',
            partKey: 'filter-001',
            catalogId: null,
            catalogKey: null,
            displayName: 'Primary Oil Filter',
            description: '',
            categoryKey: 'filters',
            unitOfMeasure: 'each',
            manufacturerName: '',
            manufacturerPartNumber: '',
            status: 'active',
            manufacturerAliases: [],
            vendorLinks: [],
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ],
      }),
    )

    const parts = await getParts('token')
    expect(parts).toHaveLength(1)
    expect(parts[0].partKey).toBe('filter-001')
  })

  it('loads inventory locations on success', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [
          {
            locationId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
            locationKey: 'main-wh',
            name: 'Main Warehouse',
            locationType: 'warehouse',
            addressLine: '',
            status: 'active',
            binCount: 0,
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ],
      }),
    )

    const locations = await getInventoryLocations('token')
    expect(locations).toHaveLength(1)
    expect(locations[0].locationKey).toBe('main-wh')
  })

  it('loads purchase requests on success', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [
          {
            purchaseRequestId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
            requestKey: 'pr-001',
            title: 'Restock',
            notes: '',
            status: 'draft',
            vendorPartyId: null,
            vendorPartyKey: null,
            vendorDisplayName: null,
            requestedByUserId: '11111111-1111-1111-1111-111111111111',
            submittedAt: null,
            submittedByUserId: null,
            approvedAt: null,
            approvedByUserId: null,
            rejectedAt: null,
            rejectedByUserId: null,
            rejectionReason: '',
            lines: [],
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ],
      }),
    )

    const requests = await getPurchaseRequests('token')
    expect(requests).toHaveLength(1)
    expect(requests[0].requestKey).toBe('pr-001')
  })

  it('loads purchase orders on success', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [
          {
            purchaseOrderId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
            orderKey: 'po-001',
            title: 'Vendor order',
            notes: '',
            status: 'draft',
            purchaseRequestId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
            purchaseRequestKey: 'pr-001',
            vendorPartyId: 'dddddddd-dddd-dddd-dddd-dddddddddddd',
            vendorPartyKey: 'vendor-a',
            vendorDisplayName: 'Acme',
            createdByUserId: '11111111-1111-1111-1111-111111111111',
            approvedAt: null,
            approvedByUserId: null,
            issuedAt: null,
            issuedByUserId: null,
            cancelledAt: null,
            cancelledByUserId: null,
            cancellationReason: '',
            lines: [],
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ],
      }),
    )

    const orders = await getPurchaseOrders('token')
    expect(orders).toHaveLength(1)
    expect(orders[0].orderKey).toBe('po-001')
  })

  it('loads receiving receipts on success', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [
          {
            receivingReceiptId: 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
            receiptKey: 'rcpt-001',
            status: 'posted',
            purchaseOrderId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
            purchaseOrderKey: 'po-001',
            inventoryBinId: 'ffffffff-ffff-ffff-ffff-ffffffffffff',
            binKey: 'a-01',
            binName: 'Aisle 01',
            inventoryLocationId: '99999999-9999-9999-9999-999999999999',
            locationKey: 'main-wh',
            locationName: 'Main',
            notes: '',
            createdByUserId: '11111111-1111-1111-1111-111111111111',
            postedAt: '2026-05-27T00:00:00Z',
            postedByUserId: '11111111-1111-1111-1111-111111111111',
            lines: [],
            exceptions: [],
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ],
      }),
    )

    const receipts = await getReceivingReceipts('token')
    expect(receipts).toHaveLength(1)
    expect(receipts[0].receiptKey).toBe('rcpt-001')
    expect(receipts[0].exceptions).toEqual([])
  })

  it('loads backorders on success', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [
          {
            backorderId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
            backorderKey: 'bo-001',
            status: 'open',
            sourceType: 'receipt_post',
            purchaseOrderId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
            purchaseOrderKey: 'po-001',
            purchaseOrderLineId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
            purchaseOrderLineNumber: 1,
            purchaseRequestId: 'dddddddd-dddd-dddd-dddd-dddddddddddd',
            purchaseRequestKey: 'pr-001',
            purchaseRequestLineId: 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
            receivingReceiptId: 'ffffffff-ffff-ffff-ffff-ffffffffffff',
            receivingReceiptKey: null,
            receivingReceiptLineId: '11111111-1111-1111-1111-111111111111',
            partId: '22222222-2222-2222-2222-222222222222',
            partKey: 'filter-001',
            partDisplayName: 'Filter',
            quantityBackordered: 2,
            quantityFulfilled: 0,
            quantityOpen: 2,
            expectedBy: null,
            notes: '',
            createdByUserId: '33333333-3333-3333-3333-333333333333',
            fulfilledByUserId: null,
            fulfilledAt: null,
            cancelledByUserId: null,
            cancelledAt: null,
            cancellationReason: '',
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ],
      }),
    )

    const backorders = await getBackorders('token', { status: 'open' })
    expect(backorders).toHaveLength(1)
    expect(backorders[0].purchaseRequestKey).toBe('pr-001')
  })

  it('loads vendor returns on success', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [
          {
            returnId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
            returnKey: 'ret-001',
            status: 'posted',
            sourceType: 'stock',
            vendorPartyId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
            vendorPartyKey: 'acme',
            vendorDisplayName: 'Acme',
            purchaseOrderId: null,
            purchaseOrderKey: null,
            purchaseRequestId: null,
            purchaseRequestKey: null,
            inventoryBinId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
            inventoryBinKey: 'bin-1',
            inventoryBinName: 'Main',
            inventoryLocationId: 'dddddddd-dddd-dddd-dddd-dddddddddddd',
            inventoryLocationKey: 'wh-1',
            inventoryLocationName: 'Warehouse',
            rmaNumber: 'RMA-42',
            notes: '',
            createdByUserId: 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
            postedByUserId: 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
            postedAt: '2026-05-27T00:00:00Z',
            cancelledByUserId: null,
            cancelledAt: null,
            cancellationReason: '',
            lines: [],
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ],
      }),
    )

    const returns = await getVendorReturns('token', { status: 'posted' })
    expect(returns).toHaveLength(1)
    expect(returns[0].rmaNumber).toBe('RMA-42')
  })

  it('loads pricing snapshots on success', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [
          {
            pricingSnapshotId: '11111111-1111-1111-1111-111111111111',
            snapshotKey: 'price-q2',
            partVendorLinkId: '22222222-2222-2222-2222-222222222222',
            partId: '33333333-3333-3333-3333-333333333333',
            partKey: 'filter-001',
            partDisplayName: 'Oil Filter',
            vendorPartyId: '44444444-4444-4444-4444-444444444444',
            vendorPartyKey: 'acme',
            vendorDisplayName: 'Acme',
            vendorPartNumber: 'V-001',
            unitPrice: 19.99,
            currencyCode: 'USD',
            minimumOrderQuantity: null,
            effectiveFrom: '2026-05-01T00:00:00Z',
            effectiveTo: null,
            source: 'manual',
            notes: '',
            isCurrent: true,
            createdByUserId: '55555555-5555-5555-5555-555555555555',
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ],
      }),
    )

    const snapshots = await getPricingSnapshots('token')
    expect(snapshots).toHaveLength(1)
    expect(snapshots[0].isCurrent).toBe(true)
  })

  it('loads lead-time snapshots on success', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [
          {
            leadTimeSnapshotId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
            snapshotKey: 'lt-q2',
            partVendorLinkId: '22222222-2222-2222-2222-222222222222',
            partId: '33333333-3333-3333-3333-333333333333',
            partKey: 'filter-001',
            partDisplayName: 'Oil Filter',
            vendorPartyId: '44444444-4444-4444-4444-444444444444',
            vendorPartyKey: 'acme',
            vendorDisplayName: 'Acme',
            vendorPartNumber: 'V-001',
            leadTimeDays: 7,
            effectiveFrom: '2026-05-01T00:00:00Z',
            effectiveTo: null,
            source: 'quote',
            notes: '',
            isCurrent: true,
            createdByUserId: '55555555-5555-5555-5555-555555555555',
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ],
      }),
    )

    const snapshots = await getLeadTimeSnapshots('token')
    expect(snapshots).toHaveLength(1)
    expect(snapshots[0].leadTimeDays).toBe(7)
  })

  it('loads availability snapshots on success', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [
          {
            availabilitySnapshotId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
            snapshotKey: 'avail-q2',
            partVendorLinkId: '22222222-2222-2222-2222-222222222222',
            partId: '33333333-3333-3333-3333-333333333333',
            partKey: 'filter-001',
            partDisplayName: 'Oil Filter',
            vendorPartyId: '44444444-4444-4444-4444-444444444444',
            vendorPartyKey: 'acme',
            vendorDisplayName: 'Acme',
            vendorPartNumber: 'V-001',
            quantityAvailable: 50,
            availabilityStatus: 'in_stock',
            effectiveFrom: '2026-05-01T00:00:00Z',
            effectiveTo: null,
            source: 'vendor_feed',
            notes: '',
            isCurrent: true,
            createdByUserId: '55555555-5555-5555-5555-555555555555',
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ],
      }),
    )

    const { getAvailabilitySnapshots } = await import('./client')
    const snapshots = await getAvailabilitySnapshots('token')
    expect(snapshots).toHaveLength(1)
    expect(snapshots[0].availabilityStatus).toBe('in_stock')
  })

  it('parses reorder evaluation response', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        status: 200,
        json: async () => ({
          evaluatedAt: '2026-05-27T12:00:00Z',
          suggestions: [
            {
              partId: 'part-1',
              partKey: 'filter-01',
              displayName: 'Oil Filter',
              unitOfMeasure: 'each',
              reorderPoint: 10,
              reorderQuantity: 24,
              quantityOnHand: 2,
              quantityReserved: 0,
              quantityAvailable: 2,
              suggestedOrderQuantity: 24,
              preferredVendorPartyId: null,
              preferredVendorPartyKey: null,
              preferredVendorDisplayName: null,
              hasOpenPurchaseRequest: false,
              skipReason: null,
            },
          ],
        }),
      }),
    )

    const { getReorderEvaluation } = await import('./client')
    const evaluation = await getReorderEvaluation('token')
    expect(evaluation.suggestions).toHaveLength(1)
    expect(evaluation.suggestions[0].suggestedOrderQuantity).toBe(24)
  })

  it('throws SupplyArrApiError on forbidden', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: false,
        status: 403,
        text: async () => 'forbidden',
      }),
    )

    await expect(getVendors('token')).rejects.toBeInstanceOf(SupplyArrApiError)
  })
})
