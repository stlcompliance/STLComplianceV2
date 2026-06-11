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
  DispatchExceptionResolutionTemplateResponse,
  CreateDispatchExceptionRequest,
  AssignDispatchExceptionRequest,
  ResolveDispatchExceptionRequest,
  LinkDispatchExceptionTripRequest,
  BulkAssignDispatchExceptionsRequest,
  BulkResolveDispatchExceptionsRequest,
  BulkDispatchExceptionActionResponse,
  ActiveTripsResponse,
  UnassignedWorkQueueResponse,
  CreateTripProofRequest,
  CorrectTripProofRequest,
  DriverPortalScheduleResponse,
  DriverPortalReportExceptionRequest,
  DriverTimeEntryResponse,
  DriverTimeTrackingResponse,
  CreateDriverTimeEntryRequest,
  UpdateDriverTimeEntryRequest,
  SubmitTripDvirRequest,
  TripExecutionSummaryResponse,
  TripCaptureReadinessResponse,
  TripAuditTrailResponse,
  TripExecutionSettingsResponse,
  UpsertTripExecutionSettingsRequest,
  TripProofRecordResponse,
  RejectTripProofRequest,
  TripDvirInspectionResponse,
  TripCaptureAttachmentResponse,
  TripCaptureAttachmentListResponse,
  UploadTripCaptureAttachmentRequest,
  TripLoadSummaryResponse,
  TransportationLoadVisibilityResponse,
  DockAppointmentNotificationResponse,
  TripPartsDemandLineResponse,
  CreateTripPartsDemandLineRequest,
  PublishTripPartsDemandRequest,
  PublishTripPartsDemandResponse,
  EntityExportManifestResponse,
  TripDetailResponse,
  HandoffSessionResponse,
  LinkRouteTripRequest,
  CheckRouteStopGeofenceRequest,
  RoutArrMeResponse,
  RoutArrSessionBootstrapResponse,
  RouteCalendarResponse,
  RouteDetailResponse,
  RouteStopSummaryResponse,
  RouteSummaryResponse,
  DriverAvailabilityPanelResponse,
  DriverListResponse,
  CreateDriverAvailabilityRequest,
  UpdateDriverAvailabilityRequest,
  DriverEligibilityCheckRequest,
  DriverEligibilityCheckResponse,
  AssetDispatchabilityCheckRequest,
  AssetDispatchabilityCheckResponse,
  DispatchWorkflowGateCheckRequest,
  DispatchWorkflowGateCheckResponse,
  EquipmentAvailabilityPanelResponse,
  VehicleRefListResponse,
  CreateEquipmentAvailabilityRequest,
  UpdateEquipmentAvailabilityRequest,
  DispatchNotificationDispatchesResponse,
  DispatchNotificationSettingsResponse,
  UpsertDispatchNotificationSettingsRequest,
  IntegrationEventSettingsResponse,
  UpsertIntegrationEventSettingsRequest,
  IntegrationOutboxEventListResponse,
  TripCompletionRollupSettingsResponse,
  UpsertTripCompletionRollupSettingsRequest,
  PendingTripCompletionRollupsResponse,
  TripCompletionRollupRunsResponse,
  TripCompletionsListResponse,
  TripCompletionDetailResponse,
  RouteCompletionsListResponse,
  AttachmentRetentionSettingsResponse,
  UpsertAttachmentRetentionSettingsRequest,
  AttachmentRetentionRunsResponse,
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

type ProblemDetailsLike = {
  title?: string
  detail?: string
  errors?: Record<string, string[] | string>
}

function extractProblemDetailsMessage(body: string): string | null {
  if (!body.trim()) {
    return null
  }

  try {
    const parsed = JSON.parse(body) as ProblemDetailsLike
    const parts: string[] = []

    if (typeof parsed.title === 'string' && parsed.title.trim()) {
      parts.push(parsed.title.trim())
    }

    if (typeof parsed.detail === 'string' && parsed.detail.trim()) {
      parts.push(parsed.detail.trim())
    }

    const errorEntries = parsed.errors ? Object.entries(parsed.errors) : []
    if (errorEntries.length > 0) {
      const flattened = errorEntries
        .flatMap(([field, value]) => {
          const values = Array.isArray(value) ? value : [value]
          return values
            .map((message) => String(message).trim())
            .filter(Boolean)
            .map((message) => `${field}: ${message}`)
        })
      if (flattened.length > 0) {
        parts.push(flattened.join('; '))
      }
    }

    return parts.length > 0 ? parts.join(' - ') : null
  } catch {
    return null
  }
}

