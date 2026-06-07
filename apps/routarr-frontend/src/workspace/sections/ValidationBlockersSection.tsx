import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { canAssignDrivers } from '../../auth/sessionStorage'
import { DispatchAssignmentPanel } from '../../components/DispatchAssignmentPanel'
import { DispatchCloseoutPanel } from '../../components/DispatchCloseoutPanel'
import { TripCaptureReadinessPanel } from '../../components/TripCaptureReadinessPanel'
import { TripProofDvirReadPanel } from '../../components/TripProofDvirReadPanel'
import { TripProfile } from './RoutingDetailProfiles'

type Props = { state: RoutArrWorkspaceState }

export function ValidationBlockersSection({ state }: Props) {
  const { session, boardScope, roleKey, isPlatformAdmin } = state
  const canAssign = canAssignDrivers(roleKey, isPlatformAdmin)

  return (
    <>
      <div className="mt-8">
        <TripCaptureReadinessPanel accessToken={session.accessToken} />
      </div>

      <div className="mt-8">
        <TripProfile state={state} />
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
          <TripProofDvirReadPanel accessToken={session.accessToken} />
        </div>
      ) : null}
    </>
  )
}
