import { NotificationSettingsPanel } from '../../components/NotificationSettingsPanel'
import { QualificationRecalculationSettingsPanel } from '../../components/QualificationRecalculationSettingsPanel'
import { RulePackImpactSettingsPanel } from '../../components/RulePackImpactSettingsPanel'
import { EvidenceRetentionSettingsPanel } from '../../components/EvidenceRetentionSettingsPanel'
import { RecertificationSettingsPanel } from '../../components/RecertificationSettingsPanel'
import { EventProcessingSettingsPanel } from '../../components/EventProcessingSettingsPanel'
import { StaffarrPublicationSettingsPanel } from '../../components/StaffarrPublicationSettingsPanel'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }

export function SettingsSection({ state }: Props) {
  const s = state
  if (!s.canNotifications) {
    return <p className="text-sm text-slate-400">You do not have permission to manage notification settings.</p>
  }

  return (
    <div className="space-y-6">
      <NotificationSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
      <RecertificationSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
      <QualificationRecalculationSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
      <RulePackImpactSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
      <EvidenceRetentionSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
      <StaffarrPublicationSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
      <EventProcessingSettingsPanel accessToken={s.accessToken} canManage={s.canNotifications} />
    </div>
  )
}
