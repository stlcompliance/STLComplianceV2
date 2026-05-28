import { PartCatalogPanel } from '../../components/PartCatalogPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function CatalogSection({ state: s }: Props) {
  return (
    <PartCatalogPanel
      catalogs={s.catalogsQuery.data ?? []}
      parts={s.partsQuery.data ?? []}
      canManage={s.canManageCatalog}
      isLoading={s.catalogsQuery.isLoading || s.partsQuery.isLoading}
      catalogKey={s.catalogKey}
      catalogName={s.catalogName}
      catalogDescription={s.catalogDescription}
      partKey={s.partKey}
      partName={s.partName}
      partCategory={s.partCategory}
      partUom={s.partUom}
      partManufacturer={s.partManufacturer}
      partMfgNumber={s.partMfgNumber}
      selectedCatalogId={s.selectedCatalogId}
      vendorPartNumber={s.vendorPartNumber}
      selectedPartId={s.selectedPartId}
      selectedVendorId={s.selectedVendorId}
      vendors={s.vendors.map((v) => ({
        partyId: v.partyId,
        displayName: v.displayName,
        partyKey: v.partyKey,
      }))}
      onCatalogKeyChange={s.setCatalogKey}
      onCatalogNameChange={s.setCatalogName}
      onCatalogDescriptionChange={s.setCatalogDescription}
      onPartKeyChange={s.setPartKey}
      onPartNameChange={s.setPartName}
      onPartCategoryChange={s.setPartCategory}
      onPartUomChange={s.setPartUom}
      onPartManufacturerChange={s.setPartManufacturer}
      onPartMfgNumberChange={s.setPartMfgNumber}
      onSelectedCatalogIdChange={s.setSelectedCatalogId}
      onVendorPartNumberChange={s.setVendorPartNumber}
      onSelectedPartIdChange={s.setSelectedPartId}
      onSelectedVendorIdChange={s.setSelectedVendorId}
      onCreateCatalog={() => s.createCatalogMutation.mutate()}
      onCreatePart={() => s.createPartMutation.mutate()}
      onLinkVendor={() => s.linkVendorMutation.mutate()}
      isCreatingCatalog={s.createCatalogMutation.isPending}
      isCreatingPart={s.createPartMutation.isPending}
      isLinkingVendor={s.linkVendorMutation.isPending}
    />
  )
}
