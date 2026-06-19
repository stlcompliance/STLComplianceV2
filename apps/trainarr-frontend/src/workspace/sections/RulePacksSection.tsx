import {
  removeTrainingDefinitionRulePackRequirement,
  removeTrainingProgramRulePackRequirement,
} from '../../api/client'
import { useLocation } from 'react-router-dom'
import { RulePackRequirementPanel } from '../../components/RulePackRequirementPanel'
import { RulePackProfile } from './TrainingDetailProfiles'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }
type RulePacksViewMode = 'drawer' | 'details' | 'create'

export function RulePacksSection({ state }: Props) {
  const s = state
  const location = useLocation()
  const mode: RulePacksViewMode = location.pathname.startsWith('/rule-packs/create')
    ? 'create'
    : location.pathname.startsWith('/rule-packs/details')
      ? 'details'
      : 'drawer'
  if (mode === 'details') {
    return <RulePackProfile state={s} />
  }

  return (
    <>
      {mode === 'create' ? (
        <div className="mb-4 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4 text-sm text-[var(--color-text-primary)]">
          <ol className="list-decimal space-y-1 pl-5">
            <li>Step 1: Choose a definition or program context for the requirement mapping.</li>
            <li>Step 2: Select a Compliance Core rule pack reference for the requirement.</li>
            <li>Step 3: Save and validate so downstream qualification logic uses the latest reference.</li>
          </ol>
        </div>
      ) : null}
      {s.selectedDefinitionIdForCitations ? (
        <RulePackRequirementPanel
          mode={mode}
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
        <p className="text-sm text-[var(--color-text-muted)]">Select a training definition on the Programs page to manage rule packs.</p>
      )}

      {s.selectedProgramId ? (
        <RulePackRequirementPanel
          mode={mode}
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

    </>
  )
}
