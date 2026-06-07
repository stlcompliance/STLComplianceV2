import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, Navigate, useNavigate, useSearchParams } from 'react-router-dom'
import {
  AlertTriangle,
  ArrowLeft,
  CheckCircle2,
  Loader2,
  Sparkles,
} from 'lucide-react'
import { ControlledSelect, PageHeader, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import {
  checkDuplicateWorkOrderDraft,
  getAssetReadiness,
  createWorkOrderDraft,
  getAssets,
  getDefects,
  getDefect,
  getMe,
  getPmSchedules,
  getWorkOrder,
  getWorkOrderCreateFieldset,
  openWorkOrderDraft,
  previewWorkOrderDraft,
  scheduleWorkOrderDraft,
  startWorkOrderDraft,
  validateWorkOrderDraft,
  updateWorkOrderDraft,
} from '../../api/client'
import type {
  AssetResponse,
  CatalogOptionResponse,
  FieldMetadataResponse,
  FieldsetResponse,
  MaintainArrMeResponse,
  WorkOrderDetailResponse,
  WorkOrderFindingResponse,
  WorkOrderPreviewResponse,
} from '../../api/types'
import { canCreateWorkOrders, loadSession } from '../../auth/sessionStorage'
import {
  fieldIsVisible,
  getFilteredOptions,
  initializeAssetFieldValues,
  validateAssetValues,
  type AssetFieldValues,
} from '../../components/AssetFieldsetWorkflow'

type StepKey =
  | 'basics'
  | 'source'
  | 'scope'
  | 'classification'
  | 'assignment'
  | 'readiness'
  | 'compliance'
  | 'scheduling'
  | 'documents'
  | 'review'

type WorkOrderCreateValues = AssetFieldValues & {
  assetId: string
  title: string
  description: string
  priority: string
  defectId: string
  pmScheduleId: string
  assignedTechnicianPersonId: string
  plannedStartAt: string
  plannedDueAt: string
}

interface StepDefinition {
  key: StepKey
  label: string
  description: string
}

const STEP_DEFINITIONS: StepDefinition[] = [
  { key: 'basics', label: 'Basics', description: 'Record identity, title, type, and priority.' },
  { key: 'source', label: 'Source', description: 'Link a defect or PM schedule if this work order came from one.' },
  { key: 'scope', label: 'Scope', description: 'Describe the work and capture the scope summary.' },
  { key: 'classification', label: 'Classification', description: 'Capture failure mode, severity, disposition, and root cause.' },
  { key: 'assignment', label: 'Assignment', description: 'Choose the technician and StaffArr ownership context.' },
  { key: 'readiness', label: 'Readiness', description: 'Check asset readiness before the work order is opened.' },
  { key: 'compliance', label: 'Compliance', description: 'Select any Compliance Core references and evidence needs.' },
  { key: 'scheduling', label: 'Scheduling', description: 'Plan start and due dates for the work order.' },
  { key: 'documents', label: 'Documents', description: 'Add document and note context for the draft.' },
  { key: 'review', label: 'Review', description: 'Preview blockers, duplicates, and choose the final action.' },
]

function trimToEmpty(value: unknown): string {
  return typeof value === 'string' ? value.trim() : ''
}

function isMeaningfulValue(value: unknown): boolean {
  if (Array.isArray(value)) {
    return value.some((item) => trimToEmpty(item).length > 0)
  }

  return trimToEmpty(value).length > 0
}

function toPickerOptions(options: CatalogOptionResponse[] | null | undefined): PickerOption[] {
  return (options ?? []).map((option) => ({
    value: option.key,
    label: option.label || option.key,
  }))
}

function formatDateTimeInput(value: string | null | undefined): string {
  if (!value) return ''
  const parsed = new Date(value)
  if (Number.isNaN(parsed.getTime())) return ''
  const offsetMs = parsed.getTimezoneOffset() * 60_000
  return new Date(parsed.getTime() - offsetMs).toISOString().slice(0, 16)
}

function parseDateTimeInput(value: string): string | null {
  const trimmed = value.trim()
  if (!trimmed) return null
  const parsed = new Date(trimmed)
  return Number.isNaN(parsed.getTime()) ? null : parsed.toISOString()
}

function humanizeKey(value: string): string {
  return value
    .replace(/[_-]+/g, ' ')
    .replace(/\b\w/g, (char) => char.toUpperCase())
}

function mapSeverityToPriority(severity: string | null | undefined): string {
  switch ((severity ?? '').toLowerCase()) {
    case 'critical':
      return 'urgent'
    case 'high':
      return 'high'
    case 'medium':
      return 'medium'
    case 'low':
      return 'low'
    default:
      return 'medium'
  }
}

function mapDueStatusToPriority(dueStatus: string | null | undefined): string {
  switch ((dueStatus ?? '').toLowerCase()) {
    case 'overdue':
    case 'due_today':
      return 'urgent'
    case 'due_soon':
      return 'high'
    default:
      return 'medium'
  }
}

function buildInitialValues(fieldset: FieldsetResponse): WorkOrderCreateValues {
  const defaults = initializeAssetFieldValues(fieldset) as AssetFieldValues
  return {
    ...defaults,
    assetId: '',
    title: '',
    description: '',
    priority: 'medium',
    defectId: '',
    pmScheduleId: '',
    assignedTechnicianPersonId: '',
    plannedStartAt: '',
    plannedDueAt: '',
  }
}

function mergeDraftIntoValues(
  fieldset: FieldsetResponse,
  draft: WorkOrderDetailResponse,
  current: WorkOrderCreateValues,
): WorkOrderCreateValues {
  const parsedPlan = parseDraftPlanJson(draft.draftPlanJson)
  const defaults = initializeAssetFieldValues(fieldset) as AssetFieldValues
  return {
    ...defaults,
    ...current,
    ...parsedPlan,
    assetId: draft.assetId,
    title: draft.title,
    description: draft.description,
    priority: draft.priority,
    defectId: draft.defectId ?? '',
    pmScheduleId: draft.pmScheduleId ?? '',
    assignedTechnicianPersonId: draft.assignedTechnicianPersonId ?? '',
    plannedStartAt: formatDateTimeInput(draft.plannedStartAt ?? null),
    plannedDueAt: formatDateTimeInput(draft.plannedDueAt ?? null),
  }
}

function parseDraftPlanJson(draftPlanJson: string | null | undefined): Record<string, unknown> {
  if (!draftPlanJson) return {}
  try {
    const parsed = JSON.parse(draftPlanJson)
    return parsed && typeof parsed === 'object' && !Array.isArray(parsed) ? (parsed as Record<string, unknown>) : {}
  } catch {
    return {}
  }
}

function clearInvalidDependentValues(
  fieldset: FieldsetResponse,
  values: WorkOrderCreateValues,
  changedFieldKey: string,
): WorkOrderCreateValues {
  let next = { ...values }
  for (const field of fieldset.fields) {
    if (field.key === changedFieldKey) continue
    if (!field.catalogKey && !field.referenceKey) continue

    const filtered = getFilteredOptions(fieldset, field, next)
    if (filtered.length === 0) continue

    const allowed = new Set(filtered.map((option) => option.key))
    const current = next[field.key]
    if (Array.isArray(current)) {
      const retained = current.filter((item) => allowed.has(String(item)))
      if (retained.length !== current.length) {
        next = { ...next, [field.key]: retained }
      }
      continue
    }

    if (current != null && trimToEmpty(current).length > 0 && !allowed.has(String(current))) {
      next = { ...next, [field.key]: '' }
    }
  }
  return next
}

function sectionFields(fieldset: FieldsetResponse | undefined, sectionKey: string, values: WorkOrderCreateValues) {
  return (fieldset?.fields ?? []).filter(
    (field) => field.sectionKey === sectionKey && fieldIsVisible(fieldset!, field, values),
  )
}

function buildDraftPlanPayload(fieldset: FieldsetResponse, values: WorkOrderCreateValues): Record<string, unknown> {
  const payload: Record<string, unknown> = {}
  const excludedKeys = new Set([
    'priority',
    'assignedTechnicianPersonId',
    'assetId',
    'title',
    'description',
    'defectId',
    'pmScheduleId',
    'plannedStartAt',
    'plannedDueAt',
  ])

  for (const field of fieldset.fields) {
    if (excludedKeys.has(field.key)) {
      continue
    }

    const value = values[field.key]
    if (Array.isArray(value)) {
      const retained = value.map((item) => String(item).trim()).filter(Boolean)
      if (retained.length > 0) {
        payload[field.key] = retained
      }
      continue
    }

    if (isMeaningfulValue(value)) {
      payload[field.key] = trimToEmpty(value)
    }
  }

  return payload
}

function buildWorkOrderRequest(fieldset: FieldsetResponse, values: WorkOrderCreateValues) {
  return {
    assetId: values.assetId.trim(),
    title: values.title.trim(),
    description: values.description.trim(),
    priority: values.priority.trim(),
    assignedTechnicianPersonId: trimToEmpty(values.assignedTechnicianPersonId) || null,
    pmScheduleId: trimToEmpty(values.pmScheduleId) || null,
    defectId: trimToEmpty(values.defectId) || null,
    draftPlanJson: JSON.stringify(buildDraftPlanPayload(fieldset, values)),
    plannedStartAt: parseDateTimeInput(values.plannedStartAt),
    plannedDueAt: parseDateTimeInput(values.plannedDueAt),
  }
}

function renderFindingBadge(severity: string): string {
  switch (severity.toLowerCase()) {
    case 'blocker':
      return 'border-rose-500/30 bg-rose-500/10 text-rose-200'
    case 'warning':
      return 'border-amber-500/30 bg-amber-500/10 text-amber-200'
    default:
      return 'border-slate-600 bg-slate-900/80 text-slate-200'
  }
}

function renderReadableFinding(field: WorkOrderFindingResponse) {
  return `${field.code}: ${field.message}`
}

export function WorkOrderCreatePage() {
  const session = loadSession()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [searchParams, setSearchParams] = useSearchParams()
  const draftWorkOrderId = searchParams.get('workOrderId')?.trim() ?? ''
  const initialAssetId = searchParams.get('assetId')?.trim() ?? ''
  const initialDefectId = searchParams.get('defectId')?.trim() ?? ''
  const initialPmScheduleId = searchParams.get('pmScheduleId')?.trim() ?? ''
  const [values, setValues] = useState<WorkOrderCreateValues>({
    assetId: '',
    title: '',
    description: '',
    priority: 'medium',
    defectId: '',
    pmScheduleId: '',
    assignedTechnicianPersonId: '',
    plannedStartAt: '',
    plannedDueAt: '',
  })
  const [currentStepIndex, setCurrentStepIndex] = useState(0)
  const [highestUnlockedStepIndex, setHighestUnlockedStepIndex] = useState(0)
  const [serverError, setServerError] = useState<string | null>(null)
  const [previewResult, setPreviewResult] = useState<WorkOrderPreviewResponse | null>(null)

  const meQuery = useQuery({
    queryKey: ['maintainarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const fieldsetQuery = useQuery({
    queryKey: ['maintainarr-work-order-create-fieldset'],
    queryFn: () => getWorkOrderCreateFieldset(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const assetsQuery = useQuery({
    queryKey: ['maintainarr-work-order-create-assets', session?.accessToken],
    queryFn: () => getAssets(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const pmSchedulesQuery = useQuery({
    queryKey: ['maintainarr-work-order-create-pm-schedules', session?.accessToken],
    queryFn: () => getPmSchedules(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const defectsQuery = useQuery({
    queryKey: ['maintainarr-work-order-create-defects', session?.accessToken, values.assetId],
    queryFn: () => getDefects(session!.accessToken, { assetId: values.assetId, status: 'open' }),
    enabled: Boolean(session?.accessToken && values.assetId.trim()),
  })

  const draftQuery = useQuery({
    queryKey: ['maintainarr-work-order-create-draft', session?.accessToken, draftWorkOrderId],
    queryFn: () => getWorkOrder(session!.accessToken, draftWorkOrderId),
    enabled: Boolean(session?.accessToken && draftWorkOrderId),
    retry: false,
  })

  const selectedDefectQuery = useQuery({
    queryKey: ['maintainarr-work-order-create-defect', session?.accessToken, values.defectId],
    queryFn: () => getDefect(session!.accessToken, values.defectId),
    enabled: Boolean(session?.accessToken && values.defectId.trim()),
    retry: false,
  })

  const assetReadinessQuery = useQuery({
    queryKey: ['maintainarr-work-order-create-readiness', session?.accessToken, values.assetId],
    queryFn: () => getAssetReadiness(session!.accessToken, values.assetId),
    enabled: Boolean(session?.accessToken && values.assetId.trim()),
    retry: false,
  })

  const selectedAsset = useMemo(
    () => assetsQuery.data?.find((asset) => asset.assetId === values.assetId) ?? null,
    [assetsQuery.data, values.assetId],
  )

  const selectedDefect = selectedDefectQuery.data ?? null

  const selectedPmSchedule = useMemo(
    () => pmSchedulesQuery.data?.find((schedule) => schedule.pmScheduleId === values.pmScheduleId) ?? null,
    [pmSchedulesQuery.data, values.pmScheduleId],
  )

  const fieldset = fieldsetQuery.data ?? null
  const isReviewStep = currentStepIndex === STEP_DEFINITIONS.length - 1

  useEffect(() => {
    if (!fieldset) {
      return
    }
    setValues((current) => ({
      ...buildInitialValues(fieldset),
      ...current,
    }))
  }, [fieldset])

  useEffect(() => {
    if (!fieldset || !draftQuery.data) {
      return
    }

    if (!draftQuery.data.status || draftQuery.data.status.toLowerCase() !== 'draft') {
      return
    }

    setValues((current) => mergeDraftIntoValues(fieldset, draftQuery.data, current))
    setHighestUnlockedStepIndex(STEP_DEFINITIONS.length - 1)
  }, [draftQuery.data, fieldset])

  useEffect(() => {
    if (!fieldset || draftWorkOrderId) {
      return
    }

    setValues((current) => {
      let next = { ...current }

      if (!trimToEmpty(next.assetId) && initialAssetId) {
        next.assetId = initialAssetId
      }
      if (!trimToEmpty(next.defectId) && initialDefectId) {
        next.defectId = initialDefectId
      }
      if (!trimToEmpty(next.pmScheduleId) && initialPmScheduleId) {
        next.pmScheduleId = initialPmScheduleId
      }
      if (!trimToEmpty(next.workOrderType)) {
        next.workOrderType = 'corrective'
      }

      return next
    })
  }, [draftWorkOrderId, fieldset, initialAssetId, initialDefectId, initialPmScheduleId])

  useEffect(() => {
    if (!fieldset || draftWorkOrderId || !selectedDefect) {
      return
    }

    setValues((current) => {
      let next = { ...current }
      if (!trimToEmpty(next.assetId)) {
        next.assetId = selectedDefect.assetId
      }
      if (!trimToEmpty(next.title)) {
        next.title = selectedDefect.title
      }
      if (!trimToEmpty(next.description)) {
        next.description = selectedDefect.description
      }
      if (!trimToEmpty(next.priority) || next.priority === 'medium') {
        next.priority = mapSeverityToPriority(selectedDefect.severity)
      }
      if (!trimToEmpty(next.workOrderType) || next.workOrderType === 'corrective') {
        next.workOrderType = 'defect_repair'
      }
      return clearInvalidDependentValues(fieldset, next, 'defectId')
    })
  }, [draftWorkOrderId, fieldset, selectedDefect])

  useEffect(() => {
    if (!fieldset || draftWorkOrderId || !selectedPmSchedule) {
      return
    }

    setValues((current) => {
      let next = { ...current }
      if (!trimToEmpty(next.assetId)) {
        next.assetId = selectedPmSchedule.assetId
      }
      if (!trimToEmpty(next.title)) {
        next.title = `PM: ${selectedPmSchedule.name}`
      }
      if (!trimToEmpty(next.description)) {
        next.description = selectedPmSchedule.description
      }
      if (!trimToEmpty(next.priority) || next.priority === 'medium') {
        next.priority = mapDueStatusToPriority(selectedPmSchedule.dueStatus)
      }
      if (!trimToEmpty(next.workOrderType) || next.workOrderType === 'corrective') {
        next.workOrderType = 'preventive'
      }
      return clearInvalidDependentValues(fieldset, next, 'pmScheduleId')
    })
  }, [draftWorkOrderId, fieldset, selectedPmSchedule])

  const assetOptions = useMemo(
    () => (assetsQuery.data ?? []).map((asset: AssetResponse) => ({ value: asset.assetId, label: `${asset.assetTag} — ${asset.name}` })),
    [assetsQuery.data],
  )
  const selectedAssetOption = useMemo(
    () => assetOptions.find((option) => option.value === values.assetId) ?? null,
    [assetOptions, values.assetId],
  )

  const defectOptions = useMemo(() => {
    const options = (defectsQuery.data ?? []).map((defect) => ({
      value: defect.defectId,
      label: `${defect.title} — ${defect.assetTag}`,
    }))

    if (selectedDefect && !options.some((option) => option.value === selectedDefect.defectId)) {
      options.unshift({
        value: selectedDefect.defectId,
        label: `${selectedDefect.title} — ${selectedDefect.assetTag}`,
      })
    }

    return options
  }, [defectsQuery.data, selectedDefect])

  const pmScheduleOptions = useMemo(
    () =>
      (pmSchedulesQuery.data ?? [])
        .filter((schedule) => !values.assetId || schedule.assetId === values.assetId)
        .map((schedule) => ({
          value: schedule.pmScheduleId,
          label: `${schedule.scheduleKey} — ${schedule.name}`,
        })),
    [pmSchedulesQuery.data, values.assetId],
  )
  const selectedPmScheduleOption = useMemo(
    () => pmScheduleOptions.find((option) => option.value === values.pmScheduleId) ?? null,
    [pmScheduleOptions, values.pmScheduleId],
  )
  const selectedDefectOption = useMemo(
    () => defectOptions.find((option) => option.value === values.defectId) ?? null,
    [defectOptions, values.defectId],
  )

  const currentStep = STEP_DEFINITIONS[currentStepIndex]
  const basicsFields = sectionFields(fieldset ?? undefined, 'basics', values)
  const scopeFields = sectionFields(fieldset ?? undefined, 'scope', values)
  const classificationFields = sectionFields(fieldset ?? undefined, 'classification', values)
  const assignmentFields = sectionFields(fieldset ?? undefined, 'assignment', values)
  const complianceFields = sectionFields(fieldset ?? undefined, 'readiness', values)
  const documentFields = sectionFields(fieldset ?? undefined, 'documents', values)

  const basicsFieldErrors = fieldset ? validateAssetValues(fieldset, values, basicsFields) : {}
  const scopeFieldErrors = fieldset ? validateAssetValues(fieldset, values, scopeFields) : {}
  const classificationFieldErrors = fieldset ? validateAssetValues(fieldset, values, classificationFields) : {}
  const assignmentFieldErrors = fieldset ? validateAssetValues(fieldset, values, assignmentFields) : {}
  const complianceFieldErrors = fieldset ? validateAssetValues(fieldset, values, complianceFields) : {}
  const documentFieldErrors = fieldset ? validateAssetValues(fieldset, values, documentFields) : {}

  const basicsErrors: Record<string, string> = {
    ...basicsFieldErrors,
  }
  if (!trimToEmpty(values.assetId)) {
    basicsErrors.assetId = 'Asset is required.'
  } else if (selectedAsset === null && assetsQuery.data?.length) {
    basicsErrors.assetId = 'Selected asset is not available.'
  }
  if (!trimToEmpty(values.title)) {
    basicsErrors.title = 'Title is required.'
  }
  if (!trimToEmpty(values.priority)) {
    basicsErrors.priority = 'Priority is required.'
  }
  if (!trimToEmpty(values.workOrderType)) {
    basicsErrors.workOrderType = 'Work order type is required.'
  }

  const sourceErrors: Record<string, string> = {}
  if (trimToEmpty(values.defectId) && trimToEmpty(values.pmScheduleId)) {
    sourceErrors.defectId = 'Choose either a defect or a PM schedule, not both.'
  }
  if (trimToEmpty(values.defectId) && selectedDefectQuery.isError) {
    sourceErrors.defectId = 'Could not load the selected defect.'
  }
  if (selectedDefect && trimToEmpty(values.assetId) && selectedDefect.assetId !== values.assetId) {
    sourceErrors.defectId = 'The selected defect does not belong to the chosen asset.'
  }
  if (trimToEmpty(values.pmScheduleId) && selectedPmSchedule && trimToEmpty(values.assetId) && selectedPmSchedule.assetId !== values.assetId) {
    sourceErrors.pmScheduleId = 'The selected PM schedule does not belong to the chosen asset.'
  }

  const schedulingErrors: Record<string, string> = {}
  if (trimToEmpty(values.plannedStartAt) && !parseDateTimeInput(values.plannedStartAt)) {
    schedulingErrors.plannedStartAt = 'Planned start must be a valid date and time.'
  }
  if (trimToEmpty(values.plannedDueAt) && !parseDateTimeInput(values.plannedDueAt)) {
    schedulingErrors.plannedDueAt = 'Planned due must be a valid date and time.'
  }
  if (
    parseDateTimeInput(values.plannedStartAt)
    && parseDateTimeInput(values.plannedDueAt)
    && new Date(values.plannedDueAt).getTime() < new Date(values.plannedStartAt).getTime()
  ) {
    schedulingErrors.plannedDueAt = 'Planned due must be on or after the planned start.'
  }

  const currentErrors = (() => {
    switch (currentStep?.key) {
      case 'basics':
        return basicsErrors
      case 'source':
        return sourceErrors
      case 'scope':
        return scopeFieldErrors
      case 'classification':
        return classificationFieldErrors
      case 'assignment':
        return assignmentFieldErrors
      case 'compliance':
        return complianceFieldErrors
      case 'scheduling':
        return schedulingErrors
      case 'documents':
        return documentFieldErrors
      default:
        return {}
    }
  })()

  const allErrors = {
    ...basicsErrors,
    ...sourceErrors,
    ...scopeFieldErrors,
    ...classificationFieldErrors,
    ...assignmentFieldErrors,
    ...complianceFieldErrors,
    ...schedulingErrors,
    ...documentFieldErrors,
  }

  const currentStepComplete = Object.keys(currentErrors).length === 0
  const basicsComplete = Object.keys(basicsErrors).length === 0
  const sourceReady = Object.keys(sourceErrors).length === 0

  const createDraftMutation = useMutation({
    mutationFn: async () => {
      if (!fieldset) {
        throw new Error('Work order fieldset is not loaded.')
      }

      const payload = buildWorkOrderRequest(fieldset, values)
      if (draftWorkOrderId) {
        return updateWorkOrderDraft(session!.accessToken, draftWorkOrderId, payload)
      }
      return createWorkOrderDraft(session!.accessToken, payload)
    },
    onSuccess: async (saved) => {
      setServerError(null)
      setPreviewResult(null)
      const nextParams = new URLSearchParams(searchParams)
      nextParams.set('workOrderId', saved.workOrderId)
      setSearchParams(nextParams, { replace: true })
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-work-orders'] })
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to save work order draft.')
    },
  })

  const previewMutation = useMutation({
    mutationFn: async () => {
      const saved = draftWorkOrderId
        ? null
        : await createDraftMutation.mutateAsync()
      const workOrderId = saved?.workOrderId ?? draftWorkOrderId
      if (!workOrderId) {
        throw new Error('Save the draft before previewing it.')
      }
      return previewWorkOrderDraft(session!.accessToken, workOrderId)
    },
    onSuccess: (result) => {
      setServerError(null)
      setPreviewResult(result)
      setCurrentStepIndex(STEP_DEFINITIONS.length - 1)
      setHighestUnlockedStepIndex(STEP_DEFINITIONS.length - 1)
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to preview work order draft.')
    },
  })

  const validationQuery = useQuery({
    queryKey: ['maintainarr-work-order-create-validation', session?.accessToken, draftWorkOrderId, isReviewStep],
    queryFn: () => validateWorkOrderDraft(session!.accessToken, draftWorkOrderId),
    enabled: Boolean(session?.accessToken && draftWorkOrderId && isReviewStep),
    retry: false,
  })

  const duplicateQuery = useQuery({
    queryKey: ['maintainarr-work-order-create-duplicates', session?.accessToken, draftWorkOrderId, isReviewStep],
    queryFn: () => checkDuplicateWorkOrderDraft(session!.accessToken, draftWorkOrderId),
    enabled: Boolean(session?.accessToken && draftWorkOrderId && isReviewStep),
    retry: false,
  })

  const openMutation = useMutation({
    mutationFn: async () => {
      const saved = draftWorkOrderId
        ? null
        : await createDraftMutation.mutateAsync()
      const workOrderId = saved?.workOrderId ?? draftWorkOrderId
      if (!workOrderId) {
        throw new Error('Save the draft before opening it.')
      }
      return openWorkOrderDraft(session!.accessToken, workOrderId)
    },
    onSuccess: async (updated) => {
      setServerError(null)
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-work-orders'] })
      navigate(updated.downtimeFollowUp?.deepLinkPath ?? `/work-orders/details?workOrderId=${updated.workOrderId}`)
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to open work order.')
    },
  })

  const scheduleMutation = useMutation({
    mutationFn: async () => {
      const saved = draftWorkOrderId
        ? null
        : await createDraftMutation.mutateAsync()
      const workOrderId = saved?.workOrderId ?? draftWorkOrderId
      if (!workOrderId) {
        throw new Error('Save the draft before scheduling it.')
      }
      return scheduleWorkOrderDraft(session!.accessToken, workOrderId)
    },
    onSuccess: async (updated) => {
      setServerError(null)
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-work-orders'] })
      navigate(updated.downtimeFollowUp?.deepLinkPath ?? `/work-orders/details?workOrderId=${updated.workOrderId}`)
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to schedule work order.')
    },
  })

  const startMutation = useMutation({
    mutationFn: async () => {
      const saved = draftWorkOrderId
        ? null
        : await createDraftMutation.mutateAsync()
      const workOrderId = saved?.workOrderId ?? draftWorkOrderId
      if (!workOrderId) {
        throw new Error('Save the draft before starting it.')
      }
      return startWorkOrderDraft(session!.accessToken, workOrderId)
    },
    onSuccess: async (updated) => {
      setServerError(null)
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-work-orders'] })
      navigate(updated.downtimeFollowUp?.deepLinkPath ?? `/work-orders/details?workOrderId=${updated.workOrderId}`)
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to start work order.')
    },
  })

  if (!session) {
    return <Navigate to="/launch" replace />
  }

  const canCreate = meQuery.data
    ? canCreateWorkOrders((meQuery.data as MaintainArrMeResponse).tenantRoleKey, (meQuery.data as MaintainArrMeResponse).isPlatformAdmin)
    : false

  if (draftQuery.data && draftQuery.data.status.toLowerCase() !== 'draft') {
    return <Navigate to={`/work-orders/details?workOrderId=${draftQuery.data.workOrderId}`} replace />
  }

  const assetReadiness = assetReadinessQuery.data ?? null

  const handleValueChange = (fieldKey: string, value: unknown) => {
    if (!fieldset) {
      return
    }

    setPreviewResult(null)
    setServerError(null)
    setValues((current) => {
      let next = { ...current, [fieldKey]: value } as WorkOrderCreateValues
      const currentWorkOrderType = trimToEmpty(next.workOrderType)
      const nextDefectId = fieldKey === 'defectId' ? trimToEmpty(value) : trimToEmpty(next.defectId)
      const nextPmScheduleId = fieldKey === 'pmScheduleId' ? trimToEmpty(value) : trimToEmpty(next.pmScheduleId)

      if (fieldKey === 'defectId' && nextDefectId) {
        next = { ...next, pmScheduleId: '' }
        if (!currentWorkOrderType || currentWorkOrderType === 'corrective' || currentWorkOrderType === 'preventive') {
          next.workOrderType = 'defect_repair'
        }
      }
      if (fieldKey === 'pmScheduleId' && nextPmScheduleId) {
        next = { ...next, defectId: '' }
        if (!currentWorkOrderType || currentWorkOrderType === 'corrective' || currentWorkOrderType === 'defect_repair') {
          next.workOrderType = 'preventive'
        }
      }
      if (fieldKey === 'assetId' && !trimToEmpty(value)) {
        next = { ...next, defectId: '', pmScheduleId: '' }
        if (currentWorkOrderType === 'defect_repair' || currentWorkOrderType === 'preventive') {
          next.workOrderType = 'corrective'
        }
      }

      if (!nextDefectId && !nextPmScheduleId && (currentWorkOrderType === 'defect_repair' || currentWorkOrderType === 'preventive')) {
        next.workOrderType = 'corrective'
      }

      if (fieldset.fields.some((field) => field.key === fieldKey)) {
        next = clearInvalidDependentValues(fieldset, next, fieldKey)
      }

      return next
    })
  }

  const currentVisibleStep = STEP_DEFINITIONS[currentStepIndex]
  const currentStepErrorCount = Object.keys(currentErrors).length
  const canMoveForward = currentStepIndex < STEP_DEFINITIONS.length - 1 && currentStepComplete && !createDraftMutation.isPending
  const canMoveBackward = currentStepIndex > 0
  const canPersistDraft = basicsComplete && sourceReady && !createDraftMutation.isPending
  const canRunPreview = canPersistDraft && !previewMutation.isPending
  const previewFindings = previewResult?.findings ?? []
  const serverValidationFindings = validationQuery.data?.findings ?? []
  const duplicateMatches = previewResult?.duplicateMatches ?? duplicateQuery.data ?? []

  const fieldRenderer = (field: FieldMetadataResponse) => {
    const fieldId = `work-order-field-${field.key}`
    const error = currentErrors[field.key]
    const filteredOptions = getFilteredOptions(fieldset!, field, values)
    const pickerOptions = toPickerOptions(filteredOptions)
    const currentValue = values[field.key]

    const fieldShell = (content: React.ReactNode) => (
      <div key={field.key} className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <div className="flex items-start justify-between gap-3">
          <div>
            <label htmlFor={fieldId} className="block text-sm font-medium text-white">
              {field.label}
              {field.required ? ' *' : ''}
            </label>
            <p className="mt-1 text-xs text-slate-500">{field.description}</p>
          </div>
          <span className="rounded-full border border-slate-700 px-2 py-1 text-[10px] uppercase tracking-wide text-slate-500">
            {humanizeKey(field.sourceOfTruth)}
          </span>
        </div>
        <div className="mt-3">{content}</div>
        {error ? <p className="mt-2 text-xs text-rose-300">{error}</p> : null}
      </div>
    )

    if (field.control === 'asyncCombobox') {
      return fieldShell(
        <StaticSearchPicker
          id={fieldId}
          label={field.label}
          value={trimToEmpty(currentValue)}
          onChange={(nextValue) => handleValueChange(field.key, nextValue)}
          options={pickerOptions}
          placeholder={`Search ${field.label.toLowerCase()}...`}
          testId={fieldId}
          selectedOption={pickerOptions.find((option) => option.value === trimToEmpty(currentValue))}
        />,
      )
    }

    if (field.control === 'multiSelect') {
      return fieldShell(
        <select
          id={fieldId}
          multiple
          className="min-h-32 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
          value={Array.isArray(currentValue) ? currentValue.map(String) : []}
          onChange={(event) => {
            const next = Array.from(event.currentTarget.selectedOptions).map((option) => option.value)
            handleValueChange(field.key, next)
          }}
        >
          {pickerOptions.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>,
      )
    }

    if (field.control === 'select') {
      return fieldShell(
        <ControlledSelect
          label={field.label}
          value={trimToEmpty(currentValue)}
          onChange={(nextValue) => handleValueChange(field.key, nextValue)}
          options={pickerOptions}
          emptyLabel={`Select ${field.label.toLowerCase()}…`}
          testId={fieldId}
          className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
        />,
      )
    }

    const isNarrative = field.key.toLowerCase().includes('summary') || field.key.toLowerCase().includes('notes')
    return fieldShell(
      isNarrative ? (
        <textarea
          id={fieldId}
          className="min-h-28 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
          value={trimToEmpty(currentValue)}
          onChange={(event) => handleValueChange(field.key, event.target.value)}
        />
      ) : (
        <input
          id={fieldId}
          className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
          value={trimToEmpty(currentValue)}
          onChange={(event) => handleValueChange(field.key, event.target.value)}
        />
      ),
    )
  }

  const renderStepContent = () => {
    if (!fieldset) {
      return null
    }

    switch (currentVisibleStep.key) {
      case 'basics': {
        const workOrderTypeField = basicsFields.find((field) => field.key === 'workOrderType')
        const priorityField = basicsFields.find((field) => field.key === 'priority')
        return (
          <div className="grid gap-4 lg:grid-cols-2">
            <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
              <label htmlFor="work-order-asset" className="block text-sm font-medium text-white">
                Asset *
              </label>
              <p className="mt-1 text-xs text-slate-500">Choose the MaintainArr asset that this work order will live on.</p>
              <div className="mt-3">
                <StaticSearchPicker
                  id="work-order-asset"
                  label="Asset"
                  value={values.assetId}
                  onChange={(nextValue) => handleValueChange('assetId', nextValue)}
                  options={assetOptions}
                  placeholder="Search assets..."
                  testId="work-order-asset-picker"
                  selectedOption={selectedAssetOption ?? undefined}
                />
              </div>
              {currentErrors.assetId ? <p className="mt-2 text-xs text-rose-300">{currentErrors.assetId}</p> : null}
            </div>

            <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
              <label htmlFor="work-order-title" className="block text-sm font-medium text-white">
                Title *
              </label>
              <p className="mt-1 text-xs text-slate-500">A concise, operational title that will appear on the work order board.</p>
              <input
                id="work-order-title"
                className="mt-3 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={values.title}
                onChange={(event) => handleValueChange('title', event.target.value)}
              />
              {currentErrors.title ? <p className="mt-2 text-xs text-rose-300">{currentErrors.title}</p> : null}
            </div>

            <div className="lg:col-span-2 grid gap-4 md:grid-cols-2">
              {workOrderTypeField ? fieldRenderer(workOrderTypeField) : null}
              {priorityField ? fieldRenderer(priorityField) : null}
            </div>
          </div>
        )
      }
      case 'source':
        return (
          <div className="grid gap-4 lg:grid-cols-2">
            <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
              <label htmlFor="work-order-defect" className="block text-sm font-medium text-white">
                Defect
              </label>
              <p className="mt-1 text-xs text-slate-500">
                Link a defect when this work order is being created from a reported issue.
              </p>
              <div className="mt-3">
                <StaticSearchPicker
                  id="work-order-defect"
                  label="Defect"
                  value={values.defectId}
                  onChange={(nextValue) => handleValueChange('defectId', nextValue)}
                  options={defectOptions}
                  placeholder={values.assetId ? 'Select a defect...' : 'Select an asset first'}
                  testId="work-order-defect-picker"
                  selectedOption={selectedDefectOption ?? (selectedDefect ? { value: selectedDefect.defectId, label: `${selectedDefect.title} — ${selectedDefect.assetTag}` } : undefined)}
                />
              </div>
              {currentErrors.defectId ? <p className="mt-2 text-xs text-rose-300">{currentErrors.defectId}</p> : null}
            </div>

            <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
              <label htmlFor="work-order-pm-schedule" className="block text-sm font-medium text-white">
                PM schedule
              </label>
              <p className="mt-1 text-xs text-slate-500">
                Link the schedule that caused the work order to be generated, if applicable.
              </p>
              <div className="mt-3">
                <StaticSearchPicker
                  id="work-order-pm-schedule"
                  label="PM schedule"
                  value={values.pmScheduleId}
                  onChange={(nextValue) => handleValueChange('pmScheduleId', nextValue)}
                  options={pmScheduleOptions}
                  placeholder={values.assetId ? 'Select a schedule...' : 'Choose an asset first'}
                  testId="work-order-pm-schedule-picker"
                  selectedOption={selectedPmScheduleOption ?? undefined}
                />
              </div>
              {currentErrors.pmScheduleId ? <p className="mt-2 text-xs text-rose-300">{currentErrors.pmScheduleId}</p> : null}
            </div>
          </div>
        )
      case 'scope':
        return (
          <div className="grid gap-4">
            <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
              <label htmlFor="work-order-description" className="block text-sm font-medium text-white">
                Description
              </label>
              <p className="mt-1 text-xs text-slate-500">Describe the issue or the intended maintenance scope.</p>
              <textarea
                id="work-order-description"
                className="mt-3 min-h-32 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={values.description}
                onChange={(event) => handleValueChange('description', event.target.value)}
              />
            </div>
            {scopeFields.map((field) => fieldRenderer(field))}
          </div>
        )
      case 'classification':
        return <div className="grid gap-4 md:grid-cols-2">{classificationFields.map((field) => fieldRenderer(field))}</div>
      case 'assignment':
        return <div className="grid gap-4 md:grid-cols-2">{assignmentFields.map((field) => fieldRenderer(field))}</div>
      case 'readiness':
        return (
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
            <div className="flex items-center gap-2 text-white">
              <CheckCircle2 className="h-4 w-4 text-emerald-300" />
              <h3 className="text-sm font-semibold">Asset readiness</h3>
            </div>
            <p className="mt-2 text-sm text-slate-400">
              {selectedAsset
                ? `Readiness is checked against ${selectedAsset.assetTag} — ${selectedAsset.name}.`
                : 'Select an asset to see the readiness snapshot.'}
            </p>
            <div className="mt-4 rounded-lg border border-slate-800 bg-slate-900/60 p-4">
              {assetReadinessQuery.isFetching ? (
                <div className="flex items-center gap-2 text-sm text-slate-300">
                  <Loader2 className="h-4 w-4 animate-spin" />
                  Loading readiness snapshot...
                </div>
              ) : assetReadiness ? (
                <div className="grid gap-4 md:grid-cols-2">
                  <div>
                    <div className="text-xs uppercase tracking-wide text-slate-500">Status</div>
                    <div className="mt-1 text-lg font-semibold text-white">{assetReadiness.readinessStatus}</div>
                    <div className="text-sm text-slate-400">{assetReadiness.readinessBasis}</div>
                  </div>
                  <div>
                    <div className="text-xs uppercase tracking-wide text-slate-500">Blockers</div>
                    <div className="mt-1 text-lg font-semibold text-white">{assetReadiness.blockers.length}</div>
                    <div className="text-sm text-slate-400">
                      {assetReadiness.blockers.length === 0 ? 'No active blockers' : 'Readiness issues exist'}
                    </div>
                  </div>
                </div>
              ) : (
                <p className="text-sm text-slate-400">No readiness snapshot yet.</p>
              )}
            </div>
          </div>
        )
      case 'compliance':
        return <div className="grid gap-4 md:grid-cols-2">{complianceFields.map((field) => fieldRenderer(field))}</div>
      case 'scheduling':
        return (
          <div className="grid gap-4 md:grid-cols-2">
            <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
              <label htmlFor="work-order-planned-start" className="block text-sm font-medium text-white">
                Planned start
              </label>
              <input
                id="work-order-planned-start"
                type="datetime-local"
                className="mt-3 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={values.plannedStartAt}
                onChange={(event) => handleValueChange('plannedStartAt', event.target.value)}
              />
              {currentErrors.plannedStartAt ? <p className="mt-2 text-xs text-rose-300">{currentErrors.plannedStartAt}</p> : null}
            </div>
            <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
              <label htmlFor="work-order-planned-due" className="block text-sm font-medium text-white">
                Planned due
              </label>
              <input
                id="work-order-planned-due"
                type="datetime-local"
                className="mt-3 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={values.plannedDueAt}
                onChange={(event) => handleValueChange('plannedDueAt', event.target.value)}
              />
              {currentErrors.plannedDueAt ? <p className="mt-2 text-xs text-rose-300">{currentErrors.plannedDueAt}</p> : null}
            </div>
          </div>
        )
      case 'documents':
        return <div className="grid gap-4 md:grid-cols-2">{documentFields.map((field) => fieldRenderer(field))}</div>
      case 'review':
        return (
          <div className="space-y-6">
            <div className="grid gap-4 lg:grid-cols-3">
              <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
                <div className="text-xs uppercase tracking-wide text-slate-500">Asset</div>
                <div className="mt-1 text-lg font-semibold text-white">{selectedAsset ? `${selectedAsset.assetTag} — ${selectedAsset.name}` : 'Not selected'}</div>
                <div className="text-sm text-slate-400">{values.assetId || 'No asset chosen yet'}</div>
              </div>
              <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
                <div className="text-xs uppercase tracking-wide text-slate-500">Draft</div>
                <div className="mt-1 text-lg font-semibold text-white">{draftWorkOrderId || 'Not saved yet'}</div>
                <div className="text-sm text-slate-400">Save the draft to get a resumable work order ID.</div>
              </div>
              <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
                <div className="text-xs uppercase tracking-wide text-slate-500">Preview</div>
                <div className="mt-1 text-lg font-semibold text-white">{previewResult ? 'Complete' : 'Not run'}</div>
                <div className="text-sm text-slate-400">{previewResult ? 'Final actions are unlocked.' : 'Run preview before opening, scheduling, or starting.'}</div>
              </div>
            </div>

            <div className="grid gap-4 lg:grid-cols-2">
              <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
                <h3 className="text-sm font-semibold text-white">Validation findings</h3>
                {Object.keys(allErrors).length > 0 ? (
                  <ul className="mt-3 space-y-2 text-sm text-slate-300">
                    {Object.entries(allErrors).map(([key, message]) => (
                      <li key={key} className="rounded border border-rose-500/20 bg-rose-500/10 px-3 py-2 text-rose-100">
                        {message}
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p className="mt-2 text-sm text-emerald-300">No blocking validation issues remain in the current draft state.</p>
                )}
                {validationQuery.isFetching ? (
                  <div className="mt-3 flex items-center gap-2 text-xs text-slate-400">
                    <Loader2 className="h-3.5 w-3.5 animate-spin" />
                    Loading server validation...
                  </div>
                ) : null}
                {serverValidationFindings.length > 0 ? (
                  <div className="mt-4">
                    <h4 className="text-xs uppercase tracking-wide text-slate-500">Server findings</h4>
                    <ul className="mt-2 space-y-2 text-sm text-slate-300">
                      {serverValidationFindings.map((finding, index) => (
                        <li
                          key={`${finding.code}-${index}`}
                          className={`rounded border px-3 py-2 ${renderFindingBadge(finding.severity)}`}
                        >
                          <div className="font-medium">
                            {finding.category.toUpperCase()} · {finding.severity.toUpperCase()}
                          </div>
                          <div className="mt-1">{renderReadableFinding(finding)}</div>
                        </li>
                      ))}
                    </ul>
                  </div>
                ) : null}
              </div>

              <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
                <h3 className="text-sm font-semibold text-white">Readiness and compliance preview</h3>
                {previewResult ? (
                  <div className="mt-3 space-y-3">
                    <div className="text-sm text-slate-300">
                      <span className="font-medium text-white">Open:</span> {previewResult.canOpen ? 'Yes' : 'No'}
                      {' · '}
                      <span className="font-medium text-white">Schedule:</span> {previewResult.canSchedule ? 'Yes' : 'No'}
                      {' · '}
                      <span className="font-medium text-white">Start:</span> {previewResult.canStart ? 'Yes' : 'No'}
                    </div>
                    {previewResult.assetReadiness ? (
                      <div className="rounded border border-slate-800 bg-slate-900/70 px-3 py-2 text-sm text-slate-300">
                        Asset readiness: {previewResult.assetReadiness.readinessStatus} with {previewResult.assetReadiness.blockers.length} blocker(s).
                      </div>
                    ) : null}
                  </div>
                ) : (
                  <p className="mt-2 text-sm text-slate-400">Preview has not been run yet.</p>
                )}
                {duplicateQuery.isFetching ? (
                  <div className="mt-3 flex items-center gap-2 text-xs text-slate-400">
                    <Loader2 className="h-3.5 w-3.5 animate-spin" />
                    Checking for duplicates...
                  </div>
                ) : null}
              </div>
            </div>

            {previewFindings.length > 0 ? (
              <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
                <h3 className="text-sm font-semibold text-white">Preview findings</h3>
                <ul className="mt-3 space-y-2">
                  {previewFindings.map((finding, index) => (
                    <li key={`${finding.code}-${index}`} className={`rounded border px-3 py-2 text-sm ${renderFindingBadge(finding.severity)}`}>
                      <div className="font-medium">{finding.category.toUpperCase()} · {finding.severity.toUpperCase()}</div>
                      <div className="mt-1">{renderReadableFinding(finding)}</div>
                    </li>
                  ))}
                </ul>
              </div>
            ) : null}

            {duplicateMatches.length > 0 ? (
              <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
                <h3 className="text-sm font-semibold text-white">Potential duplicates</h3>
                <ul className="mt-3 space-y-2">
                  {duplicateMatches.map((match) => (
                    <li key={match.workOrderId} className="rounded border border-amber-500/20 bg-amber-500/10 px-3 py-2 text-sm text-amber-100">
                      <div className="font-medium">{match.workOrderNumber} · {match.title}</div>
                      <div className="mt-1 text-amber-50/80">
                        {match.matchReason} · Similarity {match.similarityScore}%
                      </div>
                    </li>
                  ))}
                </ul>
              </div>
            ) : null}
          </div>
        )
      default:
        return null
    }
  }

  const handleSaveDraft = async () => {
    await createDraftMutation.mutateAsync()
  }

  const handlePreview = async () => {
    await previewMutation.mutateAsync()
  }

  const handleFinalAction = async (action: 'open' | 'schedule' | 'start') => {
    switch (action) {
      case 'open':
        await openMutation.mutateAsync()
        break
      case 'schedule':
        await scheduleMutation.mutateAsync()
        break
      case 'start':
        await startMutation.mutateAsync()
        break
    }
  }

  if (meQuery.isLoading || fieldsetQuery.isLoading || assetsQuery.isLoading || pmSchedulesQuery.isLoading) {
    return (
      <div className="mx-auto max-w-7xl space-y-6 pb-24" data-testid="work-order-create-page">
        <PageHeader title="Create Work Order" subtitle="Guided MaintainArr work order setup" />
        <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-6">
          <div className="flex items-center gap-3 text-sm text-slate-300">
            <Loader2 className="h-4 w-4 animate-spin" />
            Loading work order create wizard...
          </div>
        </section>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-7xl space-y-6 pb-28" data-testid="work-order-create-page">
      <PageHeader
        title="Create Work Order"
        subtitle="Draft, preview, and open MaintainArr work orders from a dedicated guided flow."
      />

      <div className="flex flex-wrap items-center justify-between gap-3">
        <Link to="/work-orders/drawer" className="inline-flex items-center gap-2 text-sm text-slate-300 hover:text-white">
          <ArrowLeft className="h-4 w-4" />
          Back to work orders
        </Link>
        <div className="rounded-full border border-slate-800 bg-slate-950 px-3 py-1 text-xs text-slate-400">
          {draftWorkOrderId ? `Draft ${draftWorkOrderId}` : 'A draft ID will be created after the first save'}
        </div>
      </div>

      {serverError ? (
        <div className="rounded-lg border border-rose-500/30 bg-rose-500/10 p-4 text-sm text-rose-100">
          <div className="flex items-start gap-2">
            <AlertTriangle className="mt-0.5 h-4 w-4" />
            <div>{serverError}</div>
          </div>
        </div>
      ) : null}

      {!canCreate && meQuery.data ? (
        <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-6">
          <h2 className="text-lg font-semibold text-white">Work order creation unavailable</h2>
          <p className="mt-2 text-sm text-slate-400">Your role can view work orders, but cannot create or draft new ones.</p>
        </section>
      ) : null}

      {fieldset && canCreate ? (
        <>
          <nav className="grid gap-2 sm:grid-cols-2 xl:grid-cols-5" aria-label="Work order create steps">
            {STEP_DEFINITIONS.map((step, index) => {
              const isUnlocked = index <= highestUnlockedStepIndex
              const isActive = currentStep?.key === step.key
              const stepFields = step.key === 'basics'
                ? basicsFields
                : step.key === 'source'
                  ? []
                  : step.key === 'scope'
                    ? scopeFields
                    : step.key === 'classification'
                      ? classificationFields
                      : step.key === 'assignment'
                        ? assignmentFields
                        : step.key === 'compliance'
                          ? complianceFields
                          : step.key === 'documents'
                            ? documentFields
                            : []
              const completeCount = stepFields.length > 0 ? stepFields.filter((field) => isMeaningfulValue(values[field.key])).length : 0
              const stateLabel = step.key === 'review'
                ? previewResult
                  ? 'Complete'
                  : 'Needs review'
                : Object.keys(step.key === 'basics'
                    ? basicsErrors
                    : step.key === 'source'
                      ? sourceErrors
                      : step.key === 'scope'
                        ? scopeFieldErrors
                        : step.key === 'classification'
                          ? classificationFieldErrors
                          : step.key === 'assignment'
                            ? assignmentFieldErrors
                            : step.key === 'compliance'
                              ? complianceFieldErrors
                              : step.key === 'scheduling'
                                ? schedulingErrors
                                : step.key === 'documents'
                                  ? documentFieldErrors
                                  : {}).length > 0
                  ? 'Needs review'
                  : stepFields.length > 0 && completeCount === 0
                    ? 'Optional'
                    : 'Complete'

              return (
                <button
                  key={step.key}
                  type="button"
                  disabled={!isUnlocked}
                  onClick={() => setCurrentStepIndex(index)}
                  className={`rounded-xl border px-3 py-3 text-left text-sm transition ${
                    isActive
                      ? 'border-sky-400 bg-sky-500/10 text-white'
                      : 'border-slate-800 bg-slate-950/70 text-slate-300'
                  } disabled:cursor-not-allowed disabled:opacity-45`}
                >
                  <div className="flex items-center justify-between gap-2">
                    <span className="font-medium">{step.label}</span>
                    <span className="text-[10px] uppercase tracking-wide text-slate-500">{stateLabel}</span>
                  </div>
                  <div className="mt-1 text-xs text-slate-500">
                    {step.key === 'review'
                      ? previewResult
                        ? 'Preview complete'
                        : 'Run preview before final action'
                      : stepFields.length > 0
                        ? `${completeCount}/${stepFields.length} complete`
                        : step.description}
                  </div>
                </button>
              )
            })}
          </nav>

          <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-5">
            <div className="mb-5 flex flex-wrap items-start justify-between gap-3">
              <div>
                <h2 className="text-xl font-semibold text-white">{currentVisibleStep.label}</h2>
                <p className="mt-1 text-sm text-slate-400">{currentVisibleStep.description}</p>
              </div>
              {currentVisibleStep.key !== 'review' ? (
                <span className="rounded-full border border-slate-700 px-3 py-1 text-xs text-slate-400">
                  {currentStepErrorCount} blocking issue{currentStepErrorCount === 1 ? '' : 's'}
                </span>
              ) : null}
            </div>

            {renderStepContent()}
          </section>

          <div className="sticky bottom-0 z-10 rounded-xl border border-slate-800 bg-slate-950/95 p-4 shadow-2xl">
            <div className="flex flex-wrap items-center justify-between gap-3">
              <div className="text-sm text-slate-400">
                {currentVisibleStep.key === 'review' ? (
                  previewResult ? (
                    <span className="inline-flex items-center gap-2 text-emerald-300">
                      <CheckCircle2 className="h-4 w-4" />
                      Preview complete. Choose the final action.
                    </span>
                  ) : (
                    'Run preview to inspect blockers, duplicates, and readiness before opening the work order.'
                  )
                ) : (
                  'Complete the current section, then continue through the wizard.'
                )}
              </div>
              <div className="flex flex-wrap items-center gap-2">
                <button
                  type="button"
                  className="rounded-lg border border-slate-700 px-4 py-2 text-sm text-slate-200 disabled:opacity-45"
                  disabled={!canPersistDraft}
                  onClick={handleSaveDraft}
                >
                  {createDraftMutation.isPending ? 'Saving…' : 'Save draft'}
                </button>

                {currentVisibleStep.key !== 'review' ? (
                  <>
                    <button
                      type="button"
                      className="rounded-lg border border-slate-700 px-4 py-2 text-sm text-slate-200 disabled:opacity-45"
                      disabled={!canMoveBackward}
                      onClick={() => setCurrentStepIndex((current) => Math.max(current - 1, 0))}
                    >
                      Previous
                    </button>
                    <button
                      type="button"
                      className="rounded-lg bg-sky-700 px-4 py-2 text-sm font-medium text-white disabled:opacity-45"
                      disabled={!canMoveForward}
                      onClick={() => {
                        setHighestUnlockedStepIndex((current) => Math.max(current, currentStepIndex + 1))
                        setCurrentStepIndex((current) => Math.min(current + 1, STEP_DEFINITIONS.length - 1))
                      }}
                    >
                      Next
                    </button>
                  </>
                ) : (
                  <>
                    <button
                      type="button"
                      className="inline-flex items-center gap-2 rounded-lg border border-slate-700 px-4 py-2 text-sm text-slate-200 disabled:opacity-45"
                      disabled={!canRunPreview}
                      onClick={handlePreview}
                    >
                      <Sparkles className="h-4 w-4" />
                      {previewMutation.isPending ? 'Previewing…' : 'Preview'}
                    </button>
                    <button
                      type="button"
                      className="rounded-lg bg-emerald-700 px-4 py-2 text-sm font-medium text-white disabled:opacity-45"
                      disabled={!previewResult?.canOpen || openMutation.isPending}
                      onClick={() => handleFinalAction('open')}
                    >
                      Open work order
                    </button>
                    <button
                      type="button"
                      className="rounded-lg bg-amber-700 px-4 py-2 text-sm font-medium text-white disabled:opacity-45"
                      disabled={!previewResult?.canSchedule || scheduleMutation.isPending}
                      onClick={() => handleFinalAction('schedule')}
                    >
                      Schedule work order
                    </button>
                    <button
                      type="button"
                      className="rounded-lg bg-rose-700 px-4 py-2 text-sm font-medium text-white disabled:opacity-45"
                      disabled={!previewResult?.canStart || startMutation.isPending}
                      onClick={() => handleFinalAction('start')}
                    >
                      Start work order
                    </button>
                  </>
                )}
              </div>
            </div>
          </div>
        </>
      ) : null}

      {fieldsetQuery.isError ? (
        <p className="rounded-lg border border-rose-800 bg-rose-950/40 p-3 text-sm text-rose-200">
          Failed to load the work order create fieldset.
        </p>
      ) : null}
    </div>
  )
}
