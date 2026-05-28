import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { StaffarrPublicationSettingsPanel } from './StaffarrPublicationSettingsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getStaffarrPublicationSettings: vi.fn(),
    upsertStaffarrPublicationSettings: vi.fn(),
    getStaffarrPublicationDeliveries: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <StaffarrPublicationSettingsPanel accessToken="token" canManage />
    </QueryClientProvider>,
  )
}

describe('StaffarrPublicationSettingsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders settings and empty delivery list', async () => {
    vi.mocked(client.getStaffarrPublicationSettings).mockResolvedValue({
      isEnabled: true,
      maxAttempts: 10,
      retryIntervalMinutes: 5,
      updatedAt: '2026-05-28T12:00:00Z',
    })
    vi.mocked(client.getStaffarrPublicationDeliveries).mockResolvedValue({ items: [] })

    renderPanel()

    await waitFor(() => {
      expect(screen.getByTestId('staffarr-publication-retry-enabled')).toBeChecked()
    })
    expect(screen.getByTestId('staffarr-publication-max-attempts')).toHaveValue(10)
    await waitFor(() => {
      expect(screen.getByTestId('staffarr-publication-deliveries-empty')).toBeInTheDocument()
    })
  })
})
