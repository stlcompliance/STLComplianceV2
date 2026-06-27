import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import type { FieldCompanionReleaseSafetySnapshot } from '../lib/releaseSafety'
import { FieldCompanionReleaseSafetyBanner } from './FieldCompanionReleaseSafetyBanner'

const blockedSnapshot: FieldCompanionReleaseSafetySnapshot = {
  appVersion: '1.0.0',
  minimumSupportedVersion: '2.0.0',
  releaseMode: 'required',
  releaseMessage: 'This build requires an update before you can continue.',
  stagedFlags: ['fieldcompanion-workspace-release'],
  killSwitches: ['scan'],
  isUpdateRequired: true,
  isPaused: false,
  isActionBlocked: true,
  tone: 'error',
  title: 'Update required',
  message: 'This build requires an update before you can continue.',
}

describe('FieldCompanionReleaseSafetyBanner', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders the update gate and refresh actions', () => {
    const onRefresh = vi.fn()

    render(
      <FieldCompanionReleaseSafetyBanner
        snapshot={blockedSnapshot}
        suiteHomeUrl="/suite"
        onRefresh={onRefresh}
      />,
    )

    expect(screen.getByRole('alert')).toHaveTextContent('Update required')
    expect(screen.getByText('App version: 1.0.0')).toBeInTheDocument()
    expect(screen.getByText('Kill switches: scan')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Refresh app' }))
    expect(onRefresh).toHaveBeenCalledTimes(1)
    expect(screen.getByRole('link', { name: 'Return to suite home' })).toHaveAttribute('href', '/suite')
  })
})
