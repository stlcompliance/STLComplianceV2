import { AssetReadinessDetailPanel } from '../../components/AssetReadinessDetailPanel'
import { AssetDetailsPage } from '../../components/AssetDetailsPage'
import { AssetRegistryPanel } from '../../components/AssetRegistryPanel'
import { useLocation, useNavigate } from 'react-router-dom'
import { ControlledSelect } from '@stl/shared-ui'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }
type AssetsViewMode = 'drawer' | 'details'

export function AssetsSection({ state }: Props) {
  const s = state
  const location = useLocation()
  const navigate = useNavigate()
  const mode: AssetsViewMode = location.pathname.startsWith('/assets/details')
      ? 'details'
      : 'drawer'
  const selectedAsset = (s.assetsQuery.data ?? []).find((item) => item.assetId === s.selectedAssetId)
  const selectedAssetLabel = selectedAsset
    ? `${selectedAsset.assetTag} · ${selectedAsset.name}`
    : null

  return (
    <div className="space-y-6" data-testid="maintainarr-assets-workspace">
      {mode !== 'details' ? (
        <AssetRegistryPanel
          showSourceData={false}
          showAssetsTable
          classes={s.classesQuery.data ?? []}
          types={s.typesQuery.data ?? []}
          assets={s.assetsQuery.data ?? []}
          readinessByAssetId={Object.fromEntries(
            (s.assetReadinessFleetQuery.data ?? []).map((item) => [item.assetId, item]),
          )}
          selectedAssetId={s.selectedAssetId}
          onSelectAsset={(assetId) => {
            s.setSelectedAssetId(assetId)
            navigate(`/assets/${assetId}`)
          }}
          isLoading={s.classesQuery.isLoading || s.typesQuery.isLoading || s.assetsQuery.isLoading}
          isReadinessLoading={s.assetReadinessFleetQuery.isLoading}
        />
      ) : null}

      {mode === 'details' ? (
        selectedAsset ? (
          <AssetDetailsPage
            asset={selectedAsset}
            readiness={s.assetReadinessDetailQuery.data ?? null}
            isReadinessLoading={s.assetReadinessDetailQuery.isLoading}
            readinessHistory={s.assetReadinessHistoryQuery.data ?? null}
            isReadinessHistoryLoading={s.assetReadinessHistoryQuery.isLoading}
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

      {mode !== 'details' ? (
        <AssetReadinessDetailPanel
          readiness={s.assetReadinessDetailQuery.data ?? null}
          isLoading={s.assetReadinessDetailQuery.isLoading}
          selectedAssetLabel={selectedAssetLabel}
        />
      ) : null}
    </div>
  )
}
