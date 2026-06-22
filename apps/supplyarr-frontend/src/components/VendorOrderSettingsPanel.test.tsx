import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { VendorOrderSettingsPanel } from './VendorOrderSettingsPanel'
import { getVendorOrderSettings, upsertVendorOrderSettings } from '../api/vendorOrderClient'

vi.mock('../api/vendorOrderClient', () => ({
  getVendorOrderSettings: vi.fn().mockResolvedValue({
    allowDestinationSummaryInVendorPortal: false,
    magicLinkTtlHours: 72,
    updatedAt: '2026-06-11T12:00:00Z',
  }),
  upsertVendorOrderSettings: vi.fn(),
}))

describe('VendorOrderSettingsPanel', () => {
  it('shows a safe fallback when saving settings fails', async () => {
    vi.mocked(upsertVendorOrderSettings).mockRejectedValueOnce(new Error('settings service down'))
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => undefined)
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <VendorOrderSettingsPanel accessToken="token-1" canManage />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('vendor-order-settings-panel')).toBeTruthy()

    fireEvent.click(screen.getByRole('checkbox'))

    expect(await screen.findByText('Unable to save vendor-order settings. Please try again.')).toBeTruthy()
    expect(screen.queryByText('settings service down')).toBeNull()
    expect(consoleError).toHaveBeenCalled()
    consoleError.mockRestore()
  })
})
