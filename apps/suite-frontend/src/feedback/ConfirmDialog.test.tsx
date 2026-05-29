import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ConfirmDialog } from './ConfirmDialog'

describe('ConfirmDialog', () => {
  afterEach(() => {
    cleanup()
  })

  it('calls confirm and cancel handlers', async () => {
    const user = userEvent.setup()
    const onConfirm = vi.fn()
    const onCancel = vi.fn()

    render(
      <ConfirmDialog
        open
        title="Revoke session?"
        description="This device will lose access immediately."
        confirmLabel="Revoke session"
        danger
        onConfirm={onConfirm}
        onCancel={onCancel}
      />,
    )

    expect(screen.getByRole('alertdialog')).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Cancel' }))
    expect(onCancel).toHaveBeenCalledTimes(1)

    await user.click(screen.getByRole('button', { name: 'Revoke session' }))
    expect(onConfirm).toHaveBeenCalledTimes(1)
  })

  it('does not render when closed', () => {
    render(
      <ConfirmDialog
        open={false}
        title="Hidden"
        description="Not visible"
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />,
    )

    expect(screen.queryByRole('alertdialog')).not.toBeInTheDocument()
  })
})
