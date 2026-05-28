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

export interface TrainArrMeResponse {
  userId: string
  personId: string
  email: string
  displayName: string
  tenantId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasTrainArrEntitlement: boolean
  entitlements: string[]
}

export interface TrainingDefinitionResponse {
  trainingDefinitionId: string
  definitionKey: string
  name: string
  description: string
  qualificationKey: string
  qualificationName: string
  status: string
  createdAt: string
}

export interface CreateTrainingDefinitionRequest {
  definitionKey: string
  name: string
  description: string
  qualificationKey: string
  qualificationName: string
}

export interface TrainingAssignmentSummaryResponse {
  assignmentId: string
  staffarrPersonId: string
  trainingDefinitionId: string
  trainingDefinitionName: string
  qualificationKey: string
  staffarrIncidentRemediationId: string | null
  sourceQualificationIssueId: string | null
  assignmentReason: string
  status: string
  dueAt: string | null
  createdAt: string
}

export interface TrainingEvaluationResponse {
  evaluationId: string
  trainingAssignmentId: string
  result: string
  score: number | null
  notes: string | null
  evaluatorUserId: string
  evaluatedAt: string
}

export interface TrainingSignoffResponse {
  signoffId: string
  trainingAssignmentId: string
  signoffRole: string
  signedByUserId: string
  notes: string | null
  signedAt: string
}

export interface QualificationIssueResponse {
  qualificationIssueId: string
  trainingAssignmentId: string
  staffarrPersonId: string
  qualificationKey: string
  qualificationName: string
  grantPublicationId: string
  status: string
  issuedAt: string
  statusChangedAt: string | null
  lifecycleReason: string | null
  lifecyclePublicationId: string | null
}

export interface QualificationLifecycleActionRequest {
  reason?: string | null
}

export interface TrainingAssignmentDetailResponse extends TrainingAssignmentSummaryResponse {
  trainingDefinitionKey: string
  qualificationName: string
  assignedByUserId: string | null
  blockerPublicationId: string | null
  completedAt: string | null
  completedByUserId: string | null
  updatedAt: string
  evidenceCount: number
  evaluation: TrainingEvaluationResponse | null
  signoffs: TrainingSignoffResponse[]
  completionRequirementsMet: boolean
  qualificationIssue: QualificationIssueResponse | null
}

export interface SubmitTrainingEvaluationRequest {
  trainingAssignmentId: string
  result: string
  score?: number | null
  notes?: string | null
}

export interface SubmitTrainingSignoffRequest {
  trainingAssignmentId: string
  signoffRole: 'trainee' | 'trainer'
  notes?: string | null
}

export interface TrainingProgramSummaryResponse {
  programId: string
  programKey: string
  name: string
  status: string
  definitionCount: number
  createdAt: string
  updatedAt: string
}

export interface TrainingProgramDefinitionLinkResponse {
  trainingDefinitionId: string
  definitionKey: string
  name: string
  sortOrder: number
}

export interface TrainingProgramDetailResponse {
  programId: string
  programKey: string
  name: string
  description: string
  status: string
  definitions: TrainingProgramDefinitionLinkResponse[]
  createdAt: string
  updatedAt: string
}

export interface CreateTrainingProgramRequest {
  programKey: string
  name: string
  description: string
  trainingDefinitionIds: string[]
}

export interface UpdateTrainingProgramRequest {
  name: string
  description: string
  status: string
  trainingDefinitionIds: string[]
}

export interface TrainingEvidenceResponse {
  evidenceId: string
  trainingAssignmentId: string
  evidenceTypeKey: string
  fileName: string
  contentType: string
  sizeBytes: number
  notes: string | null
  uploadedByUserId: string
  createdAt: string
}

export interface CreateTrainingEvidenceRequest {
  evidenceTypeKey: string
  fileName: string
  contentType: string
  contentBase64: string
  notes?: string | null
}

export interface CreateTrainingAssignmentRequest {
  staffarrPersonId: string
  trainingDefinitionId: string
  staffarrIncidentRemediationId?: string | null
  assignmentReason: string
  dueAt?: string | null
}

export interface CompleteTrainingAssignmentResponse {
  assignmentId: string
  status: string
  completedAt: string
  blockerPublicationId: string | null
}

