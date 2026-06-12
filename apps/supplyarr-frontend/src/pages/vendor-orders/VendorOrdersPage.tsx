import { useQuery } from '@tanstack/react-query'
import { DetailBadge } from '@stl/shared-ui'
import { Link, useSearchParams } from 'react-router-dom'
import { getVendors } from '../../api/client'
import { getVendorOrderMetadata, getVendorOrders } from '../../api/vendorOrderClient'
import { useSupplyArrPageAccess } from './useSupplyArrPageAccess'
import {
  formatVendorOrderDateTime,
  humanizeVendorOrderValue,
  quantitySummary,
  vendorOrderStatusTone,
} from './vendorOrderUi'

export function VendorOrdersPage() {
  const { session, meQuery, canReadVendorOrders, canCreateVendorOrders } = useSupplyArrPageAccess()
  const [searchParams, setSearchParams] = useSearchParams()
  const selectedStatus = searchParams.get('status') ?? ''
  const selectedVendorId = searchParams.get('vendorId') ?? ''

  if (!session) {
    return <p className="text-sm text-slate-400">Loading vendor orders…</p>
  }

  if (meQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading vendor-order access…</p>
  }

  if (!canReadVendorOrders) {
    return (
      <section className="rounded-3xl border border-slate-800 bg-slate-950/70 p-8">
        <h1 className="text-2xl font-bold text-white">Vendor orders</h1>
        <p className="mt-3 text-sm text-slate-400">
          You do not have permission to view SupplyArr vendor orders.
        </p>
      </section>
    )
  }

  const vendorsQuery = useQuery({
    queryKey: ['supplyarr-vendor-order-vendors', session.accessToken],
    queryFn: () => getVendors(session.accessToken),
  })

  const metadataQuery = useQuery({
    queryKey: ['supplyarr-vendor-order-metadata', session.accessToken],
    queryFn: () => getVendorOrderMetadata(session.accessToken),
  })

  const vendorOrdersQuery = useQuery({
    queryKey: ['supplyarr-vendor-orders', session.accessToken, selectedStatus, selectedVendorId],
    queryFn: () =>
      getVendorOrders(session.accessToken, {
        status: selectedStatus || undefined,
        vendorId: selectedVendorId || undefined,
      }),
  })

  const orderCount = vendorOrdersQuery.data?.length ?? 0
  const statusFilterOptions = [{ value: '', label: 'All statuses' }, ...(metadataQuery.data?.filterStatusOptions ?? [])]
  const readyCount =
    vendorOrdersQuery.data?.filter((item) => item.status === 'completed_ready_for_dispatch').length ?? 0
  const partialCount =
    vendorOrdersQuery.data?.filter((item) => item.status === 'partially_ready').length ?? 0
  const blockedCount =
    vendorOrdersQuery.data?.filter((item) => item.status === 'unable_to_fulfill').length ?? 0

  return (
    <div className="space-y-6">
      <section className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6 shadow-2xl shadow-sky-950/20">
        <div className="flex flex-wrap items-start justify-between gap-5">
          <div>
            <div className="mb-3 flex flex-wrap gap-2">
              <DetailBadge label="SupplyArr" tone="info" />
              <DetailBadge label="Vendor order registry" tone="neutral" />
              <DetailBadge label={session.tenantDisplayName} tone="neutral" />
            </div>
            <h1 className="text-3xl font-bold text-white">Vendor order readiness</h1>
            <p className="mt-3 max-w-3xl text-sm text-slate-300">
              Review vendor confirmations before RoutArr dispatches transportation. SupplyArr owns vendor order state,
              vendor-facing workflow, document linkage, and immutable readiness history.
            </p>
          </div>
          {canCreateVendorOrders ? (
            <Link
              to="/purchasing/vendor-orders/create"
              className="inline-flex items-center rounded-xl bg-sky-500 px-4 py-3 text-sm font-semibold text-slate-950 hover:bg-sky-400"
            >
              Create vendor order
            </Link>
          ) : null}
        </div>
      </section>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <SummaryCard label="Orders in scope" value={String(orderCount)} hint="Filtered vendor-order records" />
        <SummaryCard label="Ready for dispatch" value={String(readyCount)} hint="Vendor released orders" tone="good" />
        <SummaryCard label="Partial readiness" value={String(partialCount)} hint="Broker decision required" tone="warn" />
        <SummaryCard label="Unable to fulfill" value={String(blockedCount)} hint="Broker ops follow-up" tone="bad" />
      </div>

      <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
        <div className="flex flex-wrap items-end gap-4">
          <label className="text-sm text-slate-300">
            Status
            <select
              className="mt-1 block min-w-56 rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
              value={selectedStatus}
              onChange={(event) => {
                const next = new URLSearchParams(searchParams)
                if (event.target.value) {
                  next.set('status', event.target.value)
                } else {
                  next.delete('status')
                }
                setSearchParams(next)
              }}
            >
              {statusFilterOptions.map((status) => (
                <option key={status.value || 'all'} value={status.value}>
                  {status.label}
                </option>
              ))}
            </select>
          </label>

          <label className="text-sm text-slate-300">
            Vendor
            <select
              className="mt-1 block min-w-72 rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
              value={selectedVendorId}
              onChange={(event) => {
                const next = new URLSearchParams(searchParams)
                if (event.target.value) {
                  next.set('vendorId', event.target.value)
                } else {
                  next.delete('vendorId')
                }
                setSearchParams(next)
              }}
            >
              <option value="">All vendors</option>
              {(vendorsQuery.data ?? []).map((vendor) => (
                <option key={vendor.partyId} value={vendor.partyId}>
                  {vendor.displayName}
                </option>
              ))}
            </select>
          </label>

          {(selectedStatus || selectedVendorId) ? (
            <button
              type="button"
              className="rounded-xl border border-slate-700 bg-slate-900 px-4 py-2 text-sm text-slate-200 hover:bg-slate-800"
              onClick={() => setSearchParams(new URLSearchParams())}
            >
              Clear filters
            </button>
          ) : null}
        </div>
      </section>

      <section className="overflow-hidden rounded-2xl border border-slate-800 bg-slate-950/70">
        <div className="border-b border-slate-800 px-5 py-4">
          <h2 className="text-lg font-semibold text-white">Vendor orders</h2>
          <p className="mt-1 text-sm text-slate-400">
            Full-readiness release, partial decisions, and split lineage stay separate from RoutArr dispatch execution.
          </p>
        </div>

        {vendorOrdersQuery.isLoading ? (
          <p className="px-5 py-6 text-sm text-slate-400">Loading vendor orders…</p>
        ) : vendorOrdersQuery.isError ? (
          <p className="px-5 py-6 text-sm text-red-300">Unable to load vendor orders right now.</p>
        ) : vendorOrdersQuery.data && vendorOrdersQuery.data.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="bg-slate-950/80 text-slate-400">
                <tr>
                  <th className="px-5 py-3">Vendor order</th>
                  <th className="px-5 py-3">Vendor</th>
                  <th className="px-5 py-3">Readiness</th>
                  <th className="px-5 py-3">Expected</th>
                  <th className="px-5 py-3">Updated</th>
                  <th className="px-5 py-3 text-right">Open</th>
                </tr>
              </thead>
              <tbody>
                {vendorOrdersQuery.data.map((order) => (
                  <tr key={order.vendorOrderId} className="border-t border-slate-800 align-top">
                    <td className="px-5 py-4 text-slate-200">
                      <div className="font-medium text-white">{order.itemDescription}</div>
                      <div className="mt-1 text-xs text-slate-500">{order.vendorOrderId}</div>
                      {order.parentVendorOrderId ? (
                        <div className="mt-2 text-xs text-amber-300">
                          Child of {order.parentVendorOrderId}
                        </div>
                      ) : null}
                    </td>
                    <td className="px-5 py-4 text-slate-300">{order.vendorNameSnapshot}</td>
                    <td className="px-5 py-4">
                      <div className="flex flex-wrap items-center gap-2">
                        <DetailBadge
                          label={humanizeVendorOrderValue(order.status)}
                          tone={vendorOrderStatusTone(order.status)}
                        />
                        <span className="text-xs text-slate-400">
                          {quantitySummary(
                            order.orderedQuantity,
                            order.quantityReady,
                            order.quantityRemaining,
                            order.quantityUom,
                          )}
                        </span>
                      </div>
                    </td>
                    <td className="px-5 py-4 text-slate-300">
                      {formatVendorOrderDateTime(order.expectedReadyAt)}
                    </td>
                    <td className="px-5 py-4 text-slate-400">
                      {formatVendorOrderDateTime(order.updatedAt)}
                    </td>
                    <td className="px-5 py-4 text-right">
                      <Link
                        to={`/purchasing/vendor-orders/${order.vendorOrderId}`}
                        className="inline-flex rounded-lg border border-slate-700 px-3 py-1.5 text-xs font-semibold text-sky-300 hover:border-sky-600 hover:text-sky-200"
                      >
                        Open detail
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="px-5 py-8 text-sm text-slate-400">
            No vendor orders match the current filters.
          </div>
        )}
      </section>
    </div>
  )
}

function SummaryCard({
  label,
  value,
  hint,
  tone = 'neutral',
}: {
  label: string
  value: string
  hint: string
  tone?: 'good' | 'warn' | 'bad' | 'neutral'
}) {
  const toneClass =
    tone === 'good'
      ? 'border-emerald-500/30 bg-emerald-950/20'
      : tone === 'warn'
        ? 'border-amber-500/30 bg-amber-950/20'
        : tone === 'bad'
          ? 'border-red-500/30 bg-red-950/20'
          : 'border-slate-800 bg-slate-950/70'

  return (
    <section className={`rounded-2xl border p-4 ${toneClass}`}>
      <p className="text-sm text-sky-200/80">{label}</p>
      <p className="mt-3 text-3xl font-bold text-white">{value}</p>
      <p className="mt-2 text-xs text-slate-400">{hint}</p>
    </section>
  )
}
