import { LogOut, Smartphone } from 'lucide-react'
import { PageHeader, buildProductLaunchUrlMap, resolveSuiteHomeUrl } from '@stl/shared-ui'

import { clearSession } from '../auth/sessionStorage'
import { useFieldCompanionWorkspace } from '../hooks/useFieldCompanionWorkspace'
import { useOfflineQueue } from '../hooks/useOfflineQueue'
import { getPushPermissionState, isWebPushSupported, pushReadinessLabel } from '../lib/pushNotifications'
import { formatWhen } from '../lib/fieldInbox'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)

export function ProfilePage() {
  const { session, meQuery, accessToken } = useFieldCompanionWorkspace()
  const offlineQueue = useOfflineQueue(accessToken)
  const pushPermission = getPushPermissionState()

  if (!session || !meQuery.data) {
    return <p className="text-sm text-slate-400">Loading profile…</p>
  }

  const deviceSummary = [
    { label: 'Browser', value: navigator.userAgent || 'Unknown' },
    { label: 'Platform', value: navigator.platform || 'Unknown' },
    { label: 'Language', value: navigator.language || 'Unknown' },
    { label: 'Online', value: navigator.onLine ? 'Online' : 'Offline' },
    { label: 'Web push', value: pushReadinessLabel(pushPermission) },
    { label: 'Web push supported', value: isWebPushSupported() ? 'Yes' : 'No' },
    { label: 'Local storage', value: typeof window !== 'undefined' ? 'Available' : 'Unknown' },
  ]

  return (
    <div className="mx-auto max-w-4xl space-y-5">
      <PageHeader
        title="Profile / readiness"
        subtitle="Session, entitlement, and device context for the current field worker."
      />

      <section className="grid gap-4 lg:grid-cols-[1fr_0.9fr]">
        <div className="rounded-2xl border border-slate-700 bg-slate-900/80 p-5">
          <div className="flex items-center gap-2">
            <Smartphone className="h-5 w-5 text-teal-300" aria-hidden />
            <h2 className="text-lg font-semibold text-white">Session summary</h2>
          </div>
          <dl className="mt-4 grid gap-3 sm:grid-cols-2">
            <Detail label="Display name" value={session.displayName} />
            <Detail label="Email" value={session.email} />
            <Detail label="Tenant" value={session.tenantDisplayName} />
            <Detail label="Tenant slug" value={session.tenantSlug} />
            <Detail label="Role" value={session.tenantRoleKey} />
            <Detail label="Person ID" value={session.personId} />
            <Detail label="Session ID" value={meQuery.data ? `${session.userId} · ${session.tenantId}` : session.userId} />
            <Detail label="Access expires" value={formatWhen(session.accessTokenExpiresAt)} />
          </dl>

          <div className="mt-5">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Entitlements</p>
            <div className="mt-2 flex flex-wrap gap-2">
              {session.entitlements.length > 0 ? (
                session.entitlements.map((entitlement) => (
                  <span
                    key={entitlement}
                    className="rounded-full border border-slate-700 bg-slate-950/60 px-3 py-1 text-xs text-slate-200"
                  >
                    {entitlement}
                  </span>
                ))
              ) : (
                <span className="text-sm text-slate-400">No entitlement snapshot returned yet.</span>
              )}
            </div>
          </div>

          <div className="mt-5 flex flex-wrap gap-2">
            <button
              type="button"
              className="inline-flex min-h-11 items-center gap-2 rounded-lg border border-slate-600 px-4 py-2 text-sm font-medium text-slate-100 hover:border-teal-500"
              onClick={() => {
                clearSession()
                window.location.assign(suiteHomeUrl)
              }}
            >
              <LogOut className="h-4 w-4" aria-hidden />
              Sign out
            </button>
            <a
              href={productLaunchUrls.fieldcompanion ?? '/'}
              className="inline-flex min-h-11 items-center rounded-lg bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500"
            >
              Keep working
            </a>
          </div>
        </div>

        <div className="rounded-2xl border border-slate-700 bg-slate-900/80 p-5">
          <h2 className="text-lg font-semibold text-white">Device readiness</h2>
          <div className="mt-4 space-y-3">
            {deviceSummary.map((item) => (
              <div key={item.label} className="rounded-xl border border-slate-800 bg-slate-950/60 px-4 py-3">
                <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{item.label}</p>
                <p className="mt-1 break-words text-sm text-slate-200">{item.value}</p>
              </div>
            ))}
          </div>

          <div className="mt-5 rounded-xl border border-slate-800 bg-slate-950/60 p-4 text-sm text-slate-300">
            <p className="font-semibold text-white">Offline status</p>
            <p className="mt-2 text-slate-400">
              {offlineQueue.pendingCount} pending offline action
              {offlineQueue.pendingCount === 1 ? '' : 's'}.
            </p>
            <p className="mt-1 text-slate-400">
              Last sync {offlineQueue.lastSyncedAt ? formatWhen(offlineQueue.lastSyncedAt) : 'never'}.
            </p>
            {offlineQueue.lastSyncError && (
              <p className="mt-2 rounded-lg border border-amber-500/40 bg-amber-950/30 px-3 py-2 text-amber-100">
                {offlineQueue.lastSyncError}
              </p>
            )}
          </div>
        </div>
      </section>
    </div>
  )
}

function Detail({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-xl border border-slate-800 bg-slate-950/60 px-4 py-3">
      <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</dt>
      <dd className="mt-1 break-words text-sm text-slate-100">{value}</dd>
    </div>
  )
}
