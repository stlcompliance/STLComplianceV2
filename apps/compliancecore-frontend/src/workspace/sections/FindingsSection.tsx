import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function FindingsSection({ state }: Props) {
  const {
    checkWorkflowGateBatchMutation,
    checkWorkflowGateMutation,
    content,
    data,
    emitFindings,
    factDefinitions,
    factDefinitionsQuery,
    facts,
    findings,
    findingsQuery,
    gateKey,
    gateKeys,
    isCheckingGate,
    isCheckingGateBatch,
    isPending,
    isSeedingGate,
    lastGateBatch,
    lastGateCheck,
    mutate,
    onCheckGate,
    onCheckGateBatch,
    onSeedGate,
    rulePackContent,
    rulePackContentQuery,
    rulePacks,
    rulePacksQuery,
    seedWorkflowGateMutation,
    workflowGates,
    workflowGatesQuery,
  } = state
  return (
    <>
      <FindingsWorkflowGatesPanel
      
                rulePacks={rulePacksQuery.data ?? []}
      
                factDefinitions={factDefinitionsQuery.data ?? []}
      
                rulePackContent={rulePackContentQuery.data?.content ?? null}
      
                findings={findingsQuery.data ?? []}
      
                workflowGates={workflowGatesQuery.data ?? []}
      
                canManage={canManage}
      
                onSeedGate={() => seedWorkflowGateMutation.mutate()}
      
                isSeedingGate={seedWorkflowGateMutation.isPending}
      
                onCheckGate={(gateKey, facts, emitFindings) =>
      
                  checkWorkflowGateMutation.mutate({ gateKey, facts, emitFindings })
      
                }
      
                isCheckingGate={checkWorkflowGateMutation.isPending}
      
                lastGateCheck={lastGateCheck}
      
                onCheckGateBatch={(gateKeys, facts, emitFindings) =>
                  checkWorkflowGateBatchMutation.mutate({ gateKeys, facts, emitFindings })
                }
      
                isCheckingGateBatch={checkWorkflowGateBatchMutation.isPending}
      
                lastGateBatch={lastGateBatch}
      
              />
    </>
  )
}
