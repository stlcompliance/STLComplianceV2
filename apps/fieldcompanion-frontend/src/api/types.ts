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

export interface FieldCompanionNotificationSettingsResponse {
  isEnabled: boolean
  notificationWebhookUrl: string | null
  notifyOnHandoffRedeemed: boolean
  notifyOnFieldInboxRefreshed: boolean
  updatedAt: string | null
}

export interface UpsertFieldCompanionNotificationSettingsRequest {
  isEnabled: boolean
  notificationWebhookUrl: string | null
  notifyOnHandoffRedeemed: boolean
  notifyOnFieldInboxRefreshed: boolean
}

export interface FieldCompanionNotificationDispatchItem {
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

export interface FieldCompanionPushVapidPublicKeyResponse {
  publicKey: string
}

export interface FieldCompanionPushSubscriptionKeys {
  p256dh: string
  auth: string
}

export interface UpsertFieldCompanionPushSubscriptionRequest {
  endpoint: string
  keys: FieldCompanionPushSubscriptionKeys
  userAgent?: string | null
}

export interface UnsubscribeFieldCompanionPushRequest {
  endpoint: string
}

export interface FieldCompanionPushSubscriptionResponse {
  subscriptionId: string
  endpoint: string
  updatedAt: string
}

export interface FieldCompanionNotificationDispatchesResponse {
  items: FieldCompanionNotificationDispatchItem[]
}

export interface AggregatedFieldInboxResponse {
  summary: FieldInboxSummary
  items: FieldInboxTaskItem[]
  sources: FieldInboxProductSlice[]
}

export interface FieldCompanionSessionResponse {
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
  callbackUrl: string | null
}

export interface FieldCompanionOfflineActionPayload {
  idempotencyKey: string
  actionKind: string
  taskKey: string
  productKey: string
  clientCreatedAt: string
}

export interface SyncFieldCompanionOfflineActionsRequest {
  actions: FieldCompanionOfflineActionPayload[]
}

export interface FieldCompanionOfflineActionSyncedItem {
  idempotencyKey: string
  actionKind: string
  taskKey: string
  productKey: string
  syncedAt: string
}

export interface FieldCompanionOfflineActionRejectedItem {
  idempotencyKey: string
  reasonCode: string
  reasonMessage: string
}

export interface SyncFieldCompanionOfflineActionsResponse {
  accepted: number
  duplicates: number
  rejected: number
  synced: FieldCompanionOfflineActionSyncedItem[]
  rejectedItems: FieldCompanionOfflineActionRejectedItem[]
}

export interface FieldCompanionOfflineActionsListResponse {
  items: FieldCompanionOfflineActionSyncedItem[]
}

export interface ValidateFieldCompanionFieldTaskRequest {
  taskKey: string
  submissionKind: string
  productKey?: string | null
}

export interface ValidateFieldCompanionFieldTaskResponse {
  allowed: boolean
  reasonCode: string | null
  reasonMessage: string | null
  taskKey: string
  productKey: string
  title: string | null
  blockedReason: string | null
}

export interface SubmitFieldCompanionFieldEvidenceRequest {
  taskKey: string
  captureKind: string
  fileName: string
  contentType: string
  contentBase64: string
  notes: string | null
}

export interface FieldCompanionFieldEvidenceResponse {
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

export interface SubmitFieldCompanionFieldDvirRequest {
  taskKey: string
  phase: string
  result: string
  odometerReading: number | null
  defectNotes: string | null
  vehicleRefKey: string | null
}

export interface FieldCompanionFieldDvirResponse {
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

export interface FieldCompanionFieldInspectionChecklistItem {
  checklistItemId: string
  itemKey: string
  prompt: string
  itemType: string
  isRequired: boolean
  sortOrder: number
}

export interface FieldCompanionFieldInspectionAnswer {
  checklistItemId: string
  itemKey: string
  passFailValue: string | null
  numericValue: number | null
  textValue: string | null
  answeredAt: string
}

export interface FieldCompanionFieldInspectionDetailResponse {
  taskKey: string
  productKey: string
  inspectionRunId: string
  assetTag: string
  assetName: string
  templateName: string
  status: string
  result: string | null
  checklistItems: FieldCompanionFieldInspectionChecklistItem[]
  answers: FieldCompanionFieldInspectionAnswer[]
}

export interface FieldCompanionFieldInspectionAnswerInput {
  checklistItemId: string
  passFailValue: string | null
  numericValue: number | null
  textValue: string | null
}

export interface SubmitFieldCompanionFieldInspectionAnswersRequest {
  taskKey: string
  answers: FieldCompanionFieldInspectionAnswerInput[]
}

export interface FieldCompanionFieldInspectionAnswersResponse {
  taskKey: string
  productKey: string
  inspectionRunId: string
  status: string
  answerCount: number
  requiredItemCount: number
  answers: FieldCompanionFieldInspectionAnswer[]
}

export interface CompleteFieldCompanionFieldInspectionRequest {
  taskKey: string
}

export interface FieldCompanionFieldInspectionCompleteResponse {
  taskKey: string
  productKey: string
  inspectionRunId: string
  status: string
  result: string
  completedAt: string
}

export interface FieldCompanionFieldWorkOrderTaskLine {
  taskLineId: string
  title: string
  description: string
  sortOrder: number
  status: string
  completedAt: string | null
}

export interface FieldCompanionFieldWorkOrderLaborEntry {
  laborEntryId: string
  personId: string
  hoursWorked: number
  laborTypeKey: string
  notes: string | null
  loggedAt: string
}

export interface FieldCompanionFieldWorkOrderDetailResponse {
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
  tasks: FieldCompanionFieldWorkOrderTaskLine[]
  laborEntries: FieldCompanionFieldWorkOrderLaborEntry[]
}

export interface UpdateFieldCompanionFieldWorkOrderStatusRequest {
  taskKey: string
  status: string
}

export interface FieldCompanionFieldWorkOrderStatusResponse {
  taskKey: string
  productKey: string
  workOrderId: string
  status: string
  updatedAt: string
}

export interface LogFieldCompanionFieldWorkOrderLaborRequest {
  taskKey: string
  hoursWorked: number
  laborTypeKey: string
  notes: string | null
  workOrderTaskLineId: string | null
}

export interface FieldCompanionFieldWorkOrderLaborResponse {
  taskKey: string
  productKey: string
  workOrderId: string
  laborEntryId: string
  hoursWorked: number
  laborTypeKey: string
  status: string
  loggedAt: string
}

export interface FieldCompanionFieldReceivingLine {
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

export interface FieldCompanionFieldReceivingDetailResponse {
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
  lines: FieldCompanionFieldReceivingLine[]
}

export interface UpdateFieldCompanionFieldReceivingLineRequest {
  taskKey: string
  lineId: string
  quantityReceived: number
}

export interface FieldCompanionFieldReceivingLineResponse {
  taskKey: string
  productKey: string
  receivingReceiptId: string
  lineId: string
  quantityReceived: number
  status: string
  updatedAt: string
}

export interface PostFieldCompanionFieldReceivingRequest {
  taskKey: string
}

export interface FieldCompanionFieldReceivingPostResponse {
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

export interface FieldCompanionScanResolveRequest {
  scannedValue: string
  symbology?: string | null
}

export interface FieldCompanionScanResolveResponse {
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

export interface FieldCompanionMeResponse {
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
