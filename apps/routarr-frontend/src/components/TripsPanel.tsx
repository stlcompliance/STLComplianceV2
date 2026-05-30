import { useQuery } from '@tanstack/react-query'
import { useEffect, useMemo } from 'react'
import {
  AdvancedReferenceField,
  buildSemanticKey,
  GeneratedKeyField,
  StaticSearchPicker,
  type PickerOption,
} from '@stl/shared-ui'
import { Link } from 'react-router-dom'

import { listDrivers, listVehicleRefs } from '../api/client'
import type { TripDetailResponse, TripSummaryResponse } from '../api/types'
import {
  driverToPickerOption,
  findDriverLabel,
  findVehicleLabel,
  vehicleRefToPickerOption,
} from '../lib/referencePickers'

interface TripsPanelProps {
  accessToken: string
  canCreate: boolean
  canAssign: boolean
  canPerform: boolean
  canManage: boolean
  viewAllTrips: boolean
  sessionPersonId: string
  trips: TripSummaryResponse[]
  selectedTrip: TripDetailResponse | null
  selectedTripId: string
  tripTitle: string
  tripDescription: string
  vehicleRefKey: string
  driverPersonId: string
  loadKey: string
  loadOrigin: string
  loadDestination: string
  statusFilter: string
  isLoading: boolean
  isDetailLoading: boolean
  isCreating: boolean
  isAssigning: boolean
  isUpdatingStatus: boolean
  onSelectedTripIdChange: (value: string) => void
  onTripTitleChange: (value: string) => void
  onTripDescriptionChange: (value: string) => void
  onVehicleRefKeyChange: (value: string) => void
  onDriverPersonIdChange: (value: string) => void
  onLoadKeyChange: (value: string) => void
  onLoadOriginChange: (value: string) => void
  onLoadDestinationChange: (value: string) => void
  onStatusFilterChange: (value: string) => void
  onCreateTrip: () => void
  onAssignDriver: () => void
  onUpdateStatus: (tripId: string, status: string) => void
}

function statusOptionsFor(currentStatus: string, canManage: boolean): string[] {
  if (currentStatus === 'planned') {
    return canManage ? ['planned', 'cancelled'] : ['planned']
  }
  if (currentStatus === 'assigned') {
    const options = ['assigned', 'dispatched']
    if (canManage) options.push('cancelled')
    return options
  }
  if (currentStatus === 'dispatched') {
    const options = ['dispatched', 'in_progress']
    if (canManage) options.push('cancelled')
    return options
  }
  if (currentStatus === 'in_progress') {
    const options = ['in_progress', 'completed']
    if (canManage) options.push('cancelled')
    return options
  }
  return [currentStatus]
}

