import { FactSourceSyncPanel } from '../../components/FactSourceSyncPanel'
import { FactSourcesPanel } from '../../components/FactSourcesPanel'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function FactSourcesSection({ state }: Props) {
  const s = state
  return (
    <div className="space-y-8">
      <FactSourceSyncPanel accessToken={s.accessToken} canManage={s.canManage} />
      <FactSourcesPanel
        factDefinitions={s.factDefinitionsQuery.data ?? []}
        factSources={s.factSourcesQuery.data ?? []}
        canManage={s.canManage}
        onSeedSources={() => s.seedSourcesMutation.mutate()}
        isSeeding={s.seedSourcesMutation.isPending}
      />
    </div>
  )
}
