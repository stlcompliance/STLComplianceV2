import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { AdvancedReferenceField, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'

import { applyBulkDispatch, listDrivers, listVehicleRefs, getTrips, previewBulkDispatch } from '../api/client'
import type { BulkDispatchItemPreview, TripSummaryResponse } from '../api/types'
import {
  driverToPickerOption,
  findDriverLabel,
  findVehicleLabel,
  vehicleRefToPickerOption,
} from '../lib/referencePickers'
import {
  buildBulkDispatchPreviewResponse,
  confirmBulkDispatchPreview,
  formatBulkDispatchItemSummary,
} from '../lib/bulkDispatch'
import { DispatchAssignmentGateDetails } from './DispatchAssignmentGateDetails'

type BulkDispatchPanelProps = {
  accessToken: string
  canAssign: boolean
}

const ACTIVE_STATUSES = new Set(['planned', 'assigned', 'dispatched', 'in_progress'])
const STATUS_OPTIONS = ['', 'assigned', 'dispatched', 'in_progress', 'completed', 'cancelled']

export function BulkDispatchPanel({ accessToken, canAssign }: BulkDispatchPanelProps) {
  const queryClient = useQueryClient()
  const [selectedTripIds, setSelectedTripIds] = useState<Set<string>>(new Set())
  const [driverPersonId, setDriverPersonId] = useState('')
  const [vehicleRefKey, setVehicleRefKey] = useState('')
  const [dispatchStatus, setDispatchStatus] = useState('')
  const [previewItems, setPreviewItems] = useState<BulkDispatchItemPreview[] | null>(null)
  const [statusMessage, setStatusMessage] = useState<string | null>(null)

  const tripsQuery = useQuery({
    queryKey: ['routarr-trips-bulk', accessToken],
    queryFn: () => getTrips(accessToken),
    enabled: canAssign,
  })

  const driversQuery = useQuery({
    queryKey: ['routarr-drivers-bulk', accessToken],
    queryFn: () => listDrivers(accessToken),
    enabled: canAssign,
  })

  const vehicleRefsQuery = useQuery({
    queryKey: ['routarr-vehicle-refs-bulk', accessToken],
    queryFn: () => listVehicleRefs(accessToken),
    enabled: canAssign,
  })

  const driverOptions = useMemo(
    () => (driversQuery.data?.items ?? []).map(driverToPickerOption),
    [driversQuery.data],
  )
  const vehicleOptions = useMemo(
    () => (vehicleRefsQuery.data?.items ?? []).map(vehicleRefToPickerOption),
    [vehicleRefsQuery.data],
  )

  const selectedDriverOption = useMemo((): PickerOption | undefined => {
    const label = findDriverLabel(driversQuery.data?.items ?? [], driverPersonId)
    return label ? { value: driverPersonId, label } : undefined
  }, [driverPersonId, driversQuery.data])

  const selectedVehicleOption = useMemo((): PickerOption | undefined => {
    const label = findVehicleLabel(vehicleRefsQuery.data?.items ?? [], vehicleRefKey)
    return label ? { value: vehicleRefKey, label } : undefined
  }, [vehicleRefKey, vehicleRefsQuery.data])

  const assignableTrips = useMemo(
    () =>
      (tripsQuery.data ?? []).filter((trip) => ACTIVE_STATUSES.has(trip.dispatchStatus.toLowerCase())),
    [tripsQuery.data],
  )

  const previewMutation = useMutation({
    mutationFn: async () => {
      const items = buildActionItems(
        selectedTripIds,
        driverPersonId,
        vehicleRefKey,
        dispatchStatus,
      )
      return previewBulkDispatch(accessToken, { items })
    },
    onSuccess: (response) => {
      setPreviewItems(response.items)
      setStatusMessage(
        `Preview: ${response.summary.canApplyCount}/${response.summary.total} trips ready to apply.`,
      )
    },
    onError: (error: Error) => {
      setStatusMessage(error.message)
      setPreviewItems(null)
    },
  })

  const applyMutation = useMutation({
    mutationFn: async (ignoreFlags: {
      ignoreAvailabilityConflicts: boolean
      ignoreEligibilityBlocks: boolean
      ignoreDispatchabilityBlocks: boolean
      ignoreWorkflowGateBlocks: boolean
    }) => {
      const items = buildActionItems(
        selectedTripIds,
        driverPersonId,
        vehicleRefKey,
        dispatchStatus,
      )
      return applyBulkDispatch(accessToken, { items, ...ignoreFlags })
    },
    onSuccess: async (response) => {
      await queryClient.invalidateQueries({ queryKey: ['routarr-trips'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-trips-bulk'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-trips-assignment'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-dispatch-board'] })
      setStatusMessage(
        `Applied ${response.summary.successCount}/${response.summary.total} trip updates.`,
      )
      setPreviewItems(null)
    },
    onError: (error: Error) => {
      setStatusMessage(error.message)
    },
  })

  function toggleTrip(tripId: string) {
    setSelectedTripIds((current) => {
      const next = new Set(current)
      if (next.has(tripId)) {
        next.delete(tripId)
      } else {
        next.add(tripId)
      }
      return next
    })
    setPreviewItems(null)
  }

  function toggleAllTrips() {
    if (selectedTripIds.size === assignableTrips.length) {
      setSelectedTripIds(new Set())
    } else {
      setSelectedTripIds(new Set(assignableTrips.map((trip) => trip.tripId)))
    }
    setPreviewItems(null)
  }

  async function handlePreview() {
    if (!canAssign || selectedTripIds.size === 0) return
    if (!hasBulkAction(driverPersonId, vehicleRefKey, dispatchStatus)) {
      setStatusMessage('Enter a driver, vehicle, or status to apply to selected trips.')
      return
    }
    await previewMutation.mutateAsync()
  }

  async function handleApply() {
    if (!canAssign || selectedTripIds.size === 0) return
    if (!hasBulkAction(driverPersonId, vehicleRefKey, dispatchStatus)) {
      setStatusMessage('Enter a driver, vehicle, or status to apply to selected trips.')
      return
    }

    let previewResponse = previewItems
      ? buildBulkDispatchPreviewResponse(previewItems)
      : null

    if (!previewResponse) {
      previewResponse = await previewMutation.mutateAsync()
    }

    const ignoreFlags = confirmBulkDispatchPreview(previewResponse, (message) =>
      window.confirm(message),
    )
    if (!ignoreFlags) {
      setStatusMessage('Bulk apply cancelled.')
      return
    }

    await applyMutation.mutateAsync(ignoreFlags)
  }

  if (!canAssign) {
    return null
  }

  if (tripsQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading bulk dispatch workspace…</p>
  }

  return (
    <section className="space-y-6" aria-label="Bulk dispatch" data-testid="bulk-dispatch-panel">
      <div className="rounded-xl border border-slate-700 bg-slate-900/80 p-5">
        <h2 className="text-lg font-semibold text-slate-50">Bulk dispatch</h2>
        <p className="mt-1 text-sm text-slate-400">
          Select multiple trips and batch-assign drivers, vehicles, or dispatch status. Preview
          conflicts — including driver eligibility, asset dispatchability, and Compliance Core
          workflow gates — before applying.
        </p>
        {statusMessage ? (
          <p className="mt-3 text-sm text-amber-200" data-testid="bulk-dispatch-status">
            {statusMessage}
          </p>
        ) : null}
      </div>

      <div className="rounded-xl border border-slate-700 bg-slate-900/80 p-5">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <h3 className="text-sm font-medium text-slate-300">Active trips</h3>
          <button
            type="button"
            className="text-xs text-sky-300 hover:text-sky-200"
            onClick={toggleAllTrips}
          >
            {selectedTripIds.size === assignableTrips.length ? 'Clear selection' : 'Select all'}
          </button>
        </div>
        {assignableTrips.length === 0 ? (
          <p className="mt-3 text-sm text-slate-500">No active trips available.</p>
        ) : (
          <ul className="mt-3 space-y-2">
            {assignableTrips.map((trip) => (
              <TripSelectRow
                key={trip.tripId}
                trip={trip}
                selected={selectedTripIds.has(trip.tripId)}
                onToggle={() => toggleTrip(trip.tripId)}
              />
            ))}
          </ul>
        )}
      </div>

      <div className="rounded-xl border border-slate-700 bg-slate-900/80 p-5">
        <h3 className="text-sm font-medium text-slate-300">Batch actions</h3>
        <p className="mt-1 text-xs text-slate-500">
          Leave fields blank to skip that change. Vehicle field uses empty to clear assignment.
        </p>
        <div className="mt-4 grid gap-4 md:grid-cols-3">
          <div>
            <StaticSearchPicker
              label="Driver"
              value={driverPersonId}
              onChange={(value) => {
                setDriverPersonId(value)
                setPreviewItems(null)
              }}
              options={driverOptions}
              selectedOption={selectedDriverOption}
              placeholder="No change"
              disabled={driversQuery.isLoading}
              testId="bulk-dispatch-driver-picker"
            />
            <AdvancedReferenceField
              value={driverPersonId}
              onChange={(value) => {
                setDriverPersonId(value)
                setPreviewItems(null)
              }}
              label="Driver person id"
              testId="bulk-dispatch-driver-advanced"
            />
          </div>
          <div>
            <StaticSearchPicker
              label="Vehicle"
              value={vehicleRefKey}
              onChange={(value) => {
                setVehicleRefKey(value)
                setPreviewItems(null)
              }}
              options={vehicleOptions}
              selectedOption={selectedVehicleOption}
              placeholder="No change"
              disabled={vehicleRefsQuery.isLoading}
              testId="bulk-dispatch-vehicle-picker"
            />
            <AdvancedReferenceField
              value={vehicleRefKey}
              onChange={(value) => {
                setVehicleRefKey(value)
                setPreviewItems(null)
              }}
              label="Vehicle ref key"
              testId="bulk-dispatch-vehicle-advanced"
            />
          </div>
          <label className="block text-xs text-slate-400">
            Dispatch status
            <select
              value={dispatchStatus}
              onChange={(event) => {
                setDispatchStatus(event.target.value)
                setPreviewItems(null)
              }}
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm text-slate-100"
            >
              {STATUS_OPTIONS.map((status) => (
                <option key={status || 'none'} value={status}>
                  {status ? status.replace('_', ' ') : 'No change'}
                </option>
              ))}
            </select>
          </label>
        </div>
        <div className="mt-4 flex flex-wrap gap-3">
          <button
            type="button"
            className="rounded bg-slate-700 px-3 py-1.5 text-sm text-slate-100 hover:bg-slate-600 disabled:opacity-50"
            disabled={selectedTripIds.size === 0 || previewMutation.isPending}
            onClick={() => void handlePreview()}
            data-testid="bulk-dispatch-preview"
          >
            {previewMutation.isPending ? 'Previewing…' : 'Preview conflicts'}
          </button>
          <button
            type="button"
            className="rounded bg-sky-700 px-3 py-1.5 text-sm text-white hover:bg-sky-600 disabled:opacity-50"
            disabled={selectedTripIds.size === 0 || applyMutation.isPending}
            onClick={() => void handleApply()}
            data-testid="bulk-dispatch-apply"
          >
            {applyMutation.isPending ? 'Applying…' : 'Apply to selected'}
          </button>
        </div>
      </div>

      {previewItems ? (
        <div className="rounded-xl border border-slate-700 bg-slate-900/80 p-5">
          <h3 className="text-sm font-medium text-slate-300">Preview results</h3>
          <ul className="mt-3 space-y-2">
            {previewItems.map((item) => (
              <li
                key={item.tripId}
                className={`rounded border p-3 text-sm ${
                  item.canApply
                    ? 'border-emerald-700 text-emerald-100'
                    : 'border-amber-700 text-amber-100'
                }`}
                data-testid={`bulk-preview-${item.tripId}`}
              >
                <p className="font-medium">
                  {item.title} ({item.tripNumber})
                </p>
                <p className="text-xs opacity-80" data-testid={`bulk-preview-summary-${item.tripId}`}>
                  {formatBulkDispatchItemSummary(item)}
                </p>
                {item.driverPreview ? (
                  <DispatchAssignmentGateDetails
                    preview={item.driverPreview}
                    title="Driver assignment gates"
                    compact
                    data-testid={`bulk-preview-driver-gates-${item.tripId}`}
                  />
                ) : null}
                {item.vehiclePreview ? (
                  <DispatchAssignmentGateDetails
                    preview={item.vehiclePreview}
                    title="Vehicle assignment gates"
                    compact
                    data-testid={`bulk-preview-vehicle-gates-${item.tripId}`}
                  />
                ) : null}
              </li>
            ))}
          </ul>
        </div>
      ) : null}
    </section>
  )
}

function TripSelectRow({
  trip,
  selected,
  onToggle,
}: {
  trip: TripSummaryResponse
  selected: boolean
  onToggle: () => void
}) {
  return (
    <li className="flex items-start gap-3 rounded border border-slate-700 p-3">
      <input
        type="checkbox"
        checked={selected}
        onChange={onToggle}
        data-testid={`bulk-trip-${trip.tripId}`}
        aria-label={`Select ${trip.title}`}
      />
      <div>
        <p className="text-sm text-slate-100">{trip.title}</p>
        <p className="text-xs text-slate-500">
          {trip.tripNumber} · {trip.dispatchStatus.replace('_', ' ')}
        </p>
      </div>
    </li>
  )
}

function hasBulkAction(driverPersonId: string, vehicleRefKey: string, dispatchStatus: string) {
  return (
    driverPersonId.trim().length > 0 ||
    vehicleRefKey.length > 0 ||
    dispatchStatus.trim().length > 0
  )
}

function buildActionItems(
  selectedTripIds: Set<string>,
  driverPersonId: string,
  vehicleRefKey: string,
  dispatchStatus: string,
) {
  const driver = driverPersonId.trim() || null
  const vehicle = vehicleRefKey.length > 0 ? vehicleRefKey.trim() : null
  const status = dispatchStatus.trim() || null

  return [...selectedTripIds].map((tripId) => ({
    tripId,
    driverPersonId: driver,
    vehicleRefKey: vehicle,
    dispatchStatus: status,
  }))
}
