import { useMemo, useState } from 'react'

import type {
  InventoryBinResponse,
  PurchaseOrderResponse,
  ReceivingExceptionResponse,
  ReceivingReceiptResponse,
} from '../api/types'

interface ReceivingPanelProps {
  receivingReceipts: ReceivingReceiptResponse[]
  issuedPurchaseOrders: PurchaseOrderResponse[]
  bins: InventoryBinResponse[]
  canPerform: boolean
  isLoading: boolean
  receiptKey: string
  selectedPurchaseOrderId: string
  selectedReceivingReceiptId: string
  selectedBinId: string
  selectedLineId: string
  lineQuantityReceived: string
  exceptionType: string
  exceptionQuantity: string
  exceptionNotes: string
  onReceiptKeyChange: (value: string) => void
  onSelectedPurchaseOrderIdChange: (value: string) => void
  onSelectedReceivingReceiptIdChange: (value: string) => void
  onSelectedBinIdChange: (value: string) => void
  onSelectedLineIdChange: (value: string) => void
  onLineQuantityReceivedChange: (value: string) => void
  onExceptionTypeChange: (value: string) => void
  onExceptionQuantityChange: (value: string) => void
  onExceptionNotesChange: (value: string) => void
  onCreateFromPurchaseOrder: () => void
  onUpdateLineQuantity: () => void
  onCreateException: () => void
  onResolveException: (receivingExceptionId: string) => void
  onPost: () => void
  isCreating: boolean
  isUpdatingLine: boolean
  isCreatingException: boolean
  isResolvingException: boolean
  isPosting: boolean
}

type ExceptionFilter = 'all' | 'open' | 'resolved'

function statusBadgeClass(status: string): string {
  switch (status) {
    case 'posted':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'draft':
      return 'bg-amber-500/20 text-amber-300 ring-amber-500/40'
    case 'resolved':
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
    case 'open':
      return 'bg-rose-500/20 text-rose-300 ring-rose-500/40'
    default:
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
  }
}

function exceptionTypeLabel(type: string): string {
  switch (type) {
    case 'short':
      return 'Short'
    case 'over':
      return 'Over'
    case 'damage':
      return 'Damage'
    default:
      return type
  }
}

function formatTimestamp(value: string | null | undefined): string | null {
  if (!value) return null
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return null
  return date.toLocaleString()
}

function filterExceptions(
  exceptions: ReceivingExceptionResponse[],
  filter: ExceptionFilter,
): ReceivingExceptionResponse[] {
  if (filter === 'all') return exceptions
  return exceptions.filter((ex) => ex.status === filter)
}

