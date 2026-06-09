import { useEffect, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, Navigate, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { DetailBadge as Badge, ProfileDetailsLayout, type DetailTone } from '@stl/shared-ui'
import {
  AlertTriangle,
  CheckCircle2,
  ClipboardCheck,
  FilePenLine,
  Gauge,
  Loader2,
  MapPin,
  MoreHorizontal,
  Radar,
  ShieldCheck,
  Truck,
  Wrench,
  XCircle,
} from 'lucide-react'
import {
  getAsset,
  getAssetEditFieldset,
  getAssetFieldContext,
  acceptAssetExternalIntelligenceSuggestion,
  createAssetRecallWorkOrder,
  getAssetExternalIntelligenceOverview,
  getAssetMeters,
  getAssetReadiness,
  getAssetTelematicsIngestion,
  getDefects,
  getMe,
  getPmSchedules,
  getWorkOrders,
  refreshAssetExternalIntelligence,
  rejectAssetExternalIntelligenceSuggestion,
  updateAssetControlledV1,
} from '../../api/client'
import type {
  AssetMeterResponse,
  FieldMetadataResponse,
  FieldsetResponse,
  PmScheduleResponse,
  AssetTelematicsIngestionResponse,
} from '../../api/types'
import { canCreateWorkOrders, canManageAssets, loadSession } from '../../auth/sessionStorage'
import { AssetExternalIntelligencePanel } from '../../components/AssetExternalIntelligencePanel'
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

function humanize(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (char) => char.toUpperCase())
}

function compactNumber(value: number | null | undefined): string {
  if (value == null) return 'No reading'
  return new Intl.NumberFormat(undefined, { maximumFractionDigits: 1 }).format(value)
}

function telematicsOutcomeTone(outcome: string): DetailTone {
  switch (outcome.toLowerCase()) {
    case 'processed':
      return 'good'
    case 'ignored':
      return 'neutral'
    default:
      return 'warn'
  }
}

