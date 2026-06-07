import { Link } from 'react-router-dom'
import type { ReactNode } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }

function formatDateTime(value: string | null | undefined): string {
  if (!value) {
    return '—'
  }

  return new Date(value).toLocaleString()
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-slate-700 bg-slate-950/40 p-3">
      <p className="text-xs uppercase tracking-wide text-slate-500">{label}</p>
      <p className="mt-1 text-2xl font-semibold text-slate-100">{value}</p>
    </div>
  )
}

function SectionCard({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">{title}</h2>
      <div className="mt-3">{children}</div>
    </section>
  )
}

export function DashboardSection({ state }: Props) {
  const dashboardQuery = state.personalDashboardQuery

  if (dashboardQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading training dashboard…</p>
  }

  if (dashboardQuery.isError) {
    return (
      <ApiErrorCallout
        title="Training dashboard unavailable"
        message={getErrorMessage(dashboardQuery.error, 'Failed to load personal training dashboard.')}
        retryLabel="Retry dashboard"
        onRetry={() => {
          void dashboardQuery.refetch()
        }}
      />
    )
  }

  const dashboard = dashboardQuery.data
  if (!dashboard) {
    return <p className="text-sm text-slate-400">No dashboard data is available yet.</p>
  }

  const inboxItems = dashboard.fieldInbox.items
  const blockedInboxItems = inboxItems.filter((item) => item.blockedReason)

  return (
    <div className="space-y-6">
      <section className="rounded-2xl border border-slate-700 bg-gradient-to-br from-slate-900 to-slate-950 p-5 shadow-lg">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <h1 className="text-2xl font-semibold text-slate-50">TrainArr dashboard</h1>
            <p className="mt-1 max-w-3xl text-sm text-slate-300">
              Your current assignments, qualifications, field inbox, and recent training activity.
            </p>
          </div>
          <p className="rounded-full border border-slate-700 bg-slate-950/50 px-3 py-1 text-xs text-slate-300">
            Updated {formatDateTime(dashboard.generatedAt)}
          </p>
        </div>

        <p className="mt-4 rounded-lg border border-slate-700 bg-slate-950/50 px-3 py-2 text-xs text-slate-400">
          Scope note: the field inbox can surface read-only cross-product signals from StaffArr, RoutArr, MaintainArr, SupplyArr, and Compliance Core. TrainArr owns the training record and assignment state shown here.
        </p>

        <div className="mt-4 grid gap-3 md:grid-cols-2 xl:grid-cols-4">
          <MetricCard label="Active assignments" value={String(dashboard.summary.activeAssignmentCount)} />
          <MetricCard label="Overdue assignments" value={String(dashboard.summary.overdueAssignmentCount)} />
          <MetricCard label="Qualifications" value={String(dashboard.summary.qualificationCount)} />
          <MetricCard label="Expiring soon" value={String(dashboard.summary.expiringQualificationCount)} />
        </div>

        <div className="mt-4 flex flex-wrap gap-2 text-sm">
          <Link className="rounded bg-sky-700 px-3 py-1.5 text-sky-50 hover:bg-sky-600" to="/assignments">
            Open assignments
          </Link>
          <Link className="rounded border border-slate-600 px-3 py-1.5 text-slate-100 hover:bg-slate-800" to="/qualifications">
            Review qualifications
          </Link>
          <Link className="rounded border border-slate-600 px-3 py-1.5 text-slate-100 hover:bg-slate-800" to="/settings">
            Manage settings
          </Link>
        </div>
      </section>

      <div className="grid gap-6 xl:grid-cols-2">
        <SectionCard title="Assigned training">
          {dashboard.assignedTraining.length === 0 ? (
            <p className="text-sm text-slate-400">No assigned training yet.</p>
          ) : (
            <ul className="space-y-2">
              {dashboard.assignedTraining.slice(0, 6).map((assignment) => (
                <li key={assignment.assignmentId} className="rounded-lg border border-slate-700 bg-slate-950/40 p-3">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <Link className="font-medium text-slate-100 hover:text-sky-300" to={`/assignments/${assignment.assignmentId}`}>
                        {assignment.trainingDefinitionName}
                      </Link>
                      <p className="mt-1 text-xs text-slate-400">
                        {assignment.assignmentReason.replace(/_/g, ' ')} · {assignment.qualificationKey}
                      </p>
                    </div>
                    <span className="rounded bg-slate-800 px-2 py-0.5 text-xs text-slate-200">
                      {assignment.status.replace(/_/g, ' ')}
                    </span>
                  </div>
                  <p className="mt-2 text-xs text-slate-500">
                    Due {formatDateTime(assignment.dueAt)} · Person {assignment.staffarrPersonId}
                  </p>
                </li>
              ))}
            </ul>
          )}
        </SectionCard>

        <SectionCard title="Field inbox">
          <div className="grid gap-3 md:grid-cols-2">
            <MetricCard label="Total tasks" value={String(dashboard.fieldInbox.summary.totalCount)} />
            <MetricCard label="Blocked" value={String(dashboard.fieldInbox.summary.blockedCount)} />
          </div>
          {Object.keys(dashboard.fieldInbox.summary.countByProduct).length > 0 ? (
            <div className="mt-3 flex flex-wrap gap-2 text-xs text-slate-300">
              {Object.entries(dashboard.fieldInbox.summary.countByProduct).map(([productKey, count]) => (
                <span key={productKey} className="rounded border border-slate-700 bg-slate-950/40 px-2 py-1">
                  {productKey}: {count}
                </span>
              ))}
            </div>
          ) : null}
          {inboxItems.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No field inbox tasks are pending.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {inboxItems.slice(0, 6).map((item) => (
                <li key={item.taskKey} className="rounded-lg border border-slate-700 bg-slate-950/40 p-3 text-sm">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <p className="font-medium text-slate-100">{item.title}</p>
                      <p className="mt-1 text-xs text-slate-400">
                        {item.productKey} · {item.taskType} · {item.status}
                      </p>
                    </div>
                    {item.deepLinkPath ? (
                      <Link className="text-xs text-sky-300 hover:text-sky-200" to={item.deepLinkPath}>
                        Open
                      </Link>
                    ) : null}
                  </div>
                  {item.subtitle ? <p className="mt-2 text-xs text-slate-300">{item.subtitle}</p> : null}
                  {item.blockedReason ? (
                    <p className="mt-2 text-xs text-amber-300">{item.blockedReason}</p>
                  ) : null}
                  <p className="mt-2 text-xs text-slate-500">Due {formatDateTime(item.dueAt)}</p>
                </li>
              ))}
            </ul>
          )}
          {blockedInboxItems.length > 6 ? (
            <p className="mt-2 text-xs text-slate-500">Showing 6 of {blockedInboxItems.length} blocked tasks.</p>
          ) : null}
        </SectionCard>
      </div>

      <div className="grid gap-6 xl:grid-cols-2">
        <SectionCard title="Qualifications">
          {dashboard.qualifications.length === 0 ? (
            <p className="text-sm text-slate-400">No qualifications issued yet.</p>
          ) : (
            <ul className="space-y-2">
              {dashboard.qualifications.slice(0, 6).map((qualification) => (
                <li key={qualification.qualificationIssueId} className="rounded-lg border border-slate-700 bg-slate-950/40 p-3">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <p className="font-medium text-slate-100">{qualification.qualificationName}</p>
                      <p className="mt-1 text-xs text-slate-400">{qualification.qualificationKey}</p>
                    </div>
                    <span className="rounded bg-slate-800 px-2 py-0.5 text-xs text-slate-200">
                      {qualification.status}
                    </span>
                  </div>
                  <p className="mt-2 text-xs text-slate-500">Expires {formatDateTime(qualification.expiresAt)}</p>
                </li>
              ))}
            </ul>
          )}
        </SectionCard>

        <SectionCard title="Recent history">
          {dashboard.recentHistory.length === 0 ? (
            <p className="text-sm text-slate-400">No recent training history yet.</p>
          ) : (
            <ul className="space-y-2">
              {dashboard.recentHistory.map((item) => (
                <li key={item.entryId} className="rounded-lg border border-slate-700 bg-slate-950/40 p-3 text-sm">
                  <p className="font-medium text-slate-100">{item.summary}</p>
                  <p className="mt-1 text-xs text-slate-400">
                    {item.eventKind} · {item.relatedEntityType}
                  </p>
                  <p className="mt-1 text-xs text-slate-500">{formatDateTime(item.occurredAt)}</p>
                </li>
              ))}
            </ul>
          )}
        </SectionCard>
      </div>
    </div>
  )
}
