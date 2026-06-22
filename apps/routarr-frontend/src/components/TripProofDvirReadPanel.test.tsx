import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { TripProofDvirReadPanel } from './TripProofDvirReadPanel'

vi.mock('@stl/shared-ui', () => ({
  ApiErrorCallout: ({
    message,
    retryLabel,
    onRetry,
  }: {
    message: string
    retryLabel?: string
    onRetry?: () => void
  }) => (
    <div>
      <p>{message}</p>
      {retryLabel && onRetry ? <button type="button" onClick={onRetry}>{retryLabel}</button> : null}
    </div>
  ),
  AdvancedReferenceField: ({
    label,
    value,
    onChange,
    testId,
  }: {
    label?: string
    value: string
    onChange: (value: string) => void
    testId?: string
  }) => (
    <label>
      {label}
      <input aria-label={label} data-testid={testId} value={value} onChange={(event) => onChange(event.target.value)} />
    </label>
  ),
  StaticSearchPicker: ({
    label,
    value,
    options,
    onChange,
    placeholder,
    testId,
    disabled,
  }: {
    label?: string
    value: string
    options: Array<{ value: string; label: string }>
    onChange: (value: string) => void
    placeholder?: string
    testId?: string
    disabled?: boolean
  }) => {
    const query = value.trim().toLowerCase()
    const visibleOptions = query
      ? options.filter((option) =>
          option.label.toLowerCase().includes(query) || option.value.toLowerCase().includes(query),
        )
      : options

    return (
      <div data-testid={testId}>
        <label>
          {label}
          <input
            aria-label={label ?? placeholder ?? 'Static search picker'}
            value={value}
            onChange={(event) => onChange(event.target.value)}
            disabled={disabled}
          />
        </label>
        {visibleOptions.length > 0 ? (
          <div>
            {visibleOptions.map((option) => (
              <button key={option.value} type="button" onClick={() => onChange(option.value)}>
                {option.label}
              </button>
            ))}
          </div>
        ) : null}
      </div>
    )
  },
  getErrorMessage: (error: unknown, fallback: string) => (error instanceof Error ? error.message : fallback),
}))

