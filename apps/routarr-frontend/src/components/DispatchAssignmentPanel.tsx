import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import {
  assignTripDriver,
  assignTripVehicle,
  getDriverAvailabilityPanel,
  getEquipmentAvailabilityPanel,
  getTrips,
  previewDispatchAssignment,
} from '../api/client'
import type { DispatchAssignmentPreviewResponse, TripSummaryResponse } from '../api/types'

type DispatchAssignmentPanelProps = {
  accessToken: string
  scope: 'daily' | 'weekly'
  canAssign: boolean
}

type DragPayload =
  | { kind: 'driver'; personId: string }
  | { kind: 'vehicle'; vehicleRefKey: string }

const ACTIVE_STATUSES = new Set(['planned', 'assigned', 'dispatched', 'in_progress'])

function formatShortId(value: string) {
  return value.length > 10 ? `${value.slice(0, 8)}…` : value
}

function formatConflictMessage(preview: DispatchAssignmentPreviewResponse) {
  const parts: string[] = []
  if (preview.blockingDriverAvailability.length > 0) {
    parts.push(`${preview.blockingDriverAvailability.length} driver availability block(s)`)
  }
  if (preview.blockingEquipmentAvailability.length > 0) {
    parts.push(`${preview.blockingEquipmentAvailability.length} equipment availability block(s)`)
  }
  if (preview.overlappingTrips.length > 0) {
    parts.push(`${preview.overlappingTrips.length} overlapping trip(s)`)
  }
  if (preview.driverEligibility?.isBlocking) {
    parts.push(`eligibility: ${preview.driverEligibility.message}`)
  } else if (preview.driverEligibility?.outcome === 'warn') {
    parts.push(`eligibility warning: ${preview.driverEligibility.message}`)
  }
  if (preview.assetDispatchability?.isBlocking) {
    parts.push(`dispatchability: ${preview.assetDispatchability.message}`)
  } else if (preview.assetDispatchability?.outcome === 'warn') {
    parts.push(`dispatchability warning: ${preview.assetDispatchability.message}`)
  }
  if (preview.workflowGates?.isBlocking) {
    parts.push(`workflow gate: ${preview.workflowGates.message}`)
  } else if (preview.workflowGates?.outcome === 'warn') {
    parts.push(`workflow gate warning: ${preview.workflowGates.message}`)
  }
  return parts.join(', ')
}

function TripDropTarget({
  trip,
  canAssign,
  isAssigning,
  onDropAssignment,
}: {
  trip: TripSummaryResponse
  canAssign: boolean
  isAssigning: boolean
  onDropAssignment: (tripId: string, payload: DragPayload) => void
}) {
  const [dragOver, setDragOver] = useState(false)

  return (
    <li
      className={`rounded-lg border p-3 transition-colors ${
        dragOver ? 'border-sky-500 bg-sky-950/30' : 'border-slate-700 bg-slate-900/40'
      }`}
      onDragOver={(event) => {
        if (!canAssign) return
        event.preventDefault()
        setDragOver(true)
      }}
      onDragLeave={() => setDragOver(false)}
      onDrop={(event) => {
        event.preventDefault()
        setDragOver(false)
        if (!canAssign) return
        const raw = event.dataTransfer.getData('application/routarr-assignment')
        if (!raw) return
        try {
          const payload = JSON.parse(raw) as DragPayload
          onDropAssignment(trip.tripId, payload)
        } catch {
          // ignore malformed drag payload
        }
      }}
      data-testid={`trip-drop-${trip.tripId}`}
    >
      <p className="text-sm font-medium text-slate-100">{trip.title}</p>
      <p className="text-xs text-slate-500">
        {trip.tripNumber} · {trip.dispatchStatus.replace('_', ' ')}
      </p>
      <p className="mt-2 text-xs text-slate-400">
        {trip.assignedDriverPersonId
          ? `Driver ${formatShortId(trip.assignedDriverPersonId)}`
          : 'No driver'}
        {trip.vehicleRefKey ? ` · Vehicle ${formatShortId(trip.vehicleRefKey)}` : ' · No vehicle'}
      </p>
      {canAssign ? (
        <p className="mt-2 text-xs text-slate-500">
          {isAssigning ? 'Assigning…' : 'Drop a driver or vehicle here'}
        </p>
      ) : null}
    </li>
  )
}