function humanizeEventKind(value: string): string {
  return value.replace(/[._-]+/g, ' ').replace(/\b\w/g, (char) => char.toUpperCase())
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
  const [scanCopied, setScanCopied] = useState(false)

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

  const externalIntelligenceQuery = useQuery({
    queryKey: ['maintainarr-asset-external-intelligence', assetId],
    queryFn: () => getAssetExternalIntelligenceOverview(session!.accessToken, assetId!),
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

  const telematicsQuery = useQuery<AssetTelematicsIngestionResponse>({
    queryKey: ['maintainarr-asset-telematics-ingestion', assetId],
    queryFn: () => getAssetTelematicsIngestion(session!.accessToken, assetId!),
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
    setScanCopied(false)
  }, [assetId])

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

  const refreshIntelligenceMutation = useMutation({
    mutationFn: () => refreshAssetExternalIntelligence(session!.accessToken, assetId!),
    onSuccess: async () => {
      setServerError(null)
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-external-intelligence', assetId] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-asset', assetId] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-field-context', assetId] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-readiness-detail', assetId] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-readiness-history', assetId] }),
      ])
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to refresh external intelligence')
    },
  })

  const acceptSuggestionMutation = useMutation({
    mutationFn: (suggestionId: string) =>
      acceptAssetExternalIntelligenceSuggestion(session!.accessToken, assetId!, suggestionId),
    onSuccess: async () => {
      setServerError(null)
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-external-intelligence', assetId] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-asset', assetId] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-field-context', assetId] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-readiness-detail', assetId] }),
      ])
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to accept external intelligence suggestion')
    },
  })

  const rejectSuggestionMutation = useMutation({
    mutationFn: (suggestionId: string) =>
      rejectAssetExternalIntelligenceSuggestion(session!.accessToken, assetId!, suggestionId),
    onSuccess: async () => {
      setServerError(null)
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-external-intelligence', assetId] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-asset', assetId] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-field-context', assetId] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-readiness-detail', assetId] }),
      ])
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to reject external intelligence suggestion')
    },
  })

  const createRecallWorkOrderMutation = useMutation({
    mutationFn: (recallId: string) => createAssetRecallWorkOrder(session!.accessToken, assetId!, recallId),
    onSuccess: async (created) => {
      setServerError(null)
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-external-intelligence', assetId] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-readiness-detail', assetId] }),
      ])
      navigate(`/work-orders/${created.workOrderId}`)
    },
    onError: (error) => {
      setServerError(error instanceof Error ? error.message : 'Failed to create recall work order')
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
  const telematics = telematicsQuery.data
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
  const readinessBlocked = readiness?.readinessStatus === 'not_ready' || blockerCount > 0
  const readinessAdvisory = !readinessBlocked && advisoryCount > 0
  const readinessDecisionLabel = readinessBlocked ? 'Blocked' : readinessAdvisory ? 'Advisory' : 'Clear'
  const readinessDecisionSummary = readinessBlocked
    ? 'Hold dispatch until blockers clear'
    : readinessAdvisory
      ? 'May dispatch, monitor defects'
      : 'Ready for dispatch'
  const readinessDecisionDetail = readinessBlocked
    ? 'Open maintenance blockers should be resolved before this asset returns to service.'
    : readinessAdvisory
      ? 'No hard out-of-service blockers. Track advisory signals before the next route.'
      : 'No current blockers or advisory maintenance signals are recorded.'
  const readinessDecisionTone: DetailTone = readinessBlocked ? 'bad' : readinessAdvisory ? 'warn' : 'good'
  const shopFloorScanCode = asset ? `maintainarr://asset/${asset.assetId}` : ''
  const latestTelematicsEvent = telematics?.items[0] ?? null

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
        <ProfileDetailsLayout
          testId="asset-profile-page"
          backLabel="Assets"
          backTo="/assets/drawer"
          breadcrumbs={[asset.className, asset.assetTag]}
          icon={<Truck className="h-8 w-8" />}
          title={asset.name}
          subtitle={(
            <span className="flex flex-wrap items-center gap-2">
              <MapPin className="h-4 w-4 text-slate-400" />
              <span>{locationText}</span>
              <span className="text-slate-600">-</span>
              <span>{classificationText}</span>
              <span className="text-slate-600">-</span>
              <span>{humanize(asset.lifecycleStatus)}</span>
            </span>
          )}
          badges={[
            { label: asset.assetTag, tone: 'info' },
            outOfService ? { label: 'Out of service', tone: 'bad' } : { label: 'Available', tone: 'good' },
            { label: readinessDecisionLabel === 'Clear' ? 'Ready' : readinessDecisionLabel === 'Advisory' ? 'Ready with advisories' : 'Blocked', tone: readinessDecisionTone },
          ]}
          actions={(
            <>
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
            </>
          )}
          metrics={[
            {
              label: 'Open defects',
              value: openDefects,
              hint: `${safetyWatched} safety watched`,
              icon: <AlertTriangle className="h-5 w-5" />,
              tone: openDefects > 0 ? 'warn' : 'good',
            },
            {
              label: 'Open work orders',
              value: activeWorkOrders,
              hint: `${workOrdersQuery.data?.filter((workOrder) => workOrder.status === 'waiting_parts').length ?? 0} waiting parts`,
              icon: <Wrench className="h-5 w-5" />,
              tone: activeWorkOrders > 0 ? 'info' : 'good',
            },
            {
              label: 'PM compliance',
              value: `${pmPercent}%`,
              hint: nextPm ? `${nextPm.name} due ${formatDate(nextPm.nextDueAt)}` : 'No PM program assigned',
              icon: <Gauge className="h-5 w-5" />,
              tone: pmPercent >= 90 ? 'good' : pmPercent >= 70 ? 'warn' : 'bad',
            },
            {
              label: 'Inspection state',
              value: inspectionState,
              hint: inspectionHint,
              icon: <ClipboardCheck className="h-5 w-5" />,
              tone: failedInspections > 0 ? 'bad' : 'good',
            },
          ]}
          tabs={overviewTabs}
          snapshotTitle="Asset snapshot"
          snapshotSubtitle="Core identity, platform-populated fields, and live operating counters."
          snapshotFields={orderedSnapshotFields(fieldset, values).map((field) => ({
            label: field.label,
            value: formatAssetFieldValue(fieldset, field, values[field.key], values, displayValues),
            source: fieldSourceLabel(field),
          }))}
          mainContent={(
            <>
              {isEditing && Object.keys(validationErrors).length > 0 ? (
                <p className="rounded-xl border border-amber-700 bg-amber-950/40 p-3 text-sm text-amber-100">
                  Resolve inline validation errors before saving.
                </p>
              ) : null}
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
                  <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-4" data-testid="asset-shop-floor-card">
                    <div className="flex flex-wrap items-start justify-between gap-3">
                      <div>
                        <h3 className="font-bold text-white">Shop-floor scan card</h3>
                        <p className="mt-1 text-sm text-slate-400">
                          Display this payload on a kiosk or mobile device so technicians can open the asset context directly.
                        </p>
                      </div>
                      <span className="rounded-full border border-slate-700 px-3 py-1 text-xs text-slate-300">
                        Mobile / QR ready
                      </span>
                    </div>
                    <div className="mt-4 grid gap-3 md:grid-cols-[minmax(0,1fr)_auto] md:items-center">
                      <div className="rounded-lg border border-slate-800 bg-slate-900/80 p-3">
                        <p className="text-xs uppercase tracking-wide text-slate-500">Scan payload</p>
                        <p className="mt-1 break-all font-mono text-sm text-sky-100" data-testid="asset-shop-floor-scan-code">
                          {shopFloorScanCode}
                        </p>
                        <p className="mt-2 text-xs text-slate-500">
                          {asset.assetTag} · {asset.name} · {humanize(asset.lifecycleStatus)}
                        </p>
                      </div>
                      <button
                        type="button"
                        className="rounded-xl border border-slate-700 bg-slate-900 px-4 py-2 text-sm font-medium text-slate-200 hover:bg-slate-800"
                        onClick={async () => {
                          try {
                            await navigator.clipboard?.writeText(shopFloorScanCode)
                          } finally {
                            setScanCopied(true)
                          }
                        }}
                      >
                        {scanCopied ? 'Copied' : 'Copy payload'}
                      </button>
                    </div>
                  </section>

                  <div className="grid gap-4 lg:grid-cols-2">
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

                  <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-4">
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

                  <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-4" data-testid="asset-telematics-ingestion-card">
                    <div className="flex flex-wrap items-start justify-between gap-3">
                      <div className="flex items-start gap-3">
                        <Radar className="mt-1 h-5 w-5 text-sky-300" />
                        <div>
                          <h3 className="font-bold text-white">Telematics / diagnostics ingestion</h3>
                          <p className="mt-1 text-sm text-slate-400">
                            Recent RoutArr inbound events linked to this asset, including telemetry signals and diagnostic outcomes.
                          </p>
                        </div>
                      </div>
                      <Badge
                        label={telematics ? `${telematics.processedCount} processed` : 'No history'}
                        tone={telematics && telematics.totalCount > 0 ? 'info' : 'neutral'}
                      />
                    </div>

                    {telematicsQuery.isLoading ? (
                      <p className="mt-4 text-sm text-slate-400">Loading telematics ingestion history…</p>
                    ) : telematicsQuery.isError ? (
                      <p className="mt-4 text-sm text-red-200">
                        Unable to load telematics ingestion history.
                      </p>
                    ) : telematics ? (
                      <>
                        <div className="mt-4 grid gap-3 md:grid-cols-3">
                          <div className="rounded-xl border border-slate-800 bg-slate-950/70 p-3">
                            <p className="text-xs text-slate-500">Total events</p>
                            <p className="text-sm font-medium text-slate-100">{telematics.totalCount}</p>
                          </div>
                          <div className="rounded-xl border border-slate-800 bg-slate-950/70 p-3">
                            <p className="text-xs text-slate-500">Processed</p>
                            <p className="text-sm font-medium text-slate-100">{telematics.processedCount}</p>
                          </div>
                          <div className="rounded-xl border border-slate-800 bg-slate-950/70 p-3">
                            <p className="text-xs text-slate-500">Defect-linked</p>
                            <p className="text-sm font-medium text-slate-100">{telematics.defectCount}</p>
                          </div>
                        </div>

                        {telematics.items.length === 0 ? (
                          <p className="mt-4 text-sm text-slate-400">No telematics or diagnostic events recorded yet for this asset.</p>
                        ) : (
                          <ul className="mt-4 space-y-3">
                            {telematics.items.map((item) => (
                              <li key={item.inboundEventId} className="rounded-xl border border-slate-800 bg-slate-950/70 p-4">
                                <div className="flex flex-wrap items-center gap-2">
                                  <Badge label={humanizeEventKind(item.eventKind)} tone="info" />
                                  <Badge label={item.outcome} tone={telematicsOutcomeTone(item.outcome)} />
                                  <span className="text-xs text-slate-500">{item.sourceProduct}</span>
                                </div>
                                <p className="mt-2 text-sm font-medium text-white">{item.summary}</p>
                                <div className="mt-2 grid gap-2 text-xs text-slate-400 md:grid-cols-2">
                                  <p>Occurred: {new Date(item.occurredAt).toLocaleString()}</p>
                                  <p>Correlation: {item.correlationId}</p>
                                  {item.vehicleRefKey ? <p>Vehicle ref: {item.vehicleRefKey}</p> : null}
                                  {item.tripNumber ? <p>Trip: {item.tripNumber}</p> : null}
                                  {item.incidentType ? <p>Incident: {item.incidentType}</p> : null}
                                  {item.incidentSeverity ? <p>Severity: {item.incidentSeverity}</p> : null}
                                  {item.dvirResult ? <p>DVIR: {item.dvirResult}</p> : null}
                                  {item.createdDefectId ? <p>Defect: {item.createdDefectId}</p> : null}
                                </div>
                              </li>
                            ))}
                          </ul>
                        )}

                        {latestTelematicsEvent ? (
                          <p className="mt-3 text-xs text-slate-500">
                            Latest event: {humanizeEventKind(latestTelematicsEvent.eventKind)} ·{' '}
                            {new Date(latestTelematicsEvent.occurredAt).toLocaleString()}
                          </p>
                        ) : null}
                      </>
                    ) : (
                      <p className="mt-4 text-sm text-slate-400">Telematics ingestion history unavailable.</p>
                    )}
                  </section>
                </>
              )}
            </>
          )}
          decisionTitle="Readiness decision"
          decisionBadge={{ label: readinessDecisionLabel, tone: readinessDecisionTone }}
          decisionIcon={
            readinessBlocked ? (
              <XCircle className="h-5 w-5 text-red-300" />
            ) : readinessAdvisory ? (
              <AlertTriangle className="h-5 w-5 text-amber-300" />
            ) : (
              <CheckCircle2 className="h-5 w-5 text-emerald-300" />
            )
          }
          decisionSummary={readinessDecisionSummary}
          decisionDetail={readinessDecisionDetail}
          allowedChecks={compliantSignalCount}
          blockedChecks={blockedSignalCount}
          railSections={[
            {
              title: 'External intelligence',
              icon: <Radar className="h-5 w-5" />,
              content: (
                <AssetExternalIntelligencePanel
                  overview={externalIntelligenceQuery.data ?? null}
                  isLoading={externalIntelligenceQuery.isLoading}
                  isRefreshing={refreshIntelligenceMutation.isPending}
                  onRefresh={() => refreshIntelligenceMutation.mutate()}
                  onAcceptSuggestion={(suggestionId) => acceptSuggestionMutation.mutate(suggestionId)}
                  onRejectSuggestion={(suggestionId) => rejectSuggestionMutation.mutate(suggestionId)}
                  onCreateRecallWorkOrder={(recallId) => createRecallWorkOrderMutation.mutate(recallId)}
                />
              ),
            },
            {
              title: 'Compliance links',
              icon: <ShieldCheck className="h-5 w-5" />,
              content: (
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
              ),
            },
          ]}
        />
      ) : null}
    </div>
  )
}
