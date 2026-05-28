import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import {
  canAssignDrivers,
  canCreateTrips,
  canManageTrips,
  canPerformTrips,
  canViewAllTrips,
} from '../../auth/sessionStorage'
import { TripsPanel } from '../../components/TripsPanel'

type Props = { state: RoutArrWorkspaceState }

export function TripsSection({ state }: Props) {
  const {
    session,
    roleKey,
    isPlatformAdmin,
    tripsQuery,
    tripDetailQuery,
    selectedTripId,
    tripTitle,
    tripDescription,
    vehicleRefKey,
    driverPersonId,
    loadKey,
    loadOrigin,
    loadDestination,
    statusFilter,
    createTripMutation,
    assignDriverMutation,
    updateStatusMutation,
    setSelectedTripId,
    setTripTitle,
    setTripDescription,
    setVehicleRefKey,
    setDriverPersonId,
    setLoadKey,
    setLoadOrigin,
    setLoadDestination,
    setStatusFilter,
  } = state

  return (
    <div className="mt-8">
      <TripsPanel
        canCreate={canCreateTrips(roleKey, isPlatformAdmin)}
        canAssign={canAssignDrivers(roleKey, isPlatformAdmin)}
        canPerform={canPerformTrips(roleKey, isPlatformAdmin)}
        canManage={canManageTrips(roleKey, isPlatformAdmin)}
        viewAllTrips={canViewAllTrips(roleKey, isPlatformAdmin)}
        sessionPersonId={session.personId}
        trips={tripsQuery.data ?? []}
        selectedTrip={tripDetailQuery.data ?? null}
        selectedTripId={selectedTripId}
        tripTitle={tripTitle}
        tripDescription={tripDescription}
        vehicleRefKey={vehicleRefKey}
        driverPersonId={driverPersonId}
        loadKey={loadKey}
        loadOrigin={loadOrigin}
        loadDestination={loadDestination}
        statusFilter={statusFilter}
        isLoading={tripsQuery.isLoading}
        isDetailLoading={tripDetailQuery.isLoading}
        isCreating={createTripMutation.isPending}
        isAssigning={assignDriverMutation.isPending}
        isUpdatingStatus={updateStatusMutation.isPending}
        onSelectedTripIdChange={setSelectedTripId}
        onTripTitleChange={setTripTitle}
        onTripDescriptionChange={setTripDescription}
        onVehicleRefKeyChange={setVehicleRefKey}
        onDriverPersonIdChange={setDriverPersonId}
        onLoadKeyChange={setLoadKey}
        onLoadOriginChange={setLoadOrigin}
        onLoadDestinationChange={setLoadDestination}
        onStatusFilterChange={setStatusFilter}
        onCreateTrip={() => createTripMutation.mutate()}
        onAssignDriver={() => assignDriverMutation.mutate()}
        onUpdateStatus={(tripId, status) => {
          if (status !== tripDetailQuery.data?.dispatchStatus) {
            updateStatusMutation.mutate({ tripId, status })
          }
        }}
      />
    </div>
  )
}
