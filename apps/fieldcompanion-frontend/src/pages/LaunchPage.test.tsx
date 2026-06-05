import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, describe, expect, it } from 'vitest'

import { LaunchPage } from './LaunchPage'

describe('LaunchPage', () => {
  afterEach(() => {
    cleanup()
  })

  it('shows callout when handoff code is missing', async () => {
    render(
      <MemoryRouter initialEntries={['/launch']}>
        <Routes>
          <Route path="/launch" element={<LaunchPage />} />
        </Routes>
      </MemoryRouter>,
    )

    expect(await screen.findByText('Missing handoff code. Launch the Field Companion app from the suite.')).toBeInTheDocument()
    expect(screen.getByRole('alert')).toBeInTheDocument()
  })
})
