import {
  AlertCircle,
  CheckCircle2,
  ChevronLeft,
  ChevronRight,
  FileText,
  FileUp,
  ListChecks,
  PencilLine,
  RefreshCw,
  Send,
  Upload,
  XCircle,
} from 'lucide-react'
import { useEffect, useState } from 'react'
import type { ChangeEvent, FormEvent } from 'react'

export type SmartImportBatchRow = {
  batchId: string
  tenantId?: string
  actorPersonId?: string
  status: string
  destinationProductHint: string
  sourceLabel: string
  fileCount: number
  proposedRecordCount: number
  createdAt: string
  updatedAt: string
  errorCode?: string | null
  errorMessage?: string | null
}

export type SmartImportFileSummary = {
  fileId: string
  fileName: string
  contentType: string
  sizeBytes: number
  sha256: string
  recordArrRecordId?: string | null
  recordArrFileId?: string | null
  status: string
}

export type SmartImportClassificationSummary = {
  classificationId: string
  destinationProduct: string
  entityType: string
  confidence: number
  requiresReview: boolean
  reviewReasons: string[]
  notes?: string | null
}

export type SmartImportProposedRecordRow = {
  proposedRecordId: string
  destinationProduct: string
  entityType: string
  operation: string
  confidence: number
  reviewStatus: string
  requiresReview: boolean
  reviewReasons: string[]
  proposedPayload: unknown
}

export type SmartImportManualFieldMapping = {
  sourceField: string
  targetField: string
}

export type SmartImportCommitPlanSummary = {
  commitPlanId: string
  status: string
  stepCount: number
  completedStepCount: number
  failedStepCount: number
  createdAt: string
  approvedAt?: string | null
}

export type SmartImportAuditEventSummary = {
  auditEventId: string
  eventType: string
  actorType: string
  actorPersonId?: string | null
  result: string
  reasonCode?: string | null
  occurredAt: string
}

export type SmartImportBatchDetail = {
  batch: SmartImportBatchRow
  files: SmartImportFileSummary[]
  classifications: SmartImportClassificationSummary[]
  proposedRecords: SmartImportProposedRecordRow[]
  commitPlans: SmartImportCommitPlanSummary[]
  auditEvents?: SmartImportAuditEventSummary[]
}

export type SmartImportReviewWorkspaceProps = {
  batches: SmartImportBatchRow[]
  selectedBatch?: SmartImportBatchDetail | null
  isLoading?: boolean
  onRefresh: () => Promise<void> | void
  onSelectBatch: (batchId: string) => Promise<void> | void
  onUpload: (file: File, destinationProduct: string) => Promise<void> | void
  onReview: (proposedRecordId: string, decision: 'approved' | 'rejected' | 'needs_changes') => Promise<void> | void
  onApproveAll?: (proposedRecordIds: string[]) => Promise<void> | void
  onApplyMappingOverride?: (fieldMappings: SmartImportManualFieldMapping[]) => Promise<void> | void
  onCreateCommitPlan: (batchId: string) => Promise<void> | void
  onApproveCommitPlan?: (commitPlanId: string) => Promise<void> | void
  onCommitPlan?: (commitPlanId: string) => Promise<void> | void
  initialDestinationProduct?: string
}

const destinationOptions = [
  'staffarr',
  'trainarr',
  'maintainarr',
  'routarr',
  'supplyarr',
  'customarr',
  'ordarr',
  'compliancecore',
  'loadarr',
  'recordarr',
  'reportarr',
  'assurarr',
]

const proposedRecordsPageSize = 50
const hiddenQueueStatuses = new Set(['rejected'])

const reviewReasonLabels: Record<string, string> = {
  unsupported_product_api: 'Destination API not yet supported',
  person_create_or_link: 'Person create/link decision',
  training_or_certification_record: 'Training or certification record',
  compliancecore_import: 'Compliance Core import',
  asset_update: 'Asset create/update',
  overwrite: 'Would overwrite existing data',
  low_confidence_compliance_field: 'Low-confidence compliance field',
  money_amount: 'Money amount detected',
  duplicate_match: 'Possible duplicate match',
  unresolved_location: 'Unresolved location',
  scan_or_handwriting: 'Scan or handwriting source',
  regulatory_retention: 'Regulatory retention applies',
  conflicting_product: 'Conflicting product owner',
  duplicate_record: 'Possible duplicate record',
  customarr_fallback: 'Custom workflow fallback',
  human_confirmation_required: 'Human confirmation required',
  manual_mapping_override: 'Manual mapping override',
}

type PayloadField = {
  label: string
  value: string
}

type SourceColumnPreview = {
  sourceField: string
  sampleValues: string[]
}

function cx(...classes: Array<string | false | null | undefined>): string {
  return classes.filter(Boolean).join(' ')
}

function humanize(value: string): string {
  const spaced = value
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/[._-]+/g, ' ')
    .trim()

  if (!spaced) {
    return 'Unknown'
  }

  return spaced.charAt(0).toUpperCase() + spaced.slice(1)
}

function formatStatus(status: string): string {
  return humanize(status)
}

function formatReviewReason(reason: string): string {
  return reviewReasonLabels[reason] ?? humanize(reason)
}

function formatConfidence(confidence: number): string {
  const normalized = confidence <= 1 ? confidence * 100 : confidence
  return `${Math.round(normalized)}%`
}

function confidenceTone(confidence: number): string {
  const normalized = confidence <= 1 ? confidence * 100 : confidence
  if (normalized >= 85) {
    return 'text-emerald-200'
  }
  if (normalized >= 70) {
    return 'text-amber-200'
  }
  return 'text-red-200'
}

