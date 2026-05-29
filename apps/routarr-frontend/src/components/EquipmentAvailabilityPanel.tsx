import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useMemo, useState } from 'react'
import { AdvancedReferenceField, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'

import {
  createEquipmentAvailability,
  deleteEquipmentAvailability,
  getEquipmentAvailabilityPanel,
  listVehicleRefs,
  updateEquipmentAvailability,
} from '../api/client'
import type { EquipmentAvailabilityPanelRow } from '../api/types'
import { fromDatetimeLocalValue, toDatetimeLocalValue } from '../lib/availabilityDateTime'
import { findVehicleLabel, vehicleRefToPickerOption } from '../lib/referencePickers'

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

type EquipmentAvailabilityRecordRowProps = {
  record: EquipmentAvailabilityPanelRow
  canManage: boolean
  accessToken: string
  onChanged: () => Promise<void>
  vehicleLabel?: string
}

function EquipmentAvailabilityRecordRow({
  record,
  canManage,
  accessToken,
  onChanged,
  vehicleLabel,
}: EquipmentAvailabilityRecordRowProps) {
  const [editing, setEditing] = useState(false)
  const [availabilityStatus, setAvailabilityStatus] = useState(record.availabilityStatus)
  const [reason, setReason] = useState(record.reason ?? '')
  const [startsAt, setStartsAt] = useState(toDatetimeLocalValue(record.startsAt))
  const [endsAt, setEndsAt] = useState(toDatetimeLocalValue(record.endsAt))
  const [actionError, setActionError] = useState<string | null>(null)

  useEffect(() => {
    if (!editing) {
      setAvailabilityStatus(record.availabilityStatus)
      setReason(record.reason ?? '')
      setStartsAt(toDatetimeLocalValue(record.startsAt))
      setEndsAt(toDatetimeLocalValue(record.endsAt))
    }
  }, [record, editing])

  const updateMutation = useMutation({
    mutationFn: () =>
      updateEquipmentAvailability(accessToken, record.availabilityId, {
        availabilityStatus,
        reason: reason || null,
        startsAt: fromDatetimeLocalValue(startsAt),
        endsAt: fromDatetimeLocalValue(endsAt),
      }),
    onSuccess: async () => {
      setActionError(null)
      setEditing(false)
      await onChanged()
    },
    onError: (error: Error) => setActionError(error.message),
  })

  const deleteMutation = useMutation({
    mutationFn: () => deleteEquipmentAvailability(accessToken, record.availabilityId),
    onSuccess: async () => {
      setActionError(null)
      await onChanged()
    },
    onError: (error: Error) => setActionError(error.message),
  })

  const highlightClass = record.hasConflict
    ? 'border-red-500/60 bg-red-950/30'
    : record.availabilityStatus === 'available'
      ? 'border-emerald-500/40 bg-emerald-950/20'
      : 'border-slate-700 bg-slate-900/40'

  const isPending = updateMutation.isPending || deleteMutation.isPending

  if (editing) {
    return (
      <li className={`rounded-lg border p-3 ${highlightClass}`}>
        <form
          className="grid gap-3 sm:grid-cols-2"
          onSubmit={(event) => {
            event.preventDefault()
            updateMutation.mutate()
          }}
        >
          <label className="block text-sm text-slate-300" htmlFor="equipmentavailability-status">
          Status
          <select id="equipmentavailability-status"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
              value={availabilityStatus}
              onChange={(event) => setAvailabilityStatus(event.target.value)}
            >
              <option value="unavailable">Unavailable</option>
              <option value="limited">Limited</option>
              <option value="available">Available</option>
            </select>
          </label>
          <label className="block text-sm text-slate-300" htmlFor="equipmentavailability-reason">
          Reason
          <input id="equipmentavailability-reason"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
              value={reason}
              onChange={(event) => setReason(event.target.value)}
            />
          </label>
          <label className="block text-sm text-slate-300" htmlFor="equipmentavailability-starts-at">
          Starts at
          <input id="equipmentavailability-starts-at"
              type="datetime-local"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
              value={startsAt}
              onChange={(event) => setStartsAt(event.target.value)}
              required
            />
          </label>
          <label className="block text-sm text-slate-300" htmlFor="equipmentavailability-ends-at">
          Ends at
          <input id="equipmentavailability-ends-at"
              type="datetime-local"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
              value={endsAt}
              onChange={(event) => setEndsAt(event.target.value)}
              required
            />
          </label>
          <div className="flex flex-wrap gap-2 sm:col-span-2">
            <button
              type="submit"
              disabled={isPending || !startsAt || !endsAt}
              className="rounded bg-sky-700 px-3 py-1.5 text-sm text-white hover:bg-sky-600 disabled:opacity-50"
            >
              {updateMutation.isPending ? 'Saving…' : 'Save changes'}
            </button>
            <button
              type="button"
              disabled={isPending}
              className="rounded border border-slate-600 px-3 py-1.5 text-sm text-slate-300 hover:bg-slate-800 disabled:opacity-50"
              onClick={() => {
                setActionError(null)
                setEditing(false)
              }}
            >
              Cancel
            </button>
          </div>
        </form>
        {actionError ? (
          <p className="mt-2 text-xs text-red-300" role="alert">
            {actionError}
          </p>
        ) : null}
      </li>
    )
  }

  return (
    <li className={`rounded-lg border p-3 ${highlightClass}`}>
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <p className="text-sm font-medium text-slate-100">
            {record.availabilityStatus.replace('_', ' ')} · {vehicleLabel ?? record.vehicleRefKey}
          </p>
          {record.reason ? <p className="text-xs text-slate-400">{record.reason}</p> : null}
        </div>
        <div className="flex flex-wrap items-center gap-2">
          {record.hasConflict ? (
            <span className="text-xs font-medium text-red-300">
              {record.conflictingTripCount} trip conflict(s)
            </span>
          ) : (
            <span className="text-xs text-slate-500">No conflicts</span>
          )}
          {canManage ? (
            <>
              <button
                type="button"
                className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-300 hover:bg-slate-800"
                onClick={() => setEditing(true)}
              >
                Edit
              </button>
              <button
                type="button"
                disabled={isPending}
                className="rounded border border-red-500/50 px-2 py-1 text-xs text-red-200 hover:bg-red-950/40 disabled:opacity-50"
                onClick={() => {
                  if (
                    window.confirm(
                      'Delete this equipment availability window? Dispatch may treat the vehicle as available again.',
                    )
                  ) {
                    deleteMutation.mutate()
                  }
                }}
              >
                {deleteMutation.isPending ? 'Deleting…' : 'Delete'}
              </button>
            </>
          ) : null}
        </div>
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
      {actionError ? (
        <p className="mt-2 text-xs text-red-300" role="alert">
          {actionError}
        </p>
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

  const vehicleRefsQuery = useQuery({
    queryKey: ['routarr-vehicle-refs-availability', accessToken],
    queryFn: () => listVehicleRefs(accessToken),
    enabled: canManage,
  })

  const vehicleRefs = vehicleRefsQuery.data?.items ?? []
  const vehicleOptions = useMemo(() => vehicleRefs.map(vehicleRefToPickerOption), [vehicleRefs])
  const selectedVehicleOption = useMemo((): PickerOption | undefined => {
    const label = findVehicleLabel(vehicleRefs, vehicleRefKey)
    return label ? { value: vehicleRefKey, label } : undefined
  }, [vehicleRefKey, vehicleRefs])

  const invalidatePanel = async () => {
    await queryClient.invalidateQueries({ queryKey: ['routarr-equipment-availability'] })
  }

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
      await invalidatePanel()
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
          <div>
            <StaticSearchPicker
              label="Vehicle"
              value={vehicleRefKey}
              onChange={setVehicleRefKey}
              options={vehicleOptions}
              selectedOption={selectedVehicleOption}
              placeholder="Search vehicles…"
              disabled={vehicleRefsQuery.isLoading}
              testId="equipment-availability-vehicle-picker"
            />
            <AdvancedReferenceField
              value={vehicleRefKey}
              onChange={setVehicleRefKey}
              label="Vehicle ref key"
              testId="equipment-availability-vehicle-advanced"
            />
          </div>
          <label className="block text-sm text-slate-300" htmlFor="equipmentavailability-status-2">
          Status
          <select id="equipmentavailability-status-2"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
              value={availabilityStatus}
              onChange={(event) => setAvailabilityStatus(event.target.value)}
            >
              <option value="unavailable">Unavailable</option>
              <option value="limited">Limited</option>
              <option value="available">Available</option>
            </select>
          </label>
          <label className="block text-sm text-slate-300 sm:col-span-2" htmlFor="equipmentavailability-reason-2">
          Reason
          <input id="equipmentavailability-reason-2"
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
            <EquipmentAvailabilityRecordRow
              key={record.availabilityId}
              record={record}
              canManage={canManage}
              accessToken={accessToken}
              onChanged={invalidatePanel}
              vehicleLabel={findVehicleLabel(vehicleRefs, record.vehicleRefKey)}
            />
          ))}
        </ul>
      )}
    </section>
  )
}
