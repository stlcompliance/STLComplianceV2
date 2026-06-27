import { cleanup, render, screen, act } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useFieldCompanionWorkspace } from './useFieldCompanionWorkspace'
import { clearSession, saveSession, toStoredSession } from '../auth/sessionStorage'
import type { FieldCompanionSessionResponse } from '../api/types'

vi.mock('../api/client', () => ({
  getMe: vi.fn(async () => ({
    displayName: 'Alex Worker',
    email: 'alex.worker@example.com',
    fieldProductKeys: ['fieldcompanion'],
    isPlatformAdmin: false,
    personId: 'person-id',
    tenantDisplayName: 'Acme Logistics',
    tenantId: 'tenant-id',
    tenantRoleKey: 'worker',
    tenantSlug: 'acme-logistics',
    userId: 'user-id',
  })),
}))

const sampleSession: FieldCompanionSessionResponse = {
  accessToken: 'access-token',
  refreshToken: 'refresh-token',
  accessExpiresAt: '2026-06-26T12:01:00.000Z',
  refreshExpiresAt: '2026-06-27T12:00:00.000Z',
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
}

function Harness() {
  const { accessToken } = useFieldCompanionWorkspace()

  return <div data-testid="workspace-token">{accessToken || 'none'}</div>
}

describe('useFieldCompanionWorkspace', () => {
  beforeEach(() => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-06-26T12:00:00.000Z'))
    saveSession(toStoredSession(sampleSession))
  })

  afterEach(() => {
    cleanup()
    clearSession()
    vi.useRealTimers()
    vi.restoreAllMocks()
  })

  it('drops the access token when the renewal deadline passes', async () => {
    const queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
        },
      },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <Harness />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('workspace-token')).toHaveTextContent('access-token')

    act(() => {
      vi.advanceTimersByTime(30_001)
    })

    expect(screen.getByTestId('workspace-token')).toHaveTextContent('none')
  })
})
