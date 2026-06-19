import { useEffect, useMemo, useRef, useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, Navigate, useSearchParams } from 'react-router-dom'
import {
  ArrowLeft,
  ArrowRight,
  BadgeCheck,
  CheckCircle2,
  ChevronDown,
  ChevronUp,
  CirclePlus,
  FileStack,
  Loader2,
  RefreshCw,
  Sparkles,
  Trash2,
  Wrench,
} from 'lucide-react'
import {
  CheckboxMultiSelect,
  ControlledSelect,
  DetailBadge,
  GeneratedKeyField,
  PageHeader,
  StaticSearchPicker,
  buildSemanticKey,
  type PickerOption,
} from '@stl/shared-ui'
import {
  cloneInspectionTemplate,
  createInspectionChecklistItem,
  createInspectionTemplate,
  createInspectionTemplateCategory,
  getAssetTypes,
  getCatalogs,
  getInspectionTemplate,
  getInspectionTemplateCreateFieldset,
  getInspectionTemplates,
  getMe,
  getPeople,
  getPmProgram,
  getSites,
  getTeams,
  previewInspectionTemplate,
  publishInspectionTemplate,
  replaceInspectionTemplateAssetTypes,
  retireInspectionTemplate,
  updateInspectionTemplate,
} from '../../api/client'
import type {
  AssetTypeResponse,
  CatalogResponse,
  FieldsetResponse,
  InspectionChecklistItemResponse,
  InspectionTemplateCategoryResponse,
  InspectionTemplateDetailResponse,
  InspectionTemplatePreviewResponse,
  PmProgramDetailResponse,
  ReferenceOptionResponse,
} from '../../api/types'
import {
  canCreateInspectionTemplates,
  canManageInspectionTemplates,
  canPreviewInspectionTemplates,
  canPublishInspectionTemplates,
  canRetireInspectionTemplates,
  loadSession,
} from '../../auth/sessionStorage'

type Tone = 'good' | 'warn' | 'bad' | 'info' | 'neutral'

type SectionKey = 'basics' | 'ownership' | 'scope' | 'settings' | 'structure' | 'review'

type TemplateFormValues = {
  templateKeyOverride: string
  name: string
  description: string
  templateCategoryKey: string
  inspectionType: string
  owningSiteRef: string
  owningTeamRef: string
  ownerPersonId: string
  ownerRoleKey: string
  estimatedDurationMinutes: string
  executionMode: string
  resultMode: string
  readinessImpact: string
  tagsText: string
  assetTypeIds: string[]
}

type CategoryDraftValues = {
  keyOverride: string
  name: string
  description: string
  isRequired: boolean
  canBeSkipped: boolean
  skipReasonRequired: boolean
  timingTracked: boolean
  sortOrder: string
}

type ItemDraftValues = {
  keyOverride: string
  prompt: string
  helpText: string
  itemType: string
  categoryId: string
  isRequired: boolean
  sortOrder: string
  controlledOptionsText: string
  acceptableRangeMin: string
  acceptableRangeMax: string
  unitOfMeasure: string
}

type TemplateSourceContext = {
  sourceTemplateId?: string
  sourceTemplateName?: string
  sourceTemplateKey?: string
  sourceTemplateStatus?: string
  pmProgramId?: string
  pmProgramName?: string
  pmProgramKey?: string
  pmProgramInspectionTemplateId?: string
  pmProgramInspectionTemplateName?: string
  pmProgramInspectionTemplateKey?: string
  assetTypeId?: string
  assetTypeName?: string
  assetTypeKey?: string
  assetCategory?: string
  assetCategoryLabel?: string
  complianceCatalogId?: string
  complianceCatalogLabel?: string
  inspectionTypeOverride?: string
}

interface SectionDefinition {
  key: SectionKey
  label: string
  description: string
  icon: ReactNode
}

const SECTION_DEFINITIONS: SectionDefinition[] = [
  {
    key: 'basics',
    label: 'Basics',
    description: 'Define the template identity and classification.',
    icon: <Sparkles className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'ownership',
    label: 'Ownership',
    description: 'Choose the StaffArr site, team, person, and role context.',
    icon: <BadgeCheck className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'scope',
    label: 'Scope',
    description: 'Link the template to asset types and source context.',
    icon: <RefreshCw className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'settings',
    label: 'Settings',
    description: 'Set execution, result, readiness, and tag context.',
    icon: <Wrench className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'structure',
    label: 'Structure',
    description: 'Add categories and checklist items before publish.',
    icon: <FileStack className="h-4 w-4" aria-hidden />,
  },
  {
    key: 'review',
    label: 'Review and Publish',
    description: 'Validate, preview, clone, publish, or retire.',
    icon: <CheckCircle2 className="h-4 w-4" aria-hidden />,
  },
]

const ITEM_TYPE_OPTIONS: PickerOption[] = [
  { value: 'pass_fail', label: 'Pass / fail' },
  { value: 'yes_no', label: 'Yes / no' },
  { value: 'numeric', label: 'Numeric' },
  { value: 'text', label: 'Text' },
  { value: 'select', label: 'Select' },
  { value: 'multi_select', label: 'Multi-select' },
  { value: 'photo', label: 'Photo' },
  { value: 'signature', label: 'Signature' },
  { value: 'meter_reading', label: 'Meter reading' },
]

const theme = {
  bg: '#0B1020',
  surface: '#111B33',
  primary: '#2F5DFF',
  secondary: '#14B8A6',
  accent: '#F59E0B',
  text: '#E5EAF5',
  muted: '#8FA2C2',
  border: '#263555',
} as const

const shellStyle = {
  background: `linear-gradient(180deg, ${theme.bg} 0%, #10193a 100%)`,
  color: theme.text,
}

const panelStyle = {
  backgroundColor: theme.surface,
  borderColor: theme.border,
}

function trimToEmpty(value: unknown): string {
  return typeof value === 'string' ? value.trim() : ''
}

function splitDelimitedList(value: string): string[] {
  return value
    .split(/[\n,;]+/)
    .map((part) => part.trim())
    .filter(Boolean)
}

function humanize(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  return value
    .replace(/[_-]+/g, ' ')
    .replace(/\b\w/g, (char) => char.toUpperCase())
}

function mapCatalogOptions(catalogs: CatalogResponse[] | undefined, key: string): PickerOption[] {
  const catalog = catalogs?.find((item) => item.key.toLowerCase() === key.toLowerCase())
  return (catalog?.options ?? [])
    .filter((option) => option.isActive)
    .map((option) => ({ value: option.key, label: option.label, inactive: !option.isActive }))
}

function mapReferenceOptions(items: ReferenceOptionResponse[] | undefined): PickerOption[] {
  return (items ?? []).map((item) => ({
    value: item.key,
    label: item.label,
    inactive: !item.isActive,
  }))
}

function mapAssetTypeOptions(items: AssetTypeResponse[] | undefined): PickerOption[] {
  return (items ?? []).map((item) => ({
    value: item.assetTypeId,
    label: `${item.name} (${item.typeKey})`,
    inactive: item.status.toLowerCase() !== 'active',
  }))
}

function toPickerOption(items: PickerOption[], value: string): PickerOption | undefined {
  return items.find((option) => option.value === value)
}

function sourceText(value: unknown): string {
  if (value == null) return ''
  if (typeof value === 'string') return value
  if (typeof value === 'number' || typeof value === 'boolean') return String(value)
  return ''
}

function settingsText(settings: Record<string, unknown> | null | undefined, key: string): string {
  return sourceText(settings?.[key])
}

function parsePositiveInteger(value: string): number | null {
  const trimmed = value.trim()
  if (!trimmed) return null
  const parsed = Number(trimmed)
  if (!Number.isFinite(parsed)) return null
  return Math.trunc(parsed)
}

function splitTemplateKey(value: string): string {
  return value.trim().toLowerCase()
}

export function shouldAutoAdvanceInspectionTemplateBasics(params: {
  name: string
  inspectionType: string
  effectiveTemplateKey: string
  fieldErrors: Record<string, string>
}): boolean {
  const nameLength = trimToEmpty(params.name).length

  // Keep the guided flow from jumping forward on a tiny accidental title.
  // The backend still accepts the minimum valid 2-character name.
  return Boolean(
    nameLength >= 3 &&
      trimToEmpty(params.inspectionType) &&
      trimToEmpty(params.effectiveTemplateKey) &&
      !params.fieldErrors.templateKeyOverride &&
      !params.fieldErrors.name &&
      !params.fieldErrors.inspectionType &&
      !params.fieldErrors.estimatedDurationMinutes,
  )
}

function buildSourceContextSettings(context: TemplateSourceContext): Record<string, unknown> {
  return {
    sourceContext: {
      sourceTemplateId: context.sourceTemplateId ?? null,
      sourceTemplateName: context.sourceTemplateName ?? null,
      sourceTemplateKey: context.sourceTemplateKey ?? null,
      sourceTemplateStatus: context.sourceTemplateStatus ?? null,
      pmProgramId: context.pmProgramId ?? null,
      pmProgramName: context.pmProgramName ?? null,
      pmProgramKey: context.pmProgramKey ?? null,
      pmProgramInspectionTemplateId: context.pmProgramInspectionTemplateId ?? null,
      pmProgramInspectionTemplateName: context.pmProgramInspectionTemplateName ?? null,
      pmProgramInspectionTemplateKey: context.pmProgramInspectionTemplateKey ?? null,
      assetTypeId: context.assetTypeId ?? null,
      assetTypeName: context.assetTypeName ?? null,
      assetTypeKey: context.assetTypeKey ?? null,
      assetCategory: context.assetCategory ?? null,
      assetCategoryLabel: context.assetCategoryLabel ?? null,
      complianceCatalogId: context.complianceCatalogId ?? null,
      complianceCatalogLabel: context.complianceCatalogLabel ?? null,
      inspectionTypeOverride: context.inspectionTypeOverride ?? null,
    },
  }
}

function createInitialValues(): TemplateFormValues {
  return {
    templateKeyOverride: '',
    name: '',
    description: '',
    templateCategoryKey: '',
    inspectionType: '',
    owningSiteRef: '',
    owningTeamRef: '',
    ownerPersonId: '',
    ownerRoleKey: '',
    estimatedDurationMinutes: '',
    executionMode: 'manual',
    resultMode: 'pass_fail',
    readinessImpact: 'none',
    tagsText: '',
    assetTypeIds: [],
  }
}

function createInitialCategoryDraft(): CategoryDraftValues {
  return {
    keyOverride: '',
    name: '',
    description: '',
    isRequired: false,
    canBeSkipped: true,
    skipReasonRequired: false,
    timingTracked: false,
    sortOrder: '10',
  }
}

function createInitialItemDraft(): ItemDraftValues {
  return {
    keyOverride: '',
    prompt: '',
    helpText: '',
    itemType: 'pass_fail',
    categoryId: '',
    isRequired: true,
    sortOrder: '10',
    controlledOptionsText: '',
    acceptableRangeMin: '',
    acceptableRangeMax: '',
    unitOfMeasure: '',
  }
}

function mergePrefill(current: TemplateFormValues, next: Partial<TemplateFormValues>): TemplateFormValues {
  const merged: TemplateFormValues = {
    ...current,
    ...next,
  }

  if (current.assetTypeIds.length > 0 && next.assetTypeIds && next.assetTypeIds.length === 0) {
    merged.assetTypeIds = current.assetTypeIds
  }

  return merged
}

function validateTemplateFieldset(fieldset: FieldsetResponse | undefined, values: TemplateFormValues): Record<string, string> {
  const errors: Record<string, string> = {}
  if (!fieldset) return errors

  for (const field of fieldset.fields) {
    const rawValue = (values as Record<string, unknown>)[field.key]
    const valueList = Array.isArray(rawValue)
      ? rawValue.map((item) => trimToEmpty(item)).filter(Boolean)
      : [trimToEmpty(rawValue)].filter(Boolean)

    if (field.required && valueList.length === 0) {
      errors[field.key] = `${field.label} is required.`
      continue
    }

    if (valueList.length === 0) {
      continue
    }

    const validation = field.validation ?? {}
    const maxLength = typeof validation.maxLength === 'number' ? validation.maxLength : null
    const minLength = typeof validation.minLength === 'number' ? validation.minLength : null
    const min = typeof validation.min === 'number' ? validation.min : null
    const max = typeof validation.max === 'number' ? validation.max : null
    const pattern = typeof validation.pattern === 'string' && validation.pattern ? new RegExp(validation.pattern) : null

    for (const value of valueList) {
      if (maxLength !== null && value.length > maxLength) {
        errors[field.key] = `${field.label} must be ${maxLength} characters or fewer.`
        break
      }
      if (minLength !== null && value.length < minLength) {
        errors[field.key] = `${field.label} must be at least ${minLength} characters.`
        break
      }
      if (pattern && !pattern.test(value)) {
        errors[field.key] = `${field.label} format is invalid.`
        break
      }
      if (field.key === 'estimatedDurationMinutes') {
        const parsed = Number(value)
        if (!Number.isFinite(parsed) || parsed < 0 || !Number.isInteger(parsed)) {
          errors[field.key] = `${field.label} must be a whole number.`
          break
        }
      }
      if ((field.control === 'select' || field.control === 'multiSelect') && field.options && field.options.length > 0) {
        const allowed = new Set(field.options.filter((option) => option.isActive).map((option) => option.key))
        if (!allowed.has(value)) {
          errors[field.key] = `${field.label} must be selected from the available options.`
          break
        }
      }
      if ((field.type === 'number' || field.type === 'integer' || field.control === 'number') && value) {
        const parsed = Number(value)
        if (!Number.isFinite(parsed)) {
          errors[field.key] = `${field.label} must be a number.`
          break
        }
        if (field.type === 'integer' && !Number.isInteger(parsed)) {
          errors[field.key] = `${field.label} must be a whole number.`
          break
        }
        if (min !== null && parsed < min) {
          errors[field.key] = `${field.label} must be at least ${min}.`
          break
        }
        if (max !== null && parsed > max) {
          errors[field.key] = `${field.label} must be at most ${max}.`
          break
        }
      }
    }
  }

  return errors
}

