import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ResetPasswordPage } from './ResetPasswordPage'
import * as nexarr from '../api/nexarrClient'

vi.mock('../api/nexarrClient', () => ({
  resetPassword: vi.fn(),
}))

describe('ResetPasswordPage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders password-reset failures in shared error callout', async () => {
    vi.mocked(nexarr.resetPassword).mockRejectedValueOnce(new Error('reset failed'))

    render(
      <MemoryRouter initialEntries={['/reset-password?token=abc123']}>
        <ResetPasswordPage />
      </MemoryRouter>,
    )

    fireEvent.change(screen.getByLabelText('New password'), { target: { value: 'StrongPassword2026' } })
    fireEvent.change(screen.getByLabelText('Confirm password'), { target: { value: 'StrongPassword2026' } })
    fireEvent.click(screen.getByRole('button', { name: 'Update password' }))

    expect(await screen.findByText('NexArr is temporarily unavailable. Please try again later.')).toBeTruthy()
    expect(screen.getByRole('alert')).toBeTruthy()
  })
})