function formatDateTime(value: string | null | undefined): string {
  if (!value) {
    return 'Not recorded'
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleString(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  })
}

function formatBytes(bytes: number): string {
  if (!Number.isFinite(bytes) || bytes < 0) {
    return 'Unknown size'
  }

  const units = ['B', 'KB', 'MB', 'GB']
  let value = bytes
  let unitIndex = 0
  while (value >= 1024 && unitIndex < units.length - 1) {
    value /= 1024
    unitIndex += 1
  }

  const formatted = value >= 10 || unitIndex === 0 ? value.toFixed(0) : value.toFixed(1)
  return `${formatted} ${units[unitIndex]}`
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}

function formatPayloadValue(value: unknown): string {
  if (value === null || value === undefined || value === '') {
    return 'Not provided'
  }

  if (typeof value === 'string' || typeof value === 'number' || typeof value === 'boolean') {
    return String(value)
  }

  if (Array.isArray(value)) {
    const primitives = value.filter(
      (entry) => typeof entry === 'string' || typeof entry === 'number' || typeof entry === 'boolean',
    )
    if (primitives.length > 0) {
      const preview = primitives.slice(0, 3).map(String).join(', ')
      return primitives.length > 3 ? `${preview}, +${primitives.length - 3} more` : preview
    }

    return `${value.length} ${value.length === 1 ? 'item' : 'items'}`
  }

  if (isRecord(value)) {
    const displayValue = value.displayName ?? value.name ?? value.canonicalKey ?? value.id
    if (
      typeof displayValue === 'string'
      || typeof displayValue === 'number'
      || typeof displayValue === 'boolean'
    ) {
      return String(displayValue)
    }

    const fieldCount = Object.keys(value).length
    return `${fieldCount} ${fieldCount === 1 ? 'field' : 'fields'}`
  }

  return String(value)
}

function collectPayloadFields(
  value: unknown,
  prefix = '',
  depth = 0,
  fields: PayloadField[] = [],
): PayloadField[] {
  if (!isRecord(value) || fields.length >= 10) {
    return fields
  }

  const entries = Object.entries(value)
    .filter(([key]) => !['source', 'destinationProduct', 'entityType', 'confidence'].includes(key))
    .sort(([left], [right]) => left.localeCompare(right))

  for (const [key, entryValue] of entries) {
    if (fields.length >= 10) {
      break
    }

    const label = prefix ? `${prefix} ${humanize(key)}` : humanize(key)
    if (isRecord(entryValue) && depth < 1) {
      collectPayloadFields(entryValue, label, depth + 1, fields)
      continue
    }

    fields.push({ label, value: formatPayloadValue(entryValue) })
  }

  return fields
}

function getPayloadFields(payload: unknown): PayloadField[] {
  if (!isRecord(payload)) {
    return [{ label: 'Proposed payload', value: formatPayloadValue(payload) }]
  }

  const focusedPayload =
    isRecord(payload.proposedFields)
      ? payload.proposedFields
      : isRecord(payload.fields)
        ? payload.fields
        : payload

  const fields = collectPayloadFields(focusedPayload)
  if (fields.length > 0) {
    return fields
  }

  return [{ label: 'Proposed payload', value: `${Object.keys(payload).length} fields` }]
}

function getRecordObject(value: unknown, propertyName: string): Record<string, unknown> | null {
  if (!isRecord(value)) {
    return null
  }

  const nested = value[propertyName]
  return isRecord(nested) ? nested : null
}

function getSourceFieldEntries(payload: unknown): Array<[string, unknown]> {
  const sourceFields = getRecordObject(payload, 'sourceFields')
  if (sourceFields) {
    return Object.entries(sourceFields).filter(([key]) => key !== 'rowNumber')
  }

  const proposedFields = getRecordObject(payload, 'proposedFields')
  if (proposedFields) {
    return Object.entries(proposedFields).filter(([key]) => key !== 'rowNumber')
  }

  return []
}

function collectSourceColumnPreviews(records: SmartImportProposedRecordRow[]): SourceColumnPreview[] {
  const previews = new Map<string, SourceColumnPreview>()

  for (const record of records.slice(0, 50)) {
    for (const [sourceField, value] of getSourceFieldEntries(record.proposedPayload)) {
      if (!previews.has(sourceField)) {
        previews.set(sourceField, { sourceField, sampleValues: [] })
      }

      const preview = previews.get(sourceField)!
      const sample = formatPayloadValue(value)
      if (
        sample !== 'Not provided'
        && preview.sampleValues.length < 3
        && !preview.sampleValues.includes(sample)
      ) {
        preview.sampleValues.push(sample)
      }
    }
  }

  return Array.from(previews.values()).sort((left, right) => left.sourceField.localeCompare(right.sourceField))
}

const genericMappingTargetOptions = [
  'displayName',
  'name',
  'description',
  'externalId',
  'status',
  'lifecycleStatus',
  'siteRef',
  'siteName',
  'effectiveDate',
  'notes',
]

const maintainArrAssetMappingTargetOptions = [
  'assetTag',
  'unitNumber',
  'assetNumber',
  'displayName',
  'name',
  'assetClass',
  'assetClassName',
  'assetType',
  'assetTypeName',
  'description',
  'lifecycleStatus',
  'siteRef',
  'siteName',
  'vin',
  'serialNumber',
  'licensePlate',
  'modelYear',
  'manufacturer',
  'model',
  'fuelType',
  'meterUnit',
  'inServiceDate',
]

