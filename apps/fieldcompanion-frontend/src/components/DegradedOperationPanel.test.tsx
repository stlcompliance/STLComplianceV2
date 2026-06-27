import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { buildDeviceCapabilitySnapshot } from '../lib/deviceCapabilities'
import { buildFieldCompanionOperationalFallbackSnapshot } from '../lib/degradedOperation'
import { DegradedOperationPanel } from './DegradedOperationPanel'

describe('DegradedOperationPanel', () => {
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

  it('renders fallback guidance and copies the support summary', async () => {
    const snapshot = buildFieldCompanionOperationalFallbackSnapshot({
      deviceSnapshot: buildDeviceCapabilitySnapshot({
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
      }),
      isOnline: false,
      lastSyncError: 'Sync blocked by policy.',
    })

    render(
      <DegradedOperationPanel
        snapshot={snapshot}
        actions={[
          { label: 'Return to suite home', href: '/' },
          { label: 'Open profile', href: '/profile' },
        ]}
      />,
    )

    expect(screen.getByTestId('fieldcompanion-degraded-operation-panel')).toHaveTextContent(
      'Offline fallback',
    )
    expect(screen.getByRole('link', { name: 'Return to suite home' })).toHaveAttribute('href', '/')

    fireEvent.click(screen.getByTestId('fieldcompanion-degraded-operation-copy'))

    expect(clipboardWriteText).toHaveBeenCalledTimes(1)
    expect(await screen.findByTestId('fieldcompanion-degraded-operation-copy-status')).toHaveTextContent(
      'Support summary copied to clipboard.',
    )
  })
})
