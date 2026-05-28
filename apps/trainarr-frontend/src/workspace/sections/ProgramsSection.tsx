import { ProgramBuilderPanel } from '../../components/ProgramBuilderPanel'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }

export function ProgramsSection({ state }: Props) {
  const s = state
  return (
    <ProgramBuilderPanel
      programs={s.programsQuery.data ?? []}
      definitions={s.definitionsQuery.data ?? []}
      selectedDefinitionIds={s.selectedProgramDefinitionIds}
      selectedProgramId={s.selectedProgramId}
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
      isCreating={s.createProgramMutation.isPending}
      canManage={s.canPrograms}
    />
  )
}
