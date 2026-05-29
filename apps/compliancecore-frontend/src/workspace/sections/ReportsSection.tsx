import { ComplianceReportsPanel } from '../../components/ComplianceReportsPanel'
import { DataExportsPanel } from '../../components/DataExportsPanel'
import { OperatorReportsPanel } from '../../components/OperatorReportsPanel'
import { canExportReports, canReadReports } from '../../auth/sessionStorage'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function ReportsSection({ state }: Props) {
  const roleKey = state.me.tenantRoleKey
  const isPlatformAdmin = state.me.isPlatformAdmin

  const canRead = canReadReports(roleKey, isPlatformAdmin)
  const canExport = canExportReports(roleKey, isPlatformAdmin)
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
