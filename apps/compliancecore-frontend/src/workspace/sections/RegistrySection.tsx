import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function RegistrySection({ state }: Props) {
  const {
    advanceRulePackMutation,
    complianceKeys,
    complianceKeysQuery,
    data,
    governingBodies,
    governingBodiesQuery,
    isAdvancingRulePack,
    isCreatingTerm,
    isPending,
    isSeeding,
    jurisdictions,
    jurisdictionsQuery,
    materialKeys,
    materialKeysQuery,
    mutate,
    onAdvanceRulePack,
    onCreateTerm,
    onSeedRegistry,
    onSelectType,
    programs,
    programsQuery,
    rulePackId,
    rulePacks,
    rulePacksQuery,
    seedMutation,
    seedRegistryMutation,
    selectedTypeKey,
    setSelectedTypeKey,
    status,
    terms,
    termsQuery,
    types,
    typesQuery,
  } = state
  return (
    <>
      <>
              <VocabularyPanel
      
                types={typesQuery.data ?? []}
      
                terms={termsQuery.data ?? []}
      
                complianceKeys={complianceKeysQuery.data ?? []}
      
                materialKeys={materialKeysQuery.data ?? []}
      
                selectedTypeKey={selectedTypeKey}
      
                onSelectType={setSelectedTypeKey}
      
                canManage={canManage}
      
                onCreateTerm={() => seedMutation.mutate()}
      
                isCreatingTerm={seedMutation.isPending}
      
              />
      
              <RegulatoryRegistryPanel
      
                governingBodies={governingBodiesQuery.data ?? []}
      
                jurisdictions={jurisdictionsQuery.data ?? []}
      
                programs={programsQuery.data ?? []}
      
                rulePacks={rulePacksQuery.data ?? []}
      
                canManage={canManage}
      
                onSeedRegistry={() => seedRegistryMutation.mutate()}
      
                isSeeding={seedRegistryMutation.isPending}
      
                onAdvanceRulePack={(rulePackId, status) => advanceRulePackMutation.mutate({ rulePackId, status })}
      
                isAdvancingRulePack={advanceRulePackMutation.isPending}
      
              />
              </>
    </>
  )
}
