import {
  useMemo,
  useState,
  type ComponentType,
  type FormEvent,
  type InputHTMLAttributes,
  type ReactNode,
} from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import {
  AlertTriangle,
  Bell,
  Calendar,
  Check,
  CheckCircle,
  ClipboardList,
  GraduationCap,
  HelpCircle,
  Info,
  Link as LinkIcon,
  Loader2,
  Package,
  Paperclip,
  Save,
  Send,
  ShieldCheck,
  Users,
  Wrench,
  X,
} from 'lucide-react'
import { ApiErrorCallout, StaticSearchPicker, getErrorMessage, type PickerOption } from '@stl/shared-ui'
import {
  createPersonnelIncident,
  getStaffArrFieldset,
  getMaintainArrAssetReferences,
  getMaintainArrWorkOrderReferences,
  getOrgUnits,
  getPeople,
  getRecordArrControlledDocumentReferences,
  getRoutArrRouteReferences,
  getSupplyArrSupplierReferences,
} from '../../api/client'
import type {
  CreatePersonnelIncidentRequest,
  OrgUnitResponse,
  PersonnelIncidentReadinessDecision,
  PersonnelIncidentSeverity,
  PersonnelIncidentSource,
  PersonnelIncidentStatus,
  PersonnelIncidentType,
  StaffArrFieldsetResponse,
  StaffPersonSummaryResponse,
} from '../../api/types'
import { loadSession } from '../../auth/sessionStorage'

type SubmitMode = 'draft' | 'submitted'

type SelectOption<T extends string = string> = {
  value: T
  label: string
  hint?: string
}

const readinessPresentation: Record<string, { accent: string; selected: string }> = {
  allowed: {
    accent: 'text-emerald-200 bg-emerald-500/15 ring-emerald-400/30',
    selected: 'border-emerald-400/70 bg-emerald-500/15',
  },
  watched: {
    accent: 'text-amber-200 bg-amber-500/15 ring-amber-400/30',
    selected: 'border-amber-400/70 bg-amber-500/15',
  },
  restricted: {
    accent: 'text-rose-200 bg-rose-500/15 ring-rose-400/30',
    selected: 'border-rose-400/70 bg-rose-500/15',
  },
}

const quickReferences: Array<[string, ComponentType<{ className?: string }>]> = [
  ['Incident classification guide', ClipboardList],
  ['Severity definitions', AlertTriangle],
  ['Readiness impact guide', ShieldCheck],
  ['Training impact guide', GraduationCap],
  ['Maintenance linkage', Wrench],
  ['Notification routing', Bell],
]

function todayDateValue() {
  return new Date().toISOString().slice(0, 10)
}

function currentTimeValue() {
  return new Date().toTimeString().slice(0, 5)
}

function combineLocalDateTime(date: string, time: string): string {
  return new Date(`${date}T${time || '00:00'}`).toISOString()
}

function optionalLocalDateTime(date: string, time: string): string | null {
  if (!date) return null
  return combineLocalDateTime(date, time || '00:00')
}

function classNames(...parts: Array<string | false | null | undefined>) {
  return parts.filter(Boolean).join(' ')
}

function labelFor<T extends string>(options: SelectOption<T>[], value: T | string | null | undefined) {
  return options.find((option) => option.value === value)?.label ?? value ?? 'None'
}

function humanizeKey(value: string): string {
  return value.replaceAll('_', ' ')
}

function withCurrentOption<T extends string, TOption extends SelectOption<T>>(
  options: TOption[],
  value: T | string,
  buildOption?: (value: T | string) => TOption,
): TOption[] {
  if (!value || options.some((option) => option.value === value)) {
    return options
  }

  return [
    buildOption?.(value) ?? ({ value: value as T, label: humanizeKey(value) } as TOption),
    ...options,
  ]
}

function fieldOptions<T extends string>(
  fieldset: StaffArrFieldsetResponse | undefined,
  fieldKey: string,
  leadingOptions: SelectOption<T>[] = [],
): SelectOption<T>[] {
  const options =
    fieldset?.fields.find((field) => field.key === fieldKey)?.options.map((option) => ({
      value: option.value as T,
      label: option.label,
      hint: option.hint ?? undefined,
    })) ?? []

  return [...leadingOptions, ...options]
}

function readinessFieldOptions(fieldset: StaffArrFieldsetResponse | undefined): Array<
  SelectOption<PersonnelIncidentReadinessDecision> & {
    accent: string
    selected: string
  }
> {
  return fieldOptions<PersonnelIncidentReadinessDecision>(fieldset, 'readinessDecision').map((option) => ({
    ...option,
    accent:
      readinessPresentation[option.value]?.accent ??
      'text-slate-200 bg-slate-500/15 ring-slate-400/30',
    selected: readinessPresentation[option.value]?.selected ?? 'border-slate-400/70 bg-slate-500/15',
  }))
}

function sortOrgUnits(units: OrgUnitResponse[]) {
  return [...units].sort((a, b) => {
    if (a.unitType !== b.unitType) return a.unitType.localeCompare(b.unitType)
    return a.name.localeCompare(b.name)
  })
}

function personLabel(person: StaffPersonSummaryResponse) {
  const role = person.jobTitle ? ` · ${person.jobTitle}` : ''
  return `${person.displayName} · ${person.primaryEmail}${role}`
}

function personSelectOptions(people: StaffPersonSummaryResponse[]) {
  return people
    .filter((person) => person.employmentStatus !== 'inactive')
    .sort((a, b) => a.displayName.localeCompare(b.displayName))
}

function buildPickerOptions<T>(
  items: T[],
  getValue: (item: T) => string,
  getLabel: (item: T) => string,
) {
  return items.map((item) => ({
    value: getValue(item),
    label: getLabel(item),
  })) satisfies PickerOption[]
}

function SectionHeader({
  number,
  title,
  subtitle,
}: {
  number: number
  title: string
  subtitle: string
}) {
  return (
    <div className="mb-4 flex flex-wrap items-center gap-3">
      <span className="grid h-7 w-7 place-items-center rounded-full bg-sky-500 text-sm font-semibold text-white">
        {number}
      </span>
      <div className="min-w-0">
        <h2 className="text-base font-semibold text-slate-100">{title}</h2>
        <p className="text-xs text-slate-400">{subtitle}</p>
      </div>
    </div>
  )
}

function FormSection({
  number,
  title,
  subtitle,
  children,
}: {
  number: number
  title: string
  subtitle: string
  children: ReactNode
}) {
  return (
    <section className="rounded-lg border border-slate-700/90 bg-slate-900/70 p-4 shadow-sm shadow-black/20">
      <SectionHeader number={number} title={title} subtitle={subtitle} />
      {children}
    </section>
  )
}

function Field({
  label,
  required,
  children,
  hint,
}: {
  label: string
  required?: boolean
  children: ReactNode
  hint?: string
}) {
  return (
    <label className="block min-w-0 text-xs font-medium text-slate-300">
      <span>
        {label}
        {required ? <span className="text-rose-300"> *</span> : null}
      </span>
      <div className="mt-1">{children}</div>
      {hint ? <span className="mt-1 block text-[11px] font-normal text-slate-500">{hint}</span> : null}
    </label>
  )
}

