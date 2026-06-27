import type {
  AggregatedFieldInboxResponse,
  FieldCompanionMeResponse,
  FieldCompanionNotificationDispatchesResponse,
  FieldCompanionNotificationDispatchItem,
  FieldCompanionNotificationSettingsResponse,
  FieldCompanionOfflineActionsListResponse,
  FieldCompanionSessionResponse,
  FieldCompanionFieldEvidenceResponse,
  FieldTaskSubmissionStatusResponse,
  HandoffCreatedResponse,
  LaunchContextResponse,
  SyncFieldCompanionOfflineActionsRequest,
  SyncFieldCompanionOfflineActionsResponse,
  SubmitFieldCompanionFieldEvidenceRequest,
  SubmitFieldCompanionFieldDvirRequest,
  FieldCompanionFieldDvirResponse,
  SubmitFieldCompanionFieldInspectionAnswersRequest,
  FieldCompanionFieldInspectionAnswersResponse,
  CompleteFieldCompanionFieldInspectionRequest,
  FieldCompanionFieldInspectionCompleteResponse,
  FieldCompanionFieldInspectionDetailResponse,
  FieldCompanionFieldWorkOrderDetailResponse,
  UpdateFieldCompanionFieldWorkOrderStatusRequest,
  FieldCompanionFieldWorkOrderStatusResponse,
  LogFieldCompanionFieldWorkOrderLaborRequest,
  FieldCompanionFieldWorkOrderLaborResponse,
  FieldCompanionFieldReceivingDetailResponse,
  UpdateFieldCompanionFieldReceivingLineRequest,
  FieldCompanionFieldReceivingLineResponse,
  PostFieldCompanionFieldReceivingRequest,
  FieldCompanionFieldReceivingPostResponse,
  FieldCompanionClockStatusResponse,
  FieldCompanionClockSubmissionResponse,
  FieldCompanionScanResolveRequest,
  FieldCompanionScanResolveResponse,
  FieldCompanionPushVapidPublicKeyResponse,
  FieldCompanionPushSubscriptionResponse,
  UnsubscribeFieldCompanionPushRequest,
  UpsertFieldCompanionNotificationSettingsRequest,
  UpsertFieldCompanionPushSubscriptionRequest,
  ValidateFieldCompanionFieldTaskRequest,
  ValidateFieldCompanionFieldTaskResponse,
  SubmitFieldCompanionClockEventRequest,
} from './types'
import { clearSession, loadSession, saveSession, type StoredFieldCompanionSession } from '../auth/sessionStorage'

const apiBase = import.meta.env.VITE_NEXARR_API_BASE ?? ''
const COOKIE_SESSION_HEADER = 'X-Stl-Cookie-Session'

type CompatibilityFieldInboxProductSlice = Omit<AggregatedFieldInboxResponse['sources'][number], 'available'> & {
  available?: boolean
}

type CompatibilityAggregatedFieldInboxResponse = Omit<AggregatedFieldInboxResponse, 'sources'> & {
  sources: CompatibilityFieldInboxProductSlice[]
}

type CompatibilityFieldCompanionMePayload = Omit<FieldCompanionMeResponse, 'fieldProductKeys'> & {
  fieldProductKeys?: string[]
  launchableProductKeys?: string[]
}

type RenewAuthTokenResponse = {
  accessToken: string
  accessTokenExpiresAt: string
  refreshToken: string
  refreshTokenExpiresAt: string
  sessionId: string
  userId: string
  tenantId: string
}

function resolveCompatibilityFieldProductKeys(
  payload: { fieldProductKeys?: string[]; launchableProductKeys?: string[] },
): string[] {
  return payload.fieldProductKeys ?? payload.launchableProductKeys ?? []
}

function normalizeFieldCompanionMeResponse(
  parsed: CompatibilityFieldCompanionMePayload,
): FieldCompanionMeResponse {
  return {
    ...parsed,
    fieldProductKeys: resolveCompatibilityFieldProductKeys(parsed),
  }
}

