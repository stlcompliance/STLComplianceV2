import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

import { MaintenanceHistoryRollupSettingsPanel } from './MaintenanceHistoryRollupSettingsPanel'

vi.mock('../api/client', () => ({
  getMaintenanceHistoryRollupSettings: vi.fn().mockResolvedValue({
    isEnabled: true,
    stalenessHours: 1,
    updatedAt: null,
  }),
  upsertMaintenanceHistoryRollupSettings: vi.fn(),
  getPendingMaintenanceHistoryRollups: vi.fn().mockResolvedValue({
    asOfUtc: new Date().toISOString(),
    stalenessHours: 1,
    batchSize: 25,
    items: [],
  }),
  getMaintenanceHistoryRollupRuns: vi.fn().mockResolvedValue({ items: [] }),
}))

describe('MaintenanceHistoryRollupSettingsPanel', () => {
  it('renders worker settings panel', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <MaintenanceHistoryRollupSettingsPanel accessToken="token" canManage />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('maintenance-history-rollup-settings-panel')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Maintenance history rollup worker' })).toBeInTheDocument()
  })
})
