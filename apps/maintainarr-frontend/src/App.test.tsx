import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { Outlet } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import App from './App'

vi.mock('./layouts/ProductWorkspaceLayout', () => ({
  ProductWorkspaceLayout: () => <Outlet />,
}))

vi.mock('./pages/overview/OverviewPage', () => ({
  OverviewPage: () => <p>MaintainArr overview route</p>,
}))

describe('MaintainArr app routes', () => {
  afterEach(() => {
    cleanup()
  })

  it('redirects the workspace root to overview', async () => {
    window.history.pushState({}, '', '/')

    render(<App />)

    await waitFor(() => expect(screen.getByText('MaintainArr overview route')).toBeInTheDocument())
  })

  it('routes launch traffic to the launch page', async () => {
    window.history.pushState({}, '', '/launch')

    render(<App />)

    await waitFor(() =>
      expect(
        screen.getByText('Missing handoff code. Launch MaintainArr from the suite.'),
      ).toBeInTheDocument(),
    )
  })
})
