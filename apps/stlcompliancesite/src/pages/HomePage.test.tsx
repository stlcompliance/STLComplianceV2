import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { HomePage } from './HomePage'

describe('HomePage', () => {
  it('renders ARR positioning and product grid', () => {
    render(
      <MemoryRouter>
        <HomePage />
      </MemoryRouter>,
    )
    expect(screen.getByRole('heading', { name: /Compliance should not live/i })).toBeInTheDocument()
    expect(screen.getAllByText(/Adaptive Risk Reduction/i).length).toBeGreaterThan(0)
    expect(screen.getByRole('link', { name: /StaffArr/i })).toBeInTheDocument()
  })
})
