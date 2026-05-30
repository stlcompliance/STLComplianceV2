import { buildSemanticKey, GeneratedKeyField } from '@stl/shared-ui'
import { useEffect, useMemo, useState } from 'react'

import type { RouteDetailResponse, RouteSummaryResponse } from '../api/types'

interface RoutesPanelProps {
  canCreate: boolean
  canPerform: boolean
  viewAllRoutes: boolean
  routes: RouteSummaryResponse[]
  selectedRoute: RouteDetailResponse | null
  selectedRouteId: string
  selectedTripId: string
  routeTitle: string
  routeDescription: string
  stopKey: string
  stopLabel: string
  stopAddress: string
  stopType: string
  isLoading: boolean
  isDetailLoading: boolean
  isCreating: boolean
  isLinking: boolean
  isUpdatingStop: boolean
  onSelectedRouteIdChange: (value: string) => void
  onRouteTitleChange: (value: string) => void
  onRouteDescriptionChange: (value: string) => void
  onStopKeyChange: (value: string) => void
  onStopLabelChange: (value: string) => void
  onStopAddressChange: (value: string) => void
  onStopTypeChange: (value: string) => void
  onCreateRoute: () => void
  onLinkTrip: () => void
  onUpdateStopStatus: (stopId: string, status: string) => void
}

function nextStopStatus(currentStatus: string): string | null {
  if (currentStatus === 'pending') return 'arrived'
  if (currentStatus === 'arrived') return 'completed'
  return null
}

