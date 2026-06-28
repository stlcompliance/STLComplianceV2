import { ExternalLink } from 'lucide-react'

import { productLaunchUrl } from '../api/client'
import type { OfflineQueueConflict, QueuedOfflineAction } from '../lib/offlineQueue'
import {
  formatOfflineQueueAge,
  summarizeOfflineQueueFreshness,
} from '../lib/offlineQueueFreshness'
import { productLabel } from '../lib/fieldInbox'

interface OfflineQueuePanelProps {
  isOnline: boolean
  pendingCount: number
  pending: QueuedOfflineAction[]
  conflicts?: OfflineQueueConflict[]
  lastSyncedAt: string | null
  lastSyncError: string | null
  isSyncing: boolean
  onSyncNow: () => void
  onRetryConflict?: (idempotencyKey: string) => void | Promise<void>
  onDiscardConflict?: (idempotencyKey: string) => void
}

export function OfflineQueuePanel({
  isOnline,
  pendingCount,
  pending,
  conflicts = [],
  lastSyncedAt,
  lastSyncError,
  isSyncing,
  onSyncNow,
  onRetryConflict,
  onDiscardConflict,
}: OfflineQueuePanelProps) {
  const freshness = summarizeOfflineQueueFreshness({
    pending,
    lastSyncedAt,
  })

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="fieldcompanion-offline-queue-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Offline queue</h2>
      <p className="mt-1 text-sm text-slate-400">
        Field acknowledgments are queued locally when offline and synced to NexArr with idempotency
        keys when connectivity returns.
      </p>

      <div className="mt-4 flex flex-wrap items-center gap-3 text-sm">
        <span
          className={`rounded-full px-3 py-1 font-medium ${
            isOnline ? 'bg-emerald-900/50 text-emerald-200' : 'bg-amber-900/50 text-amber-200'
          }`}
          data-testid="fieldcompanion-connection-status"
        >
          {isOnline ? 'Online' : 'Offline'}
        </span>
        <span className="text-slate-300" data-testid="fieldcompanion-offline-pending-count">
          {pendingCount} pending
        </span>
        {lastSyncedAt && (
          <span className="text-[var(--color-text-muted)]">
            Last sync {freshness.lastSyncedAgeLabel === 'just now' ? 'just now' : `${freshness.lastSyncedAgeLabel} ago`}
          </span>
        )}
      </div>

      {lastSyncError && <p className="mt-2 text-sm text-rose-400">{lastSyncError}</p>}

      <div
        className={`mt-4 rounded-xl border p-4 ${
          freshness.isStale
            ? 'border-amber-500/30 bg-amber-950/20'
            : 'border-slate-800 bg-slate-950/40'
        }`}
        data-testid={
          freshness.isStale
            ? 'fieldcompanion-offline-freshness-stale'
            : 'fieldcompanion-offline-freshness'
        }
      >
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <p className="text-sm font-semibold text-slate-100">Queue freshness</p>
            <p className="mt-1 text-sm text-slate-300">
              {pending.length > 0
                ? `Oldest pending action queued ${
                    freshness.oldestPendingAgeLabel === 'just now'
                      ? 'just now'
                      : `${freshness.oldestPendingAgeLabel} ago`
                  }.`
                : 'No pending actions are queued locally.'}
            </p>
            <p className="mt-1 text-xs text-[var(--color-text-muted)]">
              Offline actions older than {freshness.staleThresholdLabel} are flagged as stale so you can review
              them before syncing.
            </p>
          </div>
          <span
            className={`rounded-full px-3 py-1 text-xs font-medium ${
              freshness.isStale
                ? 'bg-amber-900/50 text-amber-100'
                : pending.length > 0
                  ? 'bg-teal-900/50 text-teal-100'
                  : 'bg-slate-800 text-slate-200'
            }`}
          >
            {freshness.isStale ? 'Stale queue' : pending.length > 0 ? 'Fresh queue' : 'No queue'}
          </span>
        </div>

        <div className="mt-3 grid gap-2 text-xs text-[var(--color-text-muted)] sm:grid-cols-2">
          <p>Last sync age: {freshness.lastSyncedAgeLabel ?? 'never'}</p>
          <p>Syncing policy: {isOnline ? 'Will sync now or on reconnect.' : 'Waiting for network.'}</p>
        </div>

        {freshness.isStale ? (
          <p
            className="mt-3 rounded-lg border border-amber-500/30 bg-amber-950/30 px-3 py-2 text-sm text-amber-100"
            data-testid="fieldcompanion-offline-freshness-warning"
          >
            This queue is older than {freshness.staleThresholdLabel}. Review the items before syncing so you do
            not submit stale work.
          </p>
        ) : null}
      </div>

      {pending.length > 0 && (
        <ul className="mt-4 space-y-2 text-sm text-slate-300">
          {pending.map((item) => (
            <li
              key={item.idempotencyKey}
              className="rounded-md border border-slate-800 px-3 py-2"
              data-testid="fieldcompanion-offline-queue-item"
            >
              <span className="font-medium text-slate-100">{item.title}</span>
              <span className="ml-2 text-xs uppercase text-[var(--color-text-muted)]">{item.productKey}</span>
              <span className="ml-2 text-xs text-[var(--color-text-muted)]">
                queued{' '}
                {formatOfflineQueueAge(item.clientCreatedAt) === 'just now'
                  ? 'just now'
                  : `${formatOfflineQueueAge(item.clientCreatedAt)} ago`}
              </span>
            </li>
          ))}
        </ul>
      )}

      {conflicts.length > 0 && (
        <div className="mt-5 rounded-xl border border-amber-500/30 bg-amber-950/20 p-4">
          <div className="flex items-start justify-between gap-3">
            <div>
              <p className="text-sm font-semibold text-amber-100">Sync conflicts need review</p>
              <p className="mt-1 text-sm text-amber-200/80">
                These actions were rejected by the owning product after they left the device. Review the current workspace before you retry or discard the local copy.
              </p>
            </div>
            <span className="rounded-full bg-amber-900/50 px-3 py-1 text-xs font-medium text-amber-100">
              {conflicts.length} conflict{conflicts.length === 1 ? '' : 's'}
            </span>
          </div>

          <div className="mt-4 space-y-3">
            {conflicts.map((item) => (
              <article
                key={item.action.idempotencyKey}
                className="rounded-lg border border-amber-500/20 bg-slate-950/60 p-3"
                data-testid="fieldcompanion-offline-conflict-item"
              >
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <div>
                    <p className="font-medium text-slate-100">{item.action.title}</p>
                    <p className="text-xs uppercase tracking-wide text-amber-200/80">
                      {productLabel(item.action.productKey)} task
                    </p>
                  </div>
                  <span
                    className={`rounded-full px-2.5 py-1 text-xs font-medium ${
                      item.reasonCode === 'fieldcompanion.field_task.inbox_unavailable'
                        ? 'bg-teal-900/50 text-teal-100'
                        : isOfflineQueueConflictReviewNeeded(item)
                          ? 'bg-amber-900/50 text-amber-100'
                          : 'bg-rose-900/50 text-rose-100'
                    }`}
                  >
                    {getOfflineQueueConflictBadge(item)}
                  </span>
                </div>

                <p className="mt-2 text-sm text-slate-300">{item.reasonMessage}</p>
                <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                  Rejected {new Date(item.rejectedAt).toLocaleString()}
                </p>

                <div className="mt-3 rounded-lg border border-slate-800 bg-slate-950/60 p-3">
                  <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">
                    Recommended next step
                  </p>
                  <p className="mt-1 text-sm font-medium text-slate-100">
                    {getOfflineQueueConflictHeadline(item)}
                  </p>
                  <p className="mt-1 text-sm text-slate-300">
                    {getOfflineQueueConflictGuidance(item)}
                  </p>
                  {getOfflineQueueConflictWorkspaceHref(item) && (
                    <a
                      href={getOfflineQueueConflictWorkspaceHref(item) ?? undefined}
                      className="mt-3 inline-flex min-h-11 items-center gap-2 rounded-md border border-teal-500/40 bg-teal-600/20 px-3 py-1.5 text-sm font-medium text-teal-100 hover:border-teal-400 hover:bg-teal-600/30"
                    >
                      <ExternalLink className="h-4 w-4" aria-hidden />
                      {getOfflineQueueConflictWorkspaceLabel(item)}
                    </a>
                  )}
                </div>

                <div className="mt-3 flex flex-wrap gap-2">
                  <button
                    type="button"
                    className="rounded-md bg-teal-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
                    disabled={!onRetryConflict}
                    data-testid="fieldcompanion-offline-conflict-retry"
                    onClick={() => {
                      void onRetryConflict?.(item.action.idempotencyKey)
                    }}
                  >
                    Retry sync
                  </button>
                  <button
                    type="button"
                    className="rounded-md border border-slate-700 px-3 py-1.5 text-sm font-medium text-slate-200 hover:bg-slate-800 disabled:opacity-50"
                    disabled={!onDiscardConflict}
                    data-testid="fieldcompanion-offline-conflict-discard"
                    onClick={() => {
                      onDiscardConflict?.(item.action.idempotencyKey)
                    }}
                  >
                    Discard local copy
                  </button>
                </div>
              </article>
            ))}
          </div>
        </div>
      )}

      <button
        type="button"
        className="mt-4 rounded-md bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
        disabled={!isOnline || pendingCount === 0 || isSyncing}
        data-testid="fieldcompanion-offline-sync-now"
        onClick={onSyncNow}
      >
        {isSyncing ? 'Syncing…' : 'Sync now'}
      </button>
    </section>
  )
}

