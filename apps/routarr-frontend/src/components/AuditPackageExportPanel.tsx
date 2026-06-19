import { useEffect, useRef, useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

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
  canRead: boolean
  canExport: boolean
}

function downloadBlob(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = fileName
  anchor.click()
  URL.revokeObjectURL(url)
}

export function AuditPackageExportPanel({ accessToken, canRead, canExport }: AuditPackageExportPanelProps) {
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  const [lastJsonExport, setLastJsonExport] = useState<AuditPackageExportResponse | null>(null)
  const [activeJobId, setActiveJobId] = useState<string | null>(null)
  const downloadedJobIdRef = useRef<string | null>(null)

  const manifestQuery = useQuery({
    queryKey: ['routarr-audit-package-manifest', accessToken],
    queryFn: () => getAuditPackageManifest(accessToken),
    enabled: canRead || canExport,
  })

  const jobStatusQuery = useQuery({
    queryKey: ['routarr-audit-package-job', accessToken, activeJobId],
    queryFn: () => getAuditPackageGenerationJob(accessToken, activeJobId!),
    enabled: canExport && Boolean(activeJobId),
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
      downloadBlob(blob, `routarr-audit-package-${new Date().toISOString().slice(0, 10)}.zip`)
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
      downloadBlob(blob, `routarr-audit-package-${job.packageId ?? 'export'}.zip`)
    })
  }, [accessToken, activeJobId, jobStatusQuery.data])

  if (!canRead && !canExport) {
    return null
  }

  const jobStatus = jobStatusQuery.data
  const jobInFlight =
    Boolean(activeJobId) && jobStatus && (jobStatus.status === 'pending' || jobStatus.status === 'processing')

  return (
    <section className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5" data-testid="routarr-audit-export-panel">
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Audit package export</h2>
        <p className="mt-1 text-sm text-slate-400">
          Export dispatch, route, proof, and audit data for compliance review.
        </p>
      </header>

      <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Package sections</h3>
        <ul className="mt-2 list-inside list-disc text-sm text-slate-400">
          {(manifestQuery.data?.sections ?? []).map((section) => (
            <li key={section.key}>
              <span className="font-mono text-slate-300">{section.fileName}</span>
              <span className="text-[var(--color-text-muted)]"> - {section.label}</span>
            </li>
          ))}
        </ul>
      </div>

      <div className="grid gap-3 sm:grid-cols-2">
        <label htmlFor="routarr-audit-filter-from" className="block text-sm text-slate-300">
          From date
          <input
            id="routarr-audit-filter-from"
            type="date"
            value={fromDate}
            onChange={(event) => setFromDate(event.target.value)}
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100"
          />
        </label>
        <label htmlFor="routarr-audit-filter-to" className="block text-sm text-slate-300">
          To date
          <input
            id="routarr-audit-filter-to"
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
            disabled={zipExportMutation.isPending || jobInFlight}
            className="rounded bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          >
            {zipExportMutation.isPending ? 'Exporting…' : 'Download ZIP package'}
          </button>
          <button
            type="button"
            onClick={() => backgroundZipMutation.mutate()}
            disabled={backgroundZipMutation.isPending || jobInFlight}
            className="rounded bg-indigo-700 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-600 disabled:opacity-50"
          >
            {backgroundZipMutation.isPending || jobInFlight ? 'Background export…' : 'Background ZIP export'}
          </button>
          <button
            type="button"
            onClick={() => jsonExportMutation.mutate()}
            disabled={jsonExportMutation.isPending}
            className="rounded border border-slate-600 px-4 py-2 text-sm text-slate-100 hover:bg-slate-800 disabled:opacity-50"
          >
            {jsonExportMutation.isPending ? 'Loading…' : 'Preview JSON export'}
          </button>
        </div>
      ) : (
        <p className="text-sm text-slate-400">Audit package export requires tenant admin or RoutArr admin access.</p>
      )}

      {jobStatus ? (
        <div
          className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300"
          data-testid="audit-package-job-status"
          data-job-status={jobStatus.status}
        >
          <p>
            Background job <span className="font-mono text-sky-300">{jobStatus.jobId}</span>:{' '}
            <span className="font-medium text-slate-100">{jobStatus.status}</span>
          </p>
          {jobStatus.packageId ? (
            <p className="mt-1">
              Package <span className="font-mono text-sky-300">{jobStatus.packageId}</span>
            </p>
          ) : null}
          {jobStatus.errorMessage ? <p className="mt-1 text-rose-400">{jobStatus.errorMessage}</p> : null}
        </div>
      ) : null}

      {lastJsonExport ? (
        <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300">
          <p>
            Package <span className="font-mono text-slate-100">{lastJsonExport.packageId}</span> generated at{' '}
            {new Date(lastJsonExport.generatedAt).toLocaleString()}
          </p>
          <ul className="mt-2 grid gap-1 sm:grid-cols-2">
            <li>Audit events: {lastJsonExport.counts.auditEvents}</li>
            <li>People: {lastJsonExport.counts.people}</li>
            <li>Permission history: {lastJsonExport.counts.permissionHistory}</li>
            <li>Person certifications: {lastJsonExport.counts.personCertifications}</li>
            <li>Personnel incidents: {lastJsonExport.counts.personnelIncidents}</li>
            <li>Readiness overrides: {lastJsonExport.counts.readinessOverrides}</li>
            <li>Training blockers: {lastJsonExport.counts.trainingBlockers}</li>
          </ul>
        </div>
      ) : null}

      {zipExportMutation.isError || jsonExportMutation.isError || backgroundZipMutation.isError ? (
        <ApiErrorCallout
          title="Export failed"
          message={getErrorMessage(
            zipExportMutation.error ?? jsonExportMutation.error ?? backgroundZipMutation.error,
            'Export failed. Try again.',
          )}
        />
      ) : null}
    </section>
  )
}
