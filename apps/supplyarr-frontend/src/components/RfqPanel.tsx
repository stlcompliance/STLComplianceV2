import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import { ControlledSelect, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'

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
import { CURRENCY_OPTIONS, toPartPickerOptions } from '../forms/controlledFormHelpers'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'

interface RfqPanelProps {
  accessToken: string
  canManage: boolean
  canAward: boolean
  parts: PartResponse[]
  vendors: { partyId: string; displayName: string; partyKey: string }[]
  vendorDirectory: {
    partyId: string
    displayName: string
    partyKey: string
    approvalStatus: string
    status: string
  }[]
}

export function RfqPanel({ accessToken, canManage, canAward, parts, vendors, vendorDirectory }: RfqPanelProps) {
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
  const [quoteCurrency, setQuoteCurrency] = useState('USD')
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
  const portalOrigin = typeof window === 'undefined' ? '' : window.location.origin
  const draftQuotes = useMemo(
    () => selectedRfq?.quotes.filter((q) => q.status === 'draft') ?? [],
    [selectedRfq],
  )
  const quoteAnalytics = useMemo(() => {
    if (!selectedRfq) {
      return []
    }

    return selectedRfq.invitations.map((invite) => {
      const vendorProfile = vendorDirectory.find((vendor) => vendor.partyId === invite.vendorPartyId)
      const submittedQuotes = selectedRfq.quotes.filter((quote) => quote.vendorPartyId === invite.vendorPartyId)
      const submittedQuote =
        submittedQuotes.find((quote) => quote.status === 'submitted') ?? submittedQuotes[0] ?? null
      const responseDays =
        invite.invitedAt && submittedQuote?.submittedAt
          ? Math.max(0, (new Date(submittedQuote.submittedAt).getTime() - new Date(invite.invitedAt).getTime()) / (1000 * 60 * 60 * 24))
          : null
      const bestPriceCount = selectedRfq.quotes
        .filter((quote) => quote.status === 'submitted' && quote.totalAmount != null)
        .reduce((count, quote) => {
          const lowestTotal = Math.min(
            ...selectedRfq.quotes
              .filter((candidate) => candidate.status === 'submitted' && candidate.totalAmount != null)
              .map((candidate) => candidate.totalAmount!),
          )
          return count + (quote.vendorPartyId === invite.vendorPartyId && quote.totalAmount === lowestTotal ? 1 : 0)
        }, 0)
      return {
        vendorPartyId: invite.vendorPartyId,
        vendorDisplayName: invite.vendorDisplayName,
        approvalStatus: vendorProfile?.approvalStatus ?? 'unknown',
        vendorStatus: vendorProfile?.status ?? 'unknown',
        responseDays,
        submittedQuote,
        quoteCount: submittedQuotes.filter((quote) => quote.status === 'submitted').length,
        bestPriceCount,
      }
    })
  }, [selectedRfq, vendorDirectory])

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
    mutationFn: () => {
      const selectedVendor = vendorDirectory.find((vendor) => vendor.partyId === inviteVendorId)
      if (
        !selectedVendor ||
        selectedVendor.approvalStatus !== 'approved' ||
        selectedVendor.status !== 'active'
      ) {
        throw new Error('Select an approved, active vendor')
      }
      return inviteRfqVendors(accessToken, selectedRfqId, [inviteVendorId])
    },
    onSuccess: invalidate,
  })

  const createQuoteMutation = useMutation({
    mutationFn: () => {
      const selectedVendor = vendorDirectory.find((vendor) => vendor.partyId === quoteVendorId)
      if (
        !selectedVendor ||
        selectedVendor.approvalStatus !== 'approved' ||
        selectedVendor.status !== 'active'
      ) {
        throw new Error('Select an approved, active vendor')
      }
      return createVendorQuote(accessToken, selectedRfqId, {
        vendorPartyId: quoteVendorId,
        quoteKey,
        currencyCode: quoteCurrency,
        notes: '',
      })
    },
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

  const existingRfqKeys = useMemo(() => (rfqsQuery.data ?? []).map((rfq) => rfq.rfqKey), [rfqsQuery.data])
  const quoteVendor = selectedRfq?.invitations.find((invite) => invite.vendorPartyId === quoteVendorId)
  const quoteKeySource = quoteVendor ? `${selectedRfq?.rfqKey ?? ''}-${quoteVendor.vendorDisplayName}` : ''
  const prKeySource = selectedRfq?.title ?? ''
  const vendorPortalLinks = useMemo(
    () =>
      (selectedRfq?.invitations ?? []).map((invite) => ({
        ...invite,
        portalHref: invite.portalUrl ? `${portalOrigin}${invite.portalUrl}` : '',
      })),
    [portalOrigin, selectedRfq?.invitations],
  )
  const rfqOptions = useMemo<PickerOption[]>(
    () =>
      (rfqsQuery.data ?? []).map((rfq) => ({
        value: rfq.rfqId,
        label: `${rfq.rfqKey} · ${rfq.status} · ${rfq.title}`,
      })),
    [rfqsQuery.data],
  )
  const selectedRfqOption = useMemo<PickerOption | undefined>(
    () => rfqOptions.find((option) => option.value === selectedRfqId),
    [rfqOptions, selectedRfqId],
  )
  const vendorPickerOptions = useMemo(
    () =>
      vendors.map((vendor) => {
        const vendorProfile = vendorDirectory.find((profile) => profile.partyId === vendor.partyId)
        return {
          value: vendor.partyId,
          label: `${vendor.displayName} (${vendor.partyKey})`,
          inactive:
            !vendorProfile ||
            vendorProfile.approvalStatus !== 'approved' ||
            vendorProfile.status !== 'active',
        }
      }),
    [vendors, vendorDirectory],
  )
  const quoteVendorPickerOptions = useMemo(
    () =>
      (selectedRfq?.invitations ?? []).map((invite) => {
        const vendorProfile = vendorDirectory.find((vendor) => vendor.partyId === invite.vendorPartyId)
        return {
          value: invite.vendorPartyId,
          label: `${invite.vendorDisplayName}${vendorProfile ? ` (${vendorProfile.approvalStatus} · ${vendorProfile.status})` : ''}`,
          inactive:
            !vendorProfile ||
            vendorProfile.approvalStatus !== 'approved' ||
            vendorProfile.status !== 'active',
        }
      }),
    [selectedRfq?.invitations, vendorDirectory],
  )

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
        <label htmlFor="rfq-create-title" className="block text-sm text-slate-400 sm:col-span-2">
          RFQ title
          <input
            id="rfq-create-title"
            className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
          />
        </label>
        <ControlledSelect
          label="Part (optional line)"
          value={partId}
          onChange={setPartId}
          options={toPartPickerOptions(parts)}
          emptyLabel="Part (optional line)"
        />
      </div>
      <GeneratedKeyFieldGroup
        sourceLabel={title}
        existingKeys={existingRfqKeys}
        onKeyChange={setRfqKey}
        domain="purchase"
        kind="rfq"
        label="RFQ key"
      />
      <button
        type="button"
        className="mt-3 rounded bg-sky-600 px-3 py-1.5 text-sm text-white disabled:opacity-50"
        disabled={createMutation.isPending || !rfqKey}
        onClick={() => createMutation.mutate()}
      >
        Create RFQ
      </button>

      {rfqsQuery.isLoading && <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading RFQs…</p>}
      {rfqsQuery.data && rfqsQuery.data.length > 0 && (
        <div className="mt-4">
          <StaticSearchPicker
            id="rfq-select"
            label="Select RFQ"
            value={selectedRfqId}
            onChange={setSelectedRfqId}
            options={rfqOptions}
            selectedOption={selectedRfqOption}
            placeholder="Search RFQs…"
            testId="rfq-picker"
          />
        </div>
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
              <ControlledSelect
                label="Invite vendor"
                value={inviteVendorId}
                onChange={setInviteVendorId}
                options={vendorPickerOptions}
                emptyLabel="Invite vendor"
              />
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
              <ControlledSelect
                label="Quote vendor"
                value={quoteVendorId}
                onChange={setQuoteVendorId}
                options={quoteVendorPickerOptions}
                emptyLabel="Quote vendor"
              />
              <ControlledSelect
                label="Currency"
                value={quoteCurrency}
                onChange={setQuoteCurrency}
                options={CURRENCY_OPTIONS}
              />
              <div className="sm:col-span-2">
                <GeneratedKeyFieldGroup
                  sourceLabel={quoteKeySource}
                  existingKeys={selectedRfq.quotes.map((quote) => quote.quoteKey)}
                  onKeyChange={setQuoteKey}
                  domain="purchase"
                  kind="quote"
                  label="Quote key"
                />
              </div>
              <label htmlFor="rfq-quote-unit-price" className="block text-xs text-slate-400">
                Unit price
                <input
                  id="rfq-quote-unit-price"
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs"
                  value={quoteUnitPrice}
                  onChange={(e) => setQuoteUnitPrice(e.target.value)}
                />
              </label>
              <label htmlFor="rfq-quote-lead-days" className="block text-xs text-slate-400">
                Lead days
                <input
                  id="rfq-quote-lead-days"
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs"
                  value={quoteLeadDays}
                  onChange={(e) => setQuoteLeadDays(e.target.value)}
                />
              </label>
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
                  <tr className="text-[var(--color-text-muted)]">
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

          {quoteAnalytics.length > 0 && (
            <div>
              <h3 className="text-xs font-semibold uppercase text-slate-400">Quote analytics</h3>
              <ul className="mt-2 grid gap-2 sm:grid-cols-2">
                {quoteAnalytics.map((row) => (
                  <li key={row.vendorPartyId} className="rounded border border-slate-800 bg-slate-950/50 p-2">
                    <div className="flex items-start justify-between gap-2">
                      <div>
                        <div className="font-medium text-slate-100">{row.vendorDisplayName}</div>
                        <div className="text-xs text-[var(--color-text-muted)]">
                          {row.submittedQuote ? `Quote ${row.submittedQuote.quoteKey}` : 'No submitted quote'}
                        </div>
                        <div className="mt-1 text-[11px] uppercase tracking-wide text-[var(--color-text-muted)]">
                          {row.approvalStatus} · {row.vendorStatus}
                        </div>
                      </div>
                      {row.bestPriceCount > 0 ? (
                        <span className="rounded bg-emerald-500/20 px-2 py-0.5 text-[10px] uppercase tracking-wide text-emerald-300">
                          Best price
                        </span>
                      ) : null}
                    </div>
                    <p className="mt-2 text-xs text-slate-400">
                      Response time {formatDays(row.responseDays)} · Submitted quotes {row.quoteCount}
                    </p>
                    {row.approvalStatus !== 'approved' || row.vendorStatus !== 'active' ? (
                      <p className="mt-1 text-xs text-amber-300">
                        Source attention: {row.approvalStatus} · {row.vendorStatus}
                      </p>
                    ) : (
                      <p className="mt-1 text-xs text-emerald-300">Approved source</p>
                    )}
                  </li>
                ))}
              </ul>
            </div>
          )}

          {vendorPortalLinks.length > 0 && (
            <div>
              <h3 className="text-xs font-semibold uppercase text-slate-400">Vendor portal access</h3>
              <ul className="mt-2 grid gap-2">
                {vendorPortalLinks.map((invite) => (
                  <li key={invite.invitationId} className="rounded border border-slate-800 bg-slate-950/50 p-2">
                    <div className="flex flex-wrap items-start justify-between gap-2">
                      <div>
                        <div className="font-medium text-slate-100">{invite.vendorDisplayName}</div>
                        <div className="text-xs text-[var(--color-text-muted)]">
                          Portal code issued {formatDateTime(invite.portalAccessCodeIssuedAt)} · expires{' '}
                          {formatDateTime(invite.portalAccessExpiresAt)}
                        </div>
                      </div>
                      <span className="rounded bg-slate-800 px-2 py-0.5 text-[10px] uppercase tracking-wide text-slate-300">
                        {invite.status}
                      </span>
                    </div>
                    <div className="mt-2 grid gap-2 sm:grid-cols-2">
                      <label className="block text-xs text-slate-400">
                        Access code
                        <input
                          readOnly
                          className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-xs text-slate-200"
                          value={invite.portalAccessCode}
                        />
                      </label>
                      <label className="block text-xs text-slate-400">
                        Portal link
                        <input
                          readOnly
                          className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-xs text-slate-200"
                          value={invite.portalHref}
                        />
                      </label>
                    </div>
                    {invite.portalHref && (
                      <div className="mt-2">
                        <a
                          className="text-xs text-sky-400 underline"
                          href={invite.portalHref}
                          target="_blank"
                          rel="noreferrer"
                        >
                          Open vendor portal
                        </a>
                      </div>
                    )}
                  </li>
                ))}
              </ul>
            </div>
          )}

          {selectedRfq.status === 'submitted' && canAward && (
            <div className="flex flex-wrap items-end gap-2">
              <label htmlFor="rfq-winning-quote" className="block text-xs text-slate-400">
                Winning quote
                <select
                  id="rfq-winning-quote"
                  className="mt-1 rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs"
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
              </label>
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
              <div className="min-w-[12rem] flex-1">
                <GeneratedKeyFieldGroup
                  sourceLabel={prKeySource}
                  existingKeys={[]}
                  onKeyChange={setPrKeyFromRfq}
                  domain="purchase"
                  kind="request"
                  label="PR key from RFQ"
                />
              </div>
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

function formatDays(value: number | null): string {
  if (value == null) {
    return 'n/a'
  }
  const rounded = Math.round(value * 10) / 10
  return `${rounded} day${rounded === 1 ? '' : 's'}`
}

function formatDateTime(value: string): string {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleString()
}
