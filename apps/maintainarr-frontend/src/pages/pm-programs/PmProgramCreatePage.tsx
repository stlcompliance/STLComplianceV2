import { useEffect, useMemo, useRef, useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import {
  AlertTriangle,
  ArrowLeft,
  ArrowRight,
  BadgeCheck,
  CalendarClock,
  CheckCircle2,
  ChevronDown,
  ChevronUp,
  CirclePlus,
  Lock,
  Loader2,
  Sparkles,
  Trash2,
  Wrench,
} from 'lucide-react'
import {
  buildSemanticKey,
  ControlledSelect,
  DetailBadge,
  GeneratedKeyField,
  PageHeader,
  StaticSearchPicker,
  type PickerOption,
} from '@stl/shared-ui'
import {
  activatePmProgram,
  createPmProgram,
  getAssetClasses,
  getAssetTypes,
  getAssets,
  getCatalogs,
  getComplianceCoreCatalogOptions,
  getInspectionTemplate,
  getInspectionTemplates,
  getMe,
  getPmPrograms,
  getPeople,
  getParts,
  getDepartments,
  getSites,
  getTeams,
  previewPmProgramDue,
  previewPmProgramScope,
  MaintainArrApiError,
} from '../../api/client'
import type {
  AssetResponse,
  CatalogResponse,
  CreatePmProgramRequest,
  PmProgramDetailResponse,
  PmProgramDuePreviewResponse,
  PmProgramScopePreviewResponse,
  ReferenceOptionResponse,
} from '../../api/types'
import {
  canActivatePmPrograms,
  canCreatePmPrograms,
  canManagePmProgramAutomation,
  canPreviewPmPrograms,
  loadSession,
} from '../../auth/sessionStorage'

const theme = {
  bg: 'var(--color-bg-app)',
  surface: 'var(--color-bg-surface)',
  primary: 'var(--color-accent)',
  secondary: 'var(--color-accent)',
  accent: 'var(--tone-warning-text)',
  text: 'var(--color-text-primary)',
  muted: 'var(--color-text-muted)',
  border: 'var(--color-border-subtle)',
} as const

const shellStyle = {
  background: `linear-gradient(180deg, ${theme.bg} 0%, var(--color-bg-surface-elevated) 100%)`,
  color: theme.text,
}

const panelStyle = {
  backgroundColor: theme.surface,
  borderColor: theme.border,
}

type Tone = 'good' | 'warn' | 'bad' | 'info' | 'neutral'

type TriggerType = 'calendar' | 'meter' | 'one_time' | 'manual'

type AssignmentBehavior = 'unassigned' | 'team' | 'role' | 'person'

type ReadinessImpact =
  | 'no_impact'
  | 'warn_when_due_soon'
  | 'not_ready_overdue'
  | 'not_ready_failed_inspection'
  | 'supervisor_release'

type InspectionResultBehavior =
  | 'pass_completes'
  | 'fail_creates_defect'
  | 'fail_blocks_readiness'
  | 'fail_requires_corrective_work_order'

type CalendarBehavior = 'fixed_schedule' | 'rolling_from_completion'
type PastDueBehavior = 'mark_due' | 'mark_overdue' | 'hold'
type MissingDataBehavior = 'warn' | 'skip' | 'block'

interface TriggerDraft {
  id: string
  triggerType: TriggerType
  calendarIntervalValue: string
  calendarIntervalUnit: string
  calendarAnchorDate: string
  calendarFirstDueDate: string
  calendarBehavior: CalendarBehavior
  calendarEarlyWindowDays: string
  calendarGracePeriodDays: string
  calendarPastDueBehavior: PastDueBehavior
  meterIntervalValue: string
  meterIntervalUnit: string
  meterAnchorReading: string
  meterFirstDueReading: string
  meterCurrentReadingSource: string
  meterEarlyThreshold: string
  meterGraceThreshold: string
  meterRollingFromCompletion: boolean
  meterMissingDataBehavior: MissingDataBehavior
  oneTimeDueDate: string
}

function createTriggerDraft(triggerType: TriggerType = 'calendar'): TriggerDraft {
  return {
    id: `${Date.now()}-${Math.random().toString(16).slice(2)}`,
    triggerType,
    calendarIntervalValue: '',
    calendarIntervalUnit: 'months',
    calendarAnchorDate: '',
    calendarFirstDueDate: '',
    calendarBehavior: 'fixed_schedule',
    calendarEarlyWindowDays: '',
    calendarGracePeriodDays: '',
    calendarPastDueBehavior: 'mark_due',
    meterIntervalValue: '',
    meterIntervalUnit: 'miles',
    meterAnchorReading: '',
    meterFirstDueReading: '',
    meterCurrentReadingSource: 'manual',
    meterEarlyThreshold: '',
    meterGraceThreshold: '',
    meterRollingFromCompletion: true,
    meterMissingDataBehavior: 'warn',
    oneTimeDueDate: '',
  }
}

function humanize(value: string | null | undefined): string {
  if (!value) {
    return 'Not recorded'
  }
  return value
    .replace(/[_-]+/g, ' ')
    .replace(/\b\w/g, (char) => char.toUpperCase())
}

function formatDate(value: string | null | undefined): string {
  if (!value) {
    return 'Not recorded'
  }
  const parsed = new Date(value)
  if (Number.isNaN(parsed.getTime())) {
    return 'Not recorded'
  }
  return parsed.toLocaleDateString(undefined, {
    month: 'short',
    day: '2-digit',
    year: 'numeric',
  })
}

function trimToNull(value: string): string | null {
  const trimmed = value.trim()
  return trimmed.length > 0 ? trimmed : null
}

function splitDelimitedLines(value: string): string[] {
  return value
    .split(/[\n,;]+/)
    .map((part) => part.trim())
    .filter(Boolean)
}

function parsePositiveNumber(value: string): number | null {
  const trimmed = value.trim()
  if (!trimmed) {
    return null
  }
  const parsed = Number(trimmed)
  return Number.isFinite(parsed) ? parsed : null
}

function parsePositiveInteger(value: string): number | null {
  const parsed = parsePositiveNumber(value)
  if (parsed == null) {
    return null
  }
  return Math.trunc(parsed)
}

function titleTemplateForPattern(pattern: string, prefix: string): string {
  const token = (() => {
    switch (pattern) {
      case 'program_asset_tag':
        return '{{programName}} - {{assetTag}}'
      case 'program_asset_name':
        return '{{programName}} - {{assetName}}'
      case 'program_asset_tag_name':
        return '{{programName}} - {{assetTag}} - {{assetName}}'
      default:
        return '{{programName}}'
    }
  })()

  const prefixValue = prefix.trim()
  return prefixValue ? `${prefixValue} - ${token}` : token
}

function renderTitlePreview(
  pattern: string,
  prefix: string,
  programName: string,
  sampleAsset: AssetResponse | null,
): string {
  const prefixValue = prefix.trim()
  const assetTag = sampleAsset?.assetTag ?? 'Asset #'
  const assetName = sampleAsset?.name ?? 'Asset name'
  const tokenPreview = (() => {
    switch (pattern) {
      case 'program_asset_tag':
        return `${programName} - ${assetTag}`
      case 'program_asset_name':
        return `${programName} - ${assetName}`
      case 'program_asset_tag_name':
        return `${programName} - ${assetTag} - ${assetName}`
      default:
        return programName
    }
  })()

  return prefixValue ? `${prefixValue} - ${tokenPreview}` : tokenPreview
}

function toneForStatus(value: string | null | undefined): Tone {
  const normalized = value?.toLowerCase() ?? ''
  if (['active', 'completed', 'ready', 'passed', 'good'].includes(normalized)) return 'good'
  if (['draft', 'scheduled', 'upcoming', 'pending', 'warn', 'due'].includes(normalized)) return 'warn'
  if (['overdue', 'failed', 'blocked', 'inactive', 'retired', 'paused'].includes(normalized)) return 'bad'
  return 'neutral'
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

function mapSimpleOptions(items: Array<{ value: string; label: string }>): PickerOption[] {
  return items.map((item) => ({ value: item.value, label: item.label }))
}

function useDebouncedValue<T>(value: T, delayMs: number): T {
  const [debounced, setDebounced] = useState(value)

  useEffect(() => {
    const handle = window.setTimeout(() => setDebounced(value), delayMs)
    return () => window.clearTimeout(handle)
  }, [value, delayMs])

  return debounced
}

function MultiValueSearchField({
  label,
  placeholder,
  options,
  values,
  onChange,
  disabled,
  testId,
  hint,
}: {
  label: string
  placeholder: string
  options: PickerOption[]
  values: string[]
  onChange: (values: string[]) => void
  disabled?: boolean
  testId: string
  hint?: string
}) {
  const [selectedValue, setSelectedValue] = useState('')
  const optionLookup = useMemo(
    () => new Map(options.map((option) => [option.value, option.label])),
    [options],
  )

  const addValue = () => {
    const nextValue = selectedValue.trim()
    if (!nextValue || values.includes(nextValue)) {
      return
    }
    onChange([...values, nextValue])
    setSelectedValue('')
  }

  return (
    <div className="space-y-2">
      <StaticSearchPicker
        label={label}
        value={selectedValue}
        onChange={setSelectedValue}
        options={options}
        placeholder={placeholder}
        disabled={disabled}
        testId={testId}
      />
      <div className="flex flex-wrap items-center gap-2">
        <button
          type="button"
          className="inline-flex items-center gap-2 rounded-full border px-3 py-1 text-xs font-semibold text-slate-100 disabled:cursor-not-allowed disabled:opacity-40"
          style={{ borderColor: theme.border, backgroundColor: theme.primary }}
          disabled={disabled || !selectedValue.trim() || values.includes(selectedValue.trim())}
          onClick={addValue}
        >
          <CirclePlus className="h-3.5 w-3.5" />
          Add
        </button>
        {values.length === 0 ? (
          <span className="text-xs text-slate-400">{hint ?? 'No selections yet.'}</span>
        ) : (
          values.map((value) => {
            const labelValue = optionLookup.get(value) ?? value
            return (
              <button
                key={value}
                type="button"
                className="inline-flex items-center gap-2 rounded-full border px-3 py-1 text-xs text-slate-100"
                style={{ borderColor: theme.border, backgroundColor: 'var(--color-bg-surface-elevated)' }}
                disabled={disabled}
                onClick={() => onChange(values.filter((current) => current !== value))}
              >
                <span>{labelValue}</span>
                <Trash2 className="h-3 w-3 text-slate-400" />
              </button>
            )
          })
        )}
      </div>
    </div>
  )
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
  icon: React.ReactNode
  onToggle: () => void
  children: React.ReactNode
  footer?: React.ReactNode
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
          locked ? 'cursor-not-allowed opacity-70' : 'hover:bg-[var(--color-bg-control-hover)]'
        }`}
        disabled={locked}
        onClick={onToggle}
      >
        <div className="min-w-0">
          <div className="flex items-center gap-3">
            <div
              className="flex h-11 w-11 shrink-0 items-center justify-center rounded-2xl border"
              style={{ borderColor: theme.border, backgroundColor: 'var(--color-bg-surface-elevated)', color: theme.secondary }}
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

function buildRequestPayload({
  programName,
  programKey,
  description,
  categoryKey,
  workTypeKey,
  priorityKey,
  owningSiteRef,
  owningTeamRef,
  owningDepartmentRef,
  ownerPersonId,
  ownerRoleKey,
  tagsText,
  assetClassKeys,
  assetTypeIds,
  assetCategoryKeys,
  assetStatusKeys,
  readinessStateKeys,
  siteRefs,
  departmentRefs,
  makeKeys,
  modelKeys,
  yearFrom,
  yearTo,
  fuelTypeKeys,
  scopeTagsText,
  includedAssetIds,
  excludedAssetIds,
  manualOnlyProgram,
  dueTriggers,
  dueMatchLogic,
  warnWhenAnyApproaching,
  markDueBasedOnMostUrgent,
  generateWorkOrder,
  workOrderTitlePattern,
  workOrderTitlePrefix,
  workOrderDescription,
  defaultWorkOrderPriority,
  defaultWorkType,
  estimatedLaborHours,
  requiredSkillsText,
  safetyNotesText,
  technicianNotesText,
  requiredAttachmentsText,
  partsDemandText,
  checklistTasksText,
  attachInspectionTemplate,
  inspectionTemplateId,
  inspectionRequiredBeforeWorkOrderCompletion,
  inspectionResultBehavior,
  resumeBehaviorRespectsEngineRules,
  voiceCompatible,
  isComplianceRelated,
  governingBodyCatalogKey,
  citationReferencesText,
  readinessImpact,
  certificateRequirementsText,
  autoGenerateWorkOrder,
  autoGenerateInspection,
  leadTimeDays,
  leadThresholdValue,
  leadThresholdUnit,
  duplicatePreventionWindowDays,
  assignmentBehavior,
  assignmentTeamRef,
  assignmentPersonId,
  assignmentRoleKey,
  notificationTargetsText,
  escalationTargetsText,
  blackoutWindowsText,
  maxOpenGeneratedItemsPerAsset,
}: {
  programName: string
  programKey: string
  description: string
  categoryKey: string
  workTypeKey: string
  priorityKey: string
  owningSiteRef: string
  owningTeamRef: string
  owningDepartmentRef: string
  ownerPersonId: string
  ownerRoleKey: string
  tagsText: string
  assetClassKeys: string[]
  assetTypeIds: string[]
  assetCategoryKeys: string[]
  assetStatusKeys: string[]
  readinessStateKeys: string[]
  siteRefs: string[]
  departmentRefs: string[]
  makeKeys: string[]
  modelKeys: string[]
  yearFrom: string
  yearTo: string
  fuelTypeKeys: string[]
  scopeTagsText: string
  includedAssetIds: string[]
  excludedAssetIds: string[]
  manualOnlyProgram: boolean
  dueTriggers: TriggerDraft[]
  dueMatchLogic: string
  warnWhenAnyApproaching: boolean
  markDueBasedOnMostUrgent: boolean
  generateWorkOrder: boolean
  workOrderTitlePattern: string
  workOrderTitlePrefix: string
  workOrderDescription: string
  defaultWorkOrderPriority: string
  defaultWorkType: string
  estimatedLaborHours: string
  requiredSkillsText: string
  safetyNotesText: string
  technicianNotesText: string
  requiredAttachmentsText: string
  partsDemandText: string
  checklistTasksText: string
  attachInspectionTemplate: boolean
  inspectionTemplateId: string
  inspectionRequiredBeforeWorkOrderCompletion: boolean
  inspectionResultBehavior: string
  resumeBehaviorRespectsEngineRules: boolean
  voiceCompatible: boolean
  isComplianceRelated: boolean
  governingBodyCatalogKey: string
  citationReferencesText: string
  readinessImpact: string
  certificateRequirementsText: string
  autoGenerateWorkOrder: boolean
  autoGenerateInspection: boolean
  leadTimeDays: string
  leadThresholdValue: string
  leadThresholdUnit: string
  duplicatePreventionWindowDays: string
  assignmentBehavior: string
  assignmentTeamRef: string
  assignmentPersonId: string
  assignmentRoleKey: string
  notificationTargetsText: string
  escalationTargetsText: string
  blackoutWindowsText: string
  maxOpenGeneratedItemsPerAsset: string
}): CreatePmProgramRequest {
  const buildScopeDefinition = () => {
    const yearFromNumber = parsePositiveInteger(yearFrom)
    const yearToNumber = parsePositiveInteger(yearTo)

    return {
      assetClassKeys: assetClassKeys.length > 0 ? assetClassKeys : null,
      assetTypeIds: assetTypeIds.length > 0 ? assetTypeIds : null,
      assetCategoryKeys: assetCategoryKeys.length > 0 ? assetCategoryKeys : null,
      assetStatusKeys: assetStatusKeys.length > 0 ? assetStatusKeys : null,
      readinessStateKeys: readinessStateKeys.length > 0 ? readinessStateKeys : null,
      siteRefs: siteRefs.length > 0 ? siteRefs : null,
      departmentRefs: departmentRefs.length > 0 ? departmentRefs : null,
      locationRefs: null,
      makeKeys: makeKeys.length > 0 ? makeKeys : null,
      modelKeys: modelKeys.length > 0 ? modelKeys : null,
      yearFrom: yearFromNumber,
      yearTo: yearToNumber,
      fuelTypeKeys: fuelTypeKeys.length > 0 ? fuelTypeKeys : null,
      tags: splitDelimitedLines(scopeTagsText).length > 0 ? splitDelimitedLines(scopeTagsText) : null,
      includedAssetIds: includedAssetIds.length > 0 ? includedAssetIds : null,
      excludedAssetIds: excludedAssetIds.length > 0 ? excludedAssetIds : null,
    }
  }

  const buildDueDefinition = () => {
    const buildManualTrigger = () => ({
      triggerType: 'manual',
      calendar: null,
      meter: null,
      oneTime: null,
      manualOnly: true,
    })

    const buildCalendarTrigger = (trigger: TriggerDraft) => ({
      triggerType: 'calendar',
      calendar: {
        intervalValue: parsePositiveInteger(trigger.calendarIntervalValue) ?? 0,
        intervalUnit: trigger.calendarIntervalUnit,
        anchorDate: trigger.calendarAnchorDate || null,
        firstDueDate: trigger.calendarFirstDueDate || null,
        calendarBehavior: trigger.calendarBehavior,
        earlyWindowDays: parsePositiveInteger(trigger.calendarEarlyWindowDays) ?? 0,
        gracePeriodDays: parsePositiveInteger(trigger.calendarGracePeriodDays) ?? 0,
        pastDueBehavior: trigger.calendarPastDueBehavior,
      },
      meter: null,
      oneTime: null,
      manualOnly: false,
    })

    const buildMeterTrigger = (trigger: TriggerDraft) => ({
      triggerType: 'meter',
      calendar: null,
      meter: {
        intervalValue: parsePositiveNumber(trigger.meterIntervalValue) ?? 0,
        intervalUnit: trigger.meterIntervalUnit,
        anchorReading: parsePositiveNumber(trigger.meterAnchorReading),
        firstDueReading: parsePositiveNumber(trigger.meterFirstDueReading),
        currentReadingSource: trigger.meterCurrentReadingSource,
        earlyThreshold: parsePositiveNumber(trigger.meterEarlyThreshold),
        graceThreshold: parsePositiveNumber(trigger.meterGraceThreshold),
        rollingFromCompletion: trigger.meterRollingFromCompletion,
        missingDataBehavior: trigger.meterMissingDataBehavior,
      },
      oneTime: null,
      manualOnly: false,
    })

    const buildOneTimeTrigger = (trigger: TriggerDraft) => ({
      triggerType: 'one_time',
      calendar: null,
      meter: null,
      oneTime: {
        dueDate: trigger.oneTimeDueDate,
      },
      manualOnly: false,
    })

    const isCompleteTrigger = (trigger: TriggerDraft): boolean => {
      switch (trigger.triggerType) {
        case 'manual':
          return true
        case 'one_time':
          return Boolean(trigger.oneTimeDueDate.trim())
        case 'meter':
          return Boolean(trigger.meterIntervalValue.trim())
        default:
          return Boolean(trigger.calendarIntervalValue.trim())
      }
    }

    const triggers = manualOnlyProgram
      ? [buildManualTrigger()]
      : dueTriggers
          .filter(isCompleteTrigger)
          .map((trigger) => {
            if (trigger.triggerType === 'manual') {
              return buildManualTrigger()
            }

            if (trigger.triggerType === 'meter') {
              return buildMeterTrigger(trigger)
            }

            if (trigger.triggerType === 'one_time') {
              return buildOneTimeTrigger(trigger)
            }

            return buildCalendarTrigger(trigger)
          })

    return {
      matchLogic: dueMatchLogic,
      triggers,
      warnWhenAnyApproaching,
      markDueBasedOnMostUrgent,
    }
  }

  const buildWorkPackageDefinition = () => ({
    generateWorkOrder,
    workOrderTitleTemplate: generateWorkOrder ? titleTemplateForPattern(workOrderTitlePattern, workOrderTitlePrefix) : null,
    workOrderDescription: generateWorkOrder ? trimToNull(workOrderDescription) : null,
    defaultPriority: generateWorkOrder ? trimToNull(defaultWorkOrderPriority) : null,
    defaultWorkType: generateWorkOrder ? trimToNull(defaultWorkType) : null,
    estimatedLaborHours: generateWorkOrder ? parsePositiveNumber(estimatedLaborHours) : null,
    requiredSkills: generateWorkOrder ? splitDelimitedLines(requiredSkillsText) : null,
    safetyNotes: generateWorkOrder ? splitDelimitedLines(safetyNotesText) : null,
    technicianNotes: generateWorkOrder ? splitDelimitedLines(technicianNotesText) : null,
    requiredAttachments: generateWorkOrder ? splitDelimitedLines(requiredAttachmentsText) : null,
    partsDemand: generateWorkOrder
      ? parsePartsDemand(partsDemandText)
      : null,
    checklistTasks: generateWorkOrder
      ? parseChecklistTasks(checklistTasksText)
      : null,
  })

  const buildInspectionDefinition = () => ({
    attachInspectionTemplate,
    inspectionTemplateId: attachInspectionTemplate ? trimToNull(inspectionTemplateId) : null,
    inspectionRequiredBeforeWorkOrderCompletion: attachInspectionTemplate
      ? inspectionRequiredBeforeWorkOrderCompletion
      : false,
    inspectionResultBehavior: attachInspectionTemplate ? trimToNull(inspectionResultBehavior) ?? undefined : undefined,
    resumeBehaviorRespectsEngineRules: attachInspectionTemplate
      ? resumeBehaviorRespectsEngineRules
      : undefined,
    voiceCompatible: attachInspectionTemplate ? voiceCompatible : false,
  })

  const buildComplianceDefinition = () => ({
    isComplianceRelated,
    governingBodyCatalogKey: isComplianceRelated ? trimToNull(governingBodyCatalogKey) : null,
    citationReferences: splitDelimitedLines(citationReferencesText).length > 0 ? splitDelimitedLines(citationReferencesText) : null,
    readinessImpact,
    certificateRequirements: splitDelimitedLines(certificateRequirementsText).length > 0
      ? splitDelimitedLines(certificateRequirementsText)
      : null,
  })

  const buildAutomationDefinition = () => ({
    leadTimeDays: parsePositiveInteger(leadTimeDays) ?? 0,
    leadThresholdValue: parsePositiveNumber(leadThresholdValue),
    leadThresholdUnit: trimToNull(leadThresholdUnit),
    duplicatePreventionWindowDays: parsePositiveInteger(duplicatePreventionWindowDays) ?? 1,
    assignmentBehavior,
    assignmentRef:
      assignmentBehavior === 'team'
        ? trimToNull(assignmentTeamRef)
        : assignmentBehavior === 'person'
          ? trimToNull(assignmentPersonId)
          : assignmentBehavior === 'role'
            ? trimToNull(assignmentRoleKey)
            : null,
    notificationTargets: splitDelimitedLines(notificationTargetsText).length > 0
      ? splitDelimitedLines(notificationTargetsText)
      : null,
    escalationTargets: splitDelimitedLines(escalationTargetsText).length > 0
      ? splitDelimitedLines(escalationTargetsText)
      : null,
    blackoutWindows: splitDelimitedLines(blackoutWindowsText).length > 0
      ? splitDelimitedLines(blackoutWindowsText)
      : null,
    maxOpenGeneratedItemsPerAsset: parsePositiveInteger(maxOpenGeneratedItemsPerAsset),
  })

  return {
    programKey,
    name: programName,
    description,
    scopeType: 'custom',
    assetTypeId: null,
    assetId: null,
    pmScheduleIds: null,
    autoGenerateWorkOrder,
    defaultWorkOrderTemplateRef: null,
    autoGenerateInspection,
    inspectionTemplateId:
      attachInspectionTemplate || autoGenerateInspection ? trimToNull(inspectionTemplateId) : null,
    categoryKey,
    workTypeKey,
    priorityKey,
    owningSiteRef: trimToNull(owningSiteRef),
    owningTeamRef: trimToNull(owningTeamRef),
    owningDepartmentRef: trimToNull(owningDepartmentRef),
    ownerPersonId: trimToNull(ownerPersonId),
    ownerRoleKey: trimToNull(ownerRoleKey),
    tags: splitDelimitedLines(tagsText).length > 0 ? splitDelimitedLines(tagsText) : null,
    scopeDefinition: buildScopeDefinition(),
    dueDefinition: buildDueDefinition(),
    workPackageDefinition: buildWorkPackageDefinition(),
    inspectionDefinition: buildInspectionDefinition(),
    complianceDefinition: buildComplianceDefinition(),
    automationDefinition: buildAutomationDefinition(),
  }
}

function parsePartsDemand(text: string) {
  return splitDelimitedLines(text).map((line, index) => {
    const [itemRef, quantityValue, unitOfMeasure, description] = line.split('|').map((part) => part.trim())
    return {
      itemRef: itemRef || `part-${index + 1}`,
      description: description || itemRef || `Part ${index + 1}`,
      quantity: parsePositiveNumber(quantityValue || '1') ?? 1,
      unitOfMeasure: unitOfMeasure || 'each',
    }
  })
}

function parseChecklistTasks(text: string) {
  return splitDelimitedLines(text).map((line, index) => {
    const [taskKey, title, description, sortOrder] = line.split('|').map((part) => part.trim())
    return {
      taskKey: taskKey || `task-${index + 1}`,
      title: title || taskKey || `Task ${index + 1}`,
      description: description || null,
      sortOrder: parsePositiveInteger(sortOrder || `${index + 1}`) ?? index + 1,
    }
  })
}

function buildScopeSummary(
  matchedCount: number,
  excludedCount: number,
  siteLabels: string[],
  assetClassLabels: string[],
  assetTypeLabels: string[],
  assetCategoryLabels: string[],
): string {
  const labels = [...assetClassLabels, ...assetTypeLabels, ...assetCategoryLabels, ...siteLabels].filter(Boolean)
  if (labels.length === 0) {
    return 'Define scope filters to match assets.'
  }
  return `${matchedCount} matching assets · ${excludedCount} excluded · ${labels.slice(0, 3).join(', ')}`
}

function getTriggerSummary(trigger: TriggerDraft): string {
  switch (trigger.triggerType) {
    case 'meter':
      return `Meter trigger every ${trigger.meterIntervalValue || 'N'} ${humanize(trigger.meterIntervalUnit)}`
    case 'one_time':
      return trigger.oneTimeDueDate ? `One-time due ${formatDate(trigger.oneTimeDueDate)}` : 'One-time trigger'
    case 'manual':
      return 'Manual only'
    default:
      return `Calendar trigger every ${trigger.calendarIntervalValue || 'N'} ${humanize(trigger.calendarIntervalUnit)}`
  }
}

function getSectionTone(
  complete: boolean,
  warnings: string[],
  errors: string[],
  locked: boolean,
): Tone {
  if (locked) return 'neutral'
  if (errors.length > 0) return 'bad'
  if (warnings.length > 0) return 'warn'
  if (complete) return 'good'
  return 'info'
}

function hasText(value: string): boolean {
  return value.trim().length > 0
}

function isPositiveIntegerText(value: string): boolean {
  const parsed = parsePositiveInteger(value)
  return parsed != null && parsed > 0
}

function isNonNegativeIntegerText(value: string): boolean {
  const parsed = parsePositiveInteger(value)
  return parsed != null && parsed >= 0
}

function isPositiveNumberText(value: string): boolean {
  const parsed = parsePositiveNumber(value)
  return parsed != null && parsed > 0
}

function triggerHasContent(trigger: TriggerDraft): boolean {
  switch (trigger.triggerType) {
    case 'manual':
      return true
    case 'one_time':
      return hasText(trigger.oneTimeDueDate)
    case 'meter':
      return [
        trigger.meterIntervalValue,
        trigger.meterIntervalUnit,
        trigger.meterAnchorReading,
        trigger.meterFirstDueReading,
      ].some(hasText)
    default:
      return [
        trigger.calendarIntervalValue,
        trigger.calendarIntervalUnit,
        trigger.calendarAnchorDate,
        trigger.calendarFirstDueDate,
      ].some(hasText)
  }
}

function validateTriggerDraft(trigger: TriggerDraft): string[] {
  const errors: string[] = []
  switch (trigger.triggerType) {
    case 'manual':
      return errors
    case 'one_time':
      if (!hasText(trigger.oneTimeDueDate)) {
        errors.push('One-time trigger needs a due date.')
      }
      return errors
    case 'meter':
      if (!isPositiveNumberText(trigger.meterIntervalValue)) {
        errors.push('Meter interval must be a positive number.')
      }
      if (!hasText(trigger.meterIntervalUnit)) {
        errors.push('Meter unit is required.')
      }
      if (hasText(trigger.meterAnchorReading) && !isPositiveNumberText(trigger.meterAnchorReading)) {
        errors.push('Anchor reading must be a number.')
      }
      if (hasText(trigger.meterFirstDueReading) && !isPositiveNumberText(trigger.meterFirstDueReading)) {
        errors.push('First due reading must be a number.')
      }
      if (hasText(trigger.meterEarlyThreshold) && !isNonNegativeIntegerText(trigger.meterEarlyThreshold)) {
        errors.push('Early threshold must be zero or greater.')
      }
      if (hasText(trigger.meterGraceThreshold) && !isNonNegativeIntegerText(trigger.meterGraceThreshold)) {
        errors.push('Grace threshold must be zero or greater.')
      }
      return errors
    default:
      if (!isPositiveIntegerText(trigger.calendarIntervalValue)) {
        errors.push('Calendar interval must be a positive whole number.')
      }
      if (!hasText(trigger.calendarIntervalUnit)) {
        errors.push('Calendar unit is required.')
      }
      if (hasText(trigger.calendarEarlyWindowDays) && !isNonNegativeIntegerText(trigger.calendarEarlyWindowDays)) {
        errors.push('Early window must be zero or greater.')
      }
      if (hasText(trigger.calendarGracePeriodDays) && !isNonNegativeIntegerText(trigger.calendarGracePeriodDays)) {
        errors.push('Grace period must be zero or greater.')
      }
      return errors
  }
}

export function PmProgramCreatePage() {
  const session = loadSession()
  const navigate = useNavigate()
  const [serverError, setServerError] = useState<string | null>(null)
  const [openSectionIndex, setOpenSectionIndex] = useState(0)
  const [showKeyPolicy, setShowKeyPolicy] = useState(false)

  const [programName, setProgramName] = useState('')
  const [manualProgramKey, setManualProgramKey] = useState('')
  const [description, setDescription] = useState('')
  const [categoryKey, setCategoryKey] = useState('')
  const [workTypeKey, setWorkTypeKey] = useState('')
  const [priorityKey, setPriorityKey] = useState('')
  const [owningSiteRef, setOwningSiteRef] = useState('')
  const [owningTeamRef, setOwningTeamRef] = useState('')
  const [owningDepartmentRef, setOwningDepartmentRef] = useState('')
  const [ownerPersonId, setOwnerPersonId] = useState('')
  const [ownerRoleKey, setOwnerRoleKey] = useState('')
  const [tagsText, setTagsText] = useState('')

  const [assetClassKeys, setAssetClassKeys] = useState<string[]>([])
  const [assetTypeIds, setAssetTypeIds] = useState<string[]>([])
  const [assetCategoryKeys, setAssetCategoryKeys] = useState<string[]>([])
  const [assetStatusKeys, setAssetStatusKeys] = useState<string[]>([])
  const [readinessStateKeys, setReadinessStateKeys] = useState<string[]>([])
  const [siteRefs, setSiteRefs] = useState<string[]>([])
  const [departmentRefs, setDepartmentRefs] = useState<string[]>([])
  const [makeKeys, setMakeKeys] = useState<string[]>([])
  const [modelKeys, setModelKeys] = useState<string[]>([])
  const [fuelTypeKeys, setFuelTypeKeys] = useState<string[]>([])
  const [scopeTagsText, setScopeTagsText] = useState('')
  const [includedAssetIds, setIncludedAssetIds] = useState<string[]>([])
  const [excludedAssetIds, setExcludedAssetIds] = useState<string[]>([])
  const [yearFrom, setYearFrom] = useState('')
  const [yearTo, setYearTo] = useState('')

  const [manualOnlyProgram, setManualOnlyProgram] = useState(false)
  const [dueTriggers, setDueTriggers] = useState<TriggerDraft[]>([])
  const [dueMatchLogic, setDueMatchLogic] = useState('any')
  const [warnWhenAnyApproaching, setWarnWhenAnyApproaching] = useState(true)
  const [markDueBasedOnMostUrgent, setMarkDueBasedOnMostUrgent] = useState(false)

  const [generateWorkOrder, setGenerateWorkOrder] = useState(false)
  const [workOrderTitlePattern, setWorkOrderTitlePattern] = useState('program_asset_tag')
  const [workOrderTitlePrefix, setWorkOrderTitlePrefix] = useState('')
  const [workOrderDescription, setWorkOrderDescription] = useState('')
  const [defaultWorkOrderPriority, setDefaultWorkOrderPriority] = useState('')
  const [defaultWorkType, setDefaultWorkType] = useState('')
  const [estimatedLaborHours, setEstimatedLaborHours] = useState('')
  const [requiredSkillsText, setRequiredSkillsText] = useState('')
  const [safetyNotesText, setSafetyNotesText] = useState('')
  const [technicianNotesText, setTechnicianNotesText] = useState('')
  const [requiredAttachmentsText, setRequiredAttachmentsText] = useState('')
  const [partsDemandText, setPartsDemandText] = useState('')
  const [partSuggestionRef, setPartSuggestionRef] = useState('')
  const [checklistTasksText, setChecklistTasksText] = useState('')

  const [attachInspectionTemplate, setAttachInspectionTemplate] = useState(false)
  const [inspectionTemplateId, setInspectionTemplateId] = useState('')
  const [inspectionRequiredBeforeWorkOrderCompletion, setInspectionRequiredBeforeWorkOrderCompletion] = useState(false)
  const [inspectionResultBehavior, setInspectionResultBehavior] = useState<InspectionResultBehavior>('pass_completes')
  const [resumeBehaviorRespectsEngineRules, setResumeBehaviorRespectsEngineRules] = useState(true)
  const [voiceCompatible, setVoiceCompatible] = useState(true)

  const [isComplianceRelated, setIsComplianceRelated] = useState(false)
  const [governingBodyCatalogKey, setGoverningBodyCatalogKey] = useState('')
  const [citationReferencesText, setCitationReferencesText] = useState('')
  const [readinessImpact, setReadinessImpact] = useState<ReadinessImpact>('no_impact')
  const [certificateRequirementsText, setCertificateRequirementsText] = useState('')

  const [autoGenerateWorkOrder, setAutoGenerateWorkOrder] = useState(false)
  const [autoGenerateInspection, setAutoGenerateInspection] = useState(false)
  const [leadTimeDays, setLeadTimeDays] = useState('')
  const [leadThresholdValue, setLeadThresholdValue] = useState('')
  const [leadThresholdUnit, setLeadThresholdUnit] = useState('miles')
  const [duplicatePreventionWindowDays, setDuplicatePreventionWindowDays] = useState('')
  const [assignmentBehavior, setAssignmentBehavior] = useState<AssignmentBehavior>('unassigned')
  const [assignmentTeamRef, setAssignmentTeamRef] = useState('')
  const [assignmentPersonId, setAssignmentPersonId] = useState('')
  const [assignmentRoleKey, setAssignmentRoleKey] = useState('')
  const [notificationTargetsText, setNotificationTargetsText] = useState('')
  const [escalationTargetsText, setEscalationTargetsText] = useState('')
  const [blackoutWindowsText, setBlackoutWindowsText] = useState('')
  const [maxOpenGeneratedItemsPerAsset, setMaxOpenGeneratedItemsPerAsset] = useState('')

  const [confirmReadinessImpact, setConfirmReadinessImpact] = useState(false)
  const [confirmComplianceImpact, setConfirmComplianceImpact] = useState(false)
  const [pendingDraftId, setPendingDraftId] = useState<string | null>(null)

  const meQuery = useQuery({
    queryKey: ['maintainarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const canCreate = meQuery.data
    ? canCreatePmPrograms(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false
  const canActivate = meQuery.data
    ? canActivatePmPrograms(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false
  const canManageAutomation = meQuery.data
    ? canManagePmProgramAutomation(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false

  const baseDataEnabled = Boolean(session?.accessToken && canCreate)

  const existingProgramsQuery = useQuery({
    queryKey: ['maintainarr-pm-programs', session?.accessToken],
    queryFn: () => getPmPrograms(session!.accessToken),
    enabled: baseDataEnabled,
    retry: false,
  })

  const catalogsQuery = useQuery({
    queryKey: ['maintainarr-pm-program-catalogs', session?.accessToken],
    queryFn: () =>
      getCatalogs(session!.accessToken, [
        'PMProgram',
        'PMProgramStatus',
        'PMType',
        'priority',
        'workOrderPriority',
        'workOrderType',
        'assetStatus',
        'readinessStatus',
        'make',
        'model',
        'fuelType',
        'meterUnit',
        'meterReadingSource',
      ]),
    enabled: baseDataEnabled,
    retry: false,
  })

  const assetClassesQuery = useQuery({
    queryKey: ['maintainarr-asset-classes', session?.accessToken],
    queryFn: () => getAssetClasses(session!.accessToken),
    enabled: baseDataEnabled,
    retry: false,
  })

  const assetTypesQuery = useQuery({
    queryKey: ['maintainarr-asset-types', session?.accessToken],
    queryFn: () => getAssetTypes(session!.accessToken),
    enabled: baseDataEnabled,
    retry: false,
  })

  const assetsQuery = useQuery({
    queryKey: ['maintainarr-assets', session?.accessToken],
    queryFn: () => getAssets(session!.accessToken),
    enabled: baseDataEnabled,
    retry: false,
  })

  const sitesQuery = useQuery({
    queryKey: ['maintainarr-sites', session?.accessToken],
    queryFn: () => getSites(session!.accessToken),
    enabled: baseDataEnabled,
    retry: false,
  })

  const departmentsQuery = useQuery({
    queryKey: ['maintainarr-departments', session?.accessToken],
    queryFn: () => getDepartments(session!.accessToken),
    enabled: baseDataEnabled,
    retry: false,
  })

  const teamsQuery = useQuery({
    queryKey: ['maintainarr-teams', session?.accessToken],
    queryFn: () => getTeams(session!.accessToken),
    enabled: baseDataEnabled,
    retry: false,
  })

  const peopleQuery = useQuery({
    queryKey: ['maintainarr-people', session?.accessToken],
    queryFn: () => getPeople(session!.accessToken),
    enabled: baseDataEnabled,
    retry: false,
  })

  const inspectionTemplatesQuery = useQuery({
    queryKey: ['maintainarr-inspection-templates', session?.accessToken],
    queryFn: () => getInspectionTemplates(session!.accessToken),
    enabled: baseDataEnabled,
    retry: false,
  })

  const governingBodiesQuery = useQuery({
    queryKey: ['maintainarr-governing-bodies', session?.accessToken],
    queryFn: () => getComplianceCoreCatalogOptions(session!.accessToken, 'governingBody'),
    enabled: baseDataEnabled,
    retry: false,
  })

  const partsQuery = useQuery({
    queryKey: ['maintainarr-parts', session?.accessToken],
    queryFn: () => getParts(session!.accessToken),
    enabled: baseDataEnabled,
    retry: false,
  })

  const selectedInspectionTemplateQuery = useQuery({
    queryKey: ['maintainarr-inspection-template-detail', session?.accessToken, inspectionTemplateId],
    queryFn: () => getInspectionTemplate(session!.accessToken, inspectionTemplateId),
    enabled: baseDataEnabled && Boolean(inspectionTemplateId),
    retry: false,
  })

  const generatedProgramKey = useMemo(
    () =>
      buildSemanticKey({
        domain: 'maintainarr',
        kind: 'pm-program',
        title: programName.trim(),
        existingKeys: (existingProgramsQuery.data ?? []).map((program) => program.programKey),
        maxLength: 128,
      }),
    [existingProgramsQuery.data, programName],
  )
  const effectiveProgramKey = manualProgramKey.trim() || generatedProgramKey
  const createPayloadInputs = {
    programName,
    programKey: effectiveProgramKey,
    description,
    categoryKey,
    workTypeKey,
    priorityKey,
    owningSiteRef,
    owningTeamRef,
    owningDepartmentRef,
    ownerPersonId,
    ownerRoleKey,
    tagsText,
    assetClassKeys,
    assetTypeIds,
    assetCategoryKeys,
    assetStatusKeys,
    readinessStateKeys,
    siteRefs,
    departmentRefs,
    makeKeys,
    modelKeys,
    yearFrom,
    yearTo,
    fuelTypeKeys,
    scopeTagsText,
    includedAssetIds,
    excludedAssetIds,
    manualOnlyProgram,
    dueTriggers,
    dueMatchLogic,
    warnWhenAnyApproaching,
    markDueBasedOnMostUrgent,
    generateWorkOrder,
    workOrderTitlePattern,
    workOrderTitlePrefix,
    workOrderDescription,
    defaultWorkOrderPriority,
    defaultWorkType,
    estimatedLaborHours,
    requiredSkillsText,
    safetyNotesText,
    technicianNotesText,
    requiredAttachmentsText,
    partsDemandText,
    checklistTasksText,
    attachInspectionTemplate,
    inspectionTemplateId,
    inspectionRequiredBeforeWorkOrderCompletion,
    inspectionResultBehavior,
    resumeBehaviorRespectsEngineRules,
    voiceCompatible,
    isComplianceRelated,
    governingBodyCatalogKey,
    citationReferencesText,
    readinessImpact,
    certificateRequirementsText,
    autoGenerateWorkOrder,
    autoGenerateInspection,
    leadTimeDays,
    leadThresholdValue,
    leadThresholdUnit,
    duplicatePreventionWindowDays,
    assignmentBehavior,
    assignmentTeamRef,
    assignmentPersonId,
    assignmentRoleKey,
    notificationTargetsText,
    escalationTargetsText,
    blackoutWindowsText,
    maxOpenGeneratedItemsPerAsset,
  }

  const createPayload = useMemo(
    () => buildRequestPayload(createPayloadInputs),
    [JSON.stringify(createPayloadInputs)],
  )
  const debouncedCreatePayload = useDebouncedValue(createPayload, 450)
  useEffect(() => {
    if (pendingDraftId) {
      setPendingDraftId(null)
    }
  }, [createPayload])
  const canPreview = meQuery.data
    ? canPreviewPmPrograms(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false

  const assetClassOptions = useMemo(
    () => mapSimpleOptions((assetClassesQuery.data ?? []).map((item) => ({ value: item.classKey, label: item.name }))),
    [assetClassesQuery.data],
  )
  const assetTypeOptions = useMemo(
    () =>
      mapSimpleOptions(
        (assetTypesQuery.data ?? []).map((item) => ({
          value: item.assetTypeId,
          label: `${item.name} · ${item.className}`,
        })),
      ),
    [assetTypesQuery.data],
  )
  const assetOptions = useMemo(() => {
    const siteLookup = new Map((sitesQuery.data ?? []).map((site) => [site.key, site.label]))
    return mapSimpleOptions(
      (assetsQuery.data ?? []).map((asset) => ({
        value: asset.assetId,
        label: [asset.assetTag, asset.name, siteLookup.get(asset.siteRef ?? '')].filter(Boolean).join(' · '),
      })),
    )
  }, [assetsQuery.data, sitesQuery.data])
  const siteOptions = useMemo(() => mapReferenceOptions(sitesQuery.data), [sitesQuery.data])
  const departmentOptions = useMemo(() => mapReferenceOptions(departmentsQuery.data), [departmentsQuery.data])
  const teamOptions = useMemo(() => mapReferenceOptions(teamsQuery.data), [teamsQuery.data])
  const personOptions = useMemo(() => mapReferenceOptions(peopleQuery.data), [peopleQuery.data])
  const governingBodyOptions = useMemo(() => mapReferenceOptions(governingBodiesQuery.data), [governingBodiesQuery.data])
  const partOptions = useMemo(() => mapReferenceOptions(partsQuery.data), [partsQuery.data])
  const inspectionTemplateOptions = useMemo(
    () =>
      (inspectionTemplatesQuery.data ?? []).map((template) => ({
        value: template.inspectionTemplateId,
        label: `${template.name} · v${template.version} · ${humanize(template.status)}`,
        inactive: template.status.toLowerCase() !== 'active',
      })),
    [inspectionTemplatesQuery.data],
  )
  const programCategoryOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'PMProgram'), [catalogsQuery.data])
  const workTypeOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'PMType'), [catalogsQuery.data])
  const priorityOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'priority'), [catalogsQuery.data])
  const workOrderPriorityOptions = useMemo(
    () => mapCatalogOptions(catalogsQuery.data, 'workOrderPriority'),
    [catalogsQuery.data],
  )
  const workOrderTypeOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'workOrderType'), [catalogsQuery.data])
  const assetStatusOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'assetStatus'), [catalogsQuery.data])
  const readinessStateOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'readinessStatus'), [catalogsQuery.data])
  const assetCategoryOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'assetCategory'), [catalogsQuery.data])
  const makeOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'make'), [catalogsQuery.data])
  const modelOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'model'), [catalogsQuery.data])
  const fuelTypeOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'fuelType'), [catalogsQuery.data])
  const meterUnitOptions = useMemo(() => mapCatalogOptions(catalogsQuery.data, 'meterUnit'), [catalogsQuery.data])
  const meterReadingSourceOptions = useMemo(
    () => mapCatalogOptions(catalogsQuery.data, 'meterReadingSource'),
    [catalogsQuery.data],
  )

  const basicInputsReady =
    hasText(programName) && hasText(categoryKey) && hasText(workTypeKey) && hasText(priorityKey)
  const scopeHasSelections =
    assetClassKeys.length > 0 ||
    assetTypeIds.length > 0 ||
    assetCategoryKeys.length > 0 ||
    assetStatusKeys.length > 0 ||
    readinessStateKeys.length > 0 ||
    siteRefs.length > 0 ||
    departmentRefs.length > 0 ||
    makeKeys.length > 0 ||
    modelKeys.length > 0 ||
    fuelTypeKeys.length > 0 ||
    includedAssetIds.length > 0 ||
    excludedAssetIds.length > 0 ||
    hasText(yearFrom) ||
    hasText(yearTo) ||
    hasText(scopeTagsText)
  const dueHasSelections = manualOnlyProgram || dueTriggers.some(triggerHasContent)

  const scopePreviewQuery = useQuery<PmProgramScopePreviewResponse>({
    queryKey: ['maintainarr-pm-program-scope-preview', debouncedCreatePayload],
    queryFn: () => previewPmProgramScope(session!.accessToken, debouncedCreatePayload),
    enabled: Boolean(session?.accessToken && canPreview && basicInputsReady && scopeHasSelections),
    retry: false,
  })

  const duePreviewQuery = useQuery<PmProgramDuePreviewResponse>({
    queryKey: ['maintainarr-pm-program-due-preview', debouncedCreatePayload],
    queryFn: () => previewPmProgramDue(session!.accessToken, debouncedCreatePayload),
    enabled: Boolean(
      session?.accessToken && canPreview && basicInputsReady && scopeHasSelections && dueHasSelections,
    ),
    retry: false,
  })

  const selectedInspectionTemplate = selectedInspectionTemplateQuery.data ?? null
  const selectedTemplateAssetTypeIds = new Set(
    (selectedInspectionTemplate?.linkedAssetTypes ?? []).map((item) => item.assetTypeId),
  )
  const sampledAssetTypeIds = (scopePreviewQuery.data?.sampleAssets ?? [])
    .map((sample) => assetsQuery.data?.find((asset) => asset.assetId === sample.assetId)?.assetTypeId)
    .filter((value): value is string => Boolean(value))
  const templateMismatchDetected =
    attachInspectionTemplate &&
    Boolean(selectedInspectionTemplate) &&
    selectedTemplateAssetTypeIds.size > 0 &&
    sampledAssetTypeIds.some((assetTypeId) => !selectedTemplateAssetTypeIds.has(assetTypeId))
  const sampleAssetForTitle = useMemo(
    () =>
      (scopePreviewQuery.data?.sampleAssets ?? [])
        .map((sample) => assetsQuery.data?.find((asset) => asset.assetId === sample.assetId))
        .find((asset): asset is AssetResponse => Boolean(asset)) ?? assetsQuery.data?.[0] ?? null,
    [assetsQuery.data, scopePreviewQuery.data],
  )
  const titlePreview = renderTitlePreview(
    workOrderTitlePattern,
    workOrderTitlePrefix,
    programName.trim() || 'PM program',
    sampleAssetForTitle,
  )

  const basicErrors: string[] = []
  if (!hasText(programName)) basicErrors.push('Program name is required.')
  if (!hasText(categoryKey)) basicErrors.push('PM category is required.')
  if (!hasText(workTypeKey)) basicErrors.push('Work type is required.')
  if (!hasText(priorityKey)) basicErrors.push('Priority is required.')
  if (effectiveProgramKey.length < 3 || effectiveProgramKey.length > 128) {
    basicErrors.push('Program code must be between 3 and 128 characters.')
  }
  if (
    effectiveProgramKey &&
    (existingProgramsQuery.data ?? []).some(
      (program) => program.programKey.trim().toLowerCase() === effectiveProgramKey.trim().toLowerCase(),
    )
  ) {
    basicErrors.push('Program code already exists.')
  }
  if (description.length > 512) {
    basicErrors.push('Description must be 512 characters or fewer.')
  }

  const scopeErrors: string[] = []
  if (hasText(yearFrom) && !isNonNegativeIntegerText(yearFrom)) {
    scopeErrors.push('Year from must be zero or greater.')
  }
  if (hasText(yearTo) && !isNonNegativeIntegerText(yearTo)) {
    scopeErrors.push('Year to must be zero or greater.')
  }
  if (
    hasText(yearFrom) &&
    hasText(yearTo) &&
    Number(yearFrom) > Number(yearTo)
  ) {
    scopeErrors.push('Year range is invalid.')
  }
  if (includedAssetIds.some((id) => excludedAssetIds.includes(id))) {
    scopeErrors.push('An asset cannot be both included and excluded.')
  }
  if (!scopeHasSelections) {
    // Drafts can remain incomplete here, but the review step will block activation.
  }

  const dueErrors: string[] = []
  if (!manualOnlyProgram) {
    dueTriggers.forEach((trigger) => {
      validateTriggerDraft(trigger).forEach((message) =>
        dueErrors.push(`${getTriggerSummary(trigger)}: ${message}`),
      )
    })
    if (
      dueTriggers.some((trigger) => trigger.triggerType === 'manual') &&
      dueTriggers.length > 1
    ) {
      dueErrors.push('Manual-only due rules cannot be combined with other trigger types.')
    }
  }

  const workPackageErrors: string[] = []
  if (generateWorkOrder) {
    if (!hasText(workOrderTitlePattern)) {
      workPackageErrors.push('Work order title pattern is required.')
    }
    if (!hasText(defaultWorkOrderPriority)) {
      workPackageErrors.push('Default work order priority is required.')
    }
    if (!hasText(defaultWorkType)) {
      workPackageErrors.push('Default work type is required.')
    }
    if (hasText(estimatedLaborHours) && !isPositiveNumberText(estimatedLaborHours)) {
      workPackageErrors.push('Estimated labor hours must be a positive number.')
    }
  }

  const inspectionErrors: string[] = []
  if (attachInspectionTemplate) {
    if (!hasText(inspectionTemplateId)) {
      inspectionErrors.push('Choose an inspection template.')
    }
    if (selectedInspectionTemplateQuery.isSuccess && selectedInspectionTemplate) {
      if (selectedInspectionTemplate.status.toLowerCase() !== 'active') {
        inspectionErrors.push('Inspection template must be active.')
      }
      if (templateMismatchDetected) {
        inspectionErrors.push('Inspection template does not fit the sampled asset scope.')
      }
    }
    if (selectedInspectionTemplateQuery.isError) {
      inspectionErrors.push('The selected inspection template could not be loaded.')
    }
  }

  const complianceErrors: string[] = []
  if (isComplianceRelated && !hasText(governingBodyCatalogKey)) {
    complianceErrors.push('Select a governing body catalog item.')
  }
  if (isComplianceRelated && !confirmComplianceImpact) {
    complianceErrors.push('Confirm compliance impact before activation.')
  }
  if (readinessImpact !== 'no_impact' && !confirmReadinessImpact) {
    complianceErrors.push('Confirm readiness impact before activation.')
  }

  const automationErrors: string[] = []
  if (autoGenerateWorkOrder) {
    if (!generateWorkOrder) {
      automationErrors.push('Define a work package before enabling auto-generated work orders.')
    }
    if (!hasText(workOrderTitlePattern)) {
      automationErrors.push('Work order title pattern is required for auto-generation.')
    }
    if (!hasText(defaultWorkOrderPriority)) {
      automationErrors.push('Default work order priority is required for auto-generation.')
    }
    if (!hasText(defaultWorkType)) {
      automationErrors.push('Default work type is required for auto-generation.')
    }
    if (hasText(leadTimeDays) && !isNonNegativeIntegerText(leadTimeDays)) {
      automationErrors.push('Lead time must be zero or greater.')
    }
    if (hasText(leadThresholdValue) && !isPositiveNumberText(leadThresholdValue)) {
      automationErrors.push('Lead threshold value must be a positive number.')
    }
    if (hasText(leadThresholdValue) && !hasText(leadThresholdUnit)) {
      automationErrors.push('Lead threshold unit is required when a threshold value is set.')
    }
    if (hasText(duplicatePreventionWindowDays) && !isNonNegativeIntegerText(duplicatePreventionWindowDays)) {
      automationErrors.push('Duplicate prevention window must be zero or greater.')
    }
    if (assignmentBehavior === 'team' && !hasText(assignmentTeamRef)) {
      automationErrors.push('Select an assignment team.')
    }
    if (assignmentBehavior === 'person' && !hasText(assignmentPersonId)) {
      automationErrors.push('Select an assignment person.')
    }
    if (assignmentBehavior === 'role' && !hasText(assignmentRoleKey)) {
      automationErrors.push('Select an assignment role.')
    }
    if (hasText(maxOpenGeneratedItemsPerAsset) && !isPositiveIntegerText(maxOpenGeneratedItemsPerAsset)) {
      automationErrors.push('Maximum open generated items per asset must be a positive whole number.')
    }
  }
  if (autoGenerateInspection) {
    if (!attachInspectionTemplate || !hasText(inspectionTemplateId)) {
      automationErrors.push('Auto-generated inspections require an attached inspection template.')
    }
  }

  const previewWarnings = [
    ...(scopePreviewQuery.data?.warnings ?? []),
    ...(duePreviewQuery.data?.warnings ?? []),
  ]
  const draftErrors = [
    ...basicErrors,
    ...scopeErrors,
    ...dueErrors,
    ...workPackageErrors,
    ...inspectionErrors,
    ...complianceErrors,
    ...automationErrors,
  ]
  const activationErrors = [
    ...draftErrors,
    ...(basicInputsReady ? [] : ['Complete the basic section.']),
    ...(scopeHasSelections ? [] : ['Define at least one scope filter or include at least one asset.']),
    ...(scopePreviewQuery.data && scopePreviewQuery.data.matchedAssetCount === 0
      ? ['The scope currently matches zero assets. Save as Draft or refine the scope before activating.']
      : []),
    ...(!dueHasSelections ? ['Define at least one due trigger or switch the program to manual only.'] : []),
    ...(autoGenerateWorkOrder && !generateWorkOrder
      ? ['Auto-generated work orders require a defined work package.']
      : []),
    ...(autoGenerateInspection && !attachInspectionTemplate
      ? ['Auto-generated inspections require an attached inspection template.']
      : []),
    ...(scopePreviewQuery.isFetching ? ['Scope preview is still loading.'] : []),
    ...(duePreviewQuery.isFetching ? ['Due preview is still loading.'] : []),
    ...(templateMismatchDetected ? ['Inspection template does not cover the current matched asset sample.'] : []),
  ]
  const scopeMatchedCount = scopePreviewQuery.data?.matchedAssetCount ?? 0
  const scopeExcludedCount = scopePreviewQuery.data?.excludedAssetCount ?? 0
  const duePreviewItems = duePreviewQuery.data?.items ?? []
  const dueLogicLabel = duePreviewQuery.data?.dueLogic ?? (dueMatchLogic === 'all' ? 'all' : 'any')
  const scopeSummary = scopeHasSelections
    ? buildScopeSummary(
        scopeMatchedCount,
        scopeExcludedCount,
        siteOptions.filter((option) => siteRefs.includes(option.value)).map((option) => option.label),
        assetClassOptions.filter((option) => assetClassKeys.includes(option.value)).map((option) => option.label),
        assetTypeOptions.filter((option) => assetTypeIds.includes(option.value)).map((option) => option.label),
        assetCategoryOptions.filter((option) => assetCategoryKeys.includes(option.value)).map((option) => option.label),
      )
    : 'Define scope filters to preview matching assets.'
  const dueSummary = manualOnlyProgram
    ? 'Manual only PM program'
    : dueTriggers.filter(triggerHasContent).map(getTriggerSummary).join(' · ') || 'Define due triggers to preview timing.'
  const workSummary = generateWorkOrder
    ? `${titlePreview} · ${partsDemandText.trim() ? `${parsePartsDemand(partsDemandText).length} parts demand lines` : 'No parts demand yet'}`
    : 'No work order generated'
  const inspectionSummary = attachInspectionTemplate
    ? selectedInspectionTemplate
      ? `${selectedInspectionTemplate.name} · ${humanize(selectedInspectionTemplate.status)}`
      : 'Choose an inspection template'
    : 'No inspection template attached'
  const complianceSummary = isComplianceRelated
    ? `Compliance-related · ${humanize(readinessImpact)}`
    : 'No compliance impact'
  const automationSummary = autoGenerateWorkOrder || autoGenerateInspection
    ? `Lead ${hasText(leadTimeDays) ? leadTimeDays : '0'} days · Duplicate window ${
        hasText(duplicatePreventionWindowDays) ? duplicatePreventionWindowDays : '1'
      } days`
    : 'Manual create flow'

  const sectionCompletion: boolean[] = [
    basicInputsReady && basicErrors.length === 0,
    scopeHasSelections && scopeErrors.length === 0,
    dueHasSelections && dueErrors.length === 0,
    !generateWorkOrder || workPackageErrors.length === 0,
    !attachInspectionTemplate || inspectionErrors.length === 0,
    (!isComplianceRelated && readinessImpact === 'no_impact') ||
      (complianceErrors.length === 0 && confirmComplianceImpact && (readinessImpact === 'no_impact' || confirmReadinessImpact)),
    (!autoGenerateWorkOrder && !autoGenerateInspection) || automationErrors.length === 0,
    activationErrors.length === 0,
  ]
  const sectionWarnings: string[][] = [
    [],
    previewWarnings.length > 0 ? previewWarnings : scopeMatchedCount === 0 && scopeHasSelections ? ['No assets currently match this scope.'] : [],
    duePreviewItems.length > 0 ? [] : dueHasSelections ? ['No due preview items are available yet.'] : [],
    generateWorkOrder ? [] : ['No work order generation configured.'],
    attachInspectionTemplate ? [] : ['No inspection template attached.'],
    isComplianceRelated || readinessImpact !== 'no_impact'
      ? ['This program can affect readiness and should be activated with care.']
      : [],
    autoGenerateWorkOrder || autoGenerateInspection
      ? ['Duplicate prevention is enabled to avoid repeated generation.']
      : [],
    activationErrors.length > 0 ? activationErrors : previewWarnings,
  ]
  const sectionErrors: string[][] = [
    basicErrors,
    scopeErrors,
    dueErrors,
    workPackageErrors,
    inspectionErrors,
    complianceErrors,
    automationErrors,
    activationErrors,
  ]
  const sectionLabels: string[] = [
    basicInputsReady ? 'Complete' : 'In progress',
    sectionCompletion[1] ? 'Complete' : scopeHasSelections ? 'In progress' : 'Pending',
    sectionCompletion[2] ? 'Complete' : dueHasSelections ? 'In progress' : 'Pending',
    sectionCompletion[3] ? 'Complete' : generateWorkOrder ? 'In progress' : 'Optional',
    sectionCompletion[4] ? 'Complete' : attachInspectionTemplate ? 'In progress' : 'Optional',
    sectionCompletion[5] ? 'Complete' : isComplianceRelated || readinessImpact !== 'no_impact' ? 'In progress' : 'Optional',
    sectionCompletion[6] ? 'Complete' : autoGenerateWorkOrder || autoGenerateInspection ? 'In progress' : 'Optional',
    sectionCompletion[7] ? 'Ready' : 'Needs work',
  ]
  const sectionSummaries: string[] = [
    programName.trim() ? `${programName.trim()} · ${effectiveProgramKey}` : 'Name the PM program to begin.',
    scopeSummary,
    dueSummary,
    workSummary,
    inspectionSummary,
    complianceSummary,
    automationSummary,
    activationErrors.length === 0
      ? 'Review the configuration and activate the program.'
      : activationErrors[0] ?? 'Review the configuration.',
  ]
  const sectionIcons = [
    <BadgeCheck className="h-5 w-5" />,
    <CirclePlus className="h-5 w-5" />,
    <CalendarClock className="h-5 w-5" />,
    <Wrench className="h-5 w-5" />,
    <CheckCircle2 className="h-5 w-5" />,
    <AlertTriangle className="h-5 w-5" />,
    <Sparkles className="h-5 w-5" />,
    <Lock className="h-5 w-5" />,
  ]
  const sectionLocked: boolean[] = [
    false,
    !sectionCompletion[0],
    !sectionCompletion[1],
    !sectionCompletion[2],
    !sectionCompletion[3],
    !sectionCompletion[4],
    !sectionCompletion[5],
    !sectionCompletion[6],
  ]
  const sectionTones: Tone[] = sectionCompletion.map((complete, index) =>
    getSectionTone(complete, sectionWarnings[index] ?? [], sectionErrors[index] ?? [], sectionLocked[index] ?? false),
  )
  const referenceDataLoading =
    catalogsQuery.isLoading ||
    assetClassesQuery.isLoading ||
    assetTypesQuery.isLoading ||
    assetsQuery.isLoading ||
    sitesQuery.isLoading ||
    departmentsQuery.isLoading ||
    teamsQuery.isLoading ||
    peopleQuery.isLoading ||
    inspectionTemplatesQuery.isLoading ||
    governingBodiesQuery.isLoading ||
    partsQuery.isLoading
  const draftReady = basicInputsReady && draftErrors.length === 0
  const activationReady =
    canActivate &&
    activationErrors.length === 0 &&
    sectionCompletion.slice(0, 7).every(Boolean) &&
    !scopePreviewQuery.isFetching &&
    !duePreviewQuery.isFetching

  const triggerIdLookup = useRef<Record<number, boolean>>({})
  useEffect(() => {
    const currentComplete = sectionCompletion[openSectionIndex] ?? false
    const previousComplete = triggerIdLookup.current[openSectionIndex]
    triggerIdLookup.current[openSectionIndex] = currentComplete
    if (!currentComplete || previousComplete !== false) {
      return
    }
    const nextUnlocked = sectionLocked.findIndex((locked, index) => index > openSectionIndex && !locked)
    if (nextUnlocked !== -1) {
      setOpenSectionIndex(nextUnlocked)
    }
  }, [openSectionIndex, sectionCompletion, sectionLocked])

  useEffect(() => {
    if (!sectionLocked[openSectionIndex]) {
      return
    }
    const firstUnlocked = sectionLocked.findIndex((locked) => !locked)
    if (firstUnlocked !== -1) {
      setOpenSectionIndex(firstUnlocked)
    }
  }, [openSectionIndex, sectionLocked])

  const updateTrigger = (triggerId: string, patch: Partial<TriggerDraft>) => {
    setDueTriggers((current) => current.map((trigger) => (trigger.id === triggerId ? { ...trigger, ...patch } : trigger)))
  }

  const appendTrigger = (triggerType: TriggerType) => {
    setDueTriggers((current) => [...current, createTriggerDraft(triggerType)])
  }

  const removeTrigger = (triggerId: string) => {
    setDueTriggers((current) => current.filter((trigger) => trigger.id !== triggerId))
  }

  const appendPartDemandSuggestion = () => {
    const selectedPart = partOptions.find((option) => option.value === partSuggestionRef)
    if (!selectedPart) {
      return
    }
    const nextLine = `${selectedPart.value}|1|each|${selectedPart.label}`
    setPartsDemandText((current) => (current.trim().length > 0 ? `${current.trimEnd()}\n${nextLine}` : nextLine))
    setPartSuggestionRef('')
  }

  const createMutation = useMutation<PmProgramDetailResponse, MaintainArrApiError, void>({
    mutationFn: async () => createPmProgram(session!.accessToken, createPayload),
    onSuccess: (created) => {
      setPendingDraftId(created.pmProgramId)
    },
  })

  const activateMutation = useMutation<PmProgramDetailResponse, MaintainArrApiError, string>({
    mutationFn: async (pmProgramId) =>
      activatePmProgram(session!.accessToken, pmProgramId, {
        confirmReadinessImpact,
        confirmComplianceImpact,
      }),
    onSuccess: (activated) => {
      setPendingDraftId(activated.pmProgramId)
    },
  })

  const handleSaveDraft = async () => {
    try {
      setServerError(null)
      const created = await createMutation.mutateAsync()
      navigate(`/pm-programs/details?programId=${created.pmProgramId}&created=1`, { replace: true })
    } catch (error) {
      setServerError(
        error instanceof MaintainArrApiError
          ? error.message
          : error instanceof Error
            ? error.message
            : 'Failed to create PM program draft.',
      )
    }
  }

  const handleCreateAndActivate = async () => {
    try {
      setServerError(null)
      const draftId =
        pendingDraftId ??
        (await createMutation.mutateAsync()).pmProgramId
      setPendingDraftId(draftId)
      const activated = await activateMutation.mutateAsync(draftId)
      navigate(`/pm-programs/details?programId=${activated.pmProgramId}&created=1`, { replace: true })
    } catch (error) {
      setServerError(
        error instanceof MaintainArrApiError
          ? error.message
          : error instanceof Error
            ? error.message
            : 'Failed to create and activate PM program.',
      )
    }
  }

  if (!session) {
    return <Navigate to="/launch" replace />
  }

  if (meQuery.isLoading) {
    return (
      <div className="mx-auto flex min-h-screen max-w-5xl items-center justify-center px-4 py-10" style={shellStyle}>
        <div className="rounded-3xl border px-6 py-5 shadow-2xl" style={panelStyle}>
          <div className="flex items-center gap-3 text-sm text-slate-200">
            <Loader2 className="h-4 w-4 animate-spin" />
            Loading MaintainArr permissions...
          </div>
        </div>
      </div>
    )
  }

  if (meQuery.isError) {
    return (
      <div className="mx-auto min-h-screen max-w-5xl px-4 py-10" style={shellStyle}>
        <div className="rounded-3xl border px-6 py-5 text-sm text-red-100 shadow-2xl" style={panelStyle}>
          Unable to load your MaintainArr session. Please launch the product again.
        </div>
      </div>
    )
  }

  if (!canCreate) {
    return (
      <div className="mx-auto min-h-screen max-w-5xl px-4 py-10" style={shellStyle}>
        <PageHeader title="Create Preventive Maintenance Program" subtitle="MaintainArr guided builder" />
        <div className="rounded-3xl border px-6 py-6 text-sm text-slate-200 shadow-2xl" style={panelStyle}>
          <div className="flex items-start gap-3">
            <AlertTriangle className="mt-0.5 h-5 w-5 text-amber-400" />
            <div>
              <h2 className="text-lg font-semibold text-white">Permission denied</h2>
              <p className="mt-1 text-slate-300">
                Your role cannot create preventive maintenance programs in this tenant.
              </p>
            </div>
          </div>
        </div>
      </div>
    )
  }

  const fieldClass =
    'mt-1 w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 placeholder:text-[var(--color-text-muted)] focus:border-sky-500 focus:outline-none'
  const textareaClass = `${fieldClass} min-h-[104px]`
  const smallFieldClass = `${fieldClass} min-h-[42px]`

  const pmProgramStatusChips = ['draft', 'active', 'paused', 'retired']

  return (
    <div className="min-h-screen px-4 py-6 sm:px-6 lg:px-8" style={shellStyle} data-testid="pm-program-create-page">
      <div className="mx-auto max-w-7xl space-y-6">
        <div className="rounded-[2rem] border px-5 py-5 shadow-2xl sm:px-6" style={panelStyle}>
          <PageHeader
            title="Create Preventive Maintenance Program"
            subtitle={`Guided full-page builder for ${session.tenantDisplayName}`}
          />

          <div className="flex flex-wrap items-center justify-between gap-3">
            <Link
              to="/pm-programs"
              className="inline-flex items-center gap-2 text-sm text-slate-300 hover:text-white"
            >
              <ArrowLeft className="h-4 w-4" />
              Back to PM programs
            </Link>
            <div className="flex flex-wrap items-center gap-2">
              <DetailBadge label="Draft" tone="warn" />
              <DetailBadge label={session.tenantDisplayName} tone="info" />
              <DetailBadge label={session.displayName} tone="neutral" />
            </div>
          </div>

          {referenceDataLoading ? (
            <div className="mt-5 rounded-2xl border border-slate-700 bg-slate-950/60 p-4 text-sm text-slate-300">
              <div className="flex items-center gap-2">
                <Loader2 className="h-4 w-4 animate-spin" />
                Loading reference data...
              </div>
            </div>
          ) : null}

          {serverError ? (
            <div className="mt-5 rounded-2xl border border-red-700/60 bg-red-950/25 p-4 text-sm text-red-100">
              {serverError}
            </div>
          ) : null}

          {pendingDraftId ? (
            <div className="mt-5 rounded-2xl border border-emerald-500/30 bg-emerald-950/20 p-4 text-sm text-emerald-100">
              A draft has already been created for this submission. Retry activation will reuse the saved draft.
            </div>
          ) : null}

          <div className="mt-6 grid gap-3 md:grid-cols-2 xl:grid-cols-4">
            <div className="rounded-2xl border border-slate-700 bg-slate-950/60 p-4">
              <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Lifecycle</div>
              <div className="mt-2 flex flex-wrap items-center gap-2">
                {pmProgramStatusChips.map((status) => (
                  <DetailBadge
                    key={status}
                    label={humanize(status)}
                    tone={status === 'draft' ? 'warn' : status === 'active' ? 'good' : status === 'paused' ? 'info' : 'neutral'}
                  />
                ))}
              </div>
              <p className="mt-3 text-sm text-slate-300">
                New PM programs start in <span className="font-semibold text-white">Draft</span> until they pass review.
              </p>
            </div>
            <div className="rounded-2xl border border-slate-700 bg-slate-950/60 p-4">
              <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Program code</div>
              <div className="mt-2 font-mono text-sm text-slate-100">{effectiveProgramKey || 'Generated from the program name'}</div>
              <p className="mt-3 text-sm text-slate-300">
                The code is generated from the name and can be manually overridden.
              </p>
            </div>
            <div className="rounded-2xl border border-slate-700 bg-slate-950/60 p-4">
              <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Matched assets</div>
              <div className="mt-2 text-2xl font-semibold text-white">
                {scopePreviewQuery.data ? scopeMatchedCount : '—'}
              </div>
              <p className="mt-3 text-sm text-slate-300">
                {scopePreviewQuery.data ? scopeSummary : 'Scope preview appears after you add filters.'}
              </p>
            </div>
            <div className="rounded-2xl border border-slate-700 bg-slate-950/60 p-4">
              <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Due logic</div>
              <div className="mt-2 text-sm text-slate-100">{dueSummary}</div>
              <p className="mt-3 text-sm text-slate-300">
                {duePreviewItems.length > 0
                  ? `${duePreviewItems.length} sample due preview item${duePreviewItems.length === 1 ? '' : 's'}`
                  : 'Preview due logic once you define triggers.'}
              </p>
            </div>
          </div>
        </div>

        <div className="space-y-4">
          <SectionCard
            title="Program Basics"
            summary={sectionSummaries[0]}
            stateLabel={sectionLabels[0]}
            tone={sectionTones[0]}
            expanded={openSectionIndex === 0}
            locked={sectionLocked[0]}
            icon={sectionIcons[0]}
            onToggle={() => setOpenSectionIndex(0)}
            testId="pm-program-section-basics"
          >
            <div className="grid gap-4 xl:grid-cols-[minmax(0,1.7fr)_minmax(320px,1fr)]">
              <div className="space-y-4">
                <div className="grid gap-4 md:grid-cols-2">
                  <label className="block text-sm text-slate-300">
                    Program name
                    <input
                      className={fieldClass}
                      value={programName}
                      onChange={(event) => setProgramName(event.target.value)}
                      placeholder="Quarterly fleet safety PM"
                      data-testid="pm-program-name"
                    />
                  </label>
                  <GeneratedKeyField
                    sourceLabel="Program name"
                    generatedKey={generatedProgramKey}
                    confirmedKey={effectiveProgramKey}
                    manualOverride={manualProgramKey}
                    onManualOverrideChange={setManualProgramKey}
                    showAdvancedKey={showKeyPolicy}
                    allowManualOverride
                    collisionWarning={
                      (existingProgramsQuery.data ?? []).some(
                        (program) => program.programKey.trim().toLowerCase() === effectiveProgramKey.trim().toLowerCase(),
                      )
                        ? 'This code already exists in MaintainArr.'
                        : null
                    }
                    label="Program code"
                  />
                </div>

                <label className="block text-sm text-slate-300">
                  Description
                  <textarea
                    className={textareaClass}
                    value={description}
                    onChange={(event) => setDescription(event.target.value)}
                    placeholder="Describe the maintenance standard, seasonality, or compliance objective."
                    data-testid="pm-program-description"
                  />
                </label>

                <div className="grid gap-4 md:grid-cols-3">
                  <ControlledSelect
                    label="PM category"
                    value={categoryKey}
                    onChange={setCategoryKey}
                    options={programCategoryOptions}
                    testId="pm-program-category"
                    emptyLabel="Select category…"
                  />
                  <ControlledSelect
                    label="Work type"
                    value={workTypeKey}
                    onChange={setWorkTypeKey}
                    options={workTypeOptions}
                    testId="pm-program-work-type"
                    emptyLabel="Select work type…"
                  />
                  <ControlledSelect
                    label="Priority"
                    value={priorityKey}
                    onChange={setPriorityKey}
                    options={priorityOptions}
                    testId="pm-program-priority"
                    emptyLabel="Select priority…"
                  />
                </div>

                <div className="grid gap-4 md:grid-cols-4">
                  <StaticSearchPicker
                    label="Owning site"
                    value={owningSiteRef}
                    onChange={setOwningSiteRef}
                    options={siteOptions}
                    placeholder="Search sites…"
                    testId="pm-program-owning-site"
                  />
                  <StaticSearchPicker
                    label="Owning team"
                    value={owningTeamRef}
                    onChange={setOwningTeamRef}
                    options={teamOptions}
                    placeholder="Search teams…"
                    testId="pm-program-owning-team"
                  />
                  <StaticSearchPicker
                    label="Owning department"
                    value={owningDepartmentRef}
                    onChange={setOwningDepartmentRef}
                    options={departmentOptions}
                    placeholder="Search departments…"
                    testId="pm-program-owning-department"
                  />
                  <StaticSearchPicker
                    label="Owner person"
                    value={ownerPersonId}
                    onChange={setOwnerPersonId}
                    options={personOptions}
                    placeholder="Search people…"
                    testId="pm-program-owner-person"
                  />
                </div>

                <div className="grid gap-4 md:grid-cols-2">
                  <label className="block text-sm text-slate-300">
                    Owner role key
                    <input
                      className={fieldClass}
                      value={ownerRoleKey}
                      onChange={(event) => setOwnerRoleKey(event.target.value)}
                      placeholder="maintenance_lead"
                      data-testid="pm-program-owner-role"
                    />
                  </label>
                  <label className="block text-sm text-slate-300">
                    Tags
                    <textarea
                      className={textareaClass}
                      value={tagsText}
                      onChange={(event) => setTagsText(event.target.value)}
                      placeholder="fleet, safety, seasonal"
                      data-testid="pm-program-tags"
                    />
                  </label>
                </div>
              </div>

              <div className="space-y-4">
                <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4">
                  <div className="flex items-center justify-between gap-3">
                    <div>
                      <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Current state</div>
                      <h3 className="mt-1 text-base font-semibold text-white">Draft by default</h3>
                    </div>
                    <DetailBadge label="Draft" tone="warn" />
                  </div>
                  <p className="mt-3 text-sm text-slate-300">
                    This record will remain a draft until every section is valid and you choose to activate it.
                  </p>
                  <button
                    type="button"
                    className="mt-4 inline-flex items-center gap-2 text-xs font-semibold text-sky-300 hover:text-sky-200"
                    onClick={() => setShowKeyPolicy((current) => !current)}
                  >
                    {showKeyPolicy ? 'Hide key policy' : 'Show key policy'}
                  </button>
                </div>

                <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4">
                  <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Owner summary</div>
                  <div className="mt-3 space-y-2 text-sm text-slate-300">
                    <p>Tenant: {session.tenantDisplayName}</p>
                    <p>Site: {siteOptions.find((option) => option.value === owningSiteRef)?.label ?? 'Not set'}</p>
                    <p>Team: {teamOptions.find((option) => option.value === owningTeamRef)?.label ?? 'Not set'}</p>
                    <p>Department: {departmentOptions.find((option) => option.value === owningDepartmentRef)?.label ?? 'Not set'}</p>
                    <p>Person: {personOptions.find((option) => option.value === ownerPersonId)?.label ?? 'Not set'}</p>
                  </div>
                </div>
              </div>
            </div>
          </SectionCard>

          <SectionCard
            title="Asset Scope"
            summary={sectionSummaries[1]}
            stateLabel={sectionLabels[1]}
            tone={sectionTones[1]}
            expanded={openSectionIndex === 1}
            locked={sectionLocked[1]}
            icon={sectionIcons[1]}
            onToggle={() => setOpenSectionIndex(1)}
            testId="pm-program-section-scope"
          >
            <div className="grid gap-4 xl:grid-cols-[minmax(0,1.7fr)_minmax(340px,1fr)]">
              <div className="grid gap-4 md:grid-cols-2">
                <MultiValueSearchField
                  label="Asset classes"
                  placeholder="Search asset classes…"
                  options={assetClassOptions}
                  values={assetClassKeys}
                  onChange={setAssetClassKeys}
                  testId="pm-program-scope-asset-classes"
                />
                <MultiValueSearchField
                  label="Asset types"
                  placeholder="Search asset types…"
                  options={assetTypeOptions}
                  values={assetTypeIds}
                  onChange={setAssetTypeIds}
                  testId="pm-program-scope-asset-types"
                />
                <MultiValueSearchField
                  label="Asset categories"
                  placeholder="Search asset categories…"
                  options={assetCategoryOptions}
                  values={assetCategoryKeys}
                  onChange={setAssetCategoryKeys}
                  testId="pm-program-scope-asset-categories"
                />
                <MultiValueSearchField
                  label="Asset status"
                  placeholder="Search asset statuses…"
                  options={assetStatusOptions}
                  values={assetStatusKeys}
                  onChange={setAssetStatusKeys}
                  testId="pm-program-scope-asset-status"
                />
                <MultiValueSearchField
                  label="Readiness state"
                  placeholder="Search readiness states…"
                  options={readinessStateOptions}
                  values={readinessStateKeys}
                  onChange={setReadinessStateKeys}
                  testId="pm-program-scope-readiness"
                />
                <MultiValueSearchField
                  label="Sites"
                  placeholder="Search sites…"
                  options={siteOptions}
                  values={siteRefs}
                  onChange={setSiteRefs}
                  testId="pm-program-scope-sites"
                />
                <MultiValueSearchField
                  label="Departments"
                  placeholder="Search departments…"
                  options={departmentOptions}
                  values={departmentRefs}
                  onChange={setDepartmentRefs}
                  testId="pm-program-scope-departments"
                />
                <MultiValueSearchField
                  label="Makes"
                  placeholder="Search makes…"
                  options={makeOptions}
                  values={makeKeys}
                  onChange={setMakeKeys}
                  testId="pm-program-scope-makes"
                />
                <MultiValueSearchField
                  label="Models"
                  placeholder="Search models…"
                  options={modelOptions}
                  values={modelKeys}
                  onChange={setModelKeys}
                  testId="pm-program-scope-models"
                />
                <MultiValueSearchField
                  label="Fuel types"
                  placeholder="Search fuel types…"
                  options={fuelTypeOptions}
                  values={fuelTypeKeys}
                  onChange={setFuelTypeKeys}
                  testId="pm-program-scope-fuel-types"
                />
                <label className="block text-sm text-slate-300">
                  Year from
                  <input
                    className={smallFieldClass}
                    value={yearFrom}
                    onChange={(event) => setYearFrom(event.target.value)}
                    inputMode="numeric"
                    placeholder="2020"
                    data-testid="pm-program-scope-year-from"
                  />
                </label>
                <label className="block text-sm text-slate-300">
                  Year to
                  <input
                    className={smallFieldClass}
                    value={yearTo}
                    onChange={(event) => setYearTo(event.target.value)}
                    inputMode="numeric"
                    placeholder="2026"
                    data-testid="pm-program-scope-year-to"
                  />
                </label>
                <label className="block text-sm text-slate-300 md:col-span-2">
                  Scope tags
                  <textarea
                    className={textareaClass}
                    value={scopeTagsText}
                    onChange={(event) => setScopeTagsText(event.target.value)}
                    placeholder="urban, cold-weather, high-utilization"
                    data-testid="pm-program-scope-tags"
                  />
                </label>
                <MultiValueSearchField
                  label="Included assets"
                  placeholder="Search assets to include…"
                  options={assetOptions}
                  values={includedAssetIds}
                  onChange={setIncludedAssetIds}
                  testId="pm-program-scope-included-assets"
                  hint="Explicit inclusions override general filters."
                />
                <MultiValueSearchField
                  label="Excluded assets"
                  placeholder="Search assets to exclude…"
                  options={assetOptions}
                  values={excludedAssetIds}
                  onChange={setExcludedAssetIds}
                  testId="pm-program-scope-excluded-assets"
                  hint="Exclusions are visually obvious in the preview."
                />
              </div>

              <div className="space-y-4">
                <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4">
                  <div className="flex items-center justify-between gap-3">
                    <div>
                      <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Matched assets preview</div>
                      <h3 className="mt-1 text-base font-semibold text-white">
                        {scopePreviewQuery.isFetching
                          ? 'Previewing...'
                          : scopePreviewQuery.data
                            ? `${scopeMatchedCount} match${scopeMatchedCount === 1 ? '' : 'es'}`
                            : 'Add filters to preview'}
                      </h3>
                    </div>
                    <DetailBadge
                      label={scopePreviewQuery.data?.canActivate ? 'Preview ready' : 'Review scope'}
                      tone={scopePreviewQuery.data?.canActivate ? 'good' : 'warn'}
                    />
                  </div>

                  {scopePreviewQuery.error ? (
                    <div className="mt-3 rounded-xl border border-red-700/60 bg-red-950/25 p-3 text-sm text-red-100">
                      Unable to preview this scope.
                    </div>
                  ) : null}

                  <div className="mt-4 overflow-hidden rounded-2xl border border-slate-700">
                    <table className="min-w-full divide-y divide-slate-700 text-sm">
                      <thead className="bg-slate-950/60 text-left text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                        <tr>
                          <th className="px-3 py-2">Asset</th>
                          <th className="px-3 py-2">Type</th>
                          <th className="px-3 py-2">Site</th>
                          <th className="px-3 py-2">Status</th>
                          <th className="px-3 py-2">Last PM</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-slate-800 bg-slate-950/40 text-slate-200">
                        {scopePreviewQuery.data?.sampleAssets.length
                          ? scopePreviewQuery.data.sampleAssets.map((asset) => (
                              <tr key={asset.assetId}>
                                <td className="px-3 py-2">
                                  <div className="font-medium text-white">{asset.assetTag}</div>
                                  <div className="text-xs text-slate-400">{asset.assetName}</div>
                                </td>
                                <td className="px-3 py-2 text-slate-300">{asset.assetTypeName}</td>
                                <td className="px-3 py-2 text-slate-300">{asset.siteName}</td>
                                <td className="px-3 py-2">
                                  <DetailBadge label={humanize(asset.lifecycleStatus)} tone={toneForStatus(asset.lifecycleStatus)} />
                                </td>
                                <td className="px-3 py-2 text-slate-300">
                                  {asset.dueStatus ? `${humanize(asset.dueStatus)} · ` : ''}
                                  {asset.lastPmAt ? formatDate(asset.lastPmAt) : 'Not recorded'}
                                </td>
                              </tr>
                            ))
                          : (
                              <tr>
                                <td className="px-3 py-6 text-center text-slate-400" colSpan={5}>
                                  {scopeHasSelections
                                    ? 'No sample assets returned yet.'
                                    : 'Add scope filters to see matched assets.'}
                                </td>
                              </tr>
                            )}
                      </tbody>
                    </table>
                  </div>
                </div>

                {previewWarnings.length > 0 ? (
                  <div className="rounded-2xl border border-amber-500/30 bg-amber-950/20 p-4 text-sm text-amber-100">
                    <div className="font-semibold">Preview warnings</div>
                    <ul className="mt-2 list-disc space-y-1 pl-5">
                      {previewWarnings.map((warning) => (
                        <li key={warning}>{warning}</li>
                      ))}
                    </ul>
                  </div>
                ) : null}
              </div>
            </div>
          </SectionCard>

          <SectionCard
            title="Due Triggers"
            summary={sectionSummaries[2]}
            stateLabel={sectionLabels[2]}
            tone={sectionTones[2]}
            expanded={openSectionIndex === 2}
            locked={sectionLocked[2]}
            icon={sectionIcons[2]}
            onToggle={() => setOpenSectionIndex(2)}
            testId="pm-program-section-due"
          >
            <div className="grid gap-4 xl:grid-cols-[minmax(0,1.7fr)_minmax(360px,1fr)]">
              <div className="space-y-4">
                <div className="grid gap-4 md:grid-cols-3">
                  <ControlledSelect
                    label="Due match logic"
                    value={dueMatchLogic}
                    onChange={setDueMatchLogic}
                    options={mapSimpleOptions([
                      { value: 'any', label: 'Due when any trigger is due' },
                      { value: 'all', label: 'Due when all triggers are due' },
                    ])}
                    testId="pm-program-due-logic"
                    emptyLabel="Select logic…"
                  />
                  <label className="flex items-center gap-2 rounded-2xl border border-slate-700 bg-slate-950/70 px-3 py-4 text-sm text-slate-200">
                    <input
                      type="checkbox"
                      checked={warnWhenAnyApproaching}
                      onChange={(event) => setWarnWhenAnyApproaching(event.target.checked)}
                      className="h-4 w-4 rounded border-slate-500 bg-slate-950 text-sky-500"
                    />
                    Warn when any trigger is approaching
                  </label>
                  <label className="flex items-center gap-2 rounded-2xl border border-slate-700 bg-slate-950/70 px-3 py-4 text-sm text-slate-200">
                    <input
                      type="checkbox"
                      checked={markDueBasedOnMostUrgent}
                      onChange={(event) => setMarkDueBasedOnMostUrgent(event.target.checked)}
                      className="h-4 w-4 rounded border-slate-500 bg-slate-950 text-sky-500"
                    />
                    Mark due using the most urgent trigger
                  </label>
                </div>

                <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4">
                  <label className="flex items-center gap-2 text-sm font-medium text-slate-200">
                    <input
                      type="checkbox"
                      checked={manualOnlyProgram}
                      onChange={(event) => setManualOnlyProgram(event.target.checked)}
                      className="h-4 w-4 rounded border-slate-500 bg-slate-950 text-sky-500"
                    />
                    Manual-only PM program
                  </label>
                  <p className="mt-2 text-sm text-slate-400">
                    Manual-only programs skip automatic due calculations until a person explicitly marks them ready.
                  </p>
                </div>

                {!manualOnlyProgram ? (
                  <div className="flex flex-wrap gap-2">
                    <button
                      type="button"
                      className="inline-flex items-center gap-2 rounded-full border border-slate-700 bg-slate-950 px-3 py-2 text-xs font-semibold text-slate-100"
                      onClick={() => appendTrigger('calendar')}
                    >
                      <CirclePlus className="h-3.5 w-3.5" />
                      Add calendar trigger
                    </button>
                    <button
                      type="button"
                      className="inline-flex items-center gap-2 rounded-full border border-slate-700 bg-slate-950 px-3 py-2 text-xs font-semibold text-slate-100"
                      onClick={() => appendTrigger('meter')}
                    >
                      <CirclePlus className="h-3.5 w-3.5" />
                      Add meter trigger
                    </button>
                    <button
                      type="button"
                      className="inline-flex items-center gap-2 rounded-full border border-slate-700 bg-slate-950 px-3 py-2 text-xs font-semibold text-slate-100"
                      onClick={() => appendTrigger('one_time')}
                    >
                      <CirclePlus className="h-3.5 w-3.5" />
                      Add one-time trigger
                    </button>
                  </div>
                ) : (
                  <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4 text-sm text-slate-300">
                    Manual-only mode is active. The system will bypass automatic trigger calculations.
                  </div>
                )}

                <div className="space-y-4">
                  {(manualOnlyProgram ? [createTriggerDraft('manual')] : dueTriggers).map((trigger, index) => {
                    const triggerErrors = validateTriggerDraft(trigger)
                    return (
                      <div key={trigger.id} className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4">
                        <div className="flex flex-wrap items-center justify-between gap-3">
                          <div>
                            <h3 className="font-semibold text-white">{getTriggerSummary(trigger)}</h3>
                            <p className="text-xs text-slate-400">Trigger {index + 1}</p>
                          </div>
                          {!manualOnlyProgram ? (
                            <button
                              type="button"
                              className="inline-flex items-center gap-2 rounded-full border border-slate-700 px-3 py-2 text-xs font-semibold text-slate-200"
                              onClick={() => removeTrigger(trigger.id)}
                            >
                              <Trash2 className="h-3.5 w-3.5" />
                              Remove
                            </button>
                          ) : null}
                        </div>

                        {trigger.triggerType === 'calendar' ? (
                          <div className="mt-4 grid gap-4 md:grid-cols-2">
                            <label className="block text-sm text-slate-300">
                              Every
                              <input
                                className={smallFieldClass}
                                value={trigger.calendarIntervalValue}
                                onChange={(event) =>
                                  updateTrigger(trigger.id, { calendarIntervalValue: event.target.value })
                                }
                                inputMode="numeric"
                                placeholder="12"
                              />
                            </label>
                            <ControlledSelect
                              label="Unit"
                              value={trigger.calendarIntervalUnit}
                              onChange={(value) => updateTrigger(trigger.id, { calendarIntervalUnit: value })}
                              options={mapSimpleOptions([
                                { value: 'days', label: 'Days' },
                                { value: 'weeks', label: 'Weeks' },
                                { value: 'months', label: 'Months' },
                                { value: 'years', label: 'Years' },
                              ])}
                              emptyLabel="Select unit…"
                            />
                            <label className="block text-sm text-slate-300">
                              Anchor date
                              <input
                                type="date"
                                className={smallFieldClass}
                                value={trigger.calendarAnchorDate}
                                onChange={(event) =>
                                  updateTrigger(trigger.id, { calendarAnchorDate: event.target.value })
                                }
                              />
                            </label>
                            <label className="block text-sm text-slate-300">
                              First due date
                              <input
                                type="date"
                                className={smallFieldClass}
                                value={trigger.calendarFirstDueDate}
                                onChange={(event) =>
                                  updateTrigger(trigger.id, { calendarFirstDueDate: event.target.value })
                                }
                              />
                            </label>
                            <ControlledSelect
                              label="Calendar behavior"
                              value={trigger.calendarBehavior}
                              onChange={(value) => updateTrigger(trigger.id, { calendarBehavior: value as CalendarBehavior })}
                              options={mapSimpleOptions([
                                { value: 'fixed_schedule', label: 'Fixed schedule' },
                                { value: 'rolling_from_completion', label: 'Rolling from completion' },
                              ])}
                              emptyLabel="Select behavior…"
                            />
                            <label className="block text-sm text-slate-300">
                              Early window days
                              <input
                                className={smallFieldClass}
                                value={trigger.calendarEarlyWindowDays}
                                onChange={(event) =>
                                  updateTrigger(trigger.id, { calendarEarlyWindowDays: event.target.value })
                                }
                                inputMode="numeric"
                                placeholder="0"
                              />
                            </label>
                            <label className="block text-sm text-slate-300">
                              Grace period days
                              <input
                                className={smallFieldClass}
                                value={trigger.calendarGracePeriodDays}
                                onChange={(event) =>
                                  updateTrigger(trigger.id, { calendarGracePeriodDays: event.target.value })
                                }
                                inputMode="numeric"
                                placeholder="0"
                              />
                            </label>
                            <ControlledSelect
                              label="Past-due behavior"
                              value={trigger.calendarPastDueBehavior}
                              onChange={(value) =>
                                updateTrigger(trigger.id, { calendarPastDueBehavior: value as PastDueBehavior })
                              }
                              options={mapSimpleOptions([
                                { value: 'mark_due', label: 'Mark due' },
                                { value: 'mark_overdue', label: 'Mark overdue' },
                                { value: 'hold', label: 'Hold' },
                              ])}
                              emptyLabel="Select behavior…"
                            />
                          </div>
                        ) : trigger.triggerType === 'meter' ? (
                          <div className="mt-4 grid gap-4 md:grid-cols-2">
                            <label className="block text-sm text-slate-300">
                              Interval value
                              <input
                                className={smallFieldClass}
                                value={trigger.meterIntervalValue}
                                onChange={(event) => updateTrigger(trigger.id, { meterIntervalValue: event.target.value })}
                                inputMode="decimal"
                                placeholder="25000"
                              />
                            </label>
                            <ControlledSelect
                              label="Unit"
                              value={trigger.meterIntervalUnit}
                              onChange={(value) => updateTrigger(trigger.id, { meterIntervalUnit: value })}
                              options={meterUnitOptions}
                              emptyLabel="Select unit…"
                            />
                            <label className="block text-sm text-slate-300">
                              Anchor reading
                              <input
                                className={smallFieldClass}
                                value={trigger.meterAnchorReading}
                                onChange={(event) => updateTrigger(trigger.id, { meterAnchorReading: event.target.value })}
                                inputMode="decimal"
                                placeholder="0"
                              />
                            </label>
                            <label className="block text-sm text-slate-300">
                              First due reading
                              <input
                                className={smallFieldClass}
                                value={trigger.meterFirstDueReading}
                                onChange={(event) =>
                                  updateTrigger(trigger.id, { meterFirstDueReading: event.target.value })
                                }
                                inputMode="decimal"
                                placeholder="25000"
                              />
                            </label>
                            <ControlledSelect
                              label="Current reading source"
                              value={trigger.meterCurrentReadingSource}
                              onChange={(value) => updateTrigger(trigger.id, { meterCurrentReadingSource: value })}
                              options={meterReadingSourceOptions}
                              emptyLabel="Select source…"
                            />
                            <label className="block text-sm text-slate-300">
                              Early threshold
                              <input
                                className={smallFieldClass}
                                value={trigger.meterEarlyThreshold}
                                onChange={(event) => updateTrigger(trigger.id, { meterEarlyThreshold: event.target.value })}
                                inputMode="decimal"
                                placeholder="1000"
                              />
                            </label>
                            <label className="block text-sm text-slate-300">
                              Grace threshold
                              <input
                                className={smallFieldClass}
                                value={trigger.meterGraceThreshold}
                                onChange={(event) => updateTrigger(trigger.id, { meterGraceThreshold: event.target.value })}
                                inputMode="decimal"
                                placeholder="0"
                              />
                            </label>
                            <label className="flex items-center gap-2 rounded-2xl border border-slate-700 bg-slate-950/70 px-3 py-4 text-sm text-slate-200">
                              <input
                                type="checkbox"
                                checked={trigger.meterRollingFromCompletion}
                                onChange={(event) =>
                                  updateTrigger(trigger.id, { meterRollingFromCompletion: event.target.checked })
                                }
                                className="h-4 w-4 rounded border-slate-500 bg-slate-950 text-sky-500"
                              />
                              Rolling from completion
                            </label>
                            <ControlledSelect
                              label="Missing data behavior"
                              value={trigger.meterMissingDataBehavior}
                              onChange={(value) =>
                                updateTrigger(trigger.id, { meterMissingDataBehavior: value as MissingDataBehavior })
                              }
                              options={mapSimpleOptions([
                                { value: 'warn', label: 'Warn' },
                                { value: 'skip', label: 'Skip' },
                                { value: 'block', label: 'Block' },
                              ])}
                              emptyLabel="Select behavior…"
                            />
                          </div>
                        ) : trigger.triggerType === 'one_time' ? (
                          <div className="mt-4">
                            <label className="block text-sm text-slate-300">
                              Due date
                              <input
                                type="date"
                                className={smallFieldClass}
                                value={trigger.oneTimeDueDate}
                                onChange={(event) => updateTrigger(trigger.id, { oneTimeDueDate: event.target.value })}
                              />
                            </label>
                          </div>
                        ) : (
                          <div className="mt-4 rounded-2xl border border-slate-700 bg-slate-950/70 p-4 text-sm text-slate-300">
                            Manual due logic does not need interval fields.
                          </div>
                        )}

                        {triggerErrors.length > 0 ? (
                          <div className="mt-4 rounded-2xl border border-red-700/60 bg-red-950/25 p-4 text-sm text-red-100">
                            <div className="font-semibold">Trigger errors</div>
                            <ul className="mt-2 list-disc space-y-1 pl-5">
                              {triggerErrors.map((error) => (
                                <li key={error}>{error}</li>
                              ))}
                            </ul>
                          </div>
                        ) : null}
                      </div>
                    )
                  })}
                </div>
              </div>

              <div className="space-y-4">
                <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4">
                  <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Due preview</div>
                  <h3 className="mt-1 text-base font-semibold text-white">
                    {duePreviewQuery.isFetching
                      ? 'Loading...'
                      : duePreviewItems.length > 0
                        ? `${duePreviewItems.length} sample asset${duePreviewItems.length === 1 ? '' : 's'}`
                        : 'Add trigger data to preview'}
                  </h3>
                  <p className="mt-2 text-sm text-slate-300">
                    {dueLogicLabel === 'all'
                      ? 'Due only when every selected trigger is due.'
                      : 'Due when any selected trigger becomes due.'}
                  </p>
                  <div className="mt-4 space-y-3">
                    {duePreviewItems.length > 0 ? (
                      duePreviewItems.map((item) => (
                        <div key={item.assetId} className="rounded-xl border border-slate-700 bg-slate-950/60 p-3">
                          <div className="flex items-start justify-between gap-3">
                            <div>
                              <div className="font-medium text-white">{item.assetTag}</div>
                              <div className="text-xs text-slate-400">{item.assetName}</div>
                            </div>
                            <DetailBadge label={humanize(item.dueState)} tone={toneForStatus(item.dueState)} />
                          </div>
                          <div className="mt-2 text-xs text-slate-400">{item.triggerSummary}</div>
                          <div className="mt-2 text-sm text-slate-300">
                            Next due:{' '}
                            {item.estimatedNextDueDate ?? item.estimatedNextDueReading ?? 'Not calculated'}
                          </div>
                        </div>
                      ))
                    ) : (
                      <div className="rounded-xl border border-slate-700 bg-slate-950/40 p-4 text-sm text-slate-400">
                        Due preview appears after you define one or more complete triggers.
                      </div>
                    )}
                  </div>
                </div>

                {duePreviewQuery.data?.requiresExplicitConfirmation ? (
                  <div className="rounded-2xl border border-amber-500/30 bg-amber-950/20 p-4 text-sm text-amber-100">
                    This due model requires explicit confirmation before activation.
                  </div>
                ) : null}
                {dueErrors.length > 0 ? (
                  <div className="rounded-2xl border border-red-700/60 bg-red-950/25 p-4 text-sm text-red-100">
                    <div className="font-semibold">Trigger validation issues</div>
                    <ul className="mt-2 list-disc space-y-1 pl-5">
                      {dueErrors.map((error) => (
                        <li key={error}>{error}</li>
                      ))}
                    </ul>
                  </div>
                ) : null}
              </div>
            </div>
          </SectionCard>

          <SectionCard
            title="Work Package"
            summary={sectionSummaries[3]}
            stateLabel={sectionLabels[3]}
            tone={sectionTones[3]}
            expanded={openSectionIndex === 3}
            locked={sectionLocked[3]}
            icon={sectionIcons[3]}
            onToggle={() => setOpenSectionIndex(3)}
            testId="pm-program-section-work"
          >
            <div className="grid gap-4 xl:grid-cols-[minmax(0,1.7fr)_minmax(340px,1fr)]">
              <div className="space-y-4">
                <label className="flex items-center gap-2 rounded-2xl border border-slate-700 bg-slate-950/70 px-3 py-4 text-sm text-slate-200">
                  <input
                    type="checkbox"
                    checked={generateWorkOrder}
                    onChange={(event) => setGenerateWorkOrder(event.target.checked)}
                    className="h-4 w-4 rounded border-slate-500 bg-slate-950 text-sky-500"
                  />
                  Generate work order
                </label>

                {generateWorkOrder ? (
                  <>
                    <div className="grid gap-4 md:grid-cols-2">
                      <ControlledSelect
                        label="Title template"
                        value={workOrderTitlePattern}
                        onChange={setWorkOrderTitlePattern}
                        options={mapSimpleOptions([
                          { value: 'program_asset_tag', label: 'Program + asset tag' },
                          { value: 'program_asset_name', label: 'Program + asset name' },
                          { value: 'program_asset_tag_name', label: 'Program + asset tag + asset name' },
                          { value: 'program', label: 'Program name only' },
                        ])}
                        testId="pm-program-wo-title-pattern"
                        emptyLabel="Select template…"
                      />
                      <label className="block text-sm text-slate-300">
                        Title prefix
                        <input
                          className={smallFieldClass}
                          value={workOrderTitlePrefix}
                          onChange={(event) => setWorkOrderTitlePrefix(event.target.value)}
                          placeholder="PM"
                          data-testid="pm-program-wo-title-prefix"
                        />
                      </label>
                    </div>

                    <label className="block text-sm text-slate-300">
                      Work order description
                      <textarea
                        className={textareaClass}
                        value={workOrderDescription}
                        onChange={(event) => setWorkOrderDescription(event.target.value)}
                        placeholder="What should the technician do when this PM becomes due?"
                        data-testid="pm-program-wo-description"
                      />
                    </label>

                    <div className="grid gap-4 md:grid-cols-3">
                      <ControlledSelect
                        label="Default priority"
                        value={defaultWorkOrderPriority}
                        onChange={setDefaultWorkOrderPriority}
                        options={workOrderPriorityOptions}
                        testId="pm-program-wo-priority"
                        emptyLabel="Select priority…"
                      />
                      <ControlledSelect
                        label="Default work type"
                        value={defaultWorkType}
                        onChange={setDefaultWorkType}
                        options={workOrderTypeOptions}
                        testId="pm-program-wo-type"
                        emptyLabel="Select work type…"
                      />
                      <label className="block text-sm text-slate-300">
                        Estimated labor hours
                        <input
                          className={smallFieldClass}
                          value={estimatedLaborHours}
                          onChange={(event) => setEstimatedLaborHours(event.target.value)}
                          inputMode="decimal"
                          placeholder="2.5"
                          data-testid="pm-program-wo-hours"
                        />
                      </label>
                    </div>

                    <div className="grid gap-4 md:grid-cols-2">
                      <label className="block text-sm text-slate-300">
                        Required skills or capabilities
                        <textarea
                          className={textareaClass}
                          value={requiredSkillsText}
                          onChange={(event) => setRequiredSkillsText(event.target.value)}
                          placeholder="electrical, brake service"
                          data-testid="pm-program-wo-skills"
                        />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Safety notes
                        <textarea
                          className={textareaClass}
                          value={safetyNotesText}
                          onChange={(event) => setSafetyNotesText(event.target.value)}
                          placeholder="Lockout before service."
                          data-testid="pm-program-wo-safety"
                        />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Technician notes
                        <textarea
                          className={textareaClass}
                          value={technicianNotesText}
                          onChange={(event) => setTechnicianNotesText(event.target.value)}
                          placeholder="Record torque values and attach photos."
                          data-testid="pm-program-wo-tech-notes"
                        />
                      </label>
                      <label className="block text-sm text-slate-300">
                        Required attachments
                        <textarea
                          className={textareaClass}
                          value={requiredAttachmentsText}
                          onChange={(event) => setRequiredAttachmentsText(event.target.value)}
                          placeholder="before-photo, after-photo"
                          data-testid="pm-program-wo-attachments"
                        />
                      </label>
                    </div>

                    <div className="grid gap-4 md:grid-cols-[minmax(0,1fr)_auto]">
                      <label className="block text-sm text-slate-300">
                        Parts demand list
                        <textarea
                          className={textareaClass}
                          value={partsDemandText}
                          onChange={(event) => setPartsDemandText(event.target.value)}
                          placeholder="partRef|1|each|Description"
                          data-testid="pm-program-wo-parts"
                        />
                      </label>
                      <div className="space-y-3">
                        <StaticSearchPicker
                          label="Suggested part"
                          value={partSuggestionRef}
                          onChange={setPartSuggestionRef}
                          options={partOptions}
                          placeholder="Search parts…"
                          testId="pm-program-wo-part-suggestion"
                        />
                        <button
                          type="button"
                          className="inline-flex items-center gap-2 rounded-full border border-slate-700 bg-slate-950 px-3 py-2 text-xs font-semibold text-slate-100"
                          onClick={appendPartDemandSuggestion}
                        >
                          <CirclePlus className="h-3.5 w-3.5" />
                          Add suggested part
                        </button>
                      </div>
                    </div>

                    <div className="overflow-hidden rounded-2xl border border-slate-700">
                      <table className="min-w-full divide-y divide-slate-700 text-sm">
                        <thead className="bg-slate-950/60 text-left text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                          <tr>
                            <th className="px-3 py-2">Part</th>
                            <th className="px-3 py-2">Qty</th>
                            <th className="px-3 py-2">UOM</th>
                            <th className="px-3 py-2">Description</th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-800 bg-slate-950/40 text-slate-200">
                          {parsePartsDemand(partsDemandText).length > 0 ? (
                            parsePartsDemand(partsDemandText).map((row) => (
                              <tr key={`${row.itemRef}-${row.quantity}-${row.unitOfMeasure}`}>
                                <td className="px-3 py-2 font-medium text-white">{row.itemRef}</td>
                                <td className="px-3 py-2">{row.quantity}</td>
                                <td className="px-3 py-2">{row.unitOfMeasure}</td>
                                <td className="px-3 py-2 text-slate-300">{row.description}</td>
                              </tr>
                            ))
                          ) : (
                            <tr>
                              <td className="px-3 py-6 text-center text-slate-400" colSpan={4}>
                                No parts demand has been defined yet.
                              </td>
                            </tr>
                          )}
                        </tbody>
                      </table>
                    </div>

                    <label className="block text-sm text-slate-300">
                      Labor task list
                      <textarea
                        className={textareaClass}
                        value={checklistTasksText}
                        onChange={(event) => setChecklistTasksText(event.target.value)}
                        placeholder="taskKey|Title|Description|Sort order"
                        data-testid="pm-program-wo-tasks"
                      />
                    </label>

                    <div className="overflow-hidden rounded-2xl border border-slate-700">
                      <table className="min-w-full divide-y divide-slate-700 text-sm">
                        <thead className="bg-slate-950/60 text-left text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                          <tr>
                            <th className="px-3 py-2">Task key</th>
                            <th className="px-3 py-2">Title</th>
                            <th className="px-3 py-2">Sort</th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-slate-800 bg-slate-950/40 text-slate-200">
                          {parseChecklistTasks(checklistTasksText).length > 0 ? (
                            parseChecklistTasks(checklistTasksText).map((row) => (
                              <tr key={row.taskKey}>
                                <td className="px-3 py-2 font-medium text-white">{row.taskKey}</td>
                                <td className="px-3 py-2 text-slate-300">{row.title}</td>
                                <td className="px-3 py-2">{row.sortOrder}</td>
                              </tr>
                            ))
                          ) : (
                            <tr>
                              <td className="px-3 py-6 text-center text-slate-400" colSpan={3}>
                                No checklist tasks have been defined yet.
                              </td>
                            </tr>
                          )}
                        </tbody>
                      </table>
                    </div>
                  </>
                ) : (
                  <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4 text-sm text-slate-300">
                    Enable work order generation to define the work package that MaintainArr should create.
                  </div>
                )}
              </div>

              <div className="space-y-4">
                <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4">
                  <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Title preview</div>
                  <h3 className="mt-1 text-base font-semibold text-white">{titlePreview}</h3>
                  <p className="mt-2 text-sm text-slate-300">
                    The preview uses a sample matched asset when available.
                  </p>
                </div>
                <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4 text-sm text-slate-300">
                  <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Work package summary</div>
                  <p className="mt-2">
                    {generateWorkOrder
                      ? 'This PM program will create a work order when the due logic is met and automation is enabled.'
                      : 'No work order will be generated until you enable work order generation.'}
                  </p>
                </div>
                {workPackageErrors.length > 0 ? (
                  <div className="rounded-2xl border border-red-700/60 bg-red-950/25 p-4 text-sm text-red-100">
                    <div className="font-semibold">Work package issues</div>
                    <ul className="mt-2 list-disc space-y-1 pl-5">
                      {workPackageErrors.map((error) => (
                        <li key={error}>{error}</li>
                      ))}
                    </ul>
                  </div>
                ) : null}
              </div>
            </div>
          </SectionCard>

          <SectionCard
            title="Inspection Attachment"
            summary={sectionSummaries[4]}
            stateLabel={sectionLabels[4]}
            tone={sectionTones[4]}
            expanded={openSectionIndex === 4}
            locked={sectionLocked[4]}
            icon={sectionIcons[4]}
            onToggle={() => setOpenSectionIndex(4)}
            testId="pm-program-section-inspection"
          >
            <div className="grid gap-4 xl:grid-cols-[minmax(0,1.7fr)_minmax(340px,1fr)]">
              <div className="space-y-4">
                <label className="flex items-center gap-2 rounded-2xl border border-slate-700 bg-slate-950/70 px-3 py-4 text-sm text-slate-200">
                  <input
                    type="checkbox"
                    checked={attachInspectionTemplate}
                    onChange={(event) => setAttachInspectionTemplate(event.target.checked)}
                    className="h-4 w-4 rounded border-slate-500 bg-slate-950 text-sky-500"
                  />
                  Attach inspection template
                </label>

                <div className="grid gap-4 md:grid-cols-2">
                  <StaticSearchPicker
                    label="Inspection template"
                    value={inspectionTemplateId}
                    onChange={setInspectionTemplateId}
                    options={inspectionTemplateOptions}
                    placeholder="Search inspection templates…"
                    disabled={!attachInspectionTemplate}
                    testId="pm-program-inspection-template"
                  />
                  <label className="flex items-center gap-2 rounded-2xl border border-slate-700 bg-slate-950/70 px-3 py-4 text-sm text-slate-200">
                    <input
                      type="checkbox"
                      checked={inspectionRequiredBeforeWorkOrderCompletion}
                      onChange={(event) => setInspectionRequiredBeforeWorkOrderCompletion(event.target.checked)}
                      className="h-4 w-4 rounded border-slate-500 bg-slate-950 text-sky-500"
                    />
                    Inspection required before WO completion
                  </label>
                </div>

                <ControlledSelect
                  label="Inspection result behavior"
                  value={inspectionResultBehavior}
                  onChange={(value) => setInspectionResultBehavior(value as InspectionResultBehavior)}
                  options={mapSimpleOptions([
                    { value: 'pass_completes', label: 'Pass completes inspection only' },
                    { value: 'fail_creates_defect', label: 'Fail creates defect' },
                    { value: 'fail_blocks_readiness', label: 'Fail blocks asset readiness' },
                    { value: 'fail_requires_corrective_work_order', label: 'Fail requires corrective work order' },
                  ])}
                  testId="pm-program-inspection-result"
                  emptyLabel="Select result behavior…"
                />

                <div className="grid gap-4 md:grid-cols-2">
                  <label className="flex items-center gap-2 rounded-2xl border border-slate-700 bg-slate-950/70 px-3 py-4 text-sm text-slate-200">
                    <input
                      type="checkbox"
                      checked={resumeBehaviorRespectsEngineRules}
                      onChange={(event) => setResumeBehaviorRespectsEngineRules(event.target.checked)}
                      className="h-4 w-4 rounded border-slate-500 bg-slate-950 text-sky-500"
                    />
                    Resume respects engine rules
                  </label>
                  <label className="flex items-center gap-2 rounded-2xl border border-slate-700 bg-slate-950/70 px-3 py-4 text-sm text-slate-200">
                    <input
                      type="checkbox"
                      checked={voiceCompatible}
                      onChange={(event) => setVoiceCompatible(event.target.checked)}
                      className="h-4 w-4 rounded border-slate-500 bg-slate-950 text-sky-500"
                    />
                    Voice inspection compatible
                  </label>
                </div>
              </div>

              <div className="space-y-4">
                <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4">
                  <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Selected template</div>
                  {selectedInspectionTemplate ? (
                    <div className="mt-2 space-y-2 text-sm text-slate-300">
                      <div className="font-semibold text-white">{selectedInspectionTemplate.name}</div>
                      <div>Key: {selectedInspectionTemplate.templateKey}</div>
                      <div>Status: {humanize(selectedInspectionTemplate.status)}</div>
                      <div>Version: {selectedInspectionTemplate.version}</div>
                      <div>Checklist items: {selectedInspectionTemplate.checklistItems.length}</div>
                      <div>Linked asset types: {selectedInspectionTemplate.linkedAssetTypes.length}</div>
                    </div>
                  ) : (
                    <p className="mt-2 text-sm text-slate-300">Pick a template to see its compatibility summary.</p>
                  )}
                </div>
                {selectedInspectionTemplate?.linkedAssetTypes?.length ? (
                  <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4">
                    <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Linked asset types</div>
                    <div className="mt-3 flex flex-wrap gap-2">
                      {selectedInspectionTemplate.linkedAssetTypes.map((assetType) => (
                        <DetailBadge key={assetType.assetTypeId} label={assetType.typeName} tone="info" />
                      ))}
                    </div>
                  </div>
                ) : null}
                {inspectionErrors.length > 0 ? (
                  <div className="rounded-2xl border border-red-700/60 bg-red-950/25 p-4 text-sm text-red-100">
                    <div className="font-semibold">Inspection issues</div>
                    <ul className="mt-2 list-disc space-y-1 pl-5">
                      {inspectionErrors.map((error) => (
                        <li key={error}>{error}</li>
                      ))}
                    </ul>
                  </div>
                ) : null}
              </div>
            </div>
          </SectionCard>

          <SectionCard
            title="Compliance and Readiness"
            summary={sectionSummaries[5]}
            stateLabel={sectionLabels[5]}
            tone={sectionTones[5]}
            expanded={openSectionIndex === 5}
            locked={sectionLocked[5]}
            icon={sectionIcons[5]}
            onToggle={() => setOpenSectionIndex(5)}
            testId="pm-program-section-compliance"
          >
            <div className="grid gap-4 xl:grid-cols-[minmax(0,1.7fr)_minmax(340px,1fr)]">
              <div className="space-y-4">
                <label className="flex items-center gap-2 rounded-2xl border border-slate-700 bg-slate-950/70 px-3 py-4 text-sm text-slate-200">
                  <input
                    type="checkbox"
                    checked={isComplianceRelated}
                    onChange={(event) => setIsComplianceRelated(event.target.checked)}
                    className="h-4 w-4 rounded border-slate-500 bg-slate-950 text-sky-500"
                  />
                  Compliance related
                </label>

                <div className="grid gap-4 md:grid-cols-2">
                  <StaticSearchPicker
                    label="Governing body"
                    value={governingBodyCatalogKey}
                    onChange={setGoverningBodyCatalogKey}
                    options={governingBodyOptions}
                    placeholder="Search governing bodies…"
                    disabled={!isComplianceRelated}
                    testId="pm-program-governing-body"
                  />
                  <ControlledSelect
                    label="Readiness impact"
                    value={readinessImpact}
                    onChange={(value) => setReadinessImpact(value as ReadinessImpact)}
                    options={mapSimpleOptions([
                      { value: 'no_impact', label: 'No readiness impact' },
                      { value: 'warn_when_due_soon', label: 'Warn when due soon' },
                      { value: 'not_ready_overdue', label: 'Mark asset not ready when overdue' },
                      { value: 'not_ready_failed_inspection', label: 'Mark not ready on failed inspection' },
                      { value: 'supervisor_release', label: 'Require supervisor release after failure' },
                    ])}
                    testId="pm-program-readiness-impact"
                    emptyLabel="Select readiness impact…"
                  />
                </div>

                <label className="block text-sm text-slate-300">
                  Regulation or law citations
                  <textarea
                    className={textareaClass}
                    value={citationReferencesText}
                    onChange={(event) => setCitationReferencesText(event.target.value)}
                    placeholder="49 CFR 396.17, OSHA 1910.178"
                    data-testid="pm-program-citations"
                  />
                </label>

                <label className="block text-sm text-slate-300">
                  Certificate or qualification requirements
                  <textarea
                    className={textareaClass}
                    value={certificateRequirementsText}
                    onChange={(event) => setCertificateRequirementsText(event.target.value)}
                    placeholder="qualified technician, supervisor review"
                    data-testid="pm-program-certifications"
                  />
                </label>

                <div className="grid gap-4 md:grid-cols-2">
                  <label className="flex items-center gap-2 rounded-2xl border border-slate-700 bg-slate-950/70 px-3 py-4 text-sm text-slate-200">
                    <input
                      type="checkbox"
                      checked={confirmComplianceImpact}
                      onChange={(event) => setConfirmComplianceImpact(event.target.checked)}
                      className="h-4 w-4 rounded border-slate-500 bg-slate-950 text-sky-500"
                    />
                    Confirm compliance impact
                  </label>
                  <label className="flex items-center gap-2 rounded-2xl border border-slate-700 bg-slate-950/70 px-3 py-4 text-sm text-slate-200">
                    <input
                      type="checkbox"
                      checked={confirmReadinessImpact}
                      onChange={(event) => setConfirmReadinessImpact(event.target.checked)}
                      className="h-4 w-4 rounded border-slate-500 bg-slate-950 text-sky-500"
                    />
                    Confirm readiness impact
                  </label>
                </div>
              </div>

              <div className="space-y-4">
                <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4 text-sm text-slate-300">
                  <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Compliance summary</div>
                  <p className="mt-2">{complianceSummary}</p>
                  {readinessImpact !== 'no_impact' ? (
                    <div className="mt-3 rounded-xl border border-amber-500/30 bg-amber-950/20 p-3 text-amber-100">
                      This PM can make assets unavailable or not ready.
                    </div>
                  ) : null}
                </div>
                {complianceErrors.length > 0 ? (
                  <div className="rounded-2xl border border-red-700/60 bg-red-950/25 p-4 text-sm text-red-100">
                    <div className="font-semibold">Compliance issues</div>
                    <ul className="mt-2 list-disc space-y-1 pl-5">
                      {complianceErrors.map((error) => (
                        <li key={error}>{error}</li>
                      ))}
                    </ul>
                  </div>
                ) : null}
              </div>
            </div>
          </SectionCard>

          <SectionCard
            title="Automation and Scheduling"
            summary={sectionSummaries[6]}
            stateLabel={sectionLabels[6]}
            tone={sectionTones[6]}
            expanded={openSectionIndex === 6}
            locked={sectionLocked[6]}
            icon={sectionIcons[6]}
            onToggle={() => setOpenSectionIndex(6)}
            testId="pm-program-section-automation"
          >
            <div className="grid gap-4 xl:grid-cols-[minmax(0,1.7fr)_minmax(340px,1fr)]">
              <div className="space-y-4">
                <div className="grid gap-4 md:grid-cols-2">
                  <label className="flex items-center gap-2 rounded-2xl border border-slate-700 bg-slate-950/70 px-3 py-4 text-sm text-slate-200">
                    <input
                      type="checkbox"
                      checked={autoGenerateWorkOrder}
                      onChange={(event) => setAutoGenerateWorkOrder(event.target.checked)}
                      disabled={!canManageAutomation}
                      className="h-4 w-4 rounded border-slate-500 bg-slate-950 text-sky-500 disabled:cursor-not-allowed"
                    />
                    Auto-generate work orders
                  </label>
                  <label className="flex items-center gap-2 rounded-2xl border border-slate-700 bg-slate-950/70 px-3 py-4 text-sm text-slate-200">
                    <input
                      type="checkbox"
                      checked={autoGenerateInspection}
                      onChange={(event) => setAutoGenerateInspection(event.target.checked)}
                      disabled={!canManageAutomation}
                      className="h-4 w-4 rounded border-slate-500 bg-slate-950 text-sky-500 disabled:cursor-not-allowed"
                    />
                    Auto-generate inspections
                  </label>
                </div>

                <div className="grid gap-4 md:grid-cols-2">
                  <label className="block text-sm text-slate-300">
                    Lead time before due date
                    <input
                      className={smallFieldClass}
                      value={leadTimeDays}
                      onChange={(event) => setLeadTimeDays(event.target.value)}
                      inputMode="numeric"
                      placeholder="14"
                      disabled={!canManageAutomation}
                      data-testid="pm-program-lead-time"
                    />
                  </label>
                  <label className="block text-sm text-slate-300">
                    Lead mileage / hours / meter threshold
                    <input
                      className={smallFieldClass}
                      value={leadThresholdValue}
                      onChange={(event) => setLeadThresholdValue(event.target.value)}
                      inputMode="decimal"
                      placeholder="1000"
                      disabled={!canManageAutomation}
                      data-testid="pm-program-lead-threshold"
                    />
                  </label>
                  <ControlledSelect
                    label="Threshold unit"
                    value={leadThresholdUnit}
                    onChange={setLeadThresholdUnit}
                    options={meterUnitOptions}
                    disabled={!canManageAutomation}
                    testId="pm-program-lead-threshold-unit"
                    emptyLabel="Select unit…"
                  />
                  <label className="block text-sm text-slate-300">
                    Duplicate prevention window days
                    <input
                      className={smallFieldClass}
                      value={duplicatePreventionWindowDays}
                      onChange={(event) => setDuplicatePreventionWindowDays(event.target.value)}
                      inputMode="numeric"
                      placeholder="7"
                      disabled={!canManageAutomation}
                      data-testid="pm-program-duplicate-window"
                    />
                  </label>
                </div>

                <ControlledSelect
                  label="Assignment behavior"
                  value={assignmentBehavior}
                  onChange={(value) => setAssignmentBehavior(value as AssignmentBehavior)}
                  options={mapSimpleOptions([
                    { value: 'unassigned', label: 'Unassigned' },
                    { value: 'team', label: 'Assign to team' },
                    { value: 'role', label: 'Assign to role' },
                    { value: 'person', label: 'Assign to person' },
                  ])}
                  disabled={!canManageAutomation}
                  testId="pm-program-assignment-behavior"
                  emptyLabel="Select assignment mode…"
                />

                {assignmentBehavior === 'team' ? (
                  <StaticSearchPicker
                    label="Assignment team"
                    value={assignmentTeamRef}
                    onChange={setAssignmentTeamRef}
                    options={teamOptions}
                    placeholder="Search teams…"
                    disabled={!canManageAutomation}
                    testId="pm-program-assignment-team"
                  />
                ) : null}
                {assignmentBehavior === 'person' ? (
                  <StaticSearchPicker
                    label="Assignment person"
                    value={assignmentPersonId}
                    onChange={setAssignmentPersonId}
                    options={personOptions}
                    placeholder="Search people…"
                    disabled={!canManageAutomation}
                    testId="pm-program-assignment-person"
                  />
                ) : null}
                {assignmentBehavior === 'role' ? (
                  <label className="block text-sm text-slate-300">
                    Assignment role key
                    <input
                      className={smallFieldClass}
                      value={assignmentRoleKey}
                      onChange={(event) => setAssignmentRoleKey(event.target.value)}
                      placeholder="maintenance_supervisor"
                      disabled={!canManageAutomation}
                      data-testid="pm-program-assignment-role"
                    />
                  </label>
                ) : null}

                <div className="grid gap-4 md:grid-cols-2">
                  <label className="block text-sm text-slate-300">
                    Notification targets
                    <textarea
                      className={textareaClass}
                      value={notificationTargetsText}
                      onChange={(event) => setNotificationTargetsText(event.target.value)}
                      placeholder="maintenance_manager, site_lead"
                      disabled={!canManageAutomation}
                      data-testid="pm-program-notification-targets"
                    />
                  </label>
                  <label className="block text-sm text-slate-300">
                    Escalation targets
                    <textarea
                      className={textareaClass}
                      value={escalationTargetsText}
                      onChange={(event) => setEscalationTargetsText(event.target.value)}
                      placeholder="tenant_admin, compliance_manager"
                      disabled={!canManageAutomation}
                      data-testid="pm-program-escalation-targets"
                    />
                  </label>
                  <label className="block text-sm text-slate-300">
                    Blackout windows
                    <textarea
                      className={textareaClass}
                      value={blackoutWindowsText}
                      onChange={(event) => setBlackoutWindowsText(event.target.value)}
                      placeholder="friday-night, holiday-shutdown"
                      disabled={!canManageAutomation}
                      data-testid="pm-program-blackout-windows"
                    />
                  </label>
                  <label className="block text-sm text-slate-300">
                    Maximum open generated items per asset
                    <input
                      className={smallFieldClass}
                      value={maxOpenGeneratedItemsPerAsset}
                      onChange={(event) => setMaxOpenGeneratedItemsPerAsset(event.target.value)}
                      inputMode="numeric"
                      placeholder="1"
                      disabled={!canManageAutomation}
                      data-testid="pm-program-max-open-items"
                    />
                  </label>
                </div>
              </div>

              <div className="space-y-4">
                <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4 text-sm text-slate-300">
                  <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Automation summary</div>
                  <p className="mt-2">{automationSummary}</p>
                  <p className="mt-3 text-xs text-slate-400">
                    Duplicate prevention keeps MaintainArr from generating a new item if an open one already exists in the configured window.
                  </p>
                </div>
                {automationErrors.length > 0 ? (
                  <div className="rounded-2xl border border-red-700/60 bg-red-950/25 p-4 text-sm text-red-100">
                    <div className="font-semibold">Automation issues</div>
                    <ul className="mt-2 list-disc space-y-1 pl-5">
                      {automationErrors.map((error) => (
                        <li key={error}>{error}</li>
                      ))}
                    </ul>
                  </div>
                ) : null}
                {!canManageAutomation ? (
                  <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4 text-sm text-slate-400">
                    Your role cannot change automation settings. The controls are shown for review only.
                  </div>
                ) : null}
              </div>
            </div>
          </SectionCard>

          <SectionCard
            title="Review and Activate"
            summary={sectionSummaries[7]}
            stateLabel={sectionLabels[7]}
            tone={sectionTones[7]}
            expanded={openSectionIndex === 7}
            locked={sectionLocked[7]}
            icon={sectionIcons[7]}
            onToggle={() => setOpenSectionIndex(7)}
            testId="pm-program-section-review"
            footer={
              <div className="space-y-4">
                <div className="grid gap-4 xl:grid-cols-2">
                  <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4">
                    <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Program basics</div>
                    <div className="mt-3 space-y-2 text-sm text-slate-300">
                      <p>
                        <span className="text-[var(--color-text-muted)]">Name:</span> {programName.trim() || 'Not set'}
                      </p>
                      <p>
                        <span className="text-[var(--color-text-muted)]">Code:</span> {effectiveProgramKey || 'Not set'}
                      </p>
                      <p>
                        <span className="text-[var(--color-text-muted)]">Category:</span>{' '}
                        {programCategoryOptions.find((option) => option.value === categoryKey)?.label ?? 'Not set'}
                      </p>
                      <p>
                        <span className="text-[var(--color-text-muted)]">Work type:</span>{' '}
                        {workTypeOptions.find((option) => option.value === workTypeKey)?.label ?? 'Not set'}
                      </p>
                      <p>
                        <span className="text-[var(--color-text-muted)]">Priority:</span>{' '}
                        {priorityOptions.find((option) => option.value === priorityKey)?.label ?? 'Not set'}
                      </p>
                    </div>
                  </div>
                  <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4">
                    <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Asset scope</div>
                    <p className="mt-3 text-sm text-slate-300">{scopeSummary}</p>
                    <p className="mt-2 text-sm text-slate-400">
                      Included assets: {includedAssetIds.length} · Excluded assets: {excludedAssetIds.length}
                    </p>
                    <p className="mt-2 text-sm text-slate-400">
                      Site labels always resolve from StaffArr references.
                    </p>
                  </div>
                  <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4">
                    <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Due logic</div>
                    <p className="mt-3 text-sm text-slate-300">{dueSummary}</p>
                    <p className="mt-2 text-sm text-slate-400">
                      {duePreviewItems.length > 0
                        ? `${duePreviewItems.length} sample preview item${duePreviewItems.length === 1 ? '' : 's'}`
                        : 'No due preview items yet.'}
                    </p>
                  </div>
                  <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4">
                    <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Work, inspection, compliance, automation</div>
                    <ul className="mt-3 space-y-2 text-sm text-slate-300">
                      <li>{workSummary}</li>
                      <li>{inspectionSummary}</li>
                      <li>{complianceSummary}</li>
                      <li>{automationSummary}</li>
                    </ul>
                  </div>
                </div>

                {previewWarnings.length > 0 ? (
                  <div className="rounded-2xl border border-amber-500/30 bg-amber-950/20 p-4 text-sm text-amber-100">
                    <div className="font-semibold">Warnings</div>
                    <ul className="mt-2 list-disc space-y-1 pl-5">
                      {previewWarnings.map((warning) => (
                        <li key={warning}>{warning}</li>
                      ))}
                    </ul>
                  </div>
                ) : null}

                {activationErrors.length > 0 ? (
                  <div className="rounded-2xl border border-red-700/60 bg-red-950/25 p-4 text-sm text-red-100">
                    <div className="font-semibold">Activation blockers</div>
                    <ul className="mt-2 list-disc space-y-1 pl-5">
                      {activationErrors.map((error) => (
                        <li key={error}>{error}</li>
                      ))}
                    </ul>
                  </div>
                ) : null}

                <div className="flex flex-wrap items-center gap-3">
                  <button
                    type="button"
                    className="inline-flex items-center gap-2 rounded-xl border border-slate-700 bg-slate-950 px-4 py-3 text-sm font-semibold text-slate-100 disabled:cursor-not-allowed disabled:opacity-40"
                    onClick={handleSaveDraft}
                    disabled={!draftReady || createMutation.isPending || activateMutation.isPending}
                    data-testid="pm-program-save-draft"
                  >
                    {createMutation.isPending ? <Loader2 className="h-4 w-4 animate-spin" /> : null}
                    Save Draft
                  </button>
                  <button
                    type="button"
                    className="inline-flex items-center gap-2 rounded-xl bg-sky-600 px-4 py-3 text-sm font-semibold text-white disabled:cursor-not-allowed disabled:opacity-40"
                    onClick={handleCreateAndActivate}
                    disabled={!activationReady || createMutation.isPending || activateMutation.isPending}
                    data-testid="pm-program-create-activate"
                  >
                    {activateMutation.isPending || createMutation.isPending ? (
                      <Loader2 className="h-4 w-4 animate-spin" />
                    ) : (
                      <ArrowRight className="h-4 w-4" />
                    )}
                    Create and Activate
                  </button>
                  <Link
                    to="/pm-programs"
                    className="inline-flex items-center gap-2 rounded-xl border border-slate-700 px-4 py-3 text-sm font-semibold text-slate-100"
                  >
                    Cancel
                  </Link>
                </div>

                <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4 text-sm text-slate-300">
                  <div className="font-semibold text-white">Estimated generated workload</div>
                  <p className="mt-2">
                    {scopeMatchedCount > 0
                      ? `${scopeMatchedCount} asset${scopeMatchedCount === 1 ? '' : 's'} matched`
                      : 'No matched assets yet'}
                    {autoGenerateWorkOrder ? ` · ${scopeMatchedCount} work order${scopeMatchedCount === 1 ? '' : 's'}` : ''}
                    {autoGenerateInspection
                      ? ` · ${scopeMatchedCount} inspection${scopeMatchedCount === 1 ? '' : 's'}`
                      : ''}
                  </p>
                </div>
              </div>
            }
          >
            <div className="grid gap-4 xl:grid-cols-2">
              <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4 text-sm text-slate-300">
                <div className="font-semibold text-white">Final review</div>
                <p className="mt-2">
                  Review the summaries, warnings, and blockers before saving a draft or activating the program.
                </p>
              </div>
              <div className="rounded-2xl border border-slate-700 bg-slate-950/70 p-4 text-sm text-slate-300">
                <div className="font-semibold text-white">Status</div>
                <div className="mt-3 flex flex-wrap gap-2">
                  <DetailBadge label="Draft" tone="warn" />
                  <DetailBadge label="Active" tone="good" />
                  <DetailBadge label="Paused" tone="info" />
                  <DetailBadge label="Retired" tone="neutral" />
                </div>
                <p className="mt-3">
                  Drafts can be saved before every section is complete. Activation requires all blockers to be resolved.
                </p>
              </div>
            </div>
          </SectionCard>
        </div>
      </div>
    </div>
  )
}
