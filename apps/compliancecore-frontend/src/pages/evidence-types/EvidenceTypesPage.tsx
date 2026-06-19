import { useMemo, useState } from 'react'
import { PageHeader, DetailEmptyState } from '@stl/shared-ui'
import { useQuery } from '@tanstack/react-query'
import { getTheoreticalEvidenceOptions } from '../../api/client'
import type { TheoreticalEvidenceOptionResponse } from '../../api/types'
import { useComplianceCoreWorkspaceState } from '../../workspace/useComplianceCoreWorkspaceState'

export function EvidenceTypesPage() {
  const state = useComplianceCoreWorkspaceState()
  const [requirementKey, setRequirementKey] = useState('')
  const evidenceOptionsQuery = useQuery({
    queryKey: ['compliancecore-theoretical-evidence-options', state.accessToken, requirementKey],
    queryFn: () => getTheoreticalEvidenceOptions(state.accessToken, requirementKey || undefined),
    enabled: Boolean(state.accessToken),
  })

  const options = evidenceOptionsQuery.data ?? []
  const grouped = useMemo(() => groupByRequirement(options), [options])

  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <div className="space-y-6">
      <PageHeader
        title="Evidence types"
        subtitle="Inspect the evidence-option catalog used by theoretical situation evaluation and import mapping."
      />

      <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
        <label className="block text-sm text-slate-300">
          Requirement key filter
          <input
            value={requirementKey}
            onChange={(event) => setRequirementKey(event.target.value)}
            placeholder="Leave blank for all requirements"
            className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
          />
        </label>
      </div>

      {options.length === 0 ? (
        <DetailEmptyState text="No evidence options were returned for this filter." />
      ) : (
        <div className="space-y-4">
          {Object.entries(grouped).map(([reqKey, rows]) => (
            <section key={reqKey} className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
              <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">{reqKey}</h2>
              <div className="mt-3 grid gap-3 md:grid-cols-2 xl:grid-cols-3">
                {rows.map((option) => (
                  <EvidenceOptionCard key={option.evidenceOptionId} option={option} />
                ))}
              </div>
            </section>
          ))}
        </div>
      )}
    </div>
  )
}

function EvidenceOptionCard({ option }: { option: TheoreticalEvidenceOptionResponse }) {
  return (
    <article className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
      <div className="flex items-start justify-between gap-3">
        <div>
          <h3 className="font-medium text-slate-100">{option.evidenceOptionLabel}</h3>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">{option.evidenceOptionKey}</p>
        </div>
        <span className="rounded-full border border-slate-700 px-2 py-0.5 text-xs text-slate-400">
          {option.required ? 'required' : 'optional'}
        </span>
      </div>
      <dl className="mt-3 space-y-1 text-sm text-slate-300">
        <div className="flex gap-2">
          <dt className="text-[var(--color-text-muted)]">Kind</dt>
          <dd>{option.evidenceKind}</dd>
        </div>
        <div className="flex gap-2">
          <dt className="text-[var(--color-text-muted)]">Target</dt>
          <dd>{option.targetKind}</dd>
        </div>
        <div className="flex gap-2">
          <dt className="text-[var(--color-text-muted)]">Logic</dt>
          <dd>{option.logicType}</dd>
        </div>
        <div className="flex gap-2">
          <dt className="text-[var(--color-text-muted)]">Source</dt>
          <dd>{option.sourceProduct} / {option.sourceEntity}</dd>
        </div>
      </dl>
    </article>
  )
}

function groupByRequirement(items: TheoreticalEvidenceOptionResponse[]) {
  return items.reduce<Record<string, TheoreticalEvidenceOptionResponse[]>>((acc, item) => {
    const key = item.requirementKey || 'Unassigned requirements'
    ;(acc[key] ??= []).push(item)
    return acc
  }, {})
}
