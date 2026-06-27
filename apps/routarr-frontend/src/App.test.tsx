import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { Outlet } from 'react-router-dom'

import App from './App'

vi.mock('./layouts/ProductWorkspaceLayout', () => ({
  ProductWorkspaceLayout: () => <Outlet />,
}))

vi.mock('./pages/dashboard/DashboardPage', () => ({
  DashboardPage: () => <p>RoutArr dashboard route</p>,
}))

describe('RoutArr app routes', () => {
  afterEach(() => {
    cleanup()
  })

  it('redirects the workspace root to the dashboard', async () => {
    window.history.pushState({}, '', '/')

    render(<App />)

    await waitFor(() => expect(screen.getByText('RoutArr dashboard route')).toBeInTheDocument())
  })

  it('routes launch traffic to the launch page', async () => {
    window.history.pushState({}, '', '/launch')

    render(<App />)

    await waitFor(() =>
      expect(
        screen.getByText('Missing handoff code. Launch RoutArr from the suite.'),
      ).toBeInTheDocument(),
    )
  })
})
