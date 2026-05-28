import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { canManageNotificationSettings } from '../../auth/sessionStorage'
import { NotificationSettingsPanel } from '../../components/NotificationSettingsPanel'

type Props = { state: RoutArrWorkspaceState }

export function SettingsSection({ state }: Props) {
  const { session, roleKey, isPlatformAdmin } = state
  const canManage = canManageNotificationSettings(roleKey, isPlatformAdmin)

  if (!canManage) {
    return null
  }

  return (
    <div className="mt-8">
      <NotificationSettingsPanel accessToken={session.accessToken} canManage={canManage} />
    </div>
  )
}
