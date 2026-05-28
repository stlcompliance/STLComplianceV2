import type {
  AggregatedFieldInboxResponse,
  CompanionMeResponse,
  CompanionNotificationDispatchesResponse,
  CompanionNotificationSettingsResponse,
  CompanionOfflineActionsListResponse,
  CompanionSessionResponse,
  CompanionFieldEvidenceResponse,
  FieldTaskSubmissionStatusResponse,
  HandoffCreatedResponse,
  LaunchContextResponse,
  SyncCompanionOfflineActionsRequest,
  SyncCompanionOfflineActionsResponse,
  SubmitCompanionFieldEvidenceRequest,
  CompanionScanResolveRequest,
  CompanionScanResolveResponse,
  UpsertCompanionNotificationSettingsRequest,
} from './types'

const apiBase = import.meta.env.VITE_NEXARR_API_BASE ?? ''

export class CompanionApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly body: string,
  ) {
    super(message)
    this.name = 'CompanionApiError'
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
    throw new CompanionApiError(body || `${fallbackMessage} (${response.status})`, response.status, body)
  }

  return (await response.json()) as T
}

export async function redeemHandoff(handoffCode: string): Promise<CompanionSessionResponse> {
  const response = await fetch(`${apiBase}/api/companion/auth/handoff/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  return parseJsonResponse<CompanionSessionResponse>(response, 'Handoff redeem failed')
}

export async function getLaunchContext(
  accessToken: string,
  productKey: string,
): Promise<LaunchContextResponse> {
  const search = new URLSearchParams({ productKey })
  const response = await fetch(`${apiBase}/api/launch/context?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LaunchContextResponse>(response, 'Failed to load launch context')
}

export async function createHandoff(
  accessToken: string,
  productKey: string,
  callbackUrl: string,
): Promise<HandoffCreatedResponse> {
  const response = await fetch(`${apiBase}/api/launch/handoff`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ productKey, callbackUrl }),
  })
  return parseJsonResponse<HandoffCreatedResponse>(response, 'Failed to create product handoff')
}

export async function getMe(accessToken: string): Promise<CompanionMeResponse> {
  const response = await fetch(`${apiBase}/api/companion/me`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CompanionMeResponse>(response, 'Failed to load profile')
}

export async function getFieldInbox(accessToken: string): Promise<AggregatedFieldInboxResponse> {
  const response = await fetch(`${apiBase}/api/companion/field-inbox`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AggregatedFieldInboxResponse>(response, 'Failed to load field inbox')
}

export async function getCompanionNotificationSettings(
  accessToken: string,
): Promise<CompanionNotificationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/companion/notification-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CompanionNotificationSettingsResponse>(
    response,
    'Failed to load notification settings',
  )
}

export async function upsertCompanionNotificationSettings(
  accessToken: string,
  body: UpsertCompanionNotificationSettingsRequest,
): Promise<CompanionNotificationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/companion/notification-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<CompanionNotificationSettingsResponse>(
    response,
    'Failed to save notification settings',
  )
}

export async function getCompanionNotificationDispatches(
  accessToken: string,
  limit = 20,
): Promise<CompanionNotificationDispatchesResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(
    `${apiBase}/api/companion/notification-settings/dispatches?${search}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<CompanionNotificationDispatchesResponse>(
    response,
    'Failed to load notification dispatches',
  )
}

export async function syncCompanionOfflineActions(
  accessToken: string,
  body: SyncCompanionOfflineActionsRequest,
): Promise<SyncCompanionOfflineActionsResponse> {
  const response = await fetch(`${apiBase}/api/companion/offline-actions/sync`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<SyncCompanionOfflineActionsResponse>(
    response,
    'Failed to sync offline actions',
  )
}

export async function listCompanionOfflineActions(
  accessToken: string,
  limit = 20,
): Promise<CompanionOfflineActionsListResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/companion/offline-actions?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CompanionOfflineActionsListResponse>(
    response,
    'Failed to load offline action history',
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
    `${apiBase}/api/companion/field-tasks/submission-status?${search}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<FieldTaskSubmissionStatusResponse>(
    response,
    'Failed to load field task submission status',
  )
}

export async function resolveCompanionScan(
  accessToken: string,
  body: CompanionScanResolveRequest,
): Promise<CompanionScanResolveResponse> {
  const response = await fetch(`${apiBase}/api/companion/scan/resolve`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<CompanionScanResolveResponse>(response, 'Failed to resolve scan')
}

export async function submitCompanionFieldEvidence(
  accessToken: string,
  body: SubmitCompanionFieldEvidenceRequest,
): Promise<CompanionFieldEvidenceResponse> {
  const response = await fetch(`${apiBase}/api/companion/field-tasks/evidence`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<CompanionFieldEvidenceResponse>(
    response,
    'Failed to upload field task evidence',
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
