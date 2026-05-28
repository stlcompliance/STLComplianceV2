import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { AuditPackageExportPanel } from './AuditPackageExportPanel'

vi.mock('../api/client', () => ({
  getAuditPackageManifest: vi.fn().mockResolvedValue({
    packageVersion: '1',
    sections: [
      {
        key: 'training_assignments',
        fileName: 'training_assignments.json',
        label: 'Training assignments',
        description: 'Person training assignment records.',
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

    expect(await screen.findByText(/Training audit package export/)).toBeInTheDocument()
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

    expect(screen.getByText(/requires tenant admin/i)).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /Download ZIP package/i })).not.toBeInTheDocument()
  })
})
