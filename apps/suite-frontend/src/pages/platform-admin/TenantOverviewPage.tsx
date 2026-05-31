import { useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import * as nexarr from '../../api/nexarrClient'
import { TenantCatalogAdminPanel } from '../../components/platform-admin/TenantCatalogAdminPanel'

export function TenantOverviewPage() {
  const overviewQuery = useQuery({
    queryKey: ['platform-admin-tenant-overview'],
    queryFn: () => nexarr.getPlatformAdminTenantOverview(1, 100),
  })

  if (overviewQuery.isLoading) {
    return <p className="text-sm text-slate-500">Loading tenants…</p>
  }

  if (overviewQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(overviewQuery.error, 'Failed to load tenants.')}
        onRetry={() => void overviewQuery.refetch()}
        retryLabel="Retry tenants"
      />
    )
  }

  const tenants = overviewQuery.data!.items

  return (
    <div className="overflow-x-auto rounded-lg border border-slate-200 bg-white">
      <table className="min-w-full text-left text-sm">
        <thead className="border-b border-slate-200 bg-slate-50 text-xs uppercase text-slate-500">
          <tr>
            <th className="px-3 py-2">Tenant</th>
            <th className="px-3 py-2">Status</th>
            <th className="px-3 py-2">Entitlements</th>
            <th className="px-3 py-2">Members</th>
            <th className="px-3 py-2">Created</th>
          </tr>
        </thead>
        <tbody>
          {tenants.map((tenant) => (
            <tr key={tenant.tenantId} className="border-b border-slate-100">
              <td className="px-3 py-2">
                <span className="font-medium text-stl-navy">{tenant.displayName}</span>
                <span className="block text-xs text-slate-500">{tenant.slug}</span>
              </td>
              <td className="px-3 py-2">{tenant.status}</td>
              <td className="px-3 py-2">{tenant.activeEntitlementCount}</td>
              <td className="px-3 py-2">{tenant.membershipCount}</td>
              <td className="px-3 py-2 text-slate-600">
                {new Date(tenant.createdAt).toLocaleDateString()}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <TenantCatalogAdminPanel />
    </div>
  )
}
