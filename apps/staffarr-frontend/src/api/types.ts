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

export interface StaffArrMeResponse {
  userId: string
  personId: string
  email: string
  displayName: string
  tenantId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasStaffArrEntitlement: boolean
  primaryOrgUnitName: string | null
  jobTitle: string | null
  entitlements: string[]
}

export interface StaffPersonSummaryResponse {
  personId: string
  externalUserId: string | null
  displayName: string
  primaryEmail: string
  employmentStatus: string
  primaryOrgUnitId: string | null
  primaryOrgUnitName: string | null
  managerPersonId: string | null
  jobTitle: string | null
}

export interface StaffPersonDetailResponse {
  personId: string
  externalUserId: string | null
  givenName: string
  familyName: string
  displayName: string
  primaryEmail: string
  employmentStatus: string
  primaryOrgUnitId: string | null
  primaryOrgUnitName: string | null
  managerPersonId: string | null
  jobTitle: string | null
  createdAt: string
  updatedAt: string
}

export interface UpdateStaffPersonRequest {
  givenName: string
  familyName: string
  primaryEmail: string
  primaryOrgUnitId: string | null
  managerPersonId: string | null
  jobTitle: string | null
}

export interface UpdatePersonEmploymentStatusRequest {
  employmentStatus: string
  reason: string | null
}

export interface BulkPersonImportRowRequest {
  givenName: string
  familyName: string
  primaryEmail: string
  employmentStatus?: string
  primaryOrgUnitId?: string | null
  managerPersonId?: string | null
  managerEmail?: string | null
  jobTitle?: string | null
}

export interface BulkPersonImportRequest {
  people: BulkPersonImportRowRequest[]
  dryRun?: boolean
}

export interface BulkPersonImportRowResult {
  rowIndex: number
  primaryEmail: string
  status: string
  personId: string | null
  errorCode: string | null
  message: string | null
}

export interface BulkPersonImportResponse {
  importId: string
  dryRun: boolean
  totalRows: number
  createdCount: number
  validatedCount: number
  errorCount: number
  results: BulkPersonImportRowResult[]
}

export interface PersonExportFormatDescriptor {
  key: string
  contentType: string
  fileName: string
  description: string
}

export interface PersonExportManifestResponse {
  packageVersion: string
  csvHeader: string
  formats: PersonExportFormatDescriptor[]
}

export interface PersonExportRowItem {
  personId: string
  givenName: string
  familyName: string
  primaryEmail: string
  employmentStatus: string
  jobTitle: string | null
  managerEmail: string | null
  primaryOrgUnitId: string | null
  primaryOrgUnitName: string | null
  createdAt: string
  updatedAt: string
}

export interface PersonExportResponse {
  exportId: string
  tenantId: string
  generatedAt: string
  personCount: number
  people: PersonExportRowItem[]
}

export interface PersonExportFilters {
  employmentStatus?: string
  orgUnitId?: string
}

export interface PersonExportPresetResponse {
  employmentStatus: string | null
  orgUnitId: string | null
  presetKey: string | null
  updatedAt: string
}

export interface UpsertPersonExportPresetRequest {
  employmentStatus?: string
  orgUnitId?: string | null
  presetKey?: string | null
}

export interface PersonExportScheduleResponse {
  isEnabled: boolean
  intervalHours: number
  lastDeliveredAt: string | null
  updatedAt: string | null
  notificationWebhookUrl: string | null
  notifyOnSuccess: boolean
  notifyOnFailure: boolean
}

export interface UpsertPersonExportScheduleRequest {
  isEnabled: boolean
  intervalHours: number
  notificationWebhookUrl?: string | null
  notifyOnSuccess: boolean
  notifyOnFailure: boolean
}

export interface PersonExportDeliveryNotificationItem {
  notificationId: string
  deliveryRunId: string | null
  eventKind: string
  deliveryStatus: string
  webhookHost: string | null
  httpStatusCode: number | null
  errorMessage: string | null
  exportId: string | null
  personCount: number | null
  attemptedAt: string
}

export interface PersonExportDeliveryNotificationsResponse {
  items: PersonExportDeliveryNotificationItem[]
}

export interface OrgUnitResponse {
  orgUnitId: string
  unitType: string
  name: string
  parentOrgUnitId: string | null
  status: 'active' | 'inactive'
}

export interface CreateOrgUnitRequest {
  unitType: string
  name: string
  parentOrgUnitId: string | null
}

export interface UpdateOrgUnitRequest {
  unitType: string
  name: string
  parentOrgUnitId: string | null
}

export interface UpdateOrgUnitStatusRequest {
  status: 'active' | 'inactive'
}

export interface OrgUnitAssignmentResponse {
  assignmentId: string
  personId: string
  siteOrgUnitId: string
  departmentOrgUnitId: string
  teamOrgUnitId: string
  positionOrgUnitId: string
  status: 'active' | 'inactive'
  createdAt: string
  updatedAt: string
}

