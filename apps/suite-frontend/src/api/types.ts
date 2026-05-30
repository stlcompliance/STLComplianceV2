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

export interface ForgotPasswordResponse {
  message: string
  devResetToken: string | null
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
  productCategory?: string
  productStatus?: string
  routePath: string
  launchUrl?: string
  isCurrent?: boolean
  sortOrder: number
  surfaces: NavigationSurfaceItem[]
}

export interface NavigationResponse {
  tenantId: string
  products: NavigationItem[]
}

export interface UserSessionSummary {
  sessionId: string
  createdAt: string
  expiresAt: string
  revokedAt: string | null
  userAgent: string | null
  ipAddress: string | null
  activeTenantId: string | null
  isCurrent: boolean
  isActive: boolean
}

export interface UserSessionsResponse {
  sessions: UserSessionSummary[]
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

export interface LaunchAttemptTimelineItem {
  auditEventId: string
  tenantId: string | null
  tenantSlug: string | null
  tenantDisplayName: string | null
  actorUserId: string | null
  actorEmail: string | null
  actorDisplayName: string | null
  productKey: string | null
  productDisplayName: string | null
  action: string
  result: string
  reasonCode: string | null
  targetType: string
  targetId: string | null
  correlationId: string
  occurredAt: string
  remediationHint: string | null
}

export interface ValidateLaunchRequest {
  productKey: string
  tenantId?: string | null
}

export interface ValidateLaunchResponse {
  tenantId: string
  productKey: string
  canLaunch: boolean
  reasonCode: string | null
  launchUrl: string | null
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

export interface TenantDetailResponse {
  tenantId: string
  slug: string
  displayName: string
  status: string
  createdAt: string
  modifiedAt: string
}

export interface CreateTenantRequest {
  slug: string
  displayName: string
}

export interface UpdateTenantRequest {
  displayName: string
}

export interface UpdateTenantStatusRequest {
  status: string
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

export interface ProductDetailResponse {
  productKey: string
  displayName: string
  sortOrder: number
  isActive: boolean
}

export interface ProductManifestCallbackAllowlistResponse {
  entryId: string
  tenantId: string | null
  urlPattern: string
  patternType: string
  isActive: boolean
}

export interface ProductManifestDataPlaneProfileResponse {
  profileId: string
  tenantId: string
  deploymentMode: string
  trustStatus: string
  dataEndpointUrl: string | null
}

export interface ProductManifestResponse {
  productKey: string
  displayName: string
  productCategory: string
  productOwner: string
  productStatus: string
  isActive: boolean
  environmentKey: string
  canonicalCallbackPath: string
  launchBaseUrl: string | null
  launchPath: string | null
  launchUrl: string | null
  apiBaseUrl: string
  healthUrl: string
  serviceAudience: string
  marketingUrl: string
  documentationUrl: string
  supportUrl: string
  entitlementDependencyRules: string
  productDependencyMetadata: string
  launchProfileModifiedAt: string | null
  callbackAllowlist: ProductManifestCallbackAllowlistResponse[]
  dataPlaneProfiles: ProductManifestDataPlaneProfileResponse[]
}

export interface CreateProductRequest {
  productKey: string
  displayName: string
  sortOrder: number
  isActive?: boolean
}

export interface UpdateProductRequest {
  displayName: string
  sortOrder: number
  isActive: boolean
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

export interface PlatformAuditPackageCounts {
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

export interface PlatformAuditPackageAppliedFilters {
  tenantId: string | null
  from: string | null
  to: string | null
  action: string | null
  result: string | null
  targetType: string | null
  actorUserId: string | null
  productKey: string | null
}

export interface PlatformAuditPackageFilterOptions {
  actions: string[]
  results: string[]
  targetTypes: string[]
  productKeys: string[]
  actorUserIds: string[]
}

export interface PlatformAuditPackageBreakdownItem {
  key: string
  count: number
}

export interface PlatformAuditPackageExportSummary {
  filters: PlatformAuditPackageAppliedFilters
  counts: PlatformAuditPackageCounts
  byResult: PlatformAuditPackageBreakdownItem[]
  byAction: PlatformAuditPackageBreakdownItem[]
  generatedAt: string
}

export interface PlatformAuditPackageScope {
  from?: string
  to?: string
  tenantId?: string
  action?: string
  result?: string
  targetType?: string
  actorUserId?: string
  productKey?: string
}

export interface PlatformAuditPackageExportPreview {
  packageId: string
  scopeTenantId: string | null
  generatedAt: string
  appliedFilters?: PlatformAuditPackageAppliedFilters | null
  counts: PlatformAuditPackageCounts
}

export interface ServiceTokenCleanupSettings {
  isEnabled: boolean
  retentionDaysAfterExpiry: number
  retentionDaysAfterRevoke: number
  updatedAt: string | null
}

export interface ServiceTokenCleanupRunItem {
  runId: string
  outcome: string
  purgedCount: number
  expiredPurgeCount: number
  revokedPurgeCount: number
  skippedCount: number
  skipReason: string | null
  processedAt: string
}

export interface ServiceTokenCleanupRunsResponse {
  items: ServiceTokenCleanupRunItem[]
}

export interface EntitlementReconciliationSettings {
  isEnabled: boolean
  autoGrantFromLicense: boolean
  autoRevokeStaleEntitlements: boolean
  updatedAt: string | null
}

export interface EntitlementReconciliationRunItem {
  runId: string
  outcome: string
  driftFoundCount: number
  grantedCount: number
  revokedCount: number
  skippedCount: number
  skipReason: string | null
  processedAt: string
}

export interface EntitlementReconciliationRunsResponse {
  items: EntitlementReconciliationRunItem[]
}

export interface PendingEntitlementReconciliationItem {
  tenantId: string
  tenantDisplayName: string
  productKey: string
  productDisplayName: string
  driftKind: string
  entitlementActive: boolean
  licenseValid: boolean
}

export interface PendingEntitlementReconciliationResponse {
  asOfUtc: string
  batchSize: number
  items: PendingEntitlementReconciliationItem[]
}

export interface TenantLifecycleSettings {
  isEnabled: boolean
  autoSuspendWhenNoValidLicense: boolean
  suspendGraceDaysAfterLastLicenseExpiry: number
  autoReactivateWhenValidLicense: boolean
  revokeSessionsOnSuspend: boolean
  updatedAt: string | null
}

export interface TenantLifecycleRunItem {
  runId: string
  outcome: string
  pendingCount: number
  suspendedCount: number
  reactivatedCount: number
  sessionsRevokedCount: number
  skippedCount: number
  skipReason: string | null
  processedAt: string
}

export interface TenantLifecycleRunsResponse {
  items: TenantLifecycleRunItem[]
}

export interface PendingTenantLifecycleItem {
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  currentStatus: string
  actionKind: string
  hasValidLicense: boolean
  lastLicenseCoverageEndedAt: string | null
  eligibleAt: string | null
}

export interface PendingTenantLifecycleResponse {
  asOfUtc: string
  batchSize: number
  items: PendingTenantLifecycleItem[]
}

export interface PlatformLifecycleLatestRunSummary {
  runId: string
  outcome: string
  processedAt: string
  primaryCount: number
  primaryCountLabel: string
}

export interface PlatformLifecycleWorkerStatus {
  workerKey: string
  label: string
  description: string
  isEnabled: boolean
  pendingCount: number
  latestRun: PlatformLifecycleLatestRunSummary | null
  serviceTokenScope: string
  platformSettingsPath: string
  suiteAdminPath: string
}

export interface PlatformLifecycleOverviewResponse {
  generatedAt: string
  workers: PlatformLifecycleWorkerStatus[]
}

export interface ProductHealthProbeResult {
  productKey: string
  status: string
  readyUrl: string | null
  latencyMs: number | null
  errorCode: string | null
  errorMessage: string | null
}

export interface PlatformServiceTokenInventorySummary {
  activeCount: number
  expiringWithin24HoursCount: number
  expiredRetainedCount: number
  revokedRetainedCount: number
  pendingCleanupCount: number
}

export interface PlatformWorkerOrchestrationWorkerStatus {
  workerKey: string
  label: string
  description: string
  isEnabled: boolean
  pendingCount: number
  latestRun: PlatformLifecycleLatestRunSummary | null
  serviceTokenScope: string
  suiteAdminPath: string
}

export interface PlatformWorkerHealthOrchestrationStatusResponse {
  generatedAt: string
  platformHealthStatus: string
  productHealth: ProductHealthProbeResult[]
  serviceTokens: PlatformServiceTokenInventorySummary
  activeServiceClientCount: number
  workers: PlatformWorkerOrchestrationWorkerStatus[]
}

export interface TriggerServiceTokenCleanupOrchestrationResponse {
  asOfUtc: string
  purgedCount: number
  skippedCount: number
}

export interface TriggerEntitlementReconciliationOrchestrationResponse {
  asOfUtc: string
  grantedCount: number
  revokedCount: number
  skippedCount: number
}

export interface TriggerTenantLifecycleOrchestrationResponse {
  asOfUtc: string
  suspendedCount: number
  reactivatedCount: number
  skippedCount: number
}

export interface PlatformOutboxPublisherSettings {
  isEnabled: boolean
  maxRetryAttempts: number
  retryIntervalMinutes: number
  updatedAt: string | null
}

export interface PlatformOutboxPublisherRunItem {
  runId: string
  outcome: string
  publishedCount: number
  failedCount: number
  deadLetterCount: number
  skippedCount: number
  skipReason: string | null
  processedAt: string
}

export interface PlatformOutboxPublisherRunsResponse {
  items: PlatformOutboxPublisherRunItem[]
}

export interface PlatformOutboxPublisherStatusResponse {
  asOfUtc: string
  isEnabled: boolean
  pendingCount: number
  deadLetterCount: number
  latestRun: PlatformOutboxPublisherRunItem | null
}

export interface PlatformOutboxEventItem {
  eventId: string
  eventType: string
  tenantId: string | null
  processingStatus: string
  attemptCount: number
  errorMessage: string | null
  occurredAt: string
  publishedAt: string | null
}

export interface PlatformOutboxEventsListResponse {
  items: PlatformOutboxEventItem[]
}

export interface TriggerPlatformOutboxPublisherOrchestrationResponse {
  asOfUtc: string
  publishedCount: number
  failedCount: number
  deadLetterCount: number
  skippedCount: number
}

export interface EntitlementDetail {
  entitlementId: string
  tenantId: string
  productKey: string
  productDisplayName: string
  status: string
  grantedAt: string
  revokedAt: string | null
}

export interface GrantEntitlementRequest {
  tenantId: string
  productKey: string
}

export interface ServiceClientSummary {
  serviceClientId: string
  clientKey: string
  displayName: string
  sourceProductKey: string
  allowedProductKeys: string[]
  isActive: boolean
  createdAt: string
}

export interface RegisterServiceClientRequest {
  clientKey: string
  displayName: string
  sourceProductKey: string
  allowedProductKeys: string[]
}

export interface IssueServiceTokenRequest {
  serviceClientId: string
  tenantId?: string | null
  allowedProductKeys?: string[] | null
  actionScope?: string | null
  lifetimeMinutes?: number | null
}

export interface ServiceTokenIssueResult {
  accessToken: string
  tokenId: string
  expiresAt: string
  serviceClientId: string
  tenantId: string | null
  allowedProductKeys: string[]
  actionScope: string | null
}

export interface ServiceTokenSummary {
  tokenId: string
  serviceClientId: string
  clientKey: string
  tenantId: string | null
  allowedProductKeys: string[]
  actionScope: string | null
  expiresAt: string
  revokedAt: string | null
  createdAt: string
}

export interface DataPlaneProfile {
  profileId: string
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  productKey: string
  productDisplayName: string
  deploymentMode: string
  dataEndpointUrl: string | null
  trustStatus: string
  notes: string | null
  modifiedAt: string
}

export interface UpsertDataPlaneProfileRequest {
  tenantId: string
  productKey: string
  deploymentMode: string
  dataEndpointUrl?: string | null
  trustStatus: string
  notes?: string | null
}

export interface EffectiveDataPlaneProfile {
  tenantId: string
  productKey: string
  productDisplayName: string
  deploymentMode: string
  trustStatus: string
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
