export interface HandoffSessionResponse {
  accessToken: string
  accessTokenExpiresAt: string
  userId: string
  personId: string
  email: string
  displayName: string
  tenantId: string
  tenantSlug: string
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
