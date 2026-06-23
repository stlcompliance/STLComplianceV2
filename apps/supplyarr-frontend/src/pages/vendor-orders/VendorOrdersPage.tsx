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
    return <p className="text-sm text-[var(--color-text-muted)]">Loading vendor orders…</p>
  }

  if (meQuery.isLoading) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading vendor-order access…</p>
  }

  if (!canReadVendorOrders) {
    return (
      <section className="rounded-3xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-8">
        <h1 className="text-2xl font-bold text-[var(--color-text-primary)]">Vendor orders</h1>
        <p className="mt-3 text-sm text-[var(--color-text-secondary)]">
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
      <section className="rounded-3xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6 shadow-[var(--shadow-surface)]">
        <div className="flex flex-wrap items-start justify-between gap-5">
          <div>
            <div className="mb-3 flex flex-wrap gap-2">
              <DetailBadge label="SupplyArr" tone="info" />
              <DetailBadge label="Vendor order registry" tone="neutral" />
              <DetailBadge label={session.tenantDisplayName} tone="neutral" />
            </div>
            <h1 className="text-3xl font-bold text-[var(--color-text-primary)]">Vendor order readiness</h1>
            <p className="mt-3 max-w-3xl text-sm text-[var(--color-text-secondary)]">
              Review vendor confirmations before transportation is released. Track readiness, documents, and history in one place.
            </p>
          </div>
          {canCreateVendorOrders ? (
            <Link
              to="/purchasing/vendor-orders/create"
              className="inline-flex items-center rounded-xl bg-[var(--color-accent)] px-4 py-3 text-sm font-semibold text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)]"
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

      <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
        <div className="flex flex-wrap items-end gap-4">
          <label className="text-sm text-[var(--color-text-secondary)]">
            Status
            <select
              className="mt-1 block min-w-56 rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
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

          <label className="text-sm text-[var(--color-text-secondary)]">
            Vendor
            <select
              className="mt-1 block min-w-72 rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
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
              className="rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-4 py-2 text-sm text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)]"
              onClick={() => setSearchParams(new URLSearchParams())}
            >
              Clear filters
            </button>
          ) : null}
        </div>
      </section>

      <section className="overflow-hidden rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)]">
        <div className="border-b border-[var(--color-border-subtle)] px-5 py-4">
          <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Vendor orders</h2>
          <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
            Full readiness, partial decisions, and split lineage stay separate from dispatch execution.
          </p>
        </div>

        {vendorOrdersQuery.isLoading ? (
          <p className="px-5 py-6 text-sm text-[var(--color-text-muted)]">Loading vendor orders…</p>
        ) : vendorOrdersQuery.isError ? (
          <p className="px-5 py-6 text-sm text-[var(--tone-danger-text)]">Unable to load vendor orders right now.</p>
        ) : vendorOrdersQuery.data && vendorOrdersQuery.data.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="bg-[var(--color-bg-surface-elevated)] text-[var(--color-text-muted)]">
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
                  <tr key={order.vendorOrderId} className="border-t border-[var(--color-border-subtle)] align-top">
                    <td className="px-5 py-4 text-[var(--color-text-secondary)]">
                      <div className="font-medium text-[var(--color-text-primary)]">{order.itemDescription}</div>
                      <div className="mt-1 text-xs text-[var(--color-text-muted)]">{order.vendorOrderId}</div>
                      {order.parentVendorOrderId ? (
                        <div className="mt-2 text-xs text-[var(--color-warning-text)]">
                          Child of {order.parentVendorOrderId}
                        </div>
                      ) : null}
                    </td>
                    <td className="px-5 py-4 text-[var(--color-text-secondary)]">{order.vendorNameSnapshot}</td>
                    <td className="px-5 py-4">
                      <div className="flex flex-wrap items-center gap-2">
                        <DetailBadge
                          label={humanizeVendorOrderValue(order.status)}
                          tone={vendorOrderStatusTone(order.status)}
                        />
                        <span className="text-xs text-[var(--color-text-muted)]">
                          {quantitySummary(
                            order.orderedQuantity,
                            order.quantityReady,
                            order.quantityRemaining,
                            order.quantityUom,
                          )}
                        </span>
                      </div>
                    </td>
                    <td className="px-5 py-4 text-[var(--color-text-secondary)]">
                      {formatVendorOrderDateTime(order.expectedReadyAt)}
                    </td>
                    <td className="px-5 py-4 text-[var(--color-text-muted)]">
                      {formatVendorOrderDateTime(order.updatedAt)}
                    </td>
                    <td className="px-5 py-4 text-right">
                      <Link
                        to={`/purchasing/vendor-orders/${order.vendorOrderId}`}
                        className="inline-flex rounded-lg border border-[var(--color-border-strong)] px-3 py-1.5 text-xs font-semibold text-[var(--color-accent)] hover:border-[var(--color-accent-border)] hover:text-[var(--color-accent-hover)]"
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
          <div className="px-5 py-8 text-sm text-[var(--color-text-muted)]">
            No vendor orders match your filters.
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
      ? 'border-[var(--color-success-border)] bg-[var(--color-success-bg)]'
      : tone === 'warn'
        ? 'border-[var(--color-warning-border)] bg-[var(--color-warning-bg)]'
        : tone === 'bad'
          ? 'border-[var(--tone-danger-border)] bg-[var(--tone-danger-bg)]'
          : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)]'

  return (
    <section className={`rounded-2xl border p-4 ${toneClass}`}>
      <p className="text-sm text-[var(--color-text-secondary)]">{label}</p>
      <p className="mt-3 text-3xl font-bold text-[var(--color-text-primary)]">{value}</p>
      <p className="mt-2 text-xs text-[var(--color-text-muted)]">{hint}</p>
    </section>
  )
}
