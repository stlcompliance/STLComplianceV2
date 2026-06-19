import { useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import { getApprovalRemindersDashboard } from '../api/client'

interface ApprovalRemindersPanelProps {
  accessToken: string
  canRead: boolean
}

export function ApprovalRemindersPanel({ accessToken, canRead }: ApprovalRemindersPanelProps) {
  const dashboardQuery = useQuery({
    queryKey: ['supplyarr-approval-reminders', accessToken],
    queryFn: () => getApprovalRemindersDashboard(accessToken, false),
    enabled: canRead,
  })

  if (!canRead) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2"
      data-testid="approval-reminders-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Approval reminders</h2>
      <p className="mt-1 text-sm text-slate-400">
        Purchase requests and orders awaiting approval, including overdue items and reminder history.
      </p>

      {dashboardQuery.isLoading && (
        <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading approval reminders…</p>
      )}

      {dashboardQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Approval reminders unavailable"
            message={getErrorMessage(dashboardQuery.error, 'Failed to load approval reminders.')}
            retryLabel="Retry reminders"
            onRetry={() => {
              void dashboardQuery.refetch()
            }}
          />
        </div>
      )}

      {dashboardQuery.data && (
        <div className="mt-4">
          <div className="flex gap-4 text-sm text-slate-300">
            <span>{dashboardQuery.data.overdueCount} overdue</span>
            <span>{dashboardQuery.data.pendingCount} due for reminder</span>
          </div>

          {dashboardQuery.data.items.length === 0 && (
            <p className="mt-3 text-sm text-[var(--color-text-muted)]">No items awaiting approval.</p>
          )}

          {dashboardQuery.data.items.length > 0 && (
            <ul className="mt-3 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
              {dashboardQuery.data.items.map((item) => (
                <li key={`${item.subjectType}-${item.subjectId}`} className="px-3 py-2">
                  <div className="font-medium text-slate-100">
                    {item.documentKey} · {item.title}
                    {item.isOverdue ? (
                      <span className="ml-2 rounded bg-amber-900/60 px-2 py-0.5 text-xs text-amber-200">
                        overdue
                      </span>
                    ) : null}
                  </div>
                  <div className="text-xs text-[var(--color-text-muted)]">
                    {item.subjectType} · {Math.round(item.hoursPending)}h pending · {item.reminderCount} reminders sent
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </section>
  )
}
