import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ReadinessForecastPanel } from './ReadinessForecastPanel'

vi.mock('../api/client', () => ({
  getReadinessForecastSummary: vi.fn().mockResolvedValue({
    totalForecasts: 1,
    scopesTracked: 1,
    readyCount: 1,
    cautionCount: 0,
    notReadyCount: 0,
    unknownCount: 0,
    readinessScore: 82,
    readinessLevel: 'ready',
    lowestReadinessScore: 82,
    averageReadinessScore: 82,
    lastForecastedAt: new Date().toISOString(),
    generatedAt: new Date().toISOString(),
  }),
  listReadinessForecasts: vi.fn().mockResolvedValue([
    {
      forecastId: 'f1',
      runId: 'r1',
      scopeKey: 'tenant',
      rulePackId: 'p1',
      packKey: 'dispatch_gate',
      readinessScore: 82,
      readinessLevel: 'ready',
      riskScore: 12,
      riskLevel: 'low',
      effectivenessScore: 88,
      effectivenessLevel: 'effective',
      missingEvidenceWarningCount: 0,
      highestMissingEvidenceSeverity: 'low',
      summary:
        'Readiness forecast for dispatch_gate: 82 (ready) from risk 12, effectiveness 88, 0 missing-evidence warning(s).',
      forecastedAt: new Date().toISOString(),
    },
  ]),
  evaluateReadinessForecast: vi.fn(),
}))

describe('ReadinessForecastPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders summary and evaluate control for evaluators', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ReadinessForecastPanel accessToken="token" canEvaluate={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Readiness forecasting/)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Forecast readiness/i })).toBeInTheDocument()
    expect(
      await screen.findByText(/Readiness forecast for dispatch_gate: 82 \(ready\)/),
    ).toBeInTheDocument()
    expect(screen.getByText('Forecast')).toBeInTheDocument()
  })

  it('hides evaluate for read-only users', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ReadinessForecastPanel accessToken="token" canEvaluate={false} />
      </QueryClientProvider>,
    )

    expect(screen.getByText(/requires compliance admin/i)).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /Forecast readiness/i })).not.toBeInTheDocument()
  })
})
