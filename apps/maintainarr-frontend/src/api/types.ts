export interface HandoffSessionResponse {
  accessToken: string
  accessTokenExpiresAt: string
  userId: string
  personId: string
  email: string
  displayName: string
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  entitlements: string[]
}

export interface MaintainArrMeResponse {
  userId: string
  personId: string
  email: string
  displayName: string
  tenantId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasMaintainArrEntitlement: boolean
  entitlements: string[]
}

export interface MaintainArrSessionBootstrapResponse {
  userId: string
  personId: string
  tenantId: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasMaintainArrEntitlement: boolean
  entitlements: string[]
}

export interface AssetClassResponse {
  assetClassId: string
  classKey: string
  name: string
  description: string
  status: string
  createdAt: string
}

export interface CreateAssetClassRequest {
  classKey: string
  name: string
  description: string
}

export interface AssetTypeResponse {
  assetTypeId: string
  assetClassId: string
  classKey: string
  className: string
  typeKey: string
  name: string
  description: string
  status: string
  createdAt: string
}

export interface CreateAssetTypeRequest {
  assetClassId: string
  typeKey: string
  name: string
  description: string
}

export interface AssetResponse {
  assetId: string
  assetTypeId: string
  typeKey: string
  typeName: string
  classKey: string
  className: string
  assetTag: string
  name: string
  description: string
  lifecycleStatus: string
  siteRef: string | null
  createdAt: string
  updatedAt: string
}

export interface CreateAssetRequest {
  assetTypeId: string
  assetTag: string
  name: string
  description: string
  siteRef?: string | null
}

export interface CatalogOptionResponse {
  key: string
  label: string
  description: string
  sortOrder: number
  parentOptionKey: string | null
  isActive: boolean
  dependency: Record<string, string> | null
  metadata: Record<string, unknown> | null
}

export interface CatalogResponse {
  key: string
  label: string
  description: string
  owner: string
  scope: string
  isSystem: boolean
  isTenantExtendable: boolean
  isActive: boolean
  options: CatalogOptionResponse[]
}

export interface FieldMetadataResponse {
  key: string
  label: string
  description: string
  type: string
  control: string
  required: boolean
  catalogKey: string | null
  referenceKey: string | null
  source: string
  sourceOfTruth: string
  storedValue: string
  displayValue: string
  allowCustom: boolean
  customRequiresApproval: boolean
  drivesLogic: boolean
  drivesInspectionBranching: boolean
  drivesPMApplicability: boolean
  drivesCompliance: boolean
  drivesReporting: boolean
  drivesReadiness: boolean
  dependsOn: Record<string, string> | null
  validation: Record<string, unknown> | null
  defaultValue: unknown
  visibility: Record<string, unknown> | null
  sectionKey: string
  options: CatalogOptionResponse[] | null
}

export interface FieldsetResponse {
  key: string
  label: string
  entityType: string
  purpose: string
  fields: FieldMetadataResponse[]
}

export interface AssetUpsertV1Request {
  assetTag: string
  name: string
  description?: string | null
  values: Record<string, unknown>
}

export interface AssetFieldContextValueResponse {
  key: string
  storedValue: unknown
  displayValue: string | null
  source: string
  sourceOfTruth: string
}

export interface AssetFieldContextResponse {
  assetId: string
  fields: AssetFieldContextValueResponse[]
}

export interface InspectionTemplateSummaryResponse {
  inspectionTemplateId: string
  templateKey: string
  name: string
  description: string
  version: number
  status: string
  categoryCount: number
  checklistItemCount: number
  linkedAssetTypeCount: number
  createdAt: string
  updatedAt: string
}

export interface InspectionTemplateCategoryResponse {
  categoryId: string
  categoryKey: string
  name: string
  sortOrder: number
  createdAt: string
  updatedAt: string
}

export interface InspectionChecklistItemResponse {
  checklistItemId: string
  categoryId: string | null
  categoryKey: string | null
  itemKey: string
  prompt: string
  itemType: string
  isRequired: boolean
  sortOrder: number
  createdAt: string
  updatedAt: string
}

export interface InspectionTemplateAssetTypeLinkResponse {
  assetTypeId: string
  typeKey: string
  typeName: string
  classKey: string
  className: string
}

export interface InspectionTemplateDetailResponse {
  inspectionTemplateId: string
  templateKey: string
  name: string
  description: string
  version: number
  status: string
  categories: InspectionTemplateCategoryResponse[]
  checklistItems: InspectionChecklistItemResponse[]
  linkedAssetTypes: InspectionTemplateAssetTypeLinkResponse[]
  createdAt: string
  updatedAt: string
}

export interface CreateInspectionTemplateRequest {
  templateKey: string
  name: string
  description: string
}

export interface CreateInspectionTemplateCategoryRequest {
  categoryKey: string
  name: string
  sortOrder: number
}

export interface CreateInspectionChecklistItemRequest {
  itemKey: string
  prompt: string
  itemType: string
  isRequired: boolean
  sortOrder: number
  categoryId: string | null
}

export interface InspectionRunSummaryResponse {
  inspectionRunId: string
  assetId: string
  assetTag: string
  assetName: string
  inspectionTemplateId: string
  templateKey: string
  templateName: string
  templateVersion: number
  status: string
  result: string | null
  startedByUserId: string
  startedAt: string
  completedAt: string | null
  answerCount: number
  requiredItemCount: number
}

