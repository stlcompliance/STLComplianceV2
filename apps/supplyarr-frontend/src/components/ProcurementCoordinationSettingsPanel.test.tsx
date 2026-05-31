import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

import { ProcurementCoordinationSettingsPanel } from './ProcurementCoordinationSettingsPanel'

vi.mock('../api/client', () => ({
  getProcurementCoordinationSettings: vi.fn().mockResolvedValue({
    isEnabled: false,
    stalenessHours: 2,
    updatedAt: null,
  }),
  getPendingProcurementCoordination: vi.fn().mockResolvedValue({
    asOfUtc: new Date().toISOString(),
    stalenessHours: 2,
    batchSize: 25,
    items: [],
  }),
  getProcurementCoordinationRuns: vi.fn().mockResolvedValue({ items: [] }),
  upsertProcurementCoordinationSettings: vi.fn(),
}))

describe('ProcurementCoordinationSettingsPanel', () => {
  it('renders settings panel for admins', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ProcurementCoordinationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('procurement-coordination-settings-panel')).toBeInTheDocument()
    expect(screen.getByText(/Procurement coordination worker/i)).toBeInTheDocument()
  })

  it('returns null when user cannot manage settings', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <ProcurementCoordinationSettingsPanel accessToken="token" canManage={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })

  it('shows retry callout when settings fail to load', async () => {
    const { getProcurementCoordinationSettings } = await import('../api/client')
    vi.mocked(getProcurementCoordinationSettings).mockRejectedValueOnce(new Error('settings down'))
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ProcurementCoordinationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Coordination settings unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry settings' })).toBeInTheDocument()
  })
})
