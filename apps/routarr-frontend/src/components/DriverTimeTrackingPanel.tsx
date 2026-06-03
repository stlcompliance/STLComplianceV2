import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  createDriverPortalTimeEntry,
  getDriverPortalTimeTracking,
  updateDriverPortalTimeEntry,
} from '../api/client'
import type {
  DriverTimeEntryResponse,
  DriverTimeTrackingResponse,
} from '../api/types'
import { fromDatetimeLocalValue, toDatetimeLocalValue } from '../lib/availabilityDateTime'

type Props = {
  accessToken: string
}

function utcTodayValue() {
  return new Date().toISOString().slice(0, 10)
}

function formatTimestamp(iso: string | null | undefined) {
  if (!iso) return '—'
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}

function formatDuration(minutes: number) {
  if (minutes <= 0) return '0m'
  const hours = Math.floor(minutes / 60)
  const mins = minutes % 60
  if (hours === 0) return `${mins}m`
  return mins === 0 ? `${hours}h` : `${hours}h ${mins}m`
}

function csvEscape(value: string) {
  return `"${value.replace(/"/g, '""')}"`
}

function buildTimeTrackingCsv(panel: DriverTimeTrackingResponse) {
  const header = [
    'Date',
    'Entry Type',
    'Start At',
    'End At',
    'Duration Minutes',
    'Notes',
    'Edit Reason',
    'State',
  ]
  const rows = [
    header,
    ...panel.entries.map((entry) => [
      panel.date,
      entry.entryType,
      entry.startsAt,
      entry.endsAt ?? '',
      String(entry.durationMinutes),
      entry.notes,
      entry.editReason,
      entry.isOpen ? 'open' : 'closed',
    ]),
  ]
  return rows.map((row) => row.map(csvEscape).join(',')).join('\n')
}

function driverTimeEntryLabel(entryType: string) {
  if (entryType === 'on_duty') return 'On duty'
  if (entryType === 'off_duty') return 'Off duty'
  if (entryType === 'break') return 'Break'
  return entryType.replace(/_/g, ' ')
}

function DriverTimeEntryRow({
  accessToken,
  entry,
  onChanged,
}: {
  accessToken: string
  entry: DriverTimeEntryResponse
  onChanged: () => Promise<void>
}) {
  const [editing, setEditing] = useState(false)
  const [entryType, setEntryType] = useState(entry.entryType)
  const [startsAt, setStartsAt] = useState(toDatetimeLocalValue(entry.startsAt))
  const [endsAt, setEndsAt] = useState(entry.endsAt ? toDatetimeLocalValue(entry.endsAt) : '')
  const [notes, setNotes] = useState(entry.notes)
  const [editReason, setEditReason] = useState('')
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!editing) {
      setEntryType(entry.entryType)
      setStartsAt(toDatetimeLocalValue(entry.startsAt))
      setEndsAt(entry.endsAt ? toDatetimeLocalValue(entry.endsAt) : '')
      setNotes(entry.notes)
      setEditReason('')
    }
  }, [entry, editing])

  const updateMutation = useMutation({
    mutationFn: () =>
      updateDriverPortalTimeEntry(accessToken, entry.entryId, {
        entryType,
        startsAt: fromDatetimeLocalValue(startsAt),
        endsAt: endsAt ? fromDatetimeLocalValue(endsAt) : null,
        notes: notes || null,
        editReason,
      }),
    onSuccess: async () => {
      setError(null)
      setEditing(false)
      await onChanged()
    },
    onError: (err: Error) => setError(err.message),
  })

  if (editing) {
    return (
      <li className="rounded border border-slate-700 bg-slate-950/50 p-3">
        <form
          className="grid gap-3 sm:grid-cols-2"
          onSubmit={(event) => {
            event.preventDefault()
            updateMutation.mutate()
          }}
        >
          <label className="block text-sm text-slate-300" htmlFor={`time-entry-type-${entry.entryId}`}>
            Edit type
            <select
              id={`time-entry-type-${entry.entryId}`}
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
              value={entryType}
              onChange={(event) => setEntryType(event.target.value)}
            >
              <option value="on_duty">On duty</option>
              <option value="off_duty">Off duty</option>
              <option value="break">Break</option>
            </select>
          </label>
          <label className="block text-sm text-slate-300" htmlFor={`time-entry-start-${entry.entryId}`}>
            Edit start
            <input
              id={`time-entry-start-${entry.entryId}`}
              type="datetime-local"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
              value={startsAt}
              onChange={(event) => setStartsAt(event.target.value)}
              required
            />
          </label>
          <label className="block text-sm text-slate-300" htmlFor={`time-entry-end-${entry.entryId}`}>
            Edit end
            <input
              id={`time-entry-end-${entry.entryId}`}
              type="datetime-local"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
              value={endsAt}
              onChange={(event) => setEndsAt(event.target.value)}
            />
          </label>
          <label className="block text-sm text-slate-300 sm:col-span-2" htmlFor={`time-entry-notes-${entry.entryId}`}>
            Edit notes
            <textarea
              id={`time-entry-notes-${entry.entryId}`}
              className="mt-1 min-h-[72px] w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
              value={notes}
              onChange={(event) => setNotes(event.target.value)}
            />
          </label>
          <label className="block text-sm text-slate-300 sm:col-span-2" htmlFor={`time-entry-reason-${entry.entryId}`}>
            Edit reason
            <input
              id={`time-entry-reason-${entry.entryId}`}
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
              value={editReason}
              onChange={(event) => setEditReason(event.target.value)}
              placeholder="Reason required for manual correction"
              required
            />
          </label>
          <div className="flex flex-wrap gap-2 sm:col-span-2">
            <button
              type="submit"
              disabled={updateMutation.isPending || !editReason.trim()}
              className="rounded bg-sky-700 px-3 py-1.5 text-sm text-white hover:bg-sky-600 disabled:opacity-50"
            >
              {updateMutation.isPending ? 'Saving…' : 'Save correction'}
            </button>
            <button
              type="button"
              disabled={updateMutation.isPending}
              className="rounded border border-slate-600 px-3 py-1.5 text-sm text-slate-300 hover:bg-slate-800 disabled:opacity-50"
              onClick={() => {
                setError(null)
                setEditing(false)
              }}
            >
              Cancel
            </button>
          </div>
        </form>
        {error ? (
          <p className="mt-2 text-xs text-red-300" role="alert">
            {error}
          </p>
        ) : null}
      </li>
    )
  }

  return (
    <li className="rounded border border-slate-700 bg-slate-950/50 p-3">
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <p className="text-sm font-medium text-slate-100">
            {driverTimeEntryLabel(entry.entryType)} · {formatDuration(entry.durationMinutes)}
          </p>
          <p className="text-xs text-slate-500">
            {formatTimestamp(entry.startsAt)} → {formatTimestamp(entry.endsAt)}
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <span
            className={`text-xs font-medium ${
              entry.isOpen ? 'text-amber-300' : 'text-emerald-300'
            }`}
          >
            {entry.isOpen ? 'Open' : 'Closed'}
          </span>
          <button
            type="button"
            className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-300 hover:bg-slate-800"
            onClick={() => setEditing(true)}
          >
            Edit
          </button>
        </div>
      </div>
      {entry.notes ? <p className="mt-2 text-xs text-slate-400">{entry.notes}</p> : null}
      {entry.editReason ? <p className="mt-1 text-xs text-slate-500">Reason: {entry.editReason}</p> : null}
      {error ? (
        <p className="mt-2 text-xs text-red-300" role="alert">
          {error}
        </p>
      ) : null}
    </li>
  )
}

