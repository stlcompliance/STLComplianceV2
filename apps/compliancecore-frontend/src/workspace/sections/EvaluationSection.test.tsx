import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { EvaluationSection } from './EvaluationSection'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

vi.mock('../../components/SituationEvaluatorPanel', () => ({
  SituationEvaluatorPanel: () => <div data-testid="situation-evaluator-panel" />,
}))

vi.mock('../../components/RuleEvaluationPanel', () => ({
  RuleEvaluationPanel: () => <div data-testid="rule-evaluation-panel" />,
}))

vi.mock('../../components/EvaluationHistoryExplorerPanel', () => ({
  EvaluationHistoryExplorerPanel: () => <div data-testid="evaluation-history-explorer-panel" />,
}))

function buildState(): ComplianceCoreWorkspaceState {
  const queryStub = { data: [] }
  const mutationStub = { mutate: () => undefined, isPending: false }
  return {
    accessToken: 'token',
    me: {
      userId: 'user-1',
      personId: 'person-1',
      email: 'admin@demo.stl',
      displayName: 'Demo Admin',
      tenantId: 'tenant-1',
      tenantRoleKey: 'compliance_admin',
      isPlatformAdmin: false,
      productKey: 'compliancecore',
      hasComplianceCoreEntitlement: true,
      entitlements: ['compliancecore'],
      canManageVocabulary: true,
      canExportAuditPackage: true,
      canEvaluateRiskScores: true,
      canEvaluateMissingEvidenceWarnings: true,
      canEvaluateControlEffectiveness: true,
      canEvaluateReadinessForecast: true,
      canReadReports: true,
      canExportReports: true,
    },
    selectedRulePackId: 'rp-1',
    setSelectedRulePackId: () => undefined,
    rulePacksQuery: queryStub,
    factDefinitionsQuery: queryStub,
    factRequirementsQuery: queryStub,
    rulePackContentQuery: { data: { content: null, hasContent: false } },
    ruleEvaluationsQuery: queryStub,
    allRuleEvaluationsQuery: queryStub,
    canManage: true,
    canExportAudit: true,
    saveRuleContentMutation: mutationStub,
    seedRuleContentMutation: mutationStub,
    evaluateRulePackMutation: mutationStub,
    evaluateRulePackBatchMutation: mutationStub,
    lastEvaluation: null,
    lastBatchEvaluation: null,
    lastGateCheck: null,
    lastGateBatch: null,
  } as unknown as ComplianceCoreWorkspaceState
}

describe('EvaluationSection', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders the evaluation explorer alongside the rule evaluation panel', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <EvaluationSection state={buildState()} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('situation-evaluator-panel')).toBeTruthy()
    expect(screen.getByTestId('rule-evaluation-panel')).toBeTruthy()
    expect(screen.getByTestId('evaluation-history-explorer-panel')).toBeTruthy()
  })
})