const supplyArrPartMappingTargetOptions = [
  'partKey',
  'displayName',
  'name',
  'description',
  'categoryKey',
  'unitOfMeasure',
  'manufacturerName',
  'manufacturerPartNumber',
  'status',
  'notes',
]

const staffArrPersonMappingTargetOptions = [
  'personId',
  'personNumber',
  'displayName',
  'firstName',
  'middleName',
  'lastName',
  'preferredName',
  'legalName',
  'email',
  'phone',
  'status',
  'notes',
]

const staffArrLocationMappingTargetOptions = [
  'locationId',
  'locationKey',
  'displayName',
  'name',
  'locationType',
  'siteName',
  'siteCode',
  'status',
  'notes',
]

const customArrCustomerMappingTargetOptions = [
  'customerId',
  'customerNumber',
  'customerKey',
  'displayName',
  'legalName',
  'dbaName',
  'customerTypeKey',
  'status',
  'accountOwnerPersonId',
  'notes',
]

const routArrTripMappingTargetOptions = [
  'tripNumber',
  'routeNumber',
  'routeName',
  'dispatchNumber',
  'displayName',
  'description',
  'assignedDriverPersonId',
  'vehicleRefKey',
  'scheduledStartAt',
  'scheduledEndAt',
  'dispatchStatus',
  'notes',
]

const trainArrRecordMappingTargetOptions = [
  'programKey',
  'trainingKey',
  'displayName',
  'title',
  'description',
  'status',
  'governingBodyKey',
  'rulepackKey',
  'effectiveAt',
  'expiresAt',
  'notes',
]

const complianceCoreMappingTargetOptions = [
  'ruleKey',
  'packKey',
  'citationKey',
  'displayName',
  'title',
  'description',
  'status',
  'governingBodyKey',
  'appliesToProductKey',
  'notes',
]

const ordArrMappingTargetOptions = [
  'orderKey',
  'requestKey',
  'displayName',
  'title',
  'description',
  'status',
  'priority',
  'customerId',
  'customerKey',
  'notes',
]

const loadArrMappingTargetOptions = [
  'receiptKey',
  'inventoryKey',
  'stockKey',
  'binKey',
  'displayName',
  'description',
  'status',
  'quantity',
  'uom',
  'notes',
]

const assurArrMappingTargetOptions = [
  'caseKey',
  'nonconformanceKey',
  'capaKey',
  'displayName',
  'title',
  'description',
  'severity',
  'status',
  'rootCauseSummary',
  'notes',
]

const recordArrMappingTargetOptions = [
  'recordKey',
  'documentKey',
  'displayName',
  'title',
  'description',
  'documentType',
  'status',
  'effectiveAt',
  'expiresAt',
  'notes',
]

function getManualMappingTargetOptions(selectedBatch: SmartImportBatchDetail | null | undefined): string[] {
  const destinationProduct = selectedBatch?.classifications[0]?.destinationProduct
    ?? selectedBatch?.batch.destinationProductHint
    ?? ''
  const entityType = selectedBatch?.classifications[0]?.entityType
    ?? selectedBatch?.proposedRecords[0]?.entityType
    ?? ''
  const normalizedDestination = destinationProduct.toLowerCase()
  const normalizedEntityType = entityType.toLowerCase()
  const targetOptions = resolveManualMappingTargetOptions(normalizedDestination, normalizedEntityType)

  return Array.from(new Set(targetOptions)).sort((left, right) => left.localeCompare(right))
}

function resolveManualMappingTargetOptions(destinationProduct: string, entityType: string): string[] {
  if (destinationProduct === 'maintainarr' && entityType.includes('asset')) {
    return maintainArrAssetMappingTargetOptions
  }

  if (
    destinationProduct === 'supplyarr'
    && (entityType.includes('part') || entityType.includes('item') || entityType.includes('material'))
  ) {
    return supplyArrPartMappingTargetOptions
  }

  if (destinationProduct === 'staffarr') {
    if (entityType.includes('person') || entityType.includes('worker') || entityType.includes('employee')) {
      return staffArrPersonMappingTargetOptions
    }

    if (entityType.includes('location') || entityType.includes('site') || entityType.includes('room') || entityType.includes('dock') || entityType.includes('yard')) {
      return staffArrLocationMappingTargetOptions
    }
  }

  if (destinationProduct === 'customarr' && (entityType.includes('customer') || entityType.includes('account') || entityType.includes('contact') || entityType.includes('lead') || entityType.includes('opportunity') || entityType.includes('case'))) {
    return customArrCustomerMappingTargetOptions
  }

  if (destinationProduct === 'routarr' && (entityType.includes('trip') || entityType.includes('route') || entityType.includes('dispatch') || entityType.includes('proof') || entityType.includes('pod') || entityType.includes('delivery'))) {
    return routArrTripMappingTargetOptions
  }

  if (destinationProduct === 'trainarr' && (entityType.includes('training') || entityType.includes('cert') || entityType.includes('qualification') || entityType.includes('program'))) {
    return trainArrRecordMappingTargetOptions
  }

  if (destinationProduct === 'compliancecore' && (entityType.includes('rule') || entityType.includes('citation') || entityType.includes('pack') || entityType.includes('requirement'))) {
    return complianceCoreMappingTargetOptions
  }

  if (destinationProduct === 'ordarr' && (entityType.includes('order') || entityType.includes('request'))) {
    return ordArrMappingTargetOptions
  }

  if (destinationProduct === 'loadarr' && (entityType.includes('receipt') || entityType.includes('inventory') || entityType.includes('stock') || entityType.includes('bin'))) {
    return loadArrMappingTargetOptions
  }

  if (destinationProduct === 'assurarr' && (entityType.includes('nonconformance') || entityType.includes('capa') || entityType.includes('quality') || entityType.includes('case'))) {
    return assurArrMappingTargetOptions
  }

  if (destinationProduct === 'recordarr' && (entityType.includes('document') || entityType.includes('record') || entityType.includes('file'))) {
    return recordArrMappingTargetOptions
  }

  return genericMappingTargetOptions
}