export interface InspectionRunChecklistItemSnapshot {
  checklistItemId: string
  categoryId: string | null
  categoryKey: string | null
  itemKey: string
  prompt: string
  itemType: string
  isRequired: boolean
  sortOrder: number
}

export interface InspectionRunAnswerResponse {
  answerId: string
  checklistItemId: string
  itemKey: string
  passFailValue: string | null
  numericValue: number | null
  textValue: string | null
  answeredAt: string
  answeredByUserId: string
}

export interface InspectionRunDetailResponse {
  inspectionRunId: string
  assetId: string
  assetTag: string
  assetName: string
  inspectionTemplateId: string
  templateKey: string
  templateName: string
  templateVersion: number
  status: string
  result: string | null
  startedByUserId: string
  startedAt: string
  completedAt: string | null
  updatedAt: string
  checklistItems: InspectionRunChecklistItemSnapshot[]
  answers: InspectionRunAnswerResponse[]
}

export interface StartInspectionRunRequest {
  assetId: string
  inspectionTemplateId: string
}

export interface InspectionRunAnswerInput {
  checklistItemId: string
  passFailValue?: string | null
  numericValue?: number | null
  textValue?: string | null
}

export interface SubmitInspectionRunAnswersRequest {
  answers: InspectionRunAnswerInput[]
}

export interface TechnicianRefResponse {
  personId: string
  displayName: string
  activeStatus: string | null
  primarySite: string | null
  lastSeenAt: string
}

export interface TechnicianRefListResponse {
  items: TechnicianRefResponse[]
}

export interface UpsertTechnicianRefRequest {
  personId: string
  displayName: string
  activeStatus?: string | null
  primarySite?: string | null
  sourceUpdatedAt?: string | null
  sourceCorrelationId?: string | null
}

export interface InspectionVoicePromptResponse {
  checklistItemId: string
  itemKey: string
  prompt: string
  itemType: string
  ttsPrompt: string
  voiceAnswerHint: string
  sortOrder: number
  isAnswered: boolean
}

export interface InspectionVoiceGuidanceResponse {
  inspectionRunId: string
  prompts: InspectionVoicePromptResponse[]
  nextUnansweredIndex: number
}

export interface NormalizeVoiceNumericResponse {
  value: number | null
  normalizedText: string | null
  understood: boolean
}

export interface DefectSummaryResponse {
  defectId: string
  assetId: string
  assetTag: string
  assetName: string
  inspectionRunId: string | null
  checklistItemId: string | null
  checklistItemKey: string | null
  title: string
  severity: string
  status: string
  source: string
  reportedByUserId: string
  createdAt: string
  updatedAt: string
  resolvedAt: string | null
  evidenceCount: number
}

export interface DowntimeFollowUpResponse {
  eventId: string
  assetId: string
  deepLinkPath: string
  reason: string
  trigger: string
}

export interface DefectDetailResponse {
  defectId: string
  assetId: string
  assetTag: string
  assetName: string
  inspectionRunId: string | null
  checklistItemId: string | null
  checklistItemKey: string | null
  checklistItemPrompt: string | null
  title: string
  description: string
  severity: string
  status: string
  source: string
  reportedByUserId: string
  createdAt: string
  updatedAt: string
  resolvedAt: string | null
  evidenceCount: number
  downtimeFollowUp?: DowntimeFollowUpResponse | null
}

export interface CreateDefectRequest {
  assetId: string
  title: string
  description: string
  severity: string
}

export interface CreateDefectsFromInspectionRunRequest {
  checklistItemIds?: string[] | null
}

export interface CreateDefectsFromInspectionRunResponse {
  inspectionRunId: string
  created: DefectSummaryResponse[]
  existing: DefectSummaryResponse[]
}

export interface UpdateDefectStatusRequest {
  status: string
}

export interface CreateMaintainArrEvidenceRequest {
  evidenceTypeKey: string
  fileName: string
  contentType: string
  contentBase64: string
  notes?: string | null
  checklistItemId?: string | null
}

export interface DefectEvidenceResponse {
  evidenceId: string
  defectId: string
  evidenceTypeKey: string
  fileName: string
  contentType: string
  sizeBytes: number
  notes: string | null
  uploadedByUserId: string
  createdAt: string
}

export interface InspectionRunEvidenceResponse {
  evidenceId: string
  inspectionRunId: string
  checklistItemId: string | null
  evidenceTypeKey: string
  fileName: string
  contentType: string
  sizeBytes: number
  notes: string | null
  uploadedByUserId: string
  createdAt: string
}

export interface WorkOrderSummaryResponse {
  workOrderId: string
  workOrderNumber: string
  assetId: string
  assetTag: string
  assetName: string
  defectId: string | null
  pmScheduleId: string | null
  title: string
  priority: string
  status: string
  source: string
  assignedTechnicianPersonId: string | null
  createdByUserId: string
  createdAt: string
  updatedAt: string
  startedAt: string | null
  completedAt: string | null
  cancelledAt: string | null
}

export interface WorkOrderDetailResponse {
  workOrderId: string
  workOrderNumber: string
  assetId: string
  assetTag: string
  assetName: string
  defectId: string | null
  defectTitle: string | null
  pmScheduleId: string | null
  pmScheduleName: string | null
  title: string
  description: string
  priority: string
  status: string
  source: string
  assignedTechnicianPersonId: string | null
  createdByUserId: string
  createdAt: string
  updatedAt: string
  startedAt: string | null
  completedAt: string | null
  cancelledAt: string | null
  downtimeFollowUp?: DowntimeFollowUpResponse | null
}

