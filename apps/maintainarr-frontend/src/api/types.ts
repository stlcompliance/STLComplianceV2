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
  callbackUrl: string | null
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

export interface ReferenceOptionResponse {
  key: string
  id: string | null
  label: string
  source: string
  sourceOfTruth: string
  storedValue: string
  displayValue: string
  isActive: boolean
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

export interface AssetInstalledComponentResponse {
  componentId: string
  componentNumber: string
  parentAssetId: string
  parentComponentId: string | null
  name: string
  description: string | null
  componentType: string
  status: string
  make: string | null
  model: string | null
  serialNumber: string | null
  partNumberSnapshot: string | null
  installedPartUsageRef: string | null
  installDate: string | null
  installedByPersonId: string | null
  installedMeterReading: number | null
  removedDate: string | null
  removedByPersonId: string | null
  removedMeterReading: number | null
  removalReason: string | null
  warrantyStartDate: string | null
  warrantyEndDate: string | null
  expectedLifeHours: number | null
  expectedLifeMiles: number | null
  expectedLifeCycles: number | null
  condition: string
  replacementPartRefs: string[]
  documentRefs: string[]
  defectRefs: string[]
  workOrderRefs: string[]
  createdAt: string
  updatedAt: string
}

export interface AssetSearchResponse {
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
  staffarrSiteOrgUnitId: string | null
  staffarrSiteNameSnapshot: string
  openDefectCount: number
  openWorkOrderCount: number
  readinessStatus: string
  createdAt: string
  updatedAt: string
}

export interface InspectionTemplateSummaryResponse {
  inspectionTemplateId: string
  templateKey: string
  name: string
  description: string
  templateCategoryKey?: string | null
  owningSiteRef?: string | null
  owningTeamRef?: string | null
  ownerPersonId?: string | null
  inspectionType?: string
  version: number
  status: string
  categoryCount: number
  checklistItemCount: number
  linkedAssetTypeCount: number
  createdAt: string
  updatedAt: string
  publishedAt?: string | null
  retiredAt?: string | null
}

export interface InspectionTemplateCategoryResponse {
  categoryId: string
  categoryKey: string
  name: string
  description?: string | null
  isRequired?: boolean
  canBeSkipped?: boolean
  skipReasonRequired?: boolean
  timingTracked?: boolean
  sortOrder: number
  settings?: Record<string, unknown>
  createdAt: string
  updatedAt: string
}

export interface InspectionChecklistItemResponse {
  checklistItemId: string
  categoryId: string | null
  categoryKey: string | null
  itemKey: string
  prompt: string
  helpText?: string
  itemType: string
  controlledOptions: string[]
  acceptableRangeMin?: number | null
  acceptableRangeMax?: number | null
  unitOfMeasure?: string | null
  isRequired: boolean
  sortOrder: number
  settings?: Record<string, unknown>
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
  templateCategoryKey?: string | null
  owningSiteRef?: string | null
  owningTeamRef?: string | null
  ownerPersonId?: string | null
  ownerRoleKey?: string | null
  estimatedDurationMinutes?: number | null
  tags?: string[]
  settings?: Record<string, unknown>
  inspectionType?: string
  version: number
  status: string
  categories: InspectionTemplateCategoryResponse[]
  checklistItems: InspectionChecklistItemResponse[]
  linkedAssetTypes: InspectionTemplateAssetTypeLinkResponse[]
  createdAt: string
  updatedAt: string
  publishedAt?: string | null
  retiredAt?: string | null
  createdByPersonId?: string | null
  updatedByPersonId?: string | null
  publishedByPersonId?: string | null
  retiredByPersonId?: string | null
}

export interface CreateInspectionTemplateRequest {
  templateKey: string
  name: string
  description: string
  inspectionType?: string | null
  templateCategoryKey?: string | null
  owningSiteRef?: string | null
  owningTeamRef?: string | null
  ownerPersonId?: string | null
  ownerRoleKey?: string | null
  estimatedDurationMinutes?: number | null
  tags?: string[] | null
  settings?: Record<string, unknown> | null
}

export interface UpdateInspectionTemplateRequest {
  name: string
  description: string
  inspectionType?: string | null
  templateCategoryKey?: string | null
  owningSiteRef?: string | null
  owningTeamRef?: string | null
  ownerPersonId?: string | null
  ownerRoleKey?: string | null
  estimatedDurationMinutes?: number | null
  tags?: string[] | null
  settings?: Record<string, unknown> | null
}

export interface CreateInspectionTemplateCategoryRequest {
  categoryKey: string
  name: string
  description?: string | null
  isRequired?: boolean
  canBeSkipped?: boolean
  skipReasonRequired?: boolean
  timingTracked?: boolean
  sortOrder: number
  settings?: Record<string, unknown> | null
}

export interface UpdateInspectionTemplateCategoryRequest {
  name: string
  description?: string | null
  isRequired?: boolean
  canBeSkipped?: boolean
  skipReasonRequired?: boolean
  timingTracked?: boolean
  sortOrder: number
  settings?: Record<string, unknown> | null
}

export interface CreateInspectionChecklistItemRequest {
  itemKey: string
  prompt: string
  helpText?: string | null
  itemType: string
  isRequired: boolean
  sortOrder: number
  categoryId: string | null
  controlledOptions?: string[] | null
  acceptableRangeMin?: number | null
  acceptableRangeMax?: number | null
  unitOfMeasure?: string | null
  settings?: Record<string, unknown> | null
}

export interface UpdateInspectionChecklistItemRequest {
  prompt: string
  helpText?: string | null
  itemType: string
  isRequired: boolean
  sortOrder: number
  categoryId: string | null
  controlledOptions?: string[] | null
  acceptableRangeMin?: number | null
  acceptableRangeMax?: number | null
  unitOfMeasure?: string | null
  settings?: Record<string, unknown> | null
}

export interface UpdateInspectionTemplateStatusRequest {
  status: string
}

export interface PublishInspectionTemplateRequest {
  confirmComplianceRelated: boolean
  confirmReadinessImpact: boolean
  confirmFailureAutomation: boolean
  confirmSupervisorRelease: boolean
}

export interface RetireInspectionTemplateRequest {
  reason?: string | null
}

export interface InspectionTemplateValidationIssueResponse {
  code: string
  message: string
  section: string
  isBlocking: boolean
}

export interface InspectionTemplateValidationResponse {
  isValid: boolean
  issues: InspectionTemplateValidationIssueResponse[]
  sectionCount: number
  checklistItemCount: number
  compatibleAssetCount: number
}

export interface CompatibleAssetPreviewResponse {
  compatibleCount: number
  sampleAssets: AssetSearchResponse[]
  excludedAssets: AssetSearchResponse[]
}

export interface InspectionTemplatePreviewResponse {
  template: InspectionTemplateDetailResponse
  validation: InspectionTemplateValidationResponse
  assets: CompatibleAssetPreviewResponse
  summary: string
}

export interface InspectionRunSummaryResponse {
  inspectionRunId: string
  assetId: string
  assetTag: string
  assetName: string
  inspectionTemplateId: string
  templateKey: string
  templateName: string
  inspectionType?: string
  templateVersion: number
  status: string
  result: string | null
  startedByUserId: string
  startedAt: string
  completedAt: string | null
  staffarrLocationId?: string | null
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
  controlledOptions: string[]
  acceptableRangeMin?: number | null
  acceptableRangeMax?: number | null
  unitOfMeasure?: string | null
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
  selectedOptions: string[]
  unitOfMeasure?: string | null
  answeredAt: string
  answeredByUserId: string
}

export interface InspectionRunPauseEventResponse {
  pauseEventId: string
  pausedAt: string
  resumedAt: string | null
  durationMinutes: number | null
  reason: string | null
  notes: string | null
  pausedByUserId: string
  resumedByUserId: string | null
}

export interface InspectionRunDetailResponse {
  inspectionRunId: string
  assetId: string
  assetTag: string
  assetName: string
  inspectionTemplateId: string
  templateKey: string
  templateName: string
  inspectionType?: string
  templateVersion: number
  status: string
  result: string | null
  startedByUserId: string
  startedAt: string
  completedAt: string | null
  updatedAt: string
  sourceProduct?: string | null
  sourceObjectRef?: string | null
  staffarrLocationId?: string | null
  breakDurationMinutes?: number
  longDurationFlag?: boolean
  generatedWorkOrderRefs?: string[]
  checklistItems: InspectionRunChecklistItemSnapshot[]
  answers: InspectionRunAnswerResponse[]
  pauseEvents: InspectionRunPauseEventResponse[]
}

export interface StartInspectionRunRequest {
  assetId: string
  inspectionTemplateId: string
  sourceProduct?: string | null
  sourceObjectRef?: string | null
}

export interface InspectionRunAnswerInput {
  checklistItemId: string
  passFailValue?: string | null
  numericValue?: number | null
  textValue?: string | null
  selectedOptions?: string[] | null
}

export interface SubmitInspectionRunAnswersRequest {
  answers: InspectionRunAnswerInput[]
}

export interface PauseInspectionRunRequest {
  reason?: string | null
  notes?: string | null
}

export interface ResumeInspectionRunRequest {
  notes?: string | null
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
  controlledOptions: string[]
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
  downtimeFollowUp?: DowntimeFollowUpResponse | null
  priority?: string | null
  defectType?: string | null
  reportSource?: string | null
  reportedByPersonId?: string | null
  discoveredByPersonId?: string | null
  createdByPersonId?: string | null
  updatedByPersonId?: string | null
  reportedAt?: string | null
  discoveredAt?: string | null
  isSafetyCritical?: boolean | null
  isComplianceImpacting?: boolean | null
  isOperabilityImpacting?: boolean | null
  failureMode?: string | null
  systemKey?: string | null
  componentKey?: string | null
  symptom?: string | null
  sidePosition?: string | null
  operatingCondition?: string | null
  deferralCode?: string | null
  sourceType?: string | null
  sourceReferenceId?: string | null
  incidentReferenceId?: string | null
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
  priority?: string | null
  defectType?: string | null
  reportSource?: string | null
  reportedByPersonId?: string | null
  discoveredByPersonId?: string | null
  createdByPersonId?: string | null
  updatedByPersonId?: string | null
  reportedAt?: string | null
  discoveredAt?: string | null
  isSafetyCritical?: boolean | null
  isComplianceImpacting?: boolean | null
  isOperabilityImpacting?: boolean | null
  failureMode?: string | null
  systemKey?: string | null
  componentKey?: string | null
  symptom?: string | null
  sidePosition?: string | null
  operatingCondition?: string | null
  deferralCode?: string | null
  sourceType?: string | null
  sourceReferenceId?: string | null
  incidentReferenceId?: string | null
  readinessNotes?: string | null
  correctiveAction?: string | null
}

export interface CreateDefectRequest {
  assetId: string
  title: string
  description: string
  severity: string
}

export interface DefectValidationFindingResponse {
  category: string
  severity: string
  code: string
  message: string
  fieldKey?: string | null
  sectionKey?: string | null
  source?: string | null
}

export interface DefectValidationResponse {
  isValid: boolean
  findings: DefectValidationFindingResponse[]
}

export interface DefectDuplicateMatchResponse {
  defectId: string
  title: string
  status: string
  severity: string
  assetTag: string
  assetName: string
  matchReason: string
  similarityScore: number
}

export interface DefectDraftPreviewResponse {
  defect: DefectDetailResponse
  findings: DefectValidationFindingResponse[]
  duplicateMatches: DefectDuplicateMatchResponse[]
  assetReadiness: AssetReadinessResponse | null
  canSubmit: boolean
  canCreateWorkOrder: boolean
  canMarkAssetNotReady: boolean
}

export interface UpsertDefectDraftRequest {
  assetId: string
  title?: string | null
  description?: string | null
  severity?: string | null
  priority?: string | null
  defectType?: string | null
  reportSource?: string | null
  reportedAt?: string | null
  discoveredAt?: string | null
  reportedByPersonId?: string | null
  discoveredByPersonId?: string | null
  failureMode?: string | null
  systemKey?: string | null
  componentKey?: string | null
  symptom?: string | null
  sidePosition?: string | null
  operatingCondition?: string | null
  deferralCode?: string | null
  isSafetyCritical?: boolean | null
  isComplianceImpacting?: boolean | null
  isOperabilityImpacting?: boolean | null
  readinessNotes?: string | null
  correctiveAction?: string | null
  sourceType?: string | null
  sourceReferenceId?: string | null
  incidentReferenceId?: string | null
}

export interface SubmitDefectRequest {
  createWorkOrder: boolean
  markAssetNotReady: boolean
  workOrderTitle?: string | null
  workOrderDescription?: string | null
  workOrderPriority?: string | null
  workOrderAssignedTechnicianPersonId?: string | null
  workOrderDraftPlanJson?: string | null
  workOrderPlannedStartAt?: string | null
  workOrderPlannedDueAt?: string | null
  holdType?: string | null
  holdTitle?: string | null
  holdDescription?: string | null
  holdSeverity?: string | null
  holdSourceProduct?: string | null
  holdSourceObjectRef?: string | null
  holdCreatedByPersonId?: string | null
}

export interface DefectSubmissionResponse {
  defect: DefectDetailResponse
  workOrder: WorkOrderDetailResponse | null
  assetQualityHold: AssetQualityHoldResponse | null
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

export interface AssetQualityHoldResponse {
  holdId: string
  assetId: string
  holdType: string
  sourceProduct: string
  sourceObjectRef: string | null
  title: string
  description: string
  severity: string
  status: string
  createdAt: string
  createdByPersonId: string | null
  releasedAt: string | null
  releasedByPersonId: string | null
  releaseReason: string | null
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
  sourceProduct?: string | null
  sourceObjectRef?: string | null
  workOrderType?: string
  originType?: string
  originRef?: string | null
  staffarrSiteId?: string | null
  staffarrLocationId?: string | null
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
  sourceProduct?: string | null
  sourceObjectRef?: string | null
  workOrderType?: string
  originType?: string
  originRef?: string | null
  staffarrSiteId?: string | null
  staffarrLocationId?: string | null
  assignedTechnicianPersonIds?: string[]
  assignedSupervisorPersonId?: string | null
  requiredQualificationRefs?: string[]
  qualificationCheckResults?: WorkOrderQualificationCheckResultResponse[]
  technicianAssignments?: WorkOrderTechnicianAssignmentResponse[]
  permitRefs?: MaintenancePermitRefResponse[]
  returnToService?: ReturnToServiceResponse | null
  vendorWorkRefs?: string[]
  assignedTechnicianPersonId: string | null
  createdByUserId: string
  createdAt: string
  updatedAt: string
  startedAt: string | null
  completedAt: string | null
  cancelledAt: string | null
  draftPlanJson?: string | null
  plannedStartAt?: string | null
  plannedDueAt?: string | null
  blockers?: WorkOrderBlockerResponse[]
  closeout?: WorkOrderCloseoutResponse | null
  downtimeFollowUp?: DowntimeFollowUpResponse | null
}

export interface WorkOrderQualificationCheckResultResponse {
  checkId?: string | null
  staffarrPersonId: string | null
  qualificationKey: string
  outcome: string
  reasonCode: string
  message: string
}

export interface WorkOrderFindingResponse {
  category: string
  severity: string
  code: string
  message: string
  fieldKey?: string | null
  sectionKey?: string | null
  source?: string | null
}

export interface WorkOrderDuplicateMatchResponse {
  workOrderId: string
  workOrderNumber: string
  title: string
  status: string
  assetTag: string
  assetName: string
  matchReason: string
  similarityScore: number
}

export interface WorkOrderValidationResponse {
  isValid: boolean
  findings: WorkOrderFindingResponse[]
}

export interface WorkOrderPreviewResponse {
  workOrder: WorkOrderDetailResponse
  findings: WorkOrderFindingResponse[]
  duplicateMatches: WorkOrderDuplicateMatchResponse[]
  assetReadiness: AssetReadinessResponse | null
  canOpen: boolean
  canSchedule: boolean
  canStart: boolean
}

export interface WorkOrderTechnicianAssignmentResponse {
  assignmentId: string
  workOrderId: string
  personId: string
  assignmentRole: string
  status: string
  assignedAt: string
  assignedByPersonId: string | null
  acceptedAt: string | null
  completedAt: string | null
  requiredQualificationRefs: string[]
  qualificationCheckSnapshot: WorkOrderQualificationCheckResultResponse[]
}

export interface WorkOrderBlockerResponse {
  blockerId: string
  workOrderId: string
  blockerType: string
  sourceProduct: string
  sourceObjectRef: string | null
  title: string
  description: string
  severity: string
  status: string
  requiredAction: string | null
  createdAt: string
  createdByPersonId: string | null
  resolvedAt: string | null
  resolvedByPersonId: string | null
  overrideReason: string | null
}

export interface MaintenancePermitRefResponse {
  permitRefId: string
  workOrderId: string
  permitType: string
  sourceProduct: string
  sourceObjectRef: string | null
  recordRef: string | null
  statusSnapshot: string | null
  approvedByPersonId: string | null
  validFrom: string | null
  validTo: string | null
}

export interface ReturnToServiceResponse {
  returnToServiceId: string
  workOrderId: string
  assetId: string
  status: string
  requiredChecks: string[]
  completedChecks: string[]
  finalInspectionRef: string | null
  approvedByPersonId: string | null
  approvedAt: string | null
  rejectionReason: string | null
  finalReadinessStatus: string | null
  recordRefs: string[]
}

export interface WorkOrderCloseoutResponse {
  closeoutId: string
  workOrderId: string
  completionSummary: string
  rootCause: string | null
  correctiveAction: string | null
  preventiveActionRecommendation: string | null
  assetReturnedToService: boolean
  returnToServiceAt: string | null
  returnToServiceByPersonId: string | null
  postRepairInspectionRequired: boolean
  postRepairInspectionRef: string | null
  supervisorReviewRequired: boolean
  supervisorReviewedByPersonId: string | null
  supervisorReviewedAt: string | null
  complianceReviewRequired: boolean
  complianceReviewedByPersonId: string | null
  complianceReviewedAt: string | null
  qualityReviewRequired: boolean
  qualityReviewedByPersonId: string | null
  qualityReviewedAt: string | null
  evidenceAccepted: boolean
  unresolvedDefectRefs: string | null
  followUpWorkOrderRefs: string | null
  customerImpactSummary: string | null
  downtimeSummary: string | null
  finalAssetReadinessStatus: string | null
  finalStatus: string | null
  evidenceRecordRefs: string[]
  createdAt: string
  createdByPersonId: string | null
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
  status: string
  notes: string | null
  submittedAt: string | null
  approvedByPersonId: string | null
  approvedAt: string | null
  rejectionReason: string | null
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

export interface UpdateWorkOrderLaborEntryStatusRequest {
  status: string
  rejectionReason?: string | null
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

export interface CreateWorkOrderCommentRequest {
  body: string
  visibility?: string | null
  pinned?: boolean | null
}

export interface WorkOrderCommentResponse {
  commentId: string
  workOrderId: string
  body: string
  visibility: string
  createdAt: string
  createdByPersonId: string | null
  editedAt: string | null
  editedByPersonId: string | null
  pinned: boolean
}

export interface WorkOrderTimelineEventResponse {
  timelineEventId: string
  workOrderId: string
  eventType: string
  occurredAt: string
  actorPersonId: string | null
  actorServiceClientId: string | null
  summary: string
  sourceProduct: string | null
  sourceObjectRef: string | null
  beforeSnapshot: string | null
  afterSnapshot: string | null
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

export interface MaintenancePartsKitLineResponse {
  partsKitLineId: string
  partsKitId: string
  itemRef: string
  itemDescriptionSnapshot: string
  quantity: number
  unitOfMeasure: string
  required: boolean
  substituteAllowed: boolean
  sortOrder?: number
  supplyarrPartId?: string | null
  partNumberSnapshot?: string | null
  manufacturerPartNumberSnapshot?: string | null
  vendorPartNumberSnapshot?: string | null
  criticality?: string
  consumable?: boolean
  serialized?: boolean
  coreReturnExpected?: boolean
  hazardous?: boolean
  warrantySensitive?: boolean
  requiredByTask?: string | null
  notes?: string | null
  tags?: string[]
  preferredSubstituteRefs?: string[]
  isPlaceholder?: boolean
  createdAt: string
  updatedAt: string
}

export interface MaintenancePartsKitAssetScopeResponse {
  assetClassKeys: string[]
  assetTypeKeys: string[]
  assetCategoryKeys: string[]
  assetStatusKeys: string[]
  siteRefs: string[]
  departmentRefs: string[]
  makeKeys: string[]
  modelKeys: string[]
  yearFrom?: string | null
  yearTo?: string | null
  fuelTypeKeys: string[]
  bodyTypeKeys: string[]
  configurationKeys: string[]
  variantFlags: string[]
  requiredAttributes: string[]
  excludedAttributes: string[]
  includedAssetIds: string[]
  excludedAssetIds: string[]
}

export interface MaintenancePartsKitItemResponse {
  itemRef: string
  supplyarrPartId?: string | null
  itemDescriptionSnapshot: string
  partNumberSnapshot?: string | null
  manufacturerPartNumberSnapshot?: string | null
  vendorPartNumberSnapshot?: string | null
  quantity: number
  unitOfMeasure: string
  required: boolean
  criticality: string
  substituteAllowed: boolean
  preferredSubstituteRefs: string[]
  consumable: boolean
  serialized: boolean
  coreReturnExpected: boolean
  hazardous: boolean
  warrantySensitive: boolean
  requiredByTask?: string | null
  notes?: string | null
  tags: string[]
  isPlaceholder: boolean
}

export interface MaintenancePartsKitQuantityRuleResponse {
  ruleId: string
  ruleType: string
  appliesToItemRef: string
  assetConditionSummary?: string | null
  workConditionSummary?: string | null
  conditionSummary?: string | null
  baseQuantity: number
  multiplier: number
  minimumQuantity?: number | null
  maximumQuantity?: number | null
  roundingBehavior: string
  plainLanguageSummary: string
}

export interface MaintenancePartsKitAvailabilityResponse {
  enabled: boolean
  preferredFulfillmentSource?: string | null
  showSiteAvailability: boolean
  showNearbyAvailability: boolean
  showOnOrder: boolean
  showEstimatedLeadTime: boolean
  requestReservation: boolean
  notes?: string | null
}

export interface MaintenancePartsKitWorkOrderBehaviorResponse {
  canBeManuallyAdded: boolean
  autoSuggestOnMatchingWorkOrder: boolean
  autoAddToMatchingWorkOrder: boolean
  autoAddToPmGeneratedWorkOrder: boolean
  autoAddAfterFailedInspectionQuestion: boolean
  autoAddAfterMatchingDefectType: boolean
  requireSupervisorApprovalBeforeAdding: boolean
  requirePartsReviewBeforeWorkCanStart: boolean
  requireAvailabilityCheckBeforeScheduling: boolean
  allowTechnicianAdjustQuantities: boolean
  requireAdjustmentReason: boolean
  allowTechnicianRemoveOptionalItems: boolean
  allowTechnicianRemoveRequiredItems: boolean
  requireReasonToRemoveRequiredItem: boolean
  snapshotKitItemsOntoWorkOrder: boolean
  keepLiveReferenceAfterWorkOrderCreation: boolean
}

export interface MaintenancePartsKitComplianceResponse {
  complianceRelated: boolean
  governingBodyKeys: string[]
  citationRefs: string[]
  safetyCritical: boolean
  readinessSensitive: boolean
  missingRequiredPartsBlockWorkStart: boolean
  missingRequiredPartsBlockWorkCompletion: boolean
  requireSupervisorApprovalForSubstitution: boolean
  requireDocumentationForSubstitution: boolean
  requireFinalInspectionAfterUse: boolean
  linkedInspectionTemplateId?: string | null
}

export interface MaintenancePartsKitApprovalResponse {
  requiresApprovalBeforeActivation: boolean
  approverRoleKey?: string | null
  approverPersonId?: string | null
  retireReplacedKitAfterActivation: boolean
  notesForApprover?: string | null
}

export interface MaintenancePartsKitDefinitionResponse {
  applicabilityWorkOrderTypes: string[]
  applicabilityPmProgramRefs: string[]
  applicabilityInspectionTemplateRefs: string[]
  applicabilityDefectTypes: string[]
  applicabilityTaskTemplateRefs: string[]
  applicabilityRepairCategories: string[]
  workSourceCompatibilities: string[]
  assetScope: MaintenancePartsKitAssetScopeResponse
  items: MaintenancePartsKitItemResponse[]
  quantityRules: MaintenancePartsKitQuantityRuleResponse[]
  availability: MaintenancePartsKitAvailabilityResponse
  workOrderBehavior: MaintenancePartsKitWorkOrderBehaviorResponse
  compliance: MaintenancePartsKitComplianceResponse
  approval: MaintenancePartsKitApprovalResponse
  changeReason?: string | null
  versionLabel?: string | null
}

export interface MaintenancePartsKitValidationResponse {
  isValid: boolean
  errors: string[]
  warnings: string[]
  compatibleAssetCount: number
  sampleAssetCount: number
  requiredItemCount: number
  optionalItemCount: number
  criticalItemCount: number
  summary: string
  canDraftSave: boolean
  canActivate: boolean
  canApprove: boolean
}

export interface MaintenancePartsKitPreviewItemResponse {
  itemRef: string
  itemDescriptionSnapshot: string
  baseQuantity: number
  calculatedQuantity: number
  unitOfMeasure: string
  criticality: string
  availabilityStatus: string
  availabilityMessage: string
  supplyarrPartId?: string | null
  partNumberSnapshot?: string | null
  required: boolean
  substituteAllowed: boolean
  isPlaceholder: boolean
}

export interface MaintenancePartsKitPreviewResponse {
  validation: MaintenancePartsKitValidationResponse
  sampleAssets: AssetSearchResponse[]
  items: MaintenancePartsKitPreviewItemResponse[]
  warnings: string[]
  blockers: string[]
  assetScopeSummary: string
  workOrderBehaviorSummary: string
  complianceSummary: string
  approvalSummary: string
  availabilitySummary: string
}

export interface MaintenancePartsKitResponse {
  partsKitId: string
  kitNumber: string
  title: string
  description: string
  kitCategoryKey?: string | null
  kitTypeKey?: string | null
  priorityKey?: string | null
  owningSiteRef?: string | null
  owningTeamRef?: string | null
  ownerPersonId?: string | null
  ownerRoleKey?: string | null
  tags?: string[]
  assetTypeApplicability: string[]
  workOrderTypeApplicability: string[]
  pmPlanRef: string | null
  definition?: MaintenancePartsKitDefinitionResponse | null
  status: string
  version?: number
  lineRefs: string[]
  lines: MaintenancePartsKitLineResponse[]
  effectiveAt?: string | null
  expiresAt?: string | null
  activatedAt?: string | null
  approvedAt?: string | null
  retiredAt?: string | null
  cloneSourcePartsKitId?: string | null
  createdByPersonId?: string | null
  updatedByPersonId?: string | null
  activatedByPersonId?: string | null
  approvedByPersonId?: string | null
  retiredByPersonId?: string | null
  createdAt: string
  updatedAt: string
}

export interface MaintenancePartsKitListResponse {
  items: MaintenancePartsKitResponse[]
}

export interface MaintenanceVendorWorkResponse {
  vendorWorkId: string
  workOrderId: string
  supplierRef: string
  vendorContactSnapshot: string | null
  status: string
  workDescription: string | null
  quoteRecordRef: string | null
  approvalRef: string | null
  scheduledAt: string | null
  completedAt: string | null
  costEstimateSnapshot: string | null
  invoiceRecordRef: string | null
  warrantyFlag: boolean
  notes: string | null
  createdAt: string
  updatedAt: string
  duplicate: boolean
}

export interface MaintenanceVendorWorkListResponse {
  items: MaintenanceVendorWorkResponse[]
}

export interface CreateMaintenancePartsKitRequest {
  kitNumber: string
  title: string
  description?: string | null
  assetTypeApplicability?: string[] | null
  workOrderTypeApplicability?: string[] | null
  pmPlanRef?: string | null
  kitCategoryKey?: string | null
  kitTypeKey?: string | null
  priorityKey?: string | null
  owningSiteRef?: string | null
  owningTeamRef?: string | null
  ownerPersonId?: string | null
  ownerRoleKey?: string | null
  tags?: string[] | null
  definition?: MaintenancePartsKitDefinitionRequest | null
  effectiveAt?: string | null
  expiresAt?: string | null
  cloneSourcePartsKitId?: string | null
}

export interface UpdateMaintenancePartsKitRequest {
  title: string
  description?: string | null
  assetTypeApplicability?: string[] | null
  workOrderTypeApplicability?: string[] | null
  pmPlanRef?: string | null
  kitCategoryKey?: string | null
  kitTypeKey?: string | null
  priorityKey?: string | null
  owningSiteRef?: string | null
  owningTeamRef?: string | null
  ownerPersonId?: string | null
  ownerRoleKey?: string | null
  tags?: string[] | null
  definition?: MaintenancePartsKitDefinitionRequest | null
  effectiveAt?: string | null
  expiresAt?: string | null
}

export interface UpdateMaintenancePartsKitStatusRequest {
  status: string
}

export interface MaintenancePartsKitPreviewRequest {
  kitNumber: string
  title: string
  description?: string | null
  assetTypeApplicability?: string[] | null
  workOrderTypeApplicability?: string[] | null
  pmPlanRef?: string | null
  kitCategoryKey?: string | null
  kitTypeKey?: string | null
  priorityKey?: string | null
  owningSiteRef?: string | null
  owningTeamRef?: string | null
  ownerPersonId?: string | null
  ownerRoleKey?: string | null
  tags?: string[] | null
  definition?: MaintenancePartsKitDefinitionRequest | null
  effectiveAt?: string | null
  expiresAt?: string | null
  cloneSourcePartsKitId?: string | null
  selectedAssetId?: string | null
}

export interface MaintenancePartsKitAssetScopeRequest {
  assetClassKeys?: string[] | null
  assetTypeKeys?: string[] | null
  assetCategoryKeys?: string[] | null
  assetStatusKeys?: string[] | null
  siteRefs?: string[] | null
  departmentRefs?: string[] | null
  makeKeys?: string[] | null
  modelKeys?: string[] | null
  yearFrom?: string | null
  yearTo?: string | null
  fuelTypeKeys?: string[] | null
  bodyTypeKeys?: string[] | null
  configurationKeys?: string[] | null
  variantFlags?: string[] | null
  requiredAttributes?: string[] | null
  excludedAttributes?: string[] | null
  includedAssetIds?: string[] | null
  excludedAssetIds?: string[] | null
}

export interface MaintenancePartsKitItemRequest {
  itemRef: string
  supplyarrPartId?: string | null
  itemDescriptionSnapshot: string
  partNumberSnapshot?: string | null
  manufacturerPartNumberSnapshot?: string | null
  vendorPartNumberSnapshot?: string | null
  quantity: number
  unitOfMeasure: string
  required: boolean
  criticality: string
  substituteAllowed: boolean
  preferredSubstituteRefs?: string[] | null
  consumable: boolean
  serialized: boolean
  coreReturnExpected: boolean
  hazardous: boolean
  warrantySensitive: boolean
  requiredByTask?: string | null
  notes?: string | null
  tags?: string[] | null
  isPlaceholder: boolean
}

export interface MaintenancePartsKitQuantityRuleRequest {
  ruleId: string
  ruleType: string
  appliesToItemRef: string
  assetConditionSummary?: string | null
  workConditionSummary?: string | null
  conditionSummary?: string | null
  baseQuantity: number
  multiplier: number
  minimumQuantity?: number | null
  maximumQuantity?: number | null
  roundingBehavior: string
  plainLanguageSummary: string
}

export interface MaintenancePartsKitAvailabilityRequest {
  enabled: boolean
  preferredFulfillmentSource?: string | null
  showSiteAvailability: boolean
  showNearbyAvailability: boolean
  showOnOrder: boolean
  showEstimatedLeadTime: boolean
  requestReservation: boolean
  notes?: string | null
}

export interface MaintenancePartsKitWorkOrderBehaviorRequest {
  canBeManuallyAdded: boolean
  autoSuggestOnMatchingWorkOrder: boolean
  autoAddToMatchingWorkOrder: boolean
  autoAddToPmGeneratedWorkOrder: boolean
  autoAddAfterFailedInspectionQuestion: boolean
  autoAddAfterMatchingDefectType: boolean
  requireSupervisorApprovalBeforeAdding: boolean
  requirePartsReviewBeforeWorkCanStart: boolean
  requireAvailabilityCheckBeforeScheduling: boolean
  allowTechnicianAdjustQuantities: boolean
  requireAdjustmentReason: boolean
  allowTechnicianRemoveOptionalItems: boolean
  allowTechnicianRemoveRequiredItems: boolean
  requireReasonToRemoveRequiredItem: boolean
  snapshotKitItemsOntoWorkOrder: boolean
  keepLiveReferenceAfterWorkOrderCreation: boolean
}

export interface MaintenancePartsKitComplianceRequest {
  complianceRelated: boolean
  governingBodyKeys?: string[] | null
  citationRefs?: string[] | null
  safetyCritical: boolean
  readinessSensitive: boolean
  missingRequiredPartsBlockWorkStart: boolean
  missingRequiredPartsBlockWorkCompletion: boolean
  requireSupervisorApprovalForSubstitution: boolean
  requireDocumentationForSubstitution: boolean
  requireFinalInspectionAfterUse: boolean
  linkedInspectionTemplateId?: string | null
}

export interface MaintenancePartsKitApprovalRequest {
  requiresApprovalBeforeActivation: boolean
  approverRoleKey?: string | null
  approverPersonId?: string | null
  retireReplacedKitAfterActivation: boolean
  notesForApprover?: string | null
}

export interface MaintenancePartsKitDefinitionRequest {
  applicabilityWorkOrderTypes?: string[] | null
  applicabilityPmProgramRefs?: string[] | null
  applicabilityInspectionTemplateRefs?: string[] | null
  applicabilityDefectTypes?: string[] | null
  applicabilityTaskTemplateRefs?: string[] | null
  applicabilityRepairCategories?: string[] | null
  workSourceCompatibilities?: string[] | null
  assetScope?: MaintenancePartsKitAssetScopeRequest | null
  items?: MaintenancePartsKitItemRequest[] | null
  quantityRules?: MaintenancePartsKitQuantityRuleRequest[] | null
  availability?: MaintenancePartsKitAvailabilityRequest | null
  workOrderBehavior?: MaintenancePartsKitWorkOrderBehaviorRequest | null
  compliance?: MaintenancePartsKitComplianceRequest | null
  approval?: MaintenancePartsKitApprovalRequest | null
  changeReason?: string | null
  versionLabel?: string | null
}

export interface CreateMaintenancePartsKitLineRequest {
  itemRef: string
  itemDescriptionSnapshot: string
  quantity: number
  unitOfMeasure: string
  required: boolean
  substituteAllowed: boolean
}

export interface UpdateMaintenancePartsKitLineRequest {
  itemDescriptionSnapshot: string
  quantity: number
  unitOfMeasure: string
  required: boolean
  substituteAllowed: boolean
}

export interface UpsertMaintenanceVendorWorkRequest {
  supplierRef: string
  vendorContactSnapshot?: string | null
  status: string
  workDescription?: string | null
  quoteRecordRef?: string | null
  approvalRef?: string | null
  scheduledAt?: string | null
  completedAt?: string | null
  costEstimateSnapshot?: string | null
  invoiceRecordRef?: string | null
  warrantyFlag: boolean
  notes?: string | null
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
  defectId?: string | null
  draftPlanJson?: string | null
  plannedStartAt?: string | null
  plannedDueAt?: string | null
}

export interface CreateWorkOrderFromDefectRequest {
  title?: string | null
  description?: string | null
  priority?: string | null
  assignedTechnicianPersonId?: string | null
  draftPlanJson?: string | null
  plannedStartAt?: string | null
  plannedDueAt?: string | null
}

export interface UpdateWorkOrderRequest {
  title?: string | null
  description?: string | null
  priority?: string | null
  assignedTechnicianPersonId?: string | null
  draftPlanJson?: string | null
  plannedStartAt?: string | null
  plannedDueAt?: string | null
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
  description?: string | null
  scopeType: string
  assetTypeId: string | null
  assetTypeName: string | null
  assetId: string | null
  assetTag: string | null
  status: string
  autoGenerateWorkOrder?: boolean
  defaultWorkOrderTemplateRef?: string | null
  autoGenerateInspection?: boolean
  inspectionTemplateId?: string | null
  inspectionTemplateKey?: string | null
  inspectionTemplateName?: string | null
  scheduleCount: number
  createdAt: string
  updatedAt: string
  categoryKey?: string | null
  workTypeKey?: string | null
  priorityKey?: string | null
  owningSiteRef?: string | null
  owningTeamRef?: string | null
  owningDepartmentRef?: string | null
  ownerPersonId?: string | null
  ownerRoleKey?: string | null
  tags?: string[] | null
  activatedAt?: string | null
  pausedAt?: string | null
  retiredAt?: string | null
  matchedAssetCount?: number | null
  scopeSummary?: string | null
  dueSummary?: string | null
  workPackageSummary?: string | null
  inspectionSummary?: string | null
  complianceSummary?: string | null
  automationSummary?: string | null
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
  autoGenerateWorkOrder: boolean
  defaultWorkOrderTemplateRef: string | null
  autoGenerateInspection: boolean
  inspectionTemplateId: string | null
  inspectionTemplateKey: string | null
  inspectionTemplateName: string | null
  schedules: PmProgramScheduleLinkResponse[]
  createdAt: string
  updatedAt: string
  categoryKey?: string | null
  workTypeKey?: string | null
  priorityKey?: string | null
  owningSiteRef?: string | null
  owningTeamRef?: string | null
  owningDepartmentRef?: string | null
  ownerPersonId?: string | null
  ownerRoleKey?: string | null
  tags?: string[] | null
  activatedAt?: string | null
  activatedByPersonId?: string | null
  pausedAt?: string | null
  pausedByPersonId?: string | null
  retiredAt?: string | null
  retiredByPersonId?: string | null
  matchedAssetCount?: number | null
  scopeSummary?: string | null
  dueSummary?: string | null
  workPackageSummary?: string | null
  inspectionSummary?: string | null
  complianceSummary?: string | null
  automationSummary?: string | null
}

export interface CreatePmProgramRequest {
  programKey: string
  name: string
  description: string
  scopeType: string
  assetTypeId: string | null
  assetId: string | null
  pmScheduleIds?: string[] | null
  autoGenerateWorkOrder?: boolean
  defaultWorkOrderTemplateRef?: string | null
  autoGenerateInspection?: boolean
  inspectionTemplateId?: string | null
  categoryKey?: string | null
  workTypeKey?: string | null
  priorityKey?: string | null
  owningSiteRef?: string | null
  owningTeamRef?: string | null
  owningDepartmentRef?: string | null
  ownerPersonId?: string | null
  ownerRoleKey?: string | null
  tags?: string[] | null
  scopeDefinition?: PmProgramScopeDefinitionRequest | null
  dueDefinition?: PmProgramDueDefinitionRequest | null
  workPackageDefinition?: PmProgramWorkPackageDefinitionRequest | null
  inspectionDefinition?: PmProgramInspectionDefinitionRequest | null
  complianceDefinition?: PmProgramComplianceDefinitionRequest | null
  automationDefinition?: PmProgramAutomationDefinitionRequest | null
}

export interface UpdatePmProgramRequest {
  name: string
  description: string
  status: string
  autoGenerateWorkOrder?: boolean
  defaultWorkOrderTemplateRef?: string | null
  autoGenerateInspection?: boolean
  inspectionTemplateId?: string | null
  categoryKey?: string | null
  workTypeKey?: string | null
  priorityKey?: string | null
  owningSiteRef?: string | null
  owningTeamRef?: string | null
  owningDepartmentRef?: string | null
  ownerPersonId?: string | null
  ownerRoleKey?: string | null
  tags?: string[] | null
  scopeDefinition?: PmProgramScopeDefinitionRequest | null
  dueDefinition?: PmProgramDueDefinitionRequest | null
  workPackageDefinition?: PmProgramWorkPackageDefinitionRequest | null
  inspectionDefinition?: PmProgramInspectionDefinitionRequest | null
  complianceDefinition?: PmProgramComplianceDefinitionRequest | null
  automationDefinition?: PmProgramAutomationDefinitionRequest | null
}

export interface UpdatePmProgramStatusRequest {
  status: string
}

export interface ActivatePmProgramRequest {
  confirmReadinessImpact?: boolean
  confirmComplianceImpact?: boolean
  confirmZeroMatch?: boolean
}

export interface PmProgramScopeDefinitionRequest {
  assetClassKeys?: string[] | null
  assetTypeIds?: string[] | null
  assetCategoryKeys?: string[] | null
  assetStatusKeys?: string[] | null
  readinessStateKeys?: string[] | null
  siteRefs?: string[] | null
  departmentRefs?: string[] | null
  locationRefs?: string[] | null
  makeKeys?: string[] | null
  modelKeys?: string[] | null
  yearFrom?: number | null
  yearTo?: number | null
  fuelTypeKeys?: string[] | null
  tags?: string[] | null
  includedAssetIds?: string[] | null
  excludedAssetIds?: string[] | null
}

export interface PmProgramCalendarTriggerRequest {
  intervalValue: number
  intervalUnit: string
  anchorDate?: string | null
  firstDueDate?: string | null
  calendarBehavior?: string
  earlyWindowDays?: number
  gracePeriodDays?: number
  pastDueBehavior?: string
}

export interface PmProgramMeterTriggerRequest {
  intervalValue: number
  intervalUnit: string
  anchorReading?: number | null
  firstDueReading?: number | null
  currentReadingSource?: string
  earlyThreshold?: number | null
  graceThreshold?: number | null
  rollingFromCompletion?: boolean
  missingDataBehavior?: string
}

export interface PmProgramOneTimeTriggerRequest {
  dueDate: string
}

export interface PmProgramDueTriggerRequest {
  triggerType: string
  calendar?: PmProgramCalendarTriggerRequest | null
  meter?: PmProgramMeterTriggerRequest | null
  oneTime?: PmProgramOneTimeTriggerRequest | null
  manualOnly?: boolean | null
}

export interface PmProgramDueDefinitionRequest {
  matchLogic: string
  triggers: PmProgramDueTriggerRequest[]
  warnWhenAnyApproaching?: boolean
  markDueBasedOnMostUrgent?: boolean
}

export interface PmProgramWorkPackagePartDemandRequest {
  itemRef: string
  description: string
  quantity: number
  unitOfMeasure: string
}

export interface PmProgramChecklistTaskRequest {
  taskKey: string
  title: string
  description?: string | null
  sortOrder?: number
}

export interface PmProgramWorkPackageDefinitionRequest {
  generateWorkOrder: boolean
  workOrderTitleTemplate?: string | null
  workOrderDescription?: string | null
  defaultPriority?: string | null
  defaultWorkType?: string | null
  estimatedLaborHours?: number | null
  requiredSkills?: string[] | null
  safetyNotes?: string[] | null
  technicianNotes?: string[] | null
  requiredAttachments?: string[] | null
  partsDemand?: PmProgramWorkPackagePartDemandRequest[] | null
  checklistTasks?: PmProgramChecklistTaskRequest[] | null
}

export interface PmProgramInspectionDefinitionRequest {
  attachInspectionTemplate: boolean
  inspectionTemplateId?: string | null
  inspectionRequiredBeforeWorkOrderCompletion?: boolean
  inspectionResultBehavior?: string
  resumeBehaviorRespectsEngineRules?: boolean
  voiceCompatible?: boolean
}

export interface PmProgramComplianceDefinitionRequest {
  isComplianceRelated: boolean
  governingBodyCatalogKey?: string | null
  citationReferences?: string[] | null
  readinessImpact?: string
  certificateRequirements?: string[] | null
}

export interface PmProgramAutomationDefinitionRequest {
  leadTimeDays?: number
  leadThresholdValue?: number | null
  leadThresholdUnit?: string | null
  duplicatePreventionWindowDays?: number
  assignmentBehavior?: string
  assignmentRef?: string | null
  notificationTargets?: string[] | null
  escalationTargets?: string[] | null
  blackoutWindows?: string[] | null
  maxOpenGeneratedItemsPerAsset?: number | null
}

export interface PmProgramPreviewAssetResponse {
  assetId: string
  assetTag: string
  assetName: string
  assetTypeName: string
  siteName: string
  lifecycleStatus: string
  readinessStatus?: string | null
  dueStatus?: string | null
  lastPmAt?: string | null
  lastWorkOrderNumber?: string | null
}

export interface PmProgramScopePreviewResponse {
  matchedAssetCount: number
  excludedAssetCount: number
  sampleAssets: PmProgramPreviewAssetResponse[]
  warnings: string[]
  canActivate: boolean
}

export interface PmProgramDuePreviewItemResponse {
  assetId: string
  assetTag: string
  assetName: string
  triggerSummary: string
  estimatedNextDueDate?: string | null
  estimatedNextDueReading?: string | null
  dueState: string
}

export interface PmProgramDuePreviewResponse {
  dueLogic: string
  items: PmProgramDuePreviewItemResponse[]
  warnings: string[]
  requiresExplicitConfirmation: boolean
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

export interface ExternalIntelligenceProviderSummaryResponse {
  providerKey: string
  displayName: string
  description: string
  sourceOfTruth: string
  status: string
  supportsVinDecode: boolean
  supportsRecallLookup: boolean
  supportsComplaintLookup: boolean
  supportsReferenceLookups: boolean
  supportsEquipmentReferences: boolean
  lastCheckedAt: string | null
  lastSuccessfulAt: string | null
  lastError: string | null
}

export interface ExternalProviderHealthResponse {
  providerKey: string
  status: string
  message: string
  checkedAt: string
  latencyMs: number | null
}

export interface ExternalAssetIdentifierResponse {
  identifierId: string
  assetId: string
  sourceSystem: string
  identifierType: string
  identifierValue: string
  normalizedValue: string
  isPrimary: boolean
  isVerified: boolean
  metadata: Record<string, string | null> | null
  observedAt: string
  createdAt: string
  updatedAt: string
}

export interface AssetEnrichmentSnapshotResponse {
  snapshotId: string
  assetId: string
  providerKey: string
  snapshotType: string
  sourceObjectRef: string | null
  summary: string
  details: Record<string, string | null> | null
  capturedAt: string
  createdAt: string
  updatedAt: string
}

export interface AssetEnrichmentSuggestionResponse {
  suggestionId: string
  assetId: string
  snapshotId: string | null
  providerKey: string
  fieldKey: string
  fieldLabel: string
  currentValue: string | null
  proposedValue: string | null
  reason: string
  confidence: number
  status: string
  reviewedByPersonId: string | null
  reviewedAt: string | null
  createdAt: string
  updatedAt: string
}

export interface AssetRecallSnapshotResponse {
  recallId: string
  assetId: string
  providerKey: string
  campaignNumber: string
  actionNumber: string | null
  manufacturer: string
  component: string
  summary: string
  consequence: string
  remedy: string
  notes: string
  modelYear: string | null
  make: string | null
  model: string | null
  reportReceivedDate: string | null
  status: string
  qualityHoldId: string | null
  capturedAt: string
  createdAt: string
  updatedAt: string
}

export interface AssetComplaintSignalResponse {
  odiNumber: string
  manufacturer: string | null
  crash: boolean
  fire: boolean
  numberOfInjuries: number | null
  numberOfDeaths: number | null
  dateOfIncident: string | null
  dateComplaintFiled: string | null
  vin: string | null
  components: string[]
  summary: string
}

export interface AssetExternalIntelligenceSummaryResponse {
  identifierCount: number
  snapshotCount: number
  suggestionCount: number
  activeRecallCount: number
  complaintCount: number
  lastRefreshedAt: string | null
}

export interface AssetExternalIntelligenceOverviewResponse {
  assetId: string
  vin: string | null
  providers: ExternalIntelligenceProviderSummaryResponse[]
  summary: AssetExternalIntelligenceSummaryResponse
  identifiers: ExternalAssetIdentifierResponse[]
  snapshots: AssetEnrichmentSnapshotResponse[]
  suggestions: AssetEnrichmentSuggestionResponse[]
  recalls: AssetRecallSnapshotResponse[]
  complaints: AssetComplaintSignalResponse[]
}

export interface RecallProviderSummaryResponse {
  providerKey: string
  displayName: string
  description: string
  sourceOfTruth: string
  status: string
  supportsVehicleSearch: boolean
  supportsCampaignSearch: boolean
  supportsManualCampaigns: boolean
  lastCheckedAt: string | null
  lastSuccessfulAt: string | null
  lastError: string | null
}

export interface RecallProviderHealthResponse {
  providerKey: string
  status: string
  message: string
  checkedAt: string
  latencyMs: number | null
}

export interface RecallCampaignApplicabilityResponse {
  applicabilityId: string
  recallCampaignId: string
  modelYear: number | null
  make: string | null
  model: string | null
  assetClass: string | null
  assetType: string | null
  bodyClass: string | null
  vehicleType: string | null
  fuelType: string | null
  engineFamily: string | null
  engineManufacturer: string | null
  componentCategory: string | null
  tireBrand: string | null
  tireLine: string | null
  tireSize: string | null
  equipmentMake: string | null
  equipmentModel: string | null
  serialRangeStart: string | null
  serialRangeEnd: string | null
  productionStartDate: string | null
  productionEndDate: string | null
  notes: string | null
  createdAt: string
  updatedAt: string
}

export interface RecallCampaignResponse {
  campaignId: string
  sourceProvider: string
  sourceType: string
  sourceProviderRecordId: string | null
  nhtsaCampaignNumber: string | null
  nhtsaActionNumber: string | null
  manufacturerCampaignNumber: string | null
  campaignTitle: string | null
  manufacturer: string
  component: string
  reportReceivedDate: string | null
  campaignStartDate: string | null
  campaignEndDate: string | null
  campaignStatus: string
  potentialUnitsAffected: number | null
  summary: string
  consequence: string
  remedy: string
  notes: string
  parkIt: boolean
  parkOutside: boolean
  overTheAirUpdate: boolean
  recallType: string
  sourceUrl: string | null
  fetchedAt: string | null
  applicability: RecallCampaignApplicabilityResponse[]
  assetCaseCount: number
  openCaseCount: number
  verifiedOpenCaseCount: number
  createdAt: string
  updatedAt: string
}

export interface AssetRecallCaseResponse {
  caseId: string
  assetId: string
  recallCampaignId: string
  campaignNumber: string
  campaignTitle: string | null
  manufacturer: string
  component: string
  summary: string
  consequence: string
  remedy: string
  notes: string
  modelYear: string | null
  make: string | null
  model: string | null
  reportReceivedDate: string | null
  sourceProvider: string
  sourceType: string
  sourceUrl: string | null
  fetchedAt: string | null
  matchBasis: string
  matchConfidence: string
  matchScore: number | null
  status: string
  readinessImpact: string
  reason: string
  verificationStatus: string
  verificationSource: string | null
  verificationMethod: string | null
  verifiedByPersonId: string | null
  verifiedAt: string | null
  dismissedByPersonId: string | null
  dismissedAt: string | null
  dismissalReason: string | null
  parkIt: boolean
  parkOutside: boolean
  overTheAirUpdate: boolean
  evidenceDocumentId: string | null
  evidenceUrl: string | null
  evidenceText: string | null
  workOrderId: string | null
  inspectionRunId: string | null
  defectId: string | null
  readinessHoldId: string | null
  actionType: string
  actionStatus: string
  detectedAt: string
  lastRefreshedAt: string | null
  createdAt: string
  updatedAt: string
}

export interface RecallDashboardItemResponse {
  caseId: string
  assetId: string
  assetTag: string
  assetName: string
  campaignNumber: string
  component: string
  matchBasis: string
  matchConfidence: string
  status: string
  readinessImpact: string
  sourceProvider: string
  detectedAt: string
  lastRefreshedAt: string | null
  nextReviewAt: string | null
  workOrderId: string | null
  siteRef: string | null
}

export interface RecallDashboardResponse {
  generatedAt: string
  verifiedOpenRecallCount: number
  potentialMatchCount: number
  parkItWarningCount: number
  parkOutsideWarningCount: number
  workOrdersCreatedCount: number
  completedVerifiedThisMonthCount: number
  overdueReviewCount: number
  assetsNeverCheckedCount: number
  attentionItems: RecallDashboardItemResponse[]
}

export interface RecallVehicleSearchRequest {
  year: number
  make: string
  model: string
}

export interface RecallCampaignSearchRequest {
  campaignNumber: string
}

export interface RecallCampaignApplicabilityRequest {
  modelYear?: number | null
  make?: string | null
  model?: string | null
  assetClass?: string | null
  assetType?: string | null
  bodyClass?: string | null
  vehicleType?: string | null
  fuelType?: string | null
  engineFamily?: string | null
  engineManufacturer?: string | null
  componentCategory?: string | null
  tireBrand?: string | null
  tireLine?: string | null
  tireSize?: string | null
  equipmentMake?: string | null
  equipmentModel?: string | null
  serialRangeStart?: string | null
  serialRangeEnd?: string | null
  productionStartDate?: string | null
  productionEndDate?: string | null
  notes?: string | null
}

export interface CreateRecallCampaignRequest {
  sourceProvider: string
  sourceType: string
  sourceProviderRecordId: string | null
  nhtsaCampaignNumber: string | null
  nhtsaActionNumber: string | null
  manufacturerCampaignNumber: string | null
  campaignTitle: string | null
  manufacturer: string
  component: string
  reportReceivedDate: string | null
  campaignStartDate: string | null
  campaignEndDate: string | null
  campaignStatus: string
  potentialUnitsAffected: number | null
  summary: string
  consequence: string
  remedy: string
  notes: string
  parkIt: boolean
  parkOutside: boolean
  overTheAirUpdate: boolean
  recallType: string
  sourceUrl: string | null
  sourceRawJson: string | null
  applicability: RecallCampaignApplicabilityRequest[] | null
  affectedAssetIds: string[] | null
  createCandidatesNow?: boolean
  createWorkOrdersNow?: boolean
}

export interface UpdateRecallCampaignRequest {
  campaignTitle?: string | null
  manufacturer?: string | null
  component?: string | null
  reportReceivedDate?: string | null
  campaignStartDate?: string | null
  campaignEndDate?: string | null
  campaignStatus?: string | null
  potentialUnitsAffected?: number | null
  summary?: string | null
  consequence?: string | null
  remedy?: string | null
  notes?: string | null
  parkIt?: boolean | null
  parkOutside?: boolean | null
  overTheAirUpdate?: boolean | null
  recallType?: string | null
  sourceUrl?: string | null
  sourceRawJson?: string | null
  applicability?: RecallCampaignApplicabilityRequest[] | null
}

export interface VerifyAssetRecallRequest {
  verificationSource: string
  verificationMethod: string
  verificationStatus: string
  verifiedByPersonId: string | null
  verifiedAt: string | null
  evidenceDocumentId: string | null
  evidenceUrl: string | null
  evidenceText: string | null
  providerRawJson: string | null
  expiresAt: string | null
  nextReviewAt: string | null
  notes: string | null
}

export interface DismissAssetRecallRequest {
  dismissalReason: string
  dismissedByPersonId?: string | null
}

export interface ReleaseRecallHoldRequest {
  releasedByPersonId: string | null
  releaseReason: string | null
}

export interface CreateRecallWorkItemRequest {
  actionType: string
  actionStatus?: string | null
  requiredAction?: string | null
}

export interface ExternalVinDecodeRequest {
  vin: string
  modelYear: number | null
}

export interface ExternalVinDecodeBatchItemRequest {
  vin: string
  modelYear: number | null
}

export interface ExternalVinDecodeBatchRequest {
  items: ExternalVinDecodeBatchItemRequest[]
}

export interface ExternalVinDecodeResponse {
  providerKey: string
  vin: string
  normalizedVin: string
  modelYear: number | null
  isPartial: boolean
  searchCriteria: string | null
  message: string | null
  errorCode: string | null
  errorText: string | null
  additionalErrorText: string | null
  decodedFields: Record<string, string | null>
  suggestions: AssetEnrichmentSuggestionResponse[]
  identifiers: ExternalAssetIdentifierResponse[]
  snapshotId: string | null
  capturedAt: string | null
}

export interface ExternalVinDecodeBatchItemResponse {
  vin: string
  modelYear: number | null
  result: ExternalVinDecodeResponse | null
  error: string | null
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

export interface MaintenancePartResponse {
  partId: string
  partNumber: string
  displayName: string
  description: string
  categoryKey: string
  unitOfMeasure: string
  status: string
  sourceType: string
  sourceLabel: string
  supplyArrPartId: string | null
  manufacturerName: string | null
  manufacturerPartNumber: string | null
  sdsDocumentId: string | null
  complianceCoreMaterialKey: string | null
  complianceCoreHazardKeys: string[]
  notes: string | null
  createdByPersonId: string | null
  updatedByPersonId: string | null
  createdAt: string
  updatedAt: string
}

export interface CreateMaintenancePartRequest {
  partNumber: string
  displayName: string
  description?: string | null
  categoryKey?: string | null
  unitOfMeasure?: string | null
  status?: string | null
  sourceType?: string | null
  supplyArrPartId?: string | null
  manufacturerName?: string | null
  manufacturerPartNumber?: string | null
  sdsDocumentId?: string | null
  complianceCoreMaterialKey?: string | null
  complianceCoreHazardKeys?: string[] | null
  notes?: string | null
}

export interface UpdateMaintenancePartRequest extends CreateMaintenancePartRequest {}

export interface SchedulingWindowResponse {
  start: string | null
  end: string | null
  timezone: string | null
}

export interface SchedulingResourceAssignmentResponse {
  resourceType: string
  resourceId: string
  sourceProductKey: string | null
  displayName: string | null
  role: string | null
}

export interface SchedulingLocationAssignmentRequest {
  siteId?: string | null
  locationId?: string | null
  sourceProductKey?: string | null
  displayName?: string | null
  status?: string | null
}

export interface SchedulingSourceReferenceResponse {
  productKey: string
  objectType: string
  objectId: string
  objectNumber: string | null
}

export interface SchedulingConflictResponse {
  conflictType: string
  code: string
  severity: string
  message: string
  sourceProductKey: string | null
  sourceObjectType: string | null
  sourceObjectId: string | null
  overrideAllowed: boolean
}

export interface SchedulingDisplayItemResponse {
  productKey: string
  itemType: string
  itemId: string
  title: string
  subtitle: string | null
  currentStatus: string
  scheduleStatus: string
  priority: string
  requestedWindow: SchedulingWindowResponse | null
  promisedWindow: SchedulingWindowResponse | null
  scheduledWindow: SchedulingWindowResponse | null
  customerReference: string | null
  orderReference: string | null
  siteId: string | null
  locationId: string | null
  resourceNeeds: SchedulingResourceAssignmentResponse[]
  assignedResources: SchedulingResourceAssignmentResponse[]
  blockers: SchedulingConflictResponse[]
  warnings: SchedulingConflictResponse[]
  sourceRefs: SchedulingSourceReferenceResponse[]
  owningProductUrl: string
  allowedActions: string[]
  permissionFlags: Record<string, boolean>
  freshness: string
}

export interface SchedulingResourceLaneResponse {
  productKey: string
  resourceType: string
  resourceId: string
  displayName: string
  subtitle: string | null
  status: string
  siteId: string | null
  locationId: string | null
}

export interface SchedulingBoardResponse {
  tenantId: string
  productKey: string
  generatedAt: string
  freshness: string
  items: SchedulingDisplayItemResponse[]
  resources: SchedulingResourceLaneResponse[]
}

export interface SchedulingOverrideRequest {
  requested: boolean
  reason?: string | null
  conflictCodes: string[]
}

export interface SchedulingRequest {
  tenantId: string
  productKey: string
  itemType: string
  itemId: string
  requestedStart: string | null
  requestedEnd: string | null
  timezone: string | null
  resourceAssignments: SchedulingResourceAssignmentResponse[]
  locationAssignments: SchedulingLocationAssignmentRequest[]
  assetAssignments: SchedulingResourceAssignmentResponse[]
  reason: string | null
  correlationId: string
  idempotencyKey: string
  sourceContext: SchedulingSourceReferenceResponse[]
  override: SchedulingOverrideRequest | null
  validationOnly: boolean
}

export interface SchedulingValidationResponse {
  status: string
  allowed: boolean
  blockers: SchedulingConflictResponse[]
  warnings: SchedulingConflictResponse[]
  missingPermissions: string[]
  correlationId: string
}

export interface SchedulingMutationResponse {
  status: string
  item: SchedulingDisplayItemResponse
  validation: SchedulingValidationResponse
  eventId: string | null
}
