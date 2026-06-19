import type { PagedResult, PlatformAuditEventTimelineItem } from '../../../api/types'

type Props = {
  timeline: PagedResult<PlatformAuditEventTimelineItem> | undefined
  isLoading: boolean
  page: number
  onPreviousPage: () => void
  onNextPage: () => void
}

export function AuditExportTimelineCard({
  timeline,
  isLoading,
  page,
  onPreviousPage,
  onNextPage,
}: Props) {
  return (
    <div
      data-testid="platform-audit-timeline-section"
      className="rounded-lg border border-slate-800 bg-slate-950/50 p-4"
    >
      <div className="flex items-center justify-between gap-3">
        <h3 className="text-sm font-medium text-slate-200">Audit timeline preview</h3>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={onPreviousPage}
            disabled={page <= 1 || isLoading}
            className="rounded-md border border-slate-700 px-2 py-1 text-xs text-slate-300 disabled:opacity-50"
          >
            Previous
          </button>
          <span className="text-xs text-[var(--color-text-muted)]">Page {page}</span>
          <button
            type="button"
            onClick={onNextPage}
            disabled={!timeline?.hasNextPage || isLoading}
            className="rounded-md border border-slate-700 px-2 py-1 text-xs text-slate-300 disabled:opacity-50"
          >
            Next
          </button>
        </div>
      </div>
      {isLoading ? (
        <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading audit timeline…</p>
      ) : timeline && timeline.items.length === 0 ? (
        <p className="mt-3 text-sm text-[var(--color-text-muted)]">No audit events match these filters.</p>
      ) : timeline ? (
        <ul className="mt-3 divide-y divide-slate-800 text-sm">
          {timeline.items.map((item) => (
            <li key={item.auditEventId} className="py-2">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <span className="font-mono text-teal-300">{item.action}</span>
                <span className="text-xs text-[var(--color-text-muted)]">
                  {new Date(item.occurredAt).toLocaleString()}
                </span>
              </div>
              <p className="text-xs text-slate-400">
                {item.targetType}
                {item.targetId ? ` · ${item.targetId}` : ''} · {item.result}
                {item.tenantId ? ` · tenant ${item.tenantId}` : ''}
              </p>
            </li>
          ))}
        </ul>
      ) : null}
    </div>
  )
}
