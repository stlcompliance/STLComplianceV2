import type { QueuedOfflineAction } from '../hooks/useOfflineQueue'

interface OfflineQueuePanelProps {
  isOnline: boolean
  pendingCount: number
  pending: QueuedOfflineAction[]
  lastSyncedAt: string | null
  lastSyncError: string | null
  isSyncing: boolean
  onSyncNow: () => void
}

export function OfflineQueuePanel({
  isOnline,
  pendingCount,
  pending,
  lastSyncedAt,
  lastSyncError,
  isSyncing,
  onSyncNow,
}: OfflineQueuePanelProps) {
  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="companion-offline-queue-panel"
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
          data-testid="companion-connection-status"
        >
          {isOnline ? 'Online' : 'Offline'}
        </span>
        <span className="text-slate-300" data-testid="companion-offline-pending-count">
          {pendingCount} pending
        </span>
        {lastSyncedAt && (
          <span className="text-slate-500">Last sync {new Date(lastSyncedAt).toLocaleString()}</span>
        )}
      </div>

      {lastSyncError && <p className="mt-2 text-sm text-rose-400">{lastSyncError}</p>}

      {pending.length > 0 && (
        <ul className="mt-4 space-y-2 text-sm text-slate-300">
          {pending.map((item) => (
            <li
              key={item.idempotencyKey}
              className="rounded-md border border-slate-800 px-3 py-2"
              data-testid="companion-offline-queue-item"
            >
              <span className="font-medium text-slate-100">{item.title}</span>
              <span className="ml-2 text-xs uppercase text-slate-500">{item.productKey}</span>
            </li>
          ))}
        </ul>
      )}

      <button
        type="button"
        className="mt-4 rounded-md bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
        disabled={!isOnline || pendingCount === 0 || isSyncing}
        data-testid="companion-offline-sync-now"
        onClick={onSyncNow}
      >
        {isSyncing ? 'Syncing…' : 'Sync now'}
      </button>
    </section>
  )
}
