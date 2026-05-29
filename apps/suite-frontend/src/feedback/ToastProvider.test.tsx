import { act, cleanup, fireEvent, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ToastProvider, useToast } from './ToastProvider'

function ToastTrigger({ message }: { message: string }) {
  const { pushToast } = useToast()
  return (
    <button type="button" onClick={() => pushToast({ message, variant: 'success' })}>
      Show toast
    </button>
  )
}

describe('ToastProvider', () => {
  afterEach(() => {
    cleanup()
    vi.useRealTimers()
  })

  it('renders and dismisses toast notifications', async () => {
    const user = userEvent.setup()

    render(
      <ToastProvider>
        <ToastTrigger message="Session revoked." />
      </ToastProvider>,
    )

    await user.click(screen.getByRole('button', { name: 'Show toast' }))

    expect(screen.getByRole('status')).toHaveTextContent('Session revoked.')

    await user.click(screen.getByRole('button', { name: 'Dismiss notification' }))
    expect(screen.queryByRole('status')).not.toBeInTheDocument()
  })

  it('auto-dismisses toasts after the default duration', () => {
    vi.useFakeTimers()

    render(
      <ToastProvider>
        <ToastTrigger message="Saved." />
      </ToastProvider>,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Show toast' }))
    expect(screen.getByRole('status')).toBeInTheDocument()

    act(() => {
      vi.advanceTimersByTime(5000)
    })

    expect(screen.queryByRole('status')).not.toBeInTheDocument()
  })
})
