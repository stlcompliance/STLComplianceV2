import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { IntegrationEventSettingsPanel } from './IntegrationEventSettingsPanel'

vi.mock('../api/client', () => ({
  getIntegrationEventSettings: vi.fn().mockResolvedValue({
    tenantId: '00000000-0000-0000-0000-000000000001',
    isEnabled: true,
    maxAttempts: 5,
    retryIntervalMinutes: 15,
    updatedAt: new Date().toISOString(),
  }),
  upsertIntegrationEventSettings: vi.fn(),
  getIntegrationEventOutbox: vi.fn().mockResolvedValue({ items: [] }),
  getIntegrationEventInbox: vi.fn().mockResolvedValue({ items: [] }),
}))

describe('IntegrationEventSettingsPanel', () => {
  it('renders when user can manage settings', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <IntegrationEventSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )
    expect(await screen.findByTestId('integration-event-settings-panel')).toBeInTheDocument()
  })

  it('returns null when user cannot manage', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <IntegrationEventSettingsPanel accessToken="token" canManage={false} />
      </QueryClientProvider>,
    )
    expect(container).toBeEmptyDOMElement()
  })
})