function inputClass(extra = '') {
  return classNames(
    'w-full rounded-md border border-slate-600 bg-slate-950/80 px-3 py-2 text-sm text-slate-100 outline-none transition placeholder:text-slate-500 focus:border-sky-400 focus:ring-2 focus:ring-sky-400/20 disabled:cursor-not-allowed disabled:opacity-60',
    extra,
  )
}

function TextInput(props: InputHTMLAttributes<HTMLInputElement>) {
  return <input {...props} className={inputClass(props.className)} />
}

function SelectField<T extends string>({
  value,
  onChange,
  options,
  disabled,
  required,
}: {
  value: T | string
  onChange: (value: T) => void
  options: SelectOption<T>[]
  disabled?: boolean
  required?: boolean
}) {
  return (
    <select
      value={value}
      required={required}
      disabled={disabled}
      onChange={(event) => onChange(event.target.value as T)}
      className={inputClass('appearance-auto')}
    >
      {options.map((option) => (
        <option key={option.value} value={option.value}>
          {option.label}
        </option>
      ))}
    </select>
  )
}

function PersonSelect({
  value,
  onChange,
  people,
  placeholder,
  testId,
}: {
  value: string
  onChange: (value: string) => void
  people: StaffPersonSummaryResponse[]
  placeholder: string
  testId: string
}) {
  const options = useMemo<PickerOption[]>(
    () =>
      personSelectOptions(people).map((person) => ({
        value: person.personId,
        label: personLabel(person),
      })),
    [people],
  )
  const selectedOption = useMemo(
    () => options.find((option) => option.value === value),
    [options, value],
  )

  return (
    <StaticSearchPicker
      value={value}
      onChange={onChange}
      options={options}
      selectedOption={selectedOption}
      placeholder={placeholder}
      testId={testId}
    />
  )
}

function OrgUnitPicker({
  value,
  onChange,
  orgUnits,
  placeholder,
  testId,
}: {
  value: string
  onChange: (value: string) => void
  orgUnits: OrgUnitResponse[]
  placeholder: string
  testId: string
}) {
  const options = useMemo<PickerOption[]>(
    () =>
      sortOrgUnits(orgUnits).map((unit) => ({
        value: unit.orgUnitId,
        label: `${unit.name} · ${unit.unitType}`,
      })),
    [orgUnits],
  )
  const selectedOption = useMemo(
    () => options.find((option) => option.value === value),
    [options, value],
  )

  return (
    <StaticSearchPicker
      value={value}
      onChange={onChange}
      options={options}
      selectedOption={selectedOption}
      placeholder={placeholder}
      testId={testId}
    />
  )
}

function MultiPersonSelect({
  value,
  onChange,
  people,
}: {
  value: string[]
  onChange: (value: string[]) => void
  people: StaffPersonSummaryResponse[]
}) {
  return (
    <select
      multiple
      value={value}
      onChange={(event) =>
        onChange(Array.from(event.currentTarget.selectedOptions).map((option) => option.value))
      }
      className={inputClass('min-h-24')}
    >
      {personSelectOptions(people).map((person) => (
        <option key={person.personId} value={person.personId}>
          {personLabel(person)}
        </option>
      ))}
    </select>
  )
}

function Toggle({
  checked,
  onChange,
  label,
  hint,
}: {
  checked: boolean
  onChange: (checked: boolean) => void
  label: string
  hint?: string
}) {
  return (
    <button
      type="button"
      role="switch"
      aria-checked={checked}
      onClick={() => onChange(!checked)}
      className="flex w-full items-start gap-3 rounded-md border border-slate-700 bg-slate-950/50 p-3 text-left transition hover:border-slate-500"
    >
      <span
        className={classNames(
          'mt-0.5 flex h-5 w-5 shrink-0 items-center justify-center rounded border',
          checked ? 'border-sky-400 bg-sky-500 text-white' : 'border-slate-600 bg-slate-900',
        )}
      >
        {checked ? <Check className="h-3.5 w-3.5" /> : null}
      </span>
      <span className="min-w-0">
        <span className="block text-sm font-medium text-slate-100">{label}</span>
        {hint ? <span className="mt-1 block text-xs text-slate-400">{hint}</span> : null}
      </span>
    </button>
  )
}

function SidePanel({
  icon: Icon,
  title,
  children,
}: {
  icon: ComponentType<{ className?: string }>
  title: string
  children: ReactNode
}) {
  return (
    <aside className="rounded-lg border border-slate-700 bg-slate-900/70 p-4">
      <div className="mb-3 flex items-center gap-2">
        <Icon className="h-4 w-4 text-sky-300" />
        <h3 className="text-sm font-semibold text-slate-100">{title}</h3>
      </div>
      {children}
    </aside>
  )
}

