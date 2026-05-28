import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { EmergencyPurchasePanel } from './EmergencyPurchasePanel'

vi.mock('../api/client', () => ({
  getEmergencyPurchases: vi.fn().mockResolvedValue([]),
  listPendingEmergencyPurchases: vi.fn().mockResolvedValue([]),
  createEmergencyPurchase: vi.fn(),
  expeditedSubmitEmergencyPurchase: vi.fn(),
  managerOverrideApproveEmergencyPurchase: vi.fn(),
  issueEmergencyPurchaseOrder: vi.fn(),
}))

describe('EmergencyPurchasePanel', () => {
  it('renders when user can create emergency purchases', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <EmergencyPurchasePanel
          accessToken="token"
          canCreate={true}
          canOverrideApprove={true}
          parts={[]}
          vendors={[]}
        />
      </QueryClientProvider>,
    )
    expect(await screen.findByTestId('emergency-purchase-panel')).toBeInTheDocument()
  })

  it('returns null when user has no emergency permissions', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <EmergencyPurchasePanel
          accessToken="token"
          canCreate={false}
          canOverrideApprove={false}
          parts={[]}
          vendors={[]}
        />
      </QueryClientProvider>,
    )
    expect(container).toBeEmptyDOMElement()
  })
})
