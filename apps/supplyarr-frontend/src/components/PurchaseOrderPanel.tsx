import { useMemo } from 'react'

import { StaticSearchPicker, type PickerOption } from '@stl/shared-ui'

import type { PurchaseOrderResponse, PurchaseRequestResponse } from '../api/types'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'

interface PurchaseOrderPanelProps {
  purchaseOrders: PurchaseOrderResponse[]
  approvedPurchaseRequests: PurchaseRequestResponse[]
  canCreate: boolean
  canApprove: boolean
  isLoading: boolean
  orderKey: string
  cancellationReason: string
  selectedPurchaseRequestId: string
  selectedPurchaseOrderId: string
  onOrderKeyChange: (value: string) => void
  onCancellationReasonChange: (value: string) => void
  onSelectedPurchaseRequestIdChange: (value: string) => void
  onSelectedPurchaseOrderIdChange: (value: string) => void
  onCreateFromPurchaseRequest: () => void
  onApprove: () => void
  onIssue: () => void
  onCancel: () => void
  isCreating: boolean
  isApproving: boolean
  isIssuing: boolean
  isCancelling: boolean
}

function statusBadgeClass(status: string): string {
  switch (status) {
    case 'issued':
      return 'bg-[var(--color-success-bg)] text-[var(--color-success-text)] ring-[var(--color-success-border)]'
    case 'approved':
      return 'bg-[var(--color-info-bg)] text-[var(--color-info-text)] ring-[var(--color-info-border)]'
    case 'draft':
      return 'bg-[var(--color-warning-bg)] text-[var(--color-warning-text)] ring-[var(--color-warning-border)]'
    case 'cancelled':
      return 'bg-[var(--color-destructive-bg)] text-[var(--color-destructive-text)] ring-[var(--color-destructive-border)]'
    default:
      return 'bg-[var(--color-bg-control-hover)] text-[var(--color-text-secondary)] ring-[var(--color-border-subtle)]'
  }
}

