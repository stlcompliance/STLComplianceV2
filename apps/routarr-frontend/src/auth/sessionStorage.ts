import type { HandoffSessionResponse } from '../api/types'

const STORAGE_KEY = 'stl.routarr.session'

export interface StoredRoutArrSession {
  accessToken: string
  accessTokenExpiresAt: string
  userId: string
  personId: string
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  displayName: string
  email: string
}

export function toStoredSession(session: HandoffSessionResponse): StoredRoutArrSession {
  return {
    accessToken: session.accessToken,
    accessTokenExpiresAt: session.accessTokenExpiresAt,
    userId: session.userId,
    personId: session.personId,
    tenantId: session.tenantId,
    tenantSlug: session.tenantSlug,
    tenantDisplayName: session.tenantDisplayName,
    displayName: session.displayName,
    email: session.email,
  }
}

export function loadSession(): StoredRoutArrSession | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }
  try {
    return JSON.parse(raw) as StoredRoutArrSession
  } catch {
    sessionStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function saveSession(session: StoredRoutArrSession): void {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session))
}

export function clearSession(): void {
  sessionStorage.removeItem(STORAGE_KEY)
}

export function canManageNotificationSettings(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'routarr_admin'].includes(tenantRoleKey.toLowerCase())
}

export function canCreateTrips(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'routarr_admin', 'routarr_manager', 'routarr_dispatcher'].includes(
    tenantRoleKey.toLowerCase(),
  )
}

export function canAssignDrivers(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canCreateTrips(tenantRoleKey, isPlatformAdmin)
}

export function canManageTrips(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'routarr_admin', 'routarr_manager'].includes(tenantRoleKey.toLowerCase())
}

export function canPerformTrips(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return [
    'tenant_admin',
    'routarr_admin',
    'routarr_manager',
    'routarr_dispatcher',
    'routarr_driver',
  ].includes(tenantRoleKey.toLowerCase())
}

export function canViewAllTrips(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'routarr_admin', 'routarr_manager', 'routarr_dispatcher'].includes(
    tenantRoleKey.toLowerCase(),
  )
}

export function canManageDriverAvailability(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canAssignDrivers(tenantRoleKey, isPlatformAdmin) || canPerformTrips(tenantRoleKey, isPlatformAdmin)
}

export function canManageEquipmentAvailability(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canAssignDrivers(tenantRoleKey, isPlatformAdmin)
}

export function canReadTripVisibility(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canAssignDrivers(tenantRoleKey, isPlatformAdmin)
}
