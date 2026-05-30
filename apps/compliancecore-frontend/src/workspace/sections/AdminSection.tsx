import { AuditPackageExportPanel } from '../../components/AuditPackageExportPanel'
import { CsvImportExportPanel } from '../../components/CsvImportExportPanel'
import { ReadinessForecastPanel } from '../../components/ReadinessForecastPanel'
import { ControlEffectivenessPanel } from '../../components/ControlEffectivenessPanel'
import { MissingEvidenceWarningsPanel } from '../../components/MissingEvidenceWarningsPanel'
import { RiskScoringPanel } from '../../components/RiskScoringPanel'
import { AuditDeliveryOrchestrationPanel } from '../../components/AuditDeliveryOrchestrationPanel'
import { M12AnalyticsWorkerSettingsPanel } from '../../components/M12AnalyticsWorkerSettingsPanel'
import { RuleChangeMonitoringPanel } from '../../components/RuleChangeMonitoringPanel'
import { SourceIngestionPanel } from '../../components/SourceIngestionPanel'
import { ImportWizardPanel } from '../../components/ImportWizardPanel'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function AdminSection({ state }: Props) {
  const s = state
  const showAdminWorkspace =
    s.canManage ||
    s.canReadOrchestration ||
    s.canEvaluateRisk ||
    s.canEvaluateMissingEvidence ||
    s.canEvaluateControlEffectiveness ||
    s.canEvaluateReadinessForecast

  return (
    <>
      {showAdminWorkspace ? (
        <div className="grid gap-6" data-testid="compliancecore-settings-admin-workspace">
          <AuditDeliveryOrchestrationPanel
            accessToken={s.accessToken}
            canRead={s.canReadOrchestration}
            canTrigger={s.canManage}
          />
          <M12AnalyticsWorkerSettingsPanel accessToken={s.accessToken} canManage={s.canManage} />
          <ReadinessForecastPanel
            accessToken={s.accessToken}
            canEvaluate={s.canEvaluateReadinessForecast}
          />
          <ControlEffectivenessPanel
            accessToken={s.accessToken}
            canEvaluate={s.canEvaluateControlEffectiveness}
          />
          <MissingEvidenceWarningsPanel
            accessToken={s.accessToken}
            canEvaluate={s.canEvaluateMissingEvidence}
          />
          <RiskScoringPanel accessToken={s.accessToken} canEvaluate={s.canEvaluateRisk} />
          <RuleChangeMonitoringPanel accessToken={s.accessToken} />
          <SourceIngestionPanel accessToken={s.accessToken} canManage={s.canManage} />
          <ImportWizardPanel accessToken={s.accessToken} canManage={s.canManage} />
          <CsvImportExportPanel accessToken={s.accessToken} canManage={s.canManage} />
        </div>
      ) : null}

      {s.canExportAudit ? (
        <div className={showAdminWorkspace ? 'mt-8' : undefined}>
          <AuditPackageExportPanel accessToken={s.accessToken} canExport={s.canExportAudit} />
        </div>
      ) : null}
    </>
  )
}
