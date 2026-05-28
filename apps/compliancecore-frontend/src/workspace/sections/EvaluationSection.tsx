import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function EvaluationSection({ state }: Props) {
  const {
    content,
    data,
    emitFindings,
    evaluateRulePackBatchMutation,
    evaluateRulePackMutation,
    evaluationRuns,
    factDefinitions,
    factDefinitionsQuery,
    facts,
    hasContent,
    isEvaluating,
    isEvaluatingBatch,
    isPending,
    isSavingContent,
    isSeedingContent,
    lastBatchEvaluation,
    lastEvaluation,
    mutate,
    onEvaluate,
    onEvaluateBatch,
    onSaveContent,
    onSeedContent,
    onSelectRulePack,
    ruleEvaluationsQuery,
    rulePackContentQuery,
    rulePackKeys,
    rulePacks,
    rulePacksQuery,
    saveRuleContentMutation,
    seedRuleContentMutation,
    selectedRulePackId,
    setSelectedRulePackId,
  } = state
  return (
    <>
      <RuleEvaluationPanel
      
                rulePacks={rulePacksQuery.data ?? []}
      
                factDefinitions={factDefinitionsQuery.data ?? []}
      
                selectedRulePackId={selectedRulePackId}
      
                onSelectRulePack={setSelectedRulePackId}
      
                content={rulePackContentQuery.data?.content ?? null}
      
                hasContent={rulePackContentQuery.data?.hasContent ?? false}
      
                evaluationRuns={ruleEvaluationsQuery.data ?? []}
      
                canManage={canManage}
      
                onSaveContent={(content) => saveRuleContentMutation.mutate(content)}
      
                isSavingContent={saveRuleContentMutation.isPending}
      
                onSeedContent={() => seedRuleContentMutation.mutate()}
      
                isSeedingContent={seedRuleContentMutation.isPending}
      
                onEvaluate={(facts) => evaluateRulePackMutation.mutate(facts)}
      
                isEvaluating={evaluateRulePackMutation.isPending}
      
                lastEvaluation={lastEvaluation}
      
                onEvaluateBatch={(rulePackKeys, facts, emitFindings) =>
                  evaluateRulePackBatchMutation.mutate({ rulePackKeys, facts, emitFindings })
                }
      
                isEvaluatingBatch={evaluateRulePackBatchMutation.isPending}
      
                lastBatchEvaluation={lastBatchEvaluation}
      
              />
    </>
  )
}
