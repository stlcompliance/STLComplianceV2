import { useEffect, useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, Navigate, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import {
  AlertTriangle,
  ArrowLeft,
  CheckCircle2,
  ChevronDown,
  ClipboardCheck,
  FilePenLine,
  Gauge,
  Loader2,
  MapPin,
  MoreHorizontal,
  ShieldCheck,
  Truck,
  Wrench,
  XCircle,
} from 'lucide-react'
import {
  getAsset,
  getAssetEditFieldset,
  getAssetFieldContext,
  getAssetMeters,
  getAssetReadiness,
  getDefects,
  getMe,
  getPmSchedules,
  getWorkOrders,
  updateAssetControlledV1,
} from '../../api/client'
import type {
  AssetMeterResponse,
  FieldMetadataResponse,
  FieldsetResponse,
  PmScheduleResponse,
} from '../../api/types'
import { canCreateWorkOrders, canManageAssets, loadSession } from '../../auth/sessionStorage'
import {
  AssetSectionList,
  buildAssetUpsertPayload,
  fieldIsVisible,
  formatAssetFieldValue,
  getFilteredOptions,
  initializeAssetFieldValues,
  validateAssetValues,
  valuesFromFieldContext,
  type AssetFieldValues,
} from '../../components/AssetFieldsetWorkflow'

const overviewTabs = ['Overview', 'Inspections', 'Work Orders', 'PM Plan', 'Defects', 'Documents', 'History']

const snapshotPreferredKeys = [
  'VIN',
  'vin',
  'unitNumber',
  'assetNumber',
  'displayName',
  'year',
  'make',
  'model',
  'yearMakeModel',
  'assetClass',
  'assetType',
  'configuration',
  'fuelType',
  'licensePlate',
  'plate',
  'odometer',
  'engineHours',
  'siteId',
  'homeLocationId',
]

function badgeClass(tone: 'good' | 'warn' | 'neutral' | 'bad' | 'info'): string {
  if (tone === 'good') return 'border-emerald-400/30 bg-emerald-500/15 text-emerald-200'
  if (tone === 'warn') return 'border-amber-400/30 bg-amber-500/15 text-amber-200'
  if (tone === 'bad') return 'border-red-400/30 bg-red-500/15 text-red-200'
  if (tone === 'info') return 'border-sky-400/30 bg-sky-500/15 text-sky-200'
  return 'border-slate-500/30 bg-slate-500/10 text-slate-300'
}

function Badge({ label, tone = 'neutral' }: { label: string; tone?: 'good' | 'warn' | 'neutral' | 'bad' | 'info' }) {
  return (
    <span className={`inline-flex items-center rounded-full border px-3 py-1 text-xs font-semibold ${badgeClass(tone)}`}>
      {label}
    </span>
  )
}

function humanize(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (char) => char.toUpperCase())
}

function compactNumber(value: number | null | undefined): string {
  if (value == null) return 'No reading'
  return new Intl.NumberFormat(undefined, { maximumFractionDigits: 1 }).format(value)
}

function formatDate(value: string | null | undefined): string {
  if (!value) return 'Not scheduled'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return 'Not scheduled'
  return date.toLocaleDateString(undefined, { month: 'short', day: 'numeric' })
}

function clearInvalidDependentValues(
  fieldset: FieldsetResponse,
  values: AssetFieldValues,
  changedFieldKey: string,
): AssetFieldValues {
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

    if (current != null && String(current).trim() && !allowed.has(String(current))) {
      next = { ...next, [field.key]: '' }
    }
  }
  return next
}

function latestMeter(meters: AssetMeterResponse[]): AssetMeterResponse | null {
  if (meters.length === 0) return null
  return [...meters].sort((a, b) => {
    const aTime = a.lastReadingAt ? Date.parse(a.lastReadingAt) : 0
    const bTime = b.lastReadingAt ? Date.parse(b.lastReadingAt) : 0
    return bTime - aTime
  })[0] ?? null
}

function pmCompliancePercent(schedules: PmScheduleResponse[]): number {
  if (schedules.length === 0) return 100
  const compliant = schedules.filter((schedule) => !['due', 'overdue'].includes(schedule.dueStatus.toLowerCase())).length
  return Math.round((compliant / schedules.length) * 100)
}

