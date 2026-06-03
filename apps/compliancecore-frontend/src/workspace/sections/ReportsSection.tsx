import { ComplianceReportsPanel } from '../../components/ComplianceReportsPanel'
import { DataExportsPanel } from '../../components/DataExportsPanel'
import { OperatorReportsPanel } from '../../components/OperatorReportsPanel'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function ReportsSection({ state }: Props) {
  const canRead = state.me.canReadReports
  const canExport = state.me.canExportReports
  const showReportsWorkspace = canRead || canExport

  if (!showReportsWorkspace) {
    return (
      <p className="text-sm text-slate-400">You do not have permission to view compliance reports.</p>
    )
  }

  return (
    <div className="grid gap-6" data-testid="compliancecore-reports-workspace">
      <ComplianceReportsPanel
        accessToken={state.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
      <OperatorReportsPanel
        accessToken={state.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
      <DataExportsPanel accessToken={state.accessToken} canExport={canExport} />
    </div>
  )
}
