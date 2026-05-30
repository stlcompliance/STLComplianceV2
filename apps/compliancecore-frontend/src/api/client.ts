import type {
  ComplianceCoreMeResponse,
  ComplianceCoreSessionBootstrapResponse,
  ComplianceKeyResponse,
  CreateComplianceKeyRequest,
  CreateGoverningBodyRequest,
  CreateJurisdictionRequest,
  CreateMaterialKeyRequest,
  CreateRegulatoryProgramRequest,
  CreateFactDefinitionRequest,
  CreateFactSourceRequest,
  CreateFactRequirementRequest,
  CreateRegulatoryCitationRequest,
  CreateRegulatoryMappingRequest,
  CreateRulePackRequest,
  CreateVocabularyTermRequest,
  EvaluateRulePackBatchRequest,
  EvaluateRulePackBatchResponse,
  EvaluateRulePackRequest,
  FactDefinitionResponse,
  FactSourceResponse,
  FactRequirementResponse,
  GoverningBodyResponse,
  RegulatoryCitationResponse,
  RegulatoryMappingResponse,
  HandoffSessionResponse,
  JurisdictionResponse,
  MaterialKeyResponse,
  RegulatoryProgramResponse,
  ComplianceFindingResponse,
  CreateWorkflowGateDefinitionRequest,
  RuleEvaluationRunResponse,
  RulePackContentResponse,
  RulePackResponse,
  AuditPackageExportResponse,
  AuditPackageGenerationJobResponse,
  AuditPackageManifestResponse,
  CsvBundleManifestResponse,
  CsvImportResultResponse,
  WorkflowGateBatchCheckRequest,
  WorkflowGateBatchCheckResponse,
  WorkflowGateCheckRequest,
  WorkflowGateCheckResponse,
  WorkflowGateDefinitionResponse,
  OperatorDashboardResponse,
  FindingsReportSummaryResponse,
  OperatorReportSummaryResponse,
  EntityExportManifestResponse,
  SdsReferenceResponse,
  CreateSdsReferenceRequest,
  HazComReferenceResponse,
  CreateHazComReferenceRequest,
  RuleVersionListResponse,
  RuleVersionResponse,
  RuleVersionRollbackResponse,
  FactSourceBulkIngestionRequest,
  SourceIngestionBatchDetailResponse,
  SourceIngestionBatchResponse,
  SourceIngestionBatchSummary,
  RuleChangeEventResponse,
  RuleChangeMonitoringSummaryResponse,
  M12AnalyticsWorkerSettingsResponse,
  UpsertM12AnalyticsWorkerSettingsRequest,
  FactSourceSyncWorkerSettingsResponse,
  UpsertFactSourceSyncWorkerSettingsRequest,
  FactSourceSyncHealthResponse,
  AuditDeliveryOrchestrationStatusResponse,
  TriggerM12AnalyticsBatchResponse,
  TriggerScheduledRuleEvaluationResponse,
  EvaluateRiskScoresRequest,
  EvaluateRiskScoresResponse,
  EvaluateReadinessForecastRequest,
  EvaluateReadinessForecastResponse,
  ReadinessForecastResponse,
  ReadinessForecastSummaryResponse,
  EvaluateControlEffectivenessRequest,
  EvaluateControlEffectivenessResponse,
  ControlEffectivenessRecordResponse,
  ControlEffectivenessSummaryResponse,
  EvaluateMissingEvidenceWarningsRequest,
  EvaluateMissingEvidenceWarningsResponse,
  MissingEvidenceWarningResponse,
  MissingEvidenceWarningSummaryResponse,
  RiskScoreResponse,
  RiskScoreSummaryResponse,
  UpdateRulePackContentRequest,
  UpdateRulePackStatusRequest,
  VocabularyTermResponse,
  VocabularyTypeResponse,
  ComplianceWaiverResponse,
  CreateComplianceWaiverRequest,
  CommitPreviewResponse,
  EvidenceOptionProposalResponse,
  ImportCompletionReportResponse,
  ImportParseResponse,
  ImportSessionResponse,
  ImportUploadResponse,
  ImportValidationResultsResponse,
  MappingCandidateResponse,
  MappingDecisionResponse,
  CreateTheoreticalSituationRequest,
  TheoreticalApplicabilityResultResponse,
  TheoreticalContextFieldResponse,
  TheoreticalEvidenceOptionResponse,
  TheoreticalNextContextResponse,
  TheoreticalOptionResponse,
  TheoreticalSituationContextRequest,
  TheoreticalSituationContextResponse,
  TheoreticalSituationEvaluationResponse,
  TheoreticalSituationFactRequest,
  TheoreticalSituationFactResponse,
  TheoreticalSituationIncidentRequest,
  TheoreticalSituationIncidentResponse,
  TheoreticalSituationListItemResponse,
  TheoreticalSituationResponse,
  WizardItemResponse,
  WizardSummaryResponse,
} from './types'

const apiBase = import.meta.env.VITE_COMPLIANCECORE_API_BASE ?? ''

export class ComplianceCoreApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly body: string,
  ) {
    super(message)
    this.name = 'ComplianceCoreApiError'
  }
}

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new ComplianceCoreApiError(body || `${fallbackMessage} (${response.status})`, response.status, body)
  }

  return (await response.json()) as T
}

export async function redeemHandoff(handoffCode: string): Promise<HandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/auth/nexarr/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  return parseJsonResponse<HandoffSessionResponse>(response, 'Handoff redeem failed')
}

