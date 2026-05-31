import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ForgotPasswordPage } from './ForgotPasswordPage'
import * as nexarr from '../api/nexarrClient'

vi.mock('../api/nexarrClient', () => ({
  requestPasswordReset: vi.fn(),
}))

describe('ForgotPasswordPage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders reset-request failures in shared error callout', async () => {
    vi.mocked(nexarr.requestPasswordReset).mockRejectedValueOnce(new Error('reset unavailable'))

    render(
      <MemoryRouter>
        <ForgotPasswordPage />
      </MemoryRouter>,
    )

    fireEvent.change(screen.getByLabelText('Email'), { target: { value: 'admin@demo.stl' } })
    fireEvent.click(screen.getByRole('button', { name: 'Send reset link' }))

    expect(await screen.findByText('Could not reach NexArr. Try again later.')).toBeTruthy()
    expect(screen.getByRole('alert')).toBeTruthy()
  })
})
