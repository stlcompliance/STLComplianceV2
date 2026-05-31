import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import * as client from '../api/client'
import { TripExecutionSettingsPanel } from './TripExecutionSettingsPanel'

vi.mock('../api/client', () => ({
  getTripExecutionSettings: vi.fn().mockResolvedValue({
    requirePreTripDvirBeforeStart: true,
    requirePostTripDvirBeforeComplete: false,
    requireDeliveryProofBeforeComplete: false,
    requirePickupProofBeforeStart: false,
    blockTripStartOnDvirFail: true,
    blockTripCompleteOnDvirFail: true,
    requirePickupProofPhotoBeforeStart: false,
    requireDeliveryProofPhotoBeforeComplete: false,
    requireDeliverySignatureBeforeComplete: false,
    requirePreTripDvirPhotoBeforeStart: false,
    requirePostTripDvirPhotoBeforeComplete: false,
    updatedAt: null,
  }),
  upsertTripExecutionSettings: vi.fn(),
}))

describe('TripExecutionSettingsPanel', () => {
  afterEach(() => cleanup())

  it('renders capture policy controls', async () => {
    const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={qc}>
        <TripExecutionSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )
    expect(await screen.findByTestId('trip-execution-settings-panel')).toBeTruthy()
    expect(screen.getByText(/Require pre-trip DVIR before start/)).toBeTruthy()
  })

  it('shows retryable settings error callout when settings query fails', async () => {
    vi.mocked(client.getTripExecutionSettings).mockRejectedValueOnce(
      new Error('settings unavailable'),
    )
    const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={qc}>
        <TripExecutionSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('settings unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry settings' })).toBeTruthy()
  })
})
