import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import {
  cancelWarrantyClaim,
  closeWarrantyClaim,
  createWarrantyClaim,
  denyWarrantyClaim,
  listWarrantyClaims,
  recordWarrantyClaimVendorResponse,
  submitWarrantyClaim,
} from '../api/client'
import type { ExternalPartyResponse, PartResponse, PurchaseOrderResponse } from '../api/types'

const CLAIM_TYPES = ['defective', 'doa', 'premature_failure', 'other'] as const
const DISPOSITIONS = ['approved', 'partial_credit', 'replacement', 'denied'] as const

function statusClass(status: string): string {
  switch (status) {
    case 'draft':
      return 'bg-amber-500/20 text-amber-200'
    case 'submitted':
      return 'bg-sky-500/20 text-sky-200'
    case 'vendor_responded':
      return 'bg-violet-500/20 text-violet-200'
    case 'closed':
      return 'bg-emerald-500/20 text-emerald-200'
    case 'denied':
      return 'bg-rose-500/20 text-rose-200'
    default:
      return 'bg-slate-500/20 text-slate-300'
  }
}

interface WarrantyClaimsPanelProps {
  accessToken: string
  canManage: boolean
  vendors: ExternalPartyResponse[]
  parts: PartResponse[]
  issuedPurchaseOrders: PurchaseOrderResponse[]
}

