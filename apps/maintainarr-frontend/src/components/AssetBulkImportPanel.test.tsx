import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { AssetBulkImportPanel } from './AssetBulkImportPanel'

vi.mock('../api/client', () => ({
  validateAssetImport: vi.fn(),
  commitAssetImport: vi.fn(),
}))

function renderPanel(canImport: boolean) {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <AssetBulkImportPanel accessToken="token" canImport={canImport} />
    </QueryClientProvider>,
  )
}

describe('AssetBulkImportPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('shows read-only notice for non-writers', () => {
    renderPanel(false)
    expect(screen.getByText(/Bulk import requires tenant admin/i)).toBeTruthy()
    expect(screen.queryByRole('button', { name: /^Validate$/i })).toBeNull()
  })

  it('renders import controls for writers', () => {
    renderPanel(true)
    expect(screen.getByRole('button', { name: /^Validate$/i })).toBeTruthy()
    expect(screen.getByText(/vehicles,forklift,FLT-101/i)).toBeTruthy()
  })

  it('reports csv parse errors before submit', () => {
    renderPanel(true)

    fireEvent.change(screen.getByRole('textbox'), {
      target: { value: 'assetClassKey,assetTag,name\nvehicles,FLT-1,Forklift' },
    })
    fireEvent.click(screen.getByRole('button', { name: /^Validate$/i }))

    expect(screen.getByText(/header must include assettypekey/i)).toBeTruthy()
    expect(screen.getByRole('alert')).toBeTruthy()
  })
})
