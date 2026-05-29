import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { AttachmentRetentionSettingsPanel } from './AttachmentRetentionSettingsPanel'

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
})
