import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'

import { exportAuditPackageJson, exportAuditPackageZip, getAuditPackageManifest } from '../api/client'
import type { AuditPackageExportResponse } from '../api/types'

interface AuditPackageExportPanelProps {
  accessToken: string
  canExport: boolean
}

export function AuditPackageExportPanel({ accessToken, canExport }: AuditPackageExportPanelProps) {
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  const [lastJsonExport, setLastJsonExport] = useState<AuditPackageExportResponse | null>(null)

  const manifestQuery = useQuery({
    queryKey: ['compliancecore-audit-package-manifest', accessToken],
    queryFn: () => getAuditPackageManifest(accessToken),
  })

  const zipExportMutation = useMutation({
    mutationFn: () =>
      exportAuditPackageZip(accessToken, {
        from: fromDate || undefined,
        to: toDate || undefined,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `compliancecore-audit-package-${new Date().toISOString().slice(0, 10)}.zip`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  const jsonExportMutation = useMutation({
    mutationFn: () =>
      exportAuditPackageJson(accessToken, {
        from: fromDate || undefined,
        to: toDate || undefined,
      }),
    onSuccess: (result) => {
      setLastJsonExport(result)
    },
  })

  return (
    <section className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5">
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Audit package export</h2>
        <p className="mt-1 text-sm text-slate-400">
          Export tenant audit events, findings, evaluation runs, and rule pack metadata for compliance
          review. Findings and evaluations can be filtered by date; rule packs include full tenant
          metadata snapshot.
        </p>
      </header>

      <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Package sections</h3>
        <ul className="mt-2 list-inside list-disc text-sm text-slate-400">
          {(manifestQuery.data?.sections ?? []).map((section) => (
            <li key={section.key}>
              <span className="font-mono text-slate-300">{section.fileName}</span>
              <span className="text-slate-500"> — {section.label}</span>
            </li>
          ))}
        </ul>
      </div>

      <div className="grid gap-3 sm:grid-cols-2">
        <label className="block text-sm text-slate-300">
          From (optional)
          <input
            type="date"
            value={fromDate}
            onChange={(event) => setFromDate(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          />
        </label>
        <label className="block text-sm text-slate-300">
          To (optional)
          <input
            type="date"
            value={toDate}
            onChange={(event) => setToDate(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          />
        </label>
      </div>

      {canExport ? (
        <div className="flex flex-wrap gap-3">
          <button
            type="button"
            onClick={() => zipExportMutation.mutate()}
            disabled={zipExportMutation.isPending}
            className="rounded-md bg-violet-600 px-4 py-2 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
          >
            {zipExportMutation.isPending ? 'Exporting…' : 'Download ZIP package'}
          </button>
          <button
            type="button"
            onClick={() => jsonExportMutation.mutate()}
            disabled={jsonExportMutation.isPending}
            className="rounded-md bg-slate-700 px-4 py-2 text-sm font-medium text-slate-100 hover:bg-slate-600 disabled:opacity-50"
          >
            {jsonExportMutation.isPending ? 'Loading…' : 'Preview JSON export'}
          </button>
        </div>
      ) : (
        <p className="text-sm text-slate-500">
          Audit package export requires compliance admin, compliance reviewer, or tenant admin role.
        </p>
      )}

      {lastJsonExport ? (
        <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300">
          <p>
            Package <span className="font-mono text-violet-300">{lastJsonExport.packageId}</span> ·{' '}
            {lastJsonExport.counts.auditEvents} audit events · {lastJsonExport.counts.findings}{' '}
            findings · {lastJsonExport.counts.evaluationRuns} evaluations ·{' '}
            {lastJsonExport.counts.rulePacks} rule packs
          </p>
        </div>
      ) : null}

      {(zipExportMutation.isError || jsonExportMutation.isError) && (
        <p className="text-sm text-red-300">Export failed. Check date range and try again.</p>
      )}
    </section>
  )
}
