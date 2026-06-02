import { useEffect, useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, Navigate, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import {
  AlertTriangle,
  Archive,
  ClipboardCheck,
  FileUp,
  Gauge,
  Loader2,
  MapPin,
  Pencil,
  PlusCircle,
  RefreshCcw,
  ShieldCheck,
  TimerOff,
  Wrench,
} from 'lucide-react'
import { PageHeader } from '@stl/shared-ui'
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
import type { AssetMeterResponse, FieldsetResponse } from '../../api/types'
import { canCreateWorkOrders, canManageAssets, loadSession } from '../../auth/sessionStorage'
import {
  AssetSectionList,
  buildAssetUpsertPayload,
  getFilteredOptions,
  initializeAssetFieldValues,
  validateAssetValues,
  valuesFromFieldContext,
  type AssetFieldValues,
} from '../../components/AssetFieldsetWorkflow'

function badgeClass(tone: 'good' | 'warn' | 'neutral' | 'bad'): string {
  if (tone === 'good') return 'bg-emerald-500/10 text-emerald-300 ring-emerald-500/25'
  if (tone === 'warn') return 'bg-amber-500/10 text-amber-300 ring-amber-500/25'
  if (tone === 'bad') return 'bg-red-500/10 text-red-300 ring-red-500/25'
  return 'bg-slate-500/10 text-slate-300 ring-slate-500/25'
}

function Badge({ label, tone = 'neutral' }: { label: string; tone?: 'good' | 'warn' | 'neutral' | 'bad' }) {
  return <span className={`inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium ring-1 ${badgeClass(tone)}`}>{label}</span>
}

function humanize(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (char) => char.toUpperCase())
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

function SummaryMetric({
  label,
  value,
  hint,
  icon,
}: {
  label: string
  value: string | number
  hint: string
  icon: ReactNode
}) {
  return (
    <div className="rounded-xl border border-slate-800 bg-slate-950/70 p-4">
      <p className="text-sm text-slate-400">{label}</p>
      <p className="mt-2 text-2xl font-bold text-white">{value}</p>
      <p className="mt-1 text-xs text-slate-500">{hint}</p>
      <div className="mt-2 text-sky-300">{icon}</div>
    </div>
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

  return (
    <div className="mx-auto max-w-6xl space-y-6" data-testid="asset-profile-page">
      <PageHeader
        title={asset ? asset.name : 'Asset Profile'}
        subtitle={asset ? `${asset.assetTag} · ${asset.className} / ${asset.typeName}` : 'MaintainArr asset detail'}
      />

      <div className="flex flex-wrap items-center justify-between gap-3">
        <Link to="/assets/drawer" className="text-sm text-slate-300 hover:text-white">
          Back to assets
        </Link>
        <div className="flex flex-wrap items-center gap-2">
          {asset && !isEditing && canUpdate ? (
            <Link
              to={`/assets/${asset.assetId}/edit`}
              className="inline-flex items-center gap-2 rounded-lg bg-sky-600 px-4 py-2 text-sm font-medium text-white"
            >
              <Pencil className="h-4 w-4" />
              Edit Asset
            </Link>
          ) : null}
          {isEditing ? (
            <>
              <button
                type="button"
                className="rounded-lg border border-slate-700 px-4 py-2 text-sm text-slate-200"
                onClick={handleCancel}
              >
                Cancel
              </button>
              <button
                type="button"
                className="rounded-lg bg-amber-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
                disabled={!isDirty || Object.keys(validationErrors).length > 0 || updateMutation.isPending}
                onClick={() => updateMutation.mutate()}
              >
                {updateMutation.isPending ? 'Saving...' : 'Save Asset'}
              </button>
            </>
          ) : null}
        </div>
      </div>

      {assetQuery.isLoading || fieldsetQuery.isLoading || fieldContextQuery.isLoading ? (
        <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-6">
          <div className="flex items-center gap-3 text-sm text-slate-300">
            <Loader2 className="h-4 w-4 animate-spin" />
            Loading asset profile...
          </div>
        </section>
      ) : null}

      {created ? (
        <section className="rounded-xl border border-emerald-500/30 bg-emerald-500/10 p-4">
          <p className="text-sm font-medium text-emerald-100">Asset created.</p>
          <p className="mt-1 text-sm text-emerald-200/80">
            Optional sections can be completed now or later. Missing sections show clear empty states below.
          </p>
        </section>
      ) : null}

      {serverError ? (
        <p className="rounded-lg border border-red-800 bg-red-950/40 p-3 text-sm text-red-200">{serverError}</p>
      ) : null}

      {assetQuery.isError ? (
        <p className="rounded-lg border border-red-800 bg-red-950/40 p-3 text-sm text-red-200">Asset was not found.</p>
      ) : null}

      {asset && fieldset ? (
        <>
          <header className="rounded-xl border border-slate-800 bg-slate-900/70 p-5">
            <div className="flex flex-wrap items-start justify-between gap-4">
              <div>
                <div className="flex flex-wrap items-center gap-2">
                  <h1 className="text-2xl font-semibold text-white">{asset.name}</h1>
                  {outOfService ? <Badge label="Out of service" tone="bad" /> : null}
                  {readiness?.readinessStatus === 'ready' ? <Badge label="Ready" tone="good" /> : <Badge label="Not ready" tone="warn" />}
                  <Badge label={humanize(asset.lifecycleStatus)} />
                </div>
                <p className="mt-2 text-sm text-slate-400">{asset.assetTag} · {asset.className} / {asset.typeName}</p>
                <p className="mt-2 flex items-center gap-2 text-sm text-slate-400">
                  <MapPin className="h-4 w-4" />
                  {displayValues.siteId || asset.siteRef || 'No site assigned'}
                </p>
              </div>
              <div className="flex flex-wrap gap-2">
                <Link to="/inspections/create" className="rounded-lg border border-slate-700 px-3 py-2 text-sm text-slate-200">
                  Start Inspection
                </Link>
                <Link
                  to="/work-orders/create"
                  className={`rounded-lg border border-slate-700 px-3 py-2 text-sm ${canCreateWo ? 'text-slate-200' : 'pointer-events-none opacity-50'}`}
                >
                  Create Work Order
                </Link>
                <Link to="/defects/create" className="rounded-lg border border-slate-700 px-3 py-2 text-sm text-slate-200">
                  Report Defect
                </Link>
                <Link to="/meters/create" className="rounded-lg border border-slate-700 px-3 py-2 text-sm text-slate-200">
                  Add Meter Reading
                </Link>
                <button
                  type="button"
                  disabled={!canUpdate}
                  className="inline-flex items-center gap-2 rounded-lg border border-slate-700 px-3 py-2 text-sm text-slate-200 disabled:opacity-50"
                >
                  <FileUp className="h-4 w-4" />
                  Upload Document
                </button>
                <button
                  type="button"
                  disabled={!canUpdate}
                  className="inline-flex items-center gap-2 rounded-lg border border-slate-700 px-3 py-2 text-sm text-slate-200 disabled:opacity-50"
                >
                  <TimerOff className="h-4 w-4" />
                  {outOfService ? 'Return to Service' : 'Mark Out of Service'}
                </button>
                <button
                  type="button"
                  disabled={!canUpdate}
                  className="inline-flex items-center gap-2 rounded-lg border border-slate-700 px-3 py-2 text-sm text-slate-200 disabled:opacity-50"
                >
                  <Archive className="h-4 w-4" />
                  Archive / Retire
                </button>
              </div>
            </div>
          </header>

          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
            <SummaryMetric
              label="Open defects"
              value={readiness?.signals.openCriticalDefectCount ?? defectsQuery.data?.length ?? 0}
              hint="Critical defects"
              icon={<AlertTriangle className="h-5 w-5" />}
            />
            <SummaryMetric
              label="Open work orders"
              value={readiness?.signals.activeWorkOrderCount ?? workOrdersQuery.data?.length ?? 0}
              hint="Active repair work"
              icon={<Wrench className="h-5 w-5" />}
            />
            <SummaryMetric
              label="Next PM due"
              value={nextPm ? humanize(nextPm.dueStatus) : 'No PM'}
              hint={nextPm ? new Date(nextPm.nextDueAt).toLocaleDateString() : 'No PM program assigned'}
              icon={<Gauge className="h-5 w-5" />}
            />
            <SummaryMetric
              label="Primary meter"
              value={latest ? latest.currentReading : 'No reading'}
              hint={latest ? `${latest.name} · ${latest.unit}` : 'No meter reading recorded'}
              icon={<RefreshCcw className="h-5 w-5" />}
            />
          </div>

          {isEditing && Object.keys(validationErrors).length > 0 ? (
            <p className="rounded-lg border border-amber-700 bg-amber-950/40 p-3 text-sm text-amber-100">
              Resolve inline validation errors before saving.
            </p>
          ) : null}

          <div className="grid gap-5 lg:grid-cols-[1fr_340px]">
            <div>
              <AssetSectionList
                fieldset={fieldset}
                values={values}
                mode={isEditing ? 'edit' : 'read'}
                onChange={isEditing ? handleFieldChange : undefined}
                errors={validationErrors}
                displayValues={displayValues}
              />

              <section className="mt-4 rounded-xl border border-slate-800 bg-slate-900/60 p-5">
                <h2 className="text-lg font-semibold text-white">History / Audit</h2>
                <div className="mt-4 grid gap-3 md:grid-cols-2">
                  <div className="rounded-lg border border-slate-800 bg-slate-950/70 p-3">
                    <p className="text-xs text-slate-500">Created</p>
                    <p className="text-sm font-medium text-slate-100">{new Date(asset.createdAt).toLocaleString()}</p>
                  </div>
                  <div className="rounded-lg border border-slate-800 bg-slate-950/70 p-3">
                    <p className="text-xs text-slate-500">Last updated</p>
                    <p className="text-sm font-medium text-slate-100">{new Date(asset.updatedAt).toLocaleString()}</p>
                  </div>
                </div>
              </section>
            </div>

            <aside className="space-y-4">
              <section className="rounded-xl border border-slate-800 bg-slate-950/70 p-4">
                <div className="mb-3 flex items-center justify-between">
                  <h2 className="font-semibold text-white">Readiness</h2>
                  <ShieldCheck className="h-4 w-4 text-sky-300" />
                </div>
                {readinessQuery.isLoading ? (
                  <p className="text-sm text-slate-400">Loading readiness...</p>
                ) : readiness?.blockers.length ? (
                  <ul className="space-y-2">
                    {readiness.blockers.slice(0, 4).map((blocker) => (
                      <li key={`${blocker.blockerType}-${blocker.sourceEntityId}`} className="rounded-lg border border-amber-500/25 bg-amber-500/10 p-3 text-sm text-amber-100">
                        <p className="font-medium">{humanize(blocker.blockerType)}</p>
                        <p className="mt-1 text-amber-100/80">{blocker.message}</p>
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p className="rounded-lg border border-emerald-500/20 bg-emerald-500/10 p-3 text-sm text-emerald-100">
                    No maintenance blockers.
                  </p>
                )}
              </section>

              <section className="rounded-xl border border-slate-800 bg-slate-950/70 p-4">
                <div className="mb-3 flex items-center justify-between">
                  <h2 className="font-semibold text-white">Inspection State</h2>
                  <ClipboardCheck className="h-4 w-4 text-sky-300" />
                </div>
                <p className="text-sm text-slate-400">
                  {readiness?.signals.failedInspectionCount
                    ? `${readiness.signals.failedInspectionCount} failed inspection signal(s)`
                    : 'No failed inspections recorded'}
                </p>
                <button type="button" className="mt-3 inline-flex items-center gap-2 text-sm text-sky-300">
                  <PlusCircle className="h-4 w-4" />
                  Assign Inspection Template
                </button>
              </section>

              <section className="rounded-xl border border-slate-800 bg-slate-950/70 p-4">
                <h2 className="font-semibold text-white">Documents / Evidence</h2>
                <p className="mt-2 text-sm text-slate-400">No documents uploaded yet.</p>
                <button type="button" disabled={!canUpdate} className="mt-3 text-sm text-sky-300 disabled:opacity-50">
                  Upload Document
                </button>
              </section>
            </aside>
          </div>
        </>
      ) : null}
    </div>
  )
}
