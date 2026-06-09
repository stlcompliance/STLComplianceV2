import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import * as nexarr from '../../api/nexarrClient'
import { useToast } from '../../feedback'

const MASTER_CSV_DATASET_KEY = 'master-reference-intake'

function StatCard({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
      <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">
        {label}
      </p>
      <p className="mt-2 text-3xl font-semibold text-stl-navy">{value}</p>
    </div>
  )
}

function Field({
  label,
  children,
}: {
  label: string
  children: ReactNode
}) {
  return (
    <label className="block text-sm text-slate-700">
      <span className="font-medium text-slate-700">{label}</span>
      <div className="mt-1">{children}</div>
    </label>
  )
}

function Section({
  title,
  description,
  children,
}: {
  title: string
  description: string
  children: ReactNode
}) {
  return (
    <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h3 className="text-lg font-semibold text-stl-navy">{title}</h3>
          <p className="mt-1 text-sm text-slate-600">{description}</p>
        </div>
      </div>
      <div className="mt-4">{children}</div>
    </section>
  )
}

function StatusBadge({ value }: { value: string }) {
  const normalized = value.toLowerCase()
  const classes =
    normalized.includes('active') || normalized.includes('published') || normalized.includes('ready')
      ? 'border-emerald-200 bg-emerald-50 text-emerald-700'
      : normalized.includes('review') || normalized.includes('pending')
        ? 'border-amber-200 bg-amber-50 text-amber-700'
        : normalized.includes('failed') || normalized.includes('reject')
          ? 'border-rose-200 bg-rose-50 text-rose-700'
          : 'border-slate-200 bg-slate-50 text-slate-600'

  return (
    <span className={`inline-flex rounded-full border px-2.5 py-1 text-xs font-medium ${classes}`}>
      {value}
    </span>
  )
}

