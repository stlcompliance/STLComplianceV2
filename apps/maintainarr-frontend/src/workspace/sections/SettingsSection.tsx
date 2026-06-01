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
          mode="create"
          showSourceData
          showAssetsTable={false}
          showAssetCreateForm={false}
          canManage={canManage}
          classes={state.classesQuery.data ?? []}
          types={state.typesQuery.data ?? []}
          assets={[]}
          readinessByAssetId={{}}
          selectedAssetId={state.selectedAssetId}
          onSelectAsset={state.setSelectedAssetId}
          isLoading={state.classesQuery.isLoading || state.typesQuery.isLoading}
          isReadinessLoading={false}
          className={state.className}
          classDescription={state.classDescription}
          confirmedClassKey={state.confirmedClassKey}
          selectedClassId={state.selectedClassId}
          typeName={state.typeName}
          typeDescription={state.typeDescription}
          confirmedTypeKey={state.confirmedTypeKey}
          selectedTypeId={state.selectedTypeId}
          assetTag={state.assetTag}
          assetName={state.assetName}
          assetDescription={state.assetDescription}
          siteRef={state.siteRef}
          onClassNameChange={state.setClassName}
          onClassDescriptionChange={state.setClassDescription}
          onSelectedClassIdChange={state.setSelectedClassId}
          onTypeNameChange={state.setTypeName}
          onTypeDescriptionChange={state.setTypeDescription}
          onSelectedTypeIdChange={state.setSelectedTypeId}
          onAssetTagChange={state.setAssetTag}
          onAssetNameChange={state.setAssetName}
          onAssetDescriptionChange={state.setAssetDescription}
          onSiteRefChange={state.setSiteRef}
          onCreateClass={() => state.createClassMutation.mutate()}
          onCreateType={() => state.createTypeMutation.mutate()}
          onCreateAsset={() => state.createAssetMutation.mutate()}
          isCreatingClass={state.createClassMutation.isPending}
          isCreatingType={state.createTypeMutation.isPending}
          isCreatingAsset={state.createAssetMutation.isPending}
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
