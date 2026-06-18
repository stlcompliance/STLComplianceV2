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
  const panelClassName =
    'mt-6 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6'
  const panelHeadingClassName = 'text-sm font-medium text-[var(--color-text-secondary)]'
  const panelCopyClassName = 'text-sm text-[var(--color-text-muted)]'
  const secondaryTextClassName = 'text-[var(--color-text-secondary)]'
  const mutedTextClassName = 'text-[var(--color-text-muted)]'

  return (
    <section className={panelClassName} data-testid="person-history-summary-panel">
      <h2 className={panelHeadingClassName}>Personnel history rollup</h2>
      <p className={`mt-2 text-xs ${panelCopyClassName}`}>
        Materialized workforce history summary for {personDisplayName}, refreshed by the shared-worker personnel
        history rollup job.
      </p>

      {isLoading ? (
        <p className={`mt-4 ${panelCopyClassName}`}>Loading history summary…</p>
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
        <p className={`mt-4 ${panelCopyClassName}`}>
          History rollup has not been computed yet. The worker will materialize events on the next scheduled scan.
        </p>
      ) : (
        <dl className="mt-4 grid gap-3 text-sm md:grid-cols-2">
          <div className="flex justify-between gap-4">
            <dt className={mutedTextClassName}>Total events</dt>
            <dd className={`text-right ${secondaryTextClassName}`}>{summary.eventCount}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className={mutedTextClassName}>Last event</dt>
            <dd className={`text-right ${secondaryTextClassName}`}>
              {summary.lastEventAt ? new Date(summary.lastEventAt).toLocaleString() : 'None'}
            </dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className={mutedTextClassName}>Incidents</dt>
            <dd className={`text-right ${secondaryTextClassName}`}>{summary.incidentCount}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className={mutedTextClassName}>Certifications</dt>
            <dd className={`text-right ${secondaryTextClassName}`}>{summary.certificationCount}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className={mutedTextClassName}>Permissions</dt>
            <dd className={`text-right ${secondaryTextClassName}`}>{summary.permissionCount}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className={mutedTextClassName}>Readiness</dt>
            <dd className={`text-right ${secondaryTextClassName}`}>{summary.readinessCount}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className={mutedTextClassName}>Training blockers</dt>
            <dd className={`text-right ${secondaryTextClassName}`}>{summary.trainingBlockerCount}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className={mutedTextClassName}>Notes / documents</dt>
            <dd className={`text-right ${secondaryTextClassName}`}>
              {summary.personnelNoteCount} / {summary.personnelDocumentCount}
            </dd>
          </div>
          <div className="flex justify-between gap-4 md:col-span-2">
            <dt className={mutedTextClassName}>Computed at</dt>
            <dd className={`text-right text-xs ${mutedTextClassName}`}>{new Date(summary.computedAt).toLocaleString()}</dd>
          </div>
        </dl>
      )}
    </section>
  )
}
