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
  const canReadMaintenance = canReadMaintenanceReports(
    state.me.tenantRoleKey,
    state.me.isPlatformAdmin,
  )
  const canExportMaintenance = canExportMaintenanceReports(
    state.me.tenantRoleKey,
    state.me.isPlatformAdmin,
  )
  const canReadExecutive = canReadExecutiveReports(state.me.tenantRoleKey, state.me.isPlatformAdmin)
  const canExportExecutive = canExportExecutiveReports(
    state.me.tenantRoleKey,
    state.me.isPlatformAdmin,
  )
  const canReadCompliance = canReadComplianceReports(
    state.me.tenantRoleKey,
    state.me.isPlatformAdmin,
  )
  const canExportCompliance = canExportComplianceReports(
    state.me.tenantRoleKey,
    state.me.isPlatformAdmin,
  )
  const canExportData = canExportAuditPackage(state.me.tenantRoleKey, state.me.isPlatformAdmin)

  return (
    <>
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
    </>
  )
}
