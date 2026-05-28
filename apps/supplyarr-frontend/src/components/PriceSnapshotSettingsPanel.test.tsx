import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { PriceSnapshotSettingsPanel } from './PriceSnapshotSettingsPanel'

vi.mock('../api/client', () => ({
  getPriceSnapshotSettings: vi.fn().mockResolvedValue({
    isEnabled: false,
    stalenessHours: 24,
    updatedAt: null,
  }),
  getPendingPriceSnapshotCaptures: vi.fn().mockResolvedValue({
    asOfUtc: '2026-05-28T00:00:00Z',
    stalenessHours: 24,
    batchSize: 25,
    items: [],
  }),
  getPriceSnapshotRuns: vi.fn().mockResolvedValue({ items: [] }),
  upsertPriceSnapshotSettings: vi.fn(),
}))

describe('PriceSnapshotSettingsPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders price snapshot settings for admins', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <PriceSnapshotSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Price snapshot worker/)).toBeTruthy()
    expect(screen.getByRole('button', { name: /Save price snapshot settings/i })).toBeTruthy()
  })

  it('hides panel when user cannot manage settings', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { container } = render(
      <QueryClientProvider client={client}>
        <PriceSnapshotSettingsPanel accessToken="token" canManage={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
