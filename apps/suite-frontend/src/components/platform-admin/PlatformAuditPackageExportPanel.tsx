import { useMutation, useQuery } from '@tanstack/react-query'
import { useEffect, useRef, useState } from 'react'
import * as nexarr from '../../api/nexarrClient'
import type { PlatformAuditPackageExportPreview } from '../../api/types'

export function PlatformAuditPackageExportPanel() {
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  const [tenantId, setTenantId] = useState('')
  const [lastJsonExport, setLastJsonExport] = useState<PlatformAuditPackageExportPreview | null>(null)
  const [activeJobId, setActiveJobId] = useState<string | null>(null)
  const downloadedJobIdRef = useRef<string | null>(null)

  const scope = {
    from: fromDate || undefined,
    to: toDate || undefined,
    tenantId: tenantId.trim() || undefined,
  }

  const manifestQuery = useQuery({
    queryKey: ['platform-audit-package-manifest'],
    queryFn: () => nexarr.getPlatformAuditPackageManifest(),
  })

  const timelineQuery = useQuery({
    queryKey: ['platform-audit-package-timeline', scope],
    queryFn: () =>
      nexarr.getPlatformAuditPackageTimeline({
        ...scope,
        page: 1,
        pageSize: 15,
      }),
  })

  const jobStatusQuery = useQuery({
    queryKey: ['platform-audit-package-job', activeJobId],
    queryFn: () => nexarr.getPlatformAuditPackageGenerationJob(activeJobId!),
    enabled: Boolean(activeJobId),
    refetchInterval: (query) => {
      const status = query.state.data?.status
      return status === 'pending' || status === 'processing' ? 2000 : false
    },
  })

  const zipExportMutation = useMutation({
    mutationFn: () => nexarr.exportPlatformAuditPackageZip(scope),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `nexarr-platform-audit-package-${new Date().toISOString().slice(0, 10)}.zip`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  const backgroundZipMutation = useMutation({
    mutationFn: () =>
      nexarr.createPlatformAuditPackageGenerationJob({
        format: 'zip',
        ...scope,
      }),
    onSuccess: (job) => {
      downloadedJobIdRef.current = null
      setActiveJobId(job.jobId)
    },
  })

  const jsonExportMutation = useMutation({
    mutationFn: () => nexarr.exportPlatformAuditPackageJson(scope),
    onSuccess: setLastJsonExport,
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
    void nexarr.downloadPlatformAuditPackageGenerationJob(activeJobId).then((blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `nexarr-platform-audit-package-${job.packageId ?? 'export'}.zip`
      anchor.click()
      URL.revokeObjectURL(url)
    })
  }, [activeJobId, jobStatusQuery.data])

  const jobStatus = jobStatusQuery.data
  const jobInFlight =
    Boolean(activeJobId) && jobStatus && (jobStatus.status === 'pending' || jobStatus.status === 'processing')

  return (
    <section className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5">
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Platform audit package export</h2>
        <p className="mt-1 text-sm text-slate-400">
          Export NexArr control-plane audit events, tenants, entitlements, service clients, launch
          profiles, and callback allowlist metadata. No credential or token secrets are included.
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

      <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Audit timeline preview</h3>
        {timelineQuery.isLoading ? (
          <p className="mt-3 text-sm text-slate-500">Loading audit timeline…</p>
        ) : timelineQuery.data && timelineQuery.data.items.length === 0 ? (
          <p className="mt-3 text-sm text-slate-500">No audit events in this range.</p>
        ) : timelineQuery.data ? (
          <ul className="mt-3 divide-y divide-slate-800 text-sm">
            {timelineQuery.data.items.map((item) => (
              <li key={item.auditEventId} className="py-2">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-mono text-teal-300">{item.action}</span>
                  <span className="text-xs text-slate-500">
                    {new Date(item.occurredAt).toLocaleString()}
                  </span>
                </div>
                <p className="text-xs text-slate-400">
                  {item.targetType}
                  {item.targetId ? ` · ${item.targetId}` : ''} · {item.result}
                  {item.tenantId ? ` · tenant ${item.tenantId}` : ''}
                </p>
              </li>
            ))}
          </ul>
        ) : null}
      </div>

      <div className="grid gap-3 sm:grid-cols-3">
        <label className="block text-sm text-slate-300 sm:col-span-3">
          Tenant scope (optional GUID)
          <input
            type="text"
            value={tenantId}
            onChange={(event) => setTenantId(event.target.value)}
            placeholder="All tenants when empty"
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          />
        </label>
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

      <div className="flex flex-wrap gap-3">
        <button
          type="button"
          onClick={() => zipExportMutation.mutate()}
          disabled={zipExportMutation.isPending || jobInFlight}
          className="rounded-md bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
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

      {jobStatus ? (
        <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300">
          <p>
            Background job <span className="font-mono text-teal-300">{jobStatus.jobId}</span>:{' '}
            <span className="font-medium text-slate-100">{jobStatus.status}</span>
          </p>
          {jobStatus.errorMessage ? (
            <p className="mt-2 text-rose-400">{jobStatus.errorMessage}</p>
          ) : null}
        </div>
      ) : null}

      {lastJsonExport ? (
        <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300">
          <p>
            Package <span className="font-mono text-teal-300">{lastJsonExport.packageId}</span>{' '}
            generated at {new Date(lastJsonExport.generatedAt).toLocaleString()}.
          </p>
          <p className="mt-2">
            {lastJsonExport.counts.auditEvents} audit events · {lastJsonExport.counts.tenants} tenants
            · {lastJsonExport.counts.serviceClients} service clients
          </p>
        </div>
      ) : null}
    </section>
  )
}
