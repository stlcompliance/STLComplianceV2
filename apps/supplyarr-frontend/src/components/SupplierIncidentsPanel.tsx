import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { buildSemanticKey, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import { useMemo, useState } from 'react'

import {
  applySupplierIncidentProcurementRestriction,
  cancelSupplierIncident,
  closeSupplierIncident,
  createSupplierIncident,
  listPartySupplierIncidents,
  listSupplierIncidents,
  reopenSupplierIncident,
  resolveSupplierIncident,
  startSupplierIncidentInvestigation,
} from '../api/client'
import type { ExternalPartyResponse } from '../api/types'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'

const INCIDENT_TYPES = ['quality', 'delivery', 'compliance', 'safety', 'other'] as const
const SEVERITIES = ['low', 'medium', 'high', 'critical'] as const

interface SupplierIncidentsPanelProps {
  accessToken: string
  canManage: boolean
  incidentParties: ExternalPartyResponse[]
}

function statusTone(status: string): string {
  switch (status) {
    case 'open':
      return 'warning'
    case 'investigating':
      return 'info'
    case 'resolved':
      return 'success'
    case 'closed':
      return 'inactive'
    case 'cancelled':
      return 'danger'
    default:
      return 'danger'
  }
}

export function SupplierIncidentsPanel({
  accessToken,
  canManage,
  incidentParties,
}: SupplierIncidentsPanelProps) {
  const queryClient = useQueryClient()
  const [selectedPartyId, setSelectedPartyId] = useState('')
  const [incidentKey, setIncidentKey] = useState('')
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [incidentType, setIncidentType] = useState<(typeof INCIDENT_TYPES)[number]>('quality')
  const [severity, setSeverity] = useState<(typeof SEVERITIES)[number]>('medium')
  const [resolutionNotes] = useState('')
  const [cancelReason, setCancelReason] = useState('')
  const [reopenReason, setReopenReason] = useState('')

  const openQuery = useQuery({
    queryKey: ['supplyarr-supplier-incidents-open', accessToken],
    queryFn: () => listSupplierIncidents(accessToken, { status: 'open' }),
    enabled: canManage,
  })

  const partyIncidentsQuery = useQuery({
    queryKey: ['supplyarr-party-supplier-incidents', accessToken, selectedPartyId],
    queryFn: () => listPartySupplierIncidents(accessToken, selectedPartyId),
    enabled: Boolean(selectedPartyId),
  })

  const selectedParty = useMemo(
    () => incidentParties.find((p) => p.partyId === selectedPartyId),
    [incidentParties, selectedPartyId],
  )
  const partyOptions = useMemo<PickerOption[]>(
    () =>
      incidentParties.map((party) => ({
        value: party.partyId,
        label: `${party.unitKind === 'sub_unit' ? 'sub-unit' : 'supplier identity'} · ${party.partyKey} · ${party.displayName}`,
      })),
    [incidentParties],
  )
  const selectedPartyOption = useMemo<PickerOption | undefined>(
    () => partyOptions.find((option) => option.value === selectedPartyId),
    [partyOptions, selectedPartyId],
  )
  const incidentKeySource = useMemo(() => {
    const partyLabel = selectedParty?.displayName ?? ''
    const titleLabel = title.trim()
    if (titleLabel) {
      return `${partyLabel} ${titleLabel}`
    }
    if (!partyLabel) {
      return ''
    }
    return `${partyLabel} ${incidentType} incident`
  }, [incidentType, selectedParty, title])

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: ['supplyarr-supplier-incidents-open', accessToken] })
    void queryClient.invalidateQueries({
      queryKey: ['supplyarr-party-supplier-incidents', accessToken, selectedPartyId],
    })
  }

  const createMutation = useMutation({
    mutationFn: () =>
      createSupplierIncident(accessToken, {
        externalPartyId: selectedPartyId,
        incidentKey,
        title,
        description,
        incidentType,
        severity,
      }),
    onSuccess: () => {
      setIncidentKey('')
      setTitle('')
      setDescription('')
      invalidate()
    },
  })

  const workflowMutation = useMutation({
    mutationFn: async (action: {
      type: 'investigate' | 'resolve' | 'close' | 'cancel' | 'reopen' | 'restrict'
      incidentId: string
    }) => {
      if (action.type === 'investigate') {
        return startSupplierIncidentInvestigation(accessToken, action.incidentId)
      }
      if (action.type === 'resolve') {
        return resolveSupplierIncident(accessToken, action.incidentId, {
          resolutionNotes: resolutionNotes || 'Resolved from parties workspace',
        })
      }
      if (action.type === 'close') {
        return closeSupplierIncident(accessToken, action.incidentId)
      }
      if (action.type === 'reopen') {
        return reopenSupplierIncident(accessToken, action.incidentId, {
          reason:
            reopenReason ||
            'Reopened from parties workspace after mistaken cancellation.',
        })
      }
      if (action.type === 'cancel') {
        return cancelSupplierIncident(accessToken, action.incidentId, {
          reason: cancelReason || 'Cancelled from parties workspace',
        })
      }
      return applySupplierIncidentProcurementRestriction(accessToken, action.incidentId, {
        restrictionKey: buildSemanticKey({
          domain: 'vendor',
          kind: 'restriction',
          title: `${incidentKey || action.incidentId.slice(0, 8)} procurement hold`,
          maxLength: 128,
        }),
        scopes: ['all_procurement'],
        reason: title || 'Supplier incident procurement hold',
      })
    },
    onSuccess: invalidate,
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2"
      data-testid="supplier-incidents-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Supplier incidents</h2>
      <p className="mt-1 text-sm text-slate-400">
        Track quality, delivery, and compliance issues for supplier identities and sub-units. Apply procurement
        holds via supplier restrictions when needed.
      </p>

      {openQuery.data && (
        <p className="mt-3 text-sm text-[var(--color-text-muted)]">
          {openQuery.data.length} open incident{openQuery.data.length === 1 ? '' : 's'} tenant-wide
        </p>
      )}

      <div className="mt-4 grid gap-4 md:grid-cols-2">
        <StaticSearchPicker
          id="supplier-incident-party"
          label="Supplier identity or sub-unit"
          value={selectedPartyId}
          onChange={setSelectedPartyId}
          options={partyOptions}
          selectedOption={selectedPartyOption}
          placeholder="Search supplier identities or sub-units…"
          testId="supplier-incident-party-picker"
        />

        {selectedPartyId && (
          <>
            <GeneratedKeyFieldGroup
              sourceLabel={incidentKeySource}
              existingKeys={partyIncidentsQuery.data?.map((incident) => incident.incidentKey) ?? []}
              onKeyChange={setIncidentKey}
              domain="incident"
              kind="supplier"
              maxLength={128}
              label="Incident key"
              disabled={createMutation.isPending}
            />
            <label htmlFor="supplier-incident-type" className="block text-sm text-slate-400">
              Incident type
              <select
                id="supplier-incident-type"
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
                value={incidentType}
                onChange={(event) =>
                  setIncidentType(event.target.value as (typeof INCIDENT_TYPES)[number])
                }
              >
                {INCIDENT_TYPES.map((t) => (
                  <option key={t} value={t}>
                    {t}
                  </option>
                ))}
              </select>
            </label>
            <label htmlFor="supplier-incident-severity" className="block text-sm text-slate-400">
              Incident severity
              <select
                id="supplier-incident-severity"
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
                value={severity}
                onChange={(event) =>
                  setSeverity(event.target.value as (typeof SEVERITIES)[number])
                }
              >
                {SEVERITIES.map((s) => (
                  <option key={s} value={s}>
                    {s}
                  </option>
                ))}
              </select>
            </label>
            <label htmlFor="supplier-incident-title" className="block text-sm text-slate-400 md:col-span-2">
              Incident title
              <input
                id="supplier-incident-title"
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
                value={title}
                onChange={(event) => setTitle(event.target.value)}
              />
            </label>
            <label htmlFor="supplier-incident-description" className="block text-sm text-slate-400 md:col-span-2">
              Incident description
              <textarea
                id="supplier-incident-description"
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
                rows={2}
                value={description}
                onChange={(event) => setDescription(event.target.value)}
              />
            </label>
            <label htmlFor="supplier-incident-cancel-reason" className="block text-sm text-slate-400 md:col-span-2">
              Cancel reason (for cancel action)
              <textarea
                id="supplier-incident-cancel-reason"
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
                rows={2}
                data-testid="supplier-incident-cancel-reason"
                value={cancelReason}
                onChange={(event) => setCancelReason(event.target.value)}
              />
            </label>
            <label htmlFor="supplier-incident-reopen-reason" className="block text-sm text-slate-400 md:col-span-2">
              Reopen reason (for reopen after cancel)
              <textarea
                id="supplier-incident-reopen-reason"
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
                rows={2}
                data-testid="supplier-incident-reopen-reason"
                value={reopenReason}
                onChange={(event) => setReopenReason(event.target.value)}
              />
            </label>
            <button
              type="button"
              className="rounded bg-amber-700 px-3 py-1.5 text-sm text-white disabled:opacity-50 md:col-span-2 md:w-fit"
              disabled={createMutation.isPending || !incidentKey.trim() || !title.trim()}
              onClick={() => createMutation.mutate()}
            >
              Open incident
            </button>
          </>
        )}
      </div>

      {partyIncidentsQuery.data && partyIncidentsQuery.data.length > 0 && (
        <ul className="mt-4 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
          {partyIncidentsQuery.data.map((item) => (
            <li
              key={item.incidentId}
              className="px-3 py-3"
              data-testid={`supplier-incident-row-${item.incidentId}`}
            >
              <div className="flex flex-wrap items-start justify-between gap-2">
                <div>
                  <div className="font-medium text-slate-100">
                    {item.incidentKey} · {item.title}
                  </div>
                  <div className="text-xs text-[var(--color-text-muted)]">
                    {item.incidentType} · {item.severity}
                    {item.vendorRestrictionId ? ' · restriction applied' : ''}
                    {item.reopenCount > 0 ? ` · reopened ${item.reopenCount}×` : ''}
                  </div>
                </div>
                <span
                  className="stl-tone-badge rounded border px-2 py-0.5 text-xs"
                  data-tone={statusTone(item.status)}
                  data-testid={`supplier-incident-status-${item.incidentId}`}
                >
                  {item.status}
                </span>
              </div>
              <div className="mt-2 flex flex-wrap gap-2">
                {item.status === 'open' && (
                  <>
                    <button
                      type="button"
                      className="rounded border border-slate-600 px-2 py-0.5 text-xs text-slate-200"
                      data-testid={`supplier-incident-investigate-${item.incidentId}`}
                      onClick={() =>
                        workflowMutation.mutate({ type: 'investigate', incidentId: item.incidentId })
                      }
                    >
                      Investigate
                    </button>
                    <button
                      type="button"
                      className="rounded border border-rose-700 px-2 py-0.5 text-xs text-rose-200"
                      data-testid={`supplier-incident-cancel-${item.incidentId}`}
                      onClick={() =>
                        workflowMutation.mutate({ type: 'cancel', incidentId: item.incidentId })
                      }
                    >
                      Cancel
                    </button>
                  </>
                )}
                {item.status === 'investigating' && (
                  <>
                    <button
                      type="button"
                      className="rounded border border-slate-600 px-2 py-0.5 text-xs text-slate-200"
                      data-testid={`supplier-incident-resolve-${item.incidentId}`}
                      onClick={() =>
                        workflowMutation.mutate({ type: 'resolve', incidentId: item.incidentId })
                      }
                    >
                      Resolve
                    </button>
                    <button
                      type="button"
                      className="rounded border border-rose-700 px-2 py-0.5 text-xs text-rose-200"
                      data-testid={`supplier-incident-cancel-${item.incidentId}`}
                      onClick={() =>
                        workflowMutation.mutate({ type: 'cancel', incidentId: item.incidentId })
                      }
                    >
                      Cancel
                    </button>
                  </>
                )}
                {item.status === 'resolved' && (
                  <button
                    type="button"
                    className="rounded border border-slate-600 px-2 py-0.5 text-xs text-slate-200"
                    data-testid={`supplier-incident-close-${item.incidentId}`}
                    onClick={() =>
                      workflowMutation.mutate({ type: 'close', incidentId: item.incidentId })
                    }
                  >
                    Close
                  </button>
                )}
                {item.status === 'cancelled' && (
                  <button
                    type="button"
                    className="rounded border border-sky-600 px-2 py-0.5 text-xs text-sky-200"
                    data-testid={`supplier-incident-reopen-${item.incidentId}`}
                    onClick={() =>
                      workflowMutation.mutate({ type: 'reopen', incidentId: item.incidentId })
                    }
                  >
                    Reopen
                  </button>
                )}
                {!item.vendorRestrictionId &&
                  (item.status === 'open' || item.status === 'investigating') &&
                  (item.severity === 'high' || item.severity === 'critical') && (
                    <button
                      type="button"
                      className="rounded border border-rose-700 px-2 py-0.5 text-xs text-rose-200"
                      data-testid={`supplier-incident-restrict-${item.incidentId}`}
                      onClick={() =>
                        workflowMutation.mutate({ type: 'restrict', incidentId: item.incidentId })
                      }
                    >
                      Apply procurement hold
                    </button>
                  )}
              </div>
            </li>
          ))}
        </ul>
      )}

      {selectedParty && partyIncidentsQuery.data?.length === 0 && (
        <p className="mt-4 text-sm text-[var(--color-text-muted)]">No incidents recorded for {selectedParty.displayName}.</p>
      )}
    </section>
  )
}