function fieldSourceLabel(field: FieldMetadataResponse): string {
  if (field.sourceOfTruth) return field.sourceOfTruth
  if (field.source) return humanize(field.source)
  if (field.catalogKey) return 'Catalog'
  if (field.referenceKey) return 'Reference'
  return 'Record'
}

function hasFieldValue(value: unknown): boolean {
  if (Array.isArray(value)) return value.length > 0
  return value != null && String(value).trim().length > 0
}

function orderedSnapshotFields(
  fieldset: FieldsetResponse,
  values: AssetFieldValues,
): FieldMetadataResponse[] {
  const visibleFields = fieldset.fields.filter((field) => fieldIsVisible(fieldset, field, values))
  const byKey = new Map(visibleFields.map((field) => [field.key, field]))
  const preferred = snapshotPreferredKeys
    .map((key) => byKey.get(key))
    .filter((field): field is FieldMetadataResponse => Boolean(field))

  const remaining = visibleFields
    .filter((field) => !snapshotPreferredKeys.includes(field.key))
    .sort((a, b) => {
      const aHasValue = hasFieldValue(values[a.key]) ? 0 : 1
      const bHasValue = hasFieldValue(values[b.key]) ? 0 : 1
      return aHasValue - bHasValue || a.label.localeCompare(b.label)
    })

  return [...preferred, ...remaining].slice(0, 9)
}

function SummaryMetric({
  label,
  value,
  hint,
  icon,
  tone = 'neutral',
}: {
  label: string
  value: string | number
  hint: string
  icon: ReactNode
  tone?: 'good' | 'warn' | 'neutral' | 'bad' | 'info'
}) {
  const iconClass = {
    good: 'bg-emerald-500/15 text-emerald-300',
    warn: 'bg-amber-500/15 text-amber-300',
    bad: 'bg-red-500/15 text-red-300',
    info: 'bg-sky-500/15 text-sky-300',
    neutral: 'bg-slate-700/60 text-slate-300',
  }[tone]

  return (
    <section className="min-h-36 rounded-2xl border border-slate-800 bg-slate-950/70 p-4 shadow-[0_18px_42px_rgba(2,6,23,0.22)]">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-sm text-sky-200/80">{label}</p>
          <p className="mt-3 text-3xl font-bold tracking-normal text-white">{value}</p>
        </div>
        <div className={`rounded-xl p-3 ${iconClass}`}>{icon}</div>
      </div>
      <p className="mt-2 text-xs text-slate-400">{hint}</p>
    </section>
  )
}

function SnapshotGrid({
  fieldset,
  values,
  displayValues,
}: {
  fieldset: FieldsetResponse
  values: AssetFieldValues
  displayValues: Record<string, string>
}) {
  const fields = orderedSnapshotFields(fieldset, values)

  return (
    <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
      {fields.map((field) => (
        <div key={field.key} className="min-h-[4.5rem] rounded-xl border border-slate-800 bg-slate-950/60 p-3">
          <div className="flex items-start justify-between gap-2">
            <dt className="text-xs font-semibold uppercase tracking-normal text-sky-200/55">{field.label}</dt>
            <span className="shrink-0 text-[10px] text-slate-500">{fieldSourceLabel(field)}</span>
          </div>
          <dd className="mt-2 break-words text-sm font-semibold text-white">
            {formatAssetFieldValue(fieldset, field, values[field.key], values, displayValues)}
          </dd>
        </div>
      ))}
    </div>
  )
}

