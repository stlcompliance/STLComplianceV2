import { NotificationSettingsPanel } from '../../components/NotificationSettingsPanel'
import { RecertificationSettingsPanel } from '../../components/RecertificationSettingsPanel'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }

export function SettingsSection({ state }: Props) {
  const s = state
  if (!s.canNotifications) {
    return <p className="text-sm text-slate-400">You do not have permission to manage notification settings.</p>
  }

  return (
    <div className="space-y-6">
      <NotificationSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
      <RecertificationSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
    </div>
  )
}
