import { Link } from 'react-router-dom'
import type { MeResponse } from '../../api/types'
import type { StoredAuthSession } from '../../auth/authStorage'
import { summarizeSession } from '../../lib/dashboard'
import { isPlatformAdmin } from '../../lib/permissions'
import { DashboardCard } from './DashboardCard'

export function SessionInfoWidget({
  me,
  session,
}: {
  me: MeResponse
  session: StoredAuthSession
}) {
  const summary = summarizeSession(session)

  return (
    <DashboardCard title="Session">
      <dl className="space-y-2 text-sm text-[var(--color-text-secondary)]">
        <div>
          <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">Signed in as</dt>
          <dd className="mt-0.5 font-medium text-[var(--color-text-primary)]">{me.displayName}</dd>
          <dd className="text-xs text-[var(--color-text-muted)]">{me.email}</dd>
        </div>
        <div>
          <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">Session</dt>
          <dd className="mt-0.5 font-mono text-xs text-[var(--color-text-muted)]">{summary.sessionId}</dd>
        </div>
        <div>
          <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
            Access token
          </dt>
          <dd className="mt-0.5 text-xs">
            Expires {new Date(summary.accessExpiresAt).toLocaleString()}
            {summary.accessExpiresInMinutes !== null && (
              <span className="text-[var(--color-text-muted)]">
                {' '}
                (~{summary.accessExpiresInMinutes} min)
              </span>
            )}
          </dd>
          {summary.isAccessExpiringSoon && (
            <dd className="mt-1 text-xs text-[var(--color-warning-text)]">
              Renewing automatically on the next API call.
            </dd>
          )}
        </div>
        {isPlatformAdmin(me) && (
          <div>
            <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">Role</dt>
            <dd className="mt-0.5 text-xs font-medium text-[var(--color-accent)]">Platform administrator</dd>
          </div>
        )}
        <div>
          <Link
            to="/app/nexarr/identity"
            className="text-xs font-medium text-[var(--color-accent)] hover:text-[var(--color-accent-strong)]"
          >
            Identity & sessions
          </Link>
        </div>
      </dl>
    </DashboardCard>
  )
}
