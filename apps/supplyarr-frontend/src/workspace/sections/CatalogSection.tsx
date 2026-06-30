import { PartCatalogPanel } from '../../components/PartCatalogPanel'
import { PartSubstitutionsPanel } from '../../components/PartSubstitutionsPanel'
import { VendorCatalogApiPanel } from '../../components/VendorCatalogApiPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'
import type { SupplierUnitPickerSource } from '../../forms/controlledFormHelpers'

type Props = { state: SupplyArrWorkspaceState }

export function CatalogSection({ state: s }: Props) {
  const suppliers: SupplierUnitPickerSource[] = s.supplierDirectory.map((supplier) => ({
    supplierId: supplier.supplierId,
    displayName: supplier.displayName,
    supplierKey: supplier.supplierKey,
    parentSupplierDisplayName: supplier.parentSupplierDisplayName,
    unitKind: supplier.unitKind,
  }))

  return (
    <div className="space-y-6">
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
        partIsTrackable={s.partIsTrackable}
        partIsStocked={s.partIsStocked}
        selectedCatalogId={s.selectedCatalogId}
        selectedSourcePartId={s.selectedSourcePartId}
        partSourceType={s.partSourceType}
        partSourceLabel={s.partSourceLabel}
        partSourceNotes={s.partSourceNotes}
        supplierPartNumber={s.supplierPartNumber}
        selectedPartId={s.selectedPartId}
        selectedSupplierUnitId={s.selectedSupplierUnitId}
        suppliers={suppliers}
        onCatalogKeyChange={s.setCatalogKey}
        onCatalogNameChange={s.setCatalogName}
        onCatalogDescriptionChange={s.setCatalogDescription}
        onPartKeyChange={s.setPartKey}
        onPartNameChange={s.setPartName}
        onPartCategoryChange={s.setPartCategory}
        onPartUomChange={s.setPartUom}
        onPartManufacturerChange={s.setPartManufacturer}
        onPartMfgNumberChange={s.setPartMfgNumber}
        onPartIsTrackableChange={s.setPartIsTrackable}
        onPartIsStockedChange={s.setPartIsStocked}
        onSelectedCatalogIdChange={s.setSelectedCatalogId}
        onSelectedSourcePartIdChange={s.setSelectedSourcePartId}
        onPartSourceTypeChange={s.setPartSourceType}
        onPartSourceLabelChange={s.setPartSourceLabel}
        onPartSourceNotesChange={s.setPartSourceNotes}
        onSupplierPartNumberChange={s.setSupplierPartNumber}
        onSelectedPartIdChange={s.setSelectedPartId}
        onSelectedSupplierUnitIdChange={s.setSelectedSupplierUnitId}
        onCreateCatalog={() => s.createCatalogMutation.mutate()}
        onCreatePart={() => s.createPartMutation.mutate()}
        onCreatePartSource={() => s.createPartSourceMutation.mutate()}
        onLinkSupplierSource={() => s.linkSupplierSourceMutation.mutate()}
        isCreatingCatalog={s.createCatalogMutation.isPending}
        isCreatingPart={s.createPartMutation.isPending}
        isCreatingPartSource={s.createPartSourceMutation.isPending}
        isLinkingSupplierSource={s.linkSupplierSourceMutation.isPending}
      />

      <PartSubstitutionsPanel
        accessToken={s.accessToken}
        parts={s.partsQuery.data ?? []}
        canRead={s.canReadPartSubstitutions}
        selectedPartId={s.substitutionPartId}
        onSelectedPartIdChange={s.setSubstitutionPartId}
      />

      <VendorCatalogApiPanel
        accessToken={s.accessToken}
        canManage={s.canManageCatalog}
        parts={s.partsQuery.data ?? []}
        suppliers={suppliers}
      />
    </div>
  )
}
