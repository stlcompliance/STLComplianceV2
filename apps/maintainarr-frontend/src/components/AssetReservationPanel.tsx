import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { DetailBadge as Badge, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import { AlertTriangle, CalendarClock, Clock3, MapPin, UsersRound } from 'lucide-react'

import {
  createAssetReservation,
  getAssetReservations,
  getPeople,
  getSites,
  searchAssets,
  updateAssetReservation,
  type AssetReservationAction,
} from '../api/client'
import type {
  AssetReadinessResponse,
  AssetSearchResponse,
  CreateAssetReservationRequest,
  ReferenceOptionResponse,
} from '../api/types'

interface AssetReservationPanelProps {
  accessToken: string
  assetId: string
  assetTag: string
  assetName: string
  readiness: AssetReadinessResponse | null
  canRequest: boolean
  canManage: boolean
}

interface ReservationDraft {
  purpose: string
  requestedStartAt: string
  requestedEndAt: string
  pickupLocationRef: string
  returnLocationRef: string
  operatorPersonId: string
  driverPersonId: string
  capacityNotes: string
  equipmentNotes: string
  notes: string
}

const RESERVATION_STATUSES = [
  'requested',
  'approved',
  'reserved',
  'checked_out',
  'in_use',
  'returned',
  'inspection',
  'closed',
  'canceled',
  'no_show',
] as const

const RESERVATION_STATUS_FILTERS = [
  { value: 'active', label: 'Active reservations' },
  { value: 'all', label: 'All reservations' },
  ...RESERVATION_STATUSES.map((status) => ({
    value: status,
    label: humanize(status),
  })),
]

const RESERVATION_ACTIONS: Record<string, Array<{ action: AssetReservationAction; label: string }>> = {
  requested: [
    { action: 'approve', label: 'Approve' },
    { action: 'reserve', label: 'Reserve' },
    { action: 'cancel', label: 'Cancel' },
    { action: 'no-show', label: 'Mark no-show' },
  ],
  approved: [
    { action: 'reserve', label: 'Reserve' },
    { action: 'checkout', label: 'Checkout' },
    { action: 'cancel', label: 'Cancel' },
    { action: 'no-show', label: 'Mark no-show' },
  ],
  reserved: [
    { action: 'checkout', label: 'Checkout' },
    { action: 'start-use', label: 'Start use' },
    { action: 'cancel', label: 'Cancel' },
    { action: 'no-show', label: 'Mark no-show' },
  ],
  checked_out: [
    { action: 'start-use', label: 'Start use' },
    { action: 'return', label: 'Return' },
  ],
  in_use: [{ action: 'return', label: 'Return' }],
  returned: [
    { action: 'inspect', label: 'Inspect' },
    { action: 'close', label: 'Close' },
  ],
  inspection: [{ action: 'close', label: 'Close' }],
}

function humanize(value: string): string {
  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (character) => character.toUpperCase())
}

function formatDateTime(value: string | null | undefined): string {
  if (!value) {
    return 'Not set'
  }

  const parsed = new Date(value)
  if (Number.isNaN(parsed.getTime())) {
    return 'Not set'
  }

  return parsed.toLocaleString()
}

function toDateTimeLocalValue(date: Date): string {
  const offset = date.getTimezoneOffset()
  const adjusted = new Date(date.getTime() - offset * 60_000)
  return adjusted.toISOString().slice(0, 16)
}

function createReservationDraft(): ReservationDraft {
  const requestedStartAt = new Date()
  const requestedEndAt = new Date(requestedStartAt.getTime() + 4 * 60 * 60_000)
  return {
    purpose: '',
    requestedStartAt: toDateTimeLocalValue(requestedStartAt),
    requestedEndAt: toDateTimeLocalValue(requestedEndAt),
    pickupLocationRef: '',
    returnLocationRef: '',
    operatorPersonId: '',
    driverPersonId: '',
    capacityNotes: '',
    equipmentNotes: '',
    notes: '',
  }
}

function toPickerOptions(items: ReferenceOptionResponse[] | undefined): PickerOption[] {
  return (items ?? []).map((item) => ({
    value: item.id ?? item.key,
    label: item.label,
    inactive: !item.isActive,
  }))
}

function pickerOptionForValue(options: PickerOption[], value: string): PickerOption | undefined {
  return options.find((option) => option.value === value)
}

function decisionTone(status: string): 'good' | 'warn' | 'bad' | 'info' | 'neutral' {
  switch (status.toLowerCase()) {
    case 'clear':
      return 'good'
    case 'watch':
      return 'warn'
    case 'blocked':
      return 'bad'
    default:
      return 'neutral'
  }
}

