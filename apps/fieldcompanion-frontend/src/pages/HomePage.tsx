import { useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link, Navigate, useSearchParams } from 'react-router-dom'
import { AppWindow, Bell, Camera, Clock3, CloudOff, ExternalLink, ScanLine, UserRound } from 'lucide-react'
import { PageHeader } from '@stl/shared-ui'

import { getFieldInbox } from '../api/client'
import { useFieldCompanionProductLaunch } from '../hooks/useFieldCompanionProductLaunch'
import { useFieldCompanionWorkspace } from '../hooks/useFieldCompanionWorkspace'
import { FieldCompanionPlainReason } from '../lib/FieldCompanionPlainReason'
import { FieldInboxPanel } from '../components/FieldInboxPanel'
import { NotificationSettingsPanel } from '../components/NotificationSettingsPanel'
import { OfflineQueuePanel } from '../components/OfflineQueuePanel'
import { SubmissionActivityBanner } from '../components/SubmissionActivityBanner'
import { useFieldTaskSubmissionState } from '../hooks/useFieldTaskSubmissionState'
import { useFieldCompanionWebPush } from '../hooks/useFieldCompanionWebPush'
import { useOfflineQueue } from '../hooks/useOfflineQueue'
import { productLabel } from '../lib/fieldInbox'
import { productLaunchUrl } from '../api/client'
import { resolveSuiteHomeUrl, buildProductLaunchUrlMap } from '@stl/shared-ui'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)

const PRIMARY_ACTIONS = [
  { label: 'Clock', to: '/clock', icon: Clock3 },
  { label: 'Scan', to: '/scan', icon: ScanLine },
  { label: 'Capture', to: '/capture', icon: Camera },
  { label: 'Report', to: '/report', icon: Bell },
  { label: 'Product surfaces', to: '/surfaces', icon: AppWindow },
  { label: 'Offline queue', to: '/offline-queue', icon: CloudOff },
  { label: 'Profile', to: '/profile', icon: UserRound },
]

