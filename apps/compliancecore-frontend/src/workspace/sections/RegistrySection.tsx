import { RegulatoryRegistryPanel } from '../../components/RegulatoryRegistryPanel'
import { VocabularyPanel } from '../../components/VocabularyPanel'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function RegistrySection({ state }: Props) {
  const s = state
  return (
    <>
      <VocabularyPanel
        types={s.typesQuery.data ?? []}
        terms={s.termsQuery.data ?? []}
        complianceKeys={s.complianceKeysQuery.data ?? []}
        materialKeys={s.materialKeysQuery.data ?? []}
        selectedTypeKey={s.selectedTypeKey}
        onSelectType={s.setSelectedTypeKey}
        canManage={s.canManage}
        onCreateTerm={() => s.seedMutation.mutate()}
        isCreatingTerm={s.seedMutation.isPending}
      />
      <RegulatoryRegistryPanel
        governingBodies={s.governingBodiesQuery.data ?? []}
        jurisdictions={s.jurisdictionsQuery.data ?? []}
        programs={s.programsQuery.data ?? []}
        rulePacks={s.rulePacksQuery.data ?? []}
        canManage={s.canManage}
        onSeedRegistry={() => s.seedRegistryMutation.mutate()}
        isSeeding={s.seedRegistryMutation.isPending}
        onAdvanceRulePack={(rulePackId, status) => s.advanceRulePackMutation.mutate({ rulePackId, status })}
        isAdvancingRulePack={s.advanceRulePackMutation.isPending}
      />
    </>
  )
}
