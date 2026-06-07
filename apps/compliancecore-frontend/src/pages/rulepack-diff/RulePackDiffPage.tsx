import { PageHeader } from '@stl/shared-ui'
import { RuleChangeImpactReportPanel } from '../../components/RuleChangeImpactReportPanel'
import { RulePackImportWorkflowPanel } from '../../components/RulePackImportWorkflowPanel'
import { useComplianceCoreWorkspaceState } from '../../workspace/useComplianceCoreWorkspaceState'

export function RulePackDiffPage() {
  const state = useComplianceCoreWorkspaceState()
  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <div className="space-y-6">
      <PageHeader
        title="Rule pack diff"
        subtitle="Inspect import diff output, test results, and change impact for rule-pack lifecycle work."
      />
      <RulePackImportWorkflowPanel accessToken={state.accessToken} canManage={state.canManage} />
      <RuleChangeImpactReportPanel
        accessToken={state.accessToken}
        canRead={state.me.canReadReports}
        canExport={state.me.canExportReports}
      />
    </div>
  )
}
