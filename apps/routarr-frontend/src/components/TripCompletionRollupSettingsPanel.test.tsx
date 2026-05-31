import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { TripCompletionRollupSettingsPanel } from './TripCompletionRollupSettingsPanel'

vi.mock('../api/client', () => ({
  getTripCompletionRollupSettings: vi.fn().mockResolvedValue({
    isEnabled: false,
    stalenessHours: 1,
    updatedAt: null,
  }),
  upsertTripCompletionRollupSettings: vi.fn(),
  getPendingTripCompletionRollups: vi.fn().mockResolvedValue({
    asOfUtc: '2026-05-28T12:00:00Z',
    stalenessHours: 1,
    batchSize: 25,
    items: [],
  }),
  getTripCompletionRollupRuns: vi.fn().mockResolvedValue({ items: [] }),
}))

describe('TripCompletionRollupSettingsPanel', () => {
  it('renders trip completion rollup settings panel', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <TripCompletionRollupSettingsPanel accessToken="token" canManage />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('trip-completion-rollup-settings-panel')).toBeInTheDocument()
    expect(screen.getByText(/Trip completion rollup worker/i)).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Recent worker runs' })).toBeInTheDocument()
    expect(await screen.findByTestId('trip-completion-rollup-runs-empty')).toBeInTheDocument()
  })

  it('shows retry callout when settings fail to load', async () => {
    const { getTripCompletionRollupSettings } = await import('../api/client')
    vi.mocked(getTripCompletionRollupSettings).mockRejectedValueOnce(new Error('settings down'))
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <TripCompletionRollupSettingsPanel accessToken="token" canManage />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Rollup settings unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry settings' })).toBeInTheDocument()
  })
})
