import { useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { AdvancedReferenceField, ApiErrorCallout, StaticSearchPicker, getErrorMessage, type PickerOption } from '@stl/shared-ui'

import { getDockAppointments, getTrips } from '../api/client'

type Props = {
  accessToken: string
}

function formatTimestamp(iso: string | null) {
  if (!iso) return '—'
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}

export function DockAppointmentsPanel({ accessToken }: Props) {
  const [tripId, setTripId] = useState('')
  const [lookupTripId, setLookupTripId] = useState('')

  const tripsQuery = useQuery({
    queryKey: ['routarr-dock-appointments-trips', accessToken],
    queryFn: () => getTrips(accessToken),
  })

  const tripOptions = useMemo(
    () =>
      (tripsQuery.data ?? []).map((trip) => ({
        value: trip.tripId,
        label: `${trip.tripNumber} · ${trip.title}`,
      })),
    [tripsQuery.data],
  )

  const selectedTripOption = useMemo<PickerOption | undefined>(
    () => tripOptions.find((option) => option.value === tripId),
    [tripId, tripOptions],
  )

  const appointmentsQuery = useQuery({
    queryKey: ['routarr-dock-appointments', accessToken, lookupTripId],
    queryFn: () => getDockAppointments(accessToken, lookupTripId || undefined),
  })
  const hasFilter = Boolean(lookupTripId.trim())

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/80 p-5" data-testid="dock-appointments-panel">
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Dock appointments</h2>
        <p className="mt-1 text-sm text-slate-400">
          RoutArr appointment notifications for inbound movement and arrival/departure timing.
        </p>
      </header>

      <div className="mt-4 flex flex-wrap items-end gap-2">
        <div className="min-w-[280px] flex-1">
          <StaticSearchPicker
            label="Trip"
            value={tripId}
            onChange={setTripId}
            options={tripOptions}
            selectedOption={selectedTripOption}
            placeholder="Search trips…"
            disabled={tripsQuery.isLoading}
            testId="dock-appointments-trip-picker"
          />
          <AdvancedReferenceField
            value={tripId}
            onChange={setTripId}
            label="Trip id"
            testId="dock-appointments-trip-advanced"
          />
        </div>
        <button
          type="button"
          className="rounded bg-sky-700 px-3 py-1.5 text-sm text-white disabled:opacity-50"
          disabled={appointmentsQuery.isFetching && lookupTripId === tripId.trim()}
          onClick={() => setLookupTripId(tripId.trim())}
        >
          Refresh appointments
        </button>
      </div>

      {appointmentsQuery.isError ? (
        <ApiErrorCallout
          className="mt-4"
          message={getErrorMessage(appointmentsQuery.error, 'Failed to load dock appointments.')}
          onRetry={() => void appointmentsQuery.refetch()}
          retryLabel="Retry appointments"
        />
      ) : null}

      <div className="mt-5">
        <p className="text-xs uppercase tracking-wide text-slate-500">
          {hasFilter ? `Filtered by trip ${lookupTripId}` : 'Showing all dock appointments'}
        </p>
        {appointmentsQuery.isLoading ? (
          <p className="mt-2 text-sm text-slate-500">Loading dock appointments…</p>
        ) : (appointmentsQuery.data ?? []).length === 0 ? (
          <p className="mt-2 text-sm text-slate-500">No dock appointment notifications found for this filter.</p>
        ) : (
          <ul className="mt-3 space-y-3">
            {(appointmentsQuery.data ?? []).map((item) => (
              <li key={item.dockAppointmentNotificationId} className="rounded-lg border border-slate-700 bg-slate-950/50 p-4">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <p className="text-sm font-semibold text-slate-100">{item.notificationNumber}</p>
                    <p className="text-xs text-slate-500">
                      {item.appointmentType} · {item.status} · {item.sourceProduct}
                    </p>
                  </div>
                  <span className="rounded bg-slate-800 px-2 py-0.5 text-xs uppercase tracking-wide text-slate-300">
                    {item.sourceObjectRef ?? 'untracked'}
                  </span>
                </div>
                <p className="mt-2 text-xs text-slate-400">
                  Requested {formatTimestamp(item.requestedWindowStart)} – {formatTimestamp(item.requestedWindowEnd)}
                </p>
                <p className="mt-1 text-xs text-slate-400">
                  Confirmed {formatTimestamp(item.confirmedWindowStart)} – {formatTimestamp(item.confirmedWindowEnd)}
                </p>
                <p className="mt-1 text-xs text-slate-500">
                  ETA {formatTimestamp(item.eta)} · Sent {formatTimestamp(item.sentAt)}
                </p>
                <p className="mt-2 text-xs text-slate-400">
                  Carrier {item.carrierNameSnapshot ?? '—'} · Driver {item.driverSnapshot ?? '—'} · Vehicle {item.vehicleSnapshot ?? '—'}
                </p>
                {item.rejectionReason ? (
                  <p className="mt-2 text-xs text-rose-300">Rejection: {item.rejectionReason}</p>
                ) : null}
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
