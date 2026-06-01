import { AssetReadinessDetailPanel } from '../../components/AssetReadinessDetailPanel'
import { AssetRegistryPanel } from '../../components/AssetRegistryPanel'
import { useLocation } from 'react-router-dom'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }
type AssetsViewMode = 'drawer' | 'details' | 'create'

export function AssetsSection({ state }: Props) {
  const s = state
  const location = useLocation()
  const mode: AssetsViewMode = location.pathname.startsWith('/assets/create')
    ? 'create'
    : location.pathname.startsWith('/assets/details')
      ? 'details'
      : 'drawer'
  const selectedAsset = (s.assetsQuery.data ?? []).find((item) => item.assetId === s.selectedAssetId)
  const selectedAssetLabel = selectedAsset
    ? `${selectedAsset.assetTag} · ${selectedAsset.name}`
    : null

  return (
    <div className="space-y-6" data-testid="maintainarr-assets-workspace">
      {mode === 'create' ? (
        <div className="rounded-xl border border-amber-700/50 bg-amber-950/20 p-4 text-sm text-amber-100">
          <ol className="list-decimal space-y-1 pl-5">
            <li>Step 1: Create an asset class so related asset types share the same business grouping.</li>
            <li>Step 2: Create an asset type to standardize maintenance plans and readiness checks.</li>
            <li>Step 3: Create an asset record to place a real unit under tracking and lifecycle control.</li>
          </ol>
        </div>
      ) : null}
      <AssetRegistryPanel
        mode={mode}
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
        confirmedClassKey={s.confirmedClassKey}
        selectedClassId={s.selectedClassId}
        typeName={s.typeName}
        typeDescription={s.typeDescription}
        confirmedTypeKey={s.confirmedTypeKey}
        selectedTypeId={s.selectedTypeId}
        assetTag={s.assetTag}
        assetName={s.assetName}
        assetDescription={s.assetDescription}
        siteRef={s.siteRef}
        onClassNameChange={s.setClassName}
        onClassDescriptionChange={s.setClassDescription}
        onSelectedClassIdChange={s.setSelectedClassId}
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

      {mode !== 'create' ? (
        <AssetReadinessDetailPanel
          readiness={s.assetReadinessDetailQuery.data ?? null}
          isLoading={s.assetReadinessDetailQuery.isLoading}
          selectedAssetLabel={selectedAssetLabel}
        />
      ) : null}
    </div>
  )
}
