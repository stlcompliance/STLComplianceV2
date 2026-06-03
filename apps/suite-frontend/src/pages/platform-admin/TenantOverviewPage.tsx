import { useQuery } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import * as nexarr from '../../api/nexarrClient'
import { TenantCatalogAdminPanel } from '../../components/platform-admin/TenantCatalogAdminPanel'
import type { PlatformAuditEventTimelineItem } from '../../api/types'

export function TenantOverviewPage() {
  const [selectedTenantId, setSelectedTenantId] = useState('')

  const overviewQuery = useQuery({
    queryKey: ['platform-admin-tenant-overview'],
    queryFn: () => nexarr.getPlatformAdminTenantOverview(1, 100),
  })

  const tenants = overviewQuery.data?.items ?? []
  const selectedTenant = useMemo(
    () => tenants.find((tenant) => tenant.tenantId === selectedTenantId) ?? tenants[0] ?? null,
    [selectedTenantId, tenants],
  )

  const auditQuery = useQuery({
    queryKey: ['platform-admin-tenant-audit', selectedTenant?.tenantId],
    queryFn: () => nexarr.getTenantAuditEvents(selectedTenant!.tenantId, { page: 1, pageSize: 10 }),
    enabled: Boolean(selectedTenant),
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

  return (
    <div className="space-y-6">
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
            {tenants.map((tenant) => {
              const isSelected = selectedTenant?.tenantId === tenant.tenantId
              return (
                <tr
                  key={tenant.tenantId}
                  className={`cursor-pointer border-b border-slate-100 ${isSelected ? 'bg-amber-50' : ''}`}
                  onClick={() => setSelectedTenantId(tenant.tenantId)}
                >
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
              )
            })}
          </tbody>
        </table>
      </div>

      <section className="rounded-lg border border-slate-200 bg-white p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h3 className="text-base font-semibold text-stl-navy">Tenant audit history</h3>
            <p className="text-sm text-slate-500">
              {selectedTenant ? `Events for ${selectedTenant.displayName} (${selectedTenant.slug})` : 'Select a tenant to inspect audit events.'}
            </p>
          </div>
        </div>

        {!selectedTenant ? null : auditQuery.isLoading ? (
          <p className="mt-3 text-sm text-slate-500">Loading audit history…</p>
        ) : auditQuery.isError ? (
          <ApiErrorCallout
            message={getErrorMessage(auditQuery.error, 'Failed to load tenant audit history.')}
            onRetry={() => void auditQuery.refetch()}
            retryLabel="Retry audit"
          />
        ) : auditQuery.data?.items.length ? (
          <AuditEventList items={auditQuery.data.items} />
        ) : (
          <p className="mt-3 text-sm text-slate-500">No tenant audit events found.</p>
        )}
      </section>

      <TenantCatalogAdminPanel />
    </div>
  )
}

function AuditEventList({ items }: { items: PlatformAuditEventTimelineItem[] }) {
  return (
    <ul className="mt-3 space-y-2">
      {items.map((item) => (
        <li key={item.auditEventId} className="rounded-md border border-slate-200 bg-slate-50 p-3 text-sm">
          <div className="font-medium text-stl-navy">{item.action}</div>
          <p className="mt-1 text-xs text-slate-500">
            {item.result}
            {item.targetType ? ` · ${item.targetType}` : ''}
            {item.actorUserId ? ` · actor ${item.actorUserId}` : ''}
          </p>
          <p className="mt-1 text-xs text-slate-500">{new Date(item.occurredAt).toLocaleString()}</p>
        </li>
      ))}
    </ul>
  )
}
