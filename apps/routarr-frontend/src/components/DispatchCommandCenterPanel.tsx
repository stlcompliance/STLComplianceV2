import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, StaticSearchPicker, getErrorMessage, type PickerOption } from '@stl/shared-ui'

import {
  assignTripDriver,
  getDispatchCommandCenter,
  previewDispatchAssignment,
  updateTripStatus,
  upsertDispatchBoardState,
} from '../api/client'
import type { TripSummaryResponse } from '../api/types'
import {
  confirmDispatchAssignmentPreview,
  DRAG_MIME,
  parseDragPayload,
} from '../lib/dispatchAssignment'

type Props = {
  accessToken: string
  scope: 'daily' | 'weekly'
  onScopeChange: (scope: 'daily' | 'weekly') => void
  canAssign: boolean
}

function TripCard({
  trip,
  canAssign,
  driverOptions,
  isPending,
  onAssign,
  onDispatch,
}: {
  trip: TripSummaryResponse
  canAssign: boolean
  driverOptions: { personId: string; displayName: string }[]
  isPending: boolean
  onAssign: (tripId: string, personId: string, displayName: string) => void
  onDispatch: (tripId: string) => void
}) {
  const [selectedDriver, setSelectedDriver] = useState('')
  const [dragOver, setDragOver] = useState(false)
  const selectedDriverMatch = driverOptions.find((d) => d.personId === selectedDriver)
  const selectedDriverOption: PickerOption | undefined = selectedDriverMatch
    ? { value: selectedDriver, label: selectedDriverMatch.displayName }
    : undefined

  return (
    <li
      className={[
        'rounded-md border p-2 text-xs transition-colors',
        dragOver ? 'border-sky-500 bg-sky-950/40' : 'border-slate-700 bg-slate-950/50',
      ].join(' ')}
      data-testid={`command-center-trip-${trip.tripId}`}
      onDragOver={(event) => {
        if (!canAssign) return
        event.preventDefault()
        setDragOver(true)
      }}
      onDragLeave={() => setDragOver(false)}
      onDrop={(event) => {
        event.preventDefault()
        setDragOver(false)
        if (!canAssign) return
        const payload = parseDragPayload(event.dataTransfer.getData(DRAG_MIME))
        if (payload?.kind === 'driver') {
          const ref = driverOptions.find((d) => d.personId === payload.personId)
          onAssign(trip.tripId, payload.personId, ref?.displayName ?? payload.personId)
        }
      }}
    >
      <p className="font-medium text-slate-100">{trip.title}</p>
      <p className="text-slate-500">{trip.tripNumber}</p>
      {trip.assignedDriverPersonId ? (
        <p className="mt-1 text-slate-400">Driver: {trip.assignedDriverPersonId.slice(0, 12)}…</p>
      ) : null}
      {canAssign ? (
        <div className="mt-2 flex flex-wrap gap-1">
          <StaticSearchPicker
            id="dispatchcommandcenter-select-field"
            label="Driver"
            value={selectedDriver}
            onChange={setSelectedDriver}
            options={driverOptions.map((driver) => ({
              value: driver.personId,
              label: driver.displayName,
            }))}
            selectedOption={selectedDriverOption}
            placeholder="Assign driver…"
            testId={`command-center-driver-picker-${trip.tripId}`}
          />
          <button
            type="button"
            className="rounded bg-slate-700 px-2 py-0.5 text-white disabled:opacity-50"
            disabled={!selectedDriver || isPending}
            data-testid={`command-center-assign-${trip.tripId}`}
            onClick={() => {
              const ref = driverOptions.find((d) => d.personId === selectedDriver)
              onAssign(trip.tripId, selectedDriver, ref?.displayName ?? selectedDriver)
            }}
          >
            Assign
          </button>
          {trip.dispatchStatus === 'assigned' ? (
            <button
              type="button"
              className="rounded bg-sky-700 px-2 py-0.5 text-white disabled:opacity-50"
              disabled={isPending}
              data-testid={`command-center-dispatch-${trip.tripId}`}
              onClick={() => onDispatch(trip.tripId)}
            >
              Dispatch
            </button>
          ) : null}
        </div>
      ) : null}
    </li>
  )
}