export interface WorkOrderTaskLineResponse {
  taskLineId: string
  workOrderId: string
  title: string
  description: string
  sortOrder: number
  status: string
  createdByUserId: string
  createdAt: string
  completedAt: string | null
}

export interface CreateWorkOrderTaskLineRequest {
  title: string
  description?: string | null
  sortOrder?: number | null
}

export interface WorkOrderLaborEntryResponse {
  laborEntryId: string
  workOrderId: string
  workOrderTaskLineId: string | null
  personId: string
  hoursWorked: number
  laborTypeKey: string
  notes: string | null
  loggedByUserId: string
  loggedAt: string
}

export interface CreateWorkOrderLaborEntryRequest {
  personId: string
  hoursWorked: number
  laborTypeKey: string
  workOrderTaskLineId?: string | null
  notes?: string | null
}

export interface WorkOrderEvidenceResponse {
  evidenceId: string
  workOrderId: string
  evidenceTypeKey: string
  fileName: string
  contentType: string
  sizeBytes: number
  notes: string | null
  uploadedByUserId: string
  createdAt: string
}

export interface CreateWorkOrderEvidenceRequest {
  evidenceTypeKey: string
  fileName: string
  contentType: string
  contentBase64: string
  notes?: string | null
}

export interface WorkOrderPartsDemandLineResponse {
  demandLineId: string
  lineNumber: number
  supplyarrPartId: string | null
  partNumber: string
  description: string
  quantityRequested: number
  unitOfMeasure: string
  notes: string
  status: string
  maintainarrPublicationId: string | null
  supplyarrDemandRefId: string | null
  publishedAt: string | null
  procurementStatus: string
  supplyarrPurchaseRequestId: string | null
  supplyarrPurchaseOrderId: string | null
  quantityReceived: number
  procurementStatusMessage: string
  lastProcurementStatusAt: string | null
  createdAt: string
}

export interface CreateWorkOrderPartsDemandLineRequest {
  supplyarrPartId?: string | null
  partNumber?: string | null
  description?: string | null
  quantityRequested: number
  unitOfMeasure?: string | null
  notes?: string | null
}

export interface PublishWorkOrderPartsDemandRequest {
  createPurchaseRequestDraft?: boolean
}

export interface PublishWorkOrderPartsDemandResponse {
  publicationId: string
  supplyarrDemandRefId: string
  supplyarrPurchaseRequestId: string | null
  createdPurchaseRequestDraft: boolean
  lines: WorkOrderPartsDemandLineResponse[]
}

export interface WorkOrderPartsDemandStatusEventResponse {
  statusEventId: string
  maintainarrPublicationId: string
  supplyarrDemandRefId: string
  eventType: string
  procurementStatus: string
  supplyarrPurchaseRequestId: string | null
  supplyarrPurchaseOrderId: string | null
  supplyarrReceivingReceiptId: string | null
  message: string
  occurredAt: string
  createdAt: string
}

export interface WorkOrderSupplyReadinessBlockerResponse {
  reasonCode: string
  message: string
  sourceEntityType: string
  sourceEntityId: string
  relatedEntityId: string | null
}

export interface WorkOrderLineSupplyReadinessResponse {
  demandLineId: string
  lineNumber: number
  supplyarrPartId: string | null
  partNumber: string
  quantityRequested: number
  lineStatus: string
  readinessStatus: string | null
  readinessBasis: string | null
  skipReason: string | null
  quantityAvailable: number | null
  calculatedAt: string | null
  blockers: WorkOrderSupplyReadinessBlockerResponse[]
}

export interface WorkOrderSupplyReadinessResponse {
  workOrderId: string
  workOrderNumber: string
  generatedAt: string
  overallReadinessStatus: string
  totalDemandLines: number
  linesChecked: number
  linesReady: number
  linesBlocked: number
  linesSkipped: number
  lines: WorkOrderLineSupplyReadinessResponse[]
}

export interface CreateWorkOrderRequest {
  assetId: string
  title: string
  description: string
  priority: string
  assignedTechnicianPersonId?: string | null
  pmScheduleId?: string | null
}

export interface CreateWorkOrderFromDefectRequest {
  title?: string | null
  description?: string | null
  priority?: string | null
  assignedTechnicianPersonId?: string | null
}

export interface UpdateWorkOrderRequest {
  title?: string | null
  description?: string | null
  priority?: string | null
  assignedTechnicianPersonId?: string | null
}

export interface UpdateWorkOrderStatusRequest {
  status: string
}

export interface AssetMeterResponse {
  assetMeterId: string
  assetId: string
  assetTag: string
  assetName: string
  meterKey: string
  name: string
  description: string
  unit: string
  baselineReading: number
  currentReading: number
  lastReadingAt: string | null
  status: string
  createdAt: string
  updatedAt: string
}

export interface CreateAssetMeterRequest {
  meterKey: string
  name: string
  description: string
  unit: string
  baselineReading: number
}

export interface MeterReadingResponse {
  meterReadingId: string
  assetMeterId: string
  assetId: string
  readingValue: number
  deltaFromPrevious: number
  readAt: string
  recordedByUserId: string
  notes: string
  isCorrection: boolean
  createdAt: string
}

