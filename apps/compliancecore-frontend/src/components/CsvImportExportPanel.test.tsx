import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { CsvImportExportPanel } from './CsvImportExportPanel'
import * as client from '../api/client'

vi.mock('../api/client', () => ({
  getCsvBundleManifest: vi.fn().mockResolvedValue({
    files: [
      {
        fileName: 'controlled_vocabulary.csv',
        headers: ['term_key', 'vocabulary_type_key', 'label', 'description', 'active'],
      },
    ],
  }),
  exportCsvBundleZip: vi.fn(),
  importCsvBundle: vi.fn(),
}))

describe('CsvImportExportPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders bundle title and export action', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <CsvImportExportPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/10-CSV import \/ export/)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Download ZIP export/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Validate import/i })).toBeInTheDocument()
  })

  it('hides import controls for read-only users', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <CsvImportExportPanel accessToken="token" canManage={false} />
      </QueryClientProvider>,
    )

    expect(screen.getByText(/CSV import requires compliance admin/i)).toBeInTheDocument()
    expect(screen.queryByLabelText(/Dry run/i)).not.toBeInTheDocument()
  })

  it('shows export errors in shared callout', async () => {
    vi.mocked(client.exportCsvBundleZip).mockRejectedValueOnce(new Error('export unavailable'))

    const clientQuery = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={clientQuery}>
        <CsvImportExportPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    fireEvent.click(await screen.findByRole('button', { name: /Download ZIP export/i }))

    expect(await screen.findByText('export unavailable')).toBeInTheDocument()
    expect(screen.getByRole('alert')).toBeInTheDocument()
  })
})