export interface StaffarrIncidentRemediationResponse {
  remediationId: string
  tenantId: string
  staffarrIncidentId: string
  staffarrPersonId: string
  reasonCategoryKey: string
  status: string
  createdAt: string
}

export interface StaffarrIncidentRemediationDetailResponse extends StaffarrIncidentRemediationResponse {
  severity: string
  title: string
  description: string
  occurredAt: string
  reportedAt: string
}

export interface CreateQualificationCheckRequest {
  staffarrPersonId: string
  qualificationKey: string
  rulePackKey?: string | null
  context?: Record<string, string> | null
  trainingDefinitionId?: string | null
  trainingProgramId?: string | null
}

export interface QualificationCheckResponse {
  checkId: string
  staffarrPersonId: string
  qualificationKey: string
  outcome: 'allow' | 'warn' | 'block' | string
  reasonCode: string
  message: string
  localQualification: QualificationLocalStateResponse | null
  complianceCore: ComplianceCoreCheckSummaryResponse | null
}

export interface BatchQualificationCheckSubject {
  staffarrPersonId: string
  context?: Record<string, string> | null
}

export interface CreateBatchQualificationCheckRequest {
  qualificationKey: string
  rulePackKey?: string | null
  subjects: BatchQualificationCheckSubject[]
  trainingDefinitionId?: string | null
  trainingProgramId?: string | null
}

export interface BatchQualificationCheckSummary {
  total: number
  allowCount: number
  warnCount: number
  blockCount: number
}

export interface BatchQualificationCheckResponse {
  batchId: string
  qualificationKey: string
  results: QualificationCheckResponse[]
  summary: BatchQualificationCheckSummary
}

export interface QualificationLocalStateResponse {
  qualificationIssueId: string | null
  status: string
  message: string
}

export interface ComplianceCoreCheckSummaryResponse {
  rulePackKey: string
  outcome: string
  reasonCode: string
  message: string
  evaluationResult: string
  unresolvedFactKeys: string[]
}

export interface AttachTrainingCitationRequest {
  complianceCoreCitationId: string
  citationKey: string
  citationVersion?: number | null
}

export interface TrainingCitationMetadataResponse {
  label: string
  sourceReference: string
  description: string
  regulatoryProgramKey: string | null
  rulePackKey: string | null
  isActive: boolean
}

export interface TrainingCitationAttachmentResponse {
  attachmentId: string
  entityType: string
  entityId: string
  complianceCoreCitationId: string
  citationKey: string
  citationVersion: number
  createdAt: string
  metadata: TrainingCitationMetadataResponse | null
}

export interface UpsertTrainingRulePackRequirementRequest {
  rulePackKey: string
}

export interface TrainingRulePackMetadataResponse {
  label: string
  description: string
  regulatoryProgramKey: string
  regulatoryProgramLabel: string
  versionNumber: number
  status: string
  isActive: boolean
}

export interface TrainingRulePackRequirementResponse {
  requirementId: string
  entityType: string
  entityId: string
  rulePackKey: string
  createdAt: string
  updatedAt: string
  metadata: TrainingRulePackMetadataResponse | null
}

export interface AssessRulePackImpactRequest {
  rulePackKey: string
  expectedVersionNumber?: number | null
  expectedStatus?: string | null
}

export interface RulePackImpactCurrentStateResponse {
  label: string
  description: string
  regulatoryProgramKey: string
  regulatoryProgramLabel: string
  versionNumber: number
  status: string
  isActive: boolean
}

export interface RulePackImpactDriftResponse {
  hasVersionDrift: boolean
  baselineVersionNumber: number | null
  currentVersionNumber: number | null
  hasStatusDrift: boolean
  baselineStatus: string | null
  currentStatus: string | null
  packInactive: boolean
  packNotFound: boolean
}

export interface RulePackImpactAffectedDefinition {
  trainingDefinitionId: string
  definitionKey: string
  name: string
  qualificationKey: string
  requirementId: string
  knownVersionNumber: number | null
  knownStatus: string | null
}

export interface RulePackImpactAffectedProgram {
  trainingProgramId: string
  programKey: string
  name: string
  requirementId: string
  knownVersionNumber: number | null
  knownStatus: string | null
  memberDefinitionIds: string[]
}

