import type { HandoffSessionResponse } from '../api/types'

const STORAGE_KEY = 'stl.maintainarr.session'

export interface StoredMaintainArrSession {
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

export function toStoredSession(session: HandoffSessionResponse): StoredMaintainArrSession {
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

export function loadSession(): StoredMaintainArrSession | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }
  try {
    return JSON.parse(raw) as StoredMaintainArrSession
  } catch {
    sessionStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function saveSession(session: StoredMaintainArrSession): void {
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
  return ['tenant_admin', 'maintainarr_admin'].includes(tenantRoleKey.toLowerCase())
}

export function canReadMaintenanceReports(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return [
    'tenant_admin',
    'maintainarr_admin',
    'maintainarr_manager',
    'maintainarr_technician',
    'tenant_member',
  ].includes(tenantRoleKey.toLowerCase())
}

export function canExportMaintenanceReports(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  return canExportAuditPackage(tenantRoleKey, isPlatformAdmin)
}

export function canReadExecutiveReports(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'maintainarr_admin', 'maintainarr_manager'].includes(
    tenantRoleKey.toLowerCase(),
  )
}

export function canExportExecutiveReports(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  return canExportAuditPackage(tenantRoleKey, isPlatformAdmin)
}

export function canReadComplianceReports(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canReadExecutiveReports(tenantRoleKey, isPlatformAdmin)
}

export function canExportComplianceReports(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  return canExportAuditPackage(tenantRoleKey, isPlatformAdmin)
}

export function canExportAuditPackage(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'maintainarr_admin', 'maintainarr_manager'].includes(
    tenantRoleKey.toLowerCase(),
  )
}

export function canManageAssets(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'maintainarr_admin', 'maintainarr_manager'].includes(tenantRoleKey.toLowerCase())
}

export function canViewAllInspectionRuns(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canManageAssets(tenantRoleKey, isPlatformAdmin)
}

export function canManageDefectStatus(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canManageAssets(tenantRoleKey, isPlatformAdmin)
}

export function canCreateWorkOrders(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return [
    'tenant_admin',
    'maintainarr_admin',
    'maintainarr_manager',
    'maintainarr_technician',
    'tenant_member',
  ].includes(tenantRoleKey.toLowerCase())
}

export function canCloseWorkOrders(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canManageAssets(tenantRoleKey, isPlatformAdmin)
}
