import { InventoryPanel } from '../../components/InventoryPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function InventorySection({ state: s }: Props) {
  return (
    <InventoryPanel
      locations={s.locations}
      bins={s.binsQuery.data ?? []}
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
  )
}
