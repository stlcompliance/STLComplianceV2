import { PageHeader } from '@stl/shared-ui'
import { RuleChangeImpactReportPanel } from '../../components/RuleChangeImpactReportPanel'
import { useComplianceCoreWorkspaceState } from '../../workspace/useComplianceCoreWorkspaceState'

export function ChangeImpactPage() {
  const state = useComplianceCoreWorkspaceState()
  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <div className="space-y-6">
      <PageHeader
        title="Change impact"
        subtitle="See which rule packs were affected by recent rule and content changes."
      />
      <RuleChangeImpactReportPanel
        accessToken={state.accessToken}
        canRead={state.me.canReadReports}
        canExport={state.me.canExportReports}
      />
    </div>
  )
}
