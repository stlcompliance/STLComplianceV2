import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { DriverTimeTrackingPanel } from './DriverTimeTrackingPanel'
import { fromDatetimeLocalValue } from '../lib/availabilityDateTime'

vi.mock('../api/client', () => ({
  getDriverPortalTimeTracking: vi.fn(),
  createDriverPortalTimeEntry: vi.fn(),
  updateDriverPortalTimeEntry: vi.fn(),
}))

import * as client from '../api/client'

function renderPanel() {
  const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={qc}>
      <DriverTimeTrackingPanel accessToken="token" />
    </QueryClientProvider>,
  )
}

describe('DriverTimeTrackingPanel', () => {
  beforeEach(() => {
    vi.mocked(client.getDriverPortalTimeTracking).mockResolvedValue({
      date: '2026-05-27',
      windowStart: '2026-05-27T00:00:00Z',
      windowEnd: '2026-05-28T00:00:00Z',
      summary: {
        entryCount: 1,
        onDutyMinutes: 240,
        offDutyMinutes: 0,
        breakMinutes: 30,
        openEntryCount: 0,
        workdayStartAt: '2026-05-27T08:00:00Z',
        workdayEndAt: '2026-05-27T12:00:00Z',
        shortHaulCandidate: true,
        shortHaulException: false,
        summaryNote: 'Operational time logs are within the short-haul-style threshold.',
      },
      entries: [
        {
          entryId: 'entry-1',
          personId: 'person-1',
          entryType: 'on_duty',
          startsAt: '2026-05-27T08:00:00Z',
          endsAt: '2026-05-27T12:00:00Z',
          notes: 'Morning route',
          editReason: 'Initial entry',
          isOpen: false,
          durationMinutes: 240,
          createdByUserId: 'user-1',
          updatedByUserId: null,
          createdAt: '2026-05-27T08:00:00Z',
          updatedAt: '2026-05-27T08:00:00Z',
        },
      ],
      generatedAt: '2026-05-27T13:00:00Z',
    })
    vi.mocked(client.createDriverPortalTimeEntry).mockResolvedValue({
      entryId: 'entry-new',
      personId: 'person-1',
      entryType: 'break',
      startsAt: '2026-05-27T14:00:00Z',
      endsAt: null,
      notes: 'Lunch',
      editReason: 'Created via driver portal time tracking',
      isOpen: true,
      durationMinutes: 0,
      createdByUserId: 'user-1',
      updatedByUserId: null,
      createdAt: '2026-05-27T14:00:00Z',
      updatedAt: '2026-05-27T14:00:00Z',
    })
    vi.mocked(client.updateDriverPortalTimeEntry).mockResolvedValue({
      entryId: 'entry-1',
      personId: 'person-1',
      entryType: 'on_duty',
      startsAt: '2026-05-27T08:00:00Z',
      endsAt: '2026-05-27T12:30:00Z',
      notes: 'Morning run corrected',
      editReason: 'Adjusted end time after review',
      isOpen: false,
      durationMinutes: 270,
      createdByUserId: 'user-1',
      updatedByUserId: 'user-1',
      createdAt: '2026-05-27T08:00:00Z',
      updatedAt: '2026-05-27T13:30:00Z',
    })
  })

  afterEach(() => {
    vi.clearAllMocks()
    cleanup()
  })

  it('renders summary and lets the driver add a time entry', async () => {
    renderPanel()

    expect(await screen.findByText('Time tracking')).toBeTruthy()
    expect(screen.getByTestId('driver-time-summary-on-duty')).toHaveTextContent('4h')
    expect(screen.getByText('Short-haul candidate')).toBeTruthy()
    expect(screen.getByText('Morning route')).toBeTruthy()

    fireEvent.change(screen.getByLabelText('Type'), { target: { value: 'break' } })
    fireEvent.change(screen.getByLabelText('Start'), { target: { value: '2026-05-27T14:00' } })
    fireEvent.change(screen.getByLabelText('End'), { target: { value: '' } })
    fireEvent.change(screen.getByLabelText('Notes'), { target: { value: 'Lunch' } })
    fireEvent.click(screen.getByRole('button', { name: 'Add time entry' }))

    await waitFor(() =>
      expect(client.createDriverPortalTimeEntry).toHaveBeenCalledWith('token', {
        entryType: 'break',
        startsAt: fromDatetimeLocalValue('2026-05-27T14:00'),
        endsAt: null,
        notes: 'Lunch',
      }),
    )
  })

  it('allows correcting an existing entry with an edit reason', async () => {
    renderPanel()

    expect(await screen.findByText('Morning route')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Edit' }))

    fireEvent.change(screen.getByLabelText('Edit reason'), {
      target: { value: 'Adjusted end time after review' },
    })
    fireEvent.change(screen.getByLabelText('Edit end'), { target: { value: '2026-05-27T12:30' } })
    fireEvent.click(screen.getByRole('button', { name: 'Save correction' }))

    await waitFor(() =>
      expect(client.updateDriverPortalTimeEntry).toHaveBeenCalledWith('token', 'entry-1', {
        entryType: 'on_duty',
        startsAt: new Date('2026-05-27T08:00:00Z').toISOString(),
        endsAt: fromDatetimeLocalValue('2026-05-27T12:30'),
        notes: 'Morning route',
        editReason: 'Adjusted end time after review',
      }),
    )
  })
})
