import { DataExportsPanel } from '../../components/DataExportsPanel'
import { IncidentReportsPanel } from '../../components/IncidentReportsPanel'
import { PersonnelReportsPanel } from '../../components/PersonnelReportsPanel'
import { ReadinessReportsPanel } from '../../components/ReadinessReportsPanel'
import { canExportReports, canReadReports } from '../../auth/sessionStorage'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }

export function ReportsSection({ state }: Props) {
  const roleKey = state.me.tenantRoleKey
  const isPlatformAdmin = state.me.isPlatformAdmin

  const canRead = canReadReports(roleKey, isPlatformAdmin)
  const canExport = canExportReports(roleKey, isPlatformAdmin)
  const showReportsWorkspace = canRead || canExport

  if (!showReportsWorkspace) {
    return (
      <p className="text-sm text-slate-400">You do not have permission to view workforce reports.</p>
    )
  }

  return (
    <div className="grid gap-6" data-testid="staffarr-reports-workspace">
      <PersonnelReportsPanel
        accessToken={state.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
      <ReadinessReportsPanel
        accessToken={state.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
      <IncidentReportsPanel
        accessToken={state.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
      <DataExportsPanel accessToken={state.accessToken} canExport={canExport} />
    </div>
  )
}