function normalizeFieldInboxResponse(
  parsed: CompatibilityAggregatedFieldInboxResponse,
): AggregatedFieldInboxResponse {
  return {
    ...parsed,
    sources: parsed.sources.map((source) => ({
      productKey: source.productKey,
      available: source.available ?? false,
      fetched: source.fetched,
      errorCode: source.errorCode,
      errorMessage: source.errorMessage,
      items: source.items,
    })),
  }
}

export class FieldCompanionApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly body: string,
  ) {
    super(message)
    this.name = 'FieldCompanionApiError'
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
    throw new FieldCompanionApiError(body || `${fallbackMessage} (${response.status})`, response.status, body)
  }

  return (await response.json()) as T
}

export async function redeemHandoff(handoffCode: string): Promise<FieldCompanionSessionResponse> {
  const response = await fetch(`${apiBase}/api/fieldcompanion/auth/handoff/redeem`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      [COOKIE_SESSION_HEADER]: 'true',
    },
    body: JSON.stringify({ handoffCode }),
  })
  return parseJsonResponse<FieldCompanionSessionResponse>(response, 'Handoff redeem failed')
}

export async function renewFieldCompanionSession(): Promise<StoredFieldCompanionSession | null> {
  const currentSession = loadSession()
  if (!currentSession) {
    return null
  }

  const response = await fetch(`${apiBase}/api/auth/renew`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      [COOKIE_SESSION_HEADER]: 'true',
    },
  })

  if (!response.ok) {
    clearSession()
    return null
  }

  const tokens = (await response.json()) as RenewAuthTokenResponse
  saveSession({
    ...currentSession,
    accessToken: tokens.accessToken,
    accessTokenExpiresAt: tokens.accessTokenExpiresAt,
  })
  return loadSession()
}

export async function getLaunchContext(
  accessToken: string,
  productKey: string,
): Promise<LaunchContextResponse> {
  const search = new URLSearchParams({ productKey })
  const response = await fetch(`${apiBase}/api/v1/launch/context?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LaunchContextResponse>(response, 'Failed to load launch context')
}

export async function createHandoff(
  accessToken: string,
  productKey: string,
  callbackUrl: string,
): Promise<HandoffCreatedResponse> {
  const response = await fetch(`${apiBase}/api/v1/launch/handoff`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ productKey, callbackUrl }),
  })
  return parseJsonResponse<HandoffCreatedResponse>(response, 'Failed to create product handoff')
}

export async function getMe(accessToken: string): Promise<FieldCompanionMeResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/me`, {
    headers: authHeaders(accessToken),
  })
  const parsed = await parseJsonResponse<CompatibilityFieldCompanionMePayload>(response, 'Failed to load profile')
  return normalizeFieldCompanionMeResponse(parsed)
}

export async function getFieldInbox(accessToken: string): Promise<AggregatedFieldInboxResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/field-inbox`, {
    headers: authHeaders(accessToken),
  })
  const parsed = await parseJsonResponse<CompatibilityAggregatedFieldInboxResponse>(
    response,
    'Failed to load field inbox',
  )
  return normalizeFieldInboxResponse(parsed)
}

export async function getFieldCompanionNotificationSettings(
  accessToken: string,
): Promise<FieldCompanionNotificationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/notification-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FieldCompanionNotificationSettingsResponse>(
    response,
    'Failed to load notification settings',
  )
}

export async function upsertFieldCompanionNotificationSettings(
  accessToken: string,
  body: UpsertFieldCompanionNotificationSettingsRequest,
): Promise<FieldCompanionNotificationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/notification-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<FieldCompanionNotificationSettingsResponse>(
    response,
    'Failed to save notification settings',
  )
}

export async function sendFieldCompanionNotificationTest(
  accessToken: string,
): Promise<FieldCompanionNotificationDispatchItem> {
  const response = await fetch(`${apiBase}/api/v1/mobile/notification-settings/test`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FieldCompanionNotificationDispatchItem>(
    response,
    'Failed to send notification test',
  )
}

export async function getFieldCompanionPushVapidPublicKey(
  accessToken: string,
): Promise<FieldCompanionPushVapidPublicKeyResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/push/vapid-public-key`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FieldCompanionPushVapidPublicKeyResponse>(
    response,
    'Failed to load Web Push configuration',
  )
}

