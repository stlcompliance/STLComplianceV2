import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { canAssignDrivers } from '../../auth/sessionStorage'
import { BulkDispatchPanel } from '../../components/BulkDispatchPanel'
import { DispatchAssignmentPanel } from '../../components/DispatchAssignmentPanel'
import { DispatchBoardPanel } from '../../components/DispatchBoardPanel'
import { DispatchCloseoutPanel } from '../../components/DispatchCloseoutPanel'

type Props = { state: RoutArrWorkspaceState }

export function DispatchSection({ state }: Props) {
  const { session, boardScope, setBoardScope, roleKey, isPlatformAdmin } = state
  const canAssign = canAssignDrivers(roleKey, isPlatformAdmin)

  return (
    <>
      <DispatchBoardPanel
        accessToken={session.accessToken}
        scope={boardScope}
        onScopeChange={setBoardScope}
      />

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
