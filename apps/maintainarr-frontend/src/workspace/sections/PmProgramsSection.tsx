import { PmProgramBuilderPanel } from '../../components/PmProgramBuilderPanel'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }

export function PmProgramsSection({ state }: Props) {
  const s = state
  return (
    <div className="mb-8">
      <PmProgramBuilderPanel
        canManage={s.canManage}
        programs={s.pmProgramsQuery.data ?? []}
        selectedProgram={s.pmProgramDetailQuery.data ?? null}
        assetTypes={s.typesQuery.data ?? []}
        assets={s.assetsQuery.data ?? []}
        availableSchedules={s.scopedPmSchedules}
        isLoading={s.pmProgramsQuery.isLoading}
        isDetailLoading={s.pmProgramDetailQuery.isLoading}
        isSchedulesLoading={s.pmSchedulesQuery.isLoading}
        programKey={s.programKey}
        programName={s.programName}
        programDescription={s.programDescription}
        scopeType={s.programScopeType}
        selectedAssetTypeId={s.programAssetTypeId}
        selectedAssetId={s.programAssetId}
        selectedProgramId={s.selectedProgramId}
        selectedScheduleIds={s.selectedProgramScheduleIds}
        onProgramKeyChange={s.setProgramKey}
        onProgramNameChange={s.setProgramName}
        onProgramDescriptionChange={s.setProgramDescription}
        onScopeTypeChange={s.setProgramScopeType}
        onSelectedAssetTypeIdChange={s.setProgramAssetTypeId}
        onSelectedAssetIdChange={s.setProgramAssetId}
        onSelectedProgramIdChange={s.setSelectedProgramId}
        onSelectedScheduleIdsChange={s.setSelectedProgramScheduleIds}
        onCreateProgram={() => s.createPmProgramMutation.mutate()}
        onSaveSchedules={() => s.savePmProgramSchedulesMutation.mutate()}
        onActivateProgram={() => s.activatePmProgramMutation.mutate()}
        isCreatingProgram={s.createPmProgramMutation.isPending}
        isSavingSchedules={
          s.savePmProgramSchedulesMutation.isPending || s.activatePmProgramMutation.isPending
        }
      />
    </div>
  )
}
