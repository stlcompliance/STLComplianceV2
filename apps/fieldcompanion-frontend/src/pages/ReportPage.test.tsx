import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ReportPage } from './ReportPage'

const mutateAsync = vi.fn()

vi.mock('../hooks/useFieldCompanionWorkspace', () => ({
  useFieldCompanionWorkspace: vi.fn(() => ({
    session: {
      accessToken: 'token',
    },
    meQuery: {
      data: {
        fieldProductKeys: ['staffarr', 'maintainarr'],
      },
    },
  })),
}))

vi.mock('../hooks/useFieldCompanionProductLaunch', () => ({
  useFieldCompanionProductLaunch: vi.fn(() => ({
    isPending: false,
    mutateAsync,
  })),
}))

describe('ReportPage', () => {
  afterEach(() => {
    cleanup()
    mutateAsync.mockReset()
  })

  it('renders report shortcuts for available workspaces', () => {
    render(<ReportPage />)

    expect(screen.getByText('Report')).toBeInTheDocument()
    expect(screen.getByText('Incident report')).toBeInTheDocument()
    expect(screen.getByText('Maintenance note')).toBeInTheDocument()
    expect(screen.queryByText('Quality / CAPA')).not.toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Open Incident' }))

    expect(mutateAsync).toHaveBeenCalledWith('staffarr')
  })
})
