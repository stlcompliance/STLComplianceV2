import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { RuleChangeMonitoringPanel } from './RuleChangeMonitoringPanel'

vi.mock('../api/client', () => ({
  getRuleChangeSummary: vi.fn().mockResolvedValue({
    totalEvents: 3,
    eventsLast24Hours: 2,
    eventsLast7Days: 3,
    versionCreatedCount: 1,
    statusChangedCount: 1,
    contentUpdatedCount: 0,
    scanDetectedCount: 1,
    generatedAt: new Date().toISOString(),
  }),
  listRuleChangeEvents: vi.fn().mockResolvedValue([
    {
      eventId: 'e1',
      rulePackId: 'p1',
      packKey: 'test_pack',
      programKey: 'dot',
      changeType: 'status_changed',
      summary: 'Status changed from review to published.',
      fromStatus: 'review',
      toStatus: 'published',
      fromVersion: 1,
      toVersion: 1,
      previousContentHash: null,
      newContentHash: null,
      source: 'api',
      actorUserId: null,
      scanRunId: null,
      detectedAt: new Date().toISOString(),
    },
  ]),
}))

describe('RuleChangeMonitoringPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders summary and event list', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <RuleChangeMonitoringPanel accessToken="token" />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Rule change monitoring/)).toBeInTheDocument()
    expect(await screen.findByText(/test_pack/)).toBeInTheDocument()
    expect(screen.getByText(/Last 24 hours/)).toBeInTheDocument()
  })
})
