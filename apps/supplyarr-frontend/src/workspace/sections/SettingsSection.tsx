import { LeadTimeSnapshotSettingsPanel } from '../../components/LeadTimeSnapshotSettingsPanel'
import { NotificationSettingsPanel } from '../../components/NotificationSettingsPanel'
import { PriceSnapshotSettingsPanel } from '../../components/PriceSnapshotSettingsPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function SettingsSection({ state: s }: Props) {
  if (!s.canManageNotifications) {
    return <p className="text-sm text-slate-400">You do not have permission to manage notification settings.</p>
  }

  return (
    <div className="grid gap-6">
      <NotificationSettingsPanel accessToken={s.accessToken} canManage={s.canManageNotifications} />
      <PriceSnapshotSettingsPanel accessToken={s.accessToken} canManage={s.canManageNotifications} />
      <LeadTimeSnapshotSettingsPanel accessToken={s.accessToken} canManage={s.canManageNotifications} />
    </div>
  )
}
