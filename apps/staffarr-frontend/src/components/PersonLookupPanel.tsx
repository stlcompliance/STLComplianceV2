import { ApiErrorCallout } from '@stl/shared-ui'
import type { PersonLookupResponse } from '../api/types'

interface PersonLookupPanelProps {
  personId: string
  personDisplayName: string
  lookup: PersonLookupResponse | null
  isLoading: boolean
  isError?: boolean
  readErrorMessage?: string | null
  onRetryRead?: () => void
}

export function PersonLookupPanel({
  personId,
  personDisplayName,
  lookup,
  isLoading,
  isError = false,
  readErrorMessage = null,
  onRetryRead,
}: PersonLookupPanelProps) {
  return (
    <section
      className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6"
      data-testid="person-lookup-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-sm font-medium text-slate-300">Person lookup</h2>
          <p className="mt-1 text-xs text-slate-500">
            Product-facing identity and placement snapshot for {personDisplayName}.
          </p>
        </div>
        <span className="rounded-full bg-sky-500/10 px-2 py-1 font-mono text-[10px] uppercase tracking-wide text-sky-300 ring-1 ring-sky-500/30">
          {personId.slice(0, 8)}…
        </span>
      </div>

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading person lookup…</p>
      ) : isError ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="Person lookup unavailable"
            message={readErrorMessage ?? 'Failed to load person identity and placement details.'}
            onRetry={onRetryRead}
            retryLabel="Retry person lookup"
          />
        </div>
      ) : !lookup ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="Person lookup unavailable"
            message="Person lookup is not available for this profile yet."
          />
        </div>
      ) : (
        <div className="mt-4 space-y-5">
          <dl className="grid gap-3 text-sm md:grid-cols-2">
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Display name</dt>
              <dd className="text-right text-white">{lookup.displayName}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Email</dt>
              <dd className="text-right text-white">{lookup.primaryEmail}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Employment</dt>
              <dd className="text-right uppercase tracking-wide text-slate-300">{lookup.employmentStatus}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Job title</dt>
              <dd className="text-right text-slate-200">{lookup.jobTitle ?? 'Unspecified'}</dd>
            </div>
          </dl>

          <div>
            <h3 className="text-xs font-medium uppercase tracking-wide text-slate-500">Placement</h3>
            <dl className="mt-3 grid gap-3 text-sm md:grid-cols-2">
              <div className="flex justify-between gap-4">
                <dt className="text-slate-500">Primary org unit</dt>
                <dd className="text-right text-white">
                  {lookup.placement.primaryOrgUnitName ?? 'Unassigned'}
                  {lookup.placement.primaryOrgUnitType ? (
                    <span className="ml-2 text-xs text-slate-500">({lookup.placement.primaryOrgUnitType})</span>
                  ) : null}
                </dd>
              </div>
              <div className="flex justify-between gap-4">
                <dt className="text-slate-500">Manager</dt>
                <dd className="text-right text-white">
                  {lookup.placement.managerDisplayName ?? 'None'}
                </dd>
              </div>
            </dl>
          </div>

          <div>
            <h3 className="text-xs font-medium uppercase tracking-wide text-slate-500">Placements</h3>
            {lookup.placement.activeAssignments.length === 0 ? (
              <p className="mt-3 text-sm text-slate-400">No planned or active site/department/team assignments.</p>
            ) : (
              <ul className="mt-3 space-y-2">
                {lookup.placement.activeAssignments.map((assignment) => (
                  <li
                    key={assignment.assignmentId}
                    className="rounded-lg border border-slate-800 bg-slate-950/40 px-3 py-2 text-sm text-slate-200"
                  >
                    <div>{assignment.assignmentPath}</div>
                    <div className="mt-1 text-xs text-slate-500">
                      {assignment.isPrimary ? 'Primary · ' : ''}
                      {assignment.status}
                      {assignment.effectiveAt ? ` · effective ${new Date(assignment.effectiveAt).toLocaleString()}` : ''}
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      )}
    </section>
  )
}
