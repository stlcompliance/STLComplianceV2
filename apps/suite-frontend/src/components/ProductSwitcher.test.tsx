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
      entitlements: ['staffarr', 'fieldcompanion', 'recordarr', 'reportarr', 'assurarr', 'shared-worker'],
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
        {
          productKey: 'fieldcompanion',
          displayName: 'Field Companion',
          routePath: '/app/field-companion',
          sortOrder: 2,
          isCurrent: false,
          surfaces: [],
        },
        {
          productKey: 'recordarr',
          displayName: 'RecordArr',
          routePath: '/app/recordarr',
          sortOrder: 3,
          isCurrent: false,
          surfaces: [],
        },
        {
          productKey: 'reportarr',
          displayName: 'ReportArr',
          routePath: '/app/reportarr',
          sortOrder: 4,
          isCurrent: false,
          surfaces: [],
        },
        {
          productKey: 'assurarr',
          displayName: 'AssurArr',
          routePath: '/app/assurarr',
          sortOrder: 5,
          isCurrent: false,
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
    expect(screen.getByRole('menuitem', { name: /Field Companion/ })).toBeTruthy()
    expect(screen.getByRole('menuitem', { name: /RecordArr/ })).toBeTruthy()
    expect(screen.getByRole('menuitem', { name: /ReportArr/ })).toBeTruthy()
    expect(screen.getByRole('menuitem', { name: /AssurArr/ })).toBeTruthy()
    expect(await screen.findByText('launch failed')).toBeTruthy()
    expect(screen.getByRole('alert')).toBeTruthy()
  })

  it('hides unsupported navigation products so the suite switcher matches launched apps', async () => {
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
        {
          productKey: 'shared-worker',
          displayName: 'STL Shared Worker',
          routePath: '/app/shared-worker',
          sortOrder: 2,
          isCurrent: false,
          surfaces: [],
        },
      ],
    })

    renderSwitcher()

    await user.click(await screen.findByRole('button', { name: /StaffArr/ }))

    expect(screen.getByRole('menuitem', { name: /StaffArr/ })).toBeTruthy()
    expect(screen.queryByRole('menuitem', { name: /shared worker/i })).toBeNull()
  })
})
