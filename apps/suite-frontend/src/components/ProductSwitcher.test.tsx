import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../api/nexarrClient'
import { ProductSwitcher } from './ProductSwitcher'

vi.mock('../api/nexarrClient', () => ({
  getNavigation: vi.fn(),
}))

vi.mock('../auth/AuthProvider', () => ({
  useAuth: () => ({
    me: {
      tenantId: 'tenant-1',
      entitlements: ['staffarr'],
    },
  }),
}))

vi.mock('../hooks/useProductLaunch', () => ({
  useProductLaunch: () => ({
    isPending: false,
    mutate: vi.fn(),
    isError: true,
    error: new Error('launch failed'),
  }),
}))

function renderSwitcher() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <MemoryRouter initialEntries={['/app/staffarr']}>
        <ProductSwitcher />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('ProductSwitcher', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows launch errors in shared callout', async () => {
    const user = userEvent.setup()
    vi.mocked(nexarr.getNavigation).mockResolvedValue({
      tenantId: 'tenant-1',
      products: [
        {
          productKey: 'staffarr',
          displayName: 'StaffArr',
          routePath: '/app/staffarr',
          sortOrder: 1,
          isCurrent: true,
          surfaces: [],
        },
      ],
    })

    renderSwitcher()

    expect(vi.mocked(nexarr.getNavigation)).toHaveBeenCalledWith('staffarr')
    await user.click(await screen.findByRole('button', { name: /StaffArr/ }))
    expect(screen.getByRole('menuitem', { name: /StaffArr/ })).toHaveAttribute(
      'aria-current',
      'true',
    )
    expect(await screen.findByText('launch failed')).toBeTruthy()
    expect(screen.getByRole('alert')).toBeTruthy()
  })
})