export function DispatchAssignmentPanel({ accessToken, scope, canAssign }: DispatchAssignmentPanelProps) {
  const queryClient = useQueryClient()
  const [statusMessage, setStatusMessage] = useState<string | null>(null)
  const [assigningTripId, setAssigningTripId] = useState<string | null>(null)

  const tripsQuery = useQuery({
    queryKey: ['routarr-trips-assignment', accessToken],
    queryFn: () => getTrips(accessToken),
    enabled: canAssign,
  })

  const driverPanelQuery = useQuery({
    queryKey: ['routarr-driver-availability', accessToken, scope],
    queryFn: () => getDriverAvailabilityPanel(accessToken, scope),
    enabled: canAssign,
  })

  const equipmentPanelQuery = useQuery({
    queryKey: ['routarr-equipment-availability', accessToken, scope],
    queryFn: () => getEquipmentAvailabilityPanel(accessToken, scope),
    enabled: canAssign,
  })

  const assignMutation = useMutation({
    mutationFn: async ({
      tripId,
      payload,
      ignoreConflicts,
      ignoreEligibilityBlocks,
      ignoreDispatchabilityBlocks,
      ignoreWorkflowGateBlocks,
    }: {
      tripId: string
      payload: DragPayload
      ignoreConflicts: boolean
      ignoreEligibilityBlocks: boolean
      ignoreDispatchabilityBlocks: boolean
      ignoreWorkflowGateBlocks: boolean
    }) => {
      if (payload.kind === 'driver') {
        return assignTripDriver(accessToken, tripId, {
          driverPersonId: payload.personId,
          ignoreAvailabilityConflicts: ignoreConflicts,
          ignoreEligibilityBlocks,
          ignoreWorkflowGateBlocks,
        })
      }

      return assignTripVehicle(accessToken, tripId, {
        vehicleRefKey: payload.vehicleRefKey,
        ignoreAvailabilityConflicts: ignoreConflicts,
        ignoreDispatchabilityBlocks,
        ignoreWorkflowGateBlocks,
      })
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['routarr-trips'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-trips-assignment'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-dispatch-board'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-driver-availability'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-equipment-availability'] })
      setStatusMessage('Assignment saved.')
      setAssigningTripId(null)
    },
    onError: (error: Error) => {
      setStatusMessage(error.message)
      setAssigningTripId(null)
    },
  })

  const assignableTrips = useMemo(
    () =>
      (tripsQuery.data ?? []).filter((trip) => ACTIVE_STATUSES.has(trip.dispatchStatus.toLowerCase())),
    [tripsQuery.data],
  )

  const driverResources = useMemo(() => {
    const seen = new Set<string>()
    const resources: { personId: string; status: string }[] = []
    for (const record of driverPanelQuery.data?.records ?? []) {
      if (seen.has(record.personId)) continue
      seen.add(record.personId)
      resources.push({ personId: record.personId, status: record.availabilityStatus })
    }
    return resources
  }, [driverPanelQuery.data?.records])

  const equipmentResources = useMemo(() => {
    const seen = new Set<string>()
    const resources: { vehicleRefKey: string; status: string }[] = []
    for (const record of equipmentPanelQuery.data?.records ?? []) {
      if (seen.has(record.vehicleRefKey)) continue
      seen.add(record.vehicleRefKey)
      resources.push({ vehicleRefKey: record.vehicleRefKey, status: record.availabilityStatus })
    }
    return resources
  }, [equipmentPanelQuery.data?.records])

  async function handleDrop(tripId: string, payload: DragPayload) {
    if (!canAssign) return

    setAssigningTripId(tripId)
    setStatusMessage(null)

    try {
      const preview = await previewDispatchAssignment(accessToken, {
        tripId,
        assignmentKind: payload.kind,
        driverPersonId: payload.kind === 'driver' ? payload.personId : null,
        vehicleRefKey: payload.kind === 'vehicle' ? payload.vehicleRefKey : null,
      })

      let ignoreConflicts = false
      let ignoreEligibilityBlocks = false
      let ignoreDispatchabilityBlocks = false
      let ignoreWorkflowGateBlocks = false
      const hasAvailabilityConflict =
        preview.blockingDriverAvailability.length > 0
        || preview.blockingEquipmentAvailability.length > 0
        || preview.overlappingTrips.length > 0
      const hasEligibilityBlock = preview.driverEligibility?.isBlocking === true
      const hasEligibilityWarn =
        preview.driverEligibility?.outcome === 'warn' && !preview.driverEligibility.isBlocking
      const hasDispatchabilityBlock = preview.assetDispatchability?.isBlocking === true
      const hasDispatchabilityWarn =
        preview.assetDispatchability?.outcome === 'warn' && !preview.assetDispatchability.isBlocking
      const hasWorkflowGateBlock = preview.workflowGates?.isBlocking === true
      const hasWorkflowGateWarn =
        preview.workflowGates?.outcome === 'warn' && !preview.workflowGates.isBlocking

      if (preview.hasBlockingConflicts) {
        const confirmed = window.confirm(
          `Assignment blocked: ${formatConflictMessage(preview)}. Assign anyway?`,
        )
        if (!confirmed) {
          setStatusMessage('Assignment cancelled due to conflicts.')
          setAssigningTripId(null)
          return
        }
        ignoreConflicts = hasAvailabilityConflict
        ignoreEligibilityBlocks = hasEligibilityBlock
        ignoreDispatchabilityBlocks = hasDispatchabilityBlock
        ignoreWorkflowGateBlocks = hasWorkflowGateBlock
      } else if (hasEligibilityWarn) {
        const confirmed = window.confirm(
          `Driver eligibility warning: ${preview.driverEligibility!.message}. Continue assignment?`,
        )
        if (!confirmed) {
          setStatusMessage('Assignment cancelled due to eligibility warning.')
          setAssigningTripId(null)
          return
        }
      } else if (hasDispatchabilityWarn) {
        const confirmed = window.confirm(
          `Asset dispatchability warning: ${preview.assetDispatchability!.message}. Continue assignment?`,
        )
        if (!confirmed) {
          setStatusMessage('Assignment cancelled due to dispatchability warning.')
          setAssigningTripId(null)
          return
        }
      } else if (hasWorkflowGateWarn) {
        const confirmed = window.confirm(
          `Compliance workflow gate warning: ${preview.workflowGates!.message}. Continue assignment?`,
        )
        if (!confirmed) {
          setStatusMessage('Assignment cancelled due to workflow gate warning.')
          setAssigningTripId(null)
          return
        }
      }

      await assignMutation.mutateAsync({
        tripId,
        payload,
        ignoreConflicts,
        ignoreEligibilityBlocks,
        ignoreDispatchabilityBlocks,
        ignoreWorkflowGateBlocks,
      })
    } catch (error) {
      setStatusMessage((error as Error).message)
      setAssigningTripId(null)
    }
  }

  function startDrag(event: React.DragEvent, payload: DragPayload) {
    if (!canAssign) return
    event.dataTransfer.setData('application/routarr-assignment', JSON.stringify(payload))
    event.dataTransfer.effectAllowed = 'move'
  }

  if (!canAssign) {
    return null
  }

  const isLoading =
    tripsQuery.isLoading || driverPanelQuery.isLoading || equipmentPanelQuery.isLoading

  if (isLoading) {
    return <p className="text-sm text-slate-400">Loading drag-and-drop assignment workspace…</p>
  }

  return (
    <section className="space-y-6" aria-label="Dispatch assignment">
      <div className="rounded-xl border border-slate-700 bg-slate-900/80 p-5">
        <h2 className="text-lg font-semibold text-slate-50">Drag-and-drop assignment</h2>
        <p className="mt-1 text-sm text-slate-400">
          Drop drivers or equipment onto active trips. Conflicts are checked against availability,
          overlapping assignments, driver eligibility, and Compliance Core workflow gates before saving.
        </p>
        {statusMessage ? <p className="mt-3 text-sm text-amber-200">{statusMessage}</p> : null}
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <div className="rounded-xl border border-slate-700 bg-slate-900/80 p-5">
          <h3 className="text-sm font-medium text-slate-300">Drivers</h3>
          {driverResources.length === 0 ? (
            <p className="mt-3 text-sm text-slate-500">No driver availability records in this window.</p>
          ) : (
            <ul className="mt-3 flex flex-wrap gap-2">
              {driverResources.map((driver) => (
                <li key={driver.personId}>
                  <button
                    type="button"
                    draggable
                    onDragStart={(event) =>
                      startDrag(event, { kind: 'driver', personId: driver.personId })
                    }
                    className={`cursor-grab rounded-full border px-3 py-1.5 text-xs active:cursor-grabbing ${
                      driver.status === 'available'
                        ? 'border-emerald-600 text-emerald-200'
                        : 'border-amber-600 text-amber-200'
                    }`}
                    data-testid={`driver-chip-${driver.personId}`}
                  >
                    {formatShortId(driver.personId)} · {driver.status}
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>

        <div className="rounded-xl border border-slate-700 bg-slate-900/80 p-5">
          <h3 className="text-sm font-medium text-slate-300">Equipment</h3>
          {equipmentResources.length === 0 ? (
            <p className="mt-3 text-sm text-slate-500">
              No equipment availability records in this window.
            </p>
          ) : (
            <ul className="mt-3 flex flex-wrap gap-2">
              {equipmentResources.map((equipment) => (
                <li key={equipment.vehicleRefKey}>
                  <button
                    type="button"
                    draggable
                    onDragStart={(event) =>
                      startDrag(event, {
                        kind: 'vehicle',
                        vehicleRefKey: equipment.vehicleRefKey,
                      })
                    }
                    className={`cursor-grab rounded-full border px-3 py-1.5 text-xs active:cursor-grabbing ${
                      equipment.status === 'available'
                        ? 'border-emerald-600 text-emerald-200'
                        : 'border-amber-600 text-amber-200'
                    }`}
                    data-testid={`vehicle-chip-${equipment.vehicleRefKey}`}
                  >
                    {formatShortId(equipment.vehicleRefKey)} · {equipment.status}
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>

      <div className="rounded-xl border border-slate-700 bg-slate-900/80 p-5">
        <h3 className="text-sm font-medium text-slate-300">Active trips</h3>
        {assignableTrips.length === 0 ? (
          <p className="mt-3 text-sm text-slate-500">No active trips available for assignment.</p>
        ) : (
          <ul className="mt-3 space-y-2">
            {assignableTrips.map((trip) => (
              <TripDropTarget
                key={trip.tripId}
                trip={trip}
                canAssign={canAssign}
                isAssigning={assigningTripId === trip.tripId}
                onDropAssignment={handleDrop}
              />
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
