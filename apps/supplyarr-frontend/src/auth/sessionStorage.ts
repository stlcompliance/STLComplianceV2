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
  isPlatformAdmin: boolean
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
    isPlatformAdmin: session.isPlatformAdmin,
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

export function canReadSupplierOrders(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canReadProcurementRecords(tenantRoleKey, isPlatformAdmin)
}

export function canCreateSupplierOrders(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canCreatePurchaseRequests(tenantRoleKey, isPlatformAdmin)
}

export function canUpdateSupplierOrders(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canCreateSupplierOrders(tenantRoleKey, isPlatformAdmin)
}

export function canManageSupplierOrderSettings(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  return canManageNotificationSettings(tenantRoleKey, isPlatformAdmin)
}

export const canReadVendorOrders = canReadSupplierOrders
export const canCreateVendorOrders = canCreateSupplierOrders
export const canUpdateVendorOrders = canUpdateSupplierOrders
export const canManageVendorOrderSettings = canManageSupplierOrderSettings

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

export function canReadSuppliers(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  if (isPlatformAdmin) return true
  return supplyarrReadRoles.includes(tenantRoleKey.toLowerCase())
}

export const canReadParties = canReadSuppliers

export function canReadPartSubstitutions(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  return canReadSuppliers(tenantRoleKey, isPlatformAdmin)
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
  return canReadSuppliers(tenantRoleKey, isPlatformAdmin)
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

export function canReadSupplierReports(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return supplyarrReadRoles.includes(tenantRoleKey.toLowerCase())
}

export function canExportSupplierReports(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return supplyarrProcurementReadRoles.includes(tenantRoleKey.toLowerCase())
}

export const canReadVendorReports = canReadSupplierReports

export const canExportVendorReports = canExportSupplierReports

export function canReadPurchasingReports(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return supplyarrProcurementReadRoles.includes(tenantRoleKey.toLowerCase())
}

export function canExportPurchasingReports(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canReadPurchasingReports(tenantRoleKey, isPlatformAdmin)
}

export function canReadComplianceReports(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canReadSupplierReports(tenantRoleKey, isPlatformAdmin)
}

export function canExportComplianceReports(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  return canReadComplianceReports(tenantRoleKey, isPlatformAdmin)
}

export function canReadPartsInventoryReports(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  return canReadSuppliers(tenantRoleKey, isPlatformAdmin)
}

export function canExportPartsInventoryReports(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  return canManageParts(tenantRoleKey, isPlatformAdmin)
}
