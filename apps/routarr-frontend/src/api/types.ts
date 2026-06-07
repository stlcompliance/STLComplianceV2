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

export interface RoutArrMeResponse {
  userId: string
  personId: string
  email: string
  displayName: string
  tenantId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasRoutArrEntitlement: boolean
  entitlements: string[]
}

export interface RoutArrSessionBootstrapResponse {
  userId: string
  personId: string
  tenantId: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasRoutArrEntitlement: boolean
  entitlements: string[]
}

export interface TripLoadSummaryResponse {
  loadId: string
  loadKey: string
  description: string
  loadType: string
  status: string
  sequenceNumber: number
  originLabel: string
  destinationLabel: string
  createdAt: string
  updatedAt: string
}

export interface TripSummaryResponse {
  tripId: string
  tripNumber: string
  title: string
  dispatchStatus: string
  assignedDriverPersonId: string | null
  vehicleRefKey: string | null
  scheduledStartAt: string | null
  scheduledEndAt: string | null
  loadCount: number
  createdByUserId: string
  createdAt: string
  updatedAt: string
  assignedAt: string | null
  dispatchedAt: string | null
  startedAt: string | null
  completedAt: string | null
  closedAt: string | null
  cancelledAt: string | null
}

export interface TripDetailResponse {
  tripId: string
  tripNumber: string
  title: string
  description: string
  dispatchStatus: string
  assignedDriverPersonId: string | null
  vehicleRefKey: string | null
  scheduledStartAt: string | null
  scheduledEndAt: string | null
  loads: TripLoadSummaryResponse[]
  createdByUserId: string
  createdAt: string
  updatedAt: string
  assignedAt: string | null
  dispatchedAt: string | null
  startedAt: string | null
  completedAt: string | null
  closedAt: string | null
  cancelledAt: string | null
}

export interface CreateTripLoadRequest {
  loadKey: string
  description: string
  loadType: string
  sequenceNumber: number
  originLabel: string
  destinationLabel: string
}

export interface CreateTripRequest {
  title: string
  description: string
  vehicleRefKey?: string | null
  scheduledStartAt?: string | null
  scheduledEndAt?: string | null
  loads?: CreateTripLoadRequest[] | null
}

export interface AssignTripDriverRequest {
  driverPersonId: string
  driverDisplayName?: string | null
  ignoreAvailabilityConflicts?: boolean
  ignoreEligibilityBlocks?: boolean
  ignoreWorkflowGateBlocks?: boolean
}

export interface AssignTripVehicleRequest {
  vehicleRefKey?: string | null
  ignoreAvailabilityConflicts?: boolean
  ignoreDispatchabilityBlocks?: boolean
  ignoreWorkflowGateBlocks?: boolean
}

export interface DispatchAssignmentAvailabilityConflict {
  availabilityId: string
  availabilityStatus: string
  startsAt: string
  endsAt: string
  reason: string
}

export interface DispatchAssignmentTripConflict {
  tripId: string
  tripNumber: string
  title: string
  dispatchStatus: string
  scheduledStartAt: string | null
  scheduledEndAt: string | null
}

export interface DispatchAssignmentPreviewRequest {
  tripId: string
  assignmentKind: 'driver' | 'vehicle'
  driverPersonId?: string | null
  vehicleRefKey?: string | null
}

export interface DispatchAssignmentEligibilitySummary {
  outcome: 'allow' | 'warn' | 'block' | string
  reasonCode: string
  message: string
  isBlocking: boolean
  trainArr: {
    outcome: string
    reasonCode: string
    message: string
    qualificationKey: string | null
  } | null
  staffArr: {
    readinessStatus: string
    readinessBasis: string
    blockerCount: number
    primaryBlockerMessage: string | null
  } | null
}

export interface DispatchAssignmentDispatchabilitySummary {
  outcome: 'allow' | 'warn' | 'block' | string
  reasonCode: string
  message: string
  isBlocking: boolean
  maintainArr: {
    assetId: string
    assetTag: string
    readinessStatus: string
    readinessBasis: string
    blockerCount: number
    primaryBlockerMessage: string | null
  } | null
}

export interface DispatchWorkflowGateResultSummary {
  gateKey: string
  outcome: string
  reasonCode: string
  message: string
  isBlocking: boolean
}

export interface DispatchReleaseReadinessSnapshotResponse {
  snapshotKind: string
  capturedAt: string
  context: Record<string, string>
  gates: DispatchWorkflowGateResultSummary[]
}

export interface DispatchAssignmentWorkflowGateSummary {
  outcome: 'allow' | 'warn' | 'block' | string
  reasonCode: string
  message: string
  isBlocking: boolean
  gates: DispatchWorkflowGateResultSummary[]
  releaseSnapshot?: DispatchReleaseReadinessSnapshotResponse | null
}

export interface DispatchAssignmentConflictSummary {
  driverAvailabilityBlocks: number
  equipmentAvailabilityBlocks: number
  overlappingTrips: number
  eligibilityBlocking: boolean
  eligibilityWarning: boolean
  dispatchabilityBlocking: boolean
  dispatchabilityWarning: boolean
  workflowGateBlocking: boolean
  workflowGateWarning: boolean
  hasMissingExternalData: boolean
  hasStaleExternalData: boolean
}

export interface DispatchAssignmentPreviewResponse {
  tripId: string
  assignmentKind: string
  canAssign: boolean
  hasBlockingConflicts: boolean
  blockingDriverAvailability: DispatchAssignmentAvailabilityConflict[]
  blockingEquipmentAvailability: DispatchAssignmentAvailabilityConflict[]
  overlappingTrips: DispatchAssignmentTripConflict[]
  driverEligibility?: DispatchAssignmentEligibilitySummary | null
  assetDispatchability?: DispatchAssignmentDispatchabilitySummary | null
  workflowGates?: DispatchAssignmentWorkflowGateSummary | null
  conflictSummary?: DispatchAssignmentConflictSummary | null
  validationMessages?: string[] | null
  primaryBlockCode?: string | null
}

export interface DispatchBoardBulkAssignmentItem {
  tripId: string
  assignmentKind: 'driver' | 'vehicle'
  driverPersonId?: string | null
  vehicleRefKey?: string | null
}

export interface DispatchBoardBulkAssignmentPreviewRequest {
  items: DispatchBoardBulkAssignmentItem[]
}

