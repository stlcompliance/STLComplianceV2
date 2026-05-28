export interface FieldInboxTaskItem {
  taskKey: string
  productKey: string
  taskType: string
  title: string
  subtitle: string | null
  status: string
  priority: string | null
  dueAt: string | null
  sortAt: string | null
  deepLinkPath: string
  blockedReason?: string | null
  deepLinkUrl?: string | null
}

export interface FieldInboxSummary {
  totalCount: number
  blockedCount: number
  countByProduct: Record<string, number>
}

export interface FieldInboxProductSlice {
  productKey: string
  entitled: boolean
  fetched: boolean
  errorCode: string | null
  errorMessage: string | null
  items: FieldInboxTaskItem[]
}

export interface CompanionNotificationSettingsResponse {
  isEnabled: boolean
  notificationWebhookUrl: string | null
  notifyOnHandoffRedeemed: boolean
  notifyOnFieldInboxRefreshed: boolean
  updatedAt: string | null
}

export interface UpsertCompanionNotificationSettingsRequest {
  isEnabled: boolean
  notificationWebhookUrl: string | null
  notifyOnHandoffRedeemed: boolean
  notifyOnFieldInboxRefreshed: boolean
}

export interface CompanionNotificationDispatchItem {
  notificationId: string
  eventKind: string
  dispatchStatus: string
  actorUserId: string | null
  relatedEntityType: string
  relatedEntityId: string
  webhookHost: string | null
  httpStatusCode: number | null
  errorMessage: string | null
  createdAt: string
  dispatchedAt: string | null
}

export interface CompanionNotificationDispatchesResponse {
  items: CompanionNotificationDispatchItem[]
}

export interface AggregatedFieldInboxResponse {
  summary: FieldInboxSummary
  items: FieldInboxTaskItem[]
  sources: FieldInboxProductSlice[]
}

export interface CompanionSessionResponse {
  accessToken: string
  refreshToken: string
  accessExpiresAt: string
  refreshExpiresAt: string
  sessionId: string
  userId: string
  personId: string
  email: string
  displayName: string
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  entitlements: string[]
}

export interface CompanionOfflineActionPayload {
  idempotencyKey: string
  actionKind: string
  taskKey: string
  productKey: string
  clientCreatedAt: string
}

export interface SyncCompanionOfflineActionsRequest {
  actions: CompanionOfflineActionPayload[]
}

export interface CompanionOfflineActionSyncedItem {
  idempotencyKey: string
  actionKind: string
  taskKey: string
  productKey: string
  syncedAt: string
}

export interface SyncCompanionOfflineActionsResponse {
  accepted: number
  duplicates: number
  synced: CompanionOfflineActionSyncedItem[]
}

export interface CompanionOfflineActionsListResponse {
  items: CompanionOfflineActionSyncedItem[]
}

export interface ValidateCompanionFieldTaskRequest {
  taskKey: string
  submissionKind: string
  productKey?: string | null
}

export interface ValidateCompanionFieldTaskResponse {
  allowed: boolean
  reasonCode: string | null
  reasonMessage: string | null
  taskKey: string
  productKey: string
  title: string | null
  blockedReason: string | null
}

export interface SubmitCompanionFieldEvidenceRequest {
  taskKey: string
  captureKind: string
  fileName: string
  contentType: string
  contentBase64: string
  notes: string | null
}

export interface CompanionFieldEvidenceResponse {
  taskKey: string
  productKey: string
  evidenceId: string
  evidenceTypeKey: string
  fileName: string
  contentType: string
  sizeBytes: number
  notes: string | null
  createdAt: string
}

export interface FieldTaskSubmissionStatusItem {
  taskKey: string
  submissionKind: string
  status: string
  detailMessage: string | null
  recordedAt: string
}

export interface FieldTaskSubmissionStatusResponse {
  items: FieldTaskSubmissionStatusItem[]
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

export interface CompanionScanResolveRequest {
  scannedValue: string
  symbology?: string | null
}

export interface CompanionScanResolveResponse {
  outcome: 'resolved' | 'denied'
  reasonCode: string | null
  reasonMessage: string | null
  taskKey: string | null
  productKey: string | null
  taskType: string | null
  title: string | null
  subtitle: string | null
  status: string | null
  deepLinkPath: string | null
  deepLinkUrl: string | null
  blockedReason: string | null
}

export interface CompanionMeResponse {
  userId: string
  personId: string
  email: string
  displayName: string
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  entitlements: string[]
  fieldProductKeys: string[]
}
