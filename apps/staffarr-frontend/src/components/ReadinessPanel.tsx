import type { PersonReadinessResponse } from '../api/types'

interface ReadinessPanelProps {
  personId: string
  personDisplayName: string
  readiness: PersonReadinessResponse | null
  isLoading: boolean
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
}: ReadinessPanelProps) {
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

      {readiness.blockers.length > 0 ? (
        <div className="mt-6">
          <h3 className="text-xs font-medium uppercase tracking-wide text-slate-500">Blockers</h3>
          <ul className="mt-3 space-y-2">
            {readiness.blockers.map((blocker) => (
              <li
                key={`${blocker.certificationKey}-${blocker.blockerType}`}
                className="rounded-lg border border-amber-500/30 bg-amber-500/10 px-4 py-3 text-sm text-amber-100"
              >
                <p className="font-medium text-amber-50">{blocker.certificationName}</p>
                <p className="mt-1 text-amber-100/90">{blocker.message}</p>
              </li>
            ))}
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

      <p className="mt-4 text-xs text-slate-500">
        Calculated {new Date(readiness.calculatedAt).toLocaleString()}.
      </p>
    </section>
  )
}
