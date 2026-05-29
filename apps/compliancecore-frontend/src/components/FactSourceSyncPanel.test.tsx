import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { FactSourceSyncPanel } from './FactSourceSyncPanel'

vi.mock('../api/client', () => ({
  getFactSourceSyncWorkerSettings: vi.fn(),
  upsertFactSourceSyncWorkerSettings: vi.fn(),
  getFactSourceSyncHealth: vi.fn(),
}))

describe('FactSourceSyncPanel', () => {
  it('renders sync health summary', async () => {
    vi.mocked(client.getFactSourceSyncHealth).mockResolvedValue({
      tenantId: 'tenant-1',
      workerEnabled: true,
      intervalMinutes: 60,
      lastBatchRunAt: null,
      productApiSourceCount: 1,
      healthyCount: 1,
      staleCount: 0,
      failedCount: 0,
      pendingCount: 0,
      sources: [
        {
          factSourceId: 'source-1',
          sourceKey: 'staffarr_med_cert',
          factKey: 'medical_cert_on_file',
          sourceType: 'product_api',
          productKey: 'staffarr',
          scopeKey: 'tenant',
          healthStatus: 'healthy',
          lastAttemptAt: null,
          lastSuccessAt: '2026-05-29T12:00:00Z',
          lastFailureAt: null,
          lastErrorMessage: null,
          consecutiveFailureCount: 0,
        },
      ],
    })
    vi.mocked(client.getFactSourceSyncWorkerSettings).mockResolvedValue({
      isEnabled: true,
      defaultScopeKey: 'tenant',
      intervalMinutes: 60,
      lastBatchRunAt: null,
      updatedAt: null,
    })

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <FactSourceSyncPanel accessToken="token" canManage />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Product API sync health')).toBeInTheDocument()
    expect(await screen.findByText('staffarr_med_cert')).toBeInTheDocument()
    expect(await screen.findByText('healthy')).toBeInTheDocument()
  })
})
