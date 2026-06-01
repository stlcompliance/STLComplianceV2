import { useMutation, useQuery } from '@tanstack/react-query'
import { useEffect, useMemo, useRef, useState } from 'react'
import { ApiErrorCallout, getErrorMessage, type PickerOption } from '@stl/shared-ui'
import * as nexarr from '../../api/nexarrClient'
import type {
  PlatformAuditPackageExportPreview,
  PlatformAuditPackageScope,
} from '../../api/types'
import { AuditExportTimelineCard } from './audit-export/AuditExportTimelineCard'
import { AuditExportActionsBar } from './audit-export/AuditExportActionsBar'
import { AuditExportFiltersCard } from './audit-export/AuditExportFiltersCard'
import { dateStamp, downloadBlob } from './audit-export/utils'


export function PlatformAuditPackageExportPanel() {
  const [fromDate, setFromDate] = useState('')
  const [toDate, setToDate] = useState('')
  const [tenantId, setTenantId] = useState('')
  const [action, setAction] = useState('')
  const [result, setResult] = useState('')
  const [targetType, setTargetType] = useState('')
  const [actorUserId, setActorUserId] = useState('')
  const [productKey, setProductKey] = useState('')
  const [timelinePage, setTimelinePage] = useState(1)
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

  const tenantsQuery = useQuery({
    queryKey: ['platform-admin-tenant-overview-audit'],
    queryFn: () => nexarr.getPlatformAdminTenantOverview(1, 200),
  })

  const filterOptions = filterOptionsQuery.data

  const tenantOptions: PickerOption[] = useMemo(
    () =>
      (tenantsQuery.data?.items ?? []).map((tenant) => ({
        value: tenant.tenantId,
        label: `${tenant.displayName} (${tenant.slug})`,
      })),
    [tenantsQuery.data?.items],
  )

  const actorOptions: PickerOption[] = useMemo(
    () =>
      (filterOptions?.actorUserIds ?? []).map((actorId) => ({
        value: actorId,
        label: actorId,
      })),
    [filterOptions?.actorUserIds],
  )

  const actionOptions: PickerOption[] = useMemo(
    () => (filterOptions?.actions ?? []).map((item) => ({ value: item, label: item })),
    [filterOptions?.actions],
  )

  const resultOptions: PickerOption[] = useMemo(
    () => (filterOptions?.results ?? []).map((item) => ({ value: item, label: item })),
    [filterOptions?.results],
  )

  const targetTypeOptions: PickerOption[] = useMemo(
    () => (filterOptions?.targetTypes ?? []).map((item) => ({ value: item, label: item })),
    [filterOptions?.targetTypes],
  )

  const productKeyOptions: PickerOption[] = useMemo(
    () => (filterOptions?.productKeys ?? []).map((item) => ({ value: item, label: item })),
    [filterOptions?.productKeys],
  )

  const summaryQuery = useQuery({
    queryKey: ['platform-audit-package-summary', scope],
    queryFn: () => nexarr.getPlatformAuditPackageExportSummary(scope),
  })

  const timelineQuery = useQuery({
    queryKey: ['platform-audit-package-timeline', scope, timelinePage],
    queryFn: () =>
      nexarr.getPlatformAuditPackageTimeline({
        ...scope,
        page: timelinePage,
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

  useEffect(() => {
    setTimelinePage(1)
  }, [
    fromDate,
    toDate,
    tenantId,
    action,
    result,
    targetType,
    actorUserId,
    productKey,
  ])

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
        <h2 className="text-lg font-semibold text-slate-50">Platform audit search &amp; export</h2>
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

      <AuditExportFiltersCard
        tenantId={tenantId}
        fromDate={fromDate}
        toDate={toDate}
        action={action}
        result={result}
        targetType={targetType}
        actorUserId={actorUserId}
        productKey={productKey}
        tenantOptions={tenantOptions}
        actionOptions={actionOptions}
        resultOptions={resultOptions}
        targetTypeOptions={targetTypeOptions}
        productKeyOptions={productKeyOptions}
        actorOptions={actorOptions}
        onTenantIdChange={setTenantId}
        onFromDateChange={setFromDate}
        onToDateChange={setToDate}
        onActionChange={setAction}
        onResultChange={setResult}
        onTargetTypeChange={setTargetType}
        onActorUserIdChange={setActorUserId}
        onProductKeyChange={setProductKey}
      />

      <div
        data-testid="platform-audit-summary-section"
        className="rounded-lg border border-slate-800 bg-slate-950/50 p-4"
      >
        <h3 className="text-sm font-medium text-slate-200">Export summary</h3>
        {summaryQuery.isLoading ? (
          <p className="mt-3 text-sm text-slate-500">Calculating scoped counts…</p>
        ) : summaryQuery.isError ? (
          <ApiErrorCallout
            message={getErrorMessage(summaryQuery.error, 'Failed to load export summary.')}
            onRetry={() => void summaryQuery.refetch()}
            retryLabel="Retry summary"
          />
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

      {timelineQuery.isError ? (
        <ApiErrorCallout
          message={getErrorMessage(timelineQuery.error, 'Failed to load audit timeline preview.')}
          onRetry={() => void timelineQuery.refetch()}
          retryLabel="Retry timeline"
        />
      ) : (
        <AuditExportTimelineCard
          timeline={timelineQuery.data}
          isLoading={timelineQuery.isLoading}
          page={timelinePage}
          onPreviousPage={() => setTimelinePage((value) => Math.max(1, value - 1))}
          onNextPage={() => {
            if (timelineQuery.data?.hasNextPage) {
              setTimelinePage((value) => value + 1)
            }
          }}
        />
      )}

      <AuditExportActionsBar
        exportBusy={exportBusy}
        zipPending={zipExportMutation.isPending}
        csvPending={csvExportMutation.isPending}
        jsonFilePending={jsonFileMutation.isPending}
        backgroundPending={backgroundZipMutation.isPending || Boolean(jobInFlight)}
        previewPending={jsonExportMutation.isPending}
        onZip={() => zipExportMutation.mutate()}
        onCsv={() => csvExportMutation.mutate()}
        onJsonFile={() => jsonFileMutation.mutate()}
        onBackgroundZip={() => backgroundZipMutation.mutate()}
        onPreviewJson={() => jsonExportMutation.mutate()}
      />

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
