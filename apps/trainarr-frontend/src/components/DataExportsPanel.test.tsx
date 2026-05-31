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
    exportTrainingAssignmentsCsv: vi.fn(),
    exportQualificationIssuesCsv: vi.fn(),
    exportTrainingDefinitionsCsv: vi.fn(),
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

    expect(await screen.findByText('Export manifest unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry manifest' })).toBeInTheDocument()
  })

  it('shows export failure callout', async () => {
    vi.mocked(client.getEntityExportManifest).mockResolvedValue({
      packageVersion: 'test',
      reportExports: [],
      auditPackageFormats: [],
      entities: [
        {
          entityKey: 'training_assignments',
          exportPath: '/api/trainarr/training-assignments/export',
          displayName: 'Training assignments',
          csvHeader: 'training-assignments.csv',
          description: 'Assignments export',
          formats: [
            {
              formatKey: 'csv',
              contentType: 'text/csv',
              fileNameTemplate: 'training-assignments.csv',
              description: 'CSV export',
            },
          ],
        },
      ],
    })
    vi.mocked(client.exportTrainingAssignmentsCsv).mockRejectedValue(new Error('export down'))
    renderPanel()

    await screen.findByText('Training assignments')
    fireEvent.click(screen.getByRole('button', { name: 'Download CSV' }))
    expect(await screen.findByText('CSV export failed')).toBeInTheDocument()
  })
})