export interface RecordMeterReadingRequest {
  readingValue: number
  readAt?: string | null
  notes: string
  isCorrection: boolean
}

export interface MeterPmForecastItem {
  pmScheduleId: string
  scheduleKey: string
  name: string
  scheduleMode: string
  dueStatus: string
  nextDueAtUsage: number | null
  intervalUsage: number | null
  currentMeterReading: number
  usageUntilDue: number | null
  isDueFromUsage: boolean
}

export interface MeterPmForecastResponse {
  assetMeterId: string
  meterKey: string
  unit: string
  currentReading: number
  usageVelocityPerDay: number | null
  predictedUsageUntilDue: number | null
  predictedDaysUntilDue: number | null
  predictedDueAt: string | null
  confidenceScore: number
  isDueSoon: boolean
  linkedSchedules: MeterPmForecastItem[]
}

export interface PmScheduleResponse {
  pmScheduleId: string
  assetId: string
  assetTag: string
  assetName: string
  scheduleKey: string
  name: string
  description: string
  scheduleMode: string
  assetMeterId: string | null
  meterKey: string | null
  meterUnit: string | null
  intervalUsage: number | null
  nextDueAtUsage: number | null
  lastCompletedUsage: number | null
  intervalDays: number
  nextDueAt: string
  lastCompletedAt: string | null
  dueStatus: string
  status: string
  lastDueScanAt: string | null
  linkedWorkOrderId: string | null
  linkedWorkOrderNumber: string | null
  linkedWorkOrderStatus: string | null
  createdAt: string
  updatedAt: string
}

export interface PmProgramSummaryResponse {
  pmProgramId: string
  programKey: string
  name: string
  scopeType: string
  assetTypeId: string | null
  assetTypeName: string | null
  assetId: string | null
  assetTag: string | null
  status: string
  scheduleCount: number
  createdAt: string
  updatedAt: string
}

export interface PmProgramScheduleLinkResponse {
  pmScheduleId: string
  scheduleKey: string
  name: string
  assetTag: string
  assetName: string
  dueStatus: string
  status: string
  sortOrder: number
}

export interface PmProgramDetailResponse {
  pmProgramId: string
  programKey: string
  name: string
  description: string
  scopeType: string
  assetTypeId: string | null
  assetTypeKey: string | null
  assetTypeName: string | null
  assetId: string | null
  assetTag: string | null
  assetName: string | null
  status: string
  schedules: PmProgramScheduleLinkResponse[]
  createdAt: string
  updatedAt: string
}

export interface CreatePmProgramRequest {
  programKey: string
  name: string
  description: string
  scopeType: string
  assetTypeId: string | null
  assetId: string | null
  pmScheduleIds?: string[] | null
}

export interface ReplacePmProgramSchedulesRequest {
  pmScheduleIds: string[]
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  hasNextPage: boolean
}

export interface MaintenanceHistoryEntryResponse {
  entryId: string
  assetId: string
  category: 'inspection' | 'defect' | 'work_order' | 'pm' | string
  eventType: string
  title: string
  detail: string | null
  occurredAt: string
  actorUserId: string | null
  sourceEntityType: string
  sourceEntityId: string
  relatedEntityId: string | null
}

export interface AssetReadinessBlockerResponse {
  blockerType: string
  message: string
  sourceEntityType: string
  sourceEntityId: string
  relatedEntityId: string | null
}

export interface AssetReadinessSignalCountsResponse {
  openCriticalDefectCount: number
  openHighDefectCount: number
  activeWorkOrderCount: number
  pmDueCount: number
  pmOverdueCount: number
  failedInspectionCount: number
}

export interface AssetReadinessResponse {
  assetId: string
  assetTag: string
  assetName: string
  lifecycleStatus: string
  readinessStatus: 'ready' | 'not_ready'
  readinessBasis: 'maintenance_clear' | 'maintenance_blockers' | string
  calculatedAt: string
  blockers: AssetReadinessBlockerResponse[]
  signals: AssetReadinessSignalCountsResponse
}

export interface AssetReadinessSummaryResponse {
  assetId: string
  assetTag: string
  assetName: string
  lifecycleStatus: string
  readinessStatus: 'ready' | 'not_ready'
  blockerCount: number
  primaryBlockerMessage: string | null
}

export interface AssetReadinessHistoryItemResponse {
  entryId: string
  statusFieldKey: string
  statusValueKey: string
  notes: string | null
  changedByPersonId: string | null
  changedAt: string
  createdAt: string
}

export interface AssetReadinessHistoryResponse {
  assetId: string
  assetTag: string
  assetName: string
  totalCount: number
  limit: number
  items: AssetReadinessHistoryItemResponse[]
}

export interface AssetTelematicsIngestionEventResponse {
  inboundEventId: string
  sourceEventId: string
  sourceProduct: string
  eventKind: string
  outcome: string
  summary: string
  vehicleRefKey: string | null
  tripNumber: string | null
  incidentType: string | null
  incidentSeverity: string | null
  dvirResult: string | null
  createdDefectId: string | null
  correlationId: string
  occurredAt: string
  createdAt: string
}

export interface AssetTelematicsIngestionResponse {
  assetId: string
  assetTag: string
  assetName: string
  totalCount: number
  limit: number
  processedCount: number
  ignoredCount: number
  defectCount: number
  items: AssetTelematicsIngestionEventResponse[]
}

