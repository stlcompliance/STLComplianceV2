import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

import { VendorReportsPanel } from './VendorReportsPanel'

vi.mock('../api/client', () => ({
  getVendorReportSummary: vi.fn().mockResolvedValue({
    generatedAt: new Date().toISOString(),
    approvalStatusCounts: [{ approvalStatus: 'approved', count: 1 }],
    vendors: [
      {
        vendorPartyId: 'vendor-1',
        partyKey: 'ACME',
        displayName: 'Acme Supply',
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
        lastPurchaseOrderAt: new Date().toISOString(),
        lastReceivingPostedAt: new Date().toISOString(),
      },
    ],
  }),
  getVendorReportDetail: vi.fn(),
  exportVendorReportSummaryCsv: vi.fn(),
}))

describe('VendorReportsPanel', () => {
  it('renders vendor report summary rows', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <VendorReportsPanel accessToken="token" canRead={true} canExport={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('vendor-reports-panel')).toBeInTheDocument()
    expect(await screen.findByText(/ACME · Acme Supply/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Export CSV/i })).toBeInTheDocument()
  })

  it('returns null when user cannot read reports', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <VendorReportsPanel accessToken="token" canRead={false} canExport={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
