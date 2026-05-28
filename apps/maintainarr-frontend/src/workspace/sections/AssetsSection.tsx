import { AssetRegistryPanel } from '../../components/AssetRegistryPanel'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }

export function AssetsSection({ state }: Props) {
  const s = state
  return (
    <AssetRegistryPanel
      canManage={s.canManage}
      classes={s.classesQuery.data ?? []}
      types={s.typesQuery.data ?? []}
      assets={s.assetsQuery.data ?? []}
      readinessByAssetId={Object.fromEntries(
        (s.assetReadinessFleetQuery.data ?? []).map((item) => [item.assetId, item]),
      )}
      isLoading={s.classesQuery.isLoading || s.typesQuery.isLoading || s.assetsQuery.isLoading}
      isReadinessLoading={s.assetReadinessFleetQuery.isLoading}
      classKey={s.classKey}
      className={s.className}
      classDescription={s.classDescription}
      selectedClassId={s.selectedClassId}
      typeKey={s.typeKey}
      typeName={s.typeName}
      typeDescription={s.typeDescription}
      selectedTypeId={s.selectedTypeId}
      assetTag={s.assetTag}
      assetName={s.assetName}
      assetDescription={s.assetDescription}
      siteRef={s.siteRef}
      onClassKeyChange={s.setClassKey}
      onClassNameChange={s.setClassName}
      onClassDescriptionChange={s.setClassDescription}
      onSelectedClassIdChange={s.setSelectedClassId}
      onTypeKeyChange={s.setTypeKey}
      onTypeNameChange={s.setTypeName}
      onTypeDescriptionChange={s.setTypeDescription}
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
  )
}