export interface CreateOrgUnitAssignmentRequest {
  siteOrgUnitId: string
  departmentOrgUnitId: string
  teamOrgUnitId: string
  positionOrgUnitId: string
}

export interface UpdateOrgUnitAssignmentRequest {
  siteOrgUnitId: string
  departmentOrgUnitId: string
  teamOrgUnitId: string
  positionOrgUnitId: string
}

export interface UpdateOrgUnitAssignmentStatusRequest {
  status: 'active' | 'inactive'
}

export interface UpdatePersonManagerRequest {
  managerPersonId: string | null
}

export interface PersonManagerResponse {
  personId: string
  managerPersonId: string | null
  managerDisplayName: string | null
  updatedAt: string
}

export interface ManagerChainEntryResponse {
  personId: string
  displayName: string
  primaryEmail: string
  jobTitle: string | null
  primaryOrgUnitName: string | null
  managerPersonId: string | null
  level: number
}

export interface SubordinateSummaryResponse {
  personId: string
  displayName: string
  primaryEmail: string
  employmentStatus: string
  jobTitle: string | null
  primaryOrgUnitName: string | null
  managerPersonId: string | null
  managerDisplayName: string | null
  depth: number
  directReportCount: number
  activeAssignmentPath: string | null
}

export interface PermissionTemplateSummaryResponse {
  permissionTemplateId: string
  permissionKey: string
  name: string
  description: string | null
  status: 'active' | 'inactive'
}

export interface RoleTemplatePermissionResponse {
  mappingId: string
  permissionTemplateId: string
  permissionKey: string
  permissionName: string
  scopeType: 'tenant' | 'site' | 'department' | 'team' | 'position'
  scopeValue: string | null
}

export interface RoleTemplateResponse {
  roleTemplateId: string
  roleKey: string
  name: string
  description: string | null
  status: 'active' | 'inactive'
  permissions: RoleTemplatePermissionResponse[]
  createdAt: string
  updatedAt: string
}

export interface UpsertPermissionTemplateRequest {
  permissionKey: string
  name: string
  description: string | null
}

export interface RoleTemplatePermissionInput {
  permissionTemplateId: string
  scopeType: 'tenant' | 'site' | 'department' | 'team' | 'position'
  scopeValue: string | null
}

export interface CreateRoleTemplateRequest {
  roleKey: string
  name: string
  description: string | null
  permissions: RoleTemplatePermissionInput[]
}

export interface UpdateRoleTemplateRequest {
  name: string
  description: string | null
  status: 'active' | 'inactive'
  permissions: RoleTemplatePermissionInput[]
}

export interface PersonRoleAssignmentResponse {
  assignmentId: string
  personId: string
  roleTemplateId: string
  roleKey: string
  roleName: string
  scopeType: 'tenant' | 'site' | 'department' | 'team' | 'position'
  scopeValue: string | null
  status: 'active' | 'inactive'
  createdAt: string
  updatedAt: string
}

export interface CreatePersonRoleAssignmentRequest {
  roleTemplateId: string
  scopeType: 'tenant' | 'site' | 'department' | 'team' | 'position'
  scopeValue: string | null
}

export interface EffectivePermissionSourceResponse {
  assignmentId: string
  roleTemplateId: string
  roleKey: string
  roleName: string
  assignmentStatus: 'active' | 'inactive'
  assignmentScopeType: 'tenant' | 'site' | 'department' | 'team' | 'position'
  assignmentScopeValue: string | null
  assignedAt: string
}

export interface EffectivePermissionResponse {
  permissionKey: string
  permissionName: string
  scopeType: 'tenant' | 'site' | 'department' | 'team' | 'position'
  scopeValue: string | null
  sources: EffectivePermissionSourceResponse[]
}

export interface EffectivePermissionProjectionResponse {
  personId: string
  computedAt: string
  permissions: EffectivePermissionResponse[]
}

export interface PermissionHistoryTimelineEntryResponse {
  eventId: string
  personId: string
  assignmentId: string
  roleTemplateId: string
  permissionTemplateId: string
  actorUserId: string | null
  eventType: string
  assignmentStatus: 'active' | 'inactive'
  roleKey: string
  roleName: string
  permissionKey: string
  permissionName: string
  scopeType: 'tenant' | 'site' | 'department' | 'team' | 'position'
  scopeValue: string | null
  occurredAt: string
}

export interface CertificationDefinitionResponse {
  certificationDefinitionId: string
  certificationKey: string
  name: string
  description: string | null
  category: string
  defaultValidityDays: number | null
  status: 'active' | 'inactive'
  createdAt: string
  updatedAt: string
}

export interface UpsertCertificationDefinitionRequest {
  certificationKey: string
  name: string
  description: string | null
  category: string
  defaultValidityDays: number | null
}

