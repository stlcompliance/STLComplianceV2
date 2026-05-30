import { useEffect, useState } from 'react'
import { ControlledSelect } from '@stl/shared-ui'

import type { PartResponse, PurchaseRequestResponse } from '../api/types'
import {
  formatProcurementReason,
  PROCUREMENT_REJECTION_REASON_OPTIONS,
  toPartPickerOptions,
  toPartyPickerOptions,
} from '../forms/controlledFormHelpers'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'

interface PurchaseRequestPanelProps {
  purchaseRequests: PurchaseRequestResponse[]
  parts: PartResponse[]
  vendors: { partyId: string; displayName: string; partyKey: string }[]
  canCreate: boolean
  canApprove: boolean
  isLoading: boolean
  requestKey: string
  title: string
  notes: string
  selectedVendorId: string
  selectedPartId: string
  lineQuantity: string
  lineNotes: string
  rejectionReason: string
  selectedPurchaseRequestId: string
  onRequestKeyChange: (value: string) => void
  onTitleChange: (value: string) => void
  onNotesChange: (value: string) => void
  onSelectedVendorIdChange: (value: string) => void
  onSelectedPartIdChange: (value: string) => void
  onLineQuantityChange: (value: string) => void
  onLineNotesChange: (value: string) => void
  onRejectionReasonChange: (value: string) => void
  onSelectedPurchaseRequestIdChange: (value: string) => void
  onCreate: () => void
  onSubmit: () => void
  onApprove: () => void
  onReject: () => void
  isCreating: boolean
  isSubmitting: boolean
  isApproving: boolean
  isRejecting: boolean
}

function statusBadgeClass(status: string): string {
  switch (status) {
    case 'approved':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'submitted':
      return 'bg-sky-500/20 text-sky-300 ring-sky-500/40'
    case 'rejected':
      return 'bg-rose-500/20 text-rose-300 ring-rose-500/40'
    case 'draft':
      return 'bg-amber-500/20 text-amber-300 ring-amber-500/40'
    default:
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
  }
}

function formatTimestamp(value: string | null | undefined): string | null {
  if (!value) return null
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return null
  return date.toLocaleString()
}

