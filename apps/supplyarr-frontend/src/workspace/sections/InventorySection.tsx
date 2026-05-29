import { InventoryPanel } from '../../components/InventoryPanel'
import { StockReservationsPanel } from '../../components/StockReservationsPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function InventorySection({ state: s }: Props) {
  const bins = s.binsQuery.data ?? []

  return (
    <div className="space-y-6">
      <InventoryPanel
        locations={s.locations}
        bins={bins}
        stockLevels={s.stockQuery.data ?? []}
        parts={s.partsQuery.data ?? []}
        canManage={s.canManageInv}
        isLoading={s.locationsQuery.isLoading || s.binsQuery.isLoading || s.stockQuery.isLoading}
        locationKey={s.invLocationKey}
        locationName={s.invLocationName}
        locationType={s.invLocationType}
        addressLine={s.invAddressLine}
        binKey={s.invBinKey}
        binName={s.invBinName}
        selectedLocationId={s.selectedInvLocationId}
        selectedPartId={s.selectedStockPartId}
        selectedBinId={s.selectedStockBinId}
        stockQuantity={s.stockQuantity}
        onLocationKeyChange={s.setInvLocationKey}
        onLocationNameChange={s.setInvLocationName}
        onLocationTypeChange={s.setInvLocationType}
        onAddressLineChange={s.setInvAddressLine}
        onBinKeyChange={s.setInvBinKey}
        onBinNameChange={s.setInvBinName}
        onSelectedLocationIdChange={s.setSelectedInvLocationId}
        onSelectedPartIdChange={s.setSelectedStockPartId}
        onSelectedBinIdChange={s.setSelectedStockBinId}
        onStockQuantityChange={s.setStockQuantity}
        onCreateLocation={() => s.createLocationMutation.mutate()}
        onCreateBin={() => s.createBinMutation.mutate()}
        onUpsertStock={() => s.upsertStockMutation.mutate()}
        isCreatingLocation={s.createLocationMutation.isPending}
        isCreatingBin={s.createBinMutation.isPending}
        isUpsertingStock={s.upsertStockMutation.isPending}
      />

      <StockReservationsPanel
        reservations={s.stockReservationsQuery.data ?? []}
        stockLevels={s.stockQuery.data ?? []}
        parts={s.partsQuery.data ?? []}
        bins={bins.map((bin) => ({
          binId: bin.binId,
          binKey: bin.binKey,
          locationKey: bin.locationKey,
          name: bin.name,
        }))}
        canManage={s.canManageInv}
        isLoading={s.stockReservationsQuery.isLoading}
        reservationKey={s.reservationKey}
        selectedReservationId={s.selectedReservationId}
        selectedReservationPartId={s.selectedReservationPartId}
        selectedReservationBinId={s.selectedReservationBinId}
        reservationQuantity={s.reservationQuantity}
        reservationNotes={s.reservationNotes}
        releaseReason={s.reservationReleaseReason}
        statusFilter={s.reservationStatusFilter}
        onReservationKeyChange={s.setReservationKey}
        onSelectedReservationIdChange={s.setSelectedReservationId}
        onSelectedReservationPartIdChange={s.setSelectedReservationPartId}
        onSelectedReservationBinIdChange={s.setSelectedReservationBinId}
        onReservationQuantityChange={s.setReservationQuantity}
        onReservationNotesChange={s.setReservationNotes}
        onReleaseReasonChange={s.setReservationReleaseReason}
        onStatusFilterChange={s.setReservationStatusFilter}
        onCreateReservation={() => s.createStockReservationMutation.mutate()}
        onReleaseReservation={() => s.releaseStockReservationMutation.mutate()}
        onFulfillReservation={() => s.fulfillStockReservationMutation.mutate()}
        isCreating={s.createStockReservationMutation.isPending}
        isReleasing={s.releaseStockReservationMutation.isPending}
        isFulfilling={s.fulfillStockReservationMutation.isPending}
      />
    </div>
  )
}