function ReadinessDecision({
  readinessStatus,
  blockerCount,
  advisoryCount,
}: {
  readinessStatus: 'ready' | 'not_ready' | undefined
  blockerCount: number
  advisoryCount: number
}) {
  const isBlocked = readinessStatus === 'not_ready' || blockerCount > 0
  const isAdvisory = !isBlocked && advisoryCount > 0

  const title = isBlocked
    ? 'Hold dispatch until blockers clear'
    : isAdvisory
      ? 'May dispatch, monitor defects'
      : 'Ready for dispatch'
  const body = isBlocked
    ? 'Open maintenance blockers should be resolved before this asset returns to service.'
    : isAdvisory
      ? 'No hard out-of-service blockers. Track advisory signals before the next route.'
      : 'No current blockers or advisory maintenance signals are recorded.'
  const tone = isBlocked ? 'bad' : isAdvisory ? 'warn' : 'good'

  return (
    <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
      <div className="flex items-start justify-between gap-3">
        <h2 className="text-lg font-bold text-white">Readiness decision</h2>
        <Badge label={isBlocked ? 'Blocked' : isAdvisory ? 'Advisory' : 'Clear'} tone={tone} />
      </div>
      <div className={`mt-4 rounded-2xl border p-4 ${isBlocked ? 'border-red-500/30 bg-red-500/10' : isAdvisory ? 'border-amber-500/30 bg-amber-500/10' : 'border-emerald-500/30 bg-emerald-500/10'}`}>
        <div className="flex gap-3">
          {isBlocked ? <XCircle className="mt-0.5 h-5 w-5 shrink-0 text-red-300" /> : isAdvisory ? <AlertTriangle className="mt-0.5 h-5 w-5 shrink-0 text-amber-300" /> : <CheckCircle2 className="mt-0.5 h-5 w-5 shrink-0 text-emerald-300" />}
          <div>
            <p className="font-semibold text-white">{title}</p>
            <p className="mt-2 text-sm leading-6 text-slate-200">{body}</p>
          </div>
        </div>
      </div>
    </section>
  )
}

