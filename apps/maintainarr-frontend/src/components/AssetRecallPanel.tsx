import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ExternalLink, RefreshCw, ShieldAlert, Wrench } from 'lucide-react'
import { Link } from 'react-router-dom'

import {
  createAssetRecallCaseInspectionItem,
  createAssetRecallCaseWorkOrder,
  listAssetRecallCases,
  refreshAssetRecallCases,
} from '../api/client'
import type { AssetRecallCaseResponse } from '../api/types'

interface AssetRecallPanelProps {
  accessToken: string
  assetId: string
}

function formatDateTime(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return 'Not recorded'
  return date.toLocaleString()
}

function toneForStatus(status: string): string {
  const normalized = status.toLowerCase()
  if (['vin_confirmed_open', 'serial_confirmed_open', 'completed_claimed'].includes(normalized)) {
    return 'border-amber-500/30 bg-amber-500/10 text-amber-100'
  }
  if (['dismissed', 'confirmed_not_applicable', 'completed_verified', 'superseded'].includes(normalized)) {
    return 'border-emerald-500/30 bg-emerald-500/10 text-emerald-100'
  }
  if (['needs_manual_review', 'needs_vin_check', 'needs_serial_check'].includes(normalized)) {
    return 'border-sky-500/30 bg-sky-500/10 text-sky-100'
  }
  return 'border-slate-700 bg-slate-900 text-slate-200'
}

function toneForImpact(impact: string): string {
  const normalized = impact.toLowerCase()
  if (['park_it', 'park_outside', 'do_not_drive', 'out_of_service'].includes(normalized)) {
    return 'border-rose-500/30 bg-rose-500/10 text-rose-100'
  }
  if (['repair_required', 'inspect_before_use'].includes(normalized)) {
    return 'border-amber-500/30 bg-amber-500/10 text-amber-100'
  }
  return 'border-slate-700 bg-slate-900 text-slate-200'
}

function summaryTone(count: number): string {
  return count > 0 ? 'text-white' : 'text-slate-300'
}

function topCases(cases: AssetRecallCaseResponse[]): AssetRecallCaseResponse[] {
  return [...cases].sort((a, b) => {
    const aTime = Date.parse(a.lastRefreshedAt ?? a.detectedAt)
    const bTime = Date.parse(b.lastRefreshedAt ?? b.detectedAt)
    return bTime - aTime
  })
}

