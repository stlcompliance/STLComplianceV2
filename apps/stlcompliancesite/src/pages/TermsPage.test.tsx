import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { TermsPage } from './TermsPage'

describe('TermsPage', () => {
  it('describes product launch availability in the current platform model', () => {
    render(
      <MemoryRouter>
        <TermsPage />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'Terms and Conditions' })).toBeInTheDocument()
    expect(
      screen.getByRole('heading', { name: /Platform Access and Product Availability/ }),
    ).toBeInTheDocument()
    expect(
      screen.getByText(
        /Product launch availability follows active tenant membership and product operational state/i,
      ),
    ).toBeInTheDocument()
  })
})
