import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { ApiErrorCallout, ConfirmDialog, getErrorMessage } from '@stl/shared-ui'

import * as nexarr from '../../api/nexarrClient'
import type {
  ReferenceDatasetResponse,
  ReferenceEntityListItemResponse,
  ReferenceEntityResponse,
} from '../../api/types'
import { downloadBlob } from '../../components/platform-admin/audit-export/utils'
import { useToast } from '../../feedback'

const MASTER_CSV_DATASET_KEY = 'master-reference-intake'
const MASTER_CSV_TEMPLATE_FILE_NAME = 'nexarr-reference-data-master-import-template.csv'
const MASTER_CSV_TEMPLATE_HEADERS = [
  'dataset_key',
  'entity_type',
  'canonical_key',
  'display_name',
  'source_system',
  'source_key',
  'confidence',
  'external_id',
  'external_name',
  'notes',
] as const
const MASTER_CSV_TEMPLATE_SAMPLE_ROW = [
  'vehicle-taxonomy',
  'vehicle',
  'fleet-truck-001',
  'Fleet Truck 001',
  'legacy-catalog',
  'truck-001',
  '0.95',
  'VIN-12345',
  'Ford F-450',
  'Replace or remove this sample row before upload.',
] as const

type DatasetFormState = {
  id: string
  key: string
  name: string
  category: string
  ownerService: string
  status: string
}

type EntityEditorState = {
  id: string
  datasetId: string
  displayName: string
  canonicalKey: string
  normalizedFieldsJson: string
  sourceEvidenceJson: string
  effectiveDate: string
}

type PendingDeleteTarget =
  | { kind: 'dataset'; dataset: ReferenceDatasetResponse; message: string }
  | { kind: 'entity'; entity: ReferenceEntityListItemResponse; message: string }

type ReferenceDataView = 'catalog' | 'sources' | 'inputs' | 'review' | 'crosswalks' | 'history'

const REFERENCE_DATA_VIEWS: ReadonlyArray<{
  id: ReferenceDataView
  label: string
  helper: string
}> = [
  { id: 'catalog', label: 'Catalog', helper: 'Datasets and publish controls' },
  { id: 'sources', label: 'Sources', helper: 'Sources and import intake' },
  { id: 'inputs', label: 'Inputs', helper: 'Dataset values and entities' },
  { id: 'review', label: 'Review', helper: 'Staged rows and approvals' },
  { id: 'crosswalks', label: 'Crosswalks', helper: 'External identifier links' },
  { id: 'history', label: 'History', helper: 'Publish activity' },
] as const

function createEmptyDatasetForm(): DatasetFormState {
  return {
    id: '',
    key: '',
    name: '',
    category: 'vehicle',
    ownerService: 'ReferenceDataCore',
    status: 'draft',
  }
}

