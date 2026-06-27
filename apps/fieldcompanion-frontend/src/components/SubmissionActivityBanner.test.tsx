import { cleanup, fireEvent, render, screen, act } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { SubmissionActivityBanner } from './SubmissionActivityBanner'

describe('SubmissionActivityBanner', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })

  afterEach(() => {
    cleanup()
    vi.useRealTimers()
  })

  it('renders the latest toast and dismisses it on demand', () => {
    const onDismiss = vi.fn()

    render(
      <SubmissionActivityBanner
        toasts={[
          {
            id: 'toast-1',
            tone: 'success',
            message: 'Queued acknowledgment synced.',
            createdAt: '2026-06-26T12:00:00Z',
          },
        ]}
        onDismiss={onDismiss}
      />,
    )

    expect(screen.getByTestId('fieldcompanion-submission-toast')).toHaveTextContent(
      'Queued acknowledgment synced.',
    )

    fireEvent.click(screen.getByRole('button', { name: 'Dismiss' }))

    expect(onDismiss).toHaveBeenCalledWith('toast-1')
  })

  it('auto-dismisses the toast after the timeout window', () => {
    const onDismiss = vi.fn()

    render(
      <SubmissionActivityBanner
        toasts={[
          {
            id: 'toast-2',
            tone: 'info',
            message: 'Sync queued for review.',
            createdAt: '2026-06-26T12:05:00Z',
          },
        ]}
        onDismiss={onDismiss}
      />,
    )

    act(() => {
      vi.advanceTimersByTime(6000)
    })

    expect(onDismiss).toHaveBeenCalledWith('toast-2')
  })
})
