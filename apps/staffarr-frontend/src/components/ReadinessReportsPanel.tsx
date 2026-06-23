import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import {
  exportReadinessReportSummaryCsv,
  getReadinessReportSummary,
} from '../api/client'

interface ReadinessReportsPanelProps {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2">
      <p className="text-xs text-[var(--color-text-muted)]">{label}</p>
      <p className="text-lg font-semibold text-[var(--color-text-primary)]">{value}</p>
    </div>
  )
}

export function ReadinessReportsPanel({
  accessToken,
  canRead,
  canExport,
}: ReadinessReportsPanelProps) {
  const [scopeType, setScopeType] = useState('all')
  const [attentionOnly, setAttentionOnly] = useState(false)

  const summaryQuery = useQuery({
    queryKey: ['staffarr-readiness-report-summary', accessToken, scopeType, attentionOnly],
    queryFn: () =>
      getReadinessReportSummary(accessToken, {
        scopeType: scopeType === 'all' ? undefined : scopeType,
        attentionOnly,
      }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportReadinessReportSummaryCsv(accessToken, {
        scopeType: scopeType === 'all' ? undefined : scopeType,
        attentionOnly,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `staffarr-readiness-report-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  if (!canRead) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5 shadow-[var(--shadow-surface)]"
      data-testid="readiness-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Readiness reports</h2>
          <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
            Org-unit readiness rollups from materialized readiness rollup worker output.
          </p>
        </div>
        {canExport ? (
          <button
            type="button"
            className="rounded-md bg-[var(--color-accent)] px-3 py-1.5 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
            disabled={exportMutation.isPending}
            onClick={() => exportMutation.mutate()}
          >
            {exportMutation.isPending ? 'Exporting…' : 'Export CSV'}
          </button>
        ) : null}
      </div>

      <div className="mt-4 flex flex-wrap gap-4 text-sm text-[var(--color-text-secondary)]">
        <label htmlFor="readiness-reports-scope" className="flex items-center gap-2">
          <span>Scope</span>
          <select
            id="readiness-reports-scope"
            className="rounded-md border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-2 py-1 text-[var(--color-text-primary)]"
            value={scopeType}
            onChange={(event) => setScopeType(event.target.value)}
          >
            <option value="all">All</option>
            <option value="site">Site</option>
            <option value="department">Department</option>
            <option value="team">Team</option>
          </select>
        </label>
        <label htmlFor="readiness-reports-attention-only" className="flex items-center gap-2">
          <input
            id="readiness-reports-attention-only"
            type="checkbox"
            data-testid="readiness-reports-attention-only"
            checked={attentionOnly}
            onChange={(event) => setAttentionOnly(event.target.checked)}
          />
          Attention only (not ready)
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading readiness report summary…</p>
      )}

      {summaryQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Readiness report unavailable"
            message={getErrorMessage(summaryQuery.error, 'Failed to load readiness report summary.')}
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
            message={getErrorMessage(exportMutation.error, 'Unable to export readiness report CSV.')}
          />
        </div>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Rollups in scope" value={String(summaryQuery.data.totalRollups)} />
            <MetricCard label="Total members" value={String(summaryQuery.data.totalMembers)} />
            <MetricCard label="Ready" value={String(summaryQuery.data.readyCount)} />
            <MetricCard label="Not ready" value={String(summaryQuery.data.notReadyCount)} />
            <MetricCard
              label="Ready percent"
              value={`${summaryQuery.data.readyPercent.toFixed(1)}%`}
            />
          </div>

          {summaryQuery.data.recentRollups.length === 0 ? (
            <p className="mt-4 text-sm text-[var(--color-text-muted)]">No readiness rollups match this filter.</p>
          ) : (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b border-[var(--color-border-subtle)] text-left text-[var(--color-text-muted)]">
                    <th className="px-2 py-2">Org unit</th>
                    <th className="px-2 py-2">Scope</th>
                    <th className="px-2 py-2">Not ready</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryQuery.data.recentRollups.slice(0, 8).map((item) => (
                    <tr key={item.rollupId} className="border-b border-[var(--color-border-subtle)]">
                      <td className="px-2 py-2 text-[var(--color-text-primary)]">{item.orgUnitName}</td>
                      <td className="px-2 py-2 text-[var(--color-text-secondary)]">{item.scopeType}</td>
                      <td className="px-2 py-2 text-[var(--color-text-secondary)]">{item.notReadyCount}</td>
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
