import { fireEvent, render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

import { SupplierReportsPanel } from './VendorReportsPanel'

vi.mock('../api/client', () => ({
  getSupplierReportSummary: vi.fn().mockResolvedValue({
    generatedAt: new Date().toISOString(),
    approvalStatusCounts: [{ approvalStatus: 'approved', count: 1 }],
    suppliers: [
      {
        supplierId: 'vendor-1',
        supplierKey: 'ACME',
        supplierDisplayName: 'North Yard Counter',
        parentSupplierDisplayName: 'Acme Supply',
        supplierUnitKind: 'sub_unit',
        supplierServiceTypes: ['parts', 'maintenance'],
        supplierType: 'supplier',
        approvalStatus: 'approved',
        status: 'active',
        partVendorLinkCount: 2,
        preferredPartLinkCount: 1,
        openPurchaseRequestCount: 1,
        openPurchaseOrderCount: 0,
        issuedPurchaseOrderCount: 1,
        postedReceivingReceiptCount: 3,
        openBackorderCount: 0,
        openPurchaseOrderLineQuantity: 10,
        averageLeadTimeDays: 4,
        leadTimeSampleCount: 2,
        onTimeDeliveryRate: 100,
        onTimeDeliverySampleCount: 1,
        lastPurchaseOrderAt: new Date().toISOString(),
        lastReceivingPostedAt: new Date().toISOString(),
      },
    ],
  }),
  getRfqs: vi.fn().mockResolvedValue([
    {
      rfqId: 'rfq-1',
      rfqKey: 'RFQ-001',
      title: 'First RFQ',
      notes: '',
      status: 'submitted',
      requestedByUserId: 'user-1',
      submittedAt: '2026-05-01T00:00:00Z',
      awardedVendorPartyId: null,
      awardedVendorDisplayName: null,
      selectedVendorQuoteId: null,
      purchaseRequestId: null,
      awardedAt: null,
      lines: [],
      invitations: [
        {
          invitationId: 'inv-1',
          supplierId: 'vendor-1',
          supplierKey: 'ACME',
          supplierDisplayName: 'North Yard Counter',
          parentSupplierDisplayName: 'Acme Supply',
          supplierUnitKind: 'sub_unit',
          supplierServiceTypes: ['parts', 'maintenance'],
          vendorPartyId: 'vendor-1',
          vendorPartyKey: 'ACME',
          vendorDisplayName: 'Acme Supply',
          status: 'invited',
          invitedAt: '2026-05-01T00:00:00Z',
        },
        {
          invitationId: 'inv-2',
          supplierId: 'vendor-2',
          supplierKey: 'BETA',
          supplierDisplayName: 'Beta Parts',
          supplierUnitKind: 'identity',
          supplierServiceTypes: ['parts'],
          vendorPartyId: 'vendor-2',
          vendorPartyKey: 'BETA',
          vendorDisplayName: 'Beta Parts',
          status: 'invited',
          invitedAt: '2026-05-01T00:00:00Z',
        },
      ],
      quotes: [
        {
          vendorQuoteId: 'vq-1',
          rfqId: 'rfq-1',
          supplierId: 'vendor-1',
          supplierKey: 'ACME',
          supplierDisplayName: 'North Yard Counter',
          parentSupplierDisplayName: 'Acme Supply',
          supplierUnitKind: 'sub_unit',
          supplierServiceTypes: ['parts', 'maintenance'],
          vendorPartyId: 'vendor-1',
          vendorPartyKey: 'ACME',
          vendorDisplayName: 'Acme Supply',
          quoteKey: 'Q-ACME-1',
          status: 'submitted',
          currencyCode: 'USD',
          totalAmount: 100,
          leadTimeDays: 14,
          notes: '',
          submittedAt: '2026-05-03T00:00:00Z',
          lines: [],
          createdAt: '2026-05-01T00:00:00Z',
          updatedAt: '2026-05-03T00:00:00Z',
        },
        {
          vendorQuoteId: 'vq-2',
          rfqId: 'rfq-1',
          supplierId: 'vendor-2',
          supplierKey: 'BETA',
          supplierDisplayName: 'Beta Parts',
          supplierUnitKind: 'identity',
          supplierServiceTypes: ['parts'],
          vendorPartyId: 'vendor-2',
          vendorPartyKey: 'BETA',
          vendorDisplayName: 'Beta Parts',
          quoteKey: 'Q-BETA-1',
          status: 'submitted',
          currencyCode: 'USD',
          totalAmount: 120,
          leadTimeDays: 12,
          notes: '',
          submittedAt: '2026-05-04T00:00:00Z',
          lines: [],
          createdAt: '2026-05-01T00:00:00Z',
          updatedAt: '2026-05-04T00:00:00Z',
        },
      ],
      createdAt: '2026-05-01T00:00:00Z',
      updatedAt: '2026-05-04T00:00:00Z',
    },
    {
      rfqId: 'rfq-2',
      rfqKey: 'RFQ-002',
      title: 'Second RFQ',
      notes: '',
      status: 'submitted',
      requestedByUserId: 'user-1',
      submittedAt: '2026-06-01T00:00:00Z',
      awardedVendorPartyId: null,
      awardedVendorDisplayName: null,
      selectedVendorQuoteId: null,
      purchaseRequestId: null,
      awardedAt: null,
      lines: [],
      invitations: [
        {
          invitationId: 'inv-3',
          supplierId: 'vendor-1',
          supplierKey: 'ACME',
          supplierDisplayName: 'North Yard Counter',
          parentSupplierDisplayName: 'Acme Supply',
          supplierUnitKind: 'sub_unit',
          supplierServiceTypes: ['parts', 'maintenance'],
          vendorPartyId: 'vendor-1',
          vendorPartyKey: 'ACME',
          vendorDisplayName: 'Acme Supply',
          status: 'invited',
          invitedAt: '2026-06-01T00:00:00Z',
        },
        {
          invitationId: 'inv-4',
          supplierId: 'vendor-2',
          supplierKey: 'BETA',
          supplierDisplayName: 'Beta Parts',
          supplierUnitKind: 'identity',
          supplierServiceTypes: ['parts'],
          vendorPartyId: 'vendor-2',
          vendorPartyKey: 'BETA',
          vendorDisplayName: 'Beta Parts',
          status: 'invited',
          invitedAt: '2026-06-01T00:00:00Z',
        },
      ],
      quotes: [
        {
          vendorQuoteId: 'vq-3',
          rfqId: 'rfq-2',
          supplierId: 'vendor-1',
          supplierKey: 'ACME',
          supplierDisplayName: 'North Yard Counter',
          parentSupplierDisplayName: 'Acme Supply',
          supplierUnitKind: 'sub_unit',
          supplierServiceTypes: ['parts', 'maintenance'],
          vendorPartyId: 'vendor-1',
          vendorPartyKey: 'ACME',
          vendorDisplayName: 'Acme Supply',
          quoteKey: 'Q-ACME-2',
          status: 'submitted',
          currencyCode: 'USD',
          totalAmount: 150,
          leadTimeDays: 10,
          notes: '',
          submittedAt: '2026-06-05T00:00:00Z',
          lines: [],
          createdAt: '2026-06-01T00:00:00Z',
          updatedAt: '2026-06-05T00:00:00Z',
        },
        {
          vendorQuoteId: 'vq-4',
          rfqId: 'rfq-2',
          supplierId: 'vendor-2',
          supplierKey: 'BETA',
          supplierDisplayName: 'Beta Parts',
          supplierUnitKind: 'identity',
          supplierServiceTypes: ['parts'],
          vendorPartyId: 'vendor-2',
          vendorPartyKey: 'BETA',
          vendorDisplayName: 'Beta Parts',
          quoteKey: 'Q-BETA-2',
          status: 'submitted',
          currencyCode: 'USD',
          totalAmount: 130,
          leadTimeDays: 11,
          notes: '',
          submittedAt: '2026-06-04T00:00:00Z',
          lines: [],
          createdAt: '2026-06-01T00:00:00Z',
          updatedAt: '2026-06-04T00:00:00Z',
        },
      ],
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-05T00:00:00Z',
    },
  ]),
  getSupplierReportDetail: vi.fn().mockResolvedValue({
    summary: {
      supplierId: 'vendor-1',
      supplierKey: 'ACME',
      supplierDisplayName: 'North Yard Counter',
      parentSupplierDisplayName: 'Acme Supply',
      supplierUnitKind: 'sub_unit',
      supplierServiceTypes: ['parts', 'maintenance'],
      supplierType: 'supplier',
      approvalStatus: 'approved',
      status: 'active',
      partVendorLinkCount: 2,
      preferredPartLinkCount: 1,
      openPurchaseRequestCount: 1,
      openPurchaseOrderCount: 1,
      issuedPurchaseOrderCount: 1,
      postedReceivingReceiptCount: 3,
      openBackorderCount: 1,
      openPurchaseOrderLineQuantity: 10,
      averageLeadTimeDays: 4,
      leadTimeSampleCount: 2,
      onTimeDeliveryRate: 100,
      onTimeDeliverySampleCount: 1,
      lastPurchaseOrderAt: '2026-06-03T00:00:00Z',
      lastReceivingPostedAt: '2026-06-03T00:00:00Z',
    },
    recentPurchaseRequests: [],
    recentPurchaseOrders: [
      {
        purchaseOrderId: 'po-1',
        orderKey: 'PO-001',
        title: 'First PO',
        status: 'issued',
        lineCount: 2,
        quantityOrdered: 10,
        quantityReceived: 10,
        updatedAt: '2026-06-03T00:00:00Z',
      },
      {
        purchaseOrderId: 'po-2',
        orderKey: 'PO-002',
        title: 'Second PO',
        status: 'received',
        lineCount: 1,
        quantityOrdered: 4,
        quantityReceived: 2,
        updatedAt: '2026-06-03T00:00:00Z',
      },
    ],
    partLinks: [
      {
        partVendorLinkId: 'link-1',
        supplierId: 'vendor-1',
        supplierKey: 'ACME',
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
  getVendorReturns: vi.fn().mockResolvedValue([]),
  listWarrantyClaims: vi.fn().mockResolvedValue([]),
  getComplianceSupplierDetail: vi.fn().mockResolvedValue({
    summary: {
      supplierId: 'vendor-1',
      supplierKey: 'ACME',
      displayName: 'Acme Supply',
      approvalStatus: 'approved',
      compliancePosture: 'approved',
      documentCount: 2,
      expiredCount: 0,
      expiringSoonCount: 1,
      reviewPendingCount: 0,
    },
    documents: [
      {
        documentId: 'doc-1',
        documentKey: 'w9-001',
        documentTypeKey: 'w9',
        title: 'W-9',
        version: 1,
        reviewStatus: 'approved',
        effectiveStatus: 'approved',
        isExpired: false,
        isExpiringSoon: false,
        expiresAt: '2026-12-31T00:00:00Z',
        effectiveAt: '2026-01-01T00:00:00Z',
        fileName: 'w9.pdf',
        contentType: 'application/pdf',
        sizeBytes: 1000,
        notes: '',
        reviewedAt: '2026-01-01T00:00:00Z',
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-01T00:00:00Z',
      },
      {
        documentId: 'doc-2',
        documentKey: 'coi-001',
        documentTypeKey: 'insurance',
        title: 'Certificate of insurance',
        version: 1,
        reviewStatus: 'approved',
        effectiveStatus: 'approved',
        isExpired: false,
        isExpiringSoon: true,
        expiresAt: '2026-07-01T00:00:00Z',
        effectiveAt: '2026-01-01T00:00:00Z',
        fileName: 'coi.pdf',
        contentType: 'application/pdf',
        sizeBytes: 1500,
        notes: '',
        reviewedAt: '2026-01-01T00:00:00Z',
        createdAt: '2026-01-01T00:00:00Z',
        updatedAt: '2026-01-01T00:00:00Z',
      },
    ],
  }),
  exportSupplierReportSummaryCsv: vi.fn(),
}))

describe('SupplierReportsPanel', () => {
  it('renders supplier report summary rows', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <SupplierReportsPanel accessToken="token" canRead={true} canExport={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('supplier-reports-panel')).toBeInTheDocument()
    expect(await screen.findByText(/Acme Supply · North Yard Counter \(ACME\)/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Export CSV/i })).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: /Acme Supply · North Yard Counter \(ACME\)/i }))

    expect(await screen.findByText(/Supplier scorecard/i)).toBeInTheDocument()
    expect(screen.getAllByText(/Sub-unit/i).length).toBeGreaterThan(0)
    expect(screen.getAllByText(/Parts, Maintenance/i).length).toBeGreaterThan(0)
    expect(screen.getAllByText(/Healthy/i).length).toBeGreaterThan(0)
    expect(screen.getByText(/Recent fill rate/i)).toBeInTheDocument()
    expect(screen.getAllByText(/86%/i).length).toBeGreaterThan(0)
    expect(screen.getByText(/Average lead time/i)).toBeInTheDocument()
    expect(screen.getByText(/4 days/i)).toBeInTheDocument()
    expect(screen.getByText(/On-time delivery/i)).toBeInTheDocument()
    expect(screen.getAllByText(/100%/i).length).toBeGreaterThan(0)
    expect(screen.getByText(/Lead-time coverage/i)).toBeInTheDocument()
    expect(screen.getByText(/Quote competitiveness/i)).toBeInTheDocument()
    expect(screen.getAllByText(/50%/i).length).toBeGreaterThan(0)
    expect(screen.getByText(/Avg quote response/i)).toBeInTheDocument()
    expect(screen.getByText(/3 days/i)).toBeInTheDocument()
    expect(screen.getByText(/Approval health/i)).toBeInTheDocument()
    expect(screen.getByText(/Approved and active/i)).toBeInTheDocument()
    expect(screen.getByText(/Document posture/i)).toBeInTheDocument()
    expect(screen.getByText(/Compliance documents/i)).toBeInTheDocument()
  })

  it('returns null when user cannot read reports', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <SupplierReportsPanel accessToken="token" canRead={false} canExport={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