function validateCategoryDraft(values: CategoryDraftValues, existingKeys: string[]): Record<string, string> {
  const errors: Record<string, string> = {}
  if (!trimToEmpty(values.name)) {
    errors.name = 'Category name is required.'
  } else if (values.name.trim().length > 128) {
    errors.name = 'Category name must be 128 characters or fewer.'
  }

  const key = values.keyOverride.trim()
  if (!key) {
    errors.keyOverride = 'Category key is required.'
  } else if (!/^[a-z0-9._-]+$/.test(key)) {
    errors.keyOverride = 'Category key must use lowercase letters, numbers, dots, hyphens, or underscores.'
  } else if (existingKeys.includes(splitTemplateKey(key))) {
    errors.keyOverride = 'This category key already exists in this template.'
  }

  const sortOrder = parsePositiveInteger(values.sortOrder)
  if (sortOrder === null || sortOrder < 0) {
    errors.sortOrder = 'Sort order must be a whole number.'
  }

  if (values.description.trim().length > 512) {
    errors.description = 'Category description must be 512 characters or fewer.'
  }

  return errors
}

function validateItemDraft(values: ItemDraftValues, existingKeys: string[]): Record<string, string> {
  const errors: Record<string, string> = {}
  if (!trimToEmpty(values.prompt)) {
    errors.prompt = 'Checklist prompt is required.'
  } else if (values.prompt.trim().length > 512) {
    errors.prompt = 'Checklist prompt must be 512 characters or fewer.'
  }

  const key = values.keyOverride.trim()
  if (!key) {
    errors.keyOverride = 'Checklist item key is required.'
  } else if (!/^[a-z0-9._-]+$/.test(key)) {
    errors.keyOverride = 'Checklist item key must use lowercase letters, numbers, dots, hyphens, or underscores.'
  } else if (existingKeys.includes(splitTemplateKey(key))) {
    errors.keyOverride = 'This checklist item key already exists in this template.'
  }

  const sortOrder = parsePositiveInteger(values.sortOrder)
  if (sortOrder === null || sortOrder < 0) {
    errors.sortOrder = 'Sort order must be a whole number.'
  }

  if ((values.itemType === 'select' || values.itemType === 'multi_select') && !trimToEmpty(values.controlledOptionsText)) {
    errors.controlledOptionsText = 'Controlled options are required for select-based checklist items.'
  }
  if (values.itemType === 'meter_reading' && !trimToEmpty(values.unitOfMeasure)) {
    errors.unitOfMeasure = 'Unit of measure is required for meter-reading items.'
  }
  if (values.acceptableRangeMin.trim() && Number.isNaN(Number(values.acceptableRangeMin))) {
    errors.acceptableRangeMin = 'Minimum must be numeric.'
  }
  if (values.acceptableRangeMax.trim() && Number.isNaN(Number(values.acceptableRangeMax))) {
    errors.acceptableRangeMax = 'Maximum must be numeric.'
  }
  if (values.helpText.trim().length > 512) {
    errors.helpText = 'Help text must be 512 characters or fewer.'
  }

  return errors
}

function sectionSummary(
  key: SectionKey,
  values: TemplateFormValues,
  currentTemplate: InspectionTemplateDetailResponse | null,
  sourceContext: TemplateSourceContext,
  preview: InspectionTemplatePreviewResponse | null,
): string {
  switch (key) {
    case 'basics':
      return [
        trimToEmpty(values.name) || 'Template name required',
        trimToEmpty(values.inspectionType) ? humanize(values.inspectionType) : 'Inspection type required',
      ].join(' · ')
    case 'ownership':
      return [
        trimToEmpty(values.owningSiteRef) ? sourceContext.assetTypeName || 'Site selected' : 'No site selected',
        trimToEmpty(values.owningTeamRef) ? 'Team selected' : 'No team selected',
        trimToEmpty(values.ownerPersonId) ? 'Owner selected' : 'No owner selected',
      ].join(' · ')
    case 'scope':
      return values.assetTypeIds.length > 0
        ? `${values.assetTypeIds.length} asset type${values.assetTypeIds.length === 1 ? '' : 's'} selected`
        : sourceContext.sourceTemplateName
          ? `Prefilled from ${sourceContext.sourceTemplateName}`
          : 'No asset types selected'
    case 'settings':
      return [
        humanize(values.executionMode),
        humanize(values.resultMode),
        humanize(values.readinessImpact),
      ].join(' · ')
    case 'structure':
      if (!currentTemplate) {
        return 'Save the draft to add categories and checklist items.'
      }
      return `${currentTemplate.categories.length} categories · ${currentTemplate.checklistItems.length} checklist items`
    case 'review':
      if (!preview) {
        return 'Run Preview to see validation issues and compatible assets.'
      }
      return preview.validation.issues.length > 0
        ? `${preview.validation.issues.filter((issue) => issue.isBlocking).length} blocking issue(s) · ${preview.assets.compatibleCount} compatible assets`
        : `${preview.assets.compatibleCount} compatible assets · Ready for publish review`
  }
}

function labelForSectionState(
  key: SectionKey,
  values: TemplateFormValues,
  fieldErrors: Record<string, string>,
  currentTemplate: InspectionTemplateDetailResponse | null,
  preview: InspectionTemplatePreviewResponse | null,
  canPublish: boolean,
): { label: string; tone: Tone } {
  const sectionKeys: Record<SectionKey, string[]> = {
    basics: ['templateKeyOverride', 'name', 'description', 'templateCategoryKey', 'inspectionType', 'estimatedDurationMinutes'],
    ownership: ['owningSiteRef', 'owningTeamRef', 'ownerPersonId', 'ownerRoleKey'],
    scope: ['assetTypeIds'],
    settings: ['executionMode', 'resultMode', 'readinessImpact', 'tagsText'],
    structure: [],
    review: [],
  }

  const hasSectionErrors = sectionKeys[key].some((fieldKey) => Boolean(fieldErrors[fieldKey]))
  if (hasSectionErrors) {
    return { label: 'Needs required fields', tone: 'warn' }
  }

  switch (key) {
    case 'basics':
      return trimToEmpty(values.name) && trimToEmpty(values.inspectionType)
        ? { label: 'Complete', tone: 'good' }
        : { label: 'Not started', tone: 'neutral' }
    case 'ownership':
      return {
        label: trimToEmpty(values.owningSiteRef) || trimToEmpty(values.owningTeamRef) || trimToEmpty(values.ownerPersonId) ? 'Complete' : 'Optional',
        tone: trimToEmpty(values.owningSiteRef) || trimToEmpty(values.owningTeamRef) || trimToEmpty(values.ownerPersonId) ? 'good' : 'neutral',
      }
    case 'scope':
      return values.assetTypeIds.length > 0
        ? { label: 'Complete', tone: 'good' }
        : { label: 'Optional', tone: 'neutral' }
    case 'settings':
      return trimToEmpty(values.executionMode) || trimToEmpty(values.resultMode) || trimToEmpty(values.readinessImpact) || trimToEmpty(values.tagsText)
        ? { label: 'Complete', tone: 'good' }
        : { label: 'Optional', tone: 'neutral' }
    case 'structure':
      return currentTemplate && currentTemplate.checklistItems.length > 0
        ? { label: 'Complete', tone: 'good' }
        : { label: 'Needs review', tone: 'warn' }
    case 'review':
      if (!preview) {
        return canPublish ? { label: 'Needs preview', tone: 'warn' } : { label: 'Locked', tone: 'neutral' }
      }
      return preview.validation.issues.some((issue) => issue.isBlocking)
        ? { label: 'Needs review', tone: 'bad' }
        : { label: 'Ready to publish', tone: 'good' }
  }
}

function SectionCard({
  title,
  summary,
  stateLabel,
  tone,
  expanded,
  locked,
  icon,
  onToggle,
  children,
  footer,
  testId,
}: {
  title: string
  summary: string
  stateLabel: string
  tone: Tone
  expanded: boolean
  locked: boolean
  icon: ReactNode
  onToggle: () => void
  children: ReactNode
  footer?: ReactNode
  testId: string
}) {
  return (
    <section
      className="overflow-hidden rounded-3xl border shadow-2xl"
      style={panelStyle}
      data-testid={testId}
    >
      <button
        type="button"
        className={`flex w-full items-start justify-between gap-4 px-5 py-4 text-left transition ${
          locked ? 'cursor-not-allowed opacity-70' : 'hover:bg-white/5'
        }`}
        disabled={locked}
        onClick={onToggle}
      >
        <div className="min-w-0">
          <div className="flex items-center gap-3">
            <div
              className="flex h-11 w-11 shrink-0 items-center justify-center rounded-2xl border"
              style={{ borderColor: theme.border, backgroundColor: '#0F1730', color: theme.secondary }}
            >
              {icon}
            </div>
            <div className="min-w-0">
              <div className="flex flex-wrap items-center gap-2">
                <h2 className="text-base font-semibold text-white">{title}</h2>
                <DetailBadge label={stateLabel} tone={tone} />
              </div>
              <p className="mt-1 text-sm text-slate-300">{summary}</p>
            </div>
          </div>
        </div>
        <div className="mt-1 text-slate-300">{expanded ? <ChevronUp className="h-5 w-5" /> : <ChevronDown className="h-5 w-5" />}</div>
      </button>

      {expanded ? (
        <div className="border-t px-5 py-5" style={{ borderColor: theme.border }}>
          {children}
          {footer ? <div className="mt-5">{footer}</div> : null}
        </div>
      ) : null}
    </section>
  )
}

function sourceTemplateLabel(template: InspectionTemplateDetailResponse | null | undefined): string {
  if (!template) return 'Not sourced from another template'
  return `${template.name} (${template.templateKey})`
}

function sourceProgramLabel(program: PmProgramDetailResponse | null | undefined): string {
  if (!program) return 'Not sourced from a PM program'
  return `${program.name} (${program.programKey})`
}

