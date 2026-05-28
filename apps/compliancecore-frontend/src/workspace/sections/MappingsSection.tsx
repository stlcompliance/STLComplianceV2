import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function MappingsSection({ state }: Props) {
  const {
    citations,
    citationsQuery,
    data,
    factDefinitions,
    factDefinitionsQuery,
    factRequirements,
    factRequirementsQuery,
    isPending,
    isSeeding,
    mappings,
    mutate,
    onSeedCatalog,
    onSeedMappings,
    regulatoryMappingsQuery,
    seedCatalogMutation,
    seedMappingsMutation,
  } = state
  return (
    <>
      <>
              <RegulatoryMappingsPanel
      
                mappings={regulatoryMappingsQuery.data ?? []}
      
                canManage={canManage}
      
                onSeedMappings={() => seedMappingsMutation.mutate()}
      
                isSeeding={seedMappingsMutation.isPending}
      
              />
      
              <CitationFactCatalogPanel
      
                citations={citationsQuery.data ?? []}
      
                factDefinitions={factDefinitionsQuery.data ?? []}
      
                factRequirements={factRequirementsQuery.data ?? []}
      
                canManage={canManage}
      
                onSeedCatalog={() => seedCatalogMutation.mutate()}
      
                isSeeding={seedCatalogMutation.isPending}
      
              />
              </>
    </>
  )
}
