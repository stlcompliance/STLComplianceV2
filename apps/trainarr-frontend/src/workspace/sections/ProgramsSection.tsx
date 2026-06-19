import { ProgramBuilderPanel } from '../../components/ProgramBuilderPanel'
import { useLocation } from 'react-router-dom'
import { TrainingProgramProfile } from './TrainingDetailProfiles'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }
type ProgramsViewMode = 'drawer' | 'details' | 'create'

export function ProgramsSection({ state }: Props) {
  const s = state
  const location = useLocation()
  const mode: ProgramsViewMode = location.pathname.startsWith('/programs/create')
    ? 'create'
    : location.pathname.startsWith('/programs/details')
      ? 'details'
      : 'drawer'
  if (mode === 'details') {
    return <TrainingProgramProfile state={s} />
  }

  return (
    <div className="space-y-6">
      {mode === 'create' ? (
        <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4 text-sm text-[var(--color-text-primary)]">
          <ol className="list-decimal space-y-1 pl-5">
            <li>Step 1: Name the course and define the learning scope for the training lifecycle.</li>
            <li>Step 2: Attach training definitions so learners inherit required content and checks.</li>
            <li>Step 3: Save and publish a version to make the course available for assignment.</li>
          </ol>
        </div>
      ) : null}
      <ProgramBuilderPanel
        accessToken={s.accessToken}
        mode={mode}
        programs={s.programsQuery.data ?? []}
        definitions={s.definitionsQuery.data ?? []}
        selectedDefinitionIds={s.selectedProgramDefinitionIds}
        selectedProgramId={s.selectedProgramId}
        selectedProgramDetail={s.programDetailQuery.data}
        programVersions={s.programVersionsQuery.data ?? []}
        selectedDefinitionIdForCitations={s.selectedDefinitionIdForCitations}
        onSelectProgram={s.setSelectedProgramId}
        onSelectDefinitionForCitations={s.setSelectedDefinitionIdForCitations}
        programKey={s.programKey}
        programName={s.programName}
        programDescription={s.programDescription}
        onProgramKeyChange={s.setProgramKey}
        onProgramNameChange={s.setProgramName}
        onProgramDescriptionChange={s.setProgramDescription}
        onToggleDefinition={s.toggleProgramDefinition}
        onCreateProgram={() => s.createProgramMutation.mutate()}
        onSaveProgram={() => s.saveProgramMutation.mutate()}
        onPublishProgram={() => s.publishProgramMutation.mutate()}
        onStartRevision={() => s.startRevisionMutation.mutate()}
        isCreating={s.createProgramMutation.isPending}
        isSaving={s.saveProgramMutation.isPending}
        isPublishing={s.publishProgramMutation.isPending}
        isStartingRevision={s.startRevisionMutation.isPending}
        canManage={s.canPrograms}
      />
    </div>
  )
}
