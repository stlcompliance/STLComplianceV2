import type {
  AssignTripDriverRequest,
  AssignTripVehicleRequest,
  CreateRouteRequest,
  DispatchAssignmentAuditListResponse,
  DispatchAssignmentPreviewRequest,
  DispatchAssignmentPreviewResponse,
  DispatchBoardBulkAssignmentPreviewRequest,
  DispatchBoardBulkAssignmentPreviewResponse,
  BulkDispatchApplyRequest,
  BulkDispatchApplyResponse,
  BulkDispatchPreviewRequest,
  BulkDispatchPreviewResponse,
  DispatchCloseoutApplyResponse,
  DispatchCloseoutAuditListResponse,
  DispatchCloseoutChecklistsResponse,
  DispatchCloseoutPreviewResponse,
  DispatchCloseoutRequest,
  DispatchCloseoutSummaryResponse,
  CreateTripRequest,
  DispatchBoardResponse,
  DispatchBoardStateResponse,
  DispatchCommandCenterResponse,
  DispatchExceptionListResponse,
  DispatchExceptionSummaryResponse,
  CreateDispatchExceptionRequest,
  AssignDispatchExceptionRequest,
  ResolveDispatchExceptionRequest,
  LinkDispatchExceptionTripRequest,
  ActiveTripsResponse,
  UnassignedWorkQueueResponse,
  CreateTripProofRequest,
  DriverPortalScheduleResponse,
  SubmitTripDvirRequest,
  TripExecutionSummaryResponse,
  TripProofRecordResponse,
  TripDvirInspectionResponse,
  DispatchReportSummaryResponse,
  DispatchReportTripDetailResponse,
  DispatchReportExceptionDetailResponse,
  ProofDvirReportSummaryResponse,
  ProofDvirReportTripDetailResponse,
  ProofDvirReportProofDetailResponse,
  ProofDvirReportDvirDetailResponse,
  RouteReportSummaryResponse,
  RouteReportRouteDetailResponse,
  RouteReportStopDetailResponse,
  AuditPackageExportResponse,
  AuditPackageExportSummary,
  AuditPackageFilterOptions,
  AuditPackageGenerationJobResponse,
  AuditPackageManifestResponse,
  AuditPackageScope,
  AuditEventTimelineItem,
  EntityExportManifestResponse,
  PagedAuditTimeline,
  TripDetailResponse,
  HandoffSessionResponse,
  LinkRouteTripRequest,
  RoutArrMeResponse,
  RouteCalendarResponse,
  RouteDetailResponse,
  RouteStopSummaryResponse,
  RouteSummaryResponse,
  DriverAvailabilityPanelResponse,
  CreateDriverAvailabilityRequest,
  DriverEligibilityCheckRequest,
  DriverEligibilityCheckResponse,
  AssetDispatchabilityCheckRequest,
  AssetDispatchabilityCheckResponse,
  DispatchWorkflowGateCheckRequest,
  DispatchWorkflowGateCheckResponse,
  EquipmentAvailabilityPanelResponse,
  CreateEquipmentAvailabilityRequest,
  DispatchNotificationDispatchesResponse,
  DispatchNotificationSettingsResponse,
  UpsertDispatchNotificationSettingsRequest,
  TripCompletionRollupSettingsResponse,
  UpsertTripCompletionRollupSettingsRequest,
  PendingTripCompletionRollupsResponse,
  TripCompletionRollupRunsResponse,
  TripSummaryResponse,
  UpdateRouteStopStatusRequest,
  UpdateTripDispatchStatusRequest,
} from './types'

const apiBase = import.meta.env.VITE_ROUTARR_API_BASE ?? ''

export class RoutArrApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly body: string,
  ) {
    super(message)
    this.name = 'RoutArrApiError'
  }
}

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new RoutArrApiError(body || `${fallbackMessage} (${response.status})`, response.status, body)
  }

  return (await response.json()) as T
}

export async function redeemHandoff(handoffCode: string): Promise<HandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/auth/handoff/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  return parseJsonResponse<HandoffSessionResponse>(response, 'Handoff redeem failed')
}

