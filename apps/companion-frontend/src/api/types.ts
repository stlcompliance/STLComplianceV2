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
  pushDeliveredCount: number | null
  createdAt: string
  dispatchedAt: string | null
}

export interface CompanionPushVapidPublicKeyResponse {
  publicKey: string
}

export interface CompanionPushSubscriptionKeys {
  p256dh: string
  auth: string
}

export interface UpsertCompanionPushSubscriptionRequest {
  endpoint: string
  keys: CompanionPushSubscriptionKeys
  userAgent?: string | null
}

export interface UnsubscribeCompanionPushRequest {
  endpoint: string
}

export interface CompanionPushSubscriptionResponse {
  subscriptionId: string
  endpoint: string
  updatedAt: string
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

export interface CompanionOfflineActionRejectedItem {
  idempotencyKey: string
  reasonCode: string
  reasonMessage: string
}

export interface SyncCompanionOfflineActionsResponse {
  accepted: number
  duplicates: number
  rejected: number
  synced: CompanionOfflineActionSyncedItem[]
  rejectedItems: CompanionOfflineActionRejectedItem[]
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

export interface SubmitCompanionFieldDvirRequest {
  taskKey: string
  phase: string
  result: string
  odometerReading: number | null
  defectNotes: string | null
  vehicleRefKey: string | null
}

export interface CompanionFieldDvirResponse {
  taskKey: string
  productKey: string
  dvirId: string
  tripId: string
  phase: string
  result: string
  odometerReading: number | null
  defectNotes: string
  submittedAt: string
}

export interface CompanionFieldInspectionChecklistItem {
  checklistItemId: string
  itemKey: string
  prompt: string
  itemType: string
  isRequired: boolean
  sortOrder: number
}

export interface CompanionFieldInspectionAnswer {
  checklistItemId: string
  itemKey: string
  passFailValue: string | null
  numericValue: number | null
  textValue: string | null
  answeredAt: string
}

export interface CompanionFieldInspectionDetailResponse {
  taskKey: string
  productKey: string
  inspectionRunId: string
  assetTag: string
  assetName: string
  templateName: string
  status: string
  result: string | null
  checklistItems: CompanionFieldInspectionChecklistItem[]
  answers: CompanionFieldInspectionAnswer[]
}

export interface CompanionFieldInspectionAnswerInput {
  checklistItemId: string
  passFailValue: string | null
  numericValue: number | null
  textValue: string | null
}

export interface SubmitCompanionFieldInspectionAnswersRequest {
  taskKey: string
  answers: CompanionFieldInspectionAnswerInput[]
}

export interface CompanionFieldInspectionAnswersResponse {
  taskKey: string
  productKey: string
  inspectionRunId: string
  status: string
  answerCount: number
  requiredItemCount: number
  answers: CompanionFieldInspectionAnswer[]
}

export interface CompleteCompanionFieldInspectionRequest {
  taskKey: string
}

export interface CompanionFieldInspectionCompleteResponse {
  taskKey: string
  productKey: string
  inspectionRunId: string
  status: string
  result: string
  completedAt: string
}

export interface CompanionFieldWorkOrderTaskLine {
  taskLineId: string
  title: string
  description: string
  sortOrder: number
  status: string
  completedAt: string | null
}

export interface CompanionFieldWorkOrderLaborEntry {
  laborEntryId: string
  personId: string
  hoursWorked: number
  laborTypeKey: string
  notes: string | null
  loggedAt: string
}

export interface CompanionFieldWorkOrderDetailResponse {
  taskKey: string
  productKey: string
  workOrderId: string
  workOrderNumber: string
  assetTag: string
  assetName: string
  title: string
  description: string
  priority: string
  status: string
  tasks: CompanionFieldWorkOrderTaskLine[]
  laborEntries: CompanionFieldWorkOrderLaborEntry[]
}

export interface UpdateCompanionFieldWorkOrderStatusRequest {
  taskKey: string
  status: string
}

export interface CompanionFieldWorkOrderStatusResponse {
  taskKey: string
  productKey: string
  workOrderId: string
  status: string
  updatedAt: string
}

export interface LogCompanionFieldWorkOrderLaborRequest {
  taskKey: string
  hoursWorked: number
  laborTypeKey: string
  notes: string | null
  workOrderTaskLineId: string | null
}

export interface CompanionFieldWorkOrderLaborResponse {
  taskKey: string
  productKey: string
  workOrderId: string
  laborEntryId: string
  hoursWorked: number
  laborTypeKey: string
  status: string
  loggedAt: string
}

export interface CompanionFieldReceivingLine {
  lineId: string
  lineNumber: number
  partKey: string
  partDisplayName: string
  quantityExpected: number
  quantityReceived: number
  quantityOrdered: number
  quantityRemainingOnOrder: number
  openExceptionCount: number
}

export interface CompanionFieldReceivingDetailResponse {
  taskKey: string
  productKey: string
  receivingReceiptId: string
  receiptKey: string
  status: string
  purchaseOrderKey: string
  binKey: string
  binName: string
  locationName: string
  notes: string
  lines: CompanionFieldReceivingLine[]
}

export interface UpdateCompanionFieldReceivingLineRequest {
  taskKey: string
  lineId: string
  quantityReceived: number
}

export interface CompanionFieldReceivingLineResponse {
  taskKey: string
  productKey: string
  receivingReceiptId: string
  lineId: string
  quantityReceived: number
  status: string
  updatedAt: string
}

export interface PostCompanionFieldReceivingRequest {
  taskKey: string
}

export interface CompanionFieldReceivingPostResponse {
  taskKey: string
  productKey: string
  receivingReceiptId: string
  status: string
  postedAt: string
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