export interface RulePackImpactAffectedAssignment {
  assignmentId: string
  staffarrPersonId: string
  trainingDefinitionId: string
  trainingDefinitionName: string
  status: string
  assignmentReason: string
  createdAt: string
}

export interface RulePackImpactAffectedQualification {
  qualificationIssueId: string
  staffarrPersonId: string
  trainingAssignmentId: string
  qualificationKey: string
  qualificationName: string
  status: string
  issuedAt: string
}

export interface RulePackImpactRecommendedAction {
  actionType: string
  priority: string
  message: string
  entityType: string | null
  entityId: string | null
}

export interface RulePackImpactSummary {
  requirementCount: number
  definitionCount: number
  programCount: number
  activeAssignmentCount: number
  activeQualificationCount: number
  hasDrift: boolean
  requiresAttention: boolean
}

export interface RulePackImpactAssessmentResponse {
  assessmentId: string
  rulePackKey: string
  assessedAt: string
  triggers: string[]
  currentState: RulePackImpactCurrentStateResponse | null
  drift: RulePackImpactDriftResponse | null
  affectedDefinitions: RulePackImpactAffectedDefinition[]
  affectedPrograms: RulePackImpactAffectedProgram[]
  affectedAssignments: RulePackImpactAffectedAssignment[]
  affectedQualifications: RulePackImpactAffectedQualification[]
  recommendedActions: RulePackImpactRecommendedAction[]
  summary: RulePackImpactSummary
}

export interface TrainingNotificationSettingsResponse {
  isEnabled: boolean
  notificationWebhookUrl: string | null
  notifyOnAssignmentCreated: boolean
  notifyOnQualificationExpiring: boolean
  notifyOnQualificationExpired: boolean
  expiringLeadDays: number
  updatedAt: string | null
}

export interface UpsertTrainingNotificationSettingsRequest {
  isEnabled: boolean
  notificationWebhookUrl: string | null
  notifyOnAssignmentCreated: boolean
  notifyOnQualificationExpiring: boolean
  notifyOnQualificationExpired: boolean
  expiringLeadDays: number
}

export interface TrainingNotificationDispatchItem {
  notificationId: string
  eventKind: string
  dispatchStatus: string
  staffarrPersonId: string
  relatedEntityType: string
  relatedEntityId: string
  webhookHost: string | null
  httpStatusCode: number | null
  errorMessage: string | null
  createdAt: string
  dispatchedAt: string | null
}

export interface TrainingNotificationDispatchesResponse {
  items: TrainingNotificationDispatchItem[]
}

export interface RecertificationSettingsResponse {
  isEnabled: boolean
  leadDays: number
  updatedAt: string | null
}

export interface UpsertRecertificationSettingsRequest {
  isEnabled: boolean
  leadDays: number
}

export interface RecertificationAssignmentRunItem {
  runId: string
  qualificationIssueId: string
  trainingAssignmentId: string | null
  outcome: string
  skipReason: string | null
  processedAt: string
}

export interface RecertificationAssignmentRunsResponse {
  items: RecertificationAssignmentRunItem[]
}

export interface QualificationRecalculationSettingsResponse {
  isEnabled: boolean
  stalenessHours: number
  autoSuspendOnBlock: boolean
  updatedAt: string | null
}

export interface UpsertQualificationRecalculationSettingsRequest {
  isEnabled: boolean
  stalenessHours: number
  autoSuspendOnBlock: boolean
}

export interface QualificationRecalculationStateItem {
  qualificationIssueId: string
  staffarrPersonId: string
  qualificationKey: string
  outcome: string
  reasonCode: string
  rulePackKey: string | null
  previousOutcome: string | null
  computedAt: string
}

export interface QualificationRecalculationStatesResponse {
  items: QualificationRecalculationStateItem[]
}

export interface QualificationRecalculationRunItem {
  runId: string
  qualificationIssueId: string
  outcome: string
  checkOutcome: string | null
  skipReason: string | null
  processedAt: string
}

export interface QualificationRecalculationRunsResponse {
  items: QualificationRecalculationRunItem[]
}