export function DispatchCommandCenterPanel({
  accessToken,
  scope,
  onScopeChange,
  canAssign,
}: Props) {
  const queryClient = useQueryClient()
  const [statusMessage, setStatusMessage] = useState<string | null>(null)

  const centerQuery = useQuery({
    queryKey: ['routarr-command-center', accessToken, scope],
    queryFn: () => getDispatchCommandCenter(accessToken, scope),
  })

  const scopeMutation = useMutation({
    mutationFn: (next: 'daily' | 'weekly') => upsertDispatchBoardState(accessToken, next),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['routarr-command-center'] })
      void queryClient.invalidateQueries({ queryKey: ['routarr-dispatch-board'] })
    },
  })

  const assignMutation = useMutation({
    mutationFn: async ({
      tripId,
      personId,
      displayName,
    }: {
      tripId: string
      personId: string
      displayName: string
    }) => {
      const preview = await previewDispatchAssignment(accessToken, {
        tripId,
        assignmentKind: 'driver',
        driverPersonId: personId,
        vehicleRefKey: null,
      })

      const ignoreFlags = confirmDispatchAssignmentPreview(preview, (message) =>
        window.confirm(message),
      )
      if (!ignoreFlags) {
        throw new Error('Assignment cancelled')
      }

      return assignTripDriver(accessToken, tripId, {
        driverPersonId: personId,
        driverDisplayName: displayName,
        ignoreAvailabilityConflicts: ignoreFlags.ignoreConflicts,
        ignoreEligibilityBlocks: ignoreFlags.ignoreEligibilityBlocks,
        ignoreWorkflowGateBlocks: ignoreFlags.ignoreWorkflowGateBlocks,
      })
    },
    onSuccess: () => {
      setStatusMessage('Driver assigned.')
      void queryClient.invalidateQueries({ queryKey: ['routarr-command-center'] })
      void queryClient.invalidateQueries({ queryKey: ['routarr-dispatch-board'] })
      void queryClient.invalidateQueries({ queryKey: ['routarr-trips'] })
      void queryClient.invalidateQueries({ queryKey: ['routarr-unassigned-work-queue'] })
    },
    onError: (error: Error) => {
      if (error.message !== 'Assignment cancelled') {
        setStatusMessage(error.message)
      } else {
        setStatusMessage('Assignment cancelled.')
      }
    },
  })

  const dispatchMutation = useMutation({
    mutationFn: (tripId: string) =>
      updateTripStatus(accessToken, tripId, { dispatchStatus: 'dispatched' }),
    onSuccess: () => {
      setStatusMessage('Trip dispatched.')
      void queryClient.invalidateQueries({ queryKey: ['routarr-command-center'] })
      void queryClient.invalidateQueries({ queryKey: ['routarr-dispatch-board'] })
      void queryClient.invalidateQueries({ queryKey: ['routarr-trips'] })
    },
    onError: (error: Error) => setStatusMessage(error.message),
  })

  if (centerQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading command center…</p>
  }

  if (centerQuery.isError) {
    return (
      <ApiErrorCallout
        title="Dispatch command center unavailable"
        message={getErrorMessage(centerQuery.error, 'Unable to load dispatch command center.')}
        retryLabel="Retry load"
        onRetry={() => {
          void centerQuery.refetch()
        }}
        testId="command-center-retry"
      />
    )
  }

  const center = centerQuery.data!
  const driverOptions = center.driverRefs.items.map((d) => ({
    personId: d.personId,
    displayName: d.displayName,
  }))
  const isPending = assignMutation.isPending || dispatchMutation.isPending

  const handleScopeChange = (next: 'daily' | 'weekly') => {
    onScopeChange(next)
    if (canAssign) {
      scopeMutation.mutate(next)
    }
  }

  function startDriverDrag(event: React.DragEvent, personId: string) {
    if (!canAssign) return
    event.dataTransfer.setData(DRAG_MIME, JSON.stringify({ kind: 'driver', personId }))
    event.dataTransfer.effectAllowed = 'move'
  }

  return (
    <section
      className="rounded-xl border border-sky-800/50 bg-sky-950/20 p-5"
      data-testid="dispatch-command-center-panel"
    >
      <header className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Dispatch command center</h2>
          <p className="mt-1 text-sm text-slate-400">
            Trips by status · {center.board.workQueue.unassignedDriverTripCount} unassigned ·{' '}
            {center.board.trips.lateCount} late ·{' '}
            {center.board.workQueue.missingProofTripCount} missing proof
          </p>
        </div>
        <div className="flex gap-2">
          {(['daily', 'weekly'] as const).map((s) => (
            <button
              key={s}
              type="button"
              className={[
                'rounded-md px-3 py-1 text-sm capitalize',
                scope === s
                  ? 'bg-sky-700 text-white'
                  : 'bg-slate-800 text-slate-300 hover:bg-slate-700',
              ].join(' ')}
              onClick={() => handleScopeChange(s)}
            >
              {s}
            </button>
          ))}
        </div>
      </header>

      {statusMessage ? (
        <p className="mt-3 text-sm text-slate-300" data-testid="command-center-status">
          {statusMessage}
        </p>
      ) : null}

      {canAssign && driverOptions.length > 0 ? (
        <div
          className="mt-4 flex flex-wrap gap-2"
          data-testid="command-center-driver-chips"
        >
          <span className="w-full text-xs text-slate-500">Drag a driver onto a trip card</span>
          {driverOptions.map((driver) => (
            <button
              key={driver.personId}
              type="button"
              draggable
              className="cursor-grab rounded-full border border-slate-600 bg-slate-900 px-3 py-1 text-xs text-slate-200 active:cursor-grabbing"
              data-testid={`command-center-driver-chip-${driver.personId}`}
              onDragStart={(event) => startDriverDrag(event, driver.personId)}
            >
              {driver.displayName}
            </button>
          ))}
        </div>
      ) : null}

      <div className="mt-4 grid gap-3 md:grid-cols-2 xl:grid-cols-4">
        {center.tripColumns.map((column) => (
          <div
            key={column.dispatchStatus}
            className="rounded-lg border border-slate-700 bg-slate-900/60 p-3"
            data-testid={`trip-column-${column.dispatchStatus}`}
          >
            <h3 className="text-sm font-semibold text-slate-200">
              {column.label}{' '}
              <span className="text-slate-500">({column.count})</span>
            </h3>
            <ul className="mt-2 max-h-48 space-y-2 overflow-y-auto">
              {column.trips.length === 0 ? (
                <li className="text-xs text-slate-500">No trips</li>
              ) : (
                column.trips.slice(0, 8).map((trip) => (
                  <TripCard
                    key={trip.tripId}
                    trip={trip}
                    canAssign={canAssign}
                    driverOptions={driverOptions}
                    isPending={isPending}
                    onAssign={(tripId, personId, displayName) =>
                      assignMutation.mutate({ tripId, personId, displayName })
                    }
                    onDispatch={(tripId) => dispatchMutation.mutate(tripId)}
                  />
                ))
              )}
            </ul>
          </div>
        ))}
      </div>
    </section>
  )
}
