import type {
  AssignTripDriverRequest,
  AssignTripVehicleRequest,
  CreateRouteRequest,
  DispatchAssignmentPreviewRequest,
  DispatchAssignmentPreviewResponse,
  BulkDispatchApplyRequest,
  BulkDispatchApplyResponse,
  BulkDispatchPreviewRequest,
  BulkDispatchPreviewResponse,
  DispatchCloseoutApplyResponse,
  DispatchCloseoutPreviewResponse,
  DispatchCloseoutRequest,
  DispatchCloseoutSummaryResponse,
  CreateTripRequest,
  DispatchBoardResponse,
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
  TripDetailResponse,
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

export async function getDispatchBoard(
  accessToken: string,
  scope: 'daily' | 'weekly' = 'daily',
): Promise<DispatchBoardResponse> {
  const response = await fetch(`${apiBase}/api/dispatch/board?scope=${encodeURIComponent(scope)}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DispatchBoardResponse>(response, 'Failed to load dispatch board')
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
