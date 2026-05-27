import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import {
  applyDispatchCloseout,
  getDispatchCloseoutSummary,
  previewDispatchCloseout,
} from '../api/client'
import type { DispatchCloseoutPreviewResponse } from '../api/types'

type DispatchCloseoutPanelProps = {
  accessToken: string
  scope: 'daily' | 'weekly'
  canAssign: boolean
}

export function DispatchCloseoutPanel({ accessToken, scope, canAssign }: DispatchCloseoutPanelProps) {
  const queryClient = useQueryClient()
  const [tripDisposition, setTripDisposition] = useState<'complete' | 'cancel'>('cancel')
  const [stopDisposition, setStopDisposition] = useState<'skip' | 'complete'>('skip')
  const [preview, setPreview] = useState<DispatchCloseoutPreviewResponse | null>(null)
  const [statusMessage, setStatusMessage] = useState<string | null>(null)

  const summaryQuery = useQuery({
    queryKey: ['routarr-closeout-summary', accessToken, scope],
    queryFn: () => getDispatchCloseoutSummary(accessToken, scope),
    enabled: canAssign,
  })

  const previewMutation = useMutation({
    mutationFn: () =>
      previewDispatchCloseout(accessToken, {
        scope,
        remainingTripDisposition: tripDisposition,
        openStopDisposition: stopDisposition,
      }),
    onSuccess: (response) => {
      setPreview(response)
      setStatusMessage(
        `Preview: ${response.summary.tripsCanApply}/${response.summary.tripCount} trips, ` +
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
      applyDispatchCloseout(accessToken, {
        scope,
        remainingTripDisposition: tripDisposition,
        openStopDisposition: stopDisposition,
      }),
    onSuccess: async (response) => {
      setPreview(null)
      setStatusMessage(
        `Closeout applied: ${response.summary.tripsCanApply} trips, ` +
          `${response.summary.stopsCanApply} stops, ${response.summary.routesCanApply} routes updated.`,
      )
      await queryClient.invalidateQueries({ queryKey: ['routarr-closeout-summary'] })
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

  return (
    <section className="rounded-xl border border-amber-700/60 bg-amber-950/20 p-4">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-white">End-of-day closeout</h2>
          <p className="text-sm text-slate-400">
            Close remaining trips, routes, and stops for the {scope} dispatch window.
          </p>
        </div>
        {summary ? (
          <div className="text-right text-sm text-slate-300">
            <div>
              Open: {summary.counts.openTrips} trips · {summary.counts.openRoutes} routes ·{' '}
              {summary.counts.openStops} stops
            </div>
          </div>
        ) : null}
      </div>

      <div className="mb-4 grid gap-4 md:grid-cols-2">
        <label className="block text-sm text-slate-300">
          Remaining trips
          <select
            className="mt-1 w-full rounded border border-slate-600 bg-slate-900 px-2 py-1.5 text-white"
            value={tripDisposition}
            onChange={(event) => setTripDisposition(event.target.value as 'complete' | 'cancel')}
          >
            <option value="cancel">Cancel all open trips</option>
            <option value="complete">Complete in-flight trips (cancel planned)</option>
          </select>
        </label>
        <label className="block text-sm text-slate-300">
          Open stops
          <select
            className="mt-1 w-full rounded border border-slate-600 bg-slate-900 px-2 py-1.5 text-white"
            value={stopDisposition}
            onChange={(event) => setStopDisposition(event.target.value as 'skip' | 'complete')}
          >
            <option value="skip">Skip pending / arrived stops</option>
            <option value="complete">Complete arrived stops (skip pending)</option>
          </select>
        </label>
      </div>

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
            if (
              window.confirm(
                'Apply end-of-day closeout for all open work in this window? This cannot be undone.',
              )
            ) {
              applyMutation.mutate()
            }
          }}
        >
          Apply closeout
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
            </tbody>
          </table>
        </div>
      ) : null}
    </section>
  )
}
