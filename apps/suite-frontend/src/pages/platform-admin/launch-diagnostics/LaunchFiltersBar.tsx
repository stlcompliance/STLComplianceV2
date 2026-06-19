import { useMemo } from 'react'
import { StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import type { LaunchDiagnosticRow } from '../../../api/types'

type Props = {
  rows: LaunchDiagnosticRow[]
  tenantId: string
  productKey: string
  result: string
  userId: string
  correlationId: string
  fromUtc: string
  toUtc: string
  onTenantIdChange: (value: string) => void
  onProductKeyChange: (value: string) => void
  onResultChange: (value: string) => void
  onUserIdChange: (value: string) => void
  onCorrelationIdChange: (value: string) => void
  onFromUtcChange: (value: string) => void
  onToUtcChange: (value: string) => void
  onReset: () => void
}

export function LaunchFiltersBar({
  rows,
  tenantId,
  productKey,
  result,
  userId,
  correlationId,
  fromUtc,
  toUtc,
  onTenantIdChange,
  onProductKeyChange,
  onResultChange,
  onUserIdChange,
  onCorrelationIdChange,
  onFromUtcChange,
  onToUtcChange,
  onReset,
}: Props) {
  const tenants = [...new Map(rows.map((row) => [row.tenantId, row])).values()].sort((a, b) =>
    a.tenantDisplayName.localeCompare(b.tenantDisplayName),
  )
  const products = [...new Map(rows.map((row) => [row.productKey, row])).values()].sort((a, b) =>
    a.productDisplayName.localeCompare(b.productDisplayName),
  )
  const tenantOptions = useMemo<PickerOption[]>(
    () =>
      tenants.map((tenant) => ({
        value: tenant.tenantId,
        label: `${tenant.tenantDisplayName} (${tenant.tenantSlug})`,
      })),
    [tenants],
  )
  const productOptions = useMemo<PickerOption[]>(
    () =>
      products.map((product) => ({
        value: product.productKey,
        label: product.productDisplayName,
      })),
    [products],
  )

  return (
    <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
      <div className="grid gap-3 md:grid-cols-4 xl:grid-cols-6">
        <StaticSearchPicker
          label="Filter tenant"
          id="launch-filter-tenant"
          value={tenantId}
          onChange={onTenantIdChange}
          options={tenantOptions}
          placeholder="All tenants"
          testId="launch-filter-tenant"
        />
        <StaticSearchPicker
          label="Filter product"
          id="launch-filter-product"
          value={productKey}
          onChange={onProductKeyChange}
          options={productOptions}
          placeholder="All products"
          testId="launch-filter-product"
        />
        <label className="text-xs font-medium text-[var(--color-text-muted)]">
          Launch result
          <select
            className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-2 py-1 text-sm"
            value={result}
            onChange={(event) => onResultChange(event.target.value)}
          >
            <option value="">All results</option>
            <option value="success">Success</option>
            <option value="denied">Denied</option>
            <option value="error">Error</option>
          </select>
        </label>
        <label className="text-xs font-medium text-[var(--color-text-muted)]">
          Actor user ID
          <input
            className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-2 py-1 text-sm"
            value={userId}
            onChange={(event) => onUserIdChange(event.target.value)}
            placeholder="User UUID"
          />
        </label>
        <label className="text-xs font-medium text-[var(--color-text-muted)]">
          Correlation ID
          <input
            className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-2 py-1 text-sm"
            value={correlationId}
            onChange={(event) => onCorrelationIdChange(event.target.value)}
            placeholder="Correlation UUID"
          />
        </label>
        <label className="text-xs font-medium text-[var(--color-text-muted)]">
          From
          <input
            className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-2 py-1 text-sm"
            type="datetime-local"
            value={fromUtc}
            onChange={(event) => onFromUtcChange(event.target.value)}
          />
        </label>
        <label className="text-xs font-medium text-[var(--color-text-muted)]">
          To
          <input
            className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-2 py-1 text-sm"
            type="datetime-local"
            value={toUtc}
            onChange={(event) => onToUtcChange(event.target.value)}
          />
        </label>
        <div className="flex items-end">
          <button
            type="button"
            className="rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm font-medium text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-surface-muted)]"
            onClick={onReset}
          >
            Clear filters
          </button>
        </div>
      </div>
    </section>
  )
}
