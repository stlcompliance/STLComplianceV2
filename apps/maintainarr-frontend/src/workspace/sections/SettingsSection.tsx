import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'
import { AssetBulkImportPanel } from '../../components/AssetBulkImportPanel'
import { AssetStatusRollupSettingsPanel } from '../../components/AssetStatusRollupSettingsPanel'
import { DowntimeTrackingSettingsPanel } from '../../components/DowntimeTrackingSettingsPanel'
import { MaintenancePlatformEventSettingsPanel } from '../../components/MaintenancePlatformEventSettingsPanel'
import { MaintenanceHistoryRollupSettingsPanel } from '../../components/MaintenanceHistoryRollupSettingsPanel'
import { AuditPackageExportPanel } from '../../components/AuditPackageExportPanel'
import { DefectEscalationSettingsPanel } from '../../components/DefectEscalationSettingsPanel'
import { PmDueScanSettingsPanel } from '../../components/PmDueScanSettingsPanel'
import { NotificationSettingsPanel } from '../../components/NotificationSettingsPanel'
import { MaintainArrTenantSettingsPanel } from '../../components/MaintainArrTenantSettingsPanel'
import { AssetRegistryPanel } from '../../components/AssetRegistryPanel'
import { useLocation, Link } from 'react-router-dom'

type Props = { state: MaintainArrWorkspaceState }

export function SettingsSection({ state }: Props) {
  const location = useLocation()
  const subpage = location.pathname.startsWith('/settings/source-data') ? 'source-data' : 'workspace'
  const { accessToken, canManage, canManageNotifications, canExportAudit } = state

  return (
    <div className="space-y-6">
      <nav className="flex flex-wrap items-center gap-2" aria-label="Settings subpages">
        <Link
          to="/settings"
          className={`rounded-md border px-3 py-1.5 text-sm ${
            subpage === 'workspace'
              ? 'border-amber-600 bg-amber-600/20 text-amber-200'
              : 'border-slate-700 bg-slate-900/60 text-slate-300 hover:border-slate-600'
          }`}
        >
          Workspace
        </Link>
        <Link
          to="/settings/source-data"
          className={`rounded-md border px-3 py-1.5 text-sm ${
            subpage === 'source-data'
              ? 'border-amber-600 bg-amber-600/20 text-amber-200'
              : 'border-slate-700 bg-slate-900/60 text-slate-300 hover:border-slate-600'
          }`}
        >
          Source Data
        </Link>
      </nav>

      {subpage === 'source-data' ? (
        <AssetRegistryPanel
          showSourceData
          showAssetsTable={false}
          classes={state.classesQuery.data ?? []}
          types={state.typesQuery.data ?? []}
          assets={[]}
          readinessByAssetId={{}}
          selectedAssetId={state.selectedAssetId}
          onSelectAsset={state.setSelectedAssetId}
          isLoading={state.classesQuery.isLoading || state.typesQuery.isLoading}
          isReadinessLoading={false}
        />
      ) : (
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
          <MaintainArrTenantSettingsPanel
            accessToken={accessToken}
            canManage={canManageNotifications}
            canAudit={canManageNotifications}
          />
          <PmDueScanSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
          <MaintenanceHistoryRollupSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
          <AssetStatusRollupSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
          <MaintenancePlatformEventSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
          <DowntimeTrackingSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
          <DefectEscalationSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
          <NotificationSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
        </div>
      ) : null}

      <div className="mt-8">
        <AuditPackageExportPanel accessToken={accessToken} canRead={canExportAudit} canExport={canExportAudit} />
      </div>
        </>
      )}
    </div>
  )
}
