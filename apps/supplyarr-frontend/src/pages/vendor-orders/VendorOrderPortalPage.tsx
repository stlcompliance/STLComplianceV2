import { useMutation, useQuery } from '@tanstack/react-query'
import { Camera, CheckCircle2, Clock3, FileText, PackageCheck } from 'lucide-react'
import { useMemo, useState, type ReactNode } from 'react'
import { Navigate, useParams } from 'react-router-dom'
import type { RegisterVendorOrderDocumentRequest, UpdateVendorOrderStatusRequest } from '../../api/types'
import {
  getVendorAccessOrder,
  registerVendorAccessOrderDocument,
  submitVendorAccessOrderStatus,
} from '../../api/vendorOrderClient'
import {
  formatVendorOrderDateTime,
  humanizeVendorOrderValue,
  quantitySummary,
  vendorOrderStatusTone,
} from './vendorOrderUi'

const VENDOR_STATUS_OPTIONS = [
  'acknowledged',
  'in_progress',
  'partially_ready',
  'completed_ready_for_dispatch',
  'unable_to_fulfill',
]

const DOCUMENT_TYPES = ['photo', 'packing_slip', 'scale_ticket', 'proof_of_readiness', 'other']

export function VendorOrderPortalPage() {
  const { token } = useParams<{ token: string }>()
  const [statusDraft, setStatusDraft] = useState<UpdateVendorOrderStatusRequest>({
    newStatus: 'acknowledged',
    quantityReady: null,
    estimatedReadyAt: null,
    confirmedReadyAt: null,
    pickupWindowStart: null,
    pickupWindowEnd: null,
    note: null,
    exceptionReason: null,
    readyForPickupConfirmed: false,
  })
  const [documentDraft, setDocumentDraft] = useState<RegisterVendorOrderDocumentRequest>({
    documentType: 'photo',
    fileName: '',
    contentType: '',
    storageProvider: 'shared_blob',
    storageKey: '',
    sizeBytes: null,
  })
  const [lastSavedAt, setLastSavedAt] = useState<string | null>(null)

  if (!token) {
    return <Navigate to="/vendor-portal" replace />
  }

  const portalQuery = useQuery({
    queryKey: ['supplyarr-vendor-order-portal', token],
    queryFn: () => getVendorAccessOrder(token),
  })

  const portal = portalQuery.data
  const latestStatusUpdate = useMemo(
    () => portal?.statusHistory[portal.statusHistory.length - 1] ?? null,
    [portal],
  )

  const statusMutation = useMutation({
    mutationFn: () => submitVendorAccessOrderStatus(token, statusDraft),
    onSuccess: async (updated) => {
      setLastSavedAt(updated.statusHistory[updated.statusHistory.length - 1]?.createdAt ?? null)
      await portalQuery.refetch()
    },
  })

  const documentMutation = useMutation({
    mutationFn: () => registerVendorAccessOrderDocument(token, documentDraft),
    onSuccess: async (updated) => {
      setDocumentDraft({
        documentType: 'photo',
        fileName: '',
        contentType: '',
        storageProvider: 'shared_blob',
        storageKey: '',
        sizeBytes: null,
      })
      setLastSavedAt(updated.statusHistory[updated.statusHistory.length - 1]?.createdAt ?? lastSavedAt)
      await portalQuery.refetch()
    },
  })

  if (portalQuery.isLoading) {
    return (
      <main className="min-h-screen bg-[#fff8ee] px-4 py-8 text-slate-900">
        <div className="mx-auto max-w-4xl rounded-[2rem] border border-amber-200 bg-white p-6 shadow-sm">
          Loading vendor order…
        </div>
      </main>
    )
  }

  if (portalQuery.isError || !portal) {
    return (
      <main className="min-h-screen bg-[#fff8ee] px-4 py-8 text-slate-900">
        <div className="mx-auto max-w-4xl rounded-[2rem] border border-rose-200 bg-white p-6 shadow-sm">
          <h1 className="text-2xl font-bold text-slate-900">Vendor order link unavailable</h1>
          <p className="mt-3 text-sm text-slate-600">
            This magic link may be invalid, revoked, or expired.
          </p>
        </div>
      </main>
    )
  }

  const readyForPickupSelected = statusDraft.newStatus === 'completed_ready_for_dispatch'
  const readyConfirmationMissing = readyForPickupSelected && !statusDraft.readyForPickupConfirmed

  return (
    <main className="min-h-screen bg-[radial-gradient(circle_at_top,#fff4d6,transparent_45%),linear-gradient(180deg,#fff8ee_0%,#fff 100%)] px-4 py-6 text-slate-900 sm:px-6 sm:py-10">
      <div className="mx-auto max-w-5xl space-y-6">
        <header className="rounded-[2rem] border border-amber-200 bg-white p-6 shadow-sm">
          <div className="flex flex-wrap items-start justify-between gap-4">
            <div>
              <p className="text-xs uppercase tracking-[0.25em] text-amber-700">SupplyArr vendor portal</p>
              <h1 className="mt-2 text-3xl font-bold text-slate-900">Order readiness confirmation</h1>
              <p className="mt-3 max-w-3xl text-sm text-slate-600">
                Update only the order readiness details for this specific vendor order. Transportation dispatch, pricing,
                driver assignment, and broker-only workflow stay outside this portal.
              </p>
            </div>
            <div className="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm">
              <p className="font-semibold text-slate-900">{humanizeVendorOrderValue(portal.status)}</p>
              <p className="mt-1 text-slate-600">Link expires {formatVendorOrderDateTime(portal.linkExpiresAt)}</p>
            </div>
          </div>
        </header>

        <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          <PortalCard
            icon={<PackageCheck className="h-5 w-5" />}
            label="Current status"
            value={humanizeVendorOrderValue(portal.status)}
            detail={quantitySummary(portal.orderedQuantity, portal.quantityReady, portal.quantityRemaining, portal.quantityUom)}
          />
          <PortalCard
            icon={<Clock3 className="h-5 w-5" />}
            label="Expected ready"
            value={formatVendorOrderDateTime(portal.expectedReadyAt)}
            detail={`Confirmed ${formatVendorOrderDateTime(portal.confirmedReadyAt)}`}
          />
          <PortalCard
            icon={<FileText className="h-5 w-5" />}
            label="Pickup window"
            value={formatVendorOrderDateTime(portal.pickupWindowStart)}
            detail={`Until ${formatVendorOrderDateTime(portal.pickupWindowEnd)}`}
          />
          <PortalCard
            icon={<CheckCircle2 className="h-5 w-5" />}
            label="Last update"
            value={formatVendorOrderDateTime(latestStatusUpdate?.createdAt)}
            detail={latestStatusUpdate ? humanizeVendorOrderValue(latestStatusUpdate.newStatus) : 'No updates yet'}
          />
        </section>

        <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_22rem]">
          <div className="space-y-6">
            <section className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-sm">
              <h2 className="text-xl font-bold text-slate-900">Order summary</h2>
              <div className="mt-4 grid gap-4 md:grid-cols-2">
                <SummaryField label="Vendor organization" value={portal.vendorNameSnapshot} />
                <SummaryField label="Item description" value={portal.itemDescription} />
                <SummaryField label="Ordered quantity" value={`${portal.orderedQuantity} ${portal.quantityUom}`} />
                <SummaryField label="Current ready quantity" value={`${portal.quantityReady} ${portal.quantityUom}`} />
                <SummaryField label="Pickup location" value={portal.pickupLocationNameSnapshot} />
                <SummaryField label="Pickup address" value={portal.pickupAddressSnapshot} />
                {portal.deliveryLocationNameSnapshot ? (
                  <SummaryField label="Destination summary" value={portal.deliveryLocationNameSnapshot} />
                ) : null}
                {portal.deliveryAddressSnapshot ? (
                  <SummaryField label="Destination address" value={portal.deliveryAddressSnapshot} />
                ) : null}
                <SummaryField label="Pickup instructions" value={portal.pickupInstructions ?? 'No pickup instructions recorded'} />
              </div>
            </section>

            <section className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-sm">
              <h2 className="text-xl font-bold text-slate-900">Update readiness</h2>
              <p className="mt-2 text-sm text-slate-600">
                Choose the current order state, set the ready quantity, and confirm readiness explicitly when the order
                is complete and staged for pickup.
              </p>

              <div className="mt-5 grid gap-4 md:grid-cols-2">
                <label className="text-sm text-slate-700">
                  Status
                  <select
                    className="mt-1 block w-full rounded-2xl border border-slate-300 bg-white px-3 py-3 text-slate-900"
                    value={statusDraft.newStatus}
                    onChange={(event) => setStatusDraft({ ...statusDraft, newStatus: event.target.value })}
                  >
                    {VENDOR_STATUS_OPTIONS.map((status) => (
                      <option key={status} value={status}>
                        {humanizeVendorOrderValue(status)}
                      </option>
                    ))}
                  </select>
                </label>

                <label className="text-sm text-slate-700">
                  Quantity ready
                  <input
                    className="mt-1 block w-full rounded-2xl border border-slate-300 bg-white px-3 py-3 text-slate-900"
                    inputMode="decimal"
                    value={statusDraft.quantityReady?.toString() ?? ''}
                    onChange={(event) =>
                      setStatusDraft({
                        ...statusDraft,
                        quantityReady: event.target.value ? Number(event.target.value) : null,
                      })
                    }
                  />
                </label>

                <label className="text-sm text-slate-700">
                  Estimated ready at
                  <input
                    type="datetime-local"
                    className="mt-1 block w-full rounded-2xl border border-slate-300 bg-white px-3 py-3 text-slate-900"
                    value={toDatetimeLocalValue(statusDraft.estimatedReadyAt)}
                    onChange={(event) =>
                      setStatusDraft({ ...statusDraft, estimatedReadyAt: fromDatetimeLocalValue(event.target.value) })
                    }
                  />
                </label>

                <label className="text-sm text-slate-700">
                  Confirmed ready at
                  <input
                    type="datetime-local"
                    className="mt-1 block w-full rounded-2xl border border-slate-300 bg-white px-3 py-3 text-slate-900"
                    value={toDatetimeLocalValue(statusDraft.confirmedReadyAt)}
                    onChange={(event) =>
                      setStatusDraft({ ...statusDraft, confirmedReadyAt: fromDatetimeLocalValue(event.target.value) })
                    }
                  />
                </label>

                <label className="text-sm text-slate-700">
                  Pickup window start
                  <input
                    type="datetime-local"
                    className="mt-1 block w-full rounded-2xl border border-slate-300 bg-white px-3 py-3 text-slate-900"
                    value={toDatetimeLocalValue(statusDraft.pickupWindowStart)}
                    onChange={(event) =>
                      setStatusDraft({ ...statusDraft, pickupWindowStart: fromDatetimeLocalValue(event.target.value) })
                    }
                  />
                </label>

                <label className="text-sm text-slate-700">
                  Pickup window end
                  <input
                    type="datetime-local"
                    className="mt-1 block w-full rounded-2xl border border-slate-300 bg-white px-3 py-3 text-slate-900"
                    value={toDatetimeLocalValue(statusDraft.pickupWindowEnd)}
                    onChange={(event) =>
                      setStatusDraft({ ...statusDraft, pickupWindowEnd: fromDatetimeLocalValue(event.target.value) })
                    }
                  />
                </label>
              </div>

              <label className="mt-4 block text-sm text-slate-700">
                Pickup note or update
                <textarea
                  className="mt-1 block w-full rounded-2xl border border-slate-300 bg-white px-3 py-3 text-slate-900"
                  rows={4}
                  value={statusDraft.note ?? ''}
                  onChange={(event) => setStatusDraft({ ...statusDraft, note: event.target.value || null })}
                  placeholder="Staging note, dock readiness, loading instruction, or broker-safe comment."
                />
              </label>

              {statusDraft.newStatus === 'unable_to_fulfill' ? (
                <label className="mt-4 block text-sm text-slate-700">
                  Exception reason
                  <textarea
                    className="mt-1 block w-full rounded-2xl border border-slate-300 bg-white px-3 py-3 text-slate-900"
                    rows={3}
                    value={statusDraft.exceptionReason ?? ''}
                    onChange={(event) =>
                      setStatusDraft({ ...statusDraft, exceptionReason: event.target.value || null })
                    }
                    placeholder="Explain what prevents fulfillment."
                  />
                </label>
              ) : null}

              <label className="mt-4 flex items-start gap-3 rounded-2xl border border-amber-200 bg-amber-50 p-4 text-sm text-slate-800">
                <input
                  type="checkbox"
                  className="mt-1"
                  checked={statusDraft.readyForPickupConfirmed}
                  onChange={(event) =>
                    setStatusDraft({ ...statusDraft, readyForPickupConfirmed: event.target.checked })
                  }
                />
                <span>I confirm this order is complete, staged, and ready for pickup.</span>
              </label>

              {readyConfirmationMissing ? (
                <p className="mt-3 text-sm text-red-600">
                  The readiness confirmation checkbox is required when you mark this order ready for pickup.
                </p>
              ) : null}

              {statusMutation.error instanceof Error ? (
                <p className="mt-3 text-sm text-red-600">{statusMutation.error.message}</p>
              ) : null}

              <button
                type="button"
                className="mt-5 inline-flex rounded-2xl bg-emerald-600 px-5 py-3 text-sm font-semibold text-white hover:bg-emerald-500 disabled:opacity-50"
                disabled={statusMutation.isPending || readyConfirmationMissing}
                onClick={() => statusMutation.mutate()}
              >
                {statusMutation.isPending ? 'Saving update…' : 'Submit readiness update'}
              </button>
            </section>

            <section className="rounded-[2rem] border border-slate-200 bg-white p-6 shadow-sm">
              <h2 className="text-xl font-bold text-slate-900">Register supporting documents</h2>
              <p className="mt-2 text-sm text-slate-600">
                Add a packing slip, scale ticket, proof-of-readiness photo, or other broker-safe document reference for this order.
              </p>

              <div className="mt-4 grid gap-4 md:grid-cols-2">
                <label className="text-sm text-slate-700 md:col-span-2">
                  Pick file
                  <input
                    type="file"
                    className="mt-1 block w-full text-sm text-slate-700"
                    onChange={(event) => {
                      const file = event.target.files?.[0]
                      if (!file) {
                        return
                      }

                      setDocumentDraft((current) => ({
                        ...current,
                        fileName: file.name,
                        contentType: file.type || 'application/octet-stream',
                        sizeBytes: file.size,
                        storageKey: current.storageKey || `vendor-order/${file.name}`,
                      }))
                    }}
                  />
                </label>

                <label className="text-sm text-slate-700">
                  Document type
                  <select
                    className="mt-1 block w-full rounded-2xl border border-slate-300 bg-white px-3 py-3 text-slate-900"
                    value={documentDraft.documentType}
                    onChange={(event) => setDocumentDraft({ ...documentDraft, documentType: event.target.value })}
                  >
                    {DOCUMENT_TYPES.map((documentType) => (
                      <option key={documentType} value={documentType}>
                        {humanizeVendorOrderValue(documentType)}
                      </option>
                    ))}
                  </select>
                </label>

                <label className="text-sm text-slate-700">
                  Content type
                  <input
                    className="mt-1 block w-full rounded-2xl border border-slate-300 bg-white px-3 py-3 text-slate-900"
                    value={documentDraft.contentType}
                    onChange={(event) => setDocumentDraft({ ...documentDraft, contentType: event.target.value })}
                  />
                </label>

                <label className="text-sm text-slate-700">
                  File name
                  <input
                    className="mt-1 block w-full rounded-2xl border border-slate-300 bg-white px-3 py-3 text-slate-900"
                    value={documentDraft.fileName}
                    onChange={(event) => setDocumentDraft({ ...documentDraft, fileName: event.target.value })}
                  />
                </label>

                <label className="text-sm text-slate-700">
                  Storage key
                  <input
                    className="mt-1 block w-full rounded-2xl border border-slate-300 bg-white px-3 py-3 text-slate-900"
                    value={documentDraft.storageKey ?? ''}
                    onChange={(event) => setDocumentDraft({ ...documentDraft, storageKey: event.target.value })}
                  />
                </label>
              </div>

              {documentMutation.error instanceof Error ? (
                <p className="mt-3 text-sm text-red-600">{documentMutation.error.message}</p>
              ) : null}

              <button
                type="button"
                className="mt-5 inline-flex items-center gap-2 rounded-2xl bg-slate-900 px-5 py-3 text-sm font-semibold text-white hover:bg-slate-800 disabled:opacity-50"
                disabled={
                  documentMutation.isPending ||
                  !documentDraft.fileName.trim() ||
                  !documentDraft.contentType.trim()
                }
                onClick={() => documentMutation.mutate()}
              >
                <Camera className="h-4 w-4" />
                {documentMutation.isPending ? 'Saving document…' : 'Register document'}
              </button>
            </section>
          </div>

          <aside className="space-y-6">
            <section className="rounded-[2rem] border border-slate-200 bg-white p-5 shadow-sm">
              <h2 className="text-lg font-bold text-slate-900">Update status</h2>
              <div className="mt-4 flex flex-wrap items-center gap-2">
                <span className={`inline-flex rounded-full border px-3 py-1 text-xs font-semibold ${
                  vendorOrderStatusTone(portal.status) === 'good'
                    ? 'border-emerald-200 bg-emerald-50 text-emerald-700'
                    : vendorOrderStatusTone(portal.status) === 'bad'
                      ? 'border-red-200 bg-red-50 text-red-700'
                      : 'border-amber-200 bg-amber-50 text-amber-700'
                }`}>
                  {humanizeVendorOrderValue(portal.status)}
                </span>
              </div>
              <p className="mt-3 text-sm text-slate-600">
                {quantitySummary(portal.orderedQuantity, portal.quantityReady, portal.quantityRemaining, portal.quantityUom)}
              </p>
              {lastSavedAt ? (
                <p className="mt-3 text-sm text-emerald-700">
                  Update saved {formatVendorOrderDateTime(lastSavedAt)}.
                </p>
              ) : null}
            </section>

            <section className="rounded-[2rem] border border-slate-200 bg-white p-5 shadow-sm">
              <h2 className="text-lg font-bold text-slate-900">Recent history</h2>
              {portal.statusHistory.length > 0 ? (
                <ol className="mt-4 space-y-3">
                  {[...portal.statusHistory].reverse().slice(0, 5).map((entry) => (
                    <li key={entry.statusUpdateId} className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
                      <p className="font-medium text-slate-900">{humanizeVendorOrderValue(entry.newStatus)}</p>
                      <p className="mt-1 text-xs text-slate-600">{formatVendorOrderDateTime(entry.createdAt)}</p>
                      <p className="mt-2 text-xs text-slate-700">
                        {quantitySummary(entry.orderedQuantitySnapshot, entry.quantityReady, entry.quantityRemaining, portal.quantityUom)}
                      </p>
                      {entry.note ? <p className="mt-2 text-xs text-slate-700">{entry.note}</p> : null}
                    </li>
                  ))}
                </ol>
              ) : (
                <p className="mt-4 text-sm text-slate-600">No status updates have been recorded yet.</p>
              )}
            </section>

            <section className="rounded-[2rem] border border-slate-200 bg-white p-5 shadow-sm">
              <h2 className="text-lg font-bold text-slate-900">Documents</h2>
              {portal.documents.length > 0 ? (
                <ul className="mt-4 space-y-3">
                  {portal.documents.map((document) => (
                    <li key={document.documentId} className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
                      <p className="font-medium text-slate-900">{document.fileName}</p>
                      <p className="mt-1 text-xs text-slate-600">{humanizeVendorOrderValue(document.documentType)}</p>
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="mt-4 text-sm text-slate-600">No documents have been registered yet.</p>
              )}
            </section>
          </aside>
        </div>
      </div>
    </main>
  )
}

function PortalCard({
  icon,
  label,
  value,
  detail,
}: {
  icon: ReactNode
  label: string
  value: string
  detail: string
}) {
  return (
    <section className="rounded-[1.5rem] border border-slate-200 bg-white p-4 shadow-sm">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-xs uppercase tracking-[0.2em] text-slate-500">{label}</p>
          <p className="mt-3 text-lg font-bold text-slate-900">{value}</p>
          <p className="mt-2 text-xs text-slate-600">{detail}</p>
        </div>
        <div className="rounded-2xl bg-amber-50 p-3 text-amber-700">{icon}</div>
      </div>
    </section>
  )
}

function SummaryField({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
      <p className="text-xs uppercase tracking-[0.2em] text-slate-500">{label}</p>
      <p className="mt-2 text-sm font-medium text-slate-900">{value}</p>
    </div>
  )
}

function fromDatetimeLocalValue(value: string): string | null {
  if (!value) {
    return null
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return null
  }

  return date.toISOString()
}

function toDatetimeLocalValue(value: string | null | undefined): string {
  if (!value) {
    return ''
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return ''
  }

  const year = date.getFullYear()
  const month = `${date.getMonth() + 1}`.padStart(2, '0')
  const day = `${date.getDate()}`.padStart(2, '0')
  const hours = `${date.getHours()}`.padStart(2, '0')
  const minutes = `${date.getMinutes()}`.padStart(2, '0')
  return `${year}-${month}-${day}T${hours}:${minutes}`
}
