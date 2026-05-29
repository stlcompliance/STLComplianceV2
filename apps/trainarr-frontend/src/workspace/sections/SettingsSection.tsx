import { NotificationSettingsPanel } from '../../components/NotificationSettingsPanel'
import { AssignmentReminderEscalationSettingsPanel } from '../../components/AssignmentReminderEscalationSettingsPanel'
import { QualificationRecalculationSettingsPanel } from '../../components/QualificationRecalculationSettingsPanel'
import { RulePackImpactSettingsPanel } from '../../components/RulePackImpactSettingsPanel'
import { EvidenceRetentionSettingsPanel } from '../../components/EvidenceRetentionSettingsPanel'
import { OrphanReferenceSettingsPanel } from '../../components/OrphanReferenceSettingsPanel'
import { AuditPackageExportPanel } from '../../components/AuditPackageExportPanel'
import { RecertificationSettingsPanel } from '../../components/RecertificationSettingsPanel'
import { EventProcessingSettingsPanel } from '../../components/EventProcessingSettingsPanel'
import { StaffarrPublicationSettingsPanel } from '../../components/StaffarrPublicationSettingsPanel'
import { IntegrationSettingsPanel } from '../../components/IntegrationSettingsPanel'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }

export function SettingsSection({ state }: Props) {
  const s = state
  if (!s.canNotifications && !s.canExportAudit) {
    return <p className="text-sm text-slate-400">You do not have permission to manage settings.</p>
  }

  return (
    <div className="space-y-6">
      {s.canNotifications ? (
        <div className="grid gap-6" data-testid="trainarr-settings-admin-workspace">
          <IntegrationSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
          <NotificationSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
          <AssignmentReminderEscalationSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
          <RecertificationSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
          <QualificationRecalculationSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
          <RulePackImpactSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
          <EvidenceRetentionSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
          <OrphanReferenceSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
          <StaffarrPublicationSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
          <EventProcessingSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
        </div>
      ) : null}
      {s.canReadAudit ? (
        <AuditPackageExportPanel accessToken={s.accessToken} canExport={s.canExportAudit} />
      ) : null}
    </div>
  )
}
