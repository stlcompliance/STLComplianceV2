import { DataExportsPanel } from '../../components/DataExportsPanel'
import { PersonExportDeliverySettingsPanel } from '../../components/PersonExportDeliverySettingsPanel'
import { StaffArrScheduledWorkerSettingsPanel } from '../../components/StaffArrScheduledWorkerSettingsPanel'
import { STAFFARR_SCHEDULED_WORKER_PANELS } from '../../lib/staffarrWorkerPanels'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }

export function AdminSection({ state }: Props) {
  const s = state
  const canManageWorkerSettings = s.canManagePeopleProfiles

  return (
    canManageWorkerSettings ? (
      <div className="grid gap-6" data-testid="staffarr-settings-admin-workspace">
        <PersonExportDeliverySettingsPanel accessToken={s.accessToken} canManage={canManageWorkerSettings} />
        {STAFFARR_SCHEDULED_WORKER_PANELS.map((config) => (
          <StaffArrScheduledWorkerSettingsPanel
            key={config.workerKey}
            accessToken={s.accessToken}
            canManage={canManageWorkerSettings}
            config={config}
          />
        ))}
        <DataExportsPanel accessToken={s.accessToken} canExport={canManageWorkerSettings} />
      </div>
    ) : null
  )
}
