import type { HandoffSessionResponse } from '../api/types'

const STORAGE_KEY = 'stl.staffarr.session'

export interface StoredStaffArrSession {
  accessToken: string
  accessTokenExpiresAt: string
  userId: string
  personId: string
  tenantId: string
  tenantSlug: string
  displayName: string
  email: string
}

export function toStoredSession(session: HandoffSessionResponse): StoredStaffArrSession {
  return {
    accessToken: session.accessToken,
    accessTokenExpiresAt: session.accessTokenExpiresAt,
    userId: session.userId,
    personId: session.personId,
    tenantId: session.tenantId,
    tenantSlug: session.tenantSlug,
    displayName: session.displayName,
    email: session.email,
  }
}

export function loadSession(): StoredStaffArrSession | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }
  try {
    return JSON.parse(raw) as StoredStaffArrSession
  } catch {
    sessionStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function saveSession(session: StoredStaffArrSession): void {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session))
}

export function clearSession(): void {
  sessionStorage.removeItem(STORAGE_KEY)
}

export function hasStaffArrEntitlement(entitlements: string[]): boolean {
  return entitlements.some((e) => e.toLowerCase() === 'staffarr')
}
