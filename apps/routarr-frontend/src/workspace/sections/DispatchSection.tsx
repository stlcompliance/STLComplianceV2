import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { canAssignDrivers } from '../../auth/sessionStorage'
import { BulkDispatchPanel } from '../../components/BulkDispatchPanel'
import { DispatchAssignmentPanel } from '../../components/DispatchAssignmentPanel'
import { DispatchBoardPanel } from '../../components/DispatchBoardPanel'
import { DispatchCommandCenterPanel } from '../../components/DispatchCommandCenterPanel'
import { DispatchCloseoutPanel } from '../../components/DispatchCloseoutPanel'
import { DispatchExceptionQueuePanel } from '../../components/DispatchExceptionQueuePanel'
import { ActiveTripsPanel } from '../../components/ActiveTripsPanel'
import { TripProofDvirReadPanel } from '../../components/TripProofDvirReadPanel'
import { UnassignedWorkQueuePanel } from '../../components/UnassignedWorkQueuePanel'

type Props = { state: RoutArrWorkspaceState }

export function DispatchSection({ state }: Props) {
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
        <ActiveTripsPanel accessToken={session.accessToken} scope={boardScope} />
      </div>

      {canAssign ? (
        <div className="mt-8">
          <TripProofDvirReadPanel accessToken={session.accessToken} />
        </div>
      ) : null}

      <div className="mt-8">
        <DispatchBoardPanel
        accessToken={session.accessToken}
        scope={boardScope}
        onScopeChange={setBoardScope}
        />
      </div>

      {canAssign ? (
        <div className="mt-8">
          <DispatchCloseoutPanel
            accessToken={session.accessToken}
            scope={boardScope}
            canAssign={canAssign}
          />
        </div>
      ) : null}

      {canAssign ? (
        <div className="mt-8">
          <BulkDispatchPanel accessToken={session.accessToken} canAssign={canAssign} />
        </div>
      ) : null}

      {canAssign ? (
        <div className="mt-8">
          <DispatchAssignmentPanel
            accessToken={session.accessToken}
            scope={boardScope}
            canAssign={canAssign}
          />
        </div>
      ) : null}
    </>
  )
}
