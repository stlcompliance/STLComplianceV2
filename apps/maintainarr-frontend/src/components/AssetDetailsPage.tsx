import { AlertTriangle, CheckCircle2, ClipboardCheck, FileText, Gauge, History, MapPin, ShieldCheck, Truck, Wrench, XCircle } from 'lucide-react'

import type { AssetReadinessResponse, AssetResponse } from '../api/types'

interface AssetDetailsPageProps {
  asset: AssetResponse
  readiness: AssetReadinessResponse | null
  isReadinessLoading: boolean
}

function toneClass(tone: 'good' | 'warn' | 'neutral'): string {
  if (tone === 'good') return 'bg-emerald-500/10 text-emerald-300 ring-emerald-500/25'
  if (tone === 'warn') return 'bg-amber-500/10 text-amber-300 ring-amber-500/25'
  return 'bg-slate-500/10 text-slate-300 ring-slate-500/25'
}

function badge(label: string, tone: 'good' | 'warn' | 'neutral') {
  return <span className={`inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium ring-1 ${toneClass(tone)}`}>{label}</span>
}

export function AssetDetailsPage({ asset, readiness, isReadinessLoading }: AssetDetailsPageProps) {
  const readinessTone = readiness?.readinessStatus === 'ready' ? 'good' : 'warn'
  const blockers = readiness?.blockers ?? []

  return (
    <div className="space-y-5" data-testid="asset-details-page">
      <header className="rounded-2xl border border-slate-700 bg-slate-900/60 p-5">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div className="space-y-3">
            <div className="flex items-center gap-3">
              <div className="flex h-14 w-14 items-center justify-center rounded-xl bg-sky-500/10 ring-1 ring-sky-500/25">
                <Truck className="h-8 w-8 text-sky-300" />
              </div>
              <div>
                <h2 className="text-2xl font-semibold text-white">{asset.name}</h2>
                <p className="text-sm text-slate-400">{asset.assetTag} · {asset.className} / {asset.typeName}</p>
              </div>
            </div>
            <p className="flex items-center gap-2 text-sm text-slate-400">
              <MapPin className="h-4 w-4" />
              {asset.siteRef ?? 'Unassigned site'}
            </p>
          </div>
          <div className="flex flex-wrap items-center gap-2">
            {badge(asset.lifecycleStatus, 'neutral')}
            {badge(readiness?.readinessStatus === 'ready' ? 'Ready' : 'Not ready', readinessTone)}
          </div>
        </div>
      </header>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <div className="rounded-xl border border-slate-800 bg-slate-950/70 p-4">
          <p className="text-sm text-slate-400">Open defects</p>
          <p className="mt-2 text-2xl font-bold text-white">{readiness?.signals.openCriticalDefectCount ?? 0}</p>
          <p className="mt-1 text-xs text-slate-500">Critical defects</p>
          <AlertTriangle className="mt-2 h-5 w-5 text-amber-300" />
        </div>
        <div className="rounded-xl border border-slate-800 bg-slate-950/70 p-4">
          <p className="text-sm text-slate-400">Open work orders</p>
          <p className="mt-2 text-2xl font-bold text-white">{readiness?.signals.activeWorkOrderCount ?? 0}</p>
          <p className="mt-1 text-xs text-slate-500">Active</p>
          <Wrench className="mt-2 h-5 w-5 text-slate-300" />
        </div>
        <div className="rounded-xl border border-slate-800 bg-slate-950/70 p-4">
          <p className="text-sm text-slate-400">PM due</p>
          <p className="mt-2 text-2xl font-bold text-white">{readiness?.signals.pmDueCount ?? 0}</p>
          <p className="mt-1 text-xs text-slate-500">Overdue: {readiness?.signals.pmOverdueCount ?? 0}</p>
          <Gauge className="mt-2 h-5 w-5 text-sky-300" />
        </div>
        <div className="rounded-xl border border-slate-800 bg-slate-950/70 p-4">
          <p className="text-sm text-slate-400">Inspection state</p>
          <p className="mt-2 text-2xl font-bold text-white">{(readiness?.signals.failedInspectionCount ?? 0) > 0 ? 'Failing' : 'Pass'}</p>
          <p className="mt-1 text-xs text-slate-500">Failed: {readiness?.signals.failedInspectionCount ?? 0}</p>
          <ClipboardCheck className="mt-2 h-5 w-5 text-emerald-300" />
        </div>
      </div>

      <div className="grid gap-5 lg:grid-cols-[1fr_360px]">
        <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
          <h3 className="text-lg font-semibold text-white">Asset snapshot</h3>
          <div className="mt-4 grid gap-3 md:grid-cols-2">
            <div className="rounded-lg border border-slate-800 bg-slate-900/50 p-3">
              <p className="text-xs text-slate-500">Asset tag</p>
              <p className="text-sm font-medium text-slate-100">{asset.assetTag}</p>
            </div>
            <div className="rounded-lg border border-slate-800 bg-slate-900/50 p-3">
              <p className="text-xs text-slate-500">Lifecycle status</p>
              <p className="text-sm font-medium text-slate-100">{asset.lifecycleStatus}</p>
            </div>
            <div className="rounded-lg border border-slate-800 bg-slate-900/50 p-3">
              <p className="text-xs text-slate-500">Asset class</p>
              <p className="text-sm font-medium text-slate-100">{asset.className}</p>
            </div>
            <div className="rounded-lg border border-slate-800 bg-slate-900/50 p-3">
              <p className="text-xs text-slate-500">Asset type</p>
              <p className="text-sm font-medium text-slate-100">{asset.typeName}</p>
            </div>
            <div className="rounded-lg border border-slate-800 bg-slate-900/50 p-3 md:col-span-2">
              <p className="text-xs text-slate-500">Description</p>
              <p className="text-sm font-medium text-slate-100">{asset.description || 'No description provided'}</p>
            </div>
          </div>
        </section>

        <aside className="space-y-4">
          <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
            <div className="mb-3 flex items-center justify-between">
              <h3 className="font-semibold text-white">Readiness decision</h3>
              {badge(readiness?.readinessStatus === 'ready' ? 'Ready' : 'Blocked', readinessTone)}
            </div>
            {isReadinessLoading ? (
              <p className="text-sm text-slate-400">Loading readiness…</p>
            ) : blockers.length === 0 ? (
              <div className="rounded-lg border border-emerald-500/20 bg-emerald-500/10 p-3 text-sm text-emerald-100">
                <div className="flex items-start gap-2">
                  <CheckCircle2 className="mt-0.5 h-4 w-4" />
                  No maintenance blockers.
                </div>
              </div>
            ) : (
              <ul className="space-y-2">
                {blockers.slice(0, 5).map((blocker) => (
                  <li key={`${blocker.blockerType}-${blocker.sourceEntityId}`} className="rounded-lg border border-amber-500/25 bg-amber-500/10 p-3 text-sm text-amber-100">
                    <div className="font-medium">{blocker.blockerType}</div>
                    <p className="text-amber-200/90">{blocker.message}</p>
                  </li>
                ))}
              </ul>
            )}
          </section>

          <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
            <div className="mb-3 flex items-center justify-between">
              <h3 className="font-semibold text-white">Activity</h3>
              <History className="h-4 w-4 text-sky-300" />
            </div>
            <div className="space-y-2 text-sm text-slate-300">
              <p className="rounded-lg bg-slate-900/50 p-2">Last updated: {new Date(asset.updatedAt).toLocaleString()}</p>
              <p className="rounded-lg bg-slate-900/50 p-2">Created: {new Date(asset.createdAt).toLocaleString()}</p>
            </div>
          </section>

          <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
            <div className="mb-3 flex items-center justify-between">
              <h3 className="font-semibold text-white">Compliance links</h3>
              <ShieldCheck className="h-4 w-4 text-sky-300" />
            </div>
            <p className="text-sm text-slate-400">Rulepack alignment and required evidence mapping are shown in Compliance Core integrations.</p>
            <div className="mt-3 flex items-center gap-2 text-xs text-slate-500">
              <FileText className="h-4 w-4" />
              Latest readiness basis: {readiness?.readinessBasis?.replace(/_/g, ' ') ?? 'Unavailable'}
            </div>
            <div className="mt-1 flex items-center gap-2 text-xs text-slate-500">
              {(readiness?.readinessStatus === 'ready' ? <CheckCircle2 className="h-4 w-4 text-emerald-300" /> : <XCircle className="h-4 w-4 text-amber-300" />)}
              Calculated at: {readiness?.calculatedAt ?? 'Unavailable'}
            </div>
          </section>
        </aside>
      </div>
    </div>
  )
}

