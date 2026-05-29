import { type FormEvent, useState } from 'react'
import type { PersonReadinessResponse } from '../api/types'

interface ReadinessPanelProps {
  personId: string
  personDisplayName: string
  readiness: PersonReadinessResponse | null
  isLoading: boolean
  canOverride: boolean
  isSubmittingOverride: boolean
  overrideErrorMessage: string | null
  onGrantOverride: (request: { reason: string; expiresAt: string | null }) => Promise<void>
  onClearOverride: () => Promise<void>
}

export function canOverrideReadiness(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) {
    return true
  }

  return ['tenant_admin', 'staffarr_admin', 'hr_admin'].includes(tenantRoleKey)
}

function formatReadinessLabel(status: PersonReadinessResponse['readinessStatus']): string {
  return status === 'ready' ? 'Ready' : 'Not ready'
}

function readinessBadgeClass(status: PersonReadinessResponse['readinessStatus']): string {
  return status === 'ready'
    ? 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    : 'bg-amber-500/20 text-amber-200 ring-amber-500/40'
}

function requirementStatusLabel(status: string): string {
  switch (status) {
    case 'satisfied':
      return 'Satisfied'
    case 'missing':
      return 'Missing'
    case 'expired':
      return 'Expired'
    case 'revoked':
      return 'Revoked'
    default:
      return status
  }
}

