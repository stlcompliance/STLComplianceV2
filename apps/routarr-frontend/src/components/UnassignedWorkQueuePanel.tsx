import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import {
  applyBulkDispatch,
  assignTripDriver,
  getUnassignedWorkQueue,
} from '../api/client'
import type { UnassignedWorkQueueTripRow } from '../api/types'

type Props = {
  accessToken: string
  scope: 'daily' | 'weekly'
  canAssign: boolean
}

function formatTimestamp(iso: string | null) {
  if (!iso) return '—'
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}

function TripRow({
  trip,
  selected,
  canAssign,
  onToggle,
  driverOptions,
  onAssign,
  isPending,
}: {
  trip: UnassignedWorkQueueTripRow
  selected: boolean
  canAssign: boolean
  onToggle: () => void
  driverOptions: { personId: string; displayName: string }[]
  onAssign: (tripId: string, personId: string, displayName: string) => void
  isPending: boolean
}) {
  const [driverId, setDriverId] = useState('')

  const borderClass = trip.isLate
    ? 'border-red-500/60 bg-red-950/30'
    : trip.isAtRisk
      ? 'border-amber-500/60 bg-amber-950/20'
      : 'border-slate-700 bg-slate-950/50'

  return (
    <li
      className={`rounded-md border p-3 text-sm ${borderClass}`}
      data-testid={`unassigned-trip-${trip.tripId}`}
    >
      <div className="flex flex-wrap items-start gap-2">
        {canAssign ? (
          <input
            type="checkbox"
            checked={selected}
            onChange={onToggle}
            aria-label={`Select ${trip.title}`}
          />
        ) : null}
        <div className="min-w-0 flex-1">
          <p className="font-medium text-slate-100">{trip.title}</p>
          <p className="text-xs text-slate-500">
            {trip.tripNumber} · {trip.dispatchStatus.replace('_', ' ')}
            {trip.isLate ? ' · late' : trip.isAtRisk ? ' · at risk' : ''}
          </p>
          <p className="mt-1 text-xs text-slate-400">
            Start {formatTimestamp(trip.scheduledStartAt)} · {trip.routeCount} route(s)
          </p>
        </div>
      </div>
      {canAssign ? (
        <div className="mt-2 flex flex-wrap gap-1">
          <select
            className="min-w-0 flex-1 rounded border border-slate-600 bg-slate-900 px-2 py-1 text-xs text-slate-200"
            value={driverId}
            onChange={(e) => setDriverId(e.target.value)}
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
            className="rounded bg-violet-700 px-2 py-1 text-xs text-white disabled:opacity-50"
            disabled={!driverId || isPending}
            onClick={() => {
              const ref = driverOptions.find((d) => d.personId === driverId)
              onAssign(trip.tripId, driverId, ref?.displayName ?? driverId)
            }}
          >
            Assign
          </button>
        </div>
      ) : null}
    </li>
  )
}

export function UnassignedWorkQueuePanel({ accessToken, scope, canAssign }: Props) {
  const queryClient = useQueryClient()
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())
  const [bulkDriverId, setBulkDriverId] = useState('')

  const queueQuery = useQuery({
    queryKey: ['routarr-unassigned-queue', accessToken, scope],
    queryFn: () => getUnassignedWorkQueue(accessToken, scope),
  })

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: ['routarr-unassigned-queue'] })
    void queryClient.invalidateQueries({ queryKey: ['routarr-dispatch-board'] })
    void queryClient.invalidateQueries({ queryKey: ['routarr-command-center'] })
    void queryClient.invalidateQueries({ queryKey: ['routarr-active-trips'] })
    void queryClient.invalidateQueries({ queryKey: ['routarr-trips'] })
  }

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
      await assignTripDriver(accessToken, tripId, {
        driverPersonId: personId,
        driverDisplayName: displayName,
      })
    },
    onSuccess: () => {
      setSelectedIds(new Set())
      invalidate()
    },
  })

  const bulkMutation = useMutation({
    mutationFn: async (personId: string) => {
      const displayName =
        queueQuery.data?.driverRefs.items.find((d) => d.personId === personId)?.displayName ??
        personId
      await applyBulkDispatch(accessToken, {
        items: [...selectedIds].map((tripId) => ({
          tripId,
          driverPersonId: personId,
        })),
      })
      return displayName
    },
    onSuccess: () => {
      setSelectedIds(new Set())
      setBulkDriverId('')
      invalidate()
    },
  })

  if (queueQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading unassigned work queue…</p>
  }

  if (queueQuery.isError) {
    return <p className="text-sm text-red-300">{(queueQuery.error as Error).message}</p>
  }

  const queue = queueQuery.data!
  const driverOptions = queue.driverRefs.items.map((d) => ({
    personId: d.personId,
    displayName: d.displayName,
  }))
  const isPending = assignMutation.isPending || bulkMutation.isPending

  const toggleTrip = (tripId: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev)
      if (next.has(tripId)) {
        next.delete(tripId)
      } else {
        next.add(tripId)
      }
      return next
    })
  }

  return (
    <section
      className="rounded-xl border border-violet-800/40 bg-violet-950/15 p-5"
      data-testid="unassigned-work-queue-panel"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Unassigned work queue</h2>
        <p className="mt-1 text-sm text-slate-400">
          {queue.unassignedCount} active trips without a driver in this {queue.scope} window
        </p>
      </header>

      {canAssign && queue.items.length > 0 ? (
        <div className="mt-4 flex flex-wrap items-center gap-2 rounded-lg border border-slate-700 bg-slate-900/50 p-3">
          <select
            className="rounded border border-slate-600 bg-slate-900 px-2 py-1 text-sm text-slate-200"
            value={bulkDriverId}
            onChange={(e) => setBulkDriverId(e.target.value)}
            aria-label="Bulk assign driver"
          >
            <option value="">Bulk assign driver…</option>
            {driverOptions.map((d) => (
              <option key={d.personId} value={d.personId}>
                {d.displayName}
              </option>
            ))}
          </select>
          <button
            type="button"
            className="rounded bg-violet-700 px-3 py-1 text-sm text-white disabled:opacity-50"
            disabled={selectedIds.size === 0 || !bulkDriverId || isPending}
            onClick={() => bulkMutation.mutate(bulkDriverId)}
            data-testid="bulk-assign-unassigned"
          >
            Assign {selectedIds.size} selected
          </button>
          <button
            type="button"
            className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-400"
            onClick={() =>
              setSelectedIds(new Set(queue.items.map((t) => t.tripId)))
            }
          >
            Select all
          </button>
        </div>
      ) : null}

      <ul className="mt-4 max-h-80 space-y-2 overflow-y-auto">
        {queue.items.length === 0 ? (
          <li className="text-sm text-slate-500">No unassigned active trips in this window.</li>
        ) : (
          queue.items.map((trip) => (
            <TripRow
              key={trip.tripId}
              trip={trip}
              selected={selectedIds.has(trip.tripId)}
              canAssign={canAssign}
              onToggle={() => toggleTrip(trip.tripId)}
              driverOptions={driverOptions}
              isPending={isPending}
              onAssign={(tripId, personId, displayName) =>
                assignMutation.mutate({ tripId, personId, displayName })
              }
            />
          ))
        )}
      </ul>
    </section>
  )
}