export function AssetRecallPanel({ accessToken, assetId }: AssetRecallPanelProps) {
  const queryClient = useQueryClient()

  const casesQuery = useQuery({
    queryKey: ['maintainarr-asset-recalls', assetId],
    queryFn: () => listAssetRecallCases(accessToken, assetId, { limit: 50 }),
    enabled: Boolean(accessToken && assetId),
  })

  const refreshMutation = useMutation({
    mutationFn: () => refreshAssetRecallCases(accessToken, assetId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-recalls', assetId] })
    },
  })

  const createWorkOrderMutation = useMutation({
    mutationFn: (caseId: string) => createAssetRecallCaseWorkOrder(accessToken, assetId, caseId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-recalls', assetId] })
    },
  })

  const createInspectionItemMutation = useMutation({
    mutationFn: (caseId: string) => createAssetRecallCaseInspectionItem(accessToken, assetId, caseId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-recalls', assetId] })
    },
  })

  const cases = topCases(casesQuery.data ?? [])
  const activeCases = cases.filter((item) => !['dismissed', 'confirmed_not_applicable', 'completed_verified', 'superseded'].includes(item.status.toLowerCase()))
  const verifiedOpenCases = cases.filter((item) => ['vin_confirmed_open', 'serial_confirmed_open'].includes(item.status.toLowerCase()))
  const holdCases = cases.filter((item) => item.readinessHoldId)
  const workOrdersCreated = cases.filter((item) => item.workOrderId)
  const latestReview = cases[0] ?? null

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <p className="text-sm font-medium text-white">Recall cases</p>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
            Active and historical recall cases tracked for this asset.
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Link
            to="/recalls"
            className="inline-flex items-center gap-2 rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-xs font-medium text-slate-100 hover:bg-slate-800"
          >
            <ExternalLink className="h-4 w-4" />
            Open recall dashboard
          </Link>
          <button
            type="button"
            className="inline-flex items-center gap-2 rounded-lg border border-slate-700 bg-slate-900 px-3 py-2 text-xs font-medium text-slate-100 hover:bg-slate-800 disabled:opacity-50"
            disabled={refreshMutation.isPending}
            onClick={() => refreshMutation.mutate()}
          >
            <RefreshCw className={`h-4 w-4 ${refreshMutation.isPending ? 'animate-spin' : ''}`} />
            Refresh recalls
          </button>
        </div>
      </div>

      <div className="grid gap-2 sm:grid-cols-2 xl:grid-cols-4">
        {[
          ['Tracked', cases.length],
          ['Active', activeCases.length],
          ['Verified open', verifiedOpenCases.length],
          ['Holds', holdCases.length],
        ].map(([label, count]) => (
          <div key={label} className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
            <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</div>
            <div className={`mt-1 text-lg font-semibold ${summaryTone(Number(count))}`}>{count}</div>
          </div>
        ))}
      </div>

      {casesQuery.isLoading ? (
        <p className="text-sm text-slate-400">Loading asset recall cases…</p>
      ) : cases.length === 0 ? (
        <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-4 text-sm text-slate-400">
          No recall cases have been recorded for this asset yet.
        </div>
      ) : (
        <div className="space-y-3">
          {cases.slice(0, 4).map((item) => (
            <article key={item.caseId} className="rounded-lg border border-slate-800 bg-slate-950/70 p-3">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <div className="text-sm font-medium text-white">{item.campaignNumber}</div>
                  <div className="mt-1 text-xs text-[var(--color-text-muted)]">
                    {item.component} · {item.manufacturer}
                  </div>
                </div>
                <span className={`rounded-full border px-2 py-0.5 text-[11px] uppercase tracking-wide ${toneForStatus(item.status)}`}>
                  {item.status}
                </span>
              </div>

              <div className="mt-3 grid gap-2 text-xs text-slate-400 md:grid-cols-2">
                <div className="rounded-md border border-slate-800 bg-slate-950/80 p-2">
                  <div className="text-[10px] uppercase tracking-wide text-[var(--color-text-muted)]">Readiness impact</div>
                  <div className={`mt-1 inline-flex rounded-full border px-2 py-0.5 text-[11px] uppercase tracking-wide ${toneForImpact(item.readinessImpact)}`}>
                    {item.readinessImpact}
                  </div>
                </div>
                <div className="rounded-md border border-slate-800 bg-slate-950/80 p-2">
                  <div className="text-[10px] uppercase tracking-wide text-[var(--color-text-muted)]">Verification</div>
                  <div className="mt-1 text-slate-200">{item.verificationStatus}</div>
                </div>
                <div className="rounded-md border border-slate-800 bg-slate-950/80 p-2">
                  <div className="text-[10px] uppercase tracking-wide text-[var(--color-text-muted)]">Detected</div>
                  <div className="mt-1 text-slate-200">{formatDateTime(item.detectedAt)}</div>
                </div>
                <div className="rounded-md border border-slate-800 bg-slate-950/80 p-2">
                  <div className="text-[10px] uppercase tracking-wide text-[var(--color-text-muted)]">Review</div>
                  <div className="mt-1 text-slate-200">{formatDateTime(item.lastRefreshedAt ?? item.detectedAt)}</div>
                </div>
              </div>

              <p className="mt-3 text-xs leading-5 text-slate-400">{item.reason}</p>

              <div className="mt-3 flex flex-wrap gap-2">
                <button
                  type="button"
                  className="inline-flex items-center gap-2 rounded-lg bg-sky-600 px-3 py-1.5 text-xs font-semibold text-white hover:bg-sky-500 disabled:opacity-50"
                  disabled={createWorkOrderMutation.isPending}
                  onClick={() => createWorkOrderMutation.mutate(item.caseId)}
                >
                  <Wrench className="h-4 w-4" />
                  Create work order
                </button>
                <button
                  type="button"
                  className="inline-flex items-center gap-2 rounded-lg border border-slate-700 bg-slate-900 px-3 py-1.5 text-xs font-semibold text-slate-100 hover:bg-slate-800 disabled:opacity-50"
                  disabled={createInspectionItemMutation.isPending}
                  onClick={() => createInspectionItemMutation.mutate(item.caseId)}
                >
                  <ShieldAlert className="h-4 w-4" />
                  Create inspection item
                </button>
              </div>
            </article>
          ))}
        </div>
      )}

      {latestReview ? (
        <p className="text-xs text-[var(--color-text-muted)]">
          Latest review: {latestReview.campaignNumber} · {formatDateTime(latestReview.lastRefreshedAt ?? latestReview.detectedAt)}
        </p>
      ) : null}

      {workOrdersCreated.length > 0 ? (
        <p className="text-xs text-[var(--color-text-muted)]">
          Work orders already created for {workOrdersCreated.length} case{workOrdersCreated.length === 1 ? '' : 's'}.
        </p>
      ) : null}
    </div>
  )
}