export interface DispatchBoardBulkAssignmentItemPreview {
  tripId: string
  assignmentKind: string
  preview: DispatchAssignmentPreviewResponse
}

export interface DispatchBoardBulkAssignmentPreviewResponse {
  itemCount: number
  canAssignCount: number
  blockedCount: number
  items: DispatchBoardBulkAssignmentItemPreview[]
}

export interface DispatchAssignmentAuditEntry {
  id: string
  actorUserId: string | null
  action: string
  targetType: string
  targetId: string | null
  result: string
  occurredAt: string
}

export interface DispatchAssignmentAuditListResponse {
  entries: DispatchAssignmentAuditEntry[]
}

export interface DriverEligibilityCheckRequest {
  personId: string
  qualificationKey?: string | null
  rulePackKey?: string | null
}

export interface DriverEligibilityCheckResponse {
  personId: string
  outcome: string
  reasonCode: string
  message: string
  isBlocking: boolean
  trainArr: DispatchAssignmentEligibilitySummary['trainArr']
  staffArr: DispatchAssignmentEligibilitySummary['staffArr']
}

export interface BulkDispatchActionItem {
  tripId: string
  driverPersonId?: string | null
  vehicleRefKey?: string | null
  dispatchStatus?: string | null
}

export interface BulkDispatchPreviewRequest {
  items: BulkDispatchActionItem[]
}

export interface BulkDispatchStatusPreview {
  targetStatus: string | null
  canTransition: boolean
  errorCode: string | null
  errorMessage: string | null
}

export interface BulkDispatchItemPreview {
  tripId: string
  tripNumber: string
  title: string
  currentDispatchStatus: string
  canApply: boolean
  hasBlockingConflicts: boolean
  driverPreview: DispatchAssignmentPreviewResponse | null
  vehiclePreview: DispatchAssignmentPreviewResponse | null
  statusPreview: BulkDispatchStatusPreview | null
}

export interface BulkDispatchPreviewSummary {
  total: number
  canApplyCount: number
  blockedCount: number
}

export interface BulkDispatchPreviewResponse {
  summary: BulkDispatchPreviewSummary
  items: BulkDispatchItemPreview[]
}

export interface DispatchWorkflowGateCheckRequest {
  tripId: string
  driverPersonId?: string | null
  vehicleRefKey?: string | null
  assignmentKind?: string | null
}

export interface DispatchWorkflowGateCheckResponse {
  tripId: string
  outcome: string
  reasonCode: string
  message: string
  isBlocking: boolean
  gates: DispatchWorkflowGateResultSummary[]
  releaseSnapshot?: DispatchReleaseReadinessSnapshotResponse | null
}

export interface AssetDispatchabilityCheckRequest {
  vehicleRefKey?: string | null
  assetTag?: string | null
}

export interface AssetDispatchabilityCheckResponse {
  vehicleRefKey: string | null
  assetTag: string | null
  outcome: string
  reasonCode: string
  message: string
  isBlocking: boolean
  maintainArr: DispatchAssignmentDispatchabilitySummary['maintainArr']
}

export interface BulkDispatchApplyRequest {
  items: BulkDispatchActionItem[]
  ignoreAvailabilityConflicts?: boolean
  ignoreEligibilityBlocks?: boolean
  ignoreDispatchabilityBlocks?: boolean
  ignoreWorkflowGateBlocks?: boolean
}

export interface BulkDispatchApplyItemResult {
  tripId: string
  success: boolean
  errorCode: string | null
  errorMessage: string | null
  trip: TripDetailResponse | null
}

export interface BulkDispatchApplySummary {
  total: number
  successCount: number
  failureCount: number
}

export interface BulkDispatchApplyResponse {
  summary: BulkDispatchApplySummary
  results: BulkDispatchApplyItemResult[]
}

export interface DispatchCloseoutRequest {
  scope?: string | null
  remainingTripDisposition: string
  openStopDisposition: string
  tripIds?: string[] | null
}

export interface DispatchCloseoutCountsSummary {
  openTrips: number
  openRoutes: number
  openStops: number
  totalInScopeTrips: number
  totalInScopeRoutes: number
}

export interface DispatchCloseoutSummaryResponse {
  scope: string
  windowStart: string
  windowEnd: string
  counts: DispatchCloseoutCountsSummary
  trips: {
    planned: number
    assigned: number
    dispatched: number
    inProgress: number
    completed: number
    cancelled: number
  }
  routes: {
    draft: number
    planned: number
    active: number
    completed: number
    cancelled: number
  }
  stops: {
    pending: number
    arrived: number
    completed: number
    skipped: number
  }
  openTrips: Array<{
    tripId: string
    tripNumber: string
    title: string
    dispatchStatus: string
    assignedDriverPersonId: string | null
  }>
  openRoutes: Array<{
    routeId: string
    routeNumber: string
    title: string
    routeStatus: string
    tripId: string | null
    openStopCount: number
  }>
}

export interface DispatchCloseoutApplySummary {
  tripCount: number
  tripsCanApply: number
  tripsBlocked: number
  stopCount: number
  stopsCanApply: number
  stopsBlocked: number
  routeCount: number
  routesCanApply: number
  routesBlocked: number
}

export interface DispatchCloseoutPreviewResponse {
  scope: string
  windowStart: string
  windowEnd: string
  remainingTripDisposition: string
  openStopDisposition: string
  summary: DispatchCloseoutApplySummary
  tripActions: Array<{
    tripId: string
    tripNumber: string
    currentDispatchStatus: string
    targetDispatchStatus: string
    canApply: boolean
    blockCode: string | null
    blockMessage: string | null
    transitionSteps: string[]
  }>
  stopActions: Array<{
    stopId: string
    routeId: string
    stopKey: string
    currentStopStatus: string
    targetStopStatus: string
    canApply: boolean
    blockCode: string | null
    blockMessage: string | null
  }>
  routeActions: Array<{
    routeId: string
    routeNumber: string
    currentRouteStatus: string
    targetRouteStatus: string
    canApply: boolean
    blockCode: string | null
    blockMessage: string | null
  }>
}

