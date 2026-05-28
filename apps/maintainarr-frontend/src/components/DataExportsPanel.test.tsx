import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { DataExportsPanel } from './DataExportsPanel'

vi.mock('../api/client', () => ({
  getEntityExportManifest: vi.fn().mockResolvedValue({
    packageVersion: '1',
    entities: [
      {
        entityKey: 'assets',
        route: '/api/exports/assets',
        label: 'Assets',
        csvHeader: 'assetClassKey',
        description: 'Asset registry',
        formats: [],
      },
      {
        entityKey: 'work_orders',
        route: '/api/exports/work-orders',
        label: 'Work orders',
        csvHeader: 'workOrderNumber',
        description: 'Work orders',
        formats: [],
      },
    ],
    reportExports: [{ reportKey: 'maintenance', route: '/api/reports/maintenance/summary/export', label: 'Maintenance report CSV', description: '' }],
    auditPackageFormats: [],
  }),
  exportAssetsCsv: vi.fn(),
  exportWorkOrdersCsv: vi.fn(),
  exportInspectionRunsCsv: vi.fn(),
}))

function renderPanel(canExport: boolean) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <DataExportsPanel accessToken="token" canExport={canExport} />
    </QueryClientProvider>,
  )
}

describe('DataExportsPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('shows read-only notice for non-exporters', () => {
    renderPanel(false)
    expect(screen.getByText(/Bulk CSV exports require/i)).toBeTruthy()
    expect(screen.queryByRole('button', { name: /Download CSV/i })).toBeNull()
  })

  it('renders entity export controls for exporters', async () => {
    renderPanel(true)
    expect(await screen.findByText('Assets')).toBeTruthy()
    expect(screen.getByRole('heading', { name: 'Data exports' })).toBeTruthy()
    expect(screen.getAllByText('Work orders').length).toBeGreaterThan(0)
    expect(screen.getAllByRole('button', { name: /Download CSV/i }).length).toBe(2)
  })
})
