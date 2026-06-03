import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import {
  exportExceptionExemptionReportSummaryCsv,
  getExceptionExemptionReportSummary,
} from '../api/client'

interface ExceptionExemptionReportsPanelProps {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2">
      <p className="text-xs text-slate-400">{label}</p>
      <p className="text-lg font-semibold text-slate-50">{value}</p>
    </div>
  )
}

export function ExceptionExemptionReportsPanel({
  accessToken,
  canRead,
  canExport,
}: ExceptionExemptionReportsPanelProps) {
  const [type, setType] = useState('all')
  const [effectType, setEffectType] = useState('all')
  const [activeOnly, setActiveOnly] = useState(false)

  const summaryQuery = useQuery({
    queryKey: ['compliancecore-exception-exemption-report-summary', accessToken, type, effectType, activeOnly],
    queryFn: () =>
      getExceptionExemptionReportSummary(accessToken, {
        type: type === 'all' ? undefined : type,
        effectType: effectType === 'all' ? undefined : effectType,
        activeOnly,
      }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportExceptionExemptionReportSummaryCsv(accessToken, {
        type: type === 'all' ? undefined : type,
        effectType: effectType === 'all' ? undefined : effectType,
        activeOnly,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `compliancecore-exception-exemption-report-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  if (!canRead) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="exception-exemption-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Exception exemption reports</h2>
          <p className="mt-1 text-sm text-slate-400">
            Regulatory exceptions, waivers, variances, and related approval lifecycles.
          </p>
        </div>
        {canExport ? (
          <button
            type="button"
            className="rounded-md bg-sky-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
            disabled={exportMutation.isPending}
            onClick={() => exportMutation.mutate()}
          >
            {exportMutation.isPending ? 'Exporting…' : 'Export CSV'}
          </button>
        ) : null}
      </div>

      <div className="mt-4 flex flex-wrap gap-4 text-sm text-slate-300">
        <label htmlFor="exception-exemption-reports-type" className="flex items-center gap-2">
          Type
          <select
            id="exception-exemption-reports-type"
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={type}
            onChange={(event) => setType(event.target.value)}
          >
            <option value="all">All</option>
            <option value="regulatory_exception">Regulatory exception</option>
            <option value="regulatory_exemption">Regulatory exemption</option>
            <option value="waiver">Waiver</option>
            <option value="variance">Variance</option>
            <option value="special_permit">Special permit</option>
            <option value="approval">Approval</option>
          </select>
        </label>
        <label htmlFor="exception-exemption-reports-effect" className="flex items-center gap-2">
          Effect
          <select
            id="exception-exemption-reports-effect"
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={effectType}
            onChange={(event) => setEffectType(event.target.value)}
          >
            <option value="all">All</option>
            <option value="makes_requirement_not_applicable">Not applicable</option>
            <option value="authorizes_otherwise_blocked_action">Authorize blocked action</option>
            <option value="changes_expected_value">Changes expected value</option>
            <option value="changes_required_evidence">Changes required evidence</option>
            <option value="allows_alternate_evidence">Allows alternate evidence</option>
            <option value="reduces_requirement">Reduces requirement</option>
            <option value="extends_deadline">Extends deadline</option>
          </select>
        </label>
        <label htmlFor="exception-exemption-reports-active-only" className="flex items-center gap-2">
          <input
            id="exception-exemption-reports-active-only"
            type="checkbox"
            checked={activeOnly}
            onChange={(event) => setActiveOnly(event.target.checked)}
          />
          Active only
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-400">Loading exception exemption report summary…</p>
      )}

      {summaryQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Exception exemption report unavailable"
            message={getErrorMessage(
              summaryQuery.error,
              'Failed to load exception exemption report summary.',
            )}
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
            message={getErrorMessage(
              exportMutation.error,
              'Unable to export exception exemption report CSV.',
            )}
          />
        </div>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard
              label="Exemptions in scope"
              value={String(summaryQuery.data.totalExceptionExemptions)}
            />
            <MetricCard label="Active" value={String(summaryQuery.data.activeCount)} />
            <MetricCard label="Inactive" value={String(summaryQuery.data.inactiveCount)} />
            <MetricCard label="Expiring soon" value={String(summaryQuery.data.expiringSoonCount)} />
          </div>

          {summaryQuery.data.recentExceptionExemptions.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">No exception exemptions match this filter.</p>
          ) : (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b border-slate-700 text-left text-slate-400">
                    <th className="px-2 py-2">Exception</th>
                    <th className="px-2 py-2">Pack</th>
                    <th className="px-2 py-2">Type</th>
                    <th className="px-2 py-2">State</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryQuery.data.recentExceptionExemptions.slice(0, 8).map((item) => (
                    <tr key={item.exceptionExemptionId} className="border-b border-slate-800/60">
                      <td className="px-2 py-2 text-slate-100">
                        <div>{item.label}</div>
                        <div className="text-xs text-slate-500">
                          {item.key}
                          {item.citationKey ? ` · ${item.citationKey}` : ''}
                        </div>
                      </td>
                      <td className="px-2 py-2 text-slate-300">{item.packKey || 'Tenant-wide'}</td>
                      <td className="px-2 py-2 text-slate-300">{item.type}</td>
                      <td className="px-2 py-2 text-slate-300">{item.activeState}</td>
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
