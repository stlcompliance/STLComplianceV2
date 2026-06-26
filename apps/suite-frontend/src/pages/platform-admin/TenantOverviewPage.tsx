import { useQuery } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import {
  ApiErrorCallout,
  formatProductDisplayName,
  formatRoleDisplayName,
  formatStatusLabel,
  getErrorMessage,
} from '@stl/shared-ui'
import * as nexarr from '../../api/nexarrClient'
import {
  describeLaunchFailure,
  normalizeLaunchRemediationHint,
  resolveLaunchFailureCopy,
} from '../../lib/launchFailure'
import { TenantCatalogAdminPanel } from '../../components/platform-admin/TenantCatalogAdminPanel'
import {
  PlatformAdminKpiCard,
  PlatformAdminPageHeader,
  PlatformAdminScopeNote,
  PlatformAdminSection,
} from '../../components/platform-admin/PlatformAdminPageChrome'
import type { PlatformAuditEventTimelineItem } from '../../api/types'

function tenantStatusLabel(status: string): string {
  const normalized = status.trim().toLowerCase()
  if (normalized === 'active') {
    return 'Enabled'
  }
  if (normalized === 'suspended') {
    return 'Suspended'
  }
  return status
}

function isTenantLaunchReady(status: string): boolean {
  const normalized = status.trim().toLowerCase()
  return normalized === 'active' || normalized === 'trial'
}

function isComplianceCoreDestination(product: { productKey: string; displayName: string }): boolean {
  const normalized = `${product.productKey} ${product.displayName}`.trim().toLowerCase()
  return normalized.includes('compliance') && normalized.includes('core')
}

