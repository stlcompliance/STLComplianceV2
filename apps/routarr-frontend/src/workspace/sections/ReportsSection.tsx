import { AuditPackageExportPanel } from '../../components/AuditPackageExportPanel'
import { DataExportsPanel } from '../../components/DataExportsPanel'
import { DispatchReportsPanel } from '../../components/DispatchReportsPanel'
import { ProofDvirReportsPanel } from '../../components/ProofDvirReportsPanel'
import { RouteReportsPanel } from '../../components/RouteReportsPanel'
import {
  canExportDispatchReports,
  canReadDispatchReports,
} from '../../auth/sessionStorage'
import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'

type Props = { state: RoutArrWorkspaceState }

export function ReportsSection({ state }: Props) {
  const roleKey = state.me?.tenantRoleKey ?? ''
  const isPlatformAdmin = state.me?.isPlatformAdmin ?? false
  const canRead = canReadDispatchReports(roleKey, isPlatformAdmin)
  const canExport = canExportDispatchReports(roleKey, isPlatformAdmin)

  const showReportsWorkspace = canRead || canExport

  return (
    <>
      {showReportsWorkspace ? (
        <div className="space-y-0" data-testid="routarr-reports-workspace">
          <DispatchReportsPanel
            accessToken={state.session.accessToken}
            canRead={canRead}
            canExport={canExport}
          />
          <RouteReportsPanel
            accessToken={state.session.accessToken}
            canRead={canRead}
            canExport={canExport}
          />
          <ProofDvirReportsPanel
            accessToken={state.session.accessToken}
            canRead={canRead}
            canExport={canExport}
          />
          <DataExportsPanel accessToken={state.session.accessToken} canExport={canExport} />
        </div>
      ) : null}
      <AuditPackageExportPanel
        accessToken={state.session.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
    </>
  )
}
