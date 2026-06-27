import { LockKeyhole, RefreshCcw, ShieldAlert } from 'lucide-react'

import type { QueuedOfflineAction } from '../lib/offlineQueue'

interface SharedDeviceProtectionOverlayProps {
  isVisible: boolean
  isWarning?: boolean
  userDisplayName: string
  tenantDisplayName: string
  tenantSlug: string
  pendingActions: QueuedOfflineAction[]
  onOpenOfflineQueue: () => void
  onReauthenticate: () => void
  onDiscardQueuedWorkAndSignOut: () => void
  onStaySignedIn?: () => void
}

export function SharedDeviceProtectionOverlay({
  isVisible,
  isWarning = false,
  userDisplayName,
  tenantDisplayName,
  tenantSlug,
  pendingActions,
  onOpenOfflineQueue,
  onReauthenticate,
  onDiscardQueuedWorkAndSignOut,
  onStaySignedIn,
}: SharedDeviceProtectionOverlayProps) {
  if (!isVisible) {
    return null
  }

  if (isWarning) {
    return (
      <div className="fixed inset-x-4 bottom-4 z-50 rounded-2xl border border-amber-500/30 bg-slate-950/95 p-4 text-slate-100 shadow-2xl shadow-black/50 backdrop-blur md:inset-x-auto md:right-6 md:w-[28rem]">
        <div className="flex items-start gap-3">
          <ShieldAlert className="mt-0.5 h-5 w-5 shrink-0 text-amber-300" aria-hidden />
          <div className="min-w-0">
            <p className="text-xs font-semibold uppercase tracking-wide text-amber-300">
              Shared device warning
            </p>
            <h2 className="mt-1 text-lg font-semibold text-white">Inactivity lock is coming</h2>
            <p className="mt-2 text-sm text-slate-300">
              This Field Companion session is being protected because shared device mode is active for
              {` ${tenantDisplayName}`}. Stay active to keep working, or sign out now if you are done.
            </p>
            <p className="mt-2 text-xs text-slate-400">
              Current session: {userDisplayName} · {tenantDisplayName} ({tenantSlug})
            </p>
            {pendingActions.length > 0 ? (
              <p className="mt-2 text-xs text-amber-200/80">
                You still have {pendingActions.length} queued action{pendingActions.length === 1 ? '' : 's'} on this device.
                Sign out will stay blocked until the queue is reviewed.
              </p>
            ) : null}
          </div>
        </div>

        <div className="mt-4 flex flex-wrap gap-2">
          <button
            type="button"
            className="inline-flex min-h-11 items-center rounded-lg bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500"
            onClick={onStaySignedIn}
          >
            Stay signed in
          </button>
          <button
            type="button"
            className="inline-flex min-h-11 items-center rounded-lg border border-slate-600 px-4 py-2 text-sm font-medium text-slate-100 hover:border-teal-500"
            onClick={onReauthenticate}
          >
            Sign out now
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/85 px-4 py-8 backdrop-blur-sm">
      <div className="w-full max-w-2xl rounded-3xl border border-slate-700 bg-slate-950 p-6 text-slate-100 shadow-2xl shadow-black/60">
        <div className="flex items-start gap-3">
          <LockKeyhole className="mt-0.5 h-6 w-6 shrink-0 text-teal-300" aria-hidden />
          <div className="min-w-0">
            <p className="text-xs font-semibold uppercase tracking-wide text-teal-300">Shared device mode</p>
            <h2 className="mt-1 text-2xl font-semibold text-white">Session locked</h2>
            <p className="mt-2 text-sm text-slate-300">
              This kiosk or shared device session locked after inactivity. Reauthenticate before you
              continue. Current session: {userDisplayName} · {tenantDisplayName} ({tenantSlug}).
            </p>
          </div>
        </div>

        {pendingActions.length > 0 ? (
          <div className="mt-5 rounded-2xl border border-amber-500/30 bg-amber-950/20 p-4">
            <div className="flex items-start gap-3">
              <RefreshCcw className="mt-0.5 h-5 w-5 shrink-0 text-amber-300" aria-hidden />
              <div>
                <p className="text-sm font-semibold text-amber-100">Queued work still needs attention</p>
                <p className="mt-1 text-sm text-amber-100/80">
                  You have {pendingActions.length} pending action{pendingActions.length === 1 ? '' : 's'} on this device.
                  Review the queue before signing out if you need to sync or discard work.
                </p>
              </div>
            </div>

            <ul className="mt-3 space-y-2 text-sm text-slate-200">
              {pendingActions.slice(0, 4).map((item) => (
                <li key={item.idempotencyKey} className="rounded-lg border border-slate-800 bg-slate-950/70 px-3 py-2">
                  <p className="font-medium text-slate-100">{item.title}</p>
                  <p className="text-xs uppercase tracking-wide text-slate-400">
                    {item.productKey} · {item.taskKey}
                  </p>
                </li>
              ))}
            </ul>
          </div>
        ) : null}

        <div className="mt-5 flex flex-wrap gap-2">
          {pendingActions.length > 0 ? (
            <button
              type="button"
              className="inline-flex min-h-11 items-center rounded-lg bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500"
              onClick={onOpenOfflineQueue}
            >
              Review offline queue
            </button>
          ) : null}
          {pendingActions.length === 0 ? (
            <button
              type="button"
              className="inline-flex min-h-11 items-center rounded-lg border border-slate-600 px-4 py-2 text-sm font-medium text-slate-100 hover:border-teal-500"
              onClick={onReauthenticate}
            >
              Return to sign in
            </button>
          ) : (
            <button
              type="button"
              className="inline-flex min-h-11 items-center rounded-lg border border-rose-500/40 px-4 py-2 text-sm font-medium text-rose-100 hover:border-rose-400"
              onClick={onDiscardQueuedWorkAndSignOut}
            >
              Discard queued work and sign out
            </button>
          )}
        </div>
      </div>
    </div>
  )
}