export async function getMe(accessToken: string): Promise<RoutArrMeResponse> {
  const response = await fetch(`${apiBase}/api/me`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RoutArrMeResponse>(response, 'Failed to load profile')
}

export async function getTrips(
  accessToken: string,
  dispatchStatus?: string,
): Promise<TripSummaryResponse[]> {
  const query = dispatchStatus ? `?dispatchStatus=${encodeURIComponent(dispatchStatus)}` : ''
  const response = await fetch(`${apiBase}/api/trips${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TripSummaryResponse[]>(response, 'Failed to load trips')
}

export async function getTrip(accessToken: string, tripId: string): Promise<TripDetailResponse> {
  const response = await fetch(`${apiBase}/api/trips/${tripId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TripDetailResponse>(response, 'Failed to load trip')
}

export async function createTrip(
  accessToken: string,
  payload: CreateTripRequest,
): Promise<TripDetailResponse> {
  const response = await fetch(`${apiBase}/api/trips`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TripDetailResponse>(response, 'Failed to create trip')
}

export async function assignTripDriver(
  accessToken: string,
  tripId: string,
  payload: AssignTripDriverRequest,
): Promise<TripDetailResponse> {
  const response = await fetch(`${apiBase}/api/trips/${tripId}/assign-driver`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TripDetailResponse>(response, 'Failed to assign driver')
}

export async function assignTripVehicle(
  accessToken: string,
  tripId: string,
  payload: AssignTripVehicleRequest,
): Promise<TripDetailResponse> {
  const response = await fetch(`${apiBase}/api/trips/${tripId}/assign-vehicle`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TripDetailResponse>(response, 'Failed to assign vehicle')
}

export async function previewDispatchAssignment(
  accessToken: string,
  payload: DispatchAssignmentPreviewRequest,
): Promise<DispatchAssignmentPreviewResponse> {
  const response = await fetch(`${apiBase}/api/dispatch/assignments/preview`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DispatchAssignmentPreviewResponse>(
    response,
    'Failed to preview assignment',
  )
}

export async function previewDispatchBoardBulkAssignment(
  accessToken: string,
  payload: DispatchBoardBulkAssignmentPreviewRequest,
): Promise<DispatchBoardBulkAssignmentPreviewResponse> {
  const response = await fetch(`${apiBase}/api/dispatch/assignments/bulk-preview`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DispatchBoardBulkAssignmentPreviewResponse>(
    response,
    'Failed to preview bulk dispatch assignment',
  )
}

export async function getDispatchAssignmentAudit(
  accessToken: string,
  limit = 12,
): Promise<DispatchAssignmentAuditListResponse> {
  const response = await fetch(
    `${apiBase}/api/dispatch/assignments/audit?limit=${encodeURIComponent(String(limit))}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<DispatchAssignmentAuditListResponse>(
    response,
    'Failed to load dispatch assignment audit',
  )
}

export async function checkDriverEligibility(
  accessToken: string,
  payload: DriverEligibilityCheckRequest,
): Promise<DriverEligibilityCheckResponse> {
  const response = await fetch(`${apiBase}/api/driver-eligibility/check`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DriverEligibilityCheckResponse>(
    response,
    'Failed to check driver eligibility',
  )
}

export async function checkAssetDispatchability(
  accessToken: string,
  payload: AssetDispatchabilityCheckRequest,
): Promise<AssetDispatchabilityCheckResponse> {
  const response = await fetch(`${apiBase}/api/asset-dispatchability/check`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AssetDispatchabilityCheckResponse>(
    response,
    'Failed to check asset dispatchability',
  )
}

export async function checkDispatchWorkflowGates(
  accessToken: string,
  payload: DispatchWorkflowGateCheckRequest,
): Promise<DispatchWorkflowGateCheckResponse> {
  const response = await fetch(`${apiBase}/api/dispatch-workflow-gates/check`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DispatchWorkflowGateCheckResponse>(
    response,
    'Failed to check dispatch workflow gates',
  )
}

export async function previewBulkDispatch(
  accessToken: string,
  payload: BulkDispatchPreviewRequest,
): Promise<BulkDispatchPreviewResponse> {
  const response = await fetch(`${apiBase}/api/dispatch/bulk/preview`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<BulkDispatchPreviewResponse>(response, 'Failed to preview bulk dispatch')
}

export async function applyBulkDispatch(
  accessToken: string,
  payload: BulkDispatchApplyRequest,
): Promise<BulkDispatchApplyResponse> {
  const response = await fetch(`${apiBase}/api/dispatch/bulk/apply`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<BulkDispatchApplyResponse>(response, 'Failed to apply bulk dispatch')
}

export async function updateTripStatus(
  accessToken: string,
  tripId: string,
  payload: UpdateTripDispatchStatusRequest,
): Promise<TripDetailResponse> {
  const response = await fetch(`${apiBase}/api/trips/${tripId}/status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TripDetailResponse>(response, 'Failed to update trip status')
}

export async function getRoutes(
  accessToken: string,
  tripId?: string,
  routeStatus?: string,
): Promise<RouteSummaryResponse[]> {
  const params = new URLSearchParams()
  if (tripId) params.set('tripId', tripId)
  if (routeStatus) params.set('routeStatus', routeStatus)
  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/routes${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RouteSummaryResponse[]>(response, 'Failed to load routes')
}

export async function getRoute(accessToken: string, routeId: string): Promise<RouteDetailResponse> {
  const response = await fetch(`${apiBase}/api/routes/${routeId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RouteDetailResponse>(response, 'Failed to load route')
}

export async function createRoute(
  accessToken: string,
  payload: CreateRouteRequest,
): Promise<RouteDetailResponse> {
  const response = await fetch(`${apiBase}/api/routes`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<RouteDetailResponse>(response, 'Failed to create route')
}

export async function linkRouteTrip(
  accessToken: string,
  routeId: string,
  payload: LinkRouteTripRequest,
): Promise<RouteDetailResponse> {
  const response = await fetch(`${apiBase}/api/routes/${routeId}/link-trip`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<RouteDetailResponse>(response, 'Failed to link route to trip')
}

export async function updateRouteStopStatus(
  accessToken: string,
  stopId: string,
  payload: UpdateRouteStopStatusRequest,
): Promise<RouteStopSummaryResponse> {
  const response = await fetch(`${apiBase}/api/stops/${stopId}/status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<RouteStopSummaryResponse>(response, 'Failed to update stop status')
}

export async function getUnassignedWorkQueue(
  accessToken: string,
  scope: 'daily' | 'weekly' = 'daily',
): Promise<UnassignedWorkQueueResponse> {
  const response = await fetch(
    `${apiBase}/api/dispatch/unassigned-work-queue?scope=${encodeURIComponent(scope)}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<UnassignedWorkQueueResponse>(
    response,
    'Failed to load unassigned work queue',
  )
}

export async function getDriverPortalSchedule(
  accessToken: string,
): Promise<DriverPortalScheduleResponse> {
  const response = await fetch(`${apiBase}/api/driver-portal/schedule`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DriverPortalScheduleResponse>(
    response,
    'Failed to load driver schedule',
  )
}

async function postDriverPortalTripAction(
  accessToken: string,
  tripId: string,
  action: 'dispatch' | 'start' | 'complete' | 'close',
): Promise<TripDetailResponse> {
  const response = await fetch(`${apiBase}/api/driver-portal/trips/${tripId}/${action}`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TripDetailResponse>(response, `Failed to ${action} trip`)
}

export function dispatchDriverPortalTrip(
  accessToken: string,
  tripId: string,
): Promise<TripDetailResponse> {
  return postDriverPortalTripAction(accessToken, tripId, 'dispatch')
}

export function startDriverPortalTrip(
  accessToken: string,
  tripId: string,
): Promise<TripDetailResponse> {
  return postDriverPortalTripAction(accessToken, tripId, 'start')
}

export function completeDriverPortalTrip(
  accessToken: string,
  tripId: string,
): Promise<TripDetailResponse> {
  return postDriverPortalTripAction(accessToken, tripId, 'complete')
}

export function closeDriverPortalTrip(
  accessToken: string,
  tripId: string,
): Promise<TripDetailResponse> {
  return postDriverPortalTripAction(accessToken, tripId, 'close')
}

export async function getDriverPortalTripExecution(
  accessToken: string,
  tripId: string,
): Promise<TripExecutionSummaryResponse> {
  const response = await fetch(`${apiBase}/api/driver-portal/trips/${tripId}/execution`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TripExecutionSummaryResponse>(
    response,
    'Failed to load trip execution summary',
  )
}

export async function createDriverPortalTripProof(
  accessToken: string,
  tripId: string,
  payload: CreateTripProofRequest,
): Promise<TripProofRecordResponse> {
  const response = await fetch(`${apiBase}/api/driver-portal/trips/${tripId}/proofs`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TripProofRecordResponse>(response, 'Failed to capture trip proof')
}

export async function submitDriverPortalTripDvir(
  accessToken: string,
  tripId: string,
  payload: SubmitTripDvirRequest,
): Promise<TripDvirInspectionResponse> {
  const response = await fetch(`${apiBase}/api/driver-portal/trips/${tripId}/dvir`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TripDvirInspectionResponse>(response, 'Failed to submit DVIR')
}

export async function getTripExecutionSummary(
  accessToken: string,
  tripId: string,
): Promise<TripExecutionSummaryResponse> {
  const response = await fetch(`${apiBase}/api/trips/${tripId}/execution`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TripExecutionSummaryResponse>(
    response,
    'Failed to load trip execution summary',
  )
}

export async function getActiveTrips(
  accessToken: string,
  scope: 'daily' | 'weekly' = 'daily',
): Promise<ActiveTripsResponse> {
  const response = await fetch(
    `${apiBase}/api/dispatch/active-trips?scope=${encodeURIComponent(scope)}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<ActiveTripsResponse>(response, 'Failed to load active trips')
}

export async function getDispatchBoard(
  accessToken: string,
  scope: 'daily' | 'weekly' = 'daily',
): Promise<DispatchBoardResponse> {
  const response = await fetch(`${apiBase}/api/dispatch/board?scope=${encodeURIComponent(scope)}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DispatchBoardResponse>(response, 'Failed to load dispatch board')
}

export async function getDispatchCommandCenter(
  accessToken: string,
  scope?: 'daily' | 'weekly',
): Promise<DispatchCommandCenterResponse> {
  const params = scope ? `?scope=${encodeURIComponent(scope)}` : ''
  const response = await fetch(`${apiBase}/api/dispatch/command-center${params}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DispatchCommandCenterResponse>(
    response,
    'Failed to load dispatch command center',
  )
}

export async function getDispatchBoardState(
  accessToken: string,
): Promise<DispatchBoardStateResponse> {
  const response = await fetch(`${apiBase}/api/dispatch/board-state`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DispatchBoardStateResponse>(response, 'Failed to load dispatch board state')
}

export async function upsertDispatchBoardState(
  accessToken: string,
  defaultScope: 'daily' | 'weekly',
): Promise<DispatchBoardStateResponse> {
  const response = await fetch(`${apiBase}/api/dispatch/board-state`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ defaultScope }),
  })
  return parseJsonResponse<DispatchBoardStateResponse>(response, 'Failed to save dispatch board state')
}

export async function listDispatchExceptions(
  accessToken: string,
  status: string = 'open',
): Promise<DispatchExceptionListResponse> {
  const params = new URLSearchParams({ status })
  const response = await fetch(`${apiBase}/api/dispatch/exceptions?${params.toString()}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DispatchExceptionListResponse>(response, 'Failed to load dispatch exceptions')
}

export async function createDispatchException(
  accessToken: string,
  payload: CreateDispatchExceptionRequest,
): Promise<DispatchExceptionSummaryResponse> {
  const response = await fetch(`${apiBase}/api/dispatch/exceptions`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DispatchExceptionSummaryResponse>(response, 'Failed to create dispatch exception')
}

export async function assignDispatchException(
  accessToken: string,
  exceptionId: string,
  payload: AssignDispatchExceptionRequest,
): Promise<DispatchExceptionSummaryResponse> {
  const response = await fetch(`${apiBase}/api/dispatch/exceptions/${exceptionId}/assign`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DispatchExceptionSummaryResponse>(response, 'Failed to assign dispatch exception')
}

export async function resolveDispatchException(
  accessToken: string,
  exceptionId: string,
  payload: ResolveDispatchExceptionRequest,
): Promise<DispatchExceptionSummaryResponse> {
  const response = await fetch(`${apiBase}/api/dispatch/exceptions/${exceptionId}/resolve`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DispatchExceptionSummaryResponse>(response, 'Failed to resolve dispatch exception')
}

export async function linkDispatchExceptionTrip(
  accessToken: string,
  exceptionId: string,
  payload: LinkDispatchExceptionTripRequest,
): Promise<DispatchExceptionSummaryResponse> {
  const response = await fetch(`${apiBase}/api/dispatch/exceptions/${exceptionId}/link-trip`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DispatchExceptionSummaryResponse>(
    response,
    'Failed to link trip to dispatch exception',
  )
}

export async function getRouteCalendar(
  accessToken: string,
  scope: 'daily' | 'weekly' = 'daily',
  start?: string,
  end?: string,
): Promise<RouteCalendarResponse> {
  const params = new URLSearchParams()
  if (start && end) {
    params.set('start', start)
    params.set('end', end)
  } else {
    params.set('scope', scope)
  }

  const response = await fetch(`${apiBase}/api/dispatch/calendar?${params.toString()}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RouteCalendarResponse>(response, 'Failed to load route calendar')
}

export async function getDriverAvailabilityPanel(
  accessToken: string,
  scope: 'daily' | 'weekly' = 'daily',
): Promise<DriverAvailabilityPanelResponse> {
  const response = await fetch(
    `${apiBase}/api/dispatch/driver-availability?scope=${encodeURIComponent(scope)}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<DriverAvailabilityPanelResponse>(
    response,
    'Failed to load driver availability panel',
  )
}

export async function createDriverAvailability(
  accessToken: string,
  payload: CreateDriverAvailabilityRequest,
): Promise<unknown> {
  const response = await fetch(`${apiBase}/api/driver-availability`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse(response, 'Failed to create driver availability')
}

export async function getEquipmentAvailabilityPanel(
  accessToken: string,
  scope: 'daily' | 'weekly' = 'daily',
): Promise<EquipmentAvailabilityPanelResponse> {
  const response = await fetch(
    `${apiBase}/api/dispatch/equipment-availability?scope=${encodeURIComponent(scope)}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<EquipmentAvailabilityPanelResponse>(
    response,
    'Failed to load equipment availability panel',
  )
}

export async function createEquipmentAvailability(
  accessToken: string,
  payload: CreateEquipmentAvailabilityRequest,
): Promise<unknown> {
  const response = await fetch(`${apiBase}/api/equipment-availability`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse(response, 'Failed to create equipment availability')
}

export async function getDispatchCloseoutSummary(
  accessToken: string,
  scope: 'daily' | 'weekly' = 'daily',
): Promise<DispatchCloseoutSummaryResponse> {
  const response = await fetch(
    `${apiBase}/api/dispatch/closeout/summary?scope=${encodeURIComponent(scope)}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<DispatchCloseoutSummaryResponse>(
    response,
    'Failed to load dispatch closeout summary',
  )
}

export async function previewDispatchCloseout(
  accessToken: string,
  payload: DispatchCloseoutRequest,
): Promise<DispatchCloseoutPreviewResponse> {
  const response = await fetch(`${apiBase}/api/dispatch/closeout/preview`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DispatchCloseoutPreviewResponse>(
    response,
    'Failed to preview dispatch closeout',
  )
}

export async function applyDispatchCloseout(
  accessToken: string,
  payload: DispatchCloseoutRequest,
): Promise<DispatchCloseoutApplyResponse> {
  const response = await fetch(`${apiBase}/api/dispatch/closeout/apply`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DispatchCloseoutApplyResponse>(
    response,
    'Failed to apply dispatch closeout',
  )
}

export async function getDispatchCloseoutChecklists(
  accessToken: string,
  scope: 'daily' | 'weekly',
  remainingTripDisposition: 'complete' | 'cancel',
): Promise<DispatchCloseoutChecklistsResponse> {
  const params = new URLSearchParams({
    scope,
    remainingTripDisposition,
  })
  const response = await fetch(
    `${apiBase}/api/dispatch/closeout/checklists?${params.toString()}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<DispatchCloseoutChecklistsResponse>(
    response,
    'Failed to load dispatch closeout checklists',
  )
}

export async function getDispatchCloseoutAudit(
  accessToken: string,
  limit = 15,
): Promise<DispatchCloseoutAuditListResponse> {
  const response = await fetch(
    `${apiBase}/api/dispatch/closeout/audit?limit=${encodeURIComponent(String(limit))}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<DispatchCloseoutAuditListResponse>(
    response,
    'Failed to load dispatch closeout audit',
  )
}

export async function getDispatchNotificationSettings(
  accessToken: string,
): Promise<DispatchNotificationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/notification-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DispatchNotificationSettingsResponse>(
    response,
    'Failed to load notification settings',
  )
}

export async function upsertDispatchNotificationSettings(
  accessToken: string,
  payload: UpsertDispatchNotificationSettingsRequest,
): Promise<DispatchNotificationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/notification-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DispatchNotificationSettingsResponse>(
    response,
    'Failed to save notification settings',
  )
}

export async function getDispatchNotificationDispatches(
  accessToken: string,
  limit = 20,
): Promise<DispatchNotificationDispatchesResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/notification-settings/dispatches?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DispatchNotificationDispatchesResponse>(
    response,
    'Failed to load notification dispatches',
  )
}

export async function getTripCompletionRollupSettings(
  accessToken: string,
): Promise<TripCompletionRollupSettingsResponse> {
  const response = await fetch(`${apiBase}/api/trip-completion-rollup-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TripCompletionRollupSettingsResponse>(
    response,
    'Failed to load trip completion rollup settings',
  )
}

export async function upsertTripCompletionRollupSettings(
  accessToken: string,
  payload: UpsertTripCompletionRollupSettingsRequest,
): Promise<TripCompletionRollupSettingsResponse> {
  const response = await fetch(`${apiBase}/api/trip-completion-rollup-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TripCompletionRollupSettingsResponse>(
    response,
    'Failed to save trip completion rollup settings',
  )
}

export async function getPendingTripCompletionRollups(
  accessToken: string,
): Promise<PendingTripCompletionRollupsResponse> {
  const response = await fetch(`${apiBase}/api/trip-completion-rollup-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingTripCompletionRollupsResponse>(
    response,
    'Failed to load pending trip completion rollups',
  )
}

export async function getTripCompletionRollupRuns(
  accessToken: string,
  limit = 10,
): Promise<TripCompletionRollupRunsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/trip-completion-rollup-settings/runs?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TripCompletionRollupRunsResponse>(
    response,
    'Failed to load trip completion rollup runs',
  )
}

export async function getDispatchReportSummary(
  accessToken: string,
  options?: { scope?: 'daily' | 'weekly' },
): Promise<DispatchReportSummaryResponse> {
  const params = new URLSearchParams()
  if (options?.scope) {
    params.set('scope', options.scope)
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/reports/dispatch/summary${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DispatchReportSummaryResponse>(
    response,
    'Failed to load dispatch report summary',
  )
}

export async function getDispatchReportTripDetail(
  accessToken: string,
  tripId: string,
): Promise<DispatchReportTripDetailResponse> {
  const response = await fetch(`${apiBase}/api/reports/dispatch/trips/${tripId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DispatchReportTripDetailResponse>(
    response,
    'Failed to load dispatch report trip detail',
  )
}

export async function getDispatchReportExceptionDetail(
  accessToken: string,
  exceptionId: string,
): Promise<DispatchReportExceptionDetailResponse> {
  const response = await fetch(`${apiBase}/api/reports/dispatch/exceptions/${exceptionId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DispatchReportExceptionDetailResponse>(
    response,
    'Failed to load dispatch report exception detail',
  )
}

export async function exportDispatchReportSummaryCsv(
  accessToken: string,
  options?: { scope?: 'daily' | 'weekly' },
): Promise<Blob> {
  const params = new URLSearchParams()
  if (options?.scope) {
    params.set('scope', options.scope)
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/reports/dispatch/summary/export${query}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  })
  if (!response.ok) {
    const body = await response.text()
    throw new RoutArrApiError(
      body || `Dispatch report export failed (${response.status})`,
      response.status,
      body,
    )
  }
  return response.blob()
}

export async function getRouteReportSummary(
  accessToken: string,
  options?: { scope?: 'daily' | 'weekly' },
): Promise<RouteReportSummaryResponse> {
  const params = new URLSearchParams()
  if (options?.scope) {
    params.set('scope', options.scope)
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/reports/routes/summary${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RouteReportSummaryResponse>(
    response,
    'Failed to load route report summary',
  )
}

export async function getRouteReportRouteDetail(
  accessToken: string,
  routeId: string,
): Promise<RouteReportRouteDetailResponse> {
  const response = await fetch(`${apiBase}/api/reports/routes/${routeId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RouteReportRouteDetailResponse>(
    response,
    'Failed to load route report route detail',
  )
}

export async function getRouteReportStopDetail(
  accessToken: string,
  stopId: string,
): Promise<RouteReportStopDetailResponse> {
  const response = await fetch(`${apiBase}/api/reports/routes/stops/${stopId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RouteReportStopDetailResponse>(
    response,
    'Failed to load route report stop detail',
  )
}

export async function exportRouteReportSummaryCsv(
  accessToken: string,
  options?: { scope?: 'daily' | 'weekly' },
): Promise<Blob> {
  const params = new URLSearchParams()
  if (options?.scope) {
    params.set('scope', options.scope)
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/reports/routes/summary/export${query}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  })
  if (!response.ok) {
    const body = await response.text()
    throw new RoutArrApiError(
      body || `Route report export failed (${response.status})`,
      response.status,
      body,
    )
  }
  return response.blob()
}

export async function getProofDvirReportSummary(
  accessToken: string,
  options?: { scope?: 'daily' | 'weekly' },
): Promise<ProofDvirReportSummaryResponse> {
  const params = new URLSearchParams()
  if (options?.scope) {
    params.set('scope', options.scope)
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/reports/proof-dvir/summary${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProofDvirReportSummaryResponse>(
    response,
    'Failed to load proof/DVIR report summary',
  )
}

export async function getProofDvirReportTripDetail(
  accessToken: string,
  tripId: string,
): Promise<ProofDvirReportTripDetailResponse> {
  const response = await fetch(`${apiBase}/api/reports/proof-dvir/trips/${tripId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProofDvirReportTripDetailResponse>(
    response,
    'Failed to load proof/DVIR report trip detail',
  )
}

export async function getProofDvirReportProofDetail(
  accessToken: string,
  proofId: string,
): Promise<ProofDvirReportProofDetailResponse> {
  const response = await fetch(`${apiBase}/api/reports/proof-dvir/proofs/${proofId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProofDvirReportProofDetailResponse>(
    response,
    'Failed to load proof/DVIR report proof detail',
  )
}

export async function getProofDvirReportDvirDetail(
  accessToken: string,
  dvirId: string,
): Promise<ProofDvirReportDvirDetailResponse> {
  const response = await fetch(`${apiBase}/api/reports/proof-dvir/dvir/${dvirId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProofDvirReportDvirDetailResponse>(
    response,
    'Failed to load proof/DVIR report DVIR detail',
  )
}

export async function exportProofDvirReportSummaryCsv(
  accessToken: string,
  options?: { scope?: 'daily' | 'weekly' },
): Promise<Blob> {
  const params = new URLSearchParams()
  if (options?.scope) {
    params.set('scope', options.scope)
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/reports/proof-dvir/summary/export${query}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  })
  if (!response.ok) {
    const body = await response.text()
    throw new RoutArrApiError(
      body || `Proof/DVIR report export failed (${response.status})`,
      response.status,
      body,
    )
  }
  return response.blob()
}

export async function getEntityExportManifest(
  accessToken: string,
): Promise<EntityExportManifestResponse> {
  const response = await fetch(`${apiBase}/api/exports/manifest`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<EntityExportManifestResponse>(
    response,
    'Failed to load export manifest',
  )
}

async function downloadExportBlob(
  accessToken: string,
  path: string,
  errorMessage: string,
): Promise<Blob> {
  const response = await fetch(`${apiBase}${path}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  })
  if (!response.ok) {
    const body = await response.text()
    throw new RoutArrApiError(body || `${errorMessage} (${response.status})`, response.status, body)
  }
  return response.blob()
}

export function exportTripsCsv(accessToken: string, options?: { dispatchStatus?: string }): Promise<Blob> {
  const params = new URLSearchParams()
  if (options?.dispatchStatus) {
    params.set('dispatchStatus', options.dispatchStatus)
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  return downloadExportBlob(accessToken, `/api/exports/trips${query}`, 'Trip export failed')
}

export function exportRoutesCsv(accessToken: string, options?: { routeStatus?: string }): Promise<Blob> {
  const params = new URLSearchParams()
  if (options?.routeStatus) {
    params.set('routeStatus', options.routeStatus)
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  return downloadExportBlob(accessToken, `/api/exports/routes${query}`, 'Route export failed')
}

export function exportDispatchExceptionsCsv(
  accessToken: string,
  options?: { status?: string },
): Promise<Blob> {
  const params = new URLSearchParams()
  if (options?.status) {
    params.set('status', options.status)
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  return downloadExportBlob(
    accessToken,
    `/api/exports/dispatch-exceptions${query}`,
    'Dispatch exception export failed',
  )
}

function buildAuditPackageQuery(
  options?: AuditPackageScope & { format?: string; page?: number; pageSize?: number },
): string {
  const params = new URLSearchParams()
  if (options?.format) {
    params.set('format', options.format)
  }
  if (options?.from) {
    params.set('from', `${options.from}T00:00:00.000Z`)
  }
  if (options?.to) {
    params.set('to', `${options.to}T23:59:59.999Z`)
  }
  if (options?.action) {
    params.set('action', options.action)
  }
  if (options?.result) {
    params.set('result', options.result)
  }
  if (options?.targetType) {
    params.set('targetType', options.targetType)
  }
  if (options?.actorUserId) {
    params.set('actorUserId', options.actorUserId)
  }
  if (options?.page) {
    params.set('page', String(options.page))
  }
  if (options?.pageSize) {
    params.set('pageSize', String(options.pageSize))
  }
  const query = params.toString()
  return query ? `?${query}` : ''
}

function auditPackageJobBody(scope: AuditPackageScope & { format: string }) {
  return {
    format: scope.format,
    from: scope.from ? `${scope.from}T00:00:00.000Z` : undefined,
    to: scope.to ? `${scope.to}T23:59:59.999Z` : undefined,
    action: scope.action,
    result: scope.result,
    targetType: scope.targetType,
    actorUserId: scope.actorUserId,
  }
}

export async function getAuditPackageManifest(
  accessToken: string,
): Promise<AuditPackageManifestResponse> {
  const response = await fetch(`${apiBase}/api/audit-packages/manifest`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AuditPackageManifestResponse>(response, 'Failed to load audit package manifest')
}

export async function getAuditPackageFilterOptions(
  accessToken: string,
): Promise<AuditPackageFilterOptions> {
  const response = await fetch(`${apiBase}/api/audit-packages/filter-options`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AuditPackageFilterOptions>(response, 'Failed to load audit filter options')
}

export async function getAuditPackageExportSummary(
  accessToken: string,
  scope?: AuditPackageScope,
): Promise<AuditPackageExportSummary> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/summary${buildAuditPackageQuery(scope)}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<AuditPackageExportSummary>(response, 'Failed to load audit export summary')
}

export async function getAuditPackageTimeline(
  accessToken: string,
  scope?: AuditPackageScope & { page?: number; pageSize?: number },
): Promise<PagedAuditTimeline> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/timeline${buildAuditPackageQuery({ ...scope, page: scope?.page ?? 1, pageSize: scope?.pageSize ?? 15 })}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<PagedAuditTimeline>(response, 'Failed to load audit timeline')
}

export async function exportAuditPackageZip(
  accessToken: string,
  scope?: AuditPackageScope,
): Promise<Blob> {
  return downloadExportBlob(
    accessToken,
    `/api/audit-packages/export${buildAuditPackageQuery(scope)}`,
    'Audit package ZIP export failed',
  )
}

export async function exportAuditPackageCsv(
  accessToken: string,
  scope?: AuditPackageScope,
): Promise<Blob> {
  return downloadExportBlob(
    accessToken,
    `/api/audit-packages/export${buildAuditPackageQuery({ ...scope, format: 'csv' })}`,
    'Audit events CSV export failed',
  )
}

export async function exportAuditPackageJson(
  accessToken: string,
  scope?: AuditPackageScope,
): Promise<AuditPackageExportResponse> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/export${buildAuditPackageQuery({ ...scope, format: 'json' })}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<AuditPackageExportResponse>(response, 'Failed to export audit package JSON')
}

export async function createAuditPackageGenerationJob(
  accessToken: string,
  scope: AuditPackageScope & { format: 'zip' | 'json' },
): Promise<AuditPackageGenerationJobResponse> {
  const response = await fetch(`${apiBase}/api/audit-packages/jobs`, {
    method: 'POST',
    headers: {
      ...authHeaders(accessToken),
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(auditPackageJobBody(scope)),
  })
  return parseJsonResponse<AuditPackageGenerationJobResponse>(
    response,
    'Failed to queue audit package generation job',
  )
}

export async function getAuditPackageGenerationJob(
  accessToken: string,
  jobId: string,
): Promise<AuditPackageGenerationJobResponse> {
  const response = await fetch(`${apiBase}/api/audit-packages/jobs/${jobId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AuditPackageGenerationJobResponse>(
    response,
    'Failed to load audit package generation job',
  )
}

export async function downloadAuditPackageGenerationJob(
  accessToken: string,
  jobId: string,
): Promise<Blob> {
  return downloadExportBlob(
    accessToken,
    `/api/audit-packages/jobs/${jobId}/download`,
    'Audit package job download failed',
  )
}
