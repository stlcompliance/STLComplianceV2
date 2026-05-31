import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ProcurementApprovalAuthorityBanner } from './ProcurementApprovalAuthorityBanner'
import * as clientApi from '../api/client'

vi.mock('../api/client', () => ({
  getProcurementApprovalAuthority: vi.fn(),
}))

function renderBanner() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <ProcurementApprovalAuthorityBanner accessToken="token" canRead={true} />
    </QueryClientProvider>,
  )
}

describe('ProcurementApprovalAuthorityBanner', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retryable callout when authority mirror query fails', async () => {
    vi.mocked(clientApi.getProcurementApprovalAuthority).mockRejectedValueOnce(
      new Error('authority unavailable'),
    )

    renderBanner()

    expect(await screen.findByText('authority unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry authority mirror' })).toBeInTheDocument()
  })
})
