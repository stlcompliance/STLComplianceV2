import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import {
  createDemandProcessingPrDraft,
  getDemandProcessingDashboard,
  getDemandProcessingDetail,
  retryDemandProcessing,
} from '../api/client'
import type { DemandProcessingSummaryResponse } from '../api/types'

interface DemandProcessingPanelProps {
  accessToken: string
  canRead: boolean
  canOperate?: boolean
}

function formatOutcome(outcome: string | null): string {
  if (!outcome) return 'pending'
  return outcome.replaceAll('_', ' ')
}

function outcomeBadgeClass(outcome: string | null): string {
  switch (outcome) {
    case 'stock_available':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'stock_short':
      return 'bg-amber-500/20 text-amber-300 ring-amber-500/40'
    case 'pr_drafted':
      return 'bg-violet-500/20 text-violet-300 ring-violet-500/40'
    case 'no_catalog_parts':
      return 'bg-rose-500/20 text-rose-300 ring-rose-500/40'
    default:
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
  }
}

function DemandRefRow({
  item,
  accessToken,
  canOperate,
  isSelected,
  onSelect,
  onActionComplete,
}: {
  item: DemandProcessingSummaryResponse
  accessToken: string
  canOperate: boolean
  isSelected: boolean
  onSelect: () => void
  onActionComplete: () => void
}) {
  const detailQuery = useQuery({
    queryKey: ['supplyarr-demand-processing-detail', accessToken, item.demandRefId],
    queryFn: () => getDemandProcessingDetail(accessToken, item.demandRefId),
    enabled: isSelected,
  })

  const retryMutation = useMutation({
    mutationFn: () => retryDemandProcessing(accessToken, item.demandRefId),
    onSuccess: onActionComplete,
  })

  const createPrMutation = useMutation({
    mutationFn: () => createDemandProcessingPrDraft(accessToken, item.demandRefId),
    onSuccess: onActionComplete,
  })

  const canCreatePr =
    canOperate
    && !item.purchaseRequestId
    && item.demandRefStatus === 'received'

  return (
    <li
      className="px-3 py-3"
      data-testid={`demand-processing-row-${item.demandRefId}`}
    >
      <div className="flex flex-wrap items-start justify-between gap-2">
        <button
          type="button"
          className="min-w-0 flex-1 text-left"
          onClick={onSelect}
        >
          <div className="font-medium text-slate-100">
            {item.demandRefSource} · {item.sourceRefKey} · {item.title}
          </div>
          <div className="text-xs text-slate-500">
            {item.sourceLink.displayLabel} · status {item.demandRefStatus}
            {item.linesShortCount != null
              ? ` · ${item.linesShortCount} short of ${item.linesCatalogCount ?? 0} catalog lines`
              : ''}
          </div>
        </button>
        <span
          className={`rounded px-2 py-0.5 text-xs ring-1 ${outcomeBadgeClass(item.processingOutcome)}`}
        >
          {formatOutcome(item.processingOutcome)}
        </span>
      </div>

      {item.lastProcessingMessage ? (
        <p className="mt-2 text-slate-300">{item.lastProcessingMessage}</p>
      ) : null}

      {item.purchaseRequestId ? (
        <p className="mt-2 text-xs text-violet-300">
          Linked PR {item.purchaseRequestId.slice(0, 8)}…
        </p>
      ) : null}

      {canOperate ? (
        <div className="mt-2 flex flex-wrap gap-2">
          <button
            type="button"
            className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-200 disabled:opacity-50"
            disabled={retryMutation.isPending}
            onClick={() => retryMutation.mutate()}
          >
            Retry processing
          </button>
          {canCreatePr ? (
            <button
              type="button"
              className="rounded bg-violet-700 px-2 py-1 text-xs text-white disabled:opacity-50"
              disabled={createPrMutation.isPending}
              onClick={() => createPrMutation.mutate()}
            >
              Create PR draft
            </button>
          ) : null}
          <button
            type="button"
            className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-400"
            onClick={onSelect}
          >
            {isSelected ? 'Hide status' : 'View status'}
          </button>
        </div>
      ) : null}

      {isSelected && detailQuery.data ? (
        <div
          className="mt-3 rounded-md border border-slate-800 bg-slate-950/60 p-3 text-xs text-slate-400"
          data-testid="demand-processing-detail"
        >
          <p className="font-medium text-slate-300">Source link</p>
          <p className="mt-1">{detailQuery.data.summary.sourceLink.displayLabel}</p>
          <p className="mt-2 font-medium text-slate-300">Line availability</p>
          <ul className="mt-1 space-y-1">
            {detailQuery.data.lines.map((line) => (
              <li key={line.lineId}>
                {line.partNumber}: requested {line.quantityRequested}, available{' '}
                {line.quantityAvailable}
                {line.isShort ? ' (short)' : ''}
              </li>
            ))}
          </ul>
        </div>
      ) : null}
    </li>
  )
}

