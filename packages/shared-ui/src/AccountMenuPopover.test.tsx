import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { AccountMenuPopover } from './AccountMenuPopover'

describe('AccountMenuPopover', () => {
  afterEach(() => {
    cleanup()
  })

  it('opens a compact account menu with preferences and sign out actions', () => {
    const onSignOut = vi.fn()

    render(
      <MemoryRouter>
        <AccountMenuPopover
          displayName="Demo Admin"
          subtitle="STL Demo Tenant"
          preferencesHref="/app/preferences"
          onSignOut={onSignOut}
        />
      </MemoryRouter>,
    )

    expect(screen.queryByRole('menu')).not.toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: /demo admin/i }))

    expect(screen.getByRole('menu', { name: 'Account menu' })).toBeInTheDocument()
    expect(screen.getByRole('menuitem', { name: 'Preferences' })).toHaveAttribute(
      'href',
      '/app/preferences',
    )

    fireEvent.click(screen.getByRole('menuitem', { name: 'Sign out' }))
    expect(onSignOut).toHaveBeenCalledTimes(1)
  })

  it('still exposes Preferences when sign out is unavailable', () => {
    render(
      <MemoryRouter>
        <AccountMenuPopover displayName="Demo Admin" preferencesHref="/app/preferences" />
      </MemoryRouter>,
    )

    fireEvent.click(screen.getByRole('button', { name: /demo admin/i }))

    expect(screen.getByRole('menuitem', { name: 'Preferences' })).toHaveAttribute(
      'href',
      '/app/preferences',
    )
    expect(screen.queryByRole('menuitem', { name: 'Sign out' })).not.toBeInTheDocument()
  })
})
