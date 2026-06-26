import type {
  CompleteTrainingAssignmentResponse,
  CreateTrainingAssignmentRequest,
  CreateTrainingDefinitionRequest,
  CreateTrainingDefinitionStepRequest,
  UpdateTrainingDefinitionStepRequest,
  CreateTrainingDefinitionCompletionRuleRequest,
  TrainingCompletionRuleCatalogItemResponse,
  TrainingDefinitionCompletionRuleResponse,
  CreateTrainingDefinitionStepBranchRequest,
  TrainingStepBranchCatalogItemResponse,
  TrainingDefinitionStepBranchResponse,
  CreateTrainingEvidenceRequest,
  CreateTrainingProgramRequest,
  GenerateTrainingProgramDraftRequest,
  HandoffSessionResponse,
  PersonalTrainingDashboardResponse,
  StaffarrIncidentRemediationResponse,
  TrainArrMeResponse,
  TrainArrSessionBootstrapResponse,
  TrainingAssignmentDetailResponse,
  TrainingAssignmentSummaryResponse,
  TrainingAssignmentMaterialDemandLineResponse,
  CreateTrainingAssignmentMaterialDemandLineRequest,
  PublishTrainingAssignmentMaterialDemandRequest,
  PublishTrainingAssignmentMaterialDemandResponse,
  TrainingAssignmentMaterialDemandStatusEventResponse,
  CreateTrainingAssignmentLaborEntryRequest,
  TrainingAssignmentLaborEntryResponse,
  TrainingDefinitionResponse,
  TrainingDefinitionStepResponse,
  TrainingAssignmentStepProgressResponse,
  SubmitTrainingAssignmentStepRequest,
  TrainingEvidenceResponse,
  TrainingProgramDetailResponse,
  TrainingProgramDraftResponse,
  TrainingProgramSummaryResponse,
  TrainingProgramVersionSummaryResponse,
  CreateTrainingProgramContentReferenceRequest,
  TrainingProgramContentReferenceResponse,
  StartProgramRevisionRequest,
  TrainingMatrixViewResponse,
  CreateTrainingMatrixEntryRequest,
  TrainingMatrixEntryResponse,
  TrainingRequirementBuilderViewResponse,
  CreateTrainingApplicabilityProfileRequest,
  TrainingApplicabilityProfileResponse,
  CreateTrainingRequirementRequest,
  TrainingRequirementResponse,
  SyncRequirementToMatrixResponse,
  QualificationIssueListItemResponse,
  QualificationIssueHistoryItemResponse,
  SubmitTrainingEvaluationRequest,
  SubmitTrainingSignoffRequest,
  TrainingEvaluationResponse,
  TrainingEvaluationHistoryResponse,
  TrainingEvaluationReviewTimelineResponse,
  TrainingSignoffResponse,
  QualificationIssueResponse,
  QualificationLifecycleActionRequest,
  UpdateTrainingProgramRequest,
  CreateQualificationCheckRequest,
  CreateBatchQualificationCheckRequest,
  BatchQualificationCheckResponse,
  QualificationCheckResponse,
  AttachTrainingCitationRequest,
  TrainingCitationAttachmentResponse,
  UpsertTrainingRulePackRequirementRequest,
  TrainingRulePackRequirementResponse,
  AssessRulePackImpactRequest,
  RulePackImpactAssessmentResponse,
  QualificationWalletCredentialResponse,
  QualificationWalletVerificationRequest,
  QualificationWalletVerificationResponse,
  TrainingNotificationDispatchesResponse,
  TrainingNotificationSettingsResponse,
  UpsertTrainingNotificationSettingsRequest,
  AssignmentDueReminderSettingsResponse,
  UpsertAssignmentDueReminderSettingsRequest,
  PendingAssignmentDueRemindersResponse,
  AssignmentDueReminderRunsResponse,
  AssignmentEscalationSettingsResponse,
  UpsertAssignmentEscalationSettingsRequest,
  PendingAssignmentEscalationsResponse,
  AssignmentEscalationRunsResponse,
  AssignmentEscalationEventsResponse,
  RecertificationSettingsResponse,
  UpsertRecertificationSettingsRequest,
  RecertificationAssignmentRunsResponse,
  QualificationRecalculationSettingsResponse,
  UpsertQualificationRecalculationSettingsRequest,
  QualificationRecalculationStatesResponse,
  QualificationRecalculationRunsResponse,
  RulePackImpactSettingsResponse,
  UpsertRulePackImpactSettingsRequest,
  RulePackImpactStatesResponse,
  RulePackImpactRunsResponse,
  EvidenceRetentionSettingsResponse,
  UpsertEvidenceRetentionSettingsRequest,
  EvidenceRetentionRunsResponse,
  OrphanReferenceSettingsResponse,
  UpsertOrphanReferenceSettingsRequest,
  OrphanReferenceFindingsResponse,
  OrphanReferenceRunsResponse,
  StaffarrPublicationSettingsResponse,
  UpsertStaffarrPublicationSettingsRequest,
  StaffarrPublicationDeliveriesResponse,
  EventProcessingSettingsResponse,
  UpsertEventProcessingSettingsRequest,
  TrainingDomainEventsResponse,
  PersonTrainingHistoryResponse,
  AuditPackageExportResponse,
  AuditPackageGenerationJobResponse,
  AuditPackageManifestResponse,
  IntegrationSettingsResponse,
  UpsertIntegrationSettingsRequest,
  IntegrationProbesResponse,
  TrainArrTenantSettingsResponse,
  TrainArrTenantSettingsDefaultsResponse,
  UpdateTrainArrTenantSettingsRequest,
  PatchTrainArrTenantSettingsRequest,
  AssignmentReportSummaryResponse,
  QualificationReportSummaryResponse,
  QualificationPointInTimeReportResponse,
  ComplianceReportSummaryResponse,
  EntityExportManifestResponse,
} from './types'

const apiBase = import.meta.env.VITE_TRAINARR_API_BASE ?? ''

export class TrainArrApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly body: string,
  ) {
    super(message)
    this.name = 'TrainArrApiError'
  }
}

type ProblemDetailsLike = {
  title?: string
  detail?: string
  errors?: Record<string, string[] | string>
}

function extractProblemDetailsMessage(body: string): string | null {
  if (!body.trim()) {
    return null
  }

  try {
    const parsed = JSON.parse(body) as ProblemDetailsLike
    const parts: string[] = []

    if (typeof parsed.title === 'string' && parsed.title.trim()) {
      parts.push(parsed.title.trim())
    }

    if (typeof parsed.detail === 'string' && parsed.detail.trim()) {
      parts.push(parsed.detail.trim())
    }

    const errorEntries = parsed.errors ? Object.entries(parsed.errors) : []
    if (errorEntries.length > 0) {
      const flattened = errorEntries
        .flatMap(([field, value]) => {
          const values = Array.isArray(value) ? value : [value]
          return values
            .map((message) => String(message).trim())
            .filter(Boolean)
            .map((message) => `${field}: ${message}`)
        })
      if (flattened.length > 0) {
        parts.push(flattened.join('; '))
      }
    }

    return parts.length > 0 ? parts.join(' - ') : null
  } catch {
    return null
  }
}

async function toApiError(response: Response, fallbackMessage: string): Promise<TrainArrApiError> {
  const body = await response.text()
  const parsedMessage = extractProblemDetailsMessage(body)
  const message = parsedMessage || body || `${fallbackMessage} (${response.status})`
  return new TrainArrApiError(message, response.status, body)
}

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    throw await toApiError(response, fallbackMessage)
  }

  return (await response.json()) as T
}

