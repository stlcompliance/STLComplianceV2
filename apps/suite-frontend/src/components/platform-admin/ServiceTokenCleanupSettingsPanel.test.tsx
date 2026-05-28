import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { ServiceTokenCleanupSettingsPanel } from './ServiceTokenCleanupSettingsPanel'

vi.mock('../../api/nexarrClient', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../api/nexarrClient')>()
  return {
    ...actual,
    getServiceTokenCleanupSettings: vi.fn(),
    upsertServiceTokenCleanupSettings: vi.fn(),
    getServiceTokenCleanupRuns: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <ServiceTokenCleanupSettingsPanel />
    </QueryClientProvider>,
  )
}

describe('ServiceTokenCleanupSettingsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders settings and empty run list', async () => {
    vi.mocked(nexarr.getServiceTokenCleanupSettings).mockResolvedValue({
      isEnabled: true,
      retentionDaysAfterExpiry: 7,
      retentionDaysAfterRevoke: 30,
      updatedAt: '2026-05-28T12:00:00Z',
    })
    vi.mocked(nexarr.getServiceTokenCleanupRuns).mockResolvedValue({ items: [] })

    renderPanel()

    await waitFor(() => {
      expect(screen.getByTestId('service-token-cleanup-enabled')).toBeChecked()
    })
    expect(screen.getByTestId('service-token-cleanup-expiry-days')).toHaveValue(7)
    expect(screen.getByTestId('service-token-cleanup-revoke-days')).toHaveValue(30)
    expect(screen.getByTestId('service-token-cleanup-runs-empty')).toBeInTheDocument()
  })
})
