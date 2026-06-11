import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../api/nexarrClient'
import { NexarrApiError } from '../api/types'
import { LoginPage } from './LoginPage'

const authMock = vi.hoisted(() => ({
  isAuthenticated: false,
  login: vi.fn(),
}))
const loginMock = authMock.login

vi.mock('../auth/AuthProvider', () => ({
  useAuth: () => ({
    login: authMock.login,
    isAuthenticated: authMock.isAuthenticated,
  }),
}))

vi.mock('../api/nexarrClient', () => ({
  createHandoff: vi.fn(),
}))

function fillLoginCredentials() {
  fireEvent.change(screen.getByLabelText('Email'), {
    target: { value: 'admin@demo.stl' },
  })
  fireEvent.change(screen.getByLabelText('Password'), {
    target: { value: 'ChangeMe!Demo2026' },
  })
}

describe('LoginPage', () => {
  afterEach(() => {
    cleanup()
    authMock.isAuthenticated = false
    vi.clearAllMocks()
    vi.unstubAllGlobals()
  })

  it('renders sign-in failures in shared error callout', async () => {
    loginMock.mockRejectedValueOnce(new NexarrApiError(401, 'Invalid credentials'))

    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>,
    )

    fillLoginCredentials()
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

    fillLoginCredentials()
    fireEvent.click(screen.getByLabelText('Remember this device'))
    fireEvent.click(screen.getByRole('button', { name: 'Sign in' }))

    await waitFor(() => {
      expect(loginMock).toHaveBeenCalledWith(
        'admin@demo.stl',
        'ChangeMe!Demo2026',
        null,
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

    fillLoginCredentials()
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
        null,
        false,
        '123456',
        undefined,
      )
    })
  })

  it('creates a NexArr handoff for authenticated product callback visits', async () => {
    authMock.isAuthenticated = true
    vi.mocked(nexarr.createHandoff).mockResolvedValueOnce({
      handoffCode: 'handoff-1',
      handoffId: 'handoff-id-1',
      expiresAt: '2026-06-11T12:00:00Z',
      launchUrl: 'http://localhost:5175/launch?handoff=handoff-1',
    })
    const assign = vi.fn()
    vi.stubGlobal('location', {
      href: 'http://localhost:5174/login?productKey=staffarr',
      assign,
    })

    render(
      <MemoryRouter
        initialEntries={[
          '/login?productKey=staffarr&callbackUrl=http%3A%2F%2Flocalhost%3A5175%2Fpeople%3Ftab%3Droles',
        ]}
      >
        <LoginPage />
      </MemoryRouter>,
    )

    expect(screen.getByText('Launching product')).toBeTruthy()
    await waitFor(() => {
      expect(nexarr.createHandoff).toHaveBeenCalledWith(
        'staffarr',
        'http://localhost:5175/people?tab=roles',
      )
      expect(assign).toHaveBeenCalledWith('http://localhost:5175/launch?handoff=handoff-1')
    })
  })
})
