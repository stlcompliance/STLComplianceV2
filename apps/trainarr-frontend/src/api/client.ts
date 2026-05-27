import type {
  CompleteTrainingAssignmentResponse,
  CreateTrainingAssignmentRequest,
  CreateTrainingDefinitionRequest,
  CreateTrainingEvidenceRequest,
  CreateTrainingProgramRequest,
  HandoffSessionResponse,
  StaffarrIncidentRemediationResponse,
  TrainArrMeResponse,
  TrainingAssignmentDetailResponse,
  TrainingAssignmentSummaryResponse,
  TrainingDefinitionResponse,
  TrainingEvidenceResponse,
  TrainingProgramDetailResponse,
  TrainingProgramSummaryResponse,
  SubmitTrainingEvaluationRequest,
  SubmitTrainingSignoffRequest,
  TrainingEvaluationResponse,
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

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new TrainArrApiError(body || `${fallbackMessage} (${response.status})`, response.status, body)
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

export async function getMe(accessToken: string): Promise<TrainArrMeResponse> {
  const response = await fetch(`${apiBase}/api/me`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainArrMeResponse>(response, 'Failed to load profile')
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
    const body = await response.text()
    throw new TrainArrApiError(body || `Failed to remove citation (${response.status})`, response.status, body)
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
    const body = await response.text()
    throw new TrainArrApiError(body || `Failed to remove program citation (${response.status})`, response.status, body)
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
    const body = await response.text()
    throw new TrainArrApiError(
      body || `Failed to remove definition rule pack requirement (${response.status})`,
      response.status,
      body,
    )
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
    const body = await response.text()
    throw new TrainArrApiError(
      body || `Failed to remove program rule pack requirement (${response.status})`,
      response.status,
      body,
    )
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
