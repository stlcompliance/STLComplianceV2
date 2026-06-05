import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { FieldScanPanel } from './FieldScanPanel'
import * as client from '../api/client'

vi.mock('../api/client', () => ({
  resolveFieldCompanionScan: vi.fn(),
}))

describe('FieldScanPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders resolve failures in shared callout', async () => {
    vi.mocked(client.resolveFieldCompanionScan).mockRejectedValueOnce(new Error('scan resolve failed'))

    render(<FieldScanPanel accessToken="token" onResolved={vi.fn()} />)

    fireEvent.change(screen.getByTestId('fieldcompanion-scan-manual-input'), {
      target: { value: 'trainarr:assignment:1' },
    })
    fireEvent.click(screen.getByTestId('fieldcompanion-scan-submit'))

    expect(await screen.findByText('scan resolve failed')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-scan-error')).toBeInTheDocument()
  })
})
