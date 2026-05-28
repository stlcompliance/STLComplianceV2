import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import {
  canManageDriverAvailability,
  canManageEquipmentAvailability,
} from '../../auth/sessionStorage'
import { DriverAvailabilityPanel } from '../../components/DriverAvailabilityPanel'
import { EquipmentAvailabilityPanel } from '../../components/EquipmentAvailabilityPanel'

type Props = { state: RoutArrWorkspaceState }

export function AvailabilitySection({ state }: Props) {
  const { session, boardScope, setBoardScope, roleKey, isPlatformAdmin } = state

  return (
    <>
      <div className="mt-8">
        <DriverAvailabilityPanel
          accessToken={session.accessToken}
          scope={boardScope}
          onScopeChange={setBoardScope}
          canManage={canManageDriverAvailability(roleKey, isPlatformAdmin)}
          sessionPersonId={session.personId}
        />
      </div>

      <div className="mt-8">
        <EquipmentAvailabilityPanel
          accessToken={session.accessToken}
          scope={boardScope}
          onScopeChange={setBoardScope}
          canManage={canManageEquipmentAvailability(roleKey, isPlatformAdmin)}
        />
      </div>
    </>
  )
}