export function RoutesPanel({
  canCreate,
  canPerform,
  viewAllRoutes,
  routes,
  selectedRoute,
  selectedRouteId,
  selectedTripId,
  routeTitle,
  routeDescription,
  stopKey,
  stopLabel,
  stopAddress,
  stopType,
  isLoading,
  isDetailLoading,
  isCreating,
  isLinking,
  isUpdatingStop,
  onSelectedRouteIdChange,
  onRouteTitleChange,
  onRouteDescriptionChange,
  onStopKeyChange,
  onStopLabelChange,
  onStopAddressChange,
  onStopTypeChange,
  onCreateRoute,
  onLinkTrip,
  onUpdateStopStatus,
}: RoutesPanelProps) {
  const [showStopKeyPolicy, setShowStopKeyPolicy] = useState(false)
  const stopKeySource = stopLabel.trim() || stopAddress.trim()
  const generatedStopKey = useMemo(
    () =>
      buildSemanticKey({
        domain: 'route',
        kind: 'stop',
        title: `${stopType} ${stopKeySource}`.trim(),
        maxLength: 128,
      }),
    [stopKeySource, stopType],
  )

  useEffect(() => {
    onStopKeyChange(generatedStopKey)
  }, [generatedStopKey, onStopKeyChange])

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <div className="mb-4">
        <h2 className="text-lg font-semibold text-white">Routes & stops</h2>
        <p className="text-sm text-slate-400">
          {viewAllRoutes
            ? 'Plan ordered stop sequences and link routes to trips'
            : 'Routes for trips you created or drive'}
        </p>
      </div>

      {canCreate ? (
        <div className="mb-6 grid gap-3 rounded-lg border border-slate-700/80 bg-slate-950/40 p-4 md:grid-cols-2">
          <label className="text-sm text-slate-300 md:col-span-2" htmlFor="routes-route-title">
          Route title
          <input id="routes-route-title"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={routeTitle}
              onChange={(event) => onRouteTitleChange(event.target.value)}
            />
          </label>
          <label className="text-sm text-slate-300 md:col-span-2" htmlFor="routes-description">
          Description
          <input id="routes-description"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={routeDescription}
              onChange={(event) => onRouteDescriptionChange(event.target.value)}
            />
          </label>
          <div className="space-y-1 text-sm text-slate-300">
            <GeneratedKeyField
              sourceLabel={`${stopType} ${stopKeySource}`.trim()}
              generatedKey={generatedStopKey}
              confirmedKey={stopKey}
              manualOverride=""
              onManualOverrideChange={() => {}}
              showAdvancedKey={showStopKeyPolicy}
              disabled={isCreating}
              label="First stop key"
            />
            {!showStopKeyPolicy ? (
              <button
                type="button"
                className="text-xs text-slate-500 underline-offset-2 hover:text-slate-300 hover:underline"
                onClick={() => setShowStopKeyPolicy(true)}
                disabled={isCreating}
              >
                Key policy
              </button>
            ) : null}
          </div>
          <label className="text-sm text-slate-300" htmlFor="routes-stop-type">
          Stop type
          <select id="routes-stop-type"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={stopType}
              onChange={(event) => onStopTypeChange(event.target.value)}
            >
              <option value="pickup">Pickup</option>
              <option value="delivery">Delivery</option>
              <option value="waypoint">Waypoint</option>
              <option value="depot">Depot</option>
            </select>
          </label>
          <label className="text-sm text-slate-300" htmlFor="routes-stop-label">
          Stop label
          <input id="routes-stop-label"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={stopLabel}
              onChange={(event) => onStopLabelChange(event.target.value)}
            />
          </label>
          <label className="text-sm text-slate-300" htmlFor="routes-address-label">
          Address label
          <input id="routes-address-label"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={stopAddress}
              onChange={(event) => onStopAddressChange(event.target.value)}
            />
          </label>
          <div className="md:col-span-2">
            <button
              type="button"
              className="rounded bg-emerald-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
              disabled={isCreating || !routeTitle.trim() || !stopKey.trim()}
              onClick={onCreateRoute}
            >
              {isCreating ? 'Creating…' : 'Create route'}
            </button>
          </div>
        </div>
      ) : null}

      {isLoading ? <p className="text-sm text-slate-400">Loading routes…</p> : null}

      {!isLoading && routes.length === 0 ? (
        <p className="text-sm text-slate-400">
          {selectedTripId ? 'No routes linked to the selected trip yet.' : 'No routes yet.'}
        </p>
      ) : null}

      <div className="grid gap-4 lg:grid-cols-2">
        <ul className="space-y-2">
          {routes.map((route) => (
            <li key={route.routeId}>
              <button
                type="button"
                className={`w-full rounded-lg border px-4 py-3 text-left ${
                  selectedRouteId === route.routeId
                    ? 'border-emerald-500 bg-emerald-950/30'
                    : 'border-slate-700 bg-slate-950/30 hover:border-slate-500'
                }`}
                onClick={() => onSelectedRouteIdChange(route.routeId)}
              >
                <div className="flex items-center justify-between gap-2">
                  <span className="font-medium text-white">{route.title}</span>
                  <span className="rounded bg-slate-800 px-2 py-0.5 text-xs uppercase tracking-wide text-slate-200">
                    {route.routeStatus}
                  </span>
                </div>
                <p className="mt-1 text-xs text-slate-400">{route.routeNumber}</p>
                <p className="mt-1 text-xs text-slate-500">
                  {route.stopCount} stop{route.stopCount === 1 ? '' : 's'}
                  {route.tripId ? ' · linked to trip' : ' · unlinked'}
                </p>
              </button>
            </li>
          ))}
        </ul>

        <div className="rounded-lg border border-slate-700 bg-slate-950/30 p-4">
          {!selectedRouteId ? (
            <p className="text-sm text-slate-400">Select a route to view ordered stops.</p>
          ) : isDetailLoading ? (
            <p className="text-sm text-slate-400">Loading route detail…</p>
          ) : selectedRoute ? (
            <div className="space-y-4">
              <div>
                <h3 className="text-base font-semibold text-white">{selectedRoute.title}</h3>
                <p className="text-sm text-slate-400">{selectedRoute.routeNumber}</p>
                <p className="mt-2 text-sm text-slate-300">
                  {selectedRoute.description || 'No description'}
                </p>
              </div>

              {canCreate && !selectedRoute.tripId && selectedTripId ? (
                <button
                  type="button"
                  className="rounded bg-emerald-700 px-3 py-2 text-sm text-white disabled:opacity-50"
                  disabled={isLinking}
                  onClick={onLinkTrip}
                >
                  {isLinking ? 'Linking…' : 'Link to selected trip'}
                </button>
              ) : null}

              {selectedRoute.stops.length > 0 ? (
                <div>
                  <h4 className="text-sm font-semibold text-slate-200">Ordered stops</h4>
                  <ol className="mt-2 space-y-2">
                    {selectedRoute.stops.map((stop) => {
                      const nextStatus = nextStopStatus(stop.stopStatus)
                      return (
                        <li
                          key={stop.stopId}
                          className="rounded border border-slate-700 px-3 py-2 text-sm"
                        >
                          <div className="flex items-center justify-between gap-2">
                            <span className="font-medium text-white">
                              {stop.sequenceNumber}. {stop.label || stop.stopKey}
                            </span>
                            <span className="text-xs uppercase text-slate-400">{stop.stopStatus}</span>
                          </div>
                          <div className="text-slate-400">
                            {stop.stopType} · {stop.addressLabel || 'No address'}
                          </div>
                          {canPerform && nextStatus ? (
                            <button
                              type="button"
                              className="mt-2 rounded bg-slate-700 px-2 py-1 text-xs text-white disabled:opacity-50"
                              disabled={isUpdatingStop}
                              onClick={() => onUpdateStopStatus(stop.stopId, nextStatus)}
                            >
                              Mark {nextStatus}
                            </button>
                          ) : null}
                        </li>
                      )
                    })}
                  </ol>
                </div>
              ) : (
                <p className="text-sm text-slate-400">No stops on this route.</p>
              )}
            </div>
          ) : (
            <p className="text-sm text-slate-400">Route detail unavailable.</p>
          )}
        </div>
      </div>
    </section>
  )
}