export function ReceivingPanel({
  receivingReceipts,
  issuedPurchaseOrders,
  bins,
  canPerform,
  isLoading,
  receiptKey,
  selectedPurchaseOrderId,
  selectedReceivingReceiptId,
  selectedBinId,
  selectedLineId,
  lineQuantityReceived,
  exceptionType,
  exceptionQuantity,
  exceptionNotes,
  onReceiptKeyChange,
  onSelectedPurchaseOrderIdChange,
  onSelectedReceivingReceiptIdChange,
  onSelectedBinIdChange,
  onSelectedLineIdChange,
  onLineQuantityReceivedChange,
  onExceptionTypeChange,
  onExceptionQuantityChange,
  onExceptionNotesChange,
  onCreateFromPurchaseOrder,
  onUpdateLineQuantity,
  onCreateException,
  onResolveException,
  onPost,
  isCreating,
  isUpdatingLine,
  isCreatingException,
  isResolvingException,
  isPosting,
}: ReceivingPanelProps) {
  const [exceptionFilter, setExceptionFilter] = useState<ExceptionFilter>('all')

  const selectedReceipt = receivingReceipts.find(
    (r) => r.receivingReceiptId === selectedReceivingReceiptId,
  )
  const selectedPo = issuedPurchaseOrders.find((po) => po.purchaseOrderId === selectedPurchaseOrderId)
  const selectedLine = selectedReceipt?.lines.find((line) => line.lineId === selectedLineId)

  const filteredExceptions = useMemo(
    () => filterExceptions(selectedReceipt?.exceptions ?? [], exceptionFilter),
    [exceptionFilter, selectedReceipt?.exceptions],
  )

  const openExceptionCount = selectedReceipt?.exceptions.filter((ex) => ex.status === 'open').length ?? 0

  return (
    <section
      data-testid="supplyarr-receiving-workspace"
      className="rounded-xl border border-slate-800 bg-slate-900/60 p-5 shadow-lg"
    >
      <h2 className="text-lg font-medium text-white">Receiving</h2>
      <p className="mt-1 text-sm text-slate-400">
        Receive parts against issued purchase orders, record over/short/damage exceptions, and post
        stock into inventory bins.
      </p>

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-500" data-testid="receiving-loading">
          Loading receiving receipts…
        </p>
      ) : null}

      <ul className="mt-4 space-y-2" data-testid="receiving-receipt-list">
        {receivingReceipts.map((receipt) => (
          <li key={receipt.receivingReceiptId}>
            <button
              type="button"
              data-testid={`receiving-receipt-row-${receipt.receivingReceiptId}`}
              className={`w-full rounded-lg border px-3 py-2 text-left text-sm transition ${
                selectedReceivingReceiptId === receipt.receivingReceiptId
                  ? 'border-teal-500/60 bg-teal-500/10'
                  : 'border-slate-800 bg-slate-950/40 hover:border-slate-700'
              }`}
              onClick={() => onSelectedReceivingReceiptIdChange(receipt.receivingReceiptId)}
            >
              <div className="flex items-center justify-between gap-2">
                <span className="font-medium text-slate-200">{receipt.receiptKey}</span>
                <span
                  className={`rounded-full px-2 py-0.5 text-xs ring-1 ring-inset ${statusBadgeClass(receipt.status)}`}
                >
                  {receipt.status}
                </span>
              </div>
              <div className="mt-1 text-slate-400">
                PO {receipt.purchaseOrderKey} · {receipt.binKey} @ {receipt.locationKey}
              </div>
              <div className="mt-1 text-xs text-slate-500">
                {receipt.lines.length} line{receipt.lines.length === 1 ? '' : 's'}
                {receipt.exceptions.length > 0
                  ? ` · ${receipt.exceptions.length} exception${receipt.exceptions.length === 1 ? '' : 's'}`
                  : ''}
              </div>
            </button>
          </li>
        ))}
        {!isLoading && receivingReceipts.length === 0 ? (
          <li className="text-sm text-slate-500">No receiving receipts yet.</li>
        ) : null}
      </ul>

      {selectedReceipt ? (
        <div
          className="mt-4 rounded-lg border border-slate-800 bg-slate-950/50 p-3 text-sm"
          data-testid="receiving-receipt-detail"
        >
          <div className="font-medium text-slate-200">Receipt lines</div>
          <ul className="mt-2 space-y-2 text-slate-400" data-testid="receiving-line-list">
            {selectedReceipt.lines.map((line) => (
              <li key={line.lineId}>
                <button
                  type="button"
                  data-testid={`receiving-line-${line.lineId}`}
                  className={`w-full rounded border px-2 py-1.5 text-left ${
                    selectedLineId === line.lineId
                      ? 'border-violet-500/50 bg-violet-500/10'
                      : 'border-slate-800 hover:border-slate-700'
                  }`}
                  onClick={() => onSelectedLineIdChange(line.lineId)}
                >
                  <div className="font-medium text-slate-200">{line.partKey}</div>
                  <div>
                    Good qty {line.quantityReceived} / expected {line.quantityExpected} · PO remaining{' '}
                    {line.quantityRemainingOnOrder}
                  </div>
                  {line.exceptions.length > 0 ? (
                    <ul className="mt-1 text-xs text-slate-500">
                      {line.exceptions.map((ex) => (
                        <li key={ex.receivingExceptionId}>
                          {exceptionTypeLabel(ex.exceptionType)} {ex.quantity} ({ex.status})
                        </li>
                      ))}
                    </ul>
                  ) : null}
                </button>
              </li>
            ))}
          </ul>

          <div className="mt-3 border-t border-slate-800 pt-3" data-testid="receiving-exception-panel">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <div className="text-xs font-medium uppercase tracking-wide text-slate-500">
                Receipt exceptions
                {openExceptionCount > 0 ? (
                  <span className="ml-2 text-rose-400">{openExceptionCount} open</span>
                ) : null}
              </div>
              <label htmlFor="receiving-exception-filter" className="text-xs text-slate-500">
                Exception filter
                <select
                  id="receiving-exception-filter"
                  className="ml-2 rounded-md border border-slate-700 bg-slate-950 px-2 py-1 text-xs text-slate-200"
                  value={exceptionFilter}
                  onChange={(e) => setExceptionFilter(e.target.value as ExceptionFilter)}
                  data-testid="receiving-exception-filter"
                >
                  <option value="all">All</option>
                  <option value="open">Open</option>
                  <option value="resolved">Resolved</option>
                </select>
              </label>
            </div>

            {filteredExceptions.length > 0 ? (
              <ul className="mt-2 space-y-2" data-testid="receiving-exception-list">
                {filteredExceptions.map((ex) => (
                  <li
                    key={ex.receivingExceptionId}
                    data-testid={`receiving-exception-row-${ex.receivingExceptionId}`}
                    className="rounded border border-slate-800 bg-slate-950/40 px-3 py-2"
                  >
                    <div className="flex flex-wrap items-center justify-between gap-2">
                      <span className="text-slate-300">
                        Line {ex.lineNumber} {ex.partKey}: {exceptionTypeLabel(ex.exceptionType)}{' '}
                        {ex.quantity}
                      </span>
                      <span
                        className={`rounded-full px-2 py-0.5 text-xs ring-1 ring-inset ${statusBadgeClass(ex.status)}`}
                      >
                        {ex.status}
                      </span>
                    </div>
                    {ex.notes ? (
                      <p className="mt-1 text-xs text-slate-500" data-testid="receiving-exception-notes">
                        {ex.notes}
                      </p>
                    ) : null}
                    <dl
                      className="mt-1 space-y-0.5 text-xs text-slate-500"
                      data-testid="receiving-exception-workflow-timeline"
                    >
                      {formatTimestamp(ex.createdAt) ? (
                        <div>
                          <dt className="inline font-medium text-slate-400">Recorded: </dt>
                          <dd className="inline">{formatTimestamp(ex.createdAt)}</dd>
                        </div>
                      ) : null}
                      {formatTimestamp(ex.resolvedAt) ? (
                        <div>
                          <dt className="inline font-medium text-emerald-400/80">Resolved: </dt>
                          <dd className="inline">{formatTimestamp(ex.resolvedAt)}</dd>
                        </div>
                      ) : null}
                    </dl>
                    {canPerform &&
                    selectedReceipt.status === 'draft' &&
                    ex.status === 'open' ? (
                      <button
                        type="button"
                        className="mt-2 rounded-md bg-teal-700 px-2 py-1 text-xs font-medium text-white hover:bg-teal-600 disabled:opacity-50"
                        onClick={() => onResolveException(ex.receivingExceptionId)}
                        disabled={isResolvingException}
                        data-testid={`receiving-exception-resolve-button-${ex.receivingExceptionId}`}
                      >
                        {isResolvingException ? 'Resolving…' : 'Resolve exception'}
                      </button>
                    ) : null}
                  </li>
                ))}
              </ul>
            ) : (
              <p className="mt-2 text-xs text-slate-500" data-testid="receiving-exception-empty">
                {selectedReceipt.exceptions.length === 0
                  ? 'No exceptions recorded on this receipt.'
                  : 'No exceptions match the current filter.'}
              </p>
            )}
          </div>

          {canPerform && selectedReceipt.status === 'draft' && selectedLine ? (
            <div
              className="mt-4 space-y-3 border-t border-slate-800 pt-3"
              data-testid="receiving-line-adjustments"
            >
              <h4 className="text-sm font-medium text-slate-300">Line adjustments</h4>
              <label htmlFor="receiving-line-quantity-input" className="block text-xs text-slate-500">
                Good quantity received
                <input
                  id="receiving-line-quantity-input"
                  type="number"
                  min="0"
                  step="any"
                  className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-sm text-slate-200"
                  value={lineQuantityReceived}
                  onChange={(e) => onLineQuantityReceivedChange(e.target.value)}
                  data-testid="receiving-line-quantity-input"
                />
              </label>
              <button
                type="button"
                className="rounded-md bg-slate-700 px-3 py-1.5 text-sm text-white hover:bg-slate-600 disabled:opacity-50"
                onClick={onUpdateLineQuantity}
                disabled={isUpdatingLine || !lineQuantityReceived.trim()}
                data-testid="receiving-line-save-button"
              >
                {isUpdatingLine ? 'Saving…' : 'Save line quantity'}
              </button>

              <div data-testid="receiving-exception-record-form">
                <h4 className="text-sm font-medium text-slate-300">Record exception</h4>
                <label htmlFor="receiving-exception-type-select" className="mt-2 block text-xs text-slate-500">
                  Exception type
                  <select
                    id="receiving-exception-type-select"
                    className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-sm text-slate-200"
                    value={exceptionType}
                    onChange={(e) => onExceptionTypeChange(e.target.value)}
                    data-testid="receiving-exception-type-select"
                  >
                    <option value="short">Short shipment</option>
                    <option value="over">Over receive</option>
                    <option value="damage">Damage</option>
                  </select>
                </label>
                <label htmlFor="receiving-exception-quantity-input" className="mt-2 block text-xs text-slate-500">
                  Exception quantity
                  <input
                    id="receiving-exception-quantity-input"
                    type="number"
                    min="0"
                    step="any"
                    className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-sm text-slate-200"
                    value={exceptionQuantity}
                    onChange={(e) => onExceptionQuantityChange(e.target.value)}
                    data-testid="receiving-exception-quantity-input"
                  />
                </label>
                <label htmlFor="receiving-exception-notes-input" className="mt-2 block text-xs text-slate-500">
                  Exception notes (optional)
                  <input
                    id="receiving-exception-notes-input"
                    className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-sm text-slate-200"
                    value={exceptionNotes}
                    onChange={(e) => onExceptionNotesChange(e.target.value)}
                    data-testid="receiving-exception-notes-input"
                  />
                </label>
                <button
                  type="button"
                  className="mt-2 rounded-md bg-amber-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-amber-600 disabled:opacity-50"
                  onClick={onCreateException}
                  disabled={isCreatingException || !exceptionQuantity.trim()}
                  data-testid="receiving-exception-record-button"
                >
                  {isCreatingException ? 'Recording…' : 'Record exception'}
                </button>
              </div>
            </div>
          ) : null}

          {canPerform && selectedReceipt.status === 'draft' ? (
            <button
              type="button"
              className="mt-3 rounded-md bg-teal-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
              onClick={onPost}
              disabled={isPosting}
              data-testid="receiving-post-button"
            >
              {isPosting ? 'Posting…' : 'Post receipt'}
            </button>
          ) : null}
        </div>
      ) : null}

      {canPerform ? (
        <div
          className="mt-6 space-y-3 border-t border-slate-800 pt-4"
          data-testid="receiving-create-form"
        >
          <h3 className="text-sm font-medium text-slate-300">Create draft receipt</h3>
          <label htmlFor="receiving-create-po-select" className="block text-xs text-slate-500">
            Issued purchase order
            <select
              id="receiving-create-po-select"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-sm text-slate-200"
              value={selectedPurchaseOrderId}
              onChange={(e) => onSelectedPurchaseOrderIdChange(e.target.value)}
              data-testid="receiving-create-po-select"
            >
              <option value="">Select PO…</option>
              {issuedPurchaseOrders.map((po) => (
                <option key={po.purchaseOrderId} value={po.purchaseOrderId}>
                  {po.orderKey} — {po.title}
                </option>
              ))}
            </select>
          </label>
          {selectedPo ? (
            <p className="text-xs text-slate-500">
              {selectedPo.lines.length} line(s);{' '}
              {selectedPo.lines.reduce((sum, l) => sum + l.quantityRemaining, 0)} units remaining
            </p>
          ) : null}
          <label htmlFor="receiving-create-key-input" className="block text-xs text-slate-500">
            Receipt key
            <input
              id="receiving-create-key-input"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-sm text-slate-200"
              value={receiptKey}
              onChange={(e) => onReceiptKeyChange(e.target.value)}
              data-testid="receiving-create-key-input"
            />
          </label>
          <label htmlFor="receiving-create-bin-select" className="block text-xs text-slate-500">
            Destination bin
            <select
              id="receiving-create-bin-select"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-sm text-slate-200"
              value={selectedBinId}
              onChange={(e) => onSelectedBinIdChange(e.target.value)}
              data-testid="receiving-create-bin-select"
            >
              <option value="">Select bin…</option>
              {bins.map((bin) => (
                <option key={bin.binId} value={bin.binId}>
                  {bin.binKey} — {bin.name} ({bin.locationKey})
                </option>
              ))}
            </select>
          </label>
          <button
            type="button"
            className="rounded-md bg-violet-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
            onClick={onCreateFromPurchaseOrder}
            disabled={
              isCreating ||
              !receiptKey.trim() ||
              !selectedPurchaseOrderId ||
              !selectedBinId
            }
            data-testid="receiving-create-button"
          >
            {isCreating ? 'Creating…' : 'Create receiving receipt'}
          </button>
        </div>
      ) : null}
    </section>
  )
}
