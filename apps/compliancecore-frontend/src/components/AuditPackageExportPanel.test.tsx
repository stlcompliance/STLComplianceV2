import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { AuditPackageExportPanel } from './AuditPackageExportPanel'
import { exportAuditPackageZip } from '../api/client'

vi.mock('../api/client', () => ({
  getAuditPackageManifest: vi.fn().mockResolvedValue({
    packageVersion: '1',
    sections: [
      {
        key: 'audit_events',
        fileName: 'audit_events.json',
        label: 'Audit events',
        description: 'Tenant audit trail.',
      },
    ],
  }),
  exportAuditPackageZip: vi.fn(),
  exportAuditPackageJson: vi.fn(),
  createAuditPackageGenerationJob: vi.fn(),
  getAuditPackageGenerationJob: vi.fn(),
  downloadAuditPackageGenerationJob: vi.fn(),
}))

describe('AuditPackageExportPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders export controls for authorized users', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <AuditPackageExportPanel accessToken="token" canExport={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Audit package export/)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Download ZIP package/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Background ZIP export/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Preview JSON export/i })).toBeInTheDocument()
  })

  it('hides export actions for read-only users', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <AuditPackageExportPanel accessToken="token" canExport={false} />
      </QueryClientProvider>,
    )

    expect(screen.getByText(/requires compliance admin/i)).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /Download ZIP package/i })).not.toBeInTheDocument()
  })

  it('renders export failures in shared callout', async () => {
    vi.mocked(exportAuditPackageZip).mockRejectedValueOnce(new Error('network unavailable'))

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <AuditPackageExportPanel accessToken="token" canExport={true} />
      </QueryClientProvider>,
    )

    fireEvent.click(await screen.findByRole('button', { name: /Download ZIP package/i }))

    await waitFor(() => {
      expect(screen.getByRole('alert')).toBeInTheDocument()
    })
    expect(screen.getByText('Audit export failed')).toBeInTheDocument()
    expect(screen.getByText('network unavailable')).toBeInTheDocument()
  })
})
