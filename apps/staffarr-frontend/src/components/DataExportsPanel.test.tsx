import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { DataExportsPanel } from './DataExportsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getEntityExportManifest: vi.fn(),
    exportBulkPeopleCsv: vi.fn(),
    exportBulkPersonnelIncidentsCsv: vi.fn(),
    exportBulkPersonCertificationsCsv: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <DataExportsPanel accessToken="token" canExport />
    </QueryClientProvider>,
  )
}

describe('DataExportsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retry callout when manifest fails', async () => {
    vi.mocked(client.getEntityExportManifest).mockRejectedValue(new Error('manifest down'))
    renderPanel()

    expect(await screen.findByText('Export manifest unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry manifest' })).toBeTruthy()
  })

  it('shows export failure callout', async () => {
    vi.mocked(client.getEntityExportManifest).mockResolvedValue({
      packageVersion: '2',
      entities: [
        {
          entityKey: 'people',
          exportPath: '/v1/reports/people/export',
          displayName: 'People',
          csvHeader: 'personId,displayName',
          description: 'People export',
          formats: [
            {
              formatKey: 'csv',
              contentType: 'text/csv',
              fileNameTemplate: 'people.csv',
              description: 'People CSV export',
            },
          ],
        },
      ],
      reportExports: [],
      auditPackageFormats: [],
    })
    vi.mocked(client.exportBulkPeopleCsv).mockRejectedValue(new Error('export down'))
    renderPanel()

    await screen.findByText('People')
    fireEvent.click(screen.getByRole('button', { name: 'Download CSV' }))
    expect(await screen.findByText('CSV export failed')).toBeTruthy()
  })
})
