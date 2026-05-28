export interface AuthTokenResponse {
  accessToken: string
  refreshToken: string
  accessTokenExpiresAt: string
  refreshTokenExpiresAt: string
  sessionId: string
  userId: string
  tenantId: string
}

export interface LoginRequest {
  email: string
  password: string
  tenantId: string | null
}

export interface MeResponse {
  userId: string
  email: string
  displayName: string
  isPlatformAdmin: boolean
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  entitlements: string[]
}

export interface NavigationSurfaceItem {
  surfaceKey: string
  label: string
  relativePath: string
  iconKey: string
  sortOrder: number
  isEnabled: boolean
  permissionHint: string | null
}

export interface NavigationItem {
  productKey: string
  displayName: string
  routePath: string
  sortOrder: number
  surfaces: NavigationSurfaceItem[]
}

export interface NavigationResponse {
  tenantId: string
  products: NavigationItem[]
}

export interface TenantSummary {
  tenantId: string
  slug: string
  displayName: string
  status: string
  roleKey: string
}

export interface EntitlementSummary {
  productKey: string
  displayName: string
  status: string
}

export interface LaunchContextResponse {
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  userId: string
  userEmail: string
  productKey: string
  productDisplayName: string
  baseLaunchUrl: string
  launchUrl: string
  canLaunch: boolean
  denialReasonCode: string | null
}

export interface HandoffCreatedResponse {
  handoffCode: string
  handoffId: string
  expiresAt: string
  launchUrl: string
}

export interface PlatformAdminDashboardResponse {
  tenantCount: number
  activeTenantCount: number
  productCount: number
  activeProductCount: number
  activeEntitlementCount: number
  totalEntitlementCount: number
  serviceClientCount: number
  activeServiceTokenCount: number
  launchProfileCount: number
  pendingHandoffCount: number
  expiredUnredeemedHandoffCount: number
  auditEventsLast24Hours: number
  generatedAt: string
}

export interface LaunchDiagnosticIssue {
  issueCode: string
  severity: string
  message: string
  tenantId: string | null
  tenantSlug: string | null
  productKey: string | null
}

export interface LaunchDiagnosticRow {
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  tenantStatus: string
  productKey: string
  productDisplayName: string
  hasActiveEntitlement: boolean
  hasLaunchProfile: boolean
  launchProfileActive: boolean
  callbackAllowlistEntryCount: number
  pendingHandoffCount: number
  expiredHandoffCount: number
  launchReadiness: string
}

export interface LaunchDiagnosticsResponse {
  rows: LaunchDiagnosticRow[]
  issues: LaunchDiagnosticIssue[]
  generatedAt: string
}

export interface TenantOverviewRow {
  tenantId: string
  slug: string
  displayName: string
  status: string
  activeEntitlementCount: number
  membershipCount: number
  createdAt: string
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  hasNextPage: boolean
}

export interface ProductOverviewRow {
  productKey: string
  displayName: string
  isActive: boolean
  activeEntitlementCount: number
  hasLaunchProfile: boolean
  launchProfileActive: boolean
  baseUrl: string | null
}

export interface ApiErrorBody {
  code?: string
  message?: string
}

export interface PlatformAuditPackageSection {
  key: string
  fileName: string
  label: string
  description: string
}

export interface PlatformAuditPackageManifest {
  packageVersion: string
  sections: PlatformAuditPackageSection[]
}

export interface PlatformAuditEventTimelineItem {
  auditEventId: string
  tenantId: string | null
  actorUserId: string | null
  action: string
  targetType: string
  targetId: string | null
  result: string
  reasonCode: string | null
  correlationId: string
  occurredAt: string
}

export interface PlatformAuditPackageGenerationJob {
  jobId: string
  scopeTenantId: string | null
  status: string
  format: string
  packageId: string | null
  errorMessage: string | null
  createdAt: string
  completedAt: string | null
  downloadReady: boolean
}

export interface PlatformAuditPackageExportPreview {
  packageId: string
  scopeTenantId: string | null
  generatedAt: string
  counts: {
    auditEvents: number
    tenants: number
    tenantEntitlements: number
    productCatalog: number
    platformUsers: number
    serviceClients: number
    serviceTokens: number
    launchProfiles: number
    callbackAllowlist: number
  }
}

export class NexarrApiError extends Error {
  readonly status: number
  readonly code?: string

  constructor(status: number, message: string, code?: string) {
    super(message)
    this.name = 'NexarrApiError'
    this.status = status
    this.code = code
  }
}
