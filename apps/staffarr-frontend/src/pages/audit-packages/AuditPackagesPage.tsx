import { PageHeader } from '@stl/shared-ui'
import { useStaffArrWorkspaceState } from '../../workspace/useStaffArrWorkspaceState'
import { AuditPackageExportPanel } from '../../components/AuditPackageExportPanel'

export function AuditPackagesPage() {
  const state = useStaffArrWorkspaceState()

  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <div className="mx-auto max-w-6xl space-y-6 px-4 py-6">
      <PageHeader
        title="Audit packages"
        subtitle="Evidence bundles, timeline snapshots, and export delivery"
      />
      <AuditPackageExportPanel
        accessToken={state.accessToken}
        canRead={state.canReadReports}
        canExport={state.canExportAudit}
      />
    </div>
  )
}
