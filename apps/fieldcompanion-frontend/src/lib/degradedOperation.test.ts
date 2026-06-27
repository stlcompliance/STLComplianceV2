import { describe, expect, it } from 'vitest'

import { buildDeviceCapabilitySnapshot } from './deviceCapabilities'
import { buildFieldCompanionOperationalFallbackSnapshot } from './degradedOperation'

describe('buildFieldCompanionOperationalFallbackSnapshot', () => {
  it('summarizes degraded launch and offline conditions without personal identifiers', () => {
    const deviceSnapshot = buildDeviceCapabilitySnapshot({
      appVersion: '1.2.3',
      browserUserAgent: 'Mock Browser/1.0',
      language: 'en-US',
      connectionDownlinkMbps: 12,
      connectionEffectiveType: '4g',
      connectionSaveData: false,
      connectionSupported: true,
      online: false,
      platform: 'Mock OS',
      installedMode: true,
      serviceWorkerSupported: true,
      backgroundSyncSupported: false,
      pushPermission: 'default',
      pushSupported: true,
      cameraSupported: true,
      geolocationSupported: true,
      storageSupported: true,
      permissionApiSupported: true,
    })

    const snapshot = buildFieldCompanionOperationalFallbackSnapshot({
      deviceSnapshot,
      isOnline: false,
      launchError: 'Handoff failed.',
      pendingOfflineActions: 2,
      lastSyncError: 'Sync blocked by policy.',
    })

    expect(snapshot.isVisible).toBe(true)
    expect(snapshot.tone).toBe('error')
    expect(snapshot.title).toContain('Launch')
    expect(snapshot.summary).toContain('Handoff failed.')
    expect(snapshot.recommendedSteps).toContain(
      'Return to suite home and retry launch from a fresh session.',
    )
    expect(snapshot.recommendedSteps).toContain(
      'Review the offline queue and keep queued work intact until sync completes.',
    )
    expect(snapshot.diagnosticSummary).toContain('Field Companion degraded operation summary')
    expect(snapshot.diagnosticSummary).not.toContain('person-')
    expect(snapshot.diagnosticSummary).not.toContain('tenant-')
  })
})
