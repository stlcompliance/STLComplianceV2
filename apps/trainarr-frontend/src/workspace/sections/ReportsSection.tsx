import { AssignmentReportsPanel } from '../../components/AssignmentReportsPanel'
import { ComplianceReportsPanel } from '../../components/ComplianceReportsPanel'
import { DataExportsPanel } from '../../components/DataExportsPanel'
import { QualificationReportsPanel } from '../../components/QualificationReportsPanel'
import {
  canExportAuditPackage,
  canReadAuditPackage,
} from '../../auth/sessionStorage'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }

export function ReportsSection({ state }: Props) {
  const roleKey = state.me.tenantRoleKey
  const isPlatformAdmin = state.me.isPlatformAdmin

  const canRead = canReadAuditPackage(roleKey, isPlatformAdmin)
  const canExport = canExportAuditPackage(roleKey, isPlatformAdmin)
  const showReportsWorkspace = canRead || canExport

  if (!showReportsWorkspace) {
    return (
      <p className="text-sm text-slate-400">You do not have permission to view training reports.</p>
    )
  }

  return (
    <div className="grid gap-6" data-testid="trainarr-reports-workspace">
      <AssignmentReportsPanel
        accessToken={state.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
      <QualificationReportsPanel
        accessToken={state.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
      <ComplianceReportsPanel
        accessToken={state.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
      <DataExportsPanel accessToken={state.accessToken} canExport={canExport} />
    </div>
  )
}