export interface PersonCertificationResponse {
  personCertificationId: string
  personId: string
  certificationDefinitionId: string
  certificationKey: string
  certificationName: string
  category: string
  sourceType: string
  status: 'active' | 'expired' | 'revoked'
  effectiveStatus: 'active' | 'expired' | 'revoked'
  grantedAt: string
  expiresAt: string | null
  notes: string | null
  grantedByUserId: string | null
  externalPublicationId: string | null
  createdAt: string
  updatedAt: string
}

export interface GrantPersonCertificationRequest {
  certificationDefinitionId: string
  grantedAt: string | null
  expiresAt: string | null
  notes: string | null
}

export interface UpdatePersonCertificationRequest {
  status: 'active' | 'expired' | 'revoked'
  expiresAt: string | null
  notes: string | null
}

export interface ReadinessRequirementStatusResponse {
  certificationDefinitionId: string
  certificationKey: string
  certificationName: string
  requirementStatus: 'satisfied' | 'missing' | 'expired' | 'revoked'
  recordEffectiveStatus: string | null
  expiresAt: string | null
}

export interface ReadinessBlockerResponse {
  blockerSource: 'certification' | 'training'
  blockerType:
    | 'missing'
    | 'expired'
    | 'revoked'
    | 'missing_assignment'
    | 'overdue'
    | 'failed'
    | 'suspended'
  message: string
  certificationKey: string | null
  certificationName: string | null
  qualificationKey: string | null
  qualificationName: string | null
}

export interface ReadinessOverrideSummaryResponse {
  overrideId: string
  reason: string
  grantedAt: string
  expiresAt: string | null
  grantedByUserId: string
}

export interface PersonReadinessResponse {
  personId: string
  readinessStatus: 'ready' | 'not_ready'
  readinessBasis: 'certifications' | 'manual_override' | 'training_blockers'
  calculatedAt: string
  requirements: ReadinessRequirementStatusResponse[]
  blockers: ReadinessBlockerResponse[]
  activeOverride: ReadinessOverrideSummaryResponse | null
}

export interface GrantReadinessOverrideRequest {
  reason: string
  expiresAt: string | null
}

export interface ReadinessRollupSummaryResponse {
  orgUnitId: string
  scopeType: 'team' | 'site'
  orgUnitName: string
  totalMembers: number
  readyCount: number
  notReadyCount: number
  overrideCount: number
  readyPercent: number
  computedAt: string
}

export type PersonnelIncidentReasonCategory =
  | 'safety'
  | 'conduct'
  | 'injury'
  | 'equipment'
  | 'training_compliance'
  | 'policy'
  | 'other'

export type PersonnelIncidentSeverity = 'low' | 'medium' | 'high' | 'critical'

export interface IncidentTrainarrRoutingResponse {
  routingStatus: string
  trainarrRemediationId: string
  routedAt: string
  routedByUserId: string
}

export interface PersonnelIncidentSummaryResponse {
  incidentId: string
  personId: string
  reasonCategoryKey: PersonnelIncidentReasonCategory
  severity: PersonnelIncidentSeverity
  status: string
  title: string
  occurredAt: string
  reportedAt: string
  reportedByUserId: string
  trainarrRouting: IncidentTrainarrRoutingResponse | null
}

export interface PersonnelIncidentDetailResponse extends PersonnelIncidentSummaryResponse {
  description: string
  createdAt: string
  updatedAt: string
}

export interface RouteIncidentToTrainarrResponse {
  incidentId: string
  personId: string
  reasonCategoryKey: PersonnelIncidentReasonCategory
  status: string
  trainarrRouting: IncidentTrainarrRoutingResponse
}

export interface CreatePersonnelIncidentRequest {
  personId: string
  reasonCategoryKey: PersonnelIncidentReasonCategory
  severity: PersonnelIncidentSeverity
  title: string
  description: string
  occurredAt: string
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  hasNextPage: boolean
}

export interface PersonTimelineEntryResponse {
  entryId: string
  personId: string
  category: 'incident' | 'incident_routing' | 'readiness' | 'certification' | 'permission' | 'training_blocker'
  eventType: string
  title: string
  detail: string | null
  occurredAt: string
  actorUserId: string | null
  sourceEntityType: string
  sourceEntityId: string
  externalReferenceId: string | null
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
  people: number
  permissionHistory: number
  personCertifications: number
  personnelIncidents: number
  readinessOverrides: number
  trainingBlockers: number
}

export interface StaffArrAuditEventExportItem {
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

export interface AuditPackageExportResponse {
  packageId: string
  tenantId: string
  generatedAt: string
  dateRange: { from: string | null; to: string | null } | null
  counts: AuditPackageCountsResponse
  auditEvents: unknown[]
  people: unknown[]
  permissionHistory: unknown[]
  personCertifications: unknown[]
  personnelIncidents: unknown[]
  readinessOverrides: unknown[]
  trainingBlockers: unknown[]
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