export function WarrantyClaimsPanel({
  accessToken,
  canManage,
  vendors,
  parts,
  issuedPurchaseOrders,
}: WarrantyClaimsPanelProps) {
  const queryClient = useQueryClient()
  const [claimKey, setClaimKey] = useState('')
  const [claimType, setClaimType] = useState<(typeof CLAIM_TYPES)[number]>('defective')
  const [vendorPartyId, setVendorPartyId] = useState('')
  const [partId, setPartId] = useState('')
  const [purchaseOrderId, setPurchaseOrderId] = useState('')
  const [purchaseOrderLineId, setPurchaseOrderLineId] = useState('')
  const [quantityClaimed, setQuantityClaimed] = useState('1')
  const [problemDescription, setProblemDescription] = useState('')
  const [vendorRmaNumber, setVendorRmaNumber] = useState('')
  const [selectedClaimId, setSelectedClaimId] = useState('')
  const [vendorDisposition, setVendorDisposition] =
    useState<(typeof DISPOSITIONS)[number]>('approved')
  const [vendorResponseNotes, setVendorResponseNotes] = useState('')
  const [closureNotes, setClosureNotes] = useState('')
  const [denialReason, setDenialReason] = useState('')
  const [cancelReason, setCancelReason] = useState('')

  const claimsQuery = useQuery({
    queryKey: ['supplyarr-warranty-claims', accessToken],
    queryFn: () => listWarrantyClaims(accessToken),
    enabled: canManage,
  })

  const poLineOptions = useMemo(() => {
    const po = issuedPurchaseOrders.find((x) => x.purchaseOrderId === purchaseOrderId)
    return po?.lines ?? []
  }, [issuedPurchaseOrders, purchaseOrderId])

  const selectedClaim = claimsQuery.data?.find((x) => x.warrantyClaimId === selectedClaimId)

  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: ['supplyarr-warranty-claims', accessToken] })

  const createMutation = useMutation({
    mutationFn: () =>
      createWarrantyClaim(accessToken, {
        claimKey,
        claimType,
        vendorPartyId,
        partId,
        quantityClaimed: Number(quantityClaimed),
        problemDescription,
        purchaseOrderId: purchaseOrderId || null,
        purchaseOrderLineId: purchaseOrderLineId || null,
        vendorRmaNumber: vendorRmaNumber || null,
      }),
    onSuccess: (claim) => {
      setSelectedClaimId(claim.warrantyClaimId)
      invalidate()
    },
  })

  const submitMutation = useMutation({
    mutationFn: (id: string) => submitWarrantyClaim(accessToken, id, {}),
    onSuccess: invalidate,
  })

  const vendorResponseMutation = useMutation({
    mutationFn: (id: string) =>
      recordWarrantyClaimVendorResponse(accessToken, id, {
        vendorDisposition,
        vendorResponseNotes,
        vendorRmaNumber: vendorRmaNumber || null,
      }),
    onSuccess: invalidate,
  })

  const closeMutation = useMutation({
    mutationFn: (id: string) => closeWarrantyClaim(accessToken, id, { closureNotes }),
    onSuccess: invalidate,
  })

  const denyMutation = useMutation({
    mutationFn: (id: string) => denyWarrantyClaim(accessToken, id, { denialReason }),
    onSuccess: invalidate,
  })

  const cancelMutation = useMutation({
    mutationFn: (id: string) => cancelWarrantyClaim(accessToken, id, { reason: cancelReason }),
    onSuccess: invalidate,
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2"
      data-testid="warranty-claims-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Warranty claims</h2>
      <p className="mt-1 text-sm text-slate-400">
        File and track vendor warranty claims tied to parts and procurement receipts.
      </p>

      <div className="mt-4 grid gap-3 sm:grid-cols-2">
        <label className="text-sm text-slate-300">
          Claim key
          <input
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1"
            value={claimKey}
            onChange={(e) => setClaimKey(e.target.value)}
          />
        </label>
        <label className="text-sm text-slate-300">
          Type
          <select
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1"
            value={claimType}
            onChange={(e) => setClaimType(e.target.value as (typeof CLAIM_TYPES)[number])}
          >
            {CLAIM_TYPES.map((t) => (
              <option key={t} value={t}>
                {t}
              </option>
            ))}
          </select>
        </label>
        <label className="text-sm text-slate-300">
          Vendor
          <select
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1"
            value={vendorPartyId}
            onChange={(e) => setVendorPartyId(e.target.value)}
          >
            <option value="">Select vendor</option>
            {vendors.map((v) => (
              <option key={v.partyId} value={v.partyId}>
                {v.displayName}
              </option>
            ))}
          </select>
        </label>
        <label className="text-sm text-slate-300">
          Part
          <select
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1"
            value={partId}
            onChange={(e) => setPartId(e.target.value)}
          >
            <option value="">Select part</option>
            {parts.map((p) => (
              <option key={p.partId} value={p.partId}>
                {p.partKey} — {p.displayName}
              </option>
            ))}
          </select>
        </label>
        <label className="text-sm text-slate-300">
          Purchase order (optional)
          <select
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1"
            value={purchaseOrderId}
            onChange={(e) => {
              setPurchaseOrderId(e.target.value)
              setPurchaseOrderLineId('')
            }}
          >
            <option value="">None</option>
            {issuedPurchaseOrders.map((po) => (
              <option key={po.purchaseOrderId} value={po.purchaseOrderId}>
                {po.orderKey}
              </option>
            ))}
          </select>
        </label>
        <label className="text-sm text-slate-300">
          PO line (optional)
          <select
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1"
            value={purchaseOrderLineId}
            onChange={(e) => setPurchaseOrderLineId(e.target.value)}
            disabled={!purchaseOrderId}
          >
            <option value="">None</option>
            {poLineOptions.map((line) => (
              <option key={line.lineId} value={line.lineId}>
                Line {line.lineNumber} · qty {line.quantityOrdered}
              </option>
            ))}
          </select>
        </label>
        <label className="text-sm text-slate-300">
          Quantity claimed
          <input
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1"
            value={quantityClaimed}
            onChange={(e) => setQuantityClaimed(e.target.value)}
          />
        </label>
        <label className="text-sm text-slate-300 sm:col-span-2">
          Problem description
          <textarea
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1"
            rows={2}
            value={problemDescription}
            onChange={(e) => setProblemDescription(e.target.value)}
          />
        </label>
      </div>

      <button
        type="button"
        className="mt-3 rounded bg-sky-600 px-3 py-1.5 text-sm text-white hover:bg-sky-500"
        disabled={createMutation.isPending}
        onClick={() => createMutation.mutate()}
      >
        Create draft claim
      </button>

      {claimsQuery.isLoading && (
        <p className="mt-4 text-sm text-slate-500">Loading warranty claims…</p>
      )}

      {claimsQuery.data && claimsQuery.data.length > 0 && (
        <ul className="mt-4 divide-y divide-slate-800 rounded border border-slate-800 text-sm">
          {claimsQuery.data.map((claim) => (
            <li key={claim.warrantyClaimId} className="px-3 py-2">
              <button
                type="button"
                className="w-full text-left"
                onClick={() => setSelectedClaimId(claim.warrantyClaimId)}
              >
                <span className={`rounded px-2 py-0.5 text-xs ${statusClass(claim.status)}`}>
                  {claim.status}
                </span>{' '}
                <span className="font-medium text-slate-100">{claim.claimKey}</span>
                <span className="text-slate-400">
                  {' '}
                  · {claim.partKey} · {claim.vendorDisplayName}
                </span>
              </button>
            </li>
          ))}
        </ul>
      )}

      {selectedClaim && (
        <div className="mt-4 rounded border border-slate-800 bg-slate-950/60 p-3 text-sm">
          <p className="font-medium text-slate-100">
            {selectedClaim.claimKey} — {selectedClaim.status}
          </p>
          <p className="mt-1 text-slate-400">{selectedClaim.problemDescription}</p>

          {selectedClaim.status === 'draft' && (
            <button
              type="button"
              className="mt-2 mr-2 rounded bg-violet-600 px-2 py-1 text-xs text-white"
              onClick={() => submitMutation.mutate(selectedClaim.warrantyClaimId)}
            >
              Submit to vendor
            </button>
          )}

          {selectedClaim.status === 'submitted' && (
            <>
              <label className="mt-2 block text-slate-300">
                Vendor disposition
                <select
                  className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1"
                  value={vendorDisposition}
                  onChange={(e) =>
                    setVendorDisposition(e.target.value as (typeof DISPOSITIONS)[number])
                  }
                >
                  {DISPOSITIONS.map((d) => (
                    <option key={d} value={d}>
                      {d}
                    </option>
                  ))}
                </select>
              </label>
              <textarea
                className="mt-2 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1"
                rows={2}
                placeholder="Vendor response notes"
                value={vendorResponseNotes}
                onChange={(e) => setVendorResponseNotes(e.target.value)}
              />
              <button
                type="button"
                className="mt-2 rounded bg-violet-600 px-2 py-1 text-xs text-white"
                onClick={() => vendorResponseMutation.mutate(selectedClaim.warrantyClaimId)}
              >
                Record vendor response
              </button>
              <textarea
                className="mt-2 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1"
                rows={2}
                placeholder="Denial reason"
                value={denialReason}
                onChange={(e) => setDenialReason(e.target.value)}
              />
              <button
                type="button"
                className="mt-2 ml-2 rounded bg-rose-600 px-2 py-1 text-xs text-white"
                onClick={() => denyMutation.mutate(selectedClaim.warrantyClaimId)}
              >
                Deny claim
              </button>
            </>
          )}

          {selectedClaim.status === 'vendor_responded' &&
            selectedClaim.vendorDisposition !== 'denied' && (
              <>
                <textarea
                  className="mt-2 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1"
                  rows={2}
                  placeholder="Closure notes"
                  value={closureNotes}
                  onChange={(e) => setClosureNotes(e.target.value)}
                />
                <button
                  type="button"
                  className="mt-2 rounded bg-emerald-600 px-2 py-1 text-xs text-white"
                  onClick={() => closeMutation.mutate(selectedClaim.warrantyClaimId)}
                >
                  Close claim
                </button>
              </>
            )}

          {(selectedClaim.status === 'draft' || selectedClaim.status === 'submitted') && (
            <>
              <textarea
                className="mt-2 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1"
                rows={2}
                placeholder="Cancel reason"
                value={cancelReason}
                onChange={(e) => setCancelReason(e.target.value)}
              />
              <button
                type="button"
                className="mt-2 rounded bg-slate-600 px-2 py-1 text-xs text-white"
                onClick={() => cancelMutation.mutate(selectedClaim.warrantyClaimId)}
              >
                Cancel claim
              </button>
            </>
          )}
        </div>
      )}
    </section>
  )
}
