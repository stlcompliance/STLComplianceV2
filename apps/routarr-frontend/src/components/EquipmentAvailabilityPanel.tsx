import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import { createEquipmentAvailability, getEquipmentAvailabilityPanel } from '../api/client'
import type { EquipmentAvailabilityPanelRow } from '../api/types'

type EquipmentAvailabilityPanelProps = {
  accessToken: string
  scope: 'daily' | 'weekly'
  onScopeChange: (scope: 'daily' | 'weekly') => void
  canManage: boolean
}

function formatTimestamp(iso: string) {
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}

function AvailabilityRow({ record }: { record: EquipmentAvailabilityPanelRow }) {
  const highlightClass = record.hasConflict
    ? 'border-red-500/60 bg-red-950/30'
    : record.availabilityStatus === 'available'
      ? 'border-emerald-500/40 bg-emerald-950/20'
      : 'border-slate-700 bg-slate-900/40'

  return (
    <li className={`rounded-lg border p-3 ${highlightClass}`}>
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <p className="text-sm font-medium text-slate-100">
            {record.availabilityStatus.replace('_', ' ')} · {record.vehicleRefKey}
          </p>
          {record.reason ? <p className="text-xs text-slate-400">{record.reason}</p> : null}
        </div>
        {record.hasConflict ? (
          <span className="text-xs font-medium text-red-300">
            {record.conflictingTripCount} trip conflict(s)
          </span>
        ) : (
          <span className="text-xs text-slate-500">No conflicts</span>
        )}
      </div>
      <p className="mt-2 text-xs text-slate-400">
        {formatTimestamp(record.startsAt)} → {formatTimestamp(record.endsAt)}
      </p>
      {record.conflictingTrips.length > 0 ? (
        <ul className="mt-2 space-y-1 text-xs text-red-200/90">
          {record.conflictingTrips.map((trip) => (
            <li key={trip.tripId}>
              {trip.tripNumber} · {trip.title} ({trip.dispatchStatus.replace('_', ' ')})
            </li>
          ))}
        </ul>
      ) : null}
    </li>
  )
}

export function EquipmentAvailabilityPanel({
  accessToken,
  scope,
  onScopeChange,
  canManage,
}: EquipmentAvailabilityPanelProps) {
  const queryClient = useQueryClient()
  const [vehicleRefKey, setVehicleRefKey] = useState('')
  const [availabilityStatus, setAvailabilityStatus] = useState('unavailable')
  const [reason, setReason] = useState('')

  const panelQuery = useQuery({
    queryKey: ['routarr-equipment-availability', accessToken, scope],
    queryFn: () => getEquipmentAvailabilityPanel(accessToken, scope),
  })

  const createMutation = useMutation({
    mutationFn: () => {
      const now = new Date()
      const start = new Date(now)
      start.setHours(now.getHours() + 1, 0, 0, 0)
      const end = new Date(start)
      end.setHours(start.getHours() + 8)
      return createEquipmentAvailability(accessToken, {
        vehicleRefKey,
        availabilityStatus,
        startsAt: start.toISOString(),
        endsAt: end.toISOString(),
        reason: reason || null,
      })
    },
    onSuccess: async () => {
      setReason('')
      await queryClient.invalidateQueries({ queryKey: ['routarr-equipment-availability'] })
    },
  })

  if (panelQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading equipment availability…</p>
  }

  if (panelQuery.isError) {
    return (
      <p className="text-sm text-red-300">
        Failed to load equipment availability. Check your RoutArr entitlement and try again.
      </p>
    )
  }

  const panel = panelQuery.data!

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-950/50 p-5">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-white">Equipment availability</h2>
          <p className="text-sm text-slate-400">
            {panel.summary.recordCount} record(s) · {panel.summary.conflictCount} conflict(s)
          </p>
        </div>
        <div className="flex gap-2">
          <button
            type="button"
            className={`rounded px-3 py-1 text-sm ${scope === 'daily' ? 'bg-slate-700 text-white' : 'text-slate-400'}`}
            onClick={() => onScopeChange('daily')}
          >
            Daily
          </button>
          <button
            type="button"
            className={`rounded px-3 py-1 text-sm ${scope === 'weekly' ? 'bg-slate-700 text-white' : 'text-slate-400'}`}
            onClick={() => onScopeChange('weekly')}
          >
            Weekly
          </button>
        </div>
      </div>

      <div className="mb-4 grid gap-3 sm:grid-cols-4">
        <div className="rounded border border-slate-700 p-3 text-center">
          <p className="text-xs text-slate-500">Unavailable</p>
          <p className="text-xl font-semibold text-slate-100">{panel.summary.unavailableCount}</p>
        </div>
        <div className="rounded border border-slate-700 p-3 text-center">
          <p className="text-xs text-slate-500">Limited</p>
          <p className="text-xl font-semibold text-slate-100">{panel.summary.limitedCount}</p>
        </div>
        <div className="rounded border border-slate-700 p-3 text-center">
          <p className="text-xs text-slate-500">Available</p>
          <p className="text-xl font-semibold text-slate-100">{panel.summary.availableCount}</p>
        </div>
        <div className="rounded border border-red-500/40 p-3 text-center">
          <p className="text-xs text-red-300">Conflicts</p>
          <p className="text-xl font-semibold text-red-200">{panel.summary.conflictCount}</p>
        </div>
      </div>

      {canManage ? (
        <form
          className="mb-4 grid gap-3 rounded-lg border border-slate-700 bg-slate-900/40 p-4 sm:grid-cols-2"
          onSubmit={(event) => {
            event.preventDefault()
            createMutation.mutate()
          }}
        >
          <label className="block text-sm text-slate-300">
            Vehicle ref key
            <input
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
              value={vehicleRefKey}
              onChange={(event) => setVehicleRefKey(event.target.value)}
              placeholder="asset-123"
            />
          </label>
          <label className="block text-sm text-slate-300">
            Status
            <select
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
              value={availabilityStatus}
              onChange={(event) => setAvailabilityStatus(event.target.value)}
            >
              <option value="unavailable">Unavailable</option>
              <option value="limited">Limited</option>
              <option value="available">Available</option>
            </select>
          </label>
          <label className="block text-sm text-slate-300 sm:col-span-2">
            Reason
            <input
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
              value={reason}
              onChange={(event) => setReason(event.target.value)}
              placeholder="PM, repair, out of service, etc."
            />
          </label>
          <button
            type="submit"
            disabled={createMutation.isPending || !vehicleRefKey.trim()}
            className="rounded bg-sky-700 px-3 py-2 text-sm text-white hover:bg-sky-600 disabled:opacity-50 sm:col-span-2"
          >
            {createMutation.isPending ? 'Saving…' : 'Add availability window'}
          </button>
        </form>
      ) : null}

      {panel.records.length === 0 ? (
        <p className="text-sm text-slate-500">No equipment availability records in this window.</p>
      ) : (
        <ul className="space-y-3">
          {panel.records.map((record) => (
            <AvailabilityRow key={record.availabilityId} record={record} />
          ))}
        </ul>
      )}
    </section>
  )
}
