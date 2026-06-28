import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { DeviceCapabilityPanel } from './DeviceCapabilityPanel'
import { buildDeviceCapabilitySnapshot } from '../lib/deviceCapabilities'

vi.mock('../lib/deviceCapabilities', () => ({
  buildDeviceCapabilitySnapshot: vi.fn(() => ({
    appVersion: '1.2.3',
    browserUserAgent: 'Browser',
    checkedAt: '2026-06-23T18:00:00Z',
    capabilities: [
      {
        key: 'network',
        label: 'Network',
        status: 'degraded',
        value: 'Offline',
        fallback: 'Queued actions will sync automatically when the connection returns.',
      },
      {
        key: 'camera',
        label: 'Camera capture',
        status: 'unsupported',
        value: 'Unavailable',
        fallback: 'Use file upload or typed notes instead.',
      },
    ],
    language: 'en',
    online: false,
    platform: 'device',
    warnings: [
      'Queued actions will sync automatically when the connection returns.',
      'Use file upload or typed notes instead.',
    ],
  })),
  formatDeviceCapabilityDiagnosticSummary: vi.fn(
    (snapshot: { appVersion: string; browserUserAgent: string; platform: string }) =>
      `Field Companion device diagnostics\nApp version: ${snapshot.appVersion}\nBrowser: ${snapshot.browserUserAgent}\nDevice class: ${snapshot.platform}`,
  ),
}))

describe('DeviceCapabilityPanel', () => {
  const clipboardWriteText = vi.fn()

  beforeEach(() => {
    Object.defineProperty(navigator, 'clipboard', {
      configurable: true,
      value: {
        writeText: clipboardWriteText,
      },
    })
  })

  afterEach(() => {
    cleanup()
    clipboardWriteText.mockReset()
    Object.defineProperty(navigator, 'clipboard', {
      configurable: true,
      value: undefined,
    })
  })

  it('renders capability diagnostics and refreshes them on demand', () => {
    render(<DeviceCapabilityPanel />)

    expect(screen.getByText('Device readiness')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-device-capability-warning')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-device-capability-item-network')).toHaveTextContent('Offline')
    expect(screen.getByTestId('fieldcompanion-device-capability-item-camera')).toHaveTextContent('Use file upload or typed notes instead.')

    const mockedBuild = vi.mocked(buildDeviceCapabilitySnapshot)
    expect(mockedBuild).toHaveBeenCalledTimes(1)

    fireEvent.click(screen.getByTestId('fieldcompanion-device-capability-refresh'))

    expect(mockedBuild).toHaveBeenCalledTimes(2)
  })

  it('copies a sanitized diagnostic summary for support use', async () => {
    render(<DeviceCapabilityPanel />)

    fireEvent.click(screen.getByTestId('fieldcompanion-device-capability-copy'))

    expect(clipboardWriteText).toHaveBeenCalledTimes(1)
    expect(clipboardWriteText.mock.calls[0]?.[0]).toContain('Field Companion device diagnostics')
    expect(clipboardWriteText.mock.calls[0]?.[0]).not.toContain('Mock Browser/1.0')
    expect(clipboardWriteText.mock.calls[0]?.[0]).not.toContain('Mock OS')
    expect(await screen.findByTestId('fieldcompanion-device-capability-copy-status')).toHaveTextContent(
      'Diagnostic summary copied to clipboard.',
    )
  })
})