type LegacyHandoffSessionPayload = HandoffSessionResponse & {
  launchableProductKeys?: string[]
}

type LegacyTrainArrMePayload = TrainArrMeResponse & {
  hasTrainArrAccess?: boolean
  launchableProductKeys?: string[]
}

type LegacyTrainArrSessionBootstrapPayload = TrainArrSessionBootstrapResponse & {
  hasTrainArrAccess?: boolean
  launchableProductKeys?: string[]
}

function resolveLegacyLaunchableProductKeys(
  payload: { launchableProductKeys?: string[] },
): string[] {
  return payload.launchableProductKeys ?? []
}

function normalizeHandoffSessionResponse(payload: LegacyHandoffSessionPayload): HandoffSessionResponse {
  return {
    ...payload,
    launchableProductKeys: resolveLegacyLaunchableProductKeys(payload),
  }
}

function normalizeTrainArrMeResponse(payload: LegacyTrainArrMePayload): TrainArrMeResponse {
  return {
    ...payload,
    hasTrainArrAccess: payload.hasTrainArrAccess,
    launchableProductKeys: resolveLegacyLaunchableProductKeys(payload),
  }
}

function normalizeTrainArrSessionBootstrapResponse(
  payload: LegacyTrainArrSessionBootstrapPayload,
): TrainArrSessionBootstrapResponse {
  return {
    ...payload,
    hasTrainArrAccess: payload.hasTrainArrAccess,
    launchableProductKeys: resolveLegacyLaunchableProductKeys(payload),
  }
}

export async function redeemHandoff(handoffCode: string): Promise<HandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/auth/nexarr/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  return normalizeHandoffSessionResponse(
    await parseJsonResponse<LegacyHandoffSessionPayload>(response, 'Handoff redeem failed'),
  )
}

export async function getMe(accessToken: string): Promise<TrainArrMeResponse> {
  const response = await fetch(`${apiBase}/api/me`, {
    headers: authHeaders(accessToken),
  })
  return normalizeTrainArrMeResponse(
    await parseJsonResponse<LegacyTrainArrMePayload>(response, 'Failed to load profile'),
  )
}

export async function getSessionBootstrap(
  accessToken: string,
): Promise<TrainArrSessionBootstrapResponse> {
  const response = await fetch(`${apiBase}/api/session`, {
    headers: authHeaders(accessToken),
  })
  return normalizeTrainArrSessionBootstrapResponse(
    await parseJsonResponse<LegacyTrainArrSessionBootstrapPayload>(
      response,
      'Failed to load session bootstrap',
    ),
  )
}