function statusTone(status: string): 'good' | 'warn' | 'bad' | 'info' | 'neutral' {
  switch (status.toLowerCase()) {
    case 'requested':
      return 'warn'
    case 'approved':
    case 'reserved':
      return 'info'
    case 'checked_out':
    case 'in_use':
    case 'returned':
    case 'inspection':
      return 'warn'
    case 'closed':
      return 'good'
    case 'canceled':
    case 'no_show':
      return 'neutral'
    default:
      return 'neutral'
  }
}

function actionPayload(
  notes: string,
  chargeNotes: string,
  meterReading: string,
): { notes: string | null; chargeNotes: string | null; meterReading: number | null; occurredAt: string | null } {
  const normalizedNotes = notes.trim()
  const normalizedChargeNotes = chargeNotes.trim()
  const parsedMeterReading = meterReading.trim() ? Number(meterReading) : null
  return {
    notes: normalizedNotes || null,
    chargeNotes: normalizedChargeNotes || null,
    meterReading: parsedMeterReading != null && Number.isFinite(parsedMeterReading) ? parsedMeterReading : null,
    occurredAt: new Date().toISOString(),
  }
}

export function AssetReservationPanel({
  accessToken,
  assetId,
  assetTag,
  assetName,
  readiness,
  canRequest,
  canManage,
}: AssetReservationPanelProps) {
  const queryClient = useQueryClient()
  const [statusFilter, setStatusFilter] = useState<'active' | 'all' | (typeof RESERVATION_STATUSES)[number]>('active')
  const [selectedReservationId, setSelectedReservationId] = useState('')
  const [panelError, setPanelError] = useState<string | null>(null)
  const [actionNotes, setActionNotes] = useState('')
  const [actionChargeNotes, setActionChargeNotes] = useState('')
  const [actionMeterReading, setActionMeterReading] = useState('')
  const [draft, setDraft] = useState<ReservationDraft>(createReservationDraft)

  const reservationsQuery = useQuery({
    queryKey: ['maintainarr-asset-reservations', accessToken, assetId, statusFilter],
    queryFn: () =>
      getAssetReservations(accessToken, {
        assetId,
        status: statusFilter === 'active' || statusFilter === 'all' ? undefined : statusFilter,
        activeOnly: statusFilter === 'active' ? true : undefined,
        limit: 12,
      }),
    enabled: Boolean(accessToken && assetId),
  })

  const assetSearchQuery = useQuery({
    queryKey: ['maintainarr-asset-reservation-assets', accessToken],
    queryFn: () => searchAssets(accessToken, undefined, 100),
    enabled: Boolean(accessToken),
  })

  const sitesQuery = useQuery({
    queryKey: ['maintainarr-reservation-sites', accessToken],
    queryFn: () => getSites(accessToken),
    enabled: Boolean(accessToken),
  })

  const peopleQuery = useQuery({
    queryKey: ['maintainarr-reservation-people', accessToken],
    queryFn: () => getPeople(accessToken),
    enabled: Boolean(accessToken),
  })

  const siteOptions = useMemo(() => toPickerOptions(sitesQuery.data), [sitesQuery.data])
  const peopleOptions = useMemo(() => toPickerOptions(peopleQuery.data), [peopleQuery.data])
  const blockerCount = readiness?.blockers.length ?? 0
  const currentAsset = useMemo(
    () => assetSearchQuery.data?.find((item) => item.assetId === assetId) ?? null,
    [assetId, assetSearchQuery.data],
  )
  const suggestionCandidates = useMemo(() => {
    const shouldSuggestAlternatives = (readiness?.readinessStatus ?? 'ready') !== 'ready' || blockerCount > 0
    if (!shouldSuggestAlternatives || !currentAsset) {
      return []
    }

    return (assetSearchQuery.data ?? [])
      .filter((asset) =>
        asset.assetId !== assetId
        && asset.lifecycleStatus !== 'disposed'
        && asset.readinessStatus === 'ready'
        && asset.openWorkOrderCount === 0
        && asset.openDefectCount === 0,
      )
      .map((asset) => {
        const reasons = [
          asset.typeKey === currentAsset.typeKey ? 'same type' : null,
          asset.classKey === currentAsset.classKey ? 'same class' : null,
          currentAsset.siteRef && asset.siteRef === currentAsset.siteRef ? 'same site' : null,
        ].filter((reason): reason is string => Boolean(reason))

        if (reasons.length === 0) {
          return null
        }

        const score =
          (asset.typeKey === currentAsset.typeKey ? 3 : 0)
          + (asset.siteRef && asset.siteRef === currentAsset.siteRef ? 2 : 0)
          + (asset.classKey === currentAsset.classKey ? 1 : 0)

        return {
          asset,
          reasons: Array.from(new Set(reasons)),
          score,
        }
      })
      .filter((candidate): candidate is { asset: AssetSearchResponse; reasons: string[]; score: number } => Boolean(candidate))
      .sort((left, right) => right.score - left.score || left.asset.assetTag.localeCompare(right.asset.assetTag))
      .slice(0, 3)
  }, [assetId, assetSearchQuery.data, blockerCount, currentAsset, readiness?.readinessStatus])
  const selectedReservation = reservationsQuery.data?.find((item) => item.reservationId === selectedReservationId)
    ?? reservationsQuery.data?.[0]
    ?? null
  const usageMeterDelta =
    selectedReservation?.checkOutMeterReading != null && selectedReservation?.returnMeterReading != null
      ? selectedReservation.returnMeterReading - selectedReservation.checkOutMeterReading
      : null

  useEffect(() => {
    setDraft(createReservationDraft())
    setSelectedReservationId('')
    setPanelError(null)
    setActionNotes('')
    setActionChargeNotes('')
    setActionMeterReading('')
  }, [assetId])

  useEffect(() => {
    const reservations = reservationsQuery.data ?? []
    if (reservations.length === 0) {
      if (selectedReservationId) {
        setSelectedReservationId('')
      }
      return
    }

    if (!selectedReservationId || !reservations.some((item) => item.reservationId === selectedReservationId)) {
      setSelectedReservationId(reservations[0].reservationId)
    }
  }, [reservationsQuery.data, selectedReservationId])

  const createMutation = useMutation({
    mutationFn: () =>
      createAssetReservation(accessToken, assetId, {
        purpose: draft.purpose.trim(),
        requestedStartAt: new Date(draft.requestedStartAt).toISOString(),
        requestedEndAt: new Date(draft.requestedEndAt).toISOString(),
        pickupLocationRef: draft.pickupLocationRef,
        returnLocationRef: draft.returnLocationRef.trim() || null,
        operatorPersonId: draft.operatorPersonId,
        driverPersonId: draft.driverPersonId.trim() || null,
        capacityNotes: draft.capacityNotes.trim() || null,
        equipmentNotes: draft.equipmentNotes.trim() || null,
        notes: draft.notes.trim() || null,
      } satisfies CreateAssetReservationRequest),
    onError: (error) => {
      setPanelError(error instanceof Error ? error.message : 'Failed to create reservation request.')
    },
    onSuccess: async (created) => {
      setPanelError(null)
      setSelectedReservationId(created.reservationId)
      setDraft(createReservationDraft())
      setActionNotes('')
      setActionChargeNotes('')
      setActionMeterReading('')
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-reservations', accessToken, assetId] })
    },
  })

  const actionMutation = useMutation({
    mutationFn: (payload: { reservationId: string; action: AssetReservationAction }) =>
      updateAssetReservation(
        accessToken,
        payload.reservationId,
        payload.action,
        actionPayload(actionNotes, actionChargeNotes, actionMeterReading),
      ),
    onError: (error) => {
      setPanelError(error instanceof Error ? error.message : 'Failed to update reservation.')
    },
    onSuccess: async (updated) => {
      setPanelError(null)
      setActionNotes('')
      setActionChargeNotes('')
      setActionMeterReading('')
      setSelectedReservationId(updated.reservationId)
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-reservations', accessToken, assetId] })
    },
  })

  const reservationCount = reservationsQuery.data?.length ?? 0
  const activeCount = reservationsQuery.data?.filter((item) => !['closed', 'canceled', 'no_show'].includes(item.status)).length ?? 0

  const canSubmitCreate =
    draft.purpose.trim().length > 0
    && draft.pickupLocationRef.trim().length > 0
    && draft.operatorPersonId.trim().length > 0
    && !Number.isNaN(Date.parse(draft.requestedStartAt))
    && !Number.isNaN(Date.parse(draft.requestedEndAt))
    && Date.parse(draft.requestedStartAt) < Date.parse(draft.requestedEndAt)

  return (
    <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5" data-testid="asset-reservation-panel">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-white">Asset reservations and motor pool</h2>
          <p className="mt-1 text-sm text-slate-400">
            Request, approve, reserve, hand off, and close reservations for {assetTag} · {assetName}.
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <Badge
            label={readiness?.readinessStatus === 'ready' ? 'Ready' : 'Not ready'}
            tone={readiness?.readinessStatus === 'ready' ? 'good' : 'warn'}
          />
          <Badge label={`${reservationCount} reservations`} tone="info" />
        </div>
      </div>

      {panelError ? (
        <div className="mt-4 rounded-xl border border-rose-800/60 bg-rose-950/30 p-3 text-sm text-rose-100" role="alert">
          {panelError}
        </div>
      ) : null}

      {readiness ? (
        <div className="mt-4 rounded-xl border border-slate-800 bg-slate-950/70 p-4">
          <div className="flex flex-wrap items-center gap-2">
            <Badge
              label={readiness.readinessStatus === 'ready' ? 'Dispatch ready' : 'Dispatch blocked'}
              tone={readiness.readinessStatus === 'ready' ? 'good' : 'bad'}
            />
            <span className="text-sm text-slate-300">{humanize(readiness.readinessBasis)}</span>
          </div>
          <div className="mt-3 grid gap-2 text-sm text-slate-300 sm:grid-cols-3">
            <div className="rounded-lg border border-slate-800 bg-slate-900/60 p-3">
              <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Open defects</p>
              <p className="mt-1 text-lg font-semibold text-white">{readiness.signals.openCriticalDefectCount + readiness.signals.openHighDefectCount}</p>
            </div>
            <div className="rounded-lg border border-slate-800 bg-slate-900/60 p-3">
              <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">PM overdue</p>
              <p className="mt-1 text-lg font-semibold text-white">{readiness.signals.pmOverdueCount}</p>
            </div>
            <div className="rounded-lg border border-slate-800 bg-slate-900/60 p-3">
              <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Failed inspections</p>
              <p className="mt-1 text-lg font-semibold text-white">{readiness.signals.failedInspectionCount}</p>
            </div>
          </div>
          {blockerCount > 0 ? (
            <ul className="mt-3 space-y-2">
              {readiness.blockers.map((blocker) => (
                <li key={`${blocker.blockerType}-${blocker.sourceEntityId}`} className="rounded-lg border border-amber-500/30 bg-amber-500/5 p-3 text-sm text-amber-100">
                  <div className="font-medium">{blocker.blockerType}</div>
                  <p className="mt-1 text-amber-200/90">{blocker.message}</p>
                </li>
              ))}
            </ul>
          ) : (
            <p className="mt-3 text-sm text-emerald-300">No readiness blockers are currently holding this asset out of service.</p>
          )}
        </div>
      ) : null}

      {suggestionCandidates.length > 0 ? (
        <div className="mt-4 rounded-xl border border-slate-800 bg-slate-950/70 p-4" data-testid="asset-reservation-alternatives">
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h3 className="text-sm font-semibold text-white">Suggested alternatives</h3>
              <p className="mt-1 text-sm text-slate-400">
                These ready assets are a better fit while {assetTag} is blocked or conflicting.
              </p>
            </div>
            <Badge label={`${suggestionCandidates.length} ready option${suggestionCandidates.length === 1 ? '' : 's'}`} tone="info" />
          </div>
          <div className="mt-4 grid gap-3 xl:grid-cols-3">
            {suggestionCandidates.map(({ asset, reasons }) => (
              <article key={asset.assetId} className="rounded-lg border border-slate-800 bg-slate-900/70 p-3">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="font-medium text-white">{asset.assetTag}</p>
                    <p className="text-sm text-slate-300">{asset.name}</p>
                  </div>
                  <Badge label="Ready" tone="good" />
                </div>
                <p className="mt-2 text-xs text-slate-400">
                  {asset.typeName} · {asset.staffarrSiteNameSnapshot}
                </p>
                <div className="mt-2 flex flex-wrap gap-2">
                  {reasons.map((reason) => (
                    <Badge key={`${asset.assetId}-${reason}`} label={humanize(reason)} tone="info" />
                  ))}
                </div>
                <p className="mt-2 text-xs text-slate-400">
                  {asset.openWorkOrderCount} open work order{asset.openWorkOrderCount === 1 ? '' : 's'} · {asset.openDefectCount} open defect{asset.openDefectCount === 1 ? '' : 's'}
                </p>
              </article>
            ))}
          </div>
        </div>
      ) : null}

      <div className="mt-4 flex flex-wrap items-center gap-3">
        <label className="text-sm text-slate-300">
          <span className="block text-[var(--color-text-muted)]">Reservation view</span>
          <select
            className="mt-1 rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
            value={statusFilter}
            onChange={(event) => setStatusFilter(event.target.value as typeof statusFilter)}
          >
            {RESERVATION_STATUS_FILTERS.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </label>
        <div className="rounded-lg border border-slate-800 bg-slate-950/60 px-3 py-2 text-sm text-slate-300">
          <span className="text-[var(--color-text-muted)]">Active</span>
          <span className="ml-2 font-medium text-white">{activeCount}</span>
        </div>
      </div>

      {canRequest ? (
        <form
          className="mt-5 rounded-2xl border border-slate-800 bg-slate-950/50 p-4"
          data-testid="asset-reservation-request-form"
          onSubmit={(event) => {
            event.preventDefault()
            if (!canSubmitCreate || createMutation.isPending) {
              return
            }
            createMutation.mutate()
          }}
        >
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h3 className="text-sm font-semibold text-white">Request reservation</h3>
              <p className="mt-1 text-xs text-slate-400">
                Capture the purpose, window, locations, and operator before the reservation is approved or reserved.
              </p>
            </div>
            <Badge label="Worker request" tone="info" />
          </div>

          <div className="mt-4 grid gap-3 md:grid-cols-2">
            <label className="grid gap-1 text-sm text-slate-300 md:col-span-2">
              Purpose
              <textarea
                className="min-h-20 rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                value={draft.purpose}
                onChange={(event) => setDraft((current) => ({ ...current, purpose: event.target.value }))}
                placeholder="Haul equipment to a job site, loan to a technician, etc."
              />
            </label>
            <label className="grid gap-1 text-sm text-slate-300">
              Requested start
              <input
                type="datetime-local"
                className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                value={draft.requestedStartAt}
                onChange={(event) => setDraft((current) => ({ ...current, requestedStartAt: event.target.value }))}
              />
            </label>
            <label className="grid gap-1 text-sm text-slate-300">
              Requested end
              <input
                type="datetime-local"
                className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                value={draft.requestedEndAt}
                onChange={(event) => setDraft((current) => ({ ...current, requestedEndAt: event.target.value }))}
              />
            </label>
            <div className="md:col-span-2">
              <StaticSearchPicker
                id="reservation-pickup-location"
                label="Pickup location"
                value={draft.pickupLocationRef}
                onChange={(value) => setDraft((current) => ({ ...current, pickupLocationRef: value }))}
                options={siteOptions}
                selectedOption={pickerOptionForValue(siteOptions, draft.pickupLocationRef)}
                placeholder="Search sites"
                testId="reservation-pickup-location"
              />
            </div>
            <div className="md:col-span-2">
              <StaticSearchPicker
                id="reservation-return-location"
                label="Return location"
                value={draft.returnLocationRef}
                onChange={(value) => setDraft((current) => ({ ...current, returnLocationRef: value }))}
                options={siteOptions}
                selectedOption={pickerOptionForValue(siteOptions, draft.returnLocationRef)}
                placeholder="Search sites"
                testId="reservation-return-location"
              />
            </div>
            <div className="md:col-span-2">
              <StaticSearchPicker
                id="reservation-operator"
                label="Operator"
                value={draft.operatorPersonId}
                onChange={(value) => setDraft((current) => ({ ...current, operatorPersonId: value }))}
                options={peopleOptions}
                selectedOption={pickerOptionForValue(peopleOptions, draft.operatorPersonId)}
                placeholder="Search people"
                testId="reservation-operator"
              />
            </div>
            <div className="md:col-span-2">
              <StaticSearchPicker
                id="reservation-driver"
                label="Driver"
                value={draft.driverPersonId}
                onChange={(value) => setDraft((current) => ({ ...current, driverPersonId: value }))}
                options={peopleOptions}
                selectedOption={pickerOptionForValue(peopleOptions, draft.driverPersonId)}
                placeholder="Search people"
                testId="reservation-driver"
              />
            </div>
            <label className="grid gap-1 text-sm text-slate-300 md:col-span-2">
              Capacity notes
              <textarea
                className="min-h-16 rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                value={draft.capacityNotes}
                onChange={(event) => setDraft((current) => ({ ...current, capacityNotes: event.target.value }))}
                placeholder="Weight, passenger, attachment, or cargo notes"
              />
            </label>
            <label className="grid gap-1 text-sm text-slate-300 md:col-span-2">
              Equipment notes
              <textarea
                className="min-h-16 rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                value={draft.equipmentNotes}
                onChange={(event) => setDraft((current) => ({ ...current, equipmentNotes: event.target.value }))}
                placeholder="Keys, fuel card, charger, trailer, and other handoff items"
              />
            </label>
            <label className="grid gap-1 text-sm text-slate-300 md:col-span-2">
              Notes
              <textarea
                className="min-h-16 rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                value={draft.notes}
                onChange={(event) => setDraft((current) => ({ ...current, notes: event.target.value }))}
                placeholder="Optional request notes"
              />
            </label>
          </div>

          <div className="mt-3 flex flex-wrap items-center gap-3">
            <button
              type="submit"
              className="rounded-xl bg-sky-600 px-4 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:opacity-50"
              disabled={!canSubmitCreate || createMutation.isPending}
            >
              {createMutation.isPending ? 'Requesting...' : 'Request reservation'}
            </button>
            <p className="text-xs text-slate-400">
              Pickup location and operator are required before submission.
            </p>
          </div>
        </form>
      ) : null}

      {reservationsQuery.isLoading ? (
        <p className="mt-5 text-sm text-slate-400">Loading reservations...</p>
      ) : reservationCount === 0 ? (
        <p className="mt-5 text-sm text-slate-400">
          No reservations match the current filter yet.
        </p>
      ) : (
        <div className="mt-5 overflow-x-auto" data-testid="asset-reservation-list">
          <table className="min-w-full text-left text-sm">
            <thead className="border-b border-slate-800 text-slate-400">
              <tr>
                <th className="px-3 py-2 font-medium">Number</th>
                <th className="px-3 py-2 font-medium">Status</th>
                <th className="px-3 py-2 font-medium">Window</th>
                <th className="px-3 py-2 font-medium">Purpose</th>
                <th className="px-3 py-2 font-medium">Decision</th>
              </tr>
            </thead>
            <tbody>
              {reservationsQuery.data?.map((reservation) => (
                <tr
                  key={reservation.reservationId}
                  className={`border-b border-slate-900 text-slate-200 ${
                    reservation.reservationId === selectedReservation?.reservationId ? 'bg-sky-950/30' : ''
                  }`}
                  data-testid={`asset-reservation-row-${reservation.reservationId}`}
                >
                  <td className="px-3 py-2">
                    <button
                      type="button"
                      className="font-medium text-sky-100 hover:underline"
                      onClick={() => setSelectedReservationId(reservation.reservationId)}
                    >
                      {reservation.reservationNumber}
                    </button>
                  </td>
                  <td className="px-3 py-2">
                    <Badge label={humanize(reservation.status)} tone={statusTone(reservation.status)} />
                  </td>
                  <td className="px-3 py-2">
                    <div className="space-y-1">
                      <div className="flex items-center gap-2 text-slate-100">
                        <CalendarClock className="h-4 w-4 text-slate-400" />
                        <span>{formatDateTime(reservation.requestedStartAt)}</span>
                      </div>
                      <div className="flex items-center gap-2 text-slate-400">
                        <Clock3 className="h-4 w-4" />
                        <span>to {formatDateTime(reservation.requestedEndAt)}</span>
                      </div>
                    </div>
                  </td>
                  <td className="px-3 py-2">
                    <div className="max-w-[24rem]">
                      <p className="font-medium text-white">{reservation.purpose}</p>
                      <p className="mt-1 text-xs text-slate-400">
                        <MapPin className="mr-1 inline h-3.5 w-3.5" />
                        {reservation.pickupLocationNameSnapshot ?? reservation.pickupLocationRef ?? 'No pickup location'}
                      </p>
                    </div>
                  </td>
                  <td className="px-3 py-2">
                    <Badge label={humanize(reservation.decisionStatus)} tone={decisionTone(reservation.decisionStatus)} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {selectedReservation ? (
        <div className="mt-5 rounded-2xl border border-slate-800 bg-slate-950/70 p-4" data-testid="asset-reservation-detail">
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h3 className="text-base font-semibold text-white">
                {selectedReservation.reservationNumber} - {selectedReservation.purpose}
              </h3>
              <p className="mt-1 text-sm text-slate-400">
                {selectedReservation.assetTag} · {selectedReservation.assetName}
              </p>
            </div>
            <div className="flex flex-wrap items-center gap-2">
              <Badge label={humanize(selectedReservation.status)} tone={statusTone(selectedReservation.status)} />
              <Badge label={humanize(selectedReservation.decisionStatus)} tone={decisionTone(selectedReservation.decisionStatus)} />
            </div>
          </div>

          <div className="mt-4 grid gap-4 lg:grid-cols-3">
            <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4 lg:col-span-2">
              <h4 className="text-sm font-semibold text-white">Reservation details</h4>
              <dl className="mt-3 grid gap-3 text-sm sm:grid-cols-2">
                <div>
                  <dt className="text-[var(--color-text-muted)]">Requested by</dt>
                  <dd className="text-slate-200">
                    {selectedReservation.requestedByDisplayNameSnapshot ?? selectedReservation.requestedByPersonId ?? 'Not set'}
                  </dd>
                </div>
                <div>
                  <dt className="text-[var(--color-text-muted)]">Operator</dt>
                  <dd className="text-slate-200">
                    {selectedReservation.operatorDisplayNameSnapshot ?? selectedReservation.operatorPersonId ?? 'Not set'}
                  </dd>
                </div>
                <div>
                  <dt className="text-[var(--color-text-muted)]">Driver</dt>
                  <dd className="text-slate-200">
                    {selectedReservation.driverDisplayNameSnapshot ?? selectedReservation.driverPersonId ?? 'Not set'}
                  </dd>
                </div>
                <div>
                  <dt className="text-[var(--color-text-muted)]">Pickup / return</dt>
                  <dd className="text-slate-200">
                    {selectedReservation.pickupLocationNameSnapshot ?? selectedReservation.pickupLocationRef ?? 'Not set'}
                    <span className="text-slate-500"> - </span>
                    {selectedReservation.returnLocationNameSnapshot ?? selectedReservation.returnLocationRef ?? 'Same as pickup'}
                  </dd>
                </div>
                {selectedReservation.capacityNotes ? (
                  <div>
                    <dt className="text-[var(--color-text-muted)]">Capacity notes</dt>
                    <dd className="mt-1 whitespace-pre-wrap text-slate-200">{selectedReservation.capacityNotes}</dd>
                  </div>
                ) : null}
                {selectedReservation.equipmentNotes ? (
                  <div>
                    <dt className="text-[var(--color-text-muted)]">Handoff items</dt>
                    <dd className="mt-1 whitespace-pre-wrap text-slate-200">{selectedReservation.equipmentNotes}</dd>
                  </div>
                ) : null}
                <div>
                  <dt className="text-[var(--color-text-muted)]">Checkout meter</dt>
                  <dd className="text-slate-200">{selectedReservation.checkOutMeterReading ?? 'Not recorded'}</dd>
                </div>
                <div>
                  <dt className="text-[var(--color-text-muted)]">Return meter</dt>
                  <dd className="text-slate-200">{selectedReservation.returnMeterReading ?? 'Not recorded'}</dd>
                </div>
                {usageMeterDelta != null ? (
                  <div className="sm:col-span-2 rounded-lg border border-slate-800 bg-slate-950/60 p-3" data-testid="asset-reservation-usage">
                    <dt className="text-[var(--color-text-muted)]">Usage meter delta</dt>
                    <dd className="mt-1 text-slate-200">
                      {usageMeterDelta.toLocaleString(undefined, { maximumFractionDigits: 2 })}
                    </dd>
                    <p className="mt-1 text-xs text-slate-400">Calculated from return minus checkout meter.</p>
                  </div>
                ) : null}
              </dl>

              <div className="mt-4 grid gap-3 md:grid-cols-2">
                <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
                  <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Decision</p>
                  <p className="mt-1 font-medium text-white">{selectedReservation.decisionSummary}</p>
                  <p className="mt-2 text-sm text-slate-300">{selectedReservation.decisionDetail}</p>
                </div>
                <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
                  <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Timeline</p>
                  <p className="mt-1 font-medium text-white">{selectedReservation.timeline.length} recorded event{selectedReservation.timeline.length === 1 ? '' : 's'}</p>
                  <p className="mt-2 text-sm text-slate-300">
                    {selectedReservation.timeline[0] ? `${humanize(selectedReservation.timeline[0].eventType)} at ${formatDateTime(selectedReservation.timeline[0].occurredAt)}` : 'No timeline events yet.'}
                  </p>
                </div>
              </div>

              {selectedReservation.chargeNotes || selectedReservation.damageNotes || selectedReservation.inspectionNotes || selectedReservation.cancelReason || selectedReservation.noShowReason ? (
                <div className="mt-4 rounded-lg border border-slate-800 bg-slate-950/60 p-3" data-testid="asset-reservation-post-use">
                  <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Post-use context</p>
                  <dl className="mt-3 grid gap-3 text-sm sm:grid-cols-2">
                    {selectedReservation.chargeNotes ? (
                      <div>
                        <dt className="text-[var(--color-text-muted)]">Charge notes</dt>
                        <dd className="mt-1 whitespace-pre-wrap text-slate-200">{selectedReservation.chargeNotes}</dd>
                      </div>
                    ) : null}
                    {selectedReservation.damageNotes ? (
                      <div>
                        <dt className="text-[var(--color-text-muted)]">Damage notes</dt>
                        <dd className="mt-1 whitespace-pre-wrap text-slate-200">{selectedReservation.damageNotes}</dd>
                      </div>
                    ) : null}
                    {selectedReservation.inspectionNotes ? (
                      <div>
                        <dt className="text-[var(--color-text-muted)]">Inspection notes</dt>
                        <dd className="mt-1 whitespace-pre-wrap text-slate-200">{selectedReservation.inspectionNotes}</dd>
                      </div>
                    ) : null}
                    {selectedReservation.cancelReason ? (
                      <div>
                        <dt className="text-[var(--color-text-muted)]">Cancel reason</dt>
                        <dd className="mt-1 whitespace-pre-wrap text-slate-200">{selectedReservation.cancelReason}</dd>
                      </div>
                    ) : null}
                    {selectedReservation.noShowReason ? (
                      <div>
                        <dt className="text-[var(--color-text-muted)]">No-show reason</dt>
                        <dd className="mt-1 whitespace-pre-wrap text-slate-200">{selectedReservation.noShowReason}</dd>
                      </div>
                    ) : null}
                  </dl>
                </div>
              ) : null}
            </div>

            <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
              <h4 className="text-sm font-semibold text-white">Actions</h4>
              {canManage && RESERVATION_ACTIONS[selectedReservation.status] ? (
                <div className="mt-3 space-y-3">
                  <label className="grid gap-1 text-sm text-slate-300">
                    Action notes
                    <textarea
                      className="min-h-20 rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                      value={actionNotes}
                      onChange={(event) => setActionNotes(event.target.value)}
                      placeholder="Optional notes for the state change"
                    />
                  </label>
                  <label className="grid gap-1 text-sm text-slate-300">
                    Charge notes
                    <textarea
                      className="min-h-20 rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                      value={actionChargeNotes}
                      onChange={(event) => setActionChargeNotes(event.target.value)}
                      placeholder="Optional chargeback, cleanup, or billing context"
                    />
                  </label>
                  <label className="grid gap-1 text-sm text-slate-300">
                    Meter reading
                    <input
                      type="number"
                      step="0.1"
                      className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
                      value={actionMeterReading}
                      onChange={(event) => setActionMeterReading(event.target.value)}
                      placeholder="Optional"
                    />
                  </label>
                  <div className="flex flex-wrap gap-2">
                    {RESERVATION_ACTIONS[selectedReservation.status].map((item) => (
                      <button
                        key={item.action}
                        type="button"
                        className="rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-sm font-medium text-white hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-50"
                        disabled={actionMutation.isPending}
                        onClick={() => actionMutation.mutate({ reservationId: selectedReservation.reservationId, action: item.action })}
                      >
                        {item.label}
                      </button>
                    ))}
                  </div>
                </div>
              ) : (
                <p className="mt-3 text-sm text-slate-400">
                  {canManage ? 'No state transitions are available for this reservation status.' : 'You can view reservations, but only coordinators can advance their state.'}
                </p>
              )}

              {selectedReservation.conflicts.length > 0 ? (
                <div className="mt-4">
                  <div className="flex items-center gap-2">
                    <AlertTriangle className="h-4 w-4 text-amber-300" />
                    <h5 className="text-sm font-semibold text-amber-100">Conflicts</h5>
                  </div>
                  <ul className="mt-2 space-y-2 text-sm">
                    {selectedReservation.conflicts.map((conflict) => (
                      <li key={conflict.conflictingReservationId} className="rounded-lg border border-amber-500/30 bg-amber-500/5 p-3 text-amber-100">
                        <div className="font-medium">{conflict.conflictingReservationNumber}</div>
                        <p className="mt-1 text-amber-200/90">{conflict.message}</p>
                      </li>
                    ))}
                  </ul>
                </div>
              ) : null}

              {selectedReservation.qualificationChecks.length > 0 ? (
                <div className="mt-4">
                  <div className="flex items-center gap-2">
                    <UsersRound className="h-4 w-4 text-sky-300" />
                    <h5 className="text-sm font-semibold text-sky-100">Qualification checks</h5>
                  </div>
                  <ul className="mt-2 space-y-2 text-sm">
                    {selectedReservation.qualificationChecks.map((check) => (
                      <li key={`${check.role}-${check.personId ?? check.qualificationKey}`} className="rounded-lg border border-slate-800 bg-slate-950/60 p-3 text-slate-200">
                        <div className="font-medium">
                          {humanize(check.role)} - {check.outcome}
                        </div>
                        <p className="mt-1 text-slate-400">
                          {check.personDisplayName ?? check.personId ?? 'Unknown person'} - {check.message}
                        </p>
                      </li>
                    ))}
                  </ul>
                </div>
              ) : null}
            </div>
          </div>

          <div className="mt-4">
            <h4 className="text-sm font-semibold text-white">Timeline</h4>
            {selectedReservation.timeline.length === 0 ? (
              <p className="mt-2 text-sm text-slate-400">No timeline events have been recorded yet.</p>
            ) : (
              <ul className="mt-3 space-y-2">
                {selectedReservation.timeline.map((event) => (
                  <li key={event.eventId} className="rounded-lg border border-slate-800 bg-slate-950/60 p-3 text-sm text-slate-200">
                    <div className="flex flex-wrap items-center gap-2">
                      <Badge label={humanize(event.eventType)} tone="info" />
                      <span className="text-slate-400">{formatDateTime(event.occurredAt)}</span>
                    </div>
                    <p className="mt-1 font-medium text-white">
                      {humanize(event.fromStatus)} - {humanize(event.toStatus)}
                    </p>
                    <p className="mt-1 text-slate-300">{event.message}</p>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      ) : null}
    </section>
  )
}
