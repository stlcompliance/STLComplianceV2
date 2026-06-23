import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, DetailBadge, getErrorMessage } from '@stl/shared-ui'
import {
  createRestriction,
  getPersonRestrictions,
  liftRestriction,
} from '../../api/client'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }

function formatDate(value: string | null | undefined) {
  if (!value) return 'Not set'
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? 'Not set' : date.toLocaleString()
}

export function RestrictionsSection({ state }: Props) {
  const selectedPerson = state.selectedPerson
  const restrictionsQuery = useQuery({
    queryKey: ['staffarr-person-restrictions', state.accessToken, selectedPerson?.personId],
    queryFn: () => getPersonRestrictions(state.accessToken, selectedPerson!.personId),
    enabled: Boolean(selectedPerson?.personId),
  })
  const [reason, setReason] = useState('')
  const [expiresAt, setExpiresAt] = useState('')

  const activeRestrictions = restrictionsQuery.data?.activeRestrictions ?? []
  const readinessBlockers = restrictionsQuery.data?.readinessBlockers ?? []

  const createMutation = useMutation({
    mutationFn: () =>
      createRestriction(state.accessToken, {
        personId: selectedPerson!.personId,
        reason,
        expiresAt: expiresAt ? new Date(expiresAt).toISOString() : null,
      }),
    onSuccess: async () => {
      setReason('')
      setExpiresAt('')
      await Promise.all([
        state.queryClient.invalidateQueries({ queryKey: ['staffarr-person-restrictions', state.accessToken] }),
        state.queryClient.invalidateQueries({ queryKey: ['staffarr-person-readiness', state.accessToken] }),
        state.queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', state.accessToken] }),
      ])
    },
  })

  const liftMutation = useMutation({
    mutationFn: (restrictionId: string) => liftRestriction(state.accessToken, restrictionId),
    onSuccess: async () => {
      await Promise.all([
        state.queryClient.invalidateQueries({ queryKey: ['staffarr-person-restrictions', state.accessToken] }),
        state.queryClient.invalidateQueries({ queryKey: ['staffarr-person-readiness', state.accessToken] }),
        state.queryClient.invalidateQueries({ queryKey: ['staffarr-person-timeline', state.accessToken] }),
      ])
    },
  })

  const readinessSummary = state.personReadinessQuery.data
  const canManageRestrictions = state.canOverridePersonReadiness
  const blockerCount = readinessBlockers.length

  if (!selectedPerson) {
    return <p className="text-sm text-[var(--color-text-muted)]">Select a person on the People page to review restrictions.</p>
  }

  return (
    <section className="space-y-6">
      <div className="grid gap-6 lg:grid-cols-[minmax(0,1.2fr)_minmax(0,0.8fr)]">
        <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h2 className="text-sm font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">Active restrictions</h2>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                Restrictions are StaffArr controls that other products must respect before work is assigned.
              </p>
            </div>
            <DetailBadge
              label={readinessSummary?.readinessStatus ?? 'unknown'}
              tone={readinessSummary?.readinessStatus === 'ready' ? 'good' : 'warn'}
            />
          </div>

          {restrictionsQuery.isError ? (
            <ApiErrorCallout
              title="Restriction load failed"
              message={getErrorMessage(restrictionsQuery.error, 'Unable to load restriction data.')}
              onRetry={() => void restrictionsQuery.refetch()}
              retryLabel="Retry restrictions"
            />
          ) : restrictionsQuery.isLoading ? (
            <p className="text-sm text-[var(--color-text-muted)]">Loading restrictions…</p>
          ) : activeRestrictions.length === 0 ? (
            <p className="text-sm text-[var(--color-text-muted)]">
              No active restrictions are currently assigned to {selectedPerson.displayName}.
            </p>
          ) : (
            <div className="mt-4 space-y-3">
              {activeRestrictions.map((restriction) => (
                <article key={restriction.overrideId} className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <p className="font-medium text-[var(--color-text-primary)]">{restriction.reason}</p>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                        Granted {formatDate(restriction.grantedAt)} by {restriction.grantedByUserId}
                      </p>
                    </div>
                    <DetailBadge label={restriction.status} tone={restriction.status === 'active' ? 'warn' : 'neutral'} />
                  </div>
                  <dl className="mt-3 grid gap-1 text-xs text-[var(--color-text-muted)]">
                    <div>Expires: {formatDate(restriction.expiresAt)}</div>
                    <div>Cleared: {formatDate(restriction.clearedAt)}</div>
                    <div className="font-mono">Restriction ID: {restriction.overrideId}</div>
                  </dl>
                  {canManageRestrictions && restriction.status === 'active' ? (
                    <button
                      type="button"
                      onClick={() => liftMutation.mutate(restriction.overrideId)}
                      disabled={liftMutation.isPending}
                      className="mt-4 rounded-md border border-[var(--color-accent-border)] bg-[var(--color-accent-soft)] px-3 py-2 text-xs text-[var(--color-accent)] hover:bg-[var(--color-bg-control-hover)] disabled:opacity-50"
                    >
                      {liftMutation.isPending ? 'Lifting…' : 'Lift restriction'}
                    </button>
                  ) : null}
                </article>
              ))}
            </div>
          )}
        </div>

        <div className="space-y-4 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
          <div>
            <h2 className="text-sm font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">Readiness blockers</h2>
            <p className="mt-1 text-sm text-[var(--color-text-muted)]">
              These are the plain-language signals StaffArr returns to explain why the person is limited or blocked.
            </p>
          </div>

          <div className="space-y-3">
            {readinessBlockers.length === 0 ? (
              <p className="text-sm text-[var(--color-text-muted)]">No readiness blockers are currently reported.</p>
            ) : (
              readinessBlockers.map((blocker, index) => (
                <article key={`${blocker.blockerType}-${index}`} className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="font-medium text-[var(--color-text-primary)]">{blocker.message}</p>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {blocker.blockerSource} · {blocker.blockerType}
                      </p>
                    </div>
                    <DetailBadge label={blocker.blockerType} tone={blocker.blockerType === 'missing' ? 'warn' : 'bad'} />
                  </div>
                  <dl className="mt-3 grid gap-1 text-xs text-[var(--color-text-muted)]">
                    {blocker.certificationName ? <div>Certification: {blocker.certificationName}</div> : null}
                    {blocker.qualificationName ? <div>Qualification: {blocker.qualificationName}</div> : null}
                  </dl>
                </article>
              ))
            )}
          </div>

          {canManageRestrictions ? (
            <form
              className="space-y-3 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4"
              onSubmit={(event) => {
                event.preventDefault()
                if (!reason.trim()) {
                  return
                }
                createMutation.mutate()
              }}
            >
              <div>
                <h3 className="text-sm font-medium text-[var(--color-text-primary)]">Add restriction</h3>
                <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                  Use a clear operational reason. Keep it aligned with the record that caused the limitation.
                </p>
              </div>
              <label className="block text-sm text-[var(--color-text-secondary)]">
                Reason
                <textarea
                  value={reason}
                  onChange={(event) => setReason(event.target.value)}
                  className="mt-1 min-h-24 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                  placeholder="Example: Cannot dispatch until TrainArr retraining is complete."
                />
              </label>
              <label className="block text-sm text-[var(--color-text-secondary)]">
                Expires at
                <input
                  type="datetime-local"
                  value={expiresAt}
                  onChange={(event) => setExpiresAt(event.target.value)}
                  className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                />
              </label>
              <button
                type="submit"
                disabled={createMutation.isPending || !reason.trim()}
                className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
              >
                {createMutation.isPending ? 'Saving…' : 'Create restriction'}
              </button>
            </form>
          ) : (
            <p className="text-sm text-[var(--color-text-muted)]">
              Your role can read restrictions, but it cannot create or lift them.
            </p>
          )}
        </div>
      </div>

      <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">Restriction scope</h2>
        <p className="mt-2 text-sm text-[var(--color-text-secondary)]">
          This snapshot explains what is blocking the workflow and what must be completed to clear it.
        </p>
        <p className="mt-2 text-xs text-[var(--color-text-muted)]">
          {blockerCount} blocker{blockerCount === 1 ? '' : 's'} currently surfaced by StaffArr.
        </p>
      </section>
    </section>
  )
}
