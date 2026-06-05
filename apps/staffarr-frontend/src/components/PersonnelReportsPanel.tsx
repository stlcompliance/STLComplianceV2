import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import {
  exportPersonnelReportSummaryCsv,
  getPersonnelReportSummary,
} from '../api/client'

interface PersonnelReportsPanelProps {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2">
      <p className="text-xs text-slate-400">{label}</p>
      <p className="text-lg font-semibold text-slate-50">{value}</p>
    </div>
  )
}

export function PersonnelReportsPanel({
  accessToken,
  canRead,
  canExport,
}: PersonnelReportsPanelProps) {
  const [employmentStatus, setEmploymentStatus] = useState('all')

  const summaryQuery = useQuery({
    queryKey: ['staffarr-personnel-report-summary', accessToken, employmentStatus],
    queryFn: () =>
      getPersonnelReportSummary(accessToken, {
        employmentStatus: employmentStatus === 'all' ? undefined : employmentStatus,
      }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportPersonnelReportSummaryCsv(accessToken, {
        employmentStatus: employmentStatus === 'all' ? undefined : employmentStatus,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `staffarr-personnel-report-${new Date().toISOString().slice(0, 10)}.csv`
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
      data-testid="personnel-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Personnel reports</h2>
          <p className="mt-1 text-sm text-slate-400">
            Workforce directory rollups by employment status from StaffArr-owned tables.
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

      <label htmlFor="personnel-reports-employment-status" className="mt-4 flex items-center gap-2 text-sm text-slate-300">
        <span>Employment status</span>
        <select
          id="personnel-reports-employment-status"
          className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
          value={employmentStatus}
          onChange={(event) => setEmploymentStatus(event.target.value)}
        >
          <option value="all">All</option>
          <option value="active">Active</option>
          <option value="inactive">Inactive</option>
          <option value="leave">On leave</option>
        </select>
      </label>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-400">Loading personnel report summary…</p>
      )}

      {summaryQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Personnel report unavailable"
            message={getErrorMessage(summaryQuery.error, 'Failed to load personnel report summary.')}
            retryLabel="Retry summary"
            onRetry={() => {
              void summaryQuery.refetch()
            }}
          />
        </div>
      )}

      {exportMutation.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="CSV export failed"
            message={getErrorMessage(exportMutation.error, 'Unable to export personnel report CSV.')}
          />
        </div>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="People in scope" value={String(summaryQuery.data.totalPeople)} />
            <MetricCard label="Active" value={String(summaryQuery.data.activeCount)} />
            <MetricCard label="Inactive" value={String(summaryQuery.data.inactiveCount)} />
            <MetricCard label="On leave" value={String(summaryQuery.data.onLeaveCount)} />
            <MetricCard
              label="Active percent"
              value={`${summaryQuery.data.activePercent.toFixed(1)}%`}
            />
          </div>

          {summaryQuery.data.recentPeople.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">No people match this filter.</p>
          ) : (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b border-slate-700 text-left text-slate-400">
                    <th className="px-2 py-2">Name</th>
                    <th className="px-2 py-2">Status</th>
                    <th className="px-2 py-2">Org unit</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryQuery.data.recentPeople.slice(0, 8).map((item) => (
                    <tr key={item.personId} className="border-b border-slate-800">
                      <td className="px-2 py-2 text-slate-100">{item.displayName}</td>
                      <td className="px-2 py-2 text-slate-300">{item.employmentStatus}</td>
                      <td className="px-2 py-2 text-slate-300">
                        {item.primaryOrgUnitName ?? '—'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}
    </section>
  )
}
