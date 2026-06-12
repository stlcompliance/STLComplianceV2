import { cleanup, render, screen, fireEvent } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'
import { MemoryRouter } from 'react-router-dom'

import { RegistryDetailProfile } from './RegistryDetailProfile'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

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
    programsQuery: {
      data: [
        {
          regulatoryProgramId: 'program-1',
          regulatoryProgramKey: 'fmcsa_safety',
          regulatoryProgramLabel: 'FMCSA Safety Compliance',
          jurisdictionLabel: 'Federal',
        },
      ],
    },
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
    factDefinitionsQuery: queryStub,
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
    factSourcesQuery: {
      data: [{ factSourceId: 'source-1', factKey: 'license_valid', sourceKey: 'staffarr', sourceType: 'api', productKey: 'staffarr', scopeKey: 'tenant', healthStatus: 'healthy', lastAttemptAt: null, lastSuccessAt: null, lastFailureAt: null, lastErrorMessage: null, consecutiveFailureCount: 0, isActive: true }],
    },
    regulatoryMappingsQuery: {
      data: [
        {
          regulatoryMappingId: 'map-1',
          rulePackId: 'pack-1',
          regulatoryProgramId: 'program-1',
          label: 'License validity mapping',
          targetKind: 'compliance_key',
        },
      ],
    },
    rulePackContentQuery: {
      data: {
        content: {
          schemaVersion: 1,
          logic: 'license_valid must be true',
          rules: [
            {
              ruleKey: 'license_valid',
              label: 'License valid',
              type: 'required',
              factKey: 'license_valid',
              expectedValue: true,
            },
          ],
        },
        hasContent: true,
      },
    },
    ruleEvaluationsQuery: {
      data: [
        {
          evaluationRunId: 'eval-1',
          createdAt: '2026-06-03T00:00:00Z',
          overallResult: 'pass',
          ruleResults: [],
        },
      ],
    },
    allRuleEvaluationsQuery: queryStub,
    findingsQuery: { data: [] },
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

describe('RegistryDetailProfile', () => {
  afterEach(() => {
    cleanup()
  })

  it('switches the main panel when a tab is selected', () => {
    render(
      <MemoryRouter initialEntries={['/registry/details?tab=overview']}>
        <RegistryDetailProfile state={buildState()} />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'Rules and citations' })).toBeTruthy()

    fireEvent.click(screen.getByRole('tab', { name: 'Citations' }))

    expect(screen.getByRole('heading', { name: 'Citation links' })).toBeTruthy()
    expect(screen.queryByRole('heading', { name: 'Rules and citations' })).toBeNull()
  })
})
