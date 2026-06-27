import { describe, expect, it } from 'vitest'

import {
  buildFieldCompanionReleaseSafetySnapshot,
  compareFieldCompanionReleaseVersions,
  parseFieldCompanionReleaseCsv,
} from './releaseSafety'

describe('releaseSafety', () => {
  it('compares semantic version strings safely', () => {
    expect(compareFieldCompanionReleaseVersions('1.2.3', '1.2.4')).toBeLessThan(0)
    expect(compareFieldCompanionReleaseVersions('1.2.4', '1.2.3')).toBeGreaterThan(0)
    expect(compareFieldCompanionReleaseVersions('1.2.3', '1.2.3')).toBe(0)
    expect(compareFieldCompanionReleaseVersions('test', '1.2.3')).toBeNull()
  })

  it('parses staged flags and blocks builds below the minimum version', () => {
    const snapshot = buildFieldCompanionReleaseSafetySnapshot({
      appVersion: '1.0.0',
      minimumSupportedVersion: '2.0.0',
      releaseMode: 'paused',
      releaseMessage: null,
      stagedFlags: parseFieldCompanionReleaseCsv(' fieldcompanion-workspace-release,offline-queue '),
      killSwitches: parseFieldCompanionReleaseCsv(' scan '),
    })

    expect(snapshot.isActionBlocked).toBe(true)
    expect(snapshot.title).toBe('Update required')
    expect(snapshot.stagedFlags).toEqual(['fieldcompanion-workspace-release', 'offline-queue'])
    expect(snapshot.killSwitches).toEqual(['scan'])
  })
})
