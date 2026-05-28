import { useMutation, useQuery } from '@tanstack/react-query'
import { useEffect, useRef, useState } from 'react'
import * as nexarr from '../../api/nexarrClient'
import type {
  PlatformAuditPackageExportPreview,
  PlatformAuditPackageScope,
} from '../../api/types'

function downloadBlob(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = fileName
  anchor.click()
  URL.revokeObjectURL(url)
}

function dateStamp() {
  return new Date().toISOString().slice(0, 10)
}

export function PlatformAuditPackageExportPanel() {
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  const [tenantId, setTenantId] = useState('')
  const [action, setAction] = useState('')
  const [result, setResult] = useState('')
  const [targetType, setTargetType] = useState('')
  const [actorUserId, setActorUserId] = useState('')
  const [productKey, setProductKey] = useState('')
  const [lastJsonExport, setLastJsonExport] = useState<PlatformAuditPackageExportPreview | null>(
    null,
  )
  const [activeJobId, setActiveJobId] = useState<string | null>(null)
  const downloadedJobIdRef = useRef<string | null>(null)

  const scope: PlatformAuditPackageScope = {
    from: fromDate || undefined,
    to: toDate || undefined,
    tenantId: tenantId.trim() || undefined,
    action: action || undefined,
    result: result || undefined,
    targetType: targetType || undefined,
    actorUserId: actorUserId.trim() || undefined,
    productKey: productKey || undefined,
  }

  const manifestQuery = useQuery({
    queryKey: ['platform-audit-package-manifest'],
    queryFn: () => nexarr.getPlatformAuditPackageManifest(),
  })

  const filterOptionsQuery = useQuery({
    queryKey: ['platform-audit-package-filter-options', scope.tenantId],
    queryFn: () => nexarr.getPlatformAuditPackageFilterOptions({ tenantId: scope.tenantId }),
  })

  const summaryQuery = useQuery({
    queryKey: ['platform-audit-package-summary', scope],
    queryFn: () => nexarr.getPlatformAuditPackageExportSummary(scope),
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
      downloadBlob(blob, `nexarr-platform-audit-package-${dateStamp()}.zip`)
    },
  })

  const csvExportMutation = useMutation({
    mutationFn: () => nexarr.exportPlatformAuditPackageCsv(scope),
    onSuccess: (blob) => {
      downloadBlob(blob, `nexarr-platform-audit-events-${dateStamp()}.csv`)
    },
  })

  const jsonFileMutation = useMutation({
    mutationFn: () => nexarr.exportPlatformAuditPackageJson(scope),
    onSuccess: (payload) => {
      const blob = new Blob([JSON.stringify(payload, null, 2)], {
        type: 'application/json',
      })
      downloadBlob(blob, `nexarr-platform-audit-package-${dateStamp()}.json`)
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
      downloadBlob(blob, `nexarr-platform-audit-package-${job.packageId ?? 'export'}.zip`)
    })
  }, [activeJobId, jobStatusQuery.data])

  const filterOptions = filterOptionsQuery.data
  const summary = summaryQuery.data
  const jobStatus = jobStatusQuery.data
  const jobInFlight =
    Boolean(activeJobId) && jobStatus && (jobStatus.status === 'pending' || jobStatus.status === 'processing')
  const exportBusy =
    zipExportMutation.isPending ||
    csvExportMutation.isPending ||
    jsonFileMutation.isPending ||
    backgroundZipMutation.isPending ||
    jobInFlight

  return (
    <section
      data-testid="platform-audit-export-panel"
      className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Platform audit package export</h2>
        <p className="mt-1 text-sm text-slate-400">
          Export NexArr control-plane audit events, tenants, entitlements, service clients, launch
          profiles, and callback allowlist metadata. Filter by action, result, target type, actor,
          or product. ZIP packages include JSON and CSV audit sections. No credential or token
          secrets are included.
        </p>
      </header>

      <div
        data-testid="platform-audit-manifest-section"
        className="rounded-lg border border-slate-800 bg-slate-950/50 p-4"
      >
        <h3 className="text-sm font-medium text-slate-200">
          Package sections
          {manifestQuery.data?.packageVersion ? (
            <span className="ml-2 font-mono text-xs text-slate-500">
              v{manifestQuery.data.packageVersion}
            </span>
          ) : null}
        </h3>
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
        <h3 className="text-sm font-medium text-slate-200">Export filters</h3>
        <div className="mt-3 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          <label className="block text-sm text-slate-300 sm:col-span-2 lg:col-span-3">
            Tenant scope (optional GUID)
            <input
              type="text"
              value={tenantId}
              onChange={(event) => setTenantId(event.target.value)}
              placeholder="All tenants when empty"
              data-testid="platform-audit-filter-tenant"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label className="block text-sm text-slate-300">
            From (optional)
            <input
              type="date"
              value={fromDate}
              onChange={(event) => setFromDate(event.target.value)}
              data-testid="platform-audit-filter-from"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label className="block text-sm text-slate-300">
            To (optional)
            <input
              type="date"
              value={toDate}
              onChange={(event) => setToDate(event.target.value)}
              data-testid="platform-audit-filter-to"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label className="block text-sm text-slate-300">
            Action
            <select
              value={action}
              onChange={(event) => setAction(event.target.value)}
              data-testid="platform-audit-filter-action"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              <option value="">All actions</option>
              {(filterOptions?.actions ?? []).map((item) => (
                <option key={item} value={item}>
                  {item}
                </option>
              ))}
            </select>
          </label>
          <label className="block text-sm text-slate-300">
            Result
            <select
              value={result}
              onChange={(event) => setResult(event.target.value)}
              data-testid="platform-audit-filter-result"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              <option value="">All results</option>
              {(filterOptions?.results ?? []).map((item) => (
                <option key={item} value={item}>
                  {item}
                </option>
              ))}
            </select>
          </label>
          <label className="block text-sm text-slate-300">
            Target type
            <select
              value={targetType}
              onChange={(event) => setTargetType(event.target.value)}
              data-testid="platform-audit-filter-target-type"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              <option value="">All target types</option>
              {(filterOptions?.targetTypes ?? []).map((item) => (
                <option key={item} value={item}>
                  {item}
                </option>
              ))}
            </select>
          </label>
          <label className="block text-sm text-slate-300">
            Product key
            <select
              value={productKey}
              onChange={(event) => setProductKey(event.target.value)}
              data-testid="platform-audit-filter-product-key"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              <option value="">All products</option>
              {(filterOptions?.productKeys ?? []).map((item) => (
                <option key={item} value={item}>
                  {item}
                </option>
              ))}
            </select>
          </label>
          <label className="block text-sm text-slate-300 sm:col-span-2">
            Actor user ID (optional GUID)
            <input
              type="text"
              value={actorUserId}
              onChange={(event) => setActorUserId(event.target.value)}
              placeholder="Any actor when empty"
              data-testid="platform-audit-filter-actor"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
        </div>
      </div>

      <div
        data-testid="platform-audit-summary-section"
        className="rounded-lg border border-slate-800 bg-slate-950/50 p-4"
      >
        <h3 className="text-sm font-medium text-slate-200">Export summary</h3>
        {summaryQuery.isLoading ? (
          <p className="mt-3 text-sm text-slate-500">Calculating scoped counts…</p>
        ) : summary ? (
          <div className="mt-3 space-y-3 text-sm text-slate-300">
            <p data-testid="platform-audit-summary-counts">
              {summary.counts.auditEvents} audit events · {summary.counts.tenants} tenants ·{' '}
              {summary.counts.serviceClients} service clients · {summary.counts.tenantEntitlements}{' '}
              entitlements
            </p>
            {summary.byResult.length > 0 ? (
              <div>
                <p className="text-xs font-medium uppercase tracking-wide text-slate-500">
                  By result
                </p>
                <ul className="mt-1 flex flex-wrap gap-2">
                  {summary.byResult.map((item) => (
                    <li
                      key={item.key}
                      className="rounded-md bg-slate-800 px-2 py-1 font-mono text-xs text-slate-200"
                    >
                      {item.key}: {item.count}
                    </li>
                  ))}
                </ul>
              </div>
            ) : null}
            {summary.byAction.length > 0 ? (
              <div>
                <p className="text-xs font-medium uppercase tracking-wide text-slate-500">
                  Top actions
                </p>
                <ul className="mt-1 flex flex-wrap gap-2">
                  {summary.byAction.map((item) => (
                    <li
                      key={item.key}
                      className="rounded-md bg-slate-800 px-2 py-1 font-mono text-xs text-teal-200"
                    >
                      {item.key}: {item.count}
                    </li>
                  ))}
                </ul>
              </div>
            ) : null}
          </div>
        ) : (
          <p className="mt-3 text-sm text-slate-500">Summary unavailable.</p>
        )}
      </div>

      <div
        data-testid="platform-audit-timeline-section"
        className="rounded-lg border border-slate-800 bg-slate-950/50 p-4"
      >
        <h3 className="text-sm font-medium text-slate-200">Audit timeline preview</h3>
        {timelineQuery.isLoading ? (
          <p className="mt-3 text-sm text-slate-500">Loading audit timeline…</p>
        ) : timelineQuery.data && timelineQuery.data.items.length === 0 ? (
          <p className="mt-3 text-sm text-slate-500">No audit events match these filters.</p>
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

      <div className="flex flex-wrap gap-3">
        <button
          type="button"
          onClick={() => zipExportMutation.mutate()}
          disabled={exportBusy}
          className="rounded-md bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
        >
          {zipExportMutation.isPending ? 'Exporting…' : 'Download ZIP package'}
        </button>
        <button
          type="button"
          onClick={() => csvExportMutation.mutate()}
          disabled={exportBusy}
          data-testid="platform-audit-download-csv"
          className="rounded-md bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-600 disabled:opacity-50"
        >
          {csvExportMutation.isPending ? 'Exporting…' : 'Download audit CSV'}
        </button>
        <button
          type="button"
          onClick={() => jsonFileMutation.mutate()}
          disabled={exportBusy}
          data-testid="platform-audit-download-json"
          className="rounded-md bg-slate-600 px-4 py-2 text-sm font-medium text-slate-100 hover:bg-slate-500 disabled:opacity-50"
        >
          {jsonFileMutation.isPending ? 'Exporting…' : 'Download JSON package'}
        </button>
        <button
          type="button"
          onClick={() => backgroundZipMutation.mutate()}
          disabled={exportBusy}
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
        <div
          data-testid="platform-audit-job-status"
          data-job-status={jobStatus.status}
          className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300"
        >
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
        <div
          data-testid="platform-audit-json-preview"
          className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300"
        >
          <p>
            Package <span className="font-mono text-teal-300">{lastJsonExport.packageId}</span>{' '}
            generated at {new Date(lastJsonExport.generatedAt).toLocaleString()}.
          </p>
          <p className="mt-2">
            {lastJsonExport.counts.auditEvents} audit events · {lastJsonExport.counts.tenants}{' '}
            tenants · {lastJsonExport.counts.serviceClients} service clients
          </p>
        </div>
      ) : null}
    </section>
  )
}
