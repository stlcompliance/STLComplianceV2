import type { AssetReadinessBlockerResponse, AssetReadinessResponse } from '../api/types'

interface AssetReadinessDetailPanelProps {
  readiness: AssetReadinessResponse | null
  isLoading: boolean
  selectedAssetLabel: string | null
}

function readinessBadgeClass(status: AssetReadinessResponse['readinessStatus']): string {
  return status === 'ready'
    ? 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    : 'bg-amber-500/20 text-amber-200 ring-amber-500/40'
}

function readinessLabel(status: AssetReadinessResponse['readinessStatus']): string {
  return status === 'ready' ? 'Ready for dispatch' : 'Not ready for dispatch'
}

function formatBasis(basis: string): string {
  return basis.replace(/_/g, ' ')
}

function blockerScopeLabel(blocker: AssetReadinessBlockerResponse): string {
  const referenceId = blocker.relatedEntityId ?? blocker.sourceEntityId
  return `${blocker.sourceEntityType} · ${referenceId.slice(0, 8)}…`
}

export function AssetReadinessDetailPanel({
  readiness,
  isLoading,
  selectedAssetLabel,
}: AssetReadinessDetailPanelProps) {
  const qualityHoldBlockers = readiness?.blockers.filter((blocker) => blocker.blockerType === 'quality_hold') ?? []
  const latestQualityHold = qualityHoldBlockers[0] ?? null

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/60 p-5"
      data-testid="asset-readiness-detail-panel"
    >
      <h2 className="text-lg font-medium text-white">Asset readiness detail</h2>
      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
        Blockers and maintenance signals for RoutArr dispatch consumers. Select an asset to load
        GET /api/asset-readiness detail.
      </p>

      {!selectedAssetLabel ? (
        <p className="mt-4 text-sm text-slate-400" data-testid="asset-readiness-detail-empty">
          Select an asset from the registry to review readiness blockers and signal counts.
        </p>
      ) : isLoading ? (
        <p className="mt-4 text-sm text-slate-400" data-testid="asset-readiness-detail-loading">
          Loading readiness for {selectedAssetLabel}…
        </p>
      ) : readiness ? (
        <div className="mt-4 space-y-4" data-testid="asset-readiness-detail-content">
          <div className="flex flex-wrap items-center gap-2">
            <span className="font-medium text-white">{readiness.assetTag}</span>
            <span className="text-slate-300">{readiness.assetName}</span>
            <span
              className={`rounded-full px-2 py-0.5 text-xs ring-1 ring-inset ${readinessBadgeClass(readiness.readinessStatus)}`}
              data-testid="asset-readiness-detail-status"
            >
              {readinessLabel(readiness.readinessStatus)}
            </span>
          </div>

          <dl className="grid gap-2 text-sm sm:grid-cols-2">
            <div>
              <dt className="text-[var(--color-text-muted)]">Lifecycle</dt>
              <dd className="text-slate-200">{readiness.lifecycleStatus}</dd>
            </div>
            <div>
              <dt className="text-[var(--color-text-muted)]">Basis</dt>
              <dd className="text-slate-200">{formatBasis(readiness.readinessBasis)}</dd>
            </div>
            <div className="sm:col-span-2">
              <dt className="text-[var(--color-text-muted)]">Calculated at</dt>
              <dd className="font-mono text-xs text-slate-300">{readiness.calculatedAt}</dd>
            </div>
          </dl>

          <div data-testid="asset-readiness-signals">
            <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-400">
              Signal counts
            </h3>
            <ul className="mt-2 grid gap-2 text-sm sm:grid-cols-2 lg:grid-cols-3">
              {[
                ['Critical defects', readiness.signals.openCriticalDefectCount],
                ['High defects', readiness.signals.openHighDefectCount],
                ['Active work orders', readiness.signals.activeWorkOrderCount],
                ['PM due', readiness.signals.pmDueCount],
                ['PM overdue', readiness.signals.pmOverdueCount],
                ['Failed inspections', readiness.signals.failedInspectionCount],
              ].map(([label, count]) => (
                <li
                  key={label}
                  className="rounded-lg border border-slate-800 px-3 py-2 text-slate-300"
                >
                  <span className="text-[var(--color-text-muted)]">{label}</span>
                  <span className="ml-2 font-medium text-white">{count}</span>
                </li>
              ))}
            </ul>
          </div>

          <div data-testid="asset-readiness-blockers">
            <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-400">
              Blockers ({readiness.blockers.length})
            </h3>
            {qualityHoldBlockers.length > 0 ? (
              <div
                className="mt-2 rounded-lg border border-rose-500/30 bg-rose-500/5 p-3 text-sm"
                data-testid="asset-readiness-hold-summary"
              >
                <div className="font-medium text-rose-100">Quality hold from AssurArr</div>
                <p className="mt-1 text-rose-200/90">
                  {qualityHoldBlockers.length} active quality hold
                  {qualityHoldBlockers.length === 1 ? '' : 's'} are blocking return-to-service. MaintainArr keeps this asset not ready until AssurArr releases the matching hold scope.
                </p>
                {latestQualityHold ? (
                  <div className="mt-2 space-y-1 text-xs text-rose-200/80">
                    <div>Latest hold message: {latestQualityHold.message}</div>
                    <div>Hold scope: {blockerScopeLabel(latestQualityHold)}</div>
                  </div>
                ) : null}
              </div>
            ) : null}
            {readiness.blockers.length === 0 ? (
              <p className="mt-2 text-sm text-emerald-300" data-testid="asset-readiness-blockers-empty">
                No maintenance blockers — asset is clear for dispatch gating.
              </p>
            ) : (
              <ul className="mt-2 space-y-2" data-testid="asset-readiness-blockers-list">
                {readiness.blockers.map((blocker) => (
                  <li
                    key={`${blocker.blockerType}-${blocker.sourceEntityId}`}
                    className="rounded-lg border border-amber-500/30 bg-amber-500/5 p-3 text-sm"
                  >
                    <div className="font-medium text-amber-100">{blocker.blockerType}</div>
                    <p className="mt-1 text-amber-200/90">{blocker.message}</p>
                    <p className="mt-2 font-mono text-xs text-slate-400">
                      {blocker.sourceEntityType} · {blocker.sourceEntityId.slice(0, 8)}…
                    </p>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      ) : (
        <p className="mt-4 text-sm text-slate-400" data-testid="asset-readiness-detail-unavailable">
          Readiness detail unavailable for {selectedAssetLabel}.
        </p>
      )}
    </section>
  )
}
