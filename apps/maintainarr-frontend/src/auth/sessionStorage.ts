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
  isPlatformAdmin: boolean
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
    isPlatformAdmin: session.isPlatformAdmin,
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

export function canCreateDefects(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canReadMaintenanceReports(tenantRoleKey, isPlatformAdmin)
}

export function canSubmitDefects(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canCreateDefects(tenantRoleKey, isPlatformAdmin)
}

export function canManageDefectReadiness(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canManageAssets(tenantRoleKey, isPlatformAdmin)
}

export function canManageInspectionTemplates(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  return canManageAssets(tenantRoleKey, isPlatformAdmin)
}

export function canPreviewInspectionTemplates(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  return canManageInspectionTemplates(tenantRoleKey, isPlatformAdmin)
}

export function canCreateInspectionTemplates(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  return canManageInspectionTemplates(tenantRoleKey, isPlatformAdmin)
}

export function canPublishInspectionTemplates(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  return canManageInspectionTemplates(tenantRoleKey, isPlatformAdmin)
}

export function canRetireInspectionTemplates(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  return canManageInspectionTemplates(tenantRoleKey, isPlatformAdmin)
}

function hasPartsManageRole(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'maintainarr_admin', 'maintainarr_manager'].includes(
    tenantRoleKey.toLowerCase(),
  )
}

export function canReadParts(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canReadMaintenanceReports(tenantRoleKey, isPlatformAdmin)
}

export function canCreateParts(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return hasPartsManageRole(tenantRoleKey, isPlatformAdmin)
}

export function canUpdateParts(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return hasPartsManageRole(tenantRoleKey, isPlatformAdmin)
}

export function canArchiveParts(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return hasPartsManageRole(tenantRoleKey, isPlatformAdmin)
}

function hasPartsKitManageRole(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'maintainarr_admin', 'maintainarr_manager'].includes(
    tenantRoleKey.toLowerCase(),
  )
}

export function canReadPartsKits(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canReadMaintenanceReports(tenantRoleKey, isPlatformAdmin)
}

export function canPreviewPartsKits(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canReadPartsKits(tenantRoleKey, isPlatformAdmin)
}

export function canValidatePartsKits(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canPreviewPartsKits(tenantRoleKey, isPlatformAdmin)
}

export function canCreatePartsKits(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return hasPartsKitManageRole(tenantRoleKey, isPlatformAdmin)
}

export function canUpdatePartsKits(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return hasPartsKitManageRole(tenantRoleKey, isPlatformAdmin)
}

export function canClonePartsKits(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return hasPartsKitManageRole(tenantRoleKey, isPlatformAdmin)
}

export function canSubmitPartsKitsForApproval(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  return hasPartsKitManageRole(tenantRoleKey, isPlatformAdmin)
}

export function canActivatePartsKits(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return hasPartsKitManageRole(tenantRoleKey, isPlatformAdmin)
}

export function canRetirePartsKits(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return hasPartsKitManageRole(tenantRoleKey, isPlatformAdmin)
}

export function canCreateWorkOrderFromDefect(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  return canCreateWorkOrders(tenantRoleKey, isPlatformAdmin)
}

export function canViewAllDefects(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canViewAllInspectionRuns(tenantRoleKey, isPlatformAdmin)
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

function hasPmProgramManageRole(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'maintainarr_admin', 'maintainarr_manager'].includes(
    tenantRoleKey.toLowerCase(),
  )
}

function hasPmProgramActivationRole(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'maintainarr_admin'].includes(tenantRoleKey.toLowerCase())
}

export function canReadPmPrograms(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return hasPmProgramManageRole(tenantRoleKey, isPlatformAdmin)
}

export function canPreviewPmPrograms(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return hasPmProgramManageRole(tenantRoleKey, isPlatformAdmin)
}

export function canCreatePmPrograms(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return hasPmProgramManageRole(tenantRoleKey, isPlatformAdmin)
}

export function canUpdatePmPrograms(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return hasPmProgramManageRole(tenantRoleKey, isPlatformAdmin)
}

export function canActivatePmPrograms(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return hasPmProgramActivationRole(tenantRoleKey, isPlatformAdmin)
}

export function canPauseRetirePmPrograms(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return hasPmProgramActivationRole(tenantRoleKey, isPlatformAdmin)
}

export function canManagePmProgramAutomation(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
): boolean {
  return hasPmProgramActivationRole(tenantRoleKey, isPlatformAdmin)
}
