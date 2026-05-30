import {
  removeTrainingDefinitionRulePackRequirement,
  removeTrainingProgramRulePackRequirement,
} from '../../api/client'
import { RulePackImpactPanel } from '../../components/RulePackImpactPanel'
import { RulePackRequirementPanel } from '../../components/RulePackRequirementPanel'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }

export function RulePacksSection({ state }: Props) {
  const s = state
  return (
    <>
      {s.selectedDefinitionIdForCitations ? (
        <RulePackRequirementPanel
          title="Training definition rule pack requirements"
          requirements={s.definitionRulePackRequirementsQuery.data ?? []}
          rulePackKeyInput={s.rulePackKeyInput}
          rulePackOptions={s.rulePackOptions}
          onRulePackKeyChange={s.setRulePackKeyInput}
          onSave={() => s.upsertDefinitionRulePackMutation.mutate()}
          onRemove={async (requirementId) => {
            s.setRemovingRulePackRequirementId(requirementId)
            try {
              await removeTrainingDefinitionRulePackRequirement(
                s.accessToken,
                s.selectedDefinitionIdForCitations!,
                requirementId,
              )
              await s.queryClient.invalidateQueries({
                queryKey: ['trainarr-definition-rule-packs', s.session.accessToken, s.selectedDefinitionIdForCitations],
              })
            } finally {
              s.setRemovingRulePackRequirementId(null)
            }
          }}
          isSaving={s.upsertDefinitionRulePackMutation.isPending}
          isRemovingId={s.removingRulePackRequirementId}
          canManage={s.canPrograms}
          validateWithComplianceCore={s.validateRulePackWithComplianceCore}
          onValidateWithComplianceCoreChange={s.setValidateRulePackWithComplianceCore}
        />
      ) : (
        <p className="text-sm text-slate-400">Select a training definition on the Programs page to manage rule packs.</p>
      )}

      {s.selectedProgramId ? (
        <RulePackRequirementPanel
          title="Training program rule pack requirements"
          requirements={s.programRulePackRequirementsQuery.data ?? []}
          rulePackKeyInput={s.rulePackKeyInput}
          rulePackOptions={s.rulePackOptions}
          onRulePackKeyChange={s.setRulePackKeyInput}
          onSave={() => s.upsertProgramRulePackMutation.mutate()}
          onRemove={async (requirementId) => {
            s.setRemovingRulePackRequirementId(requirementId)
            try {
              await removeTrainingProgramRulePackRequirement(
                s.accessToken,
                s.selectedProgramId!,
                requirementId,
              )
              await s.queryClient.invalidateQueries({
                queryKey: ['trainarr-program-rule-packs', s.session.accessToken, s.selectedProgramId],
              })
            } finally {
              s.setRemovingRulePackRequirementId(null)
            }
          }}
          isSaving={s.upsertProgramRulePackMutation.isPending}
          isRemovingId={s.removingRulePackRequirementId}
          canManage={s.canPrograms}
          validateWithComplianceCore={s.validateRulePackWithComplianceCore}
          onValidateWithComplianceCoreChange={s.setValidateRulePackWithComplianceCore}
        />
      ) : null}

      {s.canImpact ? (
        <RulePackImpactPanel
          rulePackKeyInput={s.impactRulePackKeyInput}
          rulePackOptions={s.rulePackOptions}
          onRulePackKeyChange={(value) => {
            s.setImpactRulePackKeyInput(value)
            s.setRulePackImpactAssessment(null)
          }}
          onAssess={() => s.rulePackImpactMutation.mutate()}
          isAssessing={s.rulePackImpactMutation.isPending}
          canAssess={s.canImpact}
          assessment={s.rulePackImpactAssessment}
        />
      ) : null}
    </>
  )
}
