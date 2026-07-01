import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'

import { SupplierQuotePortalPage } from './SupplierQuotePortalPage'

const mockData = vi.hoisted(() => ({
  initialPortal: {
    rfqId: 'rfq-1',
    rfqKey: 'RFQ-001',
    title: 'Brake pad RFQ',
    notes: 'Urgent replenishment',
    status: 'submitted',
    supplierId: 'supplier-unit-1',
    supplierKey: 'ACME',
    supplierDisplayName: 'North Yard Counter',
    parentSupplierDisplayName: 'Acme Supply',
    supplierUnitKind: 'sub_unit',
    supplierServiceTypes: ['parts', 'maintenance'],
    invitationId: 'inv-1',
    invitationStatus: 'invited',
    invitedAt: '2026-05-01T00:00:00Z',
    portalAccessExpiresAt: '2026-05-15T00:00:00Z',
    supplierQuoteId: null,
    quoteKey: null,
    quoteStatus: null,
    currencyCode: null,
    totalAmount: null,
    leadTimeDays: null,
    quoteNotes: null,
    submittedAt: null,
    lines: [
      {
        rfqLineId: 'line-1',
        lineNumber: 1,
        partId: 'part-1',
        partKey: 'FILTER-01',
        partDisplayName: 'Oil Filter',
        quantityRequested: 10,
        unitOfMeasure: 'each',
        notes: '',
        quoteLineId: null,
        unitPrice: null,
        quantityQuoted: null,
        leadTimeDays: null,
        quoteNotes: '',
      },
    ],
    createdAt: '2026-05-01T00:00:00Z',
    updatedAt: '2026-05-01T00:00:00Z',
  },
  quotePortal: {
    rfqId: 'rfq-1',
    rfqKey: 'RFQ-001',
    title: 'Brake pad RFQ',
    notes: 'Urgent replenishment',
    status: 'submitted',
    supplierId: 'supplier-unit-1',
    supplierKey: 'ACME',
    supplierDisplayName: 'North Yard Counter',
    parentSupplierDisplayName: 'Acme Supply',
    supplierUnitKind: 'sub_unit',
    supplierServiceTypes: ['parts', 'maintenance'],
    invitationId: 'inv-1',
    invitationStatus: 'invited',
    invitedAt: '2026-05-01T00:00:00Z',
    portalAccessExpiresAt: '2026-05-15T00:00:00Z',
    supplierQuoteId: 'quote-1',
    quoteKey: 'QUOTE-1',
    quoteStatus: 'draft',
    currencyCode: 'USD',
    totalAmount: 100,
    leadTimeDays: 12,
    quoteNotes: 'Ready to quote',
    submittedAt: null,
    lines: [
      {
        rfqLineId: 'line-1',
        lineNumber: 1,
        partId: 'part-1',
        partKey: 'FILTER-01',
        partDisplayName: 'Oil Filter',
        quantityRequested: 10,
        unitOfMeasure: 'each',
        notes: '',
        quoteLineId: 'quote-line-1',
        unitPrice: 10,
        quantityQuoted: 10,
        leadTimeDays: 12,
        quoteNotes: 'Include expedited shipping',
      },
    ],
    createdAt: '2026-05-01T00:00:00Z',
    updatedAt: '2026-05-02T00:00:00Z',
  },
}))

