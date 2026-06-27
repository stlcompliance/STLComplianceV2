import { cleanup, fireEvent, render, screen, act } from '@testing-library/react'
import type { ReactNode } from 'react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { clearSession } from '../auth/sessionStorage'
import {
  clearOfflineQueueForTests,
  enqueueFieldInboxAcknowledge,
  markSyncPartial,
} from '../lib/offlineQueue'
import { ProductWorkspaceLayout } from './ProductWorkspaceLayout'

const originalLocation = window.location

vi.mock('@stl/shared-ui', () => ({
  buildProductLaunchUrlMap: vi.fn(() => ({})),
  resolveProductWorkspaceBootstrapError: vi.fn(() => null),
  resolveSuiteHomeUrl: vi.fn(() => '/suite'),
  ProductWorkspaceFrame: ({
    children,
    onSignOut,
    workspaceSession,
  }: {
    children?: ReactNode
    onSignOut?: () => void
    workspaceSession?: { userDisplayName?: string; tenantDisplayName?: string }
  }) => (
    <div data-testid="workspace-frame">
      <button type="button" onClick={onSignOut}>
        Shell sign out
      </button>
      <div data-testid="workspace-session">
        {workspaceSession
          ? `${workspaceSession.userDisplayName ?? 'Unknown'} · ${workspaceSession.tenantDisplayName ?? 'Unknown'}`
          : 'none'}
      </div>
      {children}
    </div>
  ),
}))

vi.mock('../auth/sessionStorage', () => ({
  clearSession: vi.fn(),
}))

vi.mock('../api/client', () => ({
  renewFieldCompanionSession: vi.fn().mockResolvedValue(null),
}))

vi.mock('../hooks/useFieldCompanionWorkspace', () => ({
  useFieldCompanionWorkspace: vi.fn(() => ({
    session: {
      userId: 'user-id',
      tenantId: 'tenant-id',
      tenantDisplayName: 'Acme Logistics',
      tenantSlug: 'acme-logistics',
      isPlatformAdmin: false,
    },
    accessToken: 'token',
    meQuery: {
      data: {
        displayName: 'Alex Worker',
        fieldProductKeys: ['fieldcompanion'],
        isPlatformAdmin: false,
        tenantRoleKey: 'tenant_member',
      },
      isError: false,
      isLoading: false,
      error: null,
    },
  })),
}))

vi.mock('../hooks/useFieldCompanionProductLaunch', () => ({
  useFieldCompanionProductLaunch: vi.fn(() => ({
    isPending: false,
    isError: false,
    error: null,
    mutate: vi.fn(),
    mutateAsync: vi.fn(),
  })),
}))

vi.mock('../lib/sharedDeviceProtection', async () => {
  const actual = await vi.importActual<typeof import('../lib/sharedDeviceProtection')>(
    '../lib/sharedDeviceProtection',
  )

  return {
    ...actual,
    isFieldCompanionSharedDeviceModeEnabled: vi.fn(() => true),
  }
})

describe('ProductWorkspaceLayout', () => {
  beforeEach(() => {
    vi.useFakeTimers()
    vi.mocked(clearSession).mockClear()
    Object.defineProperty(window, 'location', {
      configurable: true,
      value: {
        ...originalLocation,
        assign: vi.fn(),
      },
    })
  })

  afterEach(() => {
    cleanup()
    clearOfflineQueueForTests()
    vi.mocked(clearSession).mockClear()
    vi.restoreAllMocks()
    Object.defineProperty(window, 'location', {
      configurable: true,
      value: originalLocation,
    })
    vi.useRealTimers()
  })

  function renderLayout() {
    render(
      <MemoryRouter initialEntries={['/']}>
        <Routes>
          <Route element={<ProductWorkspaceLayout />}>
            <Route path="/" element={<div>Workspace root</div>} />
            <Route path="/offline-queue" element={<div>Offline queue route reached</div>} />
          </Route>
        </Routes>
      </MemoryRouter>,
    )
  }

  it('blocks sign-out until queued work is handled', () => {
    const queuedAction = enqueueFieldInboxAcknowledge({
      taskKey: 'trainarr:1',
      productKey: 'trainarr',
      title: 'Acknowledge training',
    })
    markSyncPartial({
      syncedKeys: new Set(),
      permanentRejectedItems: [
        {
          idempotencyKey: queuedAction.idempotencyKey,
          reasonCode: 'fieldcompanion.offline_actions.record_changed',
          reasonMessage: 'The task changed while you were offline.',
        },
      ],
      lastSyncError: 'Validation failed',
    })

    renderLayout()
    expect(screen.getByTestId('workspace-session')).toHaveTextContent('Alex Worker · Acme Logistics')

    act(() => {
      vi.advanceTimersByTime(1000)
    })

    expect(screen.getByText('Shared device warning')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Shell sign out' }))

    expect(screen.getByText('Session locked')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Review offline queue' })).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Return to sign in' })).not.toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Discard queued work and sign out' }))

    expect(clearSession).toHaveBeenCalledTimes(1)
    expect(window.location.assign).toHaveBeenCalledWith('/suite')
  })

  it('opens the offline queue from the locked overlay', () => {
    enqueueFieldInboxAcknowledge({
      taskKey: 'trainarr:2',
      productKey: 'trainarr',
      title: 'Review offline queue',
    })

    renderLayout()

    act(() => {
      vi.advanceTimersByTime(1000)
    })
    fireEvent.click(screen.getByRole('button', { name: 'Shell sign out' }))
    fireEvent.click(screen.getByRole('button', { name: 'Review offline queue' }))

    expect(screen.getByText('Offline queue route reached')).toBeInTheDocument()
  })
})
