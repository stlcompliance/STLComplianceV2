import { AuditPackageExportPanel } from '../../components/AuditPackageExportPanel'
import { TenantSettingsPanel } from '../../components/TenantSettingsPanel'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }

export function SettingsSection({ state }: Props) {
  const s = state
  if (!s.canReadSettings && !s.canExportAudit) {
    return <p className="text-sm text-slate-400">You do not have permission to manage settings.</p>
  }

  return (
    <div className="space-y-6">
      {s.canReadSettings ? (
        <div className="grid gap-6" data-testid="trainarr-settings-admin-workspace">
          <TenantSettingsPanel
            accessToken={s.accessToken}
            canRead={s.canReadSettings}
            canManage={s.canManageSettings}
          />
        </div>
      ) : null}
      {s.canReadAudit ? (
        <AuditPackageExportPanel accessToken={s.accessToken} canExport={s.canExportAudit} />
      ) : null}
    </div>
  )
}
