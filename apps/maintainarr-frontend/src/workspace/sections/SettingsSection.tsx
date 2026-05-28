import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'
import { AuditPackageExportPanel } from '../../components/AuditPackageExportPanel'
import { DefectEscalationSettingsPanel } from '../../components/DefectEscalationSettingsPanel'
import { NotificationSettingsPanel } from '../../components/NotificationSettingsPanel'

type Props = { state: MaintainArrWorkspaceState }

export function SettingsSection({ state }: Props) {
  const { accessToken, canManageNotifications, canExportAudit } = state

  return (
    <>
      {canManageNotifications ? (
        <div className="mt-8">
          <DefectEscalationSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
        </div>
      ) : null}

      {canManageNotifications ? (
        <div className="mt-8">
          <NotificationSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
        </div>
      ) : null}

      <div className="mt-8">
        <AuditPackageExportPanel accessToken={accessToken} canExport={canExportAudit} />
      </div>
    </>
  )
}
