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
  launchableProductKeys: string[]
  themePreference?: string | null
  callbackUrl: string | null
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
  launchableProductKeys: string[]
  canManageVocabulary: boolean
  canExportAuditPackage: boolean
  canEvaluateRiskScores: boolean
  canEvaluateMissingEvidenceWarnings: boolean
  canEvaluateControlEffectiveness: boolean
  canEvaluateReadinessForecast: boolean
  canReadReports: boolean
  canExportReports: boolean
}

export interface ComplianceCoreSessionBootstrapResponse {
  userId: string
  personId: string
  tenantId: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  launchableProductKeys: string[]
  canManageVocabulary: boolean
  canExportAuditPackage: boolean
  canEvaluateRiskScores: boolean
  canEvaluateMissingEvidenceWarnings: boolean
  canEvaluateControlEffectiveness: boolean
  canEvaluateReadinessForecast: boolean
  canReadReports: boolean
  canExportReports: boolean
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

export interface UpdateFactSourceRequest {
  label: string
  description: string
  productKey?: string | null
  productReference?: string | null
  configJson: string
  priority: number
  isActive: boolean
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
  /** When true, compliance waivers cannot override failures for this rule. */
  nonWaivable?: boolean
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
  nonWaivable?: boolean
  remediationRequired?: boolean
  reviewRequired?: boolean
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
  waivedCount: number
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
  workflowGateChecks: number
  waivers: number
  rulePacks: number
}

export interface AuditPackageGenerationJobResponse {
  jobId: string
  status: string
  format: string
  from: string | null
  to: string | null
  packageId: string | null
  errorMessage: string | null
  createdAt: string
  startedAt: string | null
  completedAt: string | null
  downloadReady: boolean
}

export interface AuditPackageExportResponse {
  packageId: string
  tenantId: string
  generatedAt: string
  dateRange: { from: string | null; to: string | null } | null
  counts: AuditPackageCountsResponse
  auditEvents: AuditEventExportItem[]
  findings: AuditPackageFindingItem[]
  evaluationRuns: AuditPackageEvaluationRunItem[]
  workflowGateChecks: AuditPackageWorkflowGateCheckItem[]
  waivers: AuditPackageWaiverItem[]
  rulePacks: AuditPackageRulePackItem[]
}

export interface AuditEventExportItem {
  auditEventId: string
  actorUserId: string | null
  action: string
  targetType: string
  targetId: string | null
  result: string
  reasonCode: string | null
  correlationId: string
  occurredAt: string
}

export interface AuditPackageFindingItem {
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

export interface AuditPackageEvaluationRunItem {
  evaluationRunId: string
  rulePackId: string
  packKey: string
  actorUserId: string | null
  status: string
  overallResult: string
  factInputsJson: string
  ruleResultsJson: string
  appliedWaiverId: string | null
  appliedWaiverKey: string | null
  createdAt: string
}

export interface AuditPackageWorkflowGateCheckItem {
  checkResultId: string
  gateKey: string
  rulePackId: string
  packKey: string
  ruleEvaluationRunId: string | null
  outcome: string
  reasonCode: string
  message: string
  appliedWaiverId: string | null
  appliedWaiverKey: string | null
  checkedAt: string
}

export interface AuditPackageWaiverItem {
  waiverId: string
  waiverKey: string
  rulePackId: string
  packKey: string
  ruleKey: string | null
  gateKey: string | null
  subjectScopeKey: string
  reasonCode: string
  explanation: string
  status: string
  effectiveAt: string
  expiresAt: string | null
  approvedByUserId: string | null
  approvedAt: string | null
  createdAt: string
}

export interface AuditPackageRulePackItem {
  rulePackId: string
  packKey: string
  label: string
  description: string
  versionNumber: number
  status: string
  isActive: boolean
  regulatoryProgramId: string
  programKey: string
  hasRuleContent: boolean
  createdAt: string
  updatedAt: string
}

export interface RuleEvaluationAuditExportResponse {
  exportId: string
  tenantId: string
  generatedAt: string
  evaluationRun: AuditPackageEvaluationRunItem
  workflowGateChecks: AuditPackageWorkflowGateCheckItem[]
  findings: AuditPackageFindingItem[]
  waivers: AuditPackageWaiverItem[]
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

export interface RulePackImportRunResponse {
  importId: string
  status: string
  dryRun: boolean
  createdAt: string
  result: CsvImportResultResponse
}

export interface RulePackImportDiffResponse {
  importId: string
  filesWithChanges: number
  createdCount: number
  updatedCount: number
  deactivatedCount: number
  issueCount: number
}

export interface RulePackImportTestResultsResponse {
  importId: string
  passed: boolean
  issueCount: number
  issues: CsvImportIssue[]
}

export interface RulePackImportRollbackResponse {
  importId: string
  rolledBack: boolean
  status: string
}

export interface RuleTestCaseResponse {
  ruleTestCaseId: string
  rulePackId: string
  rulePackKey: string
  rulePackVersion: number
  rulePackStatus: string
  ruleId: string
  ruleKey: string
  testKey: string
  label: string
  description: string
  expectedResult: string
  facts: Record<string, boolean>
  createdAt: string
  updatedAt: string
}

export interface CreateRuleTestCaseRequest {
  ruleKey: string
  testKey: string
  label: string
  description: string
  facts: Record<string, boolean>
  expectedResult?: string
}

export interface PatchRuleTestCaseRequest {
  ruleKey?: string | null
  testKey?: string | null
  label?: string | null
  description?: string | null
  facts?: Record<string, boolean> | null
  expectedResult?: string | null
}

export interface RuleTestCaseRunResponse {
  ruleTestCaseId: string
  ruleId: string
  expectedResult: string
  actualResult: string
  passed: boolean
  message: string
  evaluation: RuleEvaluationItemResponse
  evaluatedAt: string
}

export interface CsvImportResolutionOptions {
  regulatorySpineMode: string
  governingBodyKey?: string
  governingBodyLabel?: string
  governingBodyDescription?: string
  jurisdictionKey?: string
  jurisdictionLabel?: string
  jurisdictionDescription?: string
  programMappings?: Record<string, string>
}

export interface ImportSessionResponse {
  importSessionId: string
  tenantId: string
  uploadedByPersonId: string | null
  sourceFilename: string
  sourceHash: string
  importType: string
  status: string
  validationStatus: string
  mappingStatus: string
  commitStatus: string
  createdAt: string
  validatedAt: string | null
  mappedAt: string | null
  committedAt: string | null
  rejectedAt: string | null
  notes: string
}

export interface ImportSessionSourceFileResponse {
  sourceFileId: string
  sourceFile: string
  originalFilename: string
  fileHash: string
  byteLength: number
  validationStatus: string
  validationErrors: string[]
}

export interface ImportUploadResponse {
  session: ImportSessionResponse
  files: ImportSessionSourceFileResponse[]
}

export interface ImportStagedFileSummaryResponse {
  sourceFile: string
  rowCount: number
  validationStatus: string
  validationErrors: string[]
}

export interface ImportParseResponse {
  session: ImportSessionResponse
  files: ImportStagedFileSummaryResponse[]
}

export interface ImportStagedRowResultResponse {
  stagedRowId: string
  sourceFile: string
  rowNumber: number
  canonicalKeyCandidate: string
  validationStatus: string
  validationErrors: string[]
}

export interface ImportValidationResultsResponse {
  importSessionId: string
  validationStatus: string
  totalRows: number
  validRows: number
  invalidRows: number
  files: ImportSessionSourceFileResponse[]
  rows: ImportStagedRowResultResponse[]
}

export interface MappingCandidateResponse {
  mappingCandidateId: string
  stagedRowId: string
  stagedSourceFile: string
  stagedRowNumber: number
  sourceKey: string
  sourceLabel: string
  evidenceOptionId: string | null
  evidenceOptionKey: string
  evidenceOptionLabel: string
  optionLogicGroup: string
  targetKind: string
  targetId: string
  targetKey: string
  targetLabel: string
  confidenceScore: number
  confidenceBand: string
  matchReasons: string[]
  riskFlags: string[]
  proposedAction: string
  satisfiesRequirementIfConfirmed: boolean
  requiresAdditionalSupportingEvidence: boolean
  requiresConfirmation: boolean
}

export interface EvidenceOptionProposalResponse {
  evidenceOptionId: string
  evidenceOptionKey: string
  evidenceOptionLabel: string
  logicType: string
  evidenceKind: string
  targetKind: string
  sourceProduct: string
  sourceEntity: string
  sourceFieldOrRecordType: string
  documentTypeKey: string
  materialKey: string
  partKey: string
  systemKey: string
  assetKind: string
  externalRegistryKey: string
  factKey: string
  required: boolean
  priority: number
  confidenceHint: number | null
}

export interface WizardSummaryResponse {
  importSessionId: string
  sessionStatus: string
  mappingStatus: string
  totalItems: number
  pendingItems: number
  confirmedItems: number
  changedItems: number
  skippedItems: number
  rejectedItems: number
  blockedItems: number
  exactNoRiskItems: number
  highNoRiskItems: number
  riskFlaggedItems: number
}

export interface WizardItemResponse {
  itemId: string
  stagedRowId: string
  status: string
  requirementKey: string
  evidenceKey: string
  label: string
  auditQuestion: string
  citationKey: string
  rulePackKey: string
  complianceKeyOrDomain: string
  requiredEvidenceKind: string
  evidenceLogic: string
  suggestedEvidencePath: EvidenceOptionProposalResponse
  otherAcceptableEvidencePaths: EvidenceOptionProposalResponse[]
  sourceProduct: string
  sourceEntity: string
  sourceFieldOrRecordType: string
  suggestedTarget: string
  targetKind: string
  confidenceScore: number
  confidenceBand: string
  matchReasons: string[]
  riskFlags: string[]
  confirmationPrompt: string
  whatWillHappenIfConfirmed: string
  overrideAllowed: boolean
  remediationRequired: boolean
  exceptionProofPrompt: string
  sourceRow: Record<string, string>
  targetRecord: Record<string, string>
}

export interface MappingDecisionResponse {
  mappingDecisionId: string
  importSessionId: string
  stagedRowId: string
  mappingCandidateId: string | null
  decision: string
  selectedEvidenceOptionId: string | null
  selectedEvidenceOptionKey: string
  selectedTargetKind: string
  selectedTargetId: string
  selectedTargetKey: string
  evidenceMappingPurpose: string
  exceptionExemptionKey: string
  residualRequirements: string[]
  overrideUsed: boolean
  overrideReason: string
  decidedByPersonId: string
  decidedAt: string
}

export interface CommitPreviewActionResponse {
  action: string
  sourceKey: string
  targetKind: string
  targetKey: string
  summary: string
  evidenceMappingPurpose: string
  exceptionExemptionKey: string
  residualRequirements: string[]
  overrideUsed: boolean
}

export interface CommitPreviewResponse {
  importSessionId: string
  totalDecisions: number
  existingDocumentsMapped: number
  newDocumentsToCreate: number
  existingMaterialsMapped: number
  newMaterialsToCreate: number
  existingPartsMapped: number
  newPartsToCreate: number
  existingSystemsOrAssetsMapped: number
  newSystemsAssetsOrReferencesToCreate: number
  factDefinitionsToCreateOrUpdate: number
  factRequirementsToCreateOrUpdate: number
  evidenceOptionGroupsToCreateOrUpdate: number
  evidenceOptionsToCreateOrUpdate: number
  evidenceReferencesToCreateOrUpdate: number
  exceptionProofMappings: number
  exceptionExemptionRecordsToCreateOrUpdate: number
  overridesUsed: number
  skippedRows: number
  rejectedRows: number
  unresolvedBlockers: string[]
  actions: CommitPreviewActionResponse[]
}

export interface ImportCompletionReportResponse {
  importSessionId: string
  status: string
  createdCount: number
  updatedCount: number
  skippedCount: number
  rejectedCount: number
  overrideCount: number
  evidenceMappingsCreated: number
  newDocumentsMaterialsPartsSystemsCreated: number
  existingDocumentsMaterialsPartsSystemsMapped: number
  warnings: string[]
  errors: string[]
  auditLogReference: string
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
  appliedWaiverId?: string | null
  appliedWaiverKey?: string | null
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

export interface FactSourceIngestionRowRequest {
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

export interface FactSourceBulkIngestionRequest {
  sources: FactSourceIngestionRowRequest[]
}

export interface SourceIngestionJobResult {
  rowIndex: number
  jobKey: string
  status: string
  entityType: string | null
  entityId: string | null
  errorCode: string | null
  message: string | null
}

export interface SourceIngestionBatchResponse {
  batchId: string
  ingestionType: string
  phase: string
  dryRun: boolean
  totalJobs: number
  successCount: number
  errorCount: number
  skippedCount: number
  status: string
  jobs: SourceIngestionJobResult[]
}

export interface SourceIngestionBatchSummary {
  batchId: string
  ingestionType: string
  phase: string
  dryRun: boolean
  status: string
  totalJobs: number
  successCount: number
  errorCount: number
  skippedCount: number
  sourceProduct: string | null
  publicationId: string | null
  createdAt: string
  completedAt: string | null
}

export interface SourceIngestionBatchDetailResponse extends SourceIngestionBatchSummary {
  jobs: SourceIngestionJobResult[]
}

export interface RuleChangeEventResponse {
  eventId: string
  rulePackId: string
  packKey: string
  programKey: string
  changeType: string
  summary: string
  fromStatus: string | null
  toStatus: string | null
  fromVersion: number | null
  toVersion: number | null
  previousContentHash: string | null
  newContentHash: string | null
  source: string
  actorUserId: string | null
  scanRunId: string | null
  detectedAt: string
}

export interface EvaluateRiskScoresRequest {
  scopeKey?: string | null
  rulePackKey?: string | null
  context?: Record<string, string> | null
}

export interface RiskScoreResponse {
  riskScoreId: string
  runId: string
  scopeKey: string
  rulePackId: string
  packKey: string
  riskScore: number
  riskLevel: string
  ruleOutcome: string
  evaluationResult: string
  unresolvedFactCount: number
  failedRuleCount: number
  resolvedFactCount: number
  mirrorFactCount: number
  summary: string
  evaluatedAt: string
}

export interface EvaluateRiskScoresResponse {
  runId: string
  scopeKey: string
  packsEvaluatedCount: number
  highestRiskScore: number
  highestRiskLevel: string
  mirrorFactCount: number
  evaluatedAt: string
  scores: RiskScoreResponse[]
}

export interface RiskScoreSummaryResponse {
  totalScores: number
  scopesTracked: number
  lowCount: number
  mediumCount: number
  highCount: number
  criticalCount: number
  highestRiskScore: number
  highestRiskLevel: string
  lastEvaluatedAt: string | null
  generatedAt: string
}

export interface EvaluateMissingEvidenceWarningsRequest {
  scopeKey?: string | null
  rulePackKey?: string | null
  context?: Record<string, string> | null
}

export interface MissingEvidenceWarningResponse {
  warningId: string
  runId: string
  scopeKey: string
  rulePackId: string
  packKey: string
  factKey: string
  factDefinitionId: string | null
  warningType: string
  severity: string
  reasonCode: string
  hasMirrorAtScope: boolean
  isRequiredInRule: boolean
  isRequiredInCatalog: boolean
  summary: string
  evaluatedAt: string
}

export interface EvaluateMissingEvidenceWarningsResponse {
  runId: string
  scopeKey: string
  packsAnalyzedCount: number
  warningsEmittedCount: number
  highestSeverity: string
  mirrorFactCount: number
  evaluatedAt: string
  warnings: MissingEvidenceWarningResponse[]
}

export interface MissingEvidenceWarningSummaryResponse {
  totalWarnings: number
  scopesTracked: number
  lowCount: number
  mediumCount: number
  highCount: number
  criticalCount: number
  highestSeverity: string
  lastEvaluatedAt: string | null
  generatedAt: string
}

export interface EvidenceCompletenessReportItem {
  rulePackId: string
  packKey: string
  scopeKey: string
  totalWarnings: number
  criticalWarningCount: number
  highWarningCount: number
  mediumWarningCount: number
  lowWarningCount: number
  completenessScore: number
  completenessLevel: string
  latestWarningAt: string | null
  summary: string
}

export interface EvidenceCompletenessReportSummaryResponse {
  tenantId: string
  totalRulePacks: number
  completeRulePackCount: number
  partialRulePackCount: number
  incompleteRulePackCount: number
  totalWarnings: number
  criticalWarningCount: number
  highWarningCount: number
  mediumWarningCount: number
  lowWarningCount: number
  completenessScore: number
  generatedAt: string
  rulePacks: EvidenceCompletenessReportItem[]
}

export interface CitationReviewReportItem {
  citationId: string
  citationKey: string
  sourceReference: string
  programKey: string
  programLabel: string
  citationLabel: string
  versionNumber: number
  reviewState: string
  isActive: boolean
  hasRulePack: boolean
  rulePackKey: string | null
  rulePackLabel: string | null
  factRequirementCount: number
  mappingCount: number
  supersededByCount: number
  supersedesCitationKey: string | null
  updatedAt: string
  summary: string
}

export interface CitationReviewReportSummaryResponse {
  tenantId: string
  totalCitations: number
  activeCitationCount: number
  reviewedCitationCount: number
  needsReviewCitationCount: number
  inactiveCitationCount: number
  supersededCitationCount: number
  linkedRulePackCount: number
  totalFactRequirementCount: number
  totalMappingCount: number
  generatedAt: string
  citations: CitationReviewReportItem[]
}

export interface EvaluateControlEffectivenessRequest {
  scopeKey?: string | null
  rulePackKey?: string | null
  context?: Record<string, string> | null
}

export interface ControlEffectivenessRecordResponse {
  recordId: string
  runId: string
  scopeKey: string
  rulePackId: string
  packKey: string
  effectivenessScore: number
  effectivenessLevel: string
  controlStatus: string
  ruleOutcome: string
  evaluationResult: string
  totalRuleCount: number
  passedRuleCount: number
  failedRuleCount: number
  unresolvedFactCount: number
  resolvedFactCount: number
  summary: string
  evaluatedAt: string
}

export interface EvaluateControlEffectivenessResponse {
  runId: string
  scopeKey: string
  packsEvaluatedCount: number
  lowestEffectivenessScore: number
  lowestEffectivenessLevel: string
  averageEffectivenessScore: number
  evaluatedAt: string
  records: ControlEffectivenessRecordResponse[]
}

export interface ControlEffectivenessSummaryResponse {
  totalControls: number
  scopesTracked: number
  effectiveCount: number
  partiallyEffectiveCount: number
  ineffectiveCount: number
  unknownCount: number
  lowestEffectivenessScore: number
  lowestEffectivenessLevel: string
  averageEffectivenessScore: number
  lastEvaluatedAt: string | null
  generatedAt: string
}

export interface EvaluateReadinessForecastRequest {
  scopeKey?: string | null
  rulePackKey?: string | null
  context?: Record<string, string> | null
}

export interface ReadinessForecastResponse {
  forecastId: string
  runId: string
  scopeKey: string
  rulePackId: string
  packKey: string
  readinessScore: number
  readinessLevel: string
  riskScore: number
  riskLevel: string
  effectivenessScore: number
  effectivenessLevel: string
  missingEvidenceWarningCount: number
  highestMissingEvidenceSeverity: string
  summary: string
  forecastedAt: string
}

export interface EvaluateReadinessForecastResponse {
  runId: string
  scopeKey: string
  packsForecastCount: number
  readinessScore: number
  readinessLevel: string
  lowestReadinessScore: number
  averageReadinessScore: number
  highestRiskScore: number
  missingEvidenceWarningCount: number
  averageEffectivenessScore: number
  riskScoreRunId: string
  missingEvidenceWarningRunId: string
  controlEffectivenessRunId: string
  forecastedAt: string
  forecasts: ReadinessForecastResponse[]
}

export interface ReadinessForecastSummaryResponse {
  totalForecasts: number
  scopesTracked: number
  readyCount: number
  cautionCount: number
  notReadyCount: number
  unknownCount: number
  readinessScore: number
  readinessLevel: string
  lowestReadinessScore: number
  averageReadinessScore: number
  lastForecastedAt: string | null
  generatedAt: string
}

export interface AuditReadinessReportSummaryResponse extends ReadinessForecastSummaryResponse {
  forecasts: ReadinessForecastResponse[]
}

export interface RemediationQueueItemResponse {
  warningId: string
  runId: string
  rulePackId: string
  packKey: string
  factKey: string
  warningType: string
  severity: string
  reasonCode: string
  queueState: string
  recommendedAction: string
  hasMirrorAtScope: boolean
  isRequiredInRule: boolean
  isRequiredInCatalog: boolean
  summary: string
  evaluatedAt: string
}

export interface RemediationQueueReportSummaryResponse {
  totalWarnings: number
  queuedCount: number
  criticalCount: number
  highCount: number
  mediumCount: number
  lowCount: number
  lastEvaluatedAt: string | null
  generatedAt: string
  queueItems: RemediationQueueItemResponse[]
}

export interface RuleChangeMonitoringSummaryResponse {
  totalEvents: number
  eventsLast24Hours: number
  eventsLast7Days: number
  versionCreatedCount: number
  statusChangedCount: number
  contentUpdatedCount: number
  scanDetectedCount: number
  generatedAt: string
}

export interface RuleChangeImpactReportItem {
  rulePackId: string
  packKey: string
  programKey: string
  latestChangeType: string
  latestSummary: string
  changeEventCount: number
  versionCreatedCount: number
  statusChangedCount: number
  contentUpdatedCount: number
  evaluationRunCount: number
  findingCount: number
  waiverCount: number
  latestChangedAt: string
}

export interface RuleChangeImpactReportResponse {
  tenantId: string
  totalImpactedRulePacks: number
  totalChangeEvents: number
  totalEvaluationRuns: number
  totalFindings: number
  totalWaivers: number
  generatedAt: string
  rulePacks: RuleChangeImpactReportItem[]
}

export interface M12AnalyticsWorkerSettingsResponse {
  isEnabled: boolean
  defaultScopeKey: string
  intervalHours: number
  riskScoringEnabled: boolean
  missingEvidenceEnabled: boolean
  controlEffectivenessEnabled: boolean
  readinessForecastEnabled: boolean
  auditDeliveryEnabled: boolean
  lastBatchRunAt: string | null
  lastRiskScoringRunAt: string | null
  lastMissingEvidenceRunAt: string | null
  lastControlEffectivenessRunAt: string | null
  lastReadinessForecastRunAt: string | null
  lastAuditDeliveryRunAt: string | null
  updatedAt: string | null
}

export interface UpsertM12AnalyticsWorkerSettingsRequest {
  isEnabled: boolean
  defaultScopeKey?: string
  intervalHours?: number
  riskScoringEnabled?: boolean
  missingEvidenceEnabled?: boolean
  controlEffectivenessEnabled?: boolean
  readinessForecastEnabled?: boolean
  auditDeliveryEnabled?: boolean
}

export interface FactSourceSyncWorkerSettingsResponse {
  isEnabled: boolean
  defaultScopeKey: string
  intervalMinutes: number
  lastBatchRunAt: string | null
  updatedAt: string | null
}

export interface UpsertFactSourceSyncWorkerSettingsRequest {
  isEnabled: boolean
  defaultScopeKey?: string
  intervalMinutes?: number
}

export interface FactSourceSyncHealthItem {
  factSourceId: string
  sourceKey: string
  factKey: string
  sourceType: string
  productKey: string | null
  scopeKey: string
  healthStatus: string
  lastAttemptAt: string | null
  lastSuccessAt: string | null
  lastFailureAt: string | null
  lastErrorMessage: string | null
  consecutiveFailureCount: number
}

export interface FactSourceSyncHealthResponse {
  tenantId: string
  workerEnabled: boolean
  intervalMinutes: number
  lastBatchRunAt: string | null
  productApiSourceCount: number
  healthyCount: number
  staleCount: number
  failedCount: number
  pendingCount: number
  sources: FactSourceSyncHealthItem[]
}

export interface ProductIntegrationHealthReportSummaryResponse extends FactSourceSyncHealthResponse {}

export interface ScheduledRuleEvaluationRunSummary {
  runId: string
  startedAt: string
  completedAt: string | null
  status: string
  packsDueCount: number
  evaluatedCount: number
  skippedCount: number
  allowCount: number
  warnCount: number
  blockCount: number
}

export interface M12AnalyticsBatchRunSummary {
  runId: string
  startedAt: string
  completedAt: string | null
  status: string
  scopeKey: string
  riskScoringRan: boolean
  missingEvidenceRan: boolean
  controlEffectivenessRan: boolean
  readinessForecastRan: boolean
  auditDeliveryQueued: boolean
  auditPackageJobId: string | null
  errorMessage: string | null
}

export interface AuditPackageJobSummary {
  jobId: string
  status: string
  format: string
  createdAt: string
  completedAt: string | null
  packageId: string | null
  errorMessage: string | null
}

export interface PendingM12AnalyticsBatchTenantItem {
  tenantId: string
  defaultScopeKey: string
  intervalHours: number
  riskScoringDue: boolean
  missingEvidenceDue: boolean
  controlEffectivenessDue: boolean
  readinessForecastDue: boolean
  auditDeliveryDue: boolean
}

export interface AuditDeliveryScheduledEvaluationStatus {
  pendingPacksCount: number
  lastRun: ScheduledRuleEvaluationRunSummary | null
}

export interface AuditDeliveryM12BatchStatus {
  workerEnabled: boolean
  batchDue: boolean
  pendingSteps: PendingM12AnalyticsBatchTenantItem | null
  lastRun: M12AnalyticsBatchRunSummary | null
}

export interface AuditDeliveryAuditPackageStatus {
  pendingJobsCount: number
  recentJobs: AuditPackageJobSummary[]
}

export interface AuditDeliveryOrchestrationStatusResponse {
  workerSettings: M12AnalyticsWorkerSettingsResponse
  scheduledEvaluation: AuditDeliveryScheduledEvaluationStatus
  m12Batch: AuditDeliveryM12BatchStatus
  auditPackages: AuditDeliveryAuditPackageStatus
}

export interface TriggerScheduledRuleEvaluationResponse {
  scheduledRunId: string
  evaluatedCount: number
  skippedCount: number
  allowCount: number
  warnCount: number
  blockCount: number
}

export interface TriggerM12AnalyticsBatchResponse {
  batchRunId: string | null
  status: string
  auditDeliveryQueued: boolean
  auditPackageJobId: string | null
  errorMessage: string | null
}

export interface FindingsReportSummaryItem {
  findingId: string
  findingKey: string
  severity: string
  status: string
  title: string
  packKey: string
  createdAt: string
}

export interface FindingsReportSummaryResponse {
  totalFindings: number
  openCount: number
  acknowledgedCount: number
  resolvedCount: number
  openBlockSeverityCount: number
  openWarnSeverityCount: number
  recentFindings: FindingsReportSummaryItem[]
}

export interface OperatorReportSummaryItem {
  evaluationRunId: string
  rulePackLabel: string
  packKey: string
  overallResult: string
  createdAt: string
}

export interface OperatorReportSummaryResponse {
  evaluationTotalCount: number
  evaluationPassCount: number
  evaluationFailCount: number
  evaluationsLast24Hours: number
  workflowGateDefinitionCount: number
  workflowGateBlockCount: number
  workflowGateWarnCount: number
  rulePackPublishedCount: number
  rulePackDraftCount: number
  attentionItemCount: number
  recentEvaluations: OperatorReportSummaryItem[]
}

export interface WaiverReportSummaryItem {
  waiverId: string
  waiverKey: string
  packKey: string
  subjectScopeKey: string
  status: string
  reasonCode: string
  effectiveAt: string
  expiresAt: string | null
  updatedAt: string
}

export interface WaiverReportSummaryResponse {
  totalWaivers: number
  pendingCount: number
  approvedCount: number
  rejectedCount: number
  revokedCount: number
  expiredCount: number
  expiringSoonCount: number
  recentWaivers: WaiverReportSummaryItem[]
}

export interface ExceptionExemptionReportSummaryItem {
  exceptionExemptionId: string
  key: string
  label: string
  type: string
  effectType: string
  packKey: string
  citationKey: string | null
  activeState: string
  effectiveAt: string | null
  expiresAt: string | null
  updatedAt: string
}

export interface ExceptionExemptionReportSummaryResponse {
  totalExceptionExemptions: number
  activeCount: number
  inactiveCount: number
  waiverTypeCount: number
  varianceTypeCount: number
  specialPermitTypeCount: number
  expiringSoonCount: number
  recentExceptionExemptions: ExceptionExemptionReportSummaryItem[]
}

export interface EntityExportFormatDescriptor {
  formatKey: string
  contentType: string
  fileNameTemplate: string
  description: string
}

export interface EntityExportManifestEntity {
  entityKey: string
  exportPath: string
  displayName: string
  csvHeader: string
  description: string
  formats: EntityExportFormatDescriptor[]
}

export interface EntityExportManifestResponse {
  packageVersion: string
  entities: EntityExportManifestEntity[]
  reportExports: Array<{
    reportKey: string
    exportPath: string
    displayName: string
    description: string
  }>
  auditPackageFormats: string[]
}

export interface SdsReferenceResponse {
  sdsReferenceId: string
  sdsKey: string
  materialKeyId: string | null
  materialKey: string | null
  productName: string
  manufacturer: string
  documentUrl: string
  revisionDate: string | null
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface CreateSdsReferenceRequest {
  sdsKey: string
  materialKeyId: string | null
  productName: string
  manufacturer: string
  documentUrl: string
  revisionDate: string | null
}

export interface HazComReferenceResponse {
  hazComReferenceId: string
  hazComKey: string
  title: string
  description: string
  linkedSdsKey: string | null
  locationRef: string
  documentUrl: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface CreateHazComReferenceRequest {
  hazComKey: string
  title: string
  description: string
  linkedSdsKey: string | null
  locationRef: string
  documentUrl: string
}

export interface RuleVersionResponse {
  rulePackId: string
  packKey: string
  programKey: string
  programLabel: string
  versionNumber: number
  status: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface RuleVersionListResponse {
  items: RuleVersionResponse[]
}

export interface RuleVersionRollbackResponse {
  archivedVersion: RuleVersionResponse
  restoredVersion: RuleVersionResponse
}

export interface ComplianceWaiverResponse {
  waiverId: string
  waiverKey: string
  rulePackId: string
  packKey: string
  ruleKey: string | null
  gateKey: string | null
  subjectScopeKey: string
  reasonCode: string
  explanation: string
  status: string
  effectiveAt: string
  expiresAt: string | null
  createdByUserId: string | null
  approvedByUserId: string | null
  approvedAt: string | null
  revokedByUserId: string | null
  revokedAt: string | null
  createdAt: string
  updatedAt: string
}

export interface ComplianceExceptionExemptionResponse {
  exceptionExemptionId: string
  tenantId: string
  key: string
  label: string
  type: string
  governingBody: string
  programKey: string
  packKey: string
  citationKey: string
  applicabilityKey: string
  appliesToSubjectKind: string
  appliesToSourceProduct: string
  appliesToSourceEntity: string
  effectType: string
  conditionLogicJson: string
  requiredEvidenceOptionGroupId: string | null
  issuingAuthority: string
  authorizationNumber: string
  effectiveAt: string | null
  expiresAt: string | null
  active: boolean
  description: string
  createdAt: string
  updatedAt: string
}

export interface CreateComplianceExceptionExemptionRequest {
  key: string
  label: string
  type: string
  effectType: string
  governingBody?: string | null
  programKey?: string | null
  packKey?: string | null
  citationKey?: string | null
  applicabilityKey?: string | null
  appliesToSubjectKind?: string | null
  appliesToSourceProduct?: string | null
  appliesToSourceEntity?: string | null
  conditionLogicJson?: string | null
  requiredEvidenceOptionGroupId?: string | null
  issuingAuthority?: string | null
  authorizationNumber?: string | null
  effectiveAt?: string | null
  expiresAt?: string | null
  active?: boolean
  description?: string | null
}

export interface UpdateComplianceExceptionExemptionRequest {
  label?: string | null
  type?: string | null
  effectType?: string | null
  governingBody?: string | null
  programKey?: string | null
  packKey?: string | null
  citationKey?: string | null
  applicabilityKey?: string | null
  appliesToSubjectKind?: string | null
  appliesToSourceProduct?: string | null
  appliesToSourceEntity?: string | null
  conditionLogicJson?: string | null
  requiredEvidenceOptionGroupId?: string | null
  issuingAuthority?: string | null
  authorizationNumber?: string | null
  effectiveAt?: string | null
  expiresAt?: string | null
  active?: boolean | null
  description?: string | null
}

export interface CreateComplianceWaiverRequest {
  waiverKey: string
  rulePackId: string
  subjectScopeKey: string
  reasonCode: string
  explanation: string
  effectiveAt: string
  expiresAt?: string | null
  ruleKey?: string | null
  gateKey?: string | null
}

export interface RenewComplianceWaiverRequest {
  effectiveAt: string
  expiresAt?: string | null
  notes?: string | null
}

export interface TheoreticalOptionResponse {
  key: string
  label: string
  description: string
  category: string
  edgeCase: boolean
}

export interface TheoreticalContextFieldResponse {
  contextKey: string
  label: string
  controlType: string
  controlledVocabularyType: string
  required: boolean
  situationKinds: string[]
  values: TheoreticalOptionResponse[]
}

export interface TheoreticalSituationContextResponse {
  contextId: string
  contextKey: string
  contextLabel: string
  contextValueKey: string
  contextValueLabel: string
  controlledVocabularyType: string
  confidence: number
  createdAt: string
}

export interface TheoreticalSituationFactResponse {
  situationFactId: string
  factKey: string
  requirementKey: string
  citationKey: string
  packKey: string
  simulatedValue: string
  valueType: string
  simulatedState: string
  evidenceOptionKey: string
  evidenceKind: string
  targetKind: string
  active: boolean
  createdAt: string
}

export interface TheoreticalSituationIncidentResponse {
  situationIncidentId: string
  incidentTypeKey: string
  severityKey: string
  involvedSubjectKind: string
  involvedSubjectState: string
  triggerKey: string
  triggerValue: string
  reportabilityState: string
  remediationState: string
  createdAt: string
}

export interface TheoreticalSituationEvaluationDetailResponse {
  detailId: string
  requirementKey: string
  factKey: string
  citationKey: string
  packKey: string
  auditQuestion: string
  simulatedState: string
  expectedValue: string
  actualValue: string
  operator: string
  result: string
  failureSeverity: string
  automaticFailureFlag: boolean
  overrideAllowed: boolean
  overridePermission: string
  remediationRequired: boolean
  normalRuleResult: string
  exceptionExemptionKey: string
  exceptionExemptionType: string
  exceptionExemptionLabel: string
  exceptionExemptionConsidered: boolean
  exceptionExemptionApplies: boolean
  exceptionExemptionProofRequired: boolean
  exceptionExemptionProofValid: boolean
  resultBeforeException: string
  resultAfterException: string
  finalComplianceResult: string
  explanation: string
  suggestedNextAction: string
  visiblePriority: number
}

export interface TheoreticalSituationEvaluationResponse {
  evaluationId: string
  situationId: string
  evaluatedAt: string
  evaluatedByPersonId: string
  result: string
  summary: string
  primaryPrograms: string[]
  likelyPrograms: string[]
  edgeCases: string[]
  passCount: number
  failCount: number
  warningCount: number
  blockedCount: number
  notApplicableCount: number
  unknownCount: number
  overrideAvailableCount: number
  overrideBlockedCount: number
  details: TheoreticalSituationEvaluationDetailResponse[]
}

export interface TheoreticalSituationResponse {
  situationId: string
  tenantId: string
  createdByPersonId: string
  title: string
  situationKind: string
  status: string
  evaluationMode: string
  savedAsTemplate: boolean
  createdAt: string
  updatedAt: string
  context: TheoreticalSituationContextResponse[]
  facts: TheoreticalSituationFactResponse[]
  incidents: TheoreticalSituationIncidentResponse[]
  latestEvaluation: TheoreticalSituationEvaluationResponse | null
}

export interface TheoreticalSituationListItemResponse {
  situationId: string
  title: string
  situationKind: string
  status: string
  savedAsTemplate: boolean
  createdAt: string
  updatedAt: string
  latestResult: string | null
}

export interface TheoreticalApplicabilityResultResponse {
  applicabilityResultId: string
  programKey: string
  packKey: string
  citationKey: string
  applicabilityScore: number
  applicabilityBand: string
  matchReasons: string[]
  missingContext: string[]
  exclusionReasons: string[]
  edgeCase: boolean
  edgeCaseReason: string
  userVisiblePriority: number
  createdAt: string
}

export interface TheoreticalNextContextResponse {
  questions: TheoreticalContextFieldResponse[]
  readyForApplicability: boolean
  summary: string
}

export interface TheoreticalEvidenceOptionResponse {
  evidenceOptionId: string
  evidenceOptionKey: string
  evidenceOptionLabel: string
  logicType: string
  requirementKey: string
  factKey: string
  evidenceKind: string
  targetKind: string
  sourceProduct: string
  sourceEntity: string
  required: boolean
}

export interface Title49CalculatorItemResponse {
  factRequirementId: string
  requirementKey: string
  factKey: string
  packKey: string | null
  citationKey: string | null
  sourceProduct: string
  sourceEntity: string
  valueType: string
  operator: string
  expectedValue: string
  retentionPeriod: string
  calculatorKind: string
  parsedNumericThreshold: number | null
  parsedRetentionDays: number | null
  isReady: boolean
  updatedAt: string
}

export interface Title49CalculatorSummaryResponse {
  tenantId: string
  totalRequirements: number
  numericThresholdCount: number
  retentionDurationCount: number
  mixedCalculatorCount: number
  readyCount: number
  reviewCount: number
  generatedAt: string
  requirements: Title49CalculatorItemResponse[]
}

export interface CreateTheoreticalSituationRequest {
  situationKind: string
  title?: string | null
}

export interface TheoreticalSituationContextRequest {
  values: Array<{
    contextKey: string
    contextValueKey: string
  }>
}

export interface TheoreticalSituationFactRequest {
  facts: Array<{
    factKey: string
    simulatedState: string
    requirementKey?: string | null
    citationKey?: string | null
    packKey?: string | null
    simulatedValue?: string | null
    valueType?: string | null
    evidenceOptionKey?: string | null
    evidenceKind?: string | null
    targetKind?: string | null
  }>
}

export interface TheoreticalSituationIncidentRequest {
  incidents: Array<{
    incidentTypeKey: string
    severityKey: string
    involvedSubjectKind: string
    involvedSubjectState: string
    triggerKey: string
    triggerValue: string
    reportabilityState: string
    remediationState: string
  }>
}

