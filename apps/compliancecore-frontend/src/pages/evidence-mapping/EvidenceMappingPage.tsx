import { PageHeader } from '@stl/shared-ui'
import { SourceIngestionPanel } from '../../components/SourceIngestionPanel'
import { ImportWizardPanel } from '../../components/ImportWizardPanel'
import { useComplianceCoreWorkspaceState } from '../../workspace/useComplianceCoreWorkspaceState'

export function EvidenceMappingPage() {
  const state = useComplianceCoreWorkspaceState()
  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <div className="space-y-6">
      <PageHeader
        title="Evidence mapping wizard"
        subtitle="Map imported evidence to controlled requirements and reviewed evidence paths."
      />
      <ImportWizardPanel accessToken={state.accessToken} canManage={state.canManage} />
      <SourceIngestionPanel accessToken={state.accessToken} canManage={state.canManage} />
    </div>
  )
}
