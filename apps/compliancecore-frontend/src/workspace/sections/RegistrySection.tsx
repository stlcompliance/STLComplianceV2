import { useLocation } from 'react-router-dom'
import { RegulatoryRegistryPanel } from '../../components/RegulatoryRegistryPanel'
import { RuleTestCasesPanel } from '../../components/RuleTestCasesPanel'
import { RuleVersionManagementPanel } from '../../components/RuleVersionManagementPanel'
import { SdsHazComReferencesPanel } from '../../components/SdsHazComReferencesPanel'
import { VocabularyPanel } from '../../components/VocabularyPanel'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'
import { RegistryDetailProfile } from './RegistryDetailProfile'

type Props = { state: ComplianceCoreWorkspaceState }

export function RegistrySection({ state }: Props) {
  const s = state
  const location = useLocation()
  const isDetails = location.pathname.endsWith('/details')

  if (isDetails) {
    return <RegistryDetailProfile state={s} />
  }

  return (
    <>
      <VocabularyPanel
        types={s.typesQuery.data ?? []}
        terms={s.termsQuery.data ?? []}
        complianceKeys={s.complianceKeysQuery.data ?? []}
        materialKeys={s.materialKeysQuery.data ?? []}
        selectedTypeKey={s.selectedTypeKey}
        onSelectType={s.setSelectedTypeKey}
      />
      <RegulatoryRegistryPanel
        governingBodies={s.governingBodiesQuery.data ?? []}
        jurisdictions={s.jurisdictionsQuery.data ?? []}
        programs={s.programsQuery.data ?? []}
        rulePacks={s.rulePacksQuery.data ?? []}
        canManage={s.canManage}
        onAdvanceRulePack={(rulePackId, status) => s.advanceRulePackMutation.mutate({ rulePackId, status })}
        isAdvancingRulePack={s.advanceRulePackMutation.isPending}
      />
      <RuleVersionManagementPanel
        accessToken={s.accessToken}
        canRead
        canManage={s.canManage}
      />
      <RuleTestCasesPanel
        accessToken={s.accessToken}
        rulePacks={s.rulePacksQuery.data ?? []}
        selectedRulePackId={s.selectedRulePackId}
        onSelectRulePack={s.setSelectedRulePackId}
        canManage={s.canManage}
      />
      <SdsHazComReferencesPanel
        accessToken={s.accessToken}
        canRead
        canManage={s.canManage}
      />
    </>
  )
}
