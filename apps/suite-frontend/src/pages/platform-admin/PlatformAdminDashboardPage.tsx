import { useQuery } from '@tanstack/react-query'
import * as nexarr from '../../api/nexarrClient'

function StatCard({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-lg border border-slate-200 bg-white p-4">
      <p className="text-xs font-medium uppercase tracking-wide text-slate-500">{label}</p>
      <p className="mt-1 text-2xl font-semibold text-stl-navy">{value}</p>
    </div>
  )
}

export function PlatformAdminDashboardPage() {
  const dashboardQuery = useQuery({
    queryKey: ['platform-admin-dashboard'],
    queryFn: () => nexarr.getPlatformAdminDashboard(),
  })

  if (dashboardQuery.isLoading) {
    return <p className="text-sm text-slate-500">Loading dashboard…</p>
  }

  if (dashboardQuery.isError) {
    return (
      <p className="text-sm text-red-700" role="alert">
        Failed to load dashboard: {(dashboardQuery.error as Error).message}
      </p>
    )
  }

  const d = dashboardQuery.data!
  return (
    <div className="space-y-4">
      <p className="text-xs text-slate-500">
        Generated {new Date(d.generatedAt).toLocaleString()}
      </p>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard label="Tenants (active)" value={d.activeTenantCount} />
        <StatCard label="Products (active)" value={d.activeProductCount} />
        <StatCard label="Active entitlements" value={d.activeEntitlementCount} />
        <StatCard label="Service clients" value={d.serviceClientCount} />
        <StatCard label="Active service tokens" value={d.activeServiceTokenCount} />
        <StatCard label="Launch profiles" value={d.launchProfileCount} />
        <StatCard label="Pending handoffs" value={d.pendingHandoffCount} />
        <StatCard label="Audit events (24h)" value={d.auditEventsLast24Hours} />
      </div>
      <section className="rounded-lg border border-slate-200 bg-white p-4 text-sm text-slate-700">
        <h4 className="font-semibold text-stl-navy">Totals</h4>
        <ul className="mt-2 list-disc pl-5">
          <li>
            {d.tenantCount} tenants · {d.productCount} products · {d.totalEntitlementCount}{' '}
            entitlements
          </li>
          <li>{d.expiredUnredeemedHandoffCount} expired unredeemed handoff codes</li>
        </ul>
      </section>
    </div>
  )
}
