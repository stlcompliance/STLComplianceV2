import { MaintenanceHistoryPanel } from '../../components/MaintenanceHistoryPanel'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }

export function HistorySection({ state }: Props) {
  const s = state
  return (
    <div className="mt-8">
      <MaintenanceHistoryPanel
        assets={s.assetsQuery.data ?? []}
        entries={s.maintenanceHistoryQuery.data?.items ?? []}
        totalCount={s.maintenanceHistoryQuery.data?.totalCount ?? 0}
        selectedAssetId={s.historyAssetId}
        isLoading={s.maintenanceHistoryQuery.isLoading}
        onSelectedAssetIdChange={s.setHistoryAssetId}
      />
    </div>
  )
}
