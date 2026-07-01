import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { SupplierOrderSettingsPanel } from './SupplierOrderSettingsPanel'
import { upsertSupplierOrderSettings } from '../api/supplierOrderClient'

vi.mock('../api/supplierOrderClient', () => ({
  getSupplierOrderSettings: vi.fn().mockResolvedValue({
    allowDestinationSummaryInSupplierPortal: false,
    magicLinkTtlHours: 72,
    updatedAt: '2026-06-11T12:00:00Z',
  }),
  upsertSupplierOrderSettings: vi.fn(),
}))

describe('SupplierOrderSettingsPanel', () => {
  it('shows a safe fallback when saving settings fails', async () => {
    vi.mocked(upsertSupplierOrderSettings).mockRejectedValueOnce(new Error('settings service down'))
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => undefined)
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <SupplierOrderSettingsPanel accessToken="token-1" canManage />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('supplier-order-settings-panel')).toBeTruthy()

    fireEvent.click(screen.getByRole('checkbox'))

    expect(await screen.findByText('Unable to save supplier-order settings. Please try again.')).toBeTruthy()
    expect(screen.queryByText('settings service down')).toBeNull()
    expect(consoleError).toHaveBeenCalled()
    consoleError.mockRestore()
  })
})