export interface MaintenanceNotificationSettingsResponse {
  isEnabled: boolean
  notificationWebhookUrl: string | null
  notifyOnWorkOrderCreated: boolean
  notifyOnPmScheduleDue: boolean
  notifyOnPmScheduleOverdue: boolean
  notifyOnDefectEscalated: boolean
  updatedAt: string | null
}

export interface UpsertMaintenanceNotificationSettingsRequest {
  isEnabled: boolean
  notificationWebhookUrl: string | null
  notifyOnWorkOrderCreated: boolean
  notifyOnPmScheduleDue: boolean
  notifyOnPmScheduleOverdue: boolean
  notifyOnDefectEscalated: boolean
}

export interface MaintenanceNotificationDispatchItem {
  notificationId: string
  eventKind: string
  dispatchStatus: string
  assetId: string
  relatedEntityType: string
  relatedEntityId: string
  webhookHost: string | null
  httpStatusCode: number | null
  errorMessage: string | null
  createdAt: string
  dispatchedAt: string | null
}

export interface MaintenanceNotificationDispatchesResponse {
  items: MaintenanceNotificationDispatchItem[]
}

export interface DefectEscalationSettingsResponse {
  isEnabled: boolean
  lowThresholdHours: number
  mediumThresholdHours: number
  highThresholdHours: number
  criticalThresholdHours: number
  autoAcknowledgeOnEscalation: boolean
  autoCreateWorkOrderOnEscalation: boolean
  bumpSeverityOnRepeatEscalation: boolean
  notifyOnEscalation: boolean
  updatedAt: string | null
}

export interface UpsertDefectEscalationSettingsRequest {
  isEnabled: boolean
  lowThresholdHours: number
  mediumThresholdHours: number
  highThresholdHours: number
  criticalThresholdHours: number
  autoAcknowledgeOnEscalation: boolean
  autoCreateWorkOrderOnEscalation: boolean
  bumpSeverityOnRepeatEscalation: boolean
  notifyOnEscalation: boolean
}

export interface PendingDefectEscalationItem {
  defectId: string
  tenantId: string
  assetId: string
  title: string
  severity: string
  status: string
  escalationCount: number
  stagnationAnchorUtc: string
  thresholdHours: number
  stagnationHours: number
}

export interface PendingDefectEscalationsResponse {
  asOfUtc: string
  batchSize: number
  items: PendingDefectEscalationItem[]
}

export interface DefectEscalationRunItem {
  runId: string
  asOfUtc: string
  candidatesFound: number
  escalatedCount: number
  skippedCount: number
  createdAt: string
}

export interface DefectEscalationRunsResponse {
  items: DefectEscalationRunItem[]
}

export interface DefectEscalationEventItem {
  eventId: string
  defectId: string
  actionKind: string
  previousSeverity: string | null
  newSeverity: string | null
  previousStatus: string | null
  newStatus: string | null
  workOrderId: string | null
  createdAt: string
}

export interface DefectEscalationEventsResponse {
  items: DefectEscalationEventItem[]
}

export interface AssetStatusRollupSettingsResponse {
  isEnabled: boolean
  stalenessHours: number
  updatedAt: string | null
}

export interface UpsertAssetStatusRollupSettingsRequest {
  isEnabled: boolean
  stalenessHours: number
}

export interface PendingAssetStatusRollupItem {
  assetId: string
  assetTag: string
  assetName: string
  lastComputedAt: string | null
}

export interface PendingAssetStatusRollupsResponse {
  asOfUtc: string
  stalenessHours: number
  batchSize: number
  items: PendingAssetStatusRollupItem[]
}

export interface AssetStatusRollupRunItem {
  runId: string
  asOfUtc: string
  candidatesFound: number
  refreshedCount: number
  skippedCount: number
  scopeRollupsRefreshed: number
  createdAt: string
}

export interface AssetStatusRollupRunsResponse {
  items: AssetStatusRollupRunItem[]
}

export interface MaintenanceHistorySummaryResponse {
  assetId: string
  assetTag: string
  assetName: string
  eventCount: number
  inspectionCount: number
  defectCount: number
  workOrderCount: number
  pmCount: number
  lastEventAt: string | null
  computedAt: string
  isMaterialized: boolean
}

export interface MaintenanceHistoryRollupSettingsResponse {
  isEnabled: boolean
  stalenessHours: number
  updatedAt: string | null
}

export interface UpsertMaintenanceHistoryRollupSettingsRequest {
  isEnabled: boolean
  stalenessHours: number
}

export interface PendingMaintenanceHistoryRollupItem {
  assetId: string
  assetTag: string
  assetName: string
  lastComputedAt: string | null
}

export interface PendingMaintenanceHistoryRollupsResponse {
  asOfUtc: string
  stalenessHours: number
  batchSize: number
  items: PendingMaintenanceHistoryRollupItem[]
}

export interface MaintenanceHistoryRollupRunItem {
  runId: string
  asOfUtc: string
  candidatesFound: number
  refreshedCount: number
  skippedCount: number
  createdAt: string
}

export interface PmDueScanSettingsResponse {
  isEnabled: boolean
  scanIntervalMinutes: number
  batchSize: number
  overdueGraceDays: number
  lastRunAt: string | null
  pendingPmCount: number
  updatedAt: string | null
}

export interface UpsertPmDueScanSettingsRequest {
  isEnabled: boolean
  scanIntervalMinutes: number
  batchSize: number
  overdueGraceDays: number
}

