import { DefectsPanel } from '../../components/DefectsPanel'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }

export function DefectsSection({ state }: Props) {
  const s = state
  const selectedDefect =
    (s.defectsQuery.data ?? []).find((defect) => defect.defectId === s.selectedDefectId) ?? null

  return (
    <div className="mb-8" data-testid="maintainarr-defects-workspace">
      <DefectsPanel
        canCreate={s.canExecuteInspections}
        canCreateWorkOrder={s.canCreateWorkOrder}
        canManageStatus={s.canManageDefects}
        canUploadEvidence={s.canExecuteInspections}
        viewAllDefects={s.viewAllDefects}
        assets={s.assetsQuery.data ?? []}
        defects={s.defectsQuery.data ?? []}
        selectedDefectId={s.selectedDefectId}
        selectedDefect={selectedDefect}
        defectEvidence={s.defectEvidenceQuery.data ?? []}
        selectedAssetId={s.defectAssetId}
        defectTitle={s.defectTitle}
        defectDescription={s.defectDescription}
        defectSeverity={s.defectSeverity}
        statusFilter={s.defectStatusFilter}
        evidenceTypeKey={s.defectEvidenceTypeKey}
        evidenceNotes={s.defectEvidenceNotes}
        selectedEvidenceFileName={s.defectEvidenceFile?.name ?? null}
        isLoading={s.defectsQuery.isLoading}
        isEvidenceLoading={s.defectEvidenceQuery.isLoading}
        isCreating={s.createDefectMutation.isPending}
        isUpdatingStatus={s.updateDefectStatusMutation.isPending}
        isUploadingEvidence={s.uploadDefectEvidenceMutation.isPending}
        onSelectedDefectIdChange={s.setSelectedDefectId}
        onSelectedAssetIdChange={s.setDefectAssetId}
        onDefectTitleChange={s.setDefectTitle}
        onDefectDescriptionChange={s.setDefectDescription}
        onDefectSeverityChange={s.setDefectSeverity}
        onStatusFilterChange={s.setDefectStatusFilter}
        onEvidenceTypeKeyChange={s.setDefectEvidenceTypeKey}
        onEvidenceNotesChange={s.setDefectEvidenceNotes}
        onSelectEvidenceFile={s.setDefectEvidenceFile}
        onUploadEvidence={() => s.uploadDefectEvidenceMutation.mutate()}
        onCreateDefect={() => s.createDefectMutation.mutate()}
        onCreateWorkOrderFromDefect={(defectId) => s.createWorkOrderFromDefectMutation.mutate(defectId)}
        creatingWorkOrderDefectId={s.creatingWorkOrderDefectId}
        onUpdateStatus={(defectId, status) => s.updateDefectStatusMutation.mutate({ defectId, status })}
      />
    </div>
  )
}
