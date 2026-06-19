import { TrainingMatrixPanel } from '../../components/TrainingMatrixPanel'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }

export function MatrixSection({ state }: Props) {
  const s = state

  return (
    <div className="space-y-6">
      <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-gradient-to-br from-[var(--color-bg-surface)] to-[var(--color-bg-surface-elevated)] p-5">
        <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Training matrix</h2>
        <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
          This is the coverage map for role, site, equipment, route, and task requirements. It’s the LMS gate that
          tells other products whether a person can take the next step.
        </p>
      </section>

      <TrainingMatrixPanel
        entries={s.trainingMatrixQuery.data?.entries ?? []}
        programs={s.programsQuery.data ?? []}
        definitions={s.definitionsQuery.data ?? []}
        applicabilityKey={s.matrixApplicabilityKey}
        applicabilityLabel={s.matrixApplicabilityLabel}
        targetType={s.matrixTargetType}
        targetId={s.matrixTargetId}
        requirementLevel={s.matrixRequirementLevel}
        sortOrder={s.matrixSortOrder}
        onApplicabilityKeyChange={s.setMatrixApplicabilityKey}
        onApplicabilityLabelChange={s.setMatrixApplicabilityLabel}
        onTargetTypeChange={s.setMatrixTargetType}
        onTargetIdChange={s.setMatrixTargetId}
        onRequirementLevelChange={s.setMatrixRequirementLevel}
        onSortOrderChange={s.setMatrixSortOrder}
        onCreateEntry={() => s.createMatrixEntryMutation.mutate()}
        onDeleteEntry={(matrixEntryId) => s.deleteMatrixEntryMutation.mutate(matrixEntryId)}
        isCreating={s.createMatrixEntryMutation.isPending}
        deletingEntryId={s.deletingMatrixEntryId}
        canManage={s.canPrograms}
      />
    </div>
  )
}
