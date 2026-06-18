import { useState } from 'react'

import { ApiErrorCallout } from '@stl/shared-ui'

import type {
  MyTeamDashboardResponse,
  MyTeamMemberResponse,
  PersonnelUpdateRequestResponse,
  ReviewPersonnelUpdateRequest,
} from '../api/types'

interface MyTeamPanelProps {
  dashboard: MyTeamDashboardResponse | null
  isLoading: boolean
  errorMessage: string | null
  reviewingRequestId?: string | null
  reviewErrorMessage?: string | null
  selectedPersonId?: string | null
  onReviewRequest?: (requestId: string, review: ReviewPersonnelUpdateRequest) => Promise<void>
  onSelectPerson?: (personId: string) => void
}

function readinessLabel(status: string): string {
  return status === 'ready' ? 'Ready' : 'Not ready'
}

function readinessClass(status: string): string {
  return status === 'ready' ? 'text-emerald-300' : 'text-rose-300'
}

function formatRequestType(requestType: string): string {
  return requestType.replaceAll('_', ' ')
}

const panelClassName =
  'rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)]'
const cardClassName =
  'rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)]'
const cardHighlightClassName =
  'rounded-lg border border-[var(--color-accent-border)] bg-[var(--color-accent-soft)]'
const panelHeadingClassName = 'text-sm font-medium text-[var(--color-text-secondary)]'
const panelCopyClassName = 'text-sm text-[var(--color-text-muted)]'
const primaryTextClassName = 'text-[var(--color-text-primary)]'
const secondaryTextClassName = 'text-[var(--color-text-secondary)]'
const mutedTextClassName = 'text-[var(--color-text-muted)]'

function DashboardMetric({
  label,
  value,
  testId,
  highlight = false,
}: {
  label: string
  value: number
  testId: string
  highlight?: boolean
}) {
  return (
    <div
      className={`${highlight && value > 0 ? cardHighlightClassName : cardClassName} px-4 py-3`}
      data-testid={testId}
    >
      <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</p>
      <p className={`mt-1 text-2xl font-semibold ${primaryTextClassName}`}>{value}</p>
    </div>
  )
}

function MemberRow({
  member,
  selected,
  onSelectPerson,
}: {
  member: MyTeamMemberResponse
  selected: boolean
  onSelectPerson?: (personId: string) => void
}) {
  const { summary } = member
  const hasAttention =
    member.readinessStatus !== 'ready' ||
    member.openIncidentCount > 0 ||
    member.pendingUpdateRequestCount > 0 ||
    member.missingCertificationCount > 0 ||
    member.expiringCertificationCount > 0 ||
    member.pendingTrainingBlockerCount > 0

  return (
    <tr
      className={`${hasAttention ? 'bg-[var(--color-accent-soft)]' : ''} ${selected ? 'ring-1 ring-[var(--color-accent-border)]' : ''}`}
      data-testid={`my-team-member-${summary.personId}`}
    >
      <td className="py-3 pr-4">
        <div className={`font-medium ${primaryTextClassName}`}>{summary.displayName}</div>
        <div className={`text-xs ${mutedTextClassName}`}>{summary.primaryEmail}</div>
      </td>
      <td className={`py-3 pr-4 ${secondaryTextClassName}`}>{summary.jobTitle ?? '—'}</td>
      <td className={`py-3 pr-4 ${secondaryTextClassName}`}>
        {summary.activeAssignmentPath ?? summary.primaryOrgUnitName ?? '—'}
      </td>
      <td className={`py-3 pr-4 font-medium ${readinessClass(member.readinessStatus)}`}>
        {readinessLabel(member.readinessStatus)}
        {member.blockerCount > 0 ? (
          <span className={`ml-1 text-xs ${mutedTextClassName}`}>({member.blockerCount} blockers)</span>
        ) : null}
      </td>
      <td className={`py-3 pr-4 ${secondaryTextClassName}`}>{member.missingCertificationCount || '—'}</td>
      <td className={`py-3 pr-4 ${secondaryTextClassName}`}>{member.expiringCertificationCount || '—'}</td>
      <td className={`py-3 pr-4 ${secondaryTextClassName}`}>{member.openIncidentCount || '—'}</td>
      <td className={`py-3 pr-4 ${secondaryTextClassName}`}>{member.pendingUpdateRequestCount || '—'}</td>
      <td className={`py-3 ${secondaryTextClassName}`}>{member.pendingTrainingBlockerCount || '—'}</td>
      <td className="py-3 pl-4 text-right">
        {onSelectPerson ? (
          <button
            type="button"
            onClick={() => onSelectPerson(summary.personId)}
            className="rounded-md border border-[var(--color-border-subtle)] px-2 py-1 text-xs text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-surface-elevated)]"
          >
            {selected ? 'Selected' : 'View readiness'}
          </button>
        ) : null}
      </td>
    </tr>
  )
}

