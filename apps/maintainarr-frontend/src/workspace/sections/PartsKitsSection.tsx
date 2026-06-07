import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'
import { MaintenancePartsKitsPanel } from '../../components/MaintenancePartsKitsPanel'

type Props = { state: MaintainArrWorkspaceState }

export function PartsKitsSection({ state }: Props) {
  return <MaintenancePartsKitsPanel accessToken={state.accessToken} canManage={state.canManage} />
}
