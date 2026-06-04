import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  exportCitationReviewReportSummaryCsv,
  getCitationReviewReportSummary,
} from '../api/client'

interface CitationReviewReportsPanelProps {
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

function reviewStateBadgeClass(state: string): string {
  switch (state) {
    case 'reviewed':
      return 'bg-emerald-900/50 text-emerald-300'
    case 'needs_review':
      return 'bg-amber-900/50 text-amber-300'
    case 'superseded':
      return 'bg-rose-900/50 text-rose-300'
    default:
      return 'bg-slate-800 text-slate-300'
  }
}

export function CitationReviewReportsPanel({
  accessToken,
  canRead,
  canExport,
}: CitationReviewReportsPanelProps) {
  const [reviewState, setReviewState] = useState('all')
  const [programKey, setProgramKey] = useState('')
  const [rulePackKey, setRulePackKey] = useState('')
  const [limit, setLimit] = useState('10')

  const summaryQuery = useQuery({
    queryKey: [
      'compliancecore-citation-review-report-summary',
      accessToken,
      reviewState,
      programKey,
      rulePackKey,
      limit,
    ],
    queryFn: () =>
      getCitationReviewReportSummary(accessToken, {
        reviewState: reviewState === 'all' ? undefined : reviewState,
        programKey: programKey.trim() || undefined,
        rulePackKey: rulePackKey.trim() || undefined,
        limit: Number.parseInt(limit, 10) || 10,
      }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportCitationReviewReportSummaryCsv(accessToken, {
        reviewState: reviewState === 'all' ? undefined : reviewState,
        programKey: programKey.trim() || undefined,
        rulePackKey: rulePackKey.trim() || undefined,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `compliancecore-citation-review-report-${new Date().toISOString().slice(0, 10)}.csv`
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
      data-testid="citation-review-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Citation review report</h2>
          <p className="mt-1 text-sm text-slate-400">
            Review-state rollups for regulatory citations with supersession, rule-pack, evidence,
            and mapping traceability.
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
        <label htmlFor="citation-review-state" className="flex items-center gap-2">
          Review state
          <select
            id="citation-review-state"
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={reviewState}
            onChange={(event) => setReviewState(event.target.value)}
          >
            <option value="all">All</option>
            <option value="reviewed">Reviewed</option>
            <option value="needs_review">Needs review</option>
            <option value="inactive">Inactive</option>
            <option value="superseded">Superseded</option>
          </select>
        </label>
        <label htmlFor="citation-review-program" className="flex items-center gap-2">
          Program
          <input
            id="citation-review-program"
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={programKey}
            onChange={(event) => setProgramKey(event.target.value)}
            placeholder="optional program key"
          />
        </label>
        <label htmlFor="citation-review-rule-pack" className="flex items-center gap-2">
          Rule pack
          <input
            id="citation-review-rule-pack"
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={rulePackKey}
            onChange={(event) => setRulePackKey(event.target.value)}
            placeholder="optional pack key"
          />
        </label>
        <label htmlFor="citation-review-limit" className="flex items-center gap-2">
          Limit
          <input
            id="citation-review-limit"
            type="number"
            min={1}
            max={100}
            value={limit}
            onChange={(event) => setLimit(event.target.value)}
            className="w-20 rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
          />
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-400">Loading citation review report…</p>
      )}

      {summaryQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Citation review report unavailable"
            message={getErrorMessage(
              summaryQuery.error,
              'Failed to load citation review report summary.',
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
              'Unable to export citation review report CSV.',
            )}
          />
        </div>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Citations" value={String(summaryQuery.data.totalCitations)} />
            <MetricCard label="Reviewed" value={String(summaryQuery.data.reviewedCitationCount)} />
            <MetricCard
              label="Needs review"
              value={String(summaryQuery.data.needsReviewCitationCount)}
            />
            <MetricCard label="Superseded" value={String(summaryQuery.data.supersededCitationCount)} />
          </div>

          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Active" value={String(summaryQuery.data.activeCitationCount)} />
            <MetricCard label="Inactive" value={String(summaryQuery.data.inactiveCitationCount)} />
            <MetricCard label="Rule packs" value={String(summaryQuery.data.linkedRulePackCount)} />
            <MetricCard label="Mappings" value={String(summaryQuery.data.totalMappingCount)} />
          </div>

          {summaryQuery.data.citations.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">
              No citation review items match this filter.
            </p>
          ) : (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b border-slate-700 text-left text-slate-400">
                    <th className="px-2 py-2">Citation</th>
                    <th className="px-2 py-2">Review state</th>
                    <th className="px-2 py-2">Links</th>
                    <th className="px-2 py-2">Updated</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryQuery.data.citations.map((item) => (
                    <tr
                      key={item.citationId}
                      className="border-b border-slate-800/60 align-top"
                    >
                      <td className="px-2 py-2 text-slate-100">
                        <div className="font-medium">{item.citationKey}</div>
                        <div className="text-xs text-slate-500">{item.citationLabel}</div>
                        <div className="text-xs text-slate-500">{item.sourceReference}</div>
                        <div className="mt-1 text-xs text-slate-500">
                          {item.programKey} · v{item.versionNumber}
                        </div>
                      </td>
                      <td className="px-2 py-2 text-slate-300">
                        <div className="flex flex-wrap items-center gap-2">
                          <span
                            className={`rounded px-2 py-0.5 text-xs ${reviewStateBadgeClass(item.reviewState)}`}
                          >
                            {item.reviewState}
                          </span>
                          {item.hasRulePack ? (
                            <span className="rounded bg-slate-800 px-2 py-0.5 text-xs text-slate-300">
                              {item.rulePackKey}
                            </span>
                          ) : null}
                        </div>
                        <div className="mt-1 text-xs text-slate-500">{item.summary}</div>
                      </td>
                      <td className="px-2 py-2 text-slate-300">
                        <div>{item.factRequirementCount} fact requirement(s)</div>
                        <div>{item.mappingCount} mapping(s)</div>
                        <div>{item.supersededByCount} superseder(s)</div>
                        {item.supersedesCitationKey ? (
                          <div className="text-xs text-slate-500">
                            Supersedes {item.supersedesCitationKey}
                          </div>
                        ) : null}
                      </td>
                      <td className="px-2 py-2 text-slate-300">
                        {new Date(item.updatedAt).toLocaleString()}
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
