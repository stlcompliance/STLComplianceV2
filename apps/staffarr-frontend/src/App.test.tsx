import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { Outlet } from 'react-router-dom'

import App from './App'

vi.mock('./layouts/ProductWorkspaceLayout', () => ({
  ProductWorkspaceLayout: () => <Outlet />,
}))

describe('StaffArr app routes', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders the timekeeping workspace page', async () => {
    window.history.pushState({}, '', '/timekeeping')

    render(<App />)

    expect(await screen.findByText('Time capture, review, and payroll readiness')).toBeTruthy()
    expect(
      screen.getByText(/StaffArr is now the source of truth for worker timekeeping, leave, attendance, timesheets, approvals/i),
    ).toBeTruthy()
  })

  it('renders the timesheet detail page route', async () => {
    window.history.pushState({}, '', '/timekeeping/timesheets/timesheet-1')

    render(<App />)

    expect(await screen.findByText('Worker timesheet workspace')).toBeTruthy()
    expect(
      screen.getByText(
        'This route is reserved for the worker-level timesheet detail experience, including approvals, exceptions, attestations, corrections, leave context, attendance context, and payroll-ready locking.',
      ),
    ).toBeTruthy()
  })
})
