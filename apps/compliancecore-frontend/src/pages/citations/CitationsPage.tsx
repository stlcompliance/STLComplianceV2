import { useMemo, useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ApiErrorCallout, DetailEmptyState, PageHeader, getErrorMessage } from '@stl/shared-ui'
import { createCitation } from '../../api/client'
import type { RegulatoryCitationResponse } from '../../api/types'
import { useComplianceCoreWorkspaceState } from '../../workspace/useComplianceCoreWorkspaceState'

export function CitationsPage() {
  const state = useComplianceCoreWorkspaceState()
  const queryClient = useQueryClient()
  const programs = state.programsQuery.data ?? []
  const rulePacks = state.rulePacksQuery.data ?? []
  const citations = state.citationsQuery.data ?? []
  const [regulatoryProgramId, setRegulatoryProgramId] = useState('')
  const [rulePackId, setRulePackId] = useState('')
  const [citationKey, setCitationKey] = useState('')
  const [label, setLabel] = useState('')
  const [sourceReference, setSourceReference] = useState('')
  const [description, setDescription] = useState('')

  const selectedProgram = useMemo(
    () => programs.find((program) => program.regulatoryProgramId === regulatoryProgramId) ?? programs[0] ?? null,
    [programs, regulatoryProgramId],
  )

  const filteredCitations = useMemo(
    () =>
      citations.filter((item) => {
        const programMatch = selectedProgram ? item.regulatoryProgramId === selectedProgram.regulatoryProgramId : true
        const packMatch = rulePackId ? item.rulePackId === rulePackId : true
        return programMatch && packMatch
      }),
    [citations, selectedProgram, rulePackId],
  )

  const createMutation = useMutation({
    mutationFn: () =>
      createCitation(state.accessToken, {
        regulatoryProgramId: selectedProgram?.regulatoryProgramId ?? programs[0]?.regulatoryProgramId ?? '',
        rulePackId: rulePackId || null,
        citationKey,
        label,
        sourceReference,
        description,
      }),
    onSuccess: async () => {
      setCitationKey('')
      setLabel('')
      setSourceReference('')
      setDescription('')
      await queryClient.invalidateQueries({ queryKey: ['compliancecore-citations'] })
    },
  })

  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <div className="space-y-6">
      <PageHeader
        title="Citations"
        subtitle="Maintain the canonical citation catalog and its links to programs, rule packs, and requirements."
      />

      <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_minmax(360px,0.9fr)]">
        <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Citation catalog</h2>
            <div className="flex flex-wrap items-center gap-2 text-sm text-slate-300">
              <select
                value={regulatoryProgramId}
                onChange={(event) => setRegulatoryProgramId(event.target.value)}
                className="rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm"
              >
                <option value="">All programs</option>
                {programs.map((program) => (
                  <option key={program.regulatoryProgramId} value={program.regulatoryProgramId}>
                    {program.label}
                  </option>
                ))}
              </select>
              <select
                value={rulePackId}
                onChange={(event) => setRulePackId(event.target.value)}
                className="rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm"
              >
                <option value="">All rule packs</option>
                {rulePacks.map((pack) => (
                  <option key={pack.rulePackId} value={pack.rulePackId}>
                    {pack.packKey}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {state.citationsQuery.isError ? (
            <div className="mt-3">
              <ApiErrorCallout
                title="Unable to load citations"
                message={getErrorMessage(state.citationsQuery.error, 'Failed to load citations.')}
                retryLabel="Retry"
                onRetry={() => void state.citationsQuery.refetch()}
              />
            </div>
          ) : null}

          {filteredCitations.length === 0 ? (
            <div className="mt-4">
              <DetailEmptyState text="No citations match this filter." />
            </div>
          ) : (
            <ul className="mt-4 space-y-2">
              {filteredCitations.map((citation) => (
                <CitationRow key={citation.citationId} citation={citation} />
              ))}
            </ul>
          )}
        </section>

        <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Create citation</h2>
          <div className="mt-4 space-y-3">
            <label className="block text-sm text-slate-300">
              Regulatory program
              <select
                value={selectedProgram?.regulatoryProgramId ?? ''}
                onChange={(event) => setRegulatoryProgramId(event.target.value)}
                className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm"
              >
                <option value="">Select program…</option>
                {programs.map((program) => (
                  <option key={program.regulatoryProgramId} value={program.regulatoryProgramId}>
                    {program.label}
                  </option>
                ))}
              </select>
            </label>
            <label className="block text-sm text-slate-300">
              Rule pack
              <select
                value={rulePackId}
                onChange={(event) => setRulePackId(event.target.value)}
                className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm"
              >
                <option value="">Optional</option>
                {rulePacks.map((pack) => (
                  <option key={pack.rulePackId} value={pack.rulePackId}>
                    {pack.packKey}
                  </option>
                ))}
              </select>
            </label>
            <Field label="Citation key" value={citationKey} onChange={setCitationKey} />
            <Field label="Label" value={label} onChange={setLabel} />
            <Field label="Source reference" value={sourceReference} onChange={setSourceReference} />
            <label className="block text-sm text-slate-300">
              Description
              <textarea
                value={description}
                onChange={(event) => setDescription(event.target.value)}
                rows={4}
                className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
              />
            </label>
            <button
              type="button"
              onClick={() => createMutation.mutate()}
              disabled={createMutation.isPending || !selectedProgram}
              className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
            >
              {createMutation.isPending ? 'Creating…' : 'Create citation'}
            </button>
            {createMutation.isError ? (
              <ApiErrorCallout
                title="Create failed"
                message={getErrorMessage(createMutation.error, 'Failed to create citation.')}
              />
            ) : null}
          </div>
        </section>
      </div>
    </div>
  )
}

function Field({
  label,
  value,
  onChange,
}: {
  label: string
  value: string
  onChange: (value: string) => void
}) {
  return (
    <label className="block text-sm text-slate-300">
      {label}
      <input
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
      />
    </label>
  )
}

function CitationRow({ citation }: { citation: RegulatoryCitationResponse }) {
  return (
    <li className="rounded-xl border border-slate-800 bg-slate-900/60 px-4 py-3">
      <div className="font-medium text-slate-100">{citation.label}</div>
      <div className="mt-1 text-xs text-[var(--color-text-muted)]">{citation.citationKey}</div>
      <div className="mt-1 text-sm text-slate-300">{citation.sourceReference}</div>
      <p className="mt-2 text-sm text-slate-400">{citation.description || 'No description provided.'}</p>
    </li>
  )
}
