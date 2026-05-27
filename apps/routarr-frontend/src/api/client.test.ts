import { afterEach, describe, expect, it, vi } from 'vitest'
import { getRoutes, getTrips } from './client'

describe('routarr api client', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('parses trip list success response', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [
          {
            tripId: '11111111-1111-1111-1111-111111111111',
            tripNumber: 'TR-20260527-AB12CD34',
            title: 'North yard delivery',
            dispatchStatus: 'planned',
            assignedDriverPersonId: null,
            vehicleRefKey: null,
            scheduledStartAt: null,
            scheduledEndAt: null,
            loadCount: 0,
            createdByUserId: '22222222-2222-2222-2222-222222222222',
            createdAt: '2026-05-27T12:00:00Z',
            updatedAt: '2026-05-27T12:00:00Z',
            assignedAt: null,
            dispatchedAt: null,
            startedAt: null,
            completedAt: null,
            cancelledAt: null,
          },
        ],
      }),
    )

    const trips = await getTrips('token')
    expect(trips).toHaveLength(1)
    expect(trips[0]?.title).toBe('North yard delivery')
  })

  it('throws RoutArrApiError on forbidden trip list', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: false,
        status: 403,
        text: async () => 'forbidden',
      }),
    )

    await expect(getTrips('token')).rejects.toMatchObject({ status: 403 })
  })

  it('parses route list success response', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [
          {
            routeId: '11111111-1111-1111-1111-111111111111',
            routeNumber: 'RT-20260527-AB12CD34',
            title: 'North quarry loop',
            routeStatus: 'planned',
            tripId: null,
            stopCount: 2,
            createdByUserId: '22222222-2222-2222-2222-222222222222',
            createdAt: '2026-05-27T12:00:00Z',
            updatedAt: '2026-05-27T12:00:00Z',
            activatedAt: null,
            completedAt: null,
            cancelledAt: null,
          },
        ],
      }),
    )

    const routes = await getRoutes('token')
    expect(routes).toHaveLength(1)
    expect(routes[0]?.title).toBe('North quarry loop')
  })

  it('parses dispatch board success response', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({
          scope: 'daily',
          windowStart: '2026-05-27T00:00:00Z',
          windowEnd: '2026-05-28T00:00:00Z',
          trips: {
            plannedCount: 1,
            assignedCount: 0,
            dispatchedCount: 0,
            inProgressCount: 0,
            completedCount: 0,
            cancelledCount: 0,
            totalCount: 1,
            lateCount: 0,
            atRiskCount: 0,
          },
          routes: {
            draftCount: 0,
            plannedCount: 0,
            activeCount: 0,
            completedCount: 0,
            cancelledCount: 0,
            totalCount: 0,
          },
          stops: {
            pendingCount: 0,
            arrivedCount: 0,
            completedCount: 0,
            skippedCount: 0,
            totalCount: 0,
          },
          workQueue: {
            unassignedDriverTripCount: 1,
            unlinkedRouteCount: 0,
            pendingStopCount: 0,
          },
          assignedTrips: [],
          activeTrips: [],
          generatedAt: '2026-05-27T12:00:00Z',
        }),
      }),
    )

    const { getDispatchBoard } = await import('./client')
    const board = await getDispatchBoard('token', 'daily')
    expect(board.trips.totalCount).toBe(1)
    expect(board.workQueue.unassignedDriverTripCount).toBe(1)
  })

  it('loads route calendar with scope', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({
          scope: 'weekly',
          windowStart: '2026-05-27T00:00:00Z',
          windowEnd: '2026-06-03T00:00:00Z',
          summary: {
            tripCount: 2,
            routeCount: 1,
            stopCount: 3,
            lateTripCount: 0,
            atRiskTripCount: 1,
          },
          days: [
            {
              date: '2026-05-27T00:00:00Z',
              events: [
                {
                  eventType: 'trip',
                  entityId: '11111111-1111-1111-1111-111111111111',
                  label: 'Weekly trip',
                  status: 'planned',
                  scheduledAt: '2026-05-27T08:00:00Z',
                  scheduledEndAt: null,
                  tripId: '11111111-1111-1111-1111-111111111111',
                  routeId: null,
                  tripNumber: 'TR-001',
                  routeNumber: null,
                  assignedDriverPersonId: null,
                  isLate: false,
                  isAtRisk: false,
                },
              ],
            },
          ],
          generatedAt: '2026-05-27T12:00:00Z',
        }),
      }),
    )

    const { getRouteCalendar } = await import('./client')
    const calendar = await getRouteCalendar('token', 'weekly')
    expect(calendar.scope).toBe('weekly')
    expect(calendar.summary.tripCount).toBe(2)
    expect(calendar.days[0].events[0].label).toBe('Weekly trip')
  })

  it('parses driver availability panel success response', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({
          scope: 'daily',
          windowStart: '2026-05-27T00:00:00Z',
          windowEnd: '2026-05-28T00:00:00Z',
          summary: {
            recordCount: 1,
            unavailableCount: 1,
            limitedCount: 0,
            availableCount: 0,
            conflictCount: 1,
          },
          records: [
            {
              availabilityId: '44444444-4444-4444-4444-444444444444',
              personId: 'driver-1',
              availabilityStatus: 'unavailable',
              startsAt: '2026-05-27T08:00:00Z',
              endsAt: '2026-05-27T18:00:00Z',
              reason: 'PTO',
              hasConflict: true,
              conflictingTripCount: 1,
              conflictingTrips: [],
            },
          ],
          generatedAt: '2026-05-27T12:00:00Z',
        }),
      }),
    )

    const { getDriverAvailabilityPanel } = await import('./client')
    const panel = await getDriverAvailabilityPanel('token', 'daily')
    expect(panel.summary.conflictCount).toBe(1)
    expect(panel.records[0]?.reason).toBe('PTO')
  })

  it('parses equipment availability panel success response', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({
          scope: 'daily',
          windowStart: '2026-05-27T00:00:00Z',
          windowEnd: '2026-05-28T00:00:00Z',
          summary: {
            recordCount: 1,
            unavailableCount: 1,
            limitedCount: 0,
            availableCount: 0,
            conflictCount: 1,
          },
          records: [
            {
              availabilityId: '55555555-5555-5555-5555-555555555555',
              vehicleRefKey: 'truck-42',
              availabilityStatus: 'unavailable',
              startsAt: '2026-05-27T08:00:00Z',
              endsAt: '2026-05-27T18:00:00Z',
              reason: 'PM service',
              hasConflict: true,
              conflictingTripCount: 1,
              conflictingTrips: [],
            },
          ],
          generatedAt: '2026-05-27T12:00:00Z',
        }),
      }),
    )

    const { getEquipmentAvailabilityPanel } = await import('./client')
    const panel = await getEquipmentAvailabilityPanel('token', 'daily')
    expect(panel.summary.conflictCount).toBe(1)
    expect(panel.records[0]?.vehicleRefKey).toBe('truck-42')
  })

  it('parses dispatch assignment preview success response', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({
          tripId: '11111111-1111-1111-1111-111111111111',
          assignmentKind: 'driver',
          canAssign: false,
          hasBlockingConflicts: true,
          blockingDriverAvailability: [
            {
              availabilityId: '44444444-4444-4444-4444-444444444444',
              availabilityStatus: 'unavailable',
              startsAt: '2026-05-27T08:00:00Z',
              endsAt: '2026-05-27T18:00:00Z',
              reason: 'PTO',
            },
          ],
          blockingEquipmentAvailability: [],
          overlappingTrips: [],
        }),
      }),
    )

    const { previewDispatchAssignment } = await import('./client')
    const preview = await previewDispatchAssignment('token', {
      tripId: '11111111-1111-1111-1111-111111111111',
      assignmentKind: 'driver',
      driverPersonId: 'driver-1',
    })
    expect(preview.hasBlockingConflicts).toBe(true)
    expect(preview.blockingDriverAvailability).toHaveLength(1)
  })

  it('parses bulk dispatch preview success response', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({
          summary: { total: 1, canApplyCount: 0, blockedCount: 1 },
          items: [
            {
              tripId: '11111111-1111-1111-1111-111111111111',
              tripNumber: 'TR-1',
              title: 'North run',
              currentDispatchStatus: 'planned',
              canApply: false,
              hasBlockingConflicts: true,
              driverPreview: {
                tripId: '11111111-1111-1111-1111-111111111111',
                assignmentKind: 'driver',
                canAssign: false,
                hasBlockingConflicts: true,
                blockingDriverAvailability: [],
                blockingEquipmentAvailability: [],
                overlappingTrips: [{ tripId: '22222222-2222-2222-2222-222222222222', tripNumber: 'TR-2', title: 'South', dispatchStatus: 'assigned', scheduledStartAt: null, scheduledEndAt: null }],
              },
              vehiclePreview: null,
              statusPreview: null,
            },
          ],
        }),
      }),
    )

    const { previewBulkDispatch } = await import('./client')
    const preview = await previewBulkDispatch('token', {
      items: [{ tripId: '11111111-1111-1111-1111-111111111111', driverPersonId: 'driver-1' }],
    })
    expect(preview.summary.blockedCount).toBe(1)
    expect(preview.items[0]?.driverPreview?.overlappingTrips).toHaveLength(1)
  })
})
