import { RemediationAssignmentPanel } from '../../components/RemediationAssignmentPanel'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }

export function RemediationSection({ state }: Props) {
  const s = state
  return (
    <RemediationAssignmentPanel
      remediations={s.remediationsQuery.data ?? []}
      definitions={s.definitionsQuery.data ?? []}
      selectedRemediationId={s.selectedRemediationId}
      selectedDefinitionId={s.selectedDefinitionId}
      onSelectRemediation={(id) => {
        s.setSelectedRemediationId(id)
        s.setQualificationCheck(null)
      }}
      onSelectDefinition={(id) => {
        s.setSelectedDefinitionId(id)
        s.setQualificationCheck(null)
      }}
      onCreateAssignment={() => s.createAssignmentMutation.mutate()}
      isCreating={s.createAssignmentMutation.isPending}
      canManage={s.canManage}
      qualificationCheck={s.qualificationCheck}
      isCheckingQualification={s.qualificationCheckMutation.isPending}
      onRunQualificationCheck={() => s.qualificationCheckMutation.mutate()}
      rulePackKey={s.rulePackKey}
      onRulePackKeyChange={s.setRulePackKey}
      rulePackOptions={s.rulePackOptions}
    />
  )
}
