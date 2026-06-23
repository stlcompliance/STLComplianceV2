import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  getRoute,
  getRoutes,
  getTripByNumber,
  getTripExecutionSummary,
  listDispatchExceptions,
} from '../api/client'

type Props = {
  accessToken: string
  canRead: boolean
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-3 py-2">
      <p className="text-xs text-[var(--color-text-muted)]">{label}</p>
      <p className="text-lg font-semibold text-[var(--color-text-primary)]">{value}</p>
    </div>
  )
}

function formatTimestamp(iso: string | null | undefined): string {
  if (!iso) return 'Not recorded'
  const date = new Date(iso)
  if (Number.isNaN(date.getTime())) return 'Not recorded'
  return date.toLocaleString()
}

function humanize(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  return value.replace(/[_-]+/g, ' ')
}

function isPendingStopStatus(status: string): boolean {
  return !['completed', 'skipped', 'cancelled'].includes(status.toLowerCase())
}

export function CustomerPortalPanel({ accessToken, canRead }: Props) {
  const [tripNumberInput, setTripNumberInput] = useState('')
  const [searchTripNumber, setSearchTripNumber] = useState('')

  const tripQuery = useQuery({
    queryKey: ['routarr-customer-portal-trip', accessToken, searchTripNumber],
    queryFn: () => getTripByNumber(accessToken, searchTripNumber),
    enabled: canRead && Boolean(searchTripNumber),
  })

  const routesQuery = useQuery({
    queryKey: ['routarr-customer-portal-routes', accessToken, tripQuery.data?.tripId],
    queryFn: async () => {
      const routes = await getRoutes(accessToken, tripQuery.data!.tripId)
      return Promise.all(routes.map((route) => getRoute(accessToken, route.routeId)))
    },
    enabled: canRead && Boolean(tripQuery.data?.tripId),
  })

  const executionQuery = useQuery({
    queryKey: ['routarr-customer-portal-execution', accessToken, tripQuery.data?.tripId],
    queryFn: () => getTripExecutionSummary(accessToken, tripQuery.data!.tripId),
    enabled: canRead && Boolean(tripQuery.data?.tripId),
  })

  const exceptionsQuery = useQuery({
    queryKey: ['routarr-customer-portal-exceptions', accessToken, tripQuery.data?.tripId],
    queryFn: () => listDispatchExceptions(accessToken),
    enabled: canRead && Boolean(tripQuery.data?.tripId),
  })

  if (!canRead) {
    return null
  }

  const routes = routesQuery.data ?? []
  const routeCount = routes.length
  const pendingStopCount = routes.reduce(
    (total, route) => total + route.stops.filter((stop) => isPendingStopStatus(stop.stopStatus)).length,
    0,
  )
  const openExceptions = (exceptionsQuery.data?.items ?? []).filter(
    (item) => item.tripId === tripQuery.data?.tripId,
  )
  const openExceptionCount = openExceptions.length
  const proofCount = executionQuery.data?.proofs.length ?? 0
  const dvirCount = executionQuery.data?.dvirInspections.length ?? 0
  const stopProgressLabel =
    openExceptionCount > 0 ? 'needs attention' : pendingStopCount > 0 ? 'in progress' : 'on track'

  return (
    <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5" data-testid="customer-portal-panel">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Customer portal</h2>
          <p className="mt-1 text-sm text-[var(--color-text-muted)]">
            Read-only shipment visibility for trip status, stop progress, and captured proof history.
          </p>
        </div>
      </div>

      <div className="mt-4 flex flex-wrap gap-3">
        <label className="flex-1 min-w-[16rem] text-sm text-[var(--color-text-secondary)]" htmlFor="customer-portal-trip-number">
          Trip number
          <input
            id="customer-portal-trip-number"
            className="mt-1 w-full rounded border border-[var(--color-border-default)] bg-[var(--color-field-bg)] px-3 py-2 text-[var(--color-text-primary)]"
            value={tripNumberInput}
            onChange={(event) => setTripNumberInput(event.target.value)}
            placeholder="Enter trip number"
          />
        </label>
        <div className="flex items-end">
          <button
            type="button"
            className="rounded bg-[var(--color-accent)] px-4 py-2 text-sm font-semibold text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
            disabled={!tripNumberInput.trim()}
            onClick={() => setSearchTripNumber(tripNumberInput.trim())}
          >
            Search trip
          </button>
        </div>
      </div>

      {tripQuery.isLoading ? <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading trip…</p> : null}

      {tripQuery.isError ? (
        <div className="mt-3">
          <ApiErrorCallout
            title="Trip lookup unavailable"
            message={getErrorMessage(tripQuery.error, 'Failed to load the requested trip.')}
            retryLabel="Retry lookup"
            onRetry={() => {
              void tripQuery.refetch()
            }}
          />
        </div>
      ) : null}

      {tripQuery.data ? (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Dispatch status" value={humanize(tripQuery.data.dispatchStatus)} />
            <MetricCard
              label="Schedule"
              value={`${formatTimestamp(tripQuery.data.scheduledStartAt)} → ${formatTimestamp(tripQuery.data.scheduledEndAt)}`}
            />
            <MetricCard label="Loads" value={String(tripQuery.data.loads.length)} />
            <MetricCard
              label="Proofs / DVIRs"
              value={executionQuery.isLoading ? 'Loading…' : `${proofCount} / ${dvirCount}`}
            />
          </div>

          <div className="mt-6 grid gap-4 lg:grid-cols-2">
            <div className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4 text-sm text-[var(--color-text-secondary)]">
              <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">Trip summary</h3>
              <p className="mt-2">
                {tripQuery.data.tripNumber} — {tripQuery.data.title}
              </p>
              <p className="mt-1 text-[var(--color-text-muted)]">{tripQuery.data.description || 'No trip description recorded.'}</p>
              <p className="mt-3 text-xs text-[var(--color-text-muted)]">
                Driver {tripQuery.data.assignedDriverPersonId ?? 'unassigned'} · Vehicle{' '}
                {tripQuery.data.vehicleRefKey ?? 'unassigned'}
              </p>
              {routesQuery.data && exceptionsQuery.data ? (
                <p className="mt-2 text-xs text-[var(--color-text-muted)]">
                  {routeCount} route(s) · {pendingStopCount} pending stop(s) · {openExceptionCount} open
                  {' '}exception(s)
                </p>
              ) : null}
            </div>

            <div className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4 text-sm text-[var(--color-text-secondary)]">
              <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">Loads</h3>
              {tripQuery.data.loads.length === 0 ? (
                <p className="mt-2 text-xs text-[var(--color-text-muted)]">No loads recorded for this trip.</p>
              ) : (
                <ul className="mt-2 space-y-2">
                  {tripQuery.data.loads.map((load) => (
                    <li key={load.loadId} className="rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-3">
                      <p className="font-medium text-[var(--color-text-primary)]">
                        {load.loadKey} · {load.loadType}
                      </p>
                      <p className="text-xs text-[var(--color-text-muted)]">
                        {load.originLabel} → {load.destinationLabel}
                      </p>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          </div>

          <div className="mt-6 grid gap-4 lg:grid-cols-2">
            <div className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4 text-sm text-[var(--color-text-secondary)]">
              <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">Stop progress</h3>
              {routesQuery.isLoading || exceptionsQuery.isLoading ? (
                <p className="mt-2 text-xs text-[var(--color-text-muted)]">Loading stop progress…</p>
              ) : routesQuery.data && exceptionsQuery.data ? (
                <>
                  <p className="mt-2 text-xs text-[var(--color-text-muted)]">
                    {routeCount} route(s) · {pendingStopCount} pending stop(s) · {openExceptionCount} open
                    {' '}exception(s) · {stopProgressLabel}
                  </p>
                  <p className="mt-2 text-xs text-[var(--color-text-muted)]">
                    Scheduled {formatTimestamp(tripQuery.data.scheduledStartAt)} →{' '}
                    {formatTimestamp(tripQuery.data.scheduledEndAt)}
                  </p>
                  <p className="mt-2 text-xs text-[var(--color-text-muted)]">
                    Dispatched {formatTimestamp(tripQuery.data.dispatchedAt)} · Started{' '}
                    {formatTimestamp(tripQuery.data.startedAt)} · Completed{' '}
                    {formatTimestamp(tripQuery.data.completedAt)}
                  </p>
                </>
              ) : (
                <p className="mt-2 text-xs text-[var(--color-text-muted)]">Stop progress is unavailable right now.</p>
              )}
            </div>

            <div className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4 text-sm text-[var(--color-text-secondary)]">
              <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">Proof archive</h3>
              {executionQuery.isLoading ? (
                <p className="mt-2 text-xs text-[var(--color-text-muted)]">Loading proof archive…</p>
              ) : executionQuery.data ? (
                <>
                  <p className="mt-2 text-xs text-[var(--color-text-muted)]">
                    {executionQuery.data.hasPreTripDvir ? 'Pre-trip DVIR captured' : 'Pre-trip DVIR missing'} ·{' '}
                    {executionQuery.data.hasPostTripDvir ? 'Post-trip DVIR captured' : 'Post-trip DVIR missing'}
                  </p>
                  <div className="mt-3 space-y-3">
                    <div>
                      <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Proofs</p>
                      {executionQuery.data.proofs.length === 0 ? (
                        <p className="text-xs text-[var(--color-text-muted)]">No proofs captured yet.</p>
                      ) : (
                        <ul className="mt-2 space-y-1 text-xs text-[var(--color-text-muted)]">
                          {executionQuery.data.proofs.map((proof) => (
                            <li key={proof.proofId}>
                              {proof.proofType} · {proof.referenceKey || 'No reference'} · {humanize(proof.reviewStatus)}
                            </li>
                          ))}
                        </ul>
                      )}
                    </div>
                    <div>
                      <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">DVIRs</p>
                      {executionQuery.data.dvirInspections.length === 0 ? (
                        <p className="text-xs text-[var(--color-text-muted)]">No DVIR inspections captured yet.</p>
                      ) : (
                        <ul className="mt-2 space-y-1 text-xs text-[var(--color-text-muted)]">
                          {executionQuery.data.dvirInspections.map((dvir) => (
                            <li key={dvir.dvirId}>
                              {dvir.phase} · {humanize(dvir.result)} · {formatTimestamp(dvir.submittedAt)}
                            </li>
                          ))}
                        </ul>
                      )}
                    </div>
                  </div>
                </>
              ) : (
                <p className="mt-2 text-xs text-[var(--color-text-muted)]">Proof archive is unavailable right now.</p>
              )}
            </div>
          </div>
        </>
      ) : null}
    </section>
  )
}
