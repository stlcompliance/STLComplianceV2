import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { exportComplianceReportSummaryCsv, getComplianceReportSummary } from '../api/client'

interface ComplianceReportsPanelProps {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

export function ComplianceReportsPanel({
  accessToken,
  canRead,
  canExport,
}: ComplianceReportsPanelProps) {
  const [attentionOnly, setAttentionOnly] = useState(false)
  const [siteRef, setSiteRef] = useState('')

  const summaryQuery = useQuery({
    queryKey: ['maintainarr-compliance-report-summary', accessToken, attentionOnly, siteRef],
    queryFn: () =>
      getComplianceReportSummary(accessToken, {
        attentionOnly: attentionOnly || undefined,
        siteRef: siteRef || undefined,
      }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportComplianceReportSummaryCsv(accessToken, {
        attentionOnly: attentionOnly || undefined,
        siteRef: siteRef || undefined,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `maintainarr-compliance-report-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  if (!canRead) {
    return null
  }

  const inspection = summaryQuery.data?.inspectionTotals
  const defects = summaryQuery.data?.defectTotals
  const pm = summaryQuery.data?.pmAdherenceTotals

  return (
    <section
      className="mt-6 rounded-xl border border-emerald-800/40 bg-emerald-950/20 p-5"
      data-testid="compliance-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Compliance reports</h2>
          <p className="mt-1 text-sm text-slate-400">
            Inspection pass rates, defect severity, PM adherence, and regulatory key mirrors.
          </p>
        </div>
        {canExport ? (
          <button
            type="button"
            className="rounded-md bg-emerald-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-emerald-600 disabled:opacity-50"
            disabled={exportMutation.isPending}
            onClick={() => exportMutation.mutate()}
          >
            {exportMutation.isPending ? 'Exporting…' : 'Export CSV'}
          </button>
        ) : null}
      </div>

      <div className="mt-4 flex flex-wrap gap-4 text-sm text-slate-300">
        <label className="flex items-center gap-2">
          <input id="compliancereports"
            type="checkbox"
            checked={attentionOnly}
            onChange={(event) => setAttentionOnly(event.target.checked)}
          />
          Attention only
        </label>
        <label className="flex items-center gap-2" htmlFor="compliancereports-site">
          Site
          <input id="compliancereports-site"
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={siteRef}
            onChange={(event) => setSiteRef(event.target.value)}
            placeholder="Optional site ref"
          />
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-500">Loading compliance report…</p>
      )}

      {summaryQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Compliance report unavailable"
            message={getErrorMessage(summaryQuery.error, 'Failed to load compliance report.')}
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
            message={getErrorMessage(exportMutation.error, 'Unable to export compliance report CSV.')}
          />
        </div>
      )}

      {summaryQuery.data && inspection && defects && pm && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Inspection pass %" value={`${inspection.passRatePercent.toFixed(1)}%`} />
            <MetricCard label="Failed inspections" value={String(inspection.failedRuns)} />
            <MetricCard
              label="Open critical / high"
              value={`${defects.openCriticalCount} / ${defects.openHighCount}`}
            />
            <MetricCard label="PM adherence %" value={`${pm.adherencePercent.toFixed(1)}%`} />
            <MetricCard label="PM overdue" value={String(pm.overdueCount)} />
            <MetricCard
              label="Regulatory key mirrors"
              value={String(summaryQuery.data.regulatoryKeyMirrorCount)}
            />
            <MetricCard
              label="Inspection-sourced defects"
              value={String(defects.inspectionSourcedOpenCount)}
            />
            <MetricCard label="Attention items" value={String(summaryQuery.data.attentionItems.length)} />
          </div>

          {summaryQuery.data.regulatoryKeyGroups.length > 0 ? (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-left text-sm">
                <thead className="text-xs uppercase text-slate-500">
                  <tr>
                    <th className="px-2 py-2">Compliance key</th>
                    <th className="px-2 py-2">Material</th>
                    <th className="px-2 py-2">Subjects</th>
                    <th className="px-2 py-2">Open issues</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryQuery.data.regulatoryKeyGroups.map((group) => (
                    <tr key={`${group.complianceKey}-${group.materialKey ?? ''}`} className="border-t border-slate-800">
                      <td className="px-2 py-2 text-slate-100">{group.complianceKey}</td>
                      <td className="px-2 py-2 text-slate-400">{group.materialKey ?? '—'}</td>
                      <td className="px-2 py-2 text-slate-300">{group.linkedSubjectCount}</td>
                      <td className="px-2 py-2 text-slate-300">{group.openComplianceIssueCount}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : null}

          {summaryQuery.data.attentionItems.length > 0 ? (
            <ul className="mt-4 space-y-2 text-sm text-slate-300">
              {summaryQuery.data.attentionItems.slice(0, 10).map((item) => (
                <li key={`${item.assetId}-${item.issueType}`}>
                  {item.assetTag} — {item.message}
                </li>
              ))}
            </ul>
          ) : null}
        </>
      )}
    </section>
  )
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-slate-700 bg-slate-950/50 px-3 py-2">
      <p className="text-xs uppercase tracking-wide text-slate-500">{label}</p>
      <p className="mt-1 text-lg font-semibold text-slate-100">{value}</p>
    </div>
  )
}
