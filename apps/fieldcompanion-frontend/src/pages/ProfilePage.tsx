import { useState } from 'react'
import { LogOut, Smartphone, Trash2 } from 'lucide-react'
import { PageHeader, buildProductLaunchUrlMap, resolveSuiteHomeUrl } from '@stl/shared-ui'

import { renewFieldCompanionSession } from '../api/client'
import { clearSession } from '../auth/sessionStorage'
import { DeviceCapabilityPanel } from '../components/DeviceCapabilityPanel'
import { useFieldCompanionWorkspace } from '../hooks/useFieldCompanionWorkspace'
import { useOfflineQueue } from '../hooks/useOfflineQueue'
import { productTitle } from '../lib/fieldInbox'
import { formatWhen } from '../lib/fieldInbox'
import {
  FIELD_COMPANION_SESSION_REFRESH_WARNING_MINUTES,
  summarizeFieldCompanionSession,
} from '../lib/sessionSafety'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)

export function ProfilePage() {
  const { session, meQuery, accessToken } = useFieldCompanionWorkspace()
  const offlineQueue = useOfflineQueue(accessToken)
  const [isCleanupConfirming, setIsCleanupConfirming] = useState(false)
  const [isRefreshingSession, setIsRefreshingSession] = useState(false)
  const [, bumpSessionRefreshTick] = useState(0)

  const sessionHealth = session ? summarizeFieldCompanionSession(session) : null

  if (!session || !meQuery.data) {
    return <p className="text-sm text-slate-400">Loading profile…</p>
  }

  const clearThisDevice = () => {
    clearSession()
    window.location.assign(suiteHomeUrl)
  }

  const availableProducts = meQuery.data.fieldProductKeys ?? []

  const handleRefreshSession = async () => {
    if (isRefreshingSession) {
      return
    }

    setIsRefreshingSession(true)
    try {
      await renewFieldCompanionSession()
    } finally {
      setIsRefreshingSession(false)
      bumpSessionRefreshTick((value) => value + 1)
    }
  }

  return (
    <div className="mx-auto max-w-4xl space-y-5">
      <PageHeader
        title="Profile / readiness"
        subtitle="Session, workspace context, and device readiness for the current field worker."
      />

      <section className="grid gap-4 lg:grid-cols-[1fr_0.9fr]">
        <div className="rounded-2xl border border-slate-700 bg-slate-900/80 p-5">
          <div className="flex items-center gap-2">
            <Smartphone className="h-5 w-5 text-teal-300" aria-hidden />
            <h2 className="text-lg font-semibold text-white">Session summary</h2>
          </div>
          {sessionHealth ? (
            <div
              className={`mt-4 rounded-xl border px-4 py-3 text-sm ${
                sessionHealth.tone === 'danger'
                  ? 'border-rose-500/40 bg-rose-950/30 text-rose-50'
                  : sessionHealth.tone === 'warning'
                    ? 'border-amber-500/40 bg-amber-950/30 text-amber-50'
                    : 'border-emerald-500/30 bg-emerald-950/20 text-emerald-50'
              }`}
              data-testid="fieldcompanion-session-status"
            >
              <div className="flex flex-wrap items-start justify-between gap-2">
                <div>
                  <p className="font-semibold">{sessionHealth.statusLabel}</p>
                  <p className="mt-1 text-xs text-current/80">
                    Short-lived access stays in memory only and is renewed through the current NexArr session.
                  </p>
                </div>
                <span className="rounded-full border border-current/20 px-2.5 py-1 text-[10px] font-semibold uppercase tracking-wide">
                  Refresh window {sessionHealth.warningWindowLabel}
                </span>
              </div>
              <p className="mt-3 text-sm text-current/90">
                {sessionHealth.isAccessExpired
                  ? `Access expired ${formatWhen(session.accessTokenExpiresAt)}.`
                  : `Access expires ${formatWhen(session.accessTokenExpiresAt)}.`}
                {sessionHealth.isAccessExpiringSoon
                  ? ` Refresh before the next ${FIELD_COMPANION_SESSION_REFRESH_WARNING_MINUTES} minutes pass to avoid a forced renew.`
                  : ''}
              </p>
              <div className="mt-4 flex flex-wrap gap-2">
                <button
                  type="button"
                  className="inline-flex min-h-11 items-center rounded-lg border border-current/30 px-4 py-2 text-sm font-medium hover:bg-black/5 disabled:cursor-not-allowed disabled:opacity-60"
                  onClick={() => void handleRefreshSession()}
                  disabled={isRefreshingSession}
                >
                  {isRefreshingSession ? 'Refreshing session…' : 'Refresh session'}
                </button>
              </div>
            </div>
          ) : null}
          <dl className="mt-4 grid gap-3 sm:grid-cols-2">
            <Detail label="Display name" value={session.displayName} />
            <Detail label="Email" value={session.email} />
            <Detail label="Tenant" value={session.tenantDisplayName} />
            <Detail label="Tenant slug" value={session.tenantSlug} />
            <Detail label="Role" value={session.tenantRoleKey} />
            <Detail label="Access expires" value={formatWhen(session.accessTokenExpiresAt)} />
            <Detail label="Access status" value={sessionHealth?.statusLabel ?? 'Unknown'} />
          </dl>

          <details className="mt-4 rounded-xl border border-slate-800 bg-slate-950/40 p-4 text-sm text-slate-300">
            <summary className="cursor-pointer text-slate-100">Advanced session details</summary>
            <div className="mt-3 grid gap-3 sm:grid-cols-2">
              <Detail label="Person ID" value={session.personId} />
              <Detail label="Session ID" value={meQuery.data ? `${session.userId} · ${session.tenantId}` : session.userId} />
            </div>
          </details>

          <div className="mt-5">
            <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Available product workspaces</p>
            <div className="mt-2 flex flex-wrap gap-2">
              {availableProducts.length > 0 ? (
                availableProducts.map((productKey) => (
                  <span
                    key={productKey}
                    className="rounded-full border border-slate-700 bg-slate-950/60 px-3 py-1 text-xs text-slate-200"
                  >
                    {productTitle(productKey)}
                  </span>
                ))
              ) : (
                <span className="text-sm text-slate-400">
                  Product workspace listings are temporarily unavailable right now.
                </span>
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

        <div className="space-y-4">
          <DeviceCapabilityPanel />
          <div className="rounded-xl border border-slate-800 bg-slate-950/60 p-4 text-sm text-slate-300">
            <div className="flex items-center gap-2">
              <Trash2 className="h-5 w-5 text-rose-300" aria-hidden />
              <p className="font-semibold text-white">Device cleanup</p>
            </div>
            <p className="mt-2 text-slate-400">
              Clear this device to remove the local Field Companion session, offline queue, submission toasts,
              and push subscription references from this browser or installed app.
            </p>
            <dl className="mt-4 grid gap-3 sm:grid-cols-2">
              <Detail label="Cleanup state" value={isCleanupConfirming ? 'Confirmation pending' : 'Ready'} />
              <Detail label="Pending offline work" value={`${offlineQueue.pendingCount} item${offlineQueue.pendingCount === 1 ? '' : 's'}`} />
              <Detail label="Last sync" value={offlineQueue.lastSyncedAt ? formatWhen(offlineQueue.lastSyncedAt) : 'Never'} />
              <Detail label="Current session" value={`${session.displayName} · ${session.tenantDisplayName}`} />
            </dl>
            {offlineQueue.lastSyncError && (
              <p className="mt-4 rounded-lg border border-amber-500/40 bg-amber-950/30 px-3 py-2 text-amber-100">
                Last sync issue: {offlineQueue.lastSyncError}
              </p>
            )}
            {isCleanupConfirming ? (
              <div className="mt-4 rounded-lg border border-rose-500/40 bg-rose-950/30 p-3 text-rose-100">
                <p className="text-sm font-medium">This removes local Field Companion data from this device only.</p>
                <p className="mt-1 text-sm text-rose-100/80">
                  Committed records remain in the owning products. Unsynced work on this device is cleared.
                </p>
              </div>
            ) : null}
            <div className="mt-4 flex flex-wrap gap-2">
              {isCleanupConfirming ? (
                <>
                  <button
                    type="button"
                    className="inline-flex min-h-11 items-center rounded-lg border border-slate-600 px-4 py-2 text-sm font-medium text-slate-100 hover:border-teal-500"
                    onClick={() => setIsCleanupConfirming(false)}
                  >
                    Cancel
                  </button>
                  <button
                    type="button"
                    className="inline-flex min-h-11 items-center rounded-lg border border-rose-500/40 px-4 py-2 text-sm font-medium text-rose-100 hover:border-rose-400"
                    onClick={clearThisDevice}
                  >
                    Confirm clear
                  </button>
                </>
              ) : (
                <button
                  type="button"
                  className="inline-flex min-h-11 items-center rounded-lg border border-rose-500/40 px-4 py-2 text-sm font-medium text-rose-100 hover:border-rose-400"
                  onClick={() => setIsCleanupConfirming(true)}
                >
                  Clear this device
                </button>
              )}
            </div>
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
