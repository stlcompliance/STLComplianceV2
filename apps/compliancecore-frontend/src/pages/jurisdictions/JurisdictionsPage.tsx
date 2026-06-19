import { useMemo, useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ApiErrorCallout, DetailEmptyState, PageHeader, getErrorMessage } from '@stl/shared-ui'
import { createJurisdiction } from '../../api/client'
import type { JurisdictionResponse } from '../../api/types'
import { useComplianceCoreWorkspaceState } from '../../workspace/useComplianceCoreWorkspaceState'

export function JurisdictionsPage() {
  const state = useComplianceCoreWorkspaceState()
  const queryClient = useQueryClient()
  const bodies = state.governingBodiesQuery.data ?? []
  const jurisdictions = state.jurisdictionsQuery.data ?? []
  const [governingBodyId, setGoverningBodyId] = useState('')
  const [jurisdictionKey, setJurisdictionKey] = useState('')
  const [label, setLabel] = useState('')
  const [description, setDescription] = useState('')

  const selectedBody = useMemo(
    () => bodies.find((body) => body.governingBodyId === governingBodyId) ?? bodies[0] ?? null,
    [bodies, governingBodyId],
  )

  const filteredJurisdictions = useMemo(
    () =>
      jurisdictions.filter((item) =>
        selectedBody ? item.governingBodyId === selectedBody.governingBodyId : true,
      ),
    [jurisdictions, selectedBody],
  )

  const createMutation = useMutation({
    mutationFn: () =>
      createJurisdiction(state.accessToken, {
        governingBodyId: selectedBody?.governingBodyId ?? bodies[0]?.governingBodyId ?? '',
        jurisdictionKey,
        label,
        description,
      }),
    onSuccess: async () => {
      setJurisdictionKey('')
      setLabel('')
      setDescription('')
      await queryClient.invalidateQueries({ queryKey: ['compliancecore-jurisdictions'] })
    },
  })

  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <div className="space-y-6">
      <PageHeader
        title="Jurisdictions"
        subtitle="Maintain regulatory jurisdiction definitions scoped under each governing body."
      />

      <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_minmax(360px,0.9fr)]">
        <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
          <div className="flex items-center justify-between gap-3">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Jurisdiction catalog</h2>
            <label className="flex items-center gap-2 text-sm text-slate-300">
              Governing body
              <select
                value={governingBodyId}
                onChange={(event) => setGoverningBodyId(event.target.value)}
                className="rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm"
              >
                <option value="">All</option>
                {bodies.map((body) => (
                  <option key={body.governingBodyId} value={body.governingBodyId}>
                    {body.label}
                  </option>
                ))}
              </select>
            </label>
          </div>

          {state.jurisdictionsQuery.isError ? (
            <div className="mt-3">
              <ApiErrorCallout
                title="Unable to load jurisdictions"
                message={getErrorMessage(state.jurisdictionsQuery.error, 'Failed to load jurisdictions.')}
                retryLabel="Retry"
                onRetry={() => void state.jurisdictionsQuery.refetch()}
              />
            </div>
          ) : null}

          {filteredJurisdictions.length === 0 ? (
            <div className="mt-4">
              <DetailEmptyState text="No jurisdictions match this filter." />
            </div>
          ) : (
            <ul className="mt-4 space-y-2">
              {filteredJurisdictions.map((jurisdiction) => (
                <JurisdictionRow key={jurisdiction.jurisdictionId} jurisdiction={jurisdiction} />
              ))}
            </ul>
          )}
        </section>

        <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Create jurisdiction</h2>
          <div className="mt-4 space-y-3">
            <label className="block text-sm text-slate-300">
              Governing body
              <select
                value={selectedBody?.governingBodyId ?? ''}
                onChange={(event) => setGoverningBodyId(event.target.value)}
                className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm"
              >
                <option value="">Select governing body…</option>
                {bodies.map((body) => (
                  <option key={body.governingBodyId} value={body.governingBodyId}>
                    {body.label}
                  </option>
                ))}
              </select>
            </label>
            <Field label="Jurisdiction key" value={jurisdictionKey} onChange={setJurisdictionKey} />
            <Field label="Label" value={label} onChange={setLabel} />
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
              disabled={createMutation.isPending || !selectedBody}
              className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
            >
              {createMutation.isPending ? 'Creating…' : 'Create jurisdiction'}
            </button>
            {createMutation.isError ? (
              <ApiErrorCallout
                title="Create failed"
                message={getErrorMessage(createMutation.error, 'Failed to create jurisdiction.')}
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

function JurisdictionRow({ jurisdiction }: { jurisdiction: JurisdictionResponse }) {
  return (
    <li className="rounded-xl border border-slate-800 bg-slate-900/60 px-4 py-3">
      <div className="font-medium text-slate-100">{jurisdiction.label}</div>
      <div className="mt-1 text-xs text-[var(--color-text-muted)]">{jurisdiction.jurisdictionKey}</div>
      <div className="mt-1 text-sm text-slate-300">{jurisdiction.governingBodyLabel}</div>
      <p className="mt-2 text-sm text-slate-400">{jurisdiction.description || 'No description provided.'}</p>
    </li>
  )
}
