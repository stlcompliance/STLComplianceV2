import { Link } from 'react-router-dom'
import type { ReactNode } from 'react'
import {
  ArrowRight,
  ClipboardList,
  GitBranch,
  LayoutDashboard,
  Library,
  ListChecks,
  ShieldCheck,
  FileText,
} from 'lucide-react'

import { ComplianceReportsPanel } from '../../components/ComplianceReportsPanel'
import { ControlEffectivenessPanel } from '../../components/ControlEffectivenessPanel'
import { ImportWizardPanel } from '../../components/ImportWizardPanel'
import { MissingEvidenceWarningsPanel } from '../../components/MissingEvidenceWarningsPanel'
import { OperatorDashboardPanel } from '../../components/OperatorDashboardPanel'
import { ProductIntegrationHealthReportsPanel } from '../../components/ProductIntegrationHealthReportsPanel'
import { ReadinessForecastPanel } from '../../components/ReadinessForecastPanel'
import { RiskScoringPanel } from '../../components/RiskScoringPanel'
import { RuleChangeMonitoringPanel } from '../../components/RuleChangeMonitoringPanel'
import { SourceIngestionPanel } from '../../components/SourceIngestionPanel'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'
import { QuestionnaireFlow } from '@stl/shared-ui'

type Props = { state: ComplianceCoreWorkspaceState }

function QuickActionLink({
  to,
  icon,
  title,
  description,
}: {
  to: string
  icon: ReactNode
  title: string
  description: string
}) {
  return (
    <Link
      to={to}
      className="group flex items-start gap-3 rounded-2xl border border-slate-800 bg-slate-950/70 p-4 transition hover:border-sky-700 hover:bg-slate-900"
    >
      <span className="rounded-xl border border-slate-800 bg-slate-900 p-2 text-sky-300">{icon}</span>
      <span className="min-w-0 flex-1">
        <span className="flex items-center gap-2 text-sm font-semibold text-white">
          {title}
          <ArrowRight className="h-4 w-4 text-slate-500 transition group-hover:translate-x-0.5 group-hover:text-sky-300" />
        </span>
        <span className="mt-1 block text-xs text-slate-400">{description}</span>
      </span>
    </Link>
  )
}

function SectionCard({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="space-y-4 rounded-3xl border border-slate-800 bg-slate-950/60 p-5">
      <header className="space-y-1">
        <h2 className="text-lg font-semibold text-white">{title}</h2>
      </header>
      {children}
    </section>
  )
}

