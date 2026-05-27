import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { PersonBulkImportPanel } from './PersonBulkImportPanel'

vi.mock('../api/client', () => ({
  importPeopleBulk: vi.fn(),
}))

function renderPanel(canImport: boolean) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <PersonBulkImportPanel accessToken="token" canImport={canImport} />
    </QueryClientProvider>,
  )
}

describe('PersonBulkImportPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('shows read-only notice for non-writers', () => {
    renderPanel(false)
    expect(screen.getByText(/Bulk import requires tenant admin/i)).toBeTruthy()
    expect(screen.queryByRole('button', { name: /Validate import/i })).toBeNull()
  })

  it('renders import controls for writers', () => {
    renderPanel(true)
    expect(screen.getByRole('button', { name: /Validate import/i })).toBeTruthy()
    expect(screen.getByText(/Jane,Doe,jane.doe@example.com/i)).toBeTruthy()
  })

  it('reports csv parse errors before submit', () => {
    renderPanel(true)

    fireEvent.change(screen.getByRole('textbox'), { target: { value: 'givenName,familyName\nOnly,Header' } })
    fireEvent.click(screen.getByRole('button', { name: /Validate import/i }))

    expect(screen.getByText(/header must include primaryemail/i)).toBeTruthy()
  })
})
