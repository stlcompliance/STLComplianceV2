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
