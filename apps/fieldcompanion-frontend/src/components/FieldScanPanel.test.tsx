import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { resolveFieldCompanionScan } from '../api/client'
import type { FieldCompanionScanResolveResponse } from '../api/types'
import { FieldScanPanel } from './FieldScanPanel'

vi.mock('../api/client', () => ({
  resolveFieldCompanionScan: vi.fn(),
}))

const resolvedResponse: FieldCompanionScanResolveResponse = {
  outcome: 'resolved',
  reasonCode: null,
  reasonMessage: null,
  taskKey: 'trainarr:assignment:aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  productKey: 'trainarr',
  taskType: 'training_assignment',
  title: 'Hazmat annual',
  subtitle: 'Assignment 1',
  status: 'assigned',
  deepLinkPath: '/assignments/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  deepLinkUrl: 'https://trainarr.example/assignments/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  blockedReason: null,
}

describe('FieldScanPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('normalizes manual scan payloads before resolving and shows scan context', async () => {
    vi.mocked(resolveFieldCompanionScan).mockResolvedValueOnce(resolvedResponse)

    render(<FieldScanPanel accessToken="token" onResolved={vi.fn()} />)

    fireEvent.change(screen.getByTestId('fieldcompanion-scan-manual-input'), {
      target: { value: '/assignments/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa' },
    })
    fireEvent.click(screen.getByTestId('fieldcompanion-scan-submit'))

    expect(await screen.findByTestId('fieldcompanion-scan-result')).toHaveTextContent('Hazmat annual')
    expect(resolveFieldCompanionScan).toHaveBeenCalledWith('token', {
      scannedValue: 'trainarr:assignment:aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
      symbology: 'manual',
    })
    expect(screen.getByTestId('fieldcompanion-scan-context')).toHaveTextContent('manual entry')
    expect(screen.getByTestId('fieldcompanion-scan-context')).toHaveTextContent(
      'Code normalized and sent to the permitted product resolver.',
    )
    expect(screen.getByTestId('fieldcompanion-scan-context')).not.toHaveTextContent(
      'trainarr:assignment:aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    )
    expect(screen.getByTestId('fieldcompanion-scan-result')).not.toHaveTextContent(
      'trainarr:assignment:aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    )
    expect(screen.getByTestId('fieldcompanion-scan-open-deeplink')).toHaveAttribute(
      'href',
      'https://trainarr.example/assignments/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    )
  })

  it('ignores duplicate scans submitted in quick succession', async () => {
    vi.mocked(resolveFieldCompanionScan).mockResolvedValue(resolvedResponse)

    render(<FieldScanPanel accessToken="token" onResolved={vi.fn()} />)

    fireEvent.change(screen.getByTestId('fieldcompanion-scan-manual-input'), {
      target: { value: 'stl-field-task:trainarr:assignment:aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa' },
    })
    fireEvent.click(screen.getByTestId('fieldcompanion-scan-submit'))

    expect(await screen.findByTestId('fieldcompanion-scan-result')).toBeInTheDocument()

    fireEvent.click(screen.getByTestId('fieldcompanion-scan-submit'))

    expect(resolveFieldCompanionScan).toHaveBeenCalledTimes(1)
    expect(screen.getByTestId('fieldcompanion-scan-duplicate')).toHaveTextContent(
      'Duplicate scan ignored',
    )
  })

  it('renders resolve failures in shared callout', async () => {
    vi.mocked(resolveFieldCompanionScan).mockRejectedValueOnce(new Error('scan resolve failed'))

    render(<FieldScanPanel accessToken="token" onResolved={vi.fn()} />)

    fireEvent.change(screen.getByTestId('fieldcompanion-scan-manual-input'), {
      target: { value: 'trainarr:assignment:aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa' },
    })
    fireEvent.click(screen.getByTestId('fieldcompanion-scan-submit'))

    expect(await screen.findByText('scan resolve failed')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-scan-error')).toBeInTheDocument()
  })
})
