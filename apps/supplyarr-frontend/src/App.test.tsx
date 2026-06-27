import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { Outlet } from 'react-router-dom'

import App from './App'

vi.mock('./layouts/ProductWorkspaceLayout', () => ({
  ProductWorkspaceLayout: () => <Outlet />,
}))

vi.mock('./pages/dashboard/DashboardPage', () => ({
  DashboardPage: () => <p>SupplyArr dashboard route</p>,
}))

describe('SupplyArr app routes', () => {
  afterEach(() => {
    cleanup()
  })

  it('redirects the workspace root to the dashboard', async () => {
    window.history.pushState({}, '', '/')

    render(<App />)

    await waitFor(() => expect(screen.getByText('SupplyArr dashboard route')).toBeInTheDocument())
  })

  it('routes launch traffic to the launch page', async () => {
    window.history.pushState({}, '', '/launch')

    render(<App />)

    await waitFor(() =>
      expect(
        screen.getByText('Missing handoff code. Launch SupplyArr from the suite.'),
      ).toBeInTheDocument(),
    )
  })
})
