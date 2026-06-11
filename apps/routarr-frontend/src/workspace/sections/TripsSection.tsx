import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { useLocation } from 'react-router-dom'
import {
  canAssignDrivers,
  canCreateTrips,
  canManageTrips,
  canOverrideVendorReadiness,
  canPerformTrips,
  canViewAllTrips,
} from '../../auth/sessionStorage'
import { TripsPanel } from '../../components/TripsPanel'
import { TripProfile } from './RoutingDetailProfiles'

type Props = { state: RoutArrWorkspaceState }
type TripsViewMode = 'drawer' | 'details' | 'create'

export function TripsSection({ state }: Props) {
  const location = useLocation()
  const mode: TripsViewMode = location.pathname.startsWith('/trips/create')
    ? 'create'
    : location.pathname.startsWith('/trips/details')
      ? 'details'
      : 'drawer'
  const {
    accessToken,
    session,
    roleKey,
    isPlatformAdmin,
    tripsQuery,
    tripDetailQuery,
    selectedTripId,
    tripTitle,
    tripDescription,
    vehicleRefKey,
    vendorOrderId,
    brokerOrderId,
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
    setVendorOrderId,
    setBrokerOrderId,
    setDriverPersonId,
    setLoadKey,
    setLoadOrigin,
    setLoadDestination,
    setStatusFilter,
  } = state

  if (mode === 'details') {
    return <TripProfile state={state} />
  }

  return (
    <div className="mt-8">
      <TripsPanel
        mode={mode}
        accessToken={accessToken}
        canCreate={canCreateTrips(roleKey, isPlatformAdmin)}
        canAssign={canAssignDrivers(roleKey, isPlatformAdmin)}
        canDispatch={canAssignDrivers(roleKey, isPlatformAdmin)}
        canPerform={canPerformTrips(roleKey, isPlatformAdmin)}
        canManage={canManageTrips(roleKey, isPlatformAdmin)}
        canOverrideVendorReadiness={canOverrideVendorReadiness(roleKey, isPlatformAdmin)}
        viewAllTrips={canViewAllTrips(roleKey, isPlatformAdmin)}
        sessionPersonId={session.personId}
        trips={tripsQuery.data ?? []}
        selectedTrip={tripDetailQuery.data ?? null}
        selectedTripId={selectedTripId}
        tripTitle={tripTitle}
        tripDescription={tripDescription}
        vehicleRefKey={vehicleRefKey}
        vendorOrderId={vendorOrderId}
        brokerOrderId={brokerOrderId}
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
        onVendorOrderIdChange={setVendorOrderId}
        onBrokerOrderIdChange={setBrokerOrderId}
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
