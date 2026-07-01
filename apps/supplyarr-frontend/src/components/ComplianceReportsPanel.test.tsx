import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

import { ComplianceReportsPanel } from './ComplianceReportsPanel'

vi.mock('../api/client', () => ({
  getComplianceReportSummary: vi.fn().mockResolvedValue({
    generatedAt: new Date().toISOString(),
    totals: {
      supplierCount: 1,
      documentCount: 2,
      expiredCount: 1,
      expiringSoonCount: 0,
      reviewPendingCount: 1,
      approvedCount: 0,
      rejectedCount: 0,
    },
    suppliers: [
      {
        supplierId: 'supplier-1',
        supplierKey: 'ACME',
        displayName: 'North Yard Counter',
        parentSupplierId: 'parent-1',
        parentSupplierDisplayName: 'Acme Supply',
        supplierUnitKind: 'sub_unit',
        supplierServiceTypes: ['parts', 'maintenance'],
        approvalStatus: 'approved',
        compliancePosture: 'expired',
        documentCount: 2,
        expiredCount: 1,
        expiringSoonCount: 0,
        reviewPendingCount: 1,
      },
    ],
    documents: [
      {
        documentId: 'doc-1',
        supplierId: 'supplier-1',
        supplierKey: 'ACME',
        supplierDisplayName: 'Acme Supply',
        documentKey: 'COI-2024',
        documentTypeKey: 'certificate_of_insurance',
        title: 'Certificate of Insurance',
        version: 1,
        reviewStatus: 'pending',
        effectiveStatus: 'expired',
        isExpired: true,
        isExpiringSoon: false,
        expiresAt: new Date('2020-01-01').toISOString(),
        updatedAt: new Date().toISOString(),
      },
    ],
  }),
  getComplianceSupplierDetail: vi.fn(),
  exportComplianceReportSummaryCsv: vi.fn(),
}))

describe('ComplianceReportsPanel', () => {
  it('renders compliance report summary rows', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ComplianceReportsPanel accessToken="token" canRead={true} canExport={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('compliance-reports-panel')).toBeInTheDocument()
    expect(await screen.findByText(/Acme Supply · North Yard Counter \(ACME\)/i)).toBeInTheDocument()
    expect(await screen.findByText(/Sub-unit · Parts, Maintenance/i)).toBeInTheDocument()
    expect(await screen.findByText(/COI-2024 · Certificate of Insurance/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Export CSV/i })).toBeInTheDocument()
  })

  it('returns null when user cannot read reports', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <ComplianceReportsPanel accessToken="token" canRead={false} canExport={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
