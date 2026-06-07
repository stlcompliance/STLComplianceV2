import { useMemo, useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, DetailEmptyState, PageHeader, getErrorMessage } from '@stl/shared-ui'
import { exportTitle49CalculatorSummaryCsv, getTitle49CalculatorSummary } from '../../api/client'
import { useComplianceCoreWorkspaceState } from '../../workspace/useComplianceCoreWorkspaceState'

export function RetentionRulesPage() {
  const state = useComplianceCoreWorkspaceState()
  const [sourceProduct, setSourceProduct] = useState('')
  const [sourceEntity, setSourceEntity] = useState('')

  const summaryQuery = useQuery({
    queryKey: ['compliancecore-retention-rules', state.accessToken, sourceProduct, sourceEntity],
    queryFn: () =>
      getTitle49CalculatorSummary(state.accessToken, {
        sourceProduct: sourceProduct || undefined,
        sourceEntity: sourceEntity || undefined,
      }),
    enabled: Boolean(state.accessToken),
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportTitle49CalculatorSummaryCsv(state.accessToken, {
        sourceProduct: sourceProduct || undefined,
        sourceEntity: sourceEntity || undefined,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `compliancecore-retention-rules-${new Date().toISOString().slice(0, 10)}.csv`
      a.click()
      URL.revokeObjectURL(url)
    },
  })

  const requirements = summaryQuery.data?.requirements ?? []
  const calculatorCounts = useMemo(() => {
    const summary = summaryQuery.data
    return summary
      ? [
          ['Total requirements', String(summary.totalRequirements)],
          ['Ready', String(summary.readyCount)],
          ['Review', String(summary.reviewCount)],
          ['Retention duration', String(summary.retentionDurationCount)],
        ]
      : []
  }, [summaryQuery.data])

  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <div className="space-y-6">
      <PageHeader
        title="Retention rules"
        subtitle="Inspect the retention-rule calculator used to classify fact requirements by retention duration and readiness."
      />

      <div className="flex flex-wrap gap-3 rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
        <label className="block text-sm text-slate-300">
          Source product
          <input
            value={sourceProduct}
            onChange={(event) => setSourceProduct(event.target.value)}
            className="mt-1 block rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
          />
        </label>
        <label className="block text-sm text-slate-300">
          Source entity
          <input
            value={sourceEntity}
            onChange={(event) => setSourceEntity(event.target.value)}
            className="mt-1 block rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
          />
        </label>
        <div className="flex items-end">
          <button
            type="button"
            onClick={() => exportMutation.mutate()}
            disabled={exportMutation.isPending}
            className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          >
            {exportMutation.isPending ? 'Exporting…' : 'Export CSV'}
          </button>
        </div>
      </div>

      {summaryQuery.isError ? (
        <ApiErrorCallout
          title="Unable to load retention rules"
          message={getErrorMessage(summaryQuery.error, 'Failed to load retention rule calculator summary.')}
          retryLabel="Retry"
          onRetry={() => void summaryQuery.refetch()}
        />
      ) : null}

      {summaryQuery.data ? (
        <div className="space-y-6">
          <div className="grid gap-3 md:grid-cols-4">
            {calculatorCounts.map(([label, value]) => (
              <div key={label} className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
                <div className="text-xs uppercase tracking-wide text-slate-500">{label}</div>
                <div className="mt-2 text-2xl font-semibold text-slate-100">{value}</div>
              </div>
            ))}
          </div>

          {requirements.length === 0 ? (
            <DetailEmptyState text="No retention-rule calculator rows matched this filter." />
          ) : (
            <div className="space-y-3">
              {requirements.map((item) => (
                <article key={item.factRequirementId} className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <h2 className="font-medium text-slate-100">{item.requirementKey}</h2>
                      <p className="mt-1 text-sm text-slate-400">{item.factKey}</p>
                    </div>
                    <span className="rounded-full border border-slate-700 px-2 py-0.5 text-xs text-slate-400">
                      {item.calculatorKind}
                    </span>
                  </div>
                  <div className="mt-3 grid gap-2 text-sm text-slate-300 md:grid-cols-2 xl:grid-cols-4">
                    <Row label="Pack" value={item.packKey ?? 'n/a'} />
                    <Row label="Citation" value={item.citationKey ?? 'n/a'} />
                    <Row label="Value type" value={item.valueType} />
                    <Row label="Operator" value={item.operator} />
                    <Row label="Expected" value={item.expectedValue} />
                    <Row label="Retention period" value={item.retentionPeriod || 'n/a'} />
                    <Row label="Parsed numeric" value={item.parsedNumericThreshold?.toString() ?? 'n/a'} />
                    <Row label="Parsed days" value={item.parsedRetentionDays?.toString() ?? 'n/a'} />
                  </div>
                  <p className="mt-3 text-sm text-slate-400">
                    {item.isReady ? 'Ready for downstream compliance use.' : 'Needs review before downstream use.'}
                  </p>
                </article>
              ))}
            </div>
          )}
        </div>
      ) : (
        <p className="text-sm text-slate-400">Loading retention-rule calculator summary…</p>
      )}
    </div>
  )
}

function Row({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <div className="text-xs text-slate-500">{label}</div>
      <div className="mt-1">{value}</div>
    </div>
  )
}