async function toApiError(response: Response, fallbackMessage: string): Promise<RoutArrApiError> {
  const body = await response.text()
  const parsedMessage = extractProblemDetailsMessage(body)
  const message = parsedMessage || body || `${fallbackMessage} (${response.status})`
  return new RoutArrApiError(message, response.status, body)
}

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    throw await toApiError(response, fallbackMessage)
  }

  return (await response.json()) as T
}

export async function redeemHandoff(handoffCode: string): Promise<HandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/auth/nexarr/redeem`, {
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

export async function getSessionBootstrap(
  accessToken: string,
): Promise<RoutArrSessionBootstrapResponse> {
  const response = await fetch(`${apiBase}/api/session`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RoutArrSessionBootstrapResponse>(
    response,
    'Failed to load session bootstrap',
  )
}

export async function listDrivers(accessToken: string): Promise<DriverListResponse> {
  const response = await fetch(`${apiBase}/api/drivers`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DriverListResponse>(response, 'Failed to load drivers')
}

export async function listVehicleRefs(accessToken: string): Promise<VehicleRefListResponse> {
  const response = await fetch(`${apiBase}/api/vehicle-refs`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<VehicleRefListResponse>(response, 'Failed to load vehicle references')
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

export async function getDashboard(
  accessToken: string,
  scope?: string,
): Promise<DispatchCommandCenterResponse> {
  const query = scope ? `?scope=${encodeURIComponent(scope)}` : ''
  const response = await fetch(`${apiBase}/api/v1/dashboard${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DispatchCommandCenterResponse>(response, 'Failed to load dashboard')
}

