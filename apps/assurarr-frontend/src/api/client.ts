export interface AssurArrSessionBootstrapResponse {
  userId: string
  personId: string
  tenantId: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  launchableProductKeys: string[]
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
  launchableProductKeys: string[]
  themePreference?: string | null
  callbackUrl: string | null
}

type LegacyAssurArrSessionBootstrapPayload = AssurArrSessionBootstrapResponse & {
  hasAssurArrAccess?: boolean
}

type LegacyAssurArrHandoffSessionPayload = AssurArrHandoffSessionResponse & {
  launchableProductKeys?: string[]
}

function resolveLegacyLaunchableProductKeys(
  payload: { launchableProductKeys?: string[] },
): string[] {
  return payload.launchableProductKeys ?? []
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

function normalizeAssurArrSessionBootstrapResponse(
  response: LegacyAssurArrSessionBootstrapPayload,
): AssurArrSessionBootstrapResponse {
  const { hasAssurArrAccess: _legacyHasAssurArrAccess, ...session } = response
  return {
    ...session,
    launchableProductKeys: resolveLegacyLaunchableProductKeys(response),
  }
}

function normalizeAssurArrHandoffSessionResponse(
  response: LegacyAssurArrHandoffSessionPayload,
): AssurArrHandoffSessionResponse {
  return {
    ...response,
    launchableProductKeys: resolveLegacyLaunchableProductKeys(response),
  }
}

export async function getSessionBootstrap(
  accessToken: string,
): Promise<AssurArrSessionBootstrapResponse> {
  const response = await fetch(`${apiBase}/api/session`, {
    headers: authHeaders(accessToken),
  })
  const payload = await parseJsonResponse<LegacyAssurArrSessionBootstrapPayload>(
    response,
    'Failed to load session bootstrap',
  )
  return normalizeAssurArrSessionBootstrapResponse(payload)
}

export async function redeemHandoff(
  handoffCode: string,
): Promise<AssurArrHandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/auth/nexarr/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  const payload = await parseJsonResponse<LegacyAssurArrHandoffSessionPayload>(
    response,
    'Handoff redeem failed',
  )
  return normalizeAssurArrHandoffSessionResponse(payload)
}
