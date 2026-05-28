import { DefectsPanel } from '../../components/DefectsPanel'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }

export function DefectsSection({ state }: Props) {
  const s = state
  return (
    <div className="mb-8">
      <DefectsPanel
        canCreate={s.canExecuteInspections}
        canCreateWorkOrder={s.canCreateWorkOrder}
        canManageStatus={s.canManageDefects}
        viewAllDefects={s.viewAllDefects}
        assets={s.assetsQuery.data ?? []}
        defects={s.defectsQuery.data ?? []}
        selectedAssetId={s.defectAssetId}
        defectTitle={s.defectTitle}
        defectDescription={s.defectDescription}
        defectSeverity={s.defectSeverity}
        statusFilter={s.defectStatusFilter}
        isLoading={s.defectsQuery.isLoading}
        isCreating={s.createDefectMutation.isPending}
        isUpdatingStatus={s.updateDefectStatusMutation.isPending}
        onSelectedAssetIdChange={s.setDefectAssetId}
        onDefectTitleChange={s.setDefectTitle}
        onDefectDescriptionChange={s.setDefectDescription}
        onDefectSeverityChange={s.setDefectSeverity}
        onStatusFilterChange={s.setDefectStatusFilter}
        onCreateDefect={() => s.createDefectMutation.mutate()}
        onCreateWorkOrderFromDefect={(defectId) => s.createWorkOrderFromDefectMutation.mutate(defectId)}
        creatingWorkOrderDefectId={s.creatingWorkOrderDefectId}
        onUpdateStatus={(defectId, status) => s.updateDefectStatusMutation.mutate({ defectId, status })}
      />
    </div>
  )
}
