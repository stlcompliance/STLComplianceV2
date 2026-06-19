import { useEffect, useMemo, useRef, useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, Navigate, useNavigate, useSearchParams } from 'react-router-dom'
import {
  AlertTriangle,
  ArrowLeft,
  ArrowRight,
  CheckCircle2,
  ChevronDown,
  ChevronUp,
  Loader2,
  Paperclip,
  RefreshCw,
  Sparkles,
  Upload,
  Wrench,
} from 'lucide-react'
import {
  AsyncSearchPicker,
  ControlledSelect,
  PageHeader,
  StaticSearchPicker,
  type PickerOption,
} from '@stl/shared-ui'
import {
  createDefectDraft,
  getAsset,
  getAssetReadiness,
  getCatalogs,
  getDefect,
  getDefectCreateFieldset,
  getDefectEvidence,
  getInspectionRun,
  getMe,
  getPeople,
  getPmProgram,
  getSites,
  getWorkOrder,
  previewDefectDraft,
  searchAssets,
  submitDefectDraft,
  updateDefectDraft,
  uploadDefectEvidence,
} from '../../api/client'
import type {
  AssetReadinessResponse,
  AssetResponse,
  CatalogOptionResponse,
  DefectDetailResponse,
  DefectDraftPreviewResponse,
  DefectEvidenceResponse,
  FieldMetadataResponse,
  FieldsetResponse,
  UpsertDefectDraftRequest,
} from '../../api/types'
import {
  canCreateDefects,
  canCreateWorkOrderFromDefect,
  canManageDefectReadiness,
  canSubmitDefects,
  loadSession,
} from '../../auth/sessionStorage'
import {
  fieldIsVisible,
  getFilteredOptions,
  validateAssetValues,
  type AssetFieldValues,
} from '../../components/AssetFieldsetWorkflow'

type DefectCreateValues = AssetFieldValues & {
  assetId: string
  title: string
  description: string
  reportSource: string
  reportedByPersonId: string
  discoveredByPersonId: string
  reportedAt: string
  discoveredAt: string
  defectType: string
  priority: string
  severity: string
  systemKey: string
  componentKey: string
  failureMode: string
  symptom: string
  sidePosition: string
  operatingCondition: string
  deferralCode: string
  isSafetyCritical: boolean
  isComplianceImpacting: boolean
  isOperabilityImpacting: boolean
  readinessNotes: string
  correctiveAction: string
  sourceType: string
  sourceReferenceId: string
  incidentReferenceId: string
}

type SectionKey =
  | 'reporting'
  | 'asset'
  | 'details'
  | 'safety'
  | 'classification'
  | 'readiness'
  | 'evidence'
  | 'related'
  | 'corrective'
  | 'review'

type Tone = 'good' | 'warn' | 'bad' | 'info' | 'neutral'

interface SectionDefinition {
  key: SectionKey
  label: string
  description: string
  icon: ReactNode
}

const SECTION_DEFINITIONS: SectionDefinition[] = [
  {
    key: 'reporting',
    label: 'Report Basics',
    description: 'Capture how the defect was reported and who discovered it.',
    icon: <Sparkles className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'asset',
    label: 'Asset Selection',
    description: 'Choose the asset this defect belongs to.',
    icon: <Loader2 className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'details',
    label: 'Defect Details',
    description: 'Describe the defect and set the classification basics.',
    icon: <Wrench className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'safety',
    label: 'Severity and Safety',
    description: 'Set severity and impact flags.',
    icon: <AlertTriangle className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'classification',
    label: 'System Classification',
    description: 'Pick the system, component, symptom, and operating context.',
    icon: <Sparkles className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'readiness',
    label: 'Readiness and Compliance',
    description: 'Document readiness impact and any deferral guidance.',
    icon: <CheckCircle2 className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'evidence',
    label: 'Evidence',
    description: 'Attach photos or notes that support the defect.',
    icon: <Paperclip className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'related',
    label: 'Related Records and Duplicates',
    description: 'Confirm the source trail and review likely duplicates.',
    icon: <RefreshCw className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'corrective',
    label: 'Corrective Action',
    description: 'Capture the recommended fix or immediate response.',
    icon: <Wrench className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'review',
    label: 'Final Review',
    description: 'Review blockers, duplicates, and submit options.',
    icon: <CheckCircle2 className="h-4 w-4" aria-hidden />,
  },
]

const DEFECT_FIELD_KEYS = new Set([
  'reportSource',
  'reportedByPersonId',
  'discoveredByPersonId',
  'priority',
  'defectType',
  'severity',
  'systemKey',
  'componentKey',
  'failureMode',
  'symptom',
  'sidePosition',
  'operatingCondition',
  'deferralCode',
  'readinessNotes',
  'correctiveAction',
])

const SOURCE_REPORTING_MAP: Record<string, string> = {
  work_order: 'work_order',
  workorder: 'work_order',
  work_order_ref: 'work_order',
  inspection_run: 'inspection',
  inspection_finding: 'inspection',
  inspection: 'inspection',
  pm_program: 'pm_program',
  pmprogram: 'pm_program',
  readiness_issue: 'readiness',
  readiness: 'readiness',
  incident: 'incident',
  routarr_event: 'routarr',
  routarr: 'routarr',
  compliance_issue: 'compliance',
  compliance: 'compliance',
}

function humanize(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  return value
    .replace(/[_-]+/g, ' ')
    .replace(/\b\w/g, (char) => char.toUpperCase())
}

function truncate(value: string, maxLength = 80): string {
  if (value.length <= maxLength) return value
  return `${value.slice(0, Math.max(0, maxLength - 1)).trimEnd()}…`
}

function nowLocalInputValue(): string {
  const value = new Date()
  const offsetMs = value.getTimezoneOffset() * 60_000
  return new Date(value.getTime() - offsetMs).toISOString().slice(0, 16)
}

function toLocalDateTimeInput(value: string | null | undefined): string {
  if (!value) return ''
  const parsed = new Date(value)
  if (Number.isNaN(parsed.getTime())) return ''
  const offsetMs = parsed.getTimezoneOffset() * 60_000
  return new Date(parsed.getTime() - offsetMs).toISOString().slice(0, 16)
}

function fromLocalDateTimeInput(value: string): string | null {
  const trimmed = value.trim()
  if (!trimmed) return null
  const parsed = new Date(trimmed)
  if (Number.isNaN(parsed.getTime())) return null
  return parsed.toISOString()
}

function trimToEmpty(value: unknown): string {
  return typeof value === 'string' ? value.trim() : ''
}

function mapSourceTypeToReportSource(sourceType: string | null | undefined): string {
  const normalized = trimToEmpty(sourceType).toLowerCase()
  if (!normalized) return 'manual'
  return SOURCE_REPORTING_MAP[normalized] ?? normalized
}

function mapBackendSeverityToCatalogKey(severity: string | null | undefined): string {
  switch ((severity ?? '').trim().toLowerCase()) {
    case 'low':
      return 'minor'
    case 'medium':
      return 'moderate'
    case 'high':
      return 'major'
    case 'critical':
      return 'critical'
    default:
      return 'moderate'
  }
}

function mapCatalogSeverityToBackend(severity: string): string {
  switch (severity.trim().toLowerCase()) {
    case 'informational':
    case 'minor':
      return 'low'
    case 'moderate':
      return 'medium'
    case 'major':
      return 'high'
    case 'critical':
      return 'critical'
    default:
      return 'medium'
  }
}

function mapBackendPriorityToCatalogKey(priority: string | null | undefined): string {
  switch ((priority ?? '').trim().toLowerCase()) {
    case 'low':
      return 'low'
    case 'medium':
      return 'normal'
    case 'high':
      return 'high'
    case 'urgent':
      return 'urgent'
    default:
      return 'normal'
  }
}

function mapFieldOptions(options: readonly CatalogOptionResponse[] | null | undefined): PickerOption[] {
  return (options ?? []).map((option) => ({
    value: option.key,
    label: option.label || humanize(option.key),
  }))
}

function mapReferenceOptions(
  options: readonly { key: string; label: string; isActive?: boolean | null }[] | null | undefined,
): PickerOption[] {
  return (options ?? []).map((option) => ({
    value: option.key,
    label: option.label || humanize(option.key),
    inactive: option.isActive === false,
  }))
}

function buildInitialValues(personId: string, searchParams: URLSearchParams): DefectCreateValues {
  const now = nowLocalInputValue()
  const sourceType = trimToEmpty(searchParams.get('sourceType'))
  const sourceReferenceId = trimToEmpty(searchParams.get('sourceReferenceId'))
  const incidentReferenceId = trimToEmpty(searchParams.get('incidentReferenceId'))
  const assetId = trimToEmpty(searchParams.get('assetId'))

  return {
    assetId,
    title: '',
    description: '',
    reportSource: mapSourceTypeToReportSource(sourceType),
    reportedByPersonId: personId,
    discoveredByPersonId: personId,
    reportedAt: now,
    discoveredAt: now,
    defectType: '',
    priority: 'normal',
    severity: 'moderate',
    systemKey: '',
    componentKey: '',
    failureMode: '',
    symptom: '',
    sidePosition: '',
    operatingCondition: '',
    deferralCode: '',
    isSafetyCritical: false,
    isComplianceImpacting: false,
    isOperabilityImpacting: false,
    readinessNotes: '',
    correctiveAction: '',
    sourceType,
    sourceReferenceId,
    incidentReferenceId,
  }
}

