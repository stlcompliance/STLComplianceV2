import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { Outlet } from 'react-router-dom'

import App from './App'

vi.mock('./layouts/ProductWorkspaceLayout', () => ({
  ProductWorkspaceLayout: () => <Outlet />,
}))

vi.mock('./pages/HomePage', () => ({
  HomePage: () => <p>Field Companion home route</p>,
}))

describe('Field Companion app routes', () => {
  afterEach(() => {
    cleanup()
  })

  it('routes the workspace root to the home page', () => {
    window.history.pushState({}, '', '/')

    render(<App />)

    expect(screen.getByText('Field Companion home route')).toBeInTheDocument()
  })

  it('routes launch traffic to the launch page', async () => {
    window.history.pushState({}, '', '/launch')

    render(<App />)

    await waitFor(() =>
      expect(
        screen.getByText('Missing handoff code. Launch the Field Companion app from the suite.'),
      ).toBeInTheDocument(),
    )
  })
})
