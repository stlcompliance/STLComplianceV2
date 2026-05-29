import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { LaunchFailurePanel } from './LaunchFailurePanel'

vi.mock('../../auth/AuthProvider', () => ({
  useAuth: () => ({
    me: { isPlatformAdmin: true },
  }),
}))

describe('LaunchFailurePanel', () => {
  afterEach(() => {
    cleanup()
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
    expect(screen.getByText(/Reason code: profile_missing/i)).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Open launch diagnostics' })).toHaveAttribute(
      'href',
      '/app/platform-admin/launch',
    )
  })
})
