import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ErpExportsPanel } from './ErpExportsPanel'

describe('ErpExportsPanel', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('renders ERP export buttons and downloads purchase order csv', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      blob: vi.fn().mockResolvedValue(new Blob(['csv,data'])),
    })
    vi.stubGlobal('fetch', fetchMock)
    const createObjectUrlSpy = vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:url')
    const revokeObjectUrlSpy = vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => {})
    const clickSpy = vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => {})

    render(
      <QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}>
        <ErpExportsPanel accessToken="token" canExport={true} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('supplyarr-erp-exports-panel')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Export Purchase orders CSV/i })).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: /Export Purchase orders CSV/i }))

    await waitFor(() =>
      expect(fetchMock).toHaveBeenCalledWith(
        expect.stringContaining('/api/v1/exports/purchase-orders.csv'),
        expect.objectContaining({
          headers: expect.objectContaining({
            Authorization: 'Bearer token',
          }),
        }),
      ),
    )

    expect(createObjectUrlSpy).toHaveBeenCalled()
    expect(clickSpy).toHaveBeenCalled()
    expect(revokeObjectUrlSpy).toHaveBeenCalled()
  })

  it('returns null when user cannot export', () => {
    const { container } = render(
      <QueryClientProvider client={new QueryClient()}>
        <ErpExportsPanel accessToken="token" canExport={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
