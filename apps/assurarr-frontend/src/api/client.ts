export interface AssurArrSessionBootstrapResponse {
  userId: string
  personId: string
  tenantId: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasAssurArrEntitlement: boolean
  entitlements: string[]
}

export interface AssurArrHandoffSessionResponse {
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

const apiBase = import.meta.env.VITE_ASSURARR_API_BASE ?? ''

export class AssurArrApiError extends Error {
  constructor(message: string, public readonly status: number) {
    super(message)
    this.name = 'AssurArrApiError'
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
    throw new AssurArrApiError(body || `${fallbackMessage} (${response.status})`, response.status)
  }

  return (await response.json()) as T
}

export async function getSessionBootstrap(
  accessToken: string,
): Promise<AssurArrSessionBootstrapResponse> {
  const response = await fetch(`${apiBase}/api/session`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssurArrSessionBootstrapResponse>(
    response,
    'Failed to load session bootstrap',
  )
}

export async function redeemHandoff(
  handoffCode: string,
): Promise<AssurArrHandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/auth/nexarr/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  return parseJsonResponse<AssurArrHandoffSessionResponse>(response, 'Handoff redeem failed')
}
