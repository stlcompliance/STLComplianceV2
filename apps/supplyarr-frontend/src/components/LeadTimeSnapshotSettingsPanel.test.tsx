import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { LeadTimeSnapshotSettingsPanel } from './LeadTimeSnapshotSettingsPanel'

vi.mock('../api/client', () => ({
  getLeadTimeSnapshotSettings: vi.fn().mockResolvedValue({
    isEnabled: false,
    stalenessHours: 24,
    updatedAt: null,
  }),
  getPendingLeadTimeSnapshotCaptures: vi.fn().mockResolvedValue({
    asOfUtc: '2026-05-28T00:00:00Z',
    stalenessHours: 24,
    batchSize: 25,
    items: [],
  }),
  getLeadTimeSnapshotRuns: vi.fn().mockResolvedValue({ items: [] }),
  upsertLeadTimeSnapshotSettings: vi.fn(),
}))

describe('LeadTimeSnapshotSettingsPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders lead-time snapshot settings for admins', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <LeadTimeSnapshotSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Lead-time snapshot worker/)).toBeTruthy()
    expect(screen.getByRole('button', { name: /Save lead-time snapshot settings/i })).toBeTruthy()
  })

  it('hides panel when user cannot manage settings', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { container } = render(
      <QueryClientProvider client={client}>
        <LeadTimeSnapshotSettingsPanel accessToken="token" canManage={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
