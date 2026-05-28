import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ControlEffectivenessPanel } from './ControlEffectivenessPanel'

vi.mock('../api/client', () => ({
  getControlEffectivenessSummary: vi.fn().mockResolvedValue({
    totalControls: 1,
    scopesTracked: 1,
    effectiveCount: 0,
    partiallyEffectiveCount: 0,
    ineffectiveCount: 1,
    unknownCount: 0,
    lowestEffectivenessScore: 42,
    lowestEffectivenessLevel: 'ineffective',
    averageEffectivenessScore: 42,
    lastEvaluatedAt: new Date().toISOString(),
    generatedAt: new Date().toISOString(),
  }),
  listControlEffectivenessRecords: vi.fn().mockResolvedValue([
    {
      recordId: 'r1',
      runId: 'run1',
      scopeKey: 'tenant',
      rulePackId: 'p1',
      packKey: 'dispatch_gate',
      effectivenessScore: 42,
      effectivenessLevel: 'ineffective',
      controlStatus: 'failing',
      ruleOutcome: 'block',
      evaluationResult: 'fail',
      totalRuleCount: 1,
      passedRuleCount: 0,
      failedRuleCount: 1,
      unresolvedFactCount: 1,
      resolvedFactCount: 0,
      summary:
        'Control dispatch_gate effectiveness 42 (ineffective, failing): block with 0/1 rule(s) passing and 1 unresolved fact(s).',
      evaluatedAt: new Date().toISOString(),
    },
  ]),
  evaluateControlEffectiveness: vi.fn(),
}))

describe('ControlEffectivenessPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders summary and evaluate control for evaluators', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ControlEffectivenessPanel accessToken="token" canEvaluate={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Control effectiveness/)).toBeInTheDocument()
    expect(
      screen.getByRole('button', { name: /Evaluate control effectiveness/i }),
    ).toBeInTheDocument()
    expect(
      await screen.findByText(/Control dispatch_gate effectiveness 42/),
    ).toBeInTheDocument()
    expect(screen.getByText('Average')).toBeInTheDocument()
  })

  it('hides evaluate for read-only users', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ControlEffectivenessPanel accessToken="token" canEvaluate={false} />
      </QueryClientProvider>,
    )

    expect(screen.getByText(/requires compliance admin/i)).toBeInTheDocument()
    expect(
      screen.queryByRole('button', { name: /Evaluate control effectiveness/i }),
    ).not.toBeInTheDocument()
  })
})
