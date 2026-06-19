import type {
  QualificationIssueHistoryItemResponse,
  QualificationIssueListItemResponse,
} from '../api/types'

interface QualificationManagementPanelProps {
  issues: QualificationIssueListItemResponse[]
  statusFilter: string
  lifecycleReason: string
  selectedIssueId: string | null
  onStatusFilterChange: (value: string) => void
  onLifecycleReasonChange: (value: string) => void
  onSelectIssue: (issueId: string) => void
  onSuspend: (issueId: string) => void
  onRevoke: (issueId: string) => void
  onExpire: (issueId: string) => void
  isSuspending: boolean
  isRevoking: boolean
  isExpiring: boolean
  canManage: boolean
  history: QualificationIssueHistoryItemResponse[]
  isLoadingHistory: boolean
}

export function QualificationManagementPanel({
  issues,
  statusFilter,
  lifecycleReason,
  selectedIssueId,
  onStatusFilterChange,
  onLifecycleReasonChange,
  onSelectIssue,
  onSuspend,
  onRevoke,
  onExpire,
  isSuspending,
  isRevoking,
  isExpiring,
  canManage,
  history,
  isLoadingHistory,
}: QualificationManagementPanelProps) {
  const selected = issues.find((i) => i.qualificationIssueId === selectedIssueId) ?? null

  if (!canManage) {
    return (
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Certificate registry</h2>
        <p className="mt-3 text-sm text-slate-400">Lifecycle actions require trainarr qualifications manage access.</p>
      </section>
    )
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/60 p-4"
      data-testid="qualification-management-panel"
    >
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Certificate registry</h2>
      <p className="mt-1 text-xs text-[var(--color-text-muted)]">Issue, suspend, revoke, and expire credentials across assignments.</p>

      <label htmlFor="qualification-management-status-filter" className="mt-4 block text-xs text-slate-400">
        Status filter
        <select
          id="qualification-management-status-filter"
          className="mt-1 w-full max-w-xs rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
          value={statusFilter}
          onChange={(e) => onStatusFilterChange(e.target.value)}
        >
          <option value="">All active statuses</option>
          <option value="issued">Issued</option>
          <option value="suspended">Suspended</option>
          <option value="revoked">Revoked</option>
          <option value="expired">Expired</option>
        </select>
      </label>

      {issues.length === 0 ? (
        <p className="mt-4 text-sm text-[var(--color-text-muted)]">No qualification issues match this filter.</p>
      ) : (
        <ul className="mt-4 max-h-64 space-y-2 overflow-y-auto">
          {issues.map((issue) => (
            <li key={issue.qualificationIssueId}>
              <button
                type="button"
                className={`w-full rounded-lg border p-3 text-left text-sm ${
                  selectedIssueId === issue.qualificationIssueId
                    ? 'border-violet-500 bg-violet-950/30'
                    : 'border-slate-700 bg-slate-950/40'
                }`}
                onClick={() => onSelectIssue(issue.qualificationIssueId)}
              >
                <p className="font-medium text-slate-100">{issue.qualificationName}</p>
                <p className="mt-1 text-xs text-slate-400">
                  {issue.status} · {issue.qualificationKey}
                </p>
                <p className="mt-1 font-mono text-xs text-[var(--color-text-muted)]">{issue.staffarrPersonId}</p>
              </button>
            </li>
          ))}
        </ul>
      )}

      {selected ? (
        <div className="mt-4 space-y-3 border-t border-slate-700 pt-4">
          <p className="text-sm text-slate-300">
            Selected: <span className="text-slate-100">{selected.qualificationName}</span> ({selected.status})
          </p>
          {selected.lifecycleReason ? (
            <p className="text-xs text-slate-400">Reason: {selected.lifecycleReason}</p>
          ) : null}
          <label htmlFor="qualification-management-lifecycle-reason" className="grid gap-1 text-xs text-slate-400">
            Lifecycle reason (optional)
            <textarea
              id="qualification-management-lifecycle-reason"
              value={lifecycleReason}
              onChange={(e) => onLifecycleReasonChange(e.target.value)}
              rows={2}
              className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
            />
          </label>
          <div className="flex flex-wrap gap-2">
            {selected.status === 'issued' ? (
              <button
                type="button"
                disabled={isSuspending}
                className="rounded border border-amber-700 px-2 py-1 text-xs text-amber-100 hover:bg-amber-950/40 disabled:opacity-50"
                onClick={() => onSuspend(selected.qualificationIssueId)}
              >
                Suspend
              </button>
            ) : null}
            {['issued', 'suspended'].includes(selected.status) ? (
              <>
                <button
                  type="button"
                  disabled={isRevoking}
                  className="rounded border border-red-700 px-2 py-1 text-xs text-red-100 hover:bg-red-950/40 disabled:opacity-50"
                  onClick={() => onRevoke(selected.qualificationIssueId)}
                >
                  Revoke
                </button>
                <button
                  type="button"
                  disabled={isExpiring}
                  className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-200 hover:bg-slate-800 disabled:opacity-50"
                  onClick={() => onExpire(selected.qualificationIssueId)}
                >
                  Expire
                </button>
              </>
            ) : null}
          </div>
          <div className="rounded border border-slate-700 bg-slate-950/40 p-3">
            <h3 className="text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">
              Qualification issue history
            </h3>
            {isLoadingHistory ? (
              <p className="mt-2 text-sm text-slate-400">Loading history…</p>
            ) : history.length === 0 ? (
              <p className="mt-2 text-sm text-slate-400">No qualification history recorded yet.</p>
            ) : (
              <ul className="mt-2 space-y-2">
                {history.map((item) => (
                  <li
                    key={`${item.eventType}-${item.occurredAt}`}
                    className="rounded border border-slate-700 px-3 py-2 text-xs"
                  >
                    <p className="font-medium text-slate-100">{item.eventType}</p>
                    <p className="mt-1 text-slate-400">
                      {item.status}
                      {item.reason ? ` · ${item.reason}` : ''}
                      {item.actorUserId ? ` · actor ${item.actorUserId}` : ''}
                    </p>
                    <p className="mt-1 text-[var(--color-text-muted)]">{new Date(item.occurredAt).toLocaleString()}</p>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      ) : null}
    </section>
  )
}
