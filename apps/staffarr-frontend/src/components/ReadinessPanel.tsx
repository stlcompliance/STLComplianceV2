import { type FormEvent, useState } from 'react'
import { ApiErrorCallout } from '@stl/shared-ui'
import type { PersonReadinessResponse } from '../api/types'

interface ReadinessPanelProps {
  personId: string
  personDisplayName: string
  readiness: PersonReadinessResponse | null
  isLoading: boolean
  isError?: boolean
  readErrorMessage?: string | null
  onRetryRead?: () => void
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

function freshnessLabel(status: PersonReadinessResponse['snapshotFreshnessStatus']): string {
  switch (status) {
    case 'fresh':
      return 'Fresh'
    case 'aging':
      return 'Aging'
    case 'stale':
      return 'Stale'
    default:
      return status
  }
}

function confidenceLabel(level: PersonReadinessResponse['confidenceLevel']): string {
  switch (level) {
    case 'high':
      return 'High confidence'
    case 'medium':
      return 'Medium confidence'
    case 'low':
      return 'Low confidence'
    default:
      return level
  }
}

const panelClassName =
  'mt-6 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6'
const panelHeadingClassName = 'text-sm font-medium text-[var(--color-text-secondary)]'
const panelCopyClassName = 'text-sm text-[var(--color-text-muted)]'
const cardClassName =
  'rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-4 py-3'
const primaryTextClassName = 'text-[var(--color-text-primary)]'
const secondaryTextClassName = 'text-[var(--color-text-secondary)]'
const mutedTextClassName = 'text-[var(--color-text-muted)]'

export function ReadinessPanel({
  personId,
  personDisplayName,
  readiness,
  isLoading,
  isError = false,
  readErrorMessage = null,
  onRetryRead,
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
      <section className={panelClassName}>
        <h2 className={panelHeadingClassName}>Workforce readiness</h2>
        <p className={`mt-4 ${panelCopyClassName}`}>Calculating readiness for {personDisplayName}…</p>
      </section>
    )
  }

