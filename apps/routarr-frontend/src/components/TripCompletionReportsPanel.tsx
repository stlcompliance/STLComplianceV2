import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import { getTripCompletionDetail, getTripCompletions } from '../api/client'

interface Props {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

export function TripCompletionReportsPanel({ accessToken, canRead }: Props) {
  const [dispatchStatus, setDispatchStatus] = useState('all')
  const [selectedTripId, setSelectedTripId] = useState<string | null>(null)

  const listQuery = useQuery({
    queryKey: ['routarr-trip-completions', accessToken, dispatchStatus],
    queryFn: () =>
      getTripCompletions(accessToken, {
        dispatchStatus: dispatchStatus === 'all' ? undefined : dispatchStatus,
      }),
    enabled: canRead,
  })

  const detailQuery = useQuery({
    queryKey: ['routarr-trip-completion-detail', accessToken, selectedTripId],
    queryFn: () => getTripCompletionDetail(accessToken, selectedTripId!),
    enabled: canRead && Boolean(selectedTripId),
  })

  if (!canRead) return null

  const list = listQuery.data?.items

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/80 p-5" data-testid="trip-completion-reports-panel">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Trip completion reports</h2>
          <p className="mt-1 text-sm text-slate-400">Completed trip rollups and completion detail browsing.</p>
        </div>
      </div>

      <label htmlFor="trip-completion-status" className="mt-4 flex items-center gap-2 text-sm text-slate-300">
        <span>Dispatch status</span>
        <select
          id="trip-completion-status"
          className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
          value={dispatchStatus}
          onChange={(event) => {
            setDispatchStatus(event.target.value)
            setSelectedTripId(null)
          }}
        >
          <option value="all">All</option>
          <option value="completed">Completed</option>
          <option value="cancelled">Cancelled</option>
          <option value="in_progress">In progress</option>
        </select>
      </label>

      {listQuery.isLoading ? <p className="mt-3 text-sm text-slate-400">Loading trip completion summaries...</p> : null}
      {listQuery.isError ? (
        <div className="mt-3">
          <ApiErrorCallout
            title="Trip completion report unavailable"
            message={getErrorMessage(listQuery.error, 'Failed to load trip completion summaries.')}
            retryLabel="Retry summary"
            onRetry={() => {
              void listQuery.refetch()
            }}
          />
        </div>
      ) : null}

      {list ? (
        <div className="mt-4 grid gap-6 lg:grid-cols-2">
          <div>
            <h3 className="text-sm font-semibold text-slate-200">Trips</h3>
            <ul className="mt-2 max-h-72 space-y-1 overflow-y-auto text-sm">
              {list.map((trip) => (
                <li key={trip.tripId}>
                  <button
                    type="button"
                    className={`w-full rounded px-2 py-1 text-left hover:bg-slate-800 ${
                      selectedTripId === trip.tripId ? 'bg-slate-800' : ''
                    }`}
                    onClick={() => setSelectedTripId(trip.tripId)}
                  >
                    {trip.tripNumber} - {trip.title}
                  </button>
                </li>
              ))}
            </ul>
          </div>
          <div>
            <h3 className="text-sm font-semibold text-slate-200">Summary</h3>
            <p className="mt-2 text-sm text-slate-400">{list.length} trip completion record(s) loaded.</p>
          </div>
        </div>
      ) : null}

      {detailQuery.data ? (
        <div className="mt-6 rounded-md border border-slate-700 bg-slate-950/50 p-3 text-sm">
          <h3 className="font-semibold text-slate-100">Trip detail</h3>
          <p className="mt-1 text-slate-300">
            {detailQuery.data.summary.tripNumber} - {detailQuery.data.summary.title}
          </p>
          <p className="text-xs text-[var(--color-text-muted)]">
            {detailQuery.data.summary.completedStopCount} of {detailQuery.data.summary.stopCount} stops complete
          </p>
          <ul className="mt-2 space-y-1 text-xs text-slate-400">
            {detailQuery.data.events.map((event) => (
              <li key={`${event.sequenceNumber}-${event.eventKind}`}>
                {event.title} - {event.eventKind}
              </li>
            ))}
          </ul>
        </div>
      ) : null}
    </section>
  )
}
