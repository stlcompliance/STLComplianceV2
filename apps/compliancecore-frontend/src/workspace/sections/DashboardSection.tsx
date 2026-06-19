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
  Search,
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

type Props = { state: ComplianceCoreWorkspaceState }

const traceSteps = [
  'Rulepack',
  'Requirement',
  'Required facts',
  'Mapped sources',
  'Calculation',
  'Result',
  'Product signal',
]

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
      className="group flex items-start gap-3 rounded-lg border border-slate-800 bg-slate-950/70 p-4 transition hover:border-sky-700 hover:bg-slate-900"
    >
      <span className="rounded-md border border-slate-800 bg-slate-900 p-2 text-sky-300">{icon}</span>
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
    <section className="space-y-4 rounded-lg border border-slate-800 bg-slate-950/60 p-5">
      <header className="space-y-1">
        <h2 className="text-lg font-semibold text-white">{title}</h2>
      </header>
      {children}
    </section>
  )
}

function TraceChain() {
  return (
    <section className="rounded-lg border border-slate-800 bg-slate-950/70 p-5">
      <h2 className="text-lg font-semibold text-white">Compliance Core trace</h2>
      <p className="mt-2 max-w-3xl text-sm text-slate-300">
        Every compliance answer should be explainable from source law through product impact.
      </p>
      <ol className="mt-4 grid gap-3 md:grid-cols-7">
        {traceSteps.map((step, index) => (
          <li key={step} className="rounded-lg border border-slate-800 bg-slate-900/70 p-3">
            <span className="text-xs font-semibold text-sky-300">{index + 1}</span>
            <p className="mt-1 text-sm font-medium text-slate-100">{step}</p>
          </li>
        ))}
      </ol>
    </section>
  )
}

export function DashboardSection({ state: s }: Props) {
  const canReadReports = s.me.canReadReports
  const canExportReports = s.me.canExportReports

  return (
    <div className="space-y-8" data-testid="compliancecore-dashboard-workspace">
      <section className="rounded-lg border border-slate-800 bg-gradient-to-br from-slate-950 via-slate-950 to-slate-900/80 p-6 shadow-2xl shadow-slate-950/30">
        <div className="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
          <div className="max-w-3xl space-y-3">
            <div className="inline-flex items-center gap-2 rounded-md border border-sky-900/60 bg-sky-950/40 px-3 py-1 text-xs font-semibold uppercase text-sky-200">
              <LayoutDashboard className="h-3.5 w-3.5" />
              Compliance Core overview
            </div>
            <h1 className="text-3xl font-semibold text-white sm:text-4xl">
              Rulepacks define what may be calculated. Mappings define where data comes from.
              Evaluations explain what happened.
            </h1>
            <p className="max-w-2xl text-sm text-slate-300 sm:text-base">
              Compliance Core owns regulatory meaning, evidence interpretation, applicability,
              rule-to-product mappings, and evaluation results. Source products still own their
              people, trips, assets, training, inventory, documents, and operational workflows.
            </p>
          </div>

          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-1 xl:grid-cols-2">
            <QuickActionLink
              to="/rulepacks"
              icon={<Library className="h-4 w-4" />}
              title="Rulepacks"
              description="See what legal and policy logic exists, what it requires, and what is unmapped."
            />
            <QuickActionLink
              to="/mappings/coverage"
              icon={<GitBranch className="h-4 w-4" />}
              title="Mapping Center"
              description="Inspect coverage, fact sources, evidence mappings, vocabulary, subjects, and outputs."
            />
            <QuickActionLink
              to="/evaluation/recent"
              icon={<ShieldCheck className="h-4 w-4" />}
              title="Evaluations"
              description="Review recent runs, test situations, and inspect calculation traces."
            />
            <QuickActionLink
              to="/findings"
              icon={<Search className="h-4 w-4" />}
              title="Review Queue"
              description="Work unknown facts, conflicts, unmapped evidence, and rulepack review items."
            />
          </div>
        </div>
      </section>

      <TraceChain />

      <SectionCard title="Operating picture">
        <OperatorDashboardPanel accessToken={s.accessToken} />
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

      <section className="rounded-lg border border-slate-800 bg-slate-950/60 p-5">
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
          to="/questionnaires"
          icon={<ClipboardList className="h-4 w-4" />}
          title="Questionnaires"
          description="Capture fallback facts and assumptions when mapped product data is not enough."
        />
        <QuickActionLink
          to="/mappings/facts"
          icon={<ListChecks className="h-4 w-4" />}
          title="Fact sources"
          description="Manage fact-source configuration and sync health."
        />
        <QuickActionLink
          to="/reports"
          icon={<ShieldCheck className="h-4 w-4" />}
          title="Reports"
          description="Review audit readiness, evidence completeness, and exportable summaries."
        />
      </section>
    </div>
  )
}