export interface DispatchCloseoutApplyResponse {
  scope: string
  windowStart: string
  windowEnd: string
  summary: DispatchCloseoutApplySummary
  tripResults: Array<{
    tripId: string
    applied: boolean
    finalDispatchStatus: string | null
    errorCode: string | null
    errorMessage: string | null
  }>
  stopResults: Array<{
    stopId: string
    applied: boolean
    finalStopStatus: string | null
    errorCode: string | null
    errorMessage: string | null
  }>
  routeResults: Array<{
    routeId: string
    applied: boolean
    finalRouteStatus: string | null
    errorCode: string | null
    errorMessage: string | null
  }>
}

export interface DispatchCloseoutChecklistItem {
  key: string
  label: string
  satisfied: boolean
  required: boolean
  detail: string | null
}

export interface DispatchCloseoutTripChecklist {
  tripId: string
  tripNumber: string
  dispatchStatus: string
  readyForCloseout: boolean
  items: DispatchCloseoutChecklistItem[]
}

export interface DispatchCloseoutChecklistsResponse {
  scope: string
  windowStart: string
  windowEnd: string
  remainingTripDisposition: string
  trips: DispatchCloseoutTripChecklist[]
}

export interface DispatchCloseoutAuditEntry {
  id: string
  actorUserId: string | null
  action: string
  targetType: string
  targetId: string | null
  result: string
  occurredAt: string
}

export interface DispatchCloseoutAuditListResponse {
  entries: DispatchCloseoutAuditEntry[]
}

export interface UpdateTripDispatchStatusRequest {
  dispatchStatus: string
}

export interface RouteStopSummaryResponse {
  stopId: string
  stopKey: string
  label: string
  addressLabel: string
  stopType: string
  stopStatus: string
  sequenceNumber: number
  geofenceAnchorLatitude: number | null
  geofenceAnchorLongitude: number | null
  geofenceRadiusMeters: number | null
  lastGeofenceCheckAt: string | null
  lastGeofenceResult: string | null
  lastGeofenceDistanceMeters: number | null
  lastGeofenceReportedLatitude: number | null
  lastGeofenceReportedLongitude: number | null
  scheduledArrivalAt: string | null
  arrivedAt: string | null
  completedAt: string | null
  createdAt: string
  updatedAt: string
}

export interface RouteSummaryResponse {
  routeId: string
  routeNumber: string
  title: string
  routeStatus: string
  tripId: string | null
  stopCount: number
  createdByUserId: string
  createdAt: string
  updatedAt: string
  activatedAt: string | null
  completedAt: string | null
  cancelledAt: string | null
}

export interface RouteDetailResponse {
  routeId: string
  routeNumber: string
  title: string
  description: string
  routeStatus: string
  tripId: string | null
  stops: RouteStopSummaryResponse[]
  createdByUserId: string
  createdAt: string
  updatedAt: string
  activatedAt: string | null
  completedAt: string | null
  cancelledAt: string | null
}

export interface CreateRouteStopRequest {
  stopKey: string
  label: string
  addressLabel: string
  stopType: string
  sequenceNumber: number
  geofenceAnchorLatitude?: number | null
  geofenceAnchorLongitude?: number | null
  geofenceRadiusMeters?: number | null
  scheduledArrivalAt?: string | null
}

export interface CreateRouteRequest {
  title: string
  description: string
  tripId?: string | null
  stops?: CreateRouteStopRequest[] | null
}

export interface TripPartsDemandLineResponse {
  demandLineId: string
  lineNumber: number
  supplyarrPartId: string | null
  partNumber: string
  description: string
  quantityRequested: number
  unitOfMeasure: string
  notes: string
  status: string
  routarrPublicationId: string | null
  supplyarrDemandRefId: string | null
  publishedAt: string | null
  procurementStatus: string
  supplyarrPurchaseRequestId: string | null
  supplyarrPurchaseOrderId: string | null
  quantityReceived: number
  procurementStatusMessage: string
  lastProcurementStatusAt: string | null
  createdAt: string
  updatedAt: string
}

export interface TransportationLoadVisibilityResponse {
  transportationLoadVisibilityId: string
  loadNumber: string
  tripId: string | null
  routeId: string | null
  sourceProduct: string
  sourceObjectRef: string | null
  loadType: string
  status: string
  originLocationRef: string | null
  destinationLocationRef: string | null
  customerRef: string | null
  supplierRef: string | null
  orderRefs: string[]
  expectedReceiptRefs: string[]
  itemSummarySnapshot: string
  handlingRequirements: string[]
  temperatureRequirement: string | null
  hazmatFlag: boolean
  weightSnapshot: number | null
  volumeSnapshot: number | null
  sealNumber: string | null
  documentRefs: string[]
  createdAt: string
  updatedAt: string
}

export interface DockAppointmentNotificationResponse {
  dockAppointmentNotificationId: string
  notificationNumber: string
  sourceTripId: string | null
  sourceRouteId: string | null
  sourceStopId: string | null
  appointmentType: string
  requestedWindowStart: string | null
  requestedWindowEnd: string | null
  confirmedWindowStart: string | null
  confirmedWindowEnd: string | null
  eta: string | null
  status: string
  carrierNameSnapshot: string | null
  driverSnapshot: string | null
  vehicleSnapshot: string | null
  trailerSnapshot: string | null
  sourceProduct: string
  sourceObjectRef: string | null
  rejectionReason: string | null
  sentAt: string | null
  acknowledgedAt: string | null
  confirmedAt: string | null
  canceledAt: string | null
}

export interface CreateTripPartsDemandLineRequest {
  supplyarrPartId?: string | null
  partNumber?: string | null
  description?: string | null
  quantityRequested: number
  unitOfMeasure?: string | null
  notes?: string | null
}

export interface PublishTripPartsDemandRequest {
  createPurchaseRequestDraft: boolean
}

export interface PublishTripPartsDemandResponse {
  publicationId: string
  demandRefId: string
  purchaseRequestId: string | null
  createdPurchaseRequestDraft: boolean
  lines: TripPartsDemandLineResponse[]
}

export interface LinkRouteTripRequest {
  tripId: string
}

export interface UpdateRouteStopStatusRequest {
  stopStatus: string
}

export interface CheckRouteStopGeofenceRequest {
  reportedLatitude: number
  reportedLongitude: number
}

export interface DispatchBoardTripsSummary {
  plannedCount: number
  assignedCount: number
  dispatchedCount: number
  inProgressCount: number
  completedCount: number
  cancelledCount: number
  totalCount: number
  lateCount: number
  atRiskCount: number
}

