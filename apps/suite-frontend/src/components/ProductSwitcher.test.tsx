import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ProductSwitcher } from './ProductSwitcher'

vi.mock('../auth/AuthProvider', () => ({
  useAuth: () => ({
    me: {
      isPlatformAdmin: false,
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

function renderSwitcher(path = '/app/staffarr') {
  render(
    <MemoryRouter initialEntries={[path]}>
      <ProductSwitcher />
    </MemoryRouter>,
  )
}

describe('ProductSwitcher', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows launch errors in the shared callout', async () => {
    const user = userEvent.setup()

    renderSwitcher()

    await user.click(await screen.findByRole('button', { name: /StaffArr/ }))
    expect(screen.getByRole('menuitem', { name: /StaffArr/ })).toHaveAttribute(
      'aria-current',
      'true',
    )
    expect(screen.getByRole('menuitem', { name: /Field Companion/ })).toBeTruthy()
    expect(await screen.findByText('launch failed')).toBeTruthy()
    expect(screen.getByRole('alert')).toBeTruthy()
  })

  it('hides Compliance Core from non-admin users', async () => {
    const user = userEvent.setup()

    renderSwitcher()

    await user.click(await screen.findByRole('button', { name: /StaffArr/ }))

    expect(screen.getByRole('menuitem', { name: /RecordArr/ })).toBeTruthy()
    expect(screen.getByRole('menuitem', { name: /ReportArr/ })).toBeTruthy()
    expect(screen.getByRole('menuitem', { name: /AssurArr/ })).toBeTruthy()
    expect(screen.queryByRole('menuitem', { name: /Compliance Core/ })).not.toBeInTheDocument()
  })
})
