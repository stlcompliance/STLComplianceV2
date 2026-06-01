import {
  removeTrainingDefinitionRulePackRequirement,
  removeTrainingProgramRulePackRequirement,
} from '../../api/client'
import { ControlledSelect } from '@stl/shared-ui'
import { useLocation } from 'react-router-dom'
import { RulePackImpactPanel } from '../../components/RulePackImpactPanel'
import { RulePackRequirementPanel } from '../../components/RulePackRequirementPanel'
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
  return (
    <>
      {mode === 'create' ? (
        <div className="mb-4 rounded-xl border border-violet-700/50 bg-violet-950/20 p-4 text-sm text-violet-100">
          <ol className="list-decimal space-y-1 pl-5">
            <li>Step 1: Choose a definition or program context for the requirement mapping.</li>
            <li>Step 2: Select a Compliance Core rule pack reference for the requirement.</li>
            <li>Step 3: Save and validate so downstream qualification logic uses the latest reference.</li>
          </ol>
        </div>
      ) : null}
      {mode === 'details' ? (
        <div className="mb-4 grid gap-3 rounded-xl border border-slate-700 bg-slate-900/60 p-4 md:grid-cols-2">
          <ControlledSelect
            label="Training definition context"
            value={s.selectedDefinitionIdForCitations ?? ''}
            onChange={s.setSelectedDefinitionIdForCitations}
            options={(s.definitionsQuery.data ?? []).map((item) => ({
              value: item.trainingDefinitionId,
              label: item.name,
            }))}
            emptyLabel="Select definition"
          />
          <ControlledSelect
            label="Training program context"
            value={s.selectedProgramId ?? ''}
            onChange={s.setSelectedProgramId}
            options={(s.programsQuery.data ?? []).map((item) => ({
              value: item.programId,
              label: item.name,
            }))}
            emptyLabel="Select program"
          />
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
        <p className="text-sm text-slate-400">Select a training definition on the Programs page to manage rule packs.</p>
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

      {mode === 'details' && s.canImpact ? (
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
