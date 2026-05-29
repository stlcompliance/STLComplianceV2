import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'
import { AssetDowntimePanel } from '../../components/AssetDowntimePanel'
import {
  canManageAssets,
  canReadMaintenanceReports,
} from '../../auth/sessionStorage'

type Props = { state: MaintainArrWorkspaceState }

export function DowntimeSection({ state }: Props) {
  const roleKey = state.me.tenantRoleKey
  const isPlatformAdmin = state.me.isPlatformAdmin
  const canRead = canReadMaintenanceReports(roleKey, isPlatformAdmin)
  const canManage = canManageAssets(roleKey, isPlatformAdmin)

  if (!canRead) {
    return null
  }

  return (
    <AssetDowntimePanel
      accessToken={state.accessToken}
      canRead={canRead}
      canManage={canManage}
      assets={state.assetsQuery.data ?? []}
    />
  )
}
