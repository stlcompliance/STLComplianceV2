import { NotificationSettingsPanel } from '../../components/NotificationSettingsPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function SettingsSection({ state: s }: Props) {
  if (!s.canManageNotifications) {
    return <p className="text-sm text-slate-400">You do not have permission to manage notification settings.</p>
  }

  return <NotificationSettingsPanel accessToken={s.accessToken} canManage={s.canManageNotifications} />
}
