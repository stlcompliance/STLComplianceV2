export interface HandoffSessionResponse {
  accessToken: string
  accessTokenExpiresAt: string
  userId: string
  personId: string
  email: string
  displayName: string
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  entitlements: string[]
}

export interface ComplianceCoreMeResponse {
  userId: string
  personId: string
  email: string
  displayName: string
  tenantId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasComplianceCoreEntitlement: boolean
  entitlements: string[]
}

export interface VocabularyTypeResponse {
  typeKey: string
  label: string
  description: string
  sortOrder: number
  isActive: boolean
}

export interface VocabularyTermResponse {
  termId: string
  termKey: string
  label: string
  vocabularyTypeKey: string
  description: string
  isActive: boolean
  aliases: string[]
  createdAt: string
}

export interface CreateVocabularyTermRequest {
  termKey: string
  label: string
  vocabularyTypeKey: string
  description: string
}

export interface ComplianceKeyResponse {
  complianceKeyId: string
  key: string
  label: string
  category: string
  description: string
  isActive: boolean
  createdAt: string
}

export interface CreateComplianceKeyRequest {
  key: string
  label: string
  category: string
  description: string
}

export interface MaterialKeyResponse {
  materialKeyId: string
  key: string
  label: string
  category: string
  description: string
  isActive: boolean
  createdAt: string
}

export interface CreateMaterialKeyRequest {
  key: string
  label: string
  category: string
  description: string
}

export interface GoverningBodyResponse {
  governingBodyId: string
  bodyKey: string
  label: string
  description: string
  isActive: boolean
  createdAt: string
}

export interface CreateGoverningBodyRequest {
  bodyKey: string
  label: string
  description: string
}

export interface JurisdictionResponse {
  jurisdictionId: string
  governingBodyId: string
  governingBodyKey: string
  governingBodyLabel: string
  jurisdictionKey: string
  label: string
  description: string
  isActive: boolean
  createdAt: string
}

export interface CreateJurisdictionRequest {
  governingBodyId: string
  jurisdictionKey: string
  label: string
  description: string
}

export interface RegulatoryProgramResponse {
  regulatoryProgramId: string
  jurisdictionId: string
  jurisdictionKey: string
  jurisdictionLabel: string
  programKey: string
  label: string
  description: string
  isActive: boolean
  createdAt: string
}

export interface CreateRegulatoryProgramRequest {
  jurisdictionId: string
  programKey: string
  label: string
  description: string
}