function PendingRequestRow({
  request,
  memberNameByPersonId,
  isReviewing,
  reviewErrorMessage,
  onReviewRequest,
}: {
  request: PersonnelUpdateRequestResponse
  memberNameByPersonId: Map<string, string>
  isReviewing: boolean
  reviewErrorMessage: string | null
  onReviewRequest?: (requestId: string, review: ReviewPersonnelUpdateRequest) => Promise<void>
}) {
  const [reviewNotes, setReviewNotes] = useState('')
  const [applyToProfile, setApplyToProfile] = useState(true)

  const handleReview = async (decision: 'approve' | 'deny') => {
    if (!onReviewRequest) {
      return
    }

    await onReviewRequest(request.requestId, {
      decision,
      reviewNotes: reviewNotes.trim() || null,
      applyToProfile: decision === 'approve' ? applyToProfile : false,
    })
    setReviewNotes('')
  }

  return (
    <li
      className={`${cardClassName} px-4 py-3`}
      data-testid={`my-team-pending-request-${request.requestId}`}
    >
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <p className={`font-medium ${primaryTextClassName}`}>
            {memberNameByPersonId.get(request.personId) ?? 'Team member'}
          </p>
          <p className={`text-sm ${secondaryTextClassName}`}>
            {formatRequestType(request.requestType)} · {request.fieldKey}
          </p>
        </div>
        <span className="rounded-full bg-amber-500/15 px-2 py-0.5 text-xs text-amber-200">
          {request.status.replaceAll('_', ' ')}
        </span>
      </div>

      <p className={`mt-2 text-sm ${secondaryTextClassName}`}>
        {request.currentValue ? (
          <>
            Current: <span className={mutedTextClassName}>{request.currentValue}</span>
            {' · '}
          </>
        ) : null}
        Requested: <span className={primaryTextClassName}>{request.requestedValue}</span>
      </p>

      {request.details ? <p className={`mt-1 text-xs ${mutedTextClassName}`}>{request.details}</p> : null}

      {onReviewRequest ? (
        <div className="mt-4 space-y-3 border-t border-[var(--color-border-subtle)] pt-4">
          <label className="block text-sm">
            <span className={secondaryTextClassName}>Review notes (optional)</span>
            <textarea
              rows={2}
              className="mt-1 w-full rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
              value={reviewNotes}
              onChange={(event) => setReviewNotes(event.target.value)}
              data-testid={`my-team-review-notes-${request.requestId}`}
            />
          </label>

          <label className={`flex items-center gap-2 text-sm ${secondaryTextClassName}`}>
            <input
              type="checkbox"
              checked={applyToProfile}
              onChange={(event) => setApplyToProfile(event.target.checked)}
              data-testid={`my-team-apply-profile-${request.requestId}`}
            />
            Apply approved change to workforce profile
          </label>

          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              disabled={isReviewing}
              onClick={() => void handleReview('approve')}
              className="rounded-lg bg-emerald-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-emerald-500 disabled:opacity-50"
              data-testid={`my-team-approve-${request.requestId}`}
            >
              {isReviewing ? 'Reviewing…' : 'Approve'}
            </button>

            <button
              type="button"
              disabled={isReviewing}
              onClick={() => void handleReview('deny')}
              className="rounded-lg border border-rose-500/50 px-3 py-1.5 text-sm font-medium text-rose-700 hover:bg-rose-500/10 disabled:opacity-50 dark:text-rose-200 dark:hover:bg-rose-950/40"
              data-testid={`my-team-deny-${request.requestId}`}
            >
              Deny
            </button>
          </div>

          {reviewErrorMessage ? (
            <ApiErrorCallout title="Request review failed" message={reviewErrorMessage} />
          ) : null}
        </div>
      ) : null}
    </li>
  )
}

