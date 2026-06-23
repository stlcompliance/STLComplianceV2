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
    return <p className="text-sm text-slate-400">Select a person on the People page to review TrainArr-published certification status.</p>
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
    <section className="rounded-xl border border-slate-800 bg-slate-950/50 p-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="max-w-3xl">
          <div className="flex items-center gap-2 text-amber-300">
            <Award className="h-4 w-4" aria-hidden="true" />
            <p className="text-xs font-semibold uppercase tracking-[0.18em]">
              TrainArr-owned qualification status
            </p>
          </div>
          <h2 className="mt-2 text-lg font-semibold text-slate-100">
            Certification actions moved to TrainArr
          </h2>
          <p className="mt-2 text-sm text-slate-300">
            StaffArr now mirrors qualification and certification status for{' '}
            <span className="font-medium text-white">{s.selectedPerson.displayName}</span>. Review certification
            definitions, issuance, renewal, expiration, and revocation.
          </p>
        </div>
        <button
          type="button"
          onClick={() => void handleOpenTrainArr()}
          disabled={isLaunching}
          className="inline-flex items-center gap-2 rounded-md border border-amber-400/60 bg-amber-500/10 px-3 py-2 text-sm font-medium text-amber-100 transition hover:bg-amber-500/20 disabled:cursor-not-allowed disabled:opacity-60"
        >
          <ExternalLink className="h-4 w-4" aria-hidden="true" />
          {isLaunching ? 'Opening TrainArr…' : 'Open in TrainArr'}
        </button>
      </div>

      {s.certificationDefinitionsQuery.isLoading || s.personCertificationsQuery.isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading TrainArr-published certification status…</p>
      ) : null}

      {readErrorMessage ? <p className="mt-4 text-sm text-rose-300">{readErrorMessage}</p> : null}
      {launchError ? <p className="mt-3 text-sm text-rose-300">{launchError}</p> : null}

      <dl className="mt-5 grid gap-3 sm:grid-cols-3">
        <div className="rounded-lg border border-slate-800 bg-slate-900/70 px-3 py-3">
          <dt className="text-xs uppercase tracking-wide text-slate-400">Published definitions</dt>
          <dd className="mt-1 text-lg font-semibold text-white">
            {s.certificationDefinitions.length}
          </dd>
        </div>
        <div className="rounded-lg border border-slate-800 bg-slate-900/70 px-3 py-3">
          <dt className="text-xs uppercase tracking-wide text-slate-400">Active records</dt>
          <dd className="mt-1 text-lg font-semibold text-white">
            {s.personCertifications.filter((item) => item.effectiveStatus === 'active').length}
          </dd>
        </div>
        <div className="rounded-lg border border-slate-800 bg-slate-900/70 px-3 py-3">
          <dt className="text-xs uppercase tracking-wide text-slate-400">Readiness blockers</dt>
          <dd className="mt-1 text-lg font-semibold text-white">
            {s.personReadinessQuery?.data?.blockers.length ?? 0}
          </dd>
        </div>
      </dl>

      <div className="mt-5 space-y-3">
        {s.personCertifications.length > 0 ? (
          s.personCertifications.map((certification) => (
            <article
              key={certification.personCertificationId}
              className="rounded-lg border border-slate-800 bg-slate-900/50 px-4 py-4"
            >
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <h3 className="text-sm font-medium text-slate-100">
                    {certification.certificationName}
                  </h3>
                  <p className="mt-1 text-xs text-slate-400">
                    {certification.certificationKey} · {certification.category}
                  </p>
                </div>
                <span className="rounded-full border border-slate-700 px-2 py-1 text-xs uppercase tracking-wide text-slate-200">
                  {certification.effectiveStatus.replaceAll('_', ' ')}
                </span>
              </div>
              <p className="mt-3 text-xs text-slate-400">
                Granted {new Date(certification.grantedAt).toLocaleDateString()}
                {certification.expiresAt
                  ? ` · Expires ${new Date(certification.expiresAt).toLocaleDateString()}`
                  : ' · No published expiration'}
              </p>
              {certification.notes ? (
                <p className="mt-2 text-sm text-slate-300">{certification.notes}</p>
              ) : null}
            </article>
          ))
        ) : (
          <p className="rounded-lg border border-dashed border-slate-800 bg-slate-950/40 px-4 py-4 text-sm text-slate-400">
            No TrainArr-published certification records are currently mirrored for this person.
          </p>
        )}
      </div>
    </section>
  )
}
