import { ComplianceReportsPanel } from '../../components/ComplianceReportsPanel'
import { DataExportsPanel } from '../../components/DataExportsPanel'
import { ExecutiveReportsPanel } from '../../components/ExecutiveReportsPanel'
import { MaintenanceReportsPanel } from '../../components/MaintenanceReportsPanel'
import {
  canExportAuditPackage,
  canExportComplianceReports,
  canExportExecutiveReports,
  canExportMaintenanceReports,
  canReadComplianceReports,
  canReadExecutiveReports,
  canReadMaintenanceReports,
} from '../../auth/sessionStorage'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }

export function ReportsSection({ state }: Props) {
  const roleKey = state.me.tenantRoleKey
  const isPlatformAdmin = state.me.isPlatformAdmin

  const canReadMaintenance = canReadMaintenanceReports(roleKey, isPlatformAdmin)
  const canExportMaintenance = canExportMaintenanceReports(roleKey, isPlatformAdmin)
  const canReadExecutive = canReadExecutiveReports(roleKey, isPlatformAdmin)
  const canExportExecutive = canExportExecutiveReports(roleKey, isPlatformAdmin)
  const canReadCompliance = canReadComplianceReports(roleKey, isPlatformAdmin)
  const canExportCompliance = canExportComplianceReports(roleKey, isPlatformAdmin)
  const canExportData = canExportAuditPackage(roleKey, isPlatformAdmin)

  const showReportsWorkspace =
    canReadMaintenance ||
    canExportMaintenance ||
    canReadExecutive ||
    canExportExecutive ||
    canReadCompliance ||
    canExportCompliance ||
    canExportData

  if (!showReportsWorkspace) {
    return null
  }

  return (
    <div className="grid gap-6" data-testid="maintainarr-reports-workspace">
      <ComplianceReportsPanel
        accessToken={state.accessToken}
        canRead={canReadCompliance}
        canExport={canExportCompliance}
      />
      <ExecutiveReportsPanel
        accessToken={state.accessToken}
        canRead={canReadExecutive}
        canExport={canExportExecutive}
      />
      <MaintenanceReportsPanel
        accessToken={state.accessToken}
        canRead={canReadMaintenance}
        canExport={canExportMaintenance}
      />
      <DataExportsPanel accessToken={state.accessToken} canExport={canExportData} />
    </div>
  )
}