function hydrateValuesFromDetail(detail: DefectDetailResponse, currentPersonId: string): DefectCreateValues {
  return {
    assetId: detail.assetId,
    title: detail.title,
    description: detail.description,
    reportSource: detail.reportSource ?? detail.source ?? 'manual',
    reportedByPersonId: detail.reportedByPersonId ?? currentPersonId,
    discoveredByPersonId: detail.discoveredByPersonId ?? detail.reportedByPersonId ?? currentPersonId,
    reportedAt: toLocalDateTimeInput(detail.reportedAt ?? detail.createdAt),
    discoveredAt: toLocalDateTimeInput(detail.discoveredAt ?? detail.reportedAt ?? detail.createdAt),
    defectType: detail.defectType ?? '',
    priority: mapBackendPriorityToCatalogKey(detail.priority ?? null),
    severity: mapBackendSeverityToCatalogKey(detail.severity),
    systemKey: detail.systemKey ?? '',
    componentKey: detail.componentKey ?? '',
    failureMode: detail.failureMode ?? '',
    symptom: detail.symptom ?? '',
    sidePosition: detail.sidePosition ?? '',
    operatingCondition: detail.operatingCondition ?? '',
    deferralCode: detail.deferralCode ?? '',
    isSafetyCritical: detail.isSafetyCritical ?? false,
    isComplianceImpacting: detail.isComplianceImpacting ?? false,
    isOperabilityImpacting: detail.isOperabilityImpacting ?? false,
    readinessNotes: detail.readinessNotes ?? '',
    correctiveAction: detail.correctiveAction ?? '',
    sourceType: detail.sourceType ?? '',
    sourceReferenceId: detail.sourceReferenceId ?? '',
    incidentReferenceId: detail.incidentReferenceId ?? '',
  }
}

function mergePrefill(current: DefectCreateValues, partial: Partial<DefectCreateValues>): DefectCreateValues {
  const next = { ...current }
  for (const [key, value] of Object.entries(partial)) {
    if (value == null) continue
    const currentValue = next[key as keyof DefectCreateValues]
    const currentText = typeof currentValue === 'string' ? currentValue.trim() : ''
    const shouldSet =
      currentValue === undefined
      || currentValue === null
      || currentText.length === 0
      || (typeof currentValue === 'boolean' && value !== currentValue)

    if (shouldSet) {
      next[key as keyof DefectCreateValues] = value as never
    }
  }
  return next
}

function fieldMeta(fieldset: FieldsetResponse | undefined, key: string): FieldMetadataResponse | undefined {
  return fieldset?.fields.find((field) => field.key === key)
}

function sectionFields(
  fieldset: FieldsetResponse | undefined,
  values: DefectCreateValues,
  keys: string[],
): FieldMetadataResponse[] {
  if (!fieldset) return []
  return fieldset.fields
    .filter((field) => keys.includes(field.key) && fieldIsVisible(fieldset, field, values as AssetFieldValues))
}

function fieldErrorMap(
  fieldset: FieldsetResponse | undefined,
  values: DefectCreateValues,
  sectionKeys: string[],
): Record<string, string> {
  if (!fieldset) return {}
  const fields = sectionFields(fieldset, values, sectionKeys).filter((field) => DEFECT_FIELD_KEYS.has(field.key))
  return validateAssetValues(fieldset, values as AssetFieldValues, fields as FieldMetadataResponse[])
}

function classifyTone(complete: boolean, locked: boolean, issues: string[]): Tone {
  if (locked) return 'neutral'
  if (issues.some((item) => item.toLowerCase().includes('block'))) return 'bad'
  if (issues.length > 0) return 'warn'
  return complete ? 'good' : 'info'
}

function AssetSummaryLine({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-slate-800 bg-slate-950/60 px-3 py-2">
      <p className="text-[11px] uppercase tracking-[0.2em] text-[var(--color-text-muted)]">{label}</p>
      <p className="mt-1 text-sm text-slate-100">{value}</p>
    </div>
  )
}

function FieldRow({
  label,
  description,
  error,
  required,
  children,
  className = '',
}: {
  label: string
  description?: string | null
  error?: string
  required?: boolean
  children: ReactNode
  className?: string
}) {
  return (
    <div className={`space-y-1 ${className}`}>
      <div className="flex items-baseline justify-between gap-3">
        <label className="text-sm font-medium text-slate-200">
          {label}
          {required ? <span className="text-amber-300"> *</span> : null}
        </label>
      </div>
      {description ? <p className="text-xs text-[var(--color-text-muted)]">{description}</p> : null}
      <div>{children}</div>
      {error ? <p className="text-xs text-red-300">{error}</p> : null}
    </div>
  )
}

function TextField({
  label,
  description,
  value,
  onChange,
  error,
  required,
  placeholder,
  disabled,
  inputMode,
}: {
  label: string
  description?: string | null
  value: string
  onChange: (value: string) => void
  error?: string
  required?: boolean
  placeholder?: string
  disabled?: boolean
  inputMode?: React.HTMLAttributes<HTMLInputElement>['inputMode']
}) {
  return (
    <FieldRow label={label} description={description} error={error} required={required}>
      <input
        value={value}
        onChange={(event) => onChange(event.target.value)}
        placeholder={placeholder}
        disabled={disabled}
        inputMode={inputMode}
        className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 outline-none focus:border-sky-400"
      />
    </FieldRow>
  )
}

function TextareaField({
  label,
  description,
  value,
  onChange,
  error,
  required,
  placeholder,
  disabled,
  rows = 4,
}: {
  label: string
  description?: string | null
  value: string
  onChange: (value: string) => void
  error?: string
  required?: boolean
  placeholder?: string
  disabled?: boolean
  rows?: number
}) {
  return (
    <FieldRow label={label} description={description} error={error} required={required}>
      <textarea
        value={value}
        onChange={(event) => onChange(event.target.value)}
        placeholder={placeholder}
        disabled={disabled}
        rows={rows}
        className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 outline-none focus:border-sky-400"
      />
    </FieldRow>
  )
}

function CheckboxField({
  label,
  description,
  checked,
  onChange,
  disabled,
}: {
  label: string
  description?: string | null
  checked: boolean
  onChange: (value: boolean) => void
  disabled?: boolean
}) {
  return (
    <label className="flex cursor-pointer items-start gap-3 rounded-lg border border-slate-800 bg-slate-950/60 px-3 py-3">
      <input
        type="checkbox"
        checked={checked}
        onChange={(event) => onChange(event.target.checked)}
        disabled={disabled}
        className="mt-1 h-4 w-4 rounded border-slate-600 bg-slate-900 text-sky-500"
      />
      <span className="space-y-1">
        <span className="block text-sm font-medium text-slate-100">{label}</span>
        {description ? <span className="block text-xs text-[var(--color-text-muted)]">{description}</span> : null}
      </span>
    </label>
  )
}

function SelectField({
  label,
  description,
  value,
  onChange,
  options,
  error,
  required,
  emptyLabel = 'Select…',
  disabled,
}: {
  label: string
  description?: string | null
  value: string
  onChange: (value: string) => void
  options: PickerOption[]
  error?: string
  required?: boolean
  emptyLabel?: string
  disabled?: boolean
}) {
  return (
    <FieldRow label={label} description={description} error={error} required={required}>
      <ControlledSelect
        value={value}
        onChange={onChange}
        options={options}
        emptyLabel={emptyLabel}
        disabled={disabled}
        className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
      />
    </FieldRow>
  )
}

function PersonField({
  label,
  description,
  value,
  onChange,
  options,
  error,
  disabled,
}: {
  label: string
  description?: string | null
  value: string
  onChange: (value: string) => void
  options: PickerOption[]
  error?: string
  disabled?: boolean
}) {
  return (
    <FieldRow label={label} description={description} error={error}>
      <StaticSearchPicker
        value={value}
        onChange={onChange}
        options={options}
        disabled={disabled}
        placeholder="Search people..."
      />
    </FieldRow>
  )
}

function DateTimeField({
  label,
  description,
  value,
  onChange,
  error,
  required,
  disabled,
}: {
  label: string
  description?: string | null
  value: string
  onChange: (value: string) => void
  error?: string
  required?: boolean
  disabled?: boolean
}) {
  return (
    <FieldRow label={label} description={description} error={error} required={required}>
      <input
        type="datetime-local"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        disabled={disabled}
        className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 outline-none focus:border-sky-400"
      />
    </FieldRow>
  )
}

function SectionCard({
  label,
  summary,
  stateLabel,
  tone,
  icon,
  open,
  locked,
  onOpen,
  children,
  testId,
}: {
  label: string
  summary: string
  stateLabel: string
  tone: Tone
  icon: ReactNode
  open: boolean
  locked: boolean
  onOpen: () => void
  children: ReactNode
  testId: string
}) {
  const toneClasses: Record<Tone, string> = {
    good: 'border-emerald-500/40 bg-emerald-500/10 text-emerald-200',
    warn: 'border-amber-500/40 bg-amber-500/10 text-amber-200',
    bad: 'border-rose-500/40 bg-rose-500/10 text-rose-200',
    info: 'border-sky-500/40 bg-sky-500/10 text-sky-200',
    neutral: 'border-slate-700 bg-slate-900/60 text-slate-200',
  }

  return (
    <section
      data-testid={testId}
      className={`rounded-2xl border bg-slate-900/60 ${toneClasses[tone]}`}
    >
      <button
        type="button"
        onClick={onOpen}
        disabled={locked && !open}
        className="flex w-full items-start justify-between gap-4 px-5 py-4 text-left disabled:cursor-not-allowed disabled:opacity-60"
      >
        <div className="flex items-start gap-3">
          <span className="mt-0.5 flex h-8 w-8 items-center justify-center rounded-full border border-slate-700 bg-slate-950/80 text-slate-200">
            {icon}
          </span>
          <div>
            <div className="flex items-center gap-3">
              <h2 className="text-base font-semibold text-white">{label}</h2>
              <span className={`rounded-full border px-2 py-0.5 text-[11px] uppercase tracking-[0.18em] ${toneClasses[tone]}`}>
                {stateLabel}
              </span>
            </div>
            <p className="mt-1 text-sm text-slate-400">{summary}</p>
          </div>
        </div>
        {open ? <ChevronUp className="mt-1 h-4 w-4 shrink-0" /> : <ChevronDown className="mt-1 h-4 w-4 shrink-0" />}
      </button>
      {open ? <div className="border-t border-slate-800 px-5 py-5 text-slate-100">{children}</div> : null}
    </section>
  )
}

