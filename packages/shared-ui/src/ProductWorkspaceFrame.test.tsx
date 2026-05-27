import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { ProductWorkspaceFrame } from './ProductWorkspaceFrame'

describe('ProductWorkspaceFrame', () => {
  it('shows sign-in guidance when no session is present', () => {
    render(
      <ProductWorkspaceFrame productName="StaffArr" workspaceSession={null}>
        <p>Workspace content</p>
      </ProductWorkspaceFrame>,
    )

    expect(screen.getByText('Sign in required')).toBeInTheDocument()
    expect(screen.getByText(/Launch StaffArr from the STL Compliance suite/i)).toBeInTheDocument()
    expect(screen.queryByText('Workspace content')).not.toBeInTheDocument()
  })

  it('renders shell chrome when session bootstrap succeeds', () => {
    render(
      <MemoryRouter>
        <ProductWorkspaceFrame
          productName="StaffArr"
          workspaceSubtitle="People, org, and readiness"
          workspaceSession={{
            userDisplayName: 'Demo Admin',
            tenantDisplayName: 'demo-stl',
          }}
        >
          <p>Workspace content</p>
        </ProductWorkspaceFrame>
      </MemoryRouter>,
    )

    expect(screen.getByText('Demo Admin')).toBeInTheDocument()
    expect(screen.getByText('demo-stl')).toBeInTheDocument()
    expect(screen.getByText('Workspace content')).toBeInTheDocument()
  })

  it('shows entitlement denial messaging', () => {
    render(
      <ProductWorkspaceFrame
        productName="TrainArr"
        workspaceSession={null}
        bootstrapError="forbidden"
      >
        <p>Workspace content</p>
      </ProductWorkspaceFrame>,
    )

    expect(screen.getByText('Access denied')).toBeInTheDocument()
    expect(screen.getByText(/not entitled to TrainArr/i)).toBeInTheDocument()
  })
})