export async function getMe(accessToken: string): Promise<ComplianceCoreMeResponse> {
  const response = await fetch(`${apiBase}/api/me`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ComplianceCoreMeResponse>(response, 'Failed to load profile')
}

export async function getSessionBootstrap(
  accessToken: string,
): Promise<ComplianceCoreSessionBootstrapResponse> {
  const response = await fetch(`${apiBase}/api/session`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ComplianceCoreSessionBootstrapResponse>(
    response,
    'Failed to load session bootstrap',
  )
}

export async function getVocabularyTypes(accessToken: string): Promise<VocabularyTypeResponse[]> {
  const response = await fetch(`${apiBase}/api/vocabulary/types`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<VocabularyTypeResponse[]>(response, 'Failed to load vocabulary types')
}

export async function getVocabularyTerms(
  accessToken: string,
  vocabularyTypeKey?: string,
): Promise<VocabularyTermResponse[]> {
  const query = vocabularyTypeKey ? `?vocabularyTypeKey=${encodeURIComponent(vocabularyTypeKey)}` : ''
  const response = await fetch(`${apiBase}/api/vocabulary${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<VocabularyTermResponse[]>(response, 'Failed to load vocabulary terms')
}

export async function createVocabularyTerm(
  accessToken: string,
  payload: CreateVocabularyTermRequest,
): Promise<VocabularyTermResponse> {
  const response = await fetch(`${apiBase}/api/vocabulary`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<VocabularyTermResponse>(response, 'Failed to create vocabulary term')
}

export async function getComplianceKeys(accessToken: string): Promise<ComplianceKeyResponse[]> {
  const response = await fetch(`${apiBase}/api/compliance-keys`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ComplianceKeyResponse[]>(response, 'Failed to load compliance keys')
}

export async function createComplianceKey(
  accessToken: string,
  payload: CreateComplianceKeyRequest,
): Promise<ComplianceKeyResponse> {
  const response = await fetch(`${apiBase}/api/compliance-keys`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<ComplianceKeyResponse>(response, 'Failed to create compliance key')
}

export async function getMaterialKeys(accessToken: string): Promise<MaterialKeyResponse[]> {
  const response = await fetch(`${apiBase}/api/material-keys`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaterialKeyResponse[]>(response, 'Failed to load material keys')
}

export async function createMaterialKey(
  accessToken: string,
  payload: CreateMaterialKeyRequest,
): Promise<MaterialKeyResponse> {
  const response = await fetch(`${apiBase}/api/material-keys`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<MaterialKeyResponse>(response, 'Failed to create material key')
}

export async function getGoverningBodies(accessToken: string): Promise<GoverningBodyResponse[]> {
  const response = await fetch(`${apiBase}/api/governing-bodies`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<GoverningBodyResponse[]>(response, 'Failed to load governing bodies')
}

export async function createGoverningBody(
  accessToken: string,
  payload: CreateGoverningBodyRequest,
): Promise<GoverningBodyResponse> {
  const response = await fetch(`${apiBase}/api/governing-bodies`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<GoverningBodyResponse>(response, 'Failed to create governing body')
}

export async function getJurisdictions(
  accessToken: string,
  governingBodyId?: string,
): Promise<JurisdictionResponse[]> {
  const query = governingBodyId ? `?governingBodyId=${encodeURIComponent(governingBodyId)}` : ''
  const response = await fetch(`${apiBase}/api/jurisdictions${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<JurisdictionResponse[]>(response, 'Failed to load jurisdictions')
}

export async function createJurisdiction(
  accessToken: string,
  payload: CreateJurisdictionRequest,
): Promise<JurisdictionResponse> {
  const response = await fetch(`${apiBase}/api/jurisdictions`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<JurisdictionResponse>(response, 'Failed to create jurisdiction')
}

export async function getRegulatoryPrograms(
  accessToken: string,
  jurisdictionId?: string,
): Promise<RegulatoryProgramResponse[]> {
  const query = jurisdictionId ? `?jurisdictionId=${encodeURIComponent(jurisdictionId)}` : ''
  const response = await fetch(`${apiBase}/api/regulatory-programs${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RegulatoryProgramResponse[]>(response, 'Failed to load regulatory programs')
}

export async function createRegulatoryProgram(
  accessToken: string,
  payload: CreateRegulatoryProgramRequest,
): Promise<RegulatoryProgramResponse> {
  const response = await fetch(`${apiBase}/api/regulatory-programs`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<RegulatoryProgramResponse>(response, 'Failed to create regulatory program')
}

export async function getRulePacks(
  accessToken: string,
  regulatoryProgramId?: string,
): Promise<RulePackResponse[]> {
  const query = regulatoryProgramId ? `?regulatoryProgramId=${encodeURIComponent(regulatoryProgramId)}` : ''
  const response = await fetch(`${apiBase}/api/rule-packs${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RulePackResponse[]>(response, 'Failed to load rule packs')
}

export async function createRulePack(
  accessToken: string,
  payload: CreateRulePackRequest,
): Promise<RulePackResponse> {
  const response = await fetch(`${apiBase}/api/rule-packs`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<RulePackResponse>(response, 'Failed to create rule pack')
}

export async function updateRulePackStatus(
  accessToken: string,
  rulePackId: string,
  payload: UpdateRulePackStatusRequest,
): Promise<RulePackResponse> {
  const response = await fetch(`${apiBase}/api/rule-packs/${encodeURIComponent(rulePackId)}/status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<RulePackResponse>(response, 'Failed to update rule pack status')
}

export async function getCitations(
  accessToken: string,
  regulatoryProgramId?: string,
  rulePackId?: string,
): Promise<RegulatoryCitationResponse[]> {
  const params = new URLSearchParams()
  if (regulatoryProgramId) {
    params.set('regulatoryProgramId', regulatoryProgramId)
  }
  if (rulePackId) {
    params.set('rulePackId', rulePackId)
  }
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/citations${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RegulatoryCitationResponse[]>(response, 'Failed to load citations')
}

export async function createCitation(
  accessToken: string,
  payload: CreateRegulatoryCitationRequest,
): Promise<RegulatoryCitationResponse> {
  const response = await fetch(`${apiBase}/api/citations`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<RegulatoryCitationResponse>(response, 'Failed to create citation')
}

export async function getFactDefinitions(accessToken: string): Promise<FactDefinitionResponse[]> {
  const response = await fetch(`${apiBase}/api/fact-definitions`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FactDefinitionResponse[]>(response, 'Failed to load fact definitions')
}

export async function createFactDefinition(
  accessToken: string,
  payload: CreateFactDefinitionRequest,
): Promise<FactDefinitionResponse> {
  const response = await fetch(`${apiBase}/api/fact-definitions`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<FactDefinitionResponse>(response, 'Failed to create fact definition')
}

export async function getFactSources(
  accessToken: string,
  factDefinitionId?: string,
): Promise<FactSourceResponse[]> {
  const params = new URLSearchParams()
  if (factDefinitionId) {
    params.set('factDefinitionId', factDefinitionId)
  }
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/fact-sources${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FactSourceResponse[]>(response, 'Failed to load fact sources')
}

export async function createFactSource(
  accessToken: string,
  payload: CreateFactSourceRequest,
): Promise<FactSourceResponse> {
  const response = await fetch(`${apiBase}/api/fact-sources`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<FactSourceResponse>(response, 'Failed to create fact source')
}

export async function getFactRequirements(
  accessToken: string,
  rulePackId?: string,
  citationId?: string,
): Promise<FactRequirementResponse[]> {
  const params = new URLSearchParams()
  if (rulePackId) {
    params.set('rulePackId', rulePackId)
  }
  if (citationId) {
    params.set('citationId', citationId)
  }
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/fact-requirements${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FactRequirementResponse[]>(response, 'Failed to load fact requirements')
}

export async function createFactRequirement(
  accessToken: string,
  payload: CreateFactRequirementRequest,
): Promise<FactRequirementResponse> {
  const response = await fetch(`${apiBase}/api/fact-requirements`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<FactRequirementResponse>(response, 'Failed to create fact requirement')
}

export async function getRegulatoryMappings(
  accessToken: string,
  regulatoryProgramId?: string,
  rulePackId?: string,
  complianceKeyId?: string,
): Promise<RegulatoryMappingResponse[]> {
  const params = new URLSearchParams()
  if (regulatoryProgramId) {
    params.set('regulatoryProgramId', regulatoryProgramId)
  }
  if (rulePackId) {
    params.set('rulePackId', rulePackId)
  }
  if (complianceKeyId) {
    params.set('complianceKeyId', complianceKeyId)
  }
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/regulatory-mappings${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RegulatoryMappingResponse[]>(response, 'Failed to load regulatory mappings')
}

export async function createRegulatoryMapping(
  accessToken: string,
  payload: CreateRegulatoryMappingRequest,
): Promise<RegulatoryMappingResponse> {
  const response = await fetch(`${apiBase}/api/regulatory-mappings`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<RegulatoryMappingResponse>(response, 'Failed to create regulatory mapping')
}

export async function getRulePackContent(
  accessToken: string,
  rulePackId: string,
): Promise<RulePackContentResponse> {
  const response = await fetch(`${apiBase}/api/rule-packs/${encodeURIComponent(rulePackId)}/content`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RulePackContentResponse>(response, 'Failed to load rule pack content')
}

export async function updateRulePackContent(
  accessToken: string,
  rulePackId: string,
  payload: UpdateRulePackContentRequest,
): Promise<RulePackContentResponse> {
  const response = await fetch(`${apiBase}/api/rule-packs/${encodeURIComponent(rulePackId)}/content`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<RulePackContentResponse>(response, 'Failed to update rule pack content')
}

export async function evaluateRulePack(
  accessToken: string,
  rulePackId: string,
  payload: EvaluateRulePackRequest,
): Promise<RuleEvaluationRunResponse> {
  const response = await fetch(`${apiBase}/api/rule-packs/${encodeURIComponent(rulePackId)}/evaluate`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<RuleEvaluationRunResponse>(response, 'Failed to evaluate rule pack')
}

export async function evaluateRulePackBatch(
  accessToken: string,
  payload: EvaluateRulePackBatchRequest,
): Promise<EvaluateRulePackBatchResponse> {
  const response = await fetch(`${apiBase}/api/rule-packs/evaluate/batch`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<EvaluateRulePackBatchResponse>(response, 'Failed to run batch rule evaluation')
}

export async function getRuleEvaluations(
  accessToken: string,
  rulePackId?: string,
): Promise<RuleEvaluationRunResponse[]> {
  const query = rulePackId ? `?rulePackId=${encodeURIComponent(rulePackId)}` : ''
  const response = await fetch(`${apiBase}/api/rule-evaluations${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RuleEvaluationRunResponse[]>(response, 'Failed to load rule evaluations')
}

export async function getFindings(
  accessToken: string,
  rulePackId?: string,
  evaluationRunId?: string,
): Promise<ComplianceFindingResponse[]> {
  const params = new URLSearchParams()
  if (rulePackId) {
    params.set('rulePackId', rulePackId)
  }
  if (evaluationRunId) {
    params.set('evaluationRunId', evaluationRunId)
  }
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/findings${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ComplianceFindingResponse[]>(response, 'Failed to load findings')
}

export async function getWorkflowGates(
  accessToken: string,
): Promise<WorkflowGateDefinitionResponse[]> {
  const response = await fetch(`${apiBase}/api/workflow-gates`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WorkflowGateDefinitionResponse[]>(response, 'Failed to load workflow gates')
}

export async function createWorkflowGate(
  accessToken: string,
  payload: CreateWorkflowGateDefinitionRequest,
): Promise<WorkflowGateDefinitionResponse> {
  const response = await fetch(`${apiBase}/api/workflow-gates`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<WorkflowGateDefinitionResponse>(response, 'Failed to create workflow gate')
}

export async function checkWorkflowGate(
  accessToken: string,
  payload: WorkflowGateCheckRequest,
): Promise<WorkflowGateCheckResponse> {
  const response = await fetch(`${apiBase}/api/workflow-gates/check`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<WorkflowGateCheckResponse>(response, 'Failed to check workflow gate')
}

export async function checkWorkflowGateBatch(
  accessToken: string,
  payload: WorkflowGateBatchCheckRequest,
): Promise<WorkflowGateBatchCheckResponse> {
  const response = await fetch(`${apiBase}/api/workflow-gates/check/batch`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<WorkflowGateBatchCheckResponse>(response, 'Failed to run batch workflow gate check')
}

export async function getCsvBundleManifest(accessToken: string): Promise<CsvBundleManifestResponse> {
  const response = await fetch(`${apiBase}/api/csv-bundle/manifest`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CsvBundleManifestResponse>(response, 'Failed to load CSV bundle manifest')
}

export async function exportCsvBundleZip(accessToken: string): Promise<Blob> {
  const response = await fetch(`${apiBase}/api/csv-bundle/export`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  })
  if (!response.ok) {
    const body = await response.text()
    throw new ComplianceCoreApiError(body || `Export failed (${response.status})`, response.status, body)
  }
  return response.blob()
}

export async function importCsvBundle(
  accessToken: string,
  files: FileList,
  dryRun: boolean,
): Promise<CsvImportResultResponse> {
  const form = new FormData()
  Array.from(files).forEach((file) => form.append('file', file, file.name))
  const response = await fetch(`${apiBase}/api/csv-bundle/import?dryRun=${dryRun}`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${accessToken}` },
    body: form,
  })
  return parseJsonResponse<CsvImportResultResponse>(response, 'Failed to import CSV bundle')
}

export async function createImportSession(
  accessToken: string,
  notes?: string,
): Promise<ImportSessionResponse> {
  const response = await fetch(`${apiBase}/api/v1/import-sessions`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ notes: notes ?? 'Import Wizard session' }),
  })
  return parseJsonResponse<ImportSessionResponse>(response, 'Failed to create import session')
}

export async function uploadImportSessionBundle(
  accessToken: string,
  importSessionId: string,
  files: FileList,
): Promise<ImportUploadResponse> {
  const form = new FormData()
  Array.from(files).forEach((file) => form.append('file', file, file.name))
  const response = await fetch(`${apiBase}/api/v1/import-sessions/${importSessionId}/upload`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${accessToken}` },
    body: form,
  })
  return parseJsonResponse<ImportUploadResponse>(response, 'Failed to upload import bundle')
}

export async function parseImportSession(
  accessToken: string,
  importSessionId: string,
): Promise<ImportParseResponse> {
  const response = await fetch(`${apiBase}/api/v1/import-sessions/${importSessionId}/parse`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ImportParseResponse>(response, 'Failed to parse import session')
}

export async function validateImportSession(
  accessToken: string,
  importSessionId: string,
): Promise<ImportValidationResultsResponse> {
  const response = await fetch(`${apiBase}/api/v1/import-sessions/${importSessionId}/validate`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ImportValidationResultsResponse>(response, 'Failed to validate import session')
}

export async function generateImportMappingCandidates(
  accessToken: string,
  importSessionId: string,
): Promise<MappingCandidateResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/import-sessions/${importSessionId}/generate-mapping-candidates`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MappingCandidateResponse[]>(response, 'Failed to generate mapping candidates')
}

export async function getImportWizardSummary(
  accessToken: string,
  importSessionId: string,
): Promise<WizardSummaryResponse> {
  const response = await fetch(`${apiBase}/api/v1/import-sessions/${importSessionId}/wizard/summary`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WizardSummaryResponse>(response, 'Failed to load wizard summary')
}

export async function getNextImportWizardItem(
  accessToken: string,
  importSessionId: string,
): Promise<WizardItemResponse | null> {
  const response = await fetch(`${apiBase}/api/v1/import-sessions/${importSessionId}/wizard/next`, {
    headers: authHeaders(accessToken),
  })
  if (response.status === 204) {
    return null
  }
  return parseJsonResponse<WizardItemResponse>(response, 'Failed to load next wizard item')
}

export async function getImportWizardItem(
  accessToken: string,
  importSessionId: string,
  itemId: string,
): Promise<WizardItemResponse> {
  const response = await fetch(`${apiBase}/api/v1/import-sessions/${importSessionId}/wizard/items/${itemId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WizardItemResponse>(response, 'Failed to load wizard item')
}

export async function getImportWizardEvidenceOptions(
  accessToken: string,
  importSessionId: string,
  itemId: string,
): Promise<EvidenceOptionProposalResponse[]> {
  const response = await fetch(
    `${apiBase}/api/v1/import-sessions/${importSessionId}/wizard/items/${itemId}/evidence-options`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<EvidenceOptionProposalResponse[]>(response, 'Failed to load evidence options')
}

async function postImportWizardDecision(
  accessToken: string,
  importSessionId: string,
  itemId: string,
  action: string,
  payload?: unknown,
): Promise<MappingDecisionResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/import-sessions/${importSessionId}/wizard/items/${itemId}/${action}`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: payload === undefined ? undefined : JSON.stringify(payload),
    },
  )
  return parseJsonResponse<MappingDecisionResponse>(response, 'Failed to save mapping decision')
}

export function confirmImportWizardItem(
  accessToken: string,
  importSessionId: string,
  itemId: string,
): Promise<MappingDecisionResponse> {
  return postImportWizardDecision(accessToken, importSessionId, itemId, 'confirm')
}

export function selectImportWizardEvidenceOption(
  accessToken: string,
  importSessionId: string,
  itemId: string,
  evidenceOptionKey: string,
): Promise<MappingDecisionResponse> {
  return postImportWizardDecision(accessToken, importSessionId, itemId, 'select-evidence-option', {
    evidenceOptionKey,
  })
}

export function selectImportWizardTarget(
  accessToken: string,
  importSessionId: string,
  itemId: string,
  payload: { targetKind: string; targetId: string; targetKey: string; targetLabel: string },
): Promise<MappingDecisionResponse> {
  return postImportWizardDecision(accessToken, importSessionId, itemId, 'select-target', payload)
}

export function createImportWizardTarget(
  accessToken: string,
  importSessionId: string,
  itemId: string,
  payload: { targetKind: string; payload: Record<string, string> },
): Promise<MappingDecisionResponse> {
  return postImportWizardDecision(accessToken, importSessionId, itemId, 'create-target', payload)
}

export function markImportWizardNoDocumentRequired(
  accessToken: string,
  importSessionId: string,
  itemId: string,
): Promise<MappingDecisionResponse> {
  return postImportWizardDecision(accessToken, importSessionId, itemId, 'mark-no-document-required')
}

export function addImportWizardSupportingEvidence(
  accessToken: string,
  importSessionId: string,
  itemId: string,
  payload: { targetKind: string; targetKey: string; targetLabel: string; payload?: Record<string, string> },
): Promise<MappingDecisionResponse> {
  return postImportWizardDecision(accessToken, importSessionId, itemId, 'add-supporting-evidence', payload)
}

type ExceptionProofPayload = {
  exceptionExemptionKey: string
  targetKind: string
  targetKey: string
  targetLabel: string
  payload?: Record<string, string>
  residualRequirements?: string[]
}

export function mapImportWizardAsNormalEvidence(
  accessToken: string,
  importSessionId: string,
  itemId: string,
): Promise<MappingDecisionResponse> {
  return postImportWizardDecision(accessToken, importSessionId, itemId, 'map-as-normal-evidence')
}

export function mapImportWizardAsExceptionProof(
  accessToken: string,
  importSessionId: string,
  itemId: string,
  payload: ExceptionProofPayload,
): Promise<MappingDecisionResponse> {
  return postImportWizardDecision(accessToken, importSessionId, itemId, 'map-as-exception-proof', payload)
}

export function mapImportWizardAsExemptionProof(
  accessToken: string,
  importSessionId: string,
  itemId: string,
  payload: ExceptionProofPayload,
): Promise<MappingDecisionResponse> {
  return postImportWizardDecision(accessToken, importSessionId, itemId, 'map-as-exemption-proof', payload)
}

export function mapImportWizardAsSpecialPermitApprovalProof(
  accessToken: string,
  importSessionId: string,
  itemId: string,
  payload: ExceptionProofPayload,
): Promise<MappingDecisionResponse> {
  return postImportWizardDecision(accessToken, importSessionId, itemId, 'map-as-special-permit-approval-proof', payload)
}

export function createImportWizardExceptionExemption(
  accessToken: string,
  importSessionId: string,
  itemId: string,
  payload: ExceptionProofPayload,
): Promise<MappingDecisionResponse> {
  return postImportWizardDecision(accessToken, importSessionId, itemId, 'create-exception-exemption', payload)
}

export function selectImportWizardExceptionExemption(
  accessToken: string,
  importSessionId: string,
  itemId: string,
  payload: ExceptionProofPayload,
): Promise<MappingDecisionResponse> {
  return postImportWizardDecision(accessToken, importSessionId, itemId, 'select-exception-exemption', payload)
}

export function markImportWizardExceptionNotApplicable(
  accessToken: string,
  importSessionId: string,
  itemId: string,
): Promise<MappingDecisionResponse> {
  return postImportWizardDecision(accessToken, importSessionId, itemId, 'mark-exception-not-applicable')
}

export function markImportWizardNotApplicable(
  accessToken: string,
  importSessionId: string,
  itemId: string,
): Promise<MappingDecisionResponse> {
  return postImportWizardDecision(accessToken, importSessionId, itemId, 'not-applicable')
}

export function markImportWizardReferenceOnly(
  accessToken: string,
  importSessionId: string,
  itemId: string,
): Promise<MappingDecisionResponse> {
  return postImportWizardDecision(accessToken, importSessionId, itemId, 'reference-only')
}

export function skipImportWizardItem(
  accessToken: string,
  importSessionId: string,
  itemId: string,
): Promise<MappingDecisionResponse> {
  return postImportWizardDecision(accessToken, importSessionId, itemId, 'skip')
}

export function rejectImportWizardItem(
  accessToken: string,
  importSessionId: string,
  itemId: string,
): Promise<MappingDecisionResponse> {
  return postImportWizardDecision(accessToken, importSessionId, itemId, 'reject')
}

export function forceMapImportWizardItem(
  accessToken: string,
  importSessionId: string,
  itemId: string,
  payload: {
    targetKind: string
    targetId: string
    targetKey: string
    targetLabel: string
    overrideReason: string
    riskAcknowledged: boolean
  },
): Promise<MappingDecisionResponse> {
  return postImportWizardDecision(accessToken, importSessionId, itemId, 'force-map', payload)
}

export async function bulkConfirmImportWizardMappings(
  accessToken: string,
  importSessionId: string,
  confidenceBand: string,
  summaryConfirmed = false,
): Promise<MappingDecisionResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/import-sessions/${importSessionId}/wizard/bulk-confirm`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ confidenceBand, summaryConfirmed }),
  })
  return parseJsonResponse<MappingDecisionResponse[]>(response, 'Failed to bulk confirm mappings')
}

export async function getImportCommitPreview(
  accessToken: string,
  importSessionId: string,
): Promise<CommitPreviewResponse> {
  const response = await fetch(`${apiBase}/api/v1/import-sessions/${importSessionId}/wizard/commit-preview`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CommitPreviewResponse>(response, 'Failed to load commit preview')
}

export async function commitImportWizard(
  accessToken: string,
  importSessionId: string,
): Promise<ImportCompletionReportResponse> {
  const response = await fetch(`${apiBase}/api/v1/import-sessions/${importSessionId}/wizard/commit`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ImportCompletionReportResponse>(response, 'Failed to commit import session')
}

function buildAuditPackageQuery(options?: { from?: string; to?: string; format?: string }): string {
  const params = new URLSearchParams()
  if (options?.from) {
    params.set('from', options.from)
  }
  if (options?.to) {
    params.set('to', options.to)
  }
  if (options?.format) {
    params.set('format', options.format)
  }
  const query = params.toString()
  return query ? `?${query}` : ''
}

export async function getAuditPackageManifest(
  accessToken: string,
): Promise<AuditPackageManifestResponse> {
  const response = await fetch(`${apiBase}/api/audit-packages/manifest`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AuditPackageManifestResponse>(response, 'Failed to load audit package manifest')
}

export async function exportAuditPackageZip(
  accessToken: string,
  options?: { from?: string; to?: string },
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/export${buildAuditPackageQuery(options)}`,
    {
      headers: { Authorization: `Bearer ${accessToken}` },
    },
  )
  if (!response.ok) {
    const body = await response.text()
    throw new ComplianceCoreApiError(
      body || `Audit package export failed (${response.status})`,
      response.status,
      body,
    )
  }
  return response.blob()
}

export async function exportAuditPackageJson(
  accessToken: string,
  options?: { from?: string; to?: string },
): Promise<AuditPackageExportResponse> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/export${buildAuditPackageQuery({ ...options, format: 'json' })}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<AuditPackageExportResponse>(response, 'Failed to export audit package JSON')
}

function auditPackageDateBody(from?: string, to?: string): { from?: string; to?: string } {
  const body: { from?: string; to?: string } = {}
  if (from) {
    body.from = `${from}T00:00:00.000Z`
  }
  if (to) {
    body.to = `${to}T23:59:59.999Z`
  }
  return body
}

export async function createAuditPackageGenerationJob(
  accessToken: string,
  options: { format: 'zip' | 'json'; from?: string; to?: string },
): Promise<AuditPackageGenerationJobResponse> {
  const response = await fetch(`${apiBase}/api/audit-packages/jobs`, {
    method: 'POST',
    headers: {
      ...authHeaders(accessToken),
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      format: options.format,
      ...auditPackageDateBody(options.from, options.to),
    }),
  })
  return parseJsonResponse<AuditPackageGenerationJobResponse>(
    response,
    'Failed to queue audit package generation job',
  )
}

export async function getAuditPackageGenerationJob(
  accessToken: string,
  jobId: string,
): Promise<AuditPackageGenerationJobResponse> {
  const response = await fetch(`${apiBase}/api/audit-packages/jobs/${jobId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AuditPackageGenerationJobResponse>(
    response,
    'Failed to load audit package generation job',
  )
}

export async function downloadAuditPackageGenerationJob(
  accessToken: string,
  jobId: string,
): Promise<Blob> {
  const response = await fetch(`${apiBase}/api/audit-packages/jobs/${jobId}/download`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  })
  if (!response.ok) {
    const body = await response.text()
    throw new ComplianceCoreApiError(
      body || `Audit package download failed (${response.status})`,
      response.status,
      body,
    )
  }
  return response.blob()
}

export async function getOperatorDashboard(accessToken: string): Promise<OperatorDashboardResponse> {
  const response = await fetch(`${apiBase}/api/dashboards/operator`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OperatorDashboardResponse>(response, 'Failed to load operator dashboard')
}

export async function listSourceIngestionBatches(
  accessToken: string,
  ingestionType?: string,
): Promise<SourceIngestionBatchSummary[]> {
  const params = new URLSearchParams()
  if (ingestionType) {
    params.set('ingestionType', ingestionType)
  }
  const query = params.toString()
  const response = await fetch(
    `${apiBase}/api/source-ingestion/batches${query ? `?${query}` : ''}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<SourceIngestionBatchSummary[]>(response, 'Failed to list source ingestion batches')
}

export async function getSourceIngestionBatch(
  accessToken: string,
  batchId: string,
): Promise<SourceIngestionBatchDetailResponse> {
  const response = await fetch(`${apiBase}/api/source-ingestion/batches/${batchId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SourceIngestionBatchDetailResponse>(
    response,
    'Failed to load source ingestion batch',
  )
}

export async function validateFactSourceIngestion(
  accessToken: string,
  payload: FactSourceBulkIngestionRequest,
): Promise<SourceIngestionBatchResponse> {
  const response = await fetch(`${apiBase}/api/source-ingestion/fact-sources/validate`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<SourceIngestionBatchResponse>(
    response,
    'Failed to validate fact source ingestion',
  )
}

export async function getRiskScoreSummary(
  accessToken: string,
): Promise<RiskScoreSummaryResponse> {
  const response = await fetch(`${apiBase}/api/risk-scores/summary`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RiskScoreSummaryResponse>(response, 'Failed to load risk score summary')
}

export async function listRiskScores(
  accessToken: string,
  options?: { scopeKey?: string; rulePackKey?: string; runId?: string; limit?: number },
): Promise<RiskScoreResponse[]> {
  const params = new URLSearchParams()
  if (options?.scopeKey) {
    params.set('scopeKey', options.scopeKey)
  }
  if (options?.rulePackKey) {
    params.set('rulePackKey', options.rulePackKey)
  }
  if (options?.runId) {
    params.set('runId', options.runId)
  }
  if (options?.limit) {
    params.set('limit', String(options.limit))
  }
  const query = params.toString()
  const response = await fetch(`${apiBase}/api/risk-scores${query ? `?${query}` : ''}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RiskScoreResponse[]>(response, 'Failed to list risk scores')
}

export async function evaluateRiskScores(
  accessToken: string,
  payload: EvaluateRiskScoresRequest,
): Promise<EvaluateRiskScoresResponse> {
  const response = await fetch(`${apiBase}/api/risk-scores/evaluate`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<EvaluateRiskScoresResponse>(response, 'Failed to evaluate risk scores')
}

export async function getReadinessForecastSummary(
  accessToken: string,
): Promise<ReadinessForecastSummaryResponse> {
  const response = await fetch(`${apiBase}/api/readiness-forecasts/summary`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ReadinessForecastSummaryResponse>(
    response,
    'Failed to load readiness forecast summary',
  )
}

export async function listReadinessForecasts(
  accessToken: string,
  options?: {
    scopeKey?: string
    rulePackKey?: string
    readinessLevel?: string
    runId?: string
    limit?: number
  },
): Promise<ReadinessForecastResponse[]> {
  const params = new URLSearchParams()
  if (options?.scopeKey) {
    params.set('scopeKey', options.scopeKey)
  }
  if (options?.rulePackKey) {
    params.set('rulePackKey', options.rulePackKey)
  }
  if (options?.readinessLevel) {
    params.set('readinessLevel', options.readinessLevel)
  }
  if (options?.runId) {
    params.set('runId', options.runId)
  }
  if (options?.limit) {
    params.set('limit', String(options.limit))
  }
  const query = params.toString()
  const response = await fetch(`${apiBase}/api/readiness-forecasts${query ? `?${query}` : ''}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ReadinessForecastResponse[]>(
    response,
    'Failed to list readiness forecasts',
  )
}

export async function evaluateReadinessForecast(
  accessToken: string,
  payload: EvaluateReadinessForecastRequest,
): Promise<EvaluateReadinessForecastResponse> {
  const response = await fetch(`${apiBase}/api/readiness-forecasts/evaluate`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<EvaluateReadinessForecastResponse>(
    response,
    'Failed to evaluate readiness forecast',
  )
}

export async function getControlEffectivenessSummary(
  accessToken: string,
): Promise<ControlEffectivenessSummaryResponse> {
  const response = await fetch(`${apiBase}/api/control-effectiveness/summary`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ControlEffectivenessSummaryResponse>(
    response,
    'Failed to load control effectiveness summary',
  )
}

export async function listControlEffectivenessRecords(
  accessToken: string,
  options?: {
    scopeKey?: string
    rulePackKey?: string
    effectivenessLevel?: string
    runId?: string
    limit?: number
  },
): Promise<ControlEffectivenessRecordResponse[]> {
  const params = new URLSearchParams()
  if (options?.scopeKey) {
    params.set('scopeKey', options.scopeKey)
  }
  if (options?.rulePackKey) {
    params.set('rulePackKey', options.rulePackKey)
  }
  if (options?.effectivenessLevel) {
    params.set('effectivenessLevel', options.effectivenessLevel)
  }
  if (options?.runId) {
    params.set('runId', options.runId)
  }
  if (options?.limit) {
    params.set('limit', String(options.limit))
  }
  const query = params.toString()
  const response = await fetch(
    `${apiBase}/api/control-effectiveness${query ? `?${query}` : ''}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<ControlEffectivenessRecordResponse[]>(
    response,
    'Failed to list control effectiveness records',
  )
}

export async function evaluateControlEffectiveness(
  accessToken: string,
  payload: EvaluateControlEffectivenessRequest,
): Promise<EvaluateControlEffectivenessResponse> {
  const response = await fetch(`${apiBase}/api/control-effectiveness/evaluate`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<EvaluateControlEffectivenessResponse>(
    response,
    'Failed to evaluate control effectiveness',
  )
}

export async function getMissingEvidenceWarningSummary(
  accessToken: string,
): Promise<MissingEvidenceWarningSummaryResponse> {
  const response = await fetch(`${apiBase}/api/missing-evidence-warnings/summary`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MissingEvidenceWarningSummaryResponse>(
    response,
    'Failed to load missing evidence warning summary',
  )
}

export async function listMissingEvidenceWarnings(
  accessToken: string,
  options?: {
    scopeKey?: string
    rulePackKey?: string
    severity?: string
    runId?: string
    limit?: number
  },
): Promise<MissingEvidenceWarningResponse[]> {
  const params = new URLSearchParams()
  if (options?.scopeKey) {
    params.set('scopeKey', options.scopeKey)
  }
  if (options?.rulePackKey) {
    params.set('rulePackKey', options.rulePackKey)
  }
  if (options?.severity) {
    params.set('severity', options.severity)
  }
  if (options?.runId) {
    params.set('runId', options.runId)
  }
  if (options?.limit) {
    params.set('limit', String(options.limit))
  }
  const query = params.toString()
  const response = await fetch(
    `${apiBase}/api/missing-evidence-warnings${query ? `?${query}` : ''}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<MissingEvidenceWarningResponse[]>(
    response,
    'Failed to list missing evidence warnings',
  )
}

export async function evaluateMissingEvidenceWarnings(
  accessToken: string,
  payload: EvaluateMissingEvidenceWarningsRequest,
): Promise<EvaluateMissingEvidenceWarningsResponse> {
  const response = await fetch(`${apiBase}/api/missing-evidence-warnings/evaluate`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<EvaluateMissingEvidenceWarningsResponse>(
    response,
    'Failed to evaluate missing evidence warnings',
  )
}

export async function getRuleChangeSummary(
  accessToken: string,
): Promise<RuleChangeMonitoringSummaryResponse> {
  const response = await fetch(`${apiBase}/api/rule-changes/summary`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RuleChangeMonitoringSummaryResponse>(
    response,
    'Failed to load rule change summary',
  )
}

export async function listRuleChangeEvents(
  accessToken: string,
  options?: { packKey?: string; changeType?: string; since?: string; limit?: number },
): Promise<RuleChangeEventResponse[]> {
  const params = new URLSearchParams()
  if (options?.packKey) {
    params.set('packKey', options.packKey)
  }
  if (options?.changeType) {
    params.set('changeType', options.changeType)
  }
  if (options?.since) {
    params.set('since', options.since)
  }
  if (options?.limit) {
    params.set('limit', String(options.limit))
  }
  const query = params.toString()
  const response = await fetch(
    `${apiBase}/api/rule-changes/events${query ? `?${query}` : ''}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<RuleChangeEventResponse[]>(response, 'Failed to list rule change events')
}

export async function commitFactSourceIngestion(
  accessToken: string,
  payload: FactSourceBulkIngestionRequest,
): Promise<SourceIngestionBatchResponse> {
  const response = await fetch(`${apiBase}/api/source-ingestion/fact-sources/commit`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<SourceIngestionBatchResponse>(
    response,
    'Failed to commit fact source ingestion',
  )
}

export async function getFactSourceSyncWorkerSettings(
  accessToken: string,
): Promise<FactSourceSyncWorkerSettingsResponse> {
  const response = await fetch(`${apiBase}/api/fact-source-sync-worker-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FactSourceSyncWorkerSettingsResponse>(
    response,
    'Failed to load fact source sync worker settings',
  )
}

export async function upsertFactSourceSyncWorkerSettings(
  accessToken: string,
  payload: UpsertFactSourceSyncWorkerSettingsRequest,
): Promise<FactSourceSyncWorkerSettingsResponse> {
  const response = await fetch(`${apiBase}/api/fact-source-sync-worker-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<FactSourceSyncWorkerSettingsResponse>(
    response,
    'Failed to save fact source sync worker settings',
  )
}

export async function getFactSourceSyncHealth(
  accessToken: string,
): Promise<FactSourceSyncHealthResponse> {
  const response = await fetch(`${apiBase}/api/fact-source-sync-health`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FactSourceSyncHealthResponse>(
    response,
    'Failed to load fact source sync health',
  )
}

export async function getM12AnalyticsWorkerSettings(
  accessToken: string,
): Promise<M12AnalyticsWorkerSettingsResponse> {
  const response = await fetch(`${apiBase}/api/m12-analytics-worker-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<M12AnalyticsWorkerSettingsResponse>(
    response,
    'Failed to load M12 analytics worker settings',
  )
}

export async function upsertM12AnalyticsWorkerSettings(
  accessToken: string,
  payload: UpsertM12AnalyticsWorkerSettingsRequest,
): Promise<M12AnalyticsWorkerSettingsResponse> {
  const response = await fetch(`${apiBase}/api/m12-analytics-worker-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<M12AnalyticsWorkerSettingsResponse>(
    response,
    'Failed to save M12 analytics worker settings',
  )
}

export async function getAuditDeliveryOrchestrationStatus(
  accessToken: string,
): Promise<AuditDeliveryOrchestrationStatusResponse> {
  const response = await fetch(`${apiBase}/api/audit-delivery-orchestration`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AuditDeliveryOrchestrationStatusResponse>(
    response,
    'Failed to load audit delivery orchestration status',
  )
}

export async function triggerScheduledRuleEvaluation(
  accessToken: string,
): Promise<TriggerScheduledRuleEvaluationResponse> {
  const response = await fetch(
    `${apiBase}/api/audit-delivery-orchestration/trigger-scheduled-evaluation`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<TriggerScheduledRuleEvaluationResponse>(
    response,
    'Failed to trigger scheduled rule evaluation',
  )
}

export async function triggerM12AnalyticsBatch(
  accessToken: string,
): Promise<TriggerM12AnalyticsBatchResponse> {
  const response = await fetch(`${apiBase}/api/audit-delivery-orchestration/trigger-m12-batch`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TriggerM12AnalyticsBatchResponse>(
    response,
    'Failed to trigger M12 analytics batch',
  )
}

export async function getFindingsReportSummary(
  accessToken: string,
  params: { status?: string; severity?: string; openOnly?: boolean } = {},
): Promise<FindingsReportSummaryResponse> {
  const search = new URLSearchParams()
  if (params.status) search.set('status', params.status)
  if (params.severity) search.set('severity', params.severity)
  if (params.openOnly) search.set('openOnly', 'true')
  const query = search.toString()
  const response = await fetch(
    `${apiBase}/api/reports/findings/summary${query ? `?${query}` : ''}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<FindingsReportSummaryResponse>(
    response,
    'Failed to load findings report summary',
  )
}

export async function exportFindingsReportSummaryCsv(
  accessToken: string,
  params: { status?: string; severity?: string; openOnly?: boolean } = {},
): Promise<Blob> {
  const search = new URLSearchParams()
  if (params.status) search.set('status', params.status)
  if (params.severity) search.set('severity', params.severity)
  if (params.openOnly) search.set('openOnly', 'true')
  const query = search.toString()
  const response = await fetch(
    `${apiBase}/api/reports/findings/summary/export${query ? `?${query}` : ''}`,
    { headers: { Authorization: `Bearer ${accessToken}` } },
  )
  if (!response.ok) {
    const body = await response.text()
    throw new ComplianceCoreApiError(body || 'Failed to export findings report', response.status, body)
  }
  return response.blob()
}

export async function getOperatorReportSummary(
  accessToken: string,
  params: { attentionOnly?: boolean } = {},
): Promise<OperatorReportSummaryResponse> {
  const search = new URLSearchParams()
  if (params.attentionOnly) search.set('attentionOnly', 'true')
  const query = search.toString()
  const response = await fetch(
    `${apiBase}/api/reports/operator/summary${query ? `?${query}` : ''}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<OperatorReportSummaryResponse>(
    response,
    'Failed to load operator report summary',
  )
}

export async function exportOperatorReportSummaryCsv(
  accessToken: string,
  params: { attentionOnly?: boolean } = {},
): Promise<Blob> {
  const search = new URLSearchParams()
  if (params.attentionOnly) search.set('attentionOnly', 'true')
  const query = search.toString()
  const response = await fetch(
    `${apiBase}/api/reports/operator/summary/export${query ? `?${query}` : ''}`,
    { headers: { Authorization: `Bearer ${accessToken}` } },
  )
  if (!response.ok) {
    const body = await response.text()
    throw new ComplianceCoreApiError(body || 'Failed to export operator report', response.status, body)
  }
  return response.blob()
}

export async function getEntityExportManifest(
  accessToken: string,
): Promise<EntityExportManifestResponse> {
  const response = await fetch(`${apiBase}/api/exports/manifest`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<EntityExportManifestResponse>(
    response,
    'Failed to load export manifest',
  )
}

export async function exportBulkFindingsCsv(
  accessToken: string,
  params: { status?: string; openOnly?: boolean } = {},
): Promise<Blob> {
  const search = new URLSearchParams()
  if (params.status) search.set('status', params.status)
  if (params.openOnly) search.set('openOnly', 'true')
  const query = search.toString()
  const response = await fetch(`${apiBase}/api/exports/findings${query ? `?${query}` : ''}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  })
  if (!response.ok) {
    const body = await response.text()
    throw new ComplianceCoreApiError(body || 'Failed to export findings CSV', response.status, body)
  }
  return response.blob()
}

export async function exportBulkEvaluationsCsv(accessToken: string): Promise<Blob> {
  const response = await fetch(`${apiBase}/api/exports/evaluations`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  })
  if (!response.ok) {
    const body = await response.text()
    throw new ComplianceCoreApiError(body || 'Failed to export evaluations CSV', response.status, body)
  }
  return response.blob()
}

export async function exportBulkWorkflowGateChecksCsv(accessToken: string): Promise<Blob> {
  const response = await fetch(`${apiBase}/api/exports/workflow-gate-checks`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  })
  if (!response.ok) {
    const body = await response.text()
    throw new ComplianceCoreApiError(
      body || 'Failed to export workflow gate checks CSV',
      response.status,
      body,
    )
  }
  return response.blob()
}

export async function exportBulkRulePacksCsv(
  accessToken: string,
  params: { status?: string } = {},
): Promise<Blob> {
  const search = new URLSearchParams()
  if (params.status) search.set('status', params.status)
  const query = search.toString()
  const response = await fetch(`${apiBase}/api/exports/rule-packs${query ? `?${query}` : ''}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  })
  if (!response.ok) {
    const body = await response.text()
    throw new ComplianceCoreApiError(body || 'Failed to export rule packs CSV', response.status, body)
  }
  return response.blob()
}

export async function listSdsReferences(
  accessToken: string,
  includeInactive = false,
): Promise<SdsReferenceResponse[]> {
  const query = includeInactive ? '?includeInactive=true' : ''
  const response = await fetch(`${apiBase}/api/sds${query}`, { headers: authHeaders(accessToken) })
  return parseJsonResponse<SdsReferenceResponse[]>(response, 'Failed to load SDS references')
}

export async function createSdsReference(
  accessToken: string,
  request: CreateSdsReferenceRequest,
): Promise<SdsReferenceResponse> {
  const response = await fetch(`${apiBase}/api/sds`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<SdsReferenceResponse>(response, 'Failed to create SDS reference')
}

export async function listHazComReferences(
  accessToken: string,
  includeInactive = false,
): Promise<HazComReferenceResponse[]> {
  const query = includeInactive ? '?includeInactive=true' : ''
  const response = await fetch(`${apiBase}/api/hazcom${query}`, { headers: authHeaders(accessToken) })
  return parseJsonResponse<HazComReferenceResponse[]>(response, 'Failed to load HazCom references')
}

export async function createHazComReference(
  accessToken: string,
  request: CreateHazComReferenceRequest,
): Promise<HazComReferenceResponse> {
  const response = await fetch(`${apiBase}/api/hazcom`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<HazComReferenceResponse>(response, 'Failed to create HazCom reference')
}

export async function listRuleVersions(
  accessToken: string,
  packKey?: string,
): Promise<RuleVersionListResponse> {
  const query = packKey ? `?packKey=${encodeURIComponent(packKey)}` : ''
  const response = await fetch(`${apiBase}/api/rule-versions${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RuleVersionListResponse>(response, 'Failed to load rule versions')
}

export async function publishRuleVersion(
  accessToken: string,
  rulePackId: string,
): Promise<RuleVersionResponse> {
  const response = await fetch(`${apiBase}/api/rule-versions/${rulePackId}/publish`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RuleVersionResponse>(response, 'Failed to publish rule version')
}

export async function rollbackRuleVersion(
  accessToken: string,
  rulePackId: string,
): Promise<RuleVersionRollbackResponse> {
  const response = await fetch(`${apiBase}/api/rule-versions/${rulePackId}/rollback`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RuleVersionRollbackResponse>(response, 'Failed to roll back rule version')
}

export async function listComplianceWaivers(
  accessToken: string,
  params?: { status?: string; packKey?: string; scopeKey?: string; limit?: number },
): Promise<ComplianceWaiverResponse[]> {
  const search = new URLSearchParams()
  if (params?.status) search.set('status', params.status)
  if (params?.packKey) search.set('packKey', params.packKey)
  if (params?.scopeKey) search.set('scopeKey', params.scopeKey)
  if (params?.limit) search.set('limit', String(params.limit))
  const query = search.toString() ? `?${search.toString()}` : ''
  const response = await fetch(`${apiBase}/api/waivers${query}`, { headers: authHeaders(accessToken) })
  return parseJsonResponse<ComplianceWaiverResponse[]>(response, 'Failed to load compliance waivers')
}

export async function createComplianceWaiver(
  accessToken: string,
  request: CreateComplianceWaiverRequest,
): Promise<ComplianceWaiverResponse> {
  const response = await fetch(`${apiBase}/api/waivers`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<ComplianceWaiverResponse>(response, 'Failed to create compliance waiver')
}

export async function approveComplianceWaiver(
  accessToken: string,
  waiverId: string,
): Promise<ComplianceWaiverResponse> {
  const response = await fetch(`${apiBase}/api/waivers/${waiverId}/approve`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ComplianceWaiverResponse>(response, 'Failed to approve compliance waiver')
}

export async function revokeComplianceWaiver(
  accessToken: string,
  waiverId: string,
): Promise<ComplianceWaiverResponse> {
  const response = await fetch(`${apiBase}/api/waivers/${waiverId}/revoke`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ComplianceWaiverResponse>(response, 'Failed to revoke compliance waiver')
}

export async function getTheoreticalSituationKinds(
  accessToken: string,
): Promise<TheoreticalOptionResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/theoretical-situations/options/situation-kinds`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TheoreticalOptionResponse[]>(response, 'Failed to load situation kinds')
}

export async function getTheoreticalContextFields(
  accessToken: string,
  situationKind?: string,
): Promise<TheoreticalContextFieldResponse[]> {
  const query = situationKind ? `?situationKind=${encodeURIComponent(situationKind)}` : ''
  const response = await fetch(
    `${apiBase}/api/v1/theoretical-situations/options/context-fields${query}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<TheoreticalContextFieldResponse[]>(
    response,
    'Failed to load context fields',
  )
}

export async function getTheoreticalEvidenceStates(
  accessToken: string,
): Promise<TheoreticalOptionResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/theoretical-situations/options/evidence-states`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TheoreticalOptionResponse[]>(response, 'Failed to load evidence states')
}

export async function getTheoreticalMaterialClasses(
  accessToken: string,
): Promise<TheoreticalOptionResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/theoretical-situations/options/material-classes`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TheoreticalOptionResponse[]>(response, 'Failed to load material classes')
}

export async function getTheoreticalIncidentOptions(
  accessToken: string,
): Promise<TheoreticalOptionResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/theoretical-situations/options/incidents`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TheoreticalOptionResponse[]>(response, 'Failed to load incident options')
}

export async function getTheoreticalEvidenceOptions(
  accessToken: string,
  requirementKey?: string,
): Promise<TheoreticalEvidenceOptionResponse[]> {
  const query = requirementKey ? `?requirementKey=${encodeURIComponent(requirementKey)}` : ''
  const response = await fetch(
    `${apiBase}/api/v1/theoretical-situations/options/evidence-options${query}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<TheoreticalEvidenceOptionResponse[]>(
    response,
    'Failed to load theoretical evidence options',
  )
}

export async function createTheoreticalSituation(
  accessToken: string,
  request: CreateTheoreticalSituationRequest,
): Promise<TheoreticalSituationResponse> {
  const response = await fetch(`${apiBase}/api/v1/theoretical-situations`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<TheoreticalSituationResponse>(
    response,
    'Failed to create theoretical situation',
  )
}

export async function listTheoreticalSituations(
  accessToken: string,
): Promise<TheoreticalSituationListItemResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/theoretical-situations`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TheoreticalSituationListItemResponse[]>(
    response,
    'Failed to list theoretical situations',
  )
}

export async function getTheoreticalSituation(
  accessToken: string,
  situationId: string,
): Promise<TheoreticalSituationResponse> {
  const response = await fetch(`${apiBase}/api/v1/theoretical-situations/${situationId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TheoreticalSituationResponse>(
    response,
    'Failed to load theoretical situation',
  )
}

export async function getTheoreticalNextContext(
  accessToken: string,
  situationId: string,
): Promise<TheoreticalNextContextResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/theoretical-situations/${situationId}/options/next-context`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<TheoreticalNextContextResponse>(
    response,
    'Failed to load next context',
  )
}

export async function setTheoreticalSituationContext(
  accessToken: string,
  situationId: string,
  request: TheoreticalSituationContextRequest,
): Promise<TheoreticalSituationContextResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/theoretical-situations/${situationId}/context`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<TheoreticalSituationContextResponse[]>(
    response,
    'Failed to save theoretical situation context',
  )
}

export async function setTheoreticalSituationFacts(
  accessToken: string,
  situationId: string,
  request: TheoreticalSituationFactRequest,
): Promise<TheoreticalSituationFactResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/theoretical-situations/${situationId}/facts`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<TheoreticalSituationFactResponse[]>(
    response,
    'Failed to save theoretical situation facts',
  )
}

export async function setTheoreticalSituationIncidents(
  accessToken: string,
  situationId: string,
  request: TheoreticalSituationIncidentRequest,
): Promise<TheoreticalSituationIncidentResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/theoretical-situations/${situationId}/incidents`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<TheoreticalSituationIncidentResponse[]>(
    response,
    'Failed to save theoretical situation incidents',
  )
}

export async function resolveTheoreticalApplicability(
  accessToken: string,
  situationId: string,
): Promise<TheoreticalApplicabilityResultResponse[]> {
  const response = await fetch(
    `${apiBase}/api/v1/theoretical-situations/${situationId}/resolve-applicability`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<TheoreticalApplicabilityResultResponse[]>(
    response,
    'Failed to resolve theoretical applicability',
  )
}

export async function evaluateTheoreticalSituation(
  accessToken: string,
  situationId: string,
  includePossible = false,
): Promise<TheoreticalSituationEvaluationResponse> {
  const response = await fetch(`${apiBase}/api/v1/theoretical-situations/${situationId}/evaluate`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ includePossible }),
  })
  return parseJsonResponse<TheoreticalSituationEvaluationResponse>(
    response,
    'Failed to evaluate theoretical situation',
  )
}

export async function saveTheoreticalSituationTemplate(
  accessToken: string,
  situationId: string,
): Promise<TheoreticalSituationResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/theoretical-situations/${situationId}/save-template`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<TheoreticalSituationResponse>(
    response,
    'Failed to save theoretical situation template',
  )
}

export async function duplicateTheoreticalSituationFromTemplate(
  accessToken: string,
  templateId: string,
): Promise<TheoreticalSituationResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/theoretical-situations/from-template/${templateId}`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<TheoreticalSituationResponse>(
    response,
    'Failed to duplicate theoretical situation template',
  )
}
