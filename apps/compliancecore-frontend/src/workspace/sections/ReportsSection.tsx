import { ComplianceReportsPanel } from '../../components/ComplianceReportsPanel'
import { CitationReviewReportsPanel } from '../../components/CitationReviewReportsPanel'
import { EvidenceCompletenessReportsPanel } from '../../components/EvidenceCompletenessReportsPanel'
import { AuditReadinessReportsPanel } from '../../components/AuditReadinessReportsPanel'
import { DataExportsPanel } from '../../components/DataExportsPanel'
import { ExceptionExemptionReportsPanel } from '../../components/ExceptionExemptionReportsPanel'
import { RemediationQueueReportsPanel } from '../../components/RemediationQueueReportsPanel'
import { ProductIntegrationHealthReportsPanel } from '../../components/ProductIntegrationHealthReportsPanel'
import { OperatorReportsPanel } from '../../components/OperatorReportsPanel'
import { WaiverReportsPanel } from '../../components/WaiverReportsPanel'
import { RuleChangeImpactReportPanel } from '../../components/RuleChangeImpactReportPanel'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

export function ReportsSection({ state }: Props) {
  const canRead = state.me.canReadReports
  const canExport = state.me.canExportReports
  const showReportsWorkspace = canRead || canExport

  if (!showReportsWorkspace) {
    return (
      <p className="text-sm text-slate-400">You do not have permission to view compliance reports.</p>
    )
  }

  return (
    <div className="grid gap-6" data-testid="compliancecore-reports-workspace">
      <ComplianceReportsPanel
        accessToken={state.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
      <CitationReviewReportsPanel
        accessToken={state.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
      <EvidenceCompletenessReportsPanel
        accessToken={state.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
      <OperatorReportsPanel
        accessToken={state.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
      <RuleChangeImpactReportPanel
        accessToken={state.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
      <WaiverReportsPanel accessToken={state.accessToken} canRead={canRead} canExport={canExport} />
      <ExceptionExemptionReportsPanel
        accessToken={state.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
      <RemediationQueueReportsPanel
        accessToken={state.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
      <AuditReadinessReportsPanel
        accessToken={state.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
      <ProductIntegrationHealthReportsPanel
        accessToken={state.accessToken}
        canRead={canRead}
        canExport={canExport}
      />
      <DataExportsPanel accessToken={state.accessToken} canExport={canExport} />
    </div>
  )
}