vi.mock('../api/client', () => ({
  getTripExecutionSummary: vi.fn(),
  getTrips: vi.fn().mockResolvedValue([
    {
      tripId: 'trip-1',
      tripNumber: 'TR-001',
      title: 'North route',
      dispatchStatus: 'planned',
      assignedDriverPersonId: null,
      vehicleRefKey: null,
      scheduledStartAt: null,
      scheduledEndAt: null,
      loadCount: 0,
      createdByUserId: 'user-1',
      createdAt: '2026-05-27T08:00:00Z',
      updatedAt: '2026-05-27T08:00:00Z',
      assignedAt: null,
      dispatchedAt: null,
      startedAt: null,
      completedAt: null,
      cancelledAt: null,
      closedAt: null,
    },
  ]),
  downloadTripCaptureAttachment: vi.fn(),
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

async function selectTripFromPicker() {
  const tripInput = screen.getByLabelText('Trip')
  fireEvent.focus(tripInput)
  fireEvent.change(tripInput, { target: { value: 'TR-001' } })
  fireEvent.click(await screen.findByRole('button', { name: 'TR-001 · North route' }))
}

describe('TripProofDvirReadPanel', () => {
  afterEach(() => cleanup())

  it('loads execution summary for entered trip id', async () => {
    vi.mocked(client.getTripExecutionSummary).mockResolvedValue({
      tripId: 'trip-1',
      tripNumber: 'TR-001',
      dispatchStatus: 'completed',
      assignedDriverPersonId: 'person-1',
      closedAt: null,
      proofs: [
        {
          proofId: 'proof-1',
          tripId: 'trip-1',
          proofType: 'pickup',
          capturedByPersonId: 'person-1',
          vehicleRefKey: 'VEH-1',
          referenceKey: 'BOL-9',
          notes: 'Signed',
          reviewStatus: 'pending_review',
          reviewedByPersonId: null,
          reviewedAt: null,
          reviewNotes: '',
          capturedAt: new Date().toISOString(),
          createdAt: new Date().toISOString(),
          attachments: [
            {
              attachmentId: 'attach-1',
              tripId: 'trip-1',
              subjectType: 'proof',
              subjectId: 'proof-1',
              attachmentKind: 'photo',
              fileName: 'pickup.jpg',
              contentType: 'image/jpeg',
              sizeBytes: 1024,
              notes: '',
              capturedByPersonId: 'person-1',
              createdAt: new Date().toISOString(),
            },
          ],
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
          attachments: [],
        },
      ],
      hasPreTripDvir: true,
      hasPostTripDvir: false,
    })

    renderPanel()
    await selectTripFromPicker()
    fireEvent.click(screen.getByRole('button', { name: 'Load execution' }))

    expect(await screen.findByTestId('proof-row-proof-1')).toBeTruthy()
    expect(screen.getByTestId('trip-execution-summary-header')).toHaveTextContent('TR-001')
    expect(screen.getByTestId('dvir-row-dvir-1')).toBeTruthy()
    await waitFor(() =>
      expect(client.getTripExecutionSummary).toHaveBeenCalledWith('token', 'trip-1'),
    )
  })

  it('downloads proof attachment when dispatcher clicks attachment link', async () => {
    vi.mocked(client.getTripExecutionSummary).mockResolvedValue({
      tripId: 'trip-1',
      tripNumber: 'TR-002',
      dispatchStatus: 'in_progress',
      assignedDriverPersonId: 'person-1',
      closedAt: null,
      proofs: [
        {
          proofId: 'proof-2',
          tripId: 'trip-1',
          proofType: 'pickup',
          capturedByPersonId: 'person-1',
          vehicleRefKey: 'VEH-1',
          referenceKey: 'BOL-10',
          notes: '',
          reviewStatus: 'pending_review',
          reviewedByPersonId: null,
          reviewedAt: null,
          reviewNotes: '',
          capturedAt: new Date().toISOString(),
          createdAt: new Date().toISOString(),
          attachments: [
            {
              attachmentId: 'attach-2',
              tripId: 'trip-1',
              subjectType: 'proof',
              subjectId: 'proof-2',
              attachmentKind: 'photo',
              fileName: 'dock-photo.jpg',
              contentType: 'image/jpeg',
              sizeBytes: 1024,
              notes: '',
              capturedByPersonId: 'person-1',
              createdAt: new Date().toISOString(),
            },
          ],
        },
      ],
      dvirInspections: [],
      hasPreTripDvir: false,
      hasPostTripDvir: false,
    })

    renderPanel()
    await selectTripFromPicker()
    fireEvent.click(screen.getByRole('button', { name: 'Load execution' }))

    const downloadButton = await screen.findByTestId('proof-attachment-attach-2')
    fireEvent.click(downloadButton)

    await waitFor(() =>
      expect(client.downloadTripCaptureAttachment).toHaveBeenCalledWith(
        'token',
        'trip-1',
        'proof',
        'proof-2',
        'attach-2',
        'dock-photo.jpg',
      ),
    )
  })

  it('downloads proof document attachment when dispatcher clicks attachment link', async () => {
    vi.mocked(client.getTripExecutionSummary).mockResolvedValue({
      tripId: 'trip-1',
      tripNumber: 'TR-004',
      dispatchStatus: 'completed',
      assignedDriverPersonId: 'person-1',
      closedAt: '2026-05-28T12:00:00.000Z',
      proofs: [
        {
          proofId: 'proof-3',
          tripId: 'trip-1',
          proofType: 'pickup',
          capturedByPersonId: 'person-1',
          vehicleRefKey: 'VEH-1',
          referenceKey: 'BOL-11',
          notes: '',
          reviewStatus: 'pending_review',
          reviewedByPersonId: null,
          reviewedAt: null,
          reviewNotes: '',
          capturedAt: new Date().toISOString(),
          createdAt: new Date().toISOString(),
          attachments: [
            {
              attachmentId: 'attach-doc-1',
              tripId: 'trip-1',
              subjectType: 'proof',
              subjectId: 'proof-3',
              attachmentKind: 'document',
              fileName: 'pickup-bol.pdf',
              contentType: 'application/pdf',
              sizeBytes: 2048,
              notes: '',
              capturedByPersonId: 'person-1',
              createdAt: new Date().toISOString(),
            },
          ],
        },
      ],
      dvirInspections: [],
      hasPreTripDvir: false,
      hasPostTripDvir: false,
    })

    renderPanel()
    await selectTripFromPicker()
    fireEvent.click(screen.getByRole('button', { name: 'Load execution' }))

    const downloadButton = await screen.findByTestId('proof-attachment-attach-doc-1')
    fireEvent.click(downloadButton)

    await waitFor(() =>
      expect(client.downloadTripCaptureAttachment).toHaveBeenCalledWith(
        'token',
        'trip-1',
        'proof',
        'proof-3',
        'attach-doc-1',
        'pickup-bol.pdf',
      ),
    )
  })

  it('downloads DVIR attachment when dispatcher clicks attachment link', async () => {
    vi.mocked(client.getTripExecutionSummary).mockResolvedValue({
      tripId: 'trip-1',
      tripNumber: 'TR-003',
      dispatchStatus: 'in_progress',
      assignedDriverPersonId: 'person-1',
      closedAt: null,
      proofs: [],
      dvirInspections: [
        {
          dvirId: 'dvir-2',
          tripId: 'trip-1',
          phase: 'pre_trip',
          vehicleRefKey: 'VEH-1',
          result: 'pass',
          odometerReading: 15000,
          defectNotes: '',
          submittedByPersonId: 'person-1',
          submittedAt: new Date().toISOString(),
          attachments: [
            {
              attachmentId: 'attach-dvir-1',
              tripId: 'trip-1',
              subjectType: 'dvir',
              subjectId: 'dvir-2',
              attachmentKind: 'photo',
              fileName: 'pretrip-inspection.jpg',
              contentType: 'image/jpeg',
              sizeBytes: 1024,
              notes: '',
              capturedByPersonId: 'person-1',
              createdAt: new Date().toISOString(),
            },
          ],
        },
      ],
      hasPreTripDvir: true,
      hasPostTripDvir: false,
    })

    renderPanel()
    await selectTripFromPicker()
    fireEvent.click(screen.getByRole('button', { name: 'Load execution' }))

    const downloadButton = await screen.findByTestId('dvir-attachment-attach-dvir-1')
    fireEvent.click(downloadButton)

    await waitFor(() =>
      expect(client.downloadTripCaptureAttachment).toHaveBeenCalledWith(
        'token',
        'trip-1',
        'dvir',
        'dvir-2',
        'attach-dvir-1',
        'pretrip-inspection.jpg',
      ),
    )
  })

  it('shows retryable error callout when execution summary query fails', async () => {
    vi.mocked(client.getTripExecutionSummary).mockRejectedValueOnce(
      new Error('execution summary unavailable'),
    )

    renderPanel()
    await selectTripFromPicker()
    fireEvent.click(screen.getByRole('button', { name: 'Load execution' }))

    expect(await screen.findByText('execution summary unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry execution summary' })).toBeTruthy()
  })
})