export function AssetProfilePage({ editModeDefault = false }: { editModeDefault?: boolean }) {
  const session = loadSession()
  const { assetId } = useParams()
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [isEditing, setIsEditing] = useState(editModeDefault)
  const [values, setValues] = useState<AssetFieldValues>({})
  const [displayValues, setDisplayValues] = useState<Record<string, string>>({})
  const [savedSnapshot, setSavedSnapshot] = useState('')
  const [serverError, setServerError] = useState<string | null>(null)

  const created = searchParams.get('created') === '1'

  const meQuery = useQuery({
    queryKey: ['maintainarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const assetQuery = useQuery({
    queryKey: ['maintainarr-asset', assetId],
    queryFn: () => getAsset(session!.accessToken, assetId!),
    enabled: Boolean(session?.accessToken && assetId),
  })

  const fieldsetQuery = useQuery({
    queryKey: ['maintainarr-fieldset-assets-edit', assetId],
    queryFn: () => getAssetEditFieldset(session!.accessToken, assetId!),
    enabled: Boolean(session?.accessToken && assetId),
  })

  const fieldContextQuery = useQuery({
    queryKey: ['maintainarr-asset-field-context', assetId],
    queryFn: () => getAssetFieldContext(session!.accessToken, assetId!),
    enabled: Boolean(session?.accessToken && assetId),
  })

  const readinessQuery = useQuery({
    queryKey: ['maintainarr-asset-readiness-detail', assetId],
    queryFn: () => getAssetReadiness(session!.accessToken, assetId!),
    enabled: Boolean(session?.accessToken && assetId),
  })

  const defectsQuery = useQuery({
    queryKey: ['maintainarr-defects', assetId, 'open'],
    queryFn: () => getDefects(session!.accessToken, { assetId, status: 'open' }),
    enabled: Boolean(session?.accessToken && assetId),
  })

  const workOrdersQuery = useQuery({
    queryKey: ['maintainarr-work-orders', assetId, 'open'],
    queryFn: () => getWorkOrders(session!.accessToken, { assetId, status: 'open' }),
    enabled: Boolean(session?.accessToken && assetId),
  })

  const pmSchedulesQuery = useQuery({
    queryKey: ['maintainarr-pm-schedules'],
    queryFn: () => getPmSchedules(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const metersQuery = useQuery({
    queryKey: ['maintainarr-asset-meters', assetId],
    queryFn: () => getAssetMeters(session!.accessToken, assetId!),
    enabled: Boolean(session?.accessToken && assetId),
  })

  const fieldset = fieldsetQuery.data
  const asset = assetQuery.data
  const validationErrors = fieldset ? validateAssetValues(fieldset, values) : {}
  const isDirty = savedSnapshot !== JSON.stringify(values)
  const canUpdate = meQuery.data
    ? canManageAssets(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false
  const canCreateWo = meQuery.data
    ? canCreateWorkOrders(meQuery.data.tenantRoleKey, meQuery.data.isPlatformAdmin)
    : false

  useEffect(() => {
    setIsEditing(editModeDefault)
  }, [editModeDefault, assetId])

  useEffect(() => {
    if (!asset || !fieldset) return
    const contextValues = valuesFromFieldContext(asset, fieldContextQuery.data ?? null)
    const merged = {
      ...initializeAssetFieldValues(fieldset),
      ...contextValues.values,
    }
    setValues(merged)
    setDisplayValues(contextValues.displayValues)
    setSavedSnapshot(JSON.stringify(merged))
  }, [asset, fieldset, fieldContextQuery.data])

  useEffect(() => {
    if (!isEditing || !isDirty) return
    const handleBeforeUnload = (event: BeforeUnloadEvent) => {
      event.preventDefault()
      event.returnValue = ''
    }
    window.addEventListener('beforeunload', handleBeforeUnload)
    return () => window.removeEventListener('beforeunload', handleBeforeUnload)
  }, [isDirty, isEditing])

  const updateMutation = useMutation({
    mutationFn: () => updateAssetControlledV1(session!.accessToken, assetId!, buildAssetUpsertPayload(values)),
    onSuccess: async () => {
      setServerError(null)
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['maintainarr-asset', assetId] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-field-context', assetId] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-assets'] }),
      ])
      navigate(`/assets/${assetId}`)
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to update asset')
    },
  })

  if (!session) {
    return <Navigate to="/launch" replace />
  }

  if (!assetId) {
    return <Navigate to="/assets/drawer" replace />
  }

  const handleFieldChange = (fieldKey: string, value: unknown) => {
    if (!fieldset) return
    setValues((current) => clearInvalidDependentValues(fieldset, { ...current, [fieldKey]: value }, fieldKey))
    setServerError(null)
  }

  const handleCancel = () => {
    if (isDirty && !window.confirm('Discard unsaved asset changes?')) return
    if (editModeDefault) {
      navigate(`/assets/${assetId}`)
      return
    }
    if (asset && fieldset) {
      const contextValues = valuesFromFieldContext(asset, fieldContextQuery.data ?? null)
      const merged = {
        ...initializeAssetFieldValues(fieldset),
        ...contextValues.values,
      }
      setValues(merged)
      setSavedSnapshot(JSON.stringify(merged))
    }
    setIsEditing(false)
  }

  const readiness = readinessQuery.data
  const meters = metersQuery.data ?? []
  const latest = latestMeter(meters)
  const assetPmSchedules = (pmSchedulesQuery.data ?? [])
    .filter((schedule) => schedule.assetId === assetId)
    .sort((a, b) => Date.parse(a.nextDueAt) - Date.parse(b.nextDueAt))
  const nextPm = assetPmSchedules[0] ?? null
  const outOfService = asset?.lifecycleStatus === 'out_of_service'
  const openDefects = readiness?.signals.openCriticalDefectCount ?? defectsQuery.data?.length ?? 0
  const safetyWatched = readiness?.signals.openHighDefectCount ?? 0
  const activeWorkOrders = readiness?.signals.activeWorkOrderCount ?? workOrdersQuery.data?.length ?? 0
  const pmPercent = pmCompliancePercent(assetPmSchedules)
  const failedInspections = readiness?.signals.failedInspectionCount ?? 0
  const blockerCount = readiness?.blockers.length ?? 0
  const advisoryCount = safetyWatched + (readiness?.signals.pmDueCount ?? 0)
  const blockedSignalCount = blockerCount + failedInspections + (readiness?.signals.pmOverdueCount ?? 0)
  const compliantSignalCount = Math.max(0, 6 - blockedSignalCount)
  const locationText = displayValues.siteId || asset?.siteRef || 'No site assigned'
  const classificationText = asset ? `${asset.className} / ${asset.typeName}` : 'Asset detail'
  const inspectionState = failedInspections > 0 ? 'Action' : 'Pass'
  const inspectionHint = failedInspections > 0 ? `${failedInspections} failed checks` : 'No failed inspections recorded'

  return (
    <div className="w-full max-w-[1500px] space-y-6 pb-10" data-testid="asset-profile-page">
      {assetQuery.isLoading || fieldsetQuery.isLoading || fieldContextQuery.isLoading ? (
        <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-6">
          <div className="flex items-center gap-3 text-sm text-slate-300">
            <Loader2 className="h-4 w-4 animate-spin" />
            Loading asset profile...
          </div>
        </section>
      ) : null}

      {created ? (
        <section className="rounded-2xl border border-emerald-500/30 bg-emerald-500/10 p-4">
          <p className="text-sm font-medium text-emerald-100">Asset created.</p>
          <p className="mt-1 text-sm text-emerald-200/80">
            Optional sections can be completed now or later. Missing sections show clear empty states below.
          </p>
        </section>
      ) : null}

      {serverError ? (
        <p className="rounded-xl border border-red-800 bg-red-950/40 p-3 text-sm text-red-200">{serverError}</p>
      ) : null}

      {assetQuery.isError ? (
        <p className="rounded-xl border border-red-800 bg-red-950/40 p-3 text-sm text-red-200">Asset was not found.</p>
      ) : null}

      {asset && fieldset ? (
        <>
          <section className="rounded-[1.4rem] border border-slate-800 bg-slate-950/80 p-5 shadow-[0_24px_70px_rgba(2,6,23,0.32)]">
            <div className="flex flex-wrap items-start justify-between gap-5">
              <div className="min-w-0">
                <nav className="flex flex-wrap items-center gap-3 text-sm text-sky-200/80" aria-label="Asset breadcrumb">
                  <Link
                    to="/assets/drawer"
                    className="inline-flex items-center gap-2 rounded-xl border border-slate-800 bg-slate-950 px-3 py-2 text-slate-300 hover:text-white"
                  >
                    <ArrowLeft className="h-4 w-4" />
                    Assets
                  </Link>
                  <span className="text-slate-500">/</span>
                  <span>{asset.className}</span>
                  <span className="text-slate-500">/</span>
                  <span className="font-semibold text-white">{asset.assetTag}</span>
                </nav>

                <div className="mt-7 flex items-center gap-4">
                  <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-2xl border border-sky-400/20 bg-sky-500/15 text-sky-300">
                    <Truck className="h-8 w-8" />
                  </div>
                  <div className="min-w-0">
                    <div className="mb-3 flex flex-wrap items-center gap-2">
                      <Badge label={asset.assetTag} tone="info" />
                      {outOfService ? <Badge label="Out of service" tone="bad" /> : <Badge label="Available" tone="good" />}
                      {readiness?.readinessStatus === 'not_ready' ? (
                        <Badge label="Blocked" tone="bad" />
                      ) : advisoryCount > 0 ? (
                        <Badge label="Ready with advisories" tone="warn" />
                      ) : (
                        <Badge label="Ready" tone="good" />
                      )}
                    </div>
                    <h1 className="truncate text-3xl font-bold tracking-normal text-white md:text-4xl">{asset.name}</h1>
                    <p className="mt-2 flex flex-wrap items-center gap-2 text-sm text-sky-100/75">
                      <MapPin className="h-4 w-4 text-slate-400" />
                      <span>{locationText}</span>
                      <span className="text-slate-600">-</span>
                      <span>{classificationText}</span>
                      <span className="text-slate-600">-</span>
                      <span>{humanize(asset.lifecycleStatus)}</span>
                    </p>
                  </div>
                </div>
              </div>

              <div className="flex flex-wrap items-center justify-end gap-2">
                {isEditing ? (
                  <>
                    <button
                      type="button"
                      className="rounded-xl border border-slate-700 bg-slate-900 px-4 py-2 text-sm font-semibold text-slate-200 hover:bg-slate-800"
                      onClick={handleCancel}
                    >
                      Cancel
                    </button>
                    <button
                      type="button"
                      aria-label="Save asset"
                      className="rounded-xl bg-amber-500 px-4 py-2 text-sm font-bold text-slate-950 disabled:opacity-50"
                      disabled={!isDirty || Object.keys(validationErrors).length > 0 || updateMutation.isPending}
                      onClick={() => updateMutation.mutate()}
                    >
                      {updateMutation.isPending ? 'Saving...' : 'Save'}
                    </button>
                  </>
                ) : (
                  <>
                    <Link
                      to={`/inspections/create?assetId=${encodeURIComponent(asset.assetId)}`}
                      className="inline-flex items-center gap-2 rounded-xl bg-sky-500 px-4 py-3 text-sm font-bold text-slate-950 hover:bg-sky-400"
                    >
                      <ClipboardCheck className="h-4 w-4" />
                      Start inspection
                    </Link>
                    <Link
                      to={`/work-orders/create?assetId=${encodeURIComponent(asset.assetId)}`}
                      className={`inline-flex items-center gap-2 rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm font-bold text-white hover:bg-slate-800 ${canCreateWo ? '' : 'pointer-events-none opacity-50'}`}
                    >
                      <Wrench className="h-4 w-4" />
                      Create WO
                    </Link>
                    {canUpdate ? (
                      <Link
                        to={`/assets/${asset.assetId}/edit`}
                        aria-label="Edit asset"
                        className="inline-flex items-center gap-2 rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm font-bold text-white hover:bg-slate-800"
                      >
                        <FilePenLine className="h-4 w-4" />
                        Edit
                      </Link>
                    ) : null}
                    <button
                      type="button"
                      aria-label="More asset actions"
                      className="inline-flex h-12 w-12 items-center justify-center rounded-xl border border-slate-700 bg-slate-900 text-slate-300 hover:bg-slate-800 hover:text-white"
                    >
                      <MoreHorizontal className="h-5 w-5" />
                    </button>
                  </>
                )}
              </div>
            </div>
          </section>

          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
            <SummaryMetric
              label="Open defects"
              value={openDefects}
              hint={`${safetyWatched} safety watched`}
              icon={<AlertTriangle className="h-5 w-5" />}
              tone={openDefects > 0 ? 'warn' : 'good'}
            />
            <SummaryMetric
              label="Open work orders"
              value={activeWorkOrders}
              hint={`${workOrdersQuery.data?.filter((workOrder) => workOrder.status === 'waiting_parts').length ?? 0} waiting parts`}
              icon={<Wrench className="h-5 w-5" />}
              tone={activeWorkOrders > 0 ? 'info' : 'good'}
            />
            <SummaryMetric
              label="PM compliance"
              value={`${pmPercent}%`}
              hint={nextPm ? `${nextPm.name} due ${formatDate(nextPm.nextDueAt)}` : 'No PM program assigned'}
              icon={<Gauge className="h-5 w-5" />}
              tone={pmPercent >= 90 ? 'good' : pmPercent >= 70 ? 'warn' : 'bad'}
            />
            <SummaryMetric
              label="Inspection state"
              value={inspectionState}
              hint={inspectionHint}
              icon={<ClipboardCheck className="h-5 w-5" />}
              tone={failedInspections > 0 ? 'bad' : 'good'}
            />
          </div>

          {isEditing && Object.keys(validationErrors).length > 0 ? (
            <p className="rounded-xl border border-amber-700 bg-amber-950/40 p-3 text-sm text-amber-100">
              Resolve inline validation errors before saving.
            </p>
          ) : null}

          <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_397px]">
            <main className="min-w-0 rounded-2xl border border-slate-800 bg-slate-950/70">
              <div className="flex gap-2 overflow-x-auto border-b border-slate-800 p-3" role="tablist" aria-label="Asset detail sections">
                {overviewTabs.map((tab) => (
                  <button
                    key={tab}
                    type="button"
                    role="tab"
                    aria-selected={tab === 'Overview'}
                    className={`shrink-0 rounded-xl px-4 py-3 text-sm font-semibold ${tab === 'Overview' ? 'bg-slate-900 text-sky-300' : 'text-sky-200/70 hover:bg-slate-900/70 hover:text-white'}`}
                  >
                    {tab}
                  </button>
                ))}
              </div>

              <section className="p-5">
                <div className="mb-5 flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <h2 className="text-xl font-bold text-white">Asset snapshot</h2>
                    <p className="mt-1 text-sm text-sky-100/75">Core identity, platform-populated fields, and live operating counters.</p>
                  </div>
                  <button
                    type="button"
                    className="inline-flex items-center gap-2 rounded-xl border border-slate-800 bg-slate-900 px-4 py-3 text-sm font-semibold text-slate-200"
                  >
                    Field sources
                    <ChevronDown className="h-4 w-4" />
                  </button>
                </div>

                {isEditing ? (
                  <AssetSectionList
                    fieldset={fieldset}
                    values={values}
                    mode="edit"
                    onChange={handleFieldChange}
                    errors={validationErrors}
                    displayValues={displayValues}
                  />
                ) : (
                  <>
                    <SnapshotGrid fieldset={fieldset} values={values} displayValues={displayValues} />

                    <div className="mt-5 grid gap-4 lg:grid-cols-2">
                      <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-4">
                        <div className="flex items-start justify-between gap-3">
                          <div>
                            <h3 className="font-bold text-white">Usage and meters</h3>
                            <p className="mt-1 text-sm text-slate-400">
                              {latest ? `${latest.name} latest reading` : 'No meter reading recorded'}
                            </p>
                          </div>
                          <Badge label={latest ? latest.status : 'Empty'} tone={latest ? 'info' : 'neutral'} />
                        </div>
                        <p className="mt-5 text-3xl font-bold text-white">
                          {latest ? `${compactNumber(latest.currentReading)} ${latest.unit}` : 'No reading'}
                        </p>
                      </section>

                      <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-4">
                        <div className="flex items-start justify-between gap-3">
                          <div>
                            <h3 className="font-bold text-white">Action before dispatch</h3>
                            <p className="mt-1 text-sm text-slate-400">
                              {blockerCount > 0 ? `${blockerCount} blocker signal(s)` : 'No out-of-service blocker'}
                            </p>
                          </div>
                          <Badge label={blockerCount > 0 ? 'Blocked' : 'Ready'} tone={blockerCount > 0 ? 'bad' : 'good'} />
                        </div>
                        <p className="mt-5 text-sm leading-6 text-slate-300">
                          {readiness?.blockers[0]?.message ?? 'Keep routine PM, inspection, and defect checks current for this asset.'}
                        </p>
                      </section>
                    </div>

                    <section className="mt-5 rounded-2xl border border-slate-800 bg-slate-950/60 p-4">
                      <h3 className="font-bold text-white">History / Audit</h3>
                      <div className="mt-4 grid gap-3 md:grid-cols-2">
                        <div className="rounded-xl border border-slate-800 bg-slate-950/70 p-3">
                          <p className="text-xs text-slate-500">Created</p>
                          <p className="text-sm font-medium text-slate-100">{new Date(asset.createdAt).toLocaleString()}</p>
                        </div>
                        <div className="rounded-xl border border-slate-800 bg-slate-950/70 p-3">
                          <p className="text-xs text-slate-500">Last updated</p>
                          <p className="text-sm font-medium text-slate-100">{new Date(asset.updatedAt).toLocaleString()}</p>
                        </div>
                      </div>
                    </section>
                  </>
                )}
              </section>
            </main>

            <aside className="space-y-5">
              <ReadinessDecision
                readinessStatus={readiness?.readinessStatus}
                blockerCount={blockerCount}
                advisoryCount={advisoryCount}
              />

              <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
                <div className="grid grid-cols-2 gap-3">
                  <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
                    <CheckCircle2 className="h-5 w-5 text-emerald-300" />
                    <p className="mt-4 text-xs text-slate-400">Compliant</p>
                    <p className="text-xl font-bold text-white">{compliantSignalCount} checks</p>
                  </div>
                  <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
                    <XCircle className="h-5 w-5 text-red-300" />
                    <p className="mt-4 text-xs text-slate-400">Blocked</p>
                    <p className="text-xl font-bold text-white">{blockedSignalCount} checks</p>
                  </div>
                </div>
              </section>

              <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
                <div className="mb-4 flex items-center justify-between">
                  <h2 className="text-lg font-bold text-white">Compliance links</h2>
                  <ShieldCheck className="h-5 w-5 text-sky-300" />
                </div>
                <div className="space-y-3">
                  <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
                    <p className="text-sm font-bold text-white">Governing body</p>
                    <p className="mt-1 text-sm text-sky-100/75">{displayValues.governingBodyKey || 'FMCSA / DOT from Compliance Core catalog'}</p>
                  </div>
                  <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
                    <p className="text-sm font-bold text-white">Applicable rulepacks</p>
                    <p className="mt-1 text-sm leading-6 text-sky-100/75">
                      {displayValues.rulepackApplicabilityKeys || 'DOT Annual - DVIR - PM Evidence - Roadside Readiness'}
                    </p>
                  </div>
                  <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
                    <p className="text-sm font-bold text-white">Open references</p>
                    <p className="mt-1 text-sm text-slate-400">Evidence and inspection documents stay attached to this asset profile.</p>
                  </div>
                </div>
              </section>
            </aside>
          </div>
        </>
      ) : null}
    </div>
  )
}
