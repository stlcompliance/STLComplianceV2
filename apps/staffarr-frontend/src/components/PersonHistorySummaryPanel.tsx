import { ApiErrorCallout } from '@stl/shared-ui'
import type { PersonnelHistorySummaryResponse } from '../api/types'

interface PersonHistorySummaryPanelProps {
  personDisplayName: string
  summary: PersonnelHistorySummaryResponse | null
  isLoading: boolean
  isError?: boolean
  readErrorMessage?: string | null
  onRetryRead?: () => void
}

export function PersonHistorySummaryPanel({
  personDisplayName,
  summary,
  isLoading,
  isError = false,
  readErrorMessage = null,
  onRetryRead,
}: PersonHistorySummaryPanelProps) {
  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6" data-testid="person-history-summary-panel">
      <h2 className="text-sm font-medium text-slate-300">Personnel history rollup</h2>
      <p className="mt-2 text-xs text-slate-500">
        Materialized workforce history summary for {personDisplayName}, refreshed by the shared-worker personnel
        history rollup job.
      </p>

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading history summary…</p>
      ) : isError ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="History summary unavailable"
            message={readErrorMessage ?? 'Failed to load personnel history summary.'}
            onRetry={onRetryRead}
            retryLabel="Retry history summary"
          />
        </div>
      ) : !summary ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="History summary unavailable"
            message="History summary has not been generated yet."
          />
        </div>
      ) : !summary.isMaterialized ? (
        <p className="mt-4 text-sm text-slate-400">
          History rollup has not been computed yet. The worker will materialize events on the next scheduled scan.
        </p>
      ) : (
        <dl className="mt-4 grid gap-3 text-sm md:grid-cols-2">
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Total events</dt>
            <dd className="text-right text-white">{summary.eventCount}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Last event</dt>
            <dd className="text-right text-slate-200">
              {summary.lastEventAt ? new Date(summary.lastEventAt).toLocaleString() : 'None'}
            </dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Incidents</dt>
            <dd className="text-right text-slate-200">{summary.incidentCount}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Certifications</dt>
            <dd className="text-right text-slate-200">{summary.certificationCount}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Permissions</dt>
            <dd className="text-right text-slate-200">{summary.permissionCount}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Readiness</dt>
            <dd className="text-right text-slate-200">{summary.readinessCount}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Training blockers</dt>
            <dd className="text-right text-slate-200">{summary.trainingBlockerCount}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Notes / documents</dt>
            <dd className="text-right text-slate-200">
              {summary.personnelNoteCount} / {summary.personnelDocumentCount}
            </dd>
          </div>
          <div className="flex justify-between gap-4 md:col-span-2">
            <dt className="text-slate-500">Computed at</dt>
            <dd className="text-right text-xs text-slate-400">{new Date(summary.computedAt).toLocaleString()}</dd>
          </div>
        </dl>
      )}
    </section>
  )
}
