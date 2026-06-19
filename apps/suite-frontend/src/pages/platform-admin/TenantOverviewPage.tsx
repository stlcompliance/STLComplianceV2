import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { ApiErrorCallout, StaticSearchPicker, getErrorMessage, type PickerOption } from '@stl/shared-ui'
import * as nexarr from '../../api/nexarrClient'
import { TenantCatalogAdminPanel } from '../../components/platform-admin/TenantCatalogAdminPanel'
import {
  PlatformAdminKpiCard,
  PlatformAdminPageHeader,
  PlatformAdminScopeNote,
  PlatformAdminSection,
} from '../../components/platform-admin/PlatformAdminPageChrome'
import type { PlatformAuditEventTimelineItem } from '../../api/types'

export function TenantOverviewPage() {
  const queryClient = useQueryClient()
  const [selectedTenantId, setSelectedTenantId] = useState('')
  const [selectedProductKey, setSelectedProductKey] = useState('')

  const overviewQuery = useQuery({
    queryKey: ['platform-admin-tenant-overview'],
    queryFn: () => nexarr.getPlatformAdminTenantOverview(1, 100),
  })

  const productsQuery = useQuery({
    queryKey: ['platform-admin-product-overview'],
    queryFn: () => nexarr.listProducts(1, 100),
  })

  const tenants = overviewQuery.data?.items ?? []
  const selectedTenant = useMemo(
    () => tenants.find((tenant) => tenant.tenantId === selectedTenantId) ?? tenants[0] ?? null,
    [selectedTenantId, tenants],
  )

  const tenantDetailQuery = useQuery({
    queryKey: ['platform-admin-tenant-detail', selectedTenant?.tenantId],
    queryFn: () => nexarr.getTenant(selectedTenant!.tenantId),
    enabled: Boolean(selectedTenant),
  })

  const tenantMembersQuery = useQuery({
    queryKey: ['platform-admin-tenant-members', selectedTenant?.tenantId],
    queryFn: () => nexarr.getTenantMembers(selectedTenant!.tenantId),
    enabled: Boolean(selectedTenant),
  })

  const tenantEntitlementsQuery = useQuery({
    queryKey: ['platform-admin-tenant-entitlements', selectedTenant?.tenantId],
    queryFn: () => nexarr.listEntitlements(selectedTenant!.tenantId),
    enabled: Boolean(selectedTenant),
  })

  const orderedTenantEntitlements = useMemo(
    () =>
      [...(tenantEntitlementsQuery.data?.items ?? [])].sort((left, right) => {
        const leftActive = left.status.toLowerCase() === 'active'
        const rightActive = right.status.toLowerCase() === 'active'

        if (leftActive !== rightActive) {
          return leftActive ? -1 : 1
        }

        return left.productDisplayName.localeCompare(right.productDisplayName)
      }),
    [tenantEntitlementsQuery.data?.items],
  )

  const productOptions = useMemo<PickerOption[]>(
    () =>
      (productsQuery.data?.items ?? []).map((product) => ({
        value: product.productKey,
        label: `${product.displayName} (${product.productKey})`,
        inactive: !product.isActive,
      })),
    [productsQuery.data?.items],
  )

  const grantEntitlementMutation = useMutation({
    mutationFn: () => nexarr.grantTenantEntitlement(selectedTenant!.tenantId, selectedProductKey),
    onSuccess: () => {
      setSelectedProductKey('')
      void queryClient.invalidateQueries({ queryKey: ['platform-admin-tenant-entitlements', selectedTenant?.tenantId] })
      void queryClient.invalidateQueries({ queryKey: ['platform-admin-tenant-overview'] })
    },
  })

  const revokeEntitlementMutation = useMutation({
    mutationFn: (productKey: string) =>
      nexarr.revokeTenantEntitlement(selectedTenant!.tenantId, productKey),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['platform-admin-tenant-entitlements', selectedTenant?.tenantId] })
      void queryClient.invalidateQueries({ queryKey: ['platform-admin-tenant-overview'] })
    },
  })

  const serviceClientsQuery = useQuery({
    queryKey: ['platform-admin-service-clients'],
    queryFn: () => nexarr.listServiceClients(1, 100),
    enabled: Boolean(selectedTenant),
  })

  const tenantServiceClients = useMemo(
    () =>
      (serviceClientsQuery.data?.items ?? []).filter((client) =>
        client.allowedTenantIds.includes(selectedTenant?.tenantId ?? ''),
      ),
    [selectedTenant?.tenantId, serviceClientsQuery.data?.items],
  )

  const launchHistoryQuery = useQuery({
    queryKey: ['platform-admin-tenant-launch-history', selectedTenant?.tenantId],
    queryFn: () =>
      nexarr.getPlatformAdminLaunchAttempts({
        tenantId: selectedTenant!.tenantId,
        page: 1,
        pageSize: 10,
      }),
    enabled: Boolean(selectedTenant),
  })

  const auditQuery = useQuery({
    queryKey: ['platform-admin-tenant-audit', selectedTenant?.tenantId],
    queryFn: () => nexarr.getTenantAuditEvents(selectedTenant!.tenantId, { page: 1, pageSize: 10 }),
    enabled: Boolean(selectedTenant),
  })

  if (overviewQuery.isLoading) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading tenants…</p>
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
      <PlatformAdminPageHeader
        title="Tenant overview"
        summary="Selected tenant record, membership state, entitlement posture, launch attempts, and audit history for NexArr platform administration."
        badge={selectedTenant ? `${selectedTenant.status} tenant` : 'Tenant records'}
        updatedAt={selectedTenant ? new Date(selectedTenant.createdAt).toLocaleString() : undefined}
      />

      {selectedTenant ? (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
          <PlatformAdminKpiCard
            label="Members"
            value={selectedTenant.membershipCount}
            hint="Tenant membership records are owned by NexArr."
            tone="info"
          />
          <PlatformAdminKpiCard
            label="Entitlements"
            value={selectedTenant.activeEntitlementCount}
            hint="Product access granted through NexArr."
            tone="good"
          />
          <PlatformAdminKpiCard
            label="Launch attempts"
            value={launchHistoryQuery.data?.items.length ?? '—'}
            hint="Recent launch handoff results for this tenant."
            tone="warn"
          />
          <PlatformAdminKpiCard
            label="Service clients"
            value={tenantServiceClients.length || '—'}
            hint="Tenant-scoped service clients that can call NexArr."
            tone={tenantServiceClients.length > 0 ? 'good' : 'neutral'}
          />
        </div>
      ) : null}

      <div className="overflow-x-auto rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)]">
        <table className="min-w-full text-left text-sm">
          <thead className="border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] text-xs uppercase text-[var(--color-text-muted)]">
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
                  className={`cursor-pointer border-b border-[var(--color-border-subtle)] ${isSelected ? 'bg-amber-50' : ''}`}
                  onClick={() => setSelectedTenantId(tenant.tenantId)}
                >
                  <td className="px-3 py-2">
                    <span className="font-medium text-stl-navy">{tenant.displayName}</span>
                    <span className="block text-xs text-[var(--color-text-muted)]">{tenant.slug}</span>
                  </td>
                  <td className="px-3 py-2">{tenant.status}</td>
                  <td className="px-3 py-2">{tenant.activeEntitlementCount}</td>
                  <td className="px-3 py-2">{tenant.membershipCount}</td>
                  <td className="px-3 py-2 text-[var(--color-text-muted)]">
                    {new Date(tenant.createdAt).toLocaleDateString()}
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>

      <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h3 className="text-base font-semibold text-stl-navy">Tenant detail</h3>
            <p className="text-sm text-[var(--color-text-muted)]">
              {selectedTenant ? `Full record for ${selectedTenant.displayName}` : 'Select a tenant to inspect the full record.'}
            </p>
          </div>
        </div>

        {!selectedTenant ? null : tenantDetailQuery.isLoading ? (
          <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading tenant detail…</p>
        ) : tenantDetailQuery.isError ? (
          <ApiErrorCallout
            message={getErrorMessage(tenantDetailQuery.error, 'Failed to load tenant detail.')}
            onRetry={() => void tenantDetailQuery.refetch()}
            retryLabel="Retry tenant detail"
          />
        ) : tenantDetailQuery.data ? (
          <dl className="mt-3 grid gap-2 md:grid-cols-2">
            <DetailRow label="Tenant ID" value={tenantDetailQuery.data.tenantId} mono />
            <DetailRow label="Slug" value={tenantDetailQuery.data.slug} mono />
            <DetailRow label="Display name" value={tenantDetailQuery.data.displayName} />
            <DetailRow label="Status" value={tenantDetailQuery.data.status} />
            <DetailRow label="Subscription tier" value={tenantDetailQuery.data.subscriptionTier} />
            <DetailRow label="Trial tenant" value={tenantDetailQuery.data.isTrial ? 'Yes' : 'No'} />
            <DetailRow label="Internal tenant" value={tenantDetailQuery.data.isInternalTenant ? 'Yes' : 'No'} />
            <DetailRow
              label="Billing customer ID"
              value={tenantDetailQuery.data.billingCustomerId ?? '—'}
              mono
            />
            <DetailRow
              label="Billing subscription ID"
              value={tenantDetailQuery.data.billingSubscriptionId ?? '—'}
              mono
            />
            <DetailRow
              label="Billing grace days"
              value={tenantDetailQuery.data.billingGraceDays?.toString() ?? '—'}
            />
            <DetailRow label="Created" value={new Date(tenantDetailQuery.data.createdAt).toLocaleString()} />
            <DetailRow label="Modified" value={new Date(tenantDetailQuery.data.modifiedAt).toLocaleString()} />
          </dl>
        ) : null}
      </section>

      {selectedTenant ? (
        <PlatformAdminSection
          title="Decision summary"
          description="Readiness and entitlement posture for the selected tenant record."
        >
          <div className="grid gap-4 md:grid-cols-2">
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--color-text-muted)]">Current state</p>
              <p className="mt-2 text-lg font-semibold text-stl-navy">{selectedTenant.status}</p>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                {selectedTenant.activeEntitlementCount > 0
                  ? 'Tenant is entitled to products and can launch permitted surfaces.'
                  : 'Tenant has no active entitlements and should be reviewed before launch.'}
              </p>
            </div>
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--color-text-muted)]">Source of truth</p>
              <p className="mt-2 text-lg font-semibold text-stl-navy">NexArr tenant record</p>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                Tenant identity, membership, entitlement, and launch snapshots are owned here and surfaced to other products as references.
              </p>
            </div>
          </div>
        </PlatformAdminSection>
      ) : null}

      <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h3 className="text-base font-semibold text-stl-navy">Tenant members and entitlements</h3>
            <p className="text-sm text-[var(--color-text-muted)]">
              {selectedTenant
                ? `Live tenant members and product entitlements for ${selectedTenant.displayName}`
                : 'Select a tenant to inspect members and entitlements.'}
            </p>
          </div>
        </div>

        {!selectedTenant ? null : (
          <div className="mt-4 space-y-4">
            <div className="grid gap-4 lg:grid-cols-2">
              <div>
                <h4 className="text-sm font-semibold text-stl-navy">Members</h4>
                {tenantMembersQuery.isLoading ? (
                  <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading tenant members…</p>
                ) : tenantMembersQuery.isError ? (
                  <ApiErrorCallout
                    message={getErrorMessage(tenantMembersQuery.error, 'Failed to load tenant members.')}
                    onRetry={() => void tenantMembersQuery.refetch()}
                    retryLabel="Retry members"
                  />
                ) : tenantMembersQuery.data?.members.length ? (
                  <ul className="mt-2 space-y-2">
                    {tenantMembersQuery.data.members.map((member) => (
                      <li key={member.membershipId} className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-3 text-sm">
                        <div className="font-medium text-stl-navy">{member.displayName}</div>
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">{member.email}</p>
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                          {member.roleKey} · {member.isActive ? 'Active' : 'Inactive'}
                        </p>
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p className="mt-2 text-sm text-[var(--color-text-muted)]">No tenant members found.</p>
                )}
              </div>

              <div>
                <h4 className="text-sm font-semibold text-stl-navy">Entitlements</h4>
                {tenantEntitlementsQuery.isLoading ? (
                  <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading entitlements…</p>
                ) : tenantEntitlementsQuery.isError ? (
                  <ApiErrorCallout
                    message={getErrorMessage(tenantEntitlementsQuery.error, 'Failed to load entitlements.')}
                    onRetry={() => void tenantEntitlementsQuery.refetch()}
                    retryLabel="Retry entitlements"
                  />
                ) : tenantEntitlementsQuery.data?.items.length ? (
                  <ul className="mt-2 grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
                    {orderedTenantEntitlements.map((entitlement) => {
                      const isActive = entitlement.status.toLowerCase() === 'active'

                      return (
                        <li
                          key={entitlement.entitlementId}
                          className={[
                            'rounded-xl border p-4 text-sm shadow-sm transition',
                            isActive
                              ? 'border-cyan-500/40 bg-[var(--color-bg-surface-muted)]'
                              : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] opacity-80',
                          ].join(' ')}
                        >
                          <div className="flex items-start justify-between gap-3">
                            <div className="min-w-0">
                              <div className="font-medium text-stl-navy">{entitlement.productDisplayName}</div>
                              <p className="mt-1 break-all text-xs text-[var(--color-text-muted)]">
                                {entitlement.productKey}
                              </p>
                            </div>
                            <span
                              className={[
                                'inline-flex rounded-full border px-2.5 py-0.5 text-xs font-medium capitalize',
                                isActive
                                  ? 'border-emerald-500/30 bg-emerald-950/40 text-emerald-300'
                                  : 'border-slate-500/30 bg-slate-950/40 text-slate-300',
                              ].join(' ')}
                            >
                              {entitlement.status}
                            </span>
                          </div>

                          <label className="mt-3 flex items-start gap-2 text-xs text-[var(--color-text-muted)]">
                            <input
                              type="checkbox"
                              checked={isActive}
                              readOnly
                              aria-label={`${entitlement.productDisplayName} entitlement active`}
                              className="mt-0.5 h-4 w-4 rounded border-[var(--color-border-strong)] accent-cyan-500"
                            />
                            <span>
                              granted {new Date(entitlement.grantedAt).toLocaleString()}
                              {entitlement.revokedAt
                                ? ` · revoked ${new Date(entitlement.revokedAt).toLocaleString()}`
                                : ''}
                            </span>
                          </label>

                          {isActive ? (
                            <button
                              type="button"
                              className="mt-3 inline-flex rounded-md border border-rose-500/30 px-2.5 py-1 text-xs font-medium text-rose-300 hover:bg-rose-950/30 disabled:opacity-50"
                              onClick={() => revokeEntitlementMutation.mutate(entitlement.productKey)}
                              disabled={revokeEntitlementMutation.isPending}
                              data-testid={`tenant-entitlement-revoke-${entitlement.productKey}`}
                            >
                              Revoke entitlement
                            </button>
                          ) : null}
                        </li>
                      )
                    })}
                  </ul>
                ) : (
                  <p className="mt-2 text-sm text-[var(--color-text-muted)]">No tenant entitlements found.</p>
                )}
              </div>
            </div>

            <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <h4 className="text-sm font-semibold text-stl-navy">Grant entitlement</h4>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                Grant an active product entitlement to the selected tenant.
              </p>

              <div className="mt-3 grid gap-3 md:grid-cols-[minmax(0,1fr)_auto] md:items-end">
                <StaticSearchPicker
                  label="Product to grant"
                  id="tenant-entitlement-product"
                  value={selectedProductKey}
                  onChange={setSelectedProductKey}
                  options={productOptions}
                  placeholder="Search products"
                  testId="tenant-entitlement-product"
                />

                <button
                  type="button"
                  className="rounded-md bg-stl-navy px-4 py-2 text-sm font-medium text-white hover:bg-slate-800 disabled:opacity-50"
                  onClick={() => grantEntitlementMutation.mutate()}
                  disabled={!selectedProductKey || grantEntitlementMutation.isPending}
                  data-testid="tenant-entitlement-grant"
                >
                  {grantEntitlementMutation.isPending ? 'Granting…' : 'Grant entitlement'}
                </button>
              </div>
            </div>

            <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <h4 className="text-sm font-semibold text-stl-navy">Service clients</h4>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                Service clients that can call NexArr on behalf of this tenant.
              </p>

              {!selectedTenant ? null : serviceClientsQuery.isLoading ? (
                <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading tenant service clients…</p>
              ) : serviceClientsQuery.isError ? (
                <ApiErrorCallout
                  message={getErrorMessage(serviceClientsQuery.error, 'Failed to load tenant service clients.')}
                  onRetry={() => void serviceClientsQuery.refetch()}
                  retryLabel="Retry service clients"
                />
              ) : tenantServiceClients.length ? (
                <ul className="mt-3 space-y-2">
                  {tenantServiceClients.map((client) => (
                    <li key={client.serviceClientId} className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-3 text-sm">
                      <div className="font-medium text-stl-navy">{client.displayName}</div>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">{client.clientKey}</p>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {client.isActive ? 'Active' : 'Inactive'} · last used{' '}
                        {client.lastUsedAt ? new Date(client.lastUsedAt).toLocaleString() : 'never'}
                      </p>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {client.failedAuthenticationAttempts} failed auth attempts · allowed products{' '}
                        {client.allowedProductKeys.length}
                      </p>
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="mt-3 text-sm text-[var(--color-text-muted)]">No tenant service clients found.</p>
              )}
            </div>

            <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <h4 className="text-sm font-semibold text-stl-navy">Launch history</h4>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                Recent tenant launch attempts and failure reasons.
              </p>

              {launchHistoryQuery.isLoading ? (
                <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading launch history…</p>
              ) : launchHistoryQuery.isError ? (
                <ApiErrorCallout
                  message={getErrorMessage(launchHistoryQuery.error, 'Failed to load launch history.')}
                  onRetry={() => void launchHistoryQuery.refetch()}
                  retryLabel="Retry launch history"
                />
              ) : launchHistoryQuery.data?.items.length ? (
                <ul className="mt-3 space-y-2">
                  {launchHistoryQuery.data.items.map((attempt) => (
                    <li key={attempt.auditEventId} className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-3 text-sm">
                      <div className="font-medium text-stl-navy">
                        {attempt.productDisplayName ?? attempt.productKey ?? 'Unknown product'}
                      </div>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {attempt.action} · {attempt.result}
                        {attempt.actorDisplayName ? ` · ${attempt.actorDisplayName}` : ''}
                      </p>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {new Date(attempt.occurredAt).toLocaleString()}
                        {attempt.reasonCode ? ` · ${attempt.reasonCode}` : ''}
                      </p>
                      {attempt.remediationHint ? (
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">{attempt.remediationHint}</p>
                      ) : null}
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="mt-3 text-sm text-[var(--color-text-muted)]">No launch history found for this tenant.</p>
              )}
            </div>
          </div>
        )}
      </section>

      <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h3 className="text-base font-semibold text-stl-navy">Tenant audit history</h3>
            <p className="text-sm text-[var(--color-text-muted)]">
              {selectedTenant ? `Events for ${selectedTenant.displayName} (${selectedTenant.slug})` : 'Select a tenant to inspect audit events.'}
            </p>
          </div>
        </div>

        {!selectedTenant ? null : auditQuery.isLoading ? (
          <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading audit history…</p>
        ) : auditQuery.isError ? (
          <ApiErrorCallout
            message={getErrorMessage(auditQuery.error, 'Failed to load tenant audit history.')}
            onRetry={() => void auditQuery.refetch()}
            retryLabel="Retry audit"
          />
        ) : auditQuery.data?.items.length ? (
          <AuditEventList items={auditQuery.data.items} />
        ) : (
          <p className="mt-3 text-sm text-[var(--color-text-muted)]">No tenant audit events found.</p>
        )}
      </section>

      <PlatformAdminScopeNote>
        Detail scope: NexArr owns the tenant record, membership, entitlement, product launch handoff, and platform audit history. Product-local permissions and execution remain in the target products.
      </PlatformAdminScopeNote>

      <TenantCatalogAdminPanel />
    </div>
  )
}

function DetailRow({
  label,
  value,
  mono = false,
}: {
  label: string
  value: string
  mono?: boolean
}) {
  return (
    <div className="grid grid-cols-[8rem_1fr] gap-3">
      <dt className="font-medium text-[var(--color-text-muted)]">{label}</dt>
      <dd className={mono ? 'font-mono text-xs break-all text-[var(--color-text-secondary)]' : 'text-[var(--color-text-secondary)]'}>{value || '—'}</dd>
    </div>
  )
}

function AuditEventList({ items }: { items: PlatformAuditEventTimelineItem[] }) {
  return (
    <ul className="mt-3 space-y-2">
      {items.map((item) => (
        <li key={item.auditEventId} className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-3 text-sm">
          <div className="font-medium text-stl-navy">{item.action}</div>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
            {item.result}
            {item.targetType ? ` · ${item.targetType}` : ''}
            {item.actorUserId ? ` · actor ${item.actorUserId}` : ''}
          </p>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">{new Date(item.occurredAt).toLocaleString()}</p>
        </li>
      ))}
    </ul>
  )
}
