import { NotificationSettingsPanel } from '../../components/NotificationSettingsPanel'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }

export function SettingsSection({ state }: Props) {
  const s = state
  if (!s.canNotifications) {
    return <p className="text-sm text-slate-400">You do not have permission to manage notification settings.</p>
  }

  return <NotificationSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
}