export async function getPersonalTrainingDashboard(
  accessToken: string,
): Promise<PersonalTrainingDashboardResponse> {
  const response = await fetch(`${apiBase}/api/me/training`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonalTrainingDashboardResponse>(
    response,
    'Failed to load personal training dashboard',
  )
}

export async function getTrainingDefinitions(accessToken: string): Promise<TrainingDefinitionResponse[]> {
  const response = await fetch(`${apiBase}/api/training-definitions`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainingDefinitionResponse[]>(response, 'Failed to load training definitions')
}

export async function createTrainingDefinition(
  accessToken: string,
  payload: CreateTrainingDefinitionRequest,
): Promise<TrainingDefinitionResponse> {
  const response = await fetch(`${apiBase}/api/training-definitions`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TrainingDefinitionResponse>(response, 'Failed to create training definition')
}

export async function getTrainingDefinitionSteps(
  accessToken: string,
  trainingDefinitionId: string,
): Promise<TrainingDefinitionStepResponse[]> {
  const response = await fetch(`${apiBase}/api/training-definitions/${trainingDefinitionId}/steps`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainingDefinitionStepResponse[]>(response, 'Failed to load training definition steps')
}

export async function createTrainingDefinitionStep(
  accessToken: string,
  trainingDefinitionId: string,
  payload: CreateTrainingDefinitionStepRequest,
): Promise<TrainingDefinitionStepResponse> {
  const response = await fetch(`${apiBase}/api/training-definitions/${trainingDefinitionId}/steps`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TrainingDefinitionStepResponse>(response, 'Failed to create training definition step')
}

export async function updateTrainingDefinitionStep(
  accessToken: string,
  trainingDefinitionId: string,
  stepId: string,
  payload: UpdateTrainingDefinitionStepRequest,
): Promise<TrainingDefinitionStepResponse> {
  const response = await fetch(`${apiBase}/api/training-definitions/${trainingDefinitionId}/steps/${stepId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TrainingDefinitionStepResponse>(response, 'Failed to update training definition step')
}

export async function deleteTrainingDefinitionStep(
  accessToken: string,
  trainingDefinitionId: string,
  stepId: string,
): Promise<void> {
  const response = await fetch(`${apiBase}/api/training-definitions/${trainingDefinitionId}/steps/${stepId}`, {
    method: 'DELETE',
    headers: authHeaders(accessToken),
  })
  if (!response.ok) {
    await parseJsonResponse(response, 'Failed to delete training definition step')
  }
}

export async function getTrainingCompletionRuleCatalog(
  accessToken: string,
): Promise<TrainingCompletionRuleCatalogItemResponse[]> {
  const response = await fetch(`${apiBase}/api/training-completion-rules/catalog`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainingCompletionRuleCatalogItemResponse[]>(
    response,
    'Failed to load completion rule catalog',
  )
}

export async function getTrainingDefinitionCompletionRules(
  accessToken: string,
  trainingDefinitionId: string,
): Promise<TrainingDefinitionCompletionRuleResponse[]> {
  const response = await fetch(
    `${apiBase}/api/training-definitions/${trainingDefinitionId}/completion-rules`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<TrainingDefinitionCompletionRuleResponse[]>(
    response,
    'Failed to load completion rules',
  )
}

export async function createTrainingDefinitionCompletionRule(
  accessToken: string,
  trainingDefinitionId: string,
  payload: CreateTrainingDefinitionCompletionRuleRequest,
): Promise<TrainingDefinitionCompletionRuleResponse> {
  const response = await fetch(
    `${apiBase}/api/training-definitions/${trainingDefinitionId}/completion-rules`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(payload),
    },
  )
  return parseJsonResponse<TrainingDefinitionCompletionRuleResponse>(
    response,
    'Failed to create completion rule',
  )
}

export async function deleteTrainingDefinitionCompletionRule(
  accessToken: string,
  trainingDefinitionId: string,
  completionRuleId: string,
): Promise<void> {
  const response = await fetch(
    `${apiBase}/api/training-definitions/${trainingDefinitionId}/completion-rules/${completionRuleId}`,
    {
      method: 'DELETE',
      headers: authHeaders(accessToken),
    },
  )
  if (!response.ok) {
    await parseJsonResponse(response, 'Failed to delete completion rule')
  }
}

export async function getTrainingStepBranchCatalog(
  accessToken: string,
): Promise<TrainingStepBranchCatalogItemResponse[]> {
  const response = await fetch(`${apiBase}/api/training-step-branches/catalog`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainingStepBranchCatalogItemResponse[]>(
    response,
    'Failed to load step branch catalog',
  )
}

export async function getTrainingDefinitionStepBranches(
  accessToken: string,
  trainingDefinitionId: string,
  stepId: string,
): Promise<TrainingDefinitionStepBranchResponse[]> {
  const response = await fetch(
    `${apiBase}/api/training-definitions/${trainingDefinitionId}/steps/${stepId}/branches`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<TrainingDefinitionStepBranchResponse[]>(
    response,
    'Failed to load step branches',
  )
}

export async function createTrainingDefinitionStepBranch(
  accessToken: string,
  trainingDefinitionId: string,
  stepId: string,
  payload: CreateTrainingDefinitionStepBranchRequest,
): Promise<TrainingDefinitionStepBranchResponse> {
  const response = await fetch(
    `${apiBase}/api/training-definitions/${trainingDefinitionId}/steps/${stepId}/branches`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(payload),
    },
  )
  return parseJsonResponse<TrainingDefinitionStepBranchResponse>(
    response,
    'Failed to create step branch',
  )
}

export async function deleteTrainingDefinitionStepBranch(
  accessToken: string,
  trainingDefinitionId: string,
  stepId: string,
  branchId: string,
): Promise<void> {
  const response = await fetch(
    `${apiBase}/api/training-definitions/${trainingDefinitionId}/steps/${stepId}/branches/${branchId}`,
    {
      method: 'DELETE',
      headers: authHeaders(accessToken),
    },
  )
  if (!response.ok) {
    await parseJsonResponse(response, 'Failed to delete step branch')
  }
}

export async function getTrainingAssignmentSteps(
  accessToken: string,
  assignmentId: string,
): Promise<TrainingAssignmentStepProgressResponse[]> {
  const response = await fetch(`${apiBase}/api/training-assignments/${assignmentId}/steps`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainingAssignmentStepProgressResponse[]>(response, 'Failed to load assignment steps')
}

export async function submitTrainingAssignmentStep(
  accessToken: string,
  assignmentId: string,
  stepId: string,
  payload: SubmitTrainingAssignmentStepRequest,
): Promise<TrainingAssignmentStepProgressResponse> {
  const response = await fetch(`${apiBase}/api/training-assignments/${assignmentId}/steps/${stepId}/submit`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TrainingAssignmentStepProgressResponse>(response, 'Failed to submit assignment step')
}

export async function getTrainingAssignments(
  accessToken: string,
  params?: { staffarrPersonId?: string; staffarrIncidentRemediationId?: string; status?: string },
): Promise<TrainingAssignmentSummaryResponse[]> {
  const search = new URLSearchParams()
  if (params?.staffarrPersonId) search.set('staffarrPersonId', params.staffarrPersonId)
  if (params?.staffarrIncidentRemediationId) search.set('staffarrIncidentRemediationId', params.staffarrIncidentRemediationId)
  if (params?.status) search.set('status', params.status)
  const query = search.toString()
  const response = await fetch(`${apiBase}/api/training-assignments${query ? `?${query}` : ''}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainingAssignmentSummaryResponse[]>(response, 'Failed to load training assignments')
}

export async function getTrainingAssignment(
  accessToken: string,
  assignmentId: string,
): Promise<TrainingAssignmentDetailResponse> {
  const response = await fetch(`${apiBase}/api/training-assignments/${assignmentId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainingAssignmentDetailResponse>(response, 'Failed to load training assignment')
}

export async function createTrainingAssignment(
  accessToken: string,
  payload: CreateTrainingAssignmentRequest,
): Promise<TrainingAssignmentDetailResponse> {
  const response = await fetch(`${apiBase}/api/training-assignments`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TrainingAssignmentDetailResponse>(response, 'Failed to create training assignment')
}

export async function completeTrainingAssignment(
  accessToken: string,
  assignmentId: string,
): Promise<CompleteTrainingAssignmentResponse> {
  const response = await fetch(`${apiBase}/api/training-assignments/${assignmentId}/complete`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CompleteTrainingAssignmentResponse>(response, 'Failed to complete training assignment')
}

export async function getTrainingPrograms(accessToken: string): Promise<TrainingProgramSummaryResponse[]> {
  const response = await fetch(`${apiBase}/api/training-programs`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainingProgramSummaryResponse[]>(response, 'Failed to load training programs')
}

export async function createTrainingProgram(
  accessToken: string,
  payload: CreateTrainingProgramRequest,
): Promise<TrainingProgramDetailResponse> {
  const response = await fetch(`${apiBase}/api/training-programs`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TrainingProgramDetailResponse>(response, 'Failed to create training program')
}

export async function generateTrainingProgramDraft(
  accessToken: string,
  payload: GenerateTrainingProgramDraftRequest,
): Promise<TrainingProgramDraftResponse> {
  const response = await fetch(`${apiBase}/api/training-programs/draft`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TrainingProgramDraftResponse>(response, 'Failed to generate training program draft')
}

export async function updateTrainingProgram(
  accessToken: string,
  programId: string,
  payload: UpdateTrainingProgramRequest,
): Promise<TrainingProgramDetailResponse> {
  const response = await fetch(`${apiBase}/api/training-programs/${programId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TrainingProgramDetailResponse>(response, 'Failed to update training program')
}

export async function getTrainingProgram(
  accessToken: string,
  programId: string,
): Promise<TrainingProgramDetailResponse> {
  const response = await fetch(`${apiBase}/api/training-programs/${programId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainingProgramDetailResponse>(response, 'Failed to load training program')
}

export async function getTrainingProgramContentReferences(
  accessToken: string,
  programId: string,
): Promise<TrainingProgramContentReferenceResponse[]> {
  const response = await fetch(`${apiBase}/api/training-programs/${programId}/content-references`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainingProgramContentReferenceResponse[]>(
    response,
    'Failed to load program content references',
  )
}

export async function attachTrainingProgramContentReference(
  accessToken: string,
  programId: string,
  payload: CreateTrainingProgramContentReferenceRequest,
): Promise<TrainingProgramContentReferenceResponse> {
  const response = await fetch(`${apiBase}/api/training-programs/${programId}/content-references`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TrainingProgramContentReferenceResponse>(
    response,
    'Failed to attach program content reference',
  )
}

export async function removeTrainingProgramContentReference(
  accessToken: string,
  programId: string,
  contentReferenceId: string,
): Promise<void> {
  const response = await fetch(
    `${apiBase}/api/training-programs/${programId}/content-references/${contentReferenceId}`,
    {
      method: 'DELETE',
      headers: authHeaders(accessToken),
    },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Failed to remove program content reference')
  }
}

export async function getTrainingAssignmentLaborEntries(
  accessToken: string,
  assignmentId: string,
): Promise<TrainingAssignmentLaborEntryResponse[]> {
  const response = await fetch(`${apiBase}/api/training-assignments/${assignmentId}/labor`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainingAssignmentLaborEntryResponse[]>(
    response,
    'Failed to load assignment labor entries',
  )
}

export async function createTrainingAssignmentLaborEntry(
  accessToken: string,
  assignmentId: string,
  payload: CreateTrainingAssignmentLaborEntryRequest,
): Promise<TrainingAssignmentLaborEntryResponse> {
  const response = await fetch(`${apiBase}/api/training-assignments/${assignmentId}/labor`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TrainingAssignmentLaborEntryResponse>(
    response,
    'Failed to create assignment labor entry',
  )
}

export async function removeTrainingAssignmentLaborEntry(
  accessToken: string,
  assignmentId: string,
  laborEntryId: string,
): Promise<void> {
  const response = await fetch(
    `${apiBase}/api/training-assignments/${assignmentId}/labor/${laborEntryId}`,
    {
      method: 'DELETE',
      headers: authHeaders(accessToken),
    },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Failed to remove assignment labor entry')
  }
}

export async function getTrainingProgramVersions(
  accessToken: string,
  programId: string,
): Promise<TrainingProgramVersionSummaryResponse[]> {
  const response = await fetch(
    `${apiBase}/api/program-versions?programId=${encodeURIComponent(programId)}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse(response, 'Failed to load program versions')
}

export async function startTrainingProgramRevision(
  accessToken: string,
  payload: StartProgramRevisionRequest,
): Promise<TrainingProgramDetailResponse> {
  const response = await fetch(`${apiBase}/api/program-versions/start-revision`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TrainingProgramDetailResponse>(response, 'Failed to start program revision')
}

export async function getTrainingMatrix(
  accessToken: string,
): Promise<TrainingMatrixViewResponse> {
  const response = await fetch(`${apiBase}/api/training-matrix`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse(response, 'Failed to load training matrix')
}

export async function createTrainingMatrixEntry(
  accessToken: string,
  payload: CreateTrainingMatrixEntryRequest,
): Promise<TrainingMatrixEntryResponse> {
  const response = await fetch(`${apiBase}/api/training-matrix`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse(response, 'Failed to create training matrix entry')
}

export async function deleteTrainingMatrixEntry(
  accessToken: string,
  matrixEntryId: string,
): Promise<void> {
  const response = await fetch(`${apiBase}/api/training-matrix/${matrixEntryId}`, {
    method: 'DELETE',
    headers: authHeaders(accessToken),
  })
  if (!response.ok) {
    throw await toApiError(response, 'Failed to delete training matrix entry')
  }
}

export async function getTrainingRequirementBuilderView(
  accessToken: string,
): Promise<TrainingRequirementBuilderViewResponse> {
  const response = await fetch(`${apiBase}/api/training-requirements/builder-view`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse(response, 'Failed to load requirement builder view')
}

export async function createTrainingApplicabilityProfile(
  accessToken: string,
  payload: CreateTrainingApplicabilityProfileRequest,
): Promise<TrainingApplicabilityProfileResponse> {
  const response = await fetch(`${apiBase}/api/applicability-profiles`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse(response, 'Failed to create applicability profile')
}

export async function deleteTrainingApplicabilityProfile(
  accessToken: string,
  profileId: string,
): Promise<void> {
  const response = await fetch(`${apiBase}/api/applicability-profiles/${profileId}`, {
    method: 'DELETE',
    headers: authHeaders(accessToken),
  })
  if (!response.ok) {
    throw await toApiError(response, 'Failed to delete applicability profile')
  }
}

export async function createTrainingRequirement(
  accessToken: string,
  payload: CreateTrainingRequirementRequest,
): Promise<TrainingRequirementResponse> {
  const response = await fetch(`${apiBase}/api/training-requirements`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse(response, 'Failed to create training requirement')
}

export async function deleteTrainingRequirement(
  accessToken: string,
  requirementId: string,
): Promise<void> {
  const response = await fetch(`${apiBase}/api/training-requirements/${requirementId}`, {
    method: 'DELETE',
    headers: authHeaders(accessToken),
  })
  if (!response.ok) {
    throw await toApiError(response, 'Failed to delete training requirement')
  }
}

export async function syncTrainingRequirementToMatrix(
  accessToken: string,
  requirementId: string,
): Promise<SyncRequirementToMatrixResponse> {
  const response = await fetch(`${apiBase}/api/training-requirements/sync-to-matrix`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ requirementId }),
  })
  return parseJsonResponse(response, 'Failed to sync requirement to training matrix')
}

export async function listQualificationIssues(
  accessToken: string,
  status?: string,
): Promise<QualificationIssueListItemResponse[]> {
  const query = status ? `?status=${encodeURIComponent(status)}` : ''
  const response = await fetch(`${apiBase}/api/qualification-issues${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse(response, 'Failed to load qualification issues')
}

export async function getQualificationIssueHistory(
  accessToken: string,
  qualificationIssueId: string,
): Promise<QualificationIssueHistoryItemResponse[]> {
  const response = await fetch(`${apiBase}/api/qualification-issues/${qualificationIssueId}/history`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse(response, 'Failed to load qualification issue history')
}

export async function getQualificationWalletCredential(
  accessToken: string,
  qualificationIssueId: string,
): Promise<QualificationWalletCredentialResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/qualifications/${qualificationIssueId}/wallet-credential`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<QualificationWalletCredentialResponse>(
    response,
    'Failed to load qualification wallet credential',
  )
}

export async function verifyQualificationWalletCredential(
  accessToken: string,
  payload: QualificationWalletVerificationRequest,
): Promise<QualificationWalletVerificationResponse> {
  const response = await fetch(`${apiBase}/api/v1/qualifications/wallet/verify`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<QualificationWalletVerificationResponse>(
    response,
    'Failed to verify qualification wallet credential',
  )
}

export async function getTrainingEvidence(
  accessToken: string,
  assignmentId: string,
): Promise<TrainingEvidenceResponse[]> {
  const response = await fetch(`${apiBase}/api/training-assignments/${assignmentId}/evidence`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainingEvidenceResponse[]>(response, 'Failed to load training evidence')
}

export async function createTrainingEvidence(
  accessToken: string,
  assignmentId: string,
  payload: CreateTrainingEvidenceRequest,
): Promise<TrainingEvidenceResponse> {
  const response = await fetch(`${apiBase}/api/training-assignments/${assignmentId}/evidence`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TrainingEvidenceResponse>(response, 'Failed to upload training evidence')
}

export async function submitTrainingEvaluation(
  accessToken: string,
  assignmentId: string,
  payload: SubmitTrainingEvaluationRequest,
): Promise<TrainingEvaluationResponse> {
  const response = await fetch(`${apiBase}/api/training-assignments/${assignmentId}/evaluations`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TrainingEvaluationResponse>(response, 'Failed to submit training evaluation')
}

export async function getTrainingEvaluationHistory(
  accessToken: string,
  assignmentId: string,
): Promise<TrainingEvaluationHistoryResponse> {
  const response = await fetch(
    `${apiBase}/api/training-assignments/${assignmentId}/evaluations/history`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<TrainingEvaluationHistoryResponse>(
    response,
    'Failed to load training evaluation history',
  )
}

export async function getTrainingEvaluationReviewTimeline(
  accessToken: string,
  options?: { staffarrPersonId?: string; result?: string; limit?: number },
): Promise<TrainingEvaluationReviewTimelineResponse> {
  const params = new URLSearchParams()
  if (options?.staffarrPersonId) {
    params.set('staffarrPersonId', options.staffarrPersonId)
  }
  if (options?.result) {
    params.set('result', options.result)
  }
  if (options?.limit != null) {
    params.set('limit', String(options.limit))
  }
  const query = params.toString()
  const response = await fetch(
    `${apiBase}/api/evaluations/review-timeline${query ? `?${query}` : ''}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<TrainingEvaluationReviewTimelineResponse>(
    response,
    'Failed to load evaluation review timeline',
  )
}

export async function submitTrainingSignoff(
  accessToken: string,
  assignmentId: string,
  payload: SubmitTrainingSignoffRequest,
): Promise<TrainingSignoffResponse> {
  const response = await fetch(`${apiBase}/api/training-assignments/${assignmentId}/signoffs`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TrainingSignoffResponse>(response, 'Failed to submit training signoff')
}

export async function getTrainingAssignmentMaterialDemand(
  accessToken: string,
  assignmentId: string,
): Promise<TrainingAssignmentMaterialDemandLineResponse[]> {
  const response = await fetch(
    `${apiBase}/api/training-assignments/${assignmentId}/material-demand`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<TrainingAssignmentMaterialDemandLineResponse[]>(
    response,
    'Failed to load assignment material demand',
  )
}

export async function createTrainingAssignmentMaterialDemandLine(
  accessToken: string,
  assignmentId: string,
  payload: CreateTrainingAssignmentMaterialDemandLineRequest,
): Promise<TrainingAssignmentMaterialDemandLineResponse> {
  const response = await fetch(
    `${apiBase}/api/training-assignments/${assignmentId}/material-demand`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(payload),
    },
  )
  return parseJsonResponse<TrainingAssignmentMaterialDemandLineResponse>(
    response,
    'Failed to create assignment material demand line',
  )
}

export async function publishTrainingAssignmentMaterialDemand(
  accessToken: string,
  assignmentId: string,
  payload: PublishTrainingAssignmentMaterialDemandRequest = {},
): Promise<PublishTrainingAssignmentMaterialDemandResponse> {
  const response = await fetch(
    `${apiBase}/api/training-assignments/${assignmentId}/material-demand/publish`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(payload),
    },
  )
  return parseJsonResponse<PublishTrainingAssignmentMaterialDemandResponse>(
    response,
    'Failed to publish assignment material demand',
  )
}

export async function getTrainingAssignmentMaterialDemandStatusEvents(
  accessToken: string,
  assignmentId: string,
): Promise<TrainingAssignmentMaterialDemandStatusEventResponse[]> {
  const response = await fetch(
    `${apiBase}/api/training-assignments/${assignmentId}/material-demand/status-events`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<TrainingAssignmentMaterialDemandStatusEventResponse[]>(
    response,
    'Failed to load assignment material demand status events',
  )
}

export async function getIncidentRemediations(
  accessToken: string,
  status?: string,
): Promise<StaffarrIncidentRemediationResponse[]> {
  const query = status ? `?status=${encodeURIComponent(status)}` : ''
  const response = await fetch(`${apiBase}/api/incident-remediations${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffarrIncidentRemediationResponse[]>(response, 'Failed to load incident remediations')
}

export async function suspendQualificationIssue(
  accessToken: string,
  qualificationIssueId: string,
  payload: QualificationLifecycleActionRequest = {},
): Promise<QualificationIssueResponse> {
  const response = await fetch(`${apiBase}/api/qualification-issues/${qualificationIssueId}/suspend`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<QualificationIssueResponse>(response, 'Failed to suspend qualification issue')
}

export async function revokeQualificationIssue(
  accessToken: string,
  qualificationIssueId: string,
  payload: QualificationLifecycleActionRequest = {},
): Promise<QualificationIssueResponse> {
  const response = await fetch(`${apiBase}/api/qualification-issues/${qualificationIssueId}/revoke`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<QualificationIssueResponse>(response, 'Failed to revoke qualification issue')
}

export async function expireQualificationIssue(
  accessToken: string,
  qualificationIssueId: string,
  payload: QualificationLifecycleActionRequest = {},
): Promise<QualificationIssueResponse> {
  const response = await fetch(`${apiBase}/api/qualification-issues/${qualificationIssueId}/expire`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<QualificationIssueResponse>(response, 'Failed to expire qualification issue')
}

export async function createQualificationCheck(
  accessToken: string,
  payload: CreateQualificationCheckRequest,
): Promise<QualificationCheckResponse> {
  const response = await fetch(`${apiBase}/api/qualification-checks`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<QualificationCheckResponse>(response, 'Failed to run qualification check')
}

export async function listQualificationChecks(
  accessToken: string,
  params?: { staffarrPersonId?: string; qualificationKey?: string; limit?: number },
): Promise<import('./types').QualificationCheckHistoryItemResponse[]> {
  const search = new URLSearchParams()
  if (params?.staffarrPersonId) {
    search.set('staffarrPersonId', params.staffarrPersonId)
  }
  if (params?.qualificationKey) {
    search.set('qualificationKey', params.qualificationKey)
  }
  if (params?.limit != null) {
    search.set('limit', String(params.limit))
  }
  const query = search.toString()
  const response = await fetch(`${apiBase}/api/qualification-checks${query ? `?${query}` : ''}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse(response, 'Failed to load qualification check history')
}

export async function createBatchQualificationCheck(
  accessToken: string,
  payload: CreateBatchQualificationCheckRequest,
): Promise<BatchQualificationCheckResponse> {
  const response = await fetch(`${apiBase}/api/qualification-checks/batch`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<BatchQualificationCheckResponse>(response, 'Failed to run batch qualification check')
}

export async function getTrainingDefinitionCitations(
  accessToken: string,
  trainingDefinitionId: string,
  includeMetadata = true,
): Promise<TrainingCitationAttachmentResponse[]> {
  const query = includeMetadata ? '?includeMetadata=true' : ''
  const response = await fetch(
    `${apiBase}/api/training-definitions/${trainingDefinitionId}/citations${query}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<TrainingCitationAttachmentResponse[]>(
    response,
    'Failed to load definition citations',
  )
}

export async function attachTrainingDefinitionCitation(
  accessToken: string,
  trainingDefinitionId: string,
  payload: AttachTrainingCitationRequest,
  validateWithComplianceCore = false,
): Promise<TrainingCitationAttachmentResponse> {
  const query = validateWithComplianceCore ? '?validateWithComplianceCore=true' : ''
  const response = await fetch(
    `${apiBase}/api/training-definitions/${trainingDefinitionId}/citations${query}`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(payload),
    },
  )
  return parseJsonResponse<TrainingCitationAttachmentResponse>(
    response,
    'Failed to attach definition citation',
  )
}

export async function removeTrainingDefinitionCitation(
  accessToken: string,
  trainingDefinitionId: string,
  attachmentId: string,
): Promise<void> {
  const response = await fetch(
    `${apiBase}/api/training-definitions/${trainingDefinitionId}/citations/${attachmentId}`,
    {
      method: 'DELETE',
      headers: authHeaders(accessToken),
    },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Failed to remove citation')
  }
}

export async function getTrainingProgramCitations(
  accessToken: string,
  programId: string,
  includeMetadata = true,
): Promise<TrainingCitationAttachmentResponse[]> {
  const query = includeMetadata ? '?includeMetadata=true' : ''
  const response = await fetch(`${apiBase}/api/training-programs/${programId}/citations${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainingCitationAttachmentResponse[]>(
    response,
    'Failed to load program citations',
  )
}

export async function attachTrainingProgramCitation(
  accessToken: string,
  programId: string,
  payload: AttachTrainingCitationRequest,
  validateWithComplianceCore = false,
): Promise<TrainingCitationAttachmentResponse> {
  const query = validateWithComplianceCore ? '?validateWithComplianceCore=true' : ''
  const response = await fetch(`${apiBase}/api/training-programs/${programId}/citations${query}`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TrainingCitationAttachmentResponse>(
    response,
    'Failed to attach program citation',
  )
}

export async function removeTrainingProgramCitation(
  accessToken: string,
  programId: string,
  attachmentId: string,
): Promise<void> {
  const response = await fetch(
    `${apiBase}/api/training-programs/${programId}/citations/${attachmentId}`,
    {
      method: 'DELETE',
      headers: authHeaders(accessToken),
    },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Failed to remove program citation')
  }
}

export async function getTrainingDefinitionRulePackRequirements(
  accessToken: string,
  trainingDefinitionId: string,
  includeMetadata = true,
): Promise<TrainingRulePackRequirementResponse[]> {
  const query = includeMetadata ? '?includeMetadata=true' : ''
  const response = await fetch(
    `${apiBase}/api/training-definitions/${trainingDefinitionId}/rule-pack-requirements${query}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<TrainingRulePackRequirementResponse[]>(
    response,
    'Failed to load definition rule pack requirements',
  )
}

export async function upsertTrainingDefinitionRulePackRequirement(
  accessToken: string,
  trainingDefinitionId: string,
  payload: UpsertTrainingRulePackRequirementRequest,
  validateWithComplianceCore = false,
): Promise<TrainingRulePackRequirementResponse> {
  const query = validateWithComplianceCore ? '?validateWithComplianceCore=true' : ''
  const response = await fetch(
    `${apiBase}/api/training-definitions/${trainingDefinitionId}/rule-pack-requirements${query}`,
    {
      method: 'PUT',
      headers: authHeaders(accessToken),
      body: JSON.stringify(payload),
    },
  )
  return parseJsonResponse<TrainingRulePackRequirementResponse>(
    response,
    'Failed to save definition rule pack requirement',
  )
}

export async function removeTrainingDefinitionRulePackRequirement(
  accessToken: string,
  trainingDefinitionId: string,
  requirementId: string,
): Promise<void> {
  const response = await fetch(
    `${apiBase}/api/training-definitions/${trainingDefinitionId}/rule-pack-requirements/${requirementId}`,
    {
      method: 'DELETE',
      headers: authHeaders(accessToken),
    },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Failed to remove definition rule pack requirement')
  }
}

export async function getTrainingProgramRulePackRequirements(
  accessToken: string,
  programId: string,
  includeMetadata = true,
): Promise<TrainingRulePackRequirementResponse[]> {
  const query = includeMetadata ? '?includeMetadata=true' : ''
  const response = await fetch(
    `${apiBase}/api/training-programs/${programId}/rule-pack-requirements${query}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<TrainingRulePackRequirementResponse[]>(
    response,
    'Failed to load program rule pack requirements',
  )
}

export async function upsertTrainingProgramRulePackRequirement(
  accessToken: string,
  programId: string,
  payload: UpsertTrainingRulePackRequirementRequest,
  validateWithComplianceCore = false,
): Promise<TrainingRulePackRequirementResponse> {
  const query = validateWithComplianceCore ? '?validateWithComplianceCore=true' : ''
  const response = await fetch(
    `${apiBase}/api/training-programs/${programId}/rule-pack-requirements${query}`,
    {
      method: 'PUT',
      headers: authHeaders(accessToken),
      body: JSON.stringify(payload),
    },
  )
  return parseJsonResponse<TrainingRulePackRequirementResponse>(
    response,
    'Failed to save program rule pack requirement',
  )
}

export async function removeTrainingProgramRulePackRequirement(
  accessToken: string,
  programId: string,
  requirementId: string,
): Promise<void> {
  const response = await fetch(
    `${apiBase}/api/training-programs/${programId}/rule-pack-requirements/${requirementId}`,
    {
      method: 'DELETE',
      headers: authHeaders(accessToken),
    },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Failed to remove program rule pack requirement')
  }
}

export async function getRulePackImpactAssessment(
  accessToken: string,
  rulePackKey: string,
  params?: { expectedVersionNumber?: number; expectedStatus?: string },
): Promise<RulePackImpactAssessmentResponse> {
  const search = new URLSearchParams({ rulePackKey })
  if (params?.expectedVersionNumber != null) {
    search.set('expectedVersionNumber', String(params.expectedVersionNumber))
  }
  if (params?.expectedStatus) {
    search.set('expectedStatus', params.expectedStatus)
  }
  const response = await fetch(`${apiBase}/api/rule-pack-impact?${search.toString()}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RulePackImpactAssessmentResponse>(
    response,
    'Failed to assess rule pack impact',
  )
}

export async function assessRulePackImpact(
  accessToken: string,
  payload: AssessRulePackImpactRequest,
): Promise<RulePackImpactAssessmentResponse> {
  const response = await fetch(`${apiBase}/api/rule-pack-impact/assess`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<RulePackImpactAssessmentResponse>(
    response,
    'Failed to assess rule pack impact',
  )
}

export async function getTrainingNotificationSettings(
  accessToken: string,
): Promise<TrainingNotificationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/notification-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainingNotificationSettingsResponse>(
    response,
    'Failed to load notification settings',
  )
}

export async function upsertTrainingNotificationSettings(
  accessToken: string,
  payload: UpsertTrainingNotificationSettingsRequest,
): Promise<TrainingNotificationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/notification-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TrainingNotificationSettingsResponse>(
    response,
    'Failed to save notification settings',
  )
}

export async function getTrainingNotificationDispatches(
  accessToken: string,
  limit = 20,
): Promise<TrainingNotificationDispatchesResponse> {
  const response = await fetch(`${apiBase}/api/notification-settings/dispatches?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainingNotificationDispatchesResponse>(
    response,
    'Failed to load notification dispatches',
  )
}

export async function getAssignmentDueReminderSettings(
  accessToken: string,
): Promise<AssignmentDueReminderSettingsResponse> {
  const response = await fetch(`${apiBase}/api/assignment-due-reminder-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssignmentDueReminderSettingsResponse>(
    response,
    'Failed to load assignment due reminder settings',
  )
}

export async function upsertAssignmentDueReminderSettings(
  accessToken: string,
  payload: UpsertAssignmentDueReminderSettingsRequest,
): Promise<AssignmentDueReminderSettingsResponse> {
  const response = await fetch(`${apiBase}/api/assignment-due-reminder-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AssignmentDueReminderSettingsResponse>(
    response,
    'Failed to save assignment due reminder settings',
  )
}

export async function getPendingAssignmentDueReminders(
  accessToken: string,
): Promise<PendingAssignmentDueRemindersResponse> {
  const response = await fetch(`${apiBase}/api/assignment-due-reminder-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingAssignmentDueRemindersResponse>(
    response,
    'Failed to load pending assignment due reminders',
  )
}

export async function getAssignmentDueReminderRuns(
  accessToken: string,
  limit = 8,
): Promise<AssignmentDueReminderRunsResponse> {
  const response = await fetch(`${apiBase}/api/assignment-due-reminder-settings/runs?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssignmentDueReminderRunsResponse>(
    response,
    'Failed to load assignment due reminder runs',
  )
}

export async function getAssignmentEscalationSettings(
  accessToken: string,
): Promise<AssignmentEscalationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/assignment-escalation-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssignmentEscalationSettingsResponse>(
    response,
    'Failed to load assignment escalation settings',
  )
}

export async function upsertAssignmentEscalationSettings(
  accessToken: string,
  payload: UpsertAssignmentEscalationSettingsRequest,
): Promise<AssignmentEscalationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/assignment-escalation-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AssignmentEscalationSettingsResponse>(
    response,
    'Failed to save assignment escalation settings',
  )
}

export async function getPendingAssignmentEscalations(
  accessToken: string,
): Promise<PendingAssignmentEscalationsResponse> {
  const response = await fetch(`${apiBase}/api/assignment-escalation-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingAssignmentEscalationsResponse>(
    response,
    'Failed to load pending assignment escalations',
  )
}

export async function getAssignmentEscalationRuns(
  accessToken: string,
  limit = 8,
): Promise<AssignmentEscalationRunsResponse> {
  const response = await fetch(`${apiBase}/api/assignment-escalation-settings/runs?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssignmentEscalationRunsResponse>(
    response,
    'Failed to load assignment escalation runs',
  )
}

export async function getAssignmentEscalationEvents(
  accessToken: string,
  limit = 8,
): Promise<AssignmentEscalationEventsResponse> {
  const response = await fetch(`${apiBase}/api/assignment-escalation-settings/events?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssignmentEscalationEventsResponse>(
    response,
    'Failed to load assignment escalation events',
  )
}

export async function getRecertificationSettings(
  accessToken: string,
): Promise<RecertificationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/recertification-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RecertificationSettingsResponse>(
    response,
    'Failed to load recertification settings',
  )
}

export async function upsertRecertificationSettings(
  accessToken: string,
  payload: UpsertRecertificationSettingsRequest,
): Promise<RecertificationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/recertification-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<RecertificationSettingsResponse>(
    response,
    'Failed to save recertification settings',
  )
}

export async function getRecertificationAssignmentRuns(
  accessToken: string,
  limit = 10,
): Promise<RecertificationAssignmentRunsResponse> {
  const response = await fetch(`${apiBase}/api/recertification-settings/runs?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RecertificationAssignmentRunsResponse>(
    response,
    'Failed to load recertification assignment runs',
  )
}

export async function getQualificationRecalculationSettings(
  accessToken: string,
): Promise<QualificationRecalculationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/qualification-recalculation-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<QualificationRecalculationSettingsResponse>(
    response,
    'Failed to load qualification recalculation settings',
  )
}

export async function upsertQualificationRecalculationSettings(
  accessToken: string,
  payload: UpsertQualificationRecalculationSettingsRequest,
): Promise<QualificationRecalculationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/qualification-recalculation-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<QualificationRecalculationSettingsResponse>(
    response,
    'Failed to save qualification recalculation settings',
  )
}

export async function getQualificationRecalculationStates(
  accessToken: string,
  limit = 10,
): Promise<QualificationRecalculationStatesResponse> {
  const response = await fetch(`${apiBase}/api/qualification-recalculation-settings/states?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<QualificationRecalculationStatesResponse>(
    response,
    'Failed to load qualification recalculation states',
  )
}

export async function getQualificationRecalculationRuns(
  accessToken: string,
  limit = 10,
): Promise<QualificationRecalculationRunsResponse> {
  const response = await fetch(`${apiBase}/api/qualification-recalculation-settings/runs?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<QualificationRecalculationRunsResponse>(
    response,
    'Failed to load qualification recalculation runs',
  )
}

export async function getRulePackImpactSettings(
  accessToken: string,
): Promise<RulePackImpactSettingsResponse> {
  const response = await fetch(`${apiBase}/api/rule-pack-impact-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RulePackImpactSettingsResponse>(
    response,
    'Failed to load rule pack impact settings',
  )
}

export async function upsertRulePackImpactSettings(
  accessToken: string,
  payload: UpsertRulePackImpactSettingsRequest,
): Promise<RulePackImpactSettingsResponse> {
  const response = await fetch(`${apiBase}/api/rule-pack-impact-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<RulePackImpactSettingsResponse>(
    response,
    'Failed to save rule pack impact settings',
  )
}

export async function getRulePackImpactStates(
  accessToken: string,
  limit = 10,
): Promise<RulePackImpactStatesResponse> {
  const response = await fetch(`${apiBase}/api/rule-pack-impact-settings/states?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RulePackImpactStatesResponse>(
    response,
    'Failed to load rule pack impact states',
  )
}

export async function getRulePackImpactRuns(
  accessToken: string,
  limit = 10,
): Promise<RulePackImpactRunsResponse> {
  const response = await fetch(`${apiBase}/api/rule-pack-impact-settings/runs?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RulePackImpactRunsResponse>(
    response,
    'Failed to load rule pack impact runs',
  )
}

export async function getEvidenceRetentionSettings(
  accessToken: string,
): Promise<EvidenceRetentionSettingsResponse> {
  const response = await fetch(`${apiBase}/api/evidence-retention-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<EvidenceRetentionSettingsResponse>(
    response,
    'Failed to load evidence retention settings',
  )
}

export async function upsertEvidenceRetentionSettings(
  accessToken: string,
  payload: UpsertEvidenceRetentionSettingsRequest,
): Promise<EvidenceRetentionSettingsResponse> {
  const response = await fetch(`${apiBase}/api/evidence-retention-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<EvidenceRetentionSettingsResponse>(
    response,
    'Failed to save evidence retention settings',
  )
}

export async function getEvidenceRetentionRuns(
  accessToken: string,
  limit = 10,
): Promise<EvidenceRetentionRunsResponse> {
  const response = await fetch(`${apiBase}/api/evidence-retention-settings/runs?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<EvidenceRetentionRunsResponse>(
    response,
    'Failed to load evidence retention runs',
  )
}

export async function getOrphanReferenceSettings(
  accessToken: string,
): Promise<OrphanReferenceSettingsResponse> {
  const response = await fetch(`${apiBase}/api/orphan-reference-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OrphanReferenceSettingsResponse>(
    response,
    'Failed to load orphan reference settings',
  )
}

export async function upsertOrphanReferenceSettings(
  accessToken: string,
  payload: UpsertOrphanReferenceSettingsRequest,
): Promise<OrphanReferenceSettingsResponse> {
  const response = await fetch(`${apiBase}/api/orphan-reference-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<OrphanReferenceSettingsResponse>(
    response,
    'Failed to save orphan reference settings',
  )
}

export async function getOrphanReferenceFindings(
  accessToken: string,
  limit = 10,
): Promise<OrphanReferenceFindingsResponse> {
  const response = await fetch(`${apiBase}/api/orphan-reference-settings/findings?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OrphanReferenceFindingsResponse>(
    response,
    'Failed to load orphan reference findings',
  )
}

export async function getOrphanReferenceRuns(
  accessToken: string,
  limit = 10,
): Promise<OrphanReferenceRunsResponse> {
  const response = await fetch(`${apiBase}/api/orphan-reference-settings/runs?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OrphanReferenceRunsResponse>(
    response,
    'Failed to load orphan reference runs',
  )
}

export async function getStaffarrPublicationSettings(
  accessToken: string,
): Promise<StaffarrPublicationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/staffarr-publication-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffarrPublicationSettingsResponse>(
    response,
    'Failed to load StaffArr publication settings',
  )
}

export async function upsertStaffarrPublicationSettings(
  accessToken: string,
  body: UpsertStaffarrPublicationSettingsRequest,
): Promise<StaffarrPublicationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/staffarr-publication-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<StaffarrPublicationSettingsResponse>(
    response,
    'Failed to save StaffArr publication settings',
  )
}

export async function getStaffarrPublicationDeliveries(
  accessToken: string,
  limit = 10,
): Promise<StaffarrPublicationDeliveriesResponse> {
  const response = await fetch(`${apiBase}/api/staffarr-publication-settings/deliveries?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffarrPublicationDeliveriesResponse>(
    response,
    'Failed to load StaffArr publication deliveries',
  )
}

export async function getEventProcessingSettings(
  accessToken: string,
): Promise<EventProcessingSettingsResponse> {
  const response = await fetch(`${apiBase}/api/event-processing-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<EventProcessingSettingsResponse>(
    response,
    'Failed to load event processing settings',
  )
}

export async function upsertEventProcessingSettings(
  accessToken: string,
  body: UpsertEventProcessingSettingsRequest,
): Promise<EventProcessingSettingsResponse> {
  const response = await fetch(`${apiBase}/api/event-processing-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<EventProcessingSettingsResponse>(
    response,
    'Failed to save event processing settings',
  )
}

export async function getTrainingDomainEvents(
  accessToken: string,
  limit = 10,
): Promise<TrainingDomainEventsResponse> {
  const response = await fetch(`${apiBase}/api/event-processing-settings/events?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainingDomainEventsResponse>(
    response,
    'Failed to load training domain events',
  )
}

export async function getPersonTrainingHistory(
  accessToken: string,
  staffarrPersonId: string,
  limit = 25,
): Promise<PersonTrainingHistoryResponse> {
  const response = await fetch(
    `${apiBase}/api/person-training-history?staffarrPersonId=${encodeURIComponent(staffarrPersonId)}&limit=${limit}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<PersonTrainingHistoryResponse>(
    response,
    'Failed to load person training history',
  )
}

export async function getIntegrationSettings(accessToken: string): Promise<IntegrationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/integration-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<IntegrationSettingsResponse>(response, 'Failed to load integration settings')
}

export async function upsertIntegrationSettings(
  accessToken: string,
  body: UpsertIntegrationSettingsRequest,
): Promise<IntegrationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/integration-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<IntegrationSettingsResponse>(response, 'Failed to save integration settings')
}

export async function getIntegrationProbes(accessToken: string): Promise<IntegrationProbesResponse> {
  const response = await fetch(`${apiBase}/api/integration-settings/probes`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<IntegrationProbesResponse>(response, 'Failed to load integration probes')
}

export async function getTrainArrTenantSettings(
  accessToken: string,
): Promise<TrainArrTenantSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/tenant-settings/trainarr`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainArrTenantSettingsResponse>(
    response,
    'Failed to load TrainArr tenant settings',
  )
}

export async function getTrainArrTenantSettingsDefaults(
  accessToken: string,
): Promise<TrainArrTenantSettingsDefaultsResponse> {
  const response = await fetch(`${apiBase}/api/v1/tenant-settings/trainarr/defaults`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainArrTenantSettingsDefaultsResponse>(
    response,
    'Failed to load TrainArr tenant setting defaults',
  )
}

export async function putTrainArrTenantSettings(
  accessToken: string,
  body: UpdateTrainArrTenantSettingsRequest,
): Promise<TrainArrTenantSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/tenant-settings/trainarr`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<TrainArrTenantSettingsResponse>(
    response,
    'Failed to save TrainArr tenant settings',
  )
}

export async function patchTrainArrTenantSettings(
  accessToken: string,
  body: PatchTrainArrTenantSettingsRequest,
): Promise<TrainArrTenantSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/tenant-settings/trainarr`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<TrainArrTenantSettingsResponse>(
    response,
    'Failed to patch TrainArr tenant settings',
  )
}

function buildAuditPackageQuery(options?: { from?: string; to?: string; format?: string }): string {
  const params = new URLSearchParams()
  if (options?.format) {
    params.set('format', options.format)
  }
  if (options?.from) {
    params.set('from', options.from)
  }
  if (options?.to) {
    params.set('to', options.to)
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
  return parseJsonResponse<AuditPackageManifestResponse>(
    response,
    'Failed to load audit package manifest',
  )
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
    throw await toApiError(response, 'Audit package export failed')
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
    throw await toApiError(response, 'Audit package download failed')
  }
  return response.blob()
}

function buildReportQuery(params: Record<string, string | boolean | undefined>): string {
  const search = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === '') {
      continue
    }
    search.set(key, String(value))
  }
  const query = search.toString()
  return query ? `?${query}` : ''
}

export async function getAssignmentReportSummary(
  accessToken: string,
  options?: { status?: string; overdueOnly?: boolean },
): Promise<AssignmentReportSummaryResponse> {
  const response = await fetch(
    `${apiBase}/api/reports/assignments/summary${buildReportQuery({
      status: options?.status,
      overdueOnly: options?.overdueOnly,
    })}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<AssignmentReportSummaryResponse>(
    response,
    'Failed to load assignment report summary',
  )
}

export async function exportAssignmentReportSummaryCsv(
  accessToken: string,
  options?: { status?: string; overdueOnly?: boolean },
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/reports/assignments/summary/export${buildReportQuery({
      status: options?.status,
      overdueOnly: options?.overdueOnly,
    })}`,
    { headers: { Authorization: `Bearer ${accessToken}` } },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Assignment report export failed')
  }
  return response.blob()
}

export async function getQualificationReportSummary(
  accessToken: string,
  options?: { status?: string },
): Promise<QualificationReportSummaryResponse> {
  const response = await fetch(
    `${apiBase}/api/reports/qualifications/summary${buildReportQuery({
      status: options?.status,
    })}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<QualificationReportSummaryResponse>(
    response,
    'Failed to load qualification report summary',
  )
}

export async function exportQualificationReportSummaryCsv(
  accessToken: string,
  options?: { status?: string },
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/reports/qualifications/summary/export${buildReportQuery({
      status: options?.status,
    })}`,
    { headers: { Authorization: `Bearer ${accessToken}` } },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Qualification report export failed')
  }
  return response.blob()
}

export async function getPointInTimeQualificationReport(
  accessToken: string,
  options: { staffarrPersonId: string; qualificationKey: string; actionTask: string; asOfDate?: string },
): Promise<QualificationPointInTimeReportResponse> {
  const response = await fetch(
    `${apiBase}/api/reports/qualifications/point-in-time${buildReportQuery({
      staffarrPersonId: options.staffarrPersonId,
      qualificationKey: options.qualificationKey,
      actionTask: options.actionTask,
      asOfUtc: options.asOfDate ? new Date(`${options.asOfDate}T23:59:59.999Z`).toISOString() : undefined,
    })}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<QualificationPointInTimeReportResponse>(
    response,
    'Failed to load point-in-time qualification report',
  )
}

export async function listQualificationIssuesForReport(
  accessToken: string,
): Promise<QualificationIssueListItemResponse[]> {
  return listQualificationIssues(accessToken)
}

export async function getComplianceReportSummary(
  accessToken: string,
  options?: { attentionOnly?: boolean },
): Promise<ComplianceReportSummaryResponse> {
  const response = await fetch(
    `${apiBase}/api/reports/compliance/summary${buildReportQuery({
      attentionOnly: options?.attentionOnly,
    })}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<ComplianceReportSummaryResponse>(
    response,
    'Failed to load compliance report summary',
  )
}

export async function exportComplianceReportSummaryCsv(
  accessToken: string,
  options?: { attentionOnly?: boolean },
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/reports/compliance/summary/export${buildReportQuery({
      attentionOnly: options?.attentionOnly,
    })}`,
    { headers: { Authorization: `Bearer ${accessToken}` } },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Compliance report export failed')
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

export async function exportTrainingAssignmentsCsv(
  accessToken: string,
  options?: { status?: string },
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/exports/training-assignments${buildReportQuery({
      status: options?.status,
    })}`,
    { headers: { Authorization: `Bearer ${accessToken}` } },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Training assignments export failed')
  }
  return response.blob()
}

export async function exportQualificationIssuesCsv(
  accessToken: string,
  options?: { status?: string },
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/exports/qualification-issues${buildReportQuery({
      status: options?.status,
    })}`,
    { headers: { Authorization: `Bearer ${accessToken}` } },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Qualification issues export failed')
  }
  return response.blob()
}

export async function exportTrainingDefinitionsCsv(accessToken: string): Promise<Blob> {
  const response = await fetch(`${apiBase}/api/exports/training-definitions`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  })
  if (!response.ok) {
    throw await toApiError(response, 'Training definitions export failed')
  }
  return response.blob()
}