function isOfflineQueueConflictReviewNeeded(conflict: OfflineQueueConflict): boolean {
  return (
    conflict.reasonCode === 'fieldcompanion.field_task.not_in_inbox'
    || conflict.reasonCode === 'fieldcompanion.offline_actions.record_changed'
  )
}

function getOfflineQueueConflictBadge(conflict: OfflineQueueConflict): string {
  if (conflict.reasonCode === 'fieldcompanion.field_task.inbox_unavailable') {
    return 'Retryable'
  }

  if (isOfflineQueueConflictReviewNeeded(conflict)) {
    return 'Review needed'
  }

  if (
    conflict.reasonCode === 'fieldcompanion.offline_actions.idempotency_conflict'
    || conflict.reasonCode === 'fieldcompanion.offline_actions.payload_idempotency_mismatch'
  ) {
    return 'Discard stale copy'
  }

  return 'Rejected'
}

function getOfflineQueueConflictHeadline(conflict: OfflineQueueConflict): string {
  if (conflict.reasonCode === 'fieldcompanion.field_task.inbox_unavailable') {
    return 'Retry after the product inbox becomes available again.'
  }

  if (isOfflineQueueConflictReviewNeeded(conflict)) {
    return 'Open the current task and compare the live record before retrying.'
  }

  if (
    conflict.reasonCode === 'fieldcompanion.offline_actions.idempotency_conflict'
    || conflict.reasonCode === 'fieldcompanion.offline_actions.payload_idempotency_mismatch'
  ) {
    return 'Discard the stale queued action and create a fresh one from the workspace.'
  }

  if (conflict.reasonCode === 'fieldcompanion.offline_actions.unsupported_kind') {
    return 'This action must be completed online instead of from the offline queue.'
  }

  return 'Review the owning workspace before you retry or discard this queued action.'
}

