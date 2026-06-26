import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { LaunchFailurePanel } from './LaunchFailurePanel'

let mockMe: { isPlatformAdmin: boolean } = { isPlatformAdmin: true }

vi.mock('../../auth/AuthProvider', () => ({
  useAuth: () => ({
    me: mockMe,
  }),
}))

describe('LaunchFailurePanel', () => {
  afterEach(() => {
    cleanup()
    mockMe = { isPlatformAdmin: true }
  })

  it('renders friendly denial guidance for launch context', () => {
    render(
      <MemoryRouter>
        <LaunchFailurePanel
          productDisplayName="StaffArr"
          productKey="staffarr"
          context={{
            tenantId: 't',
            tenantSlug: 'demo',
            tenantDisplayName: 'Demo',
            userId: 'u',
            userEmail: 'a@b.c',
            productKey: 'staffarr',
            productDisplayName: 'StaffArr',
            baseLaunchUrl: '',
            launchUrl: '',
            canLaunch: false,
            denialReasonCode: 'profile_missing',
          }}
        />
      </MemoryRouter>,
    )

    expect(screen.getByTestId('launch-failure-panel')).toBeInTheDocument()
    expect(screen.getByText(/Launch profile missing/i)).toBeInTheDocument()
    expect(screen.getByText(/Code: profile_missing/i)).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Open launch diagnostics' })).toHaveAttribute(
      'href',
      '/app/platform-admin/launch',
    )
  })

  it('hides raw reason codes from non-platform-admin users', () => {
    mockMe = { isPlatformAdmin: false }

    render(
      <MemoryRouter>
        <LaunchFailurePanel
          productDisplayName="StaffArr"
          productKey="staffarr"
          context={{
            tenantId: 't',
            tenantSlug: 'demo',
            tenantDisplayName: 'Demo',
            userId: 'u',
            userEmail: 'a@b.c',
            productKey: 'staffarr',
            productDisplayName: 'StaffArr',
            baseLaunchUrl: '',
            launchUrl: '',
            canLaunch: false,
            denialReasonCode: 'launch.product_unavailable',
          }}
        />
      </MemoryRouter>,
    )

    expect(screen.getByText(/Product unavailable/i)).toBeInTheDocument()
    expect(screen.queryByText(/Code:/i)).not.toBeInTheDocument()
    expect(screen.queryByRole('link', { name: 'Open launch diagnostics' })).not.toBeInTheDocument()
  })
})
