import type {
  AggregatedFieldInboxResponse,
  CompanionMeResponse,
  CompanionSessionResponse,
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

export function productLaunchUrl(productKey: string, deepLinkPath: string): string | null {
  const envKey = `VITE_${productKey.toUpperCase()}_FRONTEND_BASE`
  const base = (import.meta.env[envKey] as string | undefined)?.trim()
  if (!base) {
    return null
  }

  const normalizedPath = deepLinkPath.startsWith('/') ? deepLinkPath : `/${deepLinkPath}`
  return `${base.replace(/\/$/, '')}${normalizedPath}`
}
