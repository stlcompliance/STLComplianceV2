import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { AvailabilitySnapshotSettingsPanel } from './AvailabilitySnapshotSettingsPanel'

vi.mock('../api/client', () => ({
  getAvailabilitySnapshotSettings: vi.fn().mockResolvedValue({
    isEnabled: false,
    stalenessHours: 24,
    updatedAt: null,
  }),
  getPendingAvailabilitySnapshotCaptures: vi.fn().mockResolvedValue({
    asOfUtc: '2026-05-28T00:00:00Z',
    stalenessHours: 24,
    batchSize: 25,
    items: [],
  }),
  getAvailabilitySnapshotRuns: vi.fn().mockResolvedValue({ items: [] }),
  upsertAvailabilitySnapshotSettings: vi.fn(),
}))

describe('AvailabilitySnapshotSettingsPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders availability snapshot settings for admins', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <AvailabilitySnapshotSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Availability snapshot worker/)).toBeTruthy()
    expect(screen.getByRole('button', { name: /Save availability snapshot settings/i })).toBeTruthy()
  })

  it('hides panel when user cannot manage settings', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { container } = render(
      <QueryClientProvider client={client}>
        <AvailabilitySnapshotSettingsPanel accessToken="token" canManage={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
