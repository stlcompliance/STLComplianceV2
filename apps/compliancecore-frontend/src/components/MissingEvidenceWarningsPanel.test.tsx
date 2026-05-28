import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { MissingEvidenceWarningsPanel } from './MissingEvidenceWarningsPanel'

vi.mock('../api/client', () => ({
  getMissingEvidenceWarningSummary: vi.fn().mockResolvedValue({
    totalWarnings: 1,
    scopesTracked: 1,
    lowCount: 0,
    mediumCount: 0,
    highCount: 1,
    criticalCount: 0,
    highestSeverity: 'high',
    lastEvaluatedAt: new Date().toISOString(),
    generatedAt: new Date().toISOString(),
  }),
  listMissingEvidenceWarnings: vi.fn().mockResolvedValue([
    {
      warningId: 'w1',
      runId: 'r1',
      scopeKey: 'tenant',
      rulePackId: 'p1',
      packKey: 'dispatch_gate',
      factKey: 'license_valid',
      factDefinitionId: 'f1',
      warningType: 'rule_pack_fact',
      severity: 'high',
      reasonCode: 'missing_mirror',
      hasMirrorAtScope: false,
      isRequiredInRule: true,
      isRequiredInCatalog: false,
      summary: 'Predicted missing evidence for dispatch_gate/license_valid: missing_mirror (high)',
      evaluatedAt: new Date().toISOString(),
    },
  ]),
  evaluateMissingEvidenceWarnings: vi.fn(),
}))

describe('MissingEvidenceWarningsPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders summary and evaluate control for evaluators', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <MissingEvidenceWarningsPanel accessToken="token" canEvaluate={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Missing evidence warnings/)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Evaluate missing evidence/i })).toBeInTheDocument()
    expect(await screen.findByText(/dispatch_gate\/license_valid/)).toBeInTheDocument()
    expect(screen.getByText('Highest')).toBeInTheDocument()
  })

  it('hides evaluate for read-only users', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <MissingEvidenceWarningsPanel accessToken="token" canEvaluate={false} />
      </QueryClientProvider>,
    )

    expect(screen.getByText(/requires compliance admin/i)).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /Evaluate missing evidence/i })).not.toBeInTheDocument()
  })
})
