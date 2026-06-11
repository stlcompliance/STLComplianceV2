import { buildSemanticKey } from '@stl/shared-ui'
import { useEffect, useMemo, useState } from 'react'

import type { RouteDetailResponse, RouteSummaryResponse } from '../api/types'
import { OpenStreetMapCard } from './OpenStreetMapCard'

interface RoutesPanelProps {
  mode: 'drawer' | 'details' | 'create'
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
  stopScheduledArrivalAt: string
  stopGeofenceAnchorLatitude: string
  stopGeofenceAnchorLongitude: string
  stopGeofenceRadiusMeters: string
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
  onStopScheduledArrivalAtChange: (value: string) => void
  onStopGeofenceAnchorLatitudeChange: (value: string) => void
  onStopGeofenceAnchorLongitudeChange: (value: string) => void
  onStopGeofenceRadiusMetersChange: (value: string) => void
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
  mode,
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
  stopScheduledArrivalAt,
  stopGeofenceAnchorLatitude,
  stopGeofenceAnchorLongitude,
  stopGeofenceRadiusMeters,
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
  onStopScheduledArrivalAtChange,
  onStopGeofenceAnchorLatitudeChange,
  onStopGeofenceAnchorLongitudeChange,
  onStopGeofenceRadiusMetersChange,
  onCreateRoute,
  onLinkTrip,
  onUpdateStopStatus,
}: RoutesPanelProps) {
  type RouteColumnKey = 'title' | 'number' | 'status' | 'stops' | 'linkedTrip'
  const storageKey = 'routarr.routes.drawer.columns.v1'
  const allColumns: Array<{ key: RouteColumnKey; label: string }> = [
    { key: 'title', label: 'Title' },
    { key: 'number', label: 'Route number' },
    { key: 'status', label: 'Status' },
    { key: 'stops', label: 'Stops' },
    { key: 'linkedTrip', label: 'Trip link' },
  ]
  const [showStopKeyPolicy, setShowStopKeyPolicy] = useState(false)
  const [selectedColumns, setSelectedColumns] = useState<RouteColumnKey[]>(['title', 'number', 'status', 'stops', 'linkedTrip'])
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
  useEffect(() => {
    try {
      const raw = window.localStorage.getItem(storageKey)
      if (!raw) return
      const parsed = JSON.parse(raw) as RouteColumnKey[]
      const valid = parsed.filter((column) => allColumns.some((candidate) => candidate.key === column)).slice(0, 5)
      if (valid.length > 0) setSelectedColumns(valid)
    } catch {}
  }, [])
  useEffect(() => {
    window.localStorage.setItem(storageKey, JSON.stringify(selectedColumns))
  }, [selectedColumns])

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

      {mode === 'create' && canCreate ? (
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
            <div className="text-xs text-slate-400">First stop reference is generated automatically.</div>
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
          <label className="text-sm text-slate-300" htmlFor="routes-stop-scheduled-arrival">
            Scheduled arrival
            <input
              id="routes-stop-scheduled-arrival"
              type="datetime-local"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={stopScheduledArrivalAt}
              onChange={(event) => onStopScheduledArrivalAtChange(event.target.value)}
              placeholder="Optional dock appointment"
            />
          </label>
          <label className="text-sm text-slate-300" htmlFor="routes-stop-geofence-latitude">
            Geofence latitude
            <input
              id="routes-stop-geofence-latitude"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={stopGeofenceAnchorLatitude}
              onChange={(event) => onStopGeofenceAnchorLatitudeChange(event.target.value)}
              placeholder="Optional anchor latitude"
            />
          </label>
          <label className="text-sm text-slate-300" htmlFor="routes-stop-geofence-longitude">
            Geofence longitude
            <input
              id="routes-stop-geofence-longitude"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={stopGeofenceAnchorLongitude}
              onChange={(event) => onStopGeofenceAnchorLongitudeChange(event.target.value)}
              placeholder="Optional anchor longitude"
            />
          </label>
          <label className="text-sm text-slate-300" htmlFor="routes-stop-geofence-radius">
            Geofence radius meters
            <input
              id="routes-stop-geofence-radius"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={stopGeofenceRadiusMeters}
              onChange={(event) => onStopGeofenceRadiusMetersChange(event.target.value)}
              placeholder="Defaults to 250"
            />
          </label>
          <div className="md:col-span-2">
            <OpenStreetMapCard
              latitude={stopGeofenceAnchorLatitude}
              longitude={stopGeofenceAnchorLongitude}
              label="First stop geofence preview"
              addressQuery={stopAddress}
              heightClassName="h-56"
              emptyMessage="Add geofence latitude and longitude to preview the first stop in OpenStreetMap."
            />
          </div>
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
        <div className="space-y-2">
          <div className="rounded-md border border-slate-700 p-2">
            <p className="text-xs text-slate-400">Visible columns (max 5)</p>
            <div className="mt-2 flex flex-wrap gap-3">
              {allColumns.map((column) => (
                <label key={column.key} className="inline-flex items-center gap-2 text-xs text-slate-300">
                  <input
                    type="checkbox"
                    checked={selectedColumns.includes(column.key)}
                    onChange={() => {
                      if (selectedColumns.includes(column.key)) {
                        const next = selectedColumns.filter((item) => item !== column.key)
                        if (next.length > 0) setSelectedColumns(next)
                      } else if (selectedColumns.length < 5) {
                        setSelectedColumns([...selectedColumns, column.key])
                      }
                    }}
                  />
                  {column.label}
                </label>
              ))}
            </div>
          </div>
          <div className="overflow-x-auto rounded-md border border-slate-700">
            <table className="min-w-full text-left text-sm">
              <thead className="bg-slate-950/70">
                <tr>
                  {selectedColumns.map((column) => (
                    <th key={column} className="px-3 py-2 text-xs font-medium uppercase tracking-wide text-slate-400">
                      {allColumns.find((item) => item.key === column)?.label}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
          {routes.map((route) => (
                <tr
                  key={route.routeId}
                  className={`border-t border-slate-800 cursor-pointer ${selectedRouteId === route.routeId ? 'bg-emerald-950/30' : ''}`}
                  onClick={() => onSelectedRouteIdChange(route.routeId)}
                >
                  {selectedColumns.map((column) => (
                    <td key={`${route.routeId}-${column}`} className="px-3 py-2 text-slate-200">
                      {column === 'title' ? route.title : null}
                      {column === 'number' ? route.routeNumber : null}
                      {column === 'status' ? route.routeStatus : null}
                      {column === 'stops' ? `${route.stopCount}` : null}
                      {column === 'linkedTrip' ? (route.tripId ? 'Linked' : 'Unlinked') : null}
                    </td>
                  ))}
                </tr>
          ))}
              </tbody>
            </table>
          </div>
        </div>

        {mode !== 'drawer' ? <div className="rounded-lg border border-slate-700 bg-slate-950/30 p-4">
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
                  {stop.geofenceAnchorLatitude != null && stop.geofenceAnchorLongitude != null ? (
                    <div className="mt-1 text-xs text-slate-500">
                      Anchor {stop.geofenceAnchorLatitude.toFixed(4)}, {stop.geofenceAnchorLongitude.toFixed(4)} ·{' '}
                      <a
                        href={`https://www.openstreetmap.org/?mlat=${stop.geofenceAnchorLatitude}&mlon=${stop.geofenceAnchorLongitude}#map=16/${stop.geofenceAnchorLatitude}/${stop.geofenceAnchorLongitude}`}
                        target="_blank"
                        rel="noreferrer"
                        className="text-sky-300 underline-offset-2 hover:text-sky-200 hover:underline"
                      >
                        OpenStreetMap
                      </a>
                    </div>
                  ) : null}
                  {stop.scheduledArrivalAt ? (
                    <div className="text-xs text-slate-500">
                      Scheduled arrival {new Date(stop.scheduledArrivalAt).toLocaleString()}
                    </div>
                  ) : null}
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
        </div> : null}
      </div>
    </section>
  )
}