function getOfflineQueueConflictGuidance(conflict: OfflineQueueConflict): string {
  if (conflict.reasonCode === 'fieldcompanion.field_task.inbox_unavailable') {
    return 'The owning product could not be checked right now. Keep the queued action intact and sync again once the inbox responds.'
  }

  if (isOfflineQueueConflictReviewNeeded(conflict)) {
    return 'The server version changed while the device was offline. Review the live task, then retry only if the original intent still applies.'
  }

  if (
    conflict.reasonCode === 'fieldcompanion.offline_actions.idempotency_conflict'
    || conflict.reasonCode === 'fieldcompanion.offline_actions.payload_idempotency_mismatch'
  ) {
    return 'That queued copy no longer matches a safe replay path. Create a new action from the current workspace instead.'
  }

  if (conflict.reasonCode === 'fieldcompanion.offline_actions.unsupported_kind') {
    return 'Only field inbox acknowledgments are queueable offline right now.'
  }

  return 'Compare the live record with the queued copy before choosing retry or discard.'
}

function getOfflineQueueConflictWorkspaceHref(conflict: OfflineQueueConflict): string | null {
  if (conflict.action.actionKind === 'staffarr.clock.punch') {
    return '/clock'
  }

  return productLaunchUrl(conflict.action.productKey, conflict.action.deepLinkPath ?? '/')
}

function getOfflineQueueConflictWorkspaceLabel(conflict: OfflineQueueConflict): string {
  if (conflict.action.actionKind === 'staffarr.clock.punch') {
    return 'Open clock'
  }

  return 'Open current task'
}
