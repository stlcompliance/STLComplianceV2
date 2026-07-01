import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'

import {
  createSupplierPortalQuote,
  getSupplierPortalRfq,
  submitSupplierPortalQuote,
  upsertSupplierPortalQuoteLine,
} from '../../api/client'
import type { SupplierPortalRfqLineResponse } from '../../api/types'
import { CURRENCY_OPTIONS } from '../../forms/controlledFormHelpers'
import {
  formatSupplierIdentityLabel,
  formatSupplierServiceTypes,
  humanizeSupplierUnitKind,
} from '../../utils/supplierPresentation'

type LineDraft = {
  unitPrice: string
  quantityQuoted: string
  leadTimeDays: string
  notes: string
}

export function SupplierQuotePortalPage() {
  const [searchParams] = useSearchParams()
  const queryClient = useQueryClient()
  const rfqId = searchParams.get('rfqId') ?? ''
  const accessCode = searchParams.get('accessCode') ?? ''
  const [quoteKey, setQuoteKey] = useState('')
  const [currencyCode, setCurrencyCode] = useState('USD')
  const [notes, setNotes] = useState('')
  const [lineDrafts, setLineDrafts] = useState<Record<string, LineDraft>>({})

  const portalQuery = useQuery({
    queryKey: ['supplyarr-supplier-quote-portal', rfqId, accessCode],
    queryFn: () => getSupplierPortalRfq(rfqId, accessCode),
    enabled: Boolean(rfqId && accessCode),
  })

  const portal = portalQuery.data
  const supplierIdentityLabel = portal
    ? formatSupplierIdentityLabel({
        supplierDisplayName: portal.supplierDisplayName,
        parentSupplierDisplayName: portal.parentSupplierDisplayName,
        supplierUnitKind: portal.supplierUnitKind,
      })
    : 'Supplier not recorded'
  const currentQuote = portal?.supplierQuoteId ?? null

  useEffect(() => {
    if (!portal) {
      return
    }

    setLineDrafts((current) => {
      const next = { ...current }
      for (const line of portal.lines) {
        if (next[line.rfqLineId]) {
          continue
        }

        next[line.rfqLineId] = createLineDraft(line)
      }
      return next
    })

    if (!quoteKey && portal.rfqKey) {
      setQuoteKey(`${portal.rfqKey}-QUOTE`)
    }
  }, [portal, quoteKey])

  const portalLineRows = useMemo(
    () =>
      portal?.lines.map((line) => ({
        ...line,
        draft: lineDrafts[line.rfqLineId] ?? createLineDraft(line),
      })) ?? [],
    [lineDrafts, portal?.lines],
  )

  const createQuoteMutation = useMutation({
    mutationFn: () => {
      if (!rfqId || !accessCode) {
        throw new Error('RFQ id and access code are required')
      }
      return createSupplierPortalQuote(rfqId, accessCode, {
        quoteKey,
        currencyCode,
        notes,
      })
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-supplier-quote-portal', rfqId, accessCode] })
    },
  })

  const saveLineMutation = useMutation({
    mutationFn: (line: SupplierPortalRfqLineResponse) => {
      if (!portal?.supplierQuoteId) {
        throw new Error('Create a quote first')
      }

      const draft = lineDrafts[line.rfqLineId] ?? createLineDraft(line)
      return upsertSupplierPortalQuoteLine(rfqId, portal.supplierQuoteId, accessCode, {
        rfqLineId: line.rfqLineId,
        unitPrice: Number(draft.unitPrice),
        quantityQuoted: Number(draft.quantityQuoted) || line.quantityRequested,
        leadTimeDays: draft.leadTimeDays ? Number(draft.leadTimeDays) : null,
        notes: draft.notes,
      })
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-supplier-quote-portal', rfqId, accessCode] })
    },
  })

  const submitQuoteMutation = useMutation({
    mutationFn: () => {
      if (!portal?.supplierQuoteId) {
        throw new Error('Create a quote first')
      }
      return submitSupplierPortalQuote(rfqId, portal.supplierQuoteId, accessCode)
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-supplier-quote-portal', rfqId, accessCode] })
    },
  })

  if (!rfqId || !accessCode) {
    return (
      <main className="min-h-screen bg-[var(--color-bg-page)] px-6 py-10 text-[var(--color-text-primary)]">
        <div className="mx-auto max-w-3xl rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6">
          <h1 className="text-2xl font-semibold">Supplier quote portal</h1>
          <p className="mt-2 text-sm text-[var(--color-text-muted)]">RFQ id and access code are required.</p>
        </div>
      </main>
    )
  }

  return (
    <main className="min-h-screen bg-[var(--color-bg-page)] px-6 py-10 text-[var(--color-text-primary)]">
      <div className="mx-auto max-w-6xl space-y-6">
        <header className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6">
          <p className="text-xs uppercase tracking-[0.3em] text-[var(--color-text-muted)]">SupplyArr supplier quote portal</p>
          <h1 className="mt-2 text-3xl font-semibold">Submit a quote for your invitation</h1>
          <p className="mt-2 max-w-3xl text-sm text-[var(--color-text-muted)]">
            Review the RFQ, prepare pricing and lead times, and submit your supplier-unit response without needing an internal login.
          </p>
        </header>

        {portalQuery.isLoading && (
          <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6 text-sm text-[var(--color-text-muted)]">
            Loading supplier quote portal…
          </section>
        )}

        {portalQuery.isError && (
          <section className="rounded-xl border border-[var(--color-success-border)] bg-[var(--color-success-bg)] p-6 text-sm text-[var(--color-success-text)]">
            Unable to load this supplier quote invitation. The access code may be invalid or expired.
          </section>
        )}

        {portal && (
          <>
            <section className="grid gap-4 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6 lg:grid-cols-3">
              <div className="lg:col-span-2">
                <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">{portal.rfqKey}</h2>
                <p className="mt-1 text-sm text-[var(--color-text-muted)]">{portal.title}</p>
                <p className="mt-2 text-sm text-[var(--color-text-muted)]">{portal.notes || 'No RFQ notes were provided.'}</p>
              </div>
              <div className="space-y-2 text-sm">
                <DetailLine label="Invitation status" value={portal.invitationStatus} />
                <DetailLine label="Portal expires" value={formatDateTime(portal.portalAccessExpiresAt)} />
                <DetailLine label="Supplier identity or sub-unit" value={supplierIdentityLabel} />
                <DetailLine label="Hierarchy role" value={humanizeSupplierUnitKind(portal.supplierUnitKind)} />
                <DetailLine label="Services provided" value={formatSupplierServiceTypes(portal.supplierServiceTypes)} />
                <DetailLine label="Current quote" value={portal.supplierQuoteId ? portal.quoteStatus ?? 'draft' : 'none'} />
              </div>
            </section>

            <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6">
              <div className="flex flex-wrap items-end gap-3">
                <label className="block min-w-[14rem] flex-1 text-sm text-[var(--color-text-secondary)]">
                  Quote key
                  <input
                    className="mt-1 w-full rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-2 py-2 text-sm text-[var(--color-text-primary)]"
                    value={quoteKey}
                    onChange={(event) => setQuoteKey(event.target.value)}
                    placeholder="SUPPLIER-QUOTE-001"
                  />
                </label>
                <label className="block min-w-[10rem] text-sm text-[var(--color-text-secondary)]">
                  Currency
                  <select
                    className="mt-1 w-full rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-2 py-2 text-sm text-[var(--color-text-primary)]"
                    value={currencyCode}
                    onChange={(event) => setCurrencyCode(event.target.value)}
                  >
                    {CURRENCY_OPTIONS.map((option) => (
                      <option key={option.value} value={option.value}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                </label>
                <button
                  type="button"
                  className="rounded bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] disabled:opacity-50"
                  disabled={createQuoteMutation.isPending || !quoteKey}
                  onClick={() => createQuoteMutation.mutate()}
                >
                  {currentQuote ? 'Refresh quote draft' : 'Create quote draft'}
                </button>
              </div>
              <label className="mt-4 block text-sm text-[var(--color-text-secondary)]">
                Notes
                <textarea
                  className="mt-1 w-full rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-2 py-2 text-sm text-[var(--color-text-primary)]"
                  rows={3}
                  value={notes}
                  onChange={(event) => setNotes(event.target.value)}
                  placeholder="Optional supplier note"
                />
              </label>
            </section>

            {portal.supplierQuoteId ? (
              <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Quote draft</h2>
                    <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                      Fill in your line prices and submit the quote when ready.
                    </p>
                  </div>
                  <button
                    type="button"
                    className="rounded border border-[var(--color-success-border)] bg-[var(--color-success-bg)] px-4 py-2 text-sm font-medium text-[var(--color-success-text)] disabled:opacity-50"
                    disabled={submitQuoteMutation.isPending || portal.quoteStatus === 'submitted'}
                    onClick={() => submitQuoteMutation.mutate()}
                  >
                    Submit quote
                  </button>
                </div>

                <div className="mt-4 overflow-x-auto">
                  <table className="min-w-full text-left text-sm">
                    <thead className="text-[var(--color-text-muted)]">
                      <tr>
                        <th className="py-2 pr-4">Part</th>
                        <th className="py-2 pr-4">Requested</th>
                        <th className="py-2 pr-4">Unit price</th>
                        <th className="py-2 pr-4">Qty quoted</th>
                        <th className="py-2 pr-4">Lead days</th>
                        <th className="py-2 pr-4">Notes</th>
                        <th className="py-2 pr-4">Action</th>
                      </tr>
                    </thead>
                    <tbody>
                      {portalLineRows.map((line) => (
                        <tr key={line.rfqLineId} className="border-t border-[var(--color-border-subtle)] align-top">
                          <td className="py-3 pr-4">
                            <div className="font-medium text-[var(--color-text-primary)]">{line.partDisplayName}</div>
                            <div className="text-xs text-[var(--color-text-muted)]">{line.partKey}</div>
                          </td>
                          <td className="py-3 pr-4 text-[var(--color-text-secondary)]">
                            {line.quantityRequested} {line.unitOfMeasure}
                          </td>
                          <td className="py-3 pr-4">
                            <input
                              className="w-28 rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-2 py-1 text-sm text-[var(--color-text-primary)]"
                              inputMode="decimal"
                              data-testid={`supplier-quote-portal-unit-price-${line.rfqLineId}`}
                              value={line.draft.unitPrice}
                              onChange={(event) =>
                                setLineDrafts((current) => ({
                                  ...current,
                                  [line.rfqLineId]: { ...line.draft, unitPrice: event.target.value },
                                }))
                              }
                            />
                          </td>
                          <td className="py-3 pr-4">
                            <input
                              className="w-24 rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-2 py-1 text-sm text-[var(--color-text-primary)]"
                              inputMode="decimal"
                              data-testid={`supplier-quote-portal-quantity-${line.rfqLineId}`}
                              value={line.draft.quantityQuoted}
                              onChange={(event) =>
                                setLineDrafts((current) => ({
                                  ...current,
                                  [line.rfqLineId]: { ...line.draft, quantityQuoted: event.target.value },
                                }))
                              }
                            />
                          </td>
                          <td className="py-3 pr-4">
                            <input
                              className="w-24 rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-2 py-1 text-sm text-[var(--color-text-primary)]"
                              inputMode="numeric"
                              data-testid={`supplier-quote-portal-lead-days-${line.rfqLineId}`}
                              value={line.draft.leadTimeDays}
                              onChange={(event) =>
                                setLineDrafts((current) => ({
                                  ...current,
                                  [line.rfqLineId]: { ...line.draft, leadTimeDays: event.target.value },
                                }))
                              }
                            />
                          </td>
                          <td className="py-3 pr-4">
                            <input
                              className="w-full rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-2 py-1 text-sm text-[var(--color-text-primary)]"
                              data-testid={`supplier-quote-portal-notes-${line.rfqLineId}`}
                              value={line.draft.notes}
                              onChange={(event) =>
                                setLineDrafts((current) => ({
                                  ...current,
                                  [line.rfqLineId]: { ...line.draft, notes: event.target.value },
                                }))
                              }
                            />
                          </td>
                          <td className="py-3 pr-4">
                            <button
                              type="button"
                              className="rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-1 text-xs text-[var(--color-text-primary)] disabled:opacity-50"
                              onClick={() => saveLineMutation.mutate(line)}
                              disabled={saveLineMutation.isPending}
                            >
                              Save line
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>

                <div className="mt-4 text-xs text-[var(--color-text-muted)]">
                  Quote total: {portal.totalAmount?.toFixed(2) ?? 'not calculated yet'} · Lead days:{' '}
                  {portal.leadTimeDays ?? '—'}
                </div>
              </section>
            ) : (
              <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6 text-sm text-[var(--color-text-muted)]">
                Create a quote draft to begin entering prices and lead times.
              </section>
            )}
          </>
        )}
      </div>
    </main>
  )
}

function createLineDraft(line: SupplierPortalRfqLineResponse): LineDraft {
  return {
    unitPrice: line.unitPrice?.toString() ?? '',
    quantityQuoted: line.quantityQuoted?.toString() ?? line.quantityRequested.toString(),
    leadTimeDays: line.leadTimeDays?.toString() ?? '',
    notes: line.quoteNotes ?? '',
  }
}

function DetailLine({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-start justify-between gap-3">
      <span className="text-[var(--color-text-muted)]">{label}</span>
      <span className="text-right text-[var(--color-text-primary)]">{value}</span>
    </div>
  )
}

function formatDateTime(value: string): string {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleString()
}