  if (!readiness) {
    return (
      <section className={panelClassName}>
        <h2 className={panelHeadingClassName}>Workforce readiness</h2>
        <div className="mt-4">
          <ApiErrorCallout
            title="Readiness summary unavailable"
            message={
              isError
                ? readErrorMessage ?? 'Failed to load readiness status for this person.'
                : 'Could not load readiness status for this person.'
            }
            onRetry={isError ? onRetryRead : undefined}
            retryLabel={isError ? 'Retry readiness' : undefined}
          />
        </div>
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

  const missingRequirements = readiness.requirements.filter((requirement) =>
    requirement.requirementStatus === 'missing'
    || requirement.requirementStatus === 'expired'
    || requirement.requirementStatus === 'revoked',
  )

  return (
    <section className={panelClassName}>
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className={panelHeadingClassName}>Workforce readiness</h2>
          <p className={`mt-1 text-xs ${mutedTextClassName}`}>
            Certification-based readiness for {personDisplayName} (person {personId}).
          </p>
        </div>
        <span
          className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold uppercase tracking-wide ring-1 ring-inset ${readinessBadgeClass(readiness.readinessStatus)}`}
        >
          {formatReadinessLabel(readiness.readinessStatus)}
        </span>
      </div>

      <div className="mt-4 grid gap-3 sm:grid-cols-3">
        <div className={cardClassName}>
          <p className="text-[11px] uppercase tracking-wide text-[var(--color-text-muted)]">Reason codes</p>
          <p className={`mt-1 text-sm ${primaryTextClassName}`}>
            {readiness.reasonCodes.length > 0 ? readiness.reasonCodes.join(', ') : 'None recorded'}
          </p>
        </div>
        <div className={cardClassName}>
          <p className="text-[11px] uppercase tracking-wide text-[var(--color-text-muted)]">Freshness</p>
          <p className={`mt-1 text-sm ${primaryTextClassName}`}>
            {freshnessLabel(readiness.snapshotFreshnessStatus)} · {readiness.snapshotAgeMinutes} min old
          </p>
        </div>
        <div className={cardClassName}>
          <p className="text-[11px] uppercase tracking-wide text-[var(--color-text-muted)]">Confidence</p>
          <p className={`mt-1 text-sm ${primaryTextClassName}`}>{confidenceLabel(readiness.confidenceLevel)}</p>
        </div>
      </div>

      {missingRequirements.length > 0 ? (
        <div className="mt-6">
          <h3 className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
            Missing requirements
          </h3>
          <ul className="mt-3 space-y-2">
            {missingRequirements.map((requirement) => (
              <li
                key={requirement.certificationDefinitionId}
                className="rounded-lg border border-amber-500/30 bg-amber-500/10 px-4 py-3 text-sm text-amber-800 dark:text-amber-100"
              >
                <div className="flex flex-wrap items-center gap-2">
                  <p className="font-medium text-amber-950 dark:text-amber-50">{requirement.certificationName}</p>
                  <span className="rounded-full bg-black/20 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wide">
                    {requirementStatusLabel(requirement.requirementStatus)}
                  </span>
                </div>
                <p className="mt-1 text-amber-900/90 dark:text-amber-100/90">{requirement.certificationKey}</p>
                {requirement.recordEffectiveStatus ? (
                  <p className="mt-1 text-xs text-amber-900/80 dark:text-amber-200/80">
                    Record status: {requirement.recordEffectiveStatus}
                  </p>
                ) : null}
              </li>
            ))}
          </ul>
        </div>
      ) : null}

      {readiness.readinessBasis === 'manual_override' && readiness.activeOverride ? (
        <div className="mt-6 rounded-lg border border-sky-500/40 bg-sky-500/10 px-4 py-3 text-sm text-sky-900 dark:text-sky-100">
          <p className="font-medium text-sky-950 dark:text-sky-50">Manual readiness override active</p>
          <p className="mt-2 text-sky-900/90 dark:text-sky-100/90">{readiness.activeOverride.reason}</p>
          <p className="mt-2 text-xs text-sky-900/80 dark:text-sky-200/80">
            Granted {new Date(readiness.activeOverride.grantedAt).toLocaleString()}
            {readiness.activeOverride.expiresAt
              ? ` · expires ${new Date(readiness.activeOverride.expiresAt).toLocaleString()}`
              : ' · no expiration'}
          </p>
        </div>
      ) : null}

      {readiness.blockers.length > 0 ? (
        <div className="mt-6">
          <h3 className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">Blockers</h3>
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
                      className={`font-medium ${blocker.blockerSource === 'training' ? 'text-violet-50' : 'text-amber-950 dark:text-amber-50'}`}
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
        <p className="mt-6 text-sm text-emerald-700 dark:text-emerald-300">
          All readiness-linked certifications are current. This person can be assigned to gated work.
        </p>
      )}

      <div className="mt-6">
        <h3 className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">Requirements</h3>
        <ul className="mt-3 divide-y divide-[var(--color-border-subtle)]">
          {readiness.requirements.map((requirement) => (
            <li key={requirement.certificationDefinitionId} className="flex items-center justify-between py-3 text-sm">
              <div>
                <p className={primaryTextClassName}>{requirement.certificationName}</p>
                <p className={`text-xs ${mutedTextClassName}`}>{requirement.certificationKey}</p>
              </div>
              <span className="text-xs uppercase tracking-wide text-[var(--color-text-secondary)]">
                {requirementStatusLabel(requirement.requirementStatus)}
              </span>
            </li>
          ))}
        </ul>
      </div>

      {canOverride ? (
        <div className="mt-6 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4">
          <h3 className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
            Manual override (staffarr.readiness.override)
          </h3>
          {readiness.activeOverride ? (
            <div className="mt-3 flex flex-wrap items-center gap-3">
              <p className={`text-sm ${secondaryTextClassName}`}>An active override is in effect for this person.</p>
              <button
                type="button"
                disabled={isSubmittingOverride}
                onClick={() => void onClearOverride()}
                className="rounded-md bg-[var(--color-bg-control)] px-3 py-2 text-xs font-medium text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)] disabled:opacity-50"
              >
                Clear override
              </button>
            </div>
          ) : (
            <form className="mt-3 grid gap-3" onSubmit={(event) => void handleGrantOverride(event)}>
              <label htmlFor="readiness-override-reason" className="grid gap-1 text-sm">
                <span className={secondaryTextClassName}>Reason (required, min 8 characters)</span>
                <textarea
                  id="readiness-override-reason"
                  value={overrideReason}
                  onChange={(event) => setOverrideReason(event.target.value)}
                  rows={3}
                  className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
                  required
                  minLength={8}
                />
              </label>
              <label htmlFor="readiness-override-expires-at" className="grid gap-1 text-sm">
                <span className={secondaryTextClassName}>Expires at (optional)</span>
                <input
                  id="readiness-override-expires-at"
                  type="datetime-local"
                  value={overrideExpiresAt}
                  onChange={(event) => setOverrideExpiresAt(event.target.value)}
                  className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-[var(--color-text-primary)]"
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
            <div className="mt-3">
              <ApiErrorCallout
                title="Readiness override failed"
                message={overrideErrorMessage}
              />
            </div>
          ) : null}
        </div>
      ) : null}

      <p className="mt-4 text-xs text-[var(--color-text-muted)]">
        Calculated {new Date(readiness.calculatedAt).toLocaleString()} from source snapshot{' '}
        {new Date(readiness.sourceTimestamp).toLocaleString()}.
        {readiness.readinessBasis === 'manual_override'
          ? ' Basis: manual override.'
          : readiness.readinessBasis === 'training_blockers'
            ? ' Basis: TrainArr training blockers.'
            : ' Basis: certifications.'}
      </p>
    </section>
  )
}
