export interface LoadArrSessionBootstrapResponse {
  userId: string
  personId: string
  tenantId: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasLoadArrEntitlement: boolean
  entitlements: string[]
}

export interface LoadArrHandoffSessionResponse {
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

const apiBase = import.meta.env.VITE_LOADARR_API_BASE ?? ''

export class LoadArrApiError extends Error {
  readonly status: number
  readonly body: string

  constructor(message: string, status: number, body: string) {
    super(message)
    this.name = 'LoadArrApiError'
    this.status = status
    this.body = body
  }
}

function authHeaders(accessToken: string): Headers {
  return createLoadArrHeaders(accessToken, {
    'Content-Type': 'application/json',
  })
}

export function resolveLoadArrApiUrl(path: string): string {
  return `${apiBase}${path}`
}

export function createLoadArrHeaders(accessToken?: string, headers?: HeadersInit): Headers {
  const requestHeaders = new Headers(headers)
  if (accessToken) {
    requestHeaders.set('Authorization', `Bearer ${accessToken}`)
  }

  return requestHeaders
}

export function loadArrFetch(
  path: string,
  accessToken?: string,
  init: RequestInit = {},
): Promise<Response> {
  return fetch(resolveLoadArrApiUrl(path), {
    ...init,
    headers: createLoadArrHeaders(accessToken, init.headers),
  })
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new LoadArrApiError(
      body || `${fallbackMessage} (${response.status})`,
      response.status,
      body,
    )
  }

  return (await response.json()) as T
}

export async function getSessionBootstrap(
  accessToken: string,
): Promise<LoadArrSessionBootstrapResponse> {
  const response = await loadArrFetch('/api/session', accessToken, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LoadArrSessionBootstrapResponse>(
    response,
    'Failed to load session bootstrap',
  )
}

export async function redeemHandoff(handoffCode: string): Promise<LoadArrHandoffSessionResponse> {
  const response = await loadArrFetch('/api/auth/nexarr/redeem', undefined, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  return parseJsonResponse<LoadArrHandoffSessionResponse>(response, 'Handoff redeem failed')
}
