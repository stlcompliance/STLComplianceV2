import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import {
  createPurchaseRequestFromRfq,
  createRfq,
  createVendorQuote,
  getRfq,
  getRfqQuoteComparison,
  getRfqs,
  inviteRfqVendors,
  selectRfqVendorQuote,
  submitRfq,
  submitVendorQuote,
  upsertVendorQuoteLine,
} from '../api/client'
import type { PartResponse, RfqResponse } from '../api/types'

interface RfqPanelProps {
  accessToken: string
  canManage: boolean
  canAward: boolean
  parts: PartResponse[]
  vendors: { partyId: string; displayName: string; partyKey: string }[]
}

export function RfqPanel({ accessToken, canManage, canAward, parts, vendors }: RfqPanelProps) {
  const queryClient = useQueryClient()
  const [selectedRfqId, setSelectedRfqId] = useState('')
  const [rfqKey, setRfqKey] = useState('')
  const [title, setTitle] = useState('')
  const [notes] = useState('')
  const [partId, setPartId] = useState('')
  const [lineQty] = useState('1')
  const [inviteVendorId, setInviteVendorId] = useState('')
  const [quoteVendorId, setQuoteVendorId] = useState('')
  const [quoteKey, setQuoteKey] = useState('')
  const [quoteUnitPrice, setQuoteUnitPrice] = useState('')
  const [quoteLeadDays, setQuoteLeadDays] = useState('')
  const [selectedQuoteId, setSelectedQuoteId] = useState('')
  const [prKeyFromRfq, setPrKeyFromRfq] = useState('')

  const rfqsQuery = useQuery({
    queryKey: ['supplyarr-rfqs', accessToken],
    queryFn: () => getRfqs(accessToken),
    enabled: canManage,
  })

  const rfqDetailQuery = useQuery({
    queryKey: ['supplyarr-rfq', accessToken, selectedRfqId],
    queryFn: () => getRfq(accessToken, selectedRfqId),
    enabled: Boolean(selectedRfqId),
  })

  const comparisonQuery = useQuery({
    queryKey: ['supplyarr-rfq-comparison', accessToken, selectedRfqId],
    queryFn: () => getRfqQuoteComparison(accessToken, selectedRfqId),
    enabled: Boolean(selectedRfqId),
  })

  const selectedRfq: RfqResponse | undefined = rfqDetailQuery.data
  const draftQuotes = useMemo(
    () => selectedRfq?.quotes.filter((q) => q.status === 'draft') ?? [],
    [selectedRfq],
  )

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: ['supplyarr-rfqs', accessToken] })
    if (selectedRfqId) {
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-rfq', accessToken, selectedRfqId] })
      void queryClient.invalidateQueries({
        queryKey: ['supplyarr-rfq-comparison', accessToken, selectedRfqId],
      })
    }
  }

  const createMutation = useMutation({
    mutationFn: () =>
      createRfq(accessToken, {
        rfqKey,
        title,
        notes,
        lines: partId
          ? [{ partId, quantityRequested: Number(lineQty) || 1, notes: '' }]
          : undefined,
      }),
    onSuccess: (created) => {
      setSelectedRfqId(created.rfqId)
      invalidate()
    },
  })

  const submitRfqMutation = useMutation({
    mutationFn: () => submitRfq(accessToken, selectedRfqId),
    onSuccess: invalidate,
  })

  const inviteMutation = useMutation({
    mutationFn: () => inviteRfqVendors(accessToken, selectedRfqId, [inviteVendorId]),
    onSuccess: invalidate,
  })

  const createQuoteMutation = useMutation({
    mutationFn: () =>
      createVendorQuote(accessToken, selectedRfqId, {
        vendorPartyId: quoteVendorId,
        quoteKey,
        currencyCode: 'USD',
        notes: '',
      }),
    onSuccess: invalidate,
  })

  const saveQuoteLineMutation = useMutation({
    mutationFn: async () => {
      const quote = draftQuotes[0]
      const line = selectedRfq?.lines[0]
      if (!quote || !line) {
        throw new Error('Select an RFQ with a draft quote and line')
      }
      return upsertVendorQuoteLine(accessToken, selectedRfqId, quote.vendorQuoteId, {
        rfqLineId: line.lineId,
        unitPrice: Number(quoteUnitPrice),
        quantityQuoted: line.quantityRequested,
        leadTimeDays: quoteLeadDays ? Number(quoteLeadDays) : null,
        notes: '',
      })
    },
    onSuccess: invalidate,
  })

  const submitQuoteMutation = useMutation({
    mutationFn: () => submitVendorQuote(accessToken, selectedRfqId, draftQuotes[0]!.vendorQuoteId),
    onSuccess: invalidate,
  })

  const selectQuoteMutation = useMutation({
    mutationFn: () => selectRfqVendorQuote(accessToken, selectedRfqId, selectedQuoteId),
    onSuccess: invalidate,
  })

  const createPrMutation = useMutation({
    mutationFn: () =>
      createPurchaseRequestFromRfq(accessToken, selectedRfqId, {
        requestKey: prKeyFromRfq,
        title: selectedRfq?.title,
        notes: selectedRfq?.notes,
      }),
    onSuccess: invalidate,
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2"
      data-testid="rfq-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">RFQs &amp; quote comparison</h2>
      <p className="mt-1 text-sm text-slate-400">
        Request vendor quotes, compare pricing and lead time, award a winner, and create a purchase request.
      </p>

      <div className="mt-4 grid gap-3 sm:grid-cols-3">
        <input
          className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm"
          placeholder="RFQ key"
          value={rfqKey}
          onChange={(e) => setRfqKey(e.target.value)}
        />
        <input
          className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm"
          placeholder="Title"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
        />
        <select
          className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm"
          value={partId}
          onChange={(e) => setPartId(e.target.value)}
        >
          <option value="">Part (optional line)</option>
          {parts.map((p) => (
            <option key={p.partId} value={p.partId}>
              {p.partKey}
            </option>
          ))}
        </select>
      </div>
      <button
        type="button"
        className="mt-3 rounded bg-sky-600 px-3 py-1.5 text-sm text-white disabled:opacity-50"
        disabled={createMutation.isPending || !rfqKey}
        onClick={() => createMutation.mutate()}
      >
        Create RFQ
      </button>

      {rfqsQuery.isLoading && <p className="mt-3 text-sm text-slate-500">Loading RFQs…</p>}
      {rfqsQuery.data && rfqsQuery.data.length > 0 && (
        <select
          className="mt-4 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm"
          value={selectedRfqId}
          onChange={(e) => setSelectedRfqId(e.target.value)}
        >
          <option value="">Select RFQ</option>
          {rfqsQuery.data.map((r) => (
            <option key={r.rfqId} value={r.rfqId}>
              {r.rfqKey} · {r.status} · {r.title}
            </option>
          ))}
        </select>
      )}

      {selectedRfq && (
        <div className="mt-4 space-y-4 rounded border border-slate-700 p-3 text-sm">
          <div className="flex flex-wrap gap-2">
            <span className="font-medium text-slate-100">{selectedRfq.rfqKey}</span>
            <span className="rounded bg-slate-800 px-2 py-0.5 text-xs uppercase text-slate-300">
              {selectedRfq.status}
            </span>
            {selectedRfq.purchaseRequestId && (
              <span className="text-xs text-emerald-400">PR linked</span>
            )}
          </div>

          {selectedRfq.status === 'draft' && (
            <button
              type="button"
              className="rounded bg-sky-700 px-2 py-1 text-xs text-white"
              onClick={() => submitRfqMutation.mutate()}
            >
              Submit RFQ
            </button>
          )}

          {selectedRfq.status === 'submitted' && (
            <div className="flex flex-wrap items-end gap-2">
              <select
                className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs"
                value={inviteVendorId}
                onChange={(e) => setInviteVendorId(e.target.value)}
              >
                <option value="">Invite vendor</option>
                {vendors.map((v) => (
                  <option key={v.partyId} value={v.partyId}>
                    {v.displayName}
                  </option>
                ))}
              </select>
              <button
                type="button"
                className="rounded bg-slate-700 px-2 py-1 text-xs text-white"
                disabled={!inviteVendorId}
                onClick={() => inviteMutation.mutate()}
              >
                Invite
              </button>
            </div>
          )}

          {selectedRfq.status === 'submitted' && selectedRfq.invitations.length > 0 && (
            <div className="grid gap-2 sm:grid-cols-4">
              <select
                className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs"
                value={quoteVendorId}
                onChange={(e) => setQuoteVendorId(e.target.value)}
              >
                <option value="">Quote vendor</option>
                {selectedRfq.invitations.map((i) => (
                  <option key={i.vendorPartyId} value={i.vendorPartyId}>
                    {i.vendorDisplayName}
                  </option>
                ))}
              </select>
              <input
                className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs"
                placeholder="Quote key"
                value={quoteKey}
                onChange={(e) => setQuoteKey(e.target.value)}
              />
              <input
                className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs"
                placeholder="Unit price"
                value={quoteUnitPrice}
                onChange={(e) => setQuoteUnitPrice(e.target.value)}
              />
              <input
                className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs"
                placeholder="Lead days"
                value={quoteLeadDays}
                onChange={(e) => setQuoteLeadDays(e.target.value)}
              />
              <button
                type="button"
                className="rounded bg-slate-700 px-2 py-1 text-xs text-white"
                disabled={!quoteVendorId || !quoteKey}
                onClick={() => createQuoteMutation.mutate()}
              >
                Start quote
              </button>
              {draftQuotes.length > 0 && (
                <>
                  <button
                    type="button"
                    className="rounded bg-slate-700 px-2 py-1 text-xs text-white"
                    onClick={() => saveQuoteLineMutation.mutate()}
                  >
                    Save line price
                  </button>
                  <button
                    type="button"
                    className="rounded bg-slate-700 px-2 py-1 text-xs text-white"
                    onClick={() => submitQuoteMutation.mutate()}
                  >
                    Submit quote
                  </button>
                </>
              )}
            </div>
          )}

          {comparisonQuery.data && comparisonQuery.data.quoteSummaries.length > 0 && (
            <div>
              <h3 className="text-xs font-semibold uppercase text-slate-400">Quote comparison</h3>
              <table className="mt-2 w-full text-left text-xs">
                <thead>
                  <tr className="text-slate-500">
                    <th className="py-1">Vendor</th>
                    <th>Total</th>
                    <th>Lead (max days)</th>
                    <th>Lines</th>
                  </tr>
                </thead>
                <tbody>
                  {comparisonQuery.data.quoteSummaries.map((s) => (
                    <tr key={s.vendorQuoteId} className="border-t border-slate-800 text-slate-200">
                      <td className="py-1">
                        {s.vendorDisplayName}
                        {s.isSelected && (
                          <span className="ml-1 text-emerald-400">(awarded)</span>
                        )}
                      </td>
                      <td>{s.totalAmount?.toFixed(2) ?? '—'}</td>
                      <td>{s.maxLeadTimeDays ?? '—'}</td>
                      <td>{s.linesQuoted}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {comparisonQuery.data.lines.map((row) => (
                <div key={row.rfqLineId} className="mt-2 rounded bg-slate-950/60 p-2">
                  <div className="font-medium text-slate-200">
                    Line {row.lineNumber}: {row.partKey} (qty {row.quantityRequested})
                  </div>
                  <ul className="mt-1 text-slate-400">
                    {row.quotes.map((q) => (
                      <li key={`${row.rfqLineId}-${q.vendorQuoteId}`}>
                        {q.vendorDisplayName}: {q.unitPrice?.toFixed(2) ?? '—'}
                        {q.isLowestPrice ? ' · lowest' : ''}
                        {q.isFastestLeadTime ? ' · fastest' : ''}
                      </li>
                    ))}
                  </ul>
                </div>
              ))}
            </div>
          )}

          {selectedRfq.status === 'submitted' && canAward && (
            <div className="flex flex-wrap items-end gap-2">
              <select
                className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs"
                value={selectedQuoteId}
                onChange={(e) => setSelectedQuoteId(e.target.value)}
              >
                <option value="">Select winning quote</option>
                {selectedRfq.quotes
                  .filter((q) => q.status === 'submitted')
                  .map((q) => (
                    <option key={q.vendorQuoteId} value={q.vendorQuoteId}>
                      {q.vendorDisplayName} · {q.totalAmount?.toFixed(2)}
                    </option>
                  ))}
              </select>
              <button
                type="button"
                className="rounded bg-emerald-700 px-2 py-1 text-xs text-white"
                disabled={!selectedQuoteId}
                onClick={() => selectQuoteMutation.mutate()}
              >
                Award quote
              </button>
            </div>
          )}

          {selectedRfq.status === 'awarded' && !selectedRfq.purchaseRequestId && (
            <div className="flex flex-wrap items-end gap-2">
              <input
                className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs"
                placeholder="PR key from RFQ"
                value={prKeyFromRfq}
                onChange={(e) => setPrKeyFromRfq(e.target.value)}
              />
              <button
                type="button"
                className="rounded bg-emerald-700 px-2 py-1 text-xs text-white"
                disabled={!prKeyFromRfq}
                onClick={() => createPrMutation.mutate()}
              >
                Create purchase request
              </button>
            </div>
          )}
        </div>
      )}
    </section>
  )
}
