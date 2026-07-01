import { useQuery } from '@tanstack/react-query'
import { DetailBadge } from '@stl/shared-ui'
import { Link, useSearchParams } from 'react-router-dom'
import { getSupplierDirectory } from '../../api/client'
import { getSupplierOrderMetadata, getSupplierOrders } from '../../api/supplierOrderClient'
import { useSupplyArrPageAccess } from './useSupplyArrPageAccess'
import {
  formatSupplierIdentityLabel,
  formatSupplierIdentitySummary,
  formatSupplierServiceTypes,
  humanizeSupplierUnitKind,
} from '../../utils/supplierPresentation'
import {
  formatSupplierOrderDateTime,
  humanizeSupplierOrderValue,
  quantitySummary,
  supplierOrderStatusTone,
} from './supplierOrderUi'

export function SupplierOrdersPage() {
  const { session, meQuery, canReadSupplierOrders, canCreateSupplierOrders } = useSupplyArrPageAccess()
  const [searchParams, setSearchParams] = useSearchParams()
  const selectedStatus = searchParams.get('status') ?? ''
  const selectedSupplierId = searchParams.get('supplierId') ?? ''

  if (!session) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading supplier orders…</p>
  }

  if (meQuery.isLoading) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading supplier-order access…</p>
  }

  if (!canReadSupplierOrders) {
    return (
      <section className="rounded-3xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-8">
        <h1 className="text-2xl font-bold text-[var(--color-text-primary)]">Supplier orders</h1>
        <p className="mt-3 text-sm text-[var(--color-text-secondary)]">
          You do not have permission to view SupplyArr supplier orders.
        </p>
      </section>
    )
  }

  const suppliersQuery = useQuery({
    queryKey: ['supplyarr-supplier-order-suppliers', session.accessToken],
    queryFn: () => getSupplierDirectory(session.accessToken),
  })

  const metadataQuery = useQuery({
    queryKey: ['supplyarr-supplier-order-metadata', session.accessToken],
    queryFn: () => getSupplierOrderMetadata(session.accessToken),
  })

  const supplierOrdersQuery = useQuery({
    queryKey: ['supplyarr-supplier-orders', session.accessToken, selectedStatus, selectedSupplierId],
    queryFn: () =>
      getSupplierOrders(session.accessToken, {
        status: selectedStatus || undefined,
        supplierId: selectedSupplierId || undefined,
      }),
  })

  const orderCount = supplierOrdersQuery.data?.length ?? 0
  const statusFilterOptions = [{ value: '', label: 'All statuses' }, ...(metadataQuery.data?.filterStatusOptions ?? [])]
  const readyCount =
    supplierOrdersQuery.data?.filter((item) => item.status === 'completed_ready_for_dispatch').length ?? 0
  const partialCount =
    supplierOrdersQuery.data?.filter((item) => item.status === 'partially_ready').length ?? 0
  const blockedCount =
    supplierOrdersQuery.data?.filter((item) => item.status === 'unable_to_fulfill').length ?? 0

  return (
    <div className="space-y-6">
      <section className="rounded-3xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6 shadow-[var(--shadow-surface)]">
        <div className="flex flex-wrap items-start justify-between gap-5">
          <div>
            <div className="mb-3 flex flex-wrap gap-2">
              <DetailBadge label="SupplyArr" tone="info" />
              <DetailBadge label="Supplier order registry" tone="neutral" />
              <DetailBadge label={session.tenantDisplayName} tone="neutral" />
            </div>
            <h1 className="text-3xl font-bold text-[var(--color-text-primary)]">Supplier order readiness</h1>
            <p className="mt-3 max-w-3xl text-sm text-[var(--color-text-secondary)]">
              Review supplier confirmations before transportation is released. Track readiness, documents, and history in one place.
            </p>
          </div>
          {canCreateSupplierOrders ? (
            <Link
              to="/purchasing/supplier-orders/create"
              className="inline-flex items-center rounded-xl bg-[var(--color-accent)] px-4 py-3 text-sm font-semibold text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)]"
            >
              Create supplier order
            </Link>
          ) : null}
        </div>
      </section>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <SummaryCard label="Orders in scope" value={String(orderCount)} hint="Filtered supplier-order records" />
        <SummaryCard label="Ready for dispatch" value={String(readyCount)} hint="Supplier released orders" tone="good" />
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
            Supplier identity or sub-unit
            <select
              className="mt-1 block min-w-72 rounded-xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
              value={selectedSupplierId}
              onChange={(event) => {
                const next = new URLSearchParams(searchParams)
                if (event.target.value) {
                  next.set('supplierId', event.target.value)
                } else {
                  next.delete('supplierId')
                }
                setSearchParams(next)
              }}
            >
              <option value="">All suppliers</option>
              {(suppliersQuery.data ?? []).map((supplier) => (
                <option key={supplier.supplierId} value={supplier.supplierId}>
                  {formatSupplierUnitLabel(supplier)}
                </option>
              ))}
            </select>
          </label>

          {(selectedStatus || selectedSupplierId) ? (
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
          <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Supplier orders</h2>
          <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
            Full readiness, partial decisions, and split lineage stay separate from dispatch execution.
          </p>
        </div>

        {supplierOrdersQuery.isLoading ? (
          <p className="px-5 py-6 text-sm text-[var(--color-text-muted)]">Loading supplier orders…</p>
        ) : supplierOrdersQuery.isError ? (
          <p className="px-5 py-6 text-sm text-[var(--tone-danger-text)]">Unable to load supplier orders right now.</p>
        ) : supplierOrdersQuery.data && supplierOrdersQuery.data.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="bg-[var(--color-bg-surface-elevated)] text-[var(--color-text-muted)]">
                <tr>
                  <th className="px-5 py-3">Supplier order</th>
                  <th className="px-5 py-3">Supplier</th>
                  <th className="px-5 py-3">Readiness</th>
                  <th className="px-5 py-3">Expected</th>
                  <th className="px-5 py-3">Updated</th>
                  <th className="px-5 py-3 text-right">Open</th>
                </tr>
              </thead>
              <tbody>
                {supplierOrdersQuery.data.map((order) => (
                  <tr key={order.supplierOrderId} className="border-t border-[var(--color-border-subtle)] align-top">
                    <td className="px-5 py-4 text-[var(--color-text-secondary)]">
                      <div className="font-medium text-[var(--color-text-primary)]">{order.itemDescription}</div>
                      <div className="mt-1 text-xs text-[var(--color-text-muted)]">{order.supplierOrderId}</div>
                      {order.parentSupplierOrderId ? (
                        <div className="mt-2 text-xs text-[var(--color-warning-text)]">
                          Child of {order.parentSupplierOrderId}
                        </div>
                      ) : null}
                    </td>
                    <td className="px-5 py-4 text-[var(--color-text-secondary)]">
                      <div className="font-medium text-[var(--color-text-primary)]">
                        {formatSupplierIdentityLabel({
                          supplierDisplayName: order.supplierNameSnapshot,
                          parentSupplierDisplayName: order.parentSupplierDisplayName,
                          supplierUnitKind: order.supplierUnitKind,
                        })}
                      </div>
                      <div className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {humanizeSupplierUnitKind(order.supplierUnitKind)}
                      </div>
                      <div className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {formatSupplierServiceTypes(order.supplierServiceTypes)}
                      </div>
                    </td>
                    <td className="px-5 py-4">
                      <div className="flex flex-wrap items-center gap-2">
                        <DetailBadge
                          label={humanizeSupplierOrderValue(order.status)}
                          tone={supplierOrderStatusTone(order.status)}
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
                      {formatSupplierOrderDateTime(order.expectedReadyAt)}
                    </td>
                    <td className="px-5 py-4 text-[var(--color-text-muted)]">
                      {formatSupplierOrderDateTime(order.updatedAt)}
                    </td>
                    <td className="px-5 py-4 text-right">
                      <Link
                        to={`/purchasing/supplier-orders/${order.supplierOrderId}`}
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
            No supplier orders match your filters.
          </div>
        )}
      </section>
    </div>
  )
}

function formatSupplierUnitLabel(supplier: {
  displayName: string
  supplierKey?: string | null
  parentSupplierDisplayName?: string | null
  unitKind?: string | null
}) {
  return [
    humanizeSupplierUnitKind(supplier.unitKind),
    formatSupplierIdentitySummary({
      displayName: supplier.displayName,
      supplierKey: supplier.supplierKey,
      parentSupplierDisplayName: supplier.parentSupplierDisplayName,
      supplierUnitKind: supplier.unitKind,
    }),
  ]
    .filter(Boolean)
    .join(' · ')
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
