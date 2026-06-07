import { PageHeader } from '@stl/shared-ui'
import { useComplianceCoreWorkspaceState } from '../../workspace/useComplianceCoreWorkspaceState'
import { SituationEvaluatorPanel } from '../../components/SituationEvaluatorPanel'

export function TheoreticalSituationPage() {
  const state = useComplianceCoreWorkspaceState()
  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <div className="space-y-6">
      <PageHeader
        title="Theoretical situation evaluation"
        subtitle="Evaluate a controlled scenario without altering operational records."
      />
      <SituationEvaluatorPanel
        accessToken={state.accessToken}
        canEvaluate={state.canManage || state.canEvaluateRisk}
        factRequirements={state.factRequirementsQuery.data ?? []}
      />
    </div>
  )
}
