import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { RfqPanel } from './RfqPanel'

vi.mock('../api/client', () => ({
  getRfqs: vi.fn().mockResolvedValue([]),
  getRfq: vi.fn(),
  getRfqQuoteComparison: vi.fn(),
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
          vendors={[]}
        />
      </QueryClientProvider>,
    )
    expect(await screen.findByTestId('rfq-panel')).toBeInTheDocument()
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
        />
      </QueryClientProvider>,
    )
    expect(container).toBeEmptyDOMElement()
  })
})
