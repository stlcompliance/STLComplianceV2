import { useMutation, useQuery } from '@tanstack/react-query'
import { useEffect, useMemo, useRef, useState } from 'react'
import {
  ApiErrorCallout,
  formatProductDisplayName,
  getErrorMessage,
  type PickerOption,
} from '@stl/shared-ui'
import * as nexarr from '../../api/nexarrClient'
import type {
  PlatformAuditPackageExportPreview,
  PlatformAuditPackageScope,
} from '../../api/types'
import { AuditExportActionsBar } from './audit-export/AuditExportActionsBar'
import { AuditExportFiltersCard } from './audit-export/AuditExportFiltersCard'
import { AuditExportManifestCard } from './audit-export/AuditExportManifestCard'
import { AuditExportSummaryCard } from './audit-export/AuditExportSummaryCard'
import { AuditExportTimelineCard } from './audit-export/AuditExportTimelineCard'
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
        label: tenant.displayName,
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
    () =>
      (filterOptions?.productKeys ?? []).map((item) => ({
        value: item,
        label: formatProductDisplayName(item),
      })),
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
  const jobInFlight = Boolean(
    activeJobId && jobStatus && (jobStatus.status === 'pending' || jobStatus.status === 'processing'),
  )
  const exportBusy =
    zipExportMutation.isPending ||
    csvExportMutation.isPending ||
    jsonFileMutation.isPending ||
    backgroundZipMutation.isPending ||
    jobInFlight

  return (
    <section
      data-testid="platform-audit-export-panel"
      className="space-y-4 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Platform audit search &amp; export</h2>
        <p className="mt-1 text-sm text-[var(--color-text-muted)]">
          Export NexArr platform audit events, tenants, tenant product destinations,
          service clients, launch settings, and callback allowlist metadata. Filter by action,
          result, target type, actor, or product. ZIP packages include JSON and CSV audit
          sections. No credential or token secrets are included.
        </p>
      </header>

      <AuditExportManifestCard
        manifest={manifestQuery.data}
        isError={manifestQuery.isError}
        error={manifestQuery.error}
        onRetry={() => void manifestQuery.refetch()}
      />

      {(filterOptionsQuery.isError || tenantsQuery.isError) ? (
        <ApiErrorCallout
          message={
            filterOptionsQuery.isError
              ? getErrorMessage(filterOptionsQuery.error, 'Failed to load filter options.')
              : getErrorMessage(tenantsQuery.error, 'Failed to load tenant filter data.')
          }
          onRetry={() => {
            if (filterOptionsQuery.isError) {
              void filterOptionsQuery.refetch()
            }
            if (tenantsQuery.isError) {
              void tenantsQuery.refetch()
            }
          }}
          retryLabel="Retry filters"
        />
      ) : null}

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

      <AuditExportSummaryCard
        isLoading={summaryQuery.isLoading}
        isError={summaryQuery.isError}
        error={summaryQuery.error}
        summary={summary}
        onRetry={() => void summaryQuery.refetch()}
      />

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
          className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4 text-sm text-[var(--color-text-secondary)]"
        >
          <p>
            Background job <span className="font-mono text-[var(--color-accent)]">{jobStatus.jobId}</span>:{' '}
            <span className="font-medium text-[var(--color-text-primary)]">{jobStatus.status}</span>
          </p>
          {jobStatus.errorMessage ? (
            <p className="mt-2 text-[var(--color-danger-text)]">{jobStatus.errorMessage}</p>
          ) : null}
        </div>
      ) : null}

      {lastJsonExport ? (
        <div
          data-testid="platform-audit-json-preview"
          className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-4 text-sm text-[var(--color-text-secondary)]"
        >
          <p>
            Package <span className="font-mono text-[var(--color-accent)]">{lastJsonExport.packageId}</span>{' '}
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
