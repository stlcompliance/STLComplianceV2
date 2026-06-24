import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import {
  ApiErrorCallout,
  ConfirmDialog,
  StaticSearchPicker,
  getErrorMessage,
  type PickerOption,
} from '@stl/shared-ui'

import {
  applyBulkDispatch,
  assignTripDriver,
  getUnassignedWorkQueue,
  previewBulkDispatch,
  previewDispatchAssignment,
} from '../api/client'
import type { UnassignedWorkQueueTripRow } from '../api/types'
import {
  confirmBulkDispatchPreview,
  formatBulkDispatchBlockedMessage,
  resolveBulkDispatchIgnoreFlags,
  type BulkDispatchIgnoreFlags,
} from '../lib/bulkDispatch'
import {
  confirmDispatchAssignmentPreview,
  resolveAssignmentIgnoreFlags,
  type AssignmentIgnoreFlags,
} from '../lib/dispatchAssignment'
import { DispatchAssignmentGateDetails } from './DispatchAssignmentGateDetails'

type Props = {
  accessToken: string
  scope: 'daily' | 'weekly'
  canAssign: boolean
}

function formatTimestamp(iso: string | null) {
  if (!iso) return '—'
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}

function formatMinutesUntilStart(minutes: number) {
  if (minutes === Number.MAX_SAFE_INTEGER || minutes > 100000) {
    return 'No start time'
  }
  if (minutes < 0) {
    return `${Math.abs(minutes)} min ago`
  }
  if (minutes === 0) {
    return 'Starting now'
  }
  return `In ${minutes} min`
}

function TripRow({
  trip,
  selected,
  canAssign,
  accessToken,
  onToggle,
  driverOptions,
  onAssign,
  isPending,
}: {
  trip: UnassignedWorkQueueTripRow
  selected: boolean
  canAssign: boolean
  accessToken: string
  onToggle: () => void
  driverOptions: { personId: string; displayName: string }[]
  onAssign: (tripId: string, personId: string, displayName: string) => void
  isPending: boolean
}) {
  const [driverId, setDriverId] = useState('')

  const assignPreviewQuery = useQuery({
    queryKey: ['routarr-unassigned-assign-preview', accessToken, trip.tripId, driverId],
    queryFn: () =>
      previewDispatchAssignment(accessToken, {
        tripId: trip.tripId,
        assignmentKind: 'driver',
        driverPersonId: driverId,
        vehicleRefKey: null,
      }),
    enabled: canAssign && driverId.length > 0,
  })

  const borderClass = trip.isLate
    ? 'border-red-500/60 bg-red-950/30'
    : trip.isAtRisk
      ? 'border-amber-500/60 bg-amber-950/20'
      : 'border-slate-700 bg-slate-950/50'

  return (
    <li
      className={`rounded-md border p-3 text-sm ${borderClass}`}
      data-testid={`unassigned-trip-${trip.tripId}`}
    >
      <div className="flex flex-wrap items-start gap-2">
        {canAssign ? (
          <input id="unassignedworkqueue-input-field"
            type="checkbox"
            checked={selected}
            onChange={onToggle}
            aria-label={`Select ${trip.title}`}
          />
        ) : null}
        <div className="min-w-0 flex-1">
          <p className="font-medium text-slate-100">{trip.title}</p>
          <p className="text-xs text-[var(--color-text-muted)]">
            {trip.tripNumber} · {trip.dispatchStatus.replace('_', ' ')}
            {trip.isLate ? ' · late' : trip.isAtRisk ? ' · at risk' : ''}
          </p>
          <p className="mt-1 text-xs text-slate-400">
            Start {formatTimestamp(trip.scheduledStartAt)} · {formatMinutesUntilStart(trip.minutesUntilStart)}
            · {trip.routeCount} route(s)
          </p>
        </div>
      </div>
      {canAssign ? (
        <div className="mt-2 space-y-2">
          <div className="flex flex-wrap gap-1">
            <StaticSearchPicker
              label={`Assign driver for ${trip.title}`}
              value={driverId}
              onChange={setDriverId}
              options={driverOptions.map((driver) => ({
                value: driver.personId,
                label: driver.displayName,
              }))}
              placeholder="Search drivers…"
              testId={`unassigned-driver-picker-${trip.tripId}`}
            />
            <button
              type="button"
              className="rounded bg-violet-700 px-2 py-1 text-xs text-white disabled:opacity-50"
              disabled={!driverId || isPending}
              onClick={() => {
                const ref = driverOptions.find((d) => d.personId === driverId)
                onAssign(trip.tripId, driverId, ref?.displayName ?? driverId)
              }}
            >
              Assign
            </button>
          </div>
          {driverId ? (
            assignPreviewQuery.isLoading ? (
              <p className="text-xs text-[var(--color-text-muted)]" data-testid={`unassigned-gate-loading-${trip.tripId}`}>
                Checking eligibility and workflow gates…
              </p>
            ) : assignPreviewQuery.data ? (
              <DispatchAssignmentGateDetails
                preview={assignPreviewQuery.data}
                title="Assignment gate preview"
                compact
                data-testid={`unassigned-gate-preview-${trip.tripId}`}
              />
            ) : assignPreviewQuery.isError ? (
              <ApiErrorCallout
                title="Assignment gate check failed"
                message={getErrorMessage(assignPreviewQuery.error, 'Unable to check assignment gates.')}
                className="text-xs"
              />
            ) : null
          ) : null}
        </div>
      ) : null}
    </li>
  )
}

