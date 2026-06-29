import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { within } from '@testing-library/dom'

import { RfqPanel } from './RfqPanel'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@stl/shared-ui')>()

  return {
    ...actual,
    StaticSearchPicker: ({
      label,
      value,
      onChange,
      options,
      placeholder,
      testId,
    }: {
      label: string
      value: string
      onChange: (value: string) => void
      options: Array<{ value: string; label: string }>
      placeholder?: string
      testId?: string
    }) => (
      <label>
        <span>{label}</span>
        <input
          aria-label={label}
          data-testid={testId}
          placeholder={placeholder}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        />
        <div data-testid={`${testId ?? 'picker'}-options`}>
          {options.map((option) => (
            <span key={option.value}>{option.label}</span>
          ))}
        </div>
      </label>
    ),
  }
})

const mockData = vi.hoisted(() => ({
  rfq: {
    rfqId: 'rfq-1',
    rfqKey: 'RFQ-001',
    title: 'Oil filters',
    notes: '',
    status: 'submitted',
    requestedByUserId: 'user-1',
    submittedAt: '2026-05-01T00:00:00Z',
    awardedVendorPartyId: null,
    awardedVendorDisplayName: null,
    selectedVendorQuoteId: null,
    purchaseRequestId: null,
    awardedAt: null,
    lines: [
      {
        lineId: 'line-1',
        lineNumber: 1,
        partId: 'part-1',
        partKey: 'FILTER-01',
        partDisplayName: 'Oil Filter',
        quantityRequested: 10,
        unitOfMeasure: 'each',
        notes: '',
        createdAt: '2026-05-01T00:00:00Z',
        updatedAt: '2026-05-01T00:00:00Z',
      },
    ],
    invitations: [
      {
        invitationId: 'inv-1',
        vendorPartyId: 'vendor-1',
        vendorPartyKey: 'ACME',
        vendorDisplayName: 'Acme Supply',
        status: 'invited',
        invitedAt: '2026-05-01T00:00:00Z',
        portalAccessCodeIssuedAt: '2026-05-01T00:00:00Z',
        portalAccessExpiresAt: '2026-05-15T00:00:00Z',
        portalAccessCode: 'portal-code-1',
        portalUrl: '/vendor-portal?rfqId=rfq-1&accessCode=portal-code-1',
      },
      {
        invitationId: 'inv-2',
        vendorPartyId: 'vendor-2',
        vendorPartyKey: 'BETA',
        vendorDisplayName: 'Beta Parts',
        status: 'invited',
        invitedAt: '2026-05-01T00:00:00Z',
        portalAccessCodeIssuedAt: '2026-05-01T00:00:00Z',
        portalAccessExpiresAt: '2026-05-15T00:00:00Z',
        portalAccessCode: 'portal-code-2',
        portalUrl: '/vendor-portal?rfqId=rfq-1&accessCode=portal-code-2',
      },
    ],
    quotes: [
      {
        vendorQuoteId: 'vq-1',
        rfqId: 'rfq-1',
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
        vendorPartyId: 'vendor-2',
        vendorPartyKey: 'BETA',
        vendorDisplayName: 'Beta Parts',
        quoteKey: 'Q-BETA-1',
        status: 'submitted',
        currencyCode: 'USD',
        totalAmount: 120,
        leadTimeDays: 10,
        notes: '',
        submittedAt: '2026-05-05T00:00:00Z',
        lines: [],
        createdAt: '2026-05-01T00:00:00Z',
        updatedAt: '2026-05-05T00:00:00Z',
      },
    ],
    createdAt: '2026-05-01T00:00:00Z',
    updatedAt: '2026-05-05T00:00:00Z',
  },
}))