export async function subscribeFieldCompanionPush(
  accessToken: string,
  body: UpsertFieldCompanionPushSubscriptionRequest,
): Promise<FieldCompanionPushSubscriptionResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/push/subscribe`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<FieldCompanionPushSubscriptionResponse>(
    response,
    'Failed to register push subscription',
  )
}

export async function unsubscribeFieldCompanionPush(
  accessToken: string,
  body: UnsubscribeFieldCompanionPushRequest,
): Promise<void> {
  const response = await fetch(`${apiBase}/api/v1/mobile/push/subscribe`, {
    method: 'DELETE',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  if (!response.ok) {
    const responseBody = await response.text()
    throw new FieldCompanionApiError(
      responseBody || `Failed to remove push subscription (${response.status})`,
      response.status,
      responseBody,
    )
  }
}

export async function getFieldCompanionNotificationDispatches(
  accessToken: string,
  limit = 20,
): Promise<FieldCompanionNotificationDispatchesResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(
    `${apiBase}/api/v1/mobile/notification-settings/dispatches?${search}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<FieldCompanionNotificationDispatchesResponse>(
    response,
    'Failed to load notification dispatches',
  )
}

export async function getFieldCompanionClockStatus(
  accessToken: string,
): Promise<FieldCompanionClockStatusResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/clock`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FieldCompanionClockStatusResponse>(response, 'Failed to load clock status')
}

export async function submitFieldCompanionClockEvent(
  accessToken: string,
  body: SubmitFieldCompanionClockEventRequest,
): Promise<FieldCompanionClockSubmissionResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/clock`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<FieldCompanionClockSubmissionResponse>(response, 'Failed to submit clock event')
}

export async function syncFieldCompanionOfflineActions(
  accessToken: string,
  body: SyncFieldCompanionOfflineActionsRequest,
): Promise<SyncFieldCompanionOfflineActionsResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/offline-actions/sync`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<SyncFieldCompanionOfflineActionsResponse>(
    response,
    'Failed to sync offline actions',
  )
}

export async function listFieldCompanionOfflineActions(
  accessToken: string,
  limit = 20,
): Promise<FieldCompanionOfflineActionsListResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/mobile/offline-actions?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FieldCompanionOfflineActionsListResponse>(
    response,
    'Failed to load offline action history',
  )
}

export async function validateFieldCompanionFieldTask(
  accessToken: string,
  body: ValidateFieldCompanionFieldTaskRequest,
): Promise<ValidateFieldCompanionFieldTaskResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/field-tasks/validate`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<ValidateFieldCompanionFieldTaskResponse>(
    response,
    'Failed to validate field task',
  )
}

export async function getFieldTaskSubmissionStatus(
  accessToken: string,
  taskKeys: string[],
): Promise<FieldTaskSubmissionStatusResponse> {
  const search = new URLSearchParams({
    taskKeys: taskKeys.slice(0, 50).join(','),
  })
  const response = await fetch(
    `${apiBase}/api/v1/mobile/field-tasks/submission-status?${search}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<FieldTaskSubmissionStatusResponse>(
    response,
    'Failed to load field task submission status',
  )
}

export async function resolveFieldCompanionScan(
  accessToken: string,
  body: FieldCompanionScanResolveRequest,
): Promise<FieldCompanionScanResolveResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/scan/resolve`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<FieldCompanionScanResolveResponse>(response, 'Failed to resolve scan')
}

export async function submitFieldCompanionFieldEvidence(
  accessToken: string,
  body: SubmitFieldCompanionFieldEvidenceRequest,
): Promise<FieldCompanionFieldEvidenceResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/field-tasks/evidence`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<FieldCompanionFieldEvidenceResponse>(
    response,
    'Failed to upload field task evidence',
  )
}

export async function submitFieldCompanionFieldDvir(
  accessToken: string,
  body: SubmitFieldCompanionFieldDvirRequest,
): Promise<FieldCompanionFieldDvirResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/field-tasks/dvir`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<FieldCompanionFieldDvirResponse>(
    response,
    'Failed to submit field task DVIR',
  )
}

