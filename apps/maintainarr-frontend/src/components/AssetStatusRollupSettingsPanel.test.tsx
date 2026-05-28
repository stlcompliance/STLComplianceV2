import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

import { AssetStatusRollupSettingsPanel } from './AssetStatusRollupSettingsPanel'

vi.mock('../api/client', () => ({
  getAssetStatusRollupSettings: vi.fn().mockResolvedValue({
    isEnabled: true,
    stalenessHours: 1,
    updatedAt: null,
  }),
  upsertAssetStatusRollupSettings: vi.fn(),
  getPendingAssetStatusRollups: vi.fn().mockResolvedValue({
    asOfUtc: new Date().toISOString(),
    stalenessHours: 1,
    batchSize: 25,
    items: [],
  }),
  getAssetStatusRollupRuns: vi.fn().mockResolvedValue({ items: [] }),
  getFleetAssetStatusRollup: vi.fn().mockRejectedValue(new Error('not found')),
}))

describe('AssetStatusRollupSettingsPanel', () => {
  it('renders worker settings panel', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <AssetStatusRollupSettingsPanel accessToken="token" canManage />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('asset-status-rollup-settings-panel')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Asset status rollup worker' })).toBeInTheDocument()
  })
})
