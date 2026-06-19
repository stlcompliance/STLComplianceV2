import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useMemo, useState } from 'react'
import { Check, X } from 'lucide-react'
import { Link } from 'react-router-dom'

import {
  applyDispatchCloseout,
  getDispatchCloseoutAudit,
  getDispatchCloseoutChecklists,
  getDispatchCloseoutSummary,
  previewDispatchCloseout,
} from '../api/client'
import type {
  DispatchCloseoutPreviewResponse,
  DispatchCloseoutRequest,
  DispatchCloseoutTripChecklist,
} from '../api/types'

type DispatchCloseoutPanelProps = {
  accessToken: string
  scope: 'daily' | 'weekly'
  canAssign: boolean
}

function buildCloseoutPayload(
  scope: 'daily' | 'weekly',
  tripDisposition: 'complete' | 'cancel',
  stopDisposition: 'skip' | 'complete',
  selectedTripIds: string[],
): DispatchCloseoutRequest {
  return {
    scope,
    remainingTripDisposition: tripDisposition,
    openStopDisposition: stopDisposition,
    tripIds: selectedTripIds.length > 0 ? selectedTripIds : null,
  }
}

export function DispatchCloseoutPanel({ accessToken, scope, canAssign }: DispatchCloseoutPanelProps) {
  const queryClient = useQueryClient()
  const [tripDisposition, setTripDisposition] = useState<'complete' | 'cancel'>('cancel')
  const [stopDisposition, setStopDisposition] = useState<'skip' | 'complete'>('skip')
  const [selectedTripIds, setSelectedTripIds] = useState<string[]>([])
  const [expandedTripId, setExpandedTripId] = useState<string | null>(null)
  const [preview, setPreview] = useState<DispatchCloseoutPreviewResponse | null>(null)
  const [statusMessage, setStatusMessage] = useState<string | null>(null)

  const summaryQuery = useQuery({
    queryKey: ['routarr-closeout-summary', accessToken, scope],
    queryFn: () => getDispatchCloseoutSummary(accessToken, scope),
    enabled: canAssign,
  })

  const checklistsQuery = useQuery({
    queryKey: ['routarr-closeout-checklists', accessToken, scope, tripDisposition],
    queryFn: () => getDispatchCloseoutChecklists(accessToken, scope, tripDisposition),
    enabled: canAssign,
  })

  const auditQuery = useQuery({
    queryKey: ['routarr-closeout-audit', accessToken],
    queryFn: () => getDispatchCloseoutAudit(accessToken, 12),
    enabled: canAssign,
  })

  const openTrips = summaryQuery.data?.openTrips

  useEffect(() => {
    setSelectedTripIds((current) =>
      current.filter((id) => (openTrips ?? []).some((trip) => trip.tripId === id)),
    )
  }, [openTrips])

  const checklistByTripId = useMemo(() => {
    const map = new Map<string, DispatchCloseoutTripChecklist>()
    for (const checklist of checklistsQuery.data?.trips ?? []) {
      map.set(checklist.tripId, checklist)
    }
    return map
  }, [checklistsQuery.data])

  const bulkMode = selectedTripIds.length > 0

  const previewMutation = useMutation({
    mutationFn: () =>
      previewDispatchCloseout(
        accessToken,
        buildCloseoutPayload(scope, tripDisposition, stopDisposition, selectedTripIds),
      ),
    onSuccess: (response) => {
      setPreview(response)
      const scopeLabel = bulkMode ? `${selectedTripIds.length} selected` : 'all open'
      setStatusMessage(
        `Preview (${scopeLabel}): ${response.summary.tripsCanApply}/${response.summary.tripCount} trips, ` +
          `${response.summary.stopsCanApply}/${response.summary.stopCount} stops, ` +
          `${response.summary.routesCanApply}/${response.summary.routeCount} routes ready.`,
      )
    },
    onError: (error: Error) => {
      setPreview(null)
      setStatusMessage(error.message)
    },
  })

  const applyMutation = useMutation({
    mutationFn: () =>
      applyDispatchCloseout(
        accessToken,
        buildCloseoutPayload(scope, tripDisposition, stopDisposition, selectedTripIds),
      ),
    onSuccess: async (response) => {
      setPreview(null)
      setSelectedTripIds([])
      setStatusMessage(
        `Closeout applied: ${response.summary.tripsCanApply} trips, ` +
          `${response.summary.stopsCanApply} stops, ` +
          `${response.summary.routesCanApply} routes updated.`,
      )
      await queryClient.invalidateQueries({ queryKey: ['routarr-closeout-summary'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-closeout-checklists'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-closeout-audit'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-trips'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-routes'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-dispatch-board'] })
    },
    onError: (error: Error) => {
      setStatusMessage(error.message)
    },
  })

  if (!canAssign) {
    return null
  }

  const summary = summaryQuery.data
  const openTripRows = openTrips ?? []
  const openRouteRows = summary?.openRoutes ?? []
  const notReadyCount = (checklistsQuery.data?.trips ?? []).filter((x) => !x.readyForCloseout).length

  const toggleTrip = (tripId: string) => {
    setSelectedTripIds((current) =>
      current.includes(tripId) ? current.filter((id) => id !== tripId) : [...current, tripId],
    )
    setPreview(null)
  }

  const selectAllOpen = () => {
    setSelectedTripIds(openTripRows.map((trip) => trip.tripId))
    setPreview(null)
  }

  const clearSelection = () => {
    setSelectedTripIds([])
    setPreview(null)
  }

  return (
    <section
      className="rounded-xl border border-amber-700/60 bg-amber-950/20 p-4"
      data-testid="dispatch-closeout-panel"
    >
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-white">End-of-day closeout</h2>
          <p className="text-sm text-slate-400">
            Review per-trip checklists, close selected trips or all open work in the {scope} window.
          </p>
        </div>
        {summary ? (
          <div className="text-right text-sm text-slate-300">
            <div>
              Open: {summary.counts.openTrips} trips · {summary.counts.openRoutes} routes ·{' '}
              {summary.counts.openStops} stops
            </div>
            {notReadyCount > 0 ? (
              <div className="text-amber-300">{notReadyCount} trip(s) not checklist-ready</div>
            ) : null}
          </div>
        ) : null}
      </div>

      <div className="mb-4 grid gap-4 md:grid-cols-2">
        <label className="block text-sm text-slate-300" htmlFor="dispatchcloseout-remaining-trips">
          Remaining trips
          <select id="dispatchcloseout-remaining-trips"
            className="mt-1 w-full rounded border border-slate-600 bg-slate-900 px-2 py-1.5 text-white"
            value={tripDisposition}
            onChange={(event) => {
              setTripDisposition(event.target.value as 'complete' | 'cancel')
              setPreview(null)
            }}
          >
            <option value="cancel">Cancel all open trips</option>
            <option value="complete">Complete in-flight trips (cancel planned)</option>
          </select>
        </label>
        <label className="block text-sm text-slate-300" htmlFor="dispatchcloseout-open-stops">
          Open stops
          <select id="dispatchcloseout-open-stops"
            className="mt-1 w-full rounded border border-slate-600 bg-slate-900 px-2 py-1.5 text-white"
            value={stopDisposition}
            onChange={(event) => {
              setStopDisposition(event.target.value as 'skip' | 'complete')
              setPreview(null)
            }}
          >
            <option value="skip">Skip pending / arrived stops</option>
            <option value="complete">Complete arrived stops (skip pending)</option>
          </select>
        </label>
      </div>

      {openTripRows.length > 0 ? (
        <div className="mb-4 rounded border border-slate-700">
          <div className="flex flex-wrap items-center justify-between gap-2 border-b border-slate-700 bg-slate-900/60 px-3 py-2">
            <span className="text-sm font-medium text-slate-200">Trip closeout checklist</span>
            <div className="flex gap-2">
              <button
                type="button"
                className="rounded bg-slate-700 px-2 py-1 text-xs text-white hover:bg-slate-600"
                onClick={selectAllOpen}
              >
                Select all open
              </button>
              <button
                type="button"
                className="rounded bg-slate-700 px-2 py-1 text-xs text-white hover:bg-slate-600 disabled:opacity-50"
                disabled={selectedTripIds.length === 0}
                onClick={clearSelection}
              >
                Clear selection
              </button>
            </div>
          </div>
          <ul className="max-h-64 divide-y divide-slate-800 overflow-y-auto">
            {openTripRows.map((trip) => {
              const checklist = checklistByTripId.get(trip.tripId)
              const isSelected = selectedTripIds.includes(trip.tripId)
              const isExpanded = expandedTripId === trip.tripId
              return (
                <li key={trip.tripId} className="px-3 py-2">
                  <div className="flex flex-wrap items-center gap-2">
                    <input id="dispatchcloseout-input-field"
                      type="checkbox"
                      checked={isSelected}
                      onChange={() => toggleTrip(trip.tripId)}
                      aria-label={`Select ${trip.tripNumber}`}
                    />
                    <button
                      type="button"
                      className="text-left text-sm text-slate-200 hover:text-white"
                      onClick={() =>
                        setExpandedTripId(isExpanded ? null : trip.tripId)
                      }
                    >
                      <span className="font-medium">{trip.tripNumber}</span>
                      <span className="ml-2 text-slate-400">{trip.dispatchStatus}</span>
                      {checklist ? (
                        <span
                          className={
                            checklist.readyForCloseout
                              ? 'ml-2 text-emerald-400'
                              : 'ml-2 text-amber-400'
                          }
                        >
                          {checklist.readyForCloseout ? 'ready' : 'blocked'}
                        </span>
                      ) : null}
                    </button>
                  </div>
                  {isExpanded && checklist ? (
                    <ul className="mt-2 space-y-1 pl-6 text-xs text-slate-300">
                      {checklist.items.map((item) => (
                        <li key={item.key} className="flex items-start gap-2">
                          {item.satisfied ? (
                            <Check className="mt-0.5 h-3.5 w-3.5 shrink-0 text-emerald-400" />
                          ) : (
                            <X
                              className={`mt-0.5 h-3.5 w-3.5 shrink-0 ${
                                item.required ? 'text-amber-400' : 'text-[var(--color-text-muted)]'
                              }`}
                            />
                          )}
                          <span>
                            {item.label}
                            {item.required ? '' : ' (recommended)'}
                            {item.detail ? ` — ${item.detail}` : ''}
                          </span>
                        </li>
                      ))}
                    </ul>
                  ) : null}
                </li>
              )
            })}
          </ul>
          <p className="border-t border-slate-700 px-3 py-2 text-xs text-slate-400">
            {bulkMode
              ? `Bulk closeout: ${selectedTripIds.length} trip(s) selected. Stops/routes on those trips only.`
              : 'No trips selected — preview/apply closes all open work in this window.'}
          </p>
        </div>
      ) : null}

      {openRouteRows.length > 0 ? (
        <div className="mb-4 rounded border border-slate-700" data-testid="closeout-open-routes">
          <div className="border-b border-slate-700 bg-slate-900/60 px-3 py-2">
            <span className="text-sm font-medium text-slate-200">Open routes in window</span>
          </div>
          <ul className="max-h-40 divide-y divide-slate-800 overflow-y-auto">
            {openRouteRows.map((route) => (
              <li key={route.routeId} className="flex flex-wrap items-center gap-2 px-3 py-2 text-sm">
                <span className="font-medium text-slate-200">{route.routeNumber}</span>
                <span className="text-slate-400">{route.routeStatus}</span>
                <span className="text-xs text-[var(--color-text-muted)]">
                  {route.openStopCount} open stop{route.openStopCount === 1 ? '' : 's'}
                </span>
                {route.tripId ? (
                  <Link
                    to={`/trips/${route.tripId}`}
                    className="ml-auto text-xs text-teal-300 hover:text-teal-200"
                  >
                    Trip workspace →
                  </Link>
                ) : null}
              </li>
            ))}
          </ul>
        </div>
      ) : null}

      <div className="flex flex-wrap gap-2">
        <button
          type="button"
          className="rounded bg-slate-700 px-3 py-1.5 text-sm text-white hover:bg-slate-600 disabled:opacity-50"
          disabled={previewMutation.isPending || summaryQuery.isLoading}
          onClick={() => previewMutation.mutate()}
        >
          Preview closeout
        </button>
        <button
          type="button"
          className="rounded bg-amber-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-amber-600 disabled:opacity-50"
          disabled={
            applyMutation.isPending ||
            previewMutation.isPending ||
            !preview ||
            preview.summary.tripsCanApply + preview.summary.stopsCanApply === 0
          }
          onClick={() => {
            const target = bulkMode
              ? `${selectedTripIds.length} selected trip(s)`
              : 'all open work in this window'
            if (
              window.confirm(
                `Apply end-of-day closeout for ${target}? This cannot be undone.`,
              )
            ) {
              applyMutation.mutate()
            }
          }}
        >
          {bulkMode ? 'Apply bulk closeout' : 'Apply closeout'}
        </button>
      </div>

      {statusMessage ? <p className="mt-3 text-sm text-slate-300">{statusMessage}</p> : null}

      {preview ? (
        <div className="mt-4 overflow-x-auto rounded border border-slate-700">
          <table className="min-w-full text-left text-sm text-slate-200">
            <thead className="bg-slate-900/80 text-slate-400">
              <tr>
                <th className="px-3 py-2">Kind</th>
                <th className="px-3 py-2">Reference</th>
                <th className="px-3 py-2">From</th>
                <th className="px-3 py-2">To</th>
                <th className="px-3 py-2">Ready</th>
              </tr>
            </thead>
            <tbody>
              {preview.tripActions.slice(0, 8).map((action) => (
                <tr key={action.tripId} className="border-t border-slate-800">
                  <td className="px-3 py-2">Trip</td>
                  <td className="px-3 py-2">{action.tripNumber}</td>
                  <td className="px-3 py-2">{action.currentDispatchStatus}</td>
                  <td className="px-3 py-2">{action.targetDispatchStatus}</td>
                  <td className="px-3 py-2">{action.canApply ? 'yes' : action.blockMessage ?? 'no'}</td>
                </tr>
              ))}
              {preview.stopActions.slice(0, 8).map((action) => (
                <tr key={action.stopId} className="border-t border-slate-800">
                  <td className="px-3 py-2">Stop</td>
                  <td className="px-3 py-2">{action.stopKey}</td>
                  <td className="px-3 py-2">{action.currentStopStatus}</td>
                  <td className="px-3 py-2">{action.targetStopStatus}</td>
                  <td className="px-3 py-2">{action.canApply ? 'yes' : action.blockMessage ?? 'no'}</td>
                </tr>
              ))}
              {preview.routeActions.slice(0, 8).map((action) => (
                <tr key={action.routeId} className="border-t border-slate-800">
                  <td className="px-3 py-2">Route</td>
                  <td className="px-3 py-2">{action.routeNumber}</td>
                  <td className="px-3 py-2">{action.currentRouteStatus}</td>
                  <td className="px-3 py-2">{action.targetRouteStatus}</td>
                  <td className="px-3 py-2">{action.canApply ? 'yes' : action.blockMessage ?? 'no'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : null}

      {auditQuery.data && auditQuery.data.entries.length > 0 ? (
        <div className="mt-4 rounded border border-slate-700">
          <h3 className="border-b border-slate-700 bg-slate-900/60 px-3 py-2 text-sm font-medium text-slate-200">
            Recent closeout audit
          </h3>
          <ul className="max-h-40 divide-y divide-slate-800 overflow-y-auto text-xs text-slate-300">
            {auditQuery.data.entries.map((entry) => (
              <li key={entry.id} className="px-3 py-2">
                <span className="text-slate-400">
                  {new Date(entry.occurredAt).toLocaleString()}
                </span>
                <span className="ml-2 text-slate-200">{entry.action}</span>
                <span className="ml-2">{entry.result}</span>
              </li>
            ))}
          </ul>
        </div>
      ) : null}
    </section>
  )
}