export interface PendingPmDueItem {
  pmScheduleId: string
  tenantId: string
  assetId: string
  assetTag: string
  assetName: string
  scheduleKey: string
  dueStatus: string
  nextDueAt: string
}

export interface PendingPmDueResponse {
  asOfUtc: string
  batchSize: number
  items: PendingPmDueItem[]
}

export interface PmDueScanRunItem {
  runId: string
  asOfUtc: string
  candidatesFound: number
  markedDueCount: number
  markedOverdueCount: number
  skippedCount: number
  workOrdersCreatedCount: number
  workOrdersLinkedCount: number
  createdAt: string
}

export interface PmDueScanRunsResponse {
  items: PmDueScanRunItem[]
}

export interface ProcessPmDueScanResponse {
  asOfUtc: string
  batchSize: number
  candidatesFound: number
  markedDueCount: number
  markedOverdueCount: number
  skippedCount: number
  workOrdersCreatedCount: number
  workOrdersLinkedCount: number
  workOrderGenerationSkippedCount: number
}

export interface TriggerPmDueScanResponse {
  result: ProcessPmDueScanResponse
}

export interface MaintenanceHistoryRollupRunsResponse {
  items: MaintenanceHistoryRollupRunItem[]
}

export interface AssetStatusScopeRollupSummaryResponse {
  scopeType: string
  scopeEntityId: string
  scopeEntityKey: string | null
  scopeLabel: string
  totalAssets: number
  readyCount: number
  notReadyCount: number
  readyPercent: number
  computedAt: string
}

export interface AuditPackageSectionDescriptor {
  key: string
  fileName: string
  label: string
  description: string
}

export interface AuditPackageManifestResponse {
  packageVersion: string
  sections: AuditPackageSectionDescriptor[]
}

export interface AuditPackageFilterOptions {
  actions: string[]
  results: string[]
  targetTypes: string[]
  actorUserIds: string[]
}

export interface AuditPackageAppliedFilters {
  from: string | null
  to: string | null
  action: string | null
  result: string | null
  targetType: string | null
  actorUserId: string | null
}

export interface AuditPackageBreakdownItem {
  key: string
  count: number
}

export interface AuditPackageExportSummary {
  filters: AuditPackageAppliedFilters
  counts: AuditPackageCountsResponse
  byResult: AuditPackageBreakdownItem[]
  byAction: AuditPackageBreakdownItem[]
  generatedAt: string
}

export interface AuditPackageScope {
  from?: string
  to?: string
  action?: string
  result?: string
  targetType?: string
  actorUserId?: string
}

export interface AuditEventTimelineItem {
  auditEventId: string
  actorUserId: string | null
  action: string
  targetType: string
  targetId: string | null
  result: string
  reasonCode: string | null
  correlationId: string
  occurredAt: string
}

export interface PagedAuditTimeline {
  items: AuditEventTimelineItem[]
  page: number
  pageSize: number
  totalCount: number
  hasNextPage: boolean
}

export interface AuditPackageCountsResponse {
  auditEvents: number
  assets: number
  workOrders: number
  defects: number
  inspectionRuns: number
  pmSchedules: number
}

export interface AuditPackageGenerationJobResponse {
  jobId: string
  status: string
  format: string
  packageId: string | null
  requestedAt: string
  startedAt: string | null
  completedAt: string | null
  downloadReady: boolean
  errorMessage: string | null
}

export interface AuditPackageExportResponse {
  packageId: string
  tenantId: string
  generatedAt: string
  counts: AuditPackageCountsResponse
}

export interface MaintenanceReportCountItem {
  key: string
  count: number
}

export interface MaintenanceReportAssetSummaryItem {
  assetId: string
  assetTag: string
  assetName: string
  lifecycleStatus: string
  siteRef: string | null
  readinessStatus: string | null
  openWorkOrderCount: number
  openDefectCount: number
  overduePmScheduleCount: number
  duePmScheduleCount: number
  lastInspectionCompletedAt: string | null
  lastWorkOrderCompletedAt: string | null
}

export interface MaintenanceReportSummaryResponse {
  generatedAt: string
  totalAssetCount: number
  activeAssetCount: number
  workOrderStatusCounts: MaintenanceReportCountItem[]
  defectStatusCounts: MaintenanceReportCountItem[]
  defectSeverityCounts: MaintenanceReportCountItem[]
  inspectionRunStatusCounts: MaintenanceReportCountItem[]
  pmDueStatusCounts: MaintenanceReportCountItem[]
  readinessStatusCounts: MaintenanceReportCountItem[]
  assets: MaintenanceReportAssetSummaryItem[]
}

export interface MaintenanceReportWorkOrderRow {
  workOrderId: string
  workOrderNumber: string
  title: string
  status: string
  priority: string
  updatedAt: string
}

export interface MaintenanceReportDefectRow {
  defectId: string
  title: string
  severity: string
  status: string
  createdAt: string
}

export interface MaintenanceReportInspectionRunRow {
  inspectionRunId: string
  templateName: string
  status: string
  result: string | null
  startedAt: string
  completedAt: string | null
}

export interface MaintenanceReportPmScheduleRow {
  pmScheduleId: string
  scheduleKey: string
  name: string
  dueStatus: string
  nextDueAt: string
  lastCompletedAt: string | null
}

