import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { HomePage } from './HomePage'

vi.mock('../hooks/useDashboardData', () => ({
  useDashboardData: vi.fn(),
}))

import { useDashboardData } from '../hooks/useDashboardData'

describe('HomePage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows dashboard load error in callout', async () => {
    vi.mocked(useDashboardData).mockReturnValue({
      me: { displayName: 'Demo User', entitlements: [] },
      session: null,
      entitlements: [],
      tenants: [],
      navigationProducts: [],
      isLoading: false,
      error: new Error('dashboard unavailable'),
    } as never)

    render(<HomePage />)

    expect(await screen.findByText('dashboard unavailable')).toBeTruthy()
  })
})
