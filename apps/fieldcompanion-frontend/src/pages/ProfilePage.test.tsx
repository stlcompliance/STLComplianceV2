import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { ProfilePage } from './ProfilePage'

vi.mock('../hooks/useFieldCompanionWorkspace', () => ({
  useFieldCompanionWorkspace: vi.fn(() => ({
    session: {
      accessToken: 'token',
      accessTokenExpiresAt: '2026-06-23T18:00:00Z',
      displayName: 'Alex Worker',
      email: 'alex.worker@example.com',
      entitlements: ['fieldcompanion.profile.view'],
      personId: 'person-123',
      tenantDisplayName: 'Acme Logistics',
      tenantId: 'tenant-789',
      tenantRoleKey: 'worker',
      tenantSlug: 'acme-logistics',
      userId: 'user-456',
    },
    accessToken: 'token',
    meQuery: { data: { ok: true } },
  })),
}))

vi.mock('../hooks/useOfflineQueue', () => ({
  useOfflineQueue: vi.fn(() => ({
    lastSyncError: null,
    lastSyncedAt: null,
    pendingCount: 0,
  })),
}))

vi.mock('../lib/pushNotifications', () => ({
  getPushPermissionState: vi.fn(() => 'granted'),
  isWebPushSupported: vi.fn(() => true),
  pushReadinessLabel: vi.fn(() => 'Ready'),
}))

vi.mock('../lib/fieldInbox', () => ({
  formatWhen: vi.fn(() => 'formatted'),
}))

describe('ProfilePage', () => {
  it('keeps raw person identifiers in advanced session details', () => {
    render(<ProfilePage />)

    expect(screen.getByText('Alex Worker')).toBeInTheDocument()
    expect(screen.getByText('person-123')).not.toBeVisible()

    fireEvent.click(screen.getByText('Advanced session details'))

    expect(screen.getByText('person-123')).toBeVisible()
    expect(screen.getByText('Session ID')).toBeInTheDocument()
  })
})
