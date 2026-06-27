import { cleanup, fireEvent, render, screen, act } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import {
  SHARED_DEVICE_LOCK_DELAY_MS,
  SHARED_DEVICE_WARNING_DELAY_MS,
  useSharedDeviceProtection,
} from './sharedDeviceProtection'

function SharedDeviceProtectionHarness({ enabled }: { enabled: boolean }) {
  const protection = useSharedDeviceProtection(enabled)

  return (
    <div>
      <div data-testid="shared-device-phase">{protection.phase}</div>
      <button type="button" onClick={protection.recordActivity}>
        Record activity
      </button>
      <button type="button" onClick={protection.lockNow}>
        Lock now
      </button>
    </div>
  )
}

describe('sharedDeviceProtection', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })

  afterEach(() => {
    cleanup()
    vi.useRealTimers()
  })

  it('advances from active to warning and locked on inactivity', () => {
    render(<SharedDeviceProtectionHarness enabled />)

    expect(screen.getByTestId('shared-device-phase')).toHaveTextContent('active')

    act(() => {
      vi.advanceTimersByTime(SHARED_DEVICE_WARNING_DELAY_MS)
    })
    expect(screen.getByTestId('shared-device-phase')).toHaveTextContent('warning')

    act(() => {
      vi.advanceTimersByTime(SHARED_DEVICE_LOCK_DELAY_MS - SHARED_DEVICE_WARNING_DELAY_MS)
    })
    expect(screen.getByTestId('shared-device-phase')).toHaveTextContent('locked')
  })

  it('resets the warning timer on explicit activity and can lock immediately', () => {
    render(<SharedDeviceProtectionHarness enabled />)

    act(() => {
      vi.advanceTimersByTime(SHARED_DEVICE_WARNING_DELAY_MS - 100)
    })
    fireEvent.click(screen.getByRole('button', { name: 'Record activity' }))

    act(() => {
      vi.advanceTimersByTime(SHARED_DEVICE_WARNING_DELAY_MS - 1)
    })
    expect(screen.getByTestId('shared-device-phase')).toHaveTextContent('active')

    act(() => {
      vi.advanceTimersByTime(1)
    })
    expect(screen.getByTestId('shared-device-phase')).toHaveTextContent('warning')

    fireEvent.click(screen.getByRole('button', { name: 'Lock now' }))
    expect(screen.getByTestId('shared-device-phase')).toHaveTextContent('locked')
  })
})
