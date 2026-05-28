import { useQuery } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { Navigate, useSearchParams } from 'react-router-dom'
import { PageHeader } from '@stl/shared-ui'
import { getFieldInbox, getMe } from '../api/client'
import { loadSession } from '../auth/sessionStorage'
import { FieldInboxPanel } from '../components/FieldInboxPanel'
import { NotificationSettingsPanel } from '../components/NotificationSettingsPanel'
import { OfflineQueuePanel } from '../components/OfflineQueuePanel'
import { useOfflineQueue } from '../hooks/useOfflineQueue'
import { entitledProductKeys } from '../lib/fieldInbox'

export function HomePage() {
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  const session = loadSession()
  const [productFilter, setProductFilter] = useState('')
  const [acknowledgedTaskKeys, setAcknowledgedTaskKeys] = useState<Set<string>>(() => new Set())

  const meQuery = useQuery({
    queryKey: ['companion-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const inboxQuery = useQuery({
    queryKey: ['companion-field-inbox', session?.accessToken],
    queryFn: () => getFieldInbox(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    refetchInterval: 60_000,
  })

  const entitledProducts = useMemo(
    () => (inboxQuery.data ? entitledProductKeys(inboxQuery.data.sources) : []),
    [inboxQuery.data],
  )

  const offlineQueue = useOfflineQueue(session?.accessToken ?? '')

  if (!session || !meQuery.data) {
    return <p className="text-sm text-slate-400">Loading field inbox…</p>
  }

  const canManageNotifications =
    meQuery.data.isPlatformAdmin || meQuery.data.tenantRoleKey === 'tenant_admin'

  return (
    <div className="mx-auto max-w-2xl space-y-4">
      <PageHeader
        title="Field inbox"
        subtitle={`${meQuery.data.displayName} · ${session.tenantSlug} · ${entitledProducts.length} entitled products`}
      />

      {inboxQuery.isLoading && (
        <p className="rounded-xl border border-slate-700 bg-slate-900/70 px-4 py-6 text-sm text-slate-300">
          Loading assigned work across products…
        </p>
      )}

      {inboxQuery.error && (
        <p className="rounded-xl border border-red-500/40 bg-red-950/30 px-4 py-3 text-sm text-red-200">
          {inboxQuery.error instanceof Error
            ? inboxQuery.error.message
            : 'Failed to load field inbox.'}
        </p>
      )}

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

      {inboxQuery.data && (
        <FieldInboxPanel
          inbox={inboxQuery.data}
          productFilter={productFilter}
          onProductFilterChange={setProductFilter}
          accessToken={session.accessToken}
          acknowledgedTaskKeys={acknowledgedTaskKeys}
          onAcknowledgeTask={(task) => {
            offlineQueue.queueAcknowledge({
              taskKey: task.taskKey,
              productKey: task.productKey,
              title: task.title,
            })
            setAcknowledgedTaskKeys((previous) => new Set(previous).add(task.taskKey))
          }}
        />
      )}

      <NotificationSettingsPanel
        accessToken={session.accessToken}
        canManage={canManageNotifications}
      />
    </div>
  )
}
