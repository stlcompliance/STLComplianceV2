import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import {
  createEmergencyPurchase,
  expeditedSubmitEmergencyPurchase,
  getEmergencyPurchases,
  issueEmergencyPurchaseOrder,
  listPendingEmergencyPurchases,
  managerOverrideApproveEmergencyPurchase,
} from '../api/client'
import type { EmergencyPurchaseResponse, PartResponse } from '../api/types'

interface EmergencyPurchasePanelProps {
  accessToken: string
  canCreate: boolean
  canOverrideApprove: boolean
  parts: PartResponse[]
  vendors: { partyId: string; displayName: string; partyKey: string }[]
}

function statusBadgeClass(status: string): string {
  switch (status) {
    case 'approved':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'submitted':
      return 'bg-amber-500/20 text-amber-200 ring-amber-500/40'
    case 'rejected':
      return 'bg-rose-500/20 text-rose-300 ring-rose-500/40'
    default:
      return 'bg-sky-500/20 text-sky-200 ring-sky-500/40'
  }
}

export function EmergencyPurchasePanel({
  accessToken,
  canCreate,
  canOverrideApprove,
  parts,
  vendors,
}: EmergencyPurchasePanelProps) {
  if (!canCreate && !canOverrideApprove) {
    return null
  }

  const queryClient = useQueryClient()
  const [selectedId, setSelectedId] = useState('')
  const [requestKey, setRequestKey] = useState('')
  const [title, setTitle] = useState('')
  const [emergencyReason, setEmergencyReason] = useState('')
  const [notes, setNotes] = useState('')
  const [vendorId, setVendorId] = useState('')
  const [partId, setPartId] = useState('')
  const [lineQty, setLineQty] = useState('1')
  const [justification, setJustification] = useState('')
  const [orderKey, setOrderKey] = useState('')

  const listQuery = useQuery({
    queryKey: ['supplyarr-emergency-purchases', accessToken],
    queryFn: () => getEmergencyPurchases(accessToken),
    enabled: canCreate || canOverrideApprove,
  })

  const pendingQuery = useQuery({
    queryKey: ['supplyarr-emergency-purchases-pending', accessToken],
    queryFn: () => listPendingEmergencyPurchases(accessToken),
    enabled: canOverrideApprove,
  })

  const selected: EmergencyPurchaseResponse | undefined = useMemo(
    () => listQuery.data?.find((x) => x.purchaseRequestId === selectedId),
    [listQuery.data, selectedId],
  )

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: ['supplyarr-emergency-purchases', accessToken] })
    void queryClient.invalidateQueries({
      queryKey: ['supplyarr-emergency-purchases-pending', accessToken],
    })
    void queryClient.invalidateQueries({ queryKey: ['supplyarr-purchase-requests'] })
    void queryClient.invalidateQueries({ queryKey: ['supplyarr-purchase-orders'] })
  }

  const createMutation = useMutation({
    mutationFn: () =>
      createEmergencyPurchase(accessToken, {
        requestKey,
        title,
        emergencyReason,
        vendorPartyId: vendorId,
        notes,
        lines: partId
          ? [{ partId, quantityRequested: Number(lineQty) || 1, notes: '' }]
          : [],
      }),
    onSuccess: (created) => {
      setSelectedId(created.purchaseRequestId)
      invalidate()
    },
  })

  const submitMutation = useMutation({
    mutationFn: () => expeditedSubmitEmergencyPurchase(accessToken, selectedId, notes),
    onSuccess: invalidate,
  })

  const approveMutation = useMutation({
    mutationFn: () =>
      managerOverrideApproveEmergencyPurchase(accessToken, selectedId, justification),
    onSuccess: invalidate,
  })

  const issueMutation = useMutation({
    mutationFn: () => issueEmergencyPurchaseOrder(accessToken, selectedId, orderKey),
    onSuccess: invalidate,
  })

  const canExpediteSubmit = selected?.status === 'draft'
  const canOverride = selected?.status === 'submitted' && canOverrideApprove
  const canIssue =
    selected?.status === 'approved' &&
    selected.managerOverrideApproved &&
    !selected.linkedPurchaseOrderId &&
    canCreate

  return (
    <section
      data-testid="emergency-purchase-panel"
      className="rounded-xl border border-rose-900/50 bg-rose-950/20 p-5 lg:col-span-2"
    >
      <h2 className="text-lg font-medium text-white">Emergency purchase</h2>
      <p className="mt-1 text-sm text-slate-400">
        Urgent procurement with expedited submit, administrator manager override, and linked PO issue.
      </p>

      {canOverrideApprove && (pendingQuery.data?.length ?? 0) > 0 ? (
        <div className="mt-4 rounded-lg border border-amber-800/50 bg-amber-950/30 p-3">
          <p className="text-sm font-medium text-amber-200">
            {pendingQuery.data!.length} awaiting manager override
          </p>
          <ul className="mt-2 space-y-1 text-sm">
            {pendingQuery.data!.map((item) => (
              <li key={item.purchaseRequestId}>
                <button
                  type="button"
                  className="text-left underline decoration-dotted hover:text-white"
                  onClick={() => setSelectedId(item.purchaseRequestId)}
                >
                  {item.title} ({item.requestKey})
                </button>
              </li>
            ))}
          </ul>
        </div>
      ) : null}

      {canCreate ? (
        <div className="mt-4 grid gap-2 sm:grid-cols-2">
          <input
            className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            placeholder="Request key"
            value={requestKey}
            onChange={(e) => setRequestKey(e.target.value)}
          />
          <input
            className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            placeholder="Title"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
          />
          <input
            className="sm:col-span-2 rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            placeholder="Emergency reason (required)"
            value={emergencyReason}
            onChange={(e) => setEmergencyReason(e.target.value)}
          />
          <select
            className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            value={vendorId}
            onChange={(e) => setVendorId(e.target.value)}
          >
            <option value="">Select vendor…</option>
            {vendors.map((v) => (
              <option key={v.partyId} value={v.partyId}>
                {v.displayName}
              </option>
            ))}
          </select>
          <select
            className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            value={partId}
            onChange={(e) => setPartId(e.target.value)}
          >
            <option value="">Select part…</option>
            {parts.map((p) => (
              <option key={p.partId} value={p.partId}>
                {p.displayName}
              </option>
            ))}
          </select>
          <button
            type="button"
            className="rounded-lg bg-rose-700 px-3 py-2 text-sm text-white disabled:opacity-50"
            disabled={
              createMutation.isPending ||
              !requestKey ||
              !title ||
              !emergencyReason ||
              !vendorId ||
              !partId
            }
            onClick={() => createMutation.mutate()}
          >
            Create emergency PR
          </button>
        </div>
      ) : null}

      <div className="mt-4">
        <label className="text-sm text-slate-400">
          Active emergency purchases
          <select
            className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            value={selectedId}
            onChange={(e) => setSelectedId(e.target.value)}
          >
            <option value="">Select…</option>
            {(listQuery.data ?? []).map((item) => (
              <option key={item.purchaseRequestId} value={item.purchaseRequestId}>
                {item.requestKey} — {item.status}
              </option>
            ))}
          </select>
        </label>
      </div>

      {selected ? (
        <div className="mt-4 rounded-lg border border-slate-800 p-3">
          <div className="flex justify-between gap-2">
            <div>
              <div className="font-medium">{selected.title}</div>
              <div className="text-sm text-slate-400">{selected.emergencyReason}</div>
            </div>
            <span
              className={`rounded-full px-2 py-0.5 text-xs ring-1 ${statusBadgeClass(selected.status)}`}
            >
              {selected.status}
            </span>
          </div>
          {selected.managerOverrideApproved ? (
            <p className="mt-2 text-sm text-emerald-300">Manager override approved</p>
          ) : null}
          {selected.linkedPurchaseOrderKey ? (
            <p className="mt-1 text-sm text-slate-400">PO: {selected.linkedPurchaseOrderKey}</p>
          ) : null}
        </div>
      ) : null}

      {canCreate && selected && canExpediteSubmit ? (
        <button
          type="button"
          className="mt-3 rounded-lg bg-amber-600 px-3 py-1.5 text-sm text-white disabled:opacity-50"
          disabled={submitMutation.isPending}
          onClick={() => submitMutation.mutate()}
        >
          Expedited submit
        </button>
      ) : null}

      {canOverride && selected ? (
        <div className="mt-3 flex flex-wrap gap-2">
          <input
            className="min-w-[12rem] flex-1 rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            placeholder="Override justification"
            value={justification}
            onChange={(e) => setJustification(e.target.value)}
          />
          <button
            type="button"
            className="rounded-lg bg-emerald-600 px-3 py-1.5 text-sm text-white disabled:opacity-50"
            disabled={approveMutation.isPending || !justification.trim()}
            onClick={() => approveMutation.mutate()}
          >
            Manager override approve
          </button>
        </div>
      ) : null}

      {canIssue && selected ? (
        <div className="mt-3 flex flex-wrap gap-2">
          <input
            className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            placeholder="PO order key"
            value={orderKey}
            onChange={(e) => setOrderKey(e.target.value)}
          />
          <button
            type="button"
            className="rounded-lg bg-sky-600 px-3 py-1.5 text-sm text-white disabled:opacity-50"
            disabled={issueMutation.isPending || !orderKey.trim()}
            onClick={() => issueMutation.mutate()}
          >
            Issue purchase order
          </button>
        </div>
      ) : null}
    </section>
  )
}
