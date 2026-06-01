import { AssetReadinessDetailPanel } from '../../components/AssetReadinessDetailPanel'
import { AssetDetailsPage } from '../../components/AssetDetailsPage'
import { AssetRegistryPanel } from '../../components/AssetRegistryPanel'
import { useLocation } from 'react-router-dom'
import { ControlledSelect } from '@stl/shared-ui'
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
            <li>Step 1: Select controlled asset classification and configuration values from the provided catalogs.</li>
            <li>Step 2: Add regulatory and organizational references from Compliance Core and StaffArr selectors.</li>
            <li>Step 3: Create the asset record with validated controlled values.</li>
          </ol>
        </div>
      ) : null}
      {mode !== 'details' ? (
        <AssetRegistryPanel
          mode={mode}
          showSourceData={false}
          showAssetsTable
          showAssetCreateForm={mode === 'create'}
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
          assetFieldset={s.assetCreateFieldsetQuery.data ?? null}
          assetFieldValues={s.assetFieldValues}
          onAssetFieldChange={(fieldKey, value) =>
            s.setAssetFieldValues((current) => ({ ...current, [fieldKey]: value }))
          }
        />
      ) : null}

      {mode === 'details' ? (
        selectedAsset ? (
          <AssetDetailsPage
            asset={selectedAsset}
            readiness={s.assetReadinessDetailQuery.data ?? null}
            isReadinessLoading={s.assetReadinessDetailQuery.isLoading}
            fieldContext={s.assetFieldContextQuery.data ?? null}
          />
        ) : (
          <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5">
            <h2 className="text-lg font-medium text-white">Asset details</h2>
            <p className="mt-1 text-sm text-slate-400">Select an asset to open details context.</p>
            <div className="mt-4 max-w-xl">
              <ControlledSelect
                label="Asset"
                value={s.selectedAssetId ?? ''}
                onChange={s.setSelectedAssetId}
                options={(s.assetsQuery.data ?? []).map((item) => ({
                  value: item.assetId,
                  label: `${item.assetTag} · ${item.name}`,
                }))}
                emptyLabel="Select an asset"
              />
            </div>
          </section>
        )
      ) : null}

      {mode !== 'create' && mode !== 'details' ? (
        <AssetReadinessDetailPanel
          readiness={s.assetReadinessDetailQuery.data ?? null}
          isLoading={s.assetReadinessDetailQuery.isLoading}
          selectedAssetLabel={selectedAssetLabel}
        />
      ) : null}
    </div>
  )
}
