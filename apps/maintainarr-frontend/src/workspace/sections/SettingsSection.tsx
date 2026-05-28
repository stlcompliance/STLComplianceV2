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
        <div className="mt-8">
          <PmDueScanSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
        </div>
      ) : null}

      {canManageNotifications ? (
        <div className="mt-8">
          <MaintenanceHistoryRollupSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
        </div>
      ) : null}

      {canManageNotifications ? (
        <div className="mt-8">
          <AssetStatusRollupSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
        </div>
      ) : null}

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
        <AuditPackageExportPanel accessToken={accessToken} canRead={canExportAudit} canExport={canExportAudit} />
      </div>
    </>
  )
}
