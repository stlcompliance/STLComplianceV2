import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { Outlet } from 'react-router-dom'

import App from './App'

vi.mock('./layouts/ProductWorkspaceLayout', () => ({
  ProductWorkspaceLayout: () => <Outlet />,
}))

vi.mock('./pages/my-training/MyTrainingPage', () => ({
  MyTrainingPage: () => <p>TrainArr my training route</p>,
}))

describe('TrainArr app routes', () => {
  afterEach(() => {
    cleanup()
  })

  it('redirects the workspace root to my training', async () => {
    window.history.pushState({}, '', '/')

    render(<App />)

    await waitFor(() => expect(screen.getByText('TrainArr my training route')).toBeInTheDocument())
  })

  it('routes launch traffic to the launch page', async () => {
    window.history.pushState({}, '', '/launch')

    render(<App />)

    await waitFor(() =>
      expect(screen.getByText('Missing handoff code. Launch TrainArr from the suite.')).toBeInTheDocument(),
    )
  })
})
