import { cleanup, render, screen, fireEvent } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter } from 'react-router-dom'

import { RequirementDetailPage } from './RequirementDetailPage'
import type { ComplianceCoreWorkspaceState } from '../../workspace/useComplianceCoreWorkspaceState'

vi.mock('../../components/SituationEvaluatorPanel', () => ({
  SituationEvaluatorPanel: () => <div data-testid="situation-evaluator-panel" />,
}))

vi.mock('../../workspace/useComplianceCoreWorkspaceState', () => ({
  useComplianceCoreWorkspaceState: () => buildState(),
}))

function buildState(): ComplianceCoreWorkspaceState {
  const queryStub = { data: [] }
  return {
    handoffRedirect: null,
    ready: true,
    loadingMessage: 'Loading compliance registry…',
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
          updatedAt: '2026-06-02T00:00:00Z',
        },
      ],
    },
    citationsQuery: {
      data: [
        {
          citationId: 'cit-1',
          rulePackId: 'pack-1',
          regulatoryProgramId: 'program-1',
          label: '49 CFR 391.11',
          sourceReference: '49 CFR 391.11',
        },
      ],
    },
    factDefinitionsQuery: {
      data: [
        {
          factDefinitionId: 'fact-1',
          factKey: 'license_valid',
          label: 'Driver license status',
          valueType: 'boolean',
        },
      ],
    },
    factRequirementsQuery: {
      data: [
        {
          factRequirementId: 'req-1',
          requirementKey: 'dq_license_check',
          factKey: 'license_valid',
          label: 'License valid',
          isRequired: true,
          isActive: true,
          rulePackId: 'pack-1',
          rulePackKey: 'driver_qualification',
          citationId: 'cit-1',
          citationKey: '49 CFR 391.11',
          factDefinitionId: 'fact-1',
          factLabel: 'Driver license status',
          description: 'License must be valid',
          createdAt: '2026-06-01T00:00:00Z',
          updatedAt: '2026-06-02T00:00:00Z',
        },
      ],
    },
    factSourcesQuery: queryStub,
    regulatoryMappingsQuery: queryStub,
    rulePackContentQuery: { data: { content: null, hasContent: false } },
    ruleEvaluationsQuery: queryStub,
    allRuleEvaluationsQuery: queryStub,
    findingsQuery: queryStub,
    workflowGatesQuery: queryStub,
    advanceRulePackMutation: { mutate: () => undefined, isPending: false },
    saveRuleContentMutation: { mutate: () => undefined, isPending: false },
    evaluateRulePackMutation: { mutate: () => undefined, isPending: false },
    evaluateRulePackBatchMutation: { mutate: () => undefined, isPending: false },
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

describe('RequirementDetailPage', () => {
  afterEach(() => {
    cleanup()
  })

  it('switches the detail panel when a tab is selected', () => {
    render(
      <MemoryRouter initialEntries={['/requirements/detail?requirementId=req-1&tab=overview']}>
        <RequirementDetailPage />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'Plain-language summary' })).toBeTruthy()

    fireEvent.click(screen.getByRole('tab', { name: 'Logic' }))

    expect(screen.getByRole('heading', { name: 'Applicability and logic' })).toBeTruthy()
    expect(screen.queryByRole('heading', { name: 'Plain-language summary' })).toBeNull()
  })
})
