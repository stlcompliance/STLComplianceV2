import { useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { PageHeader } from '@stl/shared-ui'
import { getFieldCompanionClockStatus, submitFieldCompanionClockEvent } from '../api/client'
import { useOfflineQueue } from '../hooks/useOfflineQueue'
import { useFieldCompanionWorkspace } from '../hooks/useFieldCompanionWorkspace'

const timeZone =
  typeof Intl !== 'undefined' ? Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC' : 'UTC'

export function ClockPage() {
  const queryClient = useQueryClient()
  const { accessToken, meQuery } = useFieldCompanionWorkspace()
  const [feedback, setFeedback] = useState<string | null>(null)
  const offlineQueue = useOfflineQueue(accessToken, {
    onSyncComplete: () => {
      void queryClient.invalidateQueries({ queryKey: ['fieldcompanion-clock-status', accessToken] })
    },
  })

  const statusQuery = useQuery({
    queryKey: ['fieldcompanion-clock-status', accessToken],
    queryFn: () => getFieldCompanionClockStatus(accessToken),
    enabled: Boolean(accessToken),
    refetchInterval: 30_000,
  })

  const submitMutation = useMutation({
    mutationFn: async (eventType: 'clock_in' | 'clock_out') => {
      const now = new Date().toISOString()
      if (!offlineQueue.isOnline) {
        await offlineQueue.queueClockAction({
          eventType,
          eventTimestamp: now,
          capturedAt: now,
          timezone: timeZone,
        })
        return { queued: true as const, eventType }
      }

      const geoPoint = await tryGetGeoPoint()
      return submitFieldCompanionClockEvent(accessToken, {
        eventType,
        eventTimestamp: now,
        capturedAt: now,
        timezone: timeZone,
        idempotencyKey: crypto.randomUUID(),
        sourceDeviceId: navigator.userAgent.slice(0, 120),
        geoPoint,
        notes: null,
      })
    },
    onSuccess: (result) => {
      if ('queued' in result) {
        setFeedback(`${formatEventType(result.eventType)} queued. It will sync to StaffArr when you are back online.`)
        return
      }

      setFeedback(result.created ? `Recorded ${formatEventType(result.event.eventType)}.` : 'Punch already recorded.')
      void queryClient.invalidateQueries({ queryKey: ['fieldcompanion-clock-status', accessToken] })
    },
    onError: (error: unknown) => {
      const fallback = 'Clock action failed.'
      setFeedback(error instanceof Error ? error.message || fallback : fallback)
    },
  })

  const currentStateLabel = useMemo(
    () => formatClockState(statusQuery.data?.currentState ?? 'not_clocked_in'),
    [statusQuery.data?.currentState],
  )

  if (!accessToken) {
    return <p className="text-sm text-slate-400">Launch Field Companion to start clocking time.</p>
  }

  return (
    <div className="mx-auto max-w-4xl space-y-5">
      <PageHeader
        title="Clock"
        subtitle="Record time punches from this device."
      />

      <section className="grid gap-4 lg:grid-cols-[1.1fr_0.9fr]">
        <div className="rounded-3xl border border-slate-700 bg-slate-900/80 p-5">
          <p className="text-xs font-semibold uppercase tracking-[0.24em] text-teal-300">Current status</p>
          <h2 className="mt-3 text-3xl font-semibold text-white">{currentStateLabel}</h2>
          <p className="mt-2 text-sm text-slate-300">
            Use this screen to clock in or out. Your punches sync automatically when you are back online.
          </p>
          <p className="mt-3 text-xs text-slate-400">
            {offlineQueue.isOnline
              ? 'Online now. Clock punches submit immediately.'
              : `Offline now. New punches will queue on this device and replay automatically. ${offlineQueue.pendingCount} pending.`}
          </p>

          <div className="mt-5 grid gap-3 sm:grid-cols-2">
            <button
              type="button"
              className="min-h-12 rounded-2xl bg-emerald-600 px-4 py-3 text-sm font-semibold text-white transition hover:bg-emerald-500 disabled:opacity-50"
              disabled={submitMutation.isPending}
              onClick={() => submitMutation.mutate('clock_in')}
            >
              Clock in
            </button>
            <button
              type="button"
              className="min-h-12 rounded-2xl bg-slate-700 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-600 disabled:opacity-50"
              disabled={submitMutation.isPending}
              onClick={() => submitMutation.mutate('clock_out')}
            >
              Clock out
            </button>
          </div>

          {feedback && (
            <p className="mt-4 rounded-2xl border border-slate-700 bg-slate-950/70 px-4 py-3 text-sm text-slate-200">
              {feedback}
            </p>
          )}
        </div>

        <div className="rounded-3xl border border-slate-700 bg-slate-950/70 p-5">
          <p className="text-xs font-semibold uppercase tracking-[0.24em] text-sky-300">Worker context</p>
          <dl className="mt-4 space-y-3 text-sm text-slate-300">
            <div>
              <dt className="text-[var(--color-text-muted)]">Worker</dt>
              <dd className="text-white">{meQuery.data?.displayName ?? 'Loading…'}</dd>
            </div>
            <div>
              <dt className="text-[var(--color-text-muted)]">Timezone</dt>
              <dd className="text-white">{timeZone}</dd>
            </div>
            <div>
              <dt className="text-[var(--color-text-muted)]">Last punch</dt>
              <dd className="text-white">
                {statusQuery.data?.latestEvent
                  ? `${formatEventType(statusQuery.data.latestEvent.eventType)} at ${formatDateTime(statusQuery.data.latestEvent.eventTimestamp)}`
                  : 'No punches yet'}
              </dd>
            </div>
          </dl>
        </div>
      </section>

      <section className="rounded-3xl border border-slate-700 bg-slate-900/80 p-5">
        <div className="flex items-center justify-between gap-3">
          <div>
            <h2 className="text-lg font-semibold text-white">Recent punches</h2>
            <p className="text-sm text-slate-400">Latest clock events for this device.</p>
          </div>
        </div>

        {statusQuery.isLoading && <p className="mt-4 text-sm text-slate-400">Loading recent punches…</p>}
        {statusQuery.isError && (
          <p className="mt-4 rounded-2xl border border-rose-500/40 bg-rose-950/30 px-4 py-3 text-sm text-rose-200">
            {formatClockError(statusQuery.error)}
          </p>
        )}

        {!statusQuery.isLoading && !statusQuery.isError && (
          <div className="mt-4 space-y-3">
            {statusQuery.data?.recentEvents.length ? (
              statusQuery.data.recentEvents.map((event) => (
                <article key={event.id} className="rounded-2xl border border-slate-700 bg-slate-950/60 p-4">
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <p className="text-sm font-semibold text-white">{formatEventType(event.eventType)}</p>
                    <p className="text-xs text-slate-400">{formatDateTime(event.eventTimestamp)}</p>
                  </div>
                  <p className="mt-2 text-sm text-slate-300">
                    Captured {formatDateTime(event.capturedTimestamp)} · {event.timezone}
                  </p>
                  {(event.siteRef || event.locationRef) && (
                    <p className="mt-1 text-xs text-slate-400">
                      {event.siteRef ? `Site ${event.siteRef}` : 'No site'} · {event.locationRef ? `Location ${event.locationRef}` : 'No location'}
                    </p>
                  )}
                  {event.anomalyFlags.length > 0 && (
                    <p className="mt-2 text-xs text-amber-300">
                      Flags: {event.anomalyFlags.join(', ')}
                    </p>
                  )}
                </article>
              ))
            ) : (
              <p className="text-sm text-slate-400">No StaffArr clock events have been recorded from this mobile surface yet.</p>
            )}
          </div>
        )}
      </section>
    </div>
  )
}

function formatEventType(value: string) {
  return value.replace('_', ' ')
}

function formatClockState(value: string) {
  return value.replace('_', ' ')
}

function formatDateTime(value: string) {
  return new Date(value).toLocaleString()
}

function formatClockError(error: unknown) {
  if (error instanceof Error) {
    return error.message
  }

  return 'Failed to load clock status.'
}

async function tryGetGeoPoint() {
  if (!('geolocation' in navigator)) {
    return null
  }

  try {
    const position = await new Promise<GeolocationPosition>((resolve, reject) =>
      navigator.geolocation.getCurrentPosition(resolve, reject, {
        enableHighAccuracy: false,
        maximumAge: 60_000,
        timeout: 4_000,
      }),
    )

    return `${position.coords.latitude.toFixed(6)},${position.coords.longitude.toFixed(6)}`
  } catch {
    return null
  }
}
