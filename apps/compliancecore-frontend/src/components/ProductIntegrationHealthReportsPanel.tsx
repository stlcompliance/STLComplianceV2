import { useMutation, useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  exportProductIntegrationHealthReportSummaryCsv,
  getProductIntegrationHealthReportSummary,
} from '../api/client'

interface ProductIntegrationHealthReportsPanelProps {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

function healthBadgeClass(status: string): string {
  switch (status) {
    case 'healthy':
      return 'bg-emerald-900/50 text-emerald-300'
    case 'stale':
      return 'bg-amber-900/50 text-amber-300'
    case 'failed':
      return 'bg-rose-900/50 text-rose-300'
    default:
      return 'bg-slate-800 text-slate-400'
  }
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2">
      <p className="text-xs text-slate-400">{label}</p>
      <p className="text-lg font-semibold text-slate-50">{value}</p>
    </div>
  )
}

export function ProductIntegrationHealthReportsPanel({
  accessToken,
  canRead,
  canExport,
}: ProductIntegrationHealthReportsPanelProps) {
  const summaryQuery = useQuery({
    queryKey: ['compliancecore-product-integration-health-report-summary', accessToken],
    queryFn: () => getProductIntegrationHealthReportSummary(accessToken),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () => exportProductIntegrationHealthReportSummaryCsv(accessToken),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `compliancecore-product-integration-health-report-${new Date()
        .toISOString()
        .slice(0, 10)}.csv`
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
      data-testid="product-integration-health-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Product integration health report</h2>
          <p className="mt-1 text-sm text-slate-400">
            Product API sync health by source, including freshness, failure counts, and the last
            known success or failure.
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

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-400">Loading product integration health report…</p>
      )}

      {summaryQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Product integration health report unavailable"
            message={getErrorMessage(
              summaryQuery.error,
              'Failed to load product integration health report summary.',
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
              'Unable to export product integration health report CSV.',
            )}
          />
        </div>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard
              label="Product API sources"
              value={String(summaryQuery.data.productApiSourceCount)}
            />
            <MetricCard label="Healthy" value={String(summaryQuery.data.healthyCount)} />
            <MetricCard label="Stale / failed" value={String(summaryQuery.data.staleCount + summaryQuery.data.failedCount)} />
            <MetricCard
              label="Worker"
              value={summaryQuery.data.workerEnabled ? 'Enabled' : 'Disabled'}
            />
          </div>

          {summaryQuery.data.sources.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">No product integration health data is available yet.</p>
          ) : (
            <div className="mt-4 space-y-2">
              {summaryQuery.data.sources.map((item) => (
                <div
                  key={item.factSourceId}
                  className="rounded-lg border border-slate-700 bg-slate-950/60 p-3"
                >
                  <div className="flex items-start justify-between gap-2">
                    <p className="font-medium text-slate-100">{item.sourceKey}</p>
                    <span className={`rounded px-2 py-0.5 text-xs ${healthBadgeClass(item.healthStatus)}`}>
                      {item.healthStatus}
                    </span>
                  </div>
                  <p className="font-mono text-xs text-violet-300">{item.factKey}</p>
                  <p className="mt-1 text-xs text-slate-500">
                    {item.productKey ?? 'product'} · scope {item.scopeKey}
                  </p>
                  <div className="mt-2 grid gap-2 text-xs text-slate-500 sm:grid-cols-2">
                    <p>Last success {item.lastSuccessAt ? new Date(item.lastSuccessAt).toLocaleString() : 'never'}</p>
                    <p>Last failure {item.lastFailureAt ? new Date(item.lastFailureAt).toLocaleString() : 'never'}</p>
                  </div>
                  {item.lastErrorMessage ? (
                    <p className="mt-2 text-xs text-rose-300">{item.lastErrorMessage}</p>
                  ) : null}
                </div>
              ))}
            </div>
          )}
        </>
      )}
    </section>
  )
}
