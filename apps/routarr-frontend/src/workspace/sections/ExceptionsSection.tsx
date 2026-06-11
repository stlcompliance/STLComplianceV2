import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { canAssignDrivers } from '../../auth/sessionStorage'
import { DispatchExceptionQueuePanel } from '../../components/DispatchExceptionQueuePanel'

type Props = { state: RoutArrWorkspaceState }

export function ExceptionsSection({ state }: Props) {
  const { session, roleKey, isPlatformAdmin } = state
  const canTriage = canAssignDrivers(roleKey, isPlatformAdmin)

  return (
    <div className="mt-8">
      <DispatchExceptionQueuePanel
        accessToken={session.accessToken}
        userId={session.userId}
        canTriage={canTriage}
      />
    </div>
  )
}
