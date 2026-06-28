import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { HomePage } from './HomePage'

const { useOfflineQueueMock } = vi.hoisted(() => ({
  useOfflineQueueMock: vi.fn(() => ({
    isOnline: true,
    pendingCount: 0,
    pending: [],
    lastSyncedAt: null,
    lastSyncError: null as string | null,
    isSyncing: false,
    syncPending: vi.fn(),
    queueAcknowledge: vi.fn().mockResolvedValue(undefined),
    queueClockAction: vi.fn().mockResolvedValue(undefined),
  })),
}))

vi.mock('../api/client', () => ({
  getFieldInbox: vi.fn().mockResolvedValue({
    summary: {
      totalCount: 2,
      blockedCount: 0,
      countByProduct: {
        maintainarr: 1,
        routarr: 1,
      },
    },
    items: [],
    sources: [],
  }),
  productLaunchUrl: vi.fn((productKey: string) => `/launch/${productKey}`),
}))

vi.mock('../hooks/useFieldCompanionWorkspace', () => ({
  useFieldCompanionWorkspace: vi.fn(() => ({
    session: {
      accessTokenExpiresAt: '2099-01-01T00:00:00Z',
      tenantDisplayName: 'Acme Logistics',
      tenantSlug: 'acme-logistics',
    },
    accessToken: 'token',
    meQuery: {
      data: {
        displayName: 'Alex Worker',
        fieldProductKeys: ['maintainarr', 'routarr'],
        isPlatformAdmin: true,
        tenantRoleKey: 'tenant_admin',
      },
      isError: false,
    },
  })),
}))

vi.mock('../lib/deviceCapabilities', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../lib/deviceCapabilities')>()

  return {
    ...actual,
    buildDeviceCapabilitySnapshot: vi.fn(() => ({
      appVersion: '1.2.3',
      browserUserAgent: 'Mock Browser/1.0',
      checkedAt: '2026-06-23T18:00:00Z',
      capabilities: [
        {
          key: 'install-mode',
          label: 'Install mode',
          status: 'ready',
          value: 'Installed app',
          fallback: 'Install to the home screen for faster launches and a more app-like offline flow.',
        },
      ],
      language: 'en-US',
      online: true,
      platform: 'Mock OS',
      warnings: [],
    })),
  }
})

vi.mock('../hooks/useFieldCompanionProductLaunch', () => ({
  useFieldCompanionProductLaunch: vi.fn(() => ({
    isPending: false,
    isError: false,
    mutateAsync: vi.fn(),
    mutate: vi.fn(),
  })),
}))

vi.mock('../hooks/useFieldCompanionWebPush', () => ({
  useFieldCompanionWebPush: vi.fn(),
}))

vi.mock('../hooks/useOfflineQueue', () => ({
  useOfflineQueue: useOfflineQueueMock,
}))

vi.mock('../hooks/useFieldTaskSubmissionState', () => ({
  useFieldTaskSubmissionState: vi.fn(() => ({
    toasts: [],
    dismissToast: vi.fn(),
    refreshServerStatus: vi.fn(),
    getChips: vi.fn(() => []),
  })),
}))

vi.mock('../components/FieldInboxPanel', () => ({
  FieldInboxPanel: ({
    inbox,
  }: {
    inbox: { summary: { totalCount: number } }
  }) =>
    <div data-testid="fieldcompanion-home-inbox-panel">
      {`Inbox total ${inbox.summary.totalCount}`}
    </div>,
}))

vi.mock('../components/NotificationSettingsPanel', () => ({
  NotificationSettingsPanel: ({ canManage }: { canManage: boolean }) =>
    canManage ? <div data-testid="fieldcompanion-home-notifications-panel">Notifications</div> : null,
}))

vi.mock('../components/OfflineQueuePanel', () => ({
  OfflineQueuePanel: ({
    pendingCount,
  }: {
    pendingCount: number
  }) => <div data-testid="fieldcompanion-home-offline-queue-panel">{pendingCount} pending</div>,
}))

vi.mock('../components/SubmissionActivityBanner', () => ({
  SubmissionActivityBanner: () => null,
}))

describe('HomePage', () => {
  afterEach(() => {
    cleanup()
    useOfflineQueueMock.mockReset()
    useOfflineQueueMock.mockReturnValue({
      isOnline: true,
      pendingCount: 0,
      pending: [],
      lastSyncedAt: null,
      lastSyncError: null as string | null,
      isSyncing: false,
      syncPending: vi.fn(),
      queueAcknowledge: vi.fn().mockResolvedValue(undefined),
      queueClockAction: vi.fn().mockResolvedValue(undefined),
    })
  })

  it('routes handoff traffic to the launch flow', async () => {
    render(
      <MemoryRouter initialEntries={['/?handoff=abc123']}>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/launch" element={<p>Launch route reached</p>} />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByText('Launch route reached')).toBeInTheDocument()
  })

  it('renders the work dashboard and product launch shortcuts', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={['/']}>
          <Routes>
            <Route path="/" element={<HomePage />} />
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>,
    )

    expect(await screen.findByText('My work')).toBeInTheDocument()
    expect(screen.getByText('Alex Worker · Acme Logistics · 2 product workspaces')).toBeInTheDocument()
    expect(screen.queryByText('Alex Worker · acme-logistics · 2 product workspaces')).not.toBeInTheDocument()
    expect(await screen.findByTestId('fieldcompanion-home-inbox-panel')).toHaveTextContent('Inbox total 2')
    expect(screen.getByTestId('fieldcompanion-home-notifications-panel')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-home-offline-queue-panel')).toHaveTextContent('0 pending')
    expect(screen.getAllByRole('button', { name: 'Open workspace' })).toHaveLength(2)
  })

  it('shows degraded operation guidance when offline', async () => {
    useOfflineQueueMock.mockReturnValueOnce({
      isOnline: false,
      pendingCount: 2,
      pending: [],
      lastSyncedAt: null,
      lastSyncError: 'Sync blocked by policy.',
      isSyncing: false,
      syncPending: vi.fn(),
      queueAcknowledge: vi.fn().mockResolvedValue(undefined),
      queueClockAction: vi.fn().mockResolvedValue(undefined),
    })

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={['/']}>
          <Routes>
            <Route path="/" element={<HomePage />} />
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('fieldcompanion-degraded-operation-panel')).toHaveTextContent(
      'Offline fallback',
    )
    expect(screen.getByTestId('fieldcompanion-degraded-operation-copy')).toBeInTheDocument()
  })
})
