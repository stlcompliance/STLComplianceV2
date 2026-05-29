import { useMutation, useQuery } from '@tanstack/react-query'
import { useEffect, useRef, useState } from 'react'

import {
  createAuditPackageGenerationJob,
  downloadAuditPackageGenerationJob,
  exportAuditPackageJson,
  exportAuditPackageZip,
  getAuditPackageGenerationJob,
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
  const [activeJobId, setActiveJobId] = useState<string | null>(null)
  const downloadedJobIdRef = useRef<string | null>(null)

  const manifestQuery = useQuery({
    queryKey: ['compliancecore-audit-package-manifest', accessToken],
    queryFn: () => getAuditPackageManifest(accessToken),
  })

  const jobStatusQuery = useQuery({
    queryKey: ['compliancecore-audit-package-job', accessToken, activeJobId],
    queryFn: () => getAuditPackageGenerationJob(accessToken, activeJobId!),
    enabled: Boolean(activeJobId),
    refetchInterval: (query) => {
      const status = query.state.data?.status
      return status === 'pending' || status === 'processing' ? 2000 : false
    },
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

  const backgroundZipMutation = useMutation({
    mutationFn: () =>
      createAuditPackageGenerationJob(accessToken, {
        format: 'zip',
        from: fromDate || undefined,
        to: toDate || undefined,
      }),
    onSuccess: (job) => {
      downloadedJobIdRef.current = null
      setActiveJobId(job.jobId)
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

  useEffect(() => {
    const job = jobStatusQuery.data
    if (!job || job.status !== 'completed' || !job.downloadReady || !activeJobId) {
      return
    }

    if (downloadedJobIdRef.current === job.jobId) {
      return
    }

    downloadedJobIdRef.current = job.jobId
    void downloadAuditPackageGenerationJob(accessToken, activeJobId).then((blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `compliancecore-audit-package-${job.packageId ?? 'export'}.zip`
      anchor.click()
      URL.revokeObjectURL(url)
    })
  }, [accessToken, activeJobId, jobStatusQuery.data])

  const jobStatus = jobStatusQuery.data
  const jobInFlight =
    Boolean(activeJobId)
    && jobStatus
    && (jobStatus.status === 'pending' || jobStatus.status === 'processing')

  return (
    <section
      data-testid="compliancecore-audit-export-panel"
      className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
    >
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
            disabled={zipExportMutation.isPending || jobInFlight}
            className="rounded-md bg-violet-600 px-4 py-2 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
          >
            {zipExportMutation.isPending ? 'Exporting…' : 'Download ZIP package'}
          </button>
          <button
            type="button"
            onClick={() => backgroundZipMutation.mutate()}
            disabled={backgroundZipMutation.isPending || jobInFlight}
            className="rounded-md bg-indigo-700 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-600 disabled:opacity-50"
          >
            {backgroundZipMutation.isPending || jobInFlight
              ? 'Background export…'
              : 'Background ZIP export'}
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

      {jobStatus ? (
        <div
          className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300"
          data-testid="audit-package-job-status"
        >
          <p>
            Background job <span className="font-mono text-violet-300">{jobStatus.jobId}</span>:{' '}
            <span className="font-medium text-slate-100">{jobStatus.status}</span>
          </p>
          {jobStatus.packageId ? (
            <p className="mt-1">
              Package <span className="font-mono text-violet-300">{jobStatus.packageId}</span>
              {jobStatus.completedAt
                ? ` · completed ${new Date(jobStatus.completedAt).toLocaleString()}`
                : null}
            </p>
          ) : null}
          {jobStatus.errorMessage ? (
            <p className="mt-1 text-rose-400">{jobStatus.errorMessage}</p>
          ) : null}
        </div>
      ) : null}

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

      {(zipExportMutation.isError
        || jsonExportMutation.isError
        || backgroundZipMutation.isError) && (
        <p className="text-sm text-red-300">Export failed. Check date range and try again.</p>
      )}
    </section>
  )
}