export function UnassignedWorkQueuePanel({ accessToken, scope, canAssign }: Props) {
  const queryClient = useQueryClient()
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())
  const [bulkDriverId, setBulkDriverId] = useState('')
  const [attentionOnly, setAttentionOnly] = useState(false)
  const [statusMessage, setStatusMessage] = useState<string | null>(null)
  const [pendingAssignment, setPendingAssignment] = useState<{
    tripId: string
    personId: string
    displayName: string
    message: string
    ignoreFlags: AssignmentIgnoreFlags
    danger: boolean
  } | null>(null)
  const [pendingBulkAssignment, setPendingBulkAssignment] = useState<{
    personId: string
    items: { tripId: string; driverPersonId: string }[]
    message: string
    ignoreFlags: BulkDispatchIgnoreFlags
    danger: boolean
  } | null>(null)

  const queueQuery = useQuery({
    queryKey: ['routarr-unassigned-queue', accessToken, scope, attentionOnly],
    queryFn: () => getUnassignedWorkQueue(accessToken, scope, { attentionOnly }),
  })

  const bulkPreviewQuery = useQuery({
    queryKey: [
      'routarr-unassigned-bulk-preview',
      accessToken,
      bulkDriverId,
      [...selectedIds].sort().join(','),
    ],
    queryFn: () =>
      previewBulkDispatch(accessToken, {
        items: [...selectedIds].map((tripId) => ({
          tripId,
          driverPersonId: bulkDriverId,
        })),
      }),
    enabled: canAssign && bulkDriverId.length > 0 && selectedIds.size > 0,
  })

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: ['routarr-unassigned-queue'] })
    void queryClient.invalidateQueries({ queryKey: ['routarr-dispatch-board'] })
    void queryClient.invalidateQueries({ queryKey: ['routarr-command-center'] })
    void queryClient.invalidateQueries({ queryKey: ['routarr-active-trips'] })
    void queryClient.invalidateQueries({ queryKey: ['routarr-trips'] })
  }

  const assignMutation = useMutation({
    mutationFn: async ({
      tripId,
      personId,
      displayName,
      ignoreFlags,
    }: {
      tripId: string
      personId: string
      displayName: string
      ignoreFlags: AssignmentIgnoreFlags
    }) => {
      await assignTripDriver(accessToken, tripId, {
        driverPersonId: personId,
        driverDisplayName: displayName,
        ignoreAvailabilityConflicts: ignoreFlags.ignoreConflicts,
        ignoreEligibilityBlocks: ignoreFlags.ignoreEligibilityBlocks,
        ignoreWorkflowGateBlocks: ignoreFlags.ignoreWorkflowGateBlocks,
      })
    },
    onSuccess: () => {
      setStatusMessage('Driver assigned.')
      setSelectedIds(new Set())
      setPendingAssignment(null)
      invalidate()
    },
    onError: (error: Error) => {
      setStatusMessage(error.message)
      setPendingAssignment(null)
    },
  })

  const bulkMutation = useMutation({
    mutationFn: async ({
      items,
      ignoreFlags,
    }: {
      items: { tripId: string; driverPersonId: string }[]
      ignoreFlags: BulkDispatchIgnoreFlags
    }) => {
      await applyBulkDispatch(accessToken, {
        items,
        ...ignoreFlags,
      })
    },
    onSuccess: () => {
      setStatusMessage('Bulk assignment applied.')
      setSelectedIds(new Set())
      setBulkDriverId('')
      setPendingBulkAssignment(null)
      invalidate()
    },
    onError: (error: Error) => {
      setStatusMessage(error.message)
      setPendingBulkAssignment(null)
    },
  })

  async function handleAssign(tripId: string, personId: string, displayName: string) {
    if (!canAssign) return
    setStatusMessage(null)

    try {
      const preview = await previewDispatchAssignment(accessToken, {
        tripId,
        assignmentKind: 'driver',
        driverPersonId: personId,
        vehicleRefKey: null,
      })

      let confirmationMessage: string | null = null
      const confirmedFlags = confirmDispatchAssignmentPreview(preview, (message) => {
        confirmationMessage = message
        return false
      })
      const previewFlags = resolveAssignmentIgnoreFlags(preview)

      if (!confirmedFlags) {
        setPendingAssignment({
          tripId,
          personId,
          displayName,
          message: confirmationMessage ?? 'Review this assignment before proceeding.',
          ignoreFlags: {
            ignoreConflicts: previewFlags.ignoreConflicts,
            ignoreEligibilityBlocks: previewFlags.ignoreEligibilityBlocks,
            ignoreDispatchabilityBlocks: previewFlags.ignoreDispatchabilityBlocks,
            ignoreWorkflowGateBlocks: previewFlags.ignoreWorkflowGateBlocks,
          },
          danger:
            preview.hasBlockingConflicts
            || previewFlags.hasEligibilityWarn
            || previewFlags.hasDispatchabilityWarn
            || previewFlags.hasWorkflowGateWarn,
        })
        return
      }

      await assignMutation.mutateAsync({
        tripId,
        personId,
        displayName,
        ignoreFlags: confirmedFlags,
      })
    } catch (error) {
      setStatusMessage(getErrorMessage(error, 'Assignment failed.'))
    }
  }

  async function handleBulkAssign(personId: string) {
    if (!canAssign || selectedIds.size === 0 || !personId) return
    setStatusMessage(null)

    const items = [...selectedIds].map((tripId) => ({
      tripId,
      driverPersonId: personId,
    }))

    try {
      const preview =
        bulkPreviewQuery.data ?? (await previewBulkDispatch(accessToken, { items }))
      const confirmedFlags = confirmBulkDispatchPreview(preview, () => false)

      if (!confirmedFlags) {
        const previewFlags = resolveBulkDispatchIgnoreFlags(preview.items)
        setPendingBulkAssignment({
          personId,
          items,
          message:
            preview.summary.blockedCount > 0
              ? `${preview.summary.blockedCount} trip(s) have blocking conflicts (${formatBulkDispatchBlockedMessage(preview)}). Apply anyway?`
              : 'Review this bulk assignment before proceeding.',
          ignoreFlags: previewFlags,
          danger:
            preview.summary.blockedCount > 0
            || previewFlags.ignoreEligibilityBlocks
            || previewFlags.ignoreDispatchabilityBlocks
            || previewFlags.ignoreWorkflowGateBlocks,
        })
        return
      }

      await bulkMutation.mutateAsync({
        items,
        ignoreFlags: confirmedFlags,
      })
    } catch (error) {
      setStatusMessage(getErrorMessage(error, 'Bulk assignment failed.'))
    }
  }

  const queue = queueQuery.data!
  const driverOptions = queue?.driverRefs.items.map((d) => ({
    personId: d.personId,
    displayName: d.displayName,
  })) ?? []
  const driverPickerOptions = useMemo<PickerOption[]>(
    () =>
      driverOptions.map((driver) => ({
        value: driver.personId,
        label: driver.displayName,
      })),
    [driverOptions],
  )
  const isPending = assignMutation.isPending || bulkMutation.isPending

  if (queueQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading unassigned work queue…</p>
  }

  if (queueQuery.isError) {
    return (
      <ApiErrorCallout
        title="Unassigned work queue unavailable"
        message={getErrorMessage(queueQuery.error, 'Unable to load unassigned work queue.')}
        retryLabel="Retry queue"
        onRetry={() => {
          void queueQuery.refetch()
        }}
      />
    )
  }

  const toggleTrip = (tripId: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev)
      if (next.has(tripId)) {
        next.delete(tripId)
      } else {
        next.add(tripId)
      }
      return next
    })
  }

  return (
    <section
      className="rounded-xl border border-violet-800/40 bg-violet-950/15 p-5"
      data-testid="unassigned-work-queue-panel"
    >
      <ConfirmDialog
        open={pendingAssignment !== null}
        title="Confirm assignment"
        description={pendingAssignment?.message ?? 'Review the assignment details before proceeding.'}
        confirmLabel="Assign"
        cancelLabel="Cancel"
        danger={pendingAssignment?.danger ?? false}
        onConfirm={() => {
          if (!pendingAssignment) return
          const assignment = pendingAssignment
          setPendingAssignment(null)
          void assignMutation.mutateAsync({
            tripId: assignment.tripId,
            personId: assignment.personId,
            displayName: assignment.displayName,
            ignoreFlags: assignment.ignoreFlags,
          })
        }}
        onCancel={() => {
          setPendingAssignment(null)
          setStatusMessage('Assignment cancelled.')
        }}
      />
      <ConfirmDialog
        open={pendingBulkAssignment !== null}
        title="Confirm bulk assignment"
        description={
          pendingBulkAssignment?.message ?? 'Review the bulk assignment details before proceeding.'
        }
        confirmLabel="Assign"
        cancelLabel="Cancel"
        danger={pendingBulkAssignment?.danger ?? false}
        onConfirm={() => {
          if (!pendingBulkAssignment) return
          const assignment = pendingBulkAssignment
          setPendingBulkAssignment(null)
          void bulkMutation.mutateAsync({
            items: assignment.items,
            ignoreFlags: assignment.ignoreFlags,
          })
        }}
        onCancel={() => {
          setPendingBulkAssignment(null)
          setStatusMessage('Bulk assignment cancelled.')
        }}
      />
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Unassigned work queue</h2>
        <p className="mt-1 text-sm text-slate-400">
          {queue.summary.unassignedCount} active trips without a driver · {queue.summary.urgentCount}{' '}
          urgent (late/at-risk)
        </p>
      </header>

      <div className="mt-3 flex flex-wrap items-center gap-3">
        <label className="flex items-center gap-2 text-xs text-slate-400">
          <input id="unassignedworkqueue"
            type="checkbox"
            checked={attentionOnly}
            onChange={(e) => setAttentionOnly(e.target.checked)}
            data-testid="unassigned-attention-filter"
          />
          Urgent only (late/at-risk)
        </label>
        {statusMessage ? (
          <p className="text-xs text-slate-400" data-testid="unassigned-queue-status">
            {statusMessage}
          </p>
        ) : null}
      </div>

      {canAssign && queue.items.length > 0 ? (
        <div className="mt-4 space-y-3 rounded-lg border border-slate-700 bg-slate-900/50 p-3">
          <div className="flex flex-wrap items-center gap-2">
            <StaticSearchPicker
              label="Bulk assign driver"
              value={bulkDriverId}
              onChange={setBulkDriverId}
              options={driverPickerOptions}
              placeholder="Search drivers…"
              testId="unassigned-bulk-driver-picker"
            />
            <button
              type="button"
              className="rounded bg-violet-700 px-3 py-1 text-sm text-white disabled:opacity-50"
              disabled={selectedIds.size === 0 || !bulkDriverId || isPending}
              onClick={() => void handleBulkAssign(bulkDriverId)}
              data-testid="bulk-assign-unassigned"
            >
              Assign {selectedIds.size} selected
            </button>
            <button
              type="button"
              className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-400"
              onClick={() => setSelectedIds(new Set(queue.items.map((t) => t.tripId)))}
            >
              Select all
            </button>
          </div>
          {bulkDriverId && selectedIds.size > 0 ? (
            bulkPreviewQuery.isLoading ? (
              <p className="text-xs text-[var(--color-text-muted)]" data-testid="unassigned-bulk-gate-loading">
                Previewing bulk assignment gates…
              </p>
            ) : bulkPreviewQuery.data ? (
              <div className="space-y-2" data-testid="unassigned-bulk-gate-preview">
                <p className="text-xs text-slate-400">
                  Bulk preview: {bulkPreviewQuery.data.summary.canApplyCount}/
                  {bulkPreviewQuery.data.summary.total} ready ·{' '}
                  {bulkPreviewQuery.data.summary.blockedCount} blocked
                </p>
                <ul className="space-y-2">
                  {bulkPreviewQuery.data.items.map((item) =>
                    item.driverPreview ? (
                      <li key={item.tripId}>
                        <p className="text-xs font-medium text-slate-300">
                          {item.title} ({item.tripNumber})
                        </p>
                        <DispatchAssignmentGateDetails
                          preview={item.driverPreview}
                          compact
                          data-testid={`unassigned-bulk-gates-${item.tripId}`}
                        />
                      </li>
                    ) : null,
                  )}
                </ul>
              </div>
            ) : null
          ) : null}
        </div>
      ) : null}

      <ul className="mt-4 max-h-80 space-y-2 overflow-y-auto">
        {queue.items.length === 0 ? (
          <li className="text-sm text-[var(--color-text-muted)]">No unassigned active trips match filters.</li>
        ) : (
          queue.items.map((trip) => (
              <TripRow
                key={trip.tripId}
                trip={trip}
                selected={selectedIds.has(trip.tripId)}
                canAssign={canAssign}
                accessToken={accessToken}
                onToggle={() => toggleTrip(trip.tripId)}
                driverOptions={driverOptions}
                isPending={isPending}
                onAssign={(tripId, personId, displayName) =>
                  void handleAssign(tripId, personId, displayName)
                }
              />
            ))
          )}
      </ul>
    </section>
  )
}
