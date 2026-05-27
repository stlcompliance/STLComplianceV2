import { useQuery } from '@tanstack/react-query'

import { getRouteCalendar } from '../api/client'
import type { RouteCalendarEvent, RouteCalendarResponse } from '../api/types'

type RouteCalendarPanelProps = {
  accessToken: string
  scope: 'daily' | 'weekly'
  onScopeChange: (scope: 'daily' | 'weekly') => void
}

function formatTimestamp(iso: string | null) {
  if (!iso) return '—'
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}

function formatDayLabel(iso: string) {
  try {
    return new Date(iso).toLocaleDateString(undefined, {
      weekday: 'short',
      month: 'short',
      day: 'numeric',
    })
  } catch {
    return iso
  }
}

function eventTypeLabel(eventType: string) {
  switch (eventType) {
    case 'trip':
      return 'Trip'
    case 'route':
      return 'Route'
    case 'stop':
      return 'Stop'
    default:
      return eventType
  }
}

function EventRow({ event }: { event: RouteCalendarEvent }) {
  const highlightClass =
    event.eventType === 'trip' && event.isLate
      ? 'border-red-500/60 bg-red-950/30'
      : event.eventType === 'trip' && event.isAtRisk
        ? 'border-amber-500/60 bg-amber-950/20'
        : 'border-slate-700 bg-slate-900/40'

  return (
    <li className={`rounded border p-2 ${highlightClass}`}>
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <p className="text-sm font-medium text-slate-100">{event.label}</p>
          <p className="text-xs text-slate-500">
            {eventTypeLabel(event.eventType)} · {event.status.replace('_', ' ')}
          </p>
        </div>
        {event.eventType === 'trip' && event.isLate ? (
          <span className="text-xs font-medium text-red-300">Late</span>
        ) : null}
        {event.eventType === 'trip' && !event.isLate && event.isAtRisk ? (
          <span className="text-xs font-medium text-amber-300">At risk</span>
        ) : null}
      </div>
      <p className="mt-1 text-xs text-slate-400">
        {formatTimestamp(event.scheduledAt)}
        {event.scheduledEndAt ? ` – ${formatTimestamp(event.scheduledEndAt)}` : ''}
      </p>
      {(event.tripNumber || event.routeNumber) && (
        <p className="mt-1 text-xs text-slate-500">
          {event.tripNumber ? `Trip ${event.tripNumber}` : null}
          {event.tripNumber && event.routeNumber ? ' · ' : null}
          {event.routeNumber ? `Route ${event.routeNumber}` : null}
        </p>
      )}
    </li>
  )
}

export function RouteCalendarPanel({ accessToken, scope, onScopeChange }: RouteCalendarPanelProps) {
  const calendarQuery = useQuery({
    queryKey: ['routarr-route-calendar', accessToken, scope],
    queryFn: () => getRouteCalendar(accessToken, scope),
  })

  if (calendarQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading route calendar…</p>
  }

  if (calendarQuery.isError) {
    return (
      <p className="text-sm text-red-300">
        Failed to load route calendar: {(calendarQuery.error as Error).message}
      </p>
    )
  }

  const calendar = calendarQuery.data as RouteCalendarResponse

  return (
    <section className="space-y-6" aria-label="Route calendar">
      <div className="flex flex-wrap items-center justify-between gap-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Route calendar</h2>
          <p className="mt-1 text-sm text-slate-400">
            {scope === 'daily' ? 'Daily' : 'Weekly'} view · {formatTimestamp(calendar.windowStart)} –{' '}
            {formatTimestamp(calendar.windowEnd)} · {calendar.summary.tripCount} trip(s),{' '}
            {calendar.summary.routeCount} route(s), {calendar.summary.stopCount} stop(s)
          </p>
        </div>
        <div className="flex gap-2">
          <button
            type="button"
            className={`rounded px-3 py-1.5 text-sm ${
              scope === 'daily'
                ? 'bg-sky-600 text-white'
                : 'border border-slate-600 text-slate-300 hover:bg-slate-800'
            }`}
            onClick={() => onScopeChange('daily')}
          >
            Daily
          </button>
          <button
            type="button"
            className={`rounded px-3 py-1.5 text-sm ${
              scope === 'weekly'
                ? 'bg-sky-600 text-white'
                : 'border border-slate-600 text-slate-300 hover:bg-slate-800'
            }`}
            onClick={() => onScopeChange('weekly')}
          >
            Weekly
          </button>
        </div>
      </div>

      {(calendar.summary.lateTripCount > 0 || calendar.summary.atRiskTripCount > 0) && (
        <div className="flex flex-wrap gap-4 text-sm">
          {calendar.summary.lateTripCount > 0 ? (
            <span className="text-red-300">{calendar.summary.lateTripCount} late trip(s)</span>
          ) : null}
          {calendar.summary.atRiskTripCount > 0 ? (
            <span className="text-amber-300">{calendar.summary.atRiskTripCount} at-risk trip(s)</span>
          ) : null}
        </div>
      )}

      {calendar.days.length === 0 ? (
        <p className="text-sm text-slate-500">No calendar days in this window.</p>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {calendar.days.map((day) => (
            <div
              key={day.date}
              className="rounded-xl border border-slate-700 bg-slate-900/80 p-4"
            >
              <h3 className="text-sm font-medium text-slate-200">{formatDayLabel(day.date)}</h3>
              <p className="text-xs text-slate-500">{day.events.length} event(s)</p>
              {day.events.length === 0 ? (
                <p className="mt-3 text-xs text-slate-600">No scheduled items</p>
              ) : (
                <ul className="mt-3 space-y-2">
                  {day.events.map((event) => (
                    <EventRow key={`${day.date}-${event.eventType}-${event.entityId}`} event={event} />
                  ))}
                </ul>
              )}
            </div>
          ))}
        </div>
      )}
    </section>
  )
}
