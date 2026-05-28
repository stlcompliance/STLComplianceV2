import { FactSourcesPanel } from '../../components/FactSourcesPanel'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function FactSourcesSection({ state }: Props) {
  const s = state
  return (
    <FactSourcesPanel
      factDefinitions={s.factDefinitionsQuery.data ?? []}
      factSources={s.factSourcesQuery.data ?? []}
      canManage={s.canManage}
      onSeedSources={() => s.seedSourcesMutation.mutate()}
      isSeeding={s.seedSourcesMutation.isPending}
    />
  )
}
