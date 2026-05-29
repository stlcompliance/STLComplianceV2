import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { canManageNotificationSettings } from '../../auth/sessionStorage'
import { NotificationSettingsPanel } from '../../components/NotificationSettingsPanel'
import { TripCompletionRollupSettingsPanel } from '../../components/TripCompletionRollupSettingsPanel'
import { AttachmentRetentionSettingsPanel } from '../../components/AttachmentRetentionSettingsPanel'
import { TripExecutionSettingsPanel } from '../../components/TripExecutionSettingsPanel'

type Props = { state: RoutArrWorkspaceState }

export function SettingsSection({ state }: Props) {
  const { session, roleKey, isPlatformAdmin } = state
  const canManage = canManageNotificationSettings(roleKey, isPlatformAdmin)

  if (!canManage) {
    return null
  }

  return (
    <div className="mt-8 space-y-6" data-testid="routarr-settings-admin-workspace">
      <NotificationSettingsPanel accessToken={session.accessToken} canManage={canManage} />
      <TripExecutionSettingsPanel accessToken={session.accessToken} canManage={canManage} />
      <TripCompletionRollupSettingsPanel accessToken={session.accessToken} canManage={canManage} />
      <AttachmentRetentionSettingsPanel accessToken={session.accessToken} canManage={canManage} />
    </div>
  )
}
