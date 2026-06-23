import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import type { PagedResult, LaunchAttemptTimelineItem } from '../../../api/types'
import { resultClass } from './utils'

type Props = {
  isLoading: boolean
  attemptsResult: PagedResult<LaunchAttemptTimelineItem> | undefined
  isError: boolean
  error: Error | null
  onRetry: () => void
  generatedAt: string
}

export function LaunchAttemptsTable({
  isLoading,
  attemptsResult,
  isError,
  error,
  onRetry,
  generatedAt,
}: Props) {
  const attempts = attemptsResult?.items ?? []

  return (
    <section className="space-y-3">
      <div>
        <h4 className="text-sm font-semibold text-stl-navy">Recent launch attempts</h4>
        <p className="mt-1 text-xs text-[var(--color-text-muted)]">Updated {new Date(generatedAt).toLocaleString()}</p>
      </div>
      {isError ? (
        <ApiErrorCallout
          message={getErrorMessage(error, 'Failed to load launch attempts.')}
          onRetry={onRetry}
          retryLabel="Retry launch attempts"
        />
      ) : isLoading ? (
        <p className="text-sm text-[var(--color-text-muted)]">Loading launch attempts…</p>
      ) : attempts.length === 0 ? (
        <p className="text-sm text-[var(--color-text-muted)]">No launch attempts recorded.</p>
      ) : (
        <div className="overflow-x-auto rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)]">
          <table className="min-w-full text-left text-sm">
            <thead className="border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] text-xs uppercase text-[var(--color-text-muted)]">
              <tr>
                <th className="px-3 py-2">Time</th>
                <th className="px-3 py-2">Product</th>
                <th className="px-3 py-2">Tenant</th>
                <th className="px-3 py-2">Actor</th>
                <th className="px-3 py-2">Result</th>
                <th className="px-3 py-2">Reason</th>
                <th className="px-3 py-2">Correlation</th>
                <th className="px-3 py-2">Hint</th>
              </tr>
            </thead>
            <tbody>
              {attempts.map((attempt) => (
                <tr key={attempt.auditEventId} className="border-b border-[var(--color-border-subtle)] align-top">
                  <td className="px-3 py-2 whitespace-nowrap">
                    {new Date(attempt.occurredAt).toLocaleString()}
                  </td>
                  <td className="px-3 py-2">
                    <span className="font-medium text-stl-navy">
                      {attempt.productDisplayName ?? 'Unknown'}
                    </span>
                    <span className="block text-xs text-[var(--color-text-muted)]">{attempt.action}</span>
                  </td>
                  <td className="px-3 py-2">
                    {attempt.tenantDisplayName ?? 'Unknown'}
                  </td>
                  <td className="px-3 py-2">
                    {attempt.actorDisplayName ?? attempt.actorEmail ?? 'System'}
                    {attempt.actorEmail && (
                      <span className="block text-xs text-[var(--color-text-muted)]">{attempt.actorEmail}</span>
                    )}
                  </td>
                  <td className={`px-3 py-2 font-medium ${resultClass(attempt.result)}`}>
                    {attempt.result}
                  </td>
                  <td className="px-3 py-2">{attempt.reasonCode ?? 'none'}</td>
                  <td className="px-3 py-2 font-mono text-xs text-[var(--color-text-muted)]">{attempt.correlationId}</td>
                  <td className="px-3 py-2 text-[var(--color-text-secondary)]">{attempt.remediationHint ?? 'none'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
