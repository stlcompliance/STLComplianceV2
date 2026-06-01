import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import type { PagedResult, LaunchAttemptTimelineItem } from '../../../api/types'
import { resultClass } from './utils'

type Props = {
  attemptsResult: PagedResult<LaunchAttemptTimelineItem> | undefined
  isError: boolean
  error: Error | null
  onRetry: () => void
  generatedAt: string
}

export function LaunchAttemptsTable({ attemptsResult, isError, error, onRetry, generatedAt }: Props) {
  const attempts = attemptsResult?.items ?? []

  return (
    <section className="space-y-3">
      <div>
        <h4 className="text-sm font-semibold text-stl-navy">Recent launch attempts</h4>
        <p className="mt-1 text-xs text-slate-500">Updated {new Date(generatedAt).toLocaleString()}</p>
      </div>
      {isError ? (
        <ApiErrorCallout
          message={getErrorMessage(error, 'Failed to load launch attempts.')}
          onRetry={onRetry}
          retryLabel="Retry launch attempts"
        />
      ) : attempts.length === 0 ? (
        <p className="text-sm text-slate-500">No launch attempts recorded.</p>
      ) : (
        <div className="overflow-x-auto rounded-lg border border-slate-200 bg-white">
          <table className="min-w-full text-left text-sm">
            <thead className="border-b border-slate-200 bg-slate-50 text-xs uppercase text-slate-500">
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
                <tr key={attempt.auditEventId} className="border-b border-slate-100 align-top">
                  <td className="px-3 py-2 whitespace-nowrap">
                    {new Date(attempt.occurredAt).toLocaleString()}
                  </td>
                  <td className="px-3 py-2">
                    <span className="font-medium text-stl-navy">
                      {attempt.productDisplayName ?? attempt.productKey ?? 'Unknown'}
                    </span>
                    <span className="block text-xs text-slate-500">{attempt.action}</span>
                  </td>
                  <td className="px-3 py-2">
                    {attempt.tenantDisplayName ?? 'Unknown'}
                    {attempt.tenantSlug && (
                      <span className="block text-xs text-slate-500">{attempt.tenantSlug}</span>
                    )}
                  </td>
                  <td className="px-3 py-2">
                    {attempt.actorDisplayName ?? attempt.actorEmail ?? 'System'}
                    {attempt.actorEmail && (
                      <span className="block text-xs text-slate-500">{attempt.actorEmail}</span>
                    )}
                  </td>
                  <td className={`px-3 py-2 font-medium ${resultClass(attempt.result)}`}>
                    {attempt.result}
                  </td>
                  <td className="px-3 py-2">{attempt.reasonCode ?? 'none'}</td>
                  <td className="px-3 py-2 font-mono text-xs text-slate-600">{attempt.correlationId}</td>
                  <td className="px-3 py-2 text-slate-700">{attempt.remediationHint ?? 'none'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
