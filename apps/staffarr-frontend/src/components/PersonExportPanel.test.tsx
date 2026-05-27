import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { PersonExportPanel } from './PersonExportPanel'

vi.mock('../api/client', () => ({
  getPeopleExportManifest: vi.fn().mockResolvedValue({
    packageVersion: '1',
    csvHeader: 'givenName,familyName,primaryEmail',
    formats: [{ key: 'csv', contentType: 'text/csv', fileName: 'people.csv', description: 'CSV' }],
  }),
  exportPeopleCsv: vi.fn(),
  exportPeopleJson: vi.fn(),
  exportPeopleZip: vi.fn(),
}))

function renderPanel(canExport: boolean) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <PersonExportPanel accessToken="token" canExport={canExport} />
    </QueryClientProvider>,
  )
}

describe('PersonExportPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('shows read-only notice for non-writers', () => {
    renderPanel(false)
    expect(screen.getByText(/Person export requires tenant admin/i)).toBeTruthy()
    expect(screen.queryByRole('button', { name: /Download CSV/i })).toBeNull()
  })

  it('renders export controls for writers', async () => {
    renderPanel(true)
    expect(await screen.findByText(/Person export bundle/i)).toBeTruthy()
    expect(screen.getByRole('button', { name: /Download CSV/i })).toBeTruthy()
    expect(screen.getByRole('button', { name: /Download ZIP bundle/i })).toBeTruthy()
  })
})
