import type {
  EffectivePermissionProjectionResponse,
  CreateOrgUnitAssignmentRequest,
  CreateOrgUnitRequest,
  CreateInternalLocationRequest,
  HandoffSessionResponse,
  ManagerChainEntryResponse,
  OrgUnitAssignmentResponse,
  OrgUnitResponse,
  InternalLocationResponse,
  PermissionCatalogResponse,
  ProductPermissionCatalogItemResponse,
  StaffPersonRoleAssignmentResponse,
  PersonManagerResponse,
  StaffRoleSummaryResponse,
  StaffRoleDetailResponse,
  MePortalSummaryResponse,
  MyTeamDashboardResponse,
  StaffArrPersonIntegrationSummaryResponse,
  PersonnelUpdateRequestResponse,
  PersonnelUpdateRequestReviewResponse,
  StaffArrFieldsetResponse,
  StaffArrMeResponse,
  StaffArrSessionBootstrapResponse,
  SubmitPersonnelUpdateRequest,
  ReviewPersonnelUpdateRequest,
  SubmitSelfReportedPersonnelIncidentRequest,
  CreateStaffPersonRequest,
  StaffPersonDetailResponse,
  StaffPersonSummaryResponse,
  SubordinateSummaryResponse,
  RecruitingRequisitionResponse,
  RecruitingCandidateResponse,
  RecruitingInterviewStageResponse,
  RecruitingOfferResponse,
  UpsertRecruitingCandidateRequest,
  UpsertRecruitingRequisitionRequest,
  UpsertRecruitingInterviewStageRequest,
  UpsertRecruitingOfferRequest,
  UpdatePersonManagerRequest,
  UpdateOrgUnitAssignmentRequest,
  UpdateOrgUnitAssignmentStatusRequest,
  UpdateStaffRoleRequest,
  UpdateOrgUnitRequest,
  UpdateOrgUnitStatusRequest,
  RestoreOrgUnitRequest,
  UpdateInternalLocationRequest,
  ArchiveInternalLocationRequest,
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
  StaffArrTenantSettingsResponse,
  UpsertStaffArrTenantSettingsRequest,
  EmploymentApplicationTemplateCreateRequest,
  EmploymentApplicationTemplateResponse,
  EmploymentApplicationTemplateUpsertRequest,
  EmploymentApplicationSubmissionListItemResponse,
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
  EntityExportManifestResponse,
  LaunchHandoffResponse,
  StaffArrRestrictionSnapshotResponse,
  ReadinessOverrideResponse,
  AuditPackageManifestResponse,
  AuditPackageExportResponse,
  AuditPackageGenerationJobResponse,
  AuditPackageFilterOptions,
  AuditPackageExportSummary,
  AuditPackageScope,
  StaffArrAuditEventExportItem,
  PersonnelReportSummaryResponse,
  ReadinessReportSummaryResponse,
  IncidentReportSummaryResponse,
  CertificationReportSummaryResponse,
  CreateStaffRoleRequest,
  ArchiveStaffRoleRequest,
  CloneStaffRoleRequest,
  SetStaffRolePermissionsRequest,
  SetStaffRoleScopesRequest,
  SetStaffPersonRolesRequest,
  RefreshPermissionCatalogRequest,
  RefreshPermissionCatalogResponse,
  PermissionEvaluateRequest,
  PermissionEvaluateResponse,
  EmploymentApplicationBuilderCatalogResponse,
} from './types'

const apiBase = import.meta.env.VITE_STAFFARR_API_BASE ?? ''
const maintainArrApiBase = import.meta.env.VITE_MAINTAINARR_FRONTEND_BASE ?? 'http://localhost:5178'
const routArrApiBase = import.meta.env.VITE_ROUTARR_FRONTEND_BASE ?? 'http://localhost:5180'
const supplyArrApiBase = import.meta.env.VITE_SUPPLYARR_FRONTEND_BASE ?? 'http://localhost:5179'
const recordArrApiBase = import.meta.env.VITE_RECORDARR_FRONTEND_BASE ?? 'http://localhost:5184'

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