export function MyTeamPanel({
  dashboard,
  isLoading,
  errorMessage,
  reviewingRequestId = null,
  reviewErrorMessage = null,
  selectedPersonId = null,
  onReviewRequest,
  onSelectPerson,
}: MyTeamPanelProps) {
  if (isLoading) {
    return (
      <section className={`${panelClassName} p-6`}>
        <p className={`text-sm ${panelCopyClassName}`}>Loading your team…</p>
      </section>
    )
  }

  if (errorMessage) {
    return (
      <section className="rounded-xl border border-rose-500/30 bg-rose-500/10 p-6">
        <ApiErrorCallout title="Team dashboard failed to load" message={errorMessage} />
      </section>
    )
  }

  if (!dashboard) {
    return (
      <section className={`${panelClassName} p-6`}>
        <ApiErrorCallout
          title="Team dashboard unavailable"
          message="Could not load team dashboard data."
        />
      </section>
    )
  }

  const memberNameByPersonId = new Map(
    dashboard.members.map((member) => [member.summary.personId, member.summary.displayName]),
  )

  return (
    <div className="space-y-6" data-testid="my-team-panel">
      <section className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        <DashboardMetric
          label="Direct reports"
          value={dashboard.directReportCount}
          testId="my-team-metric-headcount"
        />

        <DashboardMetric
          label="Not ready"
          value={dashboard.notReadyCount}
          testId="my-team-metric-not-ready"
          highlight
        />

        <DashboardMetric
          label="Missing certs"
          value={dashboard.missingCertificationCount}
          testId="my-team-metric-missing-certs"
          highlight
        />

        <DashboardMetric
          label="Certs expiring soon"
          value={dashboard.expiringCertificationCount}
          testId="my-team-metric-expiring-certs"
          highlight
        />

        <DashboardMetric
          label="Open incidents"
          value={dashboard.openIncidentCount}
          testId="my-team-metric-open-incidents"
          highlight
        />

        <DashboardMetric
          label="Pending update requests"
          value={dashboard.pendingUpdateRequestCount}
          testId="my-team-metric-pending-requests"
          highlight
        />

        <DashboardMetric
          label="Onboarding in progress"
          value={dashboard.onboardingInProgressCount}
          testId="my-team-metric-onboarding"
        />

        <DashboardMetric
          label="Training blockers"
          value={dashboard.pendingTrainingBlockerCount}
          testId="my-team-metric-training-blockers"
          highlight
        />
      </section>

      {dashboard.directReportCount === 0 ? (
        <section className={`${panelClassName} p-6`}>
          <h2 className={panelHeadingClassName}>Direct reports</h2>
          <p className={`mt-2 text-sm ${panelCopyClassName}`}>
            You do not have any direct reports assigned in StaffArr yet.
          </p>
        </section>
      ) : (
        <section className={`${panelClassName} p-6`}>
          <h2 className={panelHeadingClassName}>Direct reports</h2>
          <div className="mt-4 overflow-x-auto">
            <table className="min-w-full text-left text-sm" data-testid="my-team-members-table">
              <thead className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                <tr>
                  <th className="pb-2 pr-4 font-medium">Name</th>
                  <th className="pb-2 pr-4 font-medium">Title</th>
                  <th className="pb-2 pr-4 font-medium">Assignment</th>
                  <th className="pb-2 pr-4 font-medium">Readiness</th>
                  <th className="pb-2 pr-4 font-medium">Missing certs</th>
                  <th className="pb-2 pr-4 font-medium">Expiring certs</th>
                  <th className="pb-2 pr-4 font-medium">Incidents</th>
                  <th className="pb-2 pr-4 font-medium">Update reqs</th>
                  <th className="pb-2 font-medium">Training</th>
                  <th className="pb-2 pl-4 font-medium text-right">Details</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[var(--color-border-subtle)]">
                {dashboard.members.map((member) => (
                  <MemberRow
                    key={member.summary.personId}
                    member={member}
                    selected={member.summary.personId === selectedPersonId}
                    onSelectPerson={onSelectPerson}
                  />
                ))}
              </tbody>
            </table>
          </div>
        </section>
      )}

      {dashboard.pendingUpdateRequests.length > 0 ? (
        <section className={`${panelClassName} p-6`}>
          <h2 className={panelHeadingClassName}>Pending personnel update requests</h2>
          <p className={`mt-1 text-xs ${mutedTextClassName}`}>
            Review self-service profile changes from your direct reports.
          </p>
          <ul className="mt-4 space-y-3">
            {dashboard.pendingUpdateRequests.map((request) => (
              <PendingRequestRow
                key={request.requestId}
                request={request}
                memberNameByPersonId={memberNameByPersonId}
                isReviewing={reviewingRequestId === request.requestId}
                reviewErrorMessage={
                  reviewingRequestId === request.requestId ? reviewErrorMessage ?? null : null
                }
                onReviewRequest={onReviewRequest}
              />
            ))}
          </ul>
        </section>
      ) : null}
    </div>
  )
}
