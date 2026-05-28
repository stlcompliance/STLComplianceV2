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

export interface DispatchAssignmentWorkflowGateSummary {
  outcome: 'allow' | 'warn' | 'block' | string
  reasonCode: string
  message: string
  isBlocking: boolean
  gates: DispatchWorkflowGateResultSummary[]
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
  scheduledArrivalAt?: string | null
}

export interface CreateRouteRequest {
  title: string
  description: string
  tripId?: string | null
  stops?: CreateRouteStopRequest[] | null
}

export interface LinkRouteTripRequest {
  tripId: string
}

export interface UpdateRouteStopStatusRequest {
  stopStatus: string
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
}

export interface DispatchBoardTripRow {
  tripId: string
  tripNumber: string
  title: string
  dispatchStatus: string
  assignedDriverPersonId: string | null
  scheduledStartAt: string | null
  scheduledEndAt: string | null
  isLate: boolean
  isAtRisk: boolean
  routeCount: number
  pendingStopCount: number
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

export interface CreateEquipmentAvailabilityRequest {
  vehicleRefKey: string
  availabilityStatus: string
  startsAt: string
  endsAt: string
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
