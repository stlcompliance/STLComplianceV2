import { useMutation, useQuery } from '@tanstack/react-query'
import { ControlledSelect } from '@stl/shared-ui'
import { useEffect, useMemo, useRef, useState } from 'react'

import {
  createAuditPackageGenerationJob,
  downloadAuditPackageGenerationJob,
  exportAuditPackageCsv,
  exportAuditPackageJson,
  exportAuditPackageZip,
  getAuditPackageExportSummary,
  getAuditPackageFilterOptions,
  getAuditPackageGenerationJob,
  getAuditPackageManifest,
  getAuditPackageTimeline,
} from '../api/client'
import type { AuditPackageExportResponse, AuditPackageScope } from '../api/types'

type Props = {
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

function dateStamp() {
  return new Date().toISOString().slice(0, 10)
}

export function AuditPackageExportPanel({ accessToken, canRead, canExport }: Props) {
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  const [action, setAction] = useState('')
  const [result, setResult] = useState('')
  const [targetType, setTargetType] = useState('')
  const [actorUserId, setActorUserId] = useState('')
  const [lastJsonExport, setLastJsonExport] = useState<AuditPackageExportResponse | null>(null)
  const [activeJobId, setActiveJobId] = useState<string | null>(null)
  const downloadedJobIdRef = useRef<string | null>(null)

  const scope: AuditPackageScope = {
    from: fromDate || undefined,
    to: toDate || undefined,
    action: action || undefined,
    result: result || undefined,
    targetType: targetType || undefined,
    actorUserId: actorUserId.trim() || undefined,
  }

  const manifestQuery = useQuery({
    queryKey: ['routarr-audit-package-manifest', accessToken],
    queryFn: () => getAuditPackageManifest(accessToken),
    enabled: canRead,
  })

  const filterOptionsQuery = useQuery({
    queryKey: ['routarr-audit-package-filter-options', accessToken],
    queryFn: () => getAuditPackageFilterOptions(accessToken),
    enabled: canRead,
  })

  const summaryQuery = useQuery({
    queryKey: ['routarr-audit-package-summary', accessToken, scope],
    queryFn: () => getAuditPackageExportSummary(accessToken, scope),
    enabled: canRead,
  })

  const timelineQuery = useQuery({
    queryKey: ['routarr-audit-package-timeline', accessToken, scope],
    queryFn: () => getAuditPackageTimeline(accessToken, { ...scope, page: 1, pageSize: 15 }),
    enabled: canRead,
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
    mutationFn: () => exportAuditPackageZip(accessToken, scope),
    onSuccess: (blob) => downloadBlob(blob, `routarr-audit-package-${dateStamp()}.zip`),
  })

  const csvExportMutation = useMutation({
    mutationFn: () => exportAuditPackageCsv(accessToken, scope),
    onSuccess: (blob) => downloadBlob(blob, `routarr-audit-events-${dateStamp()}.csv`),
  })

  const jsonFileMutation = useMutation({
    mutationFn: () => exportAuditPackageJson(accessToken, scope),
    onSuccess: (payload) => {
      const blob = new Blob([JSON.stringify(payload, null, 2)], { type: 'application/json' })
      downloadBlob(blob, `routarr-audit-package-${dateStamp()}.json`)
    },
  })

  const backgroundZipMutation = useMutation({
    mutationFn: () => createAuditPackageGenerationJob(accessToken, { format: 'zip', ...scope }),
    onSuccess: (job) => {
      downloadedJobIdRef.current = null
      setActiveJobId(job.jobId)
    },
  })

  const jsonExportMutation = useMutation({
    mutationFn: () => exportAuditPackageJson(accessToken, scope),
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
    void downloadAuditPackageGenerationJob(accessToken, activeJobId).then((blob) => {
      downloadBlob(blob, `routarr-audit-package-${job.packageId ?? 'export'}.zip`)
    })
  }, [accessToken, activeJobId, jobStatusQuery.data])

  if (!canRead) {
    return null
  }

  const filterOptions = filterOptionsQuery.data
  const actorUserOptions = useMemo(
    () =>
      (filterOptions?.actorUserIds ?? []).map((id) => ({
        value: id,
        label: id,
      })),
    [filterOptions?.actorUserIds],
  )
  const summary = summaryQuery.data
  const jobStatus = jobStatusQuery.data
  const jobInFlight =
    Boolean(activeJobId) && jobStatus && (jobStatus.status === 'pending' || jobStatus.status === 'processing')
  const exportBusy =
    !canExport ||
    zipExportMutation.isPending ||
    csvExportMutation.isPending ||
    jsonFileMutation.isPending ||
    backgroundZipMutation.isPending ||
    jobInFlight

  return (
    <section
      data-testid="routarr-audit-export-panel"
      className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Audit package export</h2>
        <p className="mt-1 text-sm text-slate-400">
          Export tenant-scoped RoutArr audit events with optional filters. ZIP packages include JSON
          and CSV. Managers and admins can download; dispatchers can preview counts and timeline.
        </p>
      </header>

      <div data-testid="routarr-audit-manifest-section" className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
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
        <h3 className="text-sm font-medium text-slate-200">Export filters</h3>
        <div className="mt-3 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          <label className="block text-sm text-slate-300" htmlFor="auditpackageexport-from-optional">
          From (optional)
          <input id="auditpackageexport-from-optional"
              type="date"
              value={fromDate}
              onChange={(e) => setFromDate(e.target.value)}
              data-testid="routarr-audit-filter-from"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label className="block text-sm text-slate-300" htmlFor="auditpackageexport-to-optional">
          To (optional)
          <input id="auditpackageexport-to-optional"
              type="date"
              value={toDate}
              onChange={(e) => setToDate(e.target.value)}
              data-testid="routarr-audit-filter-to"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <label className="block text-sm text-slate-300" htmlFor="auditpackageexport-action">
          Action
          <select id="auditpackageexport-action"
              value={action}
              onChange={(e) => setAction(e.target.value)}
              data-testid="routarr-audit-filter-action"
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
          <label className="block text-sm text-slate-300" htmlFor="auditpackageexport-result">
          Result
          <select id="auditpackageexport-result"
              value={result}
              onChange={(e) => setResult(e.target.value)}
              data-testid="routarr-audit-filter-result"
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
          <label className="block text-sm text-slate-300" htmlFor="auditpackageexport-target-type">
          Target type
          <select id="auditpackageexport-target-type"
              value={targetType}
              onChange={(e) => setTargetType(e.target.value)}
              data-testid="routarr-audit-filter-target-type"
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
          <ControlledSelect
            label="Actor user"
            value={actorUserId}
            onChange={setActorUserId}
            options={actorUserOptions}
            emptyLabel="Any actor"
            testId="routarr-audit-filter-actor"
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 sm:col-span-2"
          />
        </div>
      </div>

      <div data-testid="routarr-audit-summary-section" className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Export summary</h3>
        {summaryQuery.isLoading ? (
          <p className="mt-3 text-sm text-slate-500">Calculating scoped counts…</p>
        ) : summary ? (
          <p className="mt-3 text-sm text-slate-300" data-testid="routarr-audit-summary-counts">
            {summary.counts.auditEvents} audit events in scope
          </p>
        ) : null}
      </div>

      <div data-testid="routarr-audit-timeline-section" className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Audit timeline preview</h3>
        {timelineQuery.isLoading ? (
          <p className="mt-3 text-sm text-slate-500">Loading audit timeline…</p>
        ) : timelineQuery.data && timelineQuery.data.items.length === 0 ? (
          <p className="mt-3 text-sm text-slate-500">No audit events match these filters.</p>
        ) : timelineQuery.data ? (
          <ul className="mt-3 divide-y divide-slate-800 text-sm">
            {timelineQuery.data.items.map((item) => (
              <li key={item.auditEventId} className="py-2">
                <div className="flex flex-wrap justify-between gap-2">
                  <span className="font-mono text-teal-300">{item.action}</span>
                  <span className="text-xs text-slate-500">
                    {new Date(item.occurredAt).toLocaleString()}
                  </span>
                </div>
                <p className="text-xs text-slate-400">
                  {item.targetType}
                  {item.targetId ? ` · ${item.targetId}` : ''} · {item.result}
                </p>
              </li>
            ))}
          </ul>
        ) : null}
      </div>

      {canExport ? (
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
            data-testid="routarr-audit-download-csv"
            className="rounded-md bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-600 disabled:opacity-50"
          >
            {csvExportMutation.isPending ? 'Exporting…' : 'Download audit CSV'}
          </button>
          <button
            type="button"
            onClick={() => jsonFileMutation.mutate()}
            disabled={exportBusy}
            data-testid="routarr-audit-download-json"
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
      ) : (
        <p className="text-sm text-slate-500">
          Export downloads require routarr manager or tenant admin role.
        </p>
      )}

      {jobStatus ? (
        <div
          data-testid="routarr-audit-job-status"
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
          data-testid="routarr-audit-json-preview"
          className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300"
        >
          <p>
            Package <span className="font-mono text-teal-300">{lastJsonExport.packageId}</span> ·{' '}
            {lastJsonExport.counts.auditEvents} audit events
          </p>
        </div>
      ) : null}
    </section>
  )
}
