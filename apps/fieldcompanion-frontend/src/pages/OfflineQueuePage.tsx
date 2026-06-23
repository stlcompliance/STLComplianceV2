import { CloudOff, RefreshCcw } from 'lucide-react'
import { PageHeader } from '@stl/shared-ui'

import { OfflineQueuePanel } from '../components/OfflineQueuePanel'
import { useFieldTaskSubmissionState } from '../hooks/useFieldTaskSubmissionState'
import { useFieldCompanionWorkspace } from '../hooks/useFieldCompanionWorkspace'
import { useOfflineQueue } from '../hooks/useOfflineQueue'

export function OfflineQueuePage() {
  const { accessToken } = useFieldCompanionWorkspace()
  const offlineQueue = useOfflineQueue(accessToken)
  const submissionState = useFieldTaskSubmissionState(accessToken, [])

  return (
    <div className="mx-auto max-w-3xl space-y-5">
      <PageHeader
        title="Offline queue"
        subtitle="Pending actions stay queued until they sync."
      />

      <section className="rounded-2xl border border-slate-700 bg-slate-900/80 p-5">
        <div className="flex items-center gap-2">
          <CloudOff className="h-5 w-5 text-teal-300" aria-hidden />
          <h2 className="text-lg font-semibold text-white">Queued actions</h2>
        </div>
        <p className="mt-2 text-sm text-slate-400">
          If the device loses connectivity, acknowledgments queue locally and sync when the app can
          reach NexArr again.
        </p>
        <div className="mt-4">
          <OfflineQueuePanel
            isOnline={offlineQueue.isOnline}
            pendingCount={offlineQueue.pendingCount}
            pending={offlineQueue.pending}
            lastSyncedAt={offlineQueue.lastSyncedAt}
            lastSyncError={offlineQueue.lastSyncError}
            isSyncing={offlineQueue.isSyncing}
            onSyncNow={() => {
              void offlineQueue.syncPending()
            }}
          />
        </div>
      </section>

      <section className="rounded-2xl border border-slate-700 bg-slate-900/80 p-5 text-sm text-slate-300">
        <div className="flex items-center gap-2 text-white">
          <RefreshCcw className="h-4 w-4 text-teal-300" aria-hidden />
          <h2 className="text-base font-semibold">How sync behaves</h2>
        </div>
        <ul className="mt-3 space-y-2 text-slate-400">
          <li>• Pending work is clearly marked until it syncs successfully.</li>
          <li>• Sync failures remain visible until they are retried or resolved.</li>
          <li>• Conflicts are surfaced as product-side validation problems, not silent overwrites.</li>
        </ul>
        <p className="mt-4 text-[var(--color-text-muted)]">
          Submission state is still tracked in the main workspace so the inbox can reflect the latest
          sync result without losing context.
        </p>
        {submissionState.isLoadingServer && (
          <p className="mt-3 text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
            Refreshing submission status…
          </p>
        )}
      </section>
    </div>
  )
}
