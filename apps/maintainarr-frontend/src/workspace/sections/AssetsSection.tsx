import { AssetReadinessDetailPanel } from '../../components/AssetReadinessDetailPanel'
import { AssetRegistryPanel } from '../../components/AssetRegistryPanel'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }

export function AssetsSection({ state }: Props) {
  const s = state
  const selectedAsset = (s.assetsQuery.data ?? []).find((item) => item.assetId === s.selectedAssetId)
  const selectedAssetLabel = selectedAsset
    ? `${selectedAsset.assetTag} · ${selectedAsset.name}`
    : null

  return (
    <div className="space-y-6" data-testid="maintainarr-assets-workspace">
      <AssetRegistryPanel
        canManage={s.canManage}
        classes={s.classesQuery.data ?? []}
        types={s.typesQuery.data ?? []}
        assets={s.assetsQuery.data ?? []}
        readinessByAssetId={Object.fromEntries(
          (s.assetReadinessFleetQuery.data ?? []).map((item) => [item.assetId, item]),
        )}
        selectedAssetId={s.selectedAssetId}
        onSelectAsset={s.setSelectedAssetId}
        isLoading={s.classesQuery.isLoading || s.typesQuery.isLoading || s.assetsQuery.isLoading}
        isReadinessLoading={s.assetReadinessFleetQuery.isLoading}
        className={s.className}
        classDescription={s.classDescription}
        classKeyManualOverride={s.classKeyManualOverride}
        confirmedClassKey={s.confirmedClassKey}
        selectedClassId={s.selectedClassId}
        typeName={s.typeName}
        typeDescription={s.typeDescription}
        typeKeyManualOverride={s.typeKeyManualOverride}
        confirmedTypeKey={s.confirmedTypeKey}
        selectedTypeId={s.selectedTypeId}
        assetTag={s.assetTag}
        assetName={s.assetName}
        assetDescription={s.assetDescription}
        siteRef={s.siteRef}
        onClassNameChange={s.setClassName}
        onClassDescriptionChange={s.setClassDescription}
        onClassKeyManualOverrideChange={s.setClassKeyManualOverride}
        onSelectedClassIdChange={s.setSelectedClassId}
        onTypeNameChange={s.setTypeName}
        onTypeDescriptionChange={s.setTypeDescription}
        onTypeKeyManualOverrideChange={s.setTypeKeyManualOverride}
        onSelectedTypeIdChange={s.setSelectedTypeId}
        onAssetTagChange={s.setAssetTag}
        onAssetNameChange={s.setAssetName}
        onAssetDescriptionChange={s.setAssetDescription}
        onSiteRefChange={s.setSiteRef}
        onCreateClass={() => s.createClassMutation.mutate()}
        onCreateType={() => s.createTypeMutation.mutate()}
        onCreateAsset={() => s.createAssetMutation.mutate()}
        isCreatingClass={s.createClassMutation.isPending}
        isCreatingType={s.createTypeMutation.isPending}
        isCreatingAsset={s.createAssetMutation.isPending}
      />

      <AssetReadinessDetailPanel
        readiness={s.assetReadinessDetailQuery.data ?? null}
        isLoading={s.assetReadinessDetailQuery.isLoading}
        selectedAssetLabel={selectedAssetLabel}
      />
    </div>
  )
}
