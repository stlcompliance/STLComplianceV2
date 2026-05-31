import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { DefectEscalationSettingsPanel } from './DefectEscalationSettingsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getDefectEscalationSettings: vi.fn(),
    getPendingDefectEscalations: vi.fn(),
    getDefectEscalationRuns: vi.fn(),
    getDefectEscalationEvents: vi.fn(),
    upsertDefectEscalationSettings: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <DefectEscalationSettingsPanel accessToken="token" canManage />
    </QueryClientProvider>,
  )
}

describe('DefectEscalationSettingsPanel', () => {
  beforeEach(() => {
    vi.mocked(client.getDefectEscalationSettings).mockResolvedValue({
      isEnabled: true,
      lowThresholdHours: 168,
      mediumThresholdHours: 72,
      highThresholdHours: 24,
      criticalThresholdHours: 8,
      autoAcknowledgeOnEscalation: true,
      autoCreateWorkOrderOnEscalation: true,
      bumpSeverityOnRepeatEscalation: true,
      notifyOnEscalation: true,
      updatedAt: null,
    })
    vi.mocked(client.getPendingDefectEscalations).mockResolvedValue({
      asOfUtc: '2026-05-31T00:00:00Z',
      batchSize: 100,
      items: [],
    })
    vi.mocked(client.getDefectEscalationRuns).mockResolvedValue({ items: [] })
    vi.mocked(client.getDefectEscalationEvents).mockResolvedValue({ items: [] })
    vi.mocked(client.upsertDefectEscalationSettings).mockResolvedValue({
      isEnabled: true,
      lowThresholdHours: 168,
      mediumThresholdHours: 72,
      highThresholdHours: 24,
      criticalThresholdHours: 8,
      autoAcknowledgeOnEscalation: true,
      autoCreateWorkOrderOnEscalation: true,
      bumpSeverityOnRepeatEscalation: true,
      notifyOnEscalation: true,
      updatedAt: null,
    })
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retry callout when settings fail to load', async () => {
    vi.mocked(client.getDefectEscalationSettings).mockRejectedValue(new Error('settings down'))
    renderPanel()

    expect(await screen.findByText('Escalation settings unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry settings' })).toBeInTheDocument()
  })
})
