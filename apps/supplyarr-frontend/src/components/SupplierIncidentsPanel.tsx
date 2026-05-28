import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import {
  applySupplierIncidentProcurementRestriction,
  closeSupplierIncident,
  createSupplierIncident,
  listPartySupplierIncidents,
  listSupplierIncidents,
  resolveSupplierIncident,
  startSupplierIncidentInvestigation,
} from '../api/client'
import type { ExternalPartyResponse } from '../api/types'

const INCIDENT_TYPES = ['quality', 'delivery', 'compliance', 'safety', 'other'] as const
const SEVERITIES = ['low', 'medium', 'high', 'critical'] as const

interface SupplierIncidentsPanelProps {
  accessToken: string
  canManage: boolean
  incidentParties: ExternalPartyResponse[]
}

function statusClass(status: string): string {
  switch (status) {
    case 'open':
      return 'bg-amber-500/20 text-amber-200'
    case 'investigating':
      return 'bg-sky-500/20 text-sky-200'
    case 'resolved':
      return 'bg-emerald-500/20 text-emerald-200'
    case 'closed':
      return 'bg-slate-500/20 text-slate-300'
    default:
      return 'bg-rose-500/20 text-rose-200'
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
  const [resolutionNotes, setResolutionNotes] = useState('')

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
      type: 'investigate' | 'resolve' | 'close' | 'restrict'
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
      return applySupplierIncidentProcurementRestriction(accessToken, action.incidentId, {
        restrictionKey: `incident-${incidentKey || action.incidentId.slice(0, 8)}`,
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
        Track quality, delivery, and compliance issues for vendors and suppliers. Apply procurement
        holds via vendor restrictions when needed.
      </p>

      {openQuery.data && (
        <p className="mt-3 text-sm text-slate-500">
          {openQuery.data.length} open incident{openQuery.data.length === 1 ? '' : 's'} tenant-wide
        </p>
      )}

      <div className="mt-4 grid gap-4 md:grid-cols-2">
        <label className="block text-sm text-slate-400 md:col-span-2">
          Party
          <select
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
            value={selectedPartyId}
            onChange={(event) => setSelectedPartyId(event.target.value)}
          >
            <option value="">Select vendor or supplier…</option>
            {incidentParties.map((party) => (
              <option key={party.partyId} value={party.partyId}>
                {party.partyType} · {party.partyKey} · {party.displayName}
              </option>
            ))}
          </select>
        </label>

        {selectedPartyId && (
          <>
            <label className="block text-sm text-slate-400">
              Incident key
              <input
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
                value={incidentKey}
                onChange={(event) => setIncidentKey(event.target.value)}
              />
            </label>
            <label className="block text-sm text-slate-400">
              Type
              <select
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
            <label className="block text-sm text-slate-400">
              Severity
              <select
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
            <label className="block text-sm text-slate-400 md:col-span-2">
              Title
              <input
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
                value={title}
                onChange={(event) => setTitle(event.target.value)}
              />
            </label>
            <label className="block text-sm text-slate-400 md:col-span-2">
              Description
              <textarea
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
                rows={2}
                value={description}
                onChange={(event) => setDescription(event.target.value)}
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
            <li key={item.incidentId} className="px-3 py-3">
              <div className="flex flex-wrap items-start justify-between gap-2">
                <div>
                  <div className="font-medium text-slate-100">
                    {item.incidentKey} · {item.title}
                  </div>
                  <div className="text-xs text-slate-500">
                    {item.incidentType} · {item.severity}
                    {item.vendorRestrictionId ? ' · restriction applied' : ''}
                  </div>
                </div>
                <span className={`rounded px-2 py-0.5 text-xs ${statusClass(item.status)}`}>
                  {item.status}
                </span>
              </div>
              <div className="mt-2 flex flex-wrap gap-2">
                {item.status === 'open' && (
                  <button
                    type="button"
                    className="rounded border border-slate-600 px-2 py-0.5 text-xs text-slate-200"
                    onClick={() =>
                      workflowMutation.mutate({ type: 'investigate', incidentId: item.incidentId })
                    }
                  >
                    Investigate
                  </button>
                )}
                {item.status === 'investigating' && (
                  <button
                    type="button"
                    className="rounded border border-slate-600 px-2 py-0.5 text-xs text-slate-200"
                    onClick={() =>
                      workflowMutation.mutate({ type: 'resolve', incidentId: item.incidentId })
                    }
                  >
                    Resolve
                  </button>
                )}
                {item.status === 'resolved' && (
                  <button
                    type="button"
                    className="rounded border border-slate-600 px-2 py-0.5 text-xs text-slate-200"
                    onClick={() =>
                      workflowMutation.mutate({ type: 'close', incidentId: item.incidentId })
                    }
                  >
                    Close
                  </button>
                )}
                {!item.vendorRestrictionId &&
                  (item.status === 'open' || item.status === 'investigating') &&
                  (item.severity === 'high' || item.severity === 'critical') && (
                    <button
                      type="button"
                      className="rounded border border-rose-700 px-2 py-0.5 text-xs text-rose-200"
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
        <p className="mt-4 text-sm text-slate-500">No incidents recorded for {selectedParty.displayName}.</p>
      )}
    </section>
  )
}
