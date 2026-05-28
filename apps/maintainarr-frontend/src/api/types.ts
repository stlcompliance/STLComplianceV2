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
