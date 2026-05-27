import { useQuery } from '@tanstack/react-query'

import { getOperatorDashboard } from '../api/client'
import type { OperatorDashboardResponse } from '../api/types'

type OperatorDashboardPanelProps = {
  accessToken: string
}

function SummaryCard({ label, value, hint }: { label: string; value: number; hint?: string }) {
  return (
    <div className="rounded-lg border border-slate-700 bg-slate-900/60 p-4">
      <p className="text-xs uppercase tracking-wide text-slate-500">{label}</p>
      <p className="mt-1 text-2xl font-semibold text-slate-50">{value}</p>
      {hint ? <p className="mt-1 text-xs text-slate-400">{hint}</p> : null}
    </div>
  )
}

function formatGeneratedAt(iso: string) {
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}

export function OperatorDashboardPanel({ accessToken }: OperatorDashboardPanelProps) {
  const dashboardQuery = useQuery({
    queryKey: ['compliancecore-operator-dashboard', accessToken],
    queryFn: () => getOperatorDashboard(accessToken),
  })

  if (dashboardQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading operator dashboard…</p>
  }

  if (dashboardQuery.isError) {
    return (
      <p className="text-sm text-red-300">
        Failed to load dashboard: {(dashboardQuery.error as Error).message}
      </p>
    )
  }

  const dashboard = dashboardQuery.data as OperatorDashboardResponse

  return (
    <section className="space-y-6" aria-label="Operator dashboard">
      <div className="rounded-xl border border-slate-700 bg-slate-900/80 p-5">
        <h2 className="text-lg font-semibold text-slate-50">Operator overview</h2>
        <p className="mt-1 text-sm text-slate-400">
          Live counts from findings, evaluations, rule packs, workflow gates, and audit events. Updated{' '}
          {formatGeneratedAt(dashboard.generatedAt)}.
        </p>
      </div>

      <div>
        <h3 className="mb-3 text-sm font-medium text-slate-300">Findings</h3>
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          <SummaryCard label="Open" value={dashboard.findings.openCount} />
          <SummaryCard
            label="Open (block severity)"
            value={dashboard.findings.openBlockSeverityCount}
          />
          <SummaryCard label="Open (warn severity)" value={dashboard.findings.openWarnSeverityCount} />
          <SummaryCard label="Acknowledged" value={dashboard.findings.acknowledgedCount} />
          <SummaryCard label="Resolved" value={dashboard.findings.resolvedCount} />
          <SummaryCard label="Total findings" value={dashboard.findings.totalCount} />
        </div>
      </div>

      <div>
        <h3 className="mb-3 text-sm font-medium text-slate-300">Rule packs by status</h3>
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
          <SummaryCard label="Draft" value={dashboard.rulePacks.draftCount} />
          <SummaryCard label="In review" value={dashboard.rulePacks.reviewCount} />
          <SummaryCard label="Published" value={dashboard.rulePacks.publishedCount} />
          <SummaryCard label="Archived" value={dashboard.rulePacks.archivedCount} />
          <SummaryCard label="Total packs" value={dashboard.rulePacks.totalCount} />
        </div>
      </div>

      <div>
        <h3 className="mb-3 text-sm font-medium text-slate-300">Rule evaluations</h3>
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <SummaryCard label="Total runs" value={dashboard.evaluations.totalCount} />
          <SummaryCard label="Last 24 hours" value={dashboard.evaluations.last24HoursCount} />
          <SummaryCard label="Pass" value={dashboard.evaluations.passCount} />
          <SummaryCard label="Fail" value={dashboard.evaluations.failCount} />
        </div>
      </div>

      <div>
        <h3 className="mb-3 text-sm font-medium text-slate-300">Workflow gates</h3>
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          <SummaryCard label="Gate definitions" value={dashboard.workflowGates.definitionCount} />
          <SummaryCard
            label="Check results (total)"
            value={dashboard.workflowGates.checkResultsTotal}
          />
          <SummaryCard
            label="Checks (24h)"
            value={dashboard.workflowGates.checkResultsLast24Hours}
          />
          <SummaryCard
            label="Block outcomes"
            value={dashboard.workflowGates.blockOutcomeCount}
            hint="Gate check failures"
          />
          <SummaryCard label="Warn outcomes" value={dashboard.workflowGates.warnOutcomeCount} />
          <SummaryCard label="Allow outcomes" value={dashboard.workflowGates.allowOutcomeCount} />
        </div>
      </div>

      <div>
        <h3 className="mb-3 text-sm font-medium text-slate-300">Audit events</h3>
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <SummaryCard label="Total events" value={dashboard.auditEvents.totalCount} />
          <SummaryCard label="Last 24 hours" value={dashboard.auditEvents.last24HoursCount} />
          <SummaryCard label="Success" value={dashboard.auditEvents.successCount} />
          <SummaryCard label="Non-success" value={dashboard.auditEvents.failureCount} />
        </div>
      </div>

      <div className="rounded-xl border border-slate-700 bg-slate-900/80 p-5">
        <h3 className="text-sm font-medium text-slate-300">Recent evaluations</h3>
        {dashboard.recentEvaluations.length === 0 ? (
          <p className="mt-3 text-sm text-slate-500">No evaluation runs yet.</p>
        ) : (
          <ul className="mt-3 divide-y divide-slate-800">
            {dashboard.recentEvaluations.map((item) => (
              <li key={item.evaluationRunId} className="flex flex-wrap items-baseline justify-between gap-2 py-2">
                <div>
                  <p className="text-sm font-medium text-slate-100">{item.rulePackLabel}</p>
                  <p className="text-xs text-slate-500">{item.packKey}</p>
                </div>
                <div className="text-right text-sm">
                  <span
                    className={
                      item.overallResult === 'pass'
                        ? 'text-emerald-400'
                        : 'text-amber-400'
                    }
                  >
                    {item.overallResult}
                  </span>
                  <p className="text-xs text-slate-500">{formatGeneratedAt(item.createdAt)}</p>
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