function buildCsvRow(values: readonly string[]) {
  return values
    .map((value) => {
      const escaped = value.replaceAll('"', '""')
      return /[",\n]/.test(value) ? `"${escaped}"` : escaped
    })
    .join(',')
}

function buildMasterCsvTemplate() {
  return [MASTER_CSV_TEMPLATE_HEADERS, MASTER_CSV_TEMPLATE_SAMPLE_ROW].map(buildCsvRow).join('\n')
}

function formatDatasetLabel(dataset: ReferenceDatasetResponse) {
  return `${dataset.ownerService} - ${dataset.name}`
}

function formatJsonForEditor(value: string | null | undefined) {
  if (!value?.trim()) {
    return '{}'
  }

  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

function getCurrentVersion(entity: ReferenceEntityResponse) {
  return entity.versions[0] ?? null
}

function buildEntityEditorState(entity: ReferenceEntityResponse): EntityEditorState {
  const currentVersion = getCurrentVersion(entity)
  return {
    id: entity.id,
    datasetId: entity.datasetId,
    displayName: entity.displayName,
    canonicalKey: entity.canonicalKey,
    normalizedFieldsJson: formatJsonForEditor(entity.normalizedFieldsJson),
    sourceEvidenceJson: formatJsonForEditor(currentVersion?.sourceEvidenceJson),
    effectiveDate: currentVersion?.effectiveDate ?? '',
  }
}

function StatCard({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4 shadow-sm">
      <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-[var(--color-text-muted)]">
        {label}
      </p>
      <p className="mt-2 text-3xl font-semibold text-[var(--color-text-primary)]">{value}</p>
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
    <label className="block text-sm text-[var(--color-text-secondary)]">
      <span className="font-medium text-[var(--color-text-secondary)]">{label}</span>
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
    <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5 shadow-sm">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h3 className="text-lg font-semibold text-[var(--color-text-primary)]">{title}</h3>
          <p className="mt-1 text-sm text-[var(--color-text-muted)]">{description}</p>
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
      ? 'border-[var(--tone-success-border)] bg-[var(--tone-success-bg)] text-[var(--tone-success-text)]'
      : normalized.includes('review') || normalized.includes('pending') || normalized.includes('draft')
        ? 'border-[var(--tone-warning-border)] bg-[var(--tone-warning-bg)] text-[var(--tone-warning-text)]'
        : normalized.includes('failed') || normalized.includes('reject') || normalized.includes('archiv')
          ? 'border-[var(--tone-danger-border)] bg-[var(--tone-danger-bg)] text-[var(--tone-danger-text)]'
          : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] text-[var(--color-text-muted)]'

  return (
    <span className={`inline-flex rounded-full border px-2.5 py-1 text-xs font-medium ${classes}`}>
      {value}
    </span>
  )
}

function Detail({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-3">
      <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-[var(--color-text-muted)]">{label}</p>
      <p className="mt-2 text-sm font-medium text-[var(--color-text-primary)]">{value}</p>
    </div>
  )
}

function WorkspaceTab({
  helper,
  id,
  isActive,
  label,
  onSelect,
}: {
  helper: string
  id: ReferenceDataView
  isActive: boolean
  label: string
  onSelect: (id: ReferenceDataView) => void
}) {
  return (
    <button
      id={`reference-data-tab-${id}`}
      type="button"
      role="tab"
      aria-selected={isActive}
      aria-controls={`reference-data-panel-${id}`}
      onClick={() => onSelect(id)}
      className={[
        'rounded-xl border px-4 py-3 text-left transition',
        isActive
          ? 'border-[var(--color-accent-border)] bg-[var(--color-accent-soft)] text-[var(--color-text-primary)] shadow-sm'
          : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] text-[var(--color-text-secondary)] hover:border-[var(--color-border-default)] hover:bg-[var(--color-bg-control-hover)]',
      ].join(' ')}
    >
      <span className="block text-sm font-semibold">{label}</span>
      <span className="mt-1 block text-xs text-[var(--color-text-muted)]">{helper}</span>
    </button>
  )
}

export function ReferenceDataPage() {
  const queryClient = useQueryClient()
  const { pushToast } = useToast()

  const [datasetForm, setDatasetForm] = useState<DatasetFormState>(createEmptyDatasetForm)
  const [activeView, setActiveView] = useState<ReferenceDataView>('catalog')
  const [selectedDatasetId, setSelectedDatasetId] = useState('')
  const [selectedDatasetIds, setSelectedDatasetIds] = useState<string[]>([])
  const [entityEditor, setEntityEditor] = useState<EntityEditorState | null>(null)

  const [singleValue, setSingleValue] = useState('')
  const [valuesText, setValuesText] = useState('')

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
  const [pendingDeleteTarget, setPendingDeleteTarget] = useState<PendingDeleteTarget | null>(null)
  const [rowTargetDatasetIds, setRowTargetDatasetIds] = useState<Record<string, string>>({})

  const downloadMasterCsvTemplate = () => {
    downloadBlob(
      new Blob([buildMasterCsvTemplate()], { type: 'text/csv;charset=utf-8' }),
      MASTER_CSV_TEMPLATE_FILE_NAME,
    )
  }

  const sourcesViewActive = activeView === 'sources'
  const inputsViewActive = activeView === 'inputs'
  const reviewViewActive = activeView === 'review'
  const crosswalksViewActive = activeView === 'crosswalks'
  const historyViewActive = activeView === 'history'
  const importsViewActive = inputsViewActive || reviewViewActive

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
    enabled: sourcesViewActive,
  })

  const importsQuery = useQuery({
    queryKey: ['platform-admin-reference-data-imports'],
    queryFn: () => nexarr.listReferenceImports(),
    enabled: importsViewActive,
  })

  const crosswalksQuery = useQuery({
    queryKey: ['platform-admin-reference-data-crosswalks'],
    queryFn: () => nexarr.listReferenceCrosswalks(),
    enabled: crosswalksViewActive,
  })

  const publishHistoryQuery = useQuery({
    queryKey: ['platform-admin-reference-data-publish-history'],
    queryFn: () => nexarr.listReferencePublishHistory(),
    enabled: historyViewActive,
  })

  const visibleDatasets = useMemo(
    () => (datasetsQuery.data ?? []).filter((dataset) => dataset.key !== MASTER_CSV_DATASET_KEY),
    [datasetsQuery.data],
  )

  useEffect(() => {
    if (visibleDatasets.length === 0) {
      setSelectedDatasetId('')
      return
    }

    if (!selectedDatasetId || !visibleDatasets.some((dataset) => dataset.id === selectedDatasetId)) {
      setSelectedDatasetId(visibleDatasets[0].id)
    }
  }, [selectedDatasetId, visibleDatasets])

  useEffect(() => {
    setSelectedDatasetIds((current) =>
      current.filter((id) =>
        visibleDatasets.some(
          (dataset) => dataset.id === id && dataset.status.toLowerCase() !== 'archived',
        ),
      ),
    )
  }, [visibleDatasets])

  useEffect(() => {
    if (entityEditor && entityEditor.datasetId !== selectedDatasetId) {
      setEntityEditor(null)
    }
  }, [entityEditor, selectedDatasetId])

  const selectedDataset = visibleDatasets.find((dataset) => dataset.id === selectedDatasetId) ?? null
  const selectableDatasetIds = useMemo(
    () =>
      visibleDatasets
        .filter((dataset) => dataset.status.toLowerCase() !== 'archived')
        .map((dataset) => dataset.id),
    [visibleDatasets],
  )
  const allSelectableDatasetsChecked =
    selectableDatasetIds.length > 0 && selectableDatasetIds.every((id) => selectedDatasetIds.includes(id))

  const datasetOptions = useMemo(
    () =>
      visibleDatasets.map((dataset) => ({
        value: dataset.id,
        label: formatDatasetLabel(dataset),
        helper: dataset.key,
      })),
    [visibleDatasets],
  )

  const datasetsByProduct = useMemo(() => {
    const grouped = new Map<string, ReferenceDatasetResponse[]>()
    for (const dataset of visibleDatasets) {
      const bucket = grouped.get(dataset.ownerService) ?? []
      bucket.push(dataset)
      grouped.set(dataset.ownerService, bucket)
    }

    return Array.from(grouped.entries())
      .map(([ownerService, datasets]) => ({
        ownerService,
        datasets: datasets.slice().sort((left, right) => left.name.localeCompare(right.name)),
      }))
      .sort((left, right) => left.ownerService.localeCompare(right.ownerService))
  }, [visibleDatasets])

  const resolvedImportDatasetId = importDatasetId || visibleDatasets[0]?.id || ''
  const resolvedImportSourceId = importSourceId || sourcesQuery.data?.[0]?.id || ''
  const resolvedSelectedImportId = selectedImportId || importsQuery.data?.[0]?.id || ''

  const entitiesQuery = useQuery({
    queryKey: ['platform-admin-reference-data-dataset-entities', selectedDatasetId],
    queryFn: () => nexarr.listReferenceDatasetEntities(selectedDatasetId),
    enabled: inputsViewActive && Boolean(selectedDatasetId),
  })

  const stagingRecordsQuery = useQuery({
    queryKey: ['platform-admin-reference-data-staging-records', resolvedSelectedImportId],
    queryFn: () => nexarr.listReferenceStagingRecords(resolvedSelectedImportId),
    enabled: reviewViewActive && Boolean(resolvedSelectedImportId),
  })

  const sourceOptions = useMemo(
    () =>
      (sourcesQuery.data ?? []).map((source) => ({
        value: source.id,
        label: `${source.name} (${source.key})`,
      })),
    [sourcesQuery.data],
  )

  const refreshAll = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['platform-admin-reference-data-dashboard'] }),
      queryClient.invalidateQueries({ queryKey: ['platform-admin-reference-data-datasets'] }),
      queryClient.invalidateQueries({ queryKey: ['platform-admin-reference-data-sources'] }),
      queryClient.invalidateQueries({ queryKey: ['platform-admin-reference-data-imports'] }),
      queryClient.invalidateQueries({ queryKey: ['platform-admin-reference-data-crosswalks'] }),
      queryClient.invalidateQueries({ queryKey: ['platform-admin-reference-data-publish-history'] }),
      queryClient.invalidateQueries({ queryKey: ['platform-admin-reference-data-staging-records'] }),
      queryClient.invalidateQueries({ queryKey: ['platform-admin-reference-data-dataset-entities'] }),
    ])
  }

  const resetDatasetForm = () => {
    setDatasetForm(createEmptyDatasetForm())
  }

  const saveDatasetMutation = useMutation({
    mutationFn: () => {
      const payload = {
        key: datasetForm.key.trim(),
        name: datasetForm.name.trim(),
        category: datasetForm.category.trim(),
        ownerService: datasetForm.ownerService.trim(),
        status: datasetForm.status.trim(),
      }

      return datasetForm.id
        ? nexarr.updateReferenceDataset(datasetForm.id, payload)
        : nexarr.createReferenceDataset(payload)
    },
    onSuccess: async (saved) => {
      pushToast({
        message: datasetForm.id ? 'Reference dataset updated.' : 'Reference dataset created.',
        variant: 'success',
      })
      resetDatasetForm()
      setSelectedDatasetId(saved.id)
      await refreshAll()
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const deleteDatasetMutation = useMutation({
    mutationFn: (dataset: ReferenceDatasetResponse) => nexarr.deleteReferenceDataset(dataset.id),
    onSuccess: async (_data, dataset) => {
      pushToast({ message: 'Reference dataset deleted.', variant: 'success' })
      setSelectedDatasetIds((current) => current.filter((id) => id !== dataset.id))
      if (selectedDatasetId === dataset.id) {
        setSelectedDatasetId('')
      }
      if (entityEditor?.datasetId === dataset.id) {
        setEntityEditor(null)
      }
      if (datasetForm.id === dataset.id) {
        resetDatasetForm()
      }
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
        datasetId: resolvedImportDatasetId,
        sourceId: resolvedImportSourceId,
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
      setActiveView('review')
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
      setActiveView('review')
      pushToast({ message: 'Master CSV uploaded for review.', variant: 'success' })
      await refreshAll()
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const publishDatasetMutation = useMutation({
    mutationFn: (dataset: ReferenceDatasetResponse) =>
      nexarr.publishReferenceDataset(dataset.id, `Published ${dataset.key} from platform admin`),
    onSuccess: async () => {
      pushToast({ message: 'Dataset published.', variant: 'success' })
      await refreshAll()
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const publishSelectedMutation = useMutation({
    mutationFn: (datasetIds: string[]) =>
      nexarr.publishReferenceDatasets({
        datasetIds,
        summary: 'Published from platform admin batch',
      }),
    onSuccess: async (result) => {
      pushToast({
        message: `${result.publishedCount} dataset${result.publishedCount === 1 ? '' : 's'} published.`,
        variant: 'success',
      })
      setSelectedDatasetIds([])
      await refreshAll()
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const publishAllMutation = useMutation({
    mutationFn: () => nexarr.publishAllReferenceDatasets('Published all reference datasets from platform admin'),
    onSuccess: async (result) => {
      pushToast({
        message: `${result.publishedCount} dataset${result.publishedCount === 1 ? '' : 's'} published.`,
        variant: 'success',
      })
      setSelectedDatasetIds([])
      await refreshAll()
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const addValueMutation = useMutation({
    mutationFn: () =>
      nexarr.createReferenceDatasetInput(selectedDatasetId, {
        value: singleValue.trim(),
      }),
    onSuccess: async (created) => {
      setSingleValue('')
      setSelectedImportId(created.id)
      pushToast({ message: 'Dataset value added.', variant: 'success' })
      await refreshAll()
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const importValuesMutation = useMutation({
    mutationFn: () =>
      nexarr.createReferenceDatasetInput(selectedDatasetId, {
        valuesText: valuesText.trim(),
      }),
    onSuccess: async (created) => {
      setValuesText('')
      setSelectedImportId(created.id)
      pushToast({ message: 'Dataset values imported.', variant: 'success' })
      await refreshAll()
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const saveEntityMutation = useMutation({
    mutationFn: () => {
      if (!entityEditor) {
        throw new Error('Select an entity to edit.')
      }

      return nexarr.updateReferenceEntity(entityEditor.id, {
        displayName: entityEditor.displayName.trim(),
        canonicalKey: entityEditor.canonicalKey.trim(),
        normalizedFieldsJson: entityEditor.normalizedFieldsJson.trim() || null,
        sourceEvidenceJson: entityEditor.sourceEvidenceJson.trim() || null,
        effectiveDate: entityEditor.effectiveDate.trim() || null,
      })
    },
    onSuccess: async (saved) => {
      setEntityEditor(buildEntityEditorState(saved))
      pushToast({ message: 'Reference entity updated.', variant: 'success' })
      await refreshAll()
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const loadEntityMutation = useMutation({
    mutationFn: (entityId: string) => nexarr.getReferenceEntity(entityId),
    onMutate: () => {
      setEntityEditor(null)
    },
    onSuccess: (entity) => {
      setEntityEditor(buildEntityEditorState(entity))
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const deleteEntityMutation = useMutation({
    mutationFn: (entity: ReferenceEntityListItemResponse) => nexarr.deleteReferenceEntity(entity.id),
    onSuccess: async (_data, entity) => {
      pushToast({ message: 'Reference entity deleted.', variant: 'success' })
      if (entityEditor?.id === entity.id) {
        setEntityEditor(null)
      }
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

  const imports = importsQuery.data ?? []
  const crosswalks = crosswalksQuery.data ?? []
  const publishHistory = publishHistoryQuery.data ?? []
  const stagingRecords = stagingRecordsQuery.data ?? []
  const datasetEntities = entitiesQuery.data ?? []
  const selectedImport = imports.find((item) => item.id === resolvedSelectedImportId) ?? null
  const selectedImportIsMasterCsv = selectedImport?.datasetKey === MASTER_CSV_DATASET_KEY
  const latestDatasetInput =
    imports.find((entry) => entry.datasetId === selectedDatasetId && entry.sourceKey === 'platform-admin-input') ??
    imports.find((entry) => entry.datasetId === selectedDatasetId) ??
    null

  const resolveRowTargetDatasetId = (record: (typeof stagingRecords)[number]) =>
    rowTargetDatasetIds[record.id] ??
    record.targetDatasetId ??
    (selectedImportIsMasterCsv ? '' : record.datasetId)

  const isLoading =
    dashboardQuery.isLoading ||
    datasetsQuery.isLoading

  if (isLoading) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading reference data...</p>
  }

  const mainError =
    dashboardQuery.error ??
    datasetsQuery.error ??
    null

  const sourcesError = sourcesQuery.error ?? null
  const inputsError = importsQuery.error ?? entitiesQuery.error ?? null
  const reviewError = importsQuery.error ?? stagingRecordsQuery.error ?? null
  const crosswalksError = crosswalksQuery.error ?? null
  const historyError = publishHistoryQuery.error ?? null

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
      <ConfirmDialog
        open={pendingDeleteTarget !== null}
        title="Confirm delete"
        description={pendingDeleteTarget?.message ?? 'Confirm the delete action.'}
        confirmLabel="Delete"
        cancelLabel="Cancel"
        danger
        onConfirm={() => {
          if (!pendingDeleteTarget) return
          const pending = pendingDeleteTarget
          setPendingDeleteTarget(null)
          if (pending.kind === 'dataset') {
            deleteDatasetMutation.mutate(pending.dataset)
          } else {
            deleteEntityMutation.mutate(pending.entity)
          }
        }}
        onCancel={() => setPendingDeleteTarget(null)}
      />
      <div className="space-y-3">
        <div>
          <h2 className="text-2xl font-semibold text-[var(--color-text-primary)]">Reference data</h2>
          <p className="mt-1 max-w-3xl text-sm text-[var(--color-text-muted)]">
            ReferenceDataCore datasets, platform-managed dataset inputs, import review, and
            publish history now live on one NexArr admin surface.
          </p>
        </div>
        <p className="text-xs text-[var(--color-text-muted)]">
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

      <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-3">
        <div
          role="tablist"
          aria-label="Reference data workspaces"
          className="grid gap-2 md:grid-cols-2 xl:grid-cols-6"
        >
          {REFERENCE_DATA_VIEWS.map((view) => (
            <WorkspaceTab
              key={view.id}
              id={view.id}
              label={view.label}
              helper={view.helper}
              isActive={activeView === view.id}
              onSelect={setActiveView}
            />
          ))}
        </div>
        <p className="mt-3 px-1 text-xs text-[var(--color-text-muted)]">
          Heavy reference-data workspaces now load on demand so dataset control stays responsive while
          deeper review tools stay available when you need them.
        </p>
      </div>

      {activeView === 'catalog' ? (
        <div
          id="reference-data-panel-catalog"
          role="tabpanel"
          aria-labelledby="reference-data-tab-catalog"
          className="space-y-6"
        >
          <Section
            title="Dataset Control Plane"
            description="Create or edit datasets, publish selected or all datasets, and manage the live catalog inventory."
          >
            <div className="grid gap-6 xl:grid-cols-[360px_1fr]">
          <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
            <div className="flex items-center justify-between gap-3">
              <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">
                {datasetForm.id ? 'Edit dataset' : 'New dataset'}
              </h4>
              {datasetForm.id ? (
                <button
                  type="button"
                  onClick={resetDatasetForm}
                  className="rounded-md border border-[var(--color-border-default)] px-3 py-1.5 text-xs font-medium text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-control-hover)]"
                >
                  Cancel
                </button>
              ) : null}
            </div>

            <div className="mt-3 space-y-3">
              <Field label="Key">
                <input
                  value={datasetForm.key}
                  onChange={(event) => setDatasetForm((current) => ({ ...current, key: event.target.value }))}
                  className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                  placeholder="vehicle-taxonomy"
                />
              </Field>
              <Field label="Name">
                <input
                  value={datasetForm.name}
                  onChange={(event) => setDatasetForm((current) => ({ ...current, name: event.target.value }))}
                  className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                  placeholder="Vehicle Taxonomy"
                />
              </Field>
              <div className="grid gap-3 md:grid-cols-3">
                <Field label="Category">
                  <input
                    value={datasetForm.category}
                    onChange={(event) =>
                      setDatasetForm((current) => ({ ...current, category: event.target.value }))
                    }
                    className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                    placeholder="vehicle"
                  />
                </Field>
                <Field label="Owner">
                  <input
                    value={datasetForm.ownerService}
                    onChange={(event) =>
                      setDatasetForm((current) => ({ ...current, ownerService: event.target.value }))
                    }
                    className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                    placeholder="ReferenceDataCore"
                  />
                </Field>
                <Field label="Status">
                  <select
                    value={datasetForm.status}
                    onChange={(event) => setDatasetForm((current) => ({ ...current, status: event.target.value }))}
                    className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                  >
                    <option value="draft">draft</option>
                    <option value="ready">ready</option>
                    <option value="published">published</option>
                    <option value="archived">archived</option>
                  </select>
                </Field>
              </div>

              <button
                type="button"
                disabled={
                  saveDatasetMutation.isPending ||
                  !datasetForm.key.trim() ||
                  !datasetForm.name.trim() ||
                  !datasetForm.category.trim() ||
                  !datasetForm.ownerService.trim()
                }
                onClick={() => saveDatasetMutation.mutate()}
                className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
              >
                {saveDatasetMutation.isPending
                  ? 'Saving...'
                  : datasetForm.id
                    ? 'Update dataset'
                    : 'Create dataset'}
              </button>
            </div>
          </div>

          <div className="space-y-4">
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div>
                  <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Batch publish</h4>
                  <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                    Publish selected datasets together or republish the full platform catalog.
                  </p>
                </div>
                <div className="flex flex-wrap gap-2">
                  <button
                    type="button"
                    disabled={publishSelectedMutation.isPending || selectedDatasetIds.length === 0}
                    onClick={() => publishSelectedMutation.mutate(selectedDatasetIds)}
                    className="rounded-md border border-[var(--color-border-default)] px-4 py-2 text-sm font-medium text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-control-hover)] disabled:opacity-50"
                  >
                    {publishSelectedMutation.isPending ? 'Publishing...' : 'Publish selected'}
                  </button>
                  <button
                    type="button"
                    disabled={publishAllMutation.isPending || selectableDatasetIds.length === 0}
                    onClick={() => publishAllMutation.mutate()}
                    className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
                  >
                    {publishAllMutation.isPending ? 'Publishing...' : 'Publish all'}
                  </button>
                </div>
              </div>
              <p className="mt-3 text-xs text-[var(--color-text-muted)]">
                {selectedDatasetIds.length} dataset{selectedDatasetIds.length === 1 ? '' : 's'} selected for batch publish.
              </p>
            </div>

            <div className="overflow-x-auto">
              <table className="min-w-full text-left text-sm">
                <thead className="border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] text-xs uppercase text-[var(--color-text-muted)]">
                  <tr>
                    <th className="px-3 py-2">
                      <input
                        type="checkbox"
                        aria-label="Select all publishable datasets"
                        checked={allSelectableDatasetsChecked}
                        onChange={(event) =>
                          setSelectedDatasetIds(event.target.checked ? selectableDatasetIds : [])
                        }
                      />
                    </th>
                    <th className="px-3 py-2">Dataset</th>
                    <th className="px-3 py-2">Status</th>
                    <th className="px-3 py-2">Entities</th>
                    <th className="px-3 py-2">Review</th>
                    <th className="px-3 py-2">Published</th>
                    <th className="px-3 py-2">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {visibleDatasets.map((dataset) => {
                    const isArchived = dataset.status.toLowerCase() === 'archived'
                    const isSelected = selectedDatasetIds.includes(dataset.id)

                    return (
                      <tr key={dataset.id} className="border-b border-[var(--color-border-subtle)]">
                        <td className="px-3 py-2">
                          <input
                            type="checkbox"
                            aria-label={`Select ${dataset.name}`}
                            disabled={isArchived}
                            checked={isSelected}
                            onChange={(event) =>
                              setSelectedDatasetIds((current) =>
                                event.target.checked
                                  ? [...current, dataset.id]
                                  : current.filter((id) => id !== dataset.id),
                              )
                            }
                          />
                        </td>
                        <td className="px-3 py-2">
                          <p className="font-medium text-[var(--color-text-primary)]">{dataset.name}</p>
                          <p className="text-xs text-[var(--color-text-muted)]">
                            {dataset.key} · {dataset.category} · {dataset.ownerService}
                          </p>
                        </td>
                        <td className="px-3 py-2">
                          <StatusBadge value={dataset.status} />
                        </td>
                        <td className="px-3 py-2">
                          {dataset.entityCount} entities
                          <span className="block text-xs text-[var(--color-text-muted)]">{dataset.sourceCount} sources</span>
                        </td>
                        <td className="px-3 py-2">
                          {dataset.pendingReviewCount} pending
                          <span className="block text-xs text-[var(--color-text-muted)]">
                            {dataset.failedImportCount} failed imports
                          </span>
                        </td>
                        <td className="px-3 py-2">
                          <span>{dataset.currentPublishedVersion ?? 'Not published'}</span>
                          <span className="block text-xs text-[var(--color-text-muted)]">
                            {dataset.lastPublishedAt
                              ? new Date(dataset.lastPublishedAt).toLocaleString()
                              : 'No publish yet'}
                          </span>
                        </td>
                        <td className="px-3 py-2">
                          <div className="flex flex-wrap gap-2">
                            <button
                              type="button"
                              onClick={() => {
                                setSelectedDatasetId(dataset.id)
                                setActiveView('inputs')
                              }}
                              className="rounded-md border border-[var(--color-border-default)] px-3 py-1.5 text-xs font-medium text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-control-hover)]"
                            >
                              Manage inputs
                            </button>
                            <button
                              type="button"
                              onClick={() =>
                                setDatasetForm({
                                  id: dataset.id,
                                  key: dataset.key,
                                  name: dataset.name,
                                  category: dataset.category,
                                  ownerService: dataset.ownerService,
                                  status: dataset.status,
                                })
                              }
                              className="rounded-md border border-[var(--color-border-default)] px-3 py-1.5 text-xs font-medium text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-control-hover)]"
                            >
                              Edit
                            </button>
                            <button
                              type="button"
                              disabled={publishDatasetMutation.isPending || isArchived}
                              onClick={() => publishDatasetMutation.mutate(dataset)}
                              className="rounded-md border border-[var(--tone-success-border)] px-3 py-1.5 text-xs font-medium text-[var(--tone-success-text)] hover:bg-[var(--tone-success-bg)] disabled:opacity-50"
                            >
                              Publish
                            </button>
                            <button
                              type="button"
                              disabled={deleteDatasetMutation.isPending}
                              onClick={() =>
                                setPendingDeleteTarget({
                                  kind: 'dataset',
                                  dataset,
                                  message: `Delete dataset "${dataset.name}"?`,
                                })
                              }
                              className="rounded-md border border-[var(--tone-danger-border)] px-3 py-1.5 text-xs font-medium text-[var(--tone-danger-text)] hover:bg-[var(--tone-danger-bg)] disabled:opacity-50"
                            >
                              Delete
                            </button>
                          </div>
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
          </div>
            </div>
          </Section>
        </div>
      ) : null}

      {activeView === 'sources' ? (
        <div
          id="reference-data-panel-sources"
          role="tabpanel"
          aria-labelledby="reference-data-tab-sources"
          className="space-y-6"
        >
          {sourcesError ? (
            <ApiErrorCallout
              message={getErrorMessage(sourcesError, 'Failed to load reference sources.')}
              onRetry={() => void refreshAll()}
              retryLabel="Retry sources"
            />
          ) : sourcesQuery.isLoading ? (
            <p className="text-sm text-[var(--color-text-muted)]">Loading sources...</p>
          ) : (
            <Section
              title="Sources And Imports"
              description="Register sources, queue review imports, and upload the master CSV routing template."
            >
              <div className="grid gap-4 2xl:grid-cols-4 xl:grid-cols-2">
          <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
            <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">New source</h4>
            <div className="mt-3 space-y-3">
              <Field label="Key">
                <input
                  value={sourceKey}
                  onChange={(event) => setSourceKey(event.target.value)}
                  className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                  placeholder="nhtsa-vpic"
                />
              </Field>
              <Field label="Name">
                <input
                  value={sourceName}
                  onChange={(event) => setSourceName(event.target.value)}
                  className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                  placeholder="NHTSA vPIC"
                />
              </Field>
              <div className="grid gap-3 md:grid-cols-2">
                <Field label="Source type">
                  <input
                    value={sourceType}
                    onChange={(event) => setSourceType(event.target.value)}
                    className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                    placeholder="connector"
                  />
                </Field>
                <Field label="Connector">
                  <input
                    value={connectorType}
                    onChange={(event) => setConnectorType(event.target.value)}
                    className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                    placeholder="manual"
                  />
                </Field>
              </div>
              <div className="grid gap-3 md:grid-cols-2">
                <Field label="Authority rank">
                  <input
                    value={authorityRank}
                    onChange={(event) => setAuthorityRank(event.target.value)}
                    className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                    placeholder="50"
                  />
                </Field>
                <Field label="Cadence">
                  <input
                    value={refreshCadence}
                    onChange={(event) => setRefreshCadence(event.target.value)}
                    className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                    placeholder="weekly"
                  />
                </Field>
              </div>
              <Field label="Terms notes">
                <textarea
                  value={termsNotes}
                  onChange={(event) => setTermsNotes(event.target.value)}
                  rows={3}
                  className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                  placeholder="Source usage notes"
                />
              </Field>
              <label className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)]">
                <input
                  type="checkbox"
                  checked={sourceEnabled}
                  onChange={(event) => setSourceEnabled(event.target.checked)}
                />
                Source enabled
              </label>
              <button
                type="button"
                disabled={createSourceMutation.isPending || !sourceKey.trim() || !sourceName.trim()}
                onClick={() => createSourceMutation.mutate()}
                className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
              >
                {createSourceMutation.isPending ? 'Saving...' : 'Create source'}
              </button>
            </div>
          </div>

          <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
            <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Queue import</h4>
            <div className="mt-3 space-y-3">
              <Field label="Dataset">
                <select
                  value={resolvedImportDatasetId}
                  onChange={(event) => setImportDatasetId(event.target.value)}
                  className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
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
                  value={resolvedImportSourceId}
                  onChange={(event) => setImportSourceId(event.target.value)}
                  className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
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
                  className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                  placeholder="vehicle-taxonomy.csv"
                />
              </Field>
              <Field label="Object key">
                <input
                  value={importObjectKey}
                  onChange={(event) => setImportObjectKey(event.target.value)}
                  className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                  placeholder="seed/reference/vehicle-taxonomy.csv"
                />
              </Field>
              <button
                type="button"
                disabled={createImportMutation.isPending || !resolvedImportDatasetId || !resolvedImportSourceId}
                onClick={() => createImportMutation.mutate()}
                className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
              >
                {createImportMutation.isPending ? 'Queueing...' : 'Queue import'}
              </button>
            </div>
          </div>

          <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
            <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Master CSV upload</h4>
            <div className="mt-3 space-y-3">
              <Field label="CSV file">
                <input
                  type="file"
                  accept=".csv,text/csv"
                  onChange={(event) => setMasterCsvFile(event.target.files?.[0] ?? null)}
                  className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                />
              </Field>
              <Field label="Object key">
                <input
                  value={masterCsvObjectKey}
                  onChange={(event) => setMasterCsvObjectKey(event.target.value)}
                  className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                  placeholder="seed/reference/master-import.csv"
                />
              </Field>
              <div className="flex flex-wrap gap-2">
                <button
                  type="button"
                  onClick={downloadMasterCsvTemplate}
                  className="rounded-md border border-[var(--color-border-default)] px-4 py-2 text-sm font-medium text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-control-hover)]"
                >
                  Download template CSV
                </button>
                <button
                  type="button"
                  disabled={createMasterCsvImportMutation.isPending || !masterCsvFile}
                  onClick={() => createMasterCsvImportMutation.mutate()}
                  className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
                >
                  {createMasterCsvImportMutation.isPending ? 'Uploading...' : 'Upload master CSV'}
                </button>
              </div>
            </div>
          </div>

          <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4 text-sm text-[var(--color-text-muted)]">
            <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Template notes</h4>
            <ul className="mt-3 space-y-2">
              <li><span className="font-medium text-[var(--color-text-secondary)]">Routing:</span> use `dataset_key`; product and dataset aliases also resolve.</li>
              <li><span className="font-medium text-[var(--color-text-secondary)]">Identity:</span> include `entity_type`, `canonical_key`, and `display_name`.</li>
              <li><span className="font-medium text-[var(--color-text-secondary)]">Optional:</span> carry source identifiers, confidence, and any reviewer-facing context columns.</li>
              <li><span className="font-medium text-[var(--color-text-secondary)]">Review:</span> uploaded rows stage first and only become canonical after approval or merge.</li>
            </ul>
          </div>
        </div>

              <div className="mt-6 overflow-x-auto">
          <table className="min-w-full text-left text-sm">
            <thead className="border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] text-xs uppercase text-[var(--color-text-muted)]">
              <tr>
                <th className="px-3 py-2">Source</th>
                <th className="px-3 py-2">Type</th>
                <th className="px-3 py-2">Rank</th>
                <th className="px-3 py-2">Cadence</th>
                <th className="px-3 py-2">State</th>
              </tr>
            </thead>
            <tbody>
              {(sourcesQuery.data ?? []).map((source) => (
                <tr key={source.id} className="border-b border-[var(--color-border-subtle)]">
                  <td className="px-3 py-2">
                    <p className="font-medium text-[var(--color-text-primary)]">{source.name}</p>
                    <p className="text-xs text-[var(--color-text-muted)]">
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
          )}
        </div>
      ) : null}

      {activeView === 'inputs' ? (
        <div
          id="reference-data-panel-inputs"
          role="tabpanel"
          aria-labelledby="reference-data-tab-inputs"
          className="space-y-6"
        >
          {inputsError ? (
            <ApiErrorCallout
              message={getErrorMessage(inputsError, 'Failed to load dataset inputs.')}
              onRetry={() => void refreshAll()}
              retryLabel="Retry inputs"
            />
          ) : (
            <Section
              title="Dataset Inputs And Current Entities"
              description="Pick a dataset, add one value or several values, then edit or delete the current canonical records from the same screen."
            >
              <div className="grid gap-6 2xl:grid-cols-[320px_320px_1fr]">
          <div className="space-y-4">
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <Field label="Dataset">
                <select
                  value={selectedDatasetId}
                  onChange={(event) => setSelectedDatasetId(event.target.value)}
                  className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                >
                  {datasetOptions.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </Field>
              <div className="mt-3 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-3 text-sm text-[var(--color-text-muted)]">
                <p className="font-medium text-[var(--color-text-primary)]">Selected dataset</p>
                <p className="mt-1">{selectedDataset ? formatDatasetLabel(selectedDataset) : '-'}</p>
                <p className="mt-1 font-mono text-xs text-[var(--color-text-muted)]">{selectedDataset?.key ?? '-'}</p>
                <div className="mt-2 flex flex-wrap gap-2">
                  {selectedDataset ? <StatusBadge value={selectedDataset.status} /> : null}
                  <span className="rounded-full bg-[var(--color-bg-control-hover)] px-2 py-1 text-xs text-[var(--color-text-muted)]">
                    {selectedDataset?.entityCount ?? 0} entities
                  </span>
                </div>
              </div>
            </div>

            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <div className="flex items-center justify-between gap-3">
                <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Browse by product</h4>
                <span className="rounded-full bg-[var(--color-bg-surface)] px-2 py-0.5 text-xs font-medium text-[var(--color-text-muted)]">
                  {visibleDatasets.length}
                </span>
              </div>
              <div className="mt-3 space-y-3">
                {datasetsByProduct.map((group) => (
                  <div key={group.ownerService} className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-3">
                    <div className="flex items-center justify-between gap-3">
                      <h5 className="font-medium text-[var(--color-text-primary)]">{group.ownerService}</h5>
                      <span className="rounded-full bg-[var(--color-bg-control-hover)] px-2 py-0.5 text-xs font-medium text-[var(--color-text-muted)]">
                        {group.datasets.length}
                      </span>
                    </div>
                    <div className="mt-3 flex flex-wrap gap-2">
                      {group.datasets.map((dataset) => {
                        const isSelected = dataset.id === selectedDatasetId
                        return (
                          <button
                            key={dataset.id}
                            type="button"
                            onClick={() => setSelectedDatasetId(dataset.id)}
                            className={[
                              'rounded-full border px-3 py-1.5 text-left text-sm transition',
                              isSelected
                                ? 'border-[var(--color-accent-border)] bg-[var(--color-accent-soft)] text-[var(--color-text-primary)] shadow-sm'
                                : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] text-[var(--color-text-secondary)] hover:border-[var(--color-border-default)] hover:bg-[var(--color-bg-surface-muted)]',
                            ].join(' ')}
                          >
                            <span className="block font-medium">{dataset.name}</span>
                            <span className="block font-mono text-[11px] text-[var(--color-text-muted)]">{dataset.key}</span>
                          </button>
                        )
                      })}
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {latestDatasetInput ? (
              <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
                <div className="flex flex-wrap items-center gap-3">
                  <div>
                    <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Latest dataset input</h4>
                    <p className="text-sm text-[var(--color-text-muted)]">
                      {latestDatasetInput.datasetName} · {latestDatasetInput.status}
                    </p>
                  </div>
                  <span className="ml-auto rounded-full bg-[var(--color-bg-surface)] px-2 py-0.5 text-xs font-medium text-[var(--color-text-secondary)]">
                    {latestDatasetInput.stagingRecordCount} records
                  </span>
                </div>
                <dl className="mt-4 grid gap-3 sm:grid-cols-2">
                  <Detail label="Source" value={latestDatasetInput.sourceName} />
                  <Detail label="Approved" value={String(latestDatasetInput.approvedCount)} />
                  <Detail label="Pending review" value={String(latestDatasetInput.pendingReviewCount)} />
                  <Detail label="Rejected" value={String(latestDatasetInput.rejectedCount)} />
                </dl>
              </div>
            ) : null}
          </div>

          <div className="space-y-4">
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Add value</h4>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">Create one canonical reference value immediately.</p>
              <div className="mt-4 space-y-3">
                <Field label="Value">
                  <input
                    value={singleValue}
                    onChange={(event) => setSingleValue(event.target.value)}
                    placeholder="Asset Class A"
                    className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                  />
                </Field>
                <button
                  type="button"
                  disabled={!selectedDatasetId || !singleValue.trim() || addValueMutation.isPending}
                  onClick={() => addValueMutation.mutate()}
                  className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
                >
                  {addValueMutation.isPending ? 'Saving...' : 'Add value'}
                </button>
              </div>
            </div>

            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Import values</h4>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">Paste one value per line to bulk-create dataset inputs.</p>
              <div className="mt-4 space-y-3">
                <Field label="Values">
                  <textarea
                    value={valuesText}
                    onChange={(event) => setValuesText(event.target.value)}
                    rows={8}
                    placeholder={`Asset Class A\nAsset Class B\nAsset Class C`}
                    className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                  />
                </Field>
                <button
                  type="button"
                  disabled={!selectedDatasetId || !valuesText.trim() || importValuesMutation.isPending}
                  onClick={() => importValuesMutation.mutate()}
                  className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
                >
                  {importValuesMutation.isPending ? 'Importing...' : 'Import values'}
                </button>
              </div>
            </div>
          </div>

          <div className="space-y-4">
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <div className="flex items-center justify-between gap-3">
                <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Entity editor</h4>
                {entityEditor ? (
                  <button
                    type="button"
                    onClick={() => setEntityEditor(null)}
                    className="rounded-md border border-[var(--color-border-default)] px-3 py-1.5 text-xs font-medium text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-control-hover)]"
                  >
                    Clear
                  </button>
                ) : null}
              </div>

              {entityEditor ? (
                <div className="mt-3 space-y-3">
                  <Field label="Display name">
                    <input
                      value={entityEditor.displayName}
                      onChange={(event) =>
                        setEntityEditor((current) =>
                          current ? { ...current, displayName: event.target.value } : current,
                        )
                      }
                      className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                    />
                  </Field>
                  <Field label="Canonical key">
                    <input
                      value={entityEditor.canonicalKey}
                      onChange={(event) =>
                        setEntityEditor((current) =>
                          current ? { ...current, canonicalKey: event.target.value } : current,
                        )
                      }
                      className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 font-mono text-sm"
                    />
                  </Field>
                  <Field label="Effective date">
                    <input
                      type="date"
                      value={entityEditor.effectiveDate}
                      onChange={(event) =>
                        setEntityEditor((current) =>
                          current ? { ...current, effectiveDate: event.target.value } : current,
                        )
                      }
                      className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                    />
                  </Field>
                  <Field label="Normalized fields JSON">
                    <textarea
                      value={entityEditor.normalizedFieldsJson}
                      onChange={(event) =>
                        setEntityEditor((current) =>
                          current ? { ...current, normalizedFieldsJson: event.target.value } : current,
                        )
                      }
                      rows={6}
                      className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 font-mono text-xs"
                    />
                  </Field>
                  <Field label="Source evidence JSON">
                    <textarea
                      value={entityEditor.sourceEvidenceJson}
                      onChange={(event) =>
                        setEntityEditor((current) =>
                          current ? { ...current, sourceEvidenceJson: event.target.value } : current,
                        )
                      }
                      rows={6}
                      className="w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 font-mono text-xs"
                    />
                  </Field>
                  <div className="flex flex-wrap gap-2">
                    <button
                      type="button"
                      disabled={
                        saveEntityMutation.isPending ||
                        !entityEditor.displayName.trim() ||
                        !entityEditor.canonicalKey.trim()
                      }
                      onClick={() => saveEntityMutation.mutate()}
                      className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
                    >
                      {saveEntityMutation.isPending ? 'Saving...' : 'Save entity'}
                    </button>
                    <button
                      type="button"
                      disabled={deleteEntityMutation.isPending}
                      onClick={() => {
                        const entity = datasetEntities.find((item) => item.id === entityEditor.id)
                        if (entity) {
                          setPendingDeleteTarget({
                            kind: 'entity',
                            entity,
                            message: `Delete entity "${entity.displayName}"?`,
                          })
                        }
                      }}
                      className="rounded-md border border-[var(--tone-danger-border)] px-4 py-2 text-sm font-medium text-[var(--tone-danger-text)] hover:bg-[var(--tone-danger-bg)] disabled:opacity-50"
                    >
                      Delete entity
                    </button>
                  </div>
                </div>
              ) : loadEntityMutation.isPending ? (
                <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading entity...</p>
              ) : (
                <p className="mt-3 text-sm text-[var(--color-text-muted)]">
                  Select an entity from the table below to edit or delete it.
                </p>
              )}
            </div>

            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <div className="flex items-center justify-between gap-3">
                <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Current entities</h4>
                <span className="rounded-full bg-[var(--color-bg-surface)] px-2 py-0.5 text-xs font-medium text-[var(--color-text-secondary)]">
                  {datasetEntities.length}
                </span>
              </div>

              {entitiesQuery.isLoading ? (
                <p className="mt-4 text-sm text-[var(--color-text-muted)]">Loading entities...</p>
              ) : datasetEntities.length === 0 ? (
                <p className="mt-4 text-sm text-[var(--color-text-muted)]">No entities exist in this dataset yet.</p>
              ) : (
                <div className="mt-4 overflow-x-auto">
                  <table className="min-w-full text-left text-sm">
                    <thead className="border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] text-xs uppercase text-[var(--color-text-muted)]">
                      <tr>
                        <th className="px-3 py-2">Display name</th>
                        <th className="px-3 py-2">Canonical key</th>
                        <th className="px-3 py-2">Version</th>
                        <th className="px-3 py-2">Published</th>
                        <th className="px-3 py-2">Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {datasetEntities.map((entity) => {
                        return (
                          <tr key={entity.id} className="border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)]">
                            <td className="px-3 py-2">
                              <p className="font-medium text-[var(--color-text-primary)]">{entity.displayName}</p>
                              <p className="text-xs text-[var(--color-text-muted)]">{entity.entityType}</p>
                            </td>
                            <td className="px-3 py-2 font-mono text-xs text-[var(--color-text-muted)]">
                              {entity.canonicalKey}
                            </td>
                            <td className="px-3 py-2">{entity.currentVersion ?? '-'}</td>
                            <td className="px-3 py-2 text-[var(--color-text-muted)]">
                              {entity.publishedAt ? new Date(entity.publishedAt).toLocaleString() : 'Pending publish'}
                            </td>
                            <td className="px-3 py-2">
                              <div className="flex flex-wrap gap-2">
                                <button
                                  type="button"
                                  disabled={loadEntityMutation.isPending || deleteEntityMutation.isPending}
                                  onClick={() => loadEntityMutation.mutate(entity.id)}
                                  className="rounded-md border border-[var(--color-border-default)] px-3 py-1.5 text-xs font-medium text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-control-hover)]"
                                >
                                  {loadEntityMutation.isPending && loadEntityMutation.variables === entity.id
                                    ? 'Loading...'
                                    : 'Edit'}
                                </button>
                                <button
                                  type="button"
                                  disabled={deleteEntityMutation.isPending}
                                  onClick={() =>
                                    setPendingDeleteTarget({
                                      kind: 'entity',
                                      entity,
                                      message: `Delete entity "${entity.displayName}"?`,
                                    })
                                  }
                                  className="rounded-md border border-[var(--tone-danger-border)] px-3 py-1.5 text-xs font-medium text-[var(--tone-danger-text)] hover:bg-[var(--tone-danger-bg)] disabled:opacity-50"
                                >
                                  Delete
                                </button>
                              </div>
                            </td>
                          </tr>
                        )
                      })}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          </div>
              </div>
            </Section>
          )}
        </div>
      ) : null}

      {activeView === 'review' ? (
        <div
          id="reference-data-panel-review"
          role="tabpanel"
          aria-labelledby="reference-data-tab-review"
          className="space-y-6"
        >
          {reviewError ? (
            <ApiErrorCallout
              message={getErrorMessage(reviewError, 'Failed to load the review queue.')}
              onRetry={() => void refreshAll()}
              retryLabel="Retry review queue"
            />
          ) : importsQuery.isLoading ? (
            <p className="text-sm text-[var(--color-text-muted)]">Loading review queue...</p>
          ) : (
            <Section
              title="Imports And Review Queue"
              description="Latest import jobs and their staged records. Review actions stay on the platform control plane."
            >
              <div className="grid gap-6 xl:grid-cols-[360px_1fr]">
          <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
            <div className="flex items-center justify-between gap-3">
              <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Imports</h4>
              <span className="text-xs text-[var(--color-text-muted)]">{imports.length} jobs</span>
            </div>
            <div className="mt-3 space-y-2">
              {imports.map((entry) => (
                <button
                  key={entry.id}
                  type="button"
                  onClick={() => setSelectedImportId(entry.id)}
                  className={`w-full rounded-lg border px-3 py-2 text-left text-sm transition ${
                    resolvedSelectedImportId === entry.id
                      ? 'border-[var(--color-accent-border)] bg-[var(--color-accent-soft)] shadow-sm'
                      : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] hover:bg-[var(--color-bg-control-hover)]'
                  }`}
                >
                  <div className="flex items-center justify-between gap-2">
                    <span className="font-medium text-[var(--color-text-primary)]">{entry.datasetName}</span>
                    <StatusBadge value={entry.status} />
                  </div>
                  <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                    {entry.datasetKey} · {entry.sourceKey}
                  </p>
                  <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                    {entry.stagingRecordCount} records · {entry.pendingReviewCount} pending
                  </p>
                </button>
              ))}
            </div>
          </div>

          <div>
            {selectedImport ? (
              <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">{selectedImport.datasetName}</h4>
                    <p className="text-xs text-[var(--color-text-muted)]">
                      {selectedImport.datasetKey} · {selectedImport.sourceKey} ·{' '}
                      {selectedImport.fileName ?? selectedImport.rawObjectKey ?? 'No file attached'}
                    </p>
                  </div>
                  <div className="text-xs text-[var(--color-text-muted)]">
                    Started {new Date(selectedImport.startedAt).toLocaleString()}
                  </div>
                </div>

                {stagingRecordsQuery.isLoading ? (
                  <p className="mt-4 text-sm text-[var(--color-text-muted)]">Loading staged records...</p>
                ) : stagingRecords.length === 0 ? (
                  <p className="mt-4 text-sm text-[var(--color-text-muted)]">No staged records found.</p>
                ) : (
                  <div className="mt-4 overflow-x-auto">
                    <table className="min-w-full text-left text-sm">
                      <thead className="border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] text-xs uppercase text-[var(--color-text-muted)]">
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
                          <tr key={record.id} className="border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)]">
                            <td className="px-3 py-2">{record.rowNumber ?? '-'}</td>
                            <td className="px-3 py-2">
                              <select
                                value={resolveRowTargetDatasetId(record)}
                                onChange={(event) =>
                                  setRowTargetDatasetIds((current) => ({
                                    ...current,
                                    [record.id]: event.target.value,
                                  }))
                                }
                                className="w-full rounded-md border border-[var(--color-border-default)] px-2 py-1 text-xs"
                              >
                                <option value="">Select dataset</option>
                                {datasetOptions.map((option) => (
                                  <option key={option.value} value={option.value}>
                                    {option.label}
                                  </option>
                                ))}
                              </select>
                              <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                                {record.targetDatasetName
                                  ? `${record.targetOwnerService ?? 'NexArr'} - ${record.targetDatasetName}`
                                  : selectedImportIsMasterCsv
                                    ? 'Assign before approving'
                                    : `${record.datasetKey} target`}
                              </p>
                            </td>
                            <td className="px-3 py-2">
                              <p className="font-medium text-[var(--color-text-primary)]">{record.proposedEntityType}</p>
                              <p className="text-xs text-[var(--color-text-muted)]">
                                {record.datasetKey} · {record.sourceKey}
                              </p>
                            </td>
                            <td className="px-3 py-2 font-mono text-xs text-[var(--color-text-muted)]">
                              {record.proposedCanonicalKey ?? '-'}
                            </td>
                            <td className="px-3 py-2">{Math.round(record.confidence * 100)}%</td>
                            <td className="px-3 py-2">
                              <StatusBadge value={record.status} />
                              {record.reviewReason ? (
                                <p className="mt-1 text-xs text-[var(--color-text-muted)]">{record.reviewReason}</p>
                              ) : null}
                            </td>
                            <td className="px-3 py-2">
                              <div className="flex flex-wrap gap-2">
                                <button
                                  type="button"
                                  disabled={
                                    reviewMutation.isPending ||
                                    (selectedImportIsMasterCsv && !resolveRowTargetDatasetId(record))
                                  }
                                  onClick={() =>
                                    reviewMutation.mutate({
                                      stagingId: record.id,
                                      action: 'approve',
                                      targetDatasetId: resolveRowTargetDatasetId(record) || null,
                                    })
                                  }
                                  className="rounded-md border border-[var(--tone-success-border)] px-3 py-1.5 text-xs font-medium text-[var(--tone-success-text)] hover:bg-[var(--tone-success-bg)] disabled:opacity-50"
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
                                  className="rounded-md border border-[var(--tone-danger-border)] px-3 py-1.5 text-xs font-medium text-[var(--tone-danger-text)] hover:bg-[var(--tone-danger-bg)] disabled:opacity-50"
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
                                  className="rounded-md border border-[var(--tone-warning-border)] px-3 py-1.5 text-xs font-medium text-[var(--tone-warning-text)] hover:bg-[var(--tone-warning-bg)] disabled:opacity-50"
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
              <div className="rounded-xl border border-dashed border-[var(--color-border-default)] bg-[var(--color-bg-surface-muted)] p-6 text-sm text-[var(--color-text-muted)]">
                Select an import to review its staged records.
              </div>
            )}
          </div>
              </div>
            </Section>
          )}
        </div>
      ) : null}

      {activeView === 'crosswalks' ? (
        <div
          id="reference-data-panel-crosswalks"
          role="tabpanel"
          aria-labelledby="reference-data-tab-crosswalks"
          className="space-y-6"
        >
          {crosswalksError ? (
            <ApiErrorCallout
              message={getErrorMessage(crosswalksError, 'Failed to load crosswalks.')}
              onRetry={() => void refreshAll()}
              retryLabel="Retry crosswalks"
            />
          ) : crosswalksQuery.isLoading ? (
            <p className="text-sm text-[var(--color-text-muted)]">Loading crosswalks...</p>
          ) : (
            <Section
              title="Crosswalks"
              description="External identifiers and the canonical reference entities they resolve to."
            >
              <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] text-xs uppercase text-[var(--color-text-muted)]">
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
                  <tr key={crosswalk.id} className="border-b border-[var(--color-border-subtle)]">
                    <td className="px-3 py-2">
                      <p className="font-medium text-[var(--color-text-primary)]">{crosswalk.externalSystem}</p>
                      <p className="text-xs text-[var(--color-text-muted)]">{crosswalk.status}</p>
                    </td>
                    <td className="px-3 py-2 font-mono text-xs text-[var(--color-text-muted)]">{crosswalk.externalKey}</td>
                    <td className="px-3 py-2">
                      <p className="font-medium text-[var(--color-text-primary)]">{crosswalk.displayName}</p>
                      <p className="text-xs text-[var(--color-text-muted)]">
                        {crosswalk.entityType} · {crosswalk.canonicalKey}
                      </p>
                    </td>
                    <td className="px-3 py-2">{crosswalk.sourceKey ?? '-'}</td>
                    <td className="px-3 py-2">{Math.round(crosswalk.confidence * 100)}%</td>
                  </tr>
                ))}
              </tbody>
            </table>
              </div>
            </Section>
          )}
        </div>
      ) : null}

      {activeView === 'history' ? (
        <div
          id="reference-data-panel-history"
          role="tabpanel"
          aria-labelledby="reference-data-tab-history"
          className="space-y-6"
        >
          {historyError ? (
            <ApiErrorCallout
              message={getErrorMessage(historyError, 'Failed to load publish history.')}
              onRetry={() => void refreshAll()}
              retryLabel="Retry history"
            />
          ) : publishHistoryQuery.isLoading ? (
            <p className="text-sm text-[var(--color-text-muted)]">Loading publish history...</p>
          ) : (
            <Section
              title="Publish History"
              description="The recent publish events that advanced datasets into visible versions."
            >
              <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] text-xs uppercase text-[var(--color-text-muted)]">
                <tr>
                  <th className="px-3 py-2">Dataset</th>
                  <th className="px-3 py-2">Version</th>
                  <th className="px-3 py-2">Summary</th>
                  <th className="px-3 py-2">Published</th>
                </tr>
              </thead>
              <tbody>
                {publishHistory.map((event) => (
                  <tr key={event.id} className="border-b border-[var(--color-border-subtle)]">
                    <td className="px-3 py-2">
                      <p className="font-medium text-[var(--color-text-primary)]">{event.datasetName}</p>
                      <p className="text-xs text-[var(--color-text-muted)]">{event.datasetKey}</p>
                    </td>
                    <td className="px-3 py-2">{event.publishedVersion}</td>
                    <td className="px-3 py-2 text-[var(--color-text-secondary)]">{event.summary}</td>
                    <td className="px-3 py-2 text-[var(--color-text-muted)]">
                      {new Date(event.createdAt).toLocaleString()}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
              </div>
            </Section>
          )}
        </div>
      ) : null}
    </div>
  )
}
