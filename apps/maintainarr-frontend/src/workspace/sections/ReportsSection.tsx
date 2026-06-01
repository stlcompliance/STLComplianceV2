import { useLocation } from 'react-router-dom'
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
type ReportsViewMode = 'compliance' | 'executive' | 'maintenance' | 'exports'

export function ReportsSection({ state }: Props) {
  const location = useLocation()
  const mode: ReportsViewMode = location.pathname.startsWith('/reports/exports')
    ? 'exports'
    : location.pathname.startsWith('/reports/maintenance')
      ? 'maintenance'
      : location.pathname.startsWith('/reports/executive')
        ? 'executive'
        : 'compliance'
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
      {mode === 'compliance' ? (
        <ComplianceReportsPanel
          accessToken={state.accessToken}
          canRead={canReadCompliance}
          canExport={canExportCompliance}
        />
      ) : null}
      {mode === 'executive' ? (
        <ExecutiveReportsPanel
          accessToken={state.accessToken}
          canRead={canReadExecutive}
          canExport={canExportExecutive}
        />
      ) : null}
      {mode === 'maintenance' ? (
        <MaintenanceReportsPanel
          accessToken={state.accessToken}
          canRead={canReadMaintenance}
          canExport={canExportMaintenance}
        />
      ) : null}
      {mode === 'exports' ? <DataExportsPanel accessToken={state.accessToken} canExport={canExportData} /> : null}
    </div>
  )
}
