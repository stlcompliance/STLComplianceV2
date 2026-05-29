import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import {
  exportMaintenanceReportSummaryCsv,
  getMaintenanceReportAssetDetail,
  getMaintenanceReportSummary,
  getMaintenanceReportWorkOrderDetail,
} from '../api/client'

interface MaintenanceReportsPanelProps {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

export function MaintenanceReportsPanel({
  accessToken,
  canRead,
  canExport,
}: MaintenanceReportsPanelProps) {
  const [lifecycleFilter, setLifecycleFilter] = useState('')
  const [selectedAssetId, setSelectedAssetId] = useState<string | null>(null)
  const [selectedWorkOrderId, setSelectedWorkOrderId] = useState<string | null>(null)

  const summaryQuery = useQuery({
    queryKey: ['maintainarr-maintenance-report-summary', accessToken, lifecycleFilter],
    queryFn: () =>
      getMaintenanceReportSummary(accessToken, {
        lifecycleStatus: lifecycleFilter || undefined,
      }),
    enabled: canRead,
  })

  const assetDetailQuery = useQuery({
    queryKey: ['maintainarr-maintenance-report-asset', accessToken, selectedAssetId],
    queryFn: () => getMaintenanceReportAssetDetail(accessToken, selectedAssetId!),
    enabled: canRead && Boolean(selectedAssetId),
  })

  const workOrderDetailQuery = useQuery({
    queryKey: ['maintainarr-maintenance-report-work-order', accessToken, selectedWorkOrderId],
    queryFn: () => getMaintenanceReportWorkOrderDetail(accessToken, selectedWorkOrderId!),
    enabled: canRead && Boolean(selectedWorkOrderId),
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportMaintenanceReportSummaryCsv(accessToken, {
        lifecycleStatus: lifecycleFilter || undefined,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `maintainarr-maintenance-report-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  if (!canRead) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="maintenance-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Maintenance reports</h2>
          <p className="mt-1 text-sm text-slate-400">
            Fleet rollups across assets, work orders, defects, inspections, and PM schedules.
          </p>
        </div>
        {canExport ? (
          <button
            type="button"
            className="rounded-md bg-sky-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
            disabled={exportMutation.isPending}
            onClick={() => exportMutation.mutate()}
          >
            {exportMutation.isPending ? 'Exporting…' : 'Export CSV'}
          </button>
        ) : null}
      </div>

      <label className="mt-4 flex items-center gap-2 text-sm text-slate-300" htmlFor="maintenancereports-lifecycle">
          Lifecycle
          <select id="maintenancereports-lifecycle"
          className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
          value={lifecycleFilter}
          onChange={(event) => {
            setLifecycleFilter(event.target.value)
            setSelectedAssetId(null)
            setSelectedWorkOrderId(null)
          }}
        >
          <option value="">All</option>
          <option value="active">Active</option>
          <option value="inactive">Inactive</option>
          <option value="retired">Retired</option>
        </select>
      </label>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-500">Loading maintenance report summary…</p>
      )}

      {summaryQuery.isError && (
        <p className="mt-3 text-sm text-rose-400">Failed to load maintenance report summary.</p>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Assets" value={String(summaryQuery.data.totalAssetCount)} />
            <MetricCard label="Active assets" value={String(summaryQuery.data.activeAssetCount)} />
            <MetricCard
              label="Open work orders"
              value={String(
                summaryQuery.data.workOrderStatusCounts
                  .filter((x) => x.key === 'open' || x.key === 'in_progress')
                  .reduce((sum, x) => sum + x.count, 0),
              )}
            />
            <MetricCard
              label="Open defects"
              value={String(
                summaryQuery.data.defectStatusCounts
                  .filter((x) => ['open', 'acknowledged', 'in_repair'].includes(x.key))
                  .reduce((sum, x) => sum + x.count, 0),
              )}
            />
          </div>

          <div className="mt-4 overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-2 py-2">Asset</th>
                  <th className="px-2 py-2">Readiness</th>
                  <th className="px-2 py-2">Open WO</th>
                  <th className="px-2 py-2">Open defects</th>
                  <th className="px-2 py-2">PM overdue</th>
                </tr>
              </thead>
              <tbody>
                {summaryQuery.data.assets.map((asset) => (
                  <tr
                    key={asset.assetId}
                    className={`cursor-pointer border-t border-slate-800 hover:bg-slate-800/60 ${
                      selectedAssetId === asset.assetId ? 'bg-slate-800/80' : ''
                    }`}
                    onClick={() => {
                      setSelectedAssetId(asset.assetId)
                      setSelectedWorkOrderId(null)
                    }}
                  >
                    <td className="px-2 py-2 text-slate-100">
                      {asset.assetTag} — {asset.assetName}
                    </td>
                    <td className="px-2 py-2 text-slate-400">{asset.readinessStatus ?? '—'}</td>
                    <td className="px-2 py-2 text-slate-300">{asset.openWorkOrderCount}</td>
                    <td className="px-2 py-2 text-slate-300">{asset.openDefectCount}</td>
                    <td className="px-2 py-2 text-slate-300">{asset.overduePmScheduleCount}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}

      {assetDetailQuery.data && (
        <div className="mt-6 rounded-lg border border-slate-700 bg-slate-950/60 p-4">
          <h3 className="text-sm font-semibold text-slate-200">Asset detail</h3>
          <p className="mt-1 text-sm text-slate-400">
            {assetDetailQuery.data.summary.assetTag} — {assetDetailQuery.data.summary.assetName} (
            {assetDetailQuery.data.summary.lifecycleStatus})
          </p>
          <ul className="mt-3 space-y-2 text-sm text-slate-300">
            {assetDetailQuery.data.recentWorkOrders.map((row) => (
              <li key={row.workOrderId}>
                <button
                  type="button"
                  className="text-left text-violet-300 hover:text-violet-200"
                  onClick={() => setSelectedWorkOrderId(row.workOrderId)}
                >
                  {row.workOrderNumber}: {row.title} ({row.status})
                </button>
              </li>
            ))}
          </ul>
        </div>
      )}

      {workOrderDetailQuery.data && (
        <div className="mt-4 rounded-lg border border-violet-800/50 bg-violet-950/20 p-4 text-sm text-slate-200">
          <h3 className="font-semibold">Work order detail</h3>
          <p className="mt-1">
            {workOrderDetailQuery.data.workOrderNumber} — {workOrderDetailQuery.data.title}
          </p>
          <p className="mt-1 text-slate-400">
            {workOrderDetailQuery.data.assetTag} · {workOrderDetailQuery.data.status} ·{' '}
            {workOrderDetailQuery.data.totalLaborHours} labor hours · {workOrderDetailQuery.data.evidenceCount}{' '}
            evidence files
          </p>
        </div>
      )}
    </section>
  )
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-slate-700 bg-slate-950/50 px-3 py-2">
      <p className="text-xs uppercase tracking-wide text-slate-500">{label}</p>
      <p className="mt-1 text-lg font-semibold text-slate-100">{value}</p>
    </div>
  )
}
