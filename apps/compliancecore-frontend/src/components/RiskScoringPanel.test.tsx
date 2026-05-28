import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { RiskScoringPanel } from './RiskScoringPanel'

vi.mock('../api/client', () => ({
  getRiskScoreSummary: vi.fn().mockResolvedValue({
    totalScores: 1,
    scopesTracked: 1,
    lowCount: 0,
    mediumCount: 0,
    highCount: 1,
    criticalCount: 0,
    highestRiskScore: 72,
    highestRiskLevel: 'high',
    lastEvaluatedAt: new Date().toISOString(),
    generatedAt: new Date().toISOString(),
  }),
  listRiskScores: vi.fn().mockResolvedValue([
    {
      riskScoreId: 's1',
      runId: 'r1',
      scopeKey: 'tenant',
      rulePackId: 'p1',
      packKey: 'dispatch_gate',
      riskScore: 72,
      riskLevel: 'high',
      ruleOutcome: 'block',
      evaluationResult: 'fail',
      unresolvedFactCount: 1,
      failedRuleCount: 1,
      resolvedFactCount: 0,
      mirrorFactCount: 0,
      summary: 'Risk 72 (high) for dispatch_gate',
      evaluatedAt: new Date().toISOString(),
    },
  ]),
  evaluateRiskScores: vi.fn(),
}))

describe('RiskScoringPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders summary and evaluate control for evaluators', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <RiskScoringPanel accessToken="token" canEvaluate={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Risk scoring/)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Evaluate risk scores/i })).toBeInTheDocument()
    expect(await screen.findByText(/Risk 72 \(high\) for dispatch_gate/)).toBeInTheDocument()
    expect(screen.getByText('Highest')).toBeInTheDocument()
  })

  it('hides evaluate for read-only users', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <RiskScoringPanel accessToken="token" canEvaluate={false} />
      </QueryClientProvider>,
    )

    expect(screen.getByText(/requires compliance admin/i)).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /Evaluate risk scores/i })).not.toBeInTheDocument()
  })
})
