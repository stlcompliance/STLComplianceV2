import { afterEach, describe, expect, it, vi } from 'vitest'
import {
  getApprovalReminderSettings,
  getApprovalRemindersDashboard,
  listVendorRestrictions,
  listSupplierRestrictionsBySupplier,
  getSupplierRestrictionEnforcement,
  createSupplierRestriction,
  liftSupplierRestriction,
  getSupplierOnboardingDocumentRequirements,
  listPendingSupplierOnboarding,
  startSupplierOnboarding,
  getSupplierOnboarding,
  listSupplierComplianceDocuments,
  approveSupplierComplianceDocument,
  getEmergencyPurchases,
  listPendingEmergencyPurchases,
  expeditedSubmitEmergencyPurchase,
  managerOverrideApproveEmergencyPurchase,
  issueEmergencyPurchaseOrder,
  forgivingSearch,
  listAuditHistory,
  listSupplierIncidents,
  listSupplierIncidentsForSupplier,
  createSupplierIncident,
  startSupplierIncidentInvestigation,
  applySupplierIncidentProcurementRestriction,
  getProcurementExceptionEscalationSettings,
  listProcurementExceptionResolutionTemplates,
  getProcurementCoordinationDashboard,
  getProcurementCoordinationSettings,
  getAvailabilitySnapshotSettings,
  getIntegrationEventSettings,
  getLeadTimeSnapshotSettings,
  getPriceSnapshotSettings,
  getProcurementNotificationSettings,
  getDemandProcessingSettings,
  getDemandProcessingDashboard,
  getDemandRefs,
  getRfqs,
  getReorderEvaluation,
  listWarrantyClaims,
  getParts,
  getSubstitutions,
  getPurchaseOrders,
  getBackorders,
  getVendorReturns,
  getVendorEmailInbox,
  ingestVendorEmailInbox,
  getPricingSnapshots,
  getLeadTimeSnapshots,
  getPurchaseRequests,
  getPurchasingReportSummary,
  getSupplierReportDetail,
  getSupplierReportSummary,
  getSupplierDirectory,
  getSupplyReadinessDashboard,
  SupplyArrApiError,
} from './client'