export async function getFieldCompanionFieldInspectionDetail(
  accessToken: string,
  taskKey: string,
): Promise<FieldCompanionFieldInspectionDetailResponse> {
  const search = new URLSearchParams({ taskKey })
  const response = await fetch(`${apiBase}/api/v1/mobile/field-tasks/inspection?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FieldCompanionFieldInspectionDetailResponse>(
    response,
    'Failed to load field inspection detail',
  )
}

export async function submitFieldCompanionFieldInspectionAnswers(
  accessToken: string,
  body: SubmitFieldCompanionFieldInspectionAnswersRequest,
): Promise<FieldCompanionFieldInspectionAnswersResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/field-tasks/inspection/answers`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<FieldCompanionFieldInspectionAnswersResponse>(
    response,
    'Failed to submit field inspection answers',
  )
}

export async function completeFieldCompanionFieldInspection(
  accessToken: string,
  body: CompleteFieldCompanionFieldInspectionRequest,
): Promise<FieldCompanionFieldInspectionCompleteResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/field-tasks/inspection/complete`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<FieldCompanionFieldInspectionCompleteResponse>(
    response,
    'Failed to complete field inspection',
  )
}

export async function getFieldCompanionFieldWorkOrderDetail(
  accessToken: string,
  taskKey: string,
): Promise<FieldCompanionFieldWorkOrderDetailResponse> {
  const search = new URLSearchParams({ taskKey })
  const response = await fetch(`${apiBase}/api/v1/mobile/field-tasks/work-order?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FieldCompanionFieldWorkOrderDetailResponse>(
    response,
    'Failed to load field work order detail',
  )
}

export async function updateFieldCompanionFieldWorkOrderStatus(
  accessToken: string,
  body: UpdateFieldCompanionFieldWorkOrderStatusRequest,
): Promise<FieldCompanionFieldWorkOrderStatusResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/field-tasks/work-order/status`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<FieldCompanionFieldWorkOrderStatusResponse>(
    response,
    'Failed to update field work order status',
  )
}

export async function logFieldCompanionFieldWorkOrderLabor(
  accessToken: string,
  body: LogFieldCompanionFieldWorkOrderLaborRequest,
): Promise<FieldCompanionFieldWorkOrderLaborResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/field-tasks/work-order/labor`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<FieldCompanionFieldWorkOrderLaborResponse>(
    response,
    'Failed to log field work order labor',
  )
}

export async function getFieldCompanionFieldReceivingDetail(
  accessToken: string,
  taskKey: string,
): Promise<FieldCompanionFieldReceivingDetailResponse> {
  const search = new URLSearchParams({ taskKey })
  const response = await fetch(`${apiBase}/api/v1/mobile/field-tasks/receiving?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FieldCompanionFieldReceivingDetailResponse>(
    response,
    'Failed to load field receiving detail',
  )
}

export async function updateFieldCompanionFieldReceivingLine(
  accessToken: string,
  body: UpdateFieldCompanionFieldReceivingLineRequest,
): Promise<FieldCompanionFieldReceivingLineResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/field-tasks/receiving/line`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<FieldCompanionFieldReceivingLineResponse>(
    response,
    'Failed to update field receiving line',
  )
}

export async function postFieldCompanionFieldReceiving(
  accessToken: string,
  body: PostFieldCompanionFieldReceivingRequest,
): Promise<FieldCompanionFieldReceivingPostResponse> {
  const response = await fetch(`${apiBase}/api/v1/mobile/field-tasks/receiving/post`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<FieldCompanionFieldReceivingPostResponse>(
    response,
    'Failed to post field receiving receipt',
  )
}

export function productLaunchUrl(productKey: string, deepLinkPath: string): string | null {
  const envKey = `VITE_${productKey.toUpperCase()}_FRONTEND_BASE`
  const base = (import.meta.env[envKey] as string | undefined)?.trim()
  if (!base) {
    return null
  }

  const normalizedPath = deepLinkPath.startsWith('/') ? deepLinkPath : `/${deepLinkPath}`
  return `${base.replace(/\/$/, '')}${normalizedPath}`
}
