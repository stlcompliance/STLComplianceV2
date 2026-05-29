import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useMemo, useState } from 'react'
import { AdvancedReferenceField, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'

import {
  createDriverAvailability,
  deleteDriverAvailability,
  getDriverAvailabilityPanel,
  listDrivers,
  updateDriverAvailability,
} from '../api/client'
import type { DriverAvailabilityPanelRow } from '../api/types'
import { fromDatetimeLocalValue, toDatetimeLocalValue } from '../lib/availabilityDateTime'
import { driverToPickerOption, findDriverLabel } from '../lib/referencePickers'

type DriverAvailabilityPanelProps = {
  accessToken: string
  scope: 'daily' | 'weekly'
  onScopeChange: (scope: 'daily' | 'weekly') => void
  canManage: boolean
  sessionPersonId: string
}

function formatTimestamp(iso: string) {
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}

type DriverAvailabilityRecordRowProps = {
  record: DriverAvailabilityPanelRow
  canManage: boolean
  accessToken: string
  onChanged: () => Promise<void>
  driverLabel?: string
}

function DriverAvailabilityRecordRow({
  record,
  canManage,
  accessToken,
  onChanged,
  driverLabel,
}: DriverAvailabilityRecordRowProps) {
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
      updateDriverAvailability(accessToken, record.availabilityId, {
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
    mutationFn: () => deleteDriverAvailability(accessToken, record.availabilityId),
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
          <label className="block text-sm text-slate-300">
            Reason
            <input
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
              value={reason}
              onChange={(event) => setReason(event.target.value)}
            />
          </label>
          <label className="block text-sm text-slate-300">
            Starts at
            <input
              type="datetime-local"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
              value={startsAt}
              onChange={(event) => setStartsAt(event.target.value)}
              required
            />
          </label>
          <label className="block text-sm text-slate-300">
            Ends at
            <input
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
            {record.availabilityStatus.replace('_', ' ')} · driver{' '}
            {driverLabel ?? record.personId.slice(0, 8) + '…'}
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
                      'Delete this driver availability window? Dispatch may treat the driver as available again.',
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

export function DriverAvailabilityPanel({
  accessToken,
  scope,
  onScopeChange,
  canManage,
  sessionPersonId,
}: DriverAvailabilityPanelProps) {
  const queryClient = useQueryClient()
  const [personId, setPersonId] = useState(sessionPersonId)
  const [availabilityStatus, setAvailabilityStatus] = useState('unavailable')
  const [reason, setReason] = useState('')

  const panelQuery = useQuery({
    queryKey: ['routarr-driver-availability', accessToken, scope],
    queryFn: () => getDriverAvailabilityPanel(accessToken, scope),
  })

  const driversQuery = useQuery({
    queryKey: ['routarr-drivers-availability', accessToken],
    queryFn: () => listDrivers(accessToken),
    enabled: canManage,
  })

  const drivers = driversQuery.data?.items ?? []
  const driverOptions = useMemo(() => drivers.map(driverToPickerOption), [drivers])
  const selectedDriverOption = useMemo((): PickerOption | undefined => {
    const label = findDriverLabel(drivers, personId)
    return label ? { value: personId, label } : undefined
  }, [drivers, personId])

  const invalidatePanel = async () => {
    await queryClient.invalidateQueries({ queryKey: ['routarr-driver-availability'] })
  }

  const createMutation = useMutation({
    mutationFn: () => {
      const now = new Date()
      const start = new Date(now)
      start.setHours(now.getHours() + 1, 0, 0, 0)
      const end = new Date(start)
      end.setHours(start.getHours() + 8)
      return createDriverAvailability(accessToken, {
        personId,
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
    return <p className="text-sm text-slate-400">Loading driver availability…</p>
  }

  if (panelQuery.isError) {
    return (
      <p className="text-sm text-red-300">
        Failed to load driver availability. Check your RoutArr entitlement and try again.
      </p>
    )
  }

  const panel = panelQuery.data!

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-950/50 p-5">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-white">Driver availability</h2>
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
              label="Driver"
              value={personId}
              onChange={setPersonId}
              options={driverOptions}
              selectedOption={selectedDriverOption}
              placeholder="Search drivers…"
              disabled={driversQuery.isLoading}
              testId="driver-availability-person-picker"
            />
            <AdvancedReferenceField
              value={personId}
              onChange={setPersonId}
              label="Person id"
              testId="driver-availability-person-advanced"
            />
          </div>
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
              placeholder="PTO, training, etc."
            />
          </label>
          <button
            type="submit"
            disabled={createMutation.isPending || !personId.trim()}
            className="rounded bg-sky-700 px-3 py-2 text-sm text-white hover:bg-sky-600 disabled:opacity-50 sm:col-span-2"
          >
            {createMutation.isPending ? 'Saving…' : 'Add availability window'}
          </button>
        </form>
      ) : null}

      {panel.records.length === 0 ? (
        <p className="text-sm text-slate-500">No driver availability records in this window.</p>
      ) : (
        <ul className="space-y-3">
          {panel.records.map((record) => (
            <DriverAvailabilityRecordRow
              key={record.availabilityId}
              record={record}
              canManage={canManage}
              accessToken={accessToken}
              onChanged={invalidatePanel}
              driverLabel={findDriverLabel(drivers, record.personId)}
            />
          ))}
        </ul>
      )}
    </section>
  )
}
