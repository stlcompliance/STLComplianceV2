import { FindingsWorkflowGatesPanel } from '../../components/FindingsWorkflowGatesPanel'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function FindingsSection({ state }: Props) {
  const s = state
  return (
    <FindingsWorkflowGatesPanel
      rulePacks={s.rulePacksQuery.data ?? []}
      factDefinitions={s.factDefinitionsQuery.data ?? []}
      rulePackContent={s.rulePackContentQuery.data?.content ?? null}
      findings={s.findingsQuery.data ?? []}
      workflowGates={s.workflowGatesQuery.data ?? []}
      canManage={s.canManage}
      onSeedGate={() => s.seedWorkflowGateMutation.mutate()}
      isSeedingGate={s.seedWorkflowGateMutation.isPending}
      onCheckGate={(gateKey, facts, emitFindings) =>
        s.checkWorkflowGateMutation.mutate({ gateKey, facts, emitFindings })
      }
      isCheckingGate={s.checkWorkflowGateMutation.isPending}
      lastGateCheck={s.lastGateCheck}
      onCheckGateBatch={(gateKeys, facts, emitFindings) =>
        s.checkWorkflowGateBatchMutation.mutate({ gateKeys, facts, emitFindings })
      }
      isCheckingGateBatch={s.checkWorkflowGateBatchMutation.isPending}
      lastGateBatch={s.lastGateBatch}
    />
  )
}