export function DemandProcessingPanel({
  accessToken,
  canRead,
  canOperate = false,
}: DemandProcessingPanelProps) {
  const queryClient = useQueryClient()
  const [selectedId, setSelectedId] = useState<string | null>(null)

  const dashboardQuery = useQuery({
    queryKey: ['supplyarr-demand-processing', accessToken],
    queryFn: () => getDemandProcessingDashboard(accessToken),
    enabled: canRead,
  })

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: ['supplyarr-demand-processing', accessToken] })
    void queryClient.invalidateQueries({
      queryKey: ['supplyarr-demand-processing-detail', accessToken],
    })
  }

  if (!canRead) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2"
      data-testid="demand-processing-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Demand processing</h2>
      <p className="mt-1 text-sm text-slate-400">
        Stock evaluation and procurement actions for demand references from enabled sources.
        Use Retry processing to refresh outcomes; Create PR draft when stock is short.
      </p>

      {dashboardQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-500">Loading demand processing dashboard…</p>
      )}

      {dashboardQuery.isError && (
        <p className="mt-3 text-sm text-rose-400">Failed to load demand processing dashboard.</p>
      )}

      {dashboardQuery.data && (
        <>
          <div className="mt-4 flex flex-wrap gap-3 text-sm">
            <span className="rounded-md bg-slate-800 px-3 py-1 text-slate-200">
              Pending: {dashboardQuery.data.pendingCount}
            </span>
            <span className="rounded-md bg-amber-950 px-3 py-1 text-amber-200">
              Stock short: {dashboardQuery.data.stockShortCount}
            </span>
            <span className="rounded-md bg-emerald-950 px-3 py-1 text-emerald-200">
              In stock: {dashboardQuery.data.stockAvailableCount}
            </span>
            <span className="rounded-md bg-violet-950 px-3 py-1 text-violet-200">
              PR drafted: {dashboardQuery.data.prDraftedCount}
            </span>
          </div>

          {dashboardQuery.data.pendingItems.length > 0 ? (
            <div className="mt-4">
              <h3 className="text-sm font-medium text-slate-200">Pending queue</h3>
              <ul className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
                {dashboardQuery.data.pendingItems.map((item) => (
                  <DemandRefRow
                    key={`pending-${item.demandRefId}`}
                    item={item}
                    accessToken={accessToken}
                    canOperate={canOperate}
                    isSelected={selectedId === item.demandRefId}
                    onSelect={() =>
                      setSelectedId((current) =>
                        current === item.demandRefId ? null : item.demandRefId,
                      )
                    }
                    onActionComplete={invalidate}
                  />
                ))}
              </ul>
            </div>
          ) : null}

          {dashboardQuery.data.processedItems.length > 0 ? (
            <div className="mt-4">
              <h3 className="text-sm font-medium text-slate-200">Recently processed</h3>
              <ul className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
                {dashboardQuery.data.processedItems.map((item) => (
                  <DemandRefRow
                    key={`processed-${item.processingStateId ?? item.demandRefId}`}
                    item={item}
                    accessToken={accessToken}
                    canOperate={canOperate}
                    isSelected={selectedId === item.demandRefId}
                    onSelect={() =>
                      setSelectedId((current) =>
                        current === item.demandRefId ? null : item.demandRefId,
                      )
                    }
                    onActionComplete={invalidate}
                  />
                ))}
              </ul>
            </div>
          ) : null}

          {dashboardQuery.data.pendingItems.length === 0
            && dashboardQuery.data.processedItems.length === 0 ? (
            <p className="mt-4 text-sm text-slate-500">No demand references in the processing queues.</p>
          ) : null}
        </>
      )}
    </section>
  )
}
