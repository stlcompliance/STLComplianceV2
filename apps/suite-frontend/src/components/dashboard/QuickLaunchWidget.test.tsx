import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { QuickLaunchWidget } from './QuickLaunchWidget'

vi.mock('../../hooks/useProductLaunch', () => ({
  useProductLaunch: () => ({
    isPending: false,
    mutate: vi.fn(),
    isError: true,
    error: new Error('launch failed'),
  }),
}))

describe('QuickLaunchWidget', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows launch mutation errors in shared callout', async () => {
    render(
      <MemoryRouter>
        <QuickLaunchWidget
          entitlements={['staffarr']}
          navigationProducts={[
            {
              productKey: 'staffarr',
              displayName: 'StaffArr',
              routePath: '/app/staffarr',
              sortOrder: 1,
              surfaces: [],
            },
          ]}
        />
      </MemoryRouter>,
    )

    expect(await screen.findByText('launch failed')).toBeTruthy()
    expect(screen.getByRole('alert')).toBeTruthy()
  })
})
