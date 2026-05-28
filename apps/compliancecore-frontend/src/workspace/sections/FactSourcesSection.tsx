import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function FactSourcesSection({ state }: Props) {
  const {
    data,
    factDefinitions,
    factDefinitionsQuery,
    factSources,
    factSourcesQuery,
    isPending,
    isSeeding,
    mutate,
    onSeedSources,
    seedSourcesMutation,
  } = state
  return (
    <>
      <FactSourcesPanel
      
                factDefinitions={factDefinitionsQuery.data ?? []}
      
                factSources={factSourcesQuery.data ?? []}
      
                canManage={canManage}
      
                onSeedSources={() => seedSourcesMutation.mutate()}
      
                isSeeding={seedSourcesMutation.isPending}
      
              />
    </>
  )
}
