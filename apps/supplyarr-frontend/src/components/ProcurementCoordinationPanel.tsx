import { useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import { getProcurementCoordinationDashboard } from '../api/client'

interface ProcurementCoordinationPanelProps {
  accessToken: string
  canRead: boolean
}

function formatStage(stage: string): string {
  return stage.replaceAll('_', ' ')
}

export function ProcurementCoordinationPanel({ accessToken, canRead }: ProcurementCoordinationPanelProps) {
  const dashboardQuery = useQuery({
    queryKey: ['supplyarr-procurement-coordination', accessToken],
    queryFn: () => getProcurementCoordinationDashboard(accessToken, true),
    enabled: canRead,
  })

  if (!canRead) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2"
      data-testid="procurement-coordination-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Procurement coordination</h2>
      <p className="mt-1 text-sm text-slate-400">
        Materialized pipeline status across purchase requests, orders, and LoadArr receiving
        progress snapshots.
      </p>

      {dashboardQuery.isLoading && (
        <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading coordination dashboard…</p>
      )}

      {dashboardQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Procurement coordination unavailable"
            message={getErrorMessage(
              dashboardQuery.error,
              'Failed to load procurement coordination dashboard.',
            )}
            retryLabel="Retry dashboard"
            onRetry={() => {
              void dashboardQuery.refetch()
            }}
          />
        </div>
      )}

      {dashboardQuery.data && (
        <>
          <div className="mt-4 flex flex-wrap gap-3 text-sm">
            <span className="rounded-md bg-slate-800 px-3 py-1 text-slate-200">
              Active: {dashboardQuery.data.activeCount}
            </span>
            <span className="rounded-md bg-slate-800 px-3 py-1 text-slate-400">
              Terminal: {dashboardQuery.data.terminalCount}
            </span>
          </div>

          {dashboardQuery.data.items.length === 0 ? (
            <p className="mt-4 text-sm text-[var(--color-text-muted)]">No active coordination records yet.</p>
          ) : (
            <ul className="mt-4 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
              {dashboardQuery.data.items.map((item) => (
                <li key={item.coordinationRecordId || `${item.subjectType}-${item.subjectId}`} className="px-3 py-3">
                  <div className="flex flex-wrap items-start justify-between gap-2">
                    <div>
                      <div className="font-medium text-slate-100">
                        {item.documentKey} · {item.title}
                      </div>
                      <div className="text-xs text-[var(--color-text-muted)]">
                        {item.vendorDisplayName || 'No vendor'} · {item.documentStatus}
                      </div>
                    </div>
                    <span className="rounded bg-sky-950 px-2 py-0.5 text-xs uppercase tracking-wide text-sky-300">
                      {formatStage(item.coordinationStage)}
                    </span>
                  </div>
                  <p className="mt-2 text-slate-300">{item.nextActionRequired}</p>
                  {item.quantityOrdered > 0 && (
                    <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                      Received {item.quantityReceived} of {item.quantityOrdered}
                      {item.receiptProgressPercent != null ? ` (${item.receiptProgressPercent}%)` : ''}
                    </p>
                  )}
                </li>
              ))}
            </ul>
          )}
        </>
      )}
    </section>
  )
}
