import { describe, expect, it } from 'vitest'

import {
  buildDeviceCapabilitySnapshot,
  classifyFieldCompanionBrowser,
  classifyFieldCompanionDeviceClass,
  formatDeviceCapabilityDiagnosticSummary,
  formatFieldCompanionLanguageGroup,
} from './deviceCapabilities'

describe('buildDeviceCapabilitySnapshot', () => {
  it('summarizes ready and degraded browser capabilities with fallback guidance', () => {
    const snapshot = buildDeviceCapabilitySnapshot({
      appVersion: '1.2.3',
      browserUserAgent: 'Mock Browser/1.0',
      language: 'en-US',
      connectionDownlinkMbps: 0.4,
      connectionEffectiveType: '2g',
      connectionSaveData: true,
      connectionSupported: true,
      online: false,
      platform: 'Mock OS',
      installedMode: false,
      serviceWorkerSupported: true,
      backgroundSyncSupported: false,
      pushPermission: 'default',
      pushSupported: true,
      cameraSupported: false,
      geolocationSupported: true,
      storageSupported: true,
      permissionApiSupported: false,
    })

    expect(snapshot.capabilities.find((item) => item.key === 'network')).toMatchObject({
      status: 'degraded',
      value: 'Offline',
    })
    expect(snapshot.capabilities.find((item) => item.key === 'connection')).toMatchObject({
      status: 'degraded',
      value: 'Low-data mode',
    })
    expect(snapshot.capabilities.find((item) => item.key === 'push')).toMatchObject({
      status: 'degraded',
      value: 'Browser push permission not requested',
    })
    expect(snapshot.capabilities.find((item) => item.key === 'camera')).toMatchObject({
      status: 'unsupported',
      fallback: 'Use file upload or typed notes instead.',
    })
    expect(snapshot.warnings).toContain('Use file upload or typed notes instead.')
    expect(snapshot.warnings).toContain(
      'Use smaller photos, shorter uploads, and retry large work on a stronger connection.',
    )
    expect(snapshot.warnings).toContain('Request permission from Notifications when you want live updates.')
    expect(snapshot.warnings).toContain('Refresh the queue when connectivity returns.')
  })

  it('formats a sanitized diagnostic summary for support use', () => {
    const snapshot = buildDeviceCapabilitySnapshot({
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
      backgroundSyncSupported: true,
      pushPermission: 'granted',
      pushSupported: true,
      cameraSupported: true,
      geolocationSupported: true,
      storageSupported: true,
      permissionApiSupported: true,
    })

    const summary = formatDeviceCapabilityDiagnosticSummary(snapshot)

    expect(summary).toContain('Field Companion device diagnostics')
    expect(summary).toContain('App version: 1.2.3')
    expect(summary).toContain('Browser: Browser')
    expect(summary).toContain('Device class: device')
    expect(summary).toContain('Language group: en')
    expect(summary).not.toContain('Mock Browser/1.0')
    expect(summary).not.toContain('Mock OS')
    expect(summary).not.toContain('en-US')
    expect(summary).toContain('Online: no')
    expect(summary).not.toContain('person-')
    expect(summary).not.toContain('tenant-')
  })

  it('classifies browser, device, and language diagnostics without retaining fingerprint strings', () => {
    expect(classifyFieldCompanionBrowser('Mozilla/5.0 Chrome/125.0.0.0 Safari/537.36')).toBe('Chrome')
    expect(classifyFieldCompanionDeviceClass('Win32', 'Mozilla/5.0 Windows NT 10.0')).toBe('Windows device')
    expect(formatFieldCompanionLanguageGroup('en-US')).toBe('en')
  })
})
