import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import {
  exportComplianceReportSummaryCsv,
  getComplianceReportSummary,
} from '../api/client'

interface ComplianceReportsPanelProps {
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

export function ComplianceReportsPanel({
  accessToken,
  canRead,
  canExport,
}: ComplianceReportsPanelProps) {
  const [attentionOnly, setAttentionOnly] = useState(false)

  const summaryQuery = useQuery({
    queryKey: ['trainarr-compliance-report-summary', accessToken, attentionOnly],
    queryFn: () => getComplianceReportSummary(accessToken, { attentionOnly }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () => exportComplianceReportSummaryCsv(accessToken, { attentionOnly }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `trainarr-compliance-report-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  if (!canRead) {
    return null
  }

  return (
    <section
      className="mt-6 rounded-xl border border-border bg-card p-5"
      data-testid="compliance-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-foreground">Compliance reports</h2>
          <p className="mt-1 text-sm text-muted-foreground">
            Citation attachments, rule-pack requirements, and incident remediation attention items.
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
        <label className="flex items-center gap-2">
          <input
            type="checkbox"
            checked={attentionOnly}
            onChange={(event) => setAttentionOnly(event.target.checked)}
          />
          Attention only
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-muted-foreground">Loading compliance report summary…</p>
      )}

      {summaryQuery.isError && (
        <p className="mt-3 text-sm text-destructive">Failed to load compliance report summary.</p>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Citation attachments" value={String(summaryQuery.data.citationAttachmentCount)} />
            <MetricCard label="Rule pack requirements" value={String(summaryQuery.data.rulePackRequirementCount)} />
            <MetricCard label="Open remediations" value={String(summaryQuery.data.openRemediationCount)} />
            <MetricCard label="Attention items" value={String(summaryQuery.data.attentionItemCount)} />
          </div>

          {summaryQuery.data.recentRemediations.length === 0 ? (
            <p className="mt-4 text-sm text-muted-foreground">No remediations match this filter.</p>
          ) : (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b border-border text-left text-muted-foreground">
                    <th className="px-2 py-2">Reason</th>
                    <th className="px-2 py-2">Status</th>
                    <th className="px-2 py-2">Updated</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryQuery.data.recentRemediations.slice(0, 8).map((item) => (
                    <tr key={item.remediationId} className="border-b border-border/60">
                      <td className="px-2 py-2">{item.reasonCategoryKey}</td>
                      <td className="px-2 py-2">{item.status}</td>
                      <td className="px-2 py-2">{new Date(item.updatedAt).toLocaleDateString()}</td>
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