describe('supplyarr api client', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('loads the supplier directory on success', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [
          {
            supplierId: '11111111-1111-1111-1111-111111111111',
            supplierKey: 'acme-parts',
            supplierType: 'supplier',
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

    const suppliers = await getSupplierDirectory('token')
    expect(suppliers).toHaveLength(1)
    expect(suppliers[0].supplierKey).toBe('acme-parts')
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

  it('loads substitutions with an optional part filter', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => [
        {
          partId: '33333333-3333-3333-3333-333333333333',
          partKey: 'filter-001',
          partDisplayName: 'Primary Oil Filter',
          aliasId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
          aliasKey: 'alias-001',
          manufacturerName: 'Acme',
          manufacturerPartNumber: 'A-100',
          createdAt: '2026-05-27T00:00:00Z',
        },
      ],
    })
    vi.stubGlobal('fetch', fetchMock)

    const substitutions = await getSubstitutions('token', '33333333-3333-3333-3333-333333333333')
    expect(substitutions).toHaveLength(1)
    expect(substitutions[0].aliasKey).toBe('alias-001')
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/v1/substitutions?partId=33333333-3333-3333-3333-333333333333',
      expect.any(Object),
    )
  })

  it('loads purchase requests on success', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => [
        {
          purchaseRequestId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
          requestKey: 'pr-001',
          title: 'Restock',
          notes: '',
          status: 'draft',
          supplierId: null,
          supplierKey: null,
          supplierDisplayName: null,
          parentSupplierId: null,
          parentSupplierDisplayName: null,
          supplierUnitKind: null,
          supplierServiceTypes: [],
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
    })
    vi.stubGlobal('fetch', fetchMock)

    const requests = await getPurchaseRequests('token')
    expect(requests).toHaveLength(1)
    expect(requests[0].requestKey).toBe('pr-001')
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/purchase-requests', expect.any(Object))
  })

  it('loads purchase orders on success', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
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
            supplierId: 'dddddddd-dddd-dddd-dddd-dddddddddddd',
            supplierKey: 'vendor-a',
            supplierDisplayName: 'Acme',
            parentSupplierId: null,
            parentSupplierDisplayName: null,
            supplierUnitKind: 'identity',
            supplierServiceTypes: ['parts'],
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
    })
    vi.stubGlobal('fetch', fetchMock)

    const orders = await getPurchaseOrders('token')
    expect(orders).toHaveLength(1)
    expect(orders[0].orderKey).toBe('po-001')
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/purchase-orders', expect.any(Object))
  })

  it('loads backorders on success', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
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
      })
    vi.stubGlobal('fetch', fetchMock)

    const backorders = await getBackorders('token', { status: 'open' })
    expect(backorders).toHaveLength(1)
    expect(backorders[0].purchaseRequestKey).toBe('pr-001')
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/backorders?status=open', expect.any(Object))
  })

  it('loads vendor returns on success', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [
          {
            returnId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
            returnKey: 'ret-001',
            status: 'posted',
            sourceType: 'stock',
            supplierId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
            supplierKey: 'acme',
            supplierDisplayName: 'Acme North Yard',
            parentSupplierId: '11111111-1111-1111-1111-111111111111',
            parentSupplierDisplayName: 'Acme Supply',
            supplierUnitKind: 'sub_unit',
            supplierServiceTypes: ['parts', 'maintenance'],
            vendorPartyId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
            vendorPartyKey: 'acme',
            vendorDisplayName: 'Acme North Yard',
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
      })
    vi.stubGlobal('fetch', fetchMock)

    const returns = await getVendorReturns('token', { status: 'posted' })
    expect(returns).toHaveLength(1)
    expect(returns[0].rmaNumber).toBe('RMA-42')
    expect(returns[0].supplierId).toBe('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb')
    expect(returns[0].parentSupplierDisplayName).toBe('Acme Supply')
    expect(returns[0].supplierUnitKind).toBe('sub_unit')
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/returns?status=posted', expect.any(Object))
  })

  it('loads pricing snapshots on success', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [
          {
            pricingSnapshotId: '11111111-1111-1111-1111-111111111111',
            snapshotKey: 'price-q2',
            partVendorLinkId: '22222222-2222-2222-2222-222222222222',
            partId: '33333333-3333-3333-3333-333333333333',
            partKey: 'filter-001',
            partDisplayName: 'Oil Filter',
            supplierId: '44444444-4444-4444-4444-444444444444',
            supplierKey: 'acme',
            supplierDisplayName: 'North Yard Counter',
            parentSupplierId: '11111111-1111-1111-1111-111111111111',
            parentSupplierDisplayName: 'Acme Supply',
            supplierUnitKind: 'sub_unit',
            supplierServiceTypes: ['parts', 'maintenance'],
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
      })
    vi.stubGlobal('fetch', fetchMock)

    const snapshots = await getPricingSnapshots('token')
    expect(snapshots).toHaveLength(1)
    expect(snapshots[0].isCurrent).toBe(true)
    expect(snapshots[0].supplierId).toBe('44444444-4444-4444-4444-444444444444')
    expect(snapshots[0].parentSupplierDisplayName).toBe('Acme Supply')
    expect(snapshots[0].supplierServiceTypes).toEqual(['parts', 'maintenance'])
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/pricing-snapshots', expect.any(Object))
  })

  it('loads lead-time snapshots on success', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [
          {
            leadTimeSnapshotId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
            snapshotKey: 'lt-q2',
            partVendorLinkId: '22222222-2222-2222-2222-222222222222',
            partId: '33333333-3333-3333-3333-333333333333',
            partKey: 'filter-001',
            partDisplayName: 'Oil Filter',
            supplierId: '44444444-4444-4444-4444-444444444444',
            supplierKey: 'acme',
            supplierDisplayName: 'North Yard Counter',
            parentSupplierId: '11111111-1111-1111-1111-111111111111',
            parentSupplierDisplayName: 'Acme Supply',
            supplierUnitKind: 'sub_unit',
            supplierServiceTypes: ['parts', 'maintenance'],
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
      })
    vi.stubGlobal('fetch', fetchMock)

    const snapshots = await getLeadTimeSnapshots('token')
    expect(snapshots).toHaveLength(1)
    expect(snapshots[0].leadTimeDays).toBe(7)
    expect(snapshots[0].supplierId).toBe('44444444-4444-4444-4444-444444444444')
    expect(snapshots[0].parentSupplierDisplayName).toBe('Acme Supply')
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/lead-time-snapshots', expect.any(Object))
  })

  it('loads availability snapshots on success', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [
          {
            availabilitySnapshotId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
            snapshotKey: 'avail-q2',
            partVendorLinkId: '22222222-2222-2222-2222-222222222222',
            partId: '33333333-3333-3333-3333-333333333333',
            partKey: 'filter-001',
            partDisplayName: 'Oil Filter',
            supplierId: '44444444-4444-4444-4444-444444444444',
            supplierKey: 'acme',
            supplierDisplayName: 'North Yard Counter',
            parentSupplierId: '11111111-1111-1111-1111-111111111111',
            parentSupplierDisplayName: 'Acme Supply',
            supplierUnitKind: 'sub_unit',
            supplierServiceTypes: ['parts', 'maintenance'],
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
      })
    vi.stubGlobal('fetch', fetchMock)

    const { getAvailabilitySnapshots } = await import('./client')
    const snapshots = await getAvailabilitySnapshots('token')
    expect(snapshots).toHaveLength(1)
    expect(snapshots[0].availabilityStatus).toBe('in_stock')
    expect(snapshots[0].supplierId).toBe('44444444-4444-4444-4444-444444444444')
    expect(snapshots[0].parentSupplierDisplayName).toBe('Acme Supply')
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/availability-snapshots', expect.any(Object))
  })

  it('parses reorder evaluation response', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
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
              preferredSupplierId: null,
              preferredSupplierKey: null,
              preferredSupplierDisplayName: null,
              preferredVendorPartyId: null,
              preferredVendorPartyKey: null,
              preferredVendorDisplayName: null,
              hasOpenPurchaseRequest: false,
              skipReason: null,
            },
          ],
        }),
      })
    vi.stubGlobal('fetch', fetchMock)

    const evaluation = await getReorderEvaluation('token')
    expect(evaluation.suggestions).toHaveLength(1)
    expect(evaluation.suggestions[0].suggestedOrderQuantity).toBe(24)
    expect(evaluation.suggestions[0].preferredSupplierId).toBeNull()
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/reorder-evaluation', expect.any(Object))
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

    await expect(getSupplierDirectory('token')).rejects.toBeInstanceOf(SupplyArrApiError)
  })

  it('surfaces problem details title/detail in API errors', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: false,
        status: 400,
        text: async () =>
          JSON.stringify({
            title: 'Vendor export blocked',
            detail: 'Missing procurement report permission.',
          }),
      }),
    )

    await expect(getSupplierDirectory('token')).rejects.toMatchObject({
      status: 400,
      message: 'Vendor export blocked - Missing procurement report permission.',
    })
  })

  it('surfaces validation errors in API error messages', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: false,
        status: 422,
        text: async () =>
          JSON.stringify({
            title: 'Validation failed',
            errors: {
              externalPartyId: ['External party is required.'],
              partyType: ['Party type is invalid.'],
            },
          }),
      }),
    )

    await expect(getSupplierDirectory('token')).rejects.toMatchObject({
      status: 422,
      message:
        'Validation failed - externalPartyId: External party is required.; partyType: Party type is invalid.',
    })
  })

  it('loads supply readiness dashboard from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        generatedAt: '2026-05-29T00:00:00Z',
        totals: {
          activePartsCount: 1,
          partsBelowReorderCount: 0,
          openBackorderCount: 0,
          openPurchaseRequestCount: 0,
          openDemandRefCount: 0,
        },
        demandRefsBySource: [],
        attentionItems: [],
      }),
    })
    vi.stubGlobal('fetch', fetchMock)

    const dashboard = await getSupplyReadinessDashboard('token')
    expect(dashboard.totals.activePartsCount).toBe(1)
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/supply-readiness/dashboard', expect.any(Object))
  })

  it('loads rfqs from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => [],
    })
    vi.stubGlobal('fetch', fetchMock)

    const rfqs = await getRfqs('token')
    expect(rfqs).toEqual([])
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/rfqs', expect.any(Object))
  })

  it('loads purchasing report summary with supplier-first document normalization', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        generatedAt: '2026-05-27T00:00:00Z',
        totals: {
          purchaseRequestCount: 1,
          openPurchaseRequestCount: 1,
          purchaseOrderCount: 1,
          openPurchaseOrderCount: 1,
          issuedPurchaseOrderCount: 0,
          draftReceivingReceiptCount: 0,
          postedReceivingReceiptCount: 0,
          openBackorderCount: 0,
          openPurchaseOrderLineQuantity: 2,
          purchaseOrderQuantityReceived: 0,
        },
        analytics: {
          pendingPurchaseRequestCount: 1,
          emergencyPurchaseRequestCount: 0,
          activeProcurementExceptionCount: 0,
          openReceivingExceptionCount: 0,
          openWarrantyClaimCount: 0,
          vendorDocumentExpiringSoonCount: 2,
          blockedVendorCount: 1,
          averageLeadTimeDays: 5,
          estimatedSpendThisMonth: 120,
        },
        purchaseRequestStatusCounts: [{ status: 'submitted', count: 1 }],
        purchaseOrderStatusCounts: [{ status: 'draft', count: 1 }],
        documents: [
          {
            documentType: 'purchase_request',
            documentId: 'pr-1',
            documentKey: 'PR-001',
            title: 'North yard restock',
            status: 'submitted',
            supplierId: 'supplier-unit-1',
            supplierKey: 'ACME-NY',
            supplierDisplayName: 'North Yard Counter',
            parentSupplierId: 'supplier-1',
            parentSupplierDisplayName: 'Acme Supply',
            supplierUnitKind: 'sub_unit',
            supplierServiceTypes: ['parts'],
            lineCount: 2,
            quantityOrdered: 2,
            quantityReceived: 0,
            updatedAt: '2026-05-27T00:00:00Z',
          },
          {
            documentType: 'purchase_order',
            documentId: 'po-1',
            documentKey: 'PO-001',
            title: 'Unassigned order',
            status: 'draft',
            supplierId: null,
            supplierKey: null,
            supplierDisplayName: '',
            parentSupplierId: null,
            parentSupplierDisplayName: null,
            supplierUnitKind: null,
            supplierServiceTypes: [],
            lineCount: 1,
            quantityOrdered: 1,
            quantityReceived: 0,
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ],
      }),
    })
    vi.stubGlobal('fetch', fetchMock)

    const summary = await getPurchasingReportSummary('token', {
      openDocumentsOnly: true,
      supplierId: 'supplier-unit-1',
    })

    expect(summary.analytics.supplierDocumentExpiringSoonCount).toBe(2)
    expect(summary.analytics.blockedSupplierCount).toBe(1)
    expect(summary.documents[0].vendorPartyId).toBe('supplier-unit-1')
    expect(summary.documents[0].vendorDisplayName).toBe('North Yard Counter')
    expect(summary.documents[1].supplierUnitKind).toBeNull()
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/reports/purchasing/summary?openDocumentsOnly=true&supplierId=supplier-unit-1',
      expect.any(Object),
    )
  })

  it('loads supplier report summary and detail with supplier-first normalization', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({
          generatedAt: '2026-05-27T00:00:00Z',
          approvalStatusCounts: [{ approvalStatus: 'approved', count: 1 }],
          suppliers: [
            {
              supplierId: 'supplier-unit-1',
              supplierKey: 'ACME-NY',
              supplierDisplayName: 'North Yard Counter',
              supplierType: 'supplier',
              parentSupplierId: 'supplier-1',
              parentSupplierDisplayName: 'Acme Supply',
              supplierUnitKind: 'sub_unit',
              supplierServiceTypes: ['parts', 'maintenance'],
              approvalStatus: 'approved',
              status: 'active',
              partVendorLinkCount: 2,
              preferredPartLinkCount: 1,
              openPurchaseRequestCount: 1,
              openPurchaseOrderCount: 1,
              issuedPurchaseOrderCount: 0,
              postedReceivingReceiptCount: 3,
              openBackorderCount: 0,
              openPurchaseOrderLineQuantity: 10,
              averageLeadTimeDays: 4,
              leadTimeSampleCount: 2,
              onTimeDeliveryRate: 100,
              onTimeDeliverySampleCount: 1,
              lastPurchaseOrderAt: '2026-05-27T00:00:00Z',
              lastReceivingPostedAt: '2026-05-27T00:00:00Z',
            },
          ],
        }),
      })
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({
          summary: {
            supplierId: 'supplier-unit-1',
            supplierKey: 'ACME-NY',
            supplierDisplayName: 'North Yard Counter',
            supplierType: 'supplier',
            parentSupplierId: 'supplier-1',
            parentSupplierDisplayName: 'Acme Supply',
            supplierUnitKind: 'sub_unit',
            supplierServiceTypes: ['parts', 'maintenance'],
            approvalStatus: 'approved',
            status: 'active',
            partVendorLinkCount: 2,
            preferredPartLinkCount: 1,
            openPurchaseRequestCount: 1,
            openPurchaseOrderCount: 1,
            issuedPurchaseOrderCount: 0,
            postedReceivingReceiptCount: 3,
            openBackorderCount: 0,
            openPurchaseOrderLineQuantity: 10,
            averageLeadTimeDays: 4,
            leadTimeSampleCount: 2,
            onTimeDeliveryRate: 100,
            onTimeDeliverySampleCount: 1,
            lastPurchaseOrderAt: '2026-05-27T00:00:00Z',
            lastReceivingPostedAt: '2026-05-27T00:00:00Z',
          },
          recentPurchaseRequests: [],
          recentPurchaseOrders: [],
          partLinks: [
            {
              partVendorLinkId: 'link-1',
              supplierId: 'supplier-unit-1',
              supplierKey: 'ACME-NY',
              supplierDisplayName: 'North Yard Counter',
              parentSupplierDisplayName: 'Acme Supply',
              supplierUnitKind: 'sub_unit',
              supplierServiceTypes: ['parts', 'maintenance'],
              partId: 'part-1',
              partKey: 'FILTER-001',
              partDisplayName: 'Oil Filter',
              vendorPartNumber: 'AC-100',
              isPreferred: true,
              catalogUnitPrice: 12.5,
              catalogAvailabilityStatus: 'in_stock',
            },
          ],
        }),
      })
    vi.stubGlobal('fetch', fetchMock)

    const summary = await getSupplierReportSummary('token', {
      approvalStatus: 'approved',
      activeOnly: true,
    })
    const detail = await getSupplierReportDetail('token', 'supplier-unit-1')

    expect(summary.suppliers[0].supplierDisplayName).toBe('North Yard Counter')
    expect(summary.suppliers[0].parentSupplierDisplayName).toBe('Acme Supply')
    expect(summary.suppliers[0].supplierServiceTypes).toEqual(['parts', 'maintenance'])
    expect(summary.suppliers[0].vendorPartyId).toBe('supplier-unit-1')
    expect(detail.summary.supplierUnitKind).toBe('sub_unit')
    expect(detail.partLinks[0].supplierDisplayName).toBe('North Yard Counter')
    expect(fetchMock).toHaveBeenNthCalledWith(
      1,
      '/api/reports/suppliers/summary?approvalStatus=approved&activeOnly=true',
      expect.any(Object),
    )
    expect(fetchMock).toHaveBeenNthCalledWith(
      2,
      '/api/reports/suppliers/supplier-unit-1',
      expect.any(Object),
    )
  })

  it('loads and ingests supplier email inbox messages with supplier-first normalization', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({
          items: [
            {
              messageId: 'message-1',
              messageKey: 'mail-001',
              messageKind: 'quote_received',
              senderEmail: 'supplier@example.com',
              senderName: 'Acme Counter',
              subject: 'RFQ-001 quote attached',
              bodyPreview: 'Quote attached.',
              matchStatus: 'matched',
              matchReason: 'matched explicit reference key',
              vendorPartyId: 'supplier-unit-1',
              vendorPartyKey: 'ACME-NY',
              vendorDisplayName: 'North Yard Counter',
              linkedReferenceType: 'rfq',
              linkedReferenceId: 'rfq-1',
              linkedReferenceKey: 'RFQ-001',
              receivedAt: '2026-05-27T00:00:00Z',
              createdAt: '2026-05-27T00:00:00Z',
              updatedAt: '2026-05-27T00:00:00Z',
              processedAt: '2026-05-27T00:00:00Z',
            },
          ],
        }),
      })
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({
          wasDuplicate: false,
          message: {
            messageId: 'message-2',
            messageKey: 'mail-002',
            messageKind: 'order_confirmation_received',
            senderEmail: 'supplier@example.com',
            senderName: 'Acme Counter',
            subject: 'PO-001 confirmation',
            bodyPreview: 'Confirmed.',
            matchStatus: 'matched',
            matchReason: 'matched sender email to supplier reference',
            supplierId: 'supplier-unit-1',
            supplierKey: 'ACME-NY',
            supplierDisplayName: 'North Yard Counter',
            linkedReferenceType: 'purchase_order',
            linkedReferenceId: 'po-1',
            linkedReferenceKey: 'PO-001',
            receivedAt: '2026-05-27T00:00:00Z',
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
            processedAt: '2026-05-27T00:00:00Z',
          },
        }),
      })
    vi.stubGlobal('fetch', fetchMock)

    const inbox = await getVendorEmailInbox('token', 10)
    const ingested = await ingestVendorEmailInbox('token', {
      messageKey: 'mail-002',
      messageKind: 'order_confirmation_received',
      senderEmail: 'supplier@example.com',
      senderName: 'Acme Counter',
      subject: 'PO-001 confirmation',
      body: 'Confirmed.',
      referenceKey: 'PO-001',
    })

    expect(inbox.items[0].supplierId).toBe('supplier-unit-1')
    expect(inbox.items[0].supplierKey).toBe('ACME-NY')
    expect(inbox.items[0].supplierDisplayName).toBe('North Yard Counter')
    expect(ingested.message.vendorPartyId).toBe('supplier-unit-1')
    expect(ingested.message.vendorDisplayName).toBe('North Yard Counter')
    expect(fetchMock).toHaveBeenNthCalledWith(
      1,
      '/api/v1/vendor-email-inbox?limit=10',
      expect.any(Object),
    )
    expect(fetchMock).toHaveBeenNthCalledWith(
      2,
      '/api/v1/vendor-email-inbox',
      expect.objectContaining({ method: 'POST' }),
    )
  })

  it('loads warranty claims from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => [
        {
          warrantyClaimId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
          claimKey: 'wc-001',
          status: 'draft',
          claimType: 'replacement',
          supplierId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
          supplierKey: 'acme',
          supplierDisplayName: 'Acme North Yard',
          parentSupplierId: '11111111-1111-1111-1111-111111111111',
          parentSupplierDisplayName: 'Acme Supply',
          supplierUnitKind: 'sub_unit',
          supplierServiceTypes: ['parts'],
          vendorPartyId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
          vendorPartyKey: 'acme',
          vendorDisplayName: 'Acme North Yard',
          partId: 'part-1',
          partKey: 'FILTER-001',
          partDisplayName: 'Oil Filter',
          purchaseOrderId: null,
          purchaseOrderKey: null,
          purchaseOrderLineId: null,
          receivingReceiptId: null,
          receivingReceiptKey: null,
          receivingReceiptLineId: null,
          quantityClaimed: 1,
          problemDescription: 'Damaged in box',
          vendorRmaNumber: '',
          vendorDisposition: '',
          vendorResponseNotes: '',
          closureNotes: '',
          denialReason: '',
          createdByUserId: 'user-1',
          submittedByUserId: null,
          submittedAt: null,
          vendorRespondedByUserId: null,
          vendorRespondedAt: null,
          closedByUserId: null,
          closedAt: null,
          deniedByUserId: null,
          deniedAt: null,
          cancellationReason: '',
          createdAt: '2026-05-27T00:00:00Z',
          updatedAt: '2026-05-27T00:00:00Z',
        },
      ],
    })
    vi.stubGlobal('fetch', fetchMock)

    const claims = await listWarrantyClaims('token', {
      supplierId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    })
    expect(claims).toHaveLength(1)
    expect(claims[0].supplierId).toBe('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb')
    expect(claims[0].parentSupplierDisplayName).toBe('Acme Supply')
    expect(claims[0].supplierServiceTypes).toEqual(['parts'])
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/v1/warranty-claims?supplierId=bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
      expect.any(Object),
    )
  })

  it('loads demand refs from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => [],
    })
    vi.stubGlobal('fetch', fetchMock)

    const refs = await getDemandRefs('token')
    expect(refs).toEqual([])
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/demand-refs', expect.any(Object))
  })

  it('loads demand processing dashboard from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        pendingCount: 0,
        stockShortCount: 0,
        stockAvailableCount: 0,
        prDraftedCount: 0,
        processedItems: [],
        pendingItems: [],
      }),
    })
    vi.stubGlobal('fetch', fetchMock)

    const dashboard = await getDemandProcessingDashboard('token')
    expect(dashboard.pendingCount).toBe(0)
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/demand-processing', expect.any(Object))
  })

  it('loads demand processing settings from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        isEnabled: true,
        checkIntervalMinutes: 5,
      }),
    })
    vi.stubGlobal('fetch', fetchMock)

    const settings = await getDemandProcessingSettings('token')
    expect(settings.isEnabled).toBe(true)
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/demand-processing-settings', expect.any(Object))
  })

  it('loads integration event settings from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        isEnabled: true,
      }),
    })
    vi.stubGlobal('fetch', fetchMock)

    const settings = await getIntegrationEventSettings('token')
    expect(settings.isEnabled).toBe(true)
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/integration-event-settings', expect.any(Object))
  })

  it('loads notification settings from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        isEnabled: true,
      }),
    })
    vi.stubGlobal('fetch', fetchMock)

    const settings = await getProcurementNotificationSettings('token')
    expect(settings.isEnabled).toBe(true)
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/notification-settings', expect.any(Object))
  })

  it('loads price snapshot settings from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        isEnabled: true,
      }),
    })
    vi.stubGlobal('fetch', fetchMock)

    const settings = await getPriceSnapshotSettings('token')
    expect(settings.isEnabled).toBe(true)
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/price-snapshot-settings', expect.any(Object))
  })

  it('loads lead-time snapshot settings from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        isEnabled: true,
      }),
    })
    vi.stubGlobal('fetch', fetchMock)

    const settings = await getLeadTimeSnapshotSettings('token')
    expect(settings.isEnabled).toBe(true)
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/lead-time-snapshot-settings', expect.any(Object))
  })

  it('loads availability snapshot settings from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        isEnabled: true,
      }),
    })
    vi.stubGlobal('fetch', fetchMock)

    const settings = await getAvailabilitySnapshotSettings('token')
    expect(settings.isEnabled).toBe(true)
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/availability-snapshot-settings', expect.any(Object))
  })

  it('loads procurement coordination dashboard from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        generatedAt: '2026-05-29T00:00:00Z',
        totals: {
          activeCount: 0,
          blockedCount: 0,
          exceptionCount: 0,
        },
        items: [],
      }),
    })
    vi.stubGlobal('fetch', fetchMock)

    await getProcurementCoordinationDashboard('token')
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/procurement-coordination?activeOnly=true', expect.any(Object))
  })

  it('loads procurement coordination settings from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        isEnabled: true,
      }),
    })
    vi.stubGlobal('fetch', fetchMock)

    const settings = await getProcurementCoordinationSettings('token')
    expect(settings.isEnabled).toBe(true)
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/procurement-coordination-settings', expect.any(Object))
  })

  it('loads approval reminders dashboard from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        generatedAt: '2026-05-29T00:00:00Z',
        totals: {
          pendingCount: 0,
          overdueCount: 0,
          upcomingCount: 0,
        },
        items: [],
      }),
    })
    vi.stubGlobal('fetch', fetchMock)

    await getApprovalRemindersDashboard('token')
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/approval-reminders?includeUpcoming=false', expect.any(Object))
  })

  it('loads approval reminder settings from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        isEnabled: true,
      }),
    })
    vi.stubGlobal('fetch', fetchMock)

    const settings = await getApprovalReminderSettings('token')
    expect(settings.isEnabled).toBe(true)
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/approval-reminder-settings', expect.any(Object))
  })

  it('loads procurement exception escalation settings from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        isEnabled: true,
      }),
    })
    vi.stubGlobal('fetch', fetchMock)

    const settings = await getProcurementExceptionEscalationSettings('token')
    expect(settings.isEnabled).toBe(true)
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/v1/procurement-exception-escalation-settings',
      expect.any(Object),
    )
  })

  it('loads procurement exception resolution templates from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => [],
    })
    vi.stubGlobal('fetch', fetchMock)

    const templates = await listProcurementExceptionResolutionTemplates('token')
    expect(templates).toEqual([])
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/v1/procurement-exceptions/resolution-templates',
      expect.any(Object),
    )
  })

  it('loads supplier incidents from v1 endpoint with filters', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => [],
    })
    vi.stubGlobal('fetch', fetchMock)

    await listSupplierIncidents('token', {
      status: 'open',
      supplierId: '11111111-1111-1111-1111-111111111111',
      severity: 'high',
    })

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/v1/supplier-incidents?status=open&supplierId=11111111-1111-1111-1111-111111111111&severity=high',
      expect.any(Object),
    )
  })

  it('loads supplier incidents for a supplier record from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => [],
    })
    vi.stubGlobal('fetch', fetchMock)

    await listSupplierIncidentsForSupplier('token', '22222222-2222-2222-2222-222222222222')
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/v1/suppliers/22222222-2222-2222-2222-222222222222/supplier-incidents',
      expect.any(Object),
    )
  })

  it('creates and updates supplier incident workflow on v1 endpoints', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({
          incidentId: '33333333-3333-3333-3333-333333333333',
          externalPartyId: '44444444-4444-4444-4444-444444444444',
          partyKey: 'ACME',
          partyDisplayName: 'Acme Supply',
          partyType: 'supplier',
          incidentKey: 'inc-001',
          title: 'Quality deviation',
          description: 'Batch mismatch',
          incidentType: 'quality',
          severity: 'high',
          status: 'open',
          purchaseRequestId: null,
          purchaseOrderId: null,
          receivingReceiptId: null,
          receivingExceptionId: null,
          vendorRestrictionId: null,
          reportedByUserId: 'u1',
          assignedToUserId: null,
          resolutionNotes: '',
          resolvedByUserId: null,
          resolvedAt: null,
          closedByUserId: null,
          closedAt: null,
          cancellationReason: '',
          cancelledByUserId: null,
          cancelledAt: null,
          reopenedByUserId: null,
          reopenedAt: null,
          lastReopenReason: '',
          reopenCount: 0,
          createdAt: '',
          updatedAt: '',
        }),
      })
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({
          incidentId: '33333333-3333-3333-3333-333333333333',
          externalPartyId: '44444444-4444-4444-4444-444444444444',
          partyKey: 'ACME',
          partyDisplayName: 'Acme Supply',
          partyType: 'supplier',
          incidentKey: 'inc-001',
          title: 'Quality deviation',
          description: 'Batch mismatch',
          incidentType: 'quality',
          severity: 'high',
          status: 'investigating',
          purchaseRequestId: null,
          purchaseOrderId: null,
          receivingReceiptId: null,
          receivingExceptionId: null,
          vendorRestrictionId: null,
          reportedByUserId: 'u1',
          assignedToUserId: null,
          resolutionNotes: '',
          resolvedByUserId: null,
          resolvedAt: null,
          closedByUserId: null,
          closedAt: null,
          cancellationReason: '',
          cancelledByUserId: null,
          cancelledAt: null,
          reopenedByUserId: null,
          reopenedAt: null,
          lastReopenReason: '',
          reopenCount: 0,
          createdAt: '',
          updatedAt: '',
        }),
      })
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({
          incidentId: '33333333-3333-3333-3333-333333333333',
          externalPartyId: '44444444-4444-4444-4444-444444444444',
          partyKey: 'ACME',
          partyDisplayName: 'Acme Supply',
          partyType: 'supplier',
          incidentKey: 'inc-001',
          title: 'Quality deviation',
          description: 'Batch mismatch',
          incidentType: 'quality',
          severity: 'high',
          status: 'investigating',
          purchaseRequestId: null,
          purchaseOrderId: null,
          receivingReceiptId: null,
          receivingExceptionId: null,
          vendorRestrictionId: 'vr-1',
          reportedByUserId: 'u1',
          assignedToUserId: null,
          resolutionNotes: '',
          resolvedByUserId: null,
          resolvedAt: null,
          closedByUserId: null,
          closedAt: null,
          cancellationReason: '',
          cancelledByUserId: null,
          cancelledAt: null,
          reopenedByUserId: null,
          reopenedAt: null,
          lastReopenReason: '',
          reopenCount: 0,
          createdAt: '',
          updatedAt: '',
        }),
      })
    vi.stubGlobal('fetch', fetchMock)

    const incidentId = '33333333-3333-3333-3333-333333333333'
    const created = await createSupplierIncident('token', {
      incidentKey: 'inc-001',
      supplierId: '44444444-4444-4444-4444-444444444444',
      title: 'Quality deviation',
      description: 'Batch mismatch',
      incidentType: 'quality',
      severity: 'high',
    })
    await startSupplierIncidentInvestigation('token', incidentId)
    await applySupplierIncidentProcurementRestriction('token', incidentId, {
      restrictionKey: 'hold-vendor',
      scopes: ['purchase_requests'],
      reason: 'Safety review',
    })

    expect(created.supplierId).toBe('44444444-4444-4444-4444-444444444444')

    expect(fetchMock).toHaveBeenNthCalledWith(
      1,
      '/api/v1/supplier-incidents',
      expect.objectContaining({
        body: expect.stringContaining('"supplierId":"44444444-4444-4444-4444-444444444444"'),
      }),
    )
    expect(fetchMock).toHaveBeenNthCalledWith(
      2,
      '/api/v1/supplier-incidents/33333333-3333-3333-3333-333333333333/start-investigation',
      expect.any(Object),
    )
    expect(fetchMock).toHaveBeenNthCalledWith(
      3,
      '/api/v1/supplier-incidents/33333333-3333-3333-3333-333333333333/apply-procurement-restriction',
      expect.any(Object),
    )
  })

  it('loads supplier restrictions from the supplier-first v1 endpoint with status filter', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => [],
    })
    vi.stubGlobal('fetch', fetchMock)

    await listVendorRestrictions('token', { status: 'active' })
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/v1/supplier-restrictions?status=active',
      expect.any(Object),
    )
  })

  it('loads supplier restrictions and enforcement from v1 endpoints', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce({
        ok: true,
        json: async () => [],
      })
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({
          externalPartyId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
          isBlocked: false,
          blockReason: null,
          activeScopes: [],
        }),
      })
    vi.stubGlobal('fetch', fetchMock)

    const supplierId = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
    await listSupplierRestrictionsBySupplier('token', supplierId)
    await getSupplierRestrictionEnforcement('token', supplierId)

    expect(fetchMock).toHaveBeenNthCalledWith(
      1,
      '/api/v1/suppliers/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/restrictions',
      expect.any(Object),
    )
    expect(fetchMock).toHaveBeenNthCalledWith(
      2,
      '/api/v1/suppliers/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/restrictions/enforcement',
      expect.any(Object),
    )
  })

  it('creates and lifts supplier restrictions on v1 endpoints', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({ restrictionId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb' }),
      })
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({ restrictionId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb' }),
      })
    vi.stubGlobal('fetch', fetchMock)

    const supplierId = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
    const restrictionId = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb'
    await createSupplierRestriction('token', supplierId, {
      restrictionKey: 'hold-vendor',
      scopes: ['purchase_orders'],
      reason: 'Pending insurance renewal',
      effectiveFrom: null,
      effectiveUntil: null,
    })
    await liftSupplierRestriction('token', restrictionId, {
      liftNotes: 'Insurance completed',
    })

    expect(fetchMock).toHaveBeenNthCalledWith(
      1,
      '/api/v1/suppliers/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/restrictions',
      expect.any(Object),
    )
    expect(fetchMock).toHaveBeenNthCalledWith(
      2,
      '/api/v1/supplier-restrictions/bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb/lift',
      expect.any(Object),
    )
  })

  it('loads supplier onboarding endpoints from v1 routes', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({ requiredDocuments: [] }),
      })
      .mockResolvedValueOnce({
        ok: true,
        json: async () => [
          {
            onboardingId: 'onb-1',
            supplierId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
            supplierKey: 'ACME',
            supplierUnitKind: 'identity',
            parentSupplierId: null,
            parentSupplierDisplayName: null,
            displayName: 'Acme Supply',
            onboardingStatus: 'draft',
            notes: '',
            submittedAt: null,
            reviewedAt: null,
            rejectionReason: '',
            documentRequirements: [],
            createdAt: '2026-01-01T00:00:00Z',
            updatedAt: '2026-01-01T00:00:00Z',
          },
        ],
      })
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({
          onboardingId: 'onb-1',
          supplierId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
          supplierKey: 'ACME',
          supplierUnitKind: 'identity',
          parentSupplierId: null,
          parentSupplierDisplayName: null,
          displayName: 'Acme Supply',
          onboardingStatus: 'draft',
          notes: '',
          submittedAt: null,
          reviewedAt: null,
          rejectionReason: '',
          documentRequirements: [],
          createdAt: '2026-01-01T00:00:00Z',
          updatedAt: '2026-01-01T00:00:00Z',
        }),
      })
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({
          onboardingId: 'onb-1',
          supplierId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
          supplierKey: 'ACME',
          supplierUnitKind: 'identity',
          parentSupplierId: null,
          parentSupplierDisplayName: null,
          displayName: 'Acme Supply',
          onboardingStatus: 'draft',
          notes: '',
          submittedAt: null,
          reviewedAt: null,
          rejectionReason: '',
          documentRequirements: [],
          createdAt: '2026-01-01T00:00:00Z',
          updatedAt: '2026-01-01T00:00:00Z',
        }),
      })
    vi.stubGlobal('fetch', fetchMock)

    await getSupplierOnboardingDocumentRequirements('token')
    const pending = await listPendingSupplierOnboarding('token')
    const started = await startSupplierOnboarding('token', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa')
    const onboarding = await getSupplierOnboarding('token', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa')

    expect(pending[0].supplierId).toBe('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa')
    expect(pending[0].supplierKey).toBe('ACME')
    expect(started.supplierId).toBe('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa')
    expect(onboarding.supplierKey).toBe('ACME')

    expect(fetchMock).toHaveBeenNthCalledWith(
      1,
      '/api/v1/supplier-onboarding/document-requirements',
      expect.any(Object),
    )
    expect(fetchMock).toHaveBeenNthCalledWith(
      2,
      '/api/v1/supplier-onboarding/pending',
      expect.any(Object),
    )
    expect(fetchMock).toHaveBeenNthCalledWith(
      3,
      '/api/v1/supplier-onboarding/start',
      expect.objectContaining({
        body: JSON.stringify({
          supplierId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
          notes: null,
        }),
      }),
    )
    expect(fetchMock).toHaveBeenNthCalledWith(
      4,
      '/api/v1/supplier-onboarding/suppliers/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
      expect.any(Object),
    )
  })

  it('loads and approves supplier compliance documents on v1 routes', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce({
        ok: true,
        json: async () => [
          {
            documentId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
            supplierId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
            supplierKey: 'ACME',
            supplierDisplayName: 'Acme Supply',
            documentKey: 'COI-1',
            documentTypeKey: 'insurance',
            title: 'Insurance',
            version: 1,
            reviewStatus: 'approved',
            expiresAt: null,
            effectiveAt: null,
            fileName: 'insurance.pdf',
            contentType: 'application/pdf',
            sizeBytes: 42,
            notes: '',
            createdAt: '2026-01-01T00:00:00Z',
            updatedAt: '2026-01-01T00:00:00Z',
          },
        ],
      })
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({
          documentId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
          supplierId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
          supplierKey: 'ACME',
          supplierDisplayName: 'Acme Supply',
          documentKey: 'COI-1',
          documentTypeKey: 'insurance',
          title: 'Insurance',
          version: 1,
          reviewStatus: 'approved',
          expiresAt: null,
          effectiveAt: null,
          fileName: 'insurance.pdf',
          contentType: 'application/pdf',
          sizeBytes: 42,
          notes: '',
          createdAt: '2026-01-01T00:00:00Z',
          updatedAt: '2026-01-01T00:00:00Z',
        }),
      })
    vi.stubGlobal('fetch', fetchMock)

    const supplierId = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
    const documentId = 'cccccccc-cccc-cccc-cccc-cccccccccccc'
    const documents = await listSupplierComplianceDocuments('token', supplierId)
    const approved = await approveSupplierComplianceDocument('token', supplierId, documentId)

    expect(documents[0].supplierId).toBe(supplierId)
    expect(approved.supplierId).toBe(supplierId)

    expect(fetchMock).toHaveBeenNthCalledWith(
      1,
      '/api/v1/suppliers/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/compliance-documents',
      expect.any(Object),
    )
    expect(fetchMock).toHaveBeenNthCalledWith(
      2,
      '/api/v1/suppliers/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/compliance-documents/cccccccc-cccc-cccc-cccc-cccccccccccc/approve',
      expect.any(Object),
    )
  })

  it('loads emergency purchases from v1 endpoints', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce({
        ok: true,
        json: async () => [],
      })
      .mockResolvedValueOnce({
        ok: true,
        json: async () => [],
      })
    vi.stubGlobal('fetch', fetchMock)

    await getEmergencyPurchases('token', 'pending')
    await listPendingEmergencyPurchases('token')

    expect(fetchMock).toHaveBeenNthCalledWith(
      1,
      '/api/v1/emergency-purchases?status=pending',
      expect.any(Object),
    )
    expect(fetchMock).toHaveBeenNthCalledWith(
      2,
      '/api/v1/emergency-purchases/pending',
      expect.any(Object),
    )
  })

  it('runs emergency purchase workflow actions on v1 endpoints', async () => {
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({ purchaseRequestId: 'dddddddd-dddd-dddd-dddd-dddddddddddd' }),
      })
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({ purchaseRequestId: 'dddddddd-dddd-dddd-dddd-dddddddddddd' }),
      })
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({ purchaseOrderId: 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee' }),
      })
    vi.stubGlobal('fetch', fetchMock)

    const purchaseRequestId = 'dddddddd-dddd-dddd-dddd-dddddddddddd'
    await expeditedSubmitEmergencyPurchase('token', purchaseRequestId, 'urgent')
    await managerOverrideApproveEmergencyPurchase('token', purchaseRequestId, 'manager approved')
    await issueEmergencyPurchaseOrder('token', purchaseRequestId, 'epo-001')

    expect(fetchMock).toHaveBeenNthCalledWith(
      1,
      '/api/v1/emergency-purchases/dddddddd-dddd-dddd-dddd-dddddddddddd/expedited-submit',
      expect.any(Object),
    )
    expect(fetchMock).toHaveBeenNthCalledWith(
      2,
      '/api/v1/emergency-purchases/dddddddd-dddd-dddd-dddd-dddddddddddd/manager-override-approve',
      expect.any(Object),
    )
    expect(fetchMock).toHaveBeenNthCalledWith(
      3,
      '/api/v1/emergency-purchases/dddddddd-dddd-dddd-dddd-dddddddddddd/issue-purchase-order',
      expect.any(Object),
    )
  })

  it('runs forgiving search on v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ query: 'oil', totalCount: 0, buckets: [], results: [] }),
    })
    vi.stubGlobal('fetch', fetchMock)

    await forgivingSearch('token', { q: 'oil', limit: 5 })
    expect(fetchMock).toHaveBeenCalledWith('/api/v1/search/forgiving?q=oil&limit=5', expect.any(Object))
  })

  it('loads audit history from v1 endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ items: [], nextCursor: null }),
    })
    vi.stubGlobal('fetch', fetchMock)

    await listAuditHistory('token', { limit: 10, action: 'test.action' })
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/v1/audit-history?limit=10&action=test.action',
      expect.any(Object),
    )
  })
})
