import { AvailabilitySnapshotsPanel } from '../../components/AvailabilitySnapshotsPanel'
import { PricingLeadTimePanel } from '../../components/PricingLeadTimePanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function PricingSection({ state: s }: Props) {
  return (
    <div className="grid gap-6 lg:grid-cols-2" data-testid="supplyarr-pricing-snapshots-workspace">
      <PricingLeadTimePanel
        parts={s.partsQuery.data ?? []}
        vendors={s.supplierDirectory}
        pricingSnapshots={s.pricingSnapshotsQuery.data ?? []}
        leadTimeSnapshots={s.leadTimeSnapshotsQuery.data ?? []}
        canManage={s.canManageCatalog}
        isLoading={s.pricingSnapshotsQuery.isLoading || s.leadTimeSnapshotsQuery.isLoading}
        pricingSnapshotKey={s.pricingSnapshotKey}
        leadTimeSnapshotKey={s.leadTimeSnapshotKey}
        selectedVendorLinkId={s.selectedSnapshotVendorLinkId}
        unitPrice={s.snapshotUnitPrice}
        currencyCode={s.snapshotCurrencyCode}
        minimumOrderQuantity={s.snapshotMinimumOrderQty}
        leadTimeDays={s.snapshotLeadTimeDays}
        snapshotNotes={s.snapshotNotes}
        currentOnlyFilter={s.snapshotCurrentOnly}
        onPricingSnapshotKeyChange={s.setPricingSnapshotKey}
        onLeadTimeSnapshotKeyChange={s.setLeadTimeSnapshotKey}
        onSelectedVendorLinkIdChange={s.setSelectedSnapshotVendorLinkId}
        onUnitPriceChange={s.setSnapshotUnitPrice}
        onCurrencyCodeChange={s.setSnapshotCurrencyCode}
        onMinimumOrderQuantityChange={s.setSnapshotMinimumOrderQty}
        onLeadTimeDaysChange={s.setSnapshotLeadTimeDays}
        onSnapshotNotesChange={s.setSnapshotNotes}
        onCurrentOnlyFilterChange={s.setSnapshotCurrentOnly}
        onCreatePricingSnapshot={() => s.createPricingSnapshotMutation.mutate()}
        onCreateLeadTimeSnapshot={() => s.createLeadTimeSnapshotMutation.mutate()}
        isCreatingPricing={s.createPricingSnapshotMutation.isPending}
        isCreatingLeadTime={s.createLeadTimeSnapshotMutation.isPending}
      />
      <AvailabilitySnapshotsPanel
        parts={s.partsQuery.data ?? []}
        availabilitySnapshots={s.availabilitySnapshotsQuery.data ?? []}
        canManage={s.canManageCatalog}
        isLoading={s.availabilitySnapshotsQuery.isLoading}
        snapshotKey={s.availabilitySnapshotKey}
        selectedVendorLinkId={s.selectedAvailabilityVendorLinkId}
        quantityAvailable={s.availabilityQuantity}
        availabilityStatus={s.availabilityStatus}
        snapshotNotes={s.availabilityNotes}
        currentOnlyFilter={s.availabilityCurrentOnly}
        onSnapshotKeyChange={s.setAvailabilitySnapshotKey}
        onSelectedVendorLinkIdChange={s.setSelectedAvailabilityVendorLinkId}
        onQuantityAvailableChange={s.setAvailabilityQuantity}
        onAvailabilityStatusChange={s.setAvailabilityStatus}
        onSnapshotNotesChange={s.setAvailabilityNotes}
        onCurrentOnlyFilterChange={s.setAvailabilityCurrentOnly}
        onCreateAvailabilitySnapshot={() => s.createAvailabilitySnapshotMutation.mutate()}
        isCreating={s.createAvailabilitySnapshotMutation.isPending}
      />
    </div>
  )
}
