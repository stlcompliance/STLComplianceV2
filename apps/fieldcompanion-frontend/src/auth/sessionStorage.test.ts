import { afterEach, describe, expect, it } from 'vitest'
import type { FieldCompanionSessionResponse } from '../api/types'
import {
  clearSession,
  getAccessToken,
  loadSession,
  saveSession,
  toStoredSession,
} from './sessionStorage'
import { getLocalSubmission, getSubmissionToasts, setLocalSubmission, pushSubmissionToast } from '../lib/submissionState'

const sampleSession: FieldCompanionSessionResponse = {
  accessToken: 'access-token',
  refreshToken: 'refresh-token',
  accessExpiresAt: new Date(Date.now() + 60_000).toISOString(),
  refreshExpiresAt: new Date(Date.now() + 86_400_000).toISOString(),
  sessionId: 'session-id',
  userId: 'user-id',
  personId: 'person-id',
  email: 'user@example.com',
  displayName: 'User Example',
  tenantId: 'tenant-id',
  tenantSlug: 'tenant-slug',
  tenantDisplayName: 'Tenant Display',
  tenantRoleKey: 'tenant_member',
  isPlatformAdmin: false,
  launchableProductKeys: ['fieldcompanion'],
  themePreference: 'dark',
  callbackUrl: 'http://localhost:5181/launch',
}

afterEach(() => {
  clearSession()
})

describe('fieldcompanion sessionStorage', () => {
  it('round-trips session metadata without persisting the access token', () => {
    const session = toStoredSession(sampleSession)
    const { accessToken: _accessToken, ...persistedSession } = session

    saveSession(session)

    expect(loadSession()).toMatchObject(persistedSession)
    expect(loadSession()?.accessToken).toBeUndefined()
    expect(getAccessToken(loadSession())).toBe(sampleSession.accessToken)
  })

  it('treats expired access tokens as unavailable', () => {
    const expiredSession = toStoredSession({
      ...sampleSession,
      accessExpiresAt: new Date(Date.now() - 1_000).toISOString(),
    })

    saveSession(expiredSession)

    expect(getAccessToken(loadSession())).toBeNull()
  })

  it('clears offline and submission state on logout', () => {
    setLocalSubmission({
      taskKey: 'trainarr:assignment:1',
      kind: 'acknowledge',
      phase: 'queued',
      message: 'Queued for sync',
    })
    pushSubmissionToast({ tone: 'info', message: 'Pending sync' })

    saveSession(toStoredSession(sampleSession))
    clearSession()

    expect(getLocalSubmission('trainarr:assignment:1', 'acknowledge')).toBeUndefined()
    expect(getSubmissionToasts()).toHaveLength(0)
    expect(loadSession()).toBeNull()
  })
})
