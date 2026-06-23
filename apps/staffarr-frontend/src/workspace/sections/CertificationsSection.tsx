import { getErrorMessage } from '@stl/shared-ui'
import { Award, ExternalLink } from 'lucide-react'
import { useState } from 'react'
import { createLaunchHandoff } from '../../api/client'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }

export function CertificationsSection({ state }: Props) {
  const s = state
  const [isLaunching, setIsLaunching] = useState(false)
  const [launchError, setLaunchError] = useState<string | null>(null)
  if (!s.selectedPerson) {
    return <p className="text-sm text-[var(--color-text-muted)]">Select a person on the People page to review TrainArr-published certification status.</p>
  }

  async function handleOpenTrainArr() {
    if (isLaunching) {
      return
    }

    setLaunchError(null)
    setIsLaunching(true)

    try {
      const handoff = await createLaunchHandoff(s.accessToken, 'trainarr', window.location.href)
      window.location.assign(handoff.launchUrl)
    } catch (error) {
      console.error('TrainArr launch handoff failed', error)
      setLaunchError('TrainArr is temporarily unavailable. Please try again.')
      setIsLaunching(false)
    }
  }

  const readErrorMessage =
    s.certificationDefinitionsQuery.isError
      ? getErrorMessage(
          s.certificationDefinitionsQuery.error,
          'Failed to load certification definitions.',
        )
      : s.personCertificationsQuery.isError
        ? getErrorMessage(
            s.personCertificationsQuery.error,
            'Failed to load person certifications.',
          )
        : null

  return (
    <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="max-w-3xl">
          <div className="flex items-center gap-2 text-[var(--color-accent)]">
            <Award className="h-4 w-4" aria-hidden="true" />
            <p className="text-xs font-semibold uppercase tracking-[0.18em]">
              TrainArr-owned qualification status
            </p>
          </div>
          <h2 className="mt-2 text-lg font-semibold text-[var(--color-text-primary)]">
            Certification actions moved to TrainArr
          </h2>
          <p className="mt-2 text-sm text-[var(--color-text-secondary)]">
            StaffArr now mirrors qualification and certification status for{' '}
            <span className="font-medium text-[var(--color-text-primary)]">{s.selectedPerson.displayName}</span>. Review certification
            definitions, issuance, renewal, expiration, and revocation.
          </p>
        </div>
        <button
          type="button"
          onClick={() => void handleOpenTrainArr()}
          disabled={isLaunching}
          className="inline-flex items-center gap-2 rounded-md border border-[var(--color-accent-border)] bg-[var(--color-accent-soft)] px-3 py-2 text-sm font-medium text-[var(--color-accent)] transition hover:bg-[var(--color-bg-control-hover)] disabled:cursor-not-allowed disabled:opacity-60"
        >
          <ExternalLink className="h-4 w-4" aria-hidden="true" />
          {isLaunching ? 'Opening TrainArr…' : 'Open in TrainArr'}
        </button>
      </div>

      {s.certificationDefinitionsQuery.isLoading || s.personCertificationsQuery.isLoading ? (
        <p className="mt-4 text-sm text-[var(--color-text-muted)]">Loading TrainArr-published certification status…</p>
      ) : null}

      {readErrorMessage ? <p className="mt-4 text-sm text-[var(--color-danger-text)]">{readErrorMessage}</p> : null}
      {launchError ? <p className="mt-3 text-sm text-[var(--color-danger-text)]">{launchError}</p> : null}

      <dl className="mt-5 grid gap-3 sm:grid-cols-3">
        <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-3">
          <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Published definitions</dt>
          <dd className="mt-1 text-lg font-semibold text-[var(--color-text-primary)]">
            {s.certificationDefinitions.length}
          </dd>
        </div>
        <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-3">
          <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Active records</dt>
          <dd className="mt-1 text-lg font-semibold text-[var(--color-text-primary)]">
            {s.personCertifications.filter((item) => item.effectiveStatus === 'active').length}
          </dd>
        </div>
        <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-3">
          <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Readiness blockers</dt>
          <dd className="mt-1 text-lg font-semibold text-[var(--color-text-primary)]">
            {s.personReadinessQuery?.data?.blockers.length ?? 0}
          </dd>
        </div>
      </dl>

      <div className="mt-5 space-y-3">
        {s.personCertifications.length > 0 ? (
          s.personCertifications.map((certification) => (
            <article
              key={certification.personCertificationId}
              className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-4 py-4"
            >
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <h3 className="text-sm font-medium text-[var(--color-text-primary)]">
                    {certification.certificationName}
                  </h3>
                  <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                    {certification.certificationKey} · {certification.category}
                  </p>
                </div>
                <span className="rounded-full border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-2 py-1 text-xs uppercase tracking-wide text-[var(--color-text-secondary)]">
                  {certification.effectiveStatus.replaceAll('_', ' ')}
                </span>
              </div>
              <p className="mt-3 text-xs text-[var(--color-text-muted)]">
                Granted {new Date(certification.grantedAt).toLocaleDateString()}
                {certification.expiresAt
                  ? ` · Expires ${new Date(certification.expiresAt).toLocaleDateString()}`
                  : ' · No published expiration'}
              </p>
              {certification.notes ? (
                <p className="mt-2 text-sm text-[var(--color-text-secondary)]">{certification.notes}</p>
              ) : null}
            </article>
          ))
        ) : (
          <p className="rounded-lg border border-dashed border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-4 py-4 text-sm text-[var(--color-text-muted)]">
            No TrainArr-published certification records are currently mirrored for this person.
          </p>
        )}
      </div>
    </section>
  )
}
