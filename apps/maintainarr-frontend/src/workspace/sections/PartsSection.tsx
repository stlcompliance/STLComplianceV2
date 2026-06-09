import { MaintenancePartsPanel } from '../../components/MaintenancePartsPanel'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

export function PartsSection({ state: _state }: { state: MaintainArrWorkspaceState }) {
  return <MaintenancePartsPanel />
}
