import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import {
  assignTripDriver,
  getDispatchCommandCenter,
  updateTripStatus,
  upsertDispatchBoardState,
} from '../api/client'
import type { TripSummaryResponse } from '../api/types'

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
  onAssign,
  onDispatch,
  isPending,
}: {
  trip: TripSummaryResponse
  canAssign: boolean
  driverOptions: { personId: string; displayName: string }[]
  onAssign: (tripId: string, personId: string, displayName: string) => void
  onDispatch: (tripId: string) => void
  isPending: boolean
}) {
  const [selectedDriver, setSelectedDriver] = useState('')

  return (
    <li className="rounded-md border border-slate-700 bg-slate-950/50 p-2 text-xs">
      <p className="font-medium text-slate-100">{trip.title}</p>
      <p className="text-slate-500">{trip.tripNumber}</p>
      {trip.assignedDriverPersonId ? (
        <p className="mt-1 text-slate-400">Driver: {trip.assignedDriverPersonId.slice(0, 12)}…</p>
      ) : null}
      {canAssign ? (
        <div className="mt-2 flex flex-wrap gap-1">
          <select
            className="min-w-0 flex-1 rounded border border-slate-600 bg-slate-900 px-1 py-0.5 text-slate-200"
            value={selectedDriver}
            onChange={(e) => setSelectedDriver(e.target.value)}
          >
            <option value="">Assign driver…</option>
            {driverOptions.map((d) => (
              <option key={d.personId} value={d.personId}>
                {d.displayName}
              </option>
            ))}
          </select>
          <button
            type="button"
            className="rounded bg-slate-700 px-2 py-0.5 text-white disabled:opacity-50"
            disabled={!selectedDriver || isPending}
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

  const actionMutation = useMutation({
    mutationFn: async ({
      tripId,
      personId,
      displayName,
      action,
    }: {
      tripId: string
      personId?: string
      displayName?: string
      action: 'assign' | 'dispatch'
    }) => {
      if (action === 'assign' && personId) {
        await assignTripDriver(accessToken, tripId, {
          driverPersonId: personId,
          driverDisplayName: displayName,
        })
      }
      if (action === 'dispatch') {
        await updateTripStatus(accessToken, tripId, { dispatchStatus: 'dispatched' })
      }
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['routarr-command-center'] })
      void queryClient.invalidateQueries({ queryKey: ['routarr-dispatch-board'] })
      void queryClient.invalidateQueries({ queryKey: ['routarr-trips'] })
    },
  })

  if (centerQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading command center…</p>
  }

  if (centerQuery.isError) {
    return (
      <p className="text-sm text-red-300">{(centerQuery.error as Error).message}</p>
    )
  }

  const center = centerQuery.data!
  const driverOptions = center.driverRefs.items.map((d) => ({
    personId: d.personId,
    displayName: d.displayName,
  }))

  const handleScopeChange = (next: 'daily' | 'weekly') => {
    onScopeChange(next)
    if (canAssign) {
      scopeMutation.mutate(next)
    }
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
            {center.board.trips.lateCount} late
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
                    isPending={actionMutation.isPending}
                    onAssign={(tripId, personId, displayName) =>
                      actionMutation.mutate({
                        tripId,
                        personId,
                        displayName,
                        action: 'assign',
                      })
                    }
                    onDispatch={(tripId) =>
                      actionMutation.mutate({ tripId, action: 'dispatch' })
                    }
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
