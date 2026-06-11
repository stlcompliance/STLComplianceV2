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
      />
      <CitationFactCatalogPanel
        citations={s.citationsQuery.data ?? []}
        factDefinitions={s.factDefinitionsQuery.data ?? []}
        factRequirements={s.factRequirementsQuery.data ?? []}
      />
    </>
  )
}