vi.mock('../../api/client', () => ({
  getSupplierPortalRfq: vi
    .fn()
    .mockResolvedValueOnce(mockData.initialPortal)
    .mockResolvedValue(mockData.quotePortal),
  createSupplierPortalQuote: vi.fn().mockResolvedValue({
    supplierQuoteId: 'quote-1',
    rfqId: 'rfq-1',
    supplierId: 'supplier-unit-1',
    supplierKey: 'ACME',
    supplierDisplayName: 'North Yard Counter',
    quoteKey: 'QUOTE-1',
    status: 'draft',
    currencyCode: 'USD',
    totalAmount: null,
    leadTimeDays: null,
    notes: 'Ready to quote',
    submittedAt: null,
    lines: [],
    createdAt: '2026-05-02T00:00:00Z',
    updatedAt: '2026-05-02T00:00:00Z',
  }),
  upsertSupplierPortalQuoteLine: vi.fn().mockResolvedValue({
    supplierQuoteId: 'quote-1',
    rfqId: 'rfq-1',
    supplierId: 'supplier-unit-1',
    supplierKey: 'ACME',
    supplierDisplayName: 'North Yard Counter',
    quoteKey: 'QUOTE-1',
    status: 'draft',
    currencyCode: 'USD',
    totalAmount: 100,
    leadTimeDays: 12,
    notes: 'Ready to quote',
    submittedAt: null,
    lines: [],
    createdAt: '2026-05-02T00:00:00Z',
    updatedAt: '2026-05-02T00:00:00Z',
  }),
  submitSupplierPortalQuote: vi.fn().mockResolvedValue({
    supplierQuoteId: 'quote-1',
    rfqId: 'rfq-1',
    supplierId: 'supplier-unit-1',
    supplierKey: 'ACME',
    supplierDisplayName: 'North Yard Counter',
    quoteKey: 'QUOTE-1',
    status: 'submitted',
    currencyCode: 'USD',
    totalAmount: 100,
    leadTimeDays: 12,
    notes: 'Ready to quote',
    submittedAt: '2026-05-02T00:00:00Z',
    lines: [],
    createdAt: '2026-05-02T00:00:00Z',
    updatedAt: '2026-05-02T00:00:00Z',
  }),
}))

describe('SupplierQuotePortalPage', () => {
  it('loads an invitation and lets the supplier create, update, and submit a quote', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={['/supplier-quote-portal?rfqId=rfq-1&accessCode=code-1']}>
          <SupplierQuotePortalPage />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    expect(await screen.findByText('RFQ-001')).toBeInTheDocument()
    expect(screen.getByText('Acme Supply · North Yard Counter')).toBeInTheDocument()
    expect(screen.getByText('Sub-unit')).toBeInTheDocument()
    expect(screen.getByText('Parts, Maintenance')).toBeInTheDocument()
    expect(screen.getByText(/Create a quote draft/i)).toBeInTheDocument()

    fireEvent.change(screen.getByLabelText(/Quote key/i), { target: { value: 'QUOTE-1' } })
    fireEvent.click(screen.getByRole('button', { name: /Create quote draft/i }))

    await waitFor(() =>
      expect(screen.getByRole('button', { name: /Refresh quote draft/i })).toBeInTheDocument(),
    )

    fireEvent.change(screen.getByTestId('supplier-quote-portal-unit-price-line-1'), {
      target: { value: '12.50' },
    })
    fireEvent.click(screen.getByRole('button', { name: /Save line/i }))
    await waitFor(() => expect(screen.getByText(/Quote total/i)).toBeInTheDocument())
    fireEvent.click(screen.getByRole('button', { name: /Submit quote/i }))

    const { createSupplierPortalQuote, submitSupplierPortalQuote, upsertSupplierPortalQuoteLine } =
      await import('../../api/client')
    expect(createSupplierPortalQuote).toHaveBeenCalledWith('rfq-1', 'code-1', {
      quoteKey: 'QUOTE-1',
      currencyCode: 'USD',
      notes: '',
    })
    expect(upsertSupplierPortalQuoteLine).toHaveBeenCalledWith('rfq-1', 'quote-1', 'code-1', {
      rfqLineId: 'line-1',
      unitPrice: 12.5,
      quantityQuoted: 10,
      leadTimeDays: null,
      notes: '',
    })
    expect(submitSupplierPortalQuote).toHaveBeenCalledWith('rfq-1', 'quote-1', 'code-1')
  })
})