function buildSubmitRequest(
  values: DefectCreateValues,
  mode: 'plain' | 'work_order' | 'not_ready',
  defectId: string,
  personId: string,
): { payload: Record<string, unknown> } {
  const createWorkOrder = mode === 'work_order'
  const markAssetNotReady = mode === 'not_ready'
  const payload: Record<string, unknown> = {
    createWorkOrder,
    markAssetNotReady,
  }

  if (createWorkOrder) {
    payload.workOrderTitle = `Repair: ${trimToEmpty(values.title) || 'Defect'}`
    payload.workOrderDescription = values.description || values.title || 'Create follow-up work order from defect draft.'
    payload.workOrderPriority = values.priority || 'normal'
    payload.workOrderAssignedTechnicianPersonId = ''
    payload.workOrderDraftPlanJson = JSON.stringify({
      defectId,
      assetId: values.assetId,
      sourceType: values.sourceType,
      sourceReferenceId: values.sourceReferenceId,
      incidentReferenceId: values.incidentReferenceId,
    })
    payload.workOrderPlannedStartAt = null
    payload.workOrderPlannedDueAt = null
  }

  if (markAssetNotReady) {
    payload.holdType = 'defect_hold'
    payload.holdTitle = `Defect hold: ${trimToEmpty(values.title) || 'Unspecified defect'}`
    payload.holdDescription = values.readinessNotes || values.description || 'Asset placed on hold due to defect.'
    payload.holdSeverity = mapCatalogSeverityToBackend(values.severity)
    payload.holdSourceProduct = 'maintainarr'
    payload.holdSourceObjectRef = defectId
    payload.holdCreatedByPersonId = personId
  }

  return { payload }
}

function buildDraftRequest(values: DefectCreateValues): UpsertDefectDraftRequest {
  return {
    assetId: values.assetId,
    title: trimToEmpty(values.title) || null,
    description: trimToEmpty(values.description) || null,
    severity: mapCatalogSeverityToBackend(values.severity),
    priority: trimToEmpty(values.priority) || null,
    defectType: trimToEmpty(values.defectType) || null,
    reportSource: trimToEmpty(values.reportSource) || null,
    reportedAt: fromLocalDateTimeInput(values.reportedAt),
    discoveredAt: fromLocalDateTimeInput(values.discoveredAt),
    reportedByPersonId: trimToEmpty(values.reportedByPersonId) || null,
    discoveredByPersonId: trimToEmpty(values.discoveredByPersonId) || null,
    failureMode: trimToEmpty(values.failureMode) || null,
    systemKey: trimToEmpty(values.systemKey) || null,
    componentKey: trimToEmpty(values.componentKey) || null,
    symptom: trimToEmpty(values.symptom) || null,
    sidePosition: trimToEmpty(values.sidePosition) || null,
    operatingCondition: trimToEmpty(values.operatingCondition) || null,
    deferralCode: trimToEmpty(values.deferralCode) || null,
    isSafetyCritical: values.isSafetyCritical,
    isComplianceImpacting: values.isComplianceImpacting,
    isOperabilityImpacting: values.isOperabilityImpacting,
    readinessNotes: trimToEmpty(values.readinessNotes) || null,
    correctiveAction: trimToEmpty(values.correctiveAction) || null,
    sourceType: trimToEmpty(values.sourceType) || null,
    sourceReferenceId: trimToEmpty(values.sourceReferenceId) || null,
    incidentReferenceId: trimToEmpty(values.incidentReferenceId) || null,
  }
}

function summaryForSection(
  sectionKey: SectionKey,
  values: DefectCreateValues,
  selectedAsset: AssetResponse | undefined,
  selectedReadiness: AssetReadinessResponse | undefined,
  evidence: DefectEvidenceResponse[] | undefined,
  preview: DefectDraftPreviewResponse | undefined,
  peopleById: Map<string, string>,
  sourceSummary: string,
  sourceReason: string,
): string {
  switch (sectionKey) {
    case 'reporting':
      return [
        `Source: ${humanize(values.reportSource)}`,
        values.reportedByPersonId ? `Reporter: ${peopleById.get(values.reportedByPersonId) ?? values.reportedByPersonId}` : null,
        values.discoveredByPersonId ? `Discovered by: ${peopleById.get(values.discoveredByPersonId) ?? values.discoveredByPersonId}` : null,
        sourceSummary,
      ].filter(Boolean).join(' • ')
    case 'asset':
      return selectedAsset
        ? `${selectedAsset.assetTag} · ${selectedAsset.name} · ${humanize(selectedAsset.lifecycleStatus)}${
            selectedReadiness ? ` · Readiness: ${humanize(selectedReadiness.readinessStatus)}` : ''
          }`
        : 'Choose an asset to continue.'
    case 'details':
      return [
        values.title ? truncate(values.title, 60) : 'No title yet',
        values.defectType ? `Type: ${humanize(values.defectType)}` : null,
        values.priority ? `Priority: ${humanize(values.priority)}` : null,
      ].filter(Boolean).join(' • ')
    case 'safety':
      return [
        `Severity: ${humanize(values.severity)}`,
        values.isSafetyCritical ? 'Safety critical' : null,
        values.isComplianceImpacting ? 'Compliance impact' : null,
        values.isOperabilityImpacting ? 'Operability impact' : null,
      ].filter(Boolean).join(' • ')
    case 'classification':
      return [
        values.systemKey ? `System: ${humanize(values.systemKey)}` : null,
        values.componentKey ? `Component: ${humanize(values.componentKey)}` : null,
        values.failureMode ? `Failure mode: ${humanize(values.failureMode)}` : null,
        values.symptom ? `Symptom: ${humanize(values.symptom)}` : null,
      ].filter(Boolean).join(' • ') || 'No system classification captured yet.'
    case 'readiness':
      return [
        values.deferralCode ? `Deferral: ${humanize(values.deferralCode)}` : null,
        values.readinessNotes ? truncate(values.readinessNotes, 56) : null,
        selectedReadiness ? `Asset readiness: ${humanize(selectedReadiness.readinessStatus)}` : null,
      ].filter(Boolean).join(' • ') || 'Readiness and compliance notes are optional.'
    case 'evidence':
      return `${evidence?.length ?? 0} evidence item${(evidence?.length ?? 0) === 1 ? '' : 's'} attached.`
    case 'related':
      return [
        sourceReason || 'No source reference yet.',
        values.sourceType ? `Source type: ${humanize(values.sourceType)}` : null,
        values.sourceReferenceId ? `Reference: ${values.sourceReferenceId}` : null,
        preview?.duplicateMatches?.length ? `${preview.duplicateMatches.length} possible duplicate${preview.duplicateMatches.length === 1 ? '' : 's'}` : null,
      ].filter(Boolean).join(' • ')
    case 'corrective':
      return values.correctiveAction ? truncate(values.correctiveAction, 72) : 'No corrective action captured yet.'
    case 'review':
      if (!preview) return 'Save a draft to preview blockers and duplicates.'
      return preview.canSubmit
        ? `${preview.findings.length} finding${preview.findings.length === 1 ? '' : 's'} · Ready to submit`
        : `${preview.findings.filter((finding) => finding.severity === 'blocker').length} blocker${preview.findings.filter((finding) => finding.severity === 'blocker').length === 1 ? '' : 's'} · Review required`
    default:
      return ''
  }
}

function isSectionComplete(
  sectionKey: SectionKey,
  values: DefectCreateValues,
  localErrors: Record<string, string>,
  preview: DefectDraftPreviewResponse | undefined,
  evidence: DefectEvidenceResponse[] | undefined,
  selectedAsset: AssetResponse | undefined,
): boolean {
  switch (sectionKey) {
    case 'reporting':
      return !localErrors.reportSource && !localErrors.reportedByPersonId && !localErrors.discoveredByPersonId
    case 'asset':
      return Boolean(values.assetId && selectedAsset && selectedAsset.lifecycleStatus === 'active' && !localErrors.assetId)
    case 'details':
      return !localErrors.title && !localErrors.description && !localErrors.priority && !localErrors.defectType
    case 'safety':
      return !localErrors.severity
    case 'classification':
      return !localErrors.systemKey && !localErrors.componentKey && !localErrors.failureMode && !localErrors.symptom && !localErrors.sidePosition && !localErrors.operatingCondition
    case 'readiness':
      return !localErrors.deferralCode && !localErrors.readinessNotes
    case 'evidence':
      void (evidence?.length ?? 0)
      return true
    case 'related':
      return true
    case 'corrective':
      return !localErrors.correctiveAction
    case 'review':
      return Boolean(preview?.canSubmit && preview?.findings)
    default:
      return false
  }
}

