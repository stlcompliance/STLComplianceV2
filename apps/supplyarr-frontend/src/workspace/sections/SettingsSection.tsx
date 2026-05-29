import { LeadTimeSnapshotSettingsPanel } from '../../components/LeadTimeSnapshotSettingsPanel'
import { AvailabilitySnapshotSettingsPanel } from '../../components/AvailabilitySnapshotSettingsPanel'
import { NotificationSettingsPanel } from '../../components/NotificationSettingsPanel'
import { PriceSnapshotSettingsPanel } from '../../components/PriceSnapshotSettingsPanel'
import { ProcurementCoordinationSettingsPanel } from '../../components/ProcurementCoordinationSettingsPanel'
import { ApprovalReminderSettingsPanel } from '../../components/ApprovalReminderSettingsPanel'
import { ProcurementExceptionEscalationSettingsPanel } from '../../components/ProcurementExceptionEscalationSettingsPanel'
import { DemandProcessingSettingsPanel } from '../../components/DemandProcessingSettingsPanel'
import { IntegrationEventSettingsPanel } from '../../components/IntegrationEventSettingsPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function SettingsSection({ state: s }: Props) {
  if (!s.canManageNotifications) {
    return <p className="text-sm text-slate-400">You do not have permission to manage notification settings.</p>
  }

  return (
    <div className="grid gap-6" data-testid="supplyarr-settings-admin-workspace">
      <NotificationSettingsPanel accessToken={s.accessToken} canManage={s.canManageNotifications} />
      <PriceSnapshotSettingsPanel accessToken={s.accessToken} canManage={s.canManageNotifications} />
      <LeadTimeSnapshotSettingsPanel accessToken={s.accessToken} canManage={s.canManageNotifications} />
      <AvailabilitySnapshotSettingsPanel accessToken={s.accessToken} canManage={s.canManageNotifications} />
      <ProcurementCoordinationSettingsPanel accessToken={s.accessToken} canManage={s.canManageNotifications} />
      <ApprovalReminderSettingsPanel accessToken={s.accessToken} canManage={s.canManageNotifications} />
      <ProcurementExceptionEscalationSettingsPanel
        accessToken={s.accessToken}
        canManage={s.canManageNotifications}
      />
      <DemandProcessingSettingsPanel accessToken={s.accessToken} canManage={s.canManageNotifications} />
      <IntegrationEventSettingsPanel accessToken={s.accessToken} canManage={s.canManageNotifications} />
    </div>
  )
}