export function IncidentCreatePage() {
  const session = loadSession()
  const [submitMode, setSubmitMode] = useState<SubmitMode>('draft')
  const [createdIncidentId, setCreatedIncidentId] = useState<string | null>(null)

  const [title, setTitle] = useState('')
  const [incidentSource, setIncidentSource] = useState<PersonnelIncidentSource>('staffarr')
  const [incidentType, setIncidentType] = useState<PersonnelIncidentType>('safety')
  const [severity, setSeverity] = useState<PersonnelIncidentSeverity>('medium')
  const [incidentDate, setIncidentDate] = useState(todayDateValue)
  const [incidentTime, setIncidentTime] = useState(currentTimeValue)
  const [discoveryDate, setDiscoveryDate] = useState('')
  const [discoveryTime, setDiscoveryTime] = useState('')
  const [siteOrgUnitId, setSiteOrgUnitId] = useState('')
  const [departmentOrgUnitId, setDepartmentOrgUnitId] = useState('')
  const [locationDetail, setLocationDetail] = useState('')
  const [affectedPersonId, setAffectedPersonId] = useState('')
  const [reporterPersonId, setReporterPersonId] = useState(session?.personId ?? '')
  const [managerPersonId, setManagerPersonId] = useState('')
  const [witnessPersonIds, setWitnessPersonIds] = useState<string[]>([])
  const [additionalInvolvedPersonIds, setAdditionalInvolvedPersonIds] = useState<string[]>([])
  const [employeeSelfReport, setEmployeeSelfReport] = useState(false)
  const [description, setDescription] = useState('')
  const [immediateActionsTaken, setImmediateActionsTaken] = useState('')
  const [rootCause, setRootCause] = useState('')
  const [categoryKeys, setCategoryKeys] = useState<PersonnelIncidentType[]>(['safety'])
  const [readinessDecision, setReadinessDecision] =
    useState<PersonnelIncidentReadinessDecision>('allowed')
  const [workRestriction, setWorkRestriction] = useState('none')
  const [returnToWorkNeeded, setReturnToWorkNeeded] = useState('no')
  const [ppeConcern, setPpeConcern] = useState('none')
  const [medicalAttention, setMedicalAttention] = useState('none')
  const [outOfServiceRemoveFromDuty, setOutOfServiceRemoveFromDuty] = useState('no')
  const [followUpRequired, setFollowUpRequired] = useState('conditional')
  const [trainingReviewRequired, setTrainingReviewRequired] = useState(true)
  const [trainingReviewReason, setTrainingReviewReason] = useState('')
  const [relatedAssetReference, setRelatedAssetReference] = useState('')
  const [relatedWorkOrderReference, setRelatedWorkOrderReference] = useState('')
  const [relatedRouteReference, setRelatedRouteReference] = useState('')
  const [relatedSupplierReference, setRelatedSupplierReference] = useState('')
  const [relatedDocumentReference, setRelatedDocumentReference] = useState('')
  const [relatedPolicyReference, setRelatedPolicyReference] = useState('')
  const [evidencePackageRequested, setEvidencePackageRequested] = useState(true)
  const [notifyManager, setNotifyManager] = useState(true)
  const [notifySafetyCompliance, setNotifySafetyCompliance] = useState(true)
  const [notifyHr, setNotifyHr] = useState(false)
  const [createFollowUpTask, setCreateFollowUpTask] = useState(true)
  const [followUpDueDate, setFollowUpDueDate] = useState('')

  const peopleQuery = useQuery({
    queryKey: ['staffarr-incident-create-people', session?.accessToken],
    queryFn: () => getPeople(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const orgUnitsQuery = useQuery({
    queryKey: ['staffarr-incident-create-org-units', session?.accessToken],
    queryFn: () => getOrgUnits(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const incidentFieldsetQuery = useQuery({
    queryKey: ['staffarr-fieldset', session?.accessToken, 'personnel-incidents.create'],
    queryFn: () => getStaffArrFieldset(session!.accessToken, 'personnel-incidents/create'),
    enabled: Boolean(session?.accessToken),
  })

  const assetReferencesQuery = useQuery({
    queryKey: ['staffarr-incident-create-maintainarr-assets', session?.accessToken],
    queryFn: () => getMaintainArrAssetReferences(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const workOrderReferencesQuery = useQuery({
    queryKey: ['staffarr-incident-create-maintainarr-work-orders', session?.accessToken],
    queryFn: () => getMaintainArrWorkOrderReferences(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const routeReferencesQuery = useQuery({
    queryKey: ['staffarr-incident-create-routarr-routes', session?.accessToken],
    queryFn: () => getRoutArrRouteReferences(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const supplierReferencesQuery = useQuery({
    queryKey: ['staffarr-incident-create-supplyarr-suppliers', session?.accessToken],
    queryFn: () => getSupplyArrSupplierReferences(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const controlledDocumentReferencesQuery = useQuery({
    queryKey: ['staffarr-incident-create-recordarr-controlled-documents', session?.accessToken],
    queryFn: () => getRecordArrControlledDocumentReferences(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const incidentSourceOptions = withCurrentOption(
    fieldOptions<PersonnelIncidentSource>(incidentFieldsetQuery.data, 'incidentSource'),
    incidentSource,
  )
  const incidentTypeOptions = withCurrentOption(
    fieldOptions<PersonnelIncidentType>(incidentFieldsetQuery.data, 'incidentType'),
    incidentType,
  )
  const severityOptions = withCurrentOption(
    fieldOptions<PersonnelIncidentSeverity>(incidentFieldsetQuery.data, 'severity'),
    severity,
  )
  const readinessOptions = withCurrentOption(
    readinessFieldOptions(incidentFieldsetQuery.data),
    readinessDecision,
    (value) => ({
      value: value as PersonnelIncidentReadinessDecision,
      label: humanizeKey(value),
      hint: undefined,
      accent:
        readinessPresentation[value]?.accent ??
        'text-slate-200 bg-slate-500/15 ring-slate-400/30',
      selected: readinessPresentation[value]?.selected ?? 'border-slate-400/70 bg-slate-500/15',
    }),
  )
  const trainingReasonOptions = fieldOptions<string>(incidentFieldsetQuery.data, 'trainingReviewReason', [
    { value: '', label: 'Select reason' },
  ])
  const workRestrictionOptions = withCurrentOption(
    fieldOptions<string>(incidentFieldsetQuery.data, 'workRestriction'),
    workRestriction,
  )
  const yesNoPendingOptions = fieldOptions<string>(incidentFieldsetQuery.data, 'yesNoPending')
  const ppeConcernOptions = withCurrentOption(fieldOptions<string>(incidentFieldsetQuery.data, 'ppeConcern'), ppeConcern)
  const medicalAttentionOptions = withCurrentOption(
    fieldOptions<string>(incidentFieldsetQuery.data, 'medicalAttention'),
    medicalAttention,
  )
  const followUpOptions = withCurrentOption(
    fieldOptions<string>(incidentFieldsetQuery.data, 'followUpRequired'),
    followUpRequired,
  )
  const people = peopleQuery.data ?? []
  const orgUnits = orgUnitsQuery.data ?? []
  const assetReferences = assetReferencesQuery.data ?? []
  const workOrderReferences = workOrderReferencesQuery.data ?? []
  const routeReferences = routeReferencesQuery.data ?? []
  const supplierReferences = supplierReferencesQuery.data ?? []
  const controlledDocumentReferences = controlledDocumentReferencesQuery.data ?? []
  const affectedPerson = people.find((person) => person.personId === affectedPersonId) ?? null
  const managerPerson = people.find((person) => person.personId === managerPersonId) ?? null
  const sortedOrgUnits = useMemo(() => sortOrgUnits(orgUnits), [orgUnits])
  const assetReferenceOptions = useMemo(
    () =>
      buildPickerOptions(assetReferences, (asset) => asset.assetId, (asset) =>
        [asset.assetTag, asset.name, asset.lifecycleStatus].filter(Boolean).join(' · '),
      ),
    [assetReferences],
  )
  const selectedAssetReferenceOption = useMemo(
    () => assetReferenceOptions.find((option) => option.value === relatedAssetReference),
    [assetReferenceOptions, relatedAssetReference],
  )
  const workOrderReferenceOptions = useMemo(
    () =>
      buildPickerOptions(workOrderReferences, (workOrder) => workOrder.workOrderId, (workOrder) =>
        [workOrder.workOrderNumber, workOrder.title, workOrder.status].filter(Boolean).join(' · '),
      ),
    [workOrderReferences],
  )
  const selectedWorkOrderReferenceOption = useMemo(
    () => workOrderReferenceOptions.find((option) => option.value === relatedWorkOrderReference),
    [relatedWorkOrderReference, workOrderReferenceOptions],
  )
  const routeReferenceOptions = useMemo(
    () =>
      buildPickerOptions(routeReferences, (route) => route.routeId, (route) =>
        [route.routeNumber, route.title, route.routeStatus].filter(Boolean).join(' · '),
      ),
    [routeReferences],
  )
  const selectedRouteReferenceOption = useMemo(
    () => routeReferenceOptions.find((option) => option.value === relatedRouteReference),
    [relatedRouteReference, routeReferenceOptions],
  )
  const supplierReferenceOptions = useMemo(
    () =>
      buildPickerOptions(supplierReferences, (supplier) => supplier.partyId, (supplier) =>
        [supplier.partyKey, supplier.displayName, supplier.status].filter(Boolean).join(' · '),
      ),
    [supplierReferences],
  )
  const selectedSupplierReferenceOption = useMemo(
    () => supplierReferenceOptions.find((option) => option.value === relatedSupplierReference),
    [relatedSupplierReference, supplierReferenceOptions],
  )
  const controlledDocumentReferenceOptions = useMemo(
    () =>
      buildPickerOptions(
        controlledDocumentReferences,
        (document) => document.controlledDocumentId,
        (document) =>
          [
            document.documentNumber,
            document.title,
            document.controlledDocumentType,
            document.status,
          ]
            .filter(Boolean)
            .join(' · '),
      ),
    [controlledDocumentReferences],
  )
  const selectedDocumentReferenceOption = useMemo(
    () => controlledDocumentReferenceOptions.find((option) => option.value === relatedDocumentReference),
    [controlledDocumentReferenceOptions, relatedDocumentReference],
  )
  const selectedPolicyReferenceOption = useMemo(
    () => controlledDocumentReferenceOptions.find((option) => option.value === relatedPolicyReference),
    [controlledDocumentReferenceOptions, relatedPolicyReference],
  )

  const requiredComplete = [
    title.trim().length >= 4,
    affectedPersonId.length > 0,
    reporterPersonId.length > 0,
    managerPersonId.length > 0,
    incidentSource.length > 0,
    incidentType.length > 0,
    severity.length > 0,
    incidentDate.length > 0,
    incidentTime.length > 0,
    locationDetail.trim().length > 0,
    description.trim().length >= 16,
    immediateActionsTaken.trim().length > 0,
  ]
  const completeRequiredCount = requiredComplete.filter(Boolean).length

  const createMutation = useMutation({
    mutationFn: (payload: CreatePersonnelIncidentRequest) =>
      createPersonnelIncident(session!.accessToken, payload),
    onSuccess: (created) => {
      setCreatedIncidentId(created.incidentId)
    },
  })

  const selectedCategories = trainingReviewRequired
    ? Array.from(new Set<PersonnelIncidentType>([...categoryKeys, 'training_issue']))
    : categoryKeys

  const buildPayload = (status: PersonnelIncidentStatus): CreatePersonnelIncidentRequest => ({
    personId: affectedPersonId,
    reasonCategoryKey: trainingReviewRequired
      ? 'training_compliance'
      : incidentType === 'training_issue'
        ? 'training_compliance'
        : incidentType,
    severity,
    status,
    title: title.trim(),
    description: description.trim(),
    occurredAt: combineLocalDateTime(incidentDate, incidentTime),
    incidentSource,
    incidentType,
    discoveredAt: optionalLocalDateTime(discoveryDate, discoveryTime),
    siteOrgUnitId: siteOrgUnitId || null,
    departmentOrgUnitId: departmentOrgUnitId || null,
    locationDetail: locationDetail.trim() || null,
    reporterPersonId: reporterPersonId || null,
    managerPersonId: managerPersonId || null,
    witnessPersonIds,
    additionalInvolvedPersonIds,
    employeeSelfReport,
    immediateActionsTaken: immediateActionsTaken.trim() || null,
    rootCause: rootCause.trim() || null,
    categoryKeys: selectedCategories,
    readinessDecision,
    workRestriction,
    returnToWorkNeeded,
    ppeConcern,
    medicalAttention,
    outOfServiceRemoveFromDuty,
    followUpRequired,
    trainingReviewRequired,
    trainingReviewReason: trainingReviewReason || null,
    relatedAssetReference: relatedAssetReference.trim() || null,
    relatedWorkOrderReference: relatedWorkOrderReference.trim() || null,
    relatedRouteReference: relatedRouteReference.trim() || null,
    relatedSupplierReference: relatedSupplierReference.trim() || null,
    relatedDocumentReference: relatedDocumentReference.trim() || null,
    relatedPolicyReference: relatedPolicyReference.trim() || null,
    evidencePackageRequested,
    notifyManager,
    notifySafetyCompliance,
    notifyHr,
    createFollowUpTask,
    followUpDueAt: followUpDueDate ? combineLocalDateTime(followUpDueDate, '17:00') : null,
  })

  const submit = async (mode: SubmitMode) => {
    setSubmitMode(mode)
    await createMutation.mutateAsync(buildPayload(mode))
  }

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    await submit('submitted')
  }

  const toggleCategory = (value: PersonnelIncidentType) => {
    setCategoryKeys((current) => {
      if (current.includes(value)) {
        return current.filter((item) => item !== value)
      }
      return [...current, value]
    })
  }

  if (!session) {
    return (
      <div className="mx-auto max-w-3xl">
        <ApiErrorCallout
          title="StaffArr session required"
          message="Open StaffArr from NexArr again to create an incident."
        />
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-[1480px] space-y-5">
      <form onSubmit={handleSubmit} className="space-y-5">
        <header className="flex flex-wrap items-start justify-between gap-4">
          <div className="min-w-0">
            <div className="mb-3 flex flex-wrap items-center gap-2 text-xs text-slate-400">
              <Link
                to="/incidents"
                className="rounded-full border border-sky-400/40 bg-sky-500/10 px-3 py-1 font-medium text-sky-200 hover:border-sky-300"
              >
                StaffArr
              </Link>
              <span>/</span>
              <span>Create Incident</span>
            </div>
            <h1 className="text-3xl font-semibold tracking-normal text-slate-50">Create Incident</h1>
            <p className="mt-2 max-w-2xl text-sm leading-6 text-slate-300">
              Incidents are centrally captured in StaffArr and may drive readiness, permissions, or
              training follow-up.
            </p>
          </div>
          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              onClick={() => void submit('draft')}
              disabled={createMutation.isPending}
              className="inline-flex items-center gap-2 rounded-md border border-slate-600 bg-slate-900 px-4 py-2 text-sm font-medium text-slate-100 hover:border-slate-400 disabled:opacity-60"
            >
              {createMutation.isPending && submitMode === 'draft' ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                <Save className="h-4 w-4" />
              )}
              Save Draft
            </button>
            <button
              type="submit"
              disabled={createMutation.isPending}
              className="inline-flex items-center gap-2 rounded-md bg-sky-600 px-4 py-2 text-sm font-semibold text-white shadow-sm shadow-sky-950/40 hover:bg-sky-500 disabled:opacity-60"
            >
              {createMutation.isPending && submitMode === 'submitted' ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                <Send className="h-4 w-4" />
              )}
              Submit Incident
            </button>
            <Link
              to="/incidents"
              className="inline-flex items-center gap-2 rounded-md border border-slate-600 bg-slate-900 px-4 py-2 text-sm font-medium text-slate-100 hover:border-slate-400"
            >
              <X className="h-4 w-4" />
              Cancel
            </Link>
          </div>
        </header>

        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          <div className="rounded-lg border border-slate-700 bg-slate-900/70 p-4">
            <div className="flex items-start gap-3">
              <ClipboardList className="h-8 w-8 rounded-full bg-sky-500/15 p-1.5 text-sky-200" />
              <div>
                <p className="text-xs font-medium uppercase text-slate-400">Required fields</p>
                <p className="mt-1 text-xl font-semibold text-slate-50">
                  {completeRequiredCount} of {requiredComplete.length}
                </p>
                <p className="mt-1 text-xs text-slate-400">Complete required fields to submit</p>
              </div>
            </div>
          </div>
          <div className="rounded-lg border border-slate-700 bg-slate-900/70 p-4">
            <div className="flex items-start gap-3">
              <ShieldCheck className="h-8 w-8 rounded-full bg-emerald-500/15 p-1.5 text-emerald-200" />
              <div>
                <p className="text-xs font-medium uppercase text-slate-400">Readiness impact</p>
                <p className="mt-1 text-xl font-semibold text-slate-50">
                  {labelFor(readinessOptions, readinessDecision)}
                </p>
                <p className="mt-1 text-xs text-slate-400">Used for readiness and restrictions</p>
              </div>
            </div>
          </div>
          <div className="rounded-lg border border-slate-700 bg-slate-900/70 p-4">
            <div className="flex items-start gap-3">
              <GraduationCap className="h-8 w-8 rounded-full bg-violet-500/15 p-1.5 text-violet-200" />
              <div>
                <p className="text-xs font-medium uppercase text-slate-400">Training review</p>
                <p className="mt-1 text-xl font-semibold text-slate-50">
                  {trainingReviewRequired ? 'Enabled' : 'Off'}
                </p>
                <p className="mt-1 text-xs text-slate-400">Can route for TrainArr remediation</p>
              </div>
            </div>
          </div>
          <div className="rounded-lg border border-slate-700 bg-slate-900/70 p-4">
            <div className="flex items-start gap-3">
              <Users className="h-8 w-8 rounded-full bg-amber-500/15 p-1.5 text-amber-200" />
              <div>
                <p className="text-xs font-medium uppercase text-slate-400">Escalation route</p>
                <p className="mt-1 text-xl font-semibold text-slate-50">
                  {managerPerson?.displayName ?? 'Manager + Safety'}
                </p>
                <p className="mt-1 text-xs text-slate-400">Notifications use selected reviewers</p>
              </div>
            </div>
          </div>
        </div>

        {createdIncidentId ? (
          <div className="rounded-lg border border-emerald-500/40 bg-emerald-500/10 p-4 text-sm text-emerald-100">
            <div className="flex flex-wrap items-center justify-between gap-3">
              <span>
                Incident {submitMode === 'draft' ? 'draft saved' : 'submitted'}:
                <span className="ml-1 font-mono">{createdIncidentId.slice(0, 8)}</span>
              </span>
              <Link to="/incidents" className="font-medium text-emerald-50 underline">
                Back to incidents
              </Link>
            </div>
          </div>
        ) : null}

        {createMutation.isError ? (
          <ApiErrorCallout
            title="Incident create failed"
            message={getErrorMessage(createMutation.error, 'Failed to create incident.')}
          />
        ) : null}

        <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_320px]">
          <main className="space-y-4">
            <FormSection
              number={1}
              title="Incident Basics"
              subtitle="Controlled fields support accurate workflow evaluation."
            >
              <div className="grid gap-3 lg:grid-cols-4">
                <Field label="Incident title" required>
                  <TextInput
                    value={title}
                    onChange={(event) => setTitle(event.target.value)}
                    required
                    minLength={4}
                    placeholder="Enter concise incident title"
                  />
                </Field>
                <Field label="Incident source" required>
                  <SelectField
                    value={incidentSource}
                    onChange={setIncidentSource}
                    options={incidentSourceOptions}
                    required
                  />
                </Field>
                <Field label="Incident type" required>
                  <SelectField
                    value={incidentType}
                    onChange={(value) => {
                      setIncidentType(value)
                      setCategoryKeys((current) =>
                        current.includes(value) ? current : [...current, value],
                      )
                    }}
                    options={incidentTypeOptions}
                    required
                  />
                </Field>
                <Field label="Severity" required>
                  <SelectField value={severity} onChange={setSeverity} options={severityOptions} required />
                </Field>
                <Field label="Status">
                  <TextInput value="Draft" disabled />
                </Field>
                <Field label="Incident date" required>
                  <TextInput
                    type="date"
                    value={incidentDate}
                    onChange={(event) => setIncidentDate(event.target.value)}
                    required
                  />
                </Field>
                <Field label="Incident time" required hint="Local time">
                  <TextInput
                    type="time"
                    value={incidentTime}
                    onChange={(event) => setIncidentTime(event.target.value)}
                    required
                  />
                </Field>
                <Field label="Discovery date">
                  <TextInput
                    type="date"
                    value={discoveryDate}
                    onChange={(event) => setDiscoveryDate(event.target.value)}
                  />
                </Field>
                <Field label="Discovery time" hint="Local time">
                  <TextInput
                    type="time"
                    value={discoveryTime}
                    onChange={(event) => setDiscoveryTime(event.target.value)}
                  />
                </Field>
                <Field label="Site">
                  <OrgUnitPicker
                    value={siteOrgUnitId}
                    onChange={setSiteOrgUnitId}
                    orgUnits={sortedOrgUnits}
                    placeholder="Search site"
                    testId="incident-site-picker"
                  />
                </Field>
                <Field label="Department">
                  <OrgUnitPicker
                    value={departmentOrgUnitId}
                    onChange={setDepartmentOrgUnitId}
                    orgUnits={sortedOrgUnits}
                    placeholder="Search department"
                    testId="incident-department-picker"
                  />
                </Field>
                <Field label="Location detail" required>
                  <TextInput
                    value={locationDetail}
                    onChange={(event) => setLocationDetail(event.target.value)}
                    required
                    placeholder="Enter specific location details"
                  />
                </Field>
              </div>
              <p className="mt-3 flex items-start gap-2 text-xs text-slate-400">
                <Info className="mt-0.5 h-3.5 w-3.5 shrink-0 text-sky-300" />
                Fields marked with * are required. Many fields are controlled lists to preserve reporting
                consistency.
              </p>
            </FormSection>

            <FormSection
              number={2}
              title="People & Involvement"
              subtitle="StaffArr remains the people source of truth."
            >
              <div className="grid gap-3 lg:grid-cols-3">
                <Field label="Affected person" required>
                  <PersonSelect
                    value={affectedPersonId}
                    onChange={(value) => {
                      setAffectedPersonId(value)
                      const selected = people.find((person) => person.personId === value)
                      if (selected?.managerPersonId && !managerPersonId) {
                        setManagerPersonId(selected.managerPersonId)
                      }
                    }}
                    people={people}
                    placeholder={peopleQuery.isLoading ? 'Loading people...' : 'Search by person or name'}
                    testId="incident-affected-person-picker"
                  />
                </Field>
                <Field label="Reporter" required>
                  <PersonSelect
                    value={reporterPersonId}
                    onChange={setReporterPersonId}
                    people={people}
                    placeholder="Search reporter"
                    testId="incident-reporter-picker"
                  />
                </Field>
                <Field label="Manager / supervisor" required>
                  <PersonSelect
                    value={managerPersonId}
                    onChange={setManagerPersonId}
                    people={people}
                    placeholder="Search manager"
                    testId="incident-manager-picker"
                  />
                </Field>
                <Field label="Witnesses" hint="Use Ctrl or Shift to select more than one">
                  <MultiPersonSelect
                    value={witnessPersonIds}
                    onChange={setWitnessPersonIds}
                    people={people}
                  />
                </Field>
                <Field label="Additional involved persons" hint="Use Ctrl or Shift to select more than one">
                  <MultiPersonSelect
                    value={additionalInvolvedPersonIds}
                    onChange={setAdditionalInvolvedPersonIds}
                    people={people}
                  />
                </Field>
                <div className="self-end">
                  <Toggle
                    checked={employeeSelfReport}
                    onChange={setEmployeeSelfReport}
                    label="Employee self-report"
                    hint="Affected person is also the reporter"
                  />
                </div>
              </div>
              {affectedPerson ? (
                <p className="mt-3 text-xs text-slate-400">
                  Selected: <span className="text-slate-200">{affectedPerson.displayName}</span>
                  {affectedPerson.primaryOrgUnitName ? ` · ${affectedPerson.primaryOrgUnitName}` : ''}
                </p>
              ) : null}
            </FormSection>

            <FormSection
              number={3}
              title="Narrative & Details"
              subtitle="Provide a clear factual account using controlled categories."
            >
              <div className="grid gap-4 xl:grid-cols-[minmax(0,1.1fr)_minmax(280px,0.9fr)]">
                <div className="space-y-3">
                  <Field label="What happened?" required>
                    <textarea
                      value={description}
                      onChange={(event) => setDescription(event.target.value)}
                      required
                      minLength={16}
                      maxLength={4000}
                      rows={5}
                      placeholder="Describe what happened, including sequence of events, conditions, and environment."
                      className={inputClass('resize-y')}
                    />
                    <span className="mt-1 block text-right text-[11px] text-slate-500">
                      {description.length} / 4000
                    </span>
                  </Field>
                  <Field label="Immediate actions taken" required>
                    <textarea
                      value={immediateActionsTaken}
                      onChange={(event) => setImmediateActionsTaken(event.target.value)}
                      required
                      maxLength={2000}
                      rows={3}
                      placeholder="Describe immediate actions taken to protect people and secure the area."
                      className={inputClass('resize-y')}
                    />
                  </Field>
                  <Field label="Root cause / preliminary cause">
                    <textarea
                      value={rootCause}
                      onChange={(event) => setRootCause(event.target.value)}
                      maxLength={2000}
                      rows={3}
                      placeholder="Provide preliminary root cause or contributing factors if known."
                      className={inputClass('resize-y')}
                    />
                  </Field>
                </div>
                <div>
                  <p className="mb-2 text-xs font-medium text-slate-300">
                    Categories <span className="text-rose-300">*</span>
                  </p>
                  <div className="grid gap-2 sm:grid-cols-2">
                    {incidentTypeOptions
                      .filter((option) => option.value !== 'other')
                      .map((option) => {
                        const selected = selectedCategories.includes(option.value)
                        return (
                          <button
                            key={option.value}
                            type="button"
                            onClick={() => toggleCategory(option.value)}
                            className={classNames(
                              'flex min-h-14 items-center gap-2 rounded-md border px-3 py-2 text-left text-sm transition',
                              selected
                                ? 'border-sky-400/70 bg-sky-500/15 text-sky-100'
                                : 'border-slate-700 bg-slate-950/50 text-slate-300 hover:border-slate-500',
                            )}
                          >
                            <span
                              className={classNames(
                                'grid h-6 w-6 shrink-0 place-items-center rounded-full',
                                selected ? 'bg-sky-400/20 text-sky-100' : 'bg-slate-800 text-slate-400',
                              )}
                            >
                              <Check className="h-3.5 w-3.5" />
                            </span>
                            {option.label}
                          </button>
                        )
                      })}
                  </div>
                </div>
              </div>
            </FormSection>

            <FormSection
              number={4}
              title="Readiness / Restriction Impact"
              subtitle="Evaluate work readiness and restriction needs."
            >
              <div className="grid gap-4 xl:grid-cols-[minmax(280px,0.9fr)_minmax(0,1.1fr)]">
                <div>
                  <p className="mb-2 text-xs font-medium text-slate-300">
                    Overall readiness decision <span className="text-rose-300">*</span>
                  </p>
                  <div className="grid gap-2 sm:grid-cols-3 xl:grid-cols-1">
                    {readinessOptions.map((option) => {
                      const selected = readinessDecision === option.value
                      return (
                        <button
                          key={option.value}
                          type="button"
                          onClick={() => setReadinessDecision(option.value)}
                          className={classNames(
                            'rounded-md border p-3 text-left transition',
                            selected
                              ? option.selected
                              : 'border-slate-700 bg-slate-950/50 hover:border-slate-500',
                          )}
                        >
                          <span
                            className={classNames(
                              'mb-2 grid h-8 w-8 place-items-center rounded-full ring-1',
                              option.accent,
                            )}
                          >
                            <ShieldCheck className="h-4 w-4" />
                          </span>
                          <span className="block text-sm font-semibold text-slate-100">{option.label}</span>
                          <span className="mt-1 block text-xs text-slate-400">{option.hint}</span>
                        </button>
                      )
                    })}
                  </div>
                </div>
                <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
                  <Field label="Work restriction" required>
                    <SelectField
                      value={workRestriction}
                      onChange={setWorkRestriction}
                      options={workRestrictionOptions}
                      required
                    />
                  </Field>
                  <Field label="Return-to-work needed">
                    <SelectField
                      value={returnToWorkNeeded}
                      onChange={setReturnToWorkNeeded}
                      options={yesNoPendingOptions}
                    />
                  </Field>
                  <Field label="PPE concern">
                    <SelectField value={ppeConcern} onChange={setPpeConcern} options={ppeConcernOptions} />
                  </Field>
                  <Field label="Medical attention">
                    <SelectField
                      value={medicalAttention}
                      onChange={setMedicalAttention}
                      options={medicalAttentionOptions}
                    />
                  </Field>
                  <Field label="Out-of-service / remove from duty">
                    <SelectField
                      value={outOfServiceRemoveFromDuty}
                      onChange={setOutOfServiceRemoveFromDuty}
                      options={yesNoPendingOptions}
                    />
                  </Field>
                  <Field label="Follow-up required" required>
                    <SelectField
                      value={followUpRequired}
                      onChange={setFollowUpRequired}
                      options={followUpOptions}
                      required
                    />
                  </Field>
                </div>
              </div>
            </FormSection>

            <FormSection
              number={5}
              title="Training / Certification Evaluation"
              subtitle="Evaluate training impact and possible TrainArr routing."
            >
              <div className="grid gap-4 lg:grid-cols-[minmax(260px,0.8fr)_minmax(0,1fr)_minmax(260px,0.8fr)]">
                <Toggle
                  checked={trainingReviewRequired}
                  onChange={setTrainingReviewRequired}
                  label="May require TrainArr review"
                  hint="Enable when this incident may affect training or certification."
                />
                <Field label="Reason">
                  <SelectField
                    value={trainingReviewReason}
                    onChange={setTrainingReviewReason}
                    options={trainingReasonOptions}
                  />
                </Field>
                <div className="rounded-md border border-sky-500/40 bg-sky-500/10 p-3 text-xs text-sky-100">
                  <div className="mb-2 flex items-center gap-2 font-semibold">
                    <GraduationCap className="h-4 w-4" />
                    TrainArr handoff
                  </div>
                  {trainingReviewRequired
                    ? 'Submitted incidents are tagged for TrainArr remediation review.'
                    : 'Training handoff is off for this incident.'}
                </div>
              </div>
            </FormSection>

            <FormSection
              number={6}
              title="Related Records & Cross-Product Links"
              subtitle="Select existing records to provide context and avoid duplication."
            >
              <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
                <Field label="Related asset">
                  <StaticSearchPicker
                    value={relatedAssetReference}
                    onChange={setRelatedAssetReference}
                    options={assetReferenceOptions}
                    selectedOption={selectedAssetReferenceOption}
                    placeholder="Search asset"
                    testId="incident-asset-reference-picker"
                  />
                </Field>
                <Field label="Related work order">
                  <StaticSearchPicker
                    value={relatedWorkOrderReference}
                    onChange={setRelatedWorkOrderReference}
                    options={workOrderReferenceOptions}
                    selectedOption={selectedWorkOrderReferenceOption}
                    placeholder="Search work order"
                    testId="incident-work-order-reference-picker"
                  />
                </Field>
                <Field label="Related route / trip">
                  <StaticSearchPicker
                    value={relatedRouteReference}
                    onChange={setRelatedRouteReference}
                    options={routeReferenceOptions}
                    selectedOption={selectedRouteReferenceOption}
                    placeholder="Search route or trip"
                    testId="incident-route-reference-picker"
                  />
                </Field>
                <Field label="Related supplier / party">
                  <StaticSearchPicker
                    value={relatedSupplierReference}
                    onChange={setRelatedSupplierReference}
                    options={supplierReferenceOptions}
                    selectedOption={selectedSupplierReferenceOption}
                    placeholder="Search supplier or party"
                    testId="incident-supplier-reference-picker"
                  />
                </Field>
                <Field label="Related documents / evidence package">
                  <StaticSearchPicker
                    value={relatedDocumentReference}
                    onChange={setRelatedDocumentReference}
                    options={controlledDocumentReferenceOptions}
                    selectedOption={selectedDocumentReferenceOption}
                    placeholder="Search documents"
                    testId="incident-document-reference-picker"
                  />
                </Field>
                <Field label="Related policy / acknowledgement">
                  <StaticSearchPicker
                    value={relatedPolicyReference}
                    onChange={setRelatedPolicyReference}
                    options={controlledDocumentReferenceOptions}
                    selectedOption={selectedPolicyReferenceOption}
                    placeholder="Search policy or acknowledgement"
                    testId="incident-policy-reference-picker"
                  />
                </Field>
              </div>
              <p className="mt-3 flex items-start gap-2 text-xs text-slate-400">
                <LinkIcon className="mt-0.5 h-3.5 w-3.5 shrink-0 text-sky-300" />
                Linked references are selected from MaintainArr, RoutArr, SupplyArr, and RecordArr source lists
                for cross-product traceability.
              </p>
            </FormSection>

            <FormSection
              number={7}
              title="Evidence & Attachments"
              subtitle="Upload photos, PDFs, statements, and forms to support the incident."
            >
              <div className="grid gap-4 lg:grid-cols-[minmax(280px,0.9fr)_minmax(0,1.1fr)]">
                <div className="rounded-lg border border-dashed border-slate-600 bg-slate-950/40 p-6 text-center">
                  <Paperclip className="mx-auto h-10 w-10 text-sky-300" />
                  <p className="mt-3 text-sm text-slate-200">Drag and drop files here</p>
                  <p className="text-xs text-slate-500">or</p>
                  <button
                    type="button"
                    className="mt-2 rounded-md border border-sky-400/50 bg-sky-500/10 px-3 py-1.5 text-sm font-medium text-sky-100"
                  >
                    Browse files
                  </button>
                  <p className="mt-3 text-xs text-slate-500">Accepted: jpg, png, pdf, docx, xlsx, txt</p>
                </div>
                <div className="space-y-2">
                  <div className="rounded-md border border-slate-700 bg-slate-950/50 px-3 py-4 text-sm text-slate-400">
                    No files attached yet.
                  </div>
                  <Toggle
                    checked={evidencePackageRequested}
                    onChange={setEvidencePackageRequested}
                    label="Generate audit package entry"
                    hint="Create an audit package entry when this incident is submitted."
                  />
                </div>
              </div>
            </FormSection>

            <FormSection
              number={8}
              title="Notifications & Workflow"
              subtitle="Configure notifications and workflow actions."
            >
              <div className="grid gap-4 lg:grid-cols-[minmax(260px,0.8fr)_minmax(0,1.2fr)]">
                <div className="space-y-2">
                  <Toggle checked={notifyManager} onChange={setNotifyManager} label="Notify manager" />
                  <Toggle
                    checked={notifySafetyCompliance}
                    onChange={setNotifySafetyCompliance}
                    label="Notify safety / compliance"
                  />
                  <Toggle checked={notifyHr} onChange={setNotifyHr} label="Notify HR" />
                </div>
                <div className="grid gap-3 md:grid-cols-2">
                  <Toggle
                    checked={createFollowUpTask}
                    onChange={setCreateFollowUpTask}
                    label="Create follow-up task"
                  />
                  <Field label="Due date for follow-up">
                    <div className="relative">
                      <Calendar className="pointer-events-none absolute left-3 top-2.5 h-4 w-4 text-slate-500" />
                      <TextInput
                        type="date"
                        value={followUpDueDate}
                        onChange={(event) => setFollowUpDueDate(event.target.value)}
                        className="pl-9"
                      />
                    </div>
                  </Field>
                  <div className="rounded-md border border-slate-700 bg-slate-950/50 p-3 md:col-span-2">
                    <p className="text-xs font-medium uppercase text-slate-400">Approval / review route</p>
                    <div className="mt-3 flex flex-wrap items-center gap-2 text-sm text-slate-200">
                      <span className="rounded-full bg-emerald-500/15 px-3 py-1 text-emerald-100 ring-1 ring-emerald-400/30">
                        Manager
                      </span>
                      <span className="text-slate-500">to</span>
                      <span className="rounded-full bg-amber-500/15 px-3 py-1 text-amber-100 ring-1 ring-amber-400/30">
                        Safety / Compliance
                      </span>
                      {notifyHr ? (
                        <>
                          <span className="text-slate-500">to</span>
                          <span className="rounded-full bg-violet-500/15 px-3 py-1 text-violet-100 ring-1 ring-violet-400/30">
                            HR
                          </span>
                        </>
                      ) : null}
                    </div>
                  </div>
                </div>
              </div>
            </FormSection>

            <FormSection
              number={9}
              title="Review & Submit"
              subtitle="Validate information and submit when complete."
            >
              <div className="grid gap-4 lg:grid-cols-3">
                <div>
                  <p className="mb-2 text-xs font-medium uppercase text-slate-400">
                    Completeness checklist
                  </p>
                  <div className="grid gap-1 text-sm">
                    {[
                      ['Incident basics', title.trim().length >= 4 && incidentDate && incidentTime],
                      ['People & involvement', affectedPersonId && reporterPersonId && managerPersonId],
                      ['Narrative & details', description.trim().length >= 16],
                      ['Readiness impact', readinessDecision],
                      ['Training evaluation', !trainingReviewRequired || trainingReviewReason],
                      ['Related records', true],
                      ['Evidence & attachments', true],
                      ['Notifications & workflow', notifyManager || notifySafetyCompliance || notifyHr],
                    ].map(([label, complete]) => (
                      <div key={String(label)} className="flex items-center gap-2 text-slate-300">
                        <CheckCircle
                          className={classNames(
                            'h-4 w-4',
                            complete ? 'text-emerald-300' : 'text-slate-600',
                          )}
                        />
                        <span>{label}</span>
                      </div>
                    ))}
                  </div>
                </div>
                <div className="rounded-md border border-slate-700 bg-slate-950/50 p-3">
                  <p className="mb-2 text-xs font-medium uppercase text-slate-400">Warnings & notices</p>
                  <div className="space-y-2 text-xs text-slate-300">
                    {severity === 'high' || severity === 'critical' ? (
                      <p className="flex items-start gap-2">
                        <AlertTriangle className="mt-0.5 h-3.5 w-3.5 shrink-0 text-amber-300" />
                        Severity selection requires manager review.
                      </p>
                    ) : null}
                    {trainingReviewRequired ? (
                      <p className="flex items-start gap-2">
                        <AlertTriangle className="mt-0.5 h-3.5 w-3.5 shrink-0 text-amber-300" />
                        TrainArr evaluation may be required.
                      </p>
                    ) : null}
                    <p className="flex items-start gap-2">
                      <Info className="mt-0.5 h-3.5 w-3.5 shrink-0 text-sky-300" />
                      Follow-up, evidence, and readiness data are submitted with this record.
                    </p>
                  </div>
                </div>
                <div className="space-y-2">
                  <button
                    type="button"
                    onClick={() => void submit('draft')}
                    disabled={createMutation.isPending}
                    className="inline-flex w-full items-center justify-center gap-2 rounded-md border border-slate-600 bg-slate-900 px-4 py-2 text-sm font-medium text-slate-100 hover:border-slate-400 disabled:opacity-60"
                  >
                    <Save className="h-4 w-4" />
                    Save Draft
                  </button>
                  <button
                    type="submit"
                    disabled={createMutation.isPending}
                    className="inline-flex w-full items-center justify-center gap-2 rounded-md bg-sky-600 px-4 py-2 text-sm font-semibold text-white hover:bg-sky-500 disabled:opacity-60"
                  >
                    <Send className="h-4 w-4" />
                    Submit Incident
                  </button>
                  <Link
                    to="/incidents"
                    className="inline-flex w-full items-center justify-center gap-2 rounded-md border border-slate-600 bg-slate-950 px-4 py-2 text-sm font-medium text-slate-100 hover:border-slate-400"
                  >
                    <X className="h-4 w-4" />
                    Cancel
                  </Link>
                </div>
              </div>
            </FormSection>
          </main>

          <div className="space-y-4 xl:sticky xl:top-4 xl:self-start">
            <SidePanel icon={ClipboardList} title="Incident workflow">
              <ol className="space-y-3 text-sm text-slate-300">
                {[
                  ['Draft', 'Incident created'],
                  ['Review', 'Manager and safety review'],
                  ['Escalation', 'Route based on severity'],
                  ['Closed', 'Incident resolved and closed'],
                ].map(([label, hint], index) => (
                  <li key={label} className="flex gap-3">
                    <span
                      className={classNames(
                        'grid h-6 w-6 shrink-0 place-items-center rounded-full text-xs font-semibold',
                        index === 0 ? 'bg-sky-500 text-white' : 'bg-slate-700 text-slate-300',
                      )}
                    >
                      {index + 1}
                    </span>
                    <span>
                      <span className="block font-medium text-slate-100">{label}</span>
                      <span className="text-xs text-slate-400">{hint}</span>
                    </span>
                  </li>
                ))}
              </ol>
            </SidePanel>

            <SidePanel icon={ShieldCheck} title="Source of truth">
              <p className="text-sm leading-6 text-slate-300">
                People and organizational data come from StaffArr. Incident metadata is stored with
                the StaffArr incident record.
              </p>
            </SidePanel>

            <SidePanel icon={CheckCircle} title="Guidance">
              <ul className="space-y-3 text-sm text-slate-300">
                {[
                  'Use controlled fields for reporting consistency.',
                  'Link related records whenever possible.',
                  'Attach evidence to support facts and findings.',
                  'Follow your organization incident policy.',
                ].map((item) => (
                  <li key={item} className="flex gap-2">
                    <Check className="mt-0.5 h-4 w-4 shrink-0 text-emerald-300" />
                    <span>{item}</span>
                  </li>
                ))}
              </ul>
            </SidePanel>

            <SidePanel icon={Package} title="Quick references">
              <div className="space-y-2 text-sm">
                {quickReferences.map(([label, Icon]) => (
                  <button
                    key={label}
                    type="button"
                    className="flex w-full items-center justify-between rounded-md border border-slate-700 bg-slate-950/50 px-3 py-2 text-left text-sky-200 hover:border-sky-500/50"
                  >
                    <span className="flex items-center gap-2">
                      <Icon className="h-4 w-4" />
                      {label}
                    </span>
                  </button>
                ))}
              </div>
            </SidePanel>

            <SidePanel icon={HelpCircle} title="Need help?">
              <div className="space-y-2 text-sm text-slate-300">
                <p>Contact your Safety or Compliance team for assistance.</p>
                <p className="text-slate-400">Email: safety@yourorg.com</p>
                <p className="text-slate-400">Phone: +1 (555) 123-4567</p>
              </div>
            </SidePanel>
          </div>
        </div>
      </form>
    </div>
  )
}