export function TenantOverviewPage() {
  const [selectedTenantId, setSelectedTenantId] = useState('')

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

  const orderedProducts = useMemo(
    () =>
      [...(productsQuery.data?.items ?? [])].sort((left, right) => {
        if (left.sortOrder !== right.sortOrder) {
          return left.sortOrder - right.sortOrder
        }

        return left.displayName.localeCompare(right.displayName)
      }),
    [productsQuery.data?.items],
  )

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
        summary="Selected tenant record, membership state, product destination status, launch attempts, and audit history for NexArr platform administration."
        badge={selectedTenant ? `${tenantStatusLabel(selectedTenant.status)} tenant` : 'Tenant records'}
        updatedAt={selectedTenant ? new Date(selectedTenant.createdAt).toLocaleString() : undefined}
      />

      {selectedTenant ? (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
          <PlatformAdminKpiCard
            label="Members"
            value={selectedTenant.membershipCount}
            hint="Tenant membership records are managed in NexArr."
            tone="info"
          />
          <PlatformAdminKpiCard
            label="Tenant status"
            value={tenantStatusLabel(selectedTenant.status)}
            hint="Launch readiness starts with the tenant lifecycle state in NexArr."
            tone={isTenantLaunchReady(selectedTenant.status) ? 'good' : 'warn'}
          />
          <PlatformAdminKpiCard
            label="Launch attempts"
            value={launchHistoryQuery.data?.items.length ?? '—'}
            hint="Recent product launch results for this tenant."
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
                className={`cursor-pointer border-b border-[var(--color-border-subtle)] ${isSelected ? 'bg-[var(--color-warning-bg)]' : ''}`}
                  onClick={() => setSelectedTenantId(tenant.tenantId)}
                >
                  <td className="px-3 py-2">
                    <span className="font-medium text-[var(--color-text-primary)]">{tenant.displayName}</span>
                  </td>
                  <td className="px-3 py-2">{formatStatusLabel(tenant.status)}</td>
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
            <h3 className="text-base font-semibold text-[var(--color-text-primary)]">Tenant detail</h3>
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
            <DetailRow label="Status" value={formatStatusLabel(tenantDetailQuery.data.status)} />
            <DetailRow label="Subscription tier" value={tenantDetailQuery.data.subscriptionTier} />
            <DetailRow label="Trial tenant" value={tenantDetailQuery.data.isTrial ? 'Enabled' : 'Disabled'} />
            <DetailRow label="Internal tenant" value={tenantDetailQuery.data.isInternalTenant ? 'Enabled' : 'Disabled'} />
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
          description="Readiness and launch posture for the selected tenant record."
        >
          <div className="grid gap-4 md:grid-cols-2">
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--color-text-muted)]">Current state</p>
              <p className="mt-2 text-lg font-semibold text-[var(--color-text-primary)]">{tenantStatusLabel(selectedTenant.status)}</p>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                {isTenantLaunchReady(selectedTenant.status)
                  ? 'Active tenant members can launch every ordinary product. Product-local permissions still apply after launch.'
                  : 'This tenant is not in an active launch-ready state. Review tenant status before launch attempts continue.'}
              </p>
            </div>
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--color-text-muted)]">Source of truth</p>
              <p className="mt-2 text-lg font-semibold text-[var(--color-text-primary)]">NexArr tenant record</p>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                Tenant identity, membership, billing profile, product destination status, and launch records are owned here and surfaced to other products as references.
              </p>
            </div>
          </div>
        </PlatformAdminSection>
      ) : null}

      <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h3 className="text-base font-semibold text-[var(--color-text-primary)]">Tenant members and product destinations</h3>
            <p className="text-sm text-[var(--color-text-muted)]">
              {selectedTenant
                ? `Current tenant members and product destinations for ${selectedTenant.displayName}`
                : 'Select a tenant to inspect members and product destinations.'}
            </p>
          </div>
        </div>

        {!selectedTenant ? null : (
          <div className="mt-4 space-y-4">
            <div className="grid gap-4 lg:grid-cols-2">
              <div>
                <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Members</h4>
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
                        <div className="font-medium text-[var(--color-text-primary)]">{member.displayName}</div>
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">{member.email}</p>
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                          {formatRoleDisplayName(member.roleKey)} · {member.isActive ? 'Enabled' : 'Disabled'}
                        </p>
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p className="mt-2 text-sm text-[var(--color-text-muted)]">No tenant members found.</p>
                )}
              </div>

              <div>
                <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Product destinations</h4>
                {productsQuery.isLoading ? (
                  <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading product destinations…</p>
                ) : productsQuery.isError ? (
                  <ApiErrorCallout
                    message={getErrorMessage(productsQuery.error, 'Failed to load product destinations.')}
                    onRetry={() => void productsQuery.refetch()}
                    retryLabel="Retry product destinations"
                  />
                ) : orderedProducts.length ? (
                  <ul className="mt-2 grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
                    {orderedProducts.map((product) => {
                      const isOperational = product.isActive && product.productStatus.toLowerCase() === 'available'
                      const detailText = isComplianceCoreDestination(product)
                        ? 'Compliance Core studio stays platform-admin-only. Runtime rule services remain available through authorized products.'
                        : isOperational
                          ? 'Active tenant members can launch this destination. Product-local permissions are checked after launch.'
                          : 'This destination is not currently in an available operating state.'

                      return (
                        <li
                          key={product.productKey}
                          className={[
                            'rounded-xl border p-4 text-sm shadow-sm transition',
                            isOperational
                              ? 'border-[var(--color-success-border)] bg-[var(--color-bg-surface-muted)]'
                              : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] opacity-80',
                          ].join(' ')}
                        >
                          <div className="flex items-start justify-between gap-3">
                            <div className="min-w-0">
                              <div className="font-medium text-[var(--color-text-primary)]">{product.displayName}</div>
                            </div>
                            <span
                              className={[
                                'inline-flex rounded-full border px-2.5 py-0.5 text-xs font-medium capitalize',
                                isOperational
                                  ? 'border-[var(--color-success-border)] bg-[var(--color-success-bg)] text-[var(--color-success-text)]'
                                  : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] text-[var(--color-text-secondary)]',
                              ].join(' ')}
                            >
                              {formatStatusLabel(product.productStatus)}
                            </span>
                          </div>
                          <p className="mt-3 text-xs text-[var(--color-text-muted)]">{detailText}</p>
                        </li>
                      )
                    })}
                  </ul>
                ) : (
                  <p className="mt-2 text-sm text-[var(--color-text-muted)]">No product destinations found.</p>
                )}
              </div>
            </div>

            <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Service clients</h4>
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
                      <div className="font-medium text-[var(--color-text-primary)]">{client.displayName}</div>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">{client.clientKey}</p>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {client.isActive ? 'Enabled' : 'Disabled'} · last used{' '}
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
              <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Launch history</h4>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                Recent product launch attempts and failure reasons for this tenant.
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
                  {launchHistoryQuery.data.items.map((attempt) => {
                    const failure = describeLaunchFailure(attempt.reasonCode)
                    const remediationHint = normalizeLaunchRemediationHint(
                      attempt.remediationHint,
                      attempt.reasonCode,
                    )

                    return (
                      <li key={attempt.auditEventId} className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-3 text-sm">
                        <div className="font-medium text-[var(--color-text-primary)]">
                          {formatProductDisplayName(attempt.productDisplayName ?? attempt.productKey ?? 'Unknown product')}
                        </div>
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                          {attempt.action} · {attempt.result}
                          {attempt.actorDisplayName ? ` · ${attempt.actorDisplayName}` : ''}
                        </p>
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                          {new Date(attempt.occurredAt).toLocaleString()}
                        </p>
                        {failure ? (
                          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                            {failure.title} · {failure.normalizedCode}
                            {failure.rawCode ? ` · raw ${failure.rawCode}` : ''}
                          </p>
                        ) : null}
                        {remediationHint ? (
                          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                            {remediationHint}
                          </p>
                        ) : attempt.reasonCode ? (
                          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                            {resolveLaunchFailureCopy(attempt.reasonCode).guidance}
                          </p>
                        ) : null}
                      </li>
                    )
                  })}
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
            <h3 className="text-base font-semibold text-[var(--color-text-primary)]">Tenant audit history</h3>
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
        Detail scope: NexArr manages the tenant record, membership, product destination status, product launch flow, and platform audit history. Product-local permissions and execution remain in the target products.
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
          <div className="font-medium text-[var(--color-text-primary)]">{item.action}</div>
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