vi.mock('../api/client', () => ({
  getRfqs: vi.fn().mockResolvedValue([mockData.rfq]),
  getRfq: vi.fn().mockResolvedValue(mockData.rfq),
  getRfqQuoteComparison: vi.fn().mockResolvedValue({
    rfqId: 'rfq-1',
    rfqKey: 'RFQ-001',
    status: 'submitted',
    quoteSummaries: [
      {
        vendorQuoteId: 'vq-1',
        vendorPartyId: 'vendor-1',
        vendorDisplayName: 'Acme Supply',
        status: 'submitted',
        totalAmount: 100,
        maxLeadTimeDays: 14,
        linesQuoted: 1,
        isSelected: false,
      },
      {
        vendorQuoteId: 'vq-2',
        vendorPartyId: 'vendor-2',
        vendorDisplayName: 'Beta Parts',
        status: 'submitted',
        totalAmount: 120,
        maxLeadTimeDays: 10,
        linesQuoted: 1,
        isSelected: false,
      },
    ],
    lines: [
      {
        rfqLineId: 'line-1',
        lineNumber: 1,
        partId: 'part-1',
        partKey: 'FILTER-01',
        partDisplayName: 'Oil Filter',
        quantityRequested: 10,
        quotes: [
          {
            vendorQuoteId: 'vq-1',
            vendorPartyId: 'vendor-1',
            vendorDisplayName: 'Acme Supply',
            quoteStatus: 'submitted',
            unitPrice: 10,
            lineTotal: 100,
            leadTimeDays: 14,
            isLowestPrice: true,
            isFastestLeadTime: false,
          },
          {
            vendorQuoteId: 'vq-2',
            vendorPartyId: 'vendor-2',
            vendorDisplayName: 'Beta Parts',
            quoteStatus: 'submitted',
            unitPrice: 12,
            lineTotal: 120,
            leadTimeDays: 10,
            isLowestPrice: false,
            isFastestLeadTime: true,
          },
        ],
      },
    ],
  }),
  createRfq: vi.fn(),
  submitRfq: vi.fn(),
  inviteRfqVendors: vi.fn(),
  createVendorQuote: vi.fn(),
  upsertVendorQuoteLine: vi.fn(),
  submitVendorQuote: vi.fn(),
  selectRfqVendorQuote: vi.fn(),
  createPurchaseRequestFromRfq: vi.fn(),
}))

describe('RfqPanel', () => {
  it('renders when user can manage RFQs', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
    <QueryClientProvider client={client}>
      <RfqPanel
        accessToken="token"
        canManage={true}
        canAward={true}
        parts={[]}
        vendors={[
          {
            partyId: 'vendor-1',
            displayName: 'Acme Supply',
            partyKey: 'ACME',
          },
          {
            partyId: 'vendor-2',
            displayName: 'Beta Parts',
            partyKey: 'BETA',
          },
        ]}
        vendorDirectory={[
          {
            partyId: 'vendor-1',
            displayName: 'Acme Supply',
            partyKey: 'ACME',
            approvalStatus: 'approved',
            status: 'active',
          },
          {
            partyId: 'vendor-2',
            displayName: 'Beta Parts',
            partyKey: 'BETA',
            approvalStatus: 'restricted',
            status: 'active',
          },
        ]}
      />
    </QueryClientProvider>,
  )
    expect(await screen.findByTestId('rfq-panel')).toBeInTheDocument()
    expect(await screen.findByTestId('rfq-picker-options')).toHaveTextContent(
      'RFQ-001 · submitted · Oil filters',
    )

    fireEvent.change(screen.getByTestId('rfq-picker'), { target: { value: 'rfq-1' } })
    expect(await screen.findByText(/Quote analytics/i)).toBeInTheDocument()
    const inviteVendorSelect = screen.getByLabelText(/Invite supplier unit/i)
    expect(within(inviteVendorSelect).getByRole('option', { name: /Acme Supply \(ACME\)/i })).not.toBeDisabled()
    expect(within(inviteVendorSelect).getByRole('option', { name: /Beta Parts \(BETA\) \(inactive\)/i })).toBeDisabled()

    const quoteVendorSelect = screen.getByLabelText(/Quote supplier unit/i)
    expect(within(quoteVendorSelect).getByRole('option', { name: /Acme Supply \(approved · active\)/i })).not.toBeDisabled()
    expect(within(quoteVendorSelect).getByRole('option', { name: /Beta Parts \(restricted · active\) \(inactive\)/i })).toBeDisabled()
    expect(screen.getByText(/Quote Q-ACME-1/i)).toBeInTheDocument()
    expect(screen.getByText(/Response time 2 days/i)).toBeInTheDocument()
    expect(screen.getByText(/Best price/i)).toBeInTheDocument()
    expect(screen.getByText(/Quote Q-BETA-1/i)).toBeInTheDocument()
    expect(screen.getByText(/Response time 4 days/i)).toBeInTheDocument()
    expect(screen.getByText(/Approved source/i)).toBeInTheDocument()
    expect(screen.getByText(/Source attention: restricted/i)).toBeInTheDocument()
    expect(screen.getByText(/Supplier portal access/i)).toBeInTheDocument()
    expect(screen.getByDisplayValue('portal-code-1')).toBeInTheDocument()
  })

  it('returns null when user cannot manage', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <RfqPanel
          accessToken="token"
          canManage={false}
          canAward={false}
          parts={[]}
          vendors={[]}
          vendorDirectory={[]}
        />
      </QueryClientProvider>,
    )
    expect(container).toBeEmptyDOMElement()
  })
})
