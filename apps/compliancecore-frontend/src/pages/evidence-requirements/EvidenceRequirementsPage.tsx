import { useEffect, useMemo, useState } from 'react'
import { PageHeader, DetailEmptyState } from '@stl/shared-ui'
import { useQuery } from '@tanstack/react-query'
import { getTheoreticalEvidenceOptions } from '../../api/client'
import type { TheoreticalEvidenceOptionResponse } from '../../api/types'
import { useComplianceCoreWorkspaceState } from '../../workspace/useComplianceCoreWorkspaceState'

export function EvidenceRequirementsPage() {
  const state = useComplianceCoreWorkspaceState()
  const requirements = state.factRequirementsQuery.data ?? []
  const [selectedRequirementKey, setSelectedRequirementKey] = useState(requirements[0]?.requirementKey ?? '')

  useEffect(() => {
    if (!selectedRequirementKey && requirements.length > 0) {
      setSelectedRequirementKey(requirements[0].requirementKey)
    }
  }, [requirements, selectedRequirementKey])

  const optionsQuery = useQuery({
    queryKey: ['compliancecore-evidence-requirements-options', state.accessToken, selectedRequirementKey],
    queryFn: () => getTheoreticalEvidenceOptions(state.accessToken, selectedRequirementKey || undefined),
    enabled: Boolean(state.accessToken) && Boolean(selectedRequirementKey),
  })

  const selectedRequirement =
    requirements.find((item) => item.requirementKey === selectedRequirementKey) ?? requirements[0] ?? null
  const options = optionsQuery.data ?? []
  const grouped = useMemo(() => groupByRequirement(options), [options])

  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <div className="space-y-6">
      <PageHeader
        title="Evidence requirements"
        subtitle="Inspect how fact requirements map to acceptable evidence paths and proof rules."
      />

      <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_minmax(360px,0.9fr)]">
        <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
          <div className="flex items-center justify-between gap-3">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Requirement catalog</h2>
            <label className="flex items-center gap-2 text-sm text-slate-300">
              Requirement
              <select
                value={selectedRequirementKey}
                onChange={(event) => setSelectedRequirementKey(event.target.value)}
                className="rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm"
              >
                {requirements.map((requirement) => (
                  <option key={requirement.factRequirementId} value={requirement.requirementKey}>
                    {requirement.requirementKey}
                  </option>
                ))}
              </select>
            </label>
          </div>

          {requirements.length === 0 ? (
            <div className="mt-4">
              <DetailEmptyState text="No fact requirements exist yet." />
            </div>
          ) : selectedRequirement ? (
            <div className="mt-4 rounded-xl border border-slate-800 bg-slate-900/60 p-4">
              <div className="font-medium text-slate-100">{selectedRequirement.label}</div>
              <p className="mt-1 text-sm text-slate-300">{selectedRequirement.description}</p>
              <div className="mt-2 flex flex-wrap gap-2 text-xs text-slate-400">
                <span>{selectedRequirement.factKey}</span>
                <span>{selectedRequirement.isRequired ? 'required' : 'optional'}</span>
                <span>{selectedRequirement.citationKey || 'no citation'}</span>
                <span>{selectedRequirement.rulePackKey || 'no rule pack'}</span>
              </div>
            </div>
          ) : null}
        </section>

        <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">
            Acceptable evidence paths
          </h2>
          {optionsQuery.isLoading ? (
            <p className="mt-4 text-sm text-slate-400">Loading evidence options…</p>
          ) : options.length === 0 ? (
            <div className="mt-4">
              <DetailEmptyState text="No evidence options were returned for this requirement." />
            </div>
          ) : (
            <div className="mt-4 space-y-3">
              {(grouped[selectedRequirementKey] ?? options).map((option) => (
                <EvidenceRequirementCard key={option.evidenceOptionId} option={option} />
              ))}
            </div>
          )}
        </section>
      </div>
    </div>
  )
}

function EvidenceRequirementCard({ option }: { option: TheoreticalEvidenceOptionResponse }) {
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
      <div className="mt-3 grid gap-2 text-sm text-slate-300 md:grid-cols-2">
        <div>
          <div className="text-xs text-[var(--color-text-muted)]">Evidence kind</div>
          <div>{option.evidenceKind}</div>
        </div>
        <div>
          <div className="text-xs text-[var(--color-text-muted)]">Target kind</div>
          <div>{option.targetKind}</div>
        </div>
        <div>
          <div className="text-xs text-[var(--color-text-muted)]">Source</div>
          <div>{option.sourceProduct} / {option.sourceEntity}</div>
        </div>
        <div>
          <div className="text-xs text-[var(--color-text-muted)]">Logic</div>
          <div>{option.logicType}</div>
        </div>
      </div>
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
