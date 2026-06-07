import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { canAssignDrivers } from '../../auth/sessionStorage'
import { ActiveTripsPanel } from '../../components/ActiveTripsPanel'
import { DispatchBoardPanel } from '../../components/DispatchBoardPanel'
import { DispatchCommandCenterPanel } from '../../components/DispatchCommandCenterPanel'
import { DispatchExceptionQueuePanel } from '../../components/DispatchExceptionQueuePanel'
import { RouteCalendarPanel } from '../../components/RouteCalendarPanel'
import { UnassignedWorkQueuePanel } from '../../components/UnassignedWorkQueuePanel'

type Props = { state: RoutArrWorkspaceState }

export function DashboardSection({ state }: Props) {
  const { session, boardScope, setBoardScope, roleKey, isPlatformAdmin } = state
  const canAssign = canAssignDrivers(roleKey, isPlatformAdmin)

  return (
    <>
      <DispatchCommandCenterPanel
        accessToken={session.accessToken}
        scope={boardScope}
        onScopeChange={setBoardScope}
        canAssign={canAssign}
      />

      <div className="mt-8">
        <ActiveTripsPanel accessToken={session.accessToken} scope={boardScope} />
      </div>

      <div className="mt-8">
        <UnassignedWorkQueuePanel
          accessToken={session.accessToken}
          scope={boardScope}
          canAssign={canAssign}
        />
      </div>

      <div className="mt-8">
        <DispatchExceptionQueuePanel
          accessToken={session.accessToken}
          userId={session.userId}
          canTriage={canAssign}
        />
      </div>

      <div className="mt-8">
        <DispatchBoardPanel
          accessToken={session.accessToken}
          scope={boardScope}
          onScopeChange={setBoardScope}
        />
      </div>

      <div className="mt-8">
        <RouteCalendarPanel
          accessToken={session.accessToken}
          scope={boardScope}
          onScopeChange={setBoardScope}
        />
      </div>
    </>
  )
}
