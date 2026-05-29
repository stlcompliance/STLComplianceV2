import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getFactSourceSyncHealth,
  getFactSourceSyncWorkerSettings,
  upsertFactSourceSyncWorkerSettings,
} from '../api/client'
import type { FactSourceSyncHealthItem } from '../api/types'

interface FactSourceSyncPanelProps {
  accessToken: string
  canManage: boolean
}

function healthBadgeClass(status: string): string {
  switch (status) {
    case 'healthy':
      return 'bg-emerald-900/50 text-emerald-300'
    case 'stale':
      return 'bg-amber-900/50 text-amber-300'
    case 'failed':
      return 'bg-rose-900/50 text-rose-300'
    default:
      return 'bg-slate-800 text-slate-400'
  }
}

function SyncHealthRow({ item }: { item: FactSourceSyncHealthItem }) {
  return (
    <li className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
      <div className="flex items-start justify-between gap-2">
        <p className="font-medium text-slate-100">{item.sourceKey}</p>
        <span className={`rounded px-2 py-0.5 text-xs ${healthBadgeClass(item.healthStatus)}`}>
          {item.healthStatus}
        </span>
      </div>
      <p className="font-mono text-xs text-violet-300">{item.factKey}</p>
      <p className="mt-1 text-xs text-slate-500">
        {item.productKey ?? 'product'} · scope {item.scopeKey}
      </p>
      {item.lastSuccessAt && (
        <p className="mt-1 text-xs text-slate-500">
          Last success {new Date(item.lastSuccessAt).toLocaleString()}
        </p>
      )}
      {item.lastErrorMessage && (
        <p className="mt-1 text-xs text-rose-400">{item.lastErrorMessage}</p>
      )}
    </li>
  )
}

export function FactSourceSyncPanel({ accessToken, canManage }: FactSourceSyncPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [defaultScopeKey, setDefaultScopeKey] = useState('tenant')
  const [intervalMinutes, setIntervalMinutes] = useState('60')

  const settingsQuery = useQuery({
    queryKey: ['compliancecore-fact-source-sync-settings', accessToken],
    queryFn: () => getFactSourceSyncWorkerSettings(accessToken),
    enabled: canManage,
  })

  const healthQuery = useQuery({
    queryKey: ['compliancecore-fact-source-sync-health', accessToken],
    queryFn: () => getFactSourceSyncHealth(accessToken),
    enabled: Boolean(accessToken),
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setDefaultScopeKey(data.defaultScopeKey)
    setIntervalMinutes(String(data.intervalMinutes))
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertFactSourceSyncWorkerSettings(accessToken, {
        isEnabled,
        defaultScopeKey: defaultScopeKey.trim() || 'tenant',
        intervalMinutes: Number.parseInt(intervalMinutes, 10) || 60,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['compliancecore-fact-source-sync-settings'] })
      void queryClient.invalidateQueries({ queryKey: ['compliancecore-fact-source-sync-health'] })
    },
  })

  const health = healthQuery.data

  return (
    <div className="space-y-6">
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">
          Product API sync health
        </h2>
        <p className="mt-2 text-sm text-slate-400">
          Background sync keeps product_api fact sources fresh and records last success, failure, and
          staleness per source.
        </p>
        {healthQuery.isLoading ? (
          <p className="mt-3 text-sm text-slate-500">Loading sync health…</p>
        ) : health ? (
          <div className="mt-3 grid gap-3 sm:grid-cols-4">
            <div className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
              <p className="text-xs text-slate-500">Product API sources</p>
              <p className="text-lg font-semibold text-slate-100">{health.productApiSourceCount}</p>
            </div>
            <div className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
              <p className="text-xs text-slate-500">Healthy</p>
              <p className="text-lg font-semibold text-emerald-300">{health.healthyCount}</p>
            </div>
            <div className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
              <p className="text-xs text-slate-500">Stale / failed</p>
              <p className="text-lg font-semibold text-amber-300">
                {health.staleCount + health.failedCount}
              </p>
            </div>
            <div className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
              <p className="text-xs text-slate-500">Worker</p>
              <p className="text-lg font-semibold text-slate-100">
                {health.workerEnabled ? 'Enabled' : 'Disabled'}
              </p>
            </div>
          </div>
        ) : (
          <p className="mt-3 text-sm text-slate-500">Sync health unavailable.</p>
        )}
        {health && health.sources.length > 0 && (
          <ul className="mt-4 space-y-2">
            {health.sources.map((item) => (
              <SyncHealthRow key={item.factSourceId} item={item} />
            ))}
          </ul>
        )}
      </section>

      {canManage && (
        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">
            Sync worker settings
          </h2>
          <div className="mt-4 space-y-4">
            <label className="flex items-center gap-2 text-sm text-slate-300">
              <input
                type="checkbox"
                checked={isEnabled}
                onChange={(e) => setIsEnabled(e.target.checked)}
                className="rounded border-slate-600"
              />
              Enable background fact source sync
            </label>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="block text-sm text-slate-400">
                Default scope key
                <input
                  type="text"
                  value={defaultScopeKey}
                  onChange={(e) => setDefaultScopeKey(e.target.value)}
                  className="mt-1 w-full rounded-md border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                />
              </label>
              <label className="block text-sm text-slate-400">
                Sync interval (minutes)
                <input
                  type="number"
                  min={5}
                  max={1440}
                  value={intervalMinutes}
                  onChange={(e) => setIntervalMinutes(e.target.value)}
                  className="mt-1 w-full rounded-md border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                />
              </label>
            </div>
            <button
              type="button"
              onClick={() => saveMutation.mutate()}
              disabled={saveMutation.isPending}
              className="rounded-md bg-violet-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-violet-500 disabled:opacity-50"
            >
              {saveMutation.isPending ? 'Saving…' : 'Save sync settings'}
            </button>
          </div>
        </section>
      )}
    </div>
  )
}