export function HomePage() {
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  const { session, accessToken, meQuery } = useFieldCompanionWorkspace()
  const [productFilter, setProductFilter] = useState('')
  const [acknowledgedTaskKeys, setAcknowledgedTaskKeys] = useState<Set<string>>(() => new Set())
  const highlightedTaskKey = null

  const inboxQuery = useQuery({
    queryKey: ['fieldcompanion-field-inbox', accessToken],
    queryFn: () => getFieldInbox(accessToken),
    enabled: Boolean(accessToken),
    refetchInterval: 60_000,
  })

  const entitledProducts = useMemo(
    () => meQuery.data?.fieldProductKeys ?? [],
    [meQuery.data?.fieldProductKeys],
  )

  const taskKeys = useMemo(
    () => (inboxQuery.data?.items ?? []).map((task) => task.taskKey),
    [inboxQuery.data?.items],
  )

  const submissionState = useFieldTaskSubmissionState(accessToken, taskKeys)

  const offlineQueue = useOfflineQueue(accessToken, {
    onSyncComplete: submissionState.refreshServerStatus,
  })

  const productLaunch = useFieldCompanionProductLaunch({
    accessToken,
    suiteHomeUrl,
    productLaunchUrls,
  })

  useFieldCompanionWebPush(accessToken)

  if (!session || !meQuery.data) {
    if (meQuery.isError) {
      return (
        <p className="rounded-xl border border-rose-500/40 bg-rose-950/30 px-4 py-3 text-sm text-rose-200">
          {FieldCompanionPlainReason(meQuery.error, 'Failed to load workspace profile.')}
        </p>
      )
    }

    return <p className="text-sm text-slate-400">Loading your work dashboard…</p>
  }

  const canManageNotifications =
    meQuery.data.isPlatformAdmin || meQuery.data.tenantRoleKey === 'tenant_admin'

  return (
    <div className="mx-auto max-w-5xl space-y-5">
      <PageHeader
        title="My work"
        subtitle={`${meQuery.data.displayName} · ${session.tenantSlug} · ${entitledProducts.length} entitled products`}
      />

      <section className="grid gap-3 md:grid-cols-3">
        <StatusTile
          label="Connection"
          value={offlineQueue.isOnline ? 'Online' : 'Offline'}
          detail={
            offlineQueue.isOnline
              ? 'Your actions sync immediately when submitted.'
              : 'Pending actions stay local until the connection returns.'
          }
          tone={offlineQueue.isOnline ? 'emerald' : 'amber'}
        />
        <StatusTile
          label="Offline queue"
          value={`${offlineQueue.pendingCount} pending`}
          detail="Queued acknowledgments and retries are tracked here."
          tone="teal"
        />
        <StatusTile
          label="Entitlements"
          value={`${entitledProducts.length} products`}
          detail="Surfaces appear only when your tenant and person are entitled."
          tone="slate"
        />
      </section>

      <section className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
        {PRIMARY_ACTIONS.map((action) => {
          const Icon = action.icon
          return (
            <Link
              key={action.to}
              to={action.to}
              className="rounded-2xl border border-slate-700 bg-slate-900/80 p-4 transition hover:border-teal-500/60 hover:bg-slate-900"
            >
              <Icon className="h-5 w-5 text-teal-300" aria-hidden />
              <p className="mt-3 text-sm font-semibold text-white">{action.label}</p>
            </Link>
          )
        })}
      </section>

      <section className="rounded-2xl border border-slate-700 bg-slate-900/80 p-5">
        <div className="flex items-center justify-between gap-3">
          <div>
            <h2 className="text-lg font-semibold text-white">Product surfaces</h2>
            <p className="text-sm text-slate-400">
              Launch the entitled product workspace that owns each action.
            </p>
          </div>
        </div>
        <div className="mt-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
          {entitledProducts.length === 0 ? (
            <p className="text-sm text-slate-400">No entitled product surfaces were returned yet.</p>
          ) : (
            entitledProducts.map((productKey) => {
              const launchUrl = productLaunchUrl(productKey, '/')
              const taskCount = inboxQuery.data?.summary.countByProduct[productKey] ?? 0
              return (
                <article key={productKey} className="rounded-2xl border border-slate-700 bg-slate-950/60 p-4">
                  <p className="text-xs font-semibold uppercase tracking-wide text-teal-300">
                    {productLabel(productKey)}
                  </p>
                  <p className="mt-2 text-sm text-slate-300">
                    {taskCount > 0
                      ? `${taskCount} assigned task${taskCount === 1 ? '' : 's'} in your inbox.`
                      : 'No assigned tasks in the field inbox right now.'}
                  </p>
                  <div className="mt-3 flex flex-wrap gap-2">
                    <button
                      type="button"
                      className="inline-flex min-h-11 items-center rounded-lg bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
                      disabled={productLaunch.isPending}
                      onClick={() => {
                        void productLaunch.mutateAsync(productKey)
                      }}
                    >
                      Open workspace
                    </button>
                    {launchUrl && (
                      <a
                        href={launchUrl}
                        className="inline-flex min-h-11 items-center gap-2 rounded-lg border border-slate-600 px-4 py-2 text-sm font-medium text-slate-100 hover:border-teal-500"
                      >
                        <ExternalLink className="h-4 w-4" aria-hidden />
                        Direct link
                      </a>
                    )}
                  </div>
                </article>
              )
            })
          )}
        </div>
      </section>

      <SubmissionActivityBanner
        toasts={submissionState.toasts}
        onDismiss={submissionState.dismissToast}
      />

      {inboxQuery.isLoading && (
        <p className="rounded-xl border border-slate-700 bg-slate-900/70 px-4 py-6 text-sm text-slate-300">
          Loading assigned work across products…
        </p>
      )}

      {inboxQuery.error && (
        <p className="rounded-xl border border-red-500/40 bg-red-950/30 px-4 py-3 text-sm text-red-200">
          {FieldCompanionPlainReason(inboxQuery.error, 'Failed to load field inbox.')}
        </p>
      )}

      {inboxQuery.data && (
        <FieldInboxPanel
          inbox={inboxQuery.data}
          productFilter={productFilter}
          onProductFilterChange={setProductFilter}
          accessToken={accessToken}
          getSubmissionChips={submissionState.getChips}
          acknowledgedTaskKeys={acknowledgedTaskKeys}
          onAcknowledgeTask={(task) => {
            void offlineQueue
              .queueAcknowledge({
                taskKey: task.taskKey,
                productKey: task.productKey,
                title: task.title,
              })
              .then(() => {
                setAcknowledgedTaskKeys((previous) => new Set(previous).add(task.taskKey))
              })
              .catch(() => undefined)
          }}
          onEvidenceUploadComplete={submissionState.refreshServerStatus}
          highlightedTaskKey={highlightedTaskKey}
        />
      )}

      <section className="grid gap-5 lg:grid-cols-[1.3fr_0.7fr]">
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

        <NotificationSettingsPanel accessToken={accessToken} canManage={canManageNotifications} />
      </section>
    </div>
  )
}

function StatusTile({
  label,
  value,
  detail,
  tone,
}: {
  label: string
  value: string
  detail: string
  tone: 'teal' | 'emerald' | 'amber' | 'slate'
}) {
  const toneStyles: Record<typeof tone, string> = {
    teal: 'border-teal-500/30 bg-teal-950/30 text-teal-100',
    emerald: 'border-emerald-500/30 bg-emerald-950/30 text-emerald-100',
    amber: 'border-amber-500/30 bg-amber-950/30 text-amber-100',
    slate: 'border-slate-700 bg-slate-900/80 text-slate-100',
  }

  return (
    <div className={`rounded-2xl border p-4 ${toneStyles[tone]}`}>
      <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-400">{label}</p>
      <p className="mt-2 text-xl font-semibold text-white">{value}</p>
      <p className="mt-2 text-sm text-slate-300">{detail}</p>
    </div>
  )
}