export function DefectCreatePage() {
  const session = loadSession()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [searchParams] = useSearchParams()

  const initialDraftId = trimToEmpty(searchParams.get('defectId'))
  const [values, setValues] = useState<DefectCreateValues>(() =>
    buildInitialValues(session?.personId ?? '', searchParams),
  )
  const [draftId, setDraftId] = useState(initialDraftId)
  const [openSectionIndex, setOpenSectionIndex] = useState(0)
  const [serverError, setServerError] = useState<string | null>(null)
  const [evidenceFile, setEvidenceFile] = useState<File | null>(null)
  const [evidenceNotes, setEvidenceNotes] = useState('')
  const [draftSourceLoaded, setDraftSourceLoaded] = useState(false)
  const [previewRevision, setPreviewRevision] = useState(0)
  const autoDraftAssetRef = useRef<string>('')

  const meQuery = useQuery({
    queryKey: ['maintainarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const fieldsetQuery = useQuery({
    queryKey: ['maintainarr-fieldset-defects-create'],
    queryFn: () => getDefectCreateFieldset(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const catalogsQuery = useQuery({
    queryKey: ['maintainarr-defect-catalogs'],
    queryFn: () =>
      getCatalogs(session!.accessToken, [
        'reportSource',
        'defectType',
        'priority',
        'severity',
        'defectSystem',
        'defectComponent',
        'failureMode',
        'symptom',
        'sidePosition',
        'operatingCondition',
        'deferralCode',
      ]),
    enabled: Boolean(session?.accessToken),
  })

  const peopleQuery = useQuery({
    queryKey: ['maintainarr-people'],
    queryFn: () => getPeople(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const sitesQuery = useQuery({
    queryKey: ['maintainarr-sites'],
    queryFn: () => getSites(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const resumeDraftQuery = useQuery({
    queryKey: ['maintainarr-defect', initialDraftId],
    queryFn: () => getDefect(session!.accessToken, initialDraftId),
    enabled: Boolean(session?.accessToken && initialDraftId),
    retry: false,
  })

  const selectedAssetQuery = useQuery({
    queryKey: ['maintainarr-asset', values.assetId],
    queryFn: () => getAsset(session!.accessToken, values.assetId),
    enabled: Boolean(session?.accessToken && values.assetId),
  })

  const selectedAssetSearchQuery = useQuery({
    queryKey: ['maintainarr-asset-search-selected', values.assetId, selectedAssetQuery.data?.assetTag],
    queryFn: async () => {
      const searchKey = selectedAssetQuery.data?.assetTag || selectedAssetQuery.data?.name || values.assetId
      const results = await searchAssets(session!.accessToken, searchKey, 20)
      return results.find((item) => item.assetId === values.assetId) ?? null
    },
    enabled: Boolean(session?.accessToken && values.assetId && selectedAssetQuery.data),
  })

  const assetReadinessQuery = useQuery({
    queryKey: ['maintainarr-asset-readiness', values.assetId],
    queryFn: () => getAssetReadiness(session!.accessToken, values.assetId),
    enabled: Boolean(session?.accessToken && values.assetId),
  })

  const workOrderIdParam = trimToEmpty(searchParams.get('workOrderId'))
  const inspectionRunIdParam = trimToEmpty(searchParams.get('inspectionRunId'))
  const inspectionFindingIdParam = trimToEmpty(searchParams.get('inspectionFindingId'))
  const pmProgramIdParam = trimToEmpty(searchParams.get('pmProgramId'))
  const readinessIssueIdParam = trimToEmpty(searchParams.get('readinessIssueId'))

  const workOrderQuery = useQuery({
    queryKey: ['maintainarr-work-order', workOrderIdParam],
    queryFn: () => getWorkOrder(session!.accessToken, workOrderIdParam),
    enabled: Boolean(session?.accessToken && workOrderIdParam),
    retry: false,
  })

  const inspectionRunQuery = useQuery({
    queryKey: ['maintainarr-inspection-run', inspectionRunIdParam],
    queryFn: () => getInspectionRun(session!.accessToken, inspectionRunIdParam),
    enabled: Boolean(session?.accessToken && inspectionRunIdParam),
    retry: false,
  })

  const pmProgramQuery = useQuery({
    queryKey: ['maintainarr-pm-program', pmProgramIdParam],
    queryFn: () => getPmProgram(session!.accessToken, pmProgramIdParam),
    enabled: Boolean(session?.accessToken && pmProgramIdParam),
    retry: false,
  })

  const defectPreviewQuery = useQuery({
    queryKey: ['maintainarr-defect-preview', draftId, previewRevision],
    queryFn: () => previewDefectDraft(session!.accessToken, draftId),
    enabled: Boolean(session?.accessToken && draftId),
    retry: false,
  })

  const defectEvidenceQuery = useQuery({
    queryKey: ['maintainarr-defect-evidence', draftId],
    queryFn: () => getDefectEvidence(session!.accessToken, draftId),
    enabled: Boolean(session?.accessToken && draftId),
    retry: false,
  })

  const peopleById = useMemo(() => {
    const map = new Map<string, string>()
    for (const person of peopleQuery.data ?? []) {
      map.set(person.key, person.label)
    }
    return map
  }, [peopleQuery.data])

  const sitesById = useMemo(() => {
    const map = new Map<string, string>()
    for (const site of sitesQuery.data ?? []) {
      map.set(site.key, site.label)
    }
    return map
  }, [sitesQuery.data])

  const fieldset = fieldsetQuery.data
  const catalogs = catalogsQuery.data
  const selectedAsset = selectedAssetQuery.data
  const selectedAssetSummary = selectedAssetSearchQuery.data
  const selectedAssetReadiness = assetReadinessQuery.data
  const preview = defectPreviewQuery.data
  const evidence = defectEvidenceQuery.data ?? []

  useEffect(() => {
    if (!resumeDraftQuery.data) return
    if (resumeDraftQuery.data.status !== 'draft') {
      setServerError('The selected defect is already submitted. Open it from the defects detail page instead.')
      return
    }
    setValues(hydrateValuesFromDetail(resumeDraftQuery.data, session?.personId ?? ''))
    setDraftId(resumeDraftQuery.data.defectId)
    setDraftSourceLoaded(true)
  }, [resumeDraftQuery.data, session?.personId])

  useEffect(() => {
    if (!workOrderQuery.data) return
    setValues((current) =>
      mergePrefill(current, {
        assetId: workOrderQuery.data.assetId || current.assetId,
        title: current.title || workOrderQuery.data.title || workOrderQuery.data.defectTitle || '',
        description: current.description || workOrderQuery.data.description || '',
        reportSource: current.reportSource || 'work_order',
        sourceType: current.sourceType || 'work_order',
        sourceReferenceId: current.sourceReferenceId || workOrderQuery.data.workOrderId,
      }),
    )
  }, [workOrderQuery.data])

  useEffect(() => {
    if (!inspectionRunQuery.data) return
    const matchingFinding =
      inspectionFindingIdParam
        ? inspectionRunQuery.data.checklistItems.find(
            (item) =>
              item.checklistItemId === inspectionFindingIdParam
              || item.itemKey === inspectionFindingIdParam,
          )
        : null
    setValues((current) =>
      mergePrefill(current, {
        assetId: current.assetId || inspectionRunQuery.data.assetId,
        title:
          current.title
          || (matchingFinding ? `Inspection finding: ${matchingFinding.prompt}` : `Inspection defect on ${inspectionRunQuery.data.assetTag}`),
        description:
          current.description
          || (matchingFinding ? matchingFinding.prompt : `Defect discovered during inspection run ${inspectionRunQuery.data.inspectionRunId}.`),
        reportSource: current.reportSource || 'inspection',
        sourceType: current.sourceType || 'inspection_run',
        sourceReferenceId: current.sourceReferenceId || inspectionRunQuery.data.inspectionRunId,
      }),
    )
  }, [inspectionRunQuery.data, inspectionFindingIdParam])

  useEffect(() => {
    if (!pmProgramQuery.data) return
    setValues((current) =>
      mergePrefill(current, {
        assetId: current.assetId || pmProgramQuery.data.assetId || '',
        title: current.title || `PM follow-up for ${pmProgramQuery.data.name}`,
        description:
          current.description
          || pmProgramQuery.data.description
          || `Defect associated with preventive maintenance program ${pmProgramQuery.data.programKey}.`,
        reportSource: current.reportSource || 'pm_program',
        sourceType: current.sourceType || 'pm_program',
        sourceReferenceId: current.sourceReferenceId || pmProgramQuery.data.pmProgramId,
      }),
    )
  }, [pmProgramQuery.data])

  useEffect(() => {
    if (!readinessIssueIdParam) return
    setValues((current) =>
      mergePrefill(current, {
        reportSource: current.reportSource || 'readiness',
        sourceType: current.sourceType || 'readiness_issue',
        sourceReferenceId: current.sourceReferenceId || readinessIssueIdParam,
      }),
    )
  }, [readinessIssueIdParam])

  useEffect(() => {
    if (!draftSourceLoaded) return
    if (!selectedAssetQuery.data) return
    if (values.assetId === selectedAssetQuery.data.assetId) return
    setValues((current) => mergePrefill(current, { assetId: selectedAssetQuery.data!.assetId }))
  }, [selectedAssetQuery.data, draftSourceLoaded, values.assetId])

  const saveDraftMutation = useMutation({
    mutationFn: async () => {
      if (!values.assetId.trim()) {
        throw new Error('Select an asset before saving a draft.')
      }
      const payload = buildDraftRequest(values)
      if (draftId) {
        return updateDefectDraft(session!.accessToken, draftId, payload)
      }
      return createDefectDraft(session!.accessToken, payload)
    },
    onSuccess: (saved) => {
      setServerError(null)
      setDraftId(saved.defectId)
      setValues(hydrateValuesFromDetail(saved, session?.personId ?? ''))
      setPreviewRevision((current) => current + 1)
      queryClient.invalidateQueries({ queryKey: ['maintainarr-defects'] })
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to save defect draft')
    },
  })

  const submitMutation = useMutation({
    mutationFn: async (mode: 'plain' | 'work_order' | 'not_ready') => {
      const saved = draftId
        ? await updateDefectDraft(session!.accessToken, draftId, buildDraftRequest(values))
        : await createDefectDraft(session!.accessToken, buildDraftRequest(values))
      setDraftId(saved.defectId)
      const { payload } = buildSubmitRequest(values, mode, saved.defectId, session?.personId ?? '')
      return submitDefectDraft(session!.accessToken, saved.defectId, payload as never)
    },
    onSuccess: (result) => {
      setServerError(null)
      queryClient.invalidateQueries({ queryKey: ['maintainarr-defects'] })
      if (result.workOrder) {
        navigate(`/defects/details?defectId=${result.defect.defectId}&workOrderId=${result.workOrder.workOrderId}`)
        return
      }
      navigate(`/defects/details?defectId=${result.defect.defectId}`)
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to submit defect')
    },
  })

  const evidenceMutation = useMutation({
    mutationFn: async () => {
      if (!draftId) {
        throw new Error('Save the defect draft before uploading evidence.')
      }
      if (!evidenceFile) {
        throw new Error('Choose a file before uploading evidence.')
      }
      const buffer = await evidenceFile.arrayBuffer()
      const bytes = new Uint8Array(buffer)
      let binary = ''
      bytes.forEach((byte) => {
        binary += String.fromCharCode(byte)
      })
      const contentBase64 = btoa(binary)
      return uploadDefectEvidence(session!.accessToken, draftId, {
        evidenceTypeKey: 'defect_photo',
        fileName: evidenceFile.name,
        contentType: evidenceFile.type || 'application/octet-stream',
        contentBase64,
        notes: trimToEmpty(evidenceNotes) || null,
      })
    },
    onSuccess: () => {
      setServerError(null)
      setEvidenceFile(null)
      setEvidenceNotes('')
      queryClient.invalidateQueries({ queryKey: ['maintainarr-defect-evidence', draftId] })
      setPreviewRevision((current) => current + 1)
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to upload evidence')
    },
  })

  useEffect(() => {
    if (!session || !canCreateDefects(meQuery.data?.tenantRoleKey ?? '', meQuery.data?.isPlatformAdmin ?? false)) {
      return
    }
    if (draftId || !values.assetId || saveDraftMutation.isPending || initialDraftId) {
      return
    }
    if (autoDraftAssetRef.current === values.assetId) {
      return
    }
    autoDraftAssetRef.current = values.assetId
    void saveDraftMutation.mutateAsync()
  }, [
    session,
    draftId,
    values.assetId,
    saveDraftMutation,
    initialDraftId,
    meQuery.data?.tenantRoleKey,
    meQuery.data?.isPlatformAdmin,
  ])

  useEffect(() => {
    if (!fieldset) return
    const firstIncompleteIndex = SECTION_DEFINITIONS.findIndex((section) => {
      const sectionKeys =
        section.key === 'reporting'
          ? ['reportSource', 'reportedByPersonId', 'discoveredByPersonId']
          : section.key === 'asset'
            ? ['assetId']
            : section.key === 'details'
              ? ['title', 'description', 'priority', 'defectType']
              : section.key === 'safety'
                ? ['severity']
                : section.key === 'classification'
                  ? ['systemKey', 'componentKey', 'failureMode', 'symptom', 'sidePosition', 'operatingCondition']
                  : section.key === 'readiness'
                    ? ['deferralCode', 'readinessNotes']
                    : section.key === 'corrective'
                      ? ['correctiveAction']
                      : []
      const errors = fieldErrorMap(fieldset, values, sectionKeys)
      return !isSectionComplete(section.key, values, errors, preview, evidence, selectedAsset)
    })
    if (firstIncompleteIndex >= 0 && firstIncompleteIndex !== openSectionIndex) {
      setOpenSectionIndex(firstIncompleteIndex)
    }
  }, [fieldset, values, preview, evidence, selectedAsset, openSectionIndex])

  useEffect(() => {
    if (!session) return
    if (!fieldset) return
    const section = SECTION_DEFINITIONS[openSectionIndex]
    if (!section) return
    const sectionMap: Record<SectionKey, string[]> = {
      reporting: ['reportSource', 'reportedByPersonId', 'discoveredByPersonId'],
      asset: ['assetId'],
      details: ['title', 'description', 'priority', 'defectType'],
      safety: ['severity'],
      classification: ['systemKey', 'componentKey', 'failureMode', 'symptom', 'sidePosition', 'operatingCondition'],
      readiness: ['deferralCode', 'readinessNotes'],
      evidence: [],
      related: [],
      corrective: ['correctiveAction'],
      review: [],
    }
    const errors = fieldErrorMap(fieldset, values, sectionMap[section.key])
    const complete = isSectionComplete(section.key, values, errors, preview, evidence, selectedAsset)
    if (complete) {
      const nextUnlocked = SECTION_DEFINITIONS.findIndex(
        (candidate, index) =>
          index > openSectionIndex
          && isSectionComplete(
            candidate.key,
            values,
            fieldErrorMap(fieldset, values, sectionMap[candidate.key]),
            preview,
            evidence,
            selectedAsset,
          ),
      )
      if (nextUnlocked >= 0 && nextUnlocked !== openSectionIndex) {
        setOpenSectionIndex(nextUnlocked)
      }
    }
  }, [session, fieldset, values, openSectionIndex, preview, evidence, selectedAsset])

  if (!session) {
    return <Navigate to="/launch" replace />
  }

  const canCreate = meQuery.data
    ? canCreateDefects(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false
  const canSubmit = meQuery.data
    ? canSubmitDefects(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false
  const canCreateWorkOrder = meQuery.data
    ? canCreateWorkOrderFromDefect(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false
  const canMarkNotReady = meQuery.data
    ? canManageDefectReadiness(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false

  const fieldsetValidationErrors = useMemo(() => {
    if (!fieldset) return {}
    return validateAssetValues(
      fieldset,
      values as AssetFieldValues,
      fieldset.fields.filter((field) => DEFECT_FIELD_KEYS.has(field.key)) as FieldMetadataResponse[],
    )
  }, [fieldset, values])

  const manualErrors = useMemo(() => {
    const errors: Record<string, string> = { ...fieldsetValidationErrors }

    if (!trimToEmpty(values.assetId)) {
      errors.assetId = 'Asset is required.'
    }
    if (!trimToEmpty(values.title)) {
      errors.title = 'Defect title is required.'
    } else if (values.title.trim().length > 256) {
      errors.title = 'Defect title must be 256 characters or fewer.'
    }
    if (values.description.trim().length > 1024) {
      errors.description = 'Description must be 1,024 characters or fewer.'
    }
    if (values.sourceType.trim().length > 64) {
      errors.sourceType = 'Source type must be 64 characters or fewer.'
    }
    if (values.sourceReferenceId.trim().length > 128) {
      errors.sourceReferenceId = 'Source reference id must be 128 characters or fewer.'
    }
    if (values.incidentReferenceId.trim().length > 128) {
      errors.incidentReferenceId = 'Incident reference id must be 128 characters or fewer.'
    }
    if (values.readinessNotes.trim().length > 1024) {
      errors.readinessNotes = 'Readiness notes must be 1,024 characters or fewer.'
    }
    if (values.correctiveAction.trim().length > 1024) {
      errors.correctiveAction = 'Corrective action must be 1,024 characters or fewer.'
    }

    if (!fromLocalDateTimeInput(values.reportedAt)) {
      errors.reportedAt = 'Reported time must be a valid date/time.'
    }
    if (!fromLocalDateTimeInput(values.discoveredAt)) {
      errors.discoveredAt = 'Discovered time must be a valid date/time.'
    }

    return errors
  }, [values, fieldsetValidationErrors])

  const sectionErrors = {
    reporting: fieldErrorMap(fieldset, values, ['reportSource', 'reportedByPersonId', 'discoveredByPersonId']).reportSource
      ? [fieldErrorMap(fieldset, values, ['reportSource', 'reportedByPersonId', 'discoveredByPersonId']).reportSource]
      : [],
    asset: manualErrors.assetId ? [manualErrors.assetId] : [],
    details: [
      ...(manualErrors.title ? [manualErrors.title] : []),
      ...(manualErrors.description ? [manualErrors.description] : []),
      ...(fieldsetValidationErrors.priority ? [fieldsetValidationErrors.priority] : []),
      ...(fieldsetValidationErrors.defectType ? [fieldsetValidationErrors.defectType] : []),
    ],
    safety: [
      ...(fieldsetValidationErrors.severity ? [fieldsetValidationErrors.severity] : []),
    ],
    classification: ['systemKey', 'componentKey', 'failureMode', 'symptom', 'sidePosition', 'operatingCondition']
      .flatMap((key) => (fieldsetValidationErrors[key] ? [fieldsetValidationErrors[key]] : [])),
    readiness: [
      ...(fieldsetValidationErrors.deferralCode ? [fieldsetValidationErrors.deferralCode] : []),
      ...(manualErrors.readinessNotes ? [manualErrors.readinessNotes] : []),
    ],
    evidence: evidenceMutation.isPending ? ['Uploading evidence…'] : [],
    related: preview?.findings.filter((finding) => finding.severity === 'blocker').map((finding) => finding.message) ?? [],
    corrective: manualErrors.correctiveAction ? [manualErrors.correctiveAction] : [],
    review: preview ? preview.findings.map((finding) => finding.message) : ['Save a draft to load review data.'],
  } satisfies Record<SectionKey, string[]>

  const sectionComplete = {
    reporting: isSectionComplete('reporting', values, manualErrors, preview, evidence, selectedAsset),
    asset: isSectionComplete('asset', values, manualErrors, preview, evidence, selectedAsset),
    details: isSectionComplete('details', values, manualErrors, preview, evidence, selectedAsset),
    safety: isSectionComplete('safety', values, manualErrors, preview, evidence, selectedAsset),
    classification: isSectionComplete('classification', values, manualErrors, preview, evidence, selectedAsset),
    readiness: isSectionComplete('readiness', values, manualErrors, preview, evidence, selectedAsset),
    evidence: isSectionComplete('evidence', values, manualErrors, preview, evidence, selectedAsset),
    related: isSectionComplete('related', values, manualErrors, preview, evidence, selectedAsset),
    corrective: isSectionComplete('corrective', values, manualErrors, preview, evidence, selectedAsset),
    review: isSectionComplete('review', values, manualErrors, preview, evidence, selectedAsset),
  } satisfies Record<SectionKey, boolean>

  const sectionLocked = SECTION_DEFINITIONS.map((_, index) => {
    if (index === 0) return false
    return !SECTION_DEFINITIONS.slice(0, index).every((candidate) => sectionComplete[candidate.key])
  })

  const sectionSummaries = useMemo(() => {
    const sourceSummary = [
      values.sourceType ? `Source type: ${humanize(values.sourceType)}` : null,
      values.sourceReferenceId ? `Reference: ${values.sourceReferenceId}` : null,
      values.incidentReferenceId ? `Incident: ${values.incidentReferenceId}` : null,
    ]
      .filter(Boolean)
      .join(' • ')

    const sourceReason =
      values.sourceType && values.sourceReferenceId
        ? `${humanize(values.sourceType)} ${values.sourceReferenceId}`
        : values.sourceReferenceId || values.sourceType || 'No source reference yet'

    return SECTION_DEFINITIONS.map((section) =>
      summaryForSection(
        section.key,
        values,
        selectedAsset ?? undefined,
        selectedAssetReadiness ?? preview?.assetReadiness ?? undefined,
        evidence,
        preview ?? undefined,
        peopleById,
        sourceSummary,
        sourceReason,
      ),
    )
  }, [values, selectedAsset, selectedAssetReadiness, evidence, preview, peopleById])

  const sectionTones = SECTION_DEFINITIONS.map((section, index) =>
    classifyTone(sectionComplete[section.key], sectionLocked[index] ?? false, sectionErrors[section.key] ?? []),
  )

  const selectedAssetOption: PickerOption | undefined = selectedAsset
    ? {
        value: selectedAsset.assetId,
        label: `${selectedAsset.assetTag} · ${selectedAsset.name}`,
        inactive: selectedAsset.lifecycleStatus !== 'active',
      }
    : selectedAssetSummary
      ? {
          value: selectedAssetSummary.assetId,
          label: `${selectedAssetSummary.assetTag} · ${selectedAssetSummary.name}`,
          inactive: selectedAssetSummary.lifecycleStatus !== 'active',
        }
      : undefined

  const assetSearchQueryFn = async (query: string) => {
    const results = await searchAssets(session.accessToken, query, 25)
    return results.map((asset) => ({
      value: asset.assetId,
      label: `${asset.assetTag} · ${asset.name} · ${humanize(asset.lifecycleStatus)} · ${asset.openDefectCount} open defect${asset.openDefectCount === 1 ? '' : 's'} · ${asset.openWorkOrderCount} open work order${asset.openWorkOrderCount === 1 ? '' : 's'} · ${humanize(asset.readinessStatus)}`,
      inactive: asset.lifecycleStatus !== 'active',
      metadata: {
        assetTag: asset.assetTag,
        assetName: asset.name,
      },
    }))
  }

  const reportedByOptions = useMemo(() => mapReferenceOptions(peopleQuery.data), [peopleQuery.data])
  const discoveredByOptions = reportedByOptions
  const reportSourceField = fieldMeta(fieldset, 'reportSource')
  const priorityField = fieldMeta(fieldset, 'priority')
  const defectTypeField = fieldMeta(fieldset, 'defectType')
  const severityField = fieldMeta(fieldset, 'severity')
  const systemField = fieldMeta(fieldset, 'systemKey')
  const componentField = fieldMeta(fieldset, 'componentKey')
  const failureModeField = fieldMeta(fieldset, 'failureMode')
  const symptomField = fieldMeta(fieldset, 'symptom')
  const sidePositionField = fieldMeta(fieldset, 'sidePosition')
  const operatingConditionField = fieldMeta(fieldset, 'operatingCondition')
  const deferralCodeField = fieldMeta(fieldset, 'deferralCode')
  const readinessNotesField = fieldMeta(fieldset, 'readinessNotes')
  const correctiveActionField = fieldMeta(fieldset, 'correctiveAction')

  const sourceReason = useMemo(() => {
    if (workOrderQuery.data) return `Work order ${workOrderQuery.data.workOrderNumber}`
    if (inspectionRunQuery.data) return `Inspection run ${inspectionRunQuery.data.inspectionRunId}`
    if (pmProgramQuery.data) return `PM program ${pmProgramQuery.data.programKey}`
    if (values.sourceType && values.sourceReferenceId) return `${humanize(values.sourceType)} ${values.sourceReferenceId}`
    if (values.sourceType) return humanize(values.sourceType)
    return 'No source reference yet'
  }, [workOrderQuery.data, inspectionRunQuery.data, pmProgramQuery.data, values.sourceType, values.sourceReferenceId])

  const openSection = SECTION_DEFINITIONS[openSectionIndex] ?? SECTION_DEFINITIONS[0]

  const renderSelectField = (
    field: FieldMetadataResponse | undefined,
    value: string,
    onChange: (value: string) => void,
    fallbackLabel: string,
    options: PickerOption[],
    error?: string,
    emptyLabel?: string,
  ) => {
    if (!field) return null
    const filteredOptions = getFilteredOptions(fieldset as FieldsetResponse, field, values as AssetFieldValues)
    return (
      <SelectField
        label={field.label || fallbackLabel}
        description={field.description}
        value={value}
        onChange={onChange}
        options={filteredOptions.length > 0 ? mapFieldOptions(filteredOptions as unknown as CatalogOptionResponse[]) : options}
        error={error}
        required={field.required}
        emptyLabel={emptyLabel ?? 'Select…'}
      />
    )
  }

  const localSaveButtonLabel = draftId ? 'Update Draft' : 'Save Draft'
  const canSubmitActions = Boolean(preview?.canSubmit ?? sectionComplete.review)
  const blockerCount = preview?.findings.filter((finding) => finding.severity === 'blocker').length ?? 0
  const warningCount = preview?.findings.filter((finding) => finding.severity === 'warning').length ?? 0

  if (!canCreate && meQuery.isSuccess) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-8">
        <PageHeader title="Create Defect" subtitle="Guided MaintainArr defect intake" />
        <section className="rounded-2xl border border-slate-800 bg-slate-900/60 p-6">
          <h2 className="text-lg font-semibold text-white">Defect creation unavailable</h2>
          <p className="mt-2 text-sm text-slate-400">
            Your role can view defects but cannot create or submit new defect drafts.
          </p>
        </section>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-7xl space-y-6 px-4 py-6" data-testid="defect-create-page">
      <PageHeader title="Create Defect" subtitle="Guided MaintainArr defect intake" />

      <div className="flex flex-wrap items-center justify-between gap-3">
        <Link to="/defects/drawer" className="inline-flex items-center gap-2 text-sm text-slate-300 hover:text-white">
          <ArrowLeft className="h-4 w-4" />
          Back to defects
        </Link>
        <div className="rounded-full border border-slate-800 bg-slate-950 px-3 py-1 text-xs text-slate-400">
          {draftId ? `Draft ${draftId.slice(0, 8)} is active` : 'Draft row will be created after an asset is selected'}
        </div>
      </div>

      {meQuery.isLoading || fieldsetQuery.isLoading || catalogsQuery.isLoading ? (
        <section className="rounded-2xl border border-slate-800 bg-slate-900/60 p-6">
          <div className="flex items-center gap-3 text-sm text-slate-300">
            <Loader2 className="h-4 w-4 animate-spin" />
            Loading defect fieldset and catalogs...
          </div>
        </section>
      ) : null}

      {serverError ? (
        <section className="rounded-2xl border border-rose-800 bg-rose-950/40 p-4 text-sm text-rose-200">
          {serverError}
        </section>
      ) : null}

      {resumeDraftQuery.isError ? (
        <section className="rounded-2xl border border-amber-800 bg-amber-950/40 p-4 text-sm text-amber-100">
          The selected defect draft could not be loaded. A new draft can still be created from this page.
        </section>
      ) : null}

      <div className="grid gap-2 md:grid-cols-2 xl:grid-cols-5" aria-label="Defect create steps">
        {SECTION_DEFINITIONS.map((section, index) => {
          const complete = sectionComplete[section.key]
          const locked = sectionLocked[index] ?? false
          const stateLabel = locked ? 'Locked' : complete ? 'Complete' : section.key === 'review' ? 'Needs work' : 'In progress'
          return (
            <button
              key={section.key}
              type="button"
              disabled={locked && !complete}
              onClick={() => setOpenSectionIndex(index)}
              className={`rounded-xl border px-3 py-3 text-left text-sm ${
                openSection.key === section.key
                  ? 'border-sky-400 bg-sky-500/10 text-white'
                  : 'border-slate-800 bg-slate-950/70 text-slate-300'
              } disabled:cursor-not-allowed disabled:opacity-40`}
            >
              <span className="block font-medium">{section.label}</span>
              <span className="mt-1 block text-xs text-[var(--color-text-muted)]">{stateLabel}</span>
              <span className="mt-2 block text-xs text-slate-400">{sectionSummaries[index]}</span>
            </button>
          )
        })}
      </div>

      <div className="space-y-4">
        <SectionCard
          label={SECTION_DEFINITIONS[0].label}
          summary={sectionSummaries[0]}
          stateLabel={sectionLocked[0] ? 'Locked' : sectionComplete.reporting ? 'Complete' : 'In progress'}
          tone={sectionTones[0]}
          icon={SECTION_DEFINITIONS[0].icon}
          open={openSectionIndex === 0}
          locked={sectionLocked[0]}
          onOpen={() => setOpenSectionIndex(0)}
          testId="defect-section-reporting"
        >
          <div className="grid gap-4 md:grid-cols-2">
            <SelectField
              label={reportSourceField?.label ?? 'Report Source'}
              description={reportSourceField?.description}
              value={values.reportSource}
              onChange={(value) => setValues((current) => ({ ...current, reportSource: value }))}
              options={mapFieldOptions(catalogs?.find((catalog) => catalog.key === 'reportSource')?.options)}
              error={manualErrors.reportSource}
              required
            />
            <PersonField
              label="Reported By"
              description={fieldMeta(fieldset, 'reportedByPersonId')?.description ?? 'StaffArr person who reported the defect.'}
              value={values.reportedByPersonId}
              onChange={(value) => setValues((current) => ({ ...current, reportedByPersonId: value }))}
              options={reportedByOptions}
              error={manualErrors.reportedByPersonId}
            />
            <PersonField
              label="Discovered By"
              description={fieldMeta(fieldset, 'discoveredByPersonId')?.description ?? 'StaffArr person who discovered the defect.'}
              value={values.discoveredByPersonId}
              onChange={(value) => setValues((current) => ({ ...current, discoveredByPersonId: value }))}
              options={discoveredByOptions}
              error={manualErrors.discoveredByPersonId}
            />
            <DateTimeField
              label="Reported At"
              description="When the defect was reported."
              value={values.reportedAt}
              onChange={(value) => setValues((current) => ({ ...current, reportedAt: value }))}
              error={manualErrors.reportedAt}
            />
            <DateTimeField
              label="Discovered At"
              description="When the defect was first discovered."
              value={values.discoveredAt}
              onChange={(value) => setValues((current) => ({ ...current, discoveredAt: value }))}
              error={manualErrors.discoveredAt}
            />
          </div>
        </SectionCard>

        <SectionCard
          label={SECTION_DEFINITIONS[1].label}
          summary={sectionSummaries[1]}
          stateLabel={sectionLocked[1] ? 'Locked' : sectionComplete.asset ? 'Complete' : 'In progress'}
          tone={sectionTones[1]}
          icon={SECTION_DEFINITIONS[1].icon}
          open={openSectionIndex === 1}
          locked={sectionLocked[1]}
          onOpen={() => setOpenSectionIndex(1)}
          testId="defect-section-asset"
        >
          <div className="space-y-4">
            <AsyncSearchPicker
              value={values.assetId}
              onChange={(value) => setValues((current) => ({ ...current, assetId: value }))}
              queryKey={['maintainarr-assets-search']}
              queryFn={assetSearchQueryFn}
              selectedOption={selectedAssetOption}
              label="Asset"
              placeholder="Search asset number, name, VIN, model, site, or status..."
              minQueryLength={1}
            />
            {selectedAsset ? (
              <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-4">
                <AssetSummaryLine label="Selected" value={`${selectedAsset.assetTag} · ${selectedAsset.name}`} />
                <AssetSummaryLine label="Status" value={humanize(selectedAsset.lifecycleStatus)} />
                <AssetSummaryLine label="Open Defects" value={`${selectedAssetSummary?.openDefectCount ?? 0}`} />
                <AssetSummaryLine label="Open Work Orders" value={`${selectedAssetSummary?.openWorkOrderCount ?? 0}`} />
                <AssetSummaryLine
                  label="Readiness"
                  value={humanize(selectedAssetReadiness?.readinessStatus ?? preview?.assetReadiness?.readinessStatus ?? null)}
                />
                <AssetSummaryLine
                  label="Site"
                  value={
                    sitesById.get(selectedAsset.siteRef ?? '') ||
                    selectedAsset.siteRef ||
                    'Not recorded'
                  }
                />
              </div>
            ) : (
              <p className="rounded-lg border border-slate-800 bg-slate-950/60 p-4 text-sm text-slate-400">
                Search and select the asset this defect belongs to.
              </p>
            )}
            {manualErrors.assetId ? <p className="text-sm text-rose-300">{manualErrors.assetId}</p> : null}
          </div>
        </SectionCard>

        <SectionCard
          label={SECTION_DEFINITIONS[2].label}
          summary={sectionSummaries[2]}
          stateLabel={sectionLocked[2] ? 'Locked' : sectionComplete.details ? 'Complete' : 'In progress'}
          tone={sectionTones[2]}
          icon={SECTION_DEFINITIONS[2].icon}
          open={openSectionIndex === 2}
          locked={sectionLocked[2]}
          onOpen={() => setOpenSectionIndex(2)}
          testId="defect-section-details"
        >
          <div className="grid gap-4 md:grid-cols-2">
            <TextField
              label="Defect Title"
              description="Short summary of the problem."
              value={values.title}
              onChange={(value) => setValues((current) => ({ ...current, title: value }))}
              error={manualErrors.title}
              required
              placeholder="Example: Driver-side mirror cracked"
            />
            <SelectField
              label={priorityField?.label ?? 'Priority'}
              description={priorityField?.description}
              value={values.priority}
              onChange={(value) => setValues((current) => ({ ...current, priority: value }))}
              options={mapFieldOptions(catalogs?.find((catalog) => catalog.key === 'priority')?.options)}
              error={fieldsetValidationErrors.priority}
              required
            />
            <TextareaField
              label="Description"
              description="Add details, symptoms, and context."
              value={values.description}
              onChange={(value) => setValues((current) => ({ ...current, description: value }))}
              error={manualErrors.description}
              placeholder="Describe what you found and any immediate observations..."
              rows={5}
            />
            <SelectField
              label={defectTypeField?.label ?? 'Defect Type'}
              description={defectTypeField?.description}
              value={values.defectType}
              onChange={(value) => setValues((current) => ({ ...current, defectType: value }))}
              options={mapFieldOptions(catalogs?.find((catalog) => catalog.key === 'defectType')?.options)}
              error={fieldsetValidationErrors.defectType}
              required
            />
          </div>
        </SectionCard>

        <SectionCard
          label={SECTION_DEFINITIONS[3].label}
          summary={sectionSummaries[3]}
          stateLabel={sectionLocked[3] ? 'Locked' : sectionComplete.safety ? 'Complete' : 'In progress'}
          tone={sectionTones[3]}
          icon={SECTION_DEFINITIONS[3].icon}
          open={openSectionIndex === 3}
          locked={sectionLocked[3]}
          onOpen={() => setOpenSectionIndex(3)}
          testId="defect-section-safety"
        >
          <div className="grid gap-4 md:grid-cols-2">
            <SelectField
              label={severityField?.label ?? 'Severity'}
              description={severityField?.description}
              value={values.severity}
              onChange={(value) => setValues((current) => ({ ...current, severity: value }))}
              options={mapFieldOptions(catalogs?.find((catalog) => catalog.key === 'severity')?.options)}
              error={fieldsetValidationErrors.severity}
              required
            />
            <div className="grid gap-3">
              <CheckboxField
                label="Safety Critical"
                description="Check when the defect creates an immediate safety concern."
                checked={values.isSafetyCritical}
                onChange={(checked) => setValues((current) => ({ ...current, isSafetyCritical: checked }))}
              />
              <CheckboxField
                label="Compliance Impacting"
                description="Check when the defect affects compliance or regulatory readiness."
                checked={values.isComplianceImpacting}
                onChange={(checked) => setValues((current) => ({ ...current, isComplianceImpacting: checked }))}
              />
              <CheckboxField
                label="Operability Impacting"
                description="Check when the defect affects dispatch or normal use."
                checked={values.isOperabilityImpacting}
                onChange={(checked) => setValues((current) => ({ ...current, isOperabilityImpacting: checked }))}
              />
            </div>
          </div>
        </SectionCard>

        <SectionCard
          label={SECTION_DEFINITIONS[4].label}
          summary={sectionSummaries[4]}
          stateLabel={sectionLocked[4] ? 'Locked' : sectionComplete.classification ? 'Complete' : 'In progress'}
          tone={sectionTones[4]}
          icon={SECTION_DEFINITIONS[4].icon}
          open={openSectionIndex === 4}
          locked={sectionLocked[4]}
          onOpen={() => setOpenSectionIndex(4)}
          testId="defect-section-classification"
        >
          <div className="grid gap-4 md:grid-cols-2">
            {renderSelectField(
              systemField,
              values.systemKey,
              (value) => setValues((current) => ({ ...current, systemKey: value })),
              'System',
              mapFieldOptions(catalogs?.find((catalog) => catalog.key === 'defectSystem')?.options),
              manualErrors.systemKey,
            )}
            {renderSelectField(
              componentField,
              values.componentKey,
              (value) => setValues((current) => ({ ...current, componentKey: value })),
              'Component',
              mapFieldOptions(catalogs?.find((catalog) => catalog.key === 'defectComponent')?.options),
              manualErrors.componentKey,
            )}
            {renderSelectField(
              failureModeField,
              values.failureMode,
              (value) => setValues((current) => ({ ...current, failureMode: value })),
              'Failure Mode',
              mapFieldOptions(catalogs?.find((catalog) => catalog.key === 'failureMode')?.options),
              manualErrors.failureMode,
            )}
            {renderSelectField(
              symptomField,
              values.symptom,
              (value) => setValues((current) => ({ ...current, symptom: value })),
              'Symptom',
              mapFieldOptions(catalogs?.find((catalog) => catalog.key === 'symptom')?.options),
              manualErrors.symptom,
            )}
            {renderSelectField(
              sidePositionField,
              values.sidePosition,
              (value) => setValues((current) => ({ ...current, sidePosition: value })),
              'Side / Position',
              mapFieldOptions(catalogs?.find((catalog) => catalog.key === 'sidePosition')?.options),
              manualErrors.sidePosition,
            )}
            {renderSelectField(
              operatingConditionField,
              values.operatingCondition,
              (value) => setValues((current) => ({ ...current, operatingCondition: value })),
              'Operating Condition',
              mapFieldOptions(catalogs?.find((catalog) => catalog.key === 'operatingCondition')?.options),
              manualErrors.operatingCondition,
            )}
          </div>
        </SectionCard>

        <SectionCard
          label={SECTION_DEFINITIONS[5].label}
          summary={sectionSummaries[5]}
          stateLabel={sectionLocked[5] ? 'Locked' : sectionComplete.readiness ? 'Complete' : 'In progress'}
          tone={sectionTones[5]}
          icon={SECTION_DEFINITIONS[5].icon}
          open={openSectionIndex === 5}
          locked={sectionLocked[5]}
          onOpen={() => setOpenSectionIndex(5)}
          testId="defect-section-readiness"
        >
          <div className="grid gap-4 md:grid-cols-2">
            {renderSelectField(
              deferralCodeField,
              values.deferralCode,
              (value) => setValues((current) => ({ ...current, deferralCode: value })),
              'Deferral Code',
              mapFieldOptions(catalogs?.find((catalog) => catalog.key === 'deferralCode')?.options),
              manualErrors.deferralCode,
            )}
            <TextareaField
              label={readinessNotesField?.label ?? 'Readiness Notes'}
              description={readinessNotesField?.description}
              value={values.readinessNotes}
              onChange={(value) => setValues((current) => ({ ...current, readinessNotes: value }))}
              error={manualErrors.readinessNotes}
              placeholder="Explain why the defect impacts readiness or compliance."
              rows={4}
            />
          </div>
        </SectionCard>

        <SectionCard
          label={SECTION_DEFINITIONS[6].label}
          summary={sectionSummaries[6]}
          stateLabel={sectionLocked[6] ? 'Locked' : sectionComplete.evidence ? 'Complete' : 'Optional'}
          tone={sectionTones[6]}
          icon={SECTION_DEFINITIONS[6].icon}
          open={openSectionIndex === 6}
          locked={sectionLocked[6]}
          onOpen={() => setOpenSectionIndex(6)}
          testId="defect-section-evidence"
        >
          <div className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <FieldRow label="Attach Evidence" description="Upload a photo or document to the draft.">
                <input
                  type="file"
                  onChange={(event) => setEvidenceFile(event.target.files?.[0] ?? null)}
                  className="block w-full text-sm text-slate-300 file:mr-4 file:rounded-lg file:border-0 file:bg-slate-800 file:px-3 file:py-2 file:text-sm file:text-slate-100"
                />
              </FieldRow>
              <TextareaField
                label="Evidence Notes"
                description="Optional note for the uploaded file."
                value={evidenceNotes}
                onChange={setEvidenceNotes}
                placeholder="What should reviewers know about this attachment?"
                rows={3}
              />
            </div>
            <div className="flex flex-wrap items-center gap-2">
              <button
                type="button"
                onClick={() => evidenceMutation.mutate()}
                disabled={!draftId || !evidenceFile || evidenceMutation.isPending}
                className="inline-flex items-center gap-2 rounded-lg border border-slate-700 bg-slate-950 px-4 py-2 text-sm text-slate-100 disabled:cursor-not-allowed disabled:opacity-40"
              >
                {evidenceMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Upload className="h-4 w-4" />}
                Upload Evidence
              </button>
              {!draftId ? (
                <span className="text-sm text-slate-400">Save the draft first to enable uploads.</span>
              ) : null}
            </div>
            {defectEvidenceQuery.isLoading ? (
              <div className="flex items-center gap-2 text-sm text-slate-400">
                <Loader2 className="h-4 w-4 animate-spin" />
                Loading evidence...
              </div>
            ) : evidence.length > 0 ? (
              <div className="space-y-2">
                {evidence.map((item) => (
                  <div key={item.evidenceId} className="rounded-lg border border-slate-800 bg-slate-950/70 px-3 py-3">
                    <div className="flex flex-wrap items-center justify-between gap-2">
                      <div>
                        <p className="text-sm font-medium text-white">{item.fileName}</p>
                        <p className="text-xs text-[var(--color-text-muted)]">{item.contentType} · {Math.round(item.sizeBytes / 1024)} KB</p>
                      </div>
                      <span className="rounded-full border border-slate-700 px-2 py-0.5 text-[11px] uppercase tracking-[0.18em] text-slate-400">
                        {humanize(item.evidenceTypeKey)}
                      </span>
                    </div>
                    {item.notes ? <p className="mt-2 text-sm text-slate-300">{item.notes}</p> : null}
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-slate-400">No evidence has been uploaded to this draft yet.</p>
            )}
          </div>
        </SectionCard>

        <SectionCard
          label={SECTION_DEFINITIONS[7].label}
          summary={sectionSummaries[7]}
          stateLabel={sectionLocked[7] ? 'Locked' : 'Ready'}
          tone={sectionTones[7]}
          icon={SECTION_DEFINITIONS[7].icon}
          open={openSectionIndex === 7}
          locked={sectionLocked[7]}
          onOpen={() => setOpenSectionIndex(7)}
          testId="defect-section-related"
        >
          <div className="grid gap-4 md:grid-cols-2">
            <TextField
              label="Source Type"
              description="Optional source classification such as inspection_run, work_order, pm_program, or incident."
              value={values.sourceType}
              onChange={(value) => setValues((current) => ({ ...current, sourceType: value }))}
              placeholder="inspection_run"
            />
            <TextField
              label="Source Reference ID"
              description="External identifier for the source record."
              value={values.sourceReferenceId}
              onChange={(value) => setValues((current) => ({ ...current, sourceReferenceId: value }))}
              error={manualErrors.sourceReferenceId}
              placeholder="UUID or external source reference"
            />
            <TextField
              label="Incident Reference ID"
              description="Reference the external incident if this defect is incident related."
              value={values.incidentReferenceId}
              onChange={(value) => setValues((current) => ({ ...current, incidentReferenceId: value }))}
              error={manualErrors.incidentReferenceId}
              placeholder="External incident ID"
            />
            <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-4">
              <p className="text-sm font-medium text-white">Related Summary</p>
              <p className="mt-2 text-sm text-slate-400">{sourceReason}</p>
              {preview?.duplicateMatches?.length ? (
                <div className="mt-4 space-y-2">
                  {preview.duplicateMatches.map((match) => (
                    <div key={match.defectId} className="rounded-md border border-slate-800 bg-slate-950 px-3 py-2">
                      <p className="text-sm text-white">{match.title}</p>
                      <p className="text-xs text-[var(--color-text-muted)]">
                        {match.assetTag} · {humanize(match.severity)} · {match.matchReason} · {match.similarityScore}%
                      </p>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="mt-3 text-sm text-[var(--color-text-muted)]">Duplicate checks will appear after the draft is saved.</p>
              )}
            </div>
          </div>
        </SectionCard>

        <SectionCard
          label={SECTION_DEFINITIONS[8].label}
          summary={sectionSummaries[8]}
          stateLabel={sectionLocked[8] ? 'Locked' : sectionComplete.corrective ? 'Complete' : 'Optional'}
          tone={sectionTones[8]}
          icon={SECTION_DEFINITIONS[8].icon}
          open={openSectionIndex === 8}
          locked={sectionLocked[8]}
          onOpen={() => setOpenSectionIndex(8)}
          testId="defect-section-corrective"
        >
          <TextareaField
            label={correctiveActionField?.label ?? 'Corrective Action'}
            description={correctiveActionField?.description}
            value={values.correctiveAction}
            onChange={(value) => setValues((current) => ({ ...current, correctiveAction: value }))}
            error={manualErrors.correctiveAction}
            placeholder="Describe the fix, next step, or corrective recommendation."
            rows={5}
          />
        </SectionCard>

        <SectionCard
          label={SECTION_DEFINITIONS[9].label}
          summary={sectionSummaries[9]}
          stateLabel={sectionLocked[9] ? 'Locked' : preview?.canSubmit ? 'Ready' : 'Needs work'}
          tone={sectionTones[9]}
          icon={SECTION_DEFINITIONS[9].icon}
          open={openSectionIndex === 9}
          locked={sectionLocked[9]}
          onOpen={() => setOpenSectionIndex(9)}
          testId="defect-section-review"
        >
          <div className="space-y-5">
            {preview ? (
              <>
                <div className="grid gap-3 md:grid-cols-3">
                  <AssetSummaryLine label="Findings" value={`${preview.findings.length}`} />
                  <AssetSummaryLine label="Blockers" value={`${blockerCount}`} />
                  <AssetSummaryLine label="Warnings" value={`${warningCount}`} />
                </div>
                <div className="space-y-2">
                  {preview.findings.length === 0 ? (
                    <p className="rounded-lg border border-emerald-800 bg-emerald-950/30 p-3 text-sm text-emerald-100">
                      The draft is valid and ready to submit.
                    </p>
                  ) : (
                    preview.findings.map((finding, index) => (
                      <div
                        key={`${finding.code}-${index}`}
                        className={`rounded-lg border px-3 py-3 text-sm ${
                          finding.severity === 'blocker'
                            ? 'border-rose-800 bg-rose-950/40 text-rose-100'
                            : 'border-amber-800 bg-amber-950/40 text-amber-100'
                        }`}
                      >
                        <p className="font-medium">{finding.message}</p>
                        <p className="mt-1 text-xs uppercase tracking-[0.18em] text-slate-300">
                          {finding.category} · {finding.code}
                        </p>
                      </div>
                    ))
                  )}
                </div>
              </>
            ) : (
              <p className="rounded-lg border border-slate-800 bg-slate-950/70 p-4 text-sm text-slate-400">
                Save the draft to preview validation findings, readiness impact, and possible duplicates.
              </p>
            )}

            {preview?.assetReadiness ? (
              <div className="rounded-lg border border-slate-800 bg-slate-950/70 p-4">
                <h3 className="text-sm font-medium text-white">Asset Readiness Snapshot</h3>
                <p className="mt-1 text-sm text-slate-400">
                  Status: {humanize(preview.assetReadiness.readinessStatus)} · Basis: {humanize(preview.assetReadiness.readinessBasis)}
                </p>
                {preview.assetReadiness.blockers.length > 0 ? (
                  <div className="mt-3 space-y-2">
                    {preview.assetReadiness.blockers.map((blocker) => (
                      <div key={`${blocker.blockerType}-${blocker.sourceEntityId}`} className="rounded-md border border-slate-800 bg-slate-950 px-3 py-2 text-sm text-slate-300">
                        <p>{blocker.message}</p>
                      </div>
                    ))}
                  </div>
                ) : null}
              </div>
            ) : null}
          </div>
        </SectionCard>
      </div>

      <div className="sticky bottom-0 z-10 rounded-2xl border border-slate-800 bg-slate-950/95 p-4 shadow-2xl">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <div className="space-y-1 text-sm text-slate-400">
            <p>
              {draftId
                ? 'Draft is ready to update. Save before uploading evidence or submitting.'
                : 'Select an asset to create the canonical defect draft row.'}
            </p>
            {preview ? (
              <p>
                {preview.canSubmit ? 'Ready to submit.' : `Submit blocked by ${blockerCount} blocker${blockerCount === 1 ? '' : 's'}.`}
              </p>
            ) : (
              <p>Save the draft to preview duplicates and readiness impact.</p>
            )}
          </div>

          <div className="flex flex-wrap items-center gap-2">
            <button
              type="button"
              onClick={() => saveDraftMutation.mutate()}
              disabled={!values.assetId || saveDraftMutation.isPending}
              className="inline-flex items-center gap-2 rounded-lg border border-slate-700 bg-slate-900 px-4 py-2 text-sm text-slate-100 disabled:cursor-not-allowed disabled:opacity-40"
            >
              {saveDraftMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <Sparkles className="h-4 w-4" />}
              {localSaveButtonLabel}
            </button>

            <button
              type="button"
              onClick={() => submitMutation.mutate('plain')}
              disabled={!draftId || !canSubmit || !canSubmitActions || submitMutation.isPending || blockerCount > 0}
              className="inline-flex items-center gap-2 rounded-lg border border-sky-500 bg-sky-500/15 px-4 py-2 text-sm text-sky-100 disabled:cursor-not-allowed disabled:opacity-40"
            >
              {submitMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : <ArrowRight className="h-4 w-4" />}
              Submit
            </button>

            {canCreateWorkOrder ? (
              <button
                type="button"
                onClick={() => submitMutation.mutate('work_order')}
                disabled={!draftId || !canSubmit || submitMutation.isPending || blockerCount > 0}
                className="inline-flex items-center gap-2 rounded-lg border border-amber-500 bg-amber-500/15 px-4 py-2 text-sm text-amber-100 disabled:cursor-not-allowed disabled:opacity-40"
              >
                <Wrench className="h-4 w-4" />
                Submit + Work Order
              </button>
            ) : null}

            {canMarkNotReady ? (
              <button
                type="button"
                onClick={() => submitMutation.mutate('not_ready')}
                disabled={!draftId || !canSubmit || submitMutation.isPending || blockerCount > 0}
                className="inline-flex items-center gap-2 rounded-lg border border-rose-500 bg-rose-500/15 px-4 py-2 text-sm text-rose-100 disabled:cursor-not-allowed disabled:opacity-40"
              >
                <AlertTriangle className="h-4 w-4" />
                Submit + Not Ready
              </button>
            ) : null}
          </div>
        </div>
      </div>
    </div>
  )
}