export function DriverTimeTrackingPanel({ accessToken }: Props) {
  const queryClient = useQueryClient()
  const [date, setDate] = useState(utcTodayValue())
  const [entryType, setEntryType] = useState('on_duty')
  const [startsAt, setStartsAt] = useState(toDatetimeLocalValue(new Date().toISOString()))
  const [endsAt, setEndsAt] = useState('')
  const [notes, setNotes] = useState('')
  const [createError, setCreateError] = useState<string | null>(null)

  const trackingQuery = useQuery({
    queryKey: ['driver-portal-time-tracking', accessToken, date],
    queryFn: () => getDriverPortalTimeTracking(accessToken, date),
  })

  const refreshTracking = async () => {
    await queryClient.invalidateQueries({ queryKey: ['driver-portal-time-tracking'] })
  }

  const createMutation = useMutation({
    mutationFn: () =>
      createDriverPortalTimeEntry(accessToken, {
        entryType,
        startsAt: fromDatetimeLocalValue(startsAt),
        endsAt: endsAt ? fromDatetimeLocalValue(endsAt) : null,
        notes: notes || null,
      }),
    onSuccess: async () => {
      setCreateError(null)
      setNotes('')
      await refreshTracking()
    },
    onError: (err: Error) => setCreateError(err.message),
  })

  const exportCsv = () => {
    if (!trackingQuery.data || trackingQuery.data.entries.length === 0 || typeof window === 'undefined') {
      return
    }

    const blob = new Blob([buildTimeTrackingCsv(trackingQuery.data)], { type: 'text/csv;charset=utf-8' })
    const url = URL.createObjectURL(blob)
    const anchor = document.createElement('a')
    anchor.href = url
    anchor.download = `driver-time-${trackingQuery.data.date}.csv`
    anchor.click()
    URL.revokeObjectURL(url)
  }

  if (trackingQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading driver time tracking…</p>
  }

  if (trackingQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(trackingQuery.error, 'Failed to load driver time tracking.')}
        onRetry={() => void trackingQuery.refetch()}
        retryLabel="Retry time tracking"
      />
    )
  }

  const panel = trackingQuery.data!

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-950/50 p-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h3 className="text-lg font-semibold text-white">Time tracking</h3>
          <p className="text-sm text-slate-400">
            Daily operational time log for on-duty, off-duty, and break intervals.
          </p>
        </div>
        <label className="block text-sm text-slate-300" htmlFor="driver-time-date">
          Date
          <input
            id="driver-time-date"
            type="date"
            className="mt-1 rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
            value={date}
            onChange={(event) => setDate(event.target.value)}
          />
        </label>
      </div>

      <div className="mt-4 grid gap-3 sm:grid-cols-5">
        <div className="rounded border border-slate-700 p-3 text-center">
          <p className="text-xs text-slate-500">Entries</p>
          <p className="text-xl font-semibold text-slate-100" data-testid="driver-time-summary-entries">
            {panel.summary.entryCount}
          </p>
        </div>
        <div className="rounded border border-slate-700 p-3 text-center">
          <p className="text-xs text-slate-500">On duty</p>
          <p className="text-xl font-semibold text-slate-100" data-testid="driver-time-summary-on-duty">
            {formatDuration(panel.summary.onDutyMinutes)}
          </p>
        </div>
        <div className="rounded border border-slate-700 p-3 text-center">
          <p className="text-xs text-slate-500">Off duty</p>
          <p className="text-xl font-semibold text-slate-100" data-testid="driver-time-summary-off-duty">
            {formatDuration(panel.summary.offDutyMinutes)}
          </p>
        </div>
        <div className="rounded border border-slate-700 p-3 text-center">
          <p className="text-xs text-slate-500">Break</p>
          <p className="text-xl font-semibold text-slate-100" data-testid="driver-time-summary-break">
            {formatDuration(panel.summary.breakMinutes)}
          </p>
        </div>
        <div className="rounded border border-slate-700 p-3 text-center">
          <p className="text-xs text-slate-500">Open entries</p>
          <p className="text-xl font-semibold text-slate-100" data-testid="driver-time-summary-open">
            {panel.summary.openEntryCount}
          </p>
        </div>
      </div>

      <div className="mt-4 rounded border border-slate-700 bg-slate-900/40 p-3" data-testid="driver-time-summary-card">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <div>
            <p className="text-sm font-medium text-slate-100">
              {panel.summary.shortHaulCandidate ? 'Short-haul candidate' : 'Short-haul exception'}
            </p>
            <p className="text-xs text-slate-500">{panel.summary.summaryNote}</p>
          </div>
          <button
            type="button"
            className="rounded border border-slate-600 px-3 py-1.5 text-xs text-slate-200 hover:bg-slate-800 disabled:opacity-50"
            disabled={panel.entries.length === 0}
            onClick={exportCsv}
          >
            Download CSV
          </button>
        </div>
      </div>

      <form
        className="mt-4 grid gap-3 rounded-lg border border-slate-700 bg-slate-900/40 p-4 sm:grid-cols-2"
        onSubmit={(event) => {
          event.preventDefault()
          createMutation.mutate()
        }}
      >
        <label className="block text-sm text-slate-300" htmlFor="driver-time-entry-type">
          Type
          <select
            id="driver-time-entry-type"
            className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
            value={entryType}
            onChange={(event) => setEntryType(event.target.value)}
          >
            <option value="on_duty">On duty</option>
            <option value="off_duty">Off duty</option>
            <option value="break">Break</option>
          </select>
        </label>
        <label className="block text-sm text-slate-300" htmlFor="driver-time-starts-at">
          Start
          <input
            id="driver-time-starts-at"
            type="datetime-local"
            className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
            value={startsAt}
            onChange={(event) => setStartsAt(event.target.value)}
            required
          />
        </label>
        <label className="block text-sm text-slate-300" htmlFor="driver-time-ends-at">
          End
          <input
            id="driver-time-ends-at"
            type="datetime-local"
            className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
            value={endsAt}
            onChange={(event) => setEndsAt(event.target.value)}
          />
        </label>
        <label className="block text-sm text-slate-300 sm:col-span-2" htmlFor="driver-time-notes">
          Notes
          <textarea
            id="driver-time-notes"
            className="mt-1 min-h-[80px] w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm"
            value={notes}
            onChange={(event) => setNotes(event.target.value)}
            placeholder="Optional context, route note, or correction details"
          />
        </label>
        <div className="sm:col-span-2">
          <button
            type="submit"
            disabled={createMutation.isPending || !startsAt}
            className="rounded bg-sky-700 px-3 py-2 text-sm text-white hover:bg-sky-600 disabled:opacity-50"
          >
            {createMutation.isPending ? 'Saving…' : 'Add time entry'}
          </button>
        </div>
      </form>

      {createError ? (
        <p className="mt-3 text-sm text-red-400" role="alert">
          {createError}
        </p>
      ) : null}

      <div className="mt-4">
        <h4 className="text-sm font-semibold text-slate-200">Entries</h4>
        {panel.entries.length === 0 ? (
          <p className="mt-2 text-sm text-slate-500">No time entries for this day.</p>
        ) : (
          <ul className="mt-2 space-y-2">
            {panel.entries.map((entry) => (
              <DriverTimeEntryRow
                key={entry.entryId}
                accessToken={accessToken}
                entry={entry}
                onChanged={refreshTracking}
              />
            ))}
          </ul>
        )}
      </div>

      <p className="mt-3 text-xs text-slate-500">
        RoutArr tracks operational time intervals for short-haul-style workflows and does not claim
        to be a certified ELD.
      </p>
    </section>
  )
}
