import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { RulePackImpactSettingsPanel } from './RulePackImpactSettingsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getRulePackImpactSettings: vi.fn(),
    upsertRulePackImpactSettings: vi.fn(),
    getRulePackImpactStates: vi.fn(),
    getRulePackImpactRuns: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <RulePackImpactSettingsPanel accessToken="token" canManage />
    </QueryClientProvider>,
  )
}

describe('RulePackImpactSettingsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders settings and empty run/state lists', async () => {
    vi.mocked(client.getRulePackImpactSettings).mockResolvedValue({
      isEnabled: true,
      stalenessHours: 24,
      autoUpdateRequirementBaselines: false,
      updatedAt: '2026-05-28T12:00:00Z',
    })
    vi.mocked(client.getRulePackImpactStates).mockResolvedValue({ items: [] })
    vi.mocked(client.getRulePackImpactRuns).mockResolvedValue({ items: [] })

    renderPanel()

    await waitFor(() => {
      expect(screen.getByTestId('rule-pack-impact-enabled')).toBeChecked()
    })
    expect(screen.getByTestId('rule-pack-impact-staleness-hours')).toHaveValue(24)
    expect(screen.getByTestId('rule-pack-impact-states-empty')).toBeInTheDocument()
    expect(screen.getByTestId('rule-pack-impact-runs-empty')).toBeInTheDocument()
  })
})
