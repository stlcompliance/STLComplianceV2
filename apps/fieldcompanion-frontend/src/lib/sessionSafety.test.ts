import { describe, expect, it } from 'vitest'

import type { StoredFieldCompanionSession } from '../auth/sessionStorage'
import {
  getFieldCompanionSessionAccessTokenRenewalDeadlineMs,
  summarizeFieldCompanionSession,
} from './sessionSafety'

const session: Pick<StoredFieldCompanionSession, 'accessTokenExpiresAt'> = {
  accessTokenExpiresAt: '2026-06-26T12:30:00.000Z',
}

describe('sessionSafety', () => {
  it('flags expiring access tokens and computes the renewal deadline', () => {
    const snapshot = summarizeFieldCompanionSession(session, new Date('2026-06-26T12:20:00.000Z'))

    expect(snapshot.isAccessExpired).toBe(false)
    expect(snapshot.isAccessExpiringSoon).toBe(true)
    expect(snapshot.statusLabel).toBe('Refresh recommended')
    expect(snapshot.tone).toBe('warning')
    expect(snapshot.warningWindowLabel).toBe('15m')
    expect(getFieldCompanionSessionAccessTokenRenewalDeadlineMs(session)).toBe(
      Date.parse('2026-06-26T12:29:30.000Z'),
    )
  })

  it('treats expired or invalid expiries as needing refresh', () => {
    const expired = summarizeFieldCompanionSession(
      { accessTokenExpiresAt: '2026-06-26T12:00:00.000Z' },
      new Date('2026-06-26T12:05:00.000Z'),
    )

    expect(expired.isAccessExpired).toBe(true)
    expect(expired.statusLabel).toBe('Session expired')
    expect(expired.tone).toBe('danger')

    const invalid = summarizeFieldCompanionSession({ accessTokenExpiresAt: 'not-a-date' })
    expect(invalid.isAccessExpired).toBe(true)
    expect(invalid.statusLabel).toBe('Session needs refresh')
  })
})
