import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { EvidenceRetentionSettingsPanel } from './EvidenceRetentionSettingsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getEvidenceRetentionSettings: vi.fn(),
    upsertEvidenceRetentionSettings: vi.fn(),
    getEvidenceRetentionRuns: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <EvidenceRetentionSettingsPanel accessToken="token" canManage />
    </QueryClientProvider>,
  )
}

describe('EvidenceRetentionSettingsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders settings and empty run list', async () => {
    vi.mocked(client.getEvidenceRetentionSettings).mockResolvedValue({
      isEnabled: true,
      retentionDaysAfterAssignmentClose: 365,
      updatedAt: '2026-05-28T12:00:00Z',
    })
    vi.mocked(client.getEvidenceRetentionRuns).mockResolvedValue({ items: [] })

    renderPanel()

    await waitFor(() => {
      expect(screen.getByTestId('evidence-retention-enabled')).toBeChecked()
    })
    expect(screen.getByTestId('evidence-retention-days')).toHaveValue(365)
    expect(screen.getByTestId('evidence-retention-runs-empty')).toBeInTheDocument()
  })
})
