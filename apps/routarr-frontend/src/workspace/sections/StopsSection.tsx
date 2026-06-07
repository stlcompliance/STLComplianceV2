import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { canExportDispatchReports, canReadDispatchReports } from '../../auth/sessionStorage'
import { RouteReportsPanel } from '../../components/RouteReportsPanel'

type Props = { state: RoutArrWorkspaceState }

export function StopsSection({ state }: Props) {
  const { session, roleKey, isPlatformAdmin } = state
  const canRead = canReadDispatchReports(roleKey, isPlatformAdmin)
  const canExport = canExportDispatchReports(roleKey, isPlatformAdmin)

  return (
    <div className="mt-8">
      <RouteReportsPanel
        accessToken={session.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
    </div>
  )
}
