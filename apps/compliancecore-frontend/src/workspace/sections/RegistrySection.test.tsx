import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter } from 'react-router-dom'

import { RegistrySection } from './RegistrySection'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

vi.mock('../../components/VocabularyPanel', () => ({
  VocabularyPanel: () => <div data-testid="vocabulary-panel" />,
}))

vi.mock('../../components/RegulatoryRegistryPanel', () => ({
  RegulatoryRegistryPanel: () => <div data-testid="regulatory-registry-panel" />,
}))

vi.mock('../../components/RuleVersionManagementPanel', () => ({
  RuleVersionManagementPanel: () => <div data-testid="rule-version-management-panel" />,
}))

vi.mock('../../components/RuleTestCasesPanel', () => ({
  RuleTestCasesPanel: () => <div data-testid="rule-test-cases-panel" />,
}))

vi.mock('../../components/SdsHazComReferencesPanel', () => ({
  SdsHazComReferencesPanel: () => <div data-testid="sds-hazcom-references-panel" />,
}))

function buildState(): ComplianceCoreWorkspaceState {
  const queryStub = { data: [] }
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
      launchableProductKeys: ['compliancecore'],
      canManageVocabulary: true,
      canExportAuditPackage: true,
      canEvaluateRiskScores: true,
      canEvaluateMissingEvidenceWarnings: true,
      canEvaluateControlEffectiveness: true,
      canEvaluateReadinessForecast: true,
      canReadReports: true,
      canExportReports: true,
    },
    selectedTypeKey: 'material_hazard',
    setSelectedTypeKey: () => undefined,
    selectedRulePackId: 'pack-1',
    setSelectedRulePackId: () => undefined,
    lastEvaluation: null,
    lastBatchEvaluation: null,
    lastGateCheck: null,
    lastGateBatch: null,
    typesQuery: queryStub,
    termsQuery: queryStub,
    complianceKeysQuery: queryStub,
    materialKeysQuery: queryStub,
    governingBodiesQuery: queryStub,
    jurisdictionsQuery: queryStub,
    programsQuery: queryStub,
    rulePacksQuery: {
      data: [
        {
          rulePackId: 'pack-1',
          regulatoryProgramId: 'program-1',
          regulatoryProgramKey: 'fmcsa_safety',
          regulatoryProgramLabel: 'FMCSA Safety Compliance',
          packKey: 'driver_qualification',
          label: 'Driver Qualification Rules',
          description: 'Driver qualification rules',
          versionNumber: 2,
          status: 'draft',
          isActive: true,
          createdAt: '2026-06-01T00:00:00Z',
          updatedAt: '2026-06-01T00:00:00Z',
        },
      ],
    },
    citationsQuery: queryStub,
    factDefinitionsQuery: queryStub,
    factRequirementsQuery: queryStub,
    factSourcesQuery: queryStub,
    regulatoryMappingsQuery: queryStub,
    rulePackContentQuery: { data: { content: null, hasContent: false } },
    ruleEvaluationsQuery: queryStub,
    allRuleEvaluationsQuery: queryStub,
    findingsQuery: queryStub,
    workflowGatesQuery: queryStub,
    seedMutation: { mutate: () => undefined, isPending: false },
    seedRegistryMutation: { mutate: () => undefined, isPending: false },
    advanceRulePackMutation: { mutate: () => undefined, isPending: false },
    seedCatalogMutation: { mutate: () => undefined, isPending: false },
    seedSourcesMutation: { mutate: () => undefined, isPending: false },
    seedMappingsMutation: { mutate: () => undefined, isPending: false },
    saveRuleContentMutation: { mutate: () => undefined, isPending: false },
    seedRuleContentMutation: { mutate: () => undefined, isPending: false },
    evaluateRulePackMutation: { mutate: () => undefined, isPending: false },
    evaluateRulePackBatchMutation: { mutate: () => undefined, isPending: false },
    seedWorkflowGateMutation: { mutate: () => undefined, isPending: false },
    checkWorkflowGateMutation: { mutate: () => undefined, isPending: false },
    checkWorkflowGateBatchMutation: { mutate: () => undefined, isPending: false },
    canManage: true,
    canExportAudit: true,
    canReadOrchestration: false,
    canEvaluateRisk: true,
    canEvaluateMissingEvidence: true,
    canEvaluateControlEffectiveness: true,
    canEvaluateReadinessForecast: true,
  } as unknown as ComplianceCoreWorkspaceState
}

describe('RegistrySection', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders the rule test cases panel in the registry workspace', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={['/registry']}>
          <RegistrySection state={buildState()} />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('vocabulary-panel')).toBeTruthy()
    expect(screen.getByTestId('regulatory-registry-panel')).toBeTruthy()
    expect(screen.getByTestId('rule-version-management-panel')).toBeTruthy()
    expect(screen.getByTestId('rule-test-cases-panel')).toBeTruthy()
    expect(screen.getByTestId('sds-hazcom-references-panel')).toBeTruthy()
  })
})

