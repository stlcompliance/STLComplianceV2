import { PageHeader } from '@stl/shared-ui'
import { CsvImportExportPanel } from '../../components/CsvImportExportPanel'
import { RulePackImportWorkflowPanel } from '../../components/RulePackImportWorkflowPanel'
import { ImportWizardPanel } from '../../components/ImportWizardPanel'
import { useComplianceCoreWorkspaceState } from '../../workspace/useComplianceCoreWorkspaceState'

export function ImportsPage() {
  const state = useComplianceCoreWorkspaceState()
  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-[var(--color-text-muted)]">{state.loadingMessage}</p>

  return (
    <div className="space-y-6">
      <PageHeader
        title="Imports"
        subtitle="CSV bundle import/export, rule-pack import diff, and staged import review."
      />
      <CsvImportExportPanel accessToken={state.accessToken} canManage={state.canManage} />
      <RulePackImportWorkflowPanel accessToken={state.accessToken} canManage={state.canManage} />
      <ImportWizardPanel accessToken={state.accessToken} canManage={state.canManage} />
    </div>
  )
}
