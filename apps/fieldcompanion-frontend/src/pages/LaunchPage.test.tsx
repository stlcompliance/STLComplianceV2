import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import type { FieldCompanionSessionResponse } from '../api/types'
import { redeemHandoff } from '../api/client'
import { saveSession } from '../auth/sessionStorage'
import type { FieldCompanionReleaseSafetySnapshot } from '../lib/releaseSafety'
import { LaunchPage } from './LaunchPage'

const readyReleaseSafetySnapshot: FieldCompanionReleaseSafetySnapshot = {
  appVersion: '1.0.0',
  minimumSupportedVersion: null,
  releaseMode: 'ready',
  releaseMessage: null,
  stagedFlags: [],
  killSwitches: [],
  isUpdateRequired: false,
  isPaused: false,
  isActionBlocked: false,
  tone: 'info',
  title: 'Release ready',
  message: 'The current build is within the supported release window.',
}

const blockedReleaseSafetySnapshot: FieldCompanionReleaseSafetySnapshot = {
  ...readyReleaseSafetySnapshot,
  minimumSupportedVersion: '2.0.0',
  releaseMode: 'required',
  releaseMessage: 'This build requires an update before you can continue.',
  stagedFlags: ['fieldcompanion-workspace-release'],
  killSwitches: ['scan'],
  isUpdateRequired: true,
  isActionBlocked: true,
  tone: 'error',
  title: 'Update required',
  message: 'This build requires an update before you can continue.',
}

let currentReleaseSafetySnapshot = readyReleaseSafetySnapshot

vi.mock('../api/client', () => ({
  redeemHandoff: vi.fn(),
}))

vi.mock('../auth/sessionStorage', () => ({
  saveSession: vi.fn(),
}))

vi.mock('../lib/releaseSafety', () => ({
  readCurrentFieldCompanionReleaseSafetySnapshot: vi.fn(() => currentReleaseSafetySnapshot),
}))

describe('LaunchPage', () => {
  afterEach(() => {
    cleanup()
    vi.mocked(redeemHandoff).mockReset()
    vi.mocked(saveSession).mockReset()
    currentReleaseSafetySnapshot = readyReleaseSafetySnapshot
  })

  it('shows callout when handoff code is missing', async () => {
    render(
      <MemoryRouter initialEntries={['/launch']}>
        <Routes>
          <Route path="/launch" element={<LaunchPage />} />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByText('Missing handoff code. Launch the Field Companion app from the suite.')).toBeInTheDocument()
    expect(screen.getByRole('alert')).toBeInTheDocument()
  })

  it('blocks sign-in when the current build requires an update', async () => {
    currentReleaseSafetySnapshot = blockedReleaseSafetySnapshot

    const session = {
      accessToken: 'access-token',
      refreshToken: 'refresh-token',
      accessExpiresAt: new Date(Date.now() + 60_000).toISOString(),
      refreshExpiresAt: new Date(Date.now() + 86_400_000).toISOString(),
      sessionId: 'session-id',
      userId: 'user-id',
      personId: 'person-id',
      email: 'user@example.com',
      displayName: 'User Example',
      tenantId: 'tenant-id',
      tenantSlug: 'tenant-slug',
      tenantDisplayName: 'Tenant Display',
      tenantRoleKey: 'tenant_member',
      isPlatformAdmin: false,
      launchableProductKeys: ['fieldcompanion'],
      themePreference: 'dark',
      callbackUrl: 'http://localhost:5181/launch',
    } satisfies FieldCompanionSessionResponse

    vi.mocked(redeemHandoff).mockResolvedValue(session)

    render(
      <MemoryRouter initialEntries={['/launch?handoff=abc123']}>
        <Routes>
          <Route path="/launch" element={<LaunchPage />} />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByRole('heading', { name: 'Update required' })).toBeInTheDocument()
    expect(screen.getByRole('alert')).toHaveTextContent('This build requires an update before you can continue.')
    expect(screen.getByTestId('fieldcompanion-degraded-operation-panel')).toHaveTextContent(
      'Recovery guidance',
    )
    expect(screen.getByRole('link', { name: 'Return to suite home' })).toBeInTheDocument()
    await waitFor(() => expect(redeemHandoff).not.toHaveBeenCalled())
    expect(saveSession).not.toHaveBeenCalled()
  })
})