export interface DispatchBoardRoutesSummary {
  draftCount: number
  plannedCount: number
  activeCount: number
  completedCount: number
  cancelledCount: number
  totalCount: number
}

export interface DispatchBoardStopsSummary {
  pendingCount: number
  arrivedCount: number
  completedCount: number
  skippedCount: number
  totalCount: number
}

export interface DispatchBoardWorkQueueSummary {
  unassignedDriverTripCount: number
  unlinkedRouteCount: number
  pendingStopCount: number
  missingProofTripCount: number
}

export interface DispatchBoardTripRow {
  tripId: string
  tripNumber: string
  title: string
  dispatchStatus: string
  assignedDriverPersonId: string | null
  vehicleRefKey: string | null
  scheduledStartAt: string | null
  scheduledEndAt: string | null
  isLate: boolean
  isAtRisk: boolean
  routeCount: number
  pendingStopCount: number
  missingRequiredProofCount: number
}

export interface DispatchBoardResponse {
  scope: string
  windowStart: string
  windowEnd: string
  trips: DispatchBoardTripsSummary
  routes: DispatchBoardRoutesSummary
  stops: DispatchBoardStopsSummary
  workQueue: DispatchBoardWorkQueueSummary
  assignedTrips: DispatchBoardTripRow[]
  activeTrips: DispatchBoardTripRow[]
  generatedAt: string
}

export interface DispatchBoardStateResponse {
  defaultScope: string
  updatedAt: string
  updatedByUserId: string | null
}

export interface StaffarrPersonRefResponse {
  personId: string
  displayName: string
  mirroredAt: string
}

export interface DispatchCommandCenterTripColumn {
  dispatchStatus: string
  label: string
  count: number
  trips: TripSummaryResponse[]
}

export interface DispatchCommandCenterActionDescriptor {
  actionKey: string
  label: string
  route: string
  httpMethod: string
  description: string
}

export interface DispatchCommandCenterResponse {
  generatedAt: string
  scope: string
  boardState: DispatchBoardStateResponse
  board: DispatchBoardResponse
  tripColumns: DispatchCommandCenterTripColumn[]
  driverRefs: { items: StaffarrPersonRefResponse[] }
  actions: DispatchCommandCenterActionDescriptor[]
}

export interface DispatchExceptionSummaryResponse {
  exceptionId: string
  exceptionKey: string
  title: string
  description: string
  category: string
  status: string
  tripId: string | null
  tripNumber: string | null
  tripTitle: string | null
  assignedToUserId: string | null
  slaDueAt: string | null
  isSlaBreached: boolean
  resolutionTemplateKey: string
  resolutionNotes: string
  createdByUserId: string
  createdAt: string
  updatedAt: string
  assignedAt: string | null
  resolvedAt: string | null
}

export interface DispatchExceptionListResponse {
  totalCount: number
  openCount: number
  overdueCount: number
  items: DispatchExceptionSummaryResponse[]
}

export interface DispatchExceptionResolutionTemplateResponse {
  templateKey: string
  label: string
  defaultResolutionNotes: string
}

export interface CreateDispatchExceptionRequest {
  title: string
  description: string
  category?: string
  tripId?: string
  assignedToUserId?: string
  slaDueAt?: string
}

export interface AssignDispatchExceptionRequest {
  assignedToUserId: string
  slaDueAt?: string
}

export interface ResolveDispatchExceptionRequest {
  resolutionNotes?: string
  resolutionTemplateKey?: string
}

export interface LinkDispatchExceptionTripRequest {
  tripId: string
}

export interface BulkAssignDispatchExceptionsRequest {
  exceptionIds: string[]
  assignedToUserId: string
  slaDueAt?: string
}

export interface BulkResolveDispatchExceptionsRequest {
  exceptionIds: string[]
  resolutionNotes?: string
  resolutionTemplateKey?: string
}

export interface BulkDispatchExceptionActionResult {
  exceptionId: string
  success: boolean
  errorCode: string | null
  errorMessage: string | null
  exception: DispatchExceptionSummaryResponse | null
}

export interface BulkDispatchExceptionActionResponse {
  totalCount: number
  successCount: number
  failureCount: number
  results: BulkDispatchExceptionActionResult[]
}

export interface ActiveTripsSummary {
  totalCount: number
  lateCount: number
  atRiskCount: number
  dispatchedCount: number
  inProgressCount: number
  unassignedCount: number
  openExceptionCount: number
}

export interface ActiveTripRow {
  tripId: string
  tripNumber: string
  title: string
  dispatchStatus: string
  assignedDriverPersonId: string | null
  assignedDriverDisplayName: string | null
  vehicleRefKey: string | null
  scheduledStartAt: string | null
  scheduledEndAt: string | null
  dispatchedAt: string | null
  startedAt: string | null
  isLate: boolean
  isAtRisk: boolean
  routeCount: number
  pendingStopCount: number
  completedStopCount: number
  totalStopCount: number
  stopProgressPercent: number
  openExceptionCount: number
  timelineOffsetPercent: number
  timelineWidthPercent: number
}

export interface ActiveTripsResponse {
  scope: string
  windowStart: string
  windowEnd: string
  summary: ActiveTripsSummary
  items: ActiveTripRow[]
  generatedAt: string
}

export interface UnassignedWorkQueueSummary {
  unassignedCount: number
  lateCount: number
  atRiskCount: number
  urgentCount: number
}

export interface UnassignedWorkQueueTripRow {
  tripId: string
  tripNumber: string
  title: string
  dispatchStatus: string
  scheduledStartAt: string | null
  scheduledEndAt: string | null
  isLate: boolean
  isAtRisk: boolean
  routeCount: number
  pendingStopCount: number
  minutesUntilStart: number
}

export interface UnassignedWorkQueueResponse {
  scope: string
  windowStart: string
  windowEnd: string
  summary: UnassignedWorkQueueSummary
  items: UnassignedWorkQueueTripRow[]
  driverRefs: { items: StaffarrPersonRefResponse[] }
  generatedAt: string
}

