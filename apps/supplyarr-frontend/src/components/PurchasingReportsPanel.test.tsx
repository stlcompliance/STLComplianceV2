import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

import { PurchasingReportsPanel } from './PurchasingReportsPanel'

vi.mock('../api/client', () => ({
  getPurchasingReportSummary: vi.fn().mockResolvedValue({
    generatedAt: new Date().toISOString(),
    totals: {
      purchaseRequestCount: 1,
      openPurchaseRequestCount: 1,
      purchaseOrderCount: 1,
      openPurchaseOrderCount: 0,
      issuedPurchaseOrderCount: 1,
      draftReceivingReceiptCount: 0,
      postedReceivingReceiptCount: 1,
      openBackorderCount: 0,
      openPurchaseOrderLineQuantity: 10,
      purchaseOrderQuantityReceived: 4,
    },
    analytics: {
      pendingPurchaseRequestCount: 1,
      emergencyPurchaseRequestCount: 1,
      activeProcurementExceptionCount: 1,
      openReceivingExceptionCount: 1,
      openWarrantyClaimCount: 1,
      vendorDocumentExpiringSoonCount: 1,
      blockedVendorCount: 1,
      averageLeadTimeDays: 8,
      estimatedSpendThisMonth: 25,
    },
    purchaseRequestStatusCounts: [{ status: 'submitted', count: 1 }],
    purchaseOrderStatusCounts: [{ status: 'issued', count: 1 }],
    documents: [
      {
        documentType: 'purchase_request',
        documentId: 'pr-1',
        documentKey: 'PR-001',
        title: 'Shop restock',
        status: 'submitted',
        vendorPartyId: 'vendor-1',
        vendorDisplayName: 'Acme Supply',
        lineCount: 2,
        quantityOrdered: 10,
        quantityReceived: 0,
        updatedAt: new Date().toISOString(),
      },
      {
        documentType: 'purchase_order',
        documentId: 'po-1',
        documentKey: 'PO-001',
        title: 'Shop restock PO',
        status: 'issued',
        vendorPartyId: 'vendor-1',
        vendorDisplayName: 'Acme Supply',
        lineCount: 2,
        quantityOrdered: 10,
        quantityReceived: 4,
        updatedAt: new Date().toISOString(),
      },
    ],
  }),
  getPurchasingPurchaseRequestDetail: vi.fn(),
  getPurchasingPurchaseOrderDetail: vi.fn(),
  exportPurchasingReportSummaryCsv: vi.fn(),
}))

describe('PurchasingReportsPanel', () => {
  it('renders purchasing report documents', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <PurchasingReportsPanel accessToken="token" canRead={true} canExport={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('purchasing-reports-panel')).toBeInTheDocument()
    expect(await screen.findByText(/PR PR-001 · Shop restock/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Export CSV/i })).toBeInTheDocument()
    expect(screen.getByText('Pending approvals')).toBeInTheDocument()
    expect(screen.getByText('Open LoadArr receiving exceptions')).toBeInTheDocument()
    expect(screen.getByText('$25')).toBeInTheDocument()
  })

  it('returns null when user cannot read reports', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <PurchasingReportsPanel accessToken="token" canRead={false} canExport={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
