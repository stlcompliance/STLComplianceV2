import type {
  ComplianceCoreMeResponse,
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
  AuditPackageManifestResponse,
  CsvBundleManifestResponse,
  CsvImportResultResponse,
  WorkflowGateBatchCheckRequest,
  WorkflowGateBatchCheckResponse,
  WorkflowGateCheckRequest,
  WorkflowGateCheckResponse,
  WorkflowGateDefinitionResponse,
  OperatorDashboardResponse,
  UpdateRulePackContentRequest,
  UpdateRulePackStatusRequest,
  VocabularyTermResponse,
  VocabularyTypeResponse,
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
  const response = await fetch(`${apiBase}/api/auth/handoff/redeem`, {
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

export async function getOperatorDashboard(accessToken: string): Promise<OperatorDashboardResponse> {
  const response = await fetch(`${apiBase}/api/dashboards/operator`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OperatorDashboardResponse>(response, 'Failed to load operator dashboard')
}