export function PurchaseRequestPanel({
  purchaseRequests,
  parts,
  vendors,
  canCreate,
  canApprove,
  isLoading,
  requestKey,
  title,
  notes,
  selectedVendorId,
  selectedPartId,
  lineQuantity,
  lineNotes,
  rejectionReason,
  selectedPurchaseRequestId,
  onRequestKeyChange,
  onTitleChange,
  onNotesChange,
  onSelectedVendorIdChange,
  onSelectedPartIdChange,
  onLineQuantityChange,
  onLineNotesChange,
  onRejectionReasonChange,
  onSelectedPurchaseRequestIdChange,
  onCreate,
  onSubmit,
  onApprove,
  onReject,
  isCreating,
  isSubmitting,
  isApproving,
  isRejecting,
}: PurchaseRequestPanelProps) {
  const [rejectionReasonCode, setRejectionReasonCode] = useState('')
  const [rejectionReasonNotes, setRejectionReasonNotes] = useState('')

  useEffect(() => {
    onRejectionReasonChange(formatProcurementReason(rejectionReasonCode, rejectionReasonNotes))
  }, [rejectionReasonCode, rejectionReasonNotes, onRejectionReasonChange])

  const selected = purchaseRequests.find((pr) => pr.purchaseRequestId === selectedPurchaseRequestId)
  const existingRequestKeys = purchaseRequests.map((pr) => pr.requestKey)

  return (
    <section
      data-testid="supplyarr-purchasing-pr-workspace"
      className="rounded-xl border border-slate-800 bg-slate-900/60 p-5 shadow-lg"
    >
      <h2 className="text-lg font-medium text-white">Purchase requests</h2>
      <p className="mt-1 text-sm text-slate-400">
        Draft, submit, approve, or reject procurement requests before PO creation.
      </p>

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-500" data-testid="purchase-request-loading">
          Loading purchase requests…
        </p>
      ) : null}

      <ul className="mt-4 space-y-2" data-testid="purchase-request-list">
        {purchaseRequests.map((pr) => (
          <li key={pr.purchaseRequestId}>
            <button
              type="button"
              data-testid={`purchase-request-row-${pr.purchaseRequestId}`}
              className={`w-full rounded-lg border px-3 py-2 text-left text-sm transition ${
                selectedPurchaseRequestId === pr.purchaseRequestId
                  ? 'border-sky-500/60 bg-sky-500/10'
                  : 'border-slate-800 bg-slate-950/40 hover:border-slate-700'
              }`}
              onClick={() => onSelectedPurchaseRequestIdChange(pr.purchaseRequestId)}
            >
              <div className="flex items-center justify-between gap-2">
                <span className="font-medium text-slate-200">{pr.requestKey}</span>
                <span
                  className={`rounded-full px-2 py-0.5 text-xs ring-1 ring-inset ${statusBadgeClass(pr.status)}`}
                >
                  {pr.status}
                </span>
              </div>
              <div className="mt-1 text-slate-400">{pr.title}</div>
              <div className="mt-1 text-xs text-slate-500">
                {pr.lines.length} line{pr.lines.length === 1 ? '' : 's'}
                {pr.vendorDisplayName ? ` · ${pr.vendorDisplayName}` : ''}
              </div>
            </button>
          </li>
        ))}
        {!isLoading && purchaseRequests.length === 0 ? (
          <li className="text-sm text-slate-500">No purchase requests yet.</li>
        ) : null}
      </ul>

      {selected ? (
        <div
          className="mt-4 rounded-lg border border-slate-800 bg-slate-950/50 p-4"
          data-testid="purchase-request-detail"
        >
          <h3 className="text-sm font-medium text-slate-200">Request detail</h3>
          <div className="mt-1 text-sm text-slate-300">{selected.title}</div>
          {selected.notes ? <p className="mt-1 text-sm text-slate-400">{selected.notes}</p> : null}
          {selected.vendorDisplayName ? (
            <p className="mt-1 text-xs text-slate-500">Vendor: {selected.vendorDisplayName}</p>
          ) : null}
          <ul className="mt-2 space-y-1 text-sm text-slate-400" data-testid="purchase-request-line-list">
            {selected.lines.map((line) => (
              <li key={line.lineId} data-testid={`purchase-request-line-${line.lineId}`}>
                #{line.lineNumber} {line.partDisplayName} ({line.partKey}) — {line.quantityRequested}{' '}
                {line.unitOfMeasure} requested
              </li>
            ))}
          </ul>
          <dl className="mt-3 space-y-1 text-xs text-slate-500" data-testid="purchase-request-workflow-timeline">
            {formatTimestamp(selected.submittedAt) ? (
              <div>
                <dt className="inline font-medium text-slate-400">Submitted: </dt>
                <dd className="inline">{formatTimestamp(selected.submittedAt)}</dd>
              </div>
            ) : null}
            {formatTimestamp(selected.approvedAt) ? (
              <div>
                <dt className="inline font-medium text-emerald-400/80">Approved: </dt>
                <dd className="inline">{formatTimestamp(selected.approvedAt)}</dd>
              </div>
            ) : null}
            {formatTimestamp(selected.rejectedAt) ? (
              <div>
                <dt className="inline font-medium text-rose-400/80">Rejected: </dt>
                <dd className="inline">{formatTimestamp(selected.rejectedAt)}</dd>
              </div>
            ) : null}
          </dl>
          {selected.status === 'rejected' && selected.rejectionReason ? (
            <p
              className="mt-2 text-sm text-rose-300"
              data-testid="purchase-request-rejection-reason-display"
            >
              Rejected: {selected.rejectionReason}
            </p>
          ) : null}
          <div className="mt-3 flex flex-wrap gap-2">
            {canCreate && selected.status === 'draft' ? (
              <button
                type="button"
                className="rounded-md bg-sky-600 px-3 py-1.5 text-sm text-white hover:bg-sky-500 disabled:opacity-50"
                disabled={isSubmitting}
                onClick={onSubmit}
                data-testid="purchase-request-submit-button"
              >
                {isSubmitting ? 'Submitting…' : 'Submit for approval'}
              </button>
            ) : null}
            {canApprove && selected.status === 'submitted' ? (
              <>
                <button
                  type="button"
                  className="rounded-md bg-emerald-600 px-3 py-1.5 text-sm text-white hover:bg-emerald-500 disabled:opacity-50"
                  disabled={isApproving}
                  onClick={onApprove}
                  data-testid="purchase-request-approve-button"
                >
                  {isApproving ? 'Approving…' : 'Approve'}
                </button>
                <div className="min-w-[12rem] flex-1 space-y-2">
                  <ControlledSelect
                    id="purchase-request-rejection-reason-code"
                    label="Rejection reason code"
                    value={rejectionReasonCode}
                    onChange={setRejectionReasonCode}
                    options={PROCUREMENT_REJECTION_REASON_OPTIONS}
                    emptyLabel="Select reason code…"
                    testId="purchase-request-rejection-reason-code"
                    className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-xs text-slate-200"
                  />
                  <label
                    htmlFor="purchase-request-rejection-reason-notes"
                    className="block text-xs text-slate-500"
                  >
                    Rejection notes (optional)
                    <textarea
                      id="purchase-request-rejection-reason-notes"
                      className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-xs text-slate-200"
                      rows={2}
                      value={rejectionReasonNotes}
                      onChange={(e) => setRejectionReasonNotes(e.target.value)}
                      data-testid="purchase-request-rejection-reason-notes"
                    />
                  </label>
                </div>
                <button
                  type="button"
                  className="rounded-md bg-rose-700 px-3 py-1.5 text-sm text-white hover:bg-rose-600 disabled:opacity-50"
                  disabled={isRejecting || !rejectionReason.trim() || !rejectionReasonCode.trim()}
                  onClick={onReject}
                  data-testid="purchase-request-reject-button"
                >
                  {isRejecting ? 'Rejecting…' : 'Reject'}
                </button>
              </>
            ) : null}
          </div>
        </div>
      ) : null}

      {canCreate ? (
        <div className="mt-6 border-t border-slate-800 pt-4" data-testid="purchase-request-create-form">
          <h3 className="text-sm font-medium text-slate-200">New purchase request</h3>
          <div className="mt-3 space-y-3">
            <label htmlFor="purchase-request-create-title" className="block text-xs text-slate-500">
              Request title
              <input
                id="purchase-request-create-title"
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-200"
                value={title}
                onChange={(e) => onTitleChange(e.target.value)}
              />
            </label>
            <GeneratedKeyFieldGroup
              sourceLabel={title}
              existingKeys={existingRequestKeys}
              onKeyChange={onRequestKeyChange}
              domain="purchase"
              kind="request"
              label="Request key"
            />
            <label htmlFor="purchase-request-create-notes" className="block text-xs text-slate-500">
              Notes
              <textarea
                id="purchase-request-create-notes"
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-200"
                rows={2}
                value={notes}
                onChange={(e) => onNotesChange(e.target.value)}
              />
            </label>
            <ControlledSelect
              label="Vendor (optional)"
              value={selectedVendorId}
              onChange={onSelectedVendorIdChange}
              options={toPartyPickerOptions(vendors)}
              emptyLabel="Vendor (optional)"
            />
            <div className="grid gap-2 sm:grid-cols-3">
              <div className="sm:col-span-2">
                <ControlledSelect
                  label="Part for first line"
                  value={selectedPartId}
                  onChange={onSelectedPartIdChange}
                  options={toPartPickerOptions(parts)}
                  emptyLabel="Part for first line"
                />
              </div>
              <label htmlFor="purchase-request-line-qty" className="block text-xs text-slate-500">
                Line quantity
                <input
                  id="purchase-request-line-qty"
                  className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-200"
                  type="number"
                  min="0"
                  step="any"
                  value={lineQuantity}
                  onChange={(e) => onLineQuantityChange(e.target.value)}
                />
              </label>
            </div>
            <label htmlFor="purchase-request-line-notes" className="block text-xs text-slate-500">
              Line notes
              <input
                id="purchase-request-line-notes"
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-200"
                value={lineNotes}
                onChange={(e) => onLineNotesChange(e.target.value)}
              />
            </label>
            <button
              type="button"
              className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
              disabled={isCreating || !requestKey.trim() || !title.trim() || !selectedPartId || !lineQuantity}
              onClick={onCreate}
              data-testid="purchase-request-create-button"
            >
              {isCreating ? 'Creating…' : 'Create draft'}
            </button>
          </div>
        </div>
      ) : null}
    </section>
  )
}
