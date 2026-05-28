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