function buildAuditPackageQuery(
  options?: AuditPackageScope & {
    format?: string
    page?: number
    pageSize?: number
  },
): string {
  const params = new URLSearchParams()
  if (options?.format) params.set('format', options.format)
  if (options?.from) params.set('from', options.from)
  if (options?.to) params.set('to', options.to)
  if (options?.action) params.set('action', options.action)
  if (options?.result) params.set('result', options.result)
  if (options?.targetType) params.set('targetType', options.targetType)
  if (options?.actorUserId) params.set('actorUserId', options.actorUserId)
  if (options?.personId) params.set('personId', options.personId)
  if (options?.page != null) params.set('page', String(options.page))
  if (options?.pageSize != null) params.set('pageSize', String(options.pageSize))
  const query = params.toString()
  return query ? `?${query}` : ''
}

function auditPackageJobBody(scope: AuditPackageScope & { format: string }) {
  return {
    format: scope.format,
    from: scope.from ? `${scope.from}T00:00:00.000Z` : undefined,
    to: scope.to ? `${scope.to}T23:59:59.999Z` : undefined,
    action: scope.action,
    result: scope.result,
    targetType: scope.targetType,
    actorUserId: scope.actorUserId,
    personId: scope.personId,
  }
}

async function parseCrossProductJsonResponse<T>(
  response: Response,
  fallbackMessage: string,
): Promise<T> {
  if (!response.ok) {
    throw await toApiError(response, fallbackMessage)
  }

  return (await response.json()) as T
}

async function fetchCrossProductJson<T>(
  apiBaseUrl: string,
  path: string,
  accessToken: string,
  fallbackMessage: string,
): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    headers: authHeaders(accessToken),
  })
  return parseCrossProductJsonResponse<T>(response, fallbackMessage)
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

export async function getStaffArrFieldset(
  accessToken: string,
  fieldsetPath: string,
): Promise<StaffArrFieldsetResponse> {
  const response = await fetch(`${apiBase}/api/v1/fieldsets/${fieldsetPath}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffArrFieldsetResponse>(response, 'Failed to load StaffArr fieldset')
}

export async function getEmploymentApplicationBuilderCatalog(
  accessToken: string,
): Promise<EmploymentApplicationBuilderCatalogResponse> {
  const response = await fetch(`${apiBase}/api/v1/fieldsets/employment-applications/builder`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<EmploymentApplicationBuilderCatalogResponse>(
    response,
    'Failed to load employment application field catalog',
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

export interface CrossProductAssetReferenceOption {
  assetId: string
  assetTag: string
  name: string
  lifecycleStatus: string
}

export interface CrossProductWorkOrderReferenceOption {
  workOrderId: string
  workOrderNumber: string
  title: string
  status: string
}

export interface CrossProductRouteReferenceOption {
  routeId: string
  routeNumber: string
  title: string
  routeStatus: string
}

export interface CrossProductSupplierReferenceOption {
  partyId: string
  partyKey: string
  displayName: string
  legalName: string
  status: string
}

export interface CrossProductControlledDocumentReferenceOption {
  controlledDocumentId: string
  documentNumber: string
  title: string
  controlledDocumentType: string
  status: string
}

export async function getMaintainArrAssetReferences(
  accessToken: string,
): Promise<CrossProductAssetReferenceOption[]> {
  return fetchCrossProductJson<CrossProductAssetReferenceOption[]>(
    maintainArrApiBase,
    '/api/assets',
    accessToken,
    'Failed to load asset references',
  )
}

export async function getMaintainArrWorkOrderReferences(
  accessToken: string,
): Promise<CrossProductWorkOrderReferenceOption[]> {
  return fetchCrossProductJson<CrossProductWorkOrderReferenceOption[]>(
    maintainArrApiBase,
    '/api/work-orders?status=open',
    accessToken,
    'Failed to load work order references',
  )
}

export async function getRoutArrRouteReferences(
  accessToken: string,
): Promise<CrossProductRouteReferenceOption[]> {
  return fetchCrossProductJson<CrossProductRouteReferenceOption[]>(
    routArrApiBase,
    '/api/routes?routeStatus=active',
    accessToken,
    'Failed to load route references',
  )
}

export async function getSupplyArrSupplierReferences(
  accessToken: string,
): Promise<CrossProductSupplierReferenceOption[]> {
  return fetchCrossProductJson<CrossProductSupplierReferenceOption[]>(
    supplyArrApiBase,
    '/api/suppliers',
    accessToken,
    'Failed to load supplier references',
  )
}

export async function getRecordArrControlledDocumentReferences(
  accessToken: string,
): Promise<CrossProductControlledDocumentReferenceOption[]> {
  return fetchCrossProductJson<CrossProductControlledDocumentReferenceOption[]>(
    recordArrApiBase,
    '/api/v1/workspace/controlled-documents',
    accessToken,
    'Failed to load controlled document references',
  )
}

export async function getPerson(accessToken: string, personId: string): Promise<StaffPersonDetailResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffPersonDetailResponse>(response, 'Failed to load person profile')
}

export async function getPersonSummary(
  accessToken: string,
  personId: string,
): Promise<StaffArrPersonIntegrationSummaryResponse> {
  const response = await fetch(`${apiBase}/api/v1/integrations/persons/${personId}/summary`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffArrPersonIntegrationSummaryResponse>(
    response,
    'Failed to load person summary',
  )
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

export async function getStaffArrTenantSettings(
  accessToken: string,
): Promise<StaffArrTenantSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/staffarr/tenant-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffArrTenantSettingsResponse>(
    response,
    'Failed to load StaffArr tenant settings',
  )
}

export async function getStaffArrTenantSettingsDefaults(
  accessToken: string,
): Promise<StaffArrTenantSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/staffarr/tenant-settings/defaults`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffArrTenantSettingsResponse>(
    response,
    'Failed to load StaffArr tenant setting defaults',
  )
}

export async function updateStaffArrTenantSettings(
  accessToken: string,
  request: UpsertStaffArrTenantSettingsRequest,
): Promise<StaffArrTenantSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/staffarr/tenant-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<StaffArrTenantSettingsResponse>(
    response,
    'Failed to save StaffArr tenant settings',
  )
}

