import { cleanup, fireEvent, render, screen } from '@testing-library/react'
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
})
