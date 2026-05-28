import { CitationFactCatalogPanel } from '../../components/CitationFactCatalogPanel'
import { RegulatoryMappingsPanel } from '../../components/RegulatoryMappingsPanel'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function MappingsSection({ state }: Props) {
  const s = state
  return (
    <>
      <RegulatoryMappingsPanel
        mappings={s.regulatoryMappingsQuery.data ?? []}
        canManage={s.canManage}
        onSeedMappings={() => s.seedMappingsMutation.mutate()}
        isSeeding={s.seedMappingsMutation.isPending}
      />
      <CitationFactCatalogPanel
        citations={s.citationsQuery.data ?? []}
        factDefinitions={s.factDefinitionsQuery.data ?? []}
        factRequirements={s.factRequirementsQuery.data ?? []}
        canManage={s.canManage}
        onSeedCatalog={() => s.seedCatalogMutation.mutate()}
        isSeeding={s.seedCatalogMutation.isPending}
      />
    </>
  )
}
