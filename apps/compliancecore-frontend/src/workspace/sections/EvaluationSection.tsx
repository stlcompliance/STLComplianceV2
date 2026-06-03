import { EvaluationHistoryExplorerPanel } from '../../components/EvaluationHistoryExplorerPanel'
import { RuleEvaluationPanel } from '../../components/RuleEvaluationPanel'
import { SituationEvaluatorPanel } from '../../components/SituationEvaluatorPanel'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function EvaluationSection({ state }: Props) {
  const s = state
  return (
    <div className="space-y-8">
      <SituationEvaluatorPanel
        accessToken={s.accessToken}
        canEvaluate={s.canManage || s.canEvaluateRisk}
        factRequirements={s.factRequirementsQuery.data ?? []}
      />
      <RuleEvaluationPanel
        rulePacks={s.rulePacksQuery.data ?? []}
        factDefinitions={s.factDefinitionsQuery.data ?? []}
        selectedRulePackId={s.selectedRulePackId}
        onSelectRulePack={s.setSelectedRulePackId}
        content={s.rulePackContentQuery.data?.content ?? null}
        hasContent={s.rulePackContentQuery.data?.hasContent ?? false}
        evaluationRuns={s.ruleEvaluationsQuery.data ?? []}
        canManage={s.canManage}
        onSaveContent={(content) => s.saveRuleContentMutation.mutate(content)}
        isSavingContent={s.saveRuleContentMutation.isPending}
        onSeedContent={() => s.seedRuleContentMutation.mutate()}
        isSeedingContent={s.seedRuleContentMutation.isPending}
        onEvaluate={(facts) => s.evaluateRulePackMutation.mutate(facts)}
        isEvaluating={s.evaluateRulePackMutation.isPending}
        lastEvaluation={s.lastEvaluation}
        onEvaluateBatch={(rulePackKeys, facts, emitFindings) =>
          s.evaluateRulePackBatchMutation.mutate({ rulePackKeys, facts, emitFindings })
        }
        isEvaluatingBatch={s.evaluateRulePackBatchMutation.isPending}
        lastBatchEvaluation={s.lastBatchEvaluation}
      />
      <EvaluationHistoryExplorerPanel
        accessToken={s.accessToken}
        rulePacks={s.rulePacksQuery.data ?? []}
        evaluationRuns={s.allRuleEvaluationsQuery.data ?? []}
        canExportAudit={s.canExportAudit}
        onFocusRulePack={s.setSelectedRulePackId}
      />
    </div>
  )
}
