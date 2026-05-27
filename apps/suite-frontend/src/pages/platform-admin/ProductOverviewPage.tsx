import { useQuery } from '@tanstack/react-query'
import * as nexarr from '../../api/nexarrClient'

export function ProductOverviewPage() {
  const overviewQuery = useQuery({
    queryKey: ['platform-admin-product-overview'],
    queryFn: () => nexarr.getPlatformAdminProductOverview(),
  })

  if (overviewQuery.isLoading) {
    return <p className="text-sm text-slate-500">Loading products…</p>
  }

  if (overviewQuery.isError) {
    return (
      <p className="text-sm text-red-700" role="alert">
        Failed to load products: {(overviewQuery.error as Error).message}
      </p>
    )
  }

  const products = overviewQuery.data!

  return (
    <div className="overflow-x-auto rounded-lg border border-slate-200 bg-white">
      <table className="min-w-full text-left text-sm">
        <thead className="border-b border-slate-200 bg-slate-50 text-xs uppercase text-slate-500">
          <tr>
            <th className="px-3 py-2">Product</th>
            <th className="px-3 py-2">Active</th>
            <th className="px-3 py-2">Entitlements</th>
            <th className="px-3 py-2">Launch profile</th>
            <th className="px-3 py-2">Base URL</th>
          </tr>
        </thead>
        <tbody>
          {products.map((product) => (
            <tr key={product.productKey} className="border-b border-slate-100">
              <td className="px-3 py-2">
                <span className="font-medium text-stl-navy">{product.displayName}</span>
                <span className="block text-xs text-slate-500">{product.productKey}</span>
              </td>
              <td className="px-3 py-2">{product.isActive ? 'Yes' : 'No'}</td>
              <td className="px-3 py-2">{product.activeEntitlementCount}</td>
              <td className="px-3 py-2">
                {product.launchProfileActive
                  ? 'Active'
                  : product.hasLaunchProfile
                    ? 'Inactive'
                    : 'Missing'}
              </td>
              <td className="px-3 py-2 font-mono text-xs text-slate-600">
                {product.baseUrl ?? '—'}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
