import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { canExportDispatchReports, canReadDispatchReports } from '../../auth/sessionStorage'
import { DispatchExceptionQueuePanel } from '../../components/DispatchExceptionQueuePanel'
import { DispatchReportsPanel } from '../../components/DispatchReportsPanel'

type Props = { state: RoutArrWorkspaceState }

export function ExceptionsSection({ state }: Props) {
  const { session, roleKey, isPlatformAdmin } = state
  const canRead = canReadDispatchReports(roleKey, isPlatformAdmin)
  const canExport = canExportDispatchReports(roleKey, isPlatformAdmin)

  return (
    <>
      <div className="mt-8">
        <DispatchExceptionQueuePanel
          accessToken={session.accessToken}
          userId={session.userId}
          canTriage={canRead || canExport}
        />
      </div>

      <div className="mt-8">
        <DispatchReportsPanel
          accessToken={session.accessToken}
          canRead={canRead}
          canExport={canExport}
        />
      </div>
    </>
  )
}
