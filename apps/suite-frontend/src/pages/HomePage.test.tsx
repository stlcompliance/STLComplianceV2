import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { HomePage } from './HomePage'

vi.mock('../components/nexarr/NexArrOverviewPanel', () => ({
  NexArrOverviewPanel: () => <div>Platform overview</div>,
}))
vi.mock('./LaunchPadPage', () => ({
  LaunchPadPage: () => <div>Launchpad</div>,
}))
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
      me: { displayName: 'Demo User', launchableProductKeys: ['nexarr', 'staffarr'], isPlatformAdmin: false },
      navigationProducts: [],
      isLoading: false,
      error: new Error('dashboard unavailable'),
    } as never)

    render(<HomePage />)

    expect(await screen.findByText('dashboard unavailable')).toBeTruthy()
  })

  it('shows the non-admin launchpad by default', () => {
    vi.mocked(useDashboardData).mockReturnValue({
      me: { displayName: 'Demo User', launchableProductKeys: ['nexarr', 'staffarr'], isPlatformAdmin: false },
      navigationProducts: [],
      isLoading: false,
      error: null,
    } as never)

    render(<HomePage />)

    expect(screen.getByText('Launchpad')).toBeTruthy()
  })

  it('keeps the admin dashboard for platform administrators', () => {
    vi.mocked(useDashboardData).mockReturnValue({
      me: { displayName: 'Demo User', launchableProductKeys: ['nexarr', 'staffarr'], isPlatformAdmin: true },
      navigationProducts: [],
      isLoading: false,
      error: null,
    } as never)

    render(<HomePage />)

    expect(screen.getByText('Platform overview')).toBeTruthy()
  })
})
