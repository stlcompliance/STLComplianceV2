import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { PrivacyPage } from './PrivacyPage'

describe('PrivacyPage', () => {
  it('describes launch-session information in the data categories', () => {
    render(
      <MemoryRouter>
        <PrivacyPage />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'Privacy Policy' })).toBeInTheDocument()
    expect(
      screen.getByText(/product launch, role, permission, and launch-session information/i),
    ).toBeInTheDocument()
    expect(
      screen.getByText(/manage tenants, roles, permissions, and product launch context/i),
    ).toBeInTheDocument()
  })
})
