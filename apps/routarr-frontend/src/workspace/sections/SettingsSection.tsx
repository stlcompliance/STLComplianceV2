import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { canManageNotificationSettings } from '../../auth/sessionStorage'
import { RoutArrTenantSettingsPanel } from '../../components/RoutArrTenantSettingsPanel'

type Props = { state: RoutArrWorkspaceState }

export function SettingsSection({ state }: Props) {
  const { session, roleKey, isPlatformAdmin } = state
  const canManage = canManageNotificationSettings(roleKey, isPlatformAdmin)

  if (!canManage) {
    return null
  }

  return (
    <div className="mt-8 space-y-6" data-testid="routarr-settings-admin-workspace">
      <RoutArrTenantSettingsPanel accessToken={session.accessToken} canManage={canManage} />
    </div>
  )
}
