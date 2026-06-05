import type {
  CreatePersonRoleAssignmentRequest,
  EffectivePermissionProjectionResponse,
  CreateOrgUnitAssignmentRequest,
  CreateRoleTemplateRequest,
  CreateOrgUnitRequest,
  HandoffSessionResponse,
  ManagerChainEntryResponse,
  OrgUnitAssignmentResponse,
  OrgUnitResponse,
  PermissionHistoryTimelineEntryResponse,
  PermissionTemplateSummaryResponse,
  ProductPermissionCatalogItemResponse,
  PersonRoleAssignmentResponse,
  PersonManagerResponse,
  RoleTemplateResponse,
  MePortalSummaryResponse,
  MyTeamDashboardResponse,
  PersonnelUpdateRequestResponse,
  PersonnelUpdateRequestReviewResponse,
  StaffArrMeResponse,
  StaffArrSessionBootstrapResponse,
  SubmitPersonnelUpdateRequest,
  ReviewPersonnelUpdateRequest,
  SubmitSelfReportedPersonnelIncidentRequest,
  CreateStaffPersonRequest,
  StaffPersonDetailResponse,
  StaffPersonSummaryResponse,
  SubordinateSummaryResponse,
  UpsertPermissionTemplateRequest,
  UpdatePersonManagerRequest,
  UpdateOrgUnitAssignmentRequest,
  UpdateOrgUnitAssignmentStatusRequest,
  UpdateRoleTemplateRequest,
  UpdateOrgUnitRequest,
  UpdateOrgUnitStatusRequest,
  CertificationDefinitionResponse,
  PersonCertificationResponse,
  GrantReadinessOverrideRequest,
  PersonReadinessResponse,
  ReadinessRollupMembersResponse,
  ReadinessRollupSummaryResponse,
  GrantPersonCertificationRequest,
  UpdatePersonCertificationRequest,
  PersonnelIncidentSummaryResponse,
  PersonnelIncidentDetailResponse,
  CreatePersonnelIncidentRequest,
  UpdatePersonnelIncidentStatusRequest,
  CreateIncidentNoteRequest,
  UpdateIncidentNoteStatusRequest,
  CreateIncidentAttachmentRequest,
  RouteIncidentToTrainarrResponse,
  PersonnelNoteSummaryResponse,
  PersonnelNoteDetailResponse,
  CreatePersonnelNoteRequest,
  PersonnelDocumentSummaryResponse,
  PersonnelDocumentDetailResponse,
  CreatePersonnelDocumentRequest,
  PagedResult,
  PersonTimelineEntryResponse,
  AuditPackageManifestResponse,
  AuditPackageExportResponse,
  AuditPackageGenerationJobResponse,
  StaffArrAuditEventExportItem,
  UpdateStaffPersonRequest,
  UpdatePersonEmploymentStatusRequest,
  BulkPersonImportRequest,
  BulkPersonImportResponse,
  PersonExportManifestResponse,
  PersonExportResponse,
  PersonExportFilters,
  PersonExportPresetResponse,
  UpsertPersonExportPresetRequest,
  PersonExportDeliveryNotificationsResponse,
  PendingPersonExportDeliveriesResponse,
  PersonExportDeliveryRunsResponse,
  StaffArrWorkerSettingsResponse,
  UpsertStaffArrWorkerSettingsRequest,
  StaffArrWorkerPendingPreviewResponse,
  StaffArrWorkerRunsResponse,
  PersonExportScheduleResponse,
  UpsertPersonExportScheduleRequest,
  PersonLookupResponse,
  PersonnelHistorySummaryResponse,
  PermissionCheckResponse,
  TrainarrPersonTrainingHistoryResponse,
  WorkforceOnboardingJourneyResponse,
  PersonOffboardingResponse,
  StartPersonOffboardingRequest,
  ExecutePersonOffboardingRequest,
  TrainingAcknowledgementResponse,
  PersonnelReportSummaryResponse,
  ReadinessReportSummaryResponse,
  IncidentReportSummaryResponse,
  CertificationReportSummaryResponse,
  EntityExportManifestResponse,
  LaunchHandoffResponse,
  StaffArrIntegrationLocationResponse,
} from './types'

const apiBase = import.meta.env.VITE_STAFFARR_API_BASE ?? ''

export class StaffArrApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly body: string,
  ) {
    super(message)
    this.name = 'StaffArrApiError'
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

async function toApiError(response: Response, fallbackMessage: string): Promise<StaffArrApiError> {
  const body = await response.text()
  const parsedMessage = extractProblemDetailsMessage(body)
  const message = parsedMessage || body || `${fallbackMessage} (${response.status})`
  return new StaffArrApiError(message, response.status, body)
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

export async function redeemHandoff(handoffCode: string): Promise<HandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/auth/nexarr/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  return parseJsonResponse<HandoffSessionResponse>(response, 'Handoff redeem failed')
}

export async function getMe(accessToken: string): Promise<StaffArrMeResponse> {
  const response = await fetch(`${apiBase}/api/me`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffArrMeResponse>(response, 'Failed to load profile')
}

export async function createLaunchHandoff(
  accessToken: string,
  productKey: string,
  callbackUrl: string,
): Promise<LaunchHandoffResponse> {
  const response = await fetch(`${apiBase}/api/v1/launch/handoff`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ productKey, callbackUrl }),
  })
  return parseJsonResponse<LaunchHandoffResponse>(response, 'Failed to create launch handoff')
}

