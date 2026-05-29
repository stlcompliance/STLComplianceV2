import type { QualificationIssueListItemResponse } from '../api/types'

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
}: QualificationManagementPanelProps) {
  const selected = issues.find((i) => i.qualificationIssueId === selectedIssueId) ?? null

  if (!canManage) {
    return (
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Qualification management</h2>
        <p className="mt-3 text-sm text-slate-400">Lifecycle actions require trainarr qualifications manage access.</p>
      </section>
    )
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/60 p-4"
      data-testid="qualification-management-panel"
    >
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Qualification management</h2>
      <p className="mt-1 text-xs text-slate-500">Issue, suspend, revoke, and expire qualifications across assignments.</p>

      <label className="mt-4 block text-xs text-slate-400">
        Status filter
        <select
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
        <p className="mt-4 text-sm text-slate-500">No qualification issues match this filter.</p>
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
                <p className="mt-1 font-mono text-xs text-slate-500">{issue.staffarrPersonId}</p>
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
          <label className="grid gap-1 text-xs text-slate-400">
            Lifecycle reason (optional)
            <textarea
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
        </div>
      ) : null}
    </section>
  )
}
