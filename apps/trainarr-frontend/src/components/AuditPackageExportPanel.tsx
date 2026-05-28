import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'

import {
  exportAuditPackageJson,
  exportAuditPackageZip,
  getAuditPackageManifest,
} from '../api/client'
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
    queryKey: ['trainarr-audit-package-manifest', accessToken],
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
      anchor.download = `trainarr-audit-package-${new Date().toISOString().slice(0, 10)}.zip`
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
    <section
      className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="trainarr-audit-package-export-panel"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Training audit package export</h2>
        <p className="mt-1 text-sm text-slate-400">
          Export tenant training definitions, programs, assignments, evidence metadata, evaluations,
          signoffs, qualifications, StaffArr publications, and person training history for compliance
          review. Date filters apply to time-bounded sections.
        </p>
      </header>

      <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Package sections</h3>
        <ul className="mt-2 list-inside list-disc text-sm text-slate-400">
          {(manifestQuery.data?.sections ?? []).map((section) => (
            <li key={section.key}>
              {section.label} ({section.fileName})
            </li>
          ))}
        </ul>
      </div>

      <div className="grid gap-3 sm:grid-cols-2">
        <label className="block text-sm text-slate-300">
          From date
          <input
            type="date"
            value={fromDate}
            onChange={(event) => setFromDate(event.target.value)}
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100"
          />
        </label>
        <label className="block text-sm text-slate-300">
          To date
          <input
            type="date"
            value={toDate}
            onChange={(event) => setToDate(event.target.value)}
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100"
          />
        </label>
      </div>

      {canExport ? (
        <div className="flex flex-wrap gap-3">
          <button
            type="button"
            onClick={() => zipExportMutation.mutate()}
            disabled={zipExportMutation.isPending}
            className="rounded bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          >
            Download ZIP package
          </button>
          <button
            type="button"
            onClick={() => jsonExportMutation.mutate()}
            disabled={jsonExportMutation.isPending}
            className="rounded border border-slate-600 px-4 py-2 text-sm text-slate-100 hover:bg-slate-800 disabled:opacity-50"
          >
            Preview JSON export
          </button>
        </div>
      ) : (
        <p className="text-sm text-slate-400">
          Training audit package export requires tenant admin or TrainArr admin access.
        </p>
      )}

      {lastJsonExport ? (
        <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300">
          <p>
            Package <span className="font-mono text-slate-100">{lastJsonExport.packageId}</span> generated at{' '}
            {new Date(lastJsonExport.generatedAt).toLocaleString()}
          </p>
          <ul className="mt-2 grid gap-1 sm:grid-cols-2">
            <li>Audit events: {lastJsonExport.counts.auditEvents}</li>
            <li>Definitions: {lastJsonExport.counts.trainingDefinitions}</li>
            <li>Programs: {lastJsonExport.counts.trainingPrograms}</li>
            <li>Assignments: {lastJsonExport.counts.trainingAssignments}</li>
            <li>Evidence: {lastJsonExport.counts.trainingEvidence}</li>
            <li>Evaluations: {lastJsonExport.counts.trainingEvaluations}</li>
            <li>Signoffs: {lastJsonExport.counts.trainingSignoffs}</li>
            <li>Qualifications: {lastJsonExport.counts.qualificationIssues}</li>
            <li>Publications: {lastJsonExport.counts.certificationPublications}</li>
            <li>History entries: {lastJsonExport.counts.personTrainingHistory}</li>
          </ul>
        </div>
      ) : null}
    </section>
  )
}
