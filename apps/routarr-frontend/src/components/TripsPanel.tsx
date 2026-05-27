import type { TripDetailResponse, TripSummaryResponse } from '../api/types'

interface TripsPanelProps {
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
  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-white">Trips & dispatch</h2>
          <p className="text-sm text-slate-400">
            {viewAllTrips ? 'Dispatch board view' : 'Showing trips you created or drive'}
          </p>
        </div>
        <label className="text-sm text-slate-300">
          Status filter
          <select
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
          <label className="text-sm text-slate-300 md:col-span-2">
            Title
            <input
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={tripTitle}
              onChange={(event) => onTripTitleChange(event.target.value)}
            />
          </label>
          <label className="text-sm text-slate-300 md:col-span-2">
            Description
            <input
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={tripDescription}
              onChange={(event) => onTripDescriptionChange(event.target.value)}
            />
          </label>
          <label className="text-sm text-slate-300">
            Vehicle ref
            <input
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={vehicleRefKey}
              onChange={(event) => onVehicleRefKeyChange(event.target.value)}
            />
          </label>
          <label className="text-sm text-slate-300">
            Initial load key
            <input
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={loadKey}
              onChange={(event) => onLoadKeyChange(event.target.value)}
            />
          </label>
          <label className="text-sm text-slate-300">
            Load origin
            <input
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={loadOrigin}
              onChange={(event) => onLoadOriginChange(event.target.value)}
            />
          </label>
          <label className="text-sm text-slate-300">
            Load destination
            <input
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
                    ? ` · driver ${trip.assignedDriverPersonId.slice(0, 8)}…`
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
                <p className="mt-2 text-sm text-slate-300">{selectedTrip.description || 'No description'}</p>
              </div>

              <dl className="grid grid-cols-2 gap-2 text-sm">
                <div>
                  <dt className="text-slate-500">Status</dt>
                  <dd className="font-medium text-slate-200">{selectedTrip.dispatchStatus}</dd>
                </div>
                <div>
                  <dt className="text-slate-500">Vehicle ref</dt>
                  <dd className="font-medium text-slate-200">{selectedTrip.vehicleRefKey ?? '—'}</dd>
                </div>
                <div className="col-span-2">
                  <dt className="text-slate-500">Assigned driver</dt>
                  <dd className="font-medium text-slate-200">
                    {selectedTrip.assignedDriverPersonId ?? 'Unassigned'}
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
                  <label className="block text-sm text-slate-300">
                    Assign driver (StaffArr person id)
                    <input
                      className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
                      value={driverPersonId}
                      placeholder={sessionPersonId}
                      onChange={(event) => onDriverPersonIdChange(event.target.value)}
                    />
                  </label>
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
                  <label className="block text-sm text-slate-300">
                    Dispatch status
                    <select
                      className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
                      defaultValue={selectedTrip.dispatchStatus}
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
