import { ApiErrorCallout } from '@stl/shared-ui'
import type { ProductPermissionCatalogItemResponse } from '../api/types'

interface ProductPermissionCatalogPanelProps {
  productKeyFilter: string
  catalog: ProductPermissionCatalogItemResponse[]
  isLoading: boolean
  isError?: boolean
  readErrorMessage?: string | null
  onRetryRead?: () => void
  onProductKeyFilterChange: (value: string) => void
}

function formatTimestamp(value: string): string {
  return new Date(value).toLocaleString()
}

export function ProductPermissionCatalogPanel({
  productKeyFilter,
  catalog,
  isLoading,
  isError = false,
  readErrorMessage = null,
  onRetryRead,
  onProductKeyFilterChange,
}: ProductPermissionCatalogPanelProps) {
  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-sm font-medium text-slate-300">Product permission catalog</h2>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
            Review permissions published by other products and map them into roles when needed.
          </p>
        </div>
        <span className="rounded-full bg-slate-800 px-3 py-1 text-xs uppercase tracking-wide text-slate-300">
          {catalog.length} entries
        </span>
      </div>

      <div className="mt-4 grid gap-3 sm:grid-cols-[1fr_auto]">
        <label htmlFor="product-permission-filter" className="block text-sm text-slate-300">
          Filter by product
          <input
            id="product-permission-filter"
            value={productKeyFilter}
            onChange={(event) => onProductKeyFilterChange(event.target.value)}
            placeholder="staffarr, maintainarr, routarr..."
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 placeholder:text-[var(--color-text-muted)]"
          />
        </label>
        <div className="flex items-end">
          <button
            type="button"
            onClick={() => onProductKeyFilterChange('')}
            disabled={!productKeyFilter}
            className="rounded-md border border-slate-600 px-3 py-2 text-sm text-slate-200 hover:bg-slate-800 disabled:opacity-50"
          >
            Clear filter
          </button>
        </div>
      </div>

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading product permission catalog…</p>
      ) : isError ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="Product permission catalog unavailable"
            message={readErrorMessage ?? 'Failed to load product permission catalog.'}
            onRetry={onRetryRead}
            retryLabel="Retry catalog"
          />
        </div>
      ) : catalog.length === 0 ? (
        <p className="mt-4 text-sm text-slate-400">
          No catalog entries were returned for this filter.
        </p>
      ) : (
        <ul className="mt-4 divide-y divide-slate-700 text-sm">
          {catalog.map((item) => (
            <li key={`${item.productKey}-${item.permissionKey}-${item.permissionTemplateId}`} className="py-3">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <p className="text-white">{item.label}</p>
                  <p className="text-xs text-slate-400">
                    <span className="font-mono text-sky-300">{item.permissionKey}</span> · {item.productKey}
                  </p>
                  {item.description ? (
                    <p className="mt-1 text-xs text-[var(--color-text-muted)]">{item.description}</p>
                  ) : null}
                </div>
                <div className="flex flex-wrap gap-2 text-[11px] uppercase tracking-wide text-slate-300">
                  <span className="rounded-full bg-slate-800 px-2 py-1">{item.scope}</span>
                  <span className="rounded-full bg-slate-800 px-2 py-1">{item.sensitivity}</span>
                  <span className="rounded-full bg-slate-800 px-2 py-1">{item.status}</span>
                </div>
              </div>
              <p className="mt-2 text-xs text-[var(--color-text-muted)]">
                Last synced {formatTimestamp(item.lastSyncedAt)}
              </p>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
