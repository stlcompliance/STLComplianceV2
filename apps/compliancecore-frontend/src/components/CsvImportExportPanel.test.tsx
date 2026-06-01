import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
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
  getRegulatoryPrograms: vi.fn().mockResolvedValue([
    {
      regulatoryProgramId: 'program-1',
      jurisdictionId: 'jurisdiction-1',
      jurisdictionKey: 'us_federal',
      jurisdictionLabel: 'United States Federal',
      programKey: 'fmcsa_safety',
      label: 'FMCSA Safety',
      description: 'Federal motor carrier safety compliance.',
      isActive: true,
      createdAt: '2026-01-01T00:00:00Z',
    },
  ]),
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

  it('passes registry resolution options into import', async () => {
    vi.mocked(client.importCsvBundle).mockResolvedValueOnce({
      dryRun: true,
      applied: false,
      files: [],
      issues: [],
    })

    const clientQuery = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={clientQuery}>
        <CsvImportExportPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    fireEvent.change(await screen.findByLabelText(/Registry resolution/i), { target: { value: 'create_missing' } })
    fireEvent.change(screen.getByLabelText(/Governing body key/i), { target: { value: 'osha' } })
    fireEvent.change(screen.getByLabelText(/Jurisdiction key/i), { target: { value: 'us_workplace' } })
    fireEvent.change(screen.getByLabelText(/Imported program key/i), { target: { value: 'external_fmcsa' } })
    await screen.findByRole('option', { name: 'fmcsa_safety' })
    fireEvent.change(screen.getByLabelText(/Existing program/i), { target: { value: 'fmcsa_safety' } })
    fireEvent.click(screen.getByRole('button', { name: /Add mapping/i }))

    const file = new File(['pack_key,program_key'], 'rule_packs.csv', { type: 'text/csv' })
    fireEvent.change(screen.getByLabelText(/CSV or ZIP bundle files/i), { target: { files: [file] } })
    fireEvent.click(screen.getByRole('button', { name: /Validate import/i }))

    await waitFor(() => expect(client.importCsvBundle).toHaveBeenCalled())
    expect(client.importCsvBundle).toHaveBeenCalledWith(
      'token',
      expect.anything(),
      true,
      expect.objectContaining({
        regulatorySpineMode: 'create_missing',
        governingBodyKey: 'osha',
        jurisdictionKey: 'us_workplace',
        programMappings: { external_fmcsa: 'fmcsa_safety' },
      }),
    )
  })
})