export function TripsPanel({
  accessToken,
  canCreate,
  canAssign,
  canPerform,
  canManage,
  viewAllTrips,
  sessionPersonId,
  trips,
  selectedTrip,
  selectedTripId,
  tripTitle,
  tripDescription,
  vehicleRefKey,
  driverPersonId,
  loadKey,
  loadOrigin,
  loadDestination,
  statusFilter,
  isLoading,
  isDetailLoading,
  isCreating,
  isAssigning,
  isUpdatingStatus,
  onSelectedTripIdChange,
  onTripTitleChange,
  onTripDescriptionChange,
  onVehicleRefKeyChange,
  onDriverPersonIdChange,
  onLoadKeyChange,
  onLoadOriginChange,
  onLoadDestinationChange,
  onStatusFilterChange,
  onCreateTrip,
  onAssignDriver,
  onUpdateStatus,
}: TripsPanelProps) {
  const driversQuery = useQuery({
    queryKey: ['routarr-drivers', accessToken],
    queryFn: () => listDrivers(accessToken),
    enabled: Boolean(accessToken) && (canCreate || canAssign),
  })

  const vehicleRefsQuery = useQuery({
    queryKey: ['routarr-vehicle-refs', accessToken],
    queryFn: () => listVehicleRefs(accessToken),
    enabled: Boolean(accessToken) && canCreate,
  })

  const drivers = driversQuery.data?.items ?? []
  const vehicleRefs = vehicleRefsQuery.data?.items ?? []

  const driverOptions = useMemo(() => drivers.map(driverToPickerOption), [drivers])
  const vehicleOptions = useMemo(() => vehicleRefs.map(vehicleRefToPickerOption), [vehicleRefs])

  const loadSourceLabel = [loadOrigin, loadDestination].filter(Boolean).join(' - ')
  const generatedLoadKey = buildSemanticKey({
    domain: 'route',
    kind: 'load',
    title: loadSourceLabel,
    maxLength: 128,
  })
  const effectiveLoadKey = generatedLoadKey

  useEffect(() => {
    if (effectiveLoadKey !== loadKey) {
      onLoadKeyChange(effectiveLoadKey)
    }
  }, [effectiveLoadKey, loadKey, onLoadKeyChange])

  const selectedDriverOption = useMemo((): PickerOption | undefined => {
    const label = findDriverLabel(drivers, driverPersonId)
    return label ? { value: driverPersonId, label } : undefined
  }, [driverPersonId, drivers])

  const selectedVehicleOption = useMemo((): PickerOption | undefined => {
    const label = findVehicleLabel(vehicleRefs, vehicleRefKey)
    return label ? { value: vehicleRefKey, label } : undefined
  }, [vehicleRefKey, vehicleRefs])

  const assignedDriverLabel =
    findDriverLabel(drivers, selectedTrip?.assignedDriverPersonId) ??
    (selectedTrip?.assignedDriverPersonId ? undefined : 'Unassigned')

  const assignedDriverSelectedOption = useMemo((): PickerOption | undefined => {
    if (!selectedTrip?.assignedDriverPersonId) {
      return undefined
    }
    const label = findDriverLabel(drivers, selectedTrip.assignedDriverPersonId)
    return {
      value: selectedTrip.assignedDriverPersonId,
      label: label ?? selectedTrip.assignedDriverPersonId,
      inactive: !label,
    }
  }, [drivers, selectedTrip?.assignedDriverPersonId])

  const detailVehicleLabel =
    findVehicleLabel(vehicleRefs, selectedTrip?.vehicleRefKey) ?? selectedTrip?.vehicleRefKey

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-white">Trips & dispatch</h2>
          <p className="text-sm text-slate-400">
            {viewAllTrips ? 'Dispatch board view' : 'Showing trips you created or drive'}
          </p>
        </div>
        <label className="text-sm text-slate-300" htmlFor="trips-status-filter">
          Status filter
          <select id="trips-status-filter"
            className="ml-2 rounded border border-slate-600 bg-slate-950 px-2 py-1"
            value={statusFilter}
            onChange={(event) => onStatusFilterChange(event.target.value)}
          >
            <option value="">All active</option>
            <option value="planned">Planned</option>
            <option value="assigned">Assigned</option>
            <option value="dispatched">Dispatched</option>
            <option value="in_progress">In progress</option>
            <option value="completed">Completed</option>
            <option value="cancelled">Cancelled</option>
          </select>
        </label>
      </div>

      {canCreate ? (
        <div className="mb-6 grid gap-3 rounded-lg border border-slate-700/80 bg-slate-950/40 p-4 md:grid-cols-2">
          <label className="text-sm text-slate-300 md:col-span-2" htmlFor="trips-title">
          Title
          <input id="trips-title"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={tripTitle}
              onChange={(event) => onTripTitleChange(event.target.value)}
            />
          </label>
          <label className="text-sm text-slate-300 md:col-span-2" htmlFor="trips-description">
          Description
          <input id="trips-description"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={tripDescription}
              onChange={(event) => onTripDescriptionChange(event.target.value)}
            />
          </label>
          <div>
            <StaticSearchPicker
              label="Vehicle"
              value={vehicleRefKey}
              onChange={onVehicleRefKeyChange}
              options={vehicleOptions}
              selectedOption={selectedVehicleOption}
              placeholder="Search vehicles…"
              disabled={vehicleRefsQuery.isLoading}
              testId="trip-create-vehicle-picker"
            />
            <AdvancedReferenceField
              value={vehicleRefKey}
              onChange={onVehicleRefKeyChange}
              label="Vehicle reference (advanced)"
              testId="trip-create-vehicle-advanced"
            />
          </div>
          <div>
            <GeneratedKeyField
              label="Initial load key"
              sourceLabel={loadSourceLabel}
              generatedKey={generatedLoadKey}
              confirmedKey={effectiveLoadKey}
              manualOverride=""
              onManualOverrideChange={() => {}}
            />
          </div>
          <label className="text-sm text-slate-300" htmlFor="trips-load-origin">
          Load origin
          <input id="trips-load-origin"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={loadOrigin}
              onChange={(event) => onLoadOriginChange(event.target.value)}
            />
          </label>
          <label className="text-sm text-slate-300" htmlFor="trips-load-destination">
          Load destination
          <input id="trips-load-destination"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={loadDestination}
              onChange={(event) => onLoadDestinationChange(event.target.value)}
            />
          </label>
          <div className="md:col-span-2">
            <button
              type="button"
              className="rounded bg-sky-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
              disabled={isCreating || !tripTitle.trim()}
              onClick={onCreateTrip}
            >
              {isCreating ? 'Creating…' : 'Create trip'}
            </button>
          </div>
        </div>
      ) : null}

      {isLoading ? <p className="text-sm text-slate-400">Loading trips…</p> : null}

      {!isLoading && trips.length === 0 ? (
        <p className="text-sm text-slate-400">No trips match the current filter.</p>
      ) : null}

      <div className="grid gap-4 lg:grid-cols-2">
        <ul className="space-y-2">
          {trips.map((trip) => (
            <li key={trip.tripId}>
              <button
                type="button"
                className={`w-full rounded-lg border px-4 py-3 text-left ${
                  selectedTripId === trip.tripId
                    ? 'border-sky-500 bg-sky-950/30'
                    : 'border-slate-700 bg-slate-950/30 hover:border-slate-500'
                }`}
                onClick={() => onSelectedTripIdChange(trip.tripId)}
              >
                <div className="flex items-center justify-between gap-2">
                  <span className="font-medium text-white">{trip.title}</span>
                  <span className="rounded bg-slate-800 px-2 py-0.5 text-xs uppercase tracking-wide text-slate-200">
                    {trip.dispatchStatus}
                  </span>
                </div>
                <p className="mt-1 text-xs text-slate-400">{trip.tripNumber}</p>
                <p className="mt-1 text-xs text-slate-500">
                  {trip.loadCount} load{trip.loadCount === 1 ? '' : 's'}
                  {trip.assignedDriverPersonId
                    ? ` · driver ${findDriverLabel(drivers, trip.assignedDriverPersonId) ?? 'assigned'}`
                    : ' · unassigned'}
                </p>
              </button>
            </li>
          ))}
        </ul>

        <div className="rounded-lg border border-slate-700 bg-slate-950/30 p-4">
          {!selectedTripId ? (
            <p className="text-sm text-slate-400">Select a trip to view dispatch details.</p>
          ) : isDetailLoading ? (
            <p className="text-sm text-slate-400">Loading trip detail…</p>
          ) : selectedTrip ? (
            <div className="space-y-4">
              <div>
                <h3 className="text-base font-semibold text-white">{selectedTrip.title}</h3>
                <p className="text-sm text-slate-400">{selectedTrip.tripNumber}</p>
                <p className="mt-2">
                  <Link
                    to={`/trips/${selectedTrip.tripId}`}
                    className="text-sm text-teal-300 hover:text-teal-200"
                    data-testid="open-trip-workspace"
                  >
                    Open execution workspace →
                  </Link>
                </p>
                <p className="mt-2 text-sm text-slate-300">{selectedTrip.description || 'No description'}</p>
              </div>

              <dl className="grid grid-cols-2 gap-2 text-sm">
                <div>
                  <dt className="text-slate-500">Status</dt>
                  <dd className="font-medium text-slate-200">{selectedTrip.dispatchStatus}</dd>
                </div>
                <div>
                  <dt className="text-slate-500">Vehicle</dt>
                  <dd className="font-medium text-slate-200">{detailVehicleLabel ?? '—'}</dd>
                </div>
                <div className="col-span-2">
                  <dt className="text-slate-500">Assigned driver</dt>
                  <dd className="font-medium text-slate-200">
                    {assignedDriverLabel ??
                      (assignedDriverSelectedOption
                        ? assignedDriverSelectedOption.label
                        : 'Unassigned')}
                  </dd>
                </div>
              </dl>

              {selectedTrip.loads.length > 0 ? (
                <div>
                  <h4 className="text-sm font-semibold text-slate-200">Loads</h4>
                  <ul className="mt-2 space-y-2">
                    {selectedTrip.loads.map((load) => (
                      <li key={load.loadId} className="rounded border border-slate-700 px-3 py-2 text-sm">
                        <div className="font-medium text-white">
                          {load.loadKey} · {load.loadType}
                        </div>
                        <div className="text-slate-400">
                          {load.originLabel} → {load.destinationLabel}
                        </div>
                      </li>
                    ))}
                  </ul>
                </div>
              ) : null}

              {canAssign &&
              selectedTrip.dispatchStatus !== 'completed' &&
              selectedTrip.dispatchStatus !== 'cancelled' ? (
                <div className="space-y-2 rounded border border-slate-700 p-3">
                  <StaticSearchPicker
                    label="Assign driver"
                    value={driverPersonId}
                    onChange={onDriverPersonIdChange}
                    options={driverOptions}
                    selectedOption={selectedDriverOption}
                    placeholder={findDriverLabel(drivers, sessionPersonId) ?? 'Search drivers…'}
                    disabled={driversQuery.isLoading}
                    testId="trip-assign-driver-picker"
                  />
                  <AdvancedReferenceField
                    value={driverPersonId}
                    onChange={onDriverPersonIdChange}
                    label="Driver person (advanced)"
                    testId="trip-assign-driver-advanced"
                  />
                  <button
                    type="button"
                    className="rounded bg-indigo-600 px-3 py-2 text-sm text-white disabled:opacity-50"
                    disabled={isAssigning || !driverPersonId.trim()}
                    onClick={onAssignDriver}
                  >
                    {isAssigning ? 'Assigning…' : 'Assign driver'}
                  </button>
                </div>
              ) : null}

              {canPerform &&
              selectedTrip.dispatchStatus !== 'completed' &&
              selectedTrip.dispatchStatus !== 'cancelled' ? (
                <div className="space-y-2">
                  <label className="block text-sm text-slate-300" htmlFor="trips-dispatch-status">
                    Dispatch status for {selectedTrip.title}
                    <select
                      id="trips-dispatch-status"
                      className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
                      defaultValue={selectedTrip.dispatchStatus}
                      aria-label={`Dispatch status for ${selectedTrip.title}`}
                      onChange={(event) => onUpdateStatus(selectedTrip.tripId, event.target.value)}
                      disabled={isUpdatingStatus}
                    >
                      {statusOptionsFor(selectedTrip.dispatchStatus, canManage).map((status) => (
                        <option key={status} value={status}>
                          {status}
                        </option>
                      ))}
                    </select>
                  </label>
                </div>
              ) : null}
            </div>
          ) : (
            <p className="text-sm text-slate-400">Trip detail unavailable.</p>
          )}
        </div>
      </div>
    </section>
  )
}
