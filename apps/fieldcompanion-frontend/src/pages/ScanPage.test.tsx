import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ScanPage } from './ScanPage'

const mutateAsync = vi.fn()

vi.mock('../hooks/useFieldCompanionWorkspace', () => ({
  useFieldCompanionWorkspace: vi.fn(() => ({
    session: {
      accessToken: 'token',
    },
    accessToken: 'token',
    meQuery: {
      data: {
        displayName: 'Alex Worker',
        fieldProductKeys: ['recordarr'],
      },
      isError: false,
    },
  })),
}))

vi.mock('../hooks/useFieldCompanionProductLaunch', () => ({
  useFieldCompanionProductLaunch: vi.fn(() => ({
    isPending: false,
    mutateAsync,
  })),
}))

vi.mock('../components/FieldScanPanel', () => ({
  FieldScanPanel: ({ accessToken }: { accessToken: string }) => (
    <div data-testid="fieldcompanion-scan-panel">Scan token {accessToken}</div>
  ),
}))

describe('ScanPage', () => {
  afterEach(() => {
    cleanup()
    mutateAsync.mockReset()
  })

  it('renders the scan resolver and capture handoff', async () => {
    render(<ScanPage />)

    expect(await screen.findByText('Scan / capture')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-scan-panel')).toHaveTextContent('Scan token token')

    fireEvent.click(screen.getByRole('button', { name: 'Open RecordArr capture' }))

    expect(mutateAsync).toHaveBeenCalledWith('recordarr')
  })
})
