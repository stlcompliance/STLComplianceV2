import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { PmDueScanSettingsPanel } from './PmDueScanSettingsPanel'

vi.mock('../api/client', () => ({
  getPmDueScanSettings: vi.fn(),
  upsertPmDueScanSettings: vi.fn(),
  getPendingPmDueScan: vi.fn(),
  getPmDueScanRuns: vi.fn(),
  triggerPmDueScan: vi.fn(),
}))

import * as client from '../api/client'

function renderPanel() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={qc}>
      <PmDueScanSettingsPanel accessToken="token" canManage />
    </QueryClientProvider>,
  )
}

describe('PmDueScanSettingsPanel', () => {
  afterEach(() => cleanup())

  it('renders settings panel with last run and pending count', async () => {
    vi.mocked(client.getPmDueScanSettings).mockResolvedValue({
      isEnabled: true,
      scanIntervalMinutes: 30,
      batchSize: 50,
      overdueGraceDays: 2,
      lastRunAt: '2026-05-28T12:00:00.000Z',
      pendingPmCount: 3,
      updatedAt: '2026-05-28T12:00:00.000Z',
    })
    vi.mocked(client.getPendingPmDueScan).mockResolvedValue({
      asOfUtc: '2026-05-28T12:00:00.000Z',
      batchSize: 50,
      items: [],
    })
    vi.mocked(client.getPmDueScanRuns).mockResolvedValue({ items: [] })

    renderPanel()

    expect(await screen.findByTestId('pm-due-scan-settings-panel')).toBeTruthy()
    await waitFor(
      () => expect(screen.getByTestId('pm-due-scan-pending-count').textContent).toBe('3'),
      { timeout: 3000 },
    )
    expect(screen.getByTestId('pm-due-scan-trigger-button')).toBeTruthy()
  })
})