export function DashboardSection({ state: s }: Props) {
  const canReadReports = s.me.canReadReports
  const canExportReports = s.me.canExportReports
  const complianceCoreApiBase = import.meta.env.VITE_COMPLIANCECORE_API_BASE ?? ''

  return (
    <div className="space-y-8" data-testid="compliancecore-dashboard-workspace">
      <section className="rounded-3xl border border-slate-800 bg-gradient-to-br from-slate-950 via-slate-950 to-slate-900/80 p-6 shadow-2xl shadow-slate-950/30">
        <div className="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
          <div className="max-w-3xl space-y-3">
            <div className="inline-flex items-center gap-2 rounded-full border border-sky-900/60 bg-sky-950/40 px-3 py-1 text-xs font-semibold uppercase tracking-[0.25em] text-sky-200">
              <LayoutDashboard className="h-3.5 w-3.5" />
              Compliance Core dashboard
            </div>
            <h1 className="text-3xl font-semibold text-white sm:text-4xl">
              Rulepack health, evidence gaps, and compliance risk in one place.
            </h1>
            <p className="max-w-2xl text-sm text-slate-300 sm:text-base">
              This dashboard is read-only and operational: it shows what is healthy, what is at risk,
              and what needs attention next. Rule meaning, evidence interpretation, and evaluation
              all remain owned by Compliance Core.
            </p>
          </div>

          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-1 xl:grid-cols-2">
            <QuickActionLink
              to="/registry/drawer"
              icon={<Library className="h-4 w-4" />}
              title="Open registry"
              description="Review governing bodies, rule packs, citations, and controlled vocabulary."
            />
            <QuickActionLink
              to="/evaluation"
              icon={<ShieldCheck className="h-4 w-4" />}
              title="Run evaluation"
              description="Check rule content, facts, and theoretical compliance conditions."
            />
            <QuickActionLink
              to="/mappings"
              icon={<GitBranch className="h-4 w-4" />}
              title="Review mappings"
              description="Inspect citations, fact requirements, and mapping coverage."
            />
            <QuickActionLink
              to="/reports"
              icon={<FileText className="h-4 w-4" />}
              title="Open reports"
              description="Review audit readiness, evidence completeness, and exportable summaries."
            />
          </div>
        </div>
      </section>

      <SectionCard title="Operating picture">
        <OperatorDashboardPanel accessToken={s.accessToken} />
      </SectionCard>

      <SectionCard title="Tenant onboarding">
        <QuestionnaireFlow
          apiBase={complianceCoreApiBase}
          accessToken={s.accessToken}
          tenantId={s.session.tenantId}
          productKey="compliancecore"
          workflowKey="tenant_onboarding"
          subjectType="tenant"
          sourceRecordId={`tenant-${s.session.tenantId}`}
          sourceEntity="tenant"
          title="Compliance Core onboarding questionnaire"
          subtitle="Capture a plain-language profile for the tenant and seed the first compliance facts."
          submitLabel="Save onboarding answers"
        />
      </SectionCard>

      <div className="grid gap-8 xl:grid-cols-2">
        <SectionCard title="Attention and readiness">
          <div className="space-y-6">
            <MissingEvidenceWarningsPanel accessToken={s.accessToken} canEvaluate={s.canEvaluateMissingEvidence} />
            <ReadinessForecastPanel accessToken={s.accessToken} canEvaluate={s.canEvaluateReadinessForecast} />
            <ControlEffectivenessPanel
              accessToken={s.accessToken}
              canEvaluate={s.canEvaluateControlEffectiveness}
            />
            <RiskScoringPanel accessToken={s.accessToken} canEvaluate={s.canEvaluateRisk} />
          </div>
        </SectionCard>

        <SectionCard title="Change and sync health">
          <div className="space-y-6">
            <RuleChangeMonitoringPanel accessToken={s.accessToken} />
            {canReadReports ? (
              <ComplianceReportsPanel
                accessToken={s.accessToken}
                canRead={canReadReports}
                canExport={canExportReports}
              />
            ) : null}
            <ProductIntegrationHealthReportsPanel
              accessToken={s.accessToken}
              canRead={canReadReports}
              canExport={canExportReports}
            />
            <SourceIngestionPanel accessToken={s.accessToken} canManage={s.canManage} />
            <ImportWizardPanel accessToken={s.accessToken} canManage={s.canManage} />
          </div>
        </SectionCard>
      </div>

      <section className="rounded-3xl border border-slate-800 bg-slate-950/60 p-5">
        <h2 className="text-lg font-semibold text-white">Scope note</h2>
        <p className="mt-2 max-w-4xl text-sm text-slate-300">
          Dashboard scope: Compliance Core owns governing body catalogs, rulepacks, requirements,
          applicability logic, evidence requirements, exceptions, exemptions, controlled vocabulary,
          and compliance evaluation results. Cross-product signals appear here as read-only references
          only and do not mutate the owning product.
        </p>
      </section>

      <section className="grid gap-3 md:grid-cols-3">
        <QuickActionLink
          to="/registry/details"
          icon={<ClipboardList className="h-4 w-4" />}
          title="Rule-pack details"
          description="Open the selected registry detail profile and review citations, facts, and findings."
        />
        <QuickActionLink
          to="/fact-sources"
          icon={<ListChecks className="h-4 w-4" />}
          title="Fact sources"
          description="Manage fact-source configuration and sync health."
        />
        <QuickActionLink
          to="/admin"
          icon={<ShieldCheck className="h-4 w-4" />}
          title="Admin tools"
          description="Review imports, audit packages, and scheduled evaluation tools."
        />
      </section>
    </div>
  )
}
