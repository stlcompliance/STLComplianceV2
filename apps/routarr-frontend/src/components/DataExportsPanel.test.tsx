import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

import { DataExportsPanel } from './DataExportsPanel'

vi.mock('../api/client', () => ({
  getEntityExportManifest: vi.fn().mockResolvedValue({
    packageVersion: '1',
    entities: [
      {
        entityKey: 'trips',
        route: '/api/exports/trips',
        label: 'Trips',
        csvHeader: 'tripNumber',
        description: 'Trip registry',
        formats: [],
      },
      {
        entityKey: 'routes',
        route: '/api/exports/routes',
        label: 'Routes',
        csvHeader: 'routeNumber',
        description: 'Routes',
        formats: [],
      },
      {
        entityKey: 'dispatch_exceptions',
        route: '/api/exports/dispatch-exceptions',
        label: 'Dispatch exceptions',
        csvHeader: 'exceptionKey',
        description: 'Exceptions',
        formats: [],
      },
    ],
    reportExports: [],
    auditPackageFormats: [],
  }),
  exportTripsCsv: vi.fn(),
  exportRoutesCsv: vi.fn(),
  exportDispatchExceptionsCsv: vi.fn(),
}))

describe('DataExportsPanel', () => {
  it('renders manifest-driven export buttons', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <DataExportsPanel accessToken="token" canExport={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('data-exports-panel')).toBeTruthy()
    expect(await screen.findByText('Trips')).toBeTruthy()
    expect(screen.getByText('Dispatch exceptions')).toBeTruthy()
    expect(screen.getAllByRole('button', { name: /Download CSV/i }).length).toBe(3)
  })

  it('shows role message when export not permitted', () => {
    const client = new QueryClient()
    render(
      <QueryClientProvider client={client}>
        <DataExportsPanel accessToken="token" canExport={false} />
      </QueryClientProvider>,
    )

    expect(screen.getByText(/Bulk CSV exports require/i)).toBeTruthy()
  })
})
