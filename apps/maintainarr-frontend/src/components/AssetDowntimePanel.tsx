import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { StaticSearchPicker } from '@stl/shared-ui'

import {
  closeDowntimeEvent,
  createManualDowntimeEvent,
  getFleetAvailability,
  listDowntimeEvents,
} from '../api/client'
import type { AssetResponse } from '../api/types'
import { parseDowntimeDeepLink } from '../lib/downtimeDeepLink'

const MANUAL_DOWNTIME_REASONS = [
  { value: 'in_repair', label: 'In repair' },
  { value: 'awaiting_parts', label: 'Awaiting parts' },
  { value: 'awaiting_technician', label: 'Awaiting technician' },
  { value: 'awaiting_vendor', label: 'Awaiting vendor' },
  { value: 'awaiting_approval', label: 'Awaiting approval' },
  { value: 'failed_inspection', label: 'Failed inspection' },
  { value: 'regulatory_hold', label: 'Regulatory hold' },
  { value: 'unknown', label: 'Unknown' },
] as const

interface AssetDowntimePanelProps {
  accessToken: string
  canRead: boolean
  canManage: boolean
  assets: AssetResponse[]
}

type ManualDowntimeReason = (typeof MANUAL_DOWNTIME_REASONS)[number]['value']

export function AssetDowntimePanel({
  accessToken,
  canRead,
  canManage,
  assets,
}: AssetDowntimePanelProps) {
  const queryClient = useQueryClient()
  const [searchParams] = useSearchParams()
  const deepLinkContext = parseDowntimeDeepLink(searchParams.toString())
  const [selectedAssetId, setSelectedAssetId] = useState(deepLinkContext.assetId ?? '')
  const [reason, setReason] = useState<ManualDowntimeReason>(MANUAL_DOWNTIME_REASONS[0].value)
  const [isPlanned, setIsPlanned] = useState(false)
  const [notes, setNotes] = useState('')
  const assetOptions = assets.map((asset) => ({
    value: asset.assetId,
    label: `${asset.assetTag} — ${asset.name}`,
  }))
  const selectedAssetOption = assetOptions.find((option) => option.value === selectedAssetId)
    ?? (selectedAssetId ? { value: selectedAssetId, label: selectedAssetId } : undefined)

  useEffect(() => {
    if (deepLinkContext.assetId) {
      setSelectedAssetId(deepLinkContext.assetId)
    }
  }, [deepLinkContext.assetId])

  const fleetQuery = useQuery({
    queryKey: ['maintainarr-fleet-availability', accessToken],
    queryFn: () => getFleetAvailability(accessToken),
    enabled: canRead,
  })

  const eventsQuery = useQuery({
    queryKey: ['maintainarr-downtime-events', accessToken, selectedAssetId],
    queryFn: () =>
      listDowntimeEvents(accessToken, {
        assetId: selectedAssetId || undefined,
        limit: 50,
      }),
    enabled: canRead,
  })

  const createMutation = useMutation({
    mutationFn: () =>
      createManualDowntimeEvent(accessToken, {
        assetId: selectedAssetId,
        reason,
        isPlanned,
        startedAt: new Date().toISOString(),
        notes: notes.trim() || undefined,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-downtime-events', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-fleet-availability', accessToken] })
      setNotes('')
    },
  })

  const closeMutation = useMutation({
    mutationFn: (eventId: string) => closeDowntimeEvent(accessToken, eventId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-downtime-events', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-fleet-availability', accessToken] })
    },
  })

  if (!canRead) {
    return null
  }

  const fleet = fleetQuery.data

  return (
    <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-6" data-testid="maintainarr-downtime-panel">
      <div className="mb-4">
        <h2 className="text-lg font-semibold text-slate-100">Downtime and availability</h2>
        <p className="text-sm text-slate-400">
          Track manual downtime events and review fleet availability metrics.
        </p>
      </div>

      {deepLinkContext.assetId || deepLinkContext.workOrderId || deepLinkContext.defectId ? (
        <div
          className="mb-4 rounded-lg border border-sky-800/60 bg-sky-950/30 px-4 py-3 text-sm text-sky-100"
          data-testid="maintainarr-downtime-deep-link-banner"
        >
          <p className="font-medium">Linked downtime context</p>
          <p className="mt-1 text-xs text-sky-200/80">
            {deepLinkContext.workOrderId ? `Work order ${deepLinkContext.workOrderId.slice(0, 8)}… · ` : ''}
            {deepLinkContext.defectId ? `Defect ${deepLinkContext.defectId.slice(0, 8)}… · ` : ''}
            {deepLinkContext.eventId ? `Event ${deepLinkContext.eventId.slice(0, 8)}…` : 'Active downtime events for this asset are shown below.'}
          </p>
        </div>
      ) : null}

      {fleet ? (
        <dl className="mb-6 grid gap-3 sm:grid-cols-2 lg:grid-cols-4" data-testid="maintainarr-fleet-availability-summary">
          <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
            <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Fleet availability</dt>
            <dd className="text-2xl font-semibold text-emerald-300">{fleet.availabilityPercent}%</dd>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
            <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Downtime hours</dt>
            <dd className="text-2xl font-semibold text-amber-300">{fleet.downtimeHours}</dd>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
            <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Active events</dt>
            <dd className="text-2xl font-semibold text-slate-100">{fleet.activeDowntimeEventCount}</dd>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
            <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Assets tracked</dt>
            <dd className="text-2xl font-semibold text-slate-100">{fleet.assetCount}</dd>
          </div>
        </dl>
      ) : null}

      {canManage ? (
        <form
          className="mb-6 grid gap-3 rounded-lg border border-slate-800 bg-slate-950/40 p-4 md:grid-cols-2"
          data-testid="maintainarr-manual-downtime-form"
          onSubmit={(event) => {
            event.preventDefault()
            if (!selectedAssetId) {
              return
            }
            createMutation.mutate()
          }}
        >
          <StaticSearchPicker
            id="maintainarr-downtime-asset"
            label="Asset"
            value={selectedAssetId}
            onChange={setSelectedAssetId}
            options={assetOptions}
            selectedOption={selectedAssetOption}
            placeholder="Search assets…"
            testId="maintainarr-downtime-asset"
          />
          <label className="grid gap-1 text-sm text-slate-300">
            Reason
            <select
              className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2"
              value={reason}
              onChange={(event) => setReason(event.target.value as ManualDowntimeReason)}
            >
              {MANUAL_DOWNTIME_REASONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
          <label className="flex items-center gap-2 text-sm text-slate-300">
            <input
              type="checkbox"
              checked={isPlanned}
              onChange={(event) => setIsPlanned(event.target.checked)}
            />
            Planned downtime
          </label>
          <label className="grid gap-1 text-sm text-slate-300 md:col-span-2">
            Notes
            <textarea
              className="min-h-20 rounded-md border border-slate-700 bg-slate-950 px-3 py-2"
              value={notes}
              onChange={(event) => setNotes(event.target.value)}
            />
          </label>
          <button
            type="submit"
            className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50 md:col-span-2 md:justify-self-start"
            disabled={!selectedAssetId || createMutation.isPending}
          >
            Record manual downtime
          </button>
        </form>
      ) : null}

      <div className="overflow-x-auto">
        <table className="min-w-full text-sm">
          <thead>
            <tr className="border-b border-slate-800 text-left text-slate-400">
              <th className="px-2 py-2">Asset</th>
              <th className="px-2 py-2">Source</th>
              <th className="px-2 py-2">Reason</th>
              <th className="px-2 py-2">Started</th>
              <th className="px-2 py-2">Ended</th>
              <th className="px-2 py-2">Status</th>
              {canManage ? <th className="px-2 py-2">Actions</th> : null}
            </tr>
          </thead>
          <tbody>
            {(eventsQuery.data ?? []).map((event) => (
              <tr
                key={event.eventId}
                className={`border-b border-slate-900 text-slate-200 ${
                  deepLinkContext.eventId === event.eventId ? 'bg-sky-950/40' : ''
                }`}
                data-testid={`maintainarr-downtime-event-${event.eventId}`}
                data-highlighted={deepLinkContext.eventId === event.eventId ? 'true' : 'false'}
              >
                <td className="px-2 py-2">{event.assetTag}</td>
                <td className="px-2 py-2">{event.source}</td>
                <td className="px-2 py-2">{event.reason}</td>
                <td className="px-2 py-2">{new Date(event.startedAt).toLocaleString()}</td>
                <td className="px-2 py-2">
                  {event.endedAt ? new Date(event.endedAt).toLocaleString() : '—'}
                </td>
                <td className="px-2 py-2">{event.isActive ? 'Active' : 'Closed'}</td>
                {canManage ? (
                  <td className="px-2 py-2">
                    {event.isActive ? (
                      <button
                        type="button"
                        className="rounded border border-slate-700 px-2 py-1 text-xs text-slate-200"
                        onClick={() => closeMutation.mutate(event.eventId)}
                        disabled={closeMutation.isPending}
                      >
                        Close
                      </button>
                    ) : null}
                  </td>
                ) : null}
              </tr>
            ))}
          </tbody>
        </table>
        {eventsQuery.isSuccess && (eventsQuery.data?.length ?? 0) === 0 ? (
          <p className="mt-3 text-sm text-[var(--color-text-muted)]">No downtime events recorded yet.</p>
        ) : null}
      </div>
    </section>
  )
}
