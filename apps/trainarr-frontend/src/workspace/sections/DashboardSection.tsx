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
    <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-3">
      <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</p>
      <p className="mt-1 text-2xl font-semibold text-[var(--color-text-primary)]">{value}</p>
    </div>
  )
}

function SectionCard({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">{title}</h2>
      <div className="mt-3">{children}</div>
    </section>
  )
}

export function DashboardSection({ state }: Props) {
  const dashboardQuery = state.personalDashboardQuery

  if (dashboardQuery.isLoading) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading My Training…</p>
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
    return <p className="text-sm text-[var(--color-text-muted)]">No dashboard data is available yet.</p>
  }

  const inboxItems = dashboard.fieldInbox.items
  const blockedInboxItems = inboxItems.filter((item) => item.blockedReason)

  return (
    <div className="space-y-6">
      <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-gradient-to-br from-[var(--color-bg-surface)] to-[var(--color-bg-surface-elevated)] p-5 shadow-xl shadow-slate-950/10">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <h1 className="text-2xl font-semibold text-[var(--color-text-primary)]">My Training</h1>
            <p className="mt-1 max-w-3xl text-sm text-[var(--color-text-secondary)]">
              Your current courses, assignments, qualifications, field inbox, and recent learning activity.
            </p>
          </div>
          <p className="rounded-full border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-1 text-xs text-[var(--color-text-secondary)]">
            Updated {formatDateTime(dashboard.generatedAt)}
          </p>
        </div>

        <p className="mt-4 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-xs text-[var(--color-text-muted)]">
              Scope note: the field inbox can surface read-only signals from people, dispatch, maintenance, supply, and compliance workflows. This view shows training records and assignment state.
        </p>

        <div className="mt-4 grid gap-3 md:grid-cols-2 xl:grid-cols-4">
          <MetricCard label="Active assignments" value={String(dashboard.summary.activeAssignmentCount)} />
          <MetricCard label="Overdue assignments" value={String(dashboard.summary.overdueAssignmentCount)} />
          <MetricCard label="Certificates" value={String(dashboard.summary.qualificationCount)} />
          <MetricCard label="Expiring soon" value={String(dashboard.summary.expiringQualificationCount)} />
        </div>

        <div className="mt-4 flex flex-wrap gap-2 text-sm">
          <Link className="rounded bg-[var(--color-accent)] px-3 py-1.5 text-white hover:bg-[var(--color-accent-hover)]" to="/assignments">
            Resume course player
          </Link>
          <Link className="rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-1.5 text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)]" to="/qualifications">
            Review certificates
          </Link>
          <Link className="rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-1.5 text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)]" to="/settings">
            Manage training settings
          </Link>
        </div>
      </section>

      <div className="grid gap-6 xl:grid-cols-2">
        <SectionCard title="Assigned learning">
          {dashboard.assignedTraining.length === 0 ? (
            <p className="text-sm text-[var(--color-text-muted)]">No assigned learning yet.</p>
          ) : (
            <ul className="space-y-2">
              {dashboard.assignedTraining.slice(0, 6).map((assignment) => (
                <li key={assignment.assignmentId} className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-3">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <Link className="font-medium text-[var(--color-text-primary)] hover:text-[var(--color-link-text)]" to={`/assignments/${assignment.assignmentId}`}>
                        {assignment.trainingDefinitionName}
                      </Link>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {assignment.assignmentReason.replace(/_/g, ' ')} · {assignment.qualificationKey}
                      </p>
                    </div>
                    <span className="rounded bg-[var(--color-bg-control-hover)] px-2 py-0.5 text-xs text-[var(--color-text-primary)]">
                      {assignment.status.replace(/_/g, ' ')}
                    </span>
                  </div>
                  <p className="mt-2 text-xs text-[var(--color-text-muted)]">
                    Due {formatDateTime(assignment.dueAt)} · Person assigned
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
            <div className="mt-3 flex flex-wrap gap-2 text-xs text-[var(--color-text-secondary)]">
              {Object.entries(dashboard.fieldInbox.summary.countByProduct).map(([productKey, count]) => (
                <span key={productKey} className="rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-2 py-1">
                  {productKey}: {count}
                </span>
              ))}
            </div>
          ) : null}
          {inboxItems.length === 0 ? (
            <p className="mt-3 text-sm text-[var(--color-text-muted)]">No field inbox tasks are pending.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {inboxItems.slice(0, 6).map((item) => (
                <li key={item.taskKey} className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-3 text-sm">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <p className="font-medium text-[var(--color-text-primary)]">{item.title}</p>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {item.productKey} · {item.taskType} · {item.status}
                      </p>
                    </div>
                    {item.deepLinkPath ? (
                      <Link className="text-xs text-[var(--color-link-text)] hover:underline" to={item.deepLinkPath}>
                        Open
                      </Link>
                    ) : null}
                  </div>
                  {item.subtitle ? <p className="mt-2 text-xs text-[var(--color-text-secondary)]">{item.subtitle}</p> : null}
                  {item.blockedReason ? (
                    <p className="mt-2 text-xs text-[var(--color-warning-text)]">{item.blockedReason}</p>
                  ) : null}
                  <p className="mt-2 text-xs text-[var(--color-text-muted)]">Due {formatDateTime(item.dueAt)}</p>
                </li>
              ))}
            </ul>
          )}
          {blockedInboxItems.length > 6 ? (
            <p className="mt-2 text-xs text-[var(--color-text-muted)]">Showing 6 of {blockedInboxItems.length} blocked tasks.</p>
          ) : null}
        </SectionCard>
      </div>

      <div className="grid gap-6 xl:grid-cols-2">
        <SectionCard title="Certificates">
          {dashboard.qualifications.length === 0 ? (
            <p className="text-sm text-[var(--color-text-muted)]">No certificates issued yet.</p>
          ) : (
            <ul className="space-y-2">
              {dashboard.qualifications.slice(0, 6).map((qualification) => (
                <li key={qualification.qualificationIssueId} className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-3">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <p className="font-medium text-[var(--color-text-primary)]">{qualification.qualificationName}</p>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">{qualification.qualificationKey}</p>
                    </div>
                    <span className="rounded bg-[var(--color-bg-control-hover)] px-2 py-0.5 text-xs text-[var(--color-text-primary)]">
                      {qualification.status}
                    </span>
                  </div>
                  <p className="mt-2 text-xs text-[var(--color-text-muted)]">Expires {formatDateTime(qualification.expiresAt)}</p>
                </li>
              ))}
            </ul>
          )}
        </SectionCard>

        <SectionCard title="Recent learning history">
          {dashboard.recentHistory.length === 0 ? (
            <p className="text-sm text-[var(--color-text-muted)]">No recent learning history yet.</p>
          ) : (
            <ul className="space-y-2">
              {dashboard.recentHistory.map((item) => (
                <li key={item.entryId} className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-3 text-sm">
                  <p className="font-medium text-[var(--color-text-primary)]">{item.summary}</p>
                  <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                    {item.eventKind} · {item.relatedEntityType}
                  </p>
                  <p className="mt-1 text-xs text-[var(--color-text-muted)]">{formatDateTime(item.occurredAt)}</p>
                </li>
              ))}
            </ul>
          )}
        </SectionCard>
      </div>
    </div>
  )
}
