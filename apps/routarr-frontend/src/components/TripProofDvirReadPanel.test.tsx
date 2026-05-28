import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { TripProofDvirReadPanel } from './TripProofDvirReadPanel'

vi.mock('../api/client', () => ({
  getTripExecutionSummary: vi.fn(),
}))

import * as client from '../api/client'

function renderPanel() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={qc}>
      <TripProofDvirReadPanel accessToken="token" />
    </QueryClientProvider>,
  )
}

describe('TripProofDvirReadPanel', () => {
  afterEach(() => cleanup())

  it('loads execution summary for entered trip id', async () => {
    vi.mocked(client.getTripExecutionSummary).mockResolvedValue({
      tripId: 'trip-1',
      tripNumber: 'TR-001',
      assignedDriverPersonId: 'person-1',
      proofs: [
        {
          proofId: 'proof-1',
          tripId: 'trip-1',
          proofType: 'pickup',
          capturedByPersonId: 'person-1',
          vehicleRefKey: 'VEH-1',
          referenceKey: 'BOL-9',
          notes: 'Signed',
          capturedAt: new Date().toISOString(),
          createdAt: new Date().toISOString(),
        },
      ],
      dvirInspections: [
        {
          dvirId: 'dvir-1',
          tripId: 'trip-1',
          phase: 'pre_trip',
          vehicleRefKey: 'VEH-1',
          result: 'pass',
          odometerReading: 12000,
          defectNotes: '',
          submittedByPersonId: 'person-1',
          submittedAt: new Date().toISOString(),
        },
      ],
      hasPreTripDvir: true,
      hasPostTripDvir: false,
    })

    renderPanel()
    fireEvent.change(screen.getByPlaceholderText('Paste trip GUID'), {
      target: { value: 'trip-1' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Load execution' }))

    expect(await screen.findByTestId('proof-row-proof-1')).toBeTruthy()
    expect(screen.getByText(/TR-001/)).toBeTruthy()
    expect(screen.getByTestId('dvir-row-dvir-1')).toBeTruthy()
    await waitFor(() =>
      expect(client.getTripExecutionSummary).toHaveBeenCalledWith('token', 'trip-1'),
    )
  })
})