export interface DriverPortalTripRow {
  tripId: string
  tripNumber: string
  title: string
  dispatchStatus: string
  vehicleRefKey: string | null
  scheduledStartAt: string | null
  scheduledEndAt: string | null
  dispatchedAt: string | null
  startedAt: string | null
  completedAt: string | null
  closedAt: string | null
  canDispatch: boolean
  canStart: boolean
  canComplete: boolean
  canClose: boolean
  proofCount: number
  hasPreTripDvir: boolean
  hasPostTripDvir: boolean
  captureStartReady: boolean
  captureCompleteReady: boolean
}

export interface TripCaptureReadinessItem {
  key: string
  label: string
  satisfied: boolean
  required: boolean
  message: string | null
}

export interface TripCaptureReadinessResponse {
  tripId: string
  dispatchStatus: string
  canStartTrip: boolean
  canCompleteTrip: boolean
  items: TripCaptureReadinessItem[]
}

export interface TripAuditTrailEntry {
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

export interface TripAuditTrailResponse {
  tripId: string
  entries: TripAuditTrailEntry[]
}

export interface TripExecutionSettingsResponse {
  requirePreTripDvirBeforeStart: boolean
  requirePostTripDvirBeforeComplete: boolean
  requireDeliveryProofBeforeComplete: boolean
  requirePickupProofBeforeStart: boolean
  blockTripStartOnDvirFail: boolean
  blockTripCompleteOnDvirFail: boolean
  requirePickupProofPhotoBeforeStart: boolean
  requireDeliveryProofPhotoBeforeComplete: boolean
  requireDeliverySignatureBeforeComplete: boolean
  requirePreTripDvirPhotoBeforeStart: boolean
  requirePostTripDvirPhotoBeforeComplete: boolean
  updatedAt: string | null
}

export interface UpsertTripExecutionSettingsRequest {
  requirePreTripDvirBeforeStart: boolean
  requirePostTripDvirBeforeComplete: boolean
  requireDeliveryProofBeforeComplete: boolean
  requirePickupProofBeforeStart: boolean
  blockTripStartOnDvirFail: boolean
  blockTripCompleteOnDvirFail: boolean
  requirePickupProofPhotoBeforeStart: boolean
  requireDeliveryProofPhotoBeforeComplete: boolean
  requireDeliverySignatureBeforeComplete: boolean
  requirePreTripDvirPhotoBeforeStart: boolean
  requirePostTripDvirPhotoBeforeComplete: boolean
}

export interface TripCaptureAttachmentResponse {
  attachmentId: string
  tripId: string
  subjectType: string
  subjectId: string
  attachmentKind: string
  fileName: string
  contentType: string
  sizeBytes: number
  notes: string | null
  capturedByPersonId: string
  createdAt: string
}

export interface UploadTripCaptureAttachmentRequest {
  attachmentKind: string
  fileName: string
  contentType: string
  contentBase64: string
  notes?: string | null
}

export interface TripCaptureAttachmentListResponse {
  tripId: string
  subjectType: string
  subjectId: string
  items: TripCaptureAttachmentResponse[]
}

export interface TripProofRecordResponse {
  proofId: string
  tripId: string
  proofType: string
  capturedByPersonId: string
  vehicleRefKey: string | null
  referenceKey: string
  notes: string
  reviewStatus: string
  reviewedByPersonId: string | null
  reviewedAt: string | null
  reviewNotes: string
  capturedAt: string
  createdAt: string
  attachments: TripCaptureAttachmentResponse[]
}

export interface CreateTripProofRequest {
  proofType: string
  vehicleRefKey?: string | null
  referenceKey?: string | null
  notes?: string | null
  capturedAt?: string | null
}

export interface RejectTripProofRequest {
  reason: string
}

export interface CorrectTripProofRequest {
  vehicleRefKey?: string | null
  referenceKey?: string | null
  notes?: string | null
  capturedAt?: string | null
  reason: string
}

export interface TripDvirInspectionResponse {
  dvirId: string
  tripId: string
  phase: string
  vehicleRefKey: string
  result: string
  odometerReading: number | null
  defectNotes: string
  submittedByPersonId: string
  submittedAt: string
  attachments: TripCaptureAttachmentResponse[]
}

export interface SubmitTripDvirRequest {
  phase: string
  vehicleRefKey?: string | null
  result: string
  odometerReading?: number | null
  defectNotes?: string | null
}

export interface TripExecutionSummaryResponse {
  tripId: string
  tripNumber: string
  dispatchStatus: string
  assignedDriverPersonId: string | null
  closedAt: string | null
  proofs: TripProofRecordResponse[]
  dvirInspections: TripDvirInspectionResponse[]
  hasPreTripDvir: boolean
  hasPostTripDvir: boolean
}

export interface DriverPortalScheduleResponse {
  todayStart: string
  todayEnd: string
  upcomingEnd: string
  todayTrips: DriverPortalTripRow[]
  upcomingTrips: DriverPortalTripRow[]
  generatedAt: string
}

export interface DriverPortalReportExceptionRequest {
  title: string
  description: string
  exceptionType?: string
}

export interface DriverTimeTrackingSummaryResponse {
  entryCount: number
  onDutyMinutes: number
  offDutyMinutes: number
  breakMinutes: number
  openEntryCount: number
  workdayStartAt: string | null
  workdayEndAt: string | null
  shortHaulCandidate: boolean
  shortHaulException: boolean
  summaryNote: string
}

export interface DriverTimeEntryResponse {
  entryId: string
  personId: string
  entryType: string
  startsAt: string
  endsAt: string | null
  notes: string
  editReason: string
  isOpen: boolean
  durationMinutes: number
  createdByUserId: string
  updatedByUserId: string | null
  createdAt: string
  updatedAt: string
}

export interface DriverTimeTrackingResponse {
  date: string
  windowStart: string
  windowEnd: string
  summary: DriverTimeTrackingSummaryResponse
  entries: DriverTimeEntryResponse[]
  generatedAt: string
}

export interface CreateDriverTimeEntryRequest {
  entryType: string
  startsAt: string
  endsAt?: string | null
  notes?: string | null
}

export interface UpdateDriverTimeEntryRequest {
  entryType?: string | null
  startsAt?: string | null
  endsAt?: string | null
  notes?: string | null
  editReason: string
}

export interface DispatchReportCountItem {
  key: string
  count: number
}

export interface DispatchReportTripSummaryItem {
  tripId: string
  tripNumber: string
  title: string
  dispatchStatus: string
  assignedDriverPersonId: string | null
  vehicleRefKey: string | null
  scheduledStartAt: string | null
  scheduledEndAt: string | null
  isLate: boolean
  isAtRisk: boolean
  isUnassigned: boolean
  routeCount: number
  openExceptionCount: number
}

export interface DispatchReportExceptionRow {
  exceptionId: string
  exceptionKey: string
  title: string
  category: string
  status: string
  tripId: string | null
  createdAt: string
  updatedAt: string
}

export interface DispatchReportSummaryResponse {
  generatedAt: string
  scope: string
  windowStart: string
  windowEnd: string
  totalTripCount: number
  lateTripCount: number
  atRiskTripCount: number
  unassignedTripCount: number
  openExceptionCount: number
  delayExceptionCount: number
  tripStatusCounts: DispatchReportCountItem[]
  exceptionStatusCounts: DispatchReportCountItem[]
  exceptionCategoryCounts: DispatchReportCountItem[]
  trips: DispatchReportTripSummaryItem[]
  recentExceptions: DispatchReportExceptionRow[]
}

export interface DispatchReportTripDetailResponse {
  tripId: string
  tripNumber: string
  title: string
  description: string
  dispatchStatus: string
  assignedDriverPersonId: string | null
  vehicleRefKey: string | null
  scheduledStartAt: string | null
  scheduledEndAt: string | null
  dispatchedAt: string | null
  startedAt: string | null
  completedAt: string | null
  cancelledAt: string | null
  isLate: boolean
  isAtRisk: boolean
  routeCount: number
  pendingStopCount: number
  linkedExceptionCount: number
  delayExceptionCount: number
  createdAt: string
  updatedAt: string
}

export interface DispatchReportExceptionDetailResponse {
  exceptionId: string
  exceptionKey: string
  title: string
  description: string
  category: string
  status: string
  tripId: string | null
  tripNumber: string | null
  tripTitle: string | null
  assignedToUserId: string | null
  resolutionNotes: string
  createdByUserId: string
  createdAt: string
  updatedAt: string
  assignedAt: string | null
  resolvedByUserId: string | null
  resolvedAt: string | null
}

export interface DispatchOverrideReportCountItem {
  key: string
  count: number
}

export interface DispatchOverrideReportEntry {
  auditEventId: string
  actorUserId: string | null
  action: string
  targetType: string
  targetId: string | null
  result: string
  overrideKinds: string[]
  occurredAt: string
}

export interface DispatchOverrideReportSummaryResponse {
  generatedAt: string
  scope: string
  windowStart: string
  windowEnd: string
  totalOverrideCount: number
  driverAssignmentOverrideCount: number
  vehicleAssignmentOverrideCount: number
  overrideKindCounts: DispatchOverrideReportCountItem[]
  recentOverrides: DispatchOverrideReportEntry[]
}

export interface RouteReportCountItem {
  key: string
  count: number
}

export interface RouteReportRouteSummaryItem {
  routeId: string
  routeNumber: string
  title: string
  routeStatus: string
  tripId: string | null
  tripNumber: string | null
  totalStopCount: number
  pendingStopCount: number
  arrivedStopCount: number
  completedStopCount: number
  skippedStopCount: number
  completionPercent: number
  waitStopCount: number
  detentionStopCount: number
  totalWaitMinutes: number
  totalDetentionMinutes: number
}

export interface RouteReportStopRow {
  stopId: string
  routeId: string
  routeNumber: string
  stopKey: string
  label: string
  stopType: string
  stopStatus: string
  sequenceNumber: number
  scheduledArrivalAt: string | null
  waitMinutes: number
  detentionMinutes: number
  updatedAt: string
}

export interface RouteReportSummaryResponse {
  generatedAt: string
  scope: string
  windowStart: string
  windowEnd: string
  totalRouteCount: number
  totalStopCount: number
  pendingStopCount: number
  arrivedStopCount: number
  completedStopCount: number
  skippedStopCount: number
  waitStopCount: number
  detentionStopCount: number
  totalWaitMinutes: number
  totalDetentionMinutes: number
  routeStatusCounts: RouteReportCountItem[]
  stopStatusCounts: RouteReportCountItem[]
  stopTypeCounts: RouteReportCountItem[]
  routes: RouteReportRouteSummaryItem[]
  recentStops: RouteReportStopRow[]
}

export interface RouteReportStopSummaryRow {
  stopId: string
  stopKey: string
  label: string
  addressLabel: string
  stopType: string
  stopStatus: string
  sequenceNumber: number
  scheduledArrivalAt: string | null
  arrivedAt: string | null
  completedAt: string | null
  waitMinutes: number
  detentionMinutes: number
  updatedAt: string
}

export interface RouteReportAuditHistoryItem {
  occurredAt: string
  action: string
  result: string
  reasonCode: string | null
  actorUserId: string | null
}

export interface RouteReportRouteDetailResponse {
  routeId: string
  routeNumber: string
  title: string
  description: string
  routeStatus: string
  tripId: string | null
  tripNumber: string | null
  tripTitle: string | null
  totalStopCount: number
  pendingStopCount: number
  completedStopCount: number
  skippedStopCount: number
  completionPercent: number
  createdAt: string
  updatedAt: string
  activatedAt: string | null
  completedAt: string | null
  stops: RouteReportStopSummaryRow[]
  history: RouteReportAuditHistoryItem[]
}

export interface RouteReportStopDetailResponse {
  stopId: string
  routeId: string
  routeNumber: string
  routeTitle: string
  tripId: string | null
  tripNumber: string | null
  stopKey: string
  label: string
  addressLabel: string
  stopType: string
  stopStatus: string
  sequenceNumber: number
  scheduledArrivalAt: string | null
  arrivedAt: string | null
  completedAt: string | null
  waitMinutes: number
  detentionMinutes: number
  createdAt: string
  updatedAt: string
}

export interface ProofDvirReportCountItem {
  key: string
  count: number
}

export interface ProofDvirReportTripSummaryItem {
  tripId: string
  tripNumber: string
  title: string
  dispatchStatus: string
  assignedDriverPersonId: string | null
  vehicleRefKey: string | null
  proofCount: number
  hasPreTripDvir: boolean
  hasPostTripDvir: boolean
  failOrConditionalDvirCount: number
}

export interface ProofDvirReportProofRow {
  proofId: string
  tripId: string
  tripNumber: string
  proofType: string
  capturedByPersonId: string
  vehicleRefKey: string | null
  referenceKey: string
  reviewStatus: string
  capturedAt: string
}

export interface ProofDvirReportDvirRow {
  dvirId: string
  tripId: string
  tripNumber: string
  phase: string
  result: string
  vehicleRefKey: string
  submittedByPersonId: string
  submittedAt: string
}

export interface ProofDvirReportSummaryResponse {
  generatedAt: string
  scope: string
  windowStart: string
  windowEnd: string
  totalProofCount: number
  totalDvirCount: number
  tripWithProofOrDvirCount: number
  preTripDvirCount: number
  postTripDvirCount: number
  failOrConditionalDvirCount: number
  proofTypeCounts: ProofDvirReportCountItem[]
  dvirPhaseCounts: ProofDvirReportCountItem[]
  dvirResultCounts: ProofDvirReportCountItem[]
  trips: ProofDvirReportTripSummaryItem[]
  recentProofs: ProofDvirReportProofRow[]
  recentDvirInspections: ProofDvirReportDvirRow[]
}

export interface ProofDvirReportTripDetailResponse {
  tripId: string
  tripNumber: string
  title: string
  dispatchStatus: string
  assignedDriverPersonId: string | null
  vehicleRefKey: string | null
  scheduledStartAt: string | null
  scheduledEndAt: string | null
  proofCount: number
  hasPreTripDvir: boolean
  hasPostTripDvir: boolean
  failOrConditionalDvirCount: number
  proofs: ProofDvirReportProofRow[]
  dvirInspections: ProofDvirReportDvirRow[]
}

export interface ProofDvirReportProofDetailResponse {
  proofId: string
  tripId: string
  tripNumber: string
  tripTitle: string
  proofType: string
  capturedByPersonId: string
  vehicleRefKey: string | null
  referenceKey: string
  notes: string
  reviewStatus: string
  reviewedByPersonId: string | null
  reviewedAt: string | null
  reviewNotes: string
  capturedAt: string
  createdAt: string
}

export interface ProofDvirReportDvirDetailResponse {
  dvirId: string
  tripId: string
  tripNumber: string
  tripTitle: string
  phase: string
  vehicleRefKey: string
  result: string
  odometerReading: number | null
  defectNotes: string
  submittedByPersonId: string
  submittedAt: string
  createdAt: string
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

export interface AuditPackageAppliedFilters {
  from: string | null
  to: string | null
  action: string | null
  result: string | null
  targetType: string | null
  actorUserId: string | null
}

export interface AuditPackageFilterOptions {
  actions: string[]
  results: string[]
  targetTypes: string[]
  actorUserIds: string[]
}

export interface AuditPackageBreakdownItem {
  key: string
  count: number
}

export interface AuditPackageExportSummary {
  filters: AuditPackageAppliedFilters
  counts: { auditEvents: number }
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

export interface AuditPackageGenerationJobResponse {
  jobId: string
  status: string
  format: string
  packageId: string | null
  errorMessage: string | null
  createdAt: string
  startedAt: string | null
  completedAt: string | null
  downloadReady: boolean
}

export interface AuditPackageExportResponse {
  packageId: string
  tenantId: string
  generatedAt: string
  appliedFilters?: AuditPackageAppliedFilters | null
  counts: { auditEvents: number }
}

export interface PagedAuditTimeline {
  items: AuditEventTimelineItem[]
  page: number
  pageSize: number
  totalCount: number
  hasNextPage: boolean
}

export interface EntityExportManifestResponse {
  packageVersion: string
  entities: EntityExportDescriptor[]
  reportExports: ReportExportDescriptor[]
  auditPackageFormats: EntityExportFormatDescriptor[]
}

export interface RouteCalendarEvent {
  eventType: string
  entityId: string
  label: string
  status: string
  scheduledAt: string
  scheduledEndAt: string | null
  tripId: string | null
  routeId: string | null
  tripNumber: string | null
  routeNumber: string | null
  assignedDriverPersonId: string | null
  isLate: boolean
  isAtRisk: boolean
}

export interface RouteCalendarDay {
  date: string
  events: RouteCalendarEvent[]
}

export interface RouteCalendarSummary {
  tripCount: number
  routeCount: number
  stopCount: number
  lateTripCount: number
  atRiskTripCount: number
}

export interface RouteCalendarResponse {
  scope: string
  windowStart: string
  windowEnd: string
  days: RouteCalendarDay[]
  summary: RouteCalendarSummary
  generatedAt: string
}

export interface DriverAvailabilityTripConflict {
  tripId: string
  tripNumber: string
  title: string
  dispatchStatus: string
  scheduledStartAt: string | null
  scheduledEndAt: string | null
}

export interface DriverAvailabilityPanelSummary {
  recordCount: number
  unavailableCount: number
  limitedCount: number
  availableCount: number
  conflictCount: number
}

export interface DriverAvailabilityPanelRow {
  availabilityId: string
  personId: string
  availabilityStatus: string
  startsAt: string
  endsAt: string
  reason: string
  hasConflict: boolean
  conflictingTripCount: number
  conflictingTrips: DriverAvailabilityTripConflict[]
}

export interface DriverAvailabilityPanelResponse {
  scope: string
  windowStart: string
  windowEnd: string
  summary: DriverAvailabilityPanelSummary
  records: DriverAvailabilityPanelRow[]
  generatedAt: string
}

export interface CreateDriverAvailabilityRequest {
  personId: string
  availabilityStatus: string
  startsAt: string
  endsAt: string
  reason?: string | null
  notes?: string | null
}

export interface UpdateDriverAvailabilityRequest {
  availabilityStatus?: string | null
  startsAt?: string | null
  endsAt?: string | null
  reason?: string | null
  notes?: string | null
}

export interface EquipmentAvailabilityTripConflict {
  tripId: string
  tripNumber: string
  title: string
  dispatchStatus: string
  scheduledStartAt: string | null
  scheduledEndAt: string | null
}

export interface EquipmentAvailabilityPanelSummary {
  recordCount: number
  unavailableCount: number
  limitedCount: number
  availableCount: number
  conflictCount: number
}

export interface EquipmentAvailabilityPanelRow {
  availabilityId: string
  vehicleRefKey: string
  availabilityStatus: string
  startsAt: string
  endsAt: string
  reason: string
  hasConflict: boolean
  conflictingTripCount: number
  conflictingTrips: EquipmentAvailabilityTripConflict[]
}

export interface EquipmentAvailabilityPanelResponse {
  scope: string
  windowStart: string
  windowEnd: string
  summary: EquipmentAvailabilityPanelSummary
  records: EquipmentAvailabilityPanelRow[]
  generatedAt: string
}

export interface DriverResponse {
  personId: string
  displayName: string
  mirroredAt: string
}

export interface DriverListResponse {
  items: DriverResponse[]
}

export interface VehicleRefResponse {
  vehicleRefKey: string
  displayLabel: string
  assetTag: string | null
  mirroredAt: string | null
  fromMirror: boolean
}

export interface VehicleRefListResponse {
  items: VehicleRefResponse[]
}

export interface CreateEquipmentAvailabilityRequest {
  vehicleRefKey: string
  availabilityStatus: string
  startsAt: string
  endsAt: string
  reason?: string | null
  notes?: string | null
}

export interface UpdateEquipmentAvailabilityRequest {
  availabilityStatus?: string | null
  startsAt?: string | null
  endsAt?: string | null
  reason?: string | null
  notes?: string | null
}

export interface DispatchNotificationSettingsResponse {
  isEnabled: boolean
  notificationWebhookUrl: string | null
  notifyOnTripAssigned: boolean
  notifyOnTripDispatched: boolean
  notifyOnTripInProgress: boolean
  notifyOnTripCompleted: boolean
  notifyOnTripCancelled: boolean
  updatedAt: string | null
}

export interface UpsertDispatchNotificationSettingsRequest {
  isEnabled: boolean
  notificationWebhookUrl: string | null
  notifyOnTripAssigned: boolean
  notifyOnTripDispatched: boolean
  notifyOnTripInProgress: boolean
  notifyOnTripCompleted: boolean
  notifyOnTripCancelled: boolean
  clearNotificationWebhookOnDisable?: boolean
}

export interface DispatchNotificationDispatchItem {
  notificationId: string
  eventKind: string
  dispatchStatus: string
  tripId: string
  driverPersonId: string | null
  relatedEntityType: string
  relatedEntityId: string
  webhookHost: string | null
  httpStatusCode: number | null
  errorMessage: string | null
  createdAt: string
  dispatchedAt: string | null
}

export interface DispatchNotificationDispatchesResponse {
  items: DispatchNotificationDispatchItem[]
}

export interface IntegrationEventSettingsResponse {
  isEnabled: boolean
  maxAttempts: number
  retryIntervalMinutes: number
  updatedAt: string | null
}

export interface UpsertIntegrationEventSettingsRequest {
  isEnabled: boolean
  maxAttempts?: number | null
  retryIntervalMinutes?: number | null
}

export interface IntegrationOutboxEventListItem {
  outboxEventId: string
  eventKind: string
  processingStatus: string
  relatedEntityType: string
  relatedEntityId: string
  attemptCount: number
  errorMessage: string | null
  createdAt: string
  processedAt: string | null
}

export interface IntegrationOutboxEventListResponse {
  items: IntegrationOutboxEventListItem[]
}

export interface TripCompletionRollupSettingsResponse {
  isEnabled: boolean
  stalenessHours: number
  updatedAt: string | null
}

export interface UpsertTripCompletionRollupSettingsRequest {
  isEnabled: boolean
  stalenessHours: number
}

export interface PendingTripCompletionRollupItem {
  tripId: string
  tripNumber: string
  title: string
  dispatchStatus: string
  tripUpdatedAt: string
  lastComputedAt: string | null
}

export interface PendingTripCompletionRollupsResponse {
  asOfUtc: string
  stalenessHours: number
  batchSize: number
  items: PendingTripCompletionRollupItem[]
}

export interface TripCompletionRollupRunItem {
  runId: string
  asOfUtc: string
  candidatesFound: number
  refreshedCount: number
  skippedCount: number
  createdAt: string
}

export interface TripCompletionRollupRunsResponse {
  items: TripCompletionRollupRunItem[]
}

export interface TripCompletionSummaryResponse {
  tripId: string
  tripNumber: string
  title: string
  dispatchStatus: string
  assignedDriverPersonId: string | null
  vehicleRefKey: string | null
  scheduledStartAt: string | null
  scheduledEndAt: string | null
  startedAt: string | null
  completedAt: string | null
  cancelledAt: string | null
  durationMinutes: number | null
  routeCount: number
  completedRouteCount: number
  stopCount: number
  completedStopCount: number
  skippedStopCount: number
  pendingStopCount: number
  loadCount: number
  deliveredLoadCount: number
  pendingLoadCount: number
  sourceUpdatedAt: string
  computedAt: string
  isMaterialized: boolean
}

export interface TripCompletionEventResponse {
  eventKind: string
  title: string
  detail: string | null
  occurredAt: string
  sequenceNumber: number
  sourceEntityType: string
  sourceEntityId: string
}

export interface TripCompletionDetailResponse {
  summary: TripCompletionSummaryResponse
  events: TripCompletionEventResponse[]
}

export interface TripCompletionsListResponse {
  items: TripCompletionSummaryResponse[]
}

export interface RouteCompletionSummaryResponse {
  routeId: string
  routeNumber: string
  title: string
  routeStatus: string
  tripId: string | null
  tripNumber: string | null
  tripDispatchStatus: string | null
  stopCount: number
  completedStopCount: number
  skippedStopCount: number
  completedAt: string | null
  computedAt: string | null
  isMaterialized: boolean
}

export interface RouteCompletionsListResponse {
  items: RouteCompletionSummaryResponse[]
}

export interface AttachmentRetentionSettingsResponse {
  isEnabled: boolean
  retentionDaysAfterTripClose: number
  updatedAt: string | null
}

export interface UpsertAttachmentRetentionSettingsRequest {
  isEnabled: boolean
  retentionDaysAfterTripClose: number
}

export interface AttachmentRetentionRunItem {
  runId: string
  outcome: string
  attachmentsPurgedCount: number
  bytesReclaimed: number
  skippedCount: number
  skipReason: string | null
  processedAt: string
}

export interface AttachmentRetentionRunsResponse {
  items: AttachmentRetentionRunItem[]
}
