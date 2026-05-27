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
  PersonRoleAssignmentResponse,
  PersonManagerResponse,
  RoleTemplateResponse,
  StaffArrMeResponse,
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
  ReadinessRollupSummaryResponse,
  GrantPersonCertificationRequest,
  UpdatePersonCertificationRequest,
  PersonnelIncidentSummaryResponse,
  PersonnelIncidentDetailResponse,
  CreatePersonnelIncidentRequest,
  RouteIncidentToTrainarrResponse,
  PagedResult,
  PersonTimelineEntryResponse,
  AuditPackageManifestResponse,
  AuditPackageExportResponse,
  UpdateStaffPersonRequest,
  UpdatePersonEmploymentStatusRequest,
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

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new StaffArrApiError(body || `${fallbackMessage} (${response.status})`, response.status, body)
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

export async function getMe(accessToken: string): Promise<StaffArrMeResponse> {
  const response = await fetch(`${apiBase}/api/me`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StaffArrMeResponse>(response, 'Failed to load profile')
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

export async function getOrgUnits(accessToken: string): Promise<OrgUnitResponse[]> {
  const response = await fetch(`${apiBase}/api/org-units`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OrgUnitResponse[]>(response, 'Failed to load org units')
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

export async function getPersonTimeline(
  accessToken: string,
  personId: string,
  page = 1,
  pageSize = 50,
): Promise<PagedResult<PersonTimelineEntryResponse>> {
  const response = await fetch(
    `${apiBase}/api/people/${personId}/timeline?page=${page}&pageSize=${pageSize}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<PagedResult<PersonTimelineEntryResponse>>(response, 'Failed to load person timeline')
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
    throw new StaffArrApiError(
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
