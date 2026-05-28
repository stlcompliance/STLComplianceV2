import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { QualificationRecalculationSettingsPanel } from './QualificationRecalculationSettingsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getQualificationRecalculationSettings: vi.fn(),
    upsertQualificationRecalculationSettings: vi.fn(),
    getQualificationRecalculationStates: vi.fn(),
    getQualificationRecalculationRuns: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <QualificationRecalculationSettingsPanel accessToken="token" canManage />
    </QueryClientProvider>,
  )
}

describe('QualificationRecalculationSettingsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders settings and empty run/state lists', async () => {
    vi.mocked(client.getQualificationRecalculationSettings).mockResolvedValue({
      isEnabled: true,
      stalenessHours: 24,
      autoSuspendOnBlock: false,
      updatedAt: '2026-05-28T12:00:00Z',
    })
    vi.mocked(client.getQualificationRecalculationStates).mockResolvedValue({ items: [] })
    vi.mocked(client.getQualificationRecalculationRuns).mockResolvedValue({ items: [] })

    renderPanel()

    await waitFor(() => {
      expect(screen.getByTestId('qualification-recalculation-enabled')).toBeChecked()
    })
    expect(screen.getByTestId('qualification-recalculation-staleness-hours')).toHaveValue(24)
    expect(screen.getByTestId('qualification-recalculation-states-empty')).toBeInTheDocument()
    expect(screen.getByTestId('qualification-recalculation-runs-empty')).toBeInTheDocument()
  })
})
