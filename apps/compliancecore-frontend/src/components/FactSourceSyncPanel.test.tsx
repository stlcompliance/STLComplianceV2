import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen } from '@testing-library/react'
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

  it('renders source-level sync failures in shared callout', async () => {
    vi.mocked(client.getFactSourceSyncHealth).mockResolvedValue({
      tenantId: 'tenant-1',
      workerEnabled: true,
      intervalMinutes: 60,
      lastBatchRunAt: null,
      productApiSourceCount: 1,
      healthyCount: 0,
      staleCount: 0,
      failedCount: 1,
      pendingCount: 0,
      sources: [
        {
          factSourceId: 'source-1',
          sourceKey: 'staffarr_med_cert',
          factKey: 'medical_cert_on_file',
          sourceType: 'product_api',
          productKey: 'staffarr',
          scopeKey: 'tenant',
          healthStatus: 'failed',
          lastAttemptAt: '2026-05-29T12:00:00Z',
          lastSuccessAt: null,
          lastFailureAt: '2026-05-29T12:00:00Z',
          lastErrorMessage: 'upstream timeout',
          consecutiveFailureCount: 1,
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

    expect(await screen.findByText('Last sync failed')).toBeInTheDocument()
    expect(screen.getByText('upstream timeout')).toBeInTheDocument()
    expect(screen.getByRole('alert')).toBeInTheDocument()
  })

  it('renders retryable callout when health query fails', async () => {
    vi.mocked(client.getFactSourceSyncHealth).mockRejectedValueOnce(new Error('health service unavailable'))
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

    expect(await screen.findByText('Sync health unavailable')).toBeInTheDocument()
    expect(screen.getByText('health service unavailable')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Retry health' }))
  })

  it('renders informational callout when health payload is unavailable', async () => {
    vi.mocked(client.getFactSourceSyncHealth).mockResolvedValue(null as never)
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

    expect(await screen.findByText('Sync health unavailable')).toBeInTheDocument()
    expect(screen.getByText('No sync health data is available yet.')).toBeInTheDocument()
  })
})
