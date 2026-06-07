import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { canAssignDrivers } from '../../auth/sessionStorage'
import { BulkDispatchPanel } from '../../components/BulkDispatchPanel'
import { DispatchAssignmentPanel } from '../../components/DispatchAssignmentPanel'
import { DispatchCloseoutPanel } from '../../components/DispatchCloseoutPanel'
import { DispatchCommandCenterPanel } from '../../components/DispatchCommandCenterPanel'
import { UnassignedWorkQueuePanel } from '../../components/UnassignedWorkQueuePanel'

type Props = { state: RoutArrWorkspaceState }

export function DispatchPlansSection({ state }: Props) {
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

      {canAssign ? (
        <div className="mt-8">
          <DispatchAssignmentPanel
            accessToken={session.accessToken}
            scope={boardScope}
            canAssign={canAssign}
          />
        </div>
      ) : null}

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
    </>
  )
}