export interface RulePackResponse {
  rulePackId: string
  regulatoryProgramId: string
  regulatoryProgramKey: string
  regulatoryProgramLabel: string
  packKey: string
  label: string
  description: string
  versionNumber: number
  status: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface CreateRulePackRequest {
  regulatoryProgramId: string
  packKey: string
  label: string
  description: string
}

export interface UpdateRulePackStatusRequest {
  status: string
}

export interface RegulatoryCitationResponse {
  citationId: string
  regulatoryProgramId: string
  regulatoryProgramKey: string
  regulatoryProgramLabel: string
  rulePackId: string | null
  rulePackKey: string | null
  rulePackLabel: string | null
  citationKey: string
  label: string
  sourceReference: string
  description: string
  versionNumber: number
  supersedesCitationId: string | null
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface CreateRegulatoryCitationRequest {
  regulatoryProgramId: string
  rulePackId?: string | null
  citationKey: string
  label: string
  sourceReference: string
  description: string
  supersedesCitationId?: string | null
}

export interface FactDefinitionResponse {
  factDefinitionId: string
  factKey: string
  label: string
  description: string
  valueType: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface CreateFactDefinitionRequest {
  factKey: string
  label: string
  description: string
  valueType: string
}

export interface FactSourceResponse {
  factSourceId: string
  factDefinitionId: string
  factKey: string
  factLabel: string
  sourceKey: string
  sourceType: string
  label: string
  description: string
  productKey: string | null
  productReference: string | null
  configJson: string
  priority: number
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface CreateFactSourceRequest {
  factDefinitionId: string
  sourceKey: string
  sourceType: string
  label: string
  description: string
  productKey?: string | null
  productReference?: string | null
  configJson: string
  priority: number
}

export interface FactRequirementResponse {
  factRequirementId: string
  factDefinitionId: string
  factKey: string
  factLabel: string
  rulePackId: string | null
  rulePackKey: string | null
  citationId: string | null
  citationKey: string | null
  requirementKey: string
  label: string
  description: string
  isRequired: boolean
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface CreateFactRequirementRequest {
  factDefinitionId: string
  rulePackId?: string | null
  citationId?: string | null
  requirementKey: string
  label: string
  description: string
  isRequired: boolean
}

export interface RegulatoryMappingResponse {
  regulatoryMappingId: string
  mappingKey: string
  label: string
  description: string
  targetKind: string
  regulatoryProgramId: string
  regulatoryProgramKey: string
  regulatoryProgramLabel: string
  rulePackId: string | null
  rulePackKey: string | null
  rulePackLabel: string | null
  citationId: string | null
  citationKey: string | null
  factDefinitionId: string | null
  factKey: string | null
  complianceKeyId: string | null
  complianceKey: string | null
  materialKeyId: string | null
  materialKey: string | null
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface CreateRegulatoryMappingRequest {
  mappingKey: string
  label: string
  description: string
  targetKind: string
  regulatoryProgramId: string
  rulePackId?: string | null
  citationId?: string | null
  factDefinitionId?: string | null
  complianceKeyId?: string | null
  materialKeyId?: string | null
}

export interface RuleDefinitionDto {
  ruleKey: string
  label: string
  type: string
  factKey: string
  expectedValue: boolean
}

export interface RulePackContentBody {
  schemaVersion: number
  logic: string
  rules: RuleDefinitionDto[]
}

export interface RulePackContentResponse {
  rulePackId: string
  packKey: string
  versionNumber: number
  status: string
  hasContent: boolean
  content: RulePackContentBody | null
  updatedAt: string
}

export interface UpdateRulePackContentRequest {
  content: RulePackContentBody
}

export interface EvaluateRulePackRequest {
  facts: Record<string, boolean>
  emitFindings?: boolean
}

export interface EvaluateRulePackBatchItem {
  rulePackKey: string
  facts?: Record<string, boolean>
}

export interface EvaluateRulePackBatchRequest {
  items: EvaluateRulePackBatchItem[]
  facts?: Record<string, boolean>
  emitFindings?: boolean
}

export interface EvaluateRulePackBatchSummary {
  total: number
  allowCount: number
  warnCount: number
  blockCount: number
}

export interface EvaluateRulePackBatchResultItem {
  rulePackKey: string
  rulePackId: string
  packLabel: string
  outcome: string
  reasonCode: string
  message: string
  overallResult: string
  evaluationRunId: string | null
  ruleResults: RuleEvaluationItemResponse[]
  findingsEmitted: ComplianceFindingResponse[]
}

export interface EvaluateRulePackBatchResponse {
  batchId: string
  results: EvaluateRulePackBatchResultItem[]
  summary: EvaluateRulePackBatchSummary
}

export interface RuleEvaluationItemResponse {
  ruleKey: string
  label: string
  result: string
  message: string
}

export interface RuleEvaluationRunResponse {
  evaluationRunId: string
  rulePackId: string
  packKey: string
  packLabel: string
  versionNumber: number
  status: string
  overallResult: string
  factInputs: Record<string, boolean>
  ruleResults: RuleEvaluationItemResponse[]
  createdAt: string
  findingsEmitted?: ComplianceFindingResponse[]
}

export interface ComplianceFindingResponse {
  findingId: string
  rulePackId: string
  packKey: string
  ruleEvaluationRunId: string | null
  findingKey: string
  severity: string
  status: string
  ruleKey: string | null
  factKey: string | null
  title: string
  message: string
  reasonCode: string
  createdAt: string
}

export interface WorkflowGateDefinitionResponse {
  workflowGateId: string
  gateKey: string
  label: string
  description: string
  rulePackId: string
  packKey: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface CreateWorkflowGateDefinitionRequest {
  gateKey: string
  label: string
  description: string
  rulePackId: string
}

export interface WorkflowGateCheckRequest {
  gateKey: string
  facts?: Record<string, boolean>
  context?: Record<string, string>
  emitFindings?: boolean
}

export interface WorkflowGateBatchCheckItem {
  gateKey: string
  facts?: Record<string, boolean>
  context?: Record<string, string>
}

export interface WorkflowGateBatchCheckRequest {
  items: WorkflowGateBatchCheckItem[]
  facts?: Record<string, boolean>
  context?: Record<string, string>
  emitFindings?: boolean
}

export interface WorkflowGateBatchCheckSummary {
  total: number
  allowCount: number
  warnCount: number
  blockCount: number
}

export interface WorkflowGateBatchCheckResponse {
  batchId: string
  results: WorkflowGateCheckResponse[]
  summary: WorkflowGateBatchCheckSummary
}

export interface WorkflowGateReasonResponse {
  code: string
  message: string
  ruleKey: string | null
  factKey: string | null
}

export interface CsvBundleFileDescriptor {
  fileName: string
  headers: string[]
}

export interface AuditPackageSectionDescriptor {
  key: string
  fileName: string
  label: string
  description: string
}

export interface AuditPackageManifestResponse {
  packageVersion: string
  sections: AuditPackageSectionDescriptor[]
}

export interface AuditPackageCountsResponse {
  auditEvents: number
  findings: number
  evaluationRuns: number
  rulePacks: number
}

export interface AuditPackageExportResponse {
  packageId: string
  tenantId: string
  generatedAt: string
  dateRange: { from: string | null; to: string | null } | null
  counts: AuditPackageCountsResponse
  auditEvents: unknown[]
  findings: unknown[]
  evaluationRuns: unknown[]
  rulePacks: unknown[]
}

export interface CsvBundleManifestResponse {
  files: CsvBundleFileDescriptor[]
}

export interface CsvImportFileSummary {
  fileName: string
  rowCount: number
  created: number
  updated: number
  deactivated: number
}

export interface CsvImportIssue {
  fileName: string
  lineNumber: number | null
  code: string
  message: string
}

export interface CsvImportResultResponse {
  dryRun: boolean
  applied: boolean
  files: CsvImportFileSummary[]
  issues: CsvImportIssue[]
}

export interface WorkflowGateCheckResponse {
  checkResultId: string
  gateKey: string
  gateLabel: string
  rulePackId: string
  packKey: string
  outcome: string
  reasonCode: string
  message: string
  ruleEvaluationRunId: string | null
  reasons: WorkflowGateReasonResponse[]
  findingsEmitted: ComplianceFindingResponse[]
  checkedAt: string
}

export interface OperatorDashboardFindingsSummary {
  openCount: number
  openBlockSeverityCount: number
  openWarnSeverityCount: number
  acknowledgedCount: number
  resolvedCount: number
  totalCount: number
}

export interface OperatorDashboardRulePackSummary {
  draftCount: number
  reviewCount: number
  publishedCount: number
  archivedCount: number
  totalCount: number
}

export interface OperatorDashboardEvaluationsSummary {
  totalCount: number
  last24HoursCount: number
  passCount: number
  failCount: number
}

export interface OperatorDashboardWorkflowGateSummary {
  definitionCount: number
  checkResultsTotal: number
  checkResultsLast24Hours: number
  blockOutcomeCount: number
  warnOutcomeCount: number
  allowOutcomeCount: number
}

export interface OperatorDashboardAuditSummary {
  totalCount: number
  last24HoursCount: number
  successCount: number
  failureCount: number
}

export interface OperatorDashboardRecentEvaluation {
  evaluationRunId: string
  rulePackId: string
  rulePackLabel: string
  packKey: string
  overallResult: string
  createdAt: string
}

export interface OperatorDashboardResponse {
  findings: OperatorDashboardFindingsSummary
  rulePacks: OperatorDashboardRulePackSummary
  evaluations: OperatorDashboardEvaluationsSummary
  workflowGates: OperatorDashboardWorkflowGateSummary
  auditEvents: OperatorDashboardAuditSummary
  recentEvaluations: OperatorDashboardRecentEvaluation[]
  generatedAt: string
}