export function ReferenceDataPage() {
  const queryClient = useQueryClient()
  const { pushToast } = useToast()
  const [datasetKey, setDatasetKey] = useState('')
  const [datasetName, setDatasetName] = useState('')
  const [datasetCategory, setDatasetCategory] = useState('vehicle')
  const [datasetOwnerService, setDatasetOwnerService] = useState('ReferenceDataCore')
  const [datasetStatus, setDatasetStatus] = useState('draft')

  const [sourceKey, setSourceKey] = useState('')
  const [sourceName, setSourceName] = useState('')
  const [sourceType, setSourceType] = useState('connector')
  const [connectorType, setConnectorType] = useState('manual')
  const [authorityRank, setAuthorityRank] = useState('50')
  const [refreshCadence, setRefreshCadence] = useState('weekly')
  const [termsNotes, setTermsNotes] = useState('')
  const [sourceEnabled, setSourceEnabled] = useState(true)

  const [importDatasetId, setImportDatasetId] = useState('')
  const [importSourceId, setImportSourceId] = useState('')
  const [importFileName, setImportFileName] = useState('')
  const [importObjectKey, setImportObjectKey] = useState('')
  const [masterCsvFile, setMasterCsvFile] = useState<File | null>(null)
  const [masterCsvObjectKey, setMasterCsvObjectKey] = useState('')
  const [selectedImportId, setSelectedImportId] = useState('')
  const [rowTargetDatasetIds, setRowTargetDatasetIds] = useState<Record<string, string>>({})
  const [publishDatasetId, setPublishDatasetId] = useState('')
  const [publishSummary, setPublishSummary] = useState('')

  const dashboardQuery = useQuery({
    queryKey: ['platform-admin-reference-data-dashboard'],
    queryFn: () => nexarr.getReferenceDataDashboard(),
  })

  const datasetsQuery = useQuery({
    queryKey: ['platform-admin-reference-data-datasets'],
    queryFn: () => nexarr.listReferenceDatasets(),
  })

  const sourcesQuery = useQuery({
    queryKey: ['platform-admin-reference-data-sources'],
    queryFn: () => nexarr.listReferenceSources(),
  })

  const importsQuery = useQuery({
    queryKey: ['platform-admin-reference-data-imports'],
    queryFn: () => nexarr.listReferenceImports(),
  })

  const crosswalksQuery = useQuery({
    queryKey: ['platform-admin-reference-data-crosswalks'],
    queryFn: () => nexarr.listReferenceCrosswalks(),
  })

  const publishHistoryQuery = useQuery({
    queryKey: ['platform-admin-reference-data-publish-history'],
    queryFn: () => nexarr.listReferencePublishHistory(),
  })

  const stagingRecordsQuery = useQuery({
    queryKey: ['platform-admin-reference-data-staging-records', selectedImportId],
    queryFn: () => nexarr.listReferenceStagingRecords(selectedImportId),
    enabled: Boolean(selectedImportId),
  })

  const datasetOptions = useMemo(
    () =>
      (datasetsQuery.data ?? [])
        .filter((dataset) => dataset.key !== MASTER_CSV_DATASET_KEY)
        .map((dataset) => ({
        value: dataset.id,
        label: `${dataset.ownerService} - ${dataset.name}`,
      })),
    [datasetsQuery.data],
  )

  const sourceOptions = useMemo(
    () =>
      (sourcesQuery.data ?? []).map((source) => ({
        value: source.id,
        label: `${source.name} (${source.key})`,
      })),
    [sourcesQuery.data],
  )

  useEffect(() => {
    if (!importDatasetId && datasetOptions.length > 0) {
      setImportDatasetId(datasetOptions[0].value)
    }
  }, [datasetOptions, importDatasetId])

  useEffect(() => {
    if (!importSourceId && sourceOptions.length > 0) {
      setImportSourceId(sourceOptions[0].value)
    }
  }, [sourceOptions, importSourceId])

  useEffect(() => {
    if (!publishDatasetId && datasetOptions.length > 0) {
      setPublishDatasetId(datasetOptions[0].value)
    }
  }, [datasetOptions, publishDatasetId])

  useEffect(() => {
    if (!selectedImportId && importsQuery.data?.length) {
      setSelectedImportId(importsQuery.data[0].id)
    }
  }, [importsQuery.data, selectedImportId])

  const refreshAll = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['platform-admin-reference-data-dashboard'] }),
      queryClient.invalidateQueries({ queryKey: ['platform-admin-reference-data-datasets'] }),
      queryClient.invalidateQueries({ queryKey: ['platform-admin-reference-data-sources'] }),
      queryClient.invalidateQueries({ queryKey: ['platform-admin-reference-data-imports'] }),
      queryClient.invalidateQueries({ queryKey: ['platform-admin-reference-data-crosswalks'] }),
      queryClient.invalidateQueries({ queryKey: ['platform-admin-reference-data-publish-history'] }),
      queryClient.invalidateQueries({ queryKey: ['platform-admin-reference-data-staging-records'] }),
    ])
  }

  const createDatasetMutation = useMutation({
    mutationFn: () =>
      nexarr.createReferenceDataset({
        key: datasetKey.trim(),
        name: datasetName.trim(),
        category: datasetCategory.trim(),
        ownerService: datasetOwnerService.trim(),
        status: datasetStatus.trim(),
      }),
    onSuccess: async () => {
      setDatasetKey('')
      setDatasetName('')
      setDatasetCategory('vehicle')
      setDatasetOwnerService('ReferenceDataCore')
      setDatasetStatus('draft')
      pushToast({ message: 'Reference dataset created.', variant: 'success' })
      await refreshAll()
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const createSourceMutation = useMutation({
    mutationFn: () =>
      nexarr.createReferenceSource({
        key: sourceKey.trim(),
        name: sourceName.trim(),
        sourceType: sourceType.trim(),
        connectorType: connectorType.trim(),
        authorityRank: Number(authorityRank) || 0,
        refreshCadence: refreshCadence.trim(),
        termsNotes: termsNotes.trim() || null,
        enabled: sourceEnabled,
      }),
    onSuccess: async () => {
      setSourceKey('')
      setSourceName('')
      setSourceType('connector')
      setConnectorType('manual')
      setAuthorityRank('50')
      setRefreshCadence('weekly')
      setTermsNotes('')
      setSourceEnabled(true)
      pushToast({ message: 'Reference source created.', variant: 'success' })
      await refreshAll()
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const createImportMutation = useMutation({
    mutationFn: () =>
      nexarr.createReferenceImport({
        datasetId: importDatasetId,
        sourceId: importSourceId,
        tenantId: null,
        requestedByPersonId: null,
        rawObjectKey: importObjectKey.trim() || null,
        fileName: importFileName.trim() || null,
        records: null,
      }),
    onSuccess: async (created) => {
      setImportFileName('')
      setImportObjectKey('')
      setSelectedImportId(created.id)
      pushToast({ message: 'Reference import queued.', variant: 'success' })
      await refreshAll()
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const createMasterCsvImportMutation = useMutation({
    mutationFn: async () => {
      if (!masterCsvFile) {
        throw new Error('Choose a CSV file first.')
      }

      const csvText = await masterCsvFile.text()
      return nexarr.createReferenceMasterCsvImport({
        csvText,
        fileName: masterCsvFile.name,
        rawObjectKey: masterCsvObjectKey.trim() || null,
      })
    },
    onSuccess: async (created) => {
      setMasterCsvFile(null)
      setMasterCsvObjectKey('')
      setSelectedImportId(created.id)
      pushToast({ message: 'Master CSV uploaded for review.', variant: 'success' })
      await refreshAll()
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const publishMutation = useMutation({
    mutationFn: ({ datasetId, summary }: { datasetId: string; summary: string }) =>
      nexarr.publishReferenceDataset(datasetId, summary),
    onSuccess: async () => {
      setPublishSummary('')
      pushToast({ message: 'Dataset published.', variant: 'success' })
      await refreshAll()
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const reviewMutation = useMutation({
    mutationFn: ({
      stagingId,
      action,
      targetDatasetId,
    }: {
      stagingId: string
      action: 'approve' | 'reject' | 'merge' | 'escalate'
      targetDatasetId: string | null
    }) =>
      (action === 'approve'
        ? nexarr.approveReferenceStagingRecord
        : action === 'reject'
          ? nexarr.rejectReferenceStagingRecord
          : action === 'merge'
            ? nexarr.mergeReferenceStagingRecord
            : nexarr.escalateReferenceStagingRecord)(stagingId, {
        reason: 'Reviewed in platform admin',
        displayName: null,
        canonicalKey: null,
        normalizedFieldsJson: null,
        sourceEvidenceJson: null,
        effectiveDate: null,
        targetDatasetId,
      }),
    onSuccess: async () => {
      pushToast({ message: 'Reference review updated.', variant: 'success' })
      await refreshAll()
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const datasets = datasetsQuery.data ?? []
  const sources = sourcesQuery.data ?? []
  const imports = importsQuery.data ?? []
  const crosswalks = crosswalksQuery.data ?? []
  const publishHistory = publishHistoryQuery.data ?? []
  const stagingRecords = stagingRecordsQuery.data ?? []
  const selectedImport = imports.find((item) => item.id === selectedImportId) ?? null
  const selectedImportIsMasterCsv = selectedImport?.datasetKey === MASTER_CSV_DATASET_KEY

  const resolveRowTargetDatasetId = (record: (typeof stagingRecords)[number]) =>
    rowTargetDatasetIds[record.id] ?? record.targetDatasetId ?? (selectedImportIsMasterCsv ? '' : record.datasetId)

  const isLoading =
    dashboardQuery.isLoading ||
    datasetsQuery.isLoading ||
    sourcesQuery.isLoading ||
    importsQuery.isLoading ||
    crosswalksQuery.isLoading ||
    publishHistoryQuery.isLoading

  if (isLoading) {
    return <p className="text-sm text-slate-500">Loading reference data…</p>
  }

  const mainError =
    dashboardQuery.error ??
    datasetsQuery.error ??
    sourcesQuery.error ??
    importsQuery.error ??
    crosswalksQuery.error ??
    publishHistoryQuery.error ??
    stagingRecordsQuery.error ??
    null

  if (mainError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(mainError, 'Failed to load reference data.')}
        onRetry={() => void refreshAll()}
        retryLabel="Retry reference data"
      />
    )
  }

  return (
    <div className="space-y-6">
      <div className="space-y-3">
        <div>
          <h2 className="text-2xl font-semibold text-white">Reference data</h2>
          <p className="mt-1 max-w-3xl text-sm text-slate-400">
            Platform-controlled reference datasets, source rankings, import queues, crosswalks,
            and publish history live in NexArr.
          </p>
        </div>
        <p className="text-xs text-slate-500">
          Snapshot generated {new Date(dashboardQuery.data!.generatedAt).toLocaleString()}
        </p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard label="Datasets" value={dashboardQuery.data!.datasetCount} />
        <StatCard label="Sources" value={dashboardQuery.data!.sourceCount} />
        <StatCard label="Imports" value={dashboardQuery.data!.jobCount} />
        <StatCard label="Pending review" value={dashboardQuery.data!.pendingReviewCount} />
        <StatCard label="Failed imports" value={dashboardQuery.data!.failedImportCount} />
        <StatCard label="Published entities" value={dashboardQuery.data!.publishedEntityCount} />
        <StatCard label="Crosswalks" value={dashboardQuery.data!.crosswalkCount} />
        <StatCard label="Publish events" value={dashboardQuery.data!.publishEventCount} />
      </div>

      <Section
        title="Create and publish"
        description="Seed datasets, register sources, queue placeholder imports, and publish dataset versions."
      >
        <div className="grid gap-4 xl:grid-cols-3">
          <div className="rounded-xl border border-slate-200 bg-slate-50 p-4">
            <h4 className="text-sm font-semibold text-stl-navy">New dataset</h4>
            <div className="mt-3 space-y-3">
              <Field label="Key">
                <input
                  value={datasetKey}
                  onChange={(event) => setDatasetKey(event.target.value)}
                  className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                  placeholder="vehicle-taxonomy"
                />
              </Field>
              <Field label="Name">
                <input
                  value={datasetName}
                  onChange={(event) => setDatasetName(event.target.value)}
                  className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                  placeholder="Vehicle Taxonomy"
                />
              </Field>
              <div className="grid gap-3 md:grid-cols-3">
                <Field label="Category">
                  <input
                    value={datasetCategory}
                    onChange={(event) => setDatasetCategory(event.target.value)}
                    className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                    placeholder="vehicle"
                  />
                </Field>
                <Field label="Owner">
                  <input
                    value={datasetOwnerService}
                    onChange={(event) => setDatasetOwnerService(event.target.value)}
                    className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                    placeholder="ReferenceDataCore"
                  />
                </Field>
                <Field label="Status">
                  <input
                    value={datasetStatus}
                    onChange={(event) => setDatasetStatus(event.target.value)}
                    className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                    placeholder="draft"
                  />
                </Field>
              </div>
              <button
                type="button"
                disabled={createDatasetMutation.isPending || !datasetKey.trim() || !datasetName.trim()}
                onClick={() => createDatasetMutation.mutate()}
                className="rounded-md bg-stl-navy px-4 py-2 text-sm font-medium text-white hover:bg-stl-navy/90 disabled:opacity-50"
              >
                {createDatasetMutation.isPending ? 'Saving…' : 'Create dataset'}
              </button>
            </div>
          </div>

          <div className="rounded-xl border border-slate-200 bg-slate-50 p-4">
            <h4 className="text-sm font-semibold text-stl-navy">New source</h4>
            <div className="mt-3 space-y-3">
              <Field label="Key">
                <input
                  value={sourceKey}
                  onChange={(event) => setSourceKey(event.target.value)}
                  className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                  placeholder="nhtsa-vpic"
                />
              </Field>
              <Field label="Name">
                <input
                  value={sourceName}
                  onChange={(event) => setSourceName(event.target.value)}
                  className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                  placeholder="NHTSA vPIC"
                />
              </Field>
              <div className="grid gap-3 md:grid-cols-2">
                <Field label="Source type">
                  <input
                    value={sourceType}
                    onChange={(event) => setSourceType(event.target.value)}
                    className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                    placeholder="connector"
                  />
                </Field>
                <Field label="Connector">
                  <input
                    value={connectorType}
                    onChange={(event) => setConnectorType(event.target.value)}
                    className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                    placeholder="nhtsa"
                  />
                </Field>
              </div>
              <div className="grid gap-3 md:grid-cols-2">
                <Field label="Authority rank">
                  <input
                    value={authorityRank}
                    onChange={(event) => setAuthorityRank(event.target.value)}
                    type="number"
                    className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                  />
                </Field>
                <Field label="Refresh cadence">
                  <input
                    value={refreshCadence}
                    onChange={(event) => setRefreshCadence(event.target.value)}
                    className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                    placeholder="weekly"
                  />
                </Field>
              </div>
              <Field label="Terms notes">
                <input
                  value={termsNotes}
                  onChange={(event) => setTermsNotes(event.target.value)}
                  className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                  placeholder="Optional usage notes"
                />
              </Field>
              <label className="flex items-center gap-2 text-sm text-slate-700">
                <input
                  type="checkbox"
                  checked={sourceEnabled}
                  onChange={(event) => setSourceEnabled(event.target.checked)}
                />
                Enabled
              </label>
              <button
                type="button"
                disabled={createSourceMutation.isPending || !sourceKey.trim() || !sourceName.trim()}
                onClick={() => createSourceMutation.mutate()}
                className="rounded-md bg-stl-navy px-4 py-2 text-sm font-medium text-white hover:bg-stl-navy/90 disabled:opacity-50"
              >
                {createSourceMutation.isPending ? 'Saving…' : 'Create source'}
              </button>
            </div>
          </div>

          <div className="rounded-xl border border-slate-200 bg-slate-50 p-4">
            <h4 className="text-sm font-semibold text-stl-navy">Queue import or publish</h4>
            <div className="mt-3 space-y-3">
              <Field label="Dataset">
                <select
                  value={importDatasetId}
                  onChange={(event) => setImportDatasetId(event.target.value)}
                  className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                >
                  {datasetOptions.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </Field>
              <Field label="Source">
                <select
                  value={importSourceId}
                  onChange={(event) => setImportSourceId(event.target.value)}
                  className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                >
                  {sourceOptions.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </Field>
              <Field label="File name">
                <input
                  value={importFileName}
                  onChange={(event) => setImportFileName(event.target.value)}
                  className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                  placeholder="vehicle-taxonomy.csv"
                />
              </Field>
              <Field label="Object key">
                <input
                  value={importObjectKey}
                  onChange={(event) => setImportObjectKey(event.target.value)}
                  className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                  placeholder="seed/reference/vehicle-taxonomy.csv"
                />
              </Field>
              <button
                type="button"
                disabled={
                  createImportMutation.isPending || !importDatasetId || !importSourceId
                }
                onClick={() => createImportMutation.mutate()}
                className="w-full rounded-md bg-stl-navy px-4 py-2 text-sm font-medium text-white hover:bg-stl-navy/90 disabled:opacity-50"
              >
                {createImportMutation.isPending ? 'Queueing…' : 'Queue import'}
              </button>

              <div className="border-t border-slate-200 pt-3">
                <Field label="Publish dataset summary">
                  <input
                    value={publishSummary}
                    onChange={(event) => setPublishSummary(event.target.value)}
                    className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                    placeholder="Initial publish for the dataset"
                  />
                </Field>
                <div className="mt-3 flex gap-2">
                  <select
                    value={publishDatasetId}
                    onChange={(event) => setPublishDatasetId(event.target.value)}
                    className="flex-1 rounded-md border border-slate-300 px-3 py-2 text-sm"
                  >
                    {datasetOptions.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                  <button
                    type="button"
                    disabled={publishMutation.isPending || !publishDatasetId}
                    onClick={() =>
                      publishMutation.mutate({
                        datasetId: publishDatasetId,
                        summary: publishSummary || 'Published from platform admin',
                      })
                    }
                    className="rounded-md border border-stl-navy px-4 py-2 text-sm font-medium text-stl-navy hover:bg-slate-100 disabled:opacity-50"
                  >
                    {publishMutation.isPending ? 'Publishing…' : 'Publish'}
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </Section>

      <Section
        title="Master CSV import"
        description="Upload a single CSV that can route rows across all product datasets. Each row is staged for live review and must be assigned before approval if the dataset cannot be inferred."
      >
        <div className="grid gap-4 xl:grid-cols-[1.2fr_0.8fr]">
          <div className="rounded-xl border border-slate-200 bg-slate-50 p-4">
            <div className="space-y-3">
              <Field label="CSV file">
                <input
                  type="file"
                  accept=".csv,text/csv"
                  onChange={(event) => setMasterCsvFile(event.target.files?.[0] ?? null)}
                  className="block w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm"
                />
              </Field>
              <Field label="Object key">
                <input
                  value={masterCsvObjectKey}
                  onChange={(event) => setMasterCsvObjectKey(event.target.value)}
                  className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
                  placeholder="seed/reference/master-import.csv"
                />
              </Field>
              <p className="text-xs text-slate-500">
                Selected file: {masterCsvFile?.name ?? 'No file selected'}
              </p>
              <button
                type="button"
                disabled={createMasterCsvImportMutation.isPending || !masterCsvFile}
                onClick={() => createMasterCsvImportMutation.mutate()}
                className="rounded-md bg-stl-navy px-4 py-2 text-sm font-medium text-white hover:bg-stl-navy/90 disabled:opacity-50"
              >
                {createMasterCsvImportMutation.isPending ? 'Uploading…' : 'Upload master CSV'}
              </button>
            </div>
          </div>

          <div className="rounded-xl border border-slate-200 bg-slate-50 p-4 text-sm text-slate-600">
            <h4 className="text-sm font-semibold text-stl-navy">Expected columns</h4>
            <ul className="mt-3 space-y-2">
              <li><span className="font-medium text-slate-700">Routing:</span> product, dataset, dataset_key, or product + dataset</li>
              <li><span className="font-medium text-slate-700">Identity:</span> entity_type, canonical_key, display_name</li>
              <li><span className="font-medium text-slate-700">Optional:</span> source_system, source_key, confidence, fields_json</li>
              <li><span className="font-medium text-slate-700">Review:</span> imported rows are staged first, then approved row by row</li>
            </ul>
          </div>
        </div>
      </Section>

      <div className="grid gap-6 xl:grid-cols-2">
        <Section
          title="Datasets"
          description="Canonical dataset definitions, owners, and published version state."
        >
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="border-b border-slate-200 bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-3 py-2">Dataset</th>
                  <th className="px-3 py-2">Status</th>
                  <th className="px-3 py-2">Entities</th>
                  <th className="px-3 py-2">Review</th>
                  <th className="px-3 py-2">Published</th>
                </tr>
              </thead>
              <tbody>
                {datasets.map((dataset) => (
                  <tr key={dataset.id} className="border-b border-slate-100">
                    <td className="px-3 py-2">
                      <p className="font-medium text-stl-navy">{dataset.name}</p>
                      <p className="text-xs text-slate-500">
                        {dataset.key} · {dataset.category} · {dataset.ownerService}
                      </p>
                    </td>
                    <td className="px-3 py-2">
                      <StatusBadge value={dataset.status} />
                    </td>
                    <td className="px-3 py-2">
                      {dataset.entityCount} entities
                      <span className="block text-xs text-slate-500">
                        {dataset.sourceCount} sources
                      </span>
                    </td>
                    <td className="px-3 py-2">
                      {dataset.pendingReviewCount} pending
                      <span className="block text-xs text-slate-500">
                        {dataset.failedImportCount} failed imports
                      </span>
                    </td>
                    <td className="px-3 py-2">
                      <div className="flex flex-wrap items-center gap-2">
                        <span>{dataset.currentPublishedVersion ?? 'Not published'}</span>
                        <button
                          type="button"
                          disabled={publishMutation.isPending}
                          onClick={() =>
                            publishMutation.mutate({
                              datasetId: dataset.id,
                              summary: `Published ${dataset.key} from platform admin`,
                            })
                          }
                          className="rounded-md border border-slate-300 px-3 py-1.5 text-xs font-medium text-slate-700 hover:bg-slate-100 disabled:opacity-50"
                        >
                          Publish
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Section>

        <Section
          title="Sources"
          description="Source ranking and connector metadata used when adjudicating imports."
        >
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="border-b border-slate-200 bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-3 py-2">Source</th>
                  <th className="px-3 py-2">Type</th>
                  <th className="px-3 py-2">Rank</th>
                  <th className="px-3 py-2">Cadence</th>
                  <th className="px-3 py-2">State</th>
                </tr>
              </thead>
              <tbody>
                {sources.map((source) => (
                  <tr key={source.id} className="border-b border-slate-100">
                    <td className="px-3 py-2">
                      <p className="font-medium text-stl-navy">{source.name}</p>
                      <p className="text-xs text-slate-500">
                        {source.key} · {source.connectorType}
                      </p>
                    </td>
                    <td className="px-3 py-2">{source.sourceType}</td>
                    <td className="px-3 py-2">{source.authorityRank}</td>
                    <td className="px-3 py-2">{source.refreshCadence}</td>
                    <td className="px-3 py-2">
                      <StatusBadge value={source.enabled ? 'enabled' : 'disabled'} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Section>
      </div>

      <Section
        title="Imports and review queue"
        description="Latest import jobs and their staged records. Review actions stay on the platform control plane."
      >
        <div className="grid gap-6 xl:grid-cols-[360px_1fr]">
          <div className="rounded-xl border border-slate-200 bg-slate-50 p-4">
            <div className="flex items-center justify-between gap-3">
              <h4 className="text-sm font-semibold text-stl-navy">Imports</h4>
              <span className="text-xs text-slate-500">{imports.length} jobs</span>
            </div>
            <div className="mt-3 space-y-2">
              {imports.map((entry) => (
                <button
                  key={entry.id}
                  type="button"
                  onClick={() => setSelectedImportId(entry.id)}
                  className={`w-full rounded-lg border px-3 py-2 text-left text-sm transition ${
                    selectedImportId === entry.id
                      ? 'border-stl-teal bg-white shadow-sm'
                      : 'border-slate-200 bg-white hover:bg-slate-100'
                  }`}
                >
                  <div className="flex items-center justify-between gap-2">
                    <span className="font-medium text-stl-navy">{entry.datasetName}</span>
                    <StatusBadge value={entry.status} />
                  </div>
                  <p className="mt-1 text-xs text-slate-500">
                    {entry.datasetKey} · {entry.sourceKey}
                  </p>
                  <p className="mt-1 text-xs text-slate-500">
                    {entry.stagingRecordCount} records · {entry.pendingReviewCount} pending
                  </p>
                </button>
              ))}
            </div>
          </div>

          <div>
            {selectedImport ? (
              <div className="rounded-xl border border-slate-200 bg-slate-50 p-4">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <h4 className="text-sm font-semibold text-stl-navy">
                      {selectedImport.datasetName}
                    </h4>
                    <p className="text-xs text-slate-500">
                      {selectedImport.datasetKey} · {selectedImport.sourceKey} ·{' '}
                      {selectedImport.fileName ?? selectedImport.rawObjectKey ?? 'No file attached'}
                    </p>
                  </div>
                  <div className="text-xs text-slate-500">
                    Started {new Date(selectedImport.startedAt).toLocaleString()}
                  </div>
                </div>

                {stagingRecordsQuery.isLoading ? (
                  <p className="mt-4 text-sm text-slate-500">Loading staged records…</p>
                ) : stagingRecords.length === 0 ? (
                  <p className="mt-4 text-sm text-slate-500">No staged records found.</p>
                ) : (
                  <div className="mt-4 overflow-x-auto">
                    <table className="min-w-full text-left text-sm">
                      <thead className="border-b border-slate-200 bg-white text-xs uppercase text-slate-500">
                        <tr>
                          <th className="px-3 py-2">Row</th>
                          <th className="px-3 py-2">Target dataset</th>
                          <th className="px-3 py-2">Entity</th>
                          <th className="px-3 py-2">Canonical key</th>
                          <th className="px-3 py-2">Confidence</th>
                          <th className="px-3 py-2">Status</th>
                          <th className="px-3 py-2">Review</th>
                        </tr>
                      </thead>
                      <tbody>
                        {stagingRecords.map((record) => (
                          <tr key={record.id} className="border-b border-slate-100 bg-white">
                            <td className="px-3 py-2">{record.rowNumber ?? '—'}</td>
                            <td className="px-3 py-2">
                              <select
                                value={resolveRowTargetDatasetId(record)}
                                onChange={(event) =>
                                  setRowTargetDatasetIds((current) => ({
                                    ...current,
                                    [record.id]: event.target.value,
                                  }))
                                }
                                className="w-full rounded-md border border-slate-300 px-2 py-1 text-xs"
                              >
                                <option value="">Select dataset</option>
                                {datasetOptions.map((option) => (
                                  <option key={option.value} value={option.value}>
                                    {option.label}
                                  </option>
                                ))}
                              </select>
                              <p className="mt-1 text-xs text-slate-500">
                                {record.targetDatasetName
                                  ? `${record.targetOwnerService ?? 'NexArr'} - ${record.targetDatasetName}`
                                  : selectedImportIsMasterCsv
                                    ? 'Assign before approving'
                                    : `${record.datasetKey} target`}
                              </p>
                            </td>
                            <td className="px-3 py-2">
                              <p className="font-medium text-stl-navy">{record.proposedEntityType}</p>
                              <p className="text-xs text-slate-500">
                                {record.datasetKey} · {record.sourceKey}
                              </p>
                            </td>
                            <td className="px-3 py-2 font-mono text-xs text-slate-600">
                              {record.proposedCanonicalKey ?? '—'}
                            </td>
                            <td className="px-3 py-2">{Math.round(record.confidence * 100)}%</td>
                            <td className="px-3 py-2">
                              <StatusBadge value={record.status} />
                              {record.reviewReason ? (
                                <p className="mt-1 text-xs text-slate-500">{record.reviewReason}</p>
                              ) : null}
                            </td>
                            <td className="px-3 py-2">
                              <div className="flex flex-wrap gap-2">
                                <button
                                  type="button"
                                  disabled={reviewMutation.isPending || (selectedImportIsMasterCsv && !resolveRowTargetDatasetId(record))}
                                  onClick={() =>
                                    reviewMutation.mutate({
                                      stagingId: record.id,
                                      action: 'approve',
                                      targetDatasetId: resolveRowTargetDatasetId(record) || null,
                                    })
                                  }
                                  className="rounded-md border border-emerald-300 px-3 py-1.5 text-xs font-medium text-emerald-700 hover:bg-emerald-50 disabled:opacity-50"
                                >
                                  Approve
                                </button>
                                <button
                                  type="button"
                                  disabled={reviewMutation.isPending}
                                  onClick={() =>
                                    reviewMutation.mutate({
                                      stagingId: record.id,
                                      action: 'reject',
                                      targetDatasetId: resolveRowTargetDatasetId(record) || null,
                                    })
                                  }
                                  className="rounded-md border border-rose-300 px-3 py-1.5 text-xs font-medium text-rose-700 hover:bg-rose-50 disabled:opacity-50"
                                >
                                  Reject
                                </button>
                                <button
                                  type="button"
                                  disabled={reviewMutation.isPending}
                                  onClick={() =>
                                    reviewMutation.mutate({
                                      stagingId: record.id,
                                      action: 'escalate',
                                      targetDatasetId: resolveRowTargetDatasetId(record) || null,
                                    })
                                  }
                                  className="rounded-md border border-amber-300 px-3 py-1.5 text-xs font-medium text-amber-700 hover:bg-amber-50 disabled:opacity-50"
                                >
                                  Escalate
                                </button>
                              </div>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
            ) : (
              <div className="rounded-xl border border-dashed border-slate-300 bg-slate-50 p-6 text-sm text-slate-500">
                Select an import to review its staged records.
              </div>
            )}
          </div>
        </div>
      </Section>

      <div className="grid gap-6 xl:grid-cols-2">
        <Section
          title="Crosswalks"
          description="External identifiers and the canonical reference entities they resolve to."
        >
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="border-b border-slate-200 bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-3 py-2">External system</th>
                  <th className="px-3 py-2">External key</th>
                  <th className="px-3 py-2">Canonical entity</th>
                  <th className="px-3 py-2">Source</th>
                  <th className="px-3 py-2">Confidence</th>
                </tr>
              </thead>
              <tbody>
                {crosswalks.map((crosswalk) => (
                  <tr key={crosswalk.id} className="border-b border-slate-100">
                    <td className="px-3 py-2">
                      <p className="font-medium text-stl-navy">{crosswalk.externalSystem}</p>
                      <p className="text-xs text-slate-500">{crosswalk.status}</p>
                    </td>
                    <td className="px-3 py-2 font-mono text-xs text-slate-600">
                      {crosswalk.externalKey}
                    </td>
                    <td className="px-3 py-2">
                      <p className="font-medium text-stl-navy">{crosswalk.displayName}</p>
                      <p className="text-xs text-slate-500">
                        {crosswalk.entityType} · {crosswalk.canonicalKey}
                      </p>
                    </td>
                    <td className="px-3 py-2">{crosswalk.sourceKey ?? '—'}</td>
                    <td className="px-3 py-2">{Math.round(crosswalk.confidence * 100)}%</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Section>

        <Section
          title="Publish history"
          description="The recent publish events that advanced datasets into a visible version."
        >
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="border-b border-slate-200 bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-3 py-2">Dataset</th>
                  <th className="px-3 py-2">Version</th>
                  <th className="px-3 py-2">Summary</th>
                  <th className="px-3 py-2">Published</th>
                </tr>
              </thead>
              <tbody>
                {publishHistory.map((event) => (
                  <tr key={event.id} className="border-b border-slate-100">
                    <td className="px-3 py-2">
                      <p className="font-medium text-stl-navy">{event.datasetName}</p>
                      <p className="text-xs text-slate-500">{event.datasetKey}</p>
                    </td>
                    <td className="px-3 py-2">{event.publishedVersion}</td>
                    <td className="px-3 py-2 text-slate-700">{event.summary}</td>
                    <td className="px-3 py-2 text-slate-600">
                      {new Date(event.createdAt).toLocaleString()}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Section>
      </div>
    </div>
  )
}
