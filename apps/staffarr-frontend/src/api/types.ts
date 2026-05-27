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
