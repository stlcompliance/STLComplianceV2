import type { HandoffSessionResponse, StaffArrMeResponse } from './types'

const apiBase = import.meta.env.VITE_STAFFARR_API_BASE ?? ''

export class StaffArrApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly body: string,
  ) {
    super(message)
    this.name = 'StaffArrApiError'
  }
}

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

export async function redeemHandoff(handoffCode: string): Promise<HandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/auth/handoff/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  if (!response.ok) {
    const body = await response.text()
    throw new StaffArrApiError(
      body || `Handoff redeem failed (${response.status})`,
      response.status,
      body,
    )
  }
  return (await response.json()) as HandoffSessionResponse
}

export async function getMe(accessToken: string): Promise<StaffArrMeResponse> {
  const response = await fetch(`${apiBase}/api/me`, {
    headers: authHeaders(accessToken),
  })
  if (!response.ok) {
    const body = await response.text()
    throw new StaffArrApiError(
      body || `Failed to load profile (${response.status})`,
      response.status,
      body,
    )
  }
  return (await response.json()) as StaffArrMeResponse
}
