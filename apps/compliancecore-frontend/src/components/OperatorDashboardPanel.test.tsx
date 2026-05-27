import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { OperatorDashboardPanel } from './OperatorDashboardPanel'

vi.mock('../api/client', () => ({
  getOperatorDashboard: vi.fn().mockResolvedValue({
    findings: {
      openCount: 2,
      openBlockSeverityCount: 1,
      openWarnSeverityCount: 1,
      acknowledgedCount: 0,
      resolvedCount: 3,
      totalCount: 5,
    },
    rulePacks: {
      draftCount: 1,
      reviewCount: 0,
      publishedCount: 2,
      archivedCount: 0,
      totalCount: 3,
    },
    evaluations: {
      totalCount: 10,
      last24HoursCount: 4,
      passCount: 6,
      failCount: 4,
    },
    workflowGates: {
      definitionCount: 2,
      checkResultsTotal: 8,
      checkResultsLast24Hours: 3,
      blockOutcomeCount: 2,
      warnOutcomeCount: 1,
      allowOutcomeCount: 5,
    },
    auditEvents: {
      totalCount: 20,
      last24HoursCount: 5,
      successCount: 18,
      failureCount: 2,
    },
    recentEvaluations: [
      {
        evaluationRunId: 'run-1',
        rulePackId: 'pack-1',
        rulePackLabel: 'Driver Qualification',
        packKey: 'driver_qualification',
        overallResult: 'fail',
        createdAt: '2026-05-27T12:00:00Z',
      },
    ],
    generatedAt: '2026-05-27T12:05:00Z',
  }),
}))

describe('OperatorDashboardPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders live dashboard counts from the API', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <OperatorDashboardPanel accessToken="token" />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Operator overview/)).toBeInTheDocument()
    expect(screen.getByText('Total findings')).toBeInTheDocument()
    expect(screen.getByText('Open (block severity)')).toBeInTheDocument()
    expect(screen.getByText('Gate check failures')).toBeInTheDocument()
    expect(screen.getByText('Driver Qualification')).toBeInTheDocument()
    expect(screen.getByText('fail')).toBeInTheDocument()
  })
})
