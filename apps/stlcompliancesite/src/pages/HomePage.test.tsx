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
    expect(screen.getByRole('heading', { name: /Operational compliance/i })).toBeInTheDocument()
    expect(screen.getByText(/Adaptive Risk Reduction/i)).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /StaffArr/i })).toBeInTheDocument()
  })
})
