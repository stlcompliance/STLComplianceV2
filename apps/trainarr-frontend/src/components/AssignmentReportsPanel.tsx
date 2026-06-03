import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import {
  exportAssignmentReportSummaryCsv,
  getAssignmentReportSummary,
} from '../api/client'

interface AssignmentReportsPanelProps {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border border-border bg-card px-3 py-2">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className="text-lg font-semibold text-foreground">{value}</p>
    </div>
  )
}

function formatPercent(value: number | null): string {
  return value == null ? '—' : `${value.toFixed(1)}%`
}

function formatDays(value: number | null): string {
  return value == null ? '—' : `${value.toFixed(1)} days`
}

function formatCurrency(value: number): string {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value)
}

export function AssignmentReportsPanel({
  accessToken,
  canRead,
  canExport,
}: AssignmentReportsPanelProps) {
  const [status, setStatus] = useState('all')
  const [overdueOnly, setOverdueOnly] = useState(false)

  const summaryQuery = useQuery({
    queryKey: ['trainarr-assignment-report-summary', accessToken, status, overdueOnly],
    queryFn: () =>
      getAssignmentReportSummary(accessToken, {
        status: status === 'all' ? undefined : status,
        overdueOnly,
      }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportAssignmentReportSummaryCsv(accessToken, {
        status: status === 'all' ? undefined : status,
        overdueOnly,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `trainarr-assignment-report-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  if (!canRead) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-border bg-card p-5"
      data-testid="assignment-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-foreground">Training assignment reports</h2>
          <p className="mt-1 text-sm text-muted-foreground">
            Assignment status rollups, overdue counts, completion rates, labor spend, and locale-tagged
            content coverage from TrainArr-owned tables.
          </p>
        </div>
        {canExport ? (
          <button
            type="button"
            className="rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50"
            disabled={exportMutation.isPending}
            onClick={() => exportMutation.mutate()}
          >
            {exportMutation.isPending ? 'Exporting…' : 'Export CSV'}
          </button>
        ) : null}
      </div>

      <div className="mt-4 flex flex-wrap gap-4 text-sm text-foreground">
        <label htmlFor="assignment-reports-status" className="flex items-center gap-2">
          <span>Status</span>
          <select
            id="assignment-reports-status"
            className="rounded border border-border bg-background px-2 py-1"
            value={status}
            onChange={(event) => setStatus(event.target.value)}
          >
            <option value="all">All</option>
            <option value="assigned">Assigned</option>
            <option value="in_progress">In progress</option>
            <option value="completed">Completed</option>
          </select>
        </label>
        <label htmlFor="assignment-reports-overdue-only" className="flex items-center gap-2">
          <input
            id="assignment-reports-overdue-only"
            type="checkbox"
            data-testid="assignment-reports-overdue-only"
            checked={overdueOnly}
            onChange={(event) => setOverdueOnly(event.target.checked)}
          />
          Overdue only
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-muted-foreground">Loading assignment report summary…</p>
      )}

      {summaryQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Assignment report unavailable"
            message={getErrorMessage(summaryQuery.error, 'Failed to load assignment report summary.')}
            retryLabel="Retry summary"
            onRetry={() => {
              void summaryQuery.refetch()
            }}
          />
        </div>
      )}

      {exportMutation.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="CSV export failed"
            message={getErrorMessage(exportMutation.error, 'Unable to export assignment report CSV.')}
          />
        </div>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Assignments in scope" value={String(summaryQuery.data.totalAssignments)} />
            <MetricCard label="Open assignments" value={String(summaryQuery.data.openAssignments)} />
            <MetricCard label="Completed assignments" value={String(summaryQuery.data.completedAssignments)} />
            <MetricCard label="Overdue assignments" value={String(summaryQuery.data.overdueAssignments)} />
            <MetricCard
              label="Completion rate"
              value={`${summaryQuery.data.completionRatePercent.toFixed(1)}%`}
            />
          </div>

          <div className="mt-5">
            <h3 className="text-sm font-semibold text-foreground">Training effectiveness</h3>
            <p className="mt-1 text-xs text-muted-foreground">
              Derived from completed assignments, evaluations, evidence, signoff activity, labor logs, and
              localized content references.
            </p>
            <div className="mt-3 grid gap-3 sm:grid-cols-2 lg:grid-cols-5 text-sm">
              <MetricCard
                label="Avg. completion time"
                value={formatDays(summaryQuery.data.analytics.averageCompletionDays)}
              />
              <MetricCard
                label="Evaluation pass rate"
                value={formatPercent(summaryQuery.data.analytics.evaluationPassRatePercent)}
              />
              <MetricCard
                label="Avg. evaluation score"
                value={
                  summaryQuery.data.analytics.averageEvaluationScore == null
                    ? '—'
                    : summaryQuery.data.analytics.averageEvaluationScore.toFixed(1)
                }
              />
              <MetricCard
                label="Evidence coverage"
                value={formatPercent(summaryQuery.data.analytics.evidenceCoveragePercent)}
              />
              <MetricCard
                label="Signoff coverage"
                value={formatPercent(summaryQuery.data.analytics.signoffCoveragePercent)}
              />
            </div>
          </div>

          <div className="mt-5">
            <h3 className="text-sm font-semibold text-foreground">Labor and localization</h3>
            <p className="mt-1 text-xs text-muted-foreground">
              Derived from assignment labor entries and locale-tagged program content references.
            </p>
            <div className="mt-3 grid gap-3 sm:grid-cols-2 lg:grid-cols-6 text-sm">
              <MetricCard label="Labor hours" value={summaryQuery.data.analytics.totalLaborHours.toFixed(2)} />
              <MetricCard label="Labor cost" value={formatCurrency(summaryQuery.data.analytics.totalLaborCost)} />
              <MetricCard
                label="Avg. labor hours / completed assignment"
                value={
                  summaryQuery.data.analytics.averageLaborHoursPerCompletedAssignment == null
                    ? '—'
                    : summaryQuery.data.analytics.averageLaborHoursPerCompletedAssignment.toFixed(2)
                }
              />
              <MetricCard
                label="Avg. labor cost / completed assignment"
                value={
                  summaryQuery.data.analytics.averageLaborCostPerCompletedAssignment == null
                    ? '—'
                    : formatCurrency(summaryQuery.data.analytics.averageLaborCostPerCompletedAssignment)
                }
              />
              <MetricCard
                label="Locale-tagged refs"
                value={String(summaryQuery.data.analytics.localizedContentReferenceCount)}
              />
              <MetricCard
                label="Locales represented"
                value={String(summaryQuery.data.analytics.distinctContentLocaleCount)}
              />
            </div>
          </div>

          {summaryQuery.data.recentAssignments.length === 0 ? (
            <p className="mt-4 text-sm text-muted-foreground">No assignments match this filter.</p>
          ) : (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b border-border text-left text-muted-foreground">
                    <th className="px-2 py-2">Definition</th>
                    <th className="px-2 py-2">Status</th>
                    <th className="px-2 py-2">Due</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryQuery.data.recentAssignments.slice(0, 8).map((item) => (
                    <tr key={item.assignmentId} className="border-b border-border/60">
                      <td className="px-2 py-2">{item.definitionName}</td>
                      <td className="px-2 py-2">{item.status}</td>
                      <td className="px-2 py-2">
                        {item.dueAt ? new Date(item.dueAt).toLocaleDateString() : '—'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}
    </section>
  )
}
