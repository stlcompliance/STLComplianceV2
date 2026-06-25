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
  rememberDevice: boolean
  mfaCode?: string | null
  recoveryCode?: string | null
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
  requiresPasswordChange: boolean
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  themePreference?: 'dark' | 'light' | 'system' | string
  entitlements: string[]
}

export interface UserPreferencesResponse {
  themePreference: 'dark' | 'light' | 'system' | string
}

export interface UpdateMyPasswordRequest {
  currentPassword: string
  newPassword: string
}

export interface UpdateMyPasswordResponse {
  passwordChangedAt: string
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
  isRemembered: boolean
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

export interface ReferenceDataDashboardResponse {
  datasetCount: number
  sourceCount: number
  jobCount: number
  pendingReviewCount: number
  failedImportCount: number
  publishedEntityCount: number
  crosswalkCount: number
  publishEventCount: number
  generatedAt: string
}

export interface ReferenceDatasetResponse {
  id: string
  key: string
  name: string
  category: string
  ownerService: string
  status: string
  currentPublishedVersion: string | null
  sourceCount: number
  entityCount: number
  pendingReviewCount: number
  failedImportCount: number
  lastPublishedAt: string | null
  createdAt: string
  updatedAt: string
}

export interface ReferenceSourceResponse {
  id: string
  key: string
  name: string
  sourceType: string
  connectorType: string
  authorityRank: number
  refreshCadence: string
  termsNotes: string | null
  enabled: boolean
  createdAt: string
  updatedAt: string
}

export interface ReferenceImportResponse {
  id: string
  datasetId: string
  datasetKey: string
  datasetName: string
  sourceId: string
  sourceKey: string
  sourceName: string
  tenantId: string | null
  requestedByPersonId: string | null
  status: string
  rawObjectKey: string | null
  fileName: string | null
  startedAt: string
  completedAt: string | null
  errorSummary: string | null
  stagingRecordCount: number
  pendingReviewCount: number
  approvedCount: number
  rejectedCount: number
  createdAt: string
  updatedAt: string
}

export interface ReferenceStagingRecordResponse {
  id: string
  jobId: string
  datasetId: string
  datasetKey: string
  sourceId: string
  sourceKey: string
  targetDatasetId: string | null
  targetDatasetKey: string | null
  targetDatasetName: string | null
  targetOwnerService: string | null
  rowNumber: number | null
  rawPayloadJson: string
  normalizedPayloadJson: string
  proposedEntityType: string
  proposedCanonicalKey: string | null
  confidence: number
  status: string
  reviewReason: string | null
  reviewerPersonId: string | null
  reviewedAt: string | null
  referenceEntityId: string | null
  createdAt: string
  updatedAt: string
}

export interface ReviewDecisionRequest {
  reason: string | null
  displayName: string | null
  canonicalKey: string | null
  normalizedFieldsJson: string | null
  sourceEvidenceJson: string | null
  effectiveDate: string | null
  targetDatasetId: string | null
}

export interface ReferenceCrosswalkResponse {
  id: string
  referenceEntityId: string
  entityType: string
  canonicalKey: string
  displayName: string
  externalSystem: string
  externalKey: string
  sourceId: string | null
  sourceKey: string | null
  confidence: number
  status: string
  createdAt: string
  updatedAt: string
}

export interface ReferenceTenantOverlayResponse {
  id: string
  tenantId: string
  referenceEntityId: string
  entityType: string
  canonicalKey: string
  localName: string | null
  localStatus: string | null
  hidden: boolean
  notes: string | null
  createdAt: string
  updatedAt: string
}

export interface ReferenceProductMappingResponse {
  id: string
  tenantId: string
  productCode: string
  referenceEntityId: string
  entityType: string
  canonicalKey: string
  localEntityType: string
  localEntityId: string
  mappingStatus: string
  createdAt: string
  updatedAt: string
}

export interface ReferenceEntityVersionResponse {
  id: string
  referenceEntityId: string
  version: number
  fieldsJson: string
  sourceEvidenceJson: string
  effectiveDate: string | null
  publishedAt: string | null
  supersededByVersionId: string | null
  createdAt: string
  updatedAt: string
}

export interface ReferenceEntityListItemResponse {
  id: string
  datasetId: string
  datasetKey: string
  datasetName: string
  entityType: string
  canonicalKey: string
  displayName: string
  status: string
  currentVersion: number | null
  publishedAt: string | null
  createdAt: string
  updatedAt: string
}

export interface ReferenceEntityResponse {
  id: string
  datasetId: string
  datasetKey: string
  datasetName: string
  entityType: string
  canonicalKey: string
  displayName: string
  status: string
  normalizedFieldsJson: string
  firstSeenSourceId: string | null
  firstSeenSourceKey: string | null
  currentVersionId: string | null
  currentVersion: number | null
  publishedAt: string | null
  createdAt: string
  updatedAt: string
  versions: ReferenceEntityVersionResponse[]
  crosswalks: ReferenceCrosswalkResponse[]
  tenantOverlays: ReferenceTenantOverlayResponse[]
  productMappings: ReferenceProductMappingResponse[]
}

export interface ReferencePublishEventResponse {
  id: string
  datasetId: string
  datasetKey: string
  datasetName: string
  publishedVersion: string
  publishedByPersonId: string | null
  summary: string
  createdAt: string
}

export interface CreateReferenceDatasetRequest {
  key: string
  name: string
  category: string
  ownerService: string
  status: string
}

export interface PublishReferenceDatasetsRequest {
  datasetIds: string[]
  summary?: string | null
}

export interface ReferencePublishBatchResponse {
  requestedCount: number
  publishedCount: number
  items: ReferencePublishEventResponse[]
  processedAt: string
}

export interface CreateReferenceSourceRequest {
  key: string
  name: string
  sourceType: string
  connectorType: string
  authorityRank: number
  refreshCadence: string
  termsNotes: string | null
  enabled: boolean
}

export interface CreateReferenceDatasetInputRequest {
  rawObjectKey?: string | null
  fileName?: string | null
  value?: string | null
  valuesText?: string | null
}

export interface UpdateReferenceEntityRequest {
  displayName?: string | null
  canonicalKey?: string | null
  normalizedFieldsJson?: string | null
  sourceEvidenceJson?: string | null
  effectiveDate?: string | null
}

export interface CreateReferenceMasterCsvImportRequest {
  csvText: string
  fileName?: string | null
  rawObjectKey?: string | null
}

export interface CreateReferenceImportRequest {
  datasetId: string
  sourceId: string
  tenantId: string | null
  requestedByPersonId: string | null
  rawObjectKey: string | null
  fileName: string | null
  records?: ReferenceImportRecordInput[] | null
}

export interface ReferenceImportRecordInput {
  rowNumber: number | null
  rawPayloadJson: string
  normalizedPayloadJson: string | null
  proposedEntityType: string
  proposedCanonicalKey: string | null
  confidence: number
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
  subscriptionTier: string
  billingCustomerId: string | null
  billingSubscriptionId: string | null
  billingGraceDays: number | null
  isTrial: boolean
  isInternalTenant: boolean
  createdAt: string
  modifiedAt: string
}

export interface TenantMemberResponse {
  membershipId: string
  userId: string
  email: string
  displayName: string
  roleKey: string
  isActive: boolean
  createdAt: string
}

export interface TenantMembersListResponse {
  tenantId: string
  members: TenantMemberResponse[]
}

export interface CreateTenantRequest {
  slug: string
  displayName: string
  subscriptionTier: string
  billingCustomerId: string | null
  billingSubscriptionId: string | null
  billingGraceDays: number | null
  isTrial: boolean
  isInternalTenant: boolean
}

export interface UpdateTenantRequest {
  displayName: string
  subscriptionTier: string
  billingCustomerId: string | null
  billingSubscriptionId: string | null
  billingGraceDays: number | null
  isTrial: boolean
  isInternalTenant: boolean
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

export interface DatabaseNukeTablePreview {
  schema: string
  table: string
  disposition: string
  reason: string
  estimatedRows: number
}

export interface DatabaseNukeTargetPreview {
  productDatabase: string
  status: string
  connectionConfigured: boolean
  tableCount: number
  truncateTableCount: number
  preserveTableCount: number
  estimatedRowsToDelete: number
  estimatedRowsPreserved: number
  tablesToTruncate: DatabaseNukeTablePreview[]
  preservedTables: DatabaseNukeTablePreview[]
  errorCode: string | null
  errorMessage: string | null
}

export interface DatabaseNukePreviewResponse {
  isEnabled: boolean
  confirmationPhrase: string
  targets: DatabaseNukeTargetPreview[]
  generatedAt: string
}

export interface ExecuteDatabaseNukeRequest {
  confirmationPhrase: string
  reason: string
}

export interface DatabaseNukeTargetExecution {
  productDatabase: string
  status: string
  truncatedTableCount: number
  preservedTableCount: number
  estimatedRowsDeleted: number
  errorCode: string | null
  errorMessage: string | null
}

export interface DatabaseNukeExecutionResponse {
  runId: string
  targets: DatabaseNukeTargetExecution[]
  truncatedTableCount: number
  preservedTableCount: number
  estimatedRowsDeleted: number
  startedAt: string
  completedAt: string
}

export interface ProductDetailResponse {
  productKey: string
  displayName: string
  sortOrder: number
  isActive: boolean
  productCategory: string
  productOwner: string
  productStatus: string
  canonicalCallbackPath: string
  apiBaseUrl: string
  healthUrl: string
  serviceAudience: string
  marketingUrl: string
  documentationUrl: string
  supportUrl: string
  environmentKey: string
  entitlementDependencyRules: string
  availabilityDependencyRules?: string
}

export interface PlatformUserListItemResponse {
  userId: string
  email: string
  displayName: string
  isActive: boolean
  isPlatformAdmin: boolean
  failedLoginCount: number
  lockedUntil: string | null
  createdAt: string
  modifiedAt: string
  lastLoginAt: string | null
  lastProductLaunchAt: string | null
  canLogin: boolean
  status: string
  isMfaEnabled: boolean
}

export interface PlatformUserDetailResponse {
  userId: string
  email: string
  displayName: string
  isActive: boolean
  isPlatformAdmin: boolean
  failedLoginCount: number
  lockedUntil: string | null
  createdAt: string
  modifiedAt: string
  lastLoginAt: string | null
  lastProductLaunchAt: string | null
  canLogin: boolean
  status: string
  isMfaEnabled: boolean
}

export interface CreatePlatformUserRequest {
  email: string
  displayName: string
  password: string
  isPlatformAdmin?: boolean
  isActive?: boolean
  requireEmailVerification?: boolean
}

export interface InvitePlatformUserRequest {
  email: string
  displayName: string
  isPlatformAdmin?: boolean
  isActive?: boolean
}

export interface PlatformUserEnableResponse {
  userId: string
  wasAlreadyEnabled: boolean
}

export interface PlatformUserDisableResponse {
  userId: string
  wasAlreadyDisabled: boolean
}

export interface PlatformUserLockResponse {
  userId: string
  wasAlreadyLocked: boolean
  lockedUntil: string | null
}

export interface PlatformUserUnlockResponse {
  userId: string
  wasAlreadyUnlocked: boolean
}

export interface AdminResetUserPasswordResponse {
  userId: string
  passwordChangedAt: string
}

export interface PlatformUserMfaResponse {
  userId: string
  isMfaEnabled: boolean
  wasAlreadySet: boolean
  modifiedAt: string
  mfaSecret: string | null
  provisioningUri: string | null
  recoveryCodes: string[] | null
}

export interface PlatformUserSessionItemResponse {
  sessionId: string
  createdAt: string
  expiresAt: string
  revokedAt: string | null
  userAgent: string | null
  ipAddress: string | null
  activeTenantId: string | null
  isCurrent: boolean
  isActive: boolean
  isRemembered: boolean
}

export interface PlatformUserSessionsResponse {
  userId: string
  sessions: PlatformUserSessionItemResponse[]
}

export interface PlatformUserTenantMembershipItemResponse {
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  roleKey: string
  isActive: boolean
  createdAt: string
}

export interface PlatformUserTenantMembershipsResponse {
  userId: string
  items: PlatformUserTenantMembershipItemResponse[]
}

export interface AssignPlatformUserTenantMembershipRequest {
  tenantId: string
  roleKey?: string
}

export interface AssignPlatformUserTenantMembershipResponse {
  userId: string
  tenantId: string
  wasReactivated: boolean
}

export interface RemovePlatformUserTenantMembershipResponse {
  userId: string
  tenantId: string
  wasAlreadyRemoved: boolean
}

export interface PlatformUserRoleItemResponse {
  roleKey: string
  isAssigned: boolean
  tenantId: string | null
}

export interface PlatformUserRolesResponse {
  userId: string
  items: PlatformUserRoleItemResponse[]
}

export interface AssignPlatformUserRoleRequest {
  roleKey: string
  tenantId?: string | null
}

export interface AssignPlatformUserRoleResponse {
  userId: string
  roleKey: string
  wasAlreadyAssigned: boolean
  tenantId: string | null
}

export interface RemovePlatformUserRoleResponse {
  userId: string
  roleKey: string
  wasAlreadyRemoved: boolean
  tenantId: string | null
}

export interface PlatformUserExternalIdentityProviderMappingItemResponse {
  mappingId: string
  userId: string
  providerKey: string
  externalSubject: string
  externalEmail: string | null
  createdAt: string
  modifiedAt: string
}

export interface PlatformUserExternalIdentityProviderMappingsResponse {
  userId: string
  items: PlatformUserExternalIdentityProviderMappingItemResponse[]
}

export interface UpsertPlatformUserExternalIdentityProviderMappingRequest {
  providerKey: string
  externalSubject: string
  externalEmail?: string | null
}

export interface UpsertPlatformUserExternalIdentityProviderMappingResponse {
  mappingId: string
  userId: string
  providerKey: string
  externalSubject: string
  externalEmail: string | null
  wasUpdated: boolean
}

export interface RemovePlatformUserExternalIdentityProviderMappingResponse {
  userId: string
  mappingId: string
  wasAlreadyRemoved: boolean
}

export interface PlatformUserAccessHistoryItemResponse {
  auditEventId: string
  userId: string
  userEmail: string | null
  userDisplayName: string | null
  tenantId: string | null
  tenantSlug: string | null
  action: string
  result: string
  reasonCode: string | null
  targetType: string
  targetId: string | null
  correlationId: string
  occurredAt: string
  productKey: string | null
  productDisplayName: string | null
}

export interface PlatformUserIdentityAuditHistoryItemResponse {
  auditEventId: string
  userId: string
  userEmail: string | null
  userDisplayName: string | null
  tenantId: string | null
  tenantSlug: string | null
  actorUserId: string | null
  actorEmail: string | null
  actorDisplayName: string | null
  action: string
  result: string
  reasonCode: string | null
  targetType: string
  targetId: string | null
  correlationId: string
  occurredAt: string
}

export interface PlatformUsersListResponse {
  totalCount: number
  page: number
  pageSize: number
  hasNextPage: boolean
  items: PlatformUserListItemResponse[]
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
  availabilityDependencyRules?: string
  productDependencyMetadata: string
  launchProfileModifiedAt: string | null
  callbackAllowlist: ProductManifestCallbackAllowlistResponse[]
  dataPlaneProfiles: ProductManifestDataPlaneProfileResponse[]
}

export interface CallbackAllowlistEntryResponse {
  entryId: string
  productKey: string
  tenantId: string | null
  urlPattern: string
  patternType: string
  isActive: boolean
  createdAt: string
}

export interface CreateCallbackAllowlistEntryRequest {
  productKey: string
  tenantId: string | null
  urlPattern: string
  patternType: string
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

export interface PlatformSessionSettings {
  accessTokenMinutes: number
  refreshTokenDays: number
  rememberedRefreshTokenDays: number
  requirePlatformAdminMfa: boolean
  passwordMinLength: number
  requirePasswordComplexity: boolean
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

export interface PlatformHealthResponse {
  status: string
  timestampUtc: string
  products: ProductHealthProbeResult[]
}

export interface HealthResponse {
  status: string
  product: string
  version: string
  timestampUtc: string
  checks?: Record<string, unknown> | null
}

export interface ProductHealthProbeResult {
  productKey: string
  status: string
  readyUrl: string | null
  latencyMs: number | null
  errorCode: string | null
  errorMessage: string | null
  detail?: HealthResponse | null
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

export interface TenantAvailabilityRecord {
  entitlementId: string
  tenantId: string
  productKey: string
  productDisplayName: string
  status: string
  grantedAt: string
  revokedAt: string | null
}

export interface TenantAvailabilityRequest {
  tenantId: string
  productKey: string
}

export interface ServiceClientSummary {
  serviceClientId: string
  clientKey: string
  displayName: string
  sourceProductKey: string
  allowedProductKeys: string[]
  allowedTenantIds: string[]
  isActive: boolean
  createdAt: string
  lastUsedAt: string | null
  failedAuthenticationAttempts: number
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

export interface ServiceTokenDiscoveryResponse {
  issuer: string
  audience: string
  jwksUri: string
  supportedAlgorithms: string[]
  publicKeyAvailable: boolean
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

export interface ValidateDataPlaneProfileRequest {
  tenantId: string
  productKey: string
  deploymentMode: string
  dataEndpointUrl?: string | null
  notes?: string | null
}

export interface ValidateDataPlaneProfileResponse {
  profile: DataPlaneProfile
  validationStatus: string
  readyUrl: string | null
  latencyMs: number | null
  errorCode: string | null
  errorMessage: string | null
  validatedAt: string
}

export interface EffectiveDataPlaneProfile {
  tenantId: string
  productKey: string
  productDisplayName: string
  deploymentMode: string
  trustStatus: string
}

export interface TenantIntegrationRouteResponse {
  routeKey: string
  method: string
  path: string
  description: string
}

export interface TenantIntegrationBrandResponse {
  mark: string
  accentColor: string
  backgroundColor: string
  textColor: string
  websiteUrl: string
  assetSourceUrl: string
  assetSourceLabel: string
  usageNote: string
}

export interface TenantIntegrationProviderResponse {
  providerKey: string
  displayName: string
  category: string
  brand: TenantIntegrationBrandResponse
  connectorFamily: string
  authType: string
  defaultDirection: string
  supportsWriteback: boolean
  requiresManualMapping: boolean
  owningProducts: string[]
  capabilities: string[]
  routes: TenantIntegrationRouteResponse[]
}

export interface TenantIntegrationCatalogResponse {
  providers: TenantIntegrationProviderResponse[]
}

export interface TenantIntegrationCredentialSummaryResponse {
  credentialId: string
  credentialKind: string
  redactedLabel: string
  encryptionKeyId: string
  expiresAt: string | null
  lastValidatedAt: string | null
  updatedAt: string
}

export interface TenantIntegrationHealthResponse {
  status: string
  checkedAt: string | null
  latencyMs: number | null
  errorCategory: string | null
  errorMessage: string | null
}

export interface TenantIntegrationSyncRunResponse {
  syncRunId: string
  tenantId: string
  connectionId: string
  providerKey: string
  status: string
  direction: string
  triggeredBy: string
  attemptCount: number
  startedAt: string
  completedAt: string | null
  nextRetryAt: string | null
  snapshotCount: number
  mappingCount: number
  errorCategory: string | null
  errorMessage: string | null
  destinationProductsJson: string
  resultSummaryJson: string
}

export interface TenantIntegrationConnectionResponse {
  connectionId: string
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  providerKey: string
  providerDisplayName: string
  category: string
  brand: TenantIntegrationBrandResponse
  status: string
  syncDirection: string
  writebacksEnabled: boolean
  manualMappingRequired: boolean
  configurationJson: string
  lastSuccessfulSyncAt: string | null
  lastFailedSyncAt: string | null
  lastErrorCategory: string | null
  lastErrorMessage: string | null
  credential: TenantIntegrationCredentialSummaryResponse | null
  health: TenantIntegrationHealthResponse | null
  latestSyncRun: TenantIntegrationSyncRunResponse | null
  routes: TenantIntegrationRouteResponse[]
  createdAt: string
  updatedAt: string
}

export interface UpsertTenantIntegrationConnectionRequest {
  status?: string | null
  syncDirection?: string | null
  writebacksEnabled?: boolean | null
  manualMappingRequired?: boolean | null
  configurationJson?: string | null
}

export interface UpsertTenantIntegrationCredentialRequest {
  credentialKind: string
  secretLabel: string
  payload: Record<string, string>
  expiresAt?: string | null
}

export interface TestTenantIntegrationConnectionResponse {
  connectionId: string
  providerKey: string
  status: string
  errorCategory: string | null
  errorMessage: string | null
  latencyMs: number | null
  checkedAt: string
}

export interface TriggerTenantIntegrationSyncRequest {
  idempotencyKey?: string | null
  force?: boolean
}

export interface TenantIntegrationMappingTemplateResponse {
  mappingTemplateId: string
  tenantId: string
  connectionId: string
  providerKey: string
  templateName: string
  sourceEntityType: string
  targetProductKey: string
  targetEntityType: string
  mappingJson: string
  isActive: boolean
  updatedAt: string
}

export interface UpsertTenantIntegrationMappingTemplateRequest {
  templateName: string
  sourceEntityType: string
  targetProductKey: string
  targetEntityType: string
  mappingJson: string
  isActive?: boolean
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