export async function listEmploymentApplicationTemplates(
  accessToken: string,
): Promise<EmploymentApplicationTemplateResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/employment-applications/templates`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<EmploymentApplicationTemplateResponse[]>(
    response,
    'Failed to load employment application templates',
  )
}

export async function getEmploymentApplicationTemplate(
  accessToken: string,
  templateId: string,
): Promise<EmploymentApplicationTemplateResponse> {
  const response = await fetch(`${apiBase}/api/v1/employment-applications/templates/${templateId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<EmploymentApplicationTemplateResponse>(
    response,
    'Failed to load employment application template',
  )
}

export async function createEmploymentApplicationTemplate(
  accessToken: string,
  request: EmploymentApplicationTemplateCreateRequest,
): Promise<EmploymentApplicationTemplateResponse> {
  const response = await fetch(`${apiBase}/api/v1/employment-applications/templates`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<EmploymentApplicationTemplateResponse>(
    response,
    'Failed to create employment application template',
  )
}

export async function updateEmploymentApplicationTemplate(
  accessToken: string,
  templateId: string,
  request: EmploymentApplicationTemplateUpsertRequest,
): Promise<EmploymentApplicationTemplateResponse> {
  const response = await fetch(`${apiBase}/api/v1/employment-applications/templates/${templateId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<EmploymentApplicationTemplateResponse>(
    response,
    'Failed to save employment application template',
  )
}

export async function publishEmploymentApplicationTemplate(
  accessToken: string,
  templateId: string,
): Promise<EmploymentApplicationTemplateResponse> {
  const response = await fetch(`${apiBase}/api/v1/employment-applications/templates/${templateId}/publish`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<EmploymentApplicationTemplateResponse>(
    response,
    'Failed to publish employment application template',
  )
}

export async function cloneEmploymentApplicationTemplate(
  accessToken: string,
  templateId: string,
): Promise<EmploymentApplicationTemplateResponse> {
  const response = await fetch(`${apiBase}/api/v1/employment-applications/templates/${templateId}/clone`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<EmploymentApplicationTemplateResponse>(
    response,
    'Failed to clone employment application template',
  )
}

export async function listEmploymentApplicationSubmissions(
  accessToken: string,
  limit = 20,
): Promise<EmploymentApplicationSubmissionListItemResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/employment-applications/submissions?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<EmploymentApplicationSubmissionListItemResponse[]>(
    response,
    'Failed to load employment application submissions',
  )
}

export async function listRecruitingRequisitions(
  accessToken: string,
): Promise<RecruitingRequisitionResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/hiring/requisitions`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RecruitingRequisitionResponse[]>(response, 'Failed to load recruiting requisitions')
}

export async function createRecruitingRequisition(
  accessToken: string,
  request: UpsertRecruitingRequisitionRequest,
): Promise<RecruitingRequisitionResponse> {
  const response = await fetch(`${apiBase}/api/v1/hiring/requisitions`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<RecruitingRequisitionResponse>(response, 'Failed to create recruiting requisition')
}

export async function updateRecruitingRequisition(
  accessToken: string,
  requisitionId: string,
  request: UpsertRecruitingRequisitionRequest,
): Promise<RecruitingRequisitionResponse> {
  const response = await fetch(`${apiBase}/api/v1/hiring/requisitions/${encodeURIComponent(requisitionId)}`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<RecruitingRequisitionResponse>(response, 'Failed to update recruiting requisition')
}

export async function archiveRecruitingRequisition(
  accessToken: string,
  requisitionId: string,
): Promise<RecruitingRequisitionResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/hiring/requisitions/${encodeURIComponent(requisitionId)}/archive`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<RecruitingRequisitionResponse>(response, 'Failed to archive recruiting requisition')
}

export async function listRecruitingCandidates(
  accessToken: string,
  requisitionId?: string,
): Promise<RecruitingCandidateResponse[]> {
  const query = requisitionId ? `?requisitionId=${encodeURIComponent(requisitionId)}` : ''
  const response = await fetch(`${apiBase}/api/v1/hiring/candidates${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RecruitingCandidateResponse[]>(response, 'Failed to load recruiting candidates')
}

export async function updateRecruitingCandidate(
  accessToken: string,
  candidateId: string,
  request: UpsertRecruitingCandidateRequest,
): Promise<RecruitingCandidateResponse> {
  const response = await fetch(`${apiBase}/api/v1/hiring/candidates/${encodeURIComponent(candidateId)}`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<RecruitingCandidateResponse>(response, 'Failed to update recruiting candidate')
}

export async function archiveRecruitingCandidate(
  accessToken: string,
  candidateId: string,
): Promise<RecruitingCandidateResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/hiring/candidates/${encodeURIComponent(candidateId)}/archive`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<RecruitingCandidateResponse>(response, 'Failed to archive recruiting candidate')
}

export async function listRecruitingInterviewStages(
  accessToken: string,
  candidateId?: string,
): Promise<RecruitingInterviewStageResponse[]> {
  if (!candidateId) {
    return []
  }

  const query = `/${encodeURIComponent(candidateId)}/interview-stages`
  const response = await fetch(`${apiBase}/api/v1/hiring/candidates${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RecruitingInterviewStageResponse[]>(response, 'Failed to load recruiting interview stages')
}

export async function createRecruitingInterviewStage(
  accessToken: string,
  request: UpsertRecruitingInterviewStageRequest,
): Promise<RecruitingInterviewStageResponse> {
  const response = await fetch(`${apiBase}/api/v1/hiring/interview-stages`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<RecruitingInterviewStageResponse>(response, 'Failed to create recruiting interview stage')
}

export async function updateRecruitingInterviewStage(
  accessToken: string,
  stageId: string,
  request: UpsertRecruitingInterviewStageRequest,
): Promise<RecruitingInterviewStageResponse> {
  const response = await fetch(`${apiBase}/api/v1/hiring/interview-stages/${encodeURIComponent(stageId)}`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<RecruitingInterviewStageResponse>(response, 'Failed to update recruiting interview stage')
}

export async function archiveRecruitingInterviewStage(
  accessToken: string,
  stageId: string,
): Promise<RecruitingInterviewStageResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/hiring/interview-stages/${encodeURIComponent(stageId)}/archive`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<RecruitingInterviewStageResponse>(response, 'Failed to archive recruiting interview stage')
}

export async function listRecruitingOffers(
  accessToken: string,
  candidateId?: string,
): Promise<RecruitingOfferResponse[]> {
  const query = candidateId ? `?candidateId=${encodeURIComponent(candidateId)}` : ''
  const response = await fetch(`${apiBase}/api/v1/hiring/offers${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RecruitingOfferResponse[]>(response, 'Failed to load recruiting offers')
}

export async function createRecruitingOffer(
  accessToken: string,
  request: UpsertRecruitingOfferRequest,
): Promise<RecruitingOfferResponse> {
  const response = await fetch(`${apiBase}/api/v1/hiring/offers`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<RecruitingOfferResponse>(response, 'Failed to create recruiting offer')
}

export async function updateRecruitingOffer(
  accessToken: string,
  offerId: string,
  request: UpsertRecruitingOfferRequest,
): Promise<RecruitingOfferResponse> {
  const response = await fetch(`${apiBase}/api/v1/hiring/offers/${encodeURIComponent(offerId)}`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<RecruitingOfferResponse>(response, 'Failed to update recruiting offer')
}

export async function archiveRecruitingOffer(
  accessToken: string,
  offerId: string,
): Promise<RecruitingOfferResponse> {
  const response = await fetch(`${apiBase}/api/v1/hiring/offers/${encodeURIComponent(offerId)}/archive`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RecruitingOfferResponse>(response, 'Failed to archive recruiting offer')
}

export async function convertEmploymentApplicationSubmissionToCandidate(
  accessToken: string,
  submissionId: string,
  requisitionId?: string | null,
): Promise<RecruitingCandidateResponse> {
  const query = requisitionId ? `?requisitionId=${encodeURIComponent(requisitionId)}` : ''
  const response = await fetch(
    `${apiBase}/api/v1/hiring/submissions/${submissionId}/candidates${query}`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<RecruitingCandidateResponse>(response, 'Failed to convert submission to candidate')
}

export async function hireRecruitingCandidate(
  accessToken: string,
  candidateId: string,
  request: CreateStaffPersonRequest,
): Promise<RecruitingCandidateResponse> {
  const response = await fetch(`${apiBase}/api/v1/hiring/candidates/${encodeURIComponent(candidateId)}/hire`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<RecruitingCandidateResponse>(response, 'Failed to hire recruiting candidate')
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

export async function getOrgUnits(
  accessToken: string,
  params: {
    includeArchived?: boolean
    search?: string | null
    type?: string | null
  } = {},
): Promise<OrgUnitResponse[]> {
  const query = new URLSearchParams()
  if (params.includeArchived) {
    query.set('includeArchived', 'true')
  }
  if (params.search?.trim()) {
    query.set('search', params.search.trim())
  }
  if (params.type?.trim()) {
    query.set('type', params.type.trim())
  }
  const suffix = query.size > 0 ? `?${query.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/org-units${suffix}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OrgUnitResponse[]>(response, 'Failed to load org units')
}

function buildLocationQueryString(params: {
  includeArchived?: boolean
  search?: string | null
  type?: string | null
  siteOrgUnitId?: string | null
} = {}): string {
  const query = new URLSearchParams()
  if (params.includeArchived) {
    query.set('includeArchived', 'true')
  }
  if (params.search?.trim()) {
    query.set('search', params.search.trim())
  }
  if (params.type?.trim()) {
    query.set('type', params.type.trim())
  }
  if (params.siteOrgUnitId?.trim()) {
    query.set('siteOrgUnitId', params.siteOrgUnitId.trim())
  }
  const suffix = query.toString()
  return suffix ? `?${suffix}` : ''
}

export async function listLocations(
  accessToken: string,
  params: {
    includeArchived?: boolean
    search?: string | null
    type?: string | null
    siteOrgUnitId?: string | null
  } = {},
): Promise<InternalLocationResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/locations${buildLocationQueryString(params)}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<InternalLocationResponse[]>(response, 'Failed to load locations')
}

export async function listLocationTree(
  accessToken: string,
  params: {
    includeArchived?: boolean
    search?: string | null
    type?: string | null
    siteOrgUnitId?: string | null
  } = {},
): Promise<InternalLocationResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/locations/tree${buildLocationQueryString(params)}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<InternalLocationResponse[]>(response, 'Failed to load locations tree')
}

export async function getLocation(
  accessToken: string,
  locationId: string,
): Promise<InternalLocationResponse> {
  const response = await fetch(`${apiBase}/api/v1/locations/${locationId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<InternalLocationResponse>(response, 'Failed to load location')
}

export async function createLocation(
  accessToken: string,
  request: CreateInternalLocationRequest,
): Promise<InternalLocationResponse> {
  const response = await fetch(`${apiBase}/api/v1/locations`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<InternalLocationResponse>(response, 'Failed to create location')
}

export async function updateLocation(
  accessToken: string,
  locationId: string,
  request: UpdateInternalLocationRequest,
): Promise<InternalLocationResponse> {
  const response = await fetch(`${apiBase}/api/v1/locations/${locationId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<InternalLocationResponse>(response, 'Failed to update location')
}

export async function archiveLocation(
  accessToken: string,
  locationId: string,
  request: ArchiveInternalLocationRequest,
): Promise<InternalLocationResponse> {
  const response = await fetch(`${apiBase}/api/v1/locations/${locationId}/archive`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<InternalLocationResponse>(response, 'Failed to archive location')
}

export async function listSiteLocations(
  accessToken: string,
  siteOrgUnitId: string,
): Promise<InternalLocationResponse[]> {
  return listLocations(accessToken, { siteOrgUnitId })
}

export async function listLocationChildren(
  accessToken: string,
  locationId: string,
): Promise<InternalLocationResponse[]> {
  const allLocations = await listLocations(accessToken)
  return allLocations.filter((location) => location.parentLocationId === locationId)
}

export async function getPersonRestrictions(
  accessToken: string,
  personId: string,
): Promise<StaffArrRestrictionSnapshotResponse> {
  const response = await fetch(`${apiBase}/api/v1/integrations/persons/${personId}/restrictions`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffArrRestrictionSnapshotResponse>(
    response,
    'Failed to load person restrictions',
  )
}

export async function createRestriction(
  accessToken: string,
  request: { personId: string; reason: string; expiresAt: string | null },
): Promise<ReadinessOverrideResponse> {
  const response = await fetch(`${apiBase}/api/v1/integrations/restrictions`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<ReadinessOverrideResponse>(response, 'Failed to create restriction')
}

export async function liftRestriction(
  accessToken: string,
  restrictionId: string,
): Promise<ReadinessOverrideResponse> {
  const response = await fetch(`${apiBase}/api/v1/integrations/restrictions/${restrictionId}/lift`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ReadinessOverrideResponse>(response, 'Failed to lift restriction')
}

export async function createOrgUnit(accessToken: string, request: CreateOrgUnitRequest): Promise<OrgUnitResponse> {
  const response = await fetch(`${apiBase}/api/v1/org-units`, {
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
  const response = await fetch(`${apiBase}/api/v1/org-units/${orgUnitId}`, {
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
  const response = await fetch(`${apiBase}/api/v1/org-units/${orgUnitId}/status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<OrgUnitResponse>(response, 'Failed to update org unit status')
}

export async function restoreOrgUnit(
  accessToken: string,
  orgUnitId: string,
  request: RestoreOrgUnitRequest = {},
): Promise<OrgUnitResponse> {
  const response = await fetch(`${apiBase}/api/v1/org-units/${orgUnitId}/restore`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<OrgUnitResponse>(response, 'Failed to restore org unit')
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

export async function getEffectivePermissions(
  accessToken: string,
  personId: string,
): Promise<EffectivePermissionProjectionResponse> {
  const response = await fetch(`${apiBase}/api/people/${personId}/permissions/effective`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<EffectivePermissionProjectionResponse>(response, 'Failed to load effective permissions')
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

export async function listStaffRoles(accessToken: string): Promise<StaffRoleSummaryResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/roles`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffRoleSummaryResponse[]>(response, 'Failed to load roles')
}

export async function getStaffRole(accessToken: string, roleId: string): Promise<StaffRoleDetailResponse> {
  const response = await fetch(`${apiBase}/api/v1/roles/${roleId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffRoleDetailResponse>(response, 'Failed to load role')
}

export async function createStaffRole(
  accessToken: string,
  request: CreateStaffRoleRequest,
): Promise<StaffRoleDetailResponse> {
  const response = await fetch(`${apiBase}/api/v1/roles`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<StaffRoleDetailResponse>(response, 'Failed to create role')
}

export async function updateStaffRole(
  accessToken: string,
  roleId: string,
  request: UpdateStaffRoleRequest,
): Promise<StaffRoleDetailResponse> {
  const response = await fetch(`${apiBase}/api/v1/roles/${roleId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<StaffRoleDetailResponse>(response, 'Failed to update role')
}

export async function archiveStaffRole(
  accessToken: string,
  roleId: string,
  request: ArchiveStaffRoleRequest,
): Promise<StaffRoleDetailResponse> {
  const response = await fetch(`${apiBase}/api/v1/roles/${roleId}/archive`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<StaffRoleDetailResponse>(response, 'Failed to archive role')
}

export async function cloneStaffRole(
  accessToken: string,
  roleId: string,
  request: CloneStaffRoleRequest,
): Promise<StaffRoleDetailResponse> {
  const response = await fetch(`${apiBase}/api/v1/roles/${roleId}/clone`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<StaffRoleDetailResponse>(response, 'Failed to clone role')
}

export async function setStaffRolePermissions(
  accessToken: string,
  roleId: string,
  request: SetStaffRolePermissionsRequest,
): Promise<StaffRoleDetailResponse['permissions']> {
  const response = await fetch(`${apiBase}/api/v1/roles/${roleId}/permissions`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<StaffRoleDetailResponse['permissions']>(response, 'Failed to save role permissions')
}

export async function setStaffRoleScopes(
  accessToken: string,
  roleId: string,
  request: SetStaffRoleScopesRequest,
): Promise<StaffRoleDetailResponse['scopes']> {
  const response = await fetch(`${apiBase}/api/v1/roles/${roleId}/scopes`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<StaffRoleDetailResponse['scopes']>(response, 'Failed to save role scopes')
}

export async function getStaffPersonRoles(
  accessToken: string,
  personId: string,
): Promise<StaffPersonRoleAssignmentResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/people/${personId}/roles`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffPersonRoleAssignmentResponse[]>(response, 'Failed to load person roles')
}

export async function setStaffPersonRoles(
  accessToken: string,
  personId: string,
  request: SetStaffPersonRolesRequest,
): Promise<StaffPersonRoleAssignmentResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/people/${personId}/roles`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<StaffPersonRoleAssignmentResponse[]>(response, 'Failed to save person roles')
}

export async function getPermissionCatalogs(accessToken: string): Promise<PermissionCatalogResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/permissions/catalog`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PermissionCatalogResponse[]>(response, 'Failed to load permission catalogs')
}

export async function refreshPermissionCatalogs(
  accessToken: string,
  request?: RefreshPermissionCatalogRequest,
): Promise<RefreshPermissionCatalogResponse> {
  const response = await fetch(`${apiBase}/api/v1/permissions/catalog/refresh`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request ?? {}),
  })
  return parseJsonResponse<RefreshPermissionCatalogResponse>(response, 'Failed to refresh permission catalogs')
}

export async function evaluatePermission(
  accessToken: string,
  request: PermissionEvaluateRequest,
): Promise<PermissionEvaluateResponse> {
  const response = await fetch(`${apiBase}/api/v1/permissions/evaluate`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PermissionEvaluateResponse>(response, 'Failed to evaluate permission')
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

export async function getAuditPackageFilterOptions(
  accessToken: string,
): Promise<AuditPackageFilterOptions> {
  const response = await fetch(`${apiBase}/api/audit-packages/filter-options`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AuditPackageFilterOptions>(response, 'Failed to load audit filter options')
}

export async function getAuditPackageExportSummary(
  accessToken: string,
  scope?: AuditPackageScope,
): Promise<AuditPackageExportSummary> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/summary${buildAuditPackageQuery(scope)}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<AuditPackageExportSummary>(response, 'Failed to load audit export summary')
}

export async function getAuditPackageTimeline(
  accessToken: string,
  options?: AuditPackageScope & { page?: number; pageSize?: number },
): Promise<PagedResult<StaffArrAuditEventExportItem>> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/timeline${buildAuditPackageQuery(options)}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<PagedResult<StaffArrAuditEventExportItem>>(
    response,
    'Failed to load audit package timeline',
  )
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

export async function exportAuditPackageCsv(
  accessToken: string,
  scope?: AuditPackageScope,
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
  options?: AuditPackageScope,
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/export${buildAuditPackageQuery(options)}`,
    { headers: { Authorization: `Bearer ${accessToken}` } },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Audit package ZIP export failed')
  }
  return response.blob()
}

export async function exportAuditPackageJson(
  accessToken: string,
  options?: AuditPackageScope,
): Promise<AuditPackageExportResponse> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/export${buildAuditPackageQuery({ ...options, format: 'json' })}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<AuditPackageExportResponse>(response, 'Failed to export audit package JSON')
}

export async function createAuditPackageGenerationJob(
  accessToken: string,
  options: AuditPackageScope & { format: 'zip' | 'json' },
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
    headers: authHeaders(accessToken),
  })
  if (!response.ok) {
    throw await toApiError(response, 'Failed to download audit package generation job')
  }
  return response.blob()
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
    throw await toApiError(response, 'Personnel report CSV export failed')
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
    throw await toApiError(response, 'Readiness report CSV export failed')
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
    throw await toApiError(response, 'Incident report CSV export failed')
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
    throw await toApiError(response, 'Certification report CSV export failed')
  }
  return response.blob()
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
