import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi, afterEach } from 'vitest'
import { cleanup } from '@testing-library/react'

import { AttachmentRetentionSettingsPanel } from './AttachmentRetentionSettingsPanel'
import * as client from '../api/client'

vi.mock('../api/client', () => ({
  getAttachmentRetentionSettings: vi.fn().mockResolvedValue({
    isEnabled: false,
    retentionDaysAfterTripClose: 365,
    updatedAt: null,
  }),
  upsertAttachmentRetentionSettings: vi.fn(),
  getAttachmentRetentionRuns: vi.fn().mockResolvedValue({ items: [] }),
}))

describe('AttachmentRetentionSettingsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders attachment retention settings panel', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <AttachmentRetentionSettingsPanel accessToken="token" canManage />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('attachment-retention-settings-panel')).toBeInTheDocument()
    expect(screen.getByText(/Trip capture attachment retention/i)).toBeInTheDocument()
  })

  it('shows retry callout when settings fail', async () => {
    vi.mocked(client.getAttachmentRetentionSettings).mockRejectedValue(new Error('settings down'))
    const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={qc}>
        <AttachmentRetentionSettingsPanel accessToken="token" canManage />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Retention settings unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry settings' })).toBeInTheDocument()
  })
})
