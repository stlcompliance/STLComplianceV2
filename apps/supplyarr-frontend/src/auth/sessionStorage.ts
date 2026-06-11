import type { HandoffSessionResponse } from '../api/types'

const STORAGE_KEY = 'stl.supplyarr.session'

export interface StoredSupplyArrSession {
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

export function toStoredSession(session: HandoffSessionResponse): StoredSupplyArrSession {
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

export function loadSession(): StoredSupplyArrSession | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }
  try {
    return JSON.parse(raw) as StoredSupplyArrSession
  } catch {
    sessionStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function saveSession(session: StoredSupplyArrSession): void {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session))
}

export function clearSession(): void {
  sessionStorage.removeItem(STORAGE_KEY)
}

export function canManageParties(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'supplyarr_admin', 'supplyarr_manager'].includes(tenantRoleKey.toLowerCase())
}

export function canManageParts(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canManageParties(tenantRoleKey, isPlatformAdmin)
}

export function canManageInventory(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canManageParties(tenantRoleKey, isPlatformAdmin)
}

export function canCreatePurchaseRequests(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'supplyarr_admin', 'supplyarr_manager', 'supplyarr_buyer'].includes(
    tenantRoleKey.toLowerCase(),
  )
}

export function canApprovePurchaseRequests(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canManageParties(tenantRoleKey, isPlatformAdmin)
}

export function canCreateEmergencyPurchase(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canManageParties(tenantRoleKey, isPlatformAdmin)
}

export function canManagerOverrideEmergencyPurchase(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'supplyarr_admin'].includes(tenantRoleKey.toLowerCase())
}

export function canCreatePurchaseOrders(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canCreatePurchaseRequests(tenantRoleKey, isPlatformAdmin)
}

export function canApprovePurchaseOrders(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canApprovePurchaseRequests(tenantRoleKey, isPlatformAdmin)
}

export function canPerformReceiving(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'supplyarr_admin', 'supplyarr_manager', 'supplyarr_clerk'].includes(
    tenantRoleKey.toLowerCase(),
  )
}

export function canManageNotificationSettings(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'supplyarr_admin'].includes(tenantRoleKey.toLowerCase())
}

const supplyarrReadRoles = [
  'tenant_admin',
  'supplyarr_admin',
  'supplyarr_manager',
  'supplyarr_clerk',
  'tenant_member',
]

const supplyarrProcurementReadRoles = [
  ...supplyarrReadRoles,
  'supplyarr_buyer',
]

export function canReadParties(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  if (isPlatformAdmin) return true
  return supplyarrReadRoles.includes(tenantRoleKey.toLowerCase())
}

export function canReadPartSubstitutions(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  return canReadParties(tenantRoleKey, isPlatformAdmin)
}

export function canReadProcurementRecords(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  if (isPlatformAdmin) return true
  return supplyarrProcurementReadRoles.includes(tenantRoleKey.toLowerCase())
}

export function canUseForgivingSearch(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  return canReadParties(tenantRoleKey, isPlatformAdmin)
}

export function canReadAuditHistory(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'supplyarr_admin', 'supplyarr_manager'].includes(
    tenantRoleKey.toLowerCase(),
  )
}

export function canReadSupplyReadiness(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return [
    ...supplyarrReadRoles,
    'supplyarr_buyer',
  ].includes(tenantRoleKey.toLowerCase())
}