export async function getSessionBootstrap(
  accessToken: string,
): Promise<StaffArrSessionBootstrapResponse> {
  const response = await fetch(`${apiBase}/api/session`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffArrSessionBootstrapResponse>(
    response,
    'Failed to load session bootstrap',
  )
}

export async function getMePortalSummary(accessToken: string): Promise<MePortalSummaryResponse> {
  const response = await fetch(`${apiBase}/api/me/portal`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MePortalSummaryResponse>(response, 'Failed to load self-service portal')
}

export async function listMyPersonnelUpdateRequests(
  accessToken: string,
  limit = 25,
): Promise<PersonnelUpdateRequestResponse[]> {
  const response = await fetch(`${apiBase}/api/me/update-requests?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonnelUpdateRequestResponse[]>(
    response,
    'Failed to load personnel update requests',
  )
}

export async function submitPersonnelUpdateRequest(
  accessToken: string,
  request: SubmitPersonnelUpdateRequest,
): Promise<PersonnelUpdateRequestResponse> {
  const response = await fetch(`${apiBase}/api/me/update-requests`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonnelUpdateRequestResponse>(
    response,
    'Failed to submit personnel update request',
  )
}

export async function listMyPersonnelIncidents(
  accessToken: string,
  limit = 25,
): Promise<PersonnelIncidentSummaryResponse[]> {
  const response = await fetch(`${apiBase}/api/me/incidents?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonnelIncidentSummaryResponse[]>(
    response,
    'Failed to load your incident reports',
  )
}

export async function submitSelfReportedPersonnelIncident(
  accessToken: string,
  request: SubmitSelfReportedPersonnelIncidentRequest,
): Promise<PersonnelIncidentDetailResponse> {
  const response = await fetch(`${apiBase}/api/me/incidents`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonnelIncidentDetailResponse>(
    response,
    'Failed to submit incident report',
  )
}

export async function getMyTeamDashboard(
  accessToken: string,
  limit = 50,
): Promise<MyTeamDashboardResponse> {
  const response = await fetch(`${apiBase}/api/me/team?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MyTeamDashboardResponse>(response, 'Failed to load my team dashboard')
}

export async function reviewMyTeamPersonnelUpdateRequest(
  accessToken: string,
  requestId: string,
  request: ReviewPersonnelUpdateRequest,
): Promise<PersonnelUpdateRequestReviewResponse> {
  const response = await fetch(`${apiBase}/api/me/team/update-requests/${requestId}/review`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonnelUpdateRequestReviewResponse>(
    response,
    'Failed to review personnel update request',
  )
}

export async function reviewPersonnelUpdateRequest(
  accessToken: string,
  requestId: string,
  request: ReviewPersonnelUpdateRequest,
): Promise<PersonnelUpdateRequestReviewResponse> {
  const response = await fetch(`${apiBase}/api/personnel-update-requests/${requestId}/review`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonnelUpdateRequestReviewResponse>(
    response,
    'Failed to review personnel update request',
  )
}

export async function getPeople(accessToken: string): Promise<StaffPersonSummaryResponse[]> {
  const response = await fetch(`${apiBase}/api/people`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffPersonSummaryResponse[]>(response, 'Failed to load people directory')
}

export async function getPerson(accessToken: string, personId: string): Promise<StaffPersonDetailResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffPersonDetailResponse>(response, 'Failed to load person profile')
}

export async function createPerson(
  accessToken: string,
  request: CreateStaffPersonRequest,
): Promise<StaffPersonDetailResponse> {
  const response = await fetch(`${apiBase}/api/people`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<StaffPersonDetailResponse>(response, 'Failed to create person')
}

export async function updatePerson(
  accessToken: string,
  personId: string,
  request: UpdateStaffPersonRequest,
): Promise<StaffPersonDetailResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<StaffPersonDetailResponse>(response, 'Failed to update person profile')
}

export async function updatePersonEmploymentStatus(
  accessToken: string,
  personId: string,
  request: UpdatePersonEmploymentStatusRequest,
): Promise<StaffPersonDetailResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/employment-status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<StaffPersonDetailResponse>(response, 'Failed to update employment status')
}

export async function importPeopleBulk(
  accessToken: string,
  request: BulkPersonImportRequest,
): Promise<BulkPersonImportResponse> {
  const response = await fetch(`${apiBase}/api/people/import`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<BulkPersonImportResponse>(response, 'Failed to import people')
}

function buildPeopleExportQuery(filters?: PersonExportFilters, format?: string): string {
  const params = new URLSearchParams()
  if (format) {
    params.set('format', format)
  }
  if (filters?.employmentStatus) {
    params.set('employmentStatus', filters.employmentStatus)
  }
  if (filters?.orgUnitId) {
    params.set('orgUnitId', filters.orgUnitId)
  }
  const query = params.toString()
  return query ? `?${query}` : ''
}

export async function getPeopleExportManifest(accessToken: string): Promise<PersonExportManifestResponse> {
  const response = await fetch(`${apiBase}/api/people/export/manifest`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonExportManifestResponse>(response, 'Failed to load people export manifest')
}

export async function exportPeopleJson(
  accessToken: string,
  filters?: PersonExportFilters,
): Promise<PersonExportResponse> {
  const response = await fetch(
    `${apiBase}/api/people/export${buildPeopleExportQuery(filters, 'json')}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<PersonExportResponse>(response, 'Failed to export people JSON')
}

export async function exportPeopleCsv(accessToken: string, filters?: PersonExportFilters): Promise<string> {
  const response = await fetch(
    `${apiBase}/api/people/export${buildPeopleExportQuery(filters, 'csv')}`,
    { headers: authHeaders(accessToken) },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Failed to export people CSV')
  }
  return response.text()
}

export async function exportPeopleZip(accessToken: string, filters?: PersonExportFilters): Promise<Blob> {
  const response = await fetch(`${apiBase}/api/people/export${buildPeopleExportQuery(filters)}`, {
    headers: authHeaders(accessToken),
  })
  if (!response.ok) {
    throw await toApiError(response, 'Failed to export people ZIP')
  }
  return response.blob()
}

export async function getPersonExportPreset(accessToken: string): Promise<PersonExportPresetResponse | null> {
  const response = await fetch(`${apiBase}/api/people/export/preset`, {
    headers: authHeaders(accessToken),
  })
  if (response.status === 404) {
    return null
  }
  return parseJsonResponse<PersonExportPresetResponse>(response, 'Failed to load tenant export preset')
}

export async function upsertPersonExportPreset(
  accessToken: string,
  request: UpsertPersonExportPresetRequest,
): Promise<PersonExportPresetResponse> {
  const response = await fetch(`${apiBase}/api/people/export/preset`, {
    method: 'PUT',
    headers: {
      ...authHeaders(accessToken),
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonExportPresetResponse>(response, 'Failed to save tenant export preset')
}

export async function getPersonExportSchedule(accessToken: string): Promise<PersonExportScheduleResponse> {
  const response = await fetch(`${apiBase}/api/people/export/schedule`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonExportScheduleResponse>(response, 'Failed to load tenant export schedule')
}

export async function upsertPersonExportSchedule(
  accessToken: string,
  request: UpsertPersonExportScheduleRequest,
): Promise<PersonExportScheduleResponse> {
  const response = await fetch(`${apiBase}/api/people/export/schedule`, {
    method: 'PUT',
    headers: {
      ...authHeaders(accessToken),
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonExportScheduleResponse>(response, 'Failed to save tenant export schedule')
}

export async function getPersonExportDeliveryNotifications(
  accessToken: string,
  limit = 5,
): Promise<PersonExportDeliveryNotificationsResponse> {
  const response = await fetch(`${apiBase}/api/people/export/delivery-notifications?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonExportDeliveryNotificationsResponse>(
    response,
    'Failed to load export delivery notifications',
  )
}

export async function getPersonExportDeliveryPending(
  accessToken: string,
): Promise<PendingPersonExportDeliveriesResponse> {
  const response = await fetch(`${apiBase}/api/people/export/delivery-pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingPersonExportDeliveriesResponse>(
    response,
    'Failed to load export delivery pending preview',
  )
}

export async function getPersonExportDeliveryRuns(
  accessToken: string,
  limit = 5,
): Promise<PersonExportDeliveryRunsResponse> {
  const response = await fetch(`${apiBase}/api/people/export/delivery-runs?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonExportDeliveryRunsResponse>(
    response,
    'Failed to load export delivery runs',
  )
}

export async function getStaffArrWorkerSettings(
  accessToken: string,
  workerKey: string,
): Promise<StaffArrWorkerSettingsResponse> {
  const response = await fetch(`${apiBase}/api/worker-admin/${encodeURIComponent(workerKey)}/settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffArrWorkerSettingsResponse>(
    response,
    `Failed to load ${workerKey} worker settings`,
  )
}

export async function upsertStaffArrWorkerSettings(
  accessToken: string,
  workerKey: string,
  request: UpsertStaffArrWorkerSettingsRequest,
): Promise<StaffArrWorkerSettingsResponse> {
  const response = await fetch(`${apiBase}/api/worker-admin/${encodeURIComponent(workerKey)}/settings`, {
    method: 'PUT',
    headers: {
      ...authHeaders(accessToken),
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })
  return parseJsonResponse<StaffArrWorkerSettingsResponse>(
    response,
    `Failed to save ${workerKey} worker settings`,
  )
}

export async function getStaffArrWorkerPendingPreview(
  accessToken: string,
  workerKey: string,
): Promise<StaffArrWorkerPendingPreviewResponse> {
  const response = await fetch(`${apiBase}/api/worker-admin/${encodeURIComponent(workerKey)}/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffArrWorkerPendingPreviewResponse>(
    response,
    `Failed to load ${workerKey} pending preview`,
  )
}

export async function getStaffArrWorkerRuns(
  accessToken: string,
  workerKey: string,
  limit = 5,
): Promise<StaffArrWorkerRunsResponse> {
  const response = await fetch(`${apiBase}/api/worker-admin/${encodeURIComponent(workerKey)}/runs?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffArrWorkerRunsResponse>(response, `Failed to load ${workerKey} worker runs`)
}

export async function getOrgUnits(accessToken: string): Promise<OrgUnitResponse[]> {
  const response = await fetch(`${apiBase}/api/org-units`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OrgUnitResponse[]>(response, 'Failed to load org units')
}

export async function listSiteLocations(
  accessToken: string,
  siteOrgUnitId: string,
): Promise<StaffArrIntegrationLocationResponse[]> {
  const response = await fetch(
    `${apiBase}/api/v1/integrations/sites/${siteOrgUnitId}/locations`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<StaffArrIntegrationLocationResponse[]>(
    response,
    'Failed to load site locations',
  )
}

export async function createOrgUnit(accessToken: string, request: CreateOrgUnitRequest): Promise<OrgUnitResponse> {
  const response = await fetch(`${apiBase}/api/org-units`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<OrgUnitResponse>(response, 'Failed to create org unit')
}

export async function updateOrgUnit(
  accessToken: string,
  orgUnitId: string,
  request: UpdateOrgUnitRequest,
): Promise<OrgUnitResponse> {
  const response = await fetch(`${apiBase}/api/org-units/${orgUnitId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<OrgUnitResponse>(response, 'Failed to update org unit')
}

export async function updateOrgUnitStatus(
  accessToken: string,
  orgUnitId: string,
  request: UpdateOrgUnitStatusRequest,
): Promise<OrgUnitResponse> {
  const response = await fetch(`${apiBase}/api/org-units/${orgUnitId}/status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<OrgUnitResponse>(response, 'Failed to update org unit status')
}

export async function getPersonOrgAssignments(
  accessToken: string,
  personId: string,
): Promise<OrgUnitAssignmentResponse[]> {
  const response = await fetch(`${apiBase}/api/people/${personId}/org-assignments`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OrgUnitAssignmentResponse[]>(response, 'Failed to load org assignments')
}

export async function createPersonOrgAssignment(
  accessToken: string,
  personId: string,
  request: CreateOrgUnitAssignmentRequest,
): Promise<OrgUnitAssignmentResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/org-assignments`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<OrgUnitAssignmentResponse>(response, 'Failed to create org assignment')
}

export async function updatePersonOrgAssignment(
  accessToken: string,
  personId: string,
  assignmentId: string,
  request: UpdateOrgUnitAssignmentRequest,
): Promise<OrgUnitAssignmentResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/org-assignments/${assignmentId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<OrgUnitAssignmentResponse>(response, 'Failed to update org assignment')
}

export async function updatePersonOrgAssignmentStatus(
  accessToken: string,
  personId: string,
  assignmentId: string,
  request: UpdateOrgUnitAssignmentStatusRequest,
): Promise<OrgUnitAssignmentResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/org-assignments/${assignmentId}/status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<OrgUnitAssignmentResponse>(response, 'Failed to update org assignment status')
}

export async function updatePersonManager(
  accessToken: string,
  personId: string,
  request: UpdatePersonManagerRequest,
): Promise<PersonManagerResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/manager`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonManagerResponse>(response, 'Failed to update manager')
}

export async function getManagerChain(
  accessToken: string,
  personId: string,
): Promise<ManagerChainEntryResponse[]> {
  const response = await fetch(`${apiBase}/api/people/${personId}/manager-chain`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ManagerChainEntryResponse[]>(response, 'Failed to load manager chain')
}

export async function getSubordinates(
  accessToken: string,
  personId: string,
  includeIndirect = true,
  limit = 200,
): Promise<SubordinateSummaryResponse[]> {
  const response = await fetch(
    `${apiBase}/api/people/${personId}/subordinates?includeIndirect=${includeIndirect ? 'true' : 'false'}&limit=${limit}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<SubordinateSummaryResponse[]>(response, 'Failed to load subordinates')
}

export async function getSubordinateDetail(
  accessToken: string,
  personId: string,
  subordinatePersonId: string,
): Promise<SubordinateSummaryResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/subordinates/${subordinatePersonId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SubordinateSummaryResponse>(response, 'Failed to load subordinate detail')
}

export async function getPermissionTemplates(accessToken: string): Promise<PermissionTemplateSummaryResponse[]> {
  const response = await fetch(`${apiBase}/api/permissions`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PermissionTemplateSummaryResponse[]>(response, 'Failed to load permission templates')
}

export async function getProductPermissionCatalog(
  accessToken: string,
  productKey?: string,
): Promise<ProductPermissionCatalogItemResponse[]> {
  const query = productKey?.trim() ? `?productKey=${encodeURIComponent(productKey.trim())}` : ''
  const response = await fetch(`${apiBase}/api/permissions/product-catalog${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProductPermissionCatalogItemResponse[]>(
    response,
    'Failed to load product permission catalog',
  )
}

export async function upsertPermissionTemplate(
  accessToken: string,
  request: UpsertPermissionTemplateRequest,
): Promise<PermissionTemplateSummaryResponse> {
  const response = await fetch(`${apiBase}/api/permissions`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PermissionTemplateSummaryResponse>(response, 'Failed to upsert permission template')
}

export async function getRoleTemplates(accessToken: string): Promise<RoleTemplateResponse[]> {
  const response = await fetch(`${apiBase}/api/roles`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RoleTemplateResponse[]>(response, 'Failed to load role templates')
}

export async function createRoleTemplate(
  accessToken: string,
  request: CreateRoleTemplateRequest,
): Promise<RoleTemplateResponse> {
  const response = await fetch(`${apiBase}/api/roles`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<RoleTemplateResponse>(response, 'Failed to create role template')
}

export async function updateRoleTemplate(
  accessToken: string,
  roleTemplateId: string,
  request: UpdateRoleTemplateRequest,
): Promise<RoleTemplateResponse> {
  const response = await fetch(`${apiBase}/api/roles/${roleTemplateId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<RoleTemplateResponse>(response, 'Failed to update role template')
}

export async function getPersonRoleAssignments(
  accessToken: string,
  personId: string,
): Promise<PersonRoleAssignmentResponse[]> {
  const response = await fetch(`${apiBase}/api/people/${personId}/role-assignments`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonRoleAssignmentResponse[]>(response, 'Failed to load role assignments')
}

export async function createPersonRoleAssignment(
  accessToken: string,
  personId: string,
  request: CreatePersonRoleAssignmentRequest,
): Promise<PersonRoleAssignmentResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/role-assignments`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonRoleAssignmentResponse>(response, 'Failed to create role assignment')
}

export async function updatePersonRoleAssignmentStatus(
  accessToken: string,
  personId: string,
  assignmentId: string,
  status: 'active' | 'inactive',
): Promise<PersonRoleAssignmentResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/role-assignments/${assignmentId}/status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ status }),
  })
  return parseJsonResponse<PersonRoleAssignmentResponse>(response, 'Failed to update role assignment status')
}

export async function getEffectivePermissions(
  accessToken: string,
  personId: string,
): Promise<EffectivePermissionProjectionResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/permissions/effective`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<EffectivePermissionProjectionResponse>(response, 'Failed to load effective permissions')
}

export async function getPermissionHistoryTimeline(
  accessToken: string,
  personId: string,
  limit = 100,
): Promise<PermissionHistoryTimelineEntryResponse[]> {
  const response = await fetch(`${apiBase}/api/people/${personId}/permissions/history?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PermissionHistoryTimelineEntryResponse[]>(response, 'Failed to load permission history')
}

export async function checkPersonPermissions(
  accessToken: string,
  personId: string,
  permissionKeys: string[],
): Promise<PermissionCheckResponse> {
  const params = new URLSearchParams()
  permissionKeys.forEach((permissionKey) => {
    if (permissionKey.trim()) {
      params.append('permissionKey', permissionKey.trim())
    }
  })
  const response = await fetch(`${apiBase}/api/people/${personId}/permissions/check?${params.toString()}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PermissionCheckResponse>(response, 'Failed to check permissions')
}

export async function getPersonTimeline(
  accessToken: string,
  personId: string,
  page = 1,
  pageSize = 50,
  category?: string,
): Promise<PagedResult<PersonTimelineEntryResponse>> {
  const params = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
  })
  if (category?.trim()) {
    params.set('category', category.trim())
  }
  const response = await fetch(`${apiBase}/api/people/${personId}/timeline?${params}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PagedResult<PersonTimelineEntryResponse>>(response, 'Failed to load person timeline')
}

export async function getPersonHistory(
  accessToken: string,
  personId: string,
  page = 1,
  pageSize = 50,
): Promise<PagedResult<PersonTimelineEntryResponse>> {
  const response = await fetch(
    `${apiBase}/api/people/${personId}/person-history?page=${page}&pageSize=${pageSize}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<PagedResult<PersonTimelineEntryResponse>>(response, 'Failed to load person history')
}

export async function getPersonHistorySummary(
  accessToken: string,
  personId: string,
): Promise<PersonnelHistorySummaryResponse> {
  const response = await fetch(`${apiBase}/api/person-history/summary?personId=${personId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonnelHistorySummaryResponse>(response, 'Failed to load person history summary')
}

export async function getPersonTrainarrTrainingHistory(
  accessToken: string,
  personId: string,
  limit = 25,
): Promise<TrainarrPersonTrainingHistoryResponse> {
  const response = await fetch(
    `${apiBase}/api/people/${personId}/trainarr-training-history?limit=${limit}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<TrainarrPersonTrainingHistoryResponse>(
    response,
    'Failed to load TrainArr training history',
  )
}

export async function getWorkforceOnboardingJourney(
  accessToken: string,
  personId: string,
): Promise<WorkforceOnboardingJourneyResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/workforce-onboarding-journey`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WorkforceOnboardingJourneyResponse>(
    response,
    'Failed to load workforce onboarding journey',
  )
}

export async function getPersonOffboarding(
  accessToken: string,
  personId: string,
): Promise<PersonOffboardingResponse | null> {
  const response = await fetch(`${apiBase}/api/people/${personId}/offboarding`, {
    headers: authHeaders(accessToken),
  })
  if (response.status === 404) {
    return null
  }

  return parseJsonResponse<PersonOffboardingResponse>(response, 'Failed to load person offboarding')
}

export async function startPersonOffboarding(
  accessToken: string,
  request: StartPersonOffboardingRequest,
): Promise<PersonOffboardingResponse> {
  const response = await fetch(`${apiBase}/api/offboarding`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonOffboardingResponse>(response, 'Failed to start offboarding')
}

export async function executePersonOffboarding(
  accessToken: string,
  offboardingId: string,
  request: ExecutePersonOffboardingRequest,
): Promise<PersonOffboardingResponse> {
  const response = await fetch(`${apiBase}/api/offboarding/${offboardingId}/execute`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonOffboardingResponse>(response, 'Failed to execute offboarding')
}

export async function getCertificationDefinitions(
  accessToken: string,
): Promise<CertificationDefinitionResponse[]> {
  const response = await fetch(`${apiBase}/api/certifications`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CertificationDefinitionResponse[]>(response, 'Failed to load certification definitions')
}

export async function getPersonCertifications(
  accessToken: string,
  personId: string,
): Promise<PersonCertificationResponse[]> {
  const response = await fetch(`${apiBase}/api/people/${personId}/certifications`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonCertificationResponse[]>(response, 'Failed to load person certifications')
}

export async function grantPersonCertification(
  accessToken: string,
  personId: string,
  request: GrantPersonCertificationRequest,
): Promise<PersonCertificationResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/certifications`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonCertificationResponse>(response, 'Failed to grant certification')
}

export async function updatePersonCertification(
  accessToken: string,
  personId: string,
  personCertificationId: string,
  request: UpdatePersonCertificationRequest,
): Promise<PersonCertificationResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/certifications/${personCertificationId}`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonCertificationResponse>(response, 'Failed to update certification')
}

export async function getPersonReadiness(
  accessToken: string,
  personId: string,
): Promise<PersonReadinessResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/readiness`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonReadinessResponse>(response, 'Failed to load person readiness')
}

export async function getPersonLookup(
  accessToken: string,
  personId: string,
): Promise<PersonLookupResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/lookup`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonLookupResponse>(response, 'Failed to load person lookup')
}

export async function getTeamReadinessRollups(
  accessToken: string,
  siteOrgUnitId?: string,
): Promise<ReadinessRollupSummaryResponse[]> {
  const query = siteOrgUnitId ? `?siteOrgUnitId=${encodeURIComponent(siteOrgUnitId)}` : ''
  const response = await fetch(`${apiBase}/api/readiness-rollups/teams${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ReadinessRollupSummaryResponse[]>(response, 'Failed to load team readiness rollups')
}

export async function getSiteReadinessRollups(
  accessToken: string,
): Promise<ReadinessRollupSummaryResponse[]> {
  const response = await fetch(`${apiBase}/api/readiness-rollups/sites`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ReadinessRollupSummaryResponse[]>(response, 'Failed to load site readiness rollups')
}

export async function getReadinessRollupMembers(
  accessToken: string,
  scopeType: 'team' | 'site',
  orgUnitId: string,
  readinessStatus?: 'ready' | 'not_ready' | 'missing_certifications',
): Promise<ReadinessRollupMembersResponse> {
  const params = new URLSearchParams()
  if (readinessStatus) {
    params.set('readinessStatus', readinessStatus)
  }

  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(
    `${apiBase}/api/readiness-rollups/${scopeType === 'team' ? 'teams' : 'sites'}/${orgUnitId}/members${query}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<ReadinessRollupMembersResponse>(
    response,
    'Failed to load readiness rollup members',
  )
}

export async function grantPersonReadinessOverride(
  accessToken: string,
  personId: string,
  request: GrantReadinessOverrideRequest,
): Promise<PersonReadinessResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/readiness/override`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonReadinessResponse>(response, 'Failed to grant readiness override')
}

export async function clearPersonReadinessOverride(
  accessToken: string,
  personId: string,
): Promise<PersonReadinessResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/readiness/override`, {
    method: 'DELETE',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonReadinessResponse>(response, 'Failed to clear readiness override')
}

export async function listTrainingAcknowledgements(
  accessToken: string,
  personId?: string,
  status?: string,
): Promise<TrainingAcknowledgementResponse[]> {
  const params = new URLSearchParams()
  if (personId) {
    params.set('personId', personId)
  }
  if (status) {
    params.set('status', status)
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/training-acknowledgements${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainingAcknowledgementResponse[]>(
    response,
    'Failed to load training acknowledgements',
  )
}

export async function acknowledgeTrainingAssignment(
  accessToken: string,
  acknowledgementId: string,
): Promise<TrainingAcknowledgementResponse> {
  const response = await fetch(`${apiBase}/api/training-acknowledgements/${acknowledgementId}/acknowledge`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TrainingAcknowledgementResponse>(
    response,
    'Failed to acknowledge training assignment',
  )
}

export async function listPersonnelIncidents(
  accessToken: string,
  personId?: string,
): Promise<PersonnelIncidentSummaryResponse[]> {
  const query = personId ? `?personId=${encodeURIComponent(personId)}` : ''
  const response = await fetch(`${apiBase}/api/incidents${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonnelIncidentSummaryResponse[]>(response, 'Failed to load incidents')
}

export async function getPersonnelIncident(
  accessToken: string,
  incidentId: string,
): Promise<PersonnelIncidentDetailResponse> {
  const response = await fetch(`${apiBase}/api/incidents/${incidentId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonnelIncidentDetailResponse>(response, 'Failed to load incident')
}

export async function createPersonnelIncident(
  accessToken: string,
  request: CreatePersonnelIncidentRequest,
): Promise<PersonnelIncidentDetailResponse> {
  const response = await fetch(`${apiBase}/api/incidents`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonnelIncidentDetailResponse>(response, 'Failed to create incident')
}

export async function routePersonnelIncidentToTrainarr(
  accessToken: string,
  incidentId: string,
): Promise<RouteIncidentToTrainarrResponse> {
  const response = await fetch(`${apiBase}/api/incidents/${incidentId}/route-to-trainarr`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RouteIncidentToTrainarrResponse>(
    response,
    'Failed to route incident to TrainArr',
  )
}

export async function updatePersonnelIncidentStatus(
  accessToken: string,
  incidentId: string,
  request: UpdatePersonnelIncidentStatusRequest,
): Promise<PersonnelIncidentDetailResponse> {
  const response = await fetch(`${apiBase}/api/incidents/${incidentId}/status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonnelIncidentDetailResponse>(response, 'Failed to update incident status')
}

export async function createIncidentNote(
  accessToken: string,
  incidentId: string,
  request: CreateIncidentNoteRequest,
): Promise<PersonnelIncidentDetailResponse> {
  const response = await fetch(`${apiBase}/api/incidents/${incidentId}/notes`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonnelIncidentDetailResponse>(response, 'Failed to create incident note')
}

export async function updateIncidentNoteStatus(
  accessToken: string,
  incidentId: string,
  noteId: string,
  request: UpdateIncidentNoteStatusRequest,
): Promise<PersonnelIncidentDetailResponse> {
  const response = await fetch(`${apiBase}/api/incidents/${incidentId}/notes/${noteId}/status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonnelIncidentDetailResponse>(
    response,
    'Failed to update incident note status',
  )
}

export async function createIncidentAttachment(
  accessToken: string,
  incidentId: string,
  request: CreateIncidentAttachmentRequest,
): Promise<PersonnelIncidentDetailResponse> {
  const response = await fetch(`${apiBase}/api/incidents/${incidentId}/attachments`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonnelIncidentDetailResponse>(
    response,
    'Failed to upload incident attachment',
  )
}

export async function getIncidentAttachmentContent(
  accessToken: string,
  incidentId: string,
  attachmentId: string,
): Promise<{ blob: Blob; fileName: string; contentType: string }> {
  const response = await fetch(
    `${apiBase}/api/incidents/${incidentId}/attachments/${attachmentId}/content`,
    {
      headers: authHeaders(accessToken),
    },
  )

  if (!response.ok) {
    throw await toApiError(response, 'Failed to download incident attachment')
  }

  const fileName =
    response.headers.get('content-disposition')?.match(/filename="?([^"]+)"?/i)?.[1] ??
    'attachment.bin'
  return {
    blob: await response.blob(),
    fileName,
    contentType: response.headers.get('content-type') ?? 'application/octet-stream',
  }
}

export async function listPersonnelNotes(
  accessToken: string,
  personId: string,
): Promise<PersonnelNoteSummaryResponse[]> {
  const response = await fetch(`${apiBase}/api/people/${personId}/notes`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonnelNoteSummaryResponse[]>(response, 'Failed to load personnel notes')
}

export async function getPersonnelNote(
  accessToken: string,
  personId: string,
  noteId: string,
): Promise<PersonnelNoteDetailResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/notes/${noteId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonnelNoteDetailResponse>(response, 'Failed to load personnel note')
}

export async function createPersonnelNote(
  accessToken: string,
  personId: string,
  request: CreatePersonnelNoteRequest,
): Promise<PersonnelNoteDetailResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/notes`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonnelNoteDetailResponse>(response, 'Failed to create personnel note')
}

export async function listPersonnelDocuments(
  accessToken: string,
  personId: string,
): Promise<PersonnelDocumentSummaryResponse[]> {
  const response = await fetch(`${apiBase}/api/people/${personId}/documents`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonnelDocumentSummaryResponse[]>(response, 'Failed to load personnel documents')
}

export async function getPersonnelDocument(
  accessToken: string,
  personId: string,
  documentId: string,
): Promise<PersonnelDocumentDetailResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/documents/${documentId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PersonnelDocumentDetailResponse>(response, 'Failed to load personnel document')
}

export async function createPersonnelDocument(
  accessToken: string,
  personId: string,
  request: CreatePersonnelDocumentRequest,
): Promise<PersonnelDocumentDetailResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/documents`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PersonnelDocumentDetailResponse>(response, 'Failed to upload personnel document')
}

export function personnelDocumentContentUrl(personId: string, documentId: string): string {
  return `${apiBase}/api/people/${personId}/documents/${documentId}/content`
}

function buildAuditPackageQuery(
  options?: import('./types').AuditPackageScope & {
    format?: string
    page?: number
    pageSize?: number
  },
): string {
  const params = new URLSearchParams()
  if (options?.from) {
    params.set('from', `${options.from}T00:00:00.000Z`)
  }
  if (options?.to) {
    params.set('to', `${options.to}T23:59:59.999Z`)
  }
  if (options?.format) {
    params.set('format', options.format)
  }
  if (options?.action) {
    params.set('action', options.action)
  }
  if (options?.result) {
    params.set('result', options.result)
  }
  if (options?.targetType) {
    params.set('targetType', options.targetType)
  }
  if (options?.actorUserId) {
    params.set('actorUserId', options.actorUserId)
  }
  if (options?.page != null) {
    params.set('page', String(options.page))
  }
  if (options?.pageSize != null) {
    params.set('pageSize', String(options.pageSize))
  }
  const query = params.toString()
  return query ? `?${query}` : ''
}

function auditPackageJobBody(scope: import('./types').AuditPackageScope & { format: string }) {
  return {
    format: scope.format,
    from: scope.from ? `${scope.from}T00:00:00.000Z` : undefined,
    to: scope.to ? `${scope.to}T23:59:59.999Z` : undefined,
    action: scope.action,
    result: scope.result,
    targetType: scope.targetType,
    actorUserId: scope.actorUserId,
  }
}

export async function getAuditPackageFilterOptions(
  accessToken: string,
): Promise<import('./types').AuditPackageFilterOptions> {
  const response = await fetch(`${apiBase}/api/audit-packages/filter-options`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse(response, 'Failed to load audit filter options')
}

export async function getAuditPackageExportSummary(
  accessToken: string,
  scope?: import('./types').AuditPackageScope,
): Promise<import('./types').AuditPackageExportSummary> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/summary${buildAuditPackageQuery(scope)}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse(response, 'Failed to load audit export summary')
}

export async function getAuditPackageTimeline(
  accessToken: string,
  options?: import('./types').AuditPackageScope & { page?: number; pageSize?: number },
): Promise<PagedResult<StaffArrAuditEventExportItem>> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/timeline${buildAuditPackageQuery(options)}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<PagedResult<StaffArrAuditEventExportItem>>(
    response,
    'Failed to load audit timeline',
  )
}

export async function getAuditPackageManifest(
  accessToken: string,
): Promise<AuditPackageManifestResponse> {
  const response = await fetch(`${apiBase}/api/audit-packages/manifest`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AuditPackageManifestResponse>(response, 'Failed to load audit package manifest')
}

export async function exportAuditPackageCsv(
  accessToken: string,
  scope?: import('./types').AuditPackageScope,
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/export${buildAuditPackageQuery({ ...scope, format: 'csv' })}`,
    { headers: { Authorization: `Bearer ${accessToken}` } },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Audit events CSV export failed')
  }
  return response.blob()
}

export async function exportAuditPackageZip(
  accessToken: string,
  options?: import('./types').AuditPackageScope,
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
  options?: import('./types').AuditPackageScope,
): Promise<AuditPackageExportResponse> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/export${buildAuditPackageQuery({ ...options, format: 'json' })}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<AuditPackageExportResponse>(response, 'Failed to export audit package JSON')
}

export async function createAuditPackageGenerationJob(
  accessToken: string,
  options: import('./types').AuditPackageScope & { format: 'zip' | 'json' },
): Promise<AuditPackageGenerationJobResponse> {
  const response = await fetch(`${apiBase}/api/audit-packages/jobs`, {
    method: 'POST',
    headers: {
      ...authHeaders(accessToken),
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(auditPackageJobBody(options)),
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

export async function getPersonnelReportSummary(
  accessToken: string,
  options?: { employmentStatus?: string },
): Promise<PersonnelReportSummaryResponse> {
  const response = await fetch(
    `${apiBase}/api/reports/personnel/summary${buildReportQuery({
      employmentStatus: options?.employmentStatus,
    })}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<PersonnelReportSummaryResponse>(
    response,
    'Failed to load personnel report summary',
  )
}

export async function exportPersonnelReportSummaryCsv(
  accessToken: string,
  options?: { employmentStatus?: string },
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/reports/personnel/summary/export${buildReportQuery({
      employmentStatus: options?.employmentStatus,
    })}`,
    { headers: authHeaders(accessToken) },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Personnel report export failed')
  }
  return response.blob()
}

export async function getReadinessReportSummary(
  accessToken: string,
  options?: { scopeType?: string; attentionOnly?: boolean },
): Promise<ReadinessReportSummaryResponse> {
  const response = await fetch(
    `${apiBase}/api/reports/readiness/summary${buildReportQuery({
      scopeType: options?.scopeType,
      attentionOnly: options?.attentionOnly,
    })}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<ReadinessReportSummaryResponse>(
    response,
    'Failed to load readiness report summary',
  )
}

export async function exportReadinessReportSummaryCsv(
  accessToken: string,
  options?: { scopeType?: string; attentionOnly?: boolean },
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/reports/readiness/summary/export${buildReportQuery({
      scopeType: options?.scopeType,
      attentionOnly: options?.attentionOnly,
    })}`,
    { headers: authHeaders(accessToken) },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Readiness report export failed')
  }
  return response.blob()
}

export async function getIncidentReportSummary(
  accessToken: string,
  options?: { status?: string; severity?: string; openOnly?: boolean },
): Promise<IncidentReportSummaryResponse> {
  const response = await fetch(
    `${apiBase}/api/reports/incidents/summary${buildReportQuery({
      status: options?.status,
      severity: options?.severity,
      openOnly: options?.openOnly,
    })}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<IncidentReportSummaryResponse>(
    response,
    'Failed to load incident report summary',
  )
}

export async function exportIncidentReportSummaryCsv(
  accessToken: string,
  options?: { status?: string; severity?: string; openOnly?: boolean },
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/reports/incidents/summary/export${buildReportQuery({
      status: options?.status,
      severity: options?.severity,
      openOnly: options?.openOnly,
    })}`,
    { headers: authHeaders(accessToken) },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Incident report export failed')
  }
  return response.blob()
}

export async function getCertificationReportSummary(
  accessToken: string,
  options?: { missingOnly?: boolean; expiringOnly?: boolean },
): Promise<CertificationReportSummaryResponse> {
  const response = await fetch(
    `${apiBase}/api/reports/certifications/summary${buildReportQuery({
      missingOnly: options?.missingOnly,
      expiringOnly: options?.expiringOnly,
    })}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<CertificationReportSummaryResponse>(
    response,
    'Failed to load certification report summary',
  )
}

export async function exportCertificationReportSummaryCsv(
  accessToken: string,
  options?: { missingOnly?: boolean; expiringOnly?: boolean },
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/reports/certifications/summary/export${buildReportQuery({
      missingOnly: options?.missingOnly,
      expiringOnly: options?.expiringOnly,
    })}`,
    { headers: authHeaders(accessToken) },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Certification report export failed')
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

export async function exportBulkPeopleCsv(
  accessToken: string,
  options?: { employmentStatus?: string },
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/exports/people${buildReportQuery({
      employmentStatus: options?.employmentStatus,
    })}`,
    { headers: authHeaders(accessToken) },
  )
  if (!response.ok) {
    throw await toApiError(response, 'People bulk export failed')
  }
  return response.blob()
}

export async function exportBulkPersonnelIncidentsCsv(
  accessToken: string,
  options?: { status?: string },
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/exports/personnel-incidents${buildReportQuery({
      status: options?.status,
    })}`,
    { headers: authHeaders(accessToken) },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Personnel incidents bulk export failed')
  }
  return response.blob()
}

export async function exportBulkPersonCertificationsCsv(
  accessToken: string,
  options?: { status?: string },
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/exports/person-certifications${buildReportQuery({
      status: options?.status,
    })}`,
    { headers: authHeaders(accessToken) },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Person certifications export failed')
  }
  return response.blob()
}