export function InspectionTemplateCreatePage() {
  const session = loadSession()
  const queryClient = useQueryClient()
  const [searchParams, setSearchParams] = useSearchParams()

  const explicitTemplateId = trimToEmpty(searchParams.get('templateId'))
  const cloneTemplateId = trimToEmpty(searchParams.get('cloneTemplateId'))
  const sourceTemplateId = trimToEmpty(searchParams.get('sourceTemplateId'))
  const pmProgramId = trimToEmpty(searchParams.get('pmProgramId'))
  const complianceCatalogId = trimToEmpty(searchParams.get('complianceCatalogId'))
  const assetTypeParam = trimToEmpty(searchParams.get('assetType'))
  const assetCategoryParam = trimToEmpty(searchParams.get('assetCategory'))
  const inspectionTypeParam = trimToEmpty(searchParams.get('inspectionType'))

  const [currentTemplateId, setCurrentTemplateId] = useState(explicitTemplateId)
  const [templateStatus, setTemplateStatus] = useState('draft')
  const [values, setValues] = useState<TemplateFormValues>(createInitialValues)
  const [categoryDraft, setCategoryDraft] = useState<CategoryDraftValues>(createInitialCategoryDraft)
  const [itemDraft, setItemDraft] = useState<ItemDraftValues>(createInitialItemDraft)
  const [serverError, setServerError] = useState<string | null>(null)
  const [previewResult, setPreviewResult] = useState<InspectionTemplatePreviewResponse | null>(null)
  const [previewMessage, setPreviewMessage] = useState<string | null>(null)
  const [retireReason, setRetireReason] = useState('')
  const [confirmComplianceRelated, setConfirmComplianceRelated] = useState(false)
  const [confirmReadinessImpact, setConfirmReadinessImpact] = useState(false)
  const [confirmFailureAutomation, setConfirmFailureAutomation] = useState(false)
  const [confirmSupervisorRelease, setConfirmSupervisorRelease] = useState(false)
  const [openSectionIndex, setOpenSectionIndex] = useState(0)
  const [baselineSettings, setBaselineSettings] = useState<Record<string, unknown>>({})

  const currentTemplateIdRef = useRef(currentTemplateId)
  currentTemplateIdRef.current = currentTemplateId

  const meQuery = useQuery({
    queryKey: ['maintainarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const fieldsetQuery = useQuery({
    queryKey: ['maintainarr-fieldset-inspection-templates-create'],
    queryFn: () => getInspectionTemplateCreateFieldset(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const templatesQuery = useQuery({
    queryKey: ['maintainarr-inspection-templates'],
    queryFn: () => getInspectionTemplates(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const catalogsQuery = useQuery({
    queryKey: ['maintainarr-inspection-template-catalogs'],
    queryFn: () =>
      getCatalogs(session!.accessToken, [
        'inspectionTemplateCategory',
        'inspectionTemplateOwnerRole',
        'inspectionExecutionMode',
        'inspectionResultMode',
        'inspectionReadinessImpact',
        'inspectionType',
        'assetCategory',
      ]),
    enabled: Boolean(session?.accessToken),
  })

  const assetTypesQuery = useQuery({
    queryKey: ['maintainarr-asset-types'],
    queryFn: () => getAssetTypes(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const sitesQuery = useQuery({
    queryKey: ['maintainarr-sites'],
    queryFn: () => getSites(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const teamsQuery = useQuery({
    queryKey: ['maintainarr-teams'],
    queryFn: () => getTeams(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const peopleQuery = useQuery({
    queryKey: ['maintainarr-people'],
    queryFn: () => getPeople(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const pmProgramQuery = useQuery({
    queryKey: ['maintainarr-pm-program', pmProgramId],
    queryFn: () => getPmProgram(session!.accessToken, pmProgramId),
    enabled: Boolean(session?.accessToken && pmProgramId),
    retry: false,
  })

  const templateQuery = useQuery({
    queryKey: ['maintainarr-inspection-template', currentTemplateId],
    queryFn: () => getInspectionTemplate(session!.accessToken, currentTemplateId),
    enabled: Boolean(session?.accessToken && currentTemplateId),
    retry: false,
  })

  const sourceTemplateResolvedId = sourceTemplateId || cloneTemplateId || pmProgramQuery.data?.inspectionTemplateId || ''
  const sourceTemplateQuery = useQuery({
    queryKey: ['maintainarr-inspection-template-source', sourceTemplateResolvedId],
    queryFn: () => getInspectionTemplate(session!.accessToken, sourceTemplateResolvedId),
    enabled: Boolean(session?.accessToken && !currentTemplateId && sourceTemplateResolvedId),
    retry: false,
  })

  const siteOptions = useMemo(() => mapReferenceOptions(sitesQuery.data), [sitesQuery.data])
  const teamOptions = useMemo(() => mapReferenceOptions(teamsQuery.data), [teamsQuery.data])
  const personOptions = useMemo(() => mapReferenceOptions(peopleQuery.data), [peopleQuery.data])
  const categoryOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'inspectionTemplateCategory'), [catalogsQuery.data])
  const roleOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'inspectionTemplateOwnerRole'), [catalogsQuery.data])
  const executionModeOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'inspectionExecutionMode'), [catalogsQuery.data])
  const resultModeOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'inspectionResultMode'), [catalogsQuery.data])
  const readinessImpactOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'inspectionReadinessImpact'), [catalogsQuery.data])
  const inspectionTypeOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'inspectionType'), [catalogsQuery.data])
  const assetCategoryOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'assetCategory'), [catalogsQuery.data])
  const assetTypeOptions = useMemo(() => mapAssetTypeOptions(assetTypesQuery.data), [assetTypesQuery.data])
  const existingTemplateSummaries = templatesQuery.data ?? []
  const currentTemplate = templateQuery.data ?? null
  const sourceTemplate = sourceTemplateQuery.data ?? null
  const pmProgram = pmProgramQuery.data ?? null
  const currentTemplateStatusValue = currentTemplate?.status ?? templateStatus

  useEffect(() => {
    if (!explicitTemplateId) {
      return
    }
    setCurrentTemplateId(explicitTemplateId)
  }, [explicitTemplateId])

  useEffect(() => {
    if (!currentTemplate) {
      return
    }
    setTemplateStatus(currentTemplate.status)
    setBaselineSettings(currentTemplate.settings ?? {})
    setValues({
      templateKeyOverride: currentTemplate.templateKey,
      name: currentTemplate.name,
      description: currentTemplate.description ?? '',
      templateCategoryKey: currentTemplate.templateCategoryKey ?? '',
      inspectionType: currentTemplate.inspectionType ?? '',
      owningSiteRef: currentTemplate.owningSiteRef ?? '',
      owningTeamRef: currentTemplate.owningTeamRef ?? '',
      ownerPersonId: currentTemplate.ownerPersonId ?? '',
      ownerRoleKey: currentTemplate.ownerRoleKey ?? '',
      estimatedDurationMinutes: currentTemplate.estimatedDurationMinutes != null ? String(currentTemplate.estimatedDurationMinutes) : '',
      executionMode: settingsText(currentTemplate.settings, 'executionMode') || 'manual',
      resultMode: settingsText(currentTemplate.settings, 'resultMode') || 'pass_fail',
      readinessImpact: settingsText(currentTemplate.settings, 'readinessImpact') || 'none',
      tagsText: currentTemplate.tags?.join(', ') ?? '',
      assetTypeIds: currentTemplate.linkedAssetTypes.map((link) => link.assetTypeId),
    })
    setPreviewResult(null)
  }, [currentTemplate])

  useEffect(() => {
    if (currentTemplateId || !sourceTemplate) {
      return
    }

    setTemplateStatus(sourceTemplate.status)
    setBaselineSettings(sourceTemplate.settings ?? {})
    setValues((current) =>
      mergePrefill(current, {
        templateKeyOverride: '',
        name: trimToEmpty(current.name) || `${sourceTemplate.name} Copy`,
        description: trimToEmpty(current.description) || sourceTemplate.description || '',
        templateCategoryKey: trimToEmpty(current.templateCategoryKey) || trimToEmpty(sourceTemplate.templateCategoryKey),
        inspectionType: trimToEmpty(current.inspectionType) || trimToEmpty(sourceTemplate.inspectionType),
        owningSiteRef: trimToEmpty(current.owningSiteRef) || trimToEmpty(sourceTemplate.owningSiteRef),
        owningTeamRef: trimToEmpty(current.owningTeamRef) || trimToEmpty(sourceTemplate.owningTeamRef),
        ownerPersonId: trimToEmpty(current.ownerPersonId) || trimToEmpty(sourceTemplate.ownerPersonId),
        ownerRoleKey: trimToEmpty(current.ownerRoleKey) || trimToEmpty(sourceTemplate.ownerRoleKey),
        estimatedDurationMinutes:
          trimToEmpty(current.estimatedDurationMinutes) || (sourceTemplate.estimatedDurationMinutes != null ? String(sourceTemplate.estimatedDurationMinutes) : ''),
        executionMode: trimToEmpty(current.executionMode) || settingsText(sourceTemplate.settings, 'executionMode') || 'manual',
        resultMode: trimToEmpty(current.resultMode) || settingsText(sourceTemplate.settings, 'resultMode') || 'pass_fail',
        readinessImpact: trimToEmpty(current.readinessImpact) || settingsText(sourceTemplate.settings, 'readinessImpact') || 'none',
        tagsText: trimToEmpty(current.tagsText) || (sourceTemplate.tags?.join(', ') ?? ''),
        assetTypeIds: current.assetTypeIds.length > 0 ? current.assetTypeIds : sourceTemplate.linkedAssetTypes.map((link) => link.assetTypeId),
      }),
    )
    setPreviewResult(null)
  }, [sourceTemplate, currentTemplateId])

  useEffect(() => {
    if (currentTemplateId || !pmProgram) {
      return
    }

    setValues((current) =>
      mergePrefill(current, {
        name: trimToEmpty(current.name) || `${pmProgram.name} Inspection Template`,
        description:
          trimToEmpty(current.description) ||
          `Inspection template associated with PM program ${pmProgram.name}.`,
        owningSiteRef: trimToEmpty(current.owningSiteRef) || trimToEmpty(pmProgram.owningSiteRef),
        owningTeamRef: trimToEmpty(current.owningTeamRef) || trimToEmpty(pmProgram.owningTeamRef),
        ownerPersonId: trimToEmpty(current.ownerPersonId) || trimToEmpty(pmProgram.ownerPersonId),
        ownerRoleKey: trimToEmpty(current.ownerRoleKey) || trimToEmpty(pmProgram.ownerRoleKey),
        tagsText: trimToEmpty(current.tagsText) || (pmProgram.tags?.join(', ') ?? ''),
        assetTypeIds:
          current.assetTypeIds.length > 0
            ? current.assetTypeIds
            : pmProgram.scopeType === 'asset_type' && pmProgram.assetTypeId
              ? [pmProgram.assetTypeId]
              : [],
      }),
    )
    setPreviewResult(null)
  }, [pmProgram, currentTemplateId])

  useEffect(() => {
    if (currentTemplateId) {
      return
    }

    setValues((current) =>
      mergePrefill(current, {
        inspectionType: trimToEmpty(current.inspectionType) || inspectionTypeParam,
      }),
    )
    setPreviewResult(null)
  }, [inspectionTypeParam, currentTemplateId])

  useEffect(() => {
    if (currentTemplateId || !assetTypeParam) {
      return
    }

    const matched = (assetTypesQuery.data ?? []).find(
      (assetType) =>
        assetType.assetTypeId === assetTypeParam ||
        assetType.typeKey.toLowerCase() === assetTypeParam.toLowerCase() ||
        assetType.name.toLowerCase() === assetTypeParam.toLowerCase(),
    )

    if (!matched) {
      return
    }

    setValues((current) =>
      mergePrefill(current, {
        assetTypeIds: current.assetTypeIds.length > 0 ? current.assetTypeIds : [matched.assetTypeId],
      }),
    )
  }, [assetTypeParam, assetTypesQuery.data, currentTemplateId])

  useEffect(() => {
    if (currentTemplateId || !assetCategoryParam) {
      return
    }

    const match = assetCategoryOptions.find(
      (option) => option.value.toLowerCase() === assetCategoryParam.toLowerCase() || option.label.toLowerCase() === assetCategoryParam.toLowerCase(),
    )
    if (!match) {
      return
    }
    setValues((current) => mergePrefill(current, { templateCategoryKey: trimToEmpty(current.templateCategoryKey) || match.value }))
  }, [assetCategoryOptions, assetCategoryParam, currentTemplateId])

  useEffect(() => {
    if (currentTemplateId || !sourceTemplateResolvedId) {
      return
    }
    if (sourceTemplate) {
      setPreviewResult(null)
      return
    }
  }, [sourceTemplateResolvedId, currentTemplateId, sourceTemplate])

  const existingTemplateKeys = useMemo(
    () =>
      existingTemplateSummaries
        .filter((template) => template.inspectionTemplateId !== currentTemplate?.inspectionTemplateId)
        .map((template) => splitTemplateKey(template.templateKey)),
    [existingTemplateSummaries, currentTemplate?.inspectionTemplateId],
  )

  const existingCategoryKeys = useMemo(
    () => currentTemplate?.categories.map((category) => splitTemplateKey(category.categoryKey)) ?? [],
    [currentTemplate],
  )

  const existingItemKeys = useMemo(
    () => currentTemplate?.checklistItems.map((item) => splitTemplateKey(item.itemKey)) ?? [],
    [currentTemplate],
  )

  const generatedTemplateKey = useMemo(
    () =>
      buildSemanticKey({
        domain: 'inspection',
        kind: 'template',
        title: trimToEmpty(values.name) || sourceTemplate?.name || pmProgram?.name || 'inspection template',
        existingKeys: existingTemplateKeys,
        maxLength: 128,
      }),
    [existingTemplateKeys, values.name, sourceTemplate?.name, pmProgram?.name],
  )

  const generatedCategoryKey = useMemo(
    () =>
      buildSemanticKey({
        domain: 'inspection',
        kind: 'category',
        title: trimToEmpty(categoryDraft.name) || 'category',
        existingKeys: existingCategoryKeys,
        maxLength: 128,
      }),
    [categoryDraft.name, existingCategoryKeys],
  )

  const generatedItemKey = useMemo(
    () =>
      buildSemanticKey({
        domain: 'inspection',
        kind: 'item',
        title: trimToEmpty(itemDraft.prompt) || 'item',
        existingKeys: existingItemKeys,
        maxLength: 128,
      }),
    [itemDraft.prompt, existingItemKeys],
  )

  const effectiveTemplateKey = trimToEmpty(values.templateKeyOverride) || generatedTemplateKey
  const effectiveCategoryKey = trimToEmpty(categoryDraft.keyOverride) || generatedCategoryKey
  const effectiveItemKey = trimToEmpty(itemDraft.keyOverride) || generatedItemKey

  const fieldset = fieldsetQuery.data
  const fieldErrors = useMemo(() => validateTemplateFieldset(fieldset, values), [fieldset, values])
  const categoryErrors = useMemo(
    () => validateCategoryDraft({ ...categoryDraft, keyOverride: effectiveCategoryKey }, existingCategoryKeys),
    [categoryDraft, effectiveCategoryKey, existingCategoryKeys],
  )
  const itemErrors = useMemo(
    () => validateItemDraft({ ...itemDraft, keyOverride: effectiveItemKey }, existingItemKeys),
    [itemDraft, effectiveItemKey, existingItemKeys],
  )

  const ownershipFields = useMemo(
    () => fieldset?.fields.filter((field) => field.sectionKey === 'ownership') ?? [],
    [fieldset],
  )
  const scopeFields = useMemo(
    () => fieldset?.fields.filter((field) => field.sectionKey === 'scope') ?? [],
    [fieldset],
  )
  const settingsFields = useMemo(
    () => fieldset?.fields.filter((field) => field.sectionKey === 'settings') ?? [],
    [fieldset],
  )

  const basicsComplete = Boolean(
    trimToEmpty(values.name) &&
    trimToEmpty(values.inspectionType) &&
    effectiveTemplateKey &&
    !fieldErrors.templateKeyOverride &&
    !fieldErrors.name &&
    !fieldErrors.inspectionType &&
    !fieldErrors.estimatedDurationMinutes,
  )
  const basicsAutoAdvanceReady = shouldAutoAdvanceInspectionTemplateBasics({
    name: values.name,
    inspectionType: values.inspectionType,
    effectiveTemplateKey,
    fieldErrors,
  })

  const ownershipComplete = !ownershipFields.some((field) => Boolean(fieldErrors[field.key]))
  const scopeComplete = !scopeFields.some((field) => Boolean(fieldErrors[field.key]))
  const settingsComplete = !settingsFields.some((field) => Boolean(fieldErrors[field.key]))
  const structureComplete = Boolean(currentTemplateId && (currentTemplate?.checklistItems.length ?? 0) > 0)
  const reviewComplete =
    structureComplete &&
    confirmComplianceRelated &&
    confirmReadinessImpact &&
    confirmFailureAutomation &&
    confirmSupervisorRelease

  const canManage = meQuery.data
    ? canManageInspectionTemplates(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false
  const canCreate = meQuery.data
    ? canCreateInspectionTemplates(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false
  const canPreview = meQuery.data
    ? canPreviewInspectionTemplates(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false
  const canPublish = meQuery.data
    ? canPublishInspectionTemplates(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false
  const canRetire = meQuery.data
    ? canRetireInspectionTemplates(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false

  const sourceContext = useMemo<TemplateSourceContext>(() => {
    const matchedAssetType = assetTypesQuery.data?.find(
      (assetType) =>
        assetType.assetTypeId === assetTypeParam ||
        assetType.typeKey.toLowerCase() === assetTypeParam.toLowerCase() ||
        assetType.name.toLowerCase() === assetTypeParam.toLowerCase(),
    )
    const matchedAssetCategory = assetCategoryOptions.find(
      (option) => option.value.toLowerCase() === assetCategoryParam.toLowerCase() || option.label.toLowerCase() === assetCategoryParam.toLowerCase(),
    )
    const inspectionTypeLabel = toPickerOption(inspectionTypeOptions, inspectionTypeParam)?.label ?? (inspectionTypeParam ? humanize(inspectionTypeParam) : undefined)

    return {
      sourceTemplateId: sourceTemplate?.inspectionTemplateId ?? cloneTemplateId ?? sourceTemplateId ?? undefined,
      sourceTemplateName: sourceTemplate?.name,
      sourceTemplateKey: sourceTemplate?.templateKey,
      sourceTemplateStatus: sourceTemplate?.status,
      pmProgramId: pmProgram?.pmProgramId ?? pmProgramId ?? undefined,
      pmProgramName: pmProgram?.name,
      pmProgramKey: pmProgram?.programKey,
      pmProgramInspectionTemplateId: pmProgram?.inspectionTemplateId ?? undefined,
      pmProgramInspectionTemplateName: pmProgram?.inspectionTemplateName ?? undefined,
      pmProgramInspectionTemplateKey: pmProgram?.inspectionTemplateKey ?? undefined,
      assetTypeId: matchedAssetType?.assetTypeId ?? undefined,
      assetTypeName: matchedAssetType?.name ?? undefined,
      assetTypeKey: matchedAssetType?.typeKey ?? undefined,
      assetCategory: assetCategoryParam || undefined,
      assetCategoryLabel: matchedAssetCategory?.label ?? (assetCategoryParam ? humanize(assetCategoryParam) : undefined),
      complianceCatalogId: complianceCatalogId || undefined,
      complianceCatalogLabel: complianceCatalogId ? `Compliance context ${complianceCatalogId}` : undefined,
      inspectionTypeOverride: inspectionTypeLabel,
    }
  }, [
    assetCategoryOptions,
    assetCategoryParam,
    assetTypeParam,
    assetTypesQuery.data,
    cloneTemplateId,
    complianceCatalogId,
    inspectionTypeOptions,
    inspectionTypeParam,
    pmProgram,
    pmProgramId,
    sourceTemplate,
    sourceTemplateId,
  ])

  const sectionStates = useMemo(() => {
    return SECTION_DEFINITIONS.map((section) => {
      const labelData = labelForSectionState(
        section.key,
        values,
        fieldErrors,
        currentTemplate,
        previewResult,
        canPublish,
      )
      return {
        ...section,
        summary: sectionSummary(section.key, values, currentTemplate, sourceContext, previewResult),
        stateLabel: labelData.label,
        tone: labelData.tone,
      }
    })
  }, [canPublish, currentTemplate, fieldErrors, previewResult, sourceContext, values])

  useEffect(() => {
    const firstIncomplete = sectionStates.findIndex((section) => {
      switch (section.key) {
        case 'basics':
          return !basicsAutoAdvanceReady
        case 'ownership':
          return !ownershipComplete
        case 'scope':
          return !scopeComplete
        case 'settings':
          return !settingsComplete
        case 'structure':
          return !structureComplete
        case 'review':
          return !reviewComplete
        default:
          return false
      }
    })

    if (firstIncomplete >= 0 && firstIncomplete !== openSectionIndex) {
      setOpenSectionIndex(firstIncomplete)
    }
  }, [
    basicsComplete,
    basicsAutoAdvanceReady,
    ownershipComplete,
    openSectionIndex,
    reviewComplete,
    scopeComplete,
    sectionStates,
    settingsComplete,
    structureComplete,
  ])

  const invalidateTemplateCaches = async (inspectionTemplateId?: string) => {
    await queryClient.invalidateQueries({ queryKey: ['maintainarr-inspection-templates'] })
    if (inspectionTemplateId) {
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-inspection-template', inspectionTemplateId] })
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-inspection-template-source', inspectionTemplateId] })
    }
  }

  const buildSavePayload = () => ({
    templateKey: effectiveTemplateKey,
    name: trimToEmpty(values.name),
    description: trimToEmpty(values.description),
    inspectionType: trimToEmpty(values.inspectionType),
    templateCategoryKey: trimToEmpty(values.templateCategoryKey) || null,
    owningSiteRef: trimToEmpty(values.owningSiteRef) || null,
    owningTeamRef: trimToEmpty(values.owningTeamRef) || null,
    ownerPersonId: trimToEmpty(values.ownerPersonId) || null,
    ownerRoleKey: trimToEmpty(values.ownerRoleKey) || null,
    estimatedDurationMinutes: parsePositiveInteger(values.estimatedDurationMinutes),
    tags: splitDelimitedList(values.tagsText),
    settings: {
      ...baselineSettings,
      executionMode: trimToEmpty(values.executionMode) || null,
      resultMode: trimToEmpty(values.resultMode) || null,
      readinessImpact: trimToEmpty(values.readinessImpact) || null,
      ...buildSourceContextSettings(sourceContext),
    },
  })

  async function persistTemplateDraft(): Promise<InspectionTemplateDetailResponse> {
    if (!session) {
      throw new Error('You must sign in to MaintainArr before creating an inspection template.')
    }
    if (!basicsComplete) {
      throw new Error('Complete the template basics before saving the draft.')
    }

    const payload = buildSavePayload()
    const saved = currentTemplateId
      ? await updateInspectionTemplate(session.accessToken, currentTemplateId, payload)
      : await createInspectionTemplate(session.accessToken, payload)

    const synced = await replaceInspectionTemplateAssetTypes(
      session.accessToken,
      saved.inspectionTemplateId,
      values.assetTypeIds,
    )
    await invalidateTemplateCaches(synced.inspectionTemplateId)
    return synced
  }

  const updateField = <K extends keyof TemplateFormValues>(key: K, value: TemplateFormValues[K]) => {
    setValues((current) => ({ ...current, [key]: value }))
    setPreviewResult(null)
    setPreviewMessage(null)
    setServerError(null)
  }

  const saveDraftMutation = useMutation({
    mutationFn: async () => persistTemplateDraft(),
    onSuccess: (saved) => {
      setCurrentTemplateId(saved.inspectionTemplateId)
      setTemplateStatus(saved.status)
      setBaselineSettings(saved.settings ?? {})
      setValues({
        templateKeyOverride: saved.templateKey,
        name: saved.name,
        description: saved.description ?? '',
        templateCategoryKey: saved.templateCategoryKey ?? '',
        inspectionType: saved.inspectionType ?? '',
        owningSiteRef: saved.owningSiteRef ?? '',
        owningTeamRef: saved.owningTeamRef ?? '',
        ownerPersonId: saved.ownerPersonId ?? '',
        ownerRoleKey: saved.ownerRoleKey ?? '',
        estimatedDurationMinutes: saved.estimatedDurationMinutes != null ? String(saved.estimatedDurationMinutes) : '',
        executionMode: settingsText(saved.settings, 'executionMode') || 'manual',
        resultMode: settingsText(saved.settings, 'resultMode') || 'pass_fail',
        readinessImpact: settingsText(saved.settings, 'readinessImpact') || 'none',
        tagsText: saved.tags?.join(', ') ?? '',
        assetTypeIds: saved.linkedAssetTypes.map((link) => link.assetTypeId),
      })
      setPreviewResult(null)
      setPreviewMessage('Draft saved.')
      setServerError(null)
      setSearchParams((current) => {
        const next = new URLSearchParams(current)
        next.set('templateId', saved.inspectionTemplateId)
        return next
      }, { replace: true })
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to save inspection template draft.')
    },
  })

  const previewMutation = useMutation({
    mutationFn: async () => {
      const saved = await persistTemplateDraft()
      return previewInspectionTemplate(session!.accessToken, saved.inspectionTemplateId)
    },
    onSuccess: (preview) => {
      setPreviewResult(preview)
      setPreviewMessage(preview.summary)
      setServerError(null)
      setOpenSectionIndex(5)
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to preview inspection template.')
    },
  })

  const publishMutation = useMutation({
    mutationFn: async () => {
      const saved = await persistTemplateDraft()
      return publishInspectionTemplate(session!.accessToken, saved.inspectionTemplateId, {
        confirmComplianceRelated,
        confirmReadinessImpact,
        confirmFailureAutomation,
        confirmSupervisorRelease,
      })
    },
    onSuccess: async (published) => {
      setCurrentTemplateId(published.inspectionTemplateId)
      setTemplateStatus(published.status)
      setBaselineSettings(published.settings ?? {})
      setValues({
        templateKeyOverride: published.templateKey,
        name: published.name,
        description: published.description ?? '',
        templateCategoryKey: published.templateCategoryKey ?? '',
        inspectionType: published.inspectionType ?? '',
        owningSiteRef: published.owningSiteRef ?? '',
        owningTeamRef: published.owningTeamRef ?? '',
        ownerPersonId: published.ownerPersonId ?? '',
        ownerRoleKey: published.ownerRoleKey ?? '',
        estimatedDurationMinutes: published.estimatedDurationMinutes != null ? String(published.estimatedDurationMinutes) : '',
        executionMode: settingsText(published.settings, 'executionMode') || 'manual',
        resultMode: settingsText(published.settings, 'resultMode') || 'pass_fail',
        readinessImpact: settingsText(published.settings, 'readinessImpact') || 'none',
        tagsText: published.tags?.join(', ') ?? '',
        assetTypeIds: published.linkedAssetTypes.map((link) => link.assetTypeId),
      })
      setPreviewMessage('Template published.')
      setServerError(null)
      await invalidateTemplateCaches(published.inspectionTemplateId)
      setSearchParams((current) => {
        const next = new URLSearchParams(current)
        next.set('templateId', published.inspectionTemplateId)
        return next
      }, { replace: true })
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to publish inspection template.')
    },
  })

  const retireMutation = useMutation({
    mutationFn: async () => {
      const saved = await persistTemplateDraft()
      return retireInspectionTemplate(session!.accessToken, saved.inspectionTemplateId, {
        reason: trimToEmpty(retireReason) || null,
      })
    },
    onSuccess: async (retired) => {
      setCurrentTemplateId(retired.inspectionTemplateId)
      setTemplateStatus(retired.status)
      setBaselineSettings(retired.settings ?? {})
      setValues({
        templateKeyOverride: retired.templateKey,
        name: retired.name,
        description: retired.description ?? '',
        templateCategoryKey: retired.templateCategoryKey ?? '',
        inspectionType: retired.inspectionType ?? '',
        owningSiteRef: retired.owningSiteRef ?? '',
        owningTeamRef: retired.owningTeamRef ?? '',
        ownerPersonId: retired.ownerPersonId ?? '',
        ownerRoleKey: retired.ownerRoleKey ?? '',
        estimatedDurationMinutes: retired.estimatedDurationMinutes != null ? String(retired.estimatedDurationMinutes) : '',
        executionMode: settingsText(retired.settings, 'executionMode') || 'manual',
        resultMode: settingsText(retired.settings, 'resultMode') || 'pass_fail',
        readinessImpact: settingsText(retired.settings, 'readinessImpact') || 'none',
        tagsText: retired.tags?.join(', ') ?? '',
        assetTypeIds: retired.linkedAssetTypes.map((link) => link.assetTypeId),
      })
      setPreviewMessage('Template retired.')
      setServerError(null)
      await invalidateTemplateCaches(retired.inspectionTemplateId)
      setSearchParams((current) => {
        const next = new URLSearchParams(current)
        next.set('templateId', retired.inspectionTemplateId)
        return next
      }, { replace: true })
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to retire inspection template.')
    },
  })

  const cloneMutation = useMutation({
    mutationFn: async () => {
      if (!session) {
        throw new Error('You must sign in to MaintainArr before cloning an inspection template.')
      }
      const cloneSourceId = currentTemplateId || sourceTemplate?.inspectionTemplateId || pmProgram?.inspectionTemplateId || ''
      if (!cloneSourceId) {
        throw new Error('Open a saved template before cloning.')
      }
      return cloneInspectionTemplate(session.accessToken, cloneSourceId)
    },
    onSuccess: async (cloned) => {
      setCurrentTemplateId(cloned.inspectionTemplateId)
      setTemplateStatus(cloned.status)
      setBaselineSettings(cloned.settings ?? {})
      setValues({
        templateKeyOverride: cloned.templateKey,
        name: cloned.name,
        description: cloned.description ?? '',
        templateCategoryKey: cloned.templateCategoryKey ?? '',
        inspectionType: cloned.inspectionType ?? '',
        owningSiteRef: cloned.owningSiteRef ?? '',
        owningTeamRef: cloned.owningTeamRef ?? '',
        ownerPersonId: cloned.ownerPersonId ?? '',
        ownerRoleKey: cloned.ownerRoleKey ?? '',
        estimatedDurationMinutes: cloned.estimatedDurationMinutes != null ? String(cloned.estimatedDurationMinutes) : '',
        executionMode: settingsText(cloned.settings, 'executionMode') || 'manual',
        resultMode: settingsText(cloned.settings, 'resultMode') || 'pass_fail',
        readinessImpact: settingsText(cloned.settings, 'readinessImpact') || 'none',
        tagsText: cloned.tags?.join(', ') ?? '',
        assetTypeIds: cloned.linkedAssetTypes.map((link) => link.assetTypeId),
      })
      setPreviewResult(null)
      setPreviewMessage('Template cloned. Edit the new draft below.')
      setServerError(null)
      setOpenSectionIndex(0)
      await invalidateTemplateCaches(cloned.inspectionTemplateId)
      setSearchParams((current) => {
        const next = new URLSearchParams(current)
        next.set('templateId', cloned.inspectionTemplateId)
        return next
      }, { replace: true })
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to clone inspection template.')
    },
  })

  const createCategoryMutation = useMutation({
    mutationFn: async () => {
      if (!currentTemplateId) {
        throw new Error('Save the draft before adding categories.')
      }
      const payload = {
        categoryKey: effectiveCategoryKey,
        name: trimToEmpty(categoryDraft.name),
        description: trimToEmpty(categoryDraft.description) || null,
        isRequired: categoryDraft.isRequired,
        canBeSkipped: categoryDraft.canBeSkipped,
        skipReasonRequired: categoryDraft.skipReasonRequired,
        timingTracked: categoryDraft.timingTracked,
        sortOrder: parsePositiveInteger(categoryDraft.sortOrder) ?? 10,
        settings: null,
      }
      return createInspectionTemplateCategory(session!.accessToken, currentTemplateId, payload)
    },
    onSuccess: async (updated) => {
      const created = updated.categories.find((category) => category.categoryKey.toLowerCase() === splitTemplateKey(effectiveCategoryKey))
      setCategoryDraft(createInitialCategoryDraft())
      if (created) {
        setItemDraft((current) => (trimToEmpty(current.categoryId) ? current : { ...current, categoryId: created.categoryId }))
      }
      setPreviewResult(null)
      setServerError(null)
      await invalidateTemplateCaches(updated.inspectionTemplateId)
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to create category.')
    },
  })

  const createItemMutation = useMutation({
    mutationFn: async () => {
      if (!currentTemplateId) {
        throw new Error('Save the draft before adding checklist items.')
      }
      const controlledOptions =
        itemDraft.itemType === 'select' || itemDraft.itemType === 'multi_select'
          ? splitDelimitedList(itemDraft.controlledOptionsText)
          : undefined
      const payload = {
        itemKey: effectiveItemKey,
        prompt: trimToEmpty(itemDraft.prompt),
        helpText: trimToEmpty(itemDraft.helpText) || null,
        itemType: itemDraft.itemType,
        isRequired: itemDraft.isRequired,
        sortOrder: parsePositiveInteger(itemDraft.sortOrder) ?? 10,
        categoryId: trimToEmpty(itemDraft.categoryId) || null,
        controlledOptions,
        acceptableRangeMin: itemDraft.acceptableRangeMin.trim() ? Number(itemDraft.acceptableRangeMin) : null,
        acceptableRangeMax: itemDraft.acceptableRangeMax.trim() ? Number(itemDraft.acceptableRangeMax) : null,
        unitOfMeasure: trimToEmpty(itemDraft.unitOfMeasure) || null,
        settings: null,
      }
      return createInspectionChecklistItem(session!.accessToken, currentTemplateId, payload)
    },
    onSuccess: async (updated) => {
      setItemDraft(createInitialItemDraft())
      setPreviewResult(null)
      setServerError(null)
      await invalidateTemplateCaches(updated.inspectionTemplateId)
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to create checklist item.')
    },
  })

  if (!session) {
    return <Navigate to="/launch" replace />
  }

  if (!canManage && meQuery.isSuccess) {
    return (
      <div className="mx-auto max-w-6xl space-y-6 px-4 py-6" data-testid="inspection-template-create-page">
        <PageHeader title="Create Inspection Template" subtitle="Guided MaintainArr inspection template setup" />
        <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-6">
          <h2 className="text-lg font-semibold text-white">Inspection template creation unavailable</h2>
          <p className="mt-2 text-sm text-slate-400">
            Your role can access inspection templates, but it cannot create or manage them.
          </p>
        </section>
      </div>
    )
  }

  const existingTemplateCollision = existingTemplateSummaries.some(
    (template) =>
      template.inspectionTemplateId !== currentTemplate?.inspectionTemplateId &&
      splitTemplateKey(template.templateKey) === splitTemplateKey(effectiveTemplateKey),
  )

  const categoryKeyCollision = existingCategoryKeys.includes(splitTemplateKey(effectiveCategoryKey))
  const itemKeyCollision = existingItemKeys.includes(splitTemplateKey(effectiveItemKey))

  const canSaveDraft =
    canCreate &&
    basicsComplete &&
    !fieldErrors.templateKeyOverride &&
    !fieldErrors.name &&
    !fieldErrors.inspectionType &&
    !fieldErrors.estimatedDurationMinutes &&
    !existingTemplateCollision
  const canPreviewNow = canPreview && basicsComplete && !existingTemplateCollision
  const canPublishNow =
    canPublish &&
    basicsComplete &&
    structureComplete &&
    reviewComplete &&
    !existingTemplateCollision &&
    !previewMutation.isPending &&
    !publishMutation.isPending
  const canCloneNow = Boolean(currentTemplateId || sourceTemplate?.inspectionTemplateId || pmProgram?.inspectionTemplateId)
  const canRetireNow = canRetire && Boolean(currentTemplateId && currentTemplateStatusValue.toLowerCase() !== 'retired')

  const previewIssues = previewResult?.validation.issues ?? []
  const blockingPreviewIssues = previewIssues.filter((issue) => issue.isBlocking)
  const selectedAssetTypeLabels = assetTypeOptions.filter((option) => values.assetTypeIds.includes(option.value)).map((option) => option.label)
  const currentCategorySummary = currentTemplate?.categories.length ? `${currentTemplate.categories.length} categories` : 'No categories yet'
  const currentItemSummary = currentTemplate?.checklistItems.length ? `${currentTemplate.checklistItems.length} checklist items` : 'No checklist items yet'
  const effectiveStatusLabel = currentTemplateId ? humanize(currentTemplateStatusValue) : 'Unsaved draft'

  return (
    <div className="mx-auto max-w-7xl space-y-6 px-4 py-6" data-testid="inspection-template-create-page" style={shellStyle}>
      <PageHeader title="Create Inspection Template" subtitle="Guided MaintainArr inspection template setup" />

      <div className="flex flex-wrap items-center justify-between gap-3">
        <Link to="/inspection-templates/drawer" className="inline-flex items-center gap-2 text-sm text-slate-300 hover:text-white">
          <ArrowLeft className="h-4 w-4" />
          Back to inspection templates
        </Link>
        <div className="flex flex-wrap items-center gap-2">
          <DetailBadge label={effectiveStatusLabel} tone={currentTemplateStatusValue.toLowerCase() === 'active' ? 'good' : currentTemplateStatusValue.toLowerCase() === 'retired' ? 'bad' : 'warn'} />
          {currentTemplateId ? <DetailBadge label={`Draft ID ${currentTemplateId.slice(0, 8)}…`} tone="info" /> : <DetailBadge label="Unsaved draft" tone="warn" />}
        </div>
      </div>

      {fieldsetQuery.isLoading || templatesQuery.isLoading || catalogsQuery.isLoading || assetTypesQuery.isLoading || sitesQuery.isLoading || teamsQuery.isLoading || peopleQuery.isLoading ? (
        <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-6">
          <div className="flex items-center gap-3 text-sm text-slate-300">
            <Loader2 className="h-4 w-4 animate-spin" />
            Loading inspection template create workflow...
          </div>
        </section>
      ) : null}

      {fieldsetQuery.isError ? (
        <p className="rounded-lg border border-red-800 bg-red-950/40 p-3 text-sm text-red-200">
          Failed to load the inspection template create fieldset.
        </p>
      ) : null}

      {serverError ? (
        <p className="rounded-lg border border-red-800 bg-red-950/40 p-3 text-sm text-red-200" data-testid="inspection-template-create-error">
          {serverError}
        </p>
      ) : null}

      {previewMessage ? (
        <p className="rounded-lg border border-emerald-800 bg-emerald-950/40 p-3 text-sm text-emerald-200" data-testid="inspection-template-create-message">
          {previewMessage}
        </p>
      ) : null}

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_360px]">
        <div className="space-y-4">
          {sectionStates.map((section, index) => {
            const locked =
              index > 0 &&
              ((index === 1 && !basicsComplete) ||
                (index === 2 && !ownershipComplete) ||
                (index === 3 && !scopeComplete) ||
                (index === 4 && !settingsComplete) ||
                (index === 5 && !structureComplete))

            return (
              <SectionCard
                key={section.key}
                title={section.label}
                summary={section.summary}
                stateLabel={section.stateLabel}
                tone={section.tone}
                expanded={openSectionIndex === index}
                locked={locked}
                icon={section.icon}
                onToggle={() => setOpenSectionIndex(index)}
                testId={`inspection-template-section-${section.key}`}
              >
                {section.key === 'basics' ? (
                  <div className="space-y-5">
                    <div className="grid gap-4 lg:grid-cols-[minmax(0,1.4fr)_minmax(0,1fr)]">
                      <div className="space-y-4">
                        <GeneratedKeyField
                          label="Template key"
                          sourceLabel={trimToEmpty(values.name) || sourceTemplate?.name || pmProgram?.name || 'Template name'}
                          generatedKey={generatedTemplateKey}
                          confirmedKey={effectiveTemplateKey}
                          manualOverride={values.templateKeyOverride}
                          onManualOverrideChange={(value) => updateField('templateKeyOverride', value)}
                          showAdvancedKey
                          allowManualOverride
                          collisionWarning={existingTemplateCollision ? 'This template key already exists in MaintainArr.' : null}
                          disabled={saveDraftMutation.isPending || previewMutation.isPending || publishMutation.isPending}
                        />
                        <div className="grid gap-4 md:grid-cols-2">
                          <label className="block text-sm text-slate-300">
                            Name
                            <input
                              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                              value={values.name}
                              onChange={(event) => updateField('name', event.target.value)}
                              placeholder="DOT annual inspection template"
                              data-testid="inspection-template-name"
                            />
                            {fieldErrors.name ? <p className="mt-1 text-xs text-red-300">{fieldErrors.name}</p> : null}
                          </label>
                          <ControlledSelect
                            label="Inspection type"
                            value={values.inspectionType}
                            onChange={(value) => updateField('inspectionType', value)}
                            options={inspectionTypeOptions}
                            testId="inspection-template-inspection-type"
                            emptyLabel="Select inspection type…"
                            selectedOption={toPickerOption(inspectionTypeOptions, values.inspectionType)}
                          />
                        </div>
                        <label className="block text-sm text-slate-300">
                          Description
                          <textarea
                            className="mt-1 min-h-28 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                            value={values.description}
                            onChange={(event) => updateField('description', event.target.value)}
                            placeholder="Describe what this template is for and when it should be used."
                            data-testid="inspection-template-description"
                          />
                          {fieldErrors.description ? <p className="mt-1 text-xs text-red-300">{fieldErrors.description}</p> : null}
                        </label>
                      </div>

                      <div className="space-y-4 rounded-2xl border border-slate-700 bg-slate-950/50 p-4">
                        <div className="flex items-center justify-between gap-3">
                          <div>
                            <div className="text-xs uppercase tracking-wide text-slate-500">Current state</div>
                            <h3 className="mt-1 text-base font-semibold text-white">{currentTemplateId ? humanize(currentTemplateStatusValue) : 'Draft by default'}</h3>
                          </div>
                          <DetailBadge label={currentTemplateId ? humanize(currentTemplateStatusValue) : 'Draft'} tone={currentTemplateStatusValue.toLowerCase() === 'active' ? 'good' : 'warn'} />
                        </div>
                        <p className="text-sm text-slate-300">
                          Keep the template as a draft until the identity, scope, and checklist structure are ready.
                        </p>
                        <div className="grid gap-3 text-sm text-slate-300">
                          <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-3">
                            <div className="text-xs uppercase tracking-wide text-slate-500">Source template</div>
                            <div className="mt-1 font-medium text-white">{sourceTemplateLabel(sourceTemplate)}</div>
                            {sourceTemplate?.templateKey ? <div className="mt-1 text-xs text-slate-500">{sourceTemplate.templateKey}</div> : null}
                          </div>
                          <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-3">
                            <div className="text-xs uppercase tracking-wide text-slate-500">PM program source</div>
                            <div className="mt-1 font-medium text-white">{sourceProgramLabel(pmProgram)}</div>
                            {pmProgram?.inspectionTemplateName ? <div className="mt-1 text-xs text-slate-500">Uses {pmProgram.inspectionTemplateName} if present</div> : null}
                          </div>
                        </div>
                      </div>
                    </div>

                    <div className="grid gap-4 md:grid-cols-2">
                      <ControlledSelect
                        label="Template category"
                        value={values.templateCategoryKey}
                        onChange={(value) => updateField('templateCategoryKey', value)}
                        options={categoryOptions}
                        testId="inspection-template-category"
                        emptyLabel="Select template category…"
                        selectedOption={toPickerOption(categoryOptions, values.templateCategoryKey)}
                      />
                      <label className="block text-sm text-slate-300">
                        Estimated duration minutes
                        <input
                          type="number"
                          min="0"
                          step="1"
                          className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                          value={values.estimatedDurationMinutes}
                          onChange={(event) => updateField('estimatedDurationMinutes', event.target.value)}
                          placeholder="30"
                          data-testid="inspection-template-duration"
                        />
                        {fieldErrors.estimatedDurationMinutes ? <p className="mt-1 text-xs text-red-300">{fieldErrors.estimatedDurationMinutes}</p> : null}
                      </label>
                    </div>
                  </div>
                ) : null}

                {section.key === 'ownership' ? (
                  <div className="grid gap-4 lg:grid-cols-2">
                    <StaticSearchPicker
                      label="Owning site"
                      value={values.owningSiteRef}
                      onChange={(value) => updateField('owningSiteRef', value)}
                      options={siteOptions}
                      selectedOption={toPickerOption(siteOptions, values.owningSiteRef)}
                      placeholder="Search sites…"
                      testId="inspection-template-owning-site"
                    />
                    <StaticSearchPicker
                      label="Owning team"
                      value={values.owningTeamRef}
                      onChange={(value) => updateField('owningTeamRef', value)}
                      options={teamOptions}
                      selectedOption={toPickerOption(teamOptions, values.owningTeamRef)}
                      placeholder="Search teams…"
                      testId="inspection-template-owning-team"
                    />
                    <StaticSearchPicker
                      label="Owner person"
                      value={values.ownerPersonId}
                      onChange={(value) => updateField('ownerPersonId', value)}
                      options={personOptions}
                      selectedOption={toPickerOption(personOptions, values.ownerPersonId)}
                      placeholder="Search people…"
                      testId="inspection-template-owner-person"
                    />
                    <ControlledSelect
                      label="Owner role"
                      value={values.ownerRoleKey}
                      onChange={(value) => updateField('ownerRoleKey', value)}
                      options={roleOptions}
                      testId="inspection-template-owner-role"
                      emptyLabel="Select owner role…"
                      selectedOption={toPickerOption(roleOptions, values.ownerRoleKey)}
                    />
                  </div>
                ) : null}

                {section.key === 'scope' ? (
                  <div className="space-y-5">
                    <div className="rounded-2xl border border-slate-700 bg-slate-950/50 p-4">
                      <div className="flex items-center justify-between gap-3">
                        <div>
                          <h3 className="text-base font-semibold text-white">Asset type scope</h3>
                          <p className="mt-1 text-sm text-slate-400">
                            Choose the MaintainArr asset types this template should apply to.
                          </p>
                        </div>
                        <DetailBadge label={`${values.assetTypeIds.length} selected`} tone={values.assetTypeIds.length > 0 ? 'good' : 'neutral'} />
                      </div>
                      <div className="mt-4">
                        <CheckboxMultiSelect
                          label="Asset types"
                          values={values.assetTypeIds}
                          onChange={(next) => updateField('assetTypeIds', next)}
                          options={assetTypeOptions}
                          testId="inspection-template-asset-types"
                        />
                      </div>
                      <div className="mt-4 flex flex-wrap gap-2">
                        {selectedAssetTypeLabels.length === 0 ? (
                          <span className="text-sm text-slate-500">No asset types selected.</span>
                        ) : (
                          selectedAssetTypeLabels.map((label) => (
                            <span key={label} className="rounded-full border border-slate-700 bg-slate-900 px-3 py-1 text-xs text-slate-200">
                              {label}
                            </span>
                          ))
                        )}
                      </div>
                    </div>

                    <div className="rounded-2xl border border-slate-700 bg-slate-950/50 p-4">
                      <div className="flex items-center justify-between gap-3">
                        <div>
                          <h3 className="text-base font-semibold text-white">Source context</h3>
                          <p className="mt-1 text-sm text-slate-400">
                            Source records are shown as snapshots and do not become the template’s source of truth.
                          </p>
                        </div>
                        <DetailBadge label={sourceContext.sourceTemplateId ? 'Prefilled' : 'Context only'} tone={sourceContext.sourceTemplateId ? 'info' : 'neutral'} />
                      </div>

                      <div className="mt-4 grid gap-3 md:grid-cols-2">
                        <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-3">
                          <div className="text-xs uppercase tracking-wide text-slate-500">PM program</div>
                          <div className="mt-1 font-medium text-white">{sourceContext.pmProgramName ?? 'Not sourced from a PM program'}</div>
                          {sourceContext.pmProgramKey ? <div className="mt-1 text-xs text-slate-500">{sourceContext.pmProgramKey}</div> : null}
                        </div>
                        <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-3">
                          <div className="text-xs uppercase tracking-wide text-slate-500">Source template</div>
                          <div className="mt-1 font-medium text-white">{sourceContext.sourceTemplateName ?? 'Not sourced from another template'}</div>
                          {sourceContext.sourceTemplateKey ? <div className="mt-1 text-xs text-slate-500">{sourceContext.sourceTemplateKey}</div> : null}
                        </div>
                        <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-3">
                          <div className="text-xs uppercase tracking-wide text-slate-500">Asset type</div>
                          <div className="mt-1 font-medium text-white">{sourceContext.assetTypeName ?? 'No asset type source'}</div>
                          {sourceContext.assetTypeKey ? <div className="mt-1 text-xs text-slate-500">{sourceContext.assetTypeKey}</div> : null}
                        </div>
                        <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-3">
                          <div className="text-xs uppercase tracking-wide text-slate-500">Asset category / compliance</div>
                          <div className="mt-1 font-medium text-white">{sourceContext.assetCategoryLabel ?? sourceContext.complianceCatalogLabel ?? 'No additional source context'}</div>
                          {sourceContext.complianceCatalogId ? <div className="mt-1 text-xs text-slate-500">{sourceContext.complianceCatalogId}</div> : null}
                        </div>
                      </div>
                    </div>
                  </div>
                ) : null}

                {section.key === 'settings' ? (
                  <div className="grid gap-4 lg:grid-cols-2">
                    <ControlledSelect
                      label="Execution mode"
                      value={values.executionMode}
                      onChange={(value) => updateField('executionMode', value)}
                      options={executionModeOptions}
                      testId="inspection-template-execution-mode"
                      emptyLabel="Select execution mode…"
                      selectedOption={toPickerOption(executionModeOptions, values.executionMode)}
                    />
                    <ControlledSelect
                      label="Result mode"
                      value={values.resultMode}
                      onChange={(value) => updateField('resultMode', value)}
                      options={resultModeOptions}
                      testId="inspection-template-result-mode"
                      emptyLabel="Select result mode…"
                      selectedOption={toPickerOption(resultModeOptions, values.resultMode)}
                    />
                    <ControlledSelect
                      label="Readiness impact"
                      value={values.readinessImpact}
                      onChange={(value) => updateField('readinessImpact', value)}
                      options={readinessImpactOptions}
                      testId="inspection-template-readiness-impact"
                      emptyLabel="Select readiness impact…"
                      selectedOption={toPickerOption(readinessImpactOptions, values.readinessImpact)}
                    />
                    <label className="block text-sm text-slate-300 lg:col-span-1">
                      Tags
                      <textarea
                        className="mt-1 min-h-28 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                        value={values.tagsText}
                        onChange={(event) => updateField('tagsText', event.target.value)}
                        placeholder="fleet, safety, seasonal"
                        data-testid="inspection-template-tags"
                      />
                    </label>
                    <div className="rounded-2xl border border-slate-700 bg-slate-950/50 p-4 lg:col-span-2">
                      <h3 className="text-base font-semibold text-white">Settings summary</h3>
                      <div className="mt-3 grid gap-3 md:grid-cols-3">
                        <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-3">
                          <div className="text-xs uppercase tracking-wide text-slate-500">Execution mode</div>
                          <div className="mt-1 text-sm text-white">{humanize(values.executionMode)}</div>
                        </div>
                        <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-3">
                          <div className="text-xs uppercase tracking-wide text-slate-500">Result mode</div>
                          <div className="mt-1 text-sm text-white">{humanize(values.resultMode)}</div>
                        </div>
                        <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-3">
                          <div className="text-xs uppercase tracking-wide text-slate-500">Readiness impact</div>
                          <div className="mt-1 text-sm text-white">{humanize(values.readinessImpact)}</div>
                        </div>
                      </div>
                    </div>
                  </div>
                ) : null}

                {section.key === 'structure' ? (
                  <div className="space-y-5">
                    {!currentTemplateId ? (
                      <div className="rounded-2xl border border-amber-800 bg-amber-950/30 p-4 text-sm text-amber-100">
                        Save the draft first. Once MaintainArr has a draft ID, you can add categories and checklist items.
                      </div>
                    ) : null}

                    <div className="grid gap-5 xl:grid-cols-2">
                      <div className="rounded-2xl border border-slate-700 bg-slate-950/50 p-4">
                        <div className="flex items-center justify-between gap-3">
                          <div>
                            <h3 className="text-base font-semibold text-white">Add category</h3>
                            <p className="mt-1 text-sm text-slate-400">
                              Categories help group checklist items for inspection runners and review.
                            </p>
                          </div>
                          <GeneratedKeyField
                            label="Category key"
                            sourceLabel={trimToEmpty(categoryDraft.name) || 'Category name'}
                            generatedKey={generatedCategoryKey}
                            confirmedKey={effectiveCategoryKey}
                            manualOverride={categoryDraft.keyOverride}
                            onManualOverrideChange={(value) => setCategoryDraft((current) => ({ ...current, keyOverride: value }))}
                            showAdvancedKey
                            allowManualOverride
                            collisionWarning={categoryKeyCollision ? 'This category key already exists in the current template.' : null}
                            disabled={!currentTemplateId || createCategoryMutation.isPending}
                          />
                        </div>
                        <div className="mt-4 grid gap-4">
                          <label className="block text-sm text-slate-300">
                            Category name
                            <input
                              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                              value={categoryDraft.name}
                              onChange={(event) => setCategoryDraft((current) => ({ ...current, name: event.target.value }))}
                              placeholder="Cab / lighting / brakes"
                              data-testid="inspection-template-category-name"
                              disabled={!currentTemplateId || createCategoryMutation.isPending}
                            />
                            {categoryErrors.name ? <p className="mt-1 text-xs text-red-300">{categoryErrors.name}</p> : null}
                          </label>
                          <label className="block text-sm text-slate-300">
                            Description
                            <textarea
                              className="mt-1 min-h-24 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                              value={categoryDraft.description}
                              onChange={(event) => setCategoryDraft((current) => ({ ...current, description: event.target.value }))}
                              placeholder="Describe what the category covers."
                              disabled={!currentTemplateId || createCategoryMutation.isPending}
                            />
                          </label>
                          <div className="grid gap-4 md:grid-cols-2">
                            <label className="flex items-center gap-2 text-sm text-slate-200">
                              <input
                                type="checkbox"
                                checked={categoryDraft.isRequired}
                                onChange={(event) => setCategoryDraft((current) => ({ ...current, isRequired: event.target.checked }))}
                                disabled={!currentTemplateId || createCategoryMutation.isPending}
                              />
                              Required category
                            </label>
                            <label className="flex items-center gap-2 text-sm text-slate-200">
                              <input
                                type="checkbox"
                                checked={categoryDraft.canBeSkipped}
                                onChange={(event) => setCategoryDraft((current) => ({ ...current, canBeSkipped: event.target.checked }))}
                                disabled={!currentTemplateId || createCategoryMutation.isPending}
                              />
                              Category can be skipped
                            </label>
                            <label className="flex items-center gap-2 text-sm text-slate-200">
                              <input
                                type="checkbox"
                                checked={categoryDraft.skipReasonRequired}
                                onChange={(event) => setCategoryDraft((current) => ({ ...current, skipReasonRequired: event.target.checked }))}
                                disabled={!currentTemplateId || createCategoryMutation.isPending}
                              />
                              Skip reason required
                            </label>
                            <label className="flex items-center gap-2 text-sm text-slate-200">
                              <input
                                type="checkbox"
                                checked={categoryDraft.timingTracked}
                                onChange={(event) => setCategoryDraft((current) => ({ ...current, timingTracked: event.target.checked }))}
                                disabled={!currentTemplateId || createCategoryMutation.isPending}
                              />
                              Track timing
                            </label>
                          </div>
                          <label className="block text-sm text-slate-300">
                            Sort order
                            <input
                              type="number"
                              min="0"
                              step="1"
                              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                              value={categoryDraft.sortOrder}
                              onChange={(event) => setCategoryDraft((current) => ({ ...current, sortOrder: event.target.value }))}
                              disabled={!currentTemplateId || createCategoryMutation.isPending}
                            />
                            {categoryErrors.sortOrder ? <p className="mt-1 text-xs text-red-300">{categoryErrors.sortOrder}</p> : null}
                          </label>
                          <button
                            type="button"
                            className="inline-flex items-center gap-2 rounded-full border px-4 py-2 text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-40"
                            style={{ borderColor: theme.border, backgroundColor: theme.primary }}
                            disabled={!currentTemplateId || createCategoryMutation.isPending || categoryKeyCollision || !trimToEmpty(categoryDraft.name) || Boolean(categoryErrors.keyOverride) || Boolean(categoryErrors.name)}
                            onClick={() => createCategoryMutation.mutate()}
                          >
                            <CirclePlus className="h-4 w-4" />
                            {createCategoryMutation.isPending ? 'Adding category…' : 'Add category'}
                          </button>
                        </div>
                      </div>

                      <div className="rounded-2xl border border-slate-700 bg-slate-950/50 p-4">
                        <div className="flex items-center justify-between gap-3">
                          <div>
                            <h3 className="text-base font-semibold text-white">Add checklist item</h3>
                            <p className="mt-1 text-sm text-slate-400">
                              Checklist items define what the runner sees during the inspection.
                            </p>
                          </div>
                          <GeneratedKeyField
                            label="Item key"
                            sourceLabel={trimToEmpty(itemDraft.prompt) || 'Checklist prompt'}
                            generatedKey={generatedItemKey}
                            confirmedKey={effectiveItemKey}
                            manualOverride={itemDraft.keyOverride}
                            onManualOverrideChange={(value) => setItemDraft((current) => ({ ...current, keyOverride: value }))}
                            showAdvancedKey
                            allowManualOverride
                            collisionWarning={itemKeyCollision ? 'This checklist item key already exists in the current template.' : null}
                            disabled={!currentTemplateId || createItemMutation.isPending}
                          />
                        </div>

                        <div className="mt-4 grid gap-4">
                          <ControlledSelect
                            label="Item type"
                            value={itemDraft.itemType}
                            onChange={(value) => setItemDraft((current) => ({ ...current, itemType: value }))}
                            options={ITEM_TYPE_OPTIONS}
                            selectedOption={toPickerOption(ITEM_TYPE_OPTIONS, itemDraft.itemType)}
                            testId="inspection-template-item-type"
                            emptyLabel="Select item type…"
                            disabled={!currentTemplateId || createItemMutation.isPending}
                          />
                          <label className="block text-sm text-slate-300">
                            Prompt
                            <input
                              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                              value={itemDraft.prompt}
                              onChange={(event) => setItemDraft((current) => ({ ...current, prompt: event.target.value }))}
                              placeholder="Inspect right rear brake lamp"
                              disabled={!currentTemplateId || createItemMutation.isPending}
                              data-testid="inspection-template-item-prompt"
                            />
                            {itemErrors.prompt ? <p className="mt-1 text-xs text-red-300">{itemErrors.prompt}</p> : null}
                          </label>
                          <label className="block text-sm text-slate-300">
                            Help text
                            <textarea
                              className="mt-1 min-h-20 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                              value={itemDraft.helpText}
                              onChange={(event) => setItemDraft((current) => ({ ...current, helpText: event.target.value }))}
                              placeholder="Provide any guidance the inspector should know."
                              disabled={!currentTemplateId || createItemMutation.isPending}
                            />
                          </label>
                          <ControlledSelect
                            label="Category"
                            value={itemDraft.categoryId}
                            onChange={(value) => setItemDraft((current) => ({ ...current, categoryId: value }))}
                            options={(currentTemplate?.categories ?? []).map((category) => ({ value: category.categoryId, label: category.name }))}
                            selectedOption={(currentTemplate?.categories ?? [])
                              .map((category) => ({ value: category.categoryId, label: category.name }))
                              .find((option) => option.value === itemDraft.categoryId)}
                            testId="inspection-template-item-category"
                            emptyLabel="Uncategorized"
                            disabled={!currentTemplateId || createItemMutation.isPending}
                          />
                          <div className="grid gap-4 md:grid-cols-2">
                            <label className="block text-sm text-slate-300">
                              Sort order
                              <input
                                type="number"
                                min="0"
                                step="1"
                                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                                value={itemDraft.sortOrder}
                                onChange={(event) => setItemDraft((current) => ({ ...current, sortOrder: event.target.value }))}
                                disabled={!currentTemplateId || createItemMutation.isPending}
                              />
                              {itemErrors.sortOrder ? <p className="mt-1 text-xs text-red-300">{itemErrors.sortOrder}</p> : null}
                            </label>
                            <label className="flex items-center gap-2 text-sm text-slate-200">
                              <input
                                type="checkbox"
                                checked={itemDraft.isRequired}
                                onChange={(event) => setItemDraft((current) => ({ ...current, isRequired: event.target.checked }))}
                                disabled={!currentTemplateId || createItemMutation.isPending}
                              />
                              Required item
                            </label>
                          </div>

                          {(itemDraft.itemType === 'select' || itemDraft.itemType === 'multi_select') ? (
                            <label className="block text-sm text-slate-300">
                              Controlled options
                              <textarea
                                className="mt-1 min-h-24 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                                value={itemDraft.controlledOptionsText}
                                onChange={(event) => setItemDraft((current) => ({ ...current, controlledOptionsText: event.target.value }))}
                                placeholder="One option per line or separated by commas"
                                disabled={!currentTemplateId || createItemMutation.isPending}
                              />
                              {itemErrors.controlledOptionsText ? <p className="mt-1 text-xs text-red-300">{itemErrors.controlledOptionsText}</p> : null}
                            </label>
                          ) : null}

                          {itemDraft.itemType === 'meter_reading' ? (
                            <div className="grid gap-4 md:grid-cols-3">
                              <label className="block text-sm text-slate-300">
                                Unit of measure
                                <input
                                  className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                                  value={itemDraft.unitOfMeasure}
                                  onChange={(event) => setItemDraft((current) => ({ ...current, unitOfMeasure: event.target.value }))}
                                  placeholder="hours"
                                  disabled={!currentTemplateId || createItemMutation.isPending}
                                />
                                {itemErrors.unitOfMeasure ? <p className="mt-1 text-xs text-red-300">{itemErrors.unitOfMeasure}</p> : null}
                              </label>
                              <label className="block text-sm text-slate-300">
                                Acceptable minimum
                                <input
                                  type="number"
                                  step="any"
                                  className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                                  value={itemDraft.acceptableRangeMin}
                                  onChange={(event) => setItemDraft((current) => ({ ...current, acceptableRangeMin: event.target.value }))}
                                  disabled={!currentTemplateId || createItemMutation.isPending}
                                />
                                {itemErrors.acceptableRangeMin ? <p className="mt-1 text-xs text-red-300">{itemErrors.acceptableRangeMin}</p> : null}
                              </label>
                              <label className="block text-sm text-slate-300">
                                Acceptable maximum
                                <input
                                  type="number"
                                  step="any"
                                  className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                                  value={itemDraft.acceptableRangeMax}
                                  onChange={(event) => setItemDraft((current) => ({ ...current, acceptableRangeMax: event.target.value }))}
                                  disabled={!currentTemplateId || createItemMutation.isPending}
                                />
                                {itemErrors.acceptableRangeMax ? <p className="mt-1 text-xs text-red-300">{itemErrors.acceptableRangeMax}</p> : null}
                              </label>
                            </div>
                          ) : null}

                          <button
                            type="button"
                            className="inline-flex items-center gap-2 rounded-full border px-4 py-2 text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-40"
                            style={{ borderColor: theme.border, backgroundColor: theme.primary }}
                            disabled={
                              !currentTemplateId ||
                              createItemMutation.isPending ||
                              itemKeyCollision ||
                              !trimToEmpty(itemDraft.prompt) ||
                              Boolean(itemErrors.keyOverride) ||
                              Boolean(itemErrors.prompt) ||
                              Boolean(itemErrors.sortOrder) ||
                              Boolean(itemErrors.controlledOptionsText) ||
                              Boolean(itemErrors.unitOfMeasure)
                            }
                            onClick={() => createItemMutation.mutate()}
                          >
                            <CirclePlus className="h-4 w-4" />
                            {createItemMutation.isPending ? 'Adding item…' : 'Add checklist item'}
                          </button>
                        </div>
                      </div>
                    </div>

                    <div className="grid gap-4 xl:grid-cols-2">
                      <div className="rounded-2xl border border-slate-700 bg-slate-950/50 p-4">
                        <h3 className="text-base font-semibold text-white">Categories</h3>
                        <p className="mt-1 text-sm text-slate-400">{currentCategorySummary}</p>
                        <div className="mt-4 space-y-2">
                          {(currentTemplate?.categories ?? []).length === 0 ? (
                            <p className="text-sm text-slate-500">No categories yet.</p>
                          ) : (
                            (currentTemplate?.categories ?? []).map((category: InspectionTemplateCategoryResponse) => (
                              <div key={category.categoryId} className="rounded-xl border border-slate-700 bg-slate-900/60 p-3">
                                <div className="flex items-center justify-between gap-3">
                                  <div>
                                    <div className="font-medium text-white">{category.name}</div>
                                    <div className="mt-1 text-xs text-slate-500">{category.categoryKey}</div>
                                  </div>
                                  <div className="flex flex-wrap gap-2">
                                    {category.isRequired ? <DetailBadge label="Required" tone="good" /> : <DetailBadge label="Optional" tone="neutral" />}
                                    {category.timingTracked ? <DetailBadge label="Timing tracked" tone="info" /> : null}
                                  </div>
                                </div>
                                {category.description ? <p className="mt-2 text-sm text-slate-300">{category.description}</p> : null}
                              </div>
                            ))
                          )}
                        </div>
                      </div>

                      <div className="rounded-2xl border border-slate-700 bg-slate-950/50 p-4">
                        <h3 className="text-base font-semibold text-white">Checklist items</h3>
                        <p className="mt-1 text-sm text-slate-400">{currentItemSummary}</p>
                        <div className="mt-4 space-y-2">
                          {(currentTemplate?.checklistItems ?? []).length === 0 ? (
                            <p className="text-sm text-slate-500">No checklist items yet.</p>
                          ) : (
                            (currentTemplate?.checklistItems ?? []).map((item: InspectionChecklistItemResponse) => (
                              <div key={item.checklistItemId} className="rounded-xl border border-slate-700 bg-slate-900/60 p-3">
                                <div className="flex items-start justify-between gap-3">
                                  <div>
                                    <div className="font-medium text-white">{item.prompt}</div>
                                    <div className="mt-1 text-xs text-slate-500">{item.itemKey} · {humanize(item.itemType)}</div>
                                  </div>
                                  <div className="flex flex-wrap gap-2">
                                    {item.isRequired ? <DetailBadge label="Required" tone="good" /> : <DetailBadge label="Optional" tone="neutral" />}
                                    {item.categoryKey ? <DetailBadge label={item.categoryKey} tone="info" /> : null}
                                  </div>
                                </div>
                                {item.helpText ? <p className="mt-2 text-sm text-slate-300">{item.helpText}</p> : null}
                                {item.controlledOptions.length > 0 ? (
                                  <p className="mt-2 text-xs text-slate-400">Options: {item.controlledOptions.join(', ')}</p>
                                ) : null}
                              </div>
                            ))
                          )}
                        </div>
                      </div>
                    </div>
                  </div>
                ) : null}

                {section.key === 'review' ? (
                  <div className="space-y-5">
                    <div className="rounded-2xl border border-slate-700 bg-slate-950/50 p-4">
                      <div className="flex flex-wrap items-center justify-between gap-3">
                        <div>
                          <h3 className="text-base font-semibold text-white">Publish readiness</h3>
                          <p className="mt-1 text-sm text-slate-400">
                            Review the validation result, sample assets, and publish confirmations before activating the template.
                          </p>
                        </div>
                        <DetailBadge label={previewResult ? (blockingPreviewIssues.length > 0 ? 'Blocking issues' : 'Previewed') : 'Preview not run'} tone={previewResult ? (blockingPreviewIssues.length > 0 ? 'bad' : 'good') : 'warn'} />
                      </div>

                      <div className="mt-4 grid gap-4 lg:grid-cols-2">
                        <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-3">
                          <div className="text-xs uppercase tracking-wide text-slate-500">Validation</div>
                          {previewResult ? (
                            <ul className="mt-2 space-y-2 text-sm">
                              {previewIssues.length === 0 ? (
                                <li className="text-emerald-200">No validation issues detected.</li>
                              ) : (
                                previewIssues.map((issue) => (
                                  <li key={`${issue.section}-${issue.code}-${issue.message}`} className={issue.isBlocking ? 'text-rose-200' : 'text-amber-200'}>
                                    <span className="font-medium">{humanize(issue.section)}</span> · {issue.message}
                                  </li>
                                ))
                              )}
                            </ul>
                          ) : (
                            <p className="mt-2 text-sm text-slate-400">
                              Run preview to inspect blocking issues and asset compatibility.
                            </p>
                          )}
                        </div>

                        <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-3">
                          <div className="text-xs uppercase tracking-wide text-slate-500">Compatible assets</div>
                          <p className="mt-2 text-sm text-slate-200">
                            {previewResult ? `${previewResult.assets.compatibleCount} asset(s) match the current template scope.` : 'No preview has been run yet.'}
                          </p>
                          {previewResult?.assets.sampleAssets.length ? (
                            <ul className="mt-3 space-y-2 text-xs text-slate-400">
                              {previewResult.assets.sampleAssets.map((asset) => (
                                <li key={asset.assetId} className="rounded-lg border border-slate-700 bg-slate-950/50 px-3 py-2">
                                  <div className="font-medium text-slate-100">{asset.assetTag} · {asset.name}</div>
                                  <div className="mt-1">{asset.typeName} · {asset.readinessStatus}</div>
                                </li>
                              ))}
                            </ul>
                          ) : null}
                        </div>
                      </div>
                    </div>

                    <div className="grid gap-4 lg:grid-cols-2">
                      <div className="rounded-2xl border border-slate-700 bg-slate-950/50 p-4">
                        <h3 className="text-base font-semibold text-white">Publish confirmations</h3>
                        <p className="mt-1 text-sm text-slate-400">
                          The backend requires these confirmations before a template can move to active status.
                        </p>
                        <div className="mt-4 space-y-3">
                          <label className="flex items-start gap-3 text-sm text-slate-200">
                            <input
                              type="checkbox"
                              className="mt-1"
                              checked={confirmComplianceRelated}
                              onChange={(event) => setConfirmComplianceRelated(event.target.checked)}
                            />
                            <span>
                              This template’s compliance and evidence implications have been reviewed.
                            </span>
                          </label>
                          <label className="flex items-start gap-3 text-sm text-slate-200">
                            <input
                              type="checkbox"
                              className="mt-1"
                              checked={confirmReadinessImpact}
                              onChange={(event) => setConfirmReadinessImpact(event.target.checked)}
                            />
                            <span>
                              This template’s readiness impact is understood and acceptable.
                            </span>
                          </label>
                          <label className="flex items-start gap-3 text-sm text-slate-200">
                            <input
                              type="checkbox"
                              className="mt-1"
                              checked={confirmFailureAutomation}
                              onChange={(event) => setConfirmFailureAutomation(event.target.checked)}
                            />
                            <span>
                              Any failure automation or downstream follow-up behavior is intentional.
                            </span>
                          </label>
                          <label className="flex items-start gap-3 text-sm text-slate-200">
                            <input
                              type="checkbox"
                              className="mt-1"
                              checked={confirmSupervisorRelease}
                              onChange={(event) => setConfirmSupervisorRelease(event.target.checked)}
                            />
                            <span>
                              Supervisor release or escalation behavior has been reviewed.
                            </span>
                          </label>
                        </div>
                      </div>

                      <div className="rounded-2xl border border-slate-700 bg-slate-950/50 p-4">
                        <h3 className="text-base font-semibold text-white">Retire reason</h3>
                        <p className="mt-1 text-sm text-slate-400">
                          Add an optional note to explain why an active template is being retired.
                        </p>
                        <textarea
                          className="mt-4 min-h-28 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                          value={retireReason}
                          onChange={(event) => setRetireReason(event.target.value)}
                          placeholder="Retired after replacing with the new roadside inspection template."
                          data-testid="inspection-template-retire-reason"
                        />
                      </div>
                    </div>
                  </div>
                ) : null}

              </SectionCard>
            )
          })}
        </div>

        <aside className="space-y-4 xl:sticky xl:top-4 xl:self-start">
          <section className="rounded-3xl border p-5 shadow-2xl" style={panelStyle}>
            <div className="flex items-center justify-between gap-3">
              <div>
                <div className="text-xs uppercase tracking-wide text-slate-500">Template overview</div>
                <h2 className="mt-1 text-lg font-semibold text-white">{currentTemplate?.name || trimToEmpty(values.name) || 'New inspection template'}</h2>
              </div>
              <DetailBadge label={effectiveStatusLabel} tone={currentTemplateStatusValue.toLowerCase() === 'active' ? 'good' : currentTemplateStatusValue.toLowerCase() === 'retired' ? 'bad' : 'warn'} />
            </div>
            <div className="mt-4 space-y-3 text-sm text-slate-300">
              <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-3">
                <div className="text-xs uppercase tracking-wide text-slate-500">Template key</div>
                <div className="mt-1 font-medium text-white">{effectiveTemplateKey || 'Not yet generated'}</div>
              </div>
              <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-3">
                <div className="text-xs uppercase tracking-wide text-slate-500">Source template</div>
                <div className="mt-1 font-medium text-white">{sourceTemplateLabel(sourceTemplate)}</div>
              </div>
              <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-3">
                <div className="text-xs uppercase tracking-wide text-slate-500">PM program</div>
                <div className="mt-1 font-medium text-white">{sourceProgramLabel(pmProgram)}</div>
              </div>
              <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-3">
                <div className="text-xs uppercase tracking-wide text-slate-500">Scope</div>
                <div className="mt-1 font-medium text-white">{values.assetTypeIds.length > 0 ? `${values.assetTypeIds.length} asset type(s)` : 'No asset types selected'}</div>
              </div>
              <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-3">
                <div className="text-xs uppercase tracking-wide text-slate-500">Checklist structure</div>
                <div className="mt-1 font-medium text-white">{currentCategorySummary} · {currentItemSummary}</div>
              </div>
            </div>
          </section>

          <section className="rounded-3xl border p-5 shadow-2xl" style={panelStyle}>
            <div className="text-xs uppercase tracking-wide text-slate-500">Actions</div>
            <div className="mt-4 space-y-3">
              <button
                type="button"
                className="flex w-full items-center justify-between gap-3 rounded-2xl border px-4 py-3 text-left text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-40"
                style={{ borderColor: theme.border, backgroundColor: theme.primary }}
                disabled={!canSaveDraft || saveDraftMutation.isPending}
                onClick={() => saveDraftMutation.mutate()}
              >
                <span>{currentTemplateId ? 'Update Draft' : 'Save Draft'}</span>
                <ArrowRight className="h-4 w-4" />
              </button>

              <button
                type="button"
                className="flex w-full items-center justify-between gap-3 rounded-2xl border px-4 py-3 text-left text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-40"
                style={{ borderColor: theme.border, backgroundColor: '#0F1730' }}
                disabled={!canPreviewNow || previewMutation.isPending || !basicsComplete}
                onClick={() => previewMutation.mutate()}
              >
                <span>{previewMutation.isPending ? 'Running Preview…' : 'Preview'}</span>
                <RefreshCw className="h-4 w-4" />
              </button>

              <button
                type="button"
                className="flex w-full items-center justify-between gap-3 rounded-2xl border px-4 py-3 text-left text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-40"
                style={{ borderColor: theme.border, backgroundColor: '#143046' }}
                disabled={!canPublishNow}
                onClick={() => publishMutation.mutate()}
              >
                <span>{publishMutation.isPending ? 'Publishing…' : 'Publish'}</span>
                <CheckCircle2 className="h-4 w-4" />
              </button>

              <button
                type="button"
                className="flex w-full items-center justify-between gap-3 rounded-2xl border px-4 py-3 text-left text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-40"
                style={{ borderColor: theme.border, backgroundColor: '#1A2446' }}
                disabled={!canCloneNow || cloneMutation.isPending}
                onClick={() => cloneMutation.mutate()}
              >
                <span>{cloneMutation.isPending ? 'Cloning…' : 'Clone'}</span>
                <Sparkles className="h-4 w-4" />
              </button>

              <button
                type="button"
                className="flex w-full items-center justify-between gap-3 rounded-2xl border px-4 py-3 text-left text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-40"
                style={{ borderColor: '#5b1b24', backgroundColor: '#3a1118' }}
                disabled={!canRetireNow || retireMutation.isPending}
                onClick={() => retireMutation.mutate()}
              >
                <span>{retireMutation.isPending ? 'Retiring…' : 'Retire'}</span>
                <Trash2 className="h-4 w-4" />
              </button>
            </div>
            <div className="mt-4 rounded-2xl border border-slate-700 bg-slate-950/50 p-3 text-xs text-slate-400">
              Save the draft before adding categories and checklist items. Publish requires at least one checklist item and all confirmations.
            </div>
          </section>

          <section className="rounded-3xl border p-5 shadow-2xl" style={panelStyle}>
            <div className="text-xs uppercase tracking-wide text-slate-500">Quick facts</div>
            <dl className="mt-4 space-y-3 text-sm">
              <div>
                <dt className="text-slate-500">Owned by</dt>
                <dd className="text-white">{values.ownerPersonId ? personOptions.find((option) => option.value === values.ownerPersonId)?.label ?? values.ownerPersonId : 'No owner selected'}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Owning site</dt>
                <dd className="text-white">{values.owningSiteRef ? siteOptions.find((option) => option.value === values.owningSiteRef)?.label ?? values.owningSiteRef : 'No site selected'}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Owning team</dt>
                <dd className="text-white">{values.owningTeamRef ? teamOptions.find((option) => option.value === values.owningTeamRef)?.label ?? values.owningTeamRef : 'No team selected'}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Inspection type</dt>
                <dd className="text-white">{values.inspectionType ? toPickerOption(inspectionTypeOptions, values.inspectionType)?.label ?? humanize(values.inspectionType) : 'Not selected'}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Asset types</dt>
                <dd className="text-white">{values.assetTypeIds.length > 0 ? `${values.assetTypeIds.length} selected` : 'None selected'}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Tags</dt>
                <dd className="text-white">{splitDelimitedList(values.tagsText).length > 0 ? splitDelimitedList(values.tagsText).join(', ') : 'No tags'}</dd>
              </div>
            </dl>
          </section>
        </aside>
      </div>
    </div>
  )
}
