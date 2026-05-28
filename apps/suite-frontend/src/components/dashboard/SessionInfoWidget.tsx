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
      <dl className="space-y-2 text-sm text-slate-300">
        <div>
          <dt className="text-xs font-medium uppercase tracking-wide text-slate-500">Signed in as</dt>
          <dd className="mt-0.5 font-medium text-white">{me.displayName}</dd>
          <dd className="text-xs text-slate-400">{me.email}</dd>
        </div>
        <div>
          <dt className="text-xs font-medium uppercase tracking-wide text-slate-500">Session</dt>
          <dd className="mt-0.5 font-mono text-xs text-slate-400">{summary.sessionId}</dd>
        </div>
        <div>
          <dt className="text-xs font-medium uppercase tracking-wide text-slate-500">
            Access token
          </dt>
          <dd className="mt-0.5 text-xs">
            Expires {new Date(summary.accessExpiresAt).toLocaleString()}
            {summary.accessExpiresInMinutes !== null && (
              <span className="text-slate-500">
                {' '}
                (~{summary.accessExpiresInMinutes} min)
              </span>
            )}
          </dd>
          {summary.isAccessExpiringSoon && (
            <dd className="mt-1 text-xs text-amber-300">
              Renewing automatically on the next API call.
            </dd>
          )}
        </div>
        {isPlatformAdmin(me) && (
          <div>
            <dt className="text-xs font-medium uppercase tracking-wide text-slate-500">Role</dt>
            <dd className="mt-0.5 text-xs font-medium text-teal-400">Platform administrator</dd>
          </div>
        )}
      </dl>
    </DashboardCard>
  )
}
