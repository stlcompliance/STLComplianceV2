import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { OrphanReferenceSettingsPanel } from './OrphanReferenceSettingsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getOrphanReferenceSettings: vi.fn(),
    upsertOrphanReferenceSettings: vi.fn(),
    getOrphanReferenceFindings: vi.fn(),
    getOrphanReferenceRuns: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <OrphanReferenceSettingsPanel accessToken="token" canManage />
    </QueryClientProvider>,
  )
}

describe('OrphanReferenceSettingsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders settings and empty findings list', async () => {
    vi.mocked(client.getOrphanReferenceSettings).mockResolvedValue({
      isEnabled: true,
      scanStalenessHours: 24,
      updatedAt: '2026-05-28T12:00:00Z',
    })
    vi.mocked(client.getOrphanReferenceFindings).mockResolvedValue({ items: [] })
    vi.mocked(client.getOrphanReferenceRuns).mockResolvedValue({ items: [] })

    renderPanel()

    await waitFor(() => {
      expect(screen.getByTestId('orphan-reference-enabled')).toBeChecked()
    })
    expect(screen.getByTestId('orphan-reference-staleness-hours')).toHaveValue(24)
    expect(screen.getByTestId('orphan-reference-findings-empty')).toBeInTheDocument()
    expect(screen.getByTestId('orphan-reference-runs-empty')).toBeInTheDocument()
  })
})
