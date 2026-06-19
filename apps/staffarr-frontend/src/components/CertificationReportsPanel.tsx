import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  exportCertificationReportSummaryCsv,
  getCertificationReportSummary,
} from '../api/client'

interface CertificationReportsPanelProps {
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

export function CertificationReportsPanel({
  accessToken,
  canRead,
  canExport,
}: CertificationReportsPanelProps) {
  const [missingOnly, setMissingOnly] = useState(false)
  const [expiringOnly, setExpiringOnly] = useState(false)

  const summaryQuery = useQuery({
    queryKey: ['staffarr-certification-report-summary', accessToken, missingOnly, expiringOnly],
    queryFn: () =>
      getCertificationReportSummary(accessToken, {
        missingOnly,
        expiringOnly,
      }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportCertificationReportSummaryCsv(accessToken, {
        missingOnly,
        expiringOnly,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `staffarr-certification-report-${new Date().toISOString().slice(0, 10)}.csv`
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
      data-testid="certification-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Certification reports</h2>
          <p className="mt-1 text-sm text-slate-400">
            Missing and expiring certification rollups from StaffArr-owned certification data.
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

      <div className="mt-4 flex flex-wrap gap-4 text-sm text-slate-300">
        <label htmlFor="certification-reports-missing-only" className="flex items-center gap-2">
          <input
            id="certification-reports-missing-only"
            type="checkbox"
            data-testid="certification-reports-missing-only"
            checked={missingOnly}
            onChange={(event) => setMissingOnly(event.target.checked)}
          />
          Missing only
        </label>
        <label htmlFor="certification-reports-expiring-only" className="flex items-center gap-2">
          <input
            id="certification-reports-expiring-only"
            type="checkbox"
            data-testid="certification-reports-expiring-only"
            checked={expiringOnly}
            onChange={(event) => setExpiringOnly(event.target.checked)}
          />
          Expiring only
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-400">Loading certification report summary…</p>
      )}

      {summaryQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Certification report unavailable"
            message={getErrorMessage(
              summaryQuery.error,
              'Failed to load certification report summary.',
            )}
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
            message={getErrorMessage(
              exportMutation.error,
              'Unable to export certification report CSV.',
            )}
          />
        </div>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="People in scope" value={String(summaryQuery.data.totalPeople)} />
            <MetricCard
              label="Active certifications"
              value={String(summaryQuery.data.activeCertificationCount)}
            />
            <MetricCard
              label="Expiring soon"
              value={String(summaryQuery.data.expiringSoonCount)}
            />
            <MetricCard
              label="Missing certifications"
              value={String(summaryQuery.data.missingCertificationCount)}
            />
          </div>

          {summaryQuery.data.recentCertifications.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">No certifications match this filter.</p>
          ) : (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b border-slate-700 text-left text-slate-400">
                    <th className="px-2 py-2">Person</th>
                    <th className="px-2 py-2">Certification</th>
                    <th className="px-2 py-2">Status</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryQuery.data.recentCertifications.slice(0, 8).map((item) => (
                    <tr key={item.personCertificationId} className="border-b border-slate-800">
                      <td className="px-2 py-2 text-slate-100">{item.personDisplayName}</td>
                      <td className="px-2 py-2 text-slate-300">
                        {item.certificationName}
                        <span className="ml-2 text-[var(--color-text-muted)]">({item.certificationKey})</span>
                      </td>
                      <td className="px-2 py-2 text-slate-300 capitalize">{item.status}</td>
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
