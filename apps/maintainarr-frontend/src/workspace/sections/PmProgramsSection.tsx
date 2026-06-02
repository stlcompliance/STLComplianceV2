import { PmProgramBuilderPanel } from '../../components/PmProgramBuilderPanel'
import { useLocation } from 'react-router-dom'
import { PmProgramProfile } from './MaintenanceDetailProfiles'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }
type PmProgramsViewMode = 'drawer' | 'details' | 'create'

export function PmProgramsSection({ state }: Props) {
  const s = state
  const location = useLocation()
  const mode: PmProgramsViewMode = location.pathname.startsWith('/pm-programs/create')
    ? 'create'
      : location.pathname.startsWith('/pm-programs/details')
      ? 'details'
      : 'drawer'
  if (mode === 'details') {
    return <PmProgramProfile state={s} />
  }

  return (
    <div className="mb-8">
      {mode === 'create' ? (
        <div className="mb-4 rounded-xl border border-amber-700/50 bg-amber-950/20 p-4 text-sm text-amber-100">
          <ol className="list-decimal space-y-1 pl-5">
            <li>Step 1: Name and scope the PM program to the asset type or specific asset it governs.</li>
            <li>Step 2: Attach eligible schedules so compliance intervals run from one program reference.</li>
            <li>Step 3: Activate when assignments are complete so the program becomes operational.</li>
          </ol>
        </div>
      ) : null}
      <PmProgramBuilderPanel
        mode={mode}
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
