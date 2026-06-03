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

const apiBase = import.meta.env.VITE_LOADARR_API_BASE ?? ''

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new Error(body || `${fallbackMessage} (${response.status})`)
  }

  return (await response.json()) as T
}

export async function getSessionBootstrap(
  accessToken: string,
): Promise<LoadArrSessionBootstrapResponse> {
  const response = await fetch(`${apiBase}/api/session`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LoadArrSessionBootstrapResponse>(
    response,
    'Failed to load session bootstrap',
  )
}
