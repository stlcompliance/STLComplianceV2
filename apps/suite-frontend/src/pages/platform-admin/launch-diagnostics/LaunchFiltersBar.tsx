import type { LaunchDiagnosticRow } from '../../../api/types'

type Props = {
  rows: LaunchDiagnosticRow[]
  tenantId: string
  productKey: string
  result: string
  onTenantIdChange: (value: string) => void
  onProductKeyChange: (value: string) => void
  onResultChange: (value: string) => void
  onReset: () => void
}

export function LaunchFiltersBar({
  rows,
  tenantId,
  productKey,
  result,
  onTenantIdChange,
  onProductKeyChange,
  onResultChange,
  onReset,
}: Props) {
  const tenants = [...new Map(rows.map((row) => [row.tenantId, row])).values()].sort((a, b) =>
    a.tenantDisplayName.localeCompare(b.tenantDisplayName),
  )
  const products = [...new Map(rows.map((row) => [row.productKey, row])).values()].sort((a, b) =>
    a.productDisplayName.localeCompare(b.productDisplayName),
  )

  return (
    <section className="rounded-lg border border-slate-200 bg-white p-4">
      <div className="grid gap-3 md:grid-cols-4">
        <label className="text-xs font-medium text-slate-600">
          Filter tenant
          <select
            className="mt-1 w-full rounded-md border border-slate-300 px-2 py-1 text-sm"
            value={tenantId}
            onChange={(event) => onTenantIdChange(event.target.value)}
          >
            <option value="">All tenants</option>
            {tenants.map((tenant) => (
              <option key={tenant.tenantId} value={tenant.tenantId}>
                {tenant.tenantDisplayName} ({tenant.tenantSlug})
              </option>
            ))}
          </select>
        </label>
        <label className="text-xs font-medium text-slate-600">
          Filter product
          <select
            className="mt-1 w-full rounded-md border border-slate-300 px-2 py-1 text-sm"
            value={productKey}
            onChange={(event) => onProductKeyChange(event.target.value)}
          >
            <option value="">All products</option>
            {products.map((product) => (
              <option key={product.productKey} value={product.productKey}>
                {product.productDisplayName}
              </option>
            ))}
          </select>
        </label>
        <label className="text-xs font-medium text-slate-600">
          Launch result
          <select
            className="mt-1 w-full rounded-md border border-slate-300 px-2 py-1 text-sm"
            value={result}
            onChange={(event) => onResultChange(event.target.value)}
          >
            <option value="">All results</option>
            <option value="success">Success</option>
            <option value="denied">Denied</option>
            <option value="error">Error</option>
          </select>
        </label>
        <div className="flex items-end">
          <button
            type="button"
            className="rounded-md border border-slate-300 px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50"
            onClick={onReset}
          >
            Clear filters
          </button>
        </div>
      </div>
    </section>
  )
}
