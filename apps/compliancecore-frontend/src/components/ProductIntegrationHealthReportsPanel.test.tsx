import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { ProductIntegrationHealthReportsPanel } from './ProductIntegrationHealthReportsPanel'

vi.mock('../api/client', () => ({
  getProductIntegrationHealthReportSummary: vi.fn(),
  exportProductIntegrationHealthReportSummaryCsv: vi.fn(),
}))

describe('ProductIntegrationHealthReportsPanel', () => {
  it('renders integration health summary and exports csv', async () => {
    const originalCreateElement = document.createElement.bind(document)
    vi.spyOn(document, 'createElement').mockImplementation((tagName: string) => {
      if (tagName === 'a') {
        return {
          click: vi.fn(),
          href: '',
          download: '',
        } as unknown as HTMLAnchorElement
      }

      return originalCreateElement(tagName)
    })
    vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:mock')
    vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => {})
    vi.mocked(client.getProductIntegrationHealthReportSummary).mockResolvedValue({
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
          lastAttemptAt: '2026-05-29T12:00:00Z',
          lastSuccessAt: '2026-05-29T12:00:00Z',
          lastFailureAt: null,
          lastErrorMessage: null,
          consecutiveFailureCount: 0,
        },
      ],
    })
    vi.mocked(client.exportProductIntegrationHealthReportSummaryCsv).mockResolvedValue(
      new Blob(['csv'], { type: 'text/csv' }),
    )

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <ProductIntegrationHealthReportsPanel accessToken="token" canRead canExport />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Product integration health report')).toBeInTheDocument()
    expect(await screen.findByText('staffarr_med_cert')).toBeInTheDocument()
    expect(await screen.findByText('healthy')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Export CSV' }))
    await waitFor(() =>
      expect(vi.mocked(client.exportProductIntegrationHealthReportSummaryCsv)).toHaveBeenCalledWith(
        'token',
      ),
    )
  })
})
