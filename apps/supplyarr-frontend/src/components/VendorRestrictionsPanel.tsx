import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import {
  createPartyVendorRestriction,
  getPartyVendorRestrictionEnforcement,
  liftVendorRestriction,
  listPartyVendorRestrictions,
  listVendorRestrictions,
} from '../api/client'
import type { ExternalPartyResponse } from '../api/types'

const SCOPE_OPTIONS = [
  { value: 'purchase_requests', label: 'Purchase requests' },
  { value: 'purchase_orders', label: 'Purchase orders' },
  { value: 'rfq_invitations', label: 'RFQ invitations' },
  { value: 'receiving', label: 'Receiving' },
  { value: 'all_procurement', label: 'All procurement' },
] as const

interface VendorRestrictionsPanelProps {
  accessToken: string
  canManage: boolean
  restrictableParties: ExternalPartyResponse[]
}

export function VendorRestrictionsPanel({
  accessToken,
  canManage,
  restrictableParties,
}: VendorRestrictionsPanelProps) {
  const queryClient = useQueryClient()
  const [selectedPartyId, setSelectedPartyId] = useState('')
  const [restrictionKey, setRestrictionKey] = useState('')
  const [reason, setReason] = useState('')
  const [selectedScopes, setSelectedScopes] = useState<string[]>(['all_procurement'])

  const activeQuery = useQuery({
    queryKey: ['supplyarr-vendor-restrictions', accessToken],
    queryFn: () => listVendorRestrictions(accessToken, 'active'),
    enabled: canManage,
  })

  const partyRestrictionsQuery = useQuery({
    queryKey: ['supplyarr-party-vendor-restrictions', accessToken, selectedPartyId],
    queryFn: () => listPartyVendorRestrictions(accessToken, selectedPartyId),
    enabled: Boolean(selectedPartyId),
  })

  const enforcementQuery = useQuery({
    queryKey: ['supplyarr-vendor-restriction-enforcement', accessToken, selectedPartyId],
    queryFn: () => getPartyVendorRestrictionEnforcement(accessToken, selectedPartyId),
    enabled: Boolean(selectedPartyId),
  })

  const selectedParty = useMemo(
    () => restrictableParties.find((p) => p.partyId === selectedPartyId),
    [restrictableParties, selectedPartyId],
  )

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: ['supplyarr-vendor-restrictions', accessToken] })
    void queryClient.invalidateQueries({
      queryKey: ['supplyarr-party-vendor-restrictions', accessToken, selectedPartyId],
    })
    void queryClient.invalidateQueries({
      queryKey: ['supplyarr-vendor-restriction-enforcement', accessToken, selectedPartyId],
    })
  }

  const createMutation = useMutation({
    mutationFn: () =>
      createPartyVendorRestriction(accessToken, selectedPartyId, {
        restrictionKey,
        scopes: selectedScopes,
        reason,
      }),
    onSuccess: () => {
      setRestrictionKey('')
      setReason('')
      invalidate()
    },
  })

  const liftMutation = useMutation({
    mutationFn: (restrictionId: string) =>
      liftVendorRestriction(accessToken, restrictionId, { liftNotes: 'Lifted from parties workspace' }),
    onSuccess: invalidate,
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2"
      data-testid="vendor-restrictions-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Vendor restrictions</h2>
      <p className="mt-1 text-sm text-slate-400">
        Block procurement activity by scope for vendor or supplier parties. Enforcement applies to purchase
        requests, purchase orders, RFQ invitations, and receiving.
      </p>

      {activeQuery.data && (
        <p className="mt-3 text-sm text-slate-500">
          {activeQuery.data.length} active restriction{activeQuery.data.length === 1 ? '' : 's'} tenant-wide
        </p>
      )}

      <div className="mt-4 grid gap-4 md:grid-cols-2">
        <label htmlFor="vendor-restriction-party" className="block text-sm text-slate-400 md:col-span-2">
          Vendor or supplier party
          <select
            id="vendor-restriction-party"
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
            value={selectedPartyId}
            onChange={(event) => setSelectedPartyId(event.target.value)}
          >
            <option value="">Select vendor or supplier…</option>
            {restrictableParties.map((party) => (
              <option key={party.partyId} value={party.partyId}>
                {party.partyType} · {party.partyKey} · {party.displayName}
              </option>
            ))}
          </select>
        </label>

        {selectedParty && enforcementQuery.data && (
          <div className="md:col-span-2 rounded-md border border-slate-800 p-3 text-sm">
            <span
              className={
                enforcementQuery.data.isBlocked
                  ? 'text-rose-300'
                  : 'text-emerald-300'
              }
            >
              {enforcementQuery.data.isBlocked ? 'Blocked' : 'Clear'} for procurement
            </span>
            {enforcementQuery.data.activeScopes.length > 0 && (
              <p className="mt-1 text-slate-500">
                Active scopes: {enforcementQuery.data.activeScopes.join(', ')}
              </p>
            )}
            {enforcementQuery.data.blockReason && (
              <p className="mt-1 text-slate-400">{enforcementQuery.data.blockReason}</p>
            )}
          </div>
        )}

        {selectedPartyId && (
          <>
            <label htmlFor="vendor-restriction-key" className="block text-sm text-slate-400">
              Restriction key
              <input
                id="vendor-restriction-key"
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
                value={restrictionKey}
                onChange={(event) => setRestrictionKey(event.target.value)}
                placeholder="e.g. quality-hold-2026"
              />
            </label>
            <label htmlFor="vendor-restriction-reason" className="block text-sm text-slate-400 md:col-span-2">
              Restriction reason
              <textarea
                id="vendor-restriction-reason"
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
                rows={2}
                value={reason}
                onChange={(event) => setReason(event.target.value)}
              />
            </label>
            <fieldset className="md:col-span-2">
              <legend className="text-sm font-medium text-slate-300">Scopes</legend>
              <div className="mt-2 flex flex-wrap gap-3">
                {SCOPE_OPTIONS.map((scope) => (
                  <label key={scope.value} htmlFor={`vendor-restriction-scope-${scope.value}`} className="flex items-center gap-2 text-sm text-slate-400">
                    <input
                      id={`vendor-restriction-scope-${scope.value}`}
                      type="checkbox"
                      checked={selectedScopes.includes(scope.value)}
                      onChange={(event) => {
                        if (event.target.checked) {
                          setSelectedScopes((prev) => [...prev, scope.value])
                        } else {
                          setSelectedScopes((prev) => prev.filter((x) => x !== scope.value))
                        }
                      }}
                    />
                    {scope.label}
                  </label>
                ))}
              </div>
            </fieldset>
            <button
              type="button"
              className="rounded bg-rose-700 px-3 py-1.5 text-sm text-white disabled:opacity-50 md:col-span-2 md:w-fit"
              disabled={
                createMutation.isPending
                || !restrictionKey.trim()
                || !reason.trim()
                || selectedScopes.length === 0
              }
              onClick={() => createMutation.mutate()}
            >
              Apply restriction
            </button>
          </>
        )}
      </div>

      {partyRestrictionsQuery.data && partyRestrictionsQuery.data.length > 0 && (
        <ul className="mt-4 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
          {partyRestrictionsQuery.data.map((item) => (
            <li key={item.restrictionId} className="flex flex-wrap items-center justify-between gap-2 px-3 py-3">
              <div>
                <div className="font-medium text-slate-100">
                  {item.restrictionKey} · {item.status}
                </div>
                <div className="text-xs text-slate-500">{item.scopes.join(', ')}</div>
                <p className="mt-1 text-slate-400">{item.reason}</p>
              </div>
              {item.status === 'active' && (
                <button
                  type="button"
                  className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-200 disabled:opacity-50"
                  disabled={liftMutation.isPending}
                  onClick={() => liftMutation.mutate(item.restrictionId)}
                >
                  Lift
                </button>
              )}
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
