import { useMemo, useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ApiErrorCallout, DetailEmptyState, PageHeader, getErrorMessage } from '@stl/shared-ui'
import { createGoverningBody } from '../../api/client'
import type { GoverningBodyResponse } from '../../api/types'
import { useComplianceCoreWorkspaceState } from '../../workspace/useComplianceCoreWorkspaceState'

export function GoverningBodiesPage() {
  const state = useComplianceCoreWorkspaceState()
  const queryClient = useQueryClient()
  const [bodyKey, setBodyKey] = useState('')
  const [label, setLabel] = useState('')
  const [description, setDescription] = useState('')

  const bodies = state.governingBodiesQuery.data ?? []
  const selected = useMemo(() => bodies[0] ?? null, [bodies])

  const createMutation = useMutation({
    mutationFn: () =>
      createGoverningBody(state.accessToken, {
        bodyKey,
        label,
        description,
      }),
    onSuccess: async () => {
      setBodyKey('')
      setLabel('')
      setDescription('')
      await queryClient.invalidateQueries({ queryKey: ['compliancecore-governing-bodies'] })
    },
  })

  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <div className="space-y-6">
      <PageHeader
        title="Governing bodies"
        subtitle="Maintain the top-level regulatory authority catalog used by Compliance Core."
      />

      <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_minmax(360px,0.9fr)]">
        <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Catalog</h2>
          {state.governingBodiesQuery.isError ? (
            <div className="mt-3">
              <ApiErrorCallout
                title="Unable to load governing bodies"
                message={getErrorMessage(state.governingBodiesQuery.error, 'Failed to load governing bodies.')}
                retryLabel="Retry"
                onRetry={() => void state.governingBodiesQuery.refetch()}
              />
            </div>
          ) : null}
          {bodies.length === 0 ? (
            <div className="mt-4">
              <DetailEmptyState text="No governing bodies are registered yet." />
            </div>
          ) : (
            <ul className="mt-4 space-y-2">
              {bodies.map((body) => (
                <BodyRow key={body.governingBodyId} body={body} selected={selected?.governingBodyId === body.governingBodyId} />
              ))}
            </ul>
          )}
        </section>

        <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Create body</h2>
          <div className="mt-4 space-y-3">
            <Field label="Body key" value={bodyKey} onChange={setBodyKey} />
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
              disabled={createMutation.isPending}
              className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
            >
              {createMutation.isPending ? 'Creating…' : 'Create governing body'}
            </button>
            {createMutation.isError ? (
              <ApiErrorCallout
                title="Create failed"
                message={getErrorMessage(createMutation.error, 'Failed to create governing body.')}
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

function BodyRow({
  body,
  selected,
}: {
  body: GoverningBodyResponse
  selected: boolean
}) {
  return (
    <li className={`rounded-xl border px-4 py-3 ${selected ? 'border-sky-500 bg-sky-500/10' : 'border-slate-800 bg-slate-900/60'}`}>
      <div className="font-medium text-slate-100">{body.label}</div>
      <div className="mt-1 text-xs text-slate-500">{body.bodyKey}</div>
      <p className="mt-2 text-sm text-slate-300">{body.description || 'No description provided.'}</p>
    </li>
  )
}
