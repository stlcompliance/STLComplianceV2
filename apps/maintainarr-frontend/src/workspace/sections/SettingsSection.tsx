import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'
import { AssetBulkImportPanel } from '../../components/AssetBulkImportPanel'
import { AssetStatusRollupSettingsPanel } from '../../components/AssetStatusRollupSettingsPanel'
import { MaintenanceHistoryRollupSettingsPanel } from '../../components/MaintenanceHistoryRollupSettingsPanel'
import { AuditPackageExportPanel } from '../../components/AuditPackageExportPanel'
import { DefectEscalationSettingsPanel } from '../../components/DefectEscalationSettingsPanel'
import { PmDueScanSettingsPanel } from '../../components/PmDueScanSettingsPanel'
import { NotificationSettingsPanel } from '../../components/NotificationSettingsPanel'

type Props = { state: MaintainArrWorkspaceState }

export function SettingsSection({ state }: Props) {
  const { accessToken, canManage, canManageNotifications, canExportAudit } = state

  return (
    <>
      <AssetBulkImportPanel
        accessToken={accessToken}
        canImport={canManage}
        onComplete={() => {
          void state.assetsQuery.refetch()
        }}
      />

      {canManageNotifications ? (
        <div className="mt-8 grid gap-6" data-testid="maintainarr-settings-admin-workspace">
          <PmDueScanSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
          <MaintenanceHistoryRollupSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
          <AssetStatusRollupSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
          <DefectEscalationSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
          <NotificationSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
        </div>
      ) : null}

      <div className="mt-8">
        <AuditPackageExportPanel accessToken={accessToken} canRead={canExportAudit} canExport={canExportAudit} />
      </div>
    </>
  )
}