export function PurchaseOrderPanel({
  purchaseOrders,
  approvedPurchaseRequests,
  canCreate,
  canApprove,
  isLoading,
  orderKey,
  cancellationReason,
  selectedPurchaseRequestId,
  selectedPurchaseOrderId,
  onOrderKeyChange,
  onCancellationReasonChange,
  onSelectedPurchaseRequestIdChange,
  onSelectedPurchaseOrderIdChange,
  onCreateFromPurchaseRequest,
  onApprove,
  onIssue,
  onCancel,
  isCreating,
  isApproving,
  isIssuing,
  isCancelling,
}: PurchaseOrderPanelProps) {
  const selectedPo = purchaseOrders.find((po) => po.purchaseOrderId === selectedPurchaseOrderId)
  const selectedPr = approvedPurchaseRequests.find(
    (pr) => pr.purchaseRequestId === selectedPurchaseRequestId,
  )
  const purchaseRequestOptions = useMemo<PickerOption[]>(
    () =>
      approvedPurchaseRequests.map((pr) => ({
        value: pr.purchaseRequestId,
        label: `${pr.requestKey} — ${pr.title}${pr.vendorDisplayName ? ` · ${pr.vendorDisplayName}` : ''}`,
      })),
    [approvedPurchaseRequests],
  )
  const selectedPurchaseRequestOption = useMemo<PickerOption | undefined>(
    () =>
      purchaseRequestOptions.find((option) => option.value === selectedPurchaseRequestId) ??
      (selectedPr
        ? {
            value: selectedPr.purchaseRequestId,
            label: `${selectedPr.requestKey} — ${selectedPr.title}${selectedPr.vendorDisplayName ? ` · ${selectedPr.vendorDisplayName}` : ''}`,
          }
        : undefined),
    [purchaseRequestOptions, selectedPurchaseRequestId, selectedPr],
  )
  const canCancelSelected =
    canCreate && selectedPo != null && (selectedPo.status === 'draft' || selectedPo.status === 'approved')
  const orderKeySource = selectedPr
    ? `${selectedPr.requestKey} ${selectedPr.title || 'purchase order'}`
    : ''
  const existingOrderKeys = purchaseOrders.map((po) => po.orderKey)

  return (
    <section
      data-testid="supplyarr-purchasing-po-workspace"
      className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5 shadow-[var(--shadow-surface)]"
    >
      <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Purchase orders</h2>
      <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
        Create POs from approved purchase requests, approve, issue to vendors, or cancel draft and
        approved orders.
      </p>

      {isLoading ? (
        <p className="mt-4 text-sm text-[var(--color-text-muted)]" data-testid="purchase-order-loading">
          Loading purchase orders…
        </p>
      ) : null}

      <ul className="mt-4 space-y-2" data-testid="purchase-order-list">
        {purchaseOrders.map((po) => (
          <li key={po.purchaseOrderId}>
            <button
              type="button"
              data-testid={`purchase-order-row-${po.purchaseOrderId}`}
              className={`w-full rounded-lg border px-3 py-2 text-left text-sm transition ${
                selectedPurchaseOrderId === po.purchaseOrderId
                  ? 'border-[var(--color-info-border)] bg-[var(--color-info-bg)]'
                  : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] hover:border-[var(--color-border-default)] hover:bg-[var(--color-bg-control-hover)]'
              }`}
              onClick={() => onSelectedPurchaseOrderIdChange(po.purchaseOrderId)}
            >
              <div className="flex items-center justify-between gap-2">
                <span className="font-medium text-[var(--color-text-primary)]">{po.orderKey}</span>
                <span
                  className={`rounded-full px-2 py-0.5 text-xs ring-1 ring-inset ${statusBadgeClass(po.status)}`}
                >
                  {po.status}
                </span>
              </div>
              <div className="mt-1 text-[var(--color-text-secondary)]">{po.title}</div>
              <div className="mt-1 text-xs text-[var(--color-text-muted)]">
                PR {po.purchaseRequestKey} · {po.vendorDisplayName} · {po.lines.length} line
                {po.lines.length === 1 ? '' : 's'}
              </div>
            </button>
          </li>
        ))}
        {purchaseOrders.length === 0 && !isLoading ? (
          <li className="text-sm text-[var(--color-text-muted)]">No purchase orders yet.</li>
        ) : null}
      </ul>

      {selectedPo ? (
        <div className="mt-4 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] p-4" data-testid="purchase-order-detail">
          <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">Order detail</h3>
          <ul className="mt-2 space-y-1 text-sm text-[var(--color-text-secondary)]" data-testid="purchase-order-line-list">
            {selectedPo.lines.map((line) => (
              <li key={line.lineId} data-testid={`purchase-order-line-${line.lineId}`}>
                {line.partKey} — {line.quantityOrdered} {line.unitOfMeasure} ordered ·{' '}
                {line.quantityReceived} received · {line.quantityRemaining} remaining
              </li>
            ))}
          </ul>
          {selectedPo.status === 'cancelled' && selectedPo.cancellationReason ? (
            <p
              className="mt-2 text-sm text-[var(--color-destructive-text)]"
              data-testid="purchase-order-cancellation-reason-display"
            >
              Cancelled: {selectedPo.cancellationReason}
            </p>
          ) : null}
          <div className="mt-3 flex flex-wrap gap-2">
            {canApprove && selectedPo.status === 'draft' ? (
              <button
                type="button"
                className="rounded-md bg-[var(--color-accent)] px-3 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
                onClick={onApprove}
                disabled={isApproving}
              >
                {isApproving ? 'Approving…' : 'Approve PO'}
              </button>
            ) : null}
            {canCreate && selectedPo.status === 'approved' ? (
              <button
                type="button"
                className="rounded-md bg-[var(--color-success)] px-3 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:opacity-90 disabled:opacity-50"
                onClick={onIssue}
                disabled={isIssuing}
              >
                {isIssuing ? 'Issuing…' : 'Issue to vendor'}
              </button>
            ) : null}
            {canCancelSelected ? (
              <>
                <label htmlFor="purchase-order-cancellation-reason-input" className="min-w-[10rem] flex-1 text-xs text-[var(--color-text-muted)]">
                  Cancellation reason
                  <input
                    id="purchase-order-cancellation-reason-input"
                    className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-3 py-2 text-xs text-[var(--color-text-primary)]"
                    value={cancellationReason}
                    onChange={(e) => onCancellationReasonChange(e.target.value)}
                    data-testid="purchase-order-cancellation-reason-input"
                  />
                </label>
                <button
                  type="button"
                  className="rounded-md bg-[var(--color-danger)] px-3 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:opacity-90 disabled:opacity-50"
                  onClick={onCancel}
                  disabled={isCancelling || !cancellationReason.trim()}
                  data-testid="purchase-order-cancel-button"
                >
                  {isCancelling ? 'Cancelling…' : 'Cancel PO'}
                </button>
              </>
            ) : null}
          </div>
        </div>
      ) : null}

      {canCreate ? (
        <div className="mt-6 border-t border-[var(--color-border-subtle)] pt-4" data-testid="purchase-order-create-form">
          <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">Create from approved PR</h3>
          <div className="mt-3 space-y-3">
            <StaticSearchPicker
              id="purchase-order-create-pr-select"
              label="Approved purchase request"
              value={selectedPurchaseRequestId}
              onChange={onSelectedPurchaseRequestIdChange}
              options={purchaseRequestOptions}
              selectedOption={selectedPurchaseRequestOption}
              placeholder="Search approved purchase requests…"
              disabled={isLoading}
              testId="purchase-order-create-pr-picker"
            />
            {selectedPr ? (
              <p className="text-xs text-[var(--color-text-muted)]">
                Vendor: {selectedPr.vendorDisplayName ?? 'none'} · {selectedPr.lines.length} line(s)
              </p>
            ) : null}
            <GeneratedKeyFieldGroup
              sourceLabel={orderKeySource}
              existingKeys={existingOrderKeys}
              onKeyChange={onOrderKeyChange}
              domain="purchase"
              kind="order"
              maxLength={128}
              label="Order key"
              disabled={isCreating}
            />
            <button
              type="button"
              className="rounded-md bg-[var(--color-accent)] px-3 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
              onClick={onCreateFromPurchaseRequest}
              disabled={
                isCreating || !selectedPurchaseRequestId || !orderKey.trim() || !selectedPr?.vendorPartyId
              }
            >
              {isCreating ? 'Creating…' : 'Create purchase order'}
            </button>
          </div>
        </div>
      ) : null}
    </section>
  )
}