export interface MaintenanceReportAssetDetailResponse {
  summary: MaintenanceReportAssetSummaryItem
  recentWorkOrders: MaintenanceReportWorkOrderRow[]
  openDefects: MaintenanceReportDefectRow[]
  recentInspectionRuns: MaintenanceReportInspectionRunRow[]
  pmSchedules: MaintenanceReportPmScheduleRow[]
}

export interface MaintenanceReportWorkOrderDetailResponse {
  workOrderId: string
  workOrderNumber: string
  title: string
  description: string
  status: string
  priority: string
  source: string
  assetId: string
  assetTag: string
  assetName: string
  defectId: string | null
  pmScheduleId: string | null
  assignedTechnicianPersonId: string | null
  taskLineCount: number
  evidenceCount: number
  totalLaborHours: number
  createdAt: string
  updatedAt: string
  startedAt: string | null
  completedAt: string | null
}

export interface ExecutiveReportCountItem {
  key: string
  count: number
}

export interface ExecutiveReportFleetReadiness {
  totalAssets: number
  readyCount: number
  notReadyCount: number
  readyPercent: number
  computedAt: string | null
  fromScopeRollup: boolean
}

export interface ExecutiveReportScopeReadinessItem {
  scopeType: string
  scopeEntityId: string
  scopeLabel: string
  totalAssets: number
  readyCount: number
  notReadyCount: number
  readyPercent: number
  computedAt: string
}

export interface ExecutiveReportSupplyDemandSummary {
  sourceProduct: string
  totalDemandLines: number
  publishedDemandLines: number
  openProcurementLines: number
  fulfilledLines: number
  procurementStatusCounts: ExecutiveReportCountItem[]
}

export interface ExecutiveReportPartsDemandForecastItem {
  supplyarrPartId: string | null
  partNumber: string
  description: string
  unitOfMeasure: string
  forecastQuantity: number
  openLineCount: number
  openWorkOrderCount: number
  pmWorkOrderCount: number
  defectWorkOrderCount: number
  manualWorkOrderCount: number
  oldestCreatedAt: string | null
  newestCreatedAt: string | null
}

export interface ExecutiveReportPartsDemandForecastSummary {
  openLineCount: number
  distinctPartCount: number
  forecastQuantity: number
  topParts: ExecutiveReportPartsDemandForecastItem[]
}

export interface ExecutiveReportOperationalTotals {
  totalAssetCount: number
  activeAssetCount: number
  openWorkOrderCount: number
  openCriticalDefectCount: number
  openHighDefectCount: number
  overduePmScheduleCount: number
  failedInspectionCount: number
  laborHoursLast30Days: number
  workOrdersCompletedLast30Days: number
  activeTechnicianAssignments: number
}

export interface ExecutiveReportDowntimePeriodMetrics {
  periodStart: string
  periodEnd: string
  downtimeHours: number
  availabilityPercent: number
  plannedDowntimeHours: number
  unplannedDowntimeHours: number
  activeDowntimeEventCount: number
  fromMaterializedSnapshot: boolean
}

export interface ExecutiveReportDowntimeTrend {
  periodDays: number
  currentPeriod: ExecutiveReportDowntimePeriodMetrics
  previousPeriod: ExecutiveReportDowntimePeriodMetrics
  downtimeHoursDelta: number
  availabilityPercentDelta: number
  fleetSnapshotComputedAt: string | null
}

export interface ExecutiveReportSummaryResponse {
  generatedAt: string
  fleetReadiness: ExecutiveReportFleetReadiness
  operationalTotals: ExecutiveReportOperationalTotals
  downtimeTrend: ExecutiveReportDowntimeTrend
  supplyDemand: ExecutiveReportSupplyDemandSummary
  partsDemandForecast: ExecutiveReportPartsDemandForecastSummary
  scopeReadiness: ExecutiveReportScopeReadinessItem[]
  workOrderStatusCounts: ExecutiveReportCountItem[]
  defectSeverityCounts: ExecutiveReportCountItem[]
}

export interface ComplianceReportCountItem {
  key: string
  count: number
}

export interface ComplianceReportInspectionTotals {
  totalRuns: number
  completedRuns: number
  passedRuns: number
  failedRuns: number
  inProgressRuns: number
  failedChecklistAnswers: number
  passRatePercent: number
}

export interface ComplianceReportDefectTotals {
  openDefectCount: number
  openCriticalCount: number
  openHighCount: number
  inspectionSourcedOpenCount: number
  manualSourcedOpenCount: number
}

export interface ComplianceReportPmAdherenceTotals {
  activeScheduleCount: number
  overdueCount: number
  dueCount: number
  scheduledCount: number
  adherencePercent: number
}

export interface ComplianceReportRegulatoryKeyGroup {
  complianceKey: string
  materialKey: string | null
  linkedSubjectCount: number
  inspectionTemplateCount: number
  openComplianceIssueCount: number
}

export interface ComplianceReportTemplateSummaryItem {
  inspectionTemplateId: string
  templateKey: string
  templateName: string
  regulatoryKeyCount: number
  completedRunCount: number
  failedRunCount: number
  lastFailedAt: string | null
  requiresAttention: boolean
}

export interface ComplianceReportAttentionItem {
  assetId: string
  assetTag: string
  assetName: string
  siteRef: string | null
  issueType: string
  message: string
}