function truncateMiddle(value: string | null | undefined, start = 10, end = 8): string {
  if (!value) {
    return 'Not retained'
  }

  if (value.length <= start + end + 3) {
    return value
  }

  return `${value.slice(0, start)}...${value.slice(-end)}`
}

export function SmartImportReviewWorkspace({
  batches,
  selectedBatch,
  isLoading = false,
  onRefresh,
  onSelectBatch,
  onUpload,
  onReview,
  onApproveAll,
  onApplyMappingOverride,
  onCreateCommitPlan,
  onApproveCommitPlan,
  onCommitPlan,
  initialDestinationProduct,
}: SmartImportReviewWorkspaceProps) {
  const [file, setFile] = useState<File | null>(null)
  const [recordPage, setRecordPage] = useState(0)
  const [isApprovingAll, setIsApprovingAll] = useState(false)
  const [isApplyingMappingOverride, setIsApplyingMappingOverride] = useState(false)
  const [mappingTargets, setMappingTargets] = useState<Record<string, string>>({})
  const [destinationProduct, setDestinationProduct] = useState(
    initialDestinationProduct && destinationOptions.includes(initialDestinationProduct)
      ? initialDestinationProduct
      : destinationOptions[0],
  )

  const handleFileChange = (event: ChangeEvent<HTMLInputElement>) => {
    setFile(event.target.files?.[0] ?? null)
  }

  const handleUpload = async (event: FormEvent) => {
    event.preventDefault()
    if (!file) return
    await onUpload(file, destinationProduct)
    setFile(null)
  }

  const selectedBatchId = selectedBatch?.batch.batchId
  const visibleBatches = batches.filter((batch) => !hiddenQueueStatuses.has(batch.status.toLowerCase()))
  const selectedRecords = selectedBatch?.proposedRecords ?? []
  const approvedCount = selectedRecords.filter((record) => record.reviewStatus === 'approved').length
  const rejectedCount = selectedRecords.filter((record) => record.reviewStatus === 'rejected').length
  const needsChangesCount = selectedRecords.filter((record) => record.reviewStatus === 'needs_changes').length
  const needsReviewCount = selectedRecords.filter((record) => record.requiresReview && record.reviewStatus !== 'approved').length
  const recordPageCount = Math.max(1, Math.ceil(selectedRecords.length / proposedRecordsPageSize))
  const safeRecordPage = Math.min(recordPage, recordPageCount - 1)
  const visibleRecordStart = safeRecordPage * proposedRecordsPageSize
  const visibleRecords = selectedRecords.slice(
    visibleRecordStart,
    visibleRecordStart + proposedRecordsPageSize,
  )
  const bulkApprovalEligibleRecords = selectedRecords.filter((record) => {
    const reviewStatus = record.reviewStatus.toLowerCase()
    return reviewStatus !== 'approved' && reviewStatus !== 'rejected'
  })
  const bulkApprovalSkippedCount = selectedRecords.length - bulkApprovalEligibleRecords.length
  const sourceColumnPreviews = collectSourceColumnPreviews(selectedRecords)
  const manualMappingTargetOptions = getManualMappingTargetOptions(selectedBatch)
  const manualFieldMappings = Object.entries(mappingTargets)
    .map(([sourceField, targetField]) => ({
      sourceField,
      targetField: targetField.trim(),
    }))
    .filter((mapping) => mapping.targetField.length > 0)

  useEffect(() => {
    setRecordPage(0)
    setMappingTargets({})
  }, [selectedBatchId])

  const handleApproveAll = async () => {
    if (bulkApprovalEligibleRecords.length === 0 || isApprovingAll) {
      return
    }

    const message = bulkApprovalSkippedCount > 0
      ? `Approve ${bulkApprovalEligibleRecords.length} proposed records? ${bulkApprovalSkippedCount} already approved or rejected records will be skipped.`
      : `Approve ${bulkApprovalEligibleRecords.length} proposed records?`

    if (!window.confirm(message)) {
      return
    }

    setIsApprovingAll(true)
    try {
      const proposedRecordIds = bulkApprovalEligibleRecords.map((record) => record.proposedRecordId)
      if (onApproveAll) {
        await onApproveAll(proposedRecordIds)
        return
      }

      for (const proposedRecordId of proposedRecordIds) {
        await onReview(proposedRecordId, 'approved')
      }
    } finally {
      setIsApprovingAll(false)
    }
  }

  const handleApplyMappingOverride = async () => {
    if (!onApplyMappingOverride || manualFieldMappings.length === 0 || isApplyingMappingOverride) {
      return
    }

    if (!window.confirm(`Apply ${manualFieldMappings.length} manual mappings to ${selectedRecords.length} proposed records? Approved records will return to review and rejected records will be skipped.`)) {
      return
    }

    setIsApplyingMappingOverride(true)
    try {
      await onApplyMappingOverride(manualFieldMappings)
      setMappingTargets({})
    } finally {
      setIsApplyingMappingOverride(false)
    }
  }

  return (
    <div className="grid gap-4 xl:grid-cols-[360px_minmax(0,1fr)]">
      <section className="rounded-md border border-slate-700 bg-slate-900/70">
        <div className="flex items-center justify-between border-b border-slate-700 px-4 py-3">
          <h2 className="text-sm font-semibold text-white">Smart Import</h2>
          <button
            type="button"
            title="Refresh"
            aria-label="Refresh"
            onClick={() => void onRefresh()}
            className="inline-flex h-8 w-8 items-center justify-center rounded-md text-slate-300 hover:bg-slate-800 hover:text-white"
          >
            <RefreshCw className="h-4 w-4" aria-hidden />
          </button>
        </div>

        <form onSubmit={handleUpload} className="space-y-3 border-b border-slate-700 p-4">
          <label className="block text-xs font-medium text-slate-300" htmlFor="smart-import-destination">
            Destination
          </label>
          <select
            id="smart-import-destination"
            value={destinationProduct}
            onChange={(event) => setDestinationProduct(event.target.value)}
            className="w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
          >
            {destinationOptions.map((option) => (
              <option key={option} value={option}>
                {option}
              </option>
            ))}
          </select>

          <label className="flex cursor-pointer items-center gap-2 rounded-md border border-dashed border-slate-600 bg-slate-950 px-3 py-3 text-sm text-slate-200 hover:border-teal-400/60">
            <FileUp className="h-4 w-4 text-teal-300" aria-hidden />
            <span className="truncate">{file?.name ?? 'Choose source file'}</span>
            <input className="sr-only" type="file" onChange={handleFileChange} />
          </label>

          <button
            type="submit"
            disabled={!file}
            className="inline-flex w-full items-center justify-center gap-2 rounded-md bg-teal-500 px-3 py-2 text-sm font-semibold text-[var(--color-text-primary)] hover:bg-teal-400 disabled:cursor-not-allowed disabled:bg-slate-700 disabled:text-slate-400"
          >
            <Upload className="h-4 w-4" aria-hidden />
            Upload
          </button>
        </form>

        <div className="max-h-[520px] overflow-auto p-2">
          {visibleBatches.length === 0 ? (
            <div className="rounded-md border border-dashed border-slate-700 px-3 py-5 text-center text-sm text-slate-400">
              No import batches yet.
            </div>
          ) : (
            visibleBatches.map((batch) => (
              <button
                key={batch.batchId}
                type="button"
                onClick={() => void onSelectBatch(batch.batchId)}
                className={cx(
                  'mb-2 block w-full rounded-md border bg-slate-950 p-3 text-left hover:border-teal-400/50',
                  selectedBatchId === batch.batchId ? 'border-teal-400/70' : 'border-slate-700',
                )}
              >
                <div className="flex items-center justify-between gap-2">
                  <p className="truncate text-sm font-medium text-white">{batch.sourceLabel}</p>
                  <span className="rounded bg-slate-800 px-2 py-0.5 text-xs text-slate-200">
                    {formatStatus(batch.status)}
                  </span>
                </div>
                <p className="mt-1 text-xs text-slate-400">
                  {batch.destinationProductHint} · {batch.proposedRecordCount} proposed · {batch.fileCount} files
                </p>
                <p className="mt-1 text-xs text-[var(--color-text-muted)]">Updated {formatDateTime(batch.updatedAt)}</p>
                {batch.errorMessage ? (
                  <p className="mt-2 line-clamp-2 text-xs text-red-200">{batch.errorMessage}</p>
                ) : null}
              </button>
            ))
          )}
        </div>
      </section>

      <section className="rounded-md border border-slate-700 bg-slate-900/70">
        <div className="flex items-center justify-between border-b border-slate-700 px-4 py-3">
          <div className="min-w-0">
            <h2 className="truncate text-sm font-semibold text-white">
              {selectedBatch?.batch.sourceLabel ?? 'Review queue'}
            </h2>
            <p className="text-xs text-slate-400">
              {isLoading ? 'Loading' : selectedBatch ? formatStatus(selectedBatch.batch.status) : 'No batch selected'}
            </p>
          </div>
          {selectedBatch ? (
            <button
              type="button"
              onClick={() => void onCreateCommitPlan(selectedBatch.batch.batchId)}
              className="inline-flex items-center gap-2 rounded-md border border-teal-400/50 bg-teal-500/10 px-3 py-2 text-sm text-teal-100 hover:bg-teal-500/20"
            >
              <CheckCircle2 className="h-4 w-4" aria-hidden />
              Plan commit
            </button>
          ) : null}
        </div>

        <div className="space-y-5 p-4">
          {!selectedBatch ? (
            <div className="rounded-md border border-dashed border-slate-700 px-4 py-10 text-center text-sm text-slate-400">
              Select an import batch to review retained files, classifications, proposed records, and commit plans.
            </div>
          ) : (
            <>
              <div className="grid gap-4 border-b border-slate-800 pb-4 sm:grid-cols-2 lg:grid-cols-4">
                <div>
                  <p className="text-xs font-medium text-[var(--color-text-muted)]">Destination owner</p>
                  <p className="mt-1 text-sm font-semibold text-white">{selectedBatch.batch.destinationProductHint}</p>
                </div>
                <div>
                  <p className="text-xs font-medium text-[var(--color-text-muted)]">Proposed records</p>
                  <p className="mt-1 text-sm font-semibold text-white">{selectedRecords.length}</p>
                  <p className="text-xs text-slate-400">
                    {approvedCount} approved · {rejectedCount} rejected · {needsChangesCount} needs changes
                  </p>
                </div>
                <div>
                  <p className="text-xs font-medium text-[var(--color-text-muted)]">Review needed</p>
                  <p className="mt-1 text-sm font-semibold text-white">{needsReviewCount}</p>
                  <p className="text-xs text-slate-400">Human approval before commit planning</p>
                </div>
                <div>
                  <p className="text-xs font-medium text-[var(--color-text-muted)]">Last updated</p>
                  <p className="mt-1 text-sm font-semibold text-white">{formatDateTime(selectedBatch.batch.updatedAt)}</p>
                </div>
              </div>

              {selectedBatch.batch.errorMessage ? (
                <div className="flex gap-3 rounded-md border border-red-500/30 bg-red-500/10 px-3 py-3 text-sm text-red-100">
                  <AlertCircle className="mt-0.5 h-4 w-4 flex-none" aria-hidden />
                  <div>
                    <p className="font-semibold">Import error</p>
                    <p className="mt-1 text-red-100/90">{selectedBatch.batch.errorMessage}</p>
                  </div>
                </div>
              ) : null}

              <div className="grid gap-4 lg:grid-cols-2">
                <section className="space-y-2">
                  <div className="flex items-center gap-2 text-sm font-semibold text-white">
                    <FileText className="h-4 w-4 text-teal-300" aria-hidden />
                    Retained source files
                  </div>
                  {selectedBatch.files.length === 0 ? (
                    <p className="text-sm text-slate-400">No retained files are attached to this batch.</p>
                  ) : (
                    <div className="divide-y divide-slate-800 rounded-md border border-slate-800">
                      {selectedBatch.files.map((sourceFile) => (
                        <div key={sourceFile.fileId} className="space-y-2 p-3">
                          <div className="flex items-center justify-between gap-2">
                            <p className="truncate text-sm font-medium text-white">{sourceFile.fileName}</p>
                            <span className="rounded bg-slate-800 px-2 py-0.5 text-xs text-slate-200">
                              {formatStatus(sourceFile.status)}
                            </span>
                          </div>
                          <dl className="grid gap-2 text-xs text-slate-400 sm:grid-cols-2">
                            <div>
                              <dt className="text-[var(--color-text-muted)]">Type</dt>
                              <dd>{sourceFile.contentType || 'Unknown'}</dd>
                            </div>
                            <div>
                              <dt className="text-[var(--color-text-muted)]">Size</dt>
                              <dd>{formatBytes(sourceFile.sizeBytes)}</dd>
                            </div>
                            <div>
                              <dt className="text-[var(--color-text-muted)]">RecordArr record</dt>
                              <dd>{truncateMiddle(sourceFile.recordArrRecordId)}</dd>
                            </div>
                            <div>
                              <dt className="text-[var(--color-text-muted)]">SHA-256</dt>
                              <dd>{truncateMiddle(sourceFile.sha256, 8, 8)}</dd>
                            </div>
                          </dl>
                        </div>
                      ))}
                    </div>
                  )}
                </section>

                <section className="space-y-2">
                  <div className="flex items-center gap-2 text-sm font-semibold text-white">
                    <ListChecks className="h-4 w-4 text-teal-300" aria-hidden />
                    Classification evidence
                  </div>
                  {selectedBatch.classifications.length === 0 ? (
                    <p className="text-sm text-slate-400">No classifications have been recorded yet.</p>
                  ) : (
                    <div className="divide-y divide-slate-800 rounded-md border border-slate-800">
                      {selectedBatch.classifications.map((classification) => (
                        <div key={classification.classificationId} className="space-y-2 p-3">
                          <div className="flex items-start justify-between gap-2">
                            <div>
                              <p className="text-sm font-medium text-white">
                                {classification.destinationProduct} · {classification.entityType}
                              </p>
                              <p className="text-xs text-slate-400">
                                {classification.requiresReview ? 'Review required' : 'Ready for review decision'}
                              </p>
                            </div>
                            <span className={cx('text-sm font-semibold', confidenceTone(classification.confidence))}>
                              {formatConfidence(classification.confidence)}
                            </span>
                          </div>
                          {classification.notes ? (
                            <p className="text-xs text-slate-300">{classification.notes}</p>
                          ) : null}
                          {classification.reviewReasons.length > 0 ? (
                            <div className="flex flex-wrap gap-2">
                              {classification.reviewReasons.map((reason) => (
                                <span key={reason} className="rounded bg-amber-500/10 px-2 py-1 text-xs text-amber-200">
                                  {formatReviewReason(reason)}
                                </span>
                              ))}
                            </div>
                          ) : null}
                        </div>
                      ))}
                    </div>
                  )}
                </section>
              </div>

              {selectedBatch.commitPlans.length > 0 ? (
                <section className="space-y-2">
                  <div className="flex items-center gap-2 text-sm font-semibold text-white">
                    <CheckCircle2 className="h-4 w-4 text-teal-300" aria-hidden />
                    Commit plans
                  </div>
                  <div className="divide-y divide-slate-800 rounded-md border border-slate-800">
                    {selectedBatch.commitPlans.map((plan) => (
                      <div key={plan.commitPlanId} className="grid gap-3 p-3 text-sm sm:grid-cols-5">
                        <div>
                          <p className="text-xs text-[var(--color-text-muted)]">Status</p>
                          <p className="font-medium text-white">{formatStatus(plan.status)}</p>
                        </div>
                        <div>
                          <p className="text-xs text-[var(--color-text-muted)]">Steps</p>
                          <p className="font-medium text-white">{plan.stepCount}</p>
                        </div>
                        <div>
                          <p className="text-xs text-[var(--color-text-muted)]">Progress</p>
                          <p className="font-medium text-white">
                            {plan.completedStepCount} done · {plan.failedStepCount} failed
                          </p>
                        </div>
                        <div>
                          <p className="text-xs text-[var(--color-text-muted)]">Created</p>
                          <p className="font-medium text-white">{formatDateTime(plan.createdAt)}</p>
                        </div>
                        <div className="flex items-end gap-2 sm:justify-end">
                          {plan.status === 'draft' && onApproveCommitPlan ? (
                            <button
                              type="button"
                              disabled={isLoading}
                              onClick={() => void onApproveCommitPlan(plan.commitPlanId)}
                              className="inline-flex items-center gap-2 rounded-md border border-sky-400/50 bg-sky-500/10 px-3 py-2 text-xs font-medium text-sky-100 hover:bg-sky-500/20 disabled:cursor-not-allowed disabled:opacity-60"
                            >
                              <CheckCircle2 className="h-4 w-4" aria-hidden />
                              Approve plan
                            </button>
                          ) : null}
                          {['approved', 'failed', 'partially_committed'].includes(plan.status) && onCommitPlan ? (
                            <button
                              type="button"
                              disabled={isLoading}
                              onClick={() => void onCommitPlan(plan.commitPlanId)}
                              className="inline-flex items-center gap-2 rounded-md border border-teal-400/50 bg-teal-500/10 px-3 py-2 text-xs font-medium text-teal-100 hover:bg-teal-500/20 disabled:cursor-not-allowed disabled:opacity-60"
                            >
                              <Send className="h-4 w-4" aria-hidden />
                              {plan.status === 'approved' ? 'Commit' : 'Retry'}
                            </button>
                          ) : null}
                        </div>
                      </div>
                    ))}
                  </div>
                </section>
              ) : null}

              {onApplyMappingOverride && sourceColumnPreviews.length > 0 ? (
                <section className="space-y-3 border-t border-slate-800 pt-4">
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <h3 className="text-sm font-semibold text-white">Manual mapping override</h3>
                      <p className="text-xs text-slate-400">
                        Map source columns into destination fields, then review the restaged proposals.
                      </p>
                    </div>
                    <button
                      type="button"
                      disabled={isLoading || isApplyingMappingOverride || manualFieldMappings.length === 0}
                      onClick={() => void handleApplyMappingOverride()}
                      className="inline-flex items-center gap-2 rounded-md border border-sky-400/50 bg-sky-500/10 px-3 py-2 text-xs font-medium text-sky-100 hover:bg-sky-500/20 disabled:cursor-not-allowed disabled:border-slate-700 disabled:bg-slate-900 disabled:text-[var(--color-text-muted)]"
                    >
                      <PencilLine className="h-4 w-4" aria-hidden />
                      {isApplyingMappingOverride ? 'Applying mapping' : `Apply mapping (${manualFieldMappings.length})`}
                    </button>
                  </div>

                  <datalist id="smart-import-target-field-options">
                    {manualMappingTargetOptions.map((option) => (
                      <option key={option} value={option} />
                    ))}
                  </datalist>

                  <div className="overflow-hidden rounded-md border border-slate-800">
                    <div className="hidden grid-cols-[minmax(0,1fr)_minmax(0,1.2fr)_minmax(180px,240px)] gap-3 border-b border-slate-800 bg-slate-950 px-3 py-2 text-xs font-medium uppercase text-[var(--color-text-muted)] md:grid">
                      <span>Source column</span>
                      <span>Sample values</span>
                      <span>Target field</span>
                    </div>
                    <div className="max-h-80 divide-y divide-slate-800 overflow-auto">
                      {sourceColumnPreviews.map((preview) => (
                        <div
                          key={preview.sourceField}
                          className="grid gap-2 px-3 py-3 text-sm md:grid-cols-[minmax(0,1fr)_minmax(0,1.2fr)_minmax(180px,240px)] md:items-center"
                        >
                          <div>
                            <p className="text-xs font-medium uppercase text-[var(--color-text-muted)] md:hidden">Source column</p>
                            <p className="truncate font-medium text-white">{preview.sourceField}</p>
                          </div>
                          <div>
                            <p className="text-xs font-medium uppercase text-[var(--color-text-muted)] md:hidden">Sample values</p>
                            <p className="truncate text-slate-300">
                              {preview.sampleValues.length > 0 ? preview.sampleValues.join(', ') : 'No sample'}
                            </p>
                          </div>
                          <label className="block">
                            <span className="text-xs font-medium uppercase text-[var(--color-text-muted)] md:hidden">Target field</span>
                            <input
                              list="smart-import-target-field-options"
                              value={mappingTargets[preview.sourceField] ?? ''}
                              onChange={(event) => {
                                const nextValue = event.target.value
                                setMappingTargets((current) => ({
                                  ...current,
                                  [preview.sourceField]: nextValue,
                                }))
                              }}
                              placeholder="Ignore"
                              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white placeholder:text-[var(--color-text-muted)] focus:border-sky-400 focus:outline-none md:mt-0"
                            />
                          </label>
                        </div>
                      ))}
                    </div>
                  </div>
                </section>
              ) : null}

              <section className="space-y-3">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <h3 className="text-sm font-semibold text-white">Proposed records</h3>
                    <p className="text-xs text-slate-400">
                      Approving a record only clears it for commit planning. The destination product still owns the final record.
                    </p>
                    {selectedRecords.length > 1 ? (
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                        Bulk approval affects {bulkApprovalEligibleRecords.length} records; {bulkApprovalSkippedCount} already approved or rejected will be skipped.
                      </p>
                    ) : null}
                  </div>
                  <div className="flex flex-wrap items-center gap-2 text-xs text-slate-300">
                    {selectedRecords.length > 1 ? (
                      <button
                        type="button"
                        disabled={isLoading || isApprovingAll || bulkApprovalEligibleRecords.length === 0}
                        onClick={() => void handleApproveAll()}
                        className="inline-flex items-center gap-2 rounded-md border border-emerald-400/50 bg-emerald-500/10 px-3 py-2 text-xs font-medium text-emerald-100 hover:bg-emerald-500/20 disabled:cursor-not-allowed disabled:border-slate-700 disabled:bg-slate-900 disabled:text-[var(--color-text-muted)]"
                      >
                        <CheckCircle2 className="h-4 w-4" aria-hidden />
                        {isApprovingAll ? 'Approving all' : `Approve all (${bulkApprovalEligibleRecords.length})`}
                      </button>
                    ) : null}
                    {selectedRecords.length > proposedRecordsPageSize ? (
                      <>
                        <span>
                          {visibleRecordStart + 1}-{Math.min(visibleRecordStart + proposedRecordsPageSize, selectedRecords.length)} of {selectedRecords.length}
                        </span>
                        <button
                          type="button"
                          title="Previous proposed records"
                          aria-label="Previous proposed records"
                          disabled={safeRecordPage === 0}
                          onClick={() => setRecordPage((current) => Math.max(0, current - 1))}
                          className="inline-flex h-8 w-8 items-center justify-center rounded-md border border-slate-700 text-slate-200 hover:bg-slate-800 disabled:cursor-not-allowed disabled:text-[var(--color-text-muted)]"
                        >
                          <ChevronLeft className="h-4 w-4" aria-hidden />
                        </button>
                        <button
                          type="button"
                          title="Next proposed records"
                          aria-label="Next proposed records"
                          disabled={safeRecordPage >= recordPageCount - 1}
                          onClick={() => setRecordPage((current) => Math.min(recordPageCount - 1, current + 1))}
                          className="inline-flex h-8 w-8 items-center justify-center rounded-md border border-slate-700 text-slate-200 hover:bg-slate-800 disabled:cursor-not-allowed disabled:text-[var(--color-text-muted)]"
                        >
                          <ChevronRight className="h-4 w-4" aria-hidden />
                        </button>
                      </>
                    ) : null}
                  </div>
                </div>

                {selectedRecords.length === 0 ? (
                  <div className="rounded-md border border-dashed border-slate-700 px-4 py-6 text-center text-sm text-slate-400">
                    No proposed records were created for this batch.
                  </div>
                ) : (
                  visibleRecords.map((record) => {
                    const payloadFields = getPayloadFields(record.proposedPayload)
                    return (
                      <article key={record.proposedRecordId} className="rounded-md border border-slate-700 bg-slate-950 p-4">
                        <div className="flex flex-wrap items-start justify-between gap-3">
                          <div>
                            <h4 className="text-sm font-semibold text-white">
                              {record.destinationProduct} · {record.entityType}
                            </h4>
                            <p className="text-xs text-slate-400">
                              {formatStatus(record.operation)} ·{' '}
                              <span className={confidenceTone(record.confidence)}>
                                {formatConfidence(record.confidence)}
                              </span>{' '}
                              · {formatStatus(record.reviewStatus)}
                            </p>
                            <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                              {record.requiresReview
                                ? 'Human review is required before this candidate can be committed.'
                                : 'Candidate is ready for a review decision.'}
                            </p>
                          </div>
                          <div className="flex flex-wrap gap-2">
                            <button
                              type="button"
                              onClick={() => void onReview(record.proposedRecordId, 'approved')}
                              className="inline-flex items-center gap-1.5 rounded-md bg-emerald-500 px-3 py-1.5 text-xs font-semibold text-[var(--color-text-primary)] hover:bg-emerald-400"
                            >
                              <CheckCircle2 className="h-3.5 w-3.5" aria-hidden />
                              Approve
                            </button>
                            <button
                              type="button"
                              onClick={() => void onReview(record.proposedRecordId, 'needs_changes')}
                              className="inline-flex items-center gap-1.5 rounded-md border border-amber-400/50 px-3 py-1.5 text-xs text-amber-100 hover:bg-amber-500/10"
                            >
                              <PencilLine className="h-3.5 w-3.5" aria-hidden />
                              Needs changes
                            </button>
                            <button
                              type="button"
                              onClick={() => void onReview(record.proposedRecordId, 'rejected')}
                              className="inline-flex items-center gap-1.5 rounded-md border border-slate-600 px-3 py-1.5 text-xs text-slate-200 hover:bg-slate-800"
                            >
                              <XCircle className="h-3.5 w-3.5" aria-hidden />
                              Reject
                            </button>
                          </div>
                        </div>

                        {record.reviewReasons.length > 0 ? (
                          <div className="mt-3 flex flex-wrap gap-2">
                            {record.reviewReasons.map((reason) => (
                              <span key={reason} className="rounded bg-amber-500/10 px-2 py-1 text-xs text-amber-200">
                                {formatReviewReason(reason)}
                              </span>
                            ))}
                          </div>
                        ) : null}

                        <dl className="mt-4 grid gap-x-4 gap-y-3 border-t border-slate-800 pt-3 sm:grid-cols-2 xl:grid-cols-3">
                          {payloadFields.map((field) => (
                            <div key={`${record.proposedRecordId}-${field.label}`} className="min-w-0">
                              <dt className="text-xs font-medium text-[var(--color-text-muted)]">{field.label}</dt>
                              <dd className="mt-0.5 break-words text-sm text-slate-100">{field.value}</dd>
                            </div>
                          ))}
                        </dl>
                      </article>
                    )
                  })
                )}
              </section>
            </>
          )}
        </div>
      </section>
    </div>
  )
}
