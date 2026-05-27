import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { BulkDispatchPanel } from './BulkDispatchPanel'

const { previewBulkDispatch, applyBulkDispatch } = vi.hoisted(() => ({
  previewBulkDispatch: vi.fn(),
  applyBulkDispatch: vi.fn(),
}))

vi.mock('../api/client', () => ({
  getTrips: vi.fn().mockResolvedValue([
    {
      tripId: '11111111-1111-1111-1111-111111111111',
      tripNumber: 'TR-1',
      title: 'North run',
      dispatchStatus: 'planned',
      assignedDriverPersonId: null,
      vehicleRefKey: null,
      scheduledStartAt: '2026-05-27T10:00:00Z',
      scheduledEndAt: '2026-05-27T13:00:00Z',
      loadCount: 0,
      createdByUserId: 'user-1',
      createdAt: '2026-05-27T08:00:00Z',
      updatedAt: '2026-05-27T08:00:00Z',
      assignedAt: null,
      dispatchedAt: null,
      startedAt: null,
      completedAt: null,
      cancelledAt: null,
    },
    {
      tripId: '22222222-2222-2222-2222-222222222222',
      tripNumber: 'TR-2',
      title: 'South run',
      dispatchStatus: 'assigned',
      assignedDriverPersonId: 'driver-1',
      vehicleRefKey: null,
      scheduledStartAt: '2026-05-27T14:00:00Z',
      scheduledEndAt: '2026-05-27T17:00:00Z',
      loadCount: 0,
      createdByUserId: 'user-1',
      createdAt: '2026-05-27T08:00:00Z',
      updatedAt: '2026-05-27T08:00:00Z',
      assignedAt: '2026-05-27T08:30:00Z',
      dispatchedAt: null,
      startedAt: null,
      completedAt: null,
      cancelledAt: null,
    },
  ]),
  previewBulkDispatch,
  applyBulkDispatch,
}))

describe('BulkDispatchPanel', () => {
  afterEach(() => {
    cleanup()
    vi.restoreAllMocks()
  })

  it('previews and applies bulk driver assignment', async () => {
    previewBulkDispatch.mockResolvedValue({
      summary: { total: 1, canApplyCount: 1, blockedCount: 0 },
      items: [
        {
          tripId: '11111111-1111-1111-1111-111111111111',
          tripNumber: 'TR-1',
          title: 'North run',
          currentDispatchStatus: 'planned',
          canApply: true,
          hasBlockingConflicts: false,
          driverPreview: null,
          vehiclePreview: null,
          statusPreview: null,
        },
      ],
    })
    applyBulkDispatch.mockResolvedValue({
      summary: { total: 1, successCount: 1, failureCount: 0 },
      results: [{ tripId: '11111111-1111-1111-1111-111111111111', success: true, errorCode: null, errorMessage: null, trip: null }],
    })

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <BulkDispatchPanel accessToken="token" canAssign />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Bulk dispatch')).toBeTruthy()

    fireEvent.click(screen.getByTestId('bulk-trip-11111111-1111-1111-1111-111111111111'))
    fireEvent.change(screen.getByLabelText(/Driver person id/i), {
      target: { value: 'driver-bulk-1' },
    })
    fireEvent.click(screen.getByText('Preview conflicts'))

    await vi.waitFor(() => {
      expect(previewBulkDispatch).toHaveBeenCalledWith('token', {
        items: [
          {
            tripId: '11111111-1111-1111-1111-111111111111',
            driverPersonId: 'driver-bulk-1',
            vehicleRefKey: null,
            dispatchStatus: null,
          },
        ],
      })
    })

    fireEvent.click(screen.getByTestId('bulk-dispatch-apply'))

    await vi.waitFor(() => {
      expect(applyBulkDispatch).toHaveBeenCalled()
    })
  })
})
