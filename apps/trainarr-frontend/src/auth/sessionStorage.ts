import type { HandoffSessionResponse } from '../api/types'

const STORAGE_KEY = 'stl.trainarr.session'

export interface StoredTrainArrSession {
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

export function toStoredSession(session: HandoffSessionResponse): StoredTrainArrSession {
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

export function loadSession(): StoredTrainArrSession | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }
  try {
    return JSON.parse(raw) as StoredTrainArrSession
  } catch {
    sessionStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function saveSession(session: StoredTrainArrSession): void {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session))
}

export function clearSession(): void {
  sessionStorage.removeItem(STORAGE_KEY)
}

export function canManageAssignments(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'trainarr_admin'].includes(tenantRoleKey.toLowerCase())
}

export function canRunBatchQualificationChecks(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'trainarr_admin', 'trainarr_trainer'].includes(tenantRoleKey.toLowerCase())
}

export function canManageQualifications(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canManageAssignments(tenantRoleKey, isPlatformAdmin)
}

export function canManageNotificationSettings(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canManageAssignments(tenantRoleKey, isPlatformAdmin)
}

export function canManagePrograms(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canManageAssignments(tenantRoleKey, isPlatformAdmin)
}

export function canAssessRulePackImpact(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canManagePrograms(tenantRoleKey, isPlatformAdmin)
}

export function canUploadEvidence(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
  assignmentPersonId: string,
  viewerPersonId: string,
): boolean {
  return canCompleteAssignment(tenantRoleKey, isPlatformAdmin, assignmentPersonId, viewerPersonId)
}

export function canCompleteAssignment(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
  assignmentPersonId: string,
  viewerPersonId: string,
): boolean {
  if (isPlatformAdmin) return true
  const role = tenantRoleKey.toLowerCase()
  if (['tenant_admin', 'trainarr_admin', 'trainarr_trainer'].includes(role)) return true
  return role === 'tenant_member' && assignmentPersonId === viewerPersonId
}

export function canSubmitEvaluation(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'trainarr_admin', 'trainarr_trainer'].includes(tenantRoleKey.toLowerCase())
}

export function canSubmitTraineeSignoff(
  tenantRoleKey: string,
  isPlatformAdmin: boolean,
  assignmentPersonId: string,
  viewerPersonId: string,
): boolean {
  if (isPlatformAdmin) return true
  return tenantRoleKey.toLowerCase() === 'tenant_member' && assignmentPersonId === viewerPersonId
}

export function canSubmitTrainerSignoff(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  return canSubmitEvaluation(tenantRoleKey, isPlatformAdmin)
}