export function ReadinessPanel({
  personId,
  personDisplayName,
  readiness,
  isLoading,
  canOverride,
  isSubmittingOverride,
  overrideErrorMessage,
  onGrantOverride,
  onClearOverride,
}: ReadinessPanelProps) {
  const [overrideReason, setOverrideReason] = useState('')
  const [overrideExpiresAt, setOverrideExpiresAt] = useState('')

  if (isLoading) {
    return (
      <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
        <h2 className="text-sm font-medium text-slate-300">Workforce readiness</h2>
        <p className="mt-4 text-sm text-slate-400">Calculating readiness for {personDisplayName}…</p>
      </section>
    )
  }

  if (!readiness) {
    return (
      <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
        <h2 className="text-sm font-medium text-slate-300">Workforce readiness</h2>
        <p className="mt-4 text-sm text-slate-400">Readiness summary is unavailable.</p>
      </section>
    )
  }

  const handleGrantOverride = async (event: FormEvent) => {
    event.preventDefault()
    await onGrantOverride({
      reason: overrideReason,
      expiresAt: overrideExpiresAt ? new Date(overrideExpiresAt).toISOString() : null,
    })
    setOverrideReason('')
    setOverrideExpiresAt('')
  }

  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-sm font-medium text-slate-300">Workforce readiness</h2>
          <p className="mt-1 text-xs text-slate-500">
            Certification-based readiness for {personDisplayName} (person {personId}).
          </p>
        </div>
        <span
          className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold uppercase tracking-wide ring-1 ring-inset ${readinessBadgeClass(readiness.readinessStatus)}`}
        >
          {formatReadinessLabel(readiness.readinessStatus)}
        </span>
      </div>

      {readiness.readinessBasis === 'manual_override' && readiness.activeOverride ? (
        <div className="mt-6 rounded-lg border border-sky-500/40 bg-sky-500/10 px-4 py-3 text-sm text-sky-100">
          <p className="font-medium text-sky-50">Manual readiness override active</p>
          <p className="mt-2 text-sky-100/90">{readiness.activeOverride.reason}</p>
          <p className="mt-2 text-xs text-sky-200/80">
            Granted {new Date(readiness.activeOverride.grantedAt).toLocaleString()}
            {readiness.activeOverride.expiresAt
              ? ` · expires ${new Date(readiness.activeOverride.expiresAt).toLocaleString()}`
              : ' · no expiration'}
          </p>
        </div>
      ) : null}

      {readiness.blockers.length > 0 ? (
        <div className="mt-6">
          <h3 className="text-xs font-medium uppercase tracking-wide text-slate-500">Blockers</h3>
          <ul className="mt-3 space-y-2">
            {readiness.blockers.map((blocker) => {
              const blockerKey =
                blocker.blockerSource === 'training'
                  ? `training-${blocker.qualificationKey}-${blocker.blockerType}`
                  : `certification-${blocker.certificationKey}-${blocker.blockerType}`
              const blockerTitle =
                blocker.blockerSource === 'training'
                  ? (blocker.qualificationName ?? blocker.qualificationKey ?? 'Training requirement')
                  : (blocker.certificationName ?? blocker.certificationKey ?? 'Certification requirement')
              const blockerTone =
                blocker.blockerSource === 'training'
                  ? 'border-violet-500/30 bg-violet-500/10 text-violet-100'
                  : 'border-amber-500/30 bg-amber-500/10 text-amber-100'

              return (
                <li
                  key={blockerKey}
                  className={`rounded-lg border px-4 py-3 text-sm ${blockerTone}`}
                >
                  <div className="flex flex-wrap items-center gap-2">
                    <p
                      className={`font-medium ${blocker.blockerSource === 'training' ? 'text-violet-50' : 'text-amber-50'}`}
                    >
                      {blockerTitle}
                    </p>
                    <span className="rounded-full bg-black/20 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide">
                      {blocker.blockerSource === 'training' ? 'Training' : 'Certification'}
                    </span>
                  </div>
                  <p className="mt-1 opacity-90">{blocker.message}</p>
                </li>
              )
            })}
          </ul>
        </div>
      ) : (
        <p className="mt-6 text-sm text-emerald-300">
          All readiness-linked certifications are current. This person can be assigned to gated work.
        </p>
      )}

      <div className="mt-6">
        <h3 className="text-xs font-medium uppercase tracking-wide text-slate-500">Requirements</h3>
        <ul className="mt-3 divide-y divide-slate-700">
          {readiness.requirements.map((requirement) => (
            <li key={requirement.certificationDefinitionId} className="flex items-center justify-between py-3 text-sm">
              <div>
                <p className="text-white">{requirement.certificationName}</p>
                <p className="text-xs text-slate-500">{requirement.certificationKey}</p>
              </div>
              <span className="text-xs uppercase tracking-wide text-slate-400">
                {requirementStatusLabel(requirement.requirementStatus)}
              </span>
            </li>
          ))}
        </ul>
      </div>

      {canOverride ? (
        <div className="mt-6 rounded-lg border border-slate-600 bg-slate-950/40 p-4">
          <h3 className="text-xs font-medium uppercase tracking-wide text-slate-500">
            Manual override (staffarr.readiness.override)
          </h3>
          {readiness.activeOverride ? (
            <div className="mt-3 flex flex-wrap items-center gap-3">
              <p className="text-sm text-slate-300">An active override is in effect for this person.</p>
              <button
                type="button"
                disabled={isSubmittingOverride}
                onClick={() => void onClearOverride()}
                className="rounded-md bg-slate-700 px-3 py-2 text-xs font-medium text-white hover:bg-slate-600 disabled:opacity-50"
              >
                Clear override
              </button>
            </div>
          ) : (
            <form className="mt-3 grid gap-3" onSubmit={(event) => void handleGrantOverride(event)}>
              <label htmlFor="readiness-override-reason" className="grid gap-1 text-sm">
                <span className="text-slate-400">Reason (required, min 8 characters)</span>
                <textarea
                  id="readiness-override-reason"
                  value={overrideReason}
                  onChange={(event) => setOverrideReason(event.target.value)}
                  rows={3}
                  className="rounded-md border border-slate-600 bg-slate-900 px-3 py-2 text-white"
                  required
                  minLength={8}
                />
              </label>
              <label htmlFor="readiness-override-expires-at" className="grid gap-1 text-sm">
                <span className="text-slate-400">Expires at (optional)</span>
                <input
                  id="readiness-override-expires-at"
                  type="datetime-local"
                  value={overrideExpiresAt}
                  onChange={(event) => setOverrideExpiresAt(event.target.value)}
                  className="rounded-md border border-slate-600 bg-slate-900 px-3 py-2 text-white"
                />
              </label>
              <button
                type="submit"
                disabled={isSubmittingOverride || overrideReason.trim().length < 8}
                className="justify-self-start rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
              >
                Grant readiness override
              </button>
            </form>
          )}
          {overrideErrorMessage ? (
            <p className="mt-3 text-sm text-rose-300" role="alert">
              {overrideErrorMessage}
            </p>
          ) : null}
        </div>
      ) : null}

      <p className="mt-4 text-xs text-slate-500">
        Calculated {new Date(readiness.calculatedAt).toLocaleString()}.
        {readiness.readinessBasis === 'manual_override'
          ? ' Basis: manual override.'
          : readiness.readinessBasis === 'training_blockers'
            ? ' Basis: TrainArr training blockers.'
            : ' Basis: certifications.'}
      </p>
    </section>
  )
}