export async function getTrip(accessToken: string, tripId: string): Promise<TripDetailResponse> {
  const response = await fetch(`${apiBase}/api/trips/${tripId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TripDetailResponse>(response, 'Failed to load trip')
}

export async function getTripByNumber(
  accessToken: string,
  tripNumber: string,
): Promise<TripDetailResponse> {
  const response = await fetch(
    `${apiBase}/api/trips/by-number/${encodeURIComponent(tripNumber)}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<TripDetailResponse>(response, 'Failed to load trip by number')
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

export async function getRouteStops(
  accessToken: string,
  routeId: string,
): Promise<RouteStopSummaryResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/stops?routeId=${encodeURIComponent(routeId)}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RouteStopSummaryResponse[]>(response, 'Failed to load route stops')
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

export async function optimizeRouteStops(
  accessToken: string,
  routeId: string,
): Promise<RouteDetailResponse> {
  const response = await fetch(`${apiBase}/api/routes/${routeId}/optimize`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RouteDetailResponse>(response, 'Failed to optimize route stops')
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

export async function checkRouteStopGeofence(
  accessToken: string,
  stopId: string,
  payload: CheckRouteStopGeofenceRequest,
): Promise<RouteStopSummaryResponse> {
  const response = await fetch(`${apiBase}/api/routes/stops/${stopId}/geofence-check`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<RouteStopSummaryResponse>(response, 'Failed to check stop geofence')
}

export async function getTripLoads(
  accessToken: string,
  tripId: string,
): Promise<TripLoadSummaryResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/loads?tripId=${encodeURIComponent(tripId)}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TripLoadSummaryResponse[]>(response, 'Failed to load trip loads')
}

export async function getLoadVisibility(
  accessToken: string,
  tripId?: string,
): Promise<TransportationLoadVisibilityResponse[]> {
  const query = tripId ? `?tripId=${encodeURIComponent(tripId)}` : ''
  const response = await fetch(`${apiBase}/api/v1/load-visibility${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TransportationLoadVisibilityResponse[]>(
    response,
    'Failed to load transportation visibility',
  )
}

export async function getDockAppointments(
  accessToken: string,
  tripId?: string,
): Promise<DockAppointmentNotificationResponse[]> {
  const query = tripId ? `?tripId=${encodeURIComponent(tripId)}` : ''
  const response = await fetch(`${apiBase}/api/v1/dock-appointments${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DockAppointmentNotificationResponse[]>(
    response,
    'Failed to load dock appointments',
  )
}

export async function getTripPartsDemand(
  accessToken: string,
  tripId: string,
): Promise<TripPartsDemandLineResponse[]> {
  const response = await fetch(
    `${apiBase}/api/trips/${tripId}/parts-demand`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<TripPartsDemandLineResponse[]>(response, 'Failed to load trip parts demand')
}

export async function createTripPartsDemandLine(
  accessToken: string,
  tripId: string,
  payload: CreateTripPartsDemandLineRequest,
): Promise<TripPartsDemandLineResponse> {
  const response = await fetch(`${apiBase}/api/trips/${tripId}/parts-demand`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TripPartsDemandLineResponse>(response, 'Failed to create trip parts demand line')
}

export async function publishTripPartsDemand(
  accessToken: string,
  tripId: string,
  payload: PublishTripPartsDemandRequest,
): Promise<PublishTripPartsDemandResponse> {
  const response = await fetch(`${apiBase}/api/trips/${tripId}/parts-demand/publish`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<PublishTripPartsDemandResponse>(response, 'Failed to publish trip parts demand')
}

export async function getUnassignedWorkQueue(
  accessToken: string,
  scope: 'daily' | 'weekly' = 'daily',
  options?: { attentionOnly?: boolean },
): Promise<UnassignedWorkQueueResponse> {
  const params = new URLSearchParams({ scope })
  if (options?.attentionOnly) {
    params.set('attentionOnly', 'true')
  }
  const response = await fetch(
    `${apiBase}/api/dispatch/unassigned-work-queue?${params.toString()}`,
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

export async function getDriverPortalTimeTracking(
  accessToken: string,
  date?: string,
): Promise<DriverTimeTrackingResponse> {
  const params = new URLSearchParams()
  if (date) {
    params.set('date', date)
  }
  const query = params.toString()
  const response = await fetch(
    `${apiBase}/api/driver-portal/time-tracking${query ? `?${query}` : ''}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<DriverTimeTrackingResponse>(
    response,
    'Failed to load driver time tracking',
  )
}

export async function createDriverPortalTimeEntry(
  accessToken: string,
  payload: CreateDriverTimeEntryRequest,
): Promise<DriverTimeEntryResponse> {
  const response = await fetch(`${apiBase}/api/driver-portal/time-tracking`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DriverTimeEntryResponse>(
    response,
    'Failed to create driver time entry',
  )
}

export async function updateDriverPortalTimeEntry(
  accessToken: string,
  entryId: string,
  payload: UpdateDriverTimeEntryRequest,
): Promise<DriverTimeEntryResponse> {
  const response = await fetch(`${apiBase}/api/driver-portal/time-tracking/${entryId}`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DriverTimeEntryResponse>(
    response,
    'Failed to update driver time entry',
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

export async function rejectProofDvirReportProof(
  accessToken: string,
  tripId: string,
  proofId: string,
  payload: RejectTripProofRequest,
): Promise<TripProofRecordResponse> {
  const response = await fetch(`${apiBase}/api/reports/proof-dvir/trips/${tripId}/proofs/${proofId}/reject`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TripProofRecordResponse>(response, 'Failed to reject trip proof')
}

export async function correctProofDvirReportProof(
  accessToken: string,
  tripId: string,
  proofId: string,
  payload: CorrectTripProofRequest,
): Promise<TripProofRecordResponse> {
  const response = await fetch(`${apiBase}/api/reports/proof-dvir/trips/${tripId}/proofs/${proofId}`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TripProofRecordResponse>(response, 'Failed to correct trip proof')
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

export async function reportDriverPortalTripException(
  accessToken: string,
  tripId: string,
  payload: DriverPortalReportExceptionRequest,
): Promise<DispatchExceptionSummaryResponse> {
  const response = await fetch(`${apiBase}/api/driver-portal/trips/${tripId}/exceptions`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DispatchExceptionSummaryResponse>(
    response,
    'Failed to report trip exception',
  )
}

export async function submitTripDvir(
  accessToken: string,
  tripId: string,
  payload: SubmitTripDvirRequest,
): Promise<TripDvirInspectionResponse> {
  const response = await fetch(`${apiBase}/api/trips/${tripId}/dvir`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TripDvirInspectionResponse>(response, 'Failed to submit DVIR')
}

export async function getDriverPortalCaptureReadiness(
  accessToken: string,
  tripId: string,
): Promise<TripCaptureReadinessResponse> {
  const response = await fetch(`${apiBase}/api/driver-portal/trips/${tripId}/capture-readiness`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TripCaptureReadinessResponse>(
    response,
    'Failed to load trip capture readiness',
  )
}

function captureAttachmentBasePath(
  accessToken: string,
  tripId: string,
  subjectType: 'proof' | 'dvir',
  subjectId: string,
) {
  const segment = subjectType === 'proof' ? 'proofs' : 'dvir'
  return {
    listUrl: `${apiBase}/api/driver-portal/trips/${tripId}/${segment}/${subjectId}/attachments`,
    uploadUrl: `${apiBase}/api/driver-portal/trips/${tripId}/${segment}/${subjectId}/attachments`,
    contentUrl: (attachmentId: string) =>
      `${apiBase}/api/driver-portal/trips/${tripId}/${segment}/${subjectId}/attachments/${attachmentId}/content`,
    headers: authHeaders(accessToken),
  }
}

export async function listDriverPortalCaptureAttachments(
  accessToken: string,
  tripId: string,
  subjectType: 'proof' | 'dvir',
  subjectId: string,
): Promise<TripCaptureAttachmentListResponse> {
  const paths = captureAttachmentBasePath(accessToken, tripId, subjectType, subjectId)
  const response = await fetch(paths.listUrl, { headers: paths.headers })
  return parseJsonResponse<TripCaptureAttachmentListResponse>(
    response,
    'Failed to list capture attachments',
  )
}

export async function uploadDriverPortalCaptureAttachment(
  accessToken: string,
  tripId: string,
  subjectType: 'proof' | 'dvir',
  subjectId: string,
  payload: UploadTripCaptureAttachmentRequest,
): Promise<TripCaptureAttachmentResponse> {
  const paths = captureAttachmentBasePath(accessToken, tripId, subjectType, subjectId)
  const response = await fetch(paths.uploadUrl, {
    method: 'POST',
    headers: paths.headers,
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TripCaptureAttachmentResponse>(
    response,
    'Failed to upload capture attachment',
  )
}

export function getDriverPortalCaptureAttachmentContentUrl(
  accessToken: string,
  tripId: string,
  subjectType: 'proof' | 'dvir',
  subjectId: string,
  attachmentId: string,
): string {
  return captureAttachmentBasePath(accessToken, tripId, subjectType, subjectId).contentUrl(
    attachmentId,
  )
}

export async function readFileAsDataUrl(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = () => resolve(String(reader.result ?? ''))
    reader.onerror = () => reject(reader.error ?? new Error('Failed to read file'))
    reader.readAsDataURL(file)
  })
}

export async function getTripExecutionSettings(
  accessToken: string,
): Promise<TripExecutionSettingsResponse> {
  const response = await fetch(`${apiBase}/api/trip-execution-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TripExecutionSettingsResponse>(
    response,
    'Failed to load trip execution settings',
  )
}

export async function upsertTripExecutionSettings(
  accessToken: string,
  payload: UpsertTripExecutionSettingsRequest,
): Promise<TripExecutionSettingsResponse> {
  const response = await fetch(`${apiBase}/api/trip-execution-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TripExecutionSettingsResponse>(
    response,
    'Failed to save trip execution settings',
  )
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

export async function getTripCaptureReadiness(
  accessToken: string,
  tripId: string,
): Promise<TripCaptureReadinessResponse> {
  const response = await fetch(`${apiBase}/api/trips/${tripId}/capture-readiness`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TripCaptureReadinessResponse>(
    response,
    'Failed to load trip capture readiness',
  )
}

export async function getTripAuditTrail(
  accessToken: string,
  tripId: string,
  limit = 25,
): Promise<TripAuditTrailResponse> {
  const response = await fetch(
    `${apiBase}/api/trips/${tripId}/audit-trail?limit=${encodeURIComponent(String(limit))}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<TripAuditTrailResponse>(response, 'Failed to load trip audit trail')
}

export function getTripCaptureAttachmentContentUrl(
  tripId: string,
  subjectType: 'proof' | 'dvir',
  subjectId: string,
  attachmentId: string,
): string {
  const segment = subjectType === 'proof' ? 'proofs' : 'dvir'
  return `${apiBase}/api/trips/${tripId}/${segment}/${subjectId}/attachments/${attachmentId}/content`
}

export async function downloadTripCaptureAttachment(
  accessToken: string,
  tripId: string,
  subjectType: 'proof' | 'dvir',
  subjectId: string,
  attachmentId: string,
  fileName: string,
): Promise<void> {
  const url = getTripCaptureAttachmentContentUrl(tripId, subjectType, subjectId, attachmentId)
  const response = await fetch(url, { headers: authHeaders(accessToken) })
  if (!response.ok) {
    throw new Error(`Failed to download attachment (${response.status})`)
  }
  const blob = await response.blob()
  const objectUrl = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = objectUrl
  anchor.download = fileName
  anchor.click()
  URL.revokeObjectURL(objectUrl)
}

export async function uploadTripCaptureAttachment(
  accessToken: string,
  tripId: string,
  subjectType: 'proof' | 'dvir',
  subjectId: string,
  payload: UploadTripCaptureAttachmentRequest,
): Promise<TripCaptureAttachmentResponse> {
  const segment = subjectType === 'proof' ? 'proofs' : 'dvir'
  const response = await fetch(
    `${apiBase}/api/trips/${tripId}/${segment}/${subjectId}/attachments`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(payload),
    },
  )
  return parseJsonResponse<TripCaptureAttachmentResponse>(
    response,
    'Failed to upload capture attachment',
  )
}

export async function getActiveTrips(
  accessToken: string,
  scope: 'daily' | 'weekly' = 'daily',
  options?: { attentionOnly?: boolean; statusFilter?: 'all' | 'dispatched' | 'in_progress' },
): Promise<ActiveTripsResponse> {
  const params = new URLSearchParams({ scope })
  if (options?.attentionOnly) {
    params.set('attentionOnly', 'true')
  }
  if (options?.statusFilter && options.statusFilter !== 'all') {
    params.set('statusFilter', options.statusFilter)
  }
  const response = await fetch(
    `${apiBase}/api/dispatch/active-trips?${params.toString()}`,
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
  overdueOnly = false,
): Promise<DispatchExceptionListResponse> {
  const params = new URLSearchParams({ status })
  if (overdueOnly) {
    params.set('overdueOnly', 'true')
  }
  const response = await fetch(`${apiBase}/api/dispatch/exceptions?${params.toString()}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DispatchExceptionListResponse>(response, 'Failed to load dispatch exceptions')
}

export async function listDispatchExceptionResolutionTemplates(
  accessToken: string,
): Promise<DispatchExceptionResolutionTemplateResponse[]> {
  const response = await fetch(`${apiBase}/api/dispatch/exceptions/resolution-templates`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DispatchExceptionResolutionTemplateResponse[]>(
    response,
    'Failed to load dispatch exception resolution templates',
  )
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

export async function bulkAssignDispatchExceptions(
  accessToken: string,
  payload: BulkAssignDispatchExceptionsRequest,
): Promise<BulkDispatchExceptionActionResponse> {
  const response = await fetch(`${apiBase}/api/dispatch/exceptions/bulk/assign`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<BulkDispatchExceptionActionResponse>(
    response,
    'Failed to bulk assign dispatch exceptions',
  )
}

export async function bulkResolveDispatchExceptions(
  accessToken: string,
  payload: BulkResolveDispatchExceptionsRequest,
): Promise<BulkDispatchExceptionActionResponse> {
  const response = await fetch(`${apiBase}/api/dispatch/exceptions/bulk/resolve`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<BulkDispatchExceptionActionResponse>(
    response,
    'Failed to bulk resolve dispatch exceptions',
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

export async function updateDriverAvailability(
  accessToken: string,
  availabilityId: string,
  payload: UpdateDriverAvailabilityRequest,
): Promise<unknown> {
  const response = await fetch(`${apiBase}/api/driver-availability/${availabilityId}`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse(response, 'Failed to update driver availability')
}

export async function deleteDriverAvailability(
  accessToken: string,
  availabilityId: string,
): Promise<void> {
  const response = await fetch(`${apiBase}/api/driver-availability/${availabilityId}`, {
    method: 'DELETE',
    headers: authHeaders(accessToken),
  })
  if (!response.ok) {
    await parseJsonResponse(response, 'Failed to delete driver availability')
  }
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

export async function updateEquipmentAvailability(
  accessToken: string,
  availabilityId: string,
  payload: UpdateEquipmentAvailabilityRequest,
): Promise<unknown> {
  const response = await fetch(`${apiBase}/api/equipment-availability/${availabilityId}`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse(response, 'Failed to update equipment availability')
}

export async function deleteEquipmentAvailability(
  accessToken: string,
  availabilityId: string,
): Promise<void> {
  const response = await fetch(`${apiBase}/api/equipment-availability/${availabilityId}`, {
    method: 'DELETE',
    headers: authHeaders(accessToken),
  })
  if (!response.ok) {
    await parseJsonResponse(response, 'Failed to delete equipment availability')
  }
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

export async function getIntegrationEventSettings(
  accessToken: string,
): Promise<IntegrationEventSettingsResponse> {
  const response = await fetch(`${apiBase}/api/integration-event-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<IntegrationEventSettingsResponse>(
    response,
    'Failed to load integration event settings',
  )
}

export async function upsertIntegrationEventSettings(
  accessToken: string,
  payload: UpsertIntegrationEventSettingsRequest,
): Promise<IntegrationEventSettingsResponse> {
  const response = await fetch(`${apiBase}/api/integration-event-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<IntegrationEventSettingsResponse>(
    response,
    'Failed to save integration event settings',
  )
}

export async function listIntegrationOutboxEvents(
  accessToken: string,
  limit = 20,
): Promise<IntegrationOutboxEventListResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/integration-event-settings/outbox?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<IntegrationOutboxEventListResponse>(
    response,
    'Failed to load integration outbox events',
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

export async function getTripCompletions(
  accessToken: string,
  options?: { dispatchStatus?: string },
): Promise<TripCompletionsListResponse> {
  const search = new URLSearchParams()
  if (options?.dispatchStatus) {
    search.set('dispatchStatus', options.dispatchStatus)
  }
  const suffix = search.size > 0 ? `?${search}` : ''
  const response = await fetch(`${apiBase}/api/trip-completions${suffix}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TripCompletionsListResponse>(
    response,
    'Failed to load trip completion summaries',
  )
}

export async function getTripCompletionDetail(
  accessToken: string,
  tripId: string,
): Promise<TripCompletionDetailResponse> {
  const response = await fetch(`${apiBase}/api/trip-completions/${tripId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TripCompletionDetailResponse>(
    response,
    'Failed to load trip completion detail',
  )
}

export async function getRouteCompletions(
  accessToken: string,
): Promise<RouteCompletionsListResponse> {
  const response = await fetch(`${apiBase}/api/route-completions`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RouteCompletionsListResponse>(
    response,
    'Failed to load route completion summaries',
  )
}

export async function getAttachmentRetentionSettings(
  accessToken: string,
): Promise<AttachmentRetentionSettingsResponse> {
  const response = await fetch(`${apiBase}/api/attachment-retention-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AttachmentRetentionSettingsResponse>(
    response,
    'Failed to load attachment retention settings',
  )
}

export async function upsertAttachmentRetentionSettings(
  accessToken: string,
  payload: UpsertAttachmentRetentionSettingsRequest,
): Promise<AttachmentRetentionSettingsResponse> {
  const response = await fetch(`${apiBase}/api/attachment-retention-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AttachmentRetentionSettingsResponse>(
    response,
    'Failed to save attachment retention settings',
  )
}

export async function getAttachmentRetentionRuns(
  accessToken: string,
  limit = 10,
): Promise<AttachmentRetentionRunsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/attachment-retention-settings/runs?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AttachmentRetentionRunsResponse>(
    response,
    'Failed to load attachment retention runs',
  )
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
    throw await toApiError(response, errorMessage)
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

