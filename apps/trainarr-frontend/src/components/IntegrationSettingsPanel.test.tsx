import { QueryClient, QueryClientProvider } from '@tanstack/react-query'

import { cleanup, render, screen, waitFor } from '@testing-library/react'

import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'

import { IntegrationSettingsPanel } from './IntegrationSettingsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getIntegrationSettings: vi.fn(),
    upsertIntegrationSettings: vi.fn(),
    getIntegrationProbes: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <IntegrationSettingsPanel accessToken="token" canManage />
    </QueryClientProvider>,
  )
}

describe('IntegrationSettingsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders settings and connectivity probes', async () => {
    vi.mocked(client.getIntegrationSettings).mockResolvedValue({
      staffArrIntegrationEnabled: true,
      staffArrIncidentIntakeEnabled: true,
      staffArrPublicationDeliveryEnabled: true,
      complianceCoreIntegrationEnabled: true,
      complianceCoreQualificationChecksEnabled: true,
      routarrIntegrationEnabled: true,
      routarrQualificationDispatchEnabled: true,
      updatedAt: '2026-05-28T12:00:00Z',
    })
    vi.mocked(client.getIntegrationProbes).mockResolvedValue({
      probedAt: '2026-05-28T12:00:00Z',
      items: [
        {
          integrationKey: 'staffarr',
          displayName: 'StaffArr',
          status: 'reachable',
          httpStatusCode: 200,
          message: null,
          probedAt: '2026-05-28T12:00:00Z',
        },
      ],
    })

    renderPanel()

    expect(await screen.findByTestId('integration-settings-panel')).toBeTruthy()
    expect(await screen.findByTestId('integration-probes-list')).toBeTruthy()
    await waitFor(() => {
      expect(screen.getByText(/StaffArr · reachable/)).toBeTruthy()
    })
  })

  it('shows retry callout when settings fail', async () => {
    vi.mocked(client.getIntegrationSettings).mockRejectedValue(new Error('settings down'))
    vi.mocked(client.getIntegrationProbes).mockResolvedValue({ probedAt: null, items: [] })

    renderPanel()

    expect(await screen.findByText('Integration settings unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry settings' })).toBeInTheDocument()
  })
})
