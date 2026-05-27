import type { HandoffSessionResponse } from '../api/types'

const STORAGE_KEY = 'stl.compliancecore.session'

export interface StoredComplianceCoreSession {
  accessToken: string
  accessTokenExpiresAt: string
  userId: string
  personId: string
  tenantId: string
  tenantSlug: string
  displayName: string
  email: string
}

export function toStoredSession(session: HandoffSessionResponse): StoredComplianceCoreSession {
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

export function loadSession(): StoredComplianceCoreSession | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  if (!raw) {
    return null
  }
  try {
    return JSON.parse(raw) as StoredComplianceCoreSession
  } catch {
    sessionStorage.removeItem(STORAGE_KEY)
    return null
  }
}

export function saveSession(session: StoredComplianceCoreSession): void {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session))
}

export function clearSession(): void {
  sessionStorage.removeItem(STORAGE_KEY)
}

export function canManageVocabulary(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'compliance_admin'].includes(tenantRoleKey.toLowerCase())
}

export function canExportAuditPackage(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) return true
  return ['tenant_admin', 'compliance_admin', 'compliance_reviewer'].includes(
    tenantRoleKey.toLowerCase(),
  )
}
