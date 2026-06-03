import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { NexarrApiError } from '../api/types'
import { LoginPage } from './LoginPage'

const loginMock = vi.fn()

vi.mock('../auth/AuthProvider', () => ({
  useAuth: () => ({
    login: loginMock,
    isAuthenticated: false,
  }),
}))

describe('LoginPage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders sign-in failures in shared error callout', async () => {
    loginMock.mockRejectedValueOnce(new NexarrApiError(401, 'Invalid credentials'))

    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Sign in' }))

    expect(await screen.findByText('Invalid credentials')).toBeTruthy()
    expect(screen.getByRole('alert')).toBeTruthy()
  })

  it('sends remember-device when checked', async () => {
    loginMock.mockResolvedValueOnce(undefined)

    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>,
    )

    fireEvent.click(screen.getByLabelText('Remember this device'))
    fireEvent.click(screen.getByRole('button', { name: 'Sign in' }))

    await waitFor(() => {
      expect(loginMock).toHaveBeenCalledWith(
        'admin@demo.stl',
        'ChangeMe!Demo2026',
        '11111111-1111-1111-1111-111111111101',
        true,
        undefined,
        undefined,
      )
    })
  })

  it('prompts for MFA when the backend requires a challenge', async () => {
    loginMock.mockRejectedValueOnce(new NexarrApiError(403, 'Multi-factor authentication is required.', 'auth.mfa_required'))
    loginMock.mockResolvedValueOnce(undefined)

    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Sign in' }))

    expect(await screen.findByText('Multi-factor authentication required')).toBeTruthy()

    fireEvent.change(screen.getByLabelText('Authentication code'), {
      target: { value: '123456' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Verify and sign in' }))

    await waitFor(() => {
      expect(loginMock).toHaveBeenLastCalledWith(
        'admin@demo.stl',
        'ChangeMe!Demo2026',
        '11111111-1111-1111-1111-111111111101',
        false,
        '123456',
        undefined,
      )
    })
  })
})