export interface ComplianceReportSummaryResponse {
  generatedAt: string
  inspectionTotals: ComplianceReportInspectionTotals
  defectTotals: ComplianceReportDefectTotals
  pmAdherenceTotals: ComplianceReportPmAdherenceTotals
  regulatoryKeyMirrorCount: number
  regulatoryKeyGroups: ComplianceReportRegulatoryKeyGroup[]
  templateSummaries: ComplianceReportTemplateSummaryItem[]
  attentionItems: ComplianceReportAttentionItem[]
  defectSeverityCounts: ComplianceReportCountItem[]
}

export interface AssetImportRowRequest {
  assetTag: string
  name: string
  description?: string
  lifecycleStatus?: string
  values?: Record<string, string | null>
  assetClassKey?: string
  assetTypeKey?: string
  siteRef?: string | null
}

export interface AssetBulkImportRequest {
  assets: AssetImportRowRequest[]
}

export interface AssetImportRowResult {
  rowIndex: number
  assetTag: string
  status: string
  assetId: string | null
  errorCode: string | null
  message: string | null
}

export interface AssetBulkImportResponse {
  importBatchId: string
  importType: string
  phase: string
  dryRun: boolean
  totalRows: number
  successCount: number
  errorCount: number
  results: AssetImportRowResult[]
}

export interface EntityExportFormatDescriptor {
  formatKey: string
  contentType: string
  fileNamePattern: string
  description: string
}

export interface EntityExportDescriptor {
  entityKey: string
  route: string
  label: string
  csvHeader: string
  description: string
  formats: EntityExportFormatDescriptor[]
}

export interface ReportExportDescriptor {
  reportKey: string
  route: string
  label: string
  description: string
}

export interface EntityExportManifestResponse {
  packageVersion: string
  entities: EntityExportDescriptor[]
  reportExports: ReportExportDescriptor[]
  auditPackageFormats: EntityExportFormatDescriptor[]
}

export interface DowntimeTrackingSettingsResponse {
  isEnabled: boolean
  autoTrackOutOfService: boolean
  autoTrackNotReady: boolean
  availabilityPeriodDays: number
  updatedAt: string | null
}

export interface UpsertDowntimeTrackingSettingsRequest {
  isEnabled: boolean
  autoTrackOutOfService: boolean
  autoTrackNotReady: boolean
  availabilityPeriodDays: number
}

export interface AssetDowntimeEventResponse {
  eventId: string
  assetId: string
  assetTag: string
  assetName: string
  source: string
  reason: string
  isPlanned: boolean
  startedAt: string
  endedAt: string | null
  statusTrigger: string | null
  workOrderId: string | null
  defectId: string | null
  notes: string | null
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface CreateManualDowntimeEventRequest {
  assetId: string
  reason: string
  isPlanned: boolean
  startedAt: string
  notes?: string
  workOrderId?: string
  defectId?: string
}

export interface CloseDowntimeEventRequest {
  endedAt?: string
  notes?: string
}

export interface AssetAvailabilityResponse {
  assetId: string
  assetTag: string
  assetName: string
  periodStart: string
  periodEnd: string
  totalHours: number
  downtimeHours: number
  availabilityPercent: number
  plannedDowntimeHours: number
  unplannedDowntimeHours: number
  hasActiveDowntime: boolean
  computedAt: string
  isMaterialized: boolean
}

export interface FleetAvailabilityResponse {
  periodStart: string
  periodEnd: string
  assetCount: number
  totalHours: number
  downtimeHours: number
  availabilityPercent: number
  plannedDowntimeHours: number
  unplannedDowntimeHours: number
  activeDowntimeEventCount: number
  computedAt: string
  isMaterialized: boolean
}

export interface PendingAssetDowntimeSyncItem {
  assetId: string
  assetTag: string
  assetName: string
  lifecycleStatus: string
  readinessStatus: string
  hasOpenAutomaticEvent: boolean
}

export interface PendingAssetDowntimeSyncResponse {
  asOfUtc: string
  batchSize: number
  items: PendingAssetDowntimeSyncItem[]
}

export interface AssetDowntimeSyncRunItem {
  runId: string
  asOfUtc: string
  assetsScanned: number
  eventsOpened: number
  eventsClosed: number
  snapshotsRefreshed: number
  createdAt: string
}

export interface AssetDowntimeSyncRunsResponse {
  items: AssetDowntimeSyncRunItem[]
}

export interface MaintenancePlatformEventSettingsResponse {
  isEnabled: boolean
  maxAttempts: number
  retryIntervalMinutes: number
  updatedAt: string | null
}

export interface UpsertMaintenancePlatformEventSettingsRequest {
  isEnabled: boolean
  maxAttempts?: number
  retryIntervalMinutes?: number
}

export interface MaintenancePlatformOutboxEventItem {
  id: string
  eventKind: string
  processingStatus: string
  relatedEntityId: string
  attemptCount: number
  errorMessage: string | null
  createdAt: string
  processedAt: string | null
}

export interface MaintenancePlatformOutboxEventsResponse {
  items: MaintenancePlatformOutboxEventItem[]
}

export interface MaintenancePlatformEventProcessingRunItem {
  id: string
  pendingFound: number
  processedCount: number
  retriedCount: number
  abandonedCount: number
  skippedCount: number
  createdAt: string
}

export interface MaintenancePlatformEventProcessingRunsResponse {
  items: MaintenancePlatformEventProcessingRunItem[]
}
